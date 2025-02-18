// Part of FreeLibSet.
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

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Управляющий элемент для ввода месяца и дня
  /// </summary>
  [Designer(typeof(FreeLibSet.Controls.Design.MonthDayBoxDesigner))]
  [Description("Day and month selection box without year")]
  [ToolboxBitmap(typeof(MonthDayBox), "MonthDayBox.bmp")]
  [ToolboxItem(true)]
  public partial class MonthDayBox : UserControl
  {
    #region Конструктор

    public MonthDayBox()
    {
      InitializeComponent();
      cbMonth.Items.AddRange(EditableDateTimeFormatters.MonthGenitiveNames12);
      edDay.Value = 1;
      edDay.Minimum = 1;
      edDay.Maximum = 31;
      cbMonth.SelectedIndex = 0;
      cbMonth.MaxDropDownItems = 12;

      edDay.ValueChanged += new EventHandler(edDay_ValueChanged);
      cbMonth.SelectedIndexChanged += new EventHandler(cbMonth_SelectedIndexChanged);
      _InsideValueChanged = false;
    }

    #endregion

    #region Свойства

    [Description("Month from 1 to 12")]
    [Category("Appearance")]
    [DefaultValue(1)]
    public int Month
    {
      get { return cbMonth.SelectedIndex + 1; }
      set
      {
        if (value < 1 || value > 12)
          throw ExceptionFactory.ArgOutOfRange("value", value, 1, 12);
        cbMonth.SelectedIndex = value - 1;
      }
    }

    [Description("Day from 1 to 28, 30 or 31, depends on Month current value")]
    [Category("Appearance")]
    [DefaultValue(1)]
    public int Day
    {
      get { return (int)(edDay.Value); }
      set
      {
        if (value < 1 || value > (int)(edDay.Maximum))
          throw new ArgumentOutOfRangeException("value", value, String.Format(Res.MonthDayBox_Arg_DayOutOfRange,
            value, (int)(edDay.Maximum), Month));
        edDay.Value = value;
      }
    }

    /// <summary>
    /// Одновременная установка значения
    /// </summary>
    /// <param name="month">Месяц</param>
    /// <param name="day">День</param>
    public void SetValue(int month, int day)
    {

      if (month == this.Month && day == this.Day)
        return;
      _InsideValueChanged = true;
      try
      {
        this.Month = month;
        this.Day = day;
        _CurrentDayIsLast = false;
      }
      finally
      {
        _InsideValueChanged = false;
      }
      OnValueChanged();
    }

    #endregion

    #region События

    [Description("Called when Day or Month property changed")]
    [Category("Property Changed")]
    public event EventHandler ValueChanged;

    public void OnValueChanged()
    {
      if (ValueChanged != null)
        ValueChanged(this, EventArgs.Empty);
    }

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
        return new Size(180, WantedHeight);
      }
    }

    private int WantedHeight
    {
      get
      {
        if (edDay == null || cbMonth == null)
          return 20;
        else
          return Math.Max(edDay.Height, cbMonth.Height);
      }
    }


    #endregion

    #region Внутренняя реализация

    void cbMonth_SelectedIndexChanged(object sender, EventArgs args)
    {
      edDay.Maximum = GetLastDay(Month);
      if (_InsideValueChanged)
        return;
      _InsideValueChanged = true;
      try
      {

        // При смене месяца, если текущий день был последним, делаем его последним
        if (_CurrentDayIsLast)
          edDay.Value = edDay.Maximum ?? 0;
      }
      finally
      {
        _InsideValueChanged = false;
      }
      OnValueChanged();
    }

    private int GetLastDay(int month)
    {
      return DateTime.DaysInMonth(2001, month); // невисокосный год
    }

    /// <summary>
    /// Предотвращение двойного вызова события ValueChanged
    /// </summary>
    private bool _InsideValueChanged;

    void edDay_ValueChanged(object sender, EventArgs args)
    {
      if (_InsideValueChanged)
        return;

      _CurrentDayIsLast = IsLastDay;

      OnValueChanged();
    }

    /// <summary>
    /// Это свойство устанавливается при ручном вводе пользователем значения
    /// </summary>
    private bool _CurrentDayIsLast;

    /// <summary>
    /// Возвращает true, если текущий введенный день является последним днем месяца
    /// </summary>
    private bool IsLastDay
    {
      get
      {
        return Day == (int)(edDay.Maximum);
      }
    }

    #endregion
  }
}
