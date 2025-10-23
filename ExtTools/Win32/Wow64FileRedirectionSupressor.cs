using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

namespace FreeLibSet.Win32
{

  /// <summary>
  /// Подавление подстановки каталогов (Redirection) в 64-разрядных версиях Windows.
  /// Использование:
  /// using(new FileRedirectionSupressor() )
  /// { 
  ///   // Код, в котором используются истинные имена без подстановок
  /// }
  /// Используются функции Windows Wow64DisableWow64FsRedirection() и Wow64RevertWow64FsRedirection().
  /// Если операционная система не поддерживает redirection (например, является 32-разрядной), код в блоке using выполняется без дополнительных действий.
  /// Также действия не выполняются, если приложение является 64-разрядным.
  /// </summary>
  public sealed class Wow64FileRedirectionSupressor : SimpleDisposableObject
  {
    #region Функции Widnows

    private static class WindowsNative
    {
      [DllImport("kernel32.dll")]
      internal extern static Int32 Wow64DisableWow64FsRedirection(out IntPtr oldValue);

      [DllImport("kernel32.dll")]
      internal extern static Int32 Wow64RevertWow64FsRedirection(IntPtr oldValue);
    }

    #endregion

    #region Конструктор и Dispose

    /// <summary>
    /// Создает объект.
    /// Если операционная система не поддерживает redirection, ничего не делается.
    /// </summary>
    public Wow64FileRedirectionSupressor()
    {
      if (OSSupported && IntPtr.Size == 4 /* 28.06.2023 */)
      {
        if (WindowsNative.Wow64DisableWow64FsRedirection(out _OldValue) != 0)
          _Active = true;
      }
    }

    /// <summary>
    /// Отключает подавление, если оно было включено.
    /// </summary>
    /// <param name="disposing">true, если был вызван метод <see cref="IDisposable.Dispose()"/>, а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (_Active)
      {
        _Active = false;
        WindowsNative.Wow64RevertWow64FsRedirection(_OldValue);
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если redirection был включен в конструкторе
    /// </summary>
    public bool Active { get { return _Active; } }
    private bool _Active;

    private readonly IntPtr _OldValue;

    #endregion

    #region Статическое свойство и метод

    /// <summary>
    /// Свойство возвращает true, если операционная система имеет функции для redirection.
    /// Разрядность приложения не учитывается.
    /// </summary>
    public static bool OSSupported
    {
      get
      {
        // 28.06.2023
        return Environment.OSVersion.Platform == PlatformID.Win32NT && EnvironmentTools.Is64BitOperatingSystem;

        //if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        //{
        //  //if (IntPtr.Size==8)
        //  //  return true;
        //  if (Environment.OSVersion.Version.Major > 6)
        //    return true;
        //  if (Environment.OSVersion.Version.Major < 6)
        //    return false;

        //  // Проверка для WinXP 64-bit - должно возвращать true
        //  return true;
        //}
        //return false;
      }
    }

    /// <summary>
    /// Свойство возвращает true, если есть redirection 
    /// </summary>
    public static bool TestRedirection
    {
      get
      {
        bool res;
        using (Wow64FileRedirectionSupressor obj = new Wow64FileRedirectionSupressor())
        {
          res = obj.Active;
        }
        return res;
      }
    }

    #endregion
  }
}
