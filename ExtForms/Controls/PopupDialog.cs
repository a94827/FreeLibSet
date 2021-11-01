using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

#if XXX
namespace FreeLibSet.Forms
{
  /// <summary>
  /// 
  /// </summary>
  public abstract class PopupDialog
  {
#region ����� PopupForm

    /// <summary>
    /// ������� ����� ��� ���������
    /// </summary>
    protected class PopupForm : Form
    {
#region �����������

      public PopupForm()
      {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        FormButtonStub.AssignCancel(this);
        KeyPreview = true;

        FMainPanel = new Panel();
        FMainPanel.BorderStyle = BorderStyle.Fixed3D;
        Controls.Add(FMainPanel);
      }

#endregion

#region ��������

      /// <summary>
      /// �������� ������, ���������� ��� �����.
      /// ����� �������������� ��� �������� �����
      /// </summary>
      public Panel MainPanel { get { return FMainPanel; } }
      private Panel FMainPanel;

      internal PopupDialog OwnerDialog;

#endregion

#region ����������� �����

      protected override void OnLoad(EventArgs Args)
      {
        base.OnLoad(Args);
        if (MainPanel.Controls.Count != 1)
          throw new InvalidOperationException("������ ���� ������ ���� ����������� �������");

        Control TheControl = MainPanel.Controls[0];
        TheControl.PreviewKeyDown += new PreviewKeyDownEventHandler(TheControl_PreviewKeyDown);
        TheControl.KeyDown += new KeyEventHandler(TheControl_KeyDown);
        TheControl.SizeChanged += new EventHandler(TheControl_SizeChanged);
      }

      protected override bool ShowWithoutActivation
      {
        get { return false; }
      }

      protected override void OnDeactivate(EventArgs Args)
      {
        base.OnDeactivate(Args);

        // �������� ����� ��� ������� �������� ����
        try
        {
          if (Visible)
            Hide();
        }
        catch
        {
        }
      }

#endregion

#region ����������� ��������� ������������ ��������

      void TheControl_PreviewKeyDown(object Sender, PreviewKeyDownEventArgs Args)
      {
        if (Args.KeyCode == Keys.Tab || Args.KeyCode == Keys.Escape)
        {
          if ((!Args.Alt) && (!Args.Control))
            Args.IsInputKey = true;
        }
      }

      void TheControl_KeyDown(object Sender, KeyEventArgs Args)
      {
        try
        {
          switch (Args.KeyCode)
          {
            case Keys.Return:
              Owner.Value = SelectionStart;
              Owner.MainControl.SelectAll();
              FindForm().Hide();
              Args.Handled = true;
              return;
            case Keys.Tab:
              FindForm().Hide();
              Args.Handled = true;
              Owner.FindForm().SelectNextControl(Owner, !Args.Shift, true, true, true);
              return;
            case Keys.Escape:
              FindForm().Hide();
              return;
          }
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "������ ������ OnDateSelected");
        }
      }

      void TheControl_SizeChanged(object Sender, EventArgs Args)
      {
        try
        {
          Control TheControl = (Control)Sender;
          MainPanel.ClientSize = TheControl.Size;
          this.Size = MainPanel.Size;
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "������ ��������� ��������");
        }
      }

#endregion
    }

#endregion


#region �����������

#endregion

#region ��������

    /// <summary>
    /// �������� ����������� �������, ������������ �������� ����� ������� ����������� ����
    /// </summary>
    public Control ParentControl
    {
      get { return FParentControl; }
      set
      {
        FParentControl = value;
        if (value != null && value.Parent != null)
          FParentRectangle = value.Parent.RectangleToScreen(value.Bounds);
      }
    }
    private Control FParentControl;

    /// <summary>
    /// �������� ���������� ������������� �������, ������������ ������� ������ ���� �������� ����.
    /// ��� �������� ������ ��������������, ���� ������ ������ ����������� �������-��������
    /// </summary>
    public Rectangle ParentRectangle
    {
      get { return FParentRectangle; }
      set { FParentRectangle = value; }
    }
    private Rectangle FParentRectangle;

#endregion

#region ����� �������

    /// <summary>
    /// ������� �����
    /// </summary>
    /// <returns></returns>
    public abstract DialogResult ShowDialog();

    /// <summary>
    /// ���� ����� ������ �������������� � ���������� ShowDialog() ��� ������ popup-�����.
    /// ����� ������ ������ � ���������� �������� ������ ���� ������ Form.Dispose()
    /// </summary>
    /// <param name="Form">��������� ����� ������, ������������ �� PopupForm</param>
    /// <returns></returns>
    protected DialogResult ShowPopupForm(PopupForm Form)
    {
      Form.OwnerDialog = this;
      DialogResult Res = Form.ShowDialog();
      return Res;
    }

#endregion
  }

  /// <summary>
  /// ����������� ����������� ���� ��� ������ ���� � ������� �����������
  /// </summary>
  public class DatePopupDialog : PopupDialog
  {
#region �����������

    public DatePopupDialog()
    {
      FValue = DateTime.Today;
    }

#endregion

#region ��������

    /// <summary>
    /// ������� ������������� ����.
    /// �� ��������� - ������� ����
    /// � ������� �� DateInputDialog, �������� �� ����� ���� null.
    /// </summary>
    public DateTime Value
    {
      get { return FValue; }
      set { FValue = value; }
    }
    private DateTime FValue;

#endregion

#region ���������������� �����


#region ����� MyCalendar

    private class MyCalendar : MonthCalendar
    {
      protected override void OnDateSelected(DateRangeEventArgs drevent)
      {
        try
        {
          base.OnDateSelected(drevent);
          FindForm().DialogResult = DialogResult.OK;
          FindForm().Close();
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "������ ������ OnDateSelected");
        }
      }
    }

#endregion

    private class DatePopupForm : PopupForm
    {
#region �����������

      public DatePopupForm()
      {
        Calendar = new MyCalendar();
        base.MainPanel.Controls.Add(Calendar);
      }

#endregion

#region ��������

      public MyCalendar Calendar;

#endregion
    }

    public override DialogResult ShowDialog()
    {
      DialogResult Res;
      using (DatePopupForm Form = new DatePopupForm())
      {
        Form.Calendar.SelectionStart = Value;
        Form.Calendar.SelectionEnd = Value;
        Res = base.ShowPopupForm(Form);
        if (Res == DialogResult.OK)
          Value = Form.Calendar.SelectionStart;
      }

      return Res;
    }

#endregion
  }
}

#endif