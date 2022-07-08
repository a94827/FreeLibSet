// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Временный установщик свойства EFPApp.ExternalDialogOwnerWindow
  /// на текущее активное окно в Windows.
  /// Disposable-объект следует создавать на время выполнения обработчика события от внешнего приложения,
  /// например, Excel, чтобы блоки диалога выводились над внешним приложением, а не над основным.
  /// 
  /// Разрешено использовать только в основном потоке приложения, поэтому обычно требуется синхронизация
  /// потоков.
  /// Реализовано только для Windows, в других ОС никаких действий пока не выполняется
  /// </summary>
  public class EFPExternalDialogOwnerWindowHandler : SimpleDisposableObject
  {
    // 03.01.2021
    // Можно иcпользовать базовый класс без деструктора

    #region Конструктор и Dispose

    /// <summary>
    /// Инициализирует свойство EFPApp.ExternalDialogOwnerWindow, если это поддерживается операционной системой
    /// </summary>
    public EFPExternalDialogOwnerWindowHandler()
    {
#if DEBUG
      EFPApp.CheckMainThread();
#endif

      _OldDialogOwnerWindow = EFPApp.ExternalDialogOwnerWindow;

      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
          InitForWindows();
          break;
      }
    }

    private void InitForWindows()
    {
      _NV = new System.Windows.Forms.NativeWindow();
      _NV.AssignHandle(GetForegroundWindow());

      EFPApp.ExternalDialogOwnerWindow = _NV;
    }

    /// <summary>
    /// Восстанавливает значение свойства EFPApp.ExternalDialogOwnerWindow, как оно было
    /// на момент вызова конструктора
    /// </summary>
    /// <param name="disposing">true, если был вызван метод Dispose(), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        EFPApp.ExternalDialogOwnerWindow = _OldDialogOwnerWindow;

        if (_NV != null)
        {
          _NV.ReleaseHandle();
          _NV = null;
        }
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Поля

    private System.Windows.Forms.IWin32Window _OldDialogOwnerWindow;

    private System.Windows.Forms.NativeWindow _NV;

    #endregion

    #region Функции Windows

    /// <summary>
    /// The GetForegroundWindow function returns a handle to the foreground window.
    /// </summary>
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    #endregion
  }
}
