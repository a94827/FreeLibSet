using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  public class MessageBoxButtonsBox : ContainerControl
  {
    #region Конструктор

    public MessageBoxButtonsBox()
    {
      _Buttons = MessageBoxButtons.OKCancel;
      _ButtonControls = new Button[(int)(DialogResult.No) + 1];
      _Orientation = Orientation.Horizontal;
      _UseDialogResult = true;
      CreateButtonControls();
      PlaceControls();
    }

    private void CreateButtonControls()
    {
      base.Controls.Clear();
      ArrayTools.FillArray<Button>(_ButtonControls, null);

      switch (Buttons)
      {
        case MessageBoxButtons.OK:
          CreateButtonControl(DialogResult.OK);
          break;
        case MessageBoxButtons.OKCancel:
          CreateButtonControl(DialogResult.OK);
          CreateButtonControl(DialogResult.Cancel);
          break;
        case MessageBoxButtons.AbortRetryIgnore:
          CreateButtonControl(DialogResult.Abort);
          CreateButtonControl(DialogResult.Retry);
          CreateButtonControl(DialogResult.Ignore);
          break;
        case MessageBoxButtons.YesNoCancel:
          CreateButtonControl(DialogResult.Yes);
          CreateButtonControl(DialogResult.No);
          CreateButtonControl(DialogResult.Cancel);
          break;
        case MessageBoxButtons.YesNo:
          CreateButtonControl(DialogResult.Yes);
          CreateButtonControl(DialogResult.No);
          break;
        case MessageBoxButtons.RetryCancel:
          CreateButtonControl(DialogResult.Retry);
          CreateButtonControl(DialogResult.Cancel);
          break;
        default:
          throw new BugException("Buttons=" + Buttons.ToString());
      }
    }

    private void CreateButtonControl(DialogResult dialogRes)
    {
      Button btn = new Button();
      btn.Size = new System.Drawing.Size(88, 24);
      btn.Text = Res.ResourceManager.GetString("Btn_Text_" + dialogRes);
      btn.DialogResult = dialogRes;
      btn.Click += Btn_Click;
      base.Controls.Add(btn);
      _ButtonControls[(int)dialogRes] = btn;
    }

    private void Btn_Click(object sender, EventArgs args)
    {
      Button control = (Button)sender;
      Form frm = control.FindForm();
      if (frm == null)
        return;
      if (!frm.Visible)
        return;
      if (frm.Modal)
        return;
      frm.DialogResult = control.DialogResult;
      frm.Close();
      if (frm.Visible)
        frm.DialogResult = DialogResult.None;
    }

    private void PlaceControls()
    {
      System.Drawing.PointF pos = new System.Drawing.PointF(8 * this.AutoScaleFactor.Width, 8 * this.AutoScaleFactor.Height);
      foreach (Control ctrl in Controls)
      {
        ctrl.Location = System.Drawing.Point.Truncate(pos);

        if (Orientation == Orientation.Vertical)
          pos.Y += 24 * this.AutoScaleFactor.Height;
        else
          pos.X += 24 * this.AutoScaleFactor.Width;
      }
    }


    #endregion

    #region Основные свойства

    public MessageBoxButtons Buttons
    {
      get { return _Buttons; }
      set
      {
        if (value == _Buttons)
          return;
        CreateButtonControls();
        PlaceControls();
      }
    }
    private readonly MessageBoxButtons _Buttons;

    public Orientation Orientation
    {
      get { return _Orientation; }
      set
      {
        if (value == _Orientation)
          return;
        PlaceControls();
      }
    }
    private readonly Orientation _Orientation;

    public bool UseDialogResult
    {
      get { return _UseDialogResult; }
      set
      {
        if (value == _UseDialogResult)
          return;
      }
    }
    private readonly bool _UseDialogResult;

    #endregion

    #region Доступ к кнопкам

    /// <summary>
    /// Созданные кнопки.
    /// Индексом в массиве является перечисление <see cref="DialogResult"/>.
    /// Элемент с индексом 0 (<see cref="DialogResult.None"/>) не используется.
    /// </summary>
    private readonly Button[] _ButtonControls;

    public Button this[DialogResult button]
    {
      get
      {
        return _ButtonControls[(int)button];
      }
    }

    #endregion

    #region Переопределенные методы и свойства

    protected override void SetVisibleCore(bool value)
    {
      base.SetVisibleCore(value);

      Form frm = FindForm();
      if (frm != null)
      {
        if (value)
        {
          if (this[DialogResult.OK] != null)
            frm.AcceptButton = this[DialogResult.OK];
          else
            frm.AcceptButton = null;

          if (this[DialogResult.Cancel] != null)
            frm.CancelButton = this[DialogResult.Cancel];
          else
            frm.CancelButton = null;
        }
        else
        {
          frm.AcceptButton = null;
          frm.CancelButton = null;
        }
      }
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
      int controlCount = Controls.Count;
      if (controlCount == 0) // вызов из конструктора
        controlCount = 2;

      if (Orientation == Orientation.Vertical)
        return new Size(88 + 16, 8 + (24 + 8) * controlCount);
      else
        return new Size(8 + (24 + 8) * controlCount, 88 + 16);
    }

    protected override void OnDockChanged(EventArgs args)
    {
      base.OnDockChanged(args);
      switch (Dock)
      {
        case DockStyle.Top:
        case DockStyle.Bottom:
          Orientation = Orientation.Horizontal;
          break;
        case DockStyle.Left:
        case DockStyle.Right:
          Orientation = Orientation.Vertical;
          break;
      }
    }

    #endregion
  }
}
