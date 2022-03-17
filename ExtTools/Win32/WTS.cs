// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;

namespace FreeLibSet.Win32
{
  #region Перечисление WTSConnectState

  /// <summary>
  /// Возможные значения свойства WTSSession.ConnectState
  /// </summary>
  public enum WTSConnectState
  {
    /// <summary>
    /// A user is logged on to the WinStation.
    /// </summary>
    WTSActive,

    /// <summary>
    /// The WinStation is connected to the client. 
    /// </summary>
    WTSConnected,

    /// <summary>
    /// WTSConnectQuery
    /// </summary>
    WTSConnectQuery,

    /// <summary>
    /// The WinStation is shadowing another WinStation. 
    /// </summary>
    WTSShadow,

    /// <summary>
    /// The WinStation is active but the client is disconnected.
    /// </summary>
    WTSDisconnected,

    /// <summary>
    /// The WinStation is waiting for a client to connect.
    /// </summary>
    WTSIdle,

    /// <summary>
    /// The WinStation is listening for a connection. 
    /// A listener session waits for requests for new client connections. 
    /// No user is logged on a listener session. 
    /// A listener session cannot be reset, shadowed, or changed to a regular client session.
    /// </summary>
    WTSListen,

    /// <summary>
    /// The WinStation is being reset.
    /// </summary>
    WTSReset,

    /// <summary>
    /// The WinStation is down due to an error.
    /// </summary>
    WTSDown,

    /// <summary>
    /// The WinStation is initializing.
    /// </summary>
    WTSInit
  }

  #endregion

  #region Перечисление WTSColorDepth

  /// <summary>
  /// Значения свойства WTSSession.ClientDisplayColorDepth
  /// </summary>
  public enum WTSColorDepth
  {
    /// <summary>
    /// The display uses 4 bits per pixel for a maximum of 16 colors.
    /// </summary>
    BPP4 = 1,

    /// <summary>
    /// The display uses 8 bits per pixel for a maximum of 256 colors.
    /// </summary>
    BPP8 = 2,

    /// <summary>
    /// The display uses 16 bits per pixel for a maximum of 2^16 colors.
    /// </summary>
    BPP16 = 4,

    /// <summary>
    /// The display uses 3-byte RGB values for a maximum of 2^24 colors.
    /// </summary>
    ThreeByteBPP24 = 8,

    /// <summary>
    /// The display uses 15 bits per pixel for a maximum of 2^15 colors.
    /// </summary>
    BPP15 = 16,

    /// <summary>
    /// 24 bits per pixel.
    /// </summary>
    BPP24 = 24,

    /// <summary>
    /// 32 bits per pixel.
    /// </summary>
    BPP32 = 32,
  }

  #endregion

  #region Перечисление WTSClientProtocolType

  /// <summary>
  /// Значения свойства WTSSession.ClientProtocolType
  /// </summary>
  public enum WTSClientProtocolType
  {
    /// <summary>
    ///   The console session
    /// </summary>
    ConsoleSession = 0,

    /// <summary>
    /// This value is retained for legacy purposes. 
    /// </summary>
    Unused1 = 1,

    /// <summary>
    /// The RDP protocol.
    /// </summary>
    RDP = 2,
  }

  #endregion

  /// <summary>
  /// Информация о сессии Remote Desktop Services.
  /// Пока поддерживается только текущий сервер WTS_CURRENT_SERVER_HANDLE
  /// </summary>
  public sealed class WTSSession
  {
    #region Поддержка ОС

    static WTSSession()
    {
      _IsSupported = false;
      if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        return;
      if (Environment.OSVersion.Version.Major < 5) // до Windows 2000
        return;
      if (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 0) // Windows 2000
        return; // 26.10.2018
      if (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 1) // Windows XP
        return; // 28.05.2021 Не работает в XP тоже. Надоела выдавать исключения

      _IsSupported = true;
    }

    /// <summary>
    /// Возвращает true, если Remote Desktop Services поддерживаются операционной системой
    /// </summary>
    public static bool IsSupported { get { return _IsSupported; } }
    private static bool _IsSupported;

