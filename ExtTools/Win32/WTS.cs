// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;
using FreeLibSet.Core;
using System.Collections;

namespace FreeLibSet.Win32
{
  #region Перечисление WTSConnectState

  /// <summary>
  /// Возможные значения свойства <see cref="WTSSession.ConnectionState"/>
  /// </summary>
  public enum WTSConnectState
  {
    /// <summary>
    /// A user is logged on to the WinStation.
    /// </summary>
    Active,

    /// <summary>
    /// The WinStation is connected to the client. 
    /// </summary>
    Connected,

    /// <summary>
    /// WTSConnectQuery
    /// </summary>
    ConnectQuery,

    /// <summary>
    /// The WinStation is shadowing another WinStation. 
    /// </summary>
    Shadow,

    /// <summary>
    /// The WinStation is active but the client is disconnected.
    /// </summary>
    Disconnected,

    /// <summary>
    /// The WinStation is waiting for a client to connect.
    /// </summary>
    Idle,

    /// <summary>
    /// The WinStation is listening for a connection. 
    /// A listener session waits for requests for new client connections. 
    /// No user is logged on a listener session. 
    /// A listener session cannot be reset, shadowed, or changed to a regular client session.
    /// </summary>
    Listen,

    /// <summary>
    /// The WinStation is being reset.
    /// </summary>
    Reset,

    /// <summary>
    /// The WinStation is down due to an error.
    /// </summary>
    Down,

    /// <summary>
    /// The WinStation is initializing.
    /// </summary>
    Init
  }

  #endregion

  #region Перечисление WTSColorDepth

  /// <summary>
  /// Значения свойства <see cref="WTSSession.ClientDisplayColorDepth"/>
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
  /// Значения свойства <see cref="WTSSession.ClientProtocolType"/>
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
  /// Подключение к серверу Remote Desktop Services.
  /// Вызывает функции Windows API WTSOpenServer()/WTSCloseServer() и хранит дескриптор открытого сервера.
  /// Используйте конструкцию using для своевременного уничтожения объекта.
  /// Для доступа к локальному серверу WTS_CURRENT_SERVER_HANDLE используйте статическое свойство <see cref="WTSServer.CurrentServer"/>
  /// </summary>
  public sealed class WTSServer : DisposableObject
  {
    #region Конструктор и Dispose()

