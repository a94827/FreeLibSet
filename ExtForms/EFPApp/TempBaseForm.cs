using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.Logging;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// ������� ����� ��� TempMessageForm � TempWaitForm.
  /// ������������ ���������� ����� ������ ��������� ������ ��������� ����
  /// </summary>
  internal class TempBaseForm : Form
  {
    #region �����������

    public TempBaseForm()
    {
      //Enabled = false;
      ShowInTaskbar = false;
      //TopMost = true;
      StartPosition = FormStartPosition.Manual;
    }

    #endregion

    #region VisibleChanged

    protected override void OnVisibleChanged(EventArgs args)
    {
      base.OnVisibleChanged(args);

      if (DesignMode)
        return;

      DetachOwnerHandlers();

      if (Visible)
      {
        #region ����������� TheForm.Owner

        try
        {
          Owner = GetWantedOwner();
        }
        catch
        {
          Owner = null;
        }

        #endregion

        //System.Diagnostics.Trace.WriteLine("Owner=" + DataTools.GetString(Owner));
        if (Owner != null)
        {
          //System.Diagnostics.Trace.WriteLine("Owner.ContainsFocus=" + Owner.ContainsFocus.ToString());
          if (Owner.ContainsFocus)
            Owner.BringToFront();
        }

        if (Owner != null)
          AttachOwnerHandlers();
        else
        {
          Rectangle Rect = EFPApp.DefaultScreen.WorkingArea;
          SetFormPosition(Rect);
        }
        //BringToFront();
      }
      else
      {
        Owner = null;
      }
    }

    /// <summary>
    /// ����������, ����� ������������ �����
    /// </summary>
    /// <returns></returns>
    private Form GetWantedOwner()
    {
      Form WantedOwner;

      Form AF = Form.ActiveForm; // 21.12.2018. ��. ����������� � EFPApp.DialogOwnerWindow

      if (EFPApp.ActiveDialog != null)
        WantedOwner = EFPApp.ActiveDialog;
      else if (EFPApp.MainWindow != null)
        WantedOwner = EFPApp.MainWindow; // 21.09.2018
      else if (AF != null && (!object.ReferenceEquals(this, AF))) // 06.06.2017
        WantedOwner = AF;
      else
        WantedOwner = null;

      if (WantedOwner != null)
      {
        // 17.06.2018 - ���������, ��� �������� ����� ����������

        if (WantedOwner.IsDisposed)
          WantedOwner = null;
        else if (!WantedOwner.Visible)
          WantedOwner = null;
      }

      //if (WantedOwner != null)
      //{
      //  System.Diagnostics.Trace.WriteLine("WantedOwner.ContainsFocus=" + WantedOwner.ContainsFocus.ToString());
      //  if (WantedOwner.ContainsFocus)
      //    WantedOwner.Select();
      //}

      return WantedOwner;
    }

    protected override bool ShowWithoutActivation
    {
      get
      {
        return true;
      }
    }


    #endregion

    private void SetFormPosition(Rectangle rect)
    {
      this.Bounds = new Rectangle(rect.Left, rect.Bottom - 22, rect.Width, 22);
    }

    #region ����������� ��� �����-���������

    private void AttachOwnerHandlers()
    {
      Owner.LocationChanged += new EventHandler(Owner_BoundsChanged);
      Owner.SizeChanged += new EventHandler(Owner_BoundsChanged);
      Owner_BoundsChanged(null, null);
    }

    private void DetachOwnerHandlers()
    {
      #region ���������� ������� ����������� BoundsChanged

      try
      {
        if (Owner != null)
        {
          Owner.LocationChanged -= new EventHandler(Owner_BoundsChanged);
          Owner.SizeChanged -= new EventHandler(Owner_BoundsChanged);
        }
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e, "������ ���������� ����������� SizeChanged");
      }

      #endregion
    }

    private void Owner_BoundsChanged(object sender, EventArgs args)
    {
      if (this.Owner == null)
        return; // 04.10.2018
      Rectangle Rect = this.Owner.RectangleToScreen(this.Owner.ClientRectangle);
      SetFormPosition(Rect);
      //EFPApp.MessageBox(Rect.ToString()+Environment.NewLine+this.Bounds.ToString());
    }

    #endregion
  }
}