﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Маленькая кнопка без фокуса ввода для размещения справа от управляющего элемента
  /// </summary>
  [ToolboxItem(false)]
  public class ControlRightButton : Button
  {
    #region Конструктор

    public ControlRightButton()
    {
      SetStyle(ControlStyles.Selectable, false);
      SetStyle(ControlStyles.FixedHeight, true);
      SetStyle(ControlStyles.FixedWidth, true);

      base.Text = String.Empty;
      base.ImageAlign = ContentAlignment.MiddleCenter;
      base.TextImageRelation = TextImageRelation.Overlay;
      base.UseMnemonic = false;
      base.UseVisualStyleBackColor = true;
      base.Padding = Padding.Empty;

      if (!Application.RenderWithVisualStyles) // 23.08.2019
      {
        base.UseVisualStyleBackColor = false;
        base.BackColor = SystemColors.Control;
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_TheToolTip != null)
        {
          _TheToolTip.Dispose();
          _TheToolTip = null;
        }
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    [DefaultValue(false)]
    [Description("Разрешает получение кнопкой фокуса ввода")]
    [Browsable(true)]
    [Category("Behavior")]
    public bool Selectable
    {
      get { return _Selectable; }
      set
      {
        if (value == _Selectable)
          return;

        SetStyle(ControlStyles.Selectable, value);
        _Selectable = value;

        OnSelectableChanged(EventArgs.Empty);
      }
    }

    private bool _Selectable;

    [Description("Вызывается при изменении свойства Selectable")]
    [Category("PropertyChanged")]
    public event EventHandler SelectableChanged;

    protected void OnSelectableChanged(EventArgs args)
    {
      if (SelectableChanged != null)
        SelectableChanged(this, args);
    }

    [DefaultValue("")]
    [Description("Всплывающая подсказка для кнопки")]
    [Browsable(true)]
    [Category("Appearance")]
    public string ToolTipText
    {
      get
      {
        if (_TheToolTip == null)
          return String.Empty;
        else
          return _TheToolTip.GetToolTip(this);
      }
      set
      {
        if (String.IsNullOrEmpty(value))
        {
          if (_TheToolTip == null)
            return;
        }
        if (_TheToolTip == null)
          _TheToolTip = new ToolTip();
        _TheToolTip.SetToolTip(this, value);
      }
    }

    private ToolTip _TheToolTip;

    [DefaultValue(false)]
    [Description("Прорисовка кнопки ComboBox вместо заданного изображения")]
    [Browsable(true)]
    [Category("Appearance")]
    public bool ComboBoxButton
    {
      get { return _ComboBoxButton; }
      set
      {
        if (value == _ComboBoxButton)
          return;

        _ComboBoxButton = value;
        base.UseVisualStyleBackColor = (!value) && Application.RenderWithVisualStyles /* 23.08.2019 */;
        OnComboBoxButtonChanged(EventArgs.Empty);
      }
    }
    private bool _ComboBoxButton;

    [Description("Вызывается при изменении свойства ComboBoxButton")]
    [Category("PropertyChanged")]
    public event EventHandler ComboBoxButtonChanged;

    protected void OnComboBoxButtonChanged(EventArgs args)
    {
      if (ComboBoxButtonChanged != null)
        ComboBoxButtonChanged(this, args);
    }

    #endregion

    #region Внутренняя реализация

    protected override void OnPaint(PaintEventArgs args)
    {
      if (_ComboBoxButton)
      {
        bool HasMouse = false;
        bool MousePressed = false;

        if (Capture)
        {
          HasMouse = true;
          MousePressed = true;
        }
        Point pt = base.PointToClient(Cursor.Position);
        if (base.ClientRectangle.Contains(pt))
        {
          HasMouse = true;
        }
        if ((MouseButtons & MouseButtons.Left) != MouseButtons.Left)
          MousePressed = false;

        if (ComboBoxRenderer.IsSupported /*&&
          Enabled*/) // 26.08.2013, убрано 04.01.2021
        {
          // 19.09.2013
          // Для некоторых тем оформления (CodeOpus для Windows-XP)
          // при рисовании галочки не закрашивается фон. Получаются "козявки"
          // Лучше закрасить прямоугольник серым цветом
          //
          // На самом деле кнопка может располагаться на панели с какой-нибудь хитрой
          // заливкой
          args.Graphics.FillRectangle(SystemBrushes.Control, ClientRectangle);

          // 04.01.2021
          // ComboBoxState не является двоичной комбинацией флажков, как ButtonState
          //System.Windows.Forms.VisualStyles.ComboBoxState cbs = System.Windows.Forms.VisualStyles.ComboBoxState.Normal;
          ////if (!Enabled)
          ////  cbs |= System.Windows.Forms.VisualStyles.ComboBoxState.Disabled;
          //if (HasMouse)
          //  cbs |= System.Windows.Forms.VisualStyles.ComboBoxState.Hot;
          //if (MousePressed)
          //  cbs |= System.Windows.Forms.VisualStyles.ComboBoxState.Pressed;

          System.Windows.Forms.VisualStyles.ComboBoxState cbs;
          if (!Enabled)
            cbs = System.Windows.Forms.VisualStyles.ComboBoxState.Disabled; // 04.01.2021: И это нормально рисуется
          else if (MousePressed)
            cbs = System.Windows.Forms.VisualStyles.ComboBoxState.Pressed;
          else if (HasMouse)
            cbs = System.Windows.Forms.VisualStyles.ComboBoxState.Hot;
          else
            cbs = System.Windows.Forms.VisualStyles.ComboBoxState.Normal;

          ComboBoxRenderer.DrawDropDownButton(args.Graphics, ClientRectangle, cbs);
        }
        else
        {
          ButtonState bs = ButtonState.Normal;
          if (!Enabled)
          {
            bs |= ButtonState.Inactive;
            bs |= ButtonState.Flat; // 26.08.2016
          }
          if (MousePressed)
            bs |= ButtonState.Pushed;
          ControlPaint.DrawComboButton(args.Graphics, ClientRectangle, bs);
        }
      }
      else
      {
        base.OnPaint(args);
      }
    }

    protected override Size DefaultSize
    {
      get
      {
        int w = SystemInformation.VerticalScrollBarWidth;
        if (!_ComboBoxButton)
        {
          if (w < 16)
            w = 16; // Чтобы картинка поместилась

          w += 2 * SystemInformation.BorderSize.Width;
          //w += (base.Size.Width - base.ClientSize.Width);
        }
        return new Size(w, 21);
      }
    }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
      Size sz = DefaultSize;
      switch (Dock)
      {
        case DockStyle.Left:
        case DockStyle.Right:
          width = sz.Width;
          break;
        case DockStyle.Top:
        case DockStyle.Bottom:
          height = sz.Height;
          break;
        case DockStyle.None:
          width = sz.Width;
          height = sz.Height;
          break;
      }
      base.SetBoundsCore(x, y, width, height, specified);
    }

    public new void PerformClick()
    {
      OnClick(EventArgs.Empty);
    }

    #endregion

    #region Заглушки для свойств

    [DefaultValue("")]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string Text
    {
      get
      {
        return String.Empty;
      }
      set
      {
      }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new System.Drawing.ContentAlignment TextAlign { get { return base.TextAlign; } set { base.TextAlign = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new System.Drawing.ContentAlignment ImageAlign { get { return base.ImageAlign; } set { base.ImageAlign = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new TextImageRelation TextImageRelation { get { return base.TextImageRelation; } set { base.TextImageRelation = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool UseMnemonic { get { return base.UseMnemonic; } set { base.UseMnemonic = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool UseCompatibleTextRendering { get { return base.UseCompatibleTextRendering; } set { base.UseCompatibleTextRendering = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool UseVisualStyleBackColor { get { return base.UseVisualStyleBackColor; } set { base.UseVisualStyleBackColor = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new Image BackgroundImage { get { return base.BackgroundImage; } set { base.BackgroundImage = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new ImageLayout BackgroundImageLayout { get { return base.BackgroundImageLayout; } set { base.BackgroundImageLayout = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new DialogResult DialogResult { get { return base.DialogResult; } set { base.DialogResult = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool AutoEllipsis { get { return base.AutoEllipsis; } set { base.AutoEllipsis = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new Padding Padding { get { return base.Padding; } set { base.Padding = value; } }

    #endregion
  }
}