    /// <summary>
    /// Вызывает функцию WTSOpenServer() для подключения к удаленному серверу
    /// </summary>
    /// <param name="serverName">Имя сервера. Должно быть задано</param>
    public WTSServer(string serverName)
    {
      if (String.IsNullOrEmpty(serverName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("serverName");

      _ServerName = serverName;

      _Handle = WTSNativeMethods.WTSOpenServer(serverName);
      if (_Handle == IntPtr.Zero)
        WTSNativeMethods.ThrowWin32Error();
    }

    private WTSServer(IntPtr handle)
    {
      _ServerName = String.Empty;
      _Handle = handle;
    }

    /// <summary>
    /// Если <see cref="Handle"/> не относится к локальному серверу, вызывает функцию WTSCloseServer()
    /// </summary>
    /// <param name="disposing">true, если был вызван метод <see cref="IDisposable.Dispose()"/>, а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && Object.ReferenceEquals(this, _CurrentServer))
        throw new InvalidOperationException();

      if (_Handle != IntPtr.Zero)
      {
        WTSNativeMethods.WTSCloseServer(_Handle);
        _Handle = IntPtr.Zero;
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Дескриптор открытого соединения с сервером
    /// </summary>
    public IntPtr Handle
    {
      get
      {
        CheckNotDisposed();
        return _Handle;
      }
    }
    private IntPtr _Handle;

    /// <summary>
    /// Имя сервера, заданное в конструкторе
    /// </summary>
    public string ServerName { get { return _ServerName; } }
    private readonly string _ServerName;

    /// <summary>
    /// Текущий сервер для дескриптора WTS_CURRENT_SERVER_HANDLE.
    /// Не вызывайте Dispose() для этого объекта.
    /// Содержит null, если операционная система не поддерживает Remote Desktop Services
    /// </summary>
    public static WTSServer CurrentServer
    {
      get
      {
        if (_CurrentServer == null && WTSSession.IsSupported)
          _CurrentServer = new WTSServer(WTSNativeMethods.WTS_CURRENT_SERVER_HANDLE);
        return _CurrentServer;
      }
    }
    private static WTSServer _CurrentServer = null;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (this == _CurrentServer)
        return "localhost";
      else
        return _ServerName;
    }

    #endregion

    #region Перебор сессий

    /// <summary>
    /// Коллекция сессий для сервера, которые можно перечислить.
    /// Реализация свойства <see cref="WTSServer.Sessions"/>,
    /// </summary>
    public struct SessionCollection : IEnumerable<WTSSession>
    {
      #region Конструктор

      internal SessionCollection(WTSServer server)
      {
        _Server = server;
      }
      private WTSServer _Server;

      #endregion

      #region IEnumerable<WTSSession>

      /// <summary>
      /// Создает перечислитель сессий
      /// </summary>
      /// <returns>Перечислитель</returns>
      public SessionEnumerator GetEnumerator()
      {
        _Server.CheckNotDisposed();
        return new SessionEnumerator(_Server);
      }

      IEnumerator<WTSSession> IEnumerable<WTSSession>.GetEnumerator()
      {
        return GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Возвращает количество сессий.
      /// Это свойство может динамически менять значение и предназначено исключительно для информационных целей.
      /// Может быть выброшено исключение при чтении значения.
      /// </summary>
      public int Count
      {
        get
        {
          using (SessionEnumerator en = new SessionEnumerator(_Server))
          {
            return en.Count;
          }
        }
      }

      /// <summary>
      /// Возвращает текст "Count=XXX" или сообщение об ошибке
      /// </summary>
      /// <returns>Текстовое представление</returns>
      public override string ToString()
      {
        try
        {
          return Count.ToString();
        }
        catch (Exception e)
        {
          return e.ToString();
        }
      }

      #endregion
    }

    /// <summary>
    /// Перечислитель по сессиям сервера
    /// </summary>
    public struct SessionEnumerator : IEnumerator<WTSSession>
    {
      internal SessionEnumerator(WTSServer server)
      {
        _Server = server;
        _ppSessionInfo = IntPtr.Zero;

        _Count = 0;
        Int32 retval = WTSNativeMethods.WTSEnumerateSessions(server.Handle, 0, 1, ref _ppSessionInfo, ref _Count);
        if (retval == 0)
          WTSNativeMethods.ThrowWin32Error();

        _Index = -1;
        _Current = null;
      }

      private readonly WTSServer _Server;
      private readonly IntPtr _ppSessionInfo;
      internal Int32 Count { get { return _Count; } }
      private readonly Int32 _Count;
      private int _Index;

      /// <summary>
      /// Освобождает системную память, используемую для перечисления
      /// </summary>
      public void Dispose()
      {
        if (_ppSessionInfo != IntPtr.Zero)
          WTSNativeMethods.WTSFreeMemory(_ppSessionInfo);
      }

      /// <summary>
      /// Возвращает текущую перебираемую сессиию
      /// </summary>
      public WTSSession Current { get { return _Current; } }
      private WTSSession _Current;

      object IEnumerator.Current { get { return _Current; } }

      private static readonly int DataSize = Marshal.SizeOf(typeof(WTSNativeMethods.WTS_SESSION_INFO));

      /// <summary>
      /// Переход к следующей сессии
      /// </summary>
      /// <returns>Наличие перебираемых объектов</returns>
      public bool MoveNext()
      {
        _Index++;
        if (_Index >= _Count)
        {
          _Current = null;
          return false;
        }

        IntPtr currPtr = new IntPtr(_ppSessionInfo.ToInt64() + (DataSize * _Index));
        WTSNativeMethods.WTS_SESSION_INFO si = (WTSNativeMethods.WTS_SESSION_INFO)Marshal.PtrToStructure(currPtr, typeof(WTSNativeMethods.WTS_SESSION_INFO));
        _Current = new WTSSession(_Server, si.SessionID);
        return true;
      }

      void IEnumerator.Reset()
      {
        _Index = -1;
        _Current = null;
      }
    }

    /// <summary>
    /// Перечисление сессий
    /// </summary>
    public SessionCollection Sessions { get { return new SessionCollection(this); } }

    #endregion
  }

  /// <summary>
  /// Информация о сессии Remote Desktop Services.
  /// Объект не "владеет" идентификатором сессии, поэтому нет метода Dispose().
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
    private static readonly bool _IsSupported;

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
      : this(WTSServer.CurrentServer, WTSNativeMethods.WTS_CURRENT_SESSION)
    {
    }

    /// <summary>
    /// Создает объект для WTS_CURRENT_SERVER_HANDLE и заданного идентификатора сессии
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии</param>
    public WTSSession(int sessionId)
      : this(WTSServer.CurrentServer, sessionId)
    {
    }

    /// <summary>
    /// Создает объект для заданного сервера и идентификатора сессии
    /// </summary>
    /// <param name="server">Открытый сервер</param>
    /// <param name="sessionId">Идентификатор сессии</param>
    public WTSSession(WTSServer server, int sessionId)
    {
      CheckIsSupported();
      if (server == null)
        throw new ArgumentNullException("server");

      _Server = server;
      if (sessionId == WTSNativeMethods.WTS_CURRENT_SESSION)
      {
        _SessionId = WTSNativeMethods.WTSGetActiveConsoleSessionId();
        if (_SessionId == (-1))
          throw new InvalidOperationException(Res.WTS_Err_NoCurrentSession);
      }
      else
        _SessionId = sessionId;
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Открытый сервер
    /// </summary>
    public WTSServer Server { get { return _Server; } }
    private readonly WTSServer _Server;

    /// <summary>
    /// Идентификатор сессии.
    /// </summary>
    public int SessionId { get { return _SessionId; } }
    private readonly int _SessionId;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_Server.IsDisposed)
        return "Server disposed";
      else
        return _Server.ToString() + ", SessionId=" + SessionId;
    }

    #endregion

    #region Свойства, получаемые из WTSQuerySessionInformation

    /// <summary>
    /// Name of the initial program that Remote Desktop Services runs when the user logs on.
    /// </summary>
    public string InitialProgram
    {
      [DebuggerStepThrough]
      get
      {
        try { return (string)GetInfoValue(WTSNativeMethods.WtsInfoClass.WTSInitialProgram); }
        catch { return String.Empty; }
      }
    }

    /// <summary>
    /// Published name of the application that the session is running. 
    /// Windows Server 2008 R2, Windows 7, Windows Server 2008 and Windows Vista:  This value is not supported
    /// </summary>
    public string ApplicationName
    {
      [DebuggerStepThrough]
      get
      {
        return (string)GetInfoValue(WTSNativeMethods.WtsInfoClass.WTSApplicationName);
      }
    }

    /// <summary>
    /// Default directory used when launching the initial program
    /// </summary>
    public string WorkingDirectory { get { return (string)GetInfoValue(WTSNativeMethods.WtsInfoClass.WTSWorkingDirectory); } }

    // WTSOEMId пропускаем

    // WTSSessionId пропускаем, т.к. инициализируется в конструкторе

    /// <summary>
    /// Name of the user associated with the session.
    /// </summary>
    public string UserName { get { return (string)GetInfoValue(WTSNativeMethods.WtsInfoClass.WTSUserName); } }

    /// <summary>
    /// Name of the Remote Desktop Services session. 
    /// Note: 
    /// Despite its name, specifying this type does not return the window station name. 
    /// Rather, it returns the name of the Remote Desktop Services session. 
    /// Each Remote Desktop Services session is associated with an interactive window station. 
    /// Because the only supported window station name for an interactive window station is "WinSta0", 
    /// each session is associated with its own "WinSta0" window station. 
    /// For more information, see Window Stations.
    /// </summary>
    public string WinStationName { get { return (string)GetInfoValue(WTSNativeMethods.WtsInfoClass.WTSWinStationName); } }

    /// <summary>
    /// Name of the domain to which the logged-on user belongs.
    /// </summary>
    public string DomainName { get { return (string)GetInfoValue(WTSNativeMethods.WtsInfoClass.WTSDomainName); } }

    /// <summary>
    /// The session's current connection state
    /// </summary>
    public WTSConnectState ConnectionState { get { return (WTSConnectState)GetInfoValue(WTSNativeMethods.WtsInfoClass.WTSConnectState); } }

    /// <summary>
    /// The build number of the client.
    /// </summary>
    public int ClientBuildNumber { get { return (int)GetInfoValue(WTSNativeMethods.WtsInfoClass.WTSClientBuildNumber); } }

    /// <summary>
    /// Name of the client.
    /// </summary>
    public string ClientName { get { return (string)GetInfoValue(WTSNativeMethods.WtsInfoClass.WTSClientName); } }

    /// <summary>
    /// Directory in which the client is installed.
    /// </summary>
    public string ClientDirectory { get { return (string)GetInfoValue(WTSNativeMethods.WtsInfoClass.WTSClientDirectory); } }

    /// <summary>
    /// Client-specific product identifier.
    /// </summary>
    public int ClientProductId { get { return (int)GetInfoValue(WTSNativeMethods.WtsInfoClass.WTSClientProductId); } }

    /// <summary>
    /// Client-specific hardware identifier. 
    /// This option is reserved for future use. 
    /// WTSQuerySessionInformation will always return a value of 0.
    /// </summary>
    public int ClientHardwareId { get { return (int)GetInfoValue(WTSNativeMethods.WtsInfoClass.WTSClientHardwareId); } }

    // WTSClientAddress пропускаем. Х.З. как это возвращать

    private WTSNativeMethods.WTS_CLIENT_DISPLAY ClientDisplay { get { return (WTSNativeMethods.WTS_CLIENT_DISPLAY)GetInfoValue(WTSNativeMethods.WtsInfoClass.WTSClientDisplay); } }

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
    public WTSClientProtocolType ClientProtocolType { get { return (WTSClientProtocolType)GetInfoValue(WTSNativeMethods.WtsInfoClass.WTSClientProtocolType); } }

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
        return WTSNativeMethods.WTSQuerySessionInformation(_Server.Handle, SessionId,
          WTSNativeMethods.WtsInfoClass.WTSIsRemoteSession, out dummyBuffer, out dummyCount);
      }
    }
    // не так работает public bool IsRemoteSession { get { return DataTools.GetBool(GetInfoValue(NativeMethods.WtsInfoClass.WTSIsRemoteSession)); } }

