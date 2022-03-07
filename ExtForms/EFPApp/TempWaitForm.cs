// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Logging;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// ����� ��� ������ �������� EFPApp.BeginWait()/EndWait
  /// </summary>
  internal partial class TempWaitForm : TempBaseForm
  {
    #region �����������

    public TempWaitForm()
    {
      InitializeComponent();
    }

    #endregion

    #region ����������� ����� �������

    /// <summary>
    /// ����������� ��������� �����
    /// </summary>
    private static TempWaitForm _TheForm = null;

    public static void BeginWait(string message, int imageIndex, bool updateImmediately)
    {
      #region �������� �� ���������

      if (_WaitInfoStack.Count > 0)
      {
        // ���� �����-���� �������� �� ������ - ����� ������ �� ����������� ������

        WaitInfo PrevObj = _WaitInfoStack.Peek();
        if (String.IsNullOrEmpty(message))
          message = PrevObj.Message;
        if (imageIndex < 0)
          imageIndex = PrevObj.ImageIndex;
      }
      else
      {
        if (String.IsNullOrEmpty(message))
          message = "�����";
        if (imageIndex < 0)
          imageIndex = EFPApp.MainImages.Images.IndexOfKey("HourGlass");
      }

      #endregion

      WaitInfo waitObj = new WaitInfo();
      waitObj.Message = message;
      waitObj.ImageIndex = imageIndex;

      _WaitInfoStack.Push(waitObj);

      try
      {
        if (_TheForm == null)
          _TheForm = new TempWaitForm();
        else if (_TheForm.IsDisposed)
          _TheForm = new TempWaitForm();

        _TheForm.TheLabel.Text = message;
        _TheForm.TheImg.Image = EFPApp.MainImages.Images[imageIndex];
        _TheForm.Visible = true;
        if (updateImmediately)
          _TheForm.Update(); // 11.12.2018 
      }
      catch (Exception e) // 06.06.2017
      {
        LogBeginEndWaitException(e);
      }
    }

    public static void EndWait()
    {
      //#if DEBUG
      //      CheckTread();
      //#endif

      // ������� ���� �� �����
      _WaitInfoStack.Pop();

      try
      {
        if (_WaitInfoStack.Count == 0)
          _TheForm.Visible = false;
        else
        {
          WaitInfo waitObj = _WaitInfoStack.Peek();
          _TheForm.TheLabel.Text = waitObj.Message;
          _TheForm.TheImg.Image = EFPApp.MainImages.Images[waitObj.ImageIndex];
        }
      }
      catch (Exception e) // 06.06.2017
      {
        LogBeginEndWaitException(e);
      }

    }

    #endregion

    #region ��������� ������

    /// <summary>
    /// ������� ������������ ������������ ������ � BeginWait() / EndWait()
    /// </summary>
    private static bool _BeginEndWaitErrorLogged = false;

    private static void LogBeginEndWaitException(Exception e)
    {
      if (_BeginEndWaitErrorLogged)
        return;
      _BeginEndWaitErrorLogged = true;
      LogoutTools.LogoutException(e, "�������� ������ EFPApp.BeginWait()/EndWait(). ��������� ��������� �� ���������");
    }

    #endregion

    #region ���� ���������

    /// <summary>
    /// ������ ��� ����� ���������. ��� ������� WaitCursor ��� ����������
    /// � ����� �������� �������� null
    /// </summary>
    private class WaitInfo
    {
      #region ����

      public string Message;
      public int ImageIndex;

      #endregion
    }

    /// <summary>                
    /// ���� �������
    /// </summary>
    private static Stack<WaitInfo> _WaitInfoStack = new Stack<WaitInfo>();

    #endregion
  }
}