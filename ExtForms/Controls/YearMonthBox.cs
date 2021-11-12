﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using FreeLibSet.Formatting;

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
  /// Поле выбора месяца и года.
  /// Месяц выбирается из выпадающего списка.
  /// </summary>
  [Designer(typeof(FreeLibSet.Controls.Design.YearMonthBoxDesigner))]
  [Description("Поле выбора месяца и года")]
  [ToolboxBitmap(typeof(YearMonthBox), "YearMonthBox.bmp")]
  [ToolboxItem(true)]
  public partial class YearMonthBox : UserControl
  {
    #region Конструктор

    public YearMonthBox()
    {
      InitializeComponent();
      cbMonth.Items.AddRange(EditableDateTimeFormatters.MonthNames12);

      DateTime dt = DateTime.Today;
      Year = dt.Year;
      Month = dt.Month;

      edYear.ValueChanged += new EventHandler(InternalValueChanged);
      cbMonth.SelectedIndexChanged += new EventHandler(InternalValueChanged);
      cbMonth.MaxDropDownItems = 12;
    }

    #endregion

    #region Свойства

    [Description("Текущее значение - Год")]
    [Category("Appearance")]
    //[DefaultValue(2006)] нет значения по умолчанию
    public int Year
    {
      get
      {
        return (int)(edYear.Value);
      }
      set
      {
        try
        {
          edYear.Value = value;
        }
        catch (ArgumentOutOfRangeException e)
        {
          e.Data["MinimumYear"] = edYear.Minimum;
          e.Data["MaximumYear"] = edYear.Maximum;
        }
      }
    }

    [Description("Текущее значение - Месяц")]
    [Category("Appearance")]
    //[DefaultValue(1)]
    public int Month
    {
      get
      {
        return cbMonth.SelectedIndex + 1;
      }
      set
      {
        if (value < 1 || value > 12)
          throw new ArgumentOutOfRangeException("value", value, "Месяц должен быть в диапазоне от 1 до 12");
        cbMonth.SelectedIndex = value - 1;
      }
    }

    [Description("Минимальное значение для года")]
    [Category("Behavior")]
    [RefreshProperties(RefreshProperties.All)]
    [DefaultValue(1001)]
    public int MinimumYear
    {
      get
      {
        return (int)(edYear.Minimum);
      }
      set
      {
        edYear.Minimum = value;
      }
    }

    [Description("Максимальное значение для года")]
    [Category("Behavior")]
    [RefreshProperties(RefreshProperties.All)]
    [DefaultValue(9999)]
    public int MaximumYear
    {
      get
      {
        return (int)(edYear.Maximum);
      }
      set
      {
        edYear.Maximum = value;
      }
    }

    #endregion

    #region События

    [Description("Вызывается при изменении текущего года или месяца")]
    [Category("Property Changed")]
    public event EventHandler ValueChanged;

    void InternalValueChanged(object sender, EventArgs args)
    {
      if (ValueChanged != null)
        ValueChanged(this, EventArgs.Empty);
    }

    #endregion

    #region Вспомогательные методы и свойства

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public DateTime FirstDate
    {
      get
      {
        return new DateTime(Year, Month, 1);
      }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public DateTime LastDate
    {
      get
      {
        int x = DateTime.DaysInMonth(Year, Month);
        return new DateTime(Year, Month, x);
      }
    }

    public void SetDate(DateTime value)
    {
      Year = value.Year;
      Month = value.Month;
    }

    #endregion

    #region Заглушки для свойств

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
    public new AutoValidate AutoValidate { get { return base.AutoValidate; } set { base.AutoValidate = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool AutoScroll { get { return base.AutoScroll; } set { base.AutoScroll = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new Size AutoScrollMargin { get { return base.AutoScrollMargin; } set { base.AutoScrollMargin = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new Size AutoScrollMinSize { get { return base.AutoScrollMinSize; } set { base.AutoScrollMinSize = value; } }

    #endregion

    #region Ограничение размеров

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
      base.SetBoundsCore(x, y, width, WantedHeight, specified);
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
      return new Size(proposedSize.Width, WantedHeight);
    }

    protected override Size DefaultSize
    {
      get
      {
        // Метод может вызываться до вызова конструктора
        return new Size(345, WantedHeight);
      }
    }

    private int WantedHeight
    {
      get
      {
        if (cbMonth == null || edYear == null)
          return 20;
        else
          return Math.Max(cbMonth.Height, edYear.Height);
      }
    }

    #endregion
  }
}
