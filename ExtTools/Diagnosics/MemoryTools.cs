using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using FreeLibSet.Logging;

namespace FreeLibSet.Diagnostics
{
  /// <summary>
  /// ������� ��������� ����������� ������
  /// </summary>
  [Serializable]
  public enum AvailableMemoryState
  {
    /// <summary>
    /// ������� ����������� ���������� ��������� ������. ���� �������� �� ������������
    /// </summary>
    Normal,

    /// <summary>
    /// ��������� ���������� ������ �����������. ������������ ���� ��������.
    /// ���������� ���������� ������
    /// </summary>
    Swapping,

    /// <summary>
    /// ����������� ������ �������������.
    /// ������ ����������� ��������� OutOfMemoryException
    /// </summary>
    Low
  }

  /// <summary>
  /// ������ ����������� ������� ��������� ����������� ������
  /// </summary>
  public static class MemoryTools
  {
    #region CheckSufficientMemory

    private static bool CheckSufficientMemoryExceptionLogged = false;

    /// <summary>
    /// ��������� ����������� ��������� ����� ������ � ������� MemoryFailPoint
    /// </summary>
    /// <param name="sizeMB">������ ����� � ����������</param>
    /// <returns>true - ��������� ���� ��������</returns>
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
      catch (Exception e)
      {
        // 20.06.2017
        // � mono MemoryFailPoint �� �������� � ����������� NotImplementedException
        // TODO: ���� ��������� ��������� ������ ���-�� ��-�������
        if (!CheckSufficientMemoryExceptionLogged)
        {
          CheckSufficientMemoryExceptionLogged = true;
          LogoutTools.LogoutException(e, "MemoryTools.CheckSufficientMemory");
        }

        return true;
      }
    }

    #endregion

    #region P/Invoke

#pragma warning disable 649 // �������������� "�������� �� ������������� ����"

    /// <summary>
    /// contains information about the current state of both physical and virtual memory, including extended memory
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
      #region ����

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

      #region �����������

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
      #region ����

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

      #region �����������

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
    /// ������� ������������� ����������� ������ (0-100).
    /// ���������� ��������� UnknownMemoryLoad, ���� ��� �� ������ �������� ��� ��������
    /// </summary>
    public static int MemoryLoad
    {
      get
      {
        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Win32NT:
            return GetMemoryLoadNT();
          case PlatformID.Win32Windows:
            return GetMemoryLoadWin();
          default:
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

    /// <summary>
    /// ��� �������� ������������ ��������� MemoryLoad, ����� ������ �������� ������� ������������� ������
    /// </summary>
    public const int UnknownMemoryLoad = -1;

    #endregion

    #region TotalPhysicalMemory

    /// <summary>
    /// ����� ����� ������������� ���������� ������.
    /// ���������� UnknownMemorySize, ���� ���������� ����� ����������
    /// </summary>
    public static long TotalPhysicalMemory
    {
      get
      {
        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Win32NT:
            return GetTotalPhysicalMemoryNT();
          case PlatformID.Win32Windows:
            return GetTotalPhysicalMemoryWin();
          default:
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
        return UnknownMemoryLoad;
    }

    /// <summary>
    /// ��������, ������������ ��������� TotalPhysicalMemory, ���� ����� ������ ����������
    /// </summary>
    public const long UnknownMemorySize = -1L;

    #endregion

    #region AvailableMemoryState

    /// <summary>
    /// ���������� ������� ��������� ����������� ������
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
    /// ����� ��������� ����������� ������ � ��, ������� ��������� �����������.
    /// ���� �� ������� �������� ���� ������ ������ �������, ���������, ��� ������ ����.
    /// �� ��������� - 100��
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