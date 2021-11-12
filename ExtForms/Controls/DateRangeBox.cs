using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Media;
using System.Diagnostics;
using FreeLibSet.Calendar;
using FreeLibSet.Forms;
using FreeLibSet.UICore;

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
  /// Управляющий элемент для ввода двух полей даты как диапазона. 
  /// Снизу выводится текстовое представление введенного интервала дат.
  /// Справа имеются дополнительные кнопки для задания интервала
  /// </summary>
  [Designer(typeof(FreeLibSet.Controls.Design.DateRangeBoxDesigner))]
  [Description("Два поля для ввода интервала дат")]
  [ToolboxBitmap(typeof(DateRangeBox), "DateRangeBox.bmp")]
  [ToolboxItem(true)]
  public partial class DateRangeBox : UserControl
  {
    #region Конструктор

    public DateRangeBox()
    {
      _ShowPeriodText = true; // 16.01.2017 Должно быть обязательно до InitializeComponent()

      InitializeComponent();

      _Formatter = DateRangeFormatter.Default;

      DateValueChanged(null, null);

      SetStyle(ControlStyles.FixedHeight, true);

      TheMenuButton.ToolTipText = "Меню готовых интервалов дат";
      TheLeftButton.ToolTipText = "Предыдущий такой же интервал";
      TheRightButton.ToolTipText = "Следующий такой же интервал";

      // Блокировка кнопок
      First.EnabledChanged += new EventHandler(Date_EnabledChanged);
      Last.EnabledChanged += new EventHandler(Date_EnabledChanged);
      First.ReadOnlyChanged += new EventHandler(Date_EnabledChanged);
      Last.ReadOnlyChanged += new EventHandler(Date_EnabledChanged);
      First.VisibleChanged += new EventHandler(Date_VisibleChanged);
      Last.VisibleChanged += new EventHandler(Date_VisibleChanged);
      First.ValueChanged += new EventHandler(Date_EnabledChanged); // 03.06.2019
      Last.ValueChanged += new EventHandler(Date_EnabledChanged); // Учитываем переходы null-not null
    }

    public override ISite Site
    {
      get { return base.Site; }
      set
      {
        base.Site = value;

        lblPeriod.BorderStyle = BorderStyle.FixedSingle;
      }
    }

    #endregion

    #region Форматизатор

    /// <summary>
    /// Форматизатор интервала дат для строки текстового представления периода.
    /// Используется при ShowPeriodText=true.
    /// Если свойство не установлено в явном виде, используется DateRangeFormatter.Default.
    /// Для обеспечения единообразия в программе, рекомендуется не устанавливать это свойство,
    /// а переопределить глобальный форматизатор DateRangeFormatter.Default.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DateRangeFormatter Formatter
    {
      get { return _Formatter; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Formatter = value;
      }
    }
    private DateRangeFormatter _Formatter;

    #endregion

    #region Кнопка меню

    //[Description("Видимость кнопки локального меню")]
    //[DefaultValue(false)]
    //public bool MenuButtonVisible
    //{
    //  get { return TheMenuButton.Visible; }
    //  set { TheMenuButton.Visible = value; }
    //}

    /// <summary>
    /// Событие вызывает при нажатии кнопки показа меню.
    /// Если обработчик события не установлен или он не установил CancelEventArgs.Cancel=true,
    /// показывается локальное меню, заданное свойствами ContextMenuStrip или ContextMenu
    /// </summary>
    public event CancelEventHandler MenuButtonClicked;

    protected virtual void OnMenuButtonClicked(CancelEventArgs args)
    {
      if (MenuButtonClicked != null)
        MenuButtonClicked(this, args);
    }

    private void TheMenuButton_Click(object sender, EventArgs args)
    {
      CancelEventArgs Args2 = new CancelEventArgs();
      OnMenuButtonClicked(Args2);
      if (Args2.Cancel)
        return;

      //TheMenu.Show(TheMenuButton, 0, TheMenuButton.Height);
      if (this.ContextMenuStrip != null)
        this.ContextMenuStrip.Show(TheMenuButton, 0, TheMenuButton.Height);
      else if (this.ContextMenu != null)
        this.ContextMenu.Show(TheMenuButton, new Point(0, TheMenuButton.Height));
    }

    #endregion

    #region Кнопки сдвига периода

    internal void TheLeftButton_Click(object sender, EventArgs args)
    {
      ShiftRange(false);
    }

    internal void TheRightButton_Click(object sender, EventArgs args)
    {
      ShiftRange(true);
    }

    private void ShiftRange(bool forward)
    {
      if ((!First.Visible) || (!First.Enabled) ||
        (!Last.Visible) || (!Last.Enabled))
      {
        EFPApp.ShowTempMessage("Для изменения интервала необходимо, чтобы обе даты были доступны");
        return;
      }

      DateTime? dt1 = First.NValue;
      DateTime? dt2 = Last.NValue;

      if (!UITools.ShiftDateRange(ref dt1, ref dt2, forward))
      {
        EFPApp.ShowTempMessage("Для изменения интервала необходимо, чтобы обе даты были заполнены");
        return;
      }

      First.NValue = dt1;
      Last.NValue = dt2;
    }

    #endregion

    #region ShowPeriodText

    [Description("Управляет показом текста периода в нижней части элемента. Если установить в false, то элемент занимает меньше места")]
    [Category("Appearance")]
    [DefaultValue(true)]
    public bool ShowPeriodText
    {
      get { return _ShowPeriodText; }
      set
      {
        if (value == _ShowPeriodText)
          return;
        _ShowPeriodText = value;
        lblPeriod.Visible = value;
        this.Height = DefaultSize.Height;
      }
    }
    private bool _ShowPeriodText; // Нельзя использовать lblPeriod.Visible, т.к. оно меняется при инициализации

    /// <summary>
    /// Вызывает событие ShowPeriodTextChanged
    /// </summary>
    /// <param name="args">Фиктивный аргумент</param>
    protected virtual void OnShowPeriodTextChanged(EventArgs args)
    {
      if (ShowPeriodTextChanged != null)
        ShowPeriodTextChanged(this, args);
    }

    [Description("Вызывается при изменении свойства ShowPeriodText")]
    [Category("Property Changed")]
    public event EventHandler ShowPeriodTextChanged;

    #endregion

    #region Размеры элемента

    protected override Size DefaultSize
    {
      get
      {
        int w = 350;
        int h;
        if (First == null)
          h = 33;
        else
        {
          h = First.Height;
          if (ShowPeriodText)
            h += lblPeriod.Height;
        }
        return new Size(w, h);
      }
    }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
      base.SetBoundsCore(x, y, width, DefaultSize.Height, specified);

      InitPanelSizes();
    }

    private void InitPanelSizes()
    {
      // Строки
      TheLP.RowStyles[0].Height = First.Height;
      TheLP.RowStyles[1].Height = lblPeriod.Height;

      // Столбцы устанавливаются автоматически
    }

    #endregion

    #region Прочие обработчики

    protected override void OnLoad(EventArgs args)
    {
      base.OnLoad(args);
      Date_VisibleChanged(null, null);
      Date_EnabledChanged(null, null);
    }

    private void DateValueChanged(object sender, EventArgs args)
    {
      InitPeriodText();
    }

    private void InitPeriodText()
    {
      if (First.Visible && First.Enabled)
      {
        if (Last.Visible && Last.Enabled)
          lblPeriod.Text = Formatter.ToString(First.NValue, Last.NValue, true);
        else
          lblPeriod.Text = Formatter.ToString(First.NValue, true);
      }
      else
      {
        if (Last.Visible && Last.Enabled)
          lblPeriod.Text = Formatter.ToString(Last.NValue, true);
        else
          lblPeriod.Text = String.Empty;
      }
    }

    void Date_VisibleChanged(object sender, EventArgs args)
    {
      TheMenuButton.Visible = First.Visible && Last.Visible;
      TheLeftButton.Visible = TheMenuButton.Visible;
      TheRightButton.Visible = TheMenuButton.Visible;

      InitPeriodText();
    }

    void Date_EnabledChanged(object sender, EventArgs args)
    {
      try
      {
        TheMenuButton.Enabled = First.Enabled && Last.Enabled &&
          (!First.ReadOnly) && (!Last.ReadOnly);

        // Частичные сдвиги тоже возможны, поэтому проверяем два раза
        if (TheMenuButton.Enabled)
        {
          DateTime? dt1 = First.NValue;
          DateTime? dt2 = Last.NValue;
          TheRightButton.Enabled = UITools.ShiftDateRange(ref dt1, ref dt2, true);

          dt1 = First.NValue;
          dt2 = Last.NValue;
          TheLeftButton.Enabled = UITools.ShiftDateRange(ref dt1, ref dt2, false);
        }
        else
        {
          TheRightButton.Enabled = false;
          TheLeftButton.Enabled = false;
        }
        InitPeriodText();
      }
      catch (Exception e)
      {
        lblPeriod.Text = e.Message; // 05.06.2019
      }
    }

    #endregion
  }
}