    [DebuggerStepThrough]
    private object GetInfoValue(WTSNativeMethods.WtsInfoClass infoClass)
    {
      object res;
      IntPtr buffer;
      uint byteCount;
      if (!WTSNativeMethods.WTSQuerySessionInformation(_Server.Handle, SessionId, infoClass,
        out buffer, out byteCount))
        WTSNativeMethods.ThrowWin32Error();

      try
      {
        res = DoGetInfoValue(infoClass, buffer);
      }
      finally
      {
        WTSNativeMethods.WTSFreeMemory(buffer);
      }
      return res;
    }

    private static object DoGetInfoValue(WTSNativeMethods.WtsInfoClass infoClass, IntPtr buffer)
    {
      switch (infoClass)
      {
        case WTSNativeMethods.WtsInfoClass.WTSInitialProgram:
        case WTSNativeMethods.WtsInfoClass.WTSApplicationName:
        case WTSNativeMethods.WtsInfoClass.WTSWorkingDirectory:
        case WTSNativeMethods.WtsInfoClass.WTSUserName:
        case WTSNativeMethods.WtsInfoClass.WTSWinStationName:
        case WTSNativeMethods.WtsInfoClass.WTSDomainName:
        case WTSNativeMethods.WtsInfoClass.WTSClientName:
        case WTSNativeMethods.WtsInfoClass.WTSClientDirectory:
          // строки
          return Marshal.PtrToStringAnsi(buffer);

        case WTSNativeMethods.WtsInfoClass.WTSConnectState:
          // enum
          return Marshal.PtrToStructure(buffer, typeof(Int32));

        case WTSNativeMethods.WtsInfoClass.WTSClientBuildNumber:
        case WTSNativeMethods.WtsInfoClass.WTSClientHardwareId:
          // ULONG
          return Marshal.PtrToStructure(buffer, typeof(Int32));

        case WTSNativeMethods.WtsInfoClass.WTSClientProductId:
        case WTSNativeMethods.WtsInfoClass.WTSClientProtocolType:
          // USHORT
          // Тут пакость. Метод PtrToStructure() не работает с двухбайтной структурой
          //return (int)Marshal.PtrToStructure(Buffer, typeof(UInt16));
          short[] a16 = new short[1];
          Marshal.Copy(buffer, a16, 0, 1);
          return (int)(a16[0]);

        case WTSNativeMethods.WtsInfoClass.WTSIsRemoteSession:
          // BOOLEAN (1 byte)
          byte[] a8 = new byte[1];
          Marshal.Copy(buffer, a8, 0, 1);
          return (int)(a8[0]);


        case WTSNativeMethods.WtsInfoClass.WTSClientDisplay:
          return Marshal.PtrToStructure(buffer, typeof(WTSNativeMethods.WTS_CLIENT_DISPLAY));

        default:
          throw new InvalidEnumArgumentException("infoClass", (int)infoClass, typeof(WTSNativeMethods.WtsInfoClass));
      }
    }

