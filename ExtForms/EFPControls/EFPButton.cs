// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// ���������� ��� Button
  /// </summary>
  public class EFPButton : EFPTextViewControl<Button>
  {
    #region �����������

    /// <summary>
    /// ������� ��������� ������������ ��������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� �������</param>
    public EFPButton(EFPBaseProvider baseProvider, Button control)
      : base(baseProvider, control, false)
    {
      if (!DesignMode)
        control.Click += new EventHandler(Control_Click);
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���� �������� ���������� � true, �� ����� ������������� �������� �����, ���� ����������� ����������
    /// ������� EFPButton.Click. ����� ��������������� ������� ������ ������, ��� ������� ��� ��������
    /// �����������.
    /// �� ��������� �������� �� �����������, �.�. ���������� �����, � ����� ������, ��������� ��������
    /// �����.
    /// �������� �� ����� ��������������� �� ������ ����������� ������� Click.
    /// </summary>
    /// <remarks>
    /// �������� �� ������ �� ����� ������������ ����������� Button.Click.
    /// ��������� ����� ������� EFPButton.Click ��� ��� �� ������ ��������������� ������.
    /// </remarks>
    public bool PreventFormClosing
    {
      get { return _PreventFormClosing; }
      set
      {
        if (_InsideClick)
          throw new InvalidOperationException("�������� �� ����� ��������������� �� ����������� ������� Click");
        _PreventFormClosing = value;
      }
    }
    private bool _PreventFormClosing;

    #endregion

    #region SetImage

    /// <summary>
    /// ����������� ������ ����������� �� ������ EFPApp.MainImages.
    /// ������������� �������� Button.ImageAlign ��� ������������ �� ������ ����, ���� � ������ ���� �����
    /// � �� ������, ���� ������ �������� ������ ������ ��� ������
    /// </summary>
    /// <param name="imageKey">��� ����������� �� ������ EFPApp.MainImages</param>
    public void SetMainImageKey(string imageKey)
    {
      if (String.IsNullOrEmpty(imageKey))
        Control.Image = null;
      else
      {
        Control.Image = EFPApp.MainImages.Images[imageKey];
        if (String.IsNullOrEmpty(Control.Text))
          Control.ImageAlign = ContentAlignment.MiddleCenter;
        else
          Control.ImageAlign = ContentAlignment.MiddleLeft;
      }
    }

    #endregion

    #region ������� Click

    /// <summary>
    /// ������� ���������� ��� ������� ������.
    /// � ������� �� ������������� ������� Button.Click, ��� ������������� ����������
    /// � ����������� ��������� ���� ��������� �� ������, � �� ���������� ���������
    /// ���������� ���������
    /// </summary>
    public event EventHandler Click;

    void Control_Click(object sender, EventArgs args)
    {
      try
      {
        DoClick();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "������ ��� ������� ������ \"" + DisplayName + "\"");
      }
    }

    private void DoClick()
    {
      if (_InsideClick)
      {
        EFPApp.ShowTempMessage("���������� ������� ������ \"" + DisplayName + "\" ��� �� ����������");
        return;
      }

      // ��� ������, ������ ����������� ����� ���� ���� ��������� Enabled
      //bool OldEnabled = Enabled;
      //Enabled = false;
      _InsideClick = true;
      try
      {
        if (PreventFormClosing)
        {
          if (BaseProvider.ReentranceLocker.TryLock("������� ������ \"" + DisplayName + "\""))
          {
            try
            {
              OnClick();
            }
            finally
            {
              BaseProvider.ReentranceLocker.Unlock();
            }
          }
        }
        else // !PreventFormClosing
          OnClick();
      }
      finally
      {
        _InsideClick = false;
        //Enabled = OldEnabled;
      }
    }

    private bool _InsideClick = false;

    /// <summary>
    /// �������� ���������� ������� Click, ���� �� �����������
    /// </summary>
    protected virtual void OnClick()
    {
      if (Click != null)
        Click(this, EventArgs.Empty);
    }

    #endregion
  }

  /// <summary>
  /// ������ � ���������� ����, ������������� ��� ������� ������
  /// ��� ���������� ������ ���� ����� ������������ �������� CommandItems.
  /// ����� ��������������, ��� ���������������� �������� ContextMenuStrip ��� ContextMenu
  /// </summary>
  public class EFPButtonWithMenu : EFPButton
  {
    #region �����������

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� �������</param>
    public EFPButtonWithMenu(EFPBaseProvider baseProvider, Button control)
      : base(baseProvider, control)
    {
      //control.Image = EFPApp.MainImages.Images["MenuButton"];
      //if (String.IsNullOrEmpty(control.Text))
      //  control.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter; // 27.02.2020
      //else
      //  control.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft; // 27.02.2020
      SetMainImageKey("MenuButton"); // 05.11.2020
    }

    #endregion

    #region ������� ������

    /// <summary>
    /// ���������� false, ��� ��� � ��������� ���� �� ����� ��������� ������� �� ������������� ��������.
    /// </summary>
    protected override bool BaseCommandItemsNeeded { get { return false; } }

    /// <summary>
    /// ���������� ���������� ���� ����� � �������.
    /// ���������� ������� EFPButton.Click �� ����������, ���� ���� ��������� ����, ������� ����� ��������.
    /// </summary>
    protected override void OnClick()
    {
      Point startPos = new Point(Control.Width, Control.Height);
      if (Control.ContextMenuStrip != null)
        Control.ContextMenuStrip.Show(Control, startPos);
      else if (Control.ContextMenu != null)
        Control.ContextMenu.Show(Control, startPos);
      else
        base.OnClick();
    }

    #endregion
  }

  /// <summary>
  /// ������ ��� ����������� ������ ������ ErrorMessageList � ������� EFPApp.ShowErrorMessageListDialog
  /// </summary>
  public class EFPErrorMessageListButton : EFPButton
  {
    #region �����������

    /// <summary>
    /// ������� ��������� ������������ ��������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� �������</param>
    public EFPErrorMessageListButton(EFPBaseProvider baseProvider, Button control)
      : base(baseProvider, control)
    {
      _AutoText = !String.IsNullOrEmpty(control.Text);
      _AutoImageKey = true;
      _AutoToolTipText = true;
      _AutoEnabled = true;
      _AutoValidate = true;

      if (String.IsNullOrEmpty(control.Text))
        DisplayName = "���������";
    }

    #endregion

    #region �������� ������ ���������

    /// <summary>
    /// �������������� ������ ��������� �� �������.
    /// ��������� �������� ������ ������ � ����� ������
    /// </summary>
    public ErrorMessageList ErrorMessages
    {
      get { return _ErrorMessages; }
      set
      {
        _ErrorMessages = value;
        if (HasBeenCreated)
          OnInitState();
        Validate();
      }
    }
    private ErrorMessageList _ErrorMessages;


    /// <summary>
    /// ������ ������ ������� "���" � ��������. ��.�������� EFPDataGridViewColumn.TextWidth.
    /// ������� �������� (�� ���������) ������ ������� �������.
    /// </summary>
    public int CodeWidth
    {
      get
      {
        return _CodeWidth;
      }
      set
      {
#if DEBUG
        if (value < 0 || value > 20)
          throw new ArgumentException("������������ ������ ������� \"���\": " + CodeWidth);
#endif
        _CodeWidth = value;
      }
    }
    private int _CodeWidth;

    /// <summary>
    /// ������� ����������� �������������� ������ � �������.
    /// ����������, ����� ������������ ��������� ������� "�������������" ��� ������ �������.
    /// ��������� �������������� ���������� ����� �� ��������������.
    /// ���������� �������� ���� ��������� �� ������ ErrorMessages, � �������� ��������� �������
    /// ������ ���������� ���������.
    /// ���������� ����� �������� ��������� ��������� �� ������, ���� ������� � �������, � ��������
    /// ��������� ���������.
    /// �������� �� ������ ���������������, ���� "��������������" �� ����� ������. ��� ����
    /// ������� �������������� �� ����� ��������, ���� �� ���������� ���������� ������� EditData.
    /// 
    /// ���� "��������������" ����� ����� ������ ��� ��������� ��������� �� ������ (����� ���� 
    /// �������� ������� � ���������� �������), � ��� ��������� - ���, �� �������������� ����������
    /// ������ �������� ������������ �����-���� ���������, ����� ������� ����������.
    /// </summary>
    public ErrorMessageItemEventHandler EditHandler
    {
      get { return _EditHandler; }
      set { _EditHandler = value; }
    }
    private ErrorMessageItemEventHandler _EditHandler;

    #endregion

    #region �������� ��� ���������� �������

    /// <summary>
    /// ���� �������� ����������� � true (�� ���������), �� ����� ������
    /// ��������������� ������������� ��� ������������� ������. ����� ����������
    /// �������� Button.Text �������, ������� ������� ������ AutoText=false.
    /// ��������� �������� ������� ��������������� � true, ���� Button.Text ������ �������� �����.
    /// ���� � ������ ��� ������, �� �������� �������� �������� false.
    /// �������� ����� ��������������� ������ �� ������ ������ �� �����.
    /// </summary>
    public bool AutoText
    {
      get { return _AutoText; }
      set
      {
        CheckHasNotBeenCreated();
        _AutoText = value;
      }
    }
    private bool _AutoText;

    /// <summary>
    /// ���� �������� ����������� � true (�� ���������), �� ������ ������
    /// ��������������� ������������� ��� ������������� ������. ����� ����������
    /// �������� ImageKey �������, ������� ������� ������ AutoImageKey=false.
    /// �������� ����� ��������������� ������ �� ������ ������ �� �����.
    /// </summary>
    public bool AutoImageKey
    {
      get { return _AutoImageKey; }
      set
      {
        CheckHasNotBeenCreated();
        _AutoImageKey = value;
      }
    }
    private bool _AutoImageKey;

    /// <summary>
    /// ���� �������� ����������� � true (�� ���������), �� ����������� ��������� ��������� ��������
    /// ��������������� ������������� ��� ������������� ������. ����� ����������
    /// �������� ToolTipText �������, ������� ������� ������ AutoToolTipText=false.
    /// �������� ����� ��������������� ������ �� ������ ������ �� �����.
    /// </summary>
    public bool AutoToolTipText
    {
      get { return _AutoToolTipText; }
      set
      {
        CheckHasNotBeenCreated();
        _AutoToolTipText = value;
      }
    }
    private bool _AutoToolTipText;

    /// <summary>
    /// ���� �������� ����������� � true (�� ���������), �� ������ ����� ������������� �������������, 
    /// ���� ������ ������ �� ����������� ��� �� �������� ���������. ����� ������������� 
    /// �������� Enabled �������, ��� ������������ EnabledEx, ������� ������� ������ AutoEnabled=false.
    /// �������� ����� ��������������� ������ �� ������ ������ �� �����.
    /// </summary>
    public bool AutoEnabled
    {
      get { return _AutoEnabled; }
      set
      {
        CheckHasNotBeenCreated();
        _AutoEnabled = value;
      }
    }
    private bool _AutoEnabled;

    /// <summary>
    /// ���� �������� ����������� � true (�� ���������), �� ��� ������� � ������ ������ ��� �������������� ����� ��������������� ��������������� ��������
    /// �������� ValidateState. ��� ������� ������, ��������, ������ ����� ������ ������ "��".
    /// ���� �������� �������� � false, �� ValidateState ����� ���������� ������ Ok, ���������� �� ������ ������
    /// </summary>
    public bool AutoValidate
    {
      get { return _AutoValidate; }
      set
      {
        if (value == _AutoValidate)
          return;
        _AutoValidate = value;
        Validate();
      }
    }
    private bool _AutoValidate;

    #endregion

    #region ��������� ���������

    /// <summary>
    /// ���������� ��� ������ ����� �� �����
    /// </summary>
    protected override void OnCreated()
    {
      base.OnCreated();
      if (_AutoImageKey)
        Control.ImageList = EFPApp.MainImages;
      OnInitState();
    }

    private void OnInitState()
    {
      if (_AutoEnabled)
      {
        if (_ErrorMessages == null)
          Enabled = false;
        else
          Enabled = _ErrorMessages.Count > 0;
      }
      if (_AutoText)
        Text = EFPApp.GetErrorTitleText(_ErrorMessages);
      if (_AutoImageKey)
        Control.ImageKey = EFPApp.GetErrorImageKey(_ErrorMessages);
      if (_AutoToolTipText)
        ValueToolTipText = EFPApp.GetErrorToolTipText(_ErrorMessages);
    }

    #endregion

    #region ������� ������

    /// <summary>
    /// ��� ������� ������ ���������� EFPApp.ShowErrorMessageListDialog()
    /// </summary>
    protected override void OnClick()
    {
      base.OnClick();
      EFPApp.ShowErrorMessageListDialog(ErrorMessages, DisplayName, CodeWidth, EditHandler);
    }

    #endregion

    #region OnValidate()

    /// <summary>
    /// ��������� �������� ��� ������������� �������� AutoValidate
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (!AutoValidate)
        return;

      if (_ErrorMessages == null)
        return;


      switch (_ErrorMessages.Severity)
      {
        case ErrorMessageKind.Error:
          if (_ErrorMessages.Count <= 5)
          {
            ErrorMessageList lst2 = _ErrorMessages.Clone(ErrorMessageKind.Error); // 26.10.2020
            SetError(lst2.AllText);
          }
          else
            SetError("������ �������� ������ (" + _ErrorMessages.ErrorCount.ToString() + ")");
          break;

        case ErrorMessageKind.Warning:
          if (_ErrorMessages.Count <= 5)
          {
            ErrorMessageList lst2 = _ErrorMessages.Clone(ErrorMessageKind.Warning); // 26.10.2020
            //SetWarning(_ErrorMessages.AllText);
            SetWarning(lst2.AllText); // 05.01.2021
          }
          else
            SetWarning("������ �������� �������������� (" + _ErrorMessages.WarningCount.ToString() + ")");
          break;
      }
    }

    #endregion
  }
}
