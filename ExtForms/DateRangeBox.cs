using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Media;
using System.Windows.Forms.Design;
using System.ComponentModel.Design;
using System.Collections;
using System.Windows.Forms.Design.Behavior;
using System.Diagnostics;
using FreeLibSet.Calendar;
using FreeLibSet.Forms;

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
  [Designer(typeof(DateRangeBoxDesigner))]
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
      FirstDate.EnabledChanged += new EventHandler(Date_EnabledChanged);
      LastDate.EnabledChanged += new EventHandler(Date_EnabledChanged);
      FirstDate.ReadOnlyChanged += new EventHandler(Date_EnabledChanged);
      LastDate.ReadOnlyChanged += new EventHandler(Date_EnabledChanged);
      FirstDate.VisibleChanged += new EventHandler(Date_VisibleChanged);
      LastDate.VisibleChanged += new EventHandler(Date_VisibleChanged);
      FirstDate.ValueChanged += new EventHandler(Date_EnabledChanged); // 03.06.2019
      LastDate.ValueChanged += new EventHandler(Date_EnabledChanged); // Учитываем переходы null-not null
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
    /// Используемый форматизатор интервала дат.
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
      if ((!FirstDate.Visible) || (!FirstDate.Enabled) ||
        (!LastDate.Visible) || (!LastDate.Enabled))
      {
        EFPApp.ShowTempMessage("Для изменения интервала необходимо, чтобы обе даты были доступны");
        return;
      }

      DateTime? dt1 = FirstDate.Value;
      DateTime? dt2 = LastDate.Value;

      if (!ShiftDateRange(ref dt1, ref dt2, forward))
      {
        EFPApp.ShowTempMessage("Для изменения интервала необходимо, чтобы обе даты были заполнены");
        return;
      }

      FirstDate.Value = dt1;
      LastDate.Value = dt2;
    }

    /// <summary>
    /// Сдвиг интервала дат вперед или назад.
    /// Если текущий период представляет собой целое число месяцев, то сдвиг
    /// выполняется на число месяцев, содержащихся в периоде. Иначе сдвиг выполняется
    /// на число дней в периоде
    /// </summary>
    /// <param name="firstDate">Начальная дата (по ссылке)</param>
    /// <param name="lastDate">Конечная дата (по ссылке)</param>
    /// <param name="forward">true для сдвига вперед, false - назад</param>
    [DebuggerStepThrough]
    public static bool ShiftDateRange(ref DateTime firstDate, ref DateTime lastDate, bool forward)
    {
      // 05.06.2019, 25.12.2020
      // Перехватываем исключение ArgumentOutOfRangeException, которое может возникнуть 
      DateTime dt1 = firstDate;
      DateTime dt2 = lastDate;
      bool Res = false;
      try
      {
        Res = DoShiftDateRange(ref dt1, ref dt2, forward);
      }
      catch { }
      if (Res)
      {
        firstDate = dt1;
        lastDate = dt2;
      }
      return Res;
    }

    [DebuggerStepThrough]
    private static bool DoShiftDateRange(ref DateTime firstDate, ref DateTime lastDate, bool forward)
    {
      firstDate = firstDate.Date;
      lastDate = lastDate.Date;
      if (lastDate < firstDate)
        return false;
      if (firstDate.Day == 1 && lastDate.Day == DateTime.DaysInMonth(lastDate.Year, lastDate.Month))
      {
        // Сдвиг по месяцам
        int MonthCount = (lastDate.Year - firstDate.Year) * 12 + lastDate.Month - firstDate.Month + 1;
        firstDate = firstDate.AddMonths(forward ? MonthCount : -MonthCount);
        lastDate = lastDate.AddMonths(forward ? MonthCount : -MonthCount);
        firstDate = firstDate.Date;
        lastDate = new DateTime(lastDate.Year, lastDate.Month, DateTime.DaysInMonth(lastDate.Year, lastDate.Month));
      }
      else
      {
        // Сдвиг по дням
        int Days = (int)(Math.Round((lastDate - firstDate).TotalDays, 0, MidpointRounding.AwayFromZero)) + 1;
        firstDate = firstDate.AddDays(forward ? +Days : -Days);
        lastDate = lastDate.AddDays(forward ? +Days : -Days);
      }
      return true;
    }

    /// <summary>
    /// Сдвиг интервала дат вперед или назад.
    /// Если текущий период представляет собой целое число месяцев, то сдвиг
    /// выполняется на число месяцев, содержащихся в периоде. Иначе сдвиг выполняется
    /// на число дней в периоде
    /// Эта версия, позволяет сдвигать полузакрытые интервалы
    /// </summary>
    /// <param name="firstDate">Начальная дата (по ссылке)</param>
    /// <param name="lastDate">Конечная дата (по ссылке)</param>
    /// <param name="forward">true для сдвига вперед, false - назад</param>
    public static bool ShiftDateRange(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      if (firstDate.HasValue && lastDate.HasValue)
      {
        // Обычный сдвиг
        DateTime dt1 = firstDate.Value;
        DateTime dt2 = lastDate.Value;
        if (!ShiftDateRange(ref dt1, ref dt2, forward))
          return false;
        firstDate = dt1;
        lastDate = dt2;
        return true;
      }

      if (firstDate.HasValue)
      {
        if (!forward)
        {
          if (firstDate.Value == DateTime.MinValue)
            return false; // 02.09.2020
          lastDate = firstDate.Value.AddDays(-1);
          firstDate = null;
          return true;
        }
      }
      if (lastDate.HasValue)
      {
        if (forward)
        {
          if (lastDate.Value == DateTime.MaxValue.Date)
            return false; // 02.09.2020
          firstDate = lastDate.Value.AddDays(1);
          lastDate = null;
          return true;
        }
      }
      return false;
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
        if (FirstDate == null)
          h = 33;
        else
        {
          h = FirstDate.Height;
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
      TheLP.RowStyles[0].Height = FirstDate.Height;
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
      if (FirstDate.Visible && FirstDate.Enabled)
      {
        if (LastDate.Visible && LastDate.Enabled)
          lblPeriod.Text = Formatter.ToString(FirstDate.Value, LastDate.Value, true);
        else
          lblPeriod.Text = Formatter.ToString(FirstDate.Value, true);
      }
      else
      {
        if (LastDate.Visible && LastDate.Enabled)
          lblPeriod.Text = Formatter.ToString(LastDate.Value, true);
        else
          lblPeriod.Text = String.Empty;
      }
    }

    void Date_VisibleChanged(object sender, EventArgs args)
    {
      TheMenuButton.Visible = FirstDate.Visible && LastDate.Visible;
      TheLeftButton.Visible = TheMenuButton.Visible;
      TheRightButton.Visible = TheMenuButton.Visible;

      InitPeriodText();
    }

    void Date_EnabledChanged(object sender, EventArgs args)
    {
      try
      {
        TheMenuButton.Enabled = FirstDate.Enabled && LastDate.Enabled &&
          (!FirstDate.ReadOnly) && (!LastDate.ReadOnly);

        // Частичные сдвиги тоже возможны, поэтому проверяем два раза
        if (TheMenuButton.Enabled)
        {
          DateTime? dt1 = FirstDate.Value;
          DateTime? dt2 = LastDate.Value;
          TheRightButton.Enabled = ShiftDateRange(ref dt1, ref dt2, true);

          dt1 = FirstDate.Value;
          dt2 = LastDate.Value;
          TheLeftButton.Enabled = ShiftDateRange(ref dt1, ref dt2, false);
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

  public class DateRangeBoxDesigner : ControlDesigner
  {
    #region Размеры

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

    #endregion

    #region Snap lines

    // Добавляем "сиреневую" линию базовой линии текста для дизайнера формы
    // Линия берется из поля ввода первой даты
    // Взято из 
    // http://stackoverflow.com/questions/93541/baseline-snaplines-in-custom-winforms-controls
    //

    public override IList SnapLines
    {
      get
      {
        /* Code from above */
        IList snapLines = base.SnapLines;

        // *** This will need to be modified to match your user control
        DateRangeBox control = Control as DateRangeBox;
        if (control == null)
          return snapLines;

        // *** This will need to be modified to match the item in your user control
        // This is the control in your UC that you want SnapLines for the entire UC
        IDesigner designer = TypeDescriptor.CreateDesigner(control.FirstDate, typeof(IDesigner));
        if (designer == null)
          return snapLines;

        // *** This will need to be modified to match the item in your user control
        designer.Initialize(control.FirstDate);

        using (designer)
        {
          ControlDesigner boxDesigner = designer as ControlDesigner;
          if (boxDesigner == null)
            return snapLines;

          foreach (SnapLine line in boxDesigner.SnapLines)
          {
            if (line.SnapLineType == SnapLineType.Baseline)
            {
              // *** This will need to be modified to match the item in your user control
              snapLines.Add(new SnapLine(SnapLineType.Baseline,
                 line.Offset + control.FirstDate.Top, // всегда 0
                 line.Filter, line.Priority));
              break;
            }
          }
        }

        return snapLines;
      }
    }

    #endregion
  }
}
