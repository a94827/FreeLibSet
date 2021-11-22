// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace FreeLibSet.Win32
{
  /// <summary>
  /// Объект для предотвращения перехода системы в спящий режим
  /// Для ОС, начиная с Windows Vista, используется функция SetThreadExecutionState().
  /// Для ОС до Windows-XP используется сообщение WM_POWERBROADCAST.
  /// Для включения блокировки необходимо установить свойство Active в true.
  /// Также необходимо обрабатывать сообщения для главного окна программы
  /// </summary>
  public static class PowerSuspendLocker
  {
    #region Определения Windows

    const int WM_POWERBROADCAST = 0x0218;

    const int PBT_APMQUERYSUSPEND = 0x0;

    const int BROADCAST_QUERY_DENY = 0x424D5144;

    [DllImport("Kernel32.dll")]
    static extern int SetThreadExecutionState(int esFlags);

    #endregion

    #region Свойство Active

    /// <summary>
    /// Управляет подавлением спящего режима.
    /// Установка свойства вызывает SetThreadExecutionState()
    /// для Widnows Vista и новее.
    /// </summary>
    public static bool Active
    {
      get { return _Active; }
      set
      {
        if (value == _Active)
          return;
        _Active = value;

        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
          return;

        if (Environment.OSVersion.Version.Major >= 6) // Vista, Windows-7, ...
        {
          unchecked
          {
            const int ES_SYSTEM_REQUIRED = 0x00000001;
            const int ES_CONTINUOUS = (int)0x80000000;

            if (value)
              SetThreadExecutionState(ES_SYSTEM_REQUIRED | ES_CONTINUOUS);
            else
              SetThreadExecutionState(ES_CONTINUOUS);
          }
        }
      }
    }
    private static bool _Active = false;

    #endregion

    #region Обработка сообщений от главного окна

    /// <summary>
    /// Обработка сообщения WM_POWERBROADCAST.
    /// Выполняется, если Active=true.
    /// </summary>
    /// <param name="m">Оконное сообщение</param>
    public static void WndProc(ref Message m)
    {
      if (_Active &&
        m.Msg == WM_POWERBROADCAST && m.WParam.ToInt32() == PBT_APMQUERYSUSPEND)

        m.Result = new IntPtr(BROADCAST_QUERY_DENY);
    }

    #endregion

    #region Включение дисплея

    /// <summary>
    /// Включает дисплей, если он находится в выключенном режиме.
    /// Работает не на всех версиях Windows
    /// </summary>
    public static void TurnDisplayOn()
    {
      try
      {
        DoTurnDisplayOn();
      }
      catch { }
    }
    private static void DoTurnDisplayOn()
    {
      if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        return;

      if (Environment.OSVersion.Version.Major >= 5) // XP или старше. Под Windows-8 не работает
      { // Согласно документации, должно работать под Windows 2000 Professional
        // Но не знаю, какие еще версии бывают
        const int ES_DISPLAY_REQUIRED = 0x00000002;

        SetThreadExecutionState(ES_DISPLAY_REQUIRED);
      }
    }

    #endregion
  }
}
