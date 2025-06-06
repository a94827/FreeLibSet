﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Formatting;
using FreeLibSet.Core;

#pragma warning disable 1591 // используются атрибуты

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Поле выбора диапазона месяцев в пределах года
  /// </summary>
  [Designer(typeof(FreeLibSet.Controls.Design.YearMonthRangeBoxDesigner))]
  [Description("Three-control component to select pair of months within a single year")]
  [ToolboxBitmap(typeof(YearMonthRangeBox), "YearMonthRangeBox.bmp")]
  [ToolboxItem(true)]
  public partial class YearMonthRangeBox : UserControl
  {
    #region Конструктор

    public YearMonthRangeBox()
    {
      InitializeComponent();
      cbMonth1.Items.AddRange(EditableDateTimeFormatters.MonthGenitiveNames12);
      cbMonth2.Items.AddRange(EditableDateTimeFormatters.MonthNames12);

      DateTime dt = DateTime.Today;
      Year = dt.Year;
      FirstMonth = 1;
      LastMonth = 12;

      edYear.ValueChanged += new EventHandler(InternalValueChanged);
      cbMonth1.SelectedIndexChanged += new EventHandler(InternalValueChanged);
      cbMonth1.MaxDropDownItems = 12;
      cbMonth2.SelectedIndexChanged += new EventHandler(InternalValueChanged);
      cbMonth2.MaxDropDownItems = 12;
    }

    #endregion

    #region Свойства

    [Description("Current value: Year")]
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

    [Description("Current value: First month of the period")]
    [Category("Appearance")]
    [DefaultValue(1)]
    public int FirstMonth
    {
      get
      {
        return cbMonth1.SelectedIndex + 1;
      }
      set
      {
        if (value < 1 || value > 12)
          throw ExceptionFactory.ArgOutOfRange("value", value, 1, 12);
        cbMonth1.SelectedIndex = value - 1;
      }
    }

    [Description("Current value: Last month of the period")]
    [Category("Appearance")]
    [DefaultValue(12)]
    public int LastMonth
    {
      get
      {
        return cbMonth2.SelectedIndex + 1;
      }
      set
      {
        if (value < 1 || value > 12)
          throw ExceptionFactory.ArgOutOfRange("value", value, 1, 12);
        cbMonth2.SelectedIndex = value - 1;
      }
    }


    [Description("Минимальное значение для года")]
    [Category("Behavior")]
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

    [Description("Called when Year, FirstMonth or LastMonth property changed")]
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
        return new DateTime(Year, FirstMonth, 1);
      }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public DateTime LastDate
    {
      get
      {
        int x = DateTime.DaysInMonth(Year, LastMonth);
        return new DateTime(Year, LastMonth, x);
      }
    }

    public void SetDate(DateTime value)
    {
      Year = value.Year;
      FirstMonth = value.Month;
      LastMonth = value.Month;
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
        return new Size(424, WantedHeight);
      }
    }

    private int WantedHeight
    {
      get
      {
        if (cbMonth1 == null || cbMonth2 == null || edYear == null)
          return 20;
        else
          return Math.Max(Math.Max(cbMonth1.Height, cbMonth2.Height), edYear.Height);
      }
    }

    #endregion
  }
}
