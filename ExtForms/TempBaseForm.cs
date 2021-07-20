using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using AgeyevAV.Logging;

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// Базовый класс для TempMessageForm и TempWaitForm.
  /// Обеспечивает размещение формы поверх статусной строки активного окна
  /// </summary>
  internal class TempBaseForm : Form
  {
    #region Конструктор

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
        #region Определение TheForm.Owner

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
    /// Определить, какая существующая форма
    /// </summary>
    /// <returns></returns>
    private Form GetWantedOwner()
    {
      Form WantedOwner;

      Form AF = Form.ActiveForm; // 21.12.2018. См. исправление в EFPApp.DialogOwnerWindow

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
        // 17.06.2018 - проверяем, что активная форма нормальная

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

    #region Обработчики для формы-владельца

    private void AttachOwnerHandlers()
    {
      Owner.LocationChanged += new EventHandler(Owner_BoundsChanged);
      Owner.SizeChanged += new EventHandler(Owner_BoundsChanged);
      Owner_BoundsChanged(null, null);
    }

    private void DetachOwnerHandlers()
    {
      #region Отключение старого обработчика BoundsChanged

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
        LogoutTools.LogoutException(e, "Ошибка отключения обработчика SizeChanged");
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
