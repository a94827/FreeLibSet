using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// ��������� ���������� �������� EFPApp.ExternalDialogOwnerWindow
  /// �� ������� �������� ���� � Windows.
  /// Disposable-������ ������� ��������� �� ����� ���������� ����������� ������� �� �������� ����������,
  /// ��������, Excel, ����� ����� ������� ���������� ��� ������� �����������, � �� ��� ��������.
  /// 
  /// ��������� ������������ ������ � �������� ������ ����������, ������� ������ ��������� �������������
  /// �������.
  /// ����������� ������ ��� Windows, � ������ �� ������� �������� ���� �� �����������
  /// </summary>
  public class EFPExternalDialogOwnerWindowHandler : SimpleDisposableObject
  {
    // 03.01.2021
    // ����� �c���������� ������� ����� ��� �����������

    #region ����������� � Dispose

    /// <summary>
    /// �������������� �������� EFPApp.ExternalDialogOwnerWindow, ���� ��� �������������� ������������ ��������
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
    /// ��������������� �������� �������� EFPApp.ExternalDialogOwnerWindow, ��� ��� ����
    /// �� ������ ������ ������������
    /// </summary>
    /// <param name="disposing">true, ���� ��� ������ ����� Dispose(), � �� ����������</param>
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

    #region ����

    private System.Windows.Forms.IWin32Window _OldDialogOwnerWindow;

    private System.Windows.Forms.NativeWindow _NV;

    #endregion

    #region ������� Windows

    /// <summary>
    /// The GetForegroundWindow function returns a handle to the foreground window.
    /// </summary>
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    #endregion
  }
}