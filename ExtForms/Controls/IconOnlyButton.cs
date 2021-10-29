using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms.Design;

#pragma warning disable 1591

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

#if XXX // пока не знаю, как размещать на форме, чтобы не резервировалось место для увеличения панели

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Кнопка, на которой отображаетcя только изображение 16x16
  /// Кнопка всегда имеет фиксированный размер
  /// </summary>
  [Description("Кнопка с маленьким изображением")]
  //[ToolboxBitmap(typeof(IconOnlyButton), "IconOnlyButton.bmp")]
  [ToolboxItem(true)]
  [Designer(typeof(IconOnlyButtonDesigner))]
  public class IconOnlyButton : Button
  {
    #region Конструктор

    public IconOnlyButton()
    {
      base.Text = String.Empty;
      base.ImageAlign = ContentAlignment.MiddleCenter;
      SetStyle(ControlStyles.FixedHeight, true);
      SetStyle(ControlStyles.FixedWidth, true);
    }

    #endregion

    #region Размеры

    protected override Size DefaultSize
    {
      get
      {
        return new Size(32, 24);
      }
    }

    protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, BoundsSpecified specified)
    {
      //return bounds; // размеры не меняются
      //return base.GetScaledBounds(bounds, factor, specified);
      int x, y;
      if ((Anchor & AnchorStyles.Right) == 0)
        x = bounds.Location.X;
      else
        x = (int)((bounds.Location.X + bounds.Width) * factor.Width) - DefaultSize.Width;

      if ((Anchor & AnchorStyles.Bottom) == 0)
        y = bounds.Location.Y;
      else
        y = (int)((bounds.Location.Y + +bounds.Height) * factor.Height) - DefaultSize.Height;

      return new Rectangle(new Point(x, y), DefaultSize);
    }

    #endregion

    #region Отключение лишних свойств

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string Text
    {
      get
      {
        return base.Text;
      }
      set
      {
      }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new event EventHandler TextChanged
    {
      add { base.TextChanged += value; }
      remove { base.TextChanged -= value; }
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override Font Font
    {
      get { return base.Font; }
      set { base.Font = value; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new event EventHandler FontChanged
    {
      add { base.FontChanged += value; }
      remove { base.FontChanged -= value; }
    }



    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override Color ForeColor
    {
      get { return base.ForeColor; }
      set { base.ForeColor = value; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new event EventHandler ForeColorChanged
    {
      add { base.ForeColorChanged += value; }
      remove { base.ForeColorChanged -= value; }
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool AutoEllipsis
    {
      get { return base.AutoEllipsis; }
      set { base.AutoEllipsis = value; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool UseMnemonic
    {
      get { return base.UseMnemonic; }
      set { base.UseMnemonic = value; }
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override ContentAlignment TextAlign
    {
      get { return base.TextAlign; }
      set { base.TextAlign = value; }
    }



    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new ContentAlignment ImageAlign
    {
      get { return base.ImageAlign; }
      set { }
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new TextImageRelation TextImageRelation
    {
      get { return base.TextImageRelation; }
      set { base.TextImageRelation = value; }
    }



    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool AutoSize
    {
      get { return base.AutoSize; }
      set { /*base.AutoSize = value; */}
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new event EventHandler AutoSizeChanged
    {
      add { base.AutoSizeChanged += value; }
      remove { base.AutoSizeChanged -= value; }
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new AutoSizeMode AutoSizeMode
    {
      get { return base.AutoSizeMode; }
      set { base.AutoSizeMode = value; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Padding Padding
    {
      get { return base.Padding; }
      set { }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new event EventHandler PaddingChanged
    {
      add { base.PaddingChanged += value; }
      remove { base.PaddingChanged -= value; }
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Size Size
    {
      get { return base.Size; }
      set { base.Size = value; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new event EventHandler SizeChanged
    {
      add { base.SizeChanged += value; }
      remove { base.SizeChanged -= value; }
    }

    #endregion
  }


  public class IconOnlyButtonDesigner : ControlDesigner
  {
    #region Изменение размеров

    /// <summary>
    /// Разрешено менять только горизонтальные размеры
    /// </summary>
    public override SelectionRules SelectionRules
    {
      get
      {
        SelectionRules Rules = base.SelectionRules;
        Rules = Rules & (~(SelectionRules.BottomSizeable | SelectionRules.TopSizeable | SelectionRules.LeftSizeable | SelectionRules.RightSizeable));
        return Rules;
      }
    }

    #endregion
  }

}
#endif