    private static void CheckIsSupported()
    {
      if (!IsSupported)
        throw new PlatformNotSupportedException();
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает объект для WTS_CURRENT_SERVER_HANDLE и WTS_CURRENT_SESSION
    /// </summary>
    public WTSSession()
      : this(NativeMethods.WTS_CURRENT_SESSION)
    {
    }

    /// <summary>
    /// Создает объект для WTS_CURRENT_SERVER_HANDLE и заданного идентификатора сессии
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии</param>
    public WTSSession(int sessionId)
    {
      CheckIsSupported();
      _ServerHandle = NativeMethods.WTS_CURRENT_SERVER_HANDLE;
      if (sessionId == NativeMethods.WTS_CURRENT_SESSION)
      {
        _SessionId = NativeMethods.WTSGetActiveConsoleSessionId();
        if (_SessionId == (-1))
          throw new InvalidOperationException("Нет текущей сессии");
      }
      else
        _SessionId = sessionId;

      _InfoValues = new object[(int)(NativeMethods.WtsInfoClassMaxValue) + 1];
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// A handle to an RD Session Host server. 
    /// В текущей реализации всегда WTS_CURRENT_SERVER_HANDLE.
    /// </summary>
    public IntPtr ServerHandle { get { return _ServerHandle; } }
    private IntPtr _ServerHandle;

    /// <summary>
    /// Идентификатор сессии.
    /// Если конструктору не был передан идентификатор сессии, возвращает идентификатор активной сессии
    /// </summary>
    public int SessionId { get { return _SessionId; } }
    private int _SessionId;

    #endregion

    #region Свойства, получаемые из WTSQuerySessionInformation

    /// <summary>
    /// Используем буферизацию. Если одно и тоже свойство запрашивается несколько раз,
    /// не вызывает функции ОС.
    /// </summary>
    private object[] _InfoValues;

    /// <summary>
    /// Name of the initial program that Remote Desktop Services runs when the user logs on.
    /// </summary>
    public string InitialProgram
    {
      get
      {
        try { return (string)GetInfoValue(NativeMethods.WtsInfoClass.WTSInitialProgram); }
        catch { return String.Empty; }
      }
    }

    /// <summary>
    /// Published name of the application that the session is running. 
    /// Windows Server 2008 R2, Windows 7, Windows Server 2008 and Windows Vista:  This value is not supported
    /// </summary>
    public string ApplicationName { get { return (string)GetInfoValue(NativeMethods.WtsInfoClass.WTSApplicationName); } }

    /// <summary>
    /// Default directory used when launching the initial program
    /// </summary>
    public string WorkingDirectory { get { return (string)GetInfoValue(NativeMethods.WtsInfoClass.WTSWorkingDirectory); } }

    // WTSOEMId пропускаем

    // WTSSessionId пропускаем, т.к. инициализируется в конструкторе

    /// <summary>
    /// Name of the user associated with the session.
    /// </summary>
    public string UserName { get { return (string)GetInfoValue(NativeMethods.WtsInfoClass.WTSUserName); } }

    /// <summary>
    /// Name of the Remote Desktop Services session. 
    /// Note  
    /// Despite its name, specifying this type does not return the window station name. 
    /// Rather, it returns the name of the Remote Desktop Services session. 
    /// Each Remote Desktop Services session is associated with an interactive window station. 
    /// Because the only supported window station name for an interactive window station is "WinSta0", 
    /// each session is associated with its own "WinSta0" window station. 
    /// For more information, see Window Stations.
    /// </summary>
    public string WinStationName { get { return (string)GetInfoValue(NativeMethods.WtsInfoClass.WTSWinStationName); } }

    /// <summary>
    /// Name of the domain to which the logged-on user belongs.
    /// </summary>
    public string DomainName { get { return (string)GetInfoValue(NativeMethods.WtsInfoClass.WTSDomainName); } }

    /// <summary>
    /// The session's current connection state
    /// </summary>
    public WTSConnectState ConnectionState { get { return (WTSConnectState)GetInfoValue(NativeMethods.WtsInfoClass.WTSConnectState); } }

    /// <summary>
    /// The build number of the client.
    /// </summary>
    public int ClientBuildNumber { get { return (int)GetInfoValue(NativeMethods.WtsInfoClass.WTSClientBuildNumber); } }

    /// <summary>
    /// Name of the client.
    /// </summary>
    public string ClientName { get { return (string)GetInfoValue(NativeMethods.WtsInfoClass.WTSClientName); } }

    /// <summary>
    /// Directory in which the client is installed.
    /// </summary>
    public string ClientDirectory { get { return (string)GetInfoValue(NativeMethods.WtsInfoClass.WTSClientDirectory); } }

    /// <summary>
    /// Client-specific product identifier.
    /// </summary>
    public int ClientProductId { get { return (int)GetInfoValue(NativeMethods.WtsInfoClass.WTSClientProductId); } }

    /// <summary>
    /// Client-specific hardware identifier. 
    /// This option is reserved for future use. 
    /// WTSQuerySessionInformation will always return a value of 0.
    /// </summary>
    public int ClientHardwareId { get { return (int)GetInfoValue(NativeMethods.WtsInfoClass.WTSClientHardwareId); } }

    // WTSClientAddress пропускаем. Х.З. как это возвращать

    private NativeMethods.WTS_CLIENT_DISPLAY ClientDisplay { get { return (NativeMethods.WTS_CLIENT_DISPLAY)GetInfoValue(NativeMethods.WtsInfoClass.WTSClientDisplay); } }

    /// <summary>
    /// Горизонтальное разрешение
    /// </summary>
    public int ClientDisplayHorizontalResolution { get { return ClientDisplay.iHorizontalResolution; } }

    /// <summary>
    /// Вертикальное разрешение
    /// </summary>
    public int ClientDisplayVerticalResolution { get { return ClientDisplay.iVerticalResolution; } }

    /// <summary>
    /// Количество цветов
    /// </summary>
    public WTSColorDepth ClientDisplayColorDepth { get { return (WTSColorDepth)(ClientDisplay.iColorDepth); } }

    /// <summary>
    /// Information about the protocol type for the session.
    /// </summary>
    public WTSClientProtocolType ClientProtocolType { get { return (WTSClientProtocolType)GetInfoValue(NativeMethods.WtsInfoClass.WTSClientProtocolType); } }

    // WTSIdleTime, WTSLogonTime,WTSIncomingBytes,WTSOutgoingBytes,WTSIncomingFrames,WTSOutgoingFrames пропускаем

    // WTSClientInfo пропускаем, т.к. повтор других данных

    // WTSSessionInfo и WTSSessionInfoEx пропускаем - неохота разбираться.

    // WTSConfigInfo пропускаем - неохота разбираться.

    // WTSValidationInfo не поддерживается

    // WTSSessionAddressV4 пропускаем - неохота разбираться.

    /// <summary>
    /// Determines whether the current session is a remote session. 
    /// The value of TRUE indicates that the current session is a remote session, and FALSE indicates 
    /// that the current session is a local session. 
    /// This value can only be used for the local machine, 
    /// so the hServer parameter of the WTSQuerySessionInformation function must contain WTS_CURRENT_SERVER_HANDLE. 
    /// </summary>
    public bool IsRemoteSession
    {
      get
      {
        IntPtr dummyBuffer;
        uint dummyCount;
        return NativeMethods.WTSQuerySessionInformation(ServerHandle, SessionId,
          NativeMethods.WtsInfoClass.WTSIsRemoteSession, out dummyBuffer, out dummyCount);
      }
    }
    // не так работает public bool IsRemoteSession { get { return DataTools.GetBool(GetInfoValue(NativeMethods.WtsInfoClass.WTSIsRemoteSession)); } }

    private object GetInfoValue(NativeMethods.WtsInfoClass infoClass)
    {
      if (_InfoValues[(int)infoClass] == null)
      {
        IntPtr buffer;
        uint byteCount;
        if (!NativeMethods.WTSQuerySessionInformation(ServerHandle, SessionId, infoClass,
          out buffer, out byteCount))
          ThrowWin32Error();

        try
        {
          _InfoValues[(int)infoClass] = DoGetInfoValue(infoClass, buffer);
        }
        finally
        {
          NativeMethods.WTSFreeMemory(buffer);
        }
      }
      return _InfoValues[(int)infoClass];
    }

    [DebuggerStepThrough]
    private void ThrowWin32Error()
    {
      int res = Marshal.GetLastWin32Error();
      throw new System.ComponentModel.Win32Exception(res);
    }

    private static object DoGetInfoValue(NativeMethods.WtsInfoClass infoClass, IntPtr buffer)
    {
      switch (infoClass)
      {
        case NativeMethods.WtsInfoClass.WTSInitialProgram:
        case NativeMethods.WtsInfoClass.WTSApplicationName:
        case NativeMethods.WtsInfoClass.WTSWorkingDirectory:
        case NativeMethods.WtsInfoClass.WTSUserName:
        case NativeMethods.WtsInfoClass.WTSWinStationName:
        case NativeMethods.WtsInfoClass.WTSDomainName:
        case NativeMethods.WtsInfoClass.WTSClientName:
        case NativeMethods.WtsInfoClass.WTSClientDirectory:
          // строки
          return Marshal.PtrToStringAnsi(buffer);

        case NativeMethods.WtsInfoClass.WTSConnectState:
          // enum
          return Marshal.PtrToStructure(buffer, typeof(Int32));

        case NativeMethods.WtsInfoClass.WTSClientBuildNumber:
        case NativeMethods.WtsInfoClass.WTSClientHardwareId:
          // ULONG
          return Marshal.PtrToStructure(buffer, typeof(Int32));

        case NativeMethods.WtsInfoClass.WTSClientProductId:
        case NativeMethods.WtsInfoClass.WTSClientProtocolType:
          // USHORT
          // Тут пакость. Метод PtrToStructure() не работает с двухбайтной структурой
          //return (int)Marshal.PtrToStructure(Buffer, typeof(UInt16));
          short[] a16 = new short[1];
          Marshal.Copy(buffer, a16, 0, 1);
          return (int)(a16[0]);

        case NativeMethods.WtsInfoClass.WTSIsRemoteSession:
          // BOOLEAN (1 byte)
          byte[] a8 = new byte[1];
          Marshal.Copy(buffer, a8, 0, 1);
          return (int)(a8[0]);


        case NativeMethods.WtsInfoClass.WTSClientDisplay:
          return Marshal.PtrToStructure(buffer, typeof(NativeMethods.WTS_CLIENT_DISPLAY));

        default:
          throw new InvalidEnumArgumentException("infoClass", (int)infoClass, typeof(NativeMethods.WtsInfoClass));
      }
    }

    #endregion

    #region P/Invoke

    private static class NativeMethods
    {
      #region Константы

      public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

      public const int WTS_CURRENT_SESSION = -1;

      #endregion

      #region Перечисление WTS_INFO_CLASS

      /// <summary>
      /// Contains values that indicate the type of session information to retrieve in a call to the <see cref="WTSQuerySessionInformation"/> function.
      /// </summary>
      public enum WtsInfoClass
      {
        /// <summary>
        /// A null-terminated string that contains the name of the initial program that Remote Desktop Services runs when the user logs on.
        /// </summary>
        WTSInitialProgram = 0,

        /// <summary>
        /// A null-terminated string that contains the published name of the application that the session is running.
        /// </summary>
        WTSApplicationName = 1,

        /// <summary>
        /// A null-terminated string that contains the default directory used when launching the initial program.
        /// </summary>
        WTSWorkingDirectory = 2,

        /// <summary>
        /// This value is not used.
        /// </summary>
        WTSOEMId = 3,

        /// <summary>
        /// A <B>ULONG</B> value that contains the session identifier.
        /// </summary>
        WTSSessionId = 4,

        /// <summary>
        /// A null-terminated string that contains the name of the user associated with the session.
        /// </summary>
        WTSUserName = 5,

        /// <summary>
        /// A null-terminated string that contains the name of the Remote Desktop Services session. 
        /// </summary>
        /// <remarks>
        /// <B>Note</B>  Despite its name, specifying this type does not return the window station name. 
        /// Rather, it returns the name of the Remote Desktop Services session. 
        /// Each Remote Desktop Services session is associated with an interactive window station. 
        /// Because the only supported window station name for an interactive window station is "WinSta0", 
        /// each session is associated with its own "WinSta0" window station. For more information, see <see href="http://msdn.microsoft.com/en-us/library/windows/desktop/ms687096(v=vs.85).aspx">Window Stations</see>.
        /// </remarks>
        WTSWinStationName = 6,

        /// <summary>
        /// A null-terminated string that contains the name of the domain to which the logged-on user belongs.
        /// </summary>
        WTSDomainName = 7,

        /// <summary>
        /// The session's current connection state. For more information, see WTS_CONNECTSTATE_CLASS.
        /// </summary>
        WTSConnectState = 8,

        /// <summary>
        /// A <B>ULONG</B> value that contains the build number of the client.
        /// </summary>
        WTSClientBuildNumber = 9,

        /// <summary>
        /// A null-terminated string that contains the name of the client.
        /// </summary>
        WTSClientName = 10,

        /// <summary>
        /// A null-terminated string that contains the directory in which the client is installed.
        /// </summary>
        WTSClientDirectory = 11,

        /// <summary>
        /// A <B>USHORT</B> client-specific product identifier.
        /// </summary>
        WTSClientProductId = 12,

        /// <summary>
        /// A <B>ULONG</B> value that contains a client-specific hardware identifier. This option is reserved for future use. 
        /// <see cref="WTSQuerySessionInformation"/> will always return a value of 0.
        /// </summary>
        WTSClientHardwareId = 13,

        /// <summary>
        /// The network type and network address of the client. For more information, see WTS_CLIENT_ADDRESS.
        /// </summary>
        /// <remarks>The IP address is offset by two bytes from the start of the <B>Address</B> member of the WTS_CLIENT_ADDRESS structure.</remarks>
        WTSClientAddress = 14,

        /// <summary>
        /// Information about the display resolution of the client. For more information, see WTS_CLIENT_DISPLAY".
        /// </summary>
        WTSClientDisplay = 15,

        /// <summary>
        /// A USHORT value that specifies information about the protocol type for the session. This is one of the following values:<BR/>
        /// 0 - The console session.<BR/>
        /// 1 - This value is retained for legacy purposes.<BR/>
        /// 2 - The RDP protocol.<BR/>
        /// </summary>
        WTSClientProtocolType = 16,

        /// <summary>
        /// This value returns <B>FALSE</B>. If you call GetLastError() to get extended error information, <B>GetLastError</B> returns <B>ERROR_NOT_SUPPORTED</B>.
        /// </summary>
        /// <remarks>
        /// <B>Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:</B>  This value is not used.
        /// </remarks>
        WTSIdleTime = 17,

        /// <summary>
        /// This value returns <B>FALSE</B>. If you call GetLastError() to get extended error information, <B>GetLastError</B> returns <B>ERROR_NOT_SUPPORTED</B>.
        /// </summary>
        /// <remarks>
        /// <B>Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:</B>  This value is not used.
        /// </remarks>
        WTSLogonTime = 18,

        /// <summary>
        /// This value returns <B>FALSE</B>. If you call GetLastError() to get extended error information, <B>GetLastError</B> returns <B>ERROR_NOT_SUPPORTED</B>.
        /// </summary>
        /// <remarks>
        /// <B>Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:</B>  This value is not used.
        /// </remarks>
        WTSIncomingBytes = 19,

        /// <summary>
        /// This value returns <B>FALSE</B>. If you call GetLastError() to get extended error information, <B>GetLastError</B> returns <B>ERROR_NOT_SUPPORTED</B>.
        /// </summary>
        /// <remarks>
        /// <B>Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:</B>  This value is not used.
        /// </remarks>
        WTSOutgoingBytes = 20,

        /// <summary>
        /// This value returns <B>FALSE</B>. If you call GetLastError() to get extended error information, <B>GetLastError</B> returns <B>ERROR_NOT_SUPPORTED</B>.
        /// </summary>
        /// <remarks>
        /// <B>Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:</B>  This value is not used.
        /// </remarks>
        WTSIncomingFrames = 21,

        /// <summary>
        /// This value returns <B>FALSE</B>. If you call GetLastError() to get extended error information, <B>GetLastError</B> returns <B>ERROR_NOT_SUPPORTED</B>.
        /// </summary>
        /// <remarks>
        /// <B>Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:</B>  This value is not used.
        /// </remarks>
        WTSOutgoingFrames = 22,

        /// <summary>
        /// Information about a Remote Desktop Connection (RDC) client. For more information, see WTSCLIENT.
        /// </summary>
        /// <remarks>
        /// <B>Windows Vista, Windows Server 2003, and Windows XP:</B>  This value is not supported. 
        /// This value is supported beginning with Windows Server 2008 and Windows Vista with SP1.
        /// </remarks>
        WTSClientInfo = 23,

        /// <summary>
        /// Information about a client session on an RD Session Host server. For more information, see WTSINFO.
        /// </summary>
        /// <remarks>
        /// <B>Windows Vista, Windows Server 2003, and Windows XP:</B>  This value is not supported. 
        /// This value is supported beginning with Windows Server 2008 and Windows Vista with SP1.
        /// </remarks>
        WTSSessionInfo = 24,

        WTSSessionInfoEx = 25,
        WTSConfigInfo = 26,
        WTSValidationInfo = 27,
        WTSSessionAddressV4 = 28,
        WTSIsRemoteSession = 29,
      }

      public const WtsInfoClass WtsInfoClassMaxValue = WtsInfoClass.WTSIsRemoteSession;

      #endregion

      #region WTS_CLIENT_DISPLAY

      /// <summary>
      /// Structure for Terminal Service Session Client Display
      /// </summary>
      [StructLayout(LayoutKind.Sequential)]
      public struct WTS_CLIENT_DISPLAY
      {
        public int iHorizontalResolution;
        public int iVerticalResolution;
        //1 = The display uses 4 bits per pixel for a maximum of 16 colors.
        //2 = The display uses 8 bits per pixel for a maximum of 256 colors.
        //4 = The display uses 16 bits per pixel for a maximum of 2^16 colors.
        //8 = The display uses 3-byte RGB values for a maximum of 2^24 colors.
        //16 = The display uses 15 bits per pixel for a maximum of 2^15 colors.
        public int iColorDepth;
      }

      #endregion

      #region Функции

      /// <summary>
      /// The WTSGetActiveConsoleSessionId function retrieves the Remote Desktop Services session that
      /// is currently attached to the physical console. The physical console is the monitor, keyboard, and mouse.
      /// Note that it is not necessary that Remote Desktop Services be running for this function to succeed.
      /// </summary>
      /// <returns>The session identifier of the session that is attached to the physical console. If there is no
      /// session attached to the physical console, (for example, if the physical console session is in the process
      /// of being attached or detached), this function returns 0xFFFFFFFF.</returns>
      [DllImport("kernel32.dll", SetLastError = false)]
      public static extern Int32 WTSGetActiveConsoleSessionId();

      [DllImport("Wtsapi32.dll", SetLastError = true)]
      public static extern bool WTSQuerySessionInformation(
          System.IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass, out IntPtr ppBuffer, out uint pBytesReturned);

      [DllImport("wtsapi32.dll", ExactSpelling = true, SetLastError = false)]
      public static extern void WTSFreeMemory(IntPtr memory);

      #endregion
    }

    #endregion
  }
}
