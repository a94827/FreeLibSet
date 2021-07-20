using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

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

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// Управляющий элемент для ввода месяца и дня
  /// </summary>
  [Designer(typeof(MonthDayBoxDesigner))]
  [Description("Выбор дня и месяца")]
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

    [Description("Месяц (от 1 до 12)")]
    [Category("Appearance")]
    [DefaultValue(1)]
    public int Month
    {
      get { return cbMonth.SelectedIndex + 1; }
      set
      {
        if (value < 1 || value > 12)
          throw new ArgumentOutOfRangeException("value", value, "Месяц должен быть в диапазоне от 1 до 12");
        cbMonth.SelectedIndex = value - 1;
      }
    }

    [Description(" День (от 1 до 30 или 31, в зависимости от месяца; для февраля - от 1 до 28)")]
    [Category("Appearance")]
    [DefaultValue(1)]
    public int Day
    {
      get { return (int)(edDay.Value); }
      set
      {
        if (value < 1 || value > (int)(edDay.Maximum))
          throw new ArgumentOutOfRangeException("value", value, "День должен быть в диапазоне от 1 до " +
            ((int)(edDay.Maximum)).ToString() + " для месяца " + Month.ToString());
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

    [Description("Событие вызывается при изменении значения свойств Day и/или Month")]
    [Category("Property Changed")]
    public event EventHandler ValueChanged;

    public void OnValueChanged()
    {
      if (ValueChanged != null)
        ValueChanged(this, EventArgs.Empty);
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
          edDay.Value = edDay.Maximum;
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

  public class MonthDayBoxDesigner : ControlDesigner
  {
    /// <summary>
    /// Разрешено менять только горизонтальные размеры
    /// </summary>
    public override SelectionRules SelectionRules
    {
      get
      {
        SelectionRules Rules = base.SelectionRules;
        Rules = Rules & (~(System.Windows.Forms.Design.SelectionRules.BottomSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable));
        return Rules;
      }
    }
  }
}