    #endregion
  }

  #region P/Invoke

  internal static class WTSNativeMethods
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

    #region WTS_SESSION_INFO

    [StructLayout(LayoutKind.Sequential)]
    public struct WTS_SESSION_INFO
    {
      public Int32 SessionID;

      [MarshalAs(UnmanagedType.LPStr)]
      public String pWinStationName;

      public WTSConnectState State;
    }

    #endregion

    #region Функции Windows API

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

    [DllImport("wtsapi32.dll", SetLastError = true)]
    public static extern IntPtr WTSOpenServer(string serverName);

    [DllImport("wtsapi32.dll")]
    public static extern void WTSCloseServer(IntPtr hServer);

    [DllImport("wtsapi32.dll", SetLastError = true)]
    public static extern Int32 WTSEnumerateSessions(
       IntPtr hServer,
       [MarshalAs(UnmanagedType.U4)] Int32 Reserved,
       [MarshalAs(UnmanagedType.U4)] Int32 Version,
       ref IntPtr ppSessionInfo,
       [MarshalAs(UnmanagedType.U4)] ref Int32 pCount);

    #endregion

    #region Дополнительно

    [DebuggerStepThrough]
    public static void ThrowWin32Error()
    {
      int res = Marshal.GetLastWin32Error();
      throw new System.ComponentModel.Win32Exception(res);
    }

    #endregion
  }

  #endregion
}
