using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
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

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// Поле ввода даты.
  /// Содержит выпадающий список для выбора даты из календаря
  /// </summary>
  [Description("Поле для ввода даты с выпадающим календариком")]
  [ToolboxBitmap(typeof(DateBox), "DateBox.bmp")]
  [ToolboxItem(true)]
  [DefaultProperty("Value")]
  [DefaultBindingProperty("Value")]
  [DefaultEvent("ValueChanged")]
  public class DateBox : UserTextComboBoxBase
  {
    #region Конструктор и Dispose

    public DateBox()
      : base(new MaskedTextBox())
    {
      MainControl.Mask = EditableDateTimeFormatters.Date.EditMask; // 29.04.2021
      _Value = null;
      _InsideInitText = false;

      // ReSharper disable once VirtualMemberCallInConstructor
      this.InitButtonsEnabled();
    }


    protected override void Dispose(bool disposing)
    {
      if (_CalendarForm != null)
      {
        _CalendarForm.Dispose();
        _CalendarForm = null;
        // Calendar разрушать не надо, он уже разрушен вместе с формой
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойство MainControl

    protected new MaskedTextBox MainControl { get { return (MaskedTextBox)(base.MainControl); } }

    #endregion

    #region Value

    [Description("Редактируемое значение")]
    [Category("Appearance")]
    [DefaultValue(null)]
    [Bindable(true)]
    public Nullable<DateTime> Value
    {
      get
      {
        return _Value;
      }
      set
      {
        // 26.08.2013
        // Отбрасываем время
        if (value.HasValue)
          value = value.Value.Date;

        if (value == _Value)
          return;
        _Value = value;
        InitText();
        OnValueChanged();
      }
    }
    private Nullable<DateTime> _Value;


    [Description("Вызывается при изменении даты (свойство Value)")]
    [Category("Property Changed")]
    public event EventHandler ValueChanged;

    protected virtual void OnValueChanged()
    {
      if (ValueChanged != null)
        ValueChanged(this, EventArgs.Empty);
    }

    #endregion

    #region DefaultYear

    [Description("Год по умолчанию. Если задано ненулевое значение, то пользователь может ввести только день и месяц, после чего дата будет считаться введенной")]
    [Category("Behavior")]
    [DefaultValue(0)]
    public int DefaultYear
    {
      get { return _DefaultYear; }
      set
      {
        if (value < 0 || value > DateTime.MaxValue.Year)
          throw new ArgumentOutOfRangeException();
        if (value == _DefaultYear)
          return;
        _DefaultYear = value;
        OnTextChanged(EventArgs.Empty);
      }
    }
    private int _DefaultYear;

    [Description("Вызывается при изменении года по умолчанию (свойство DefaultYear)")]
    [Category("Property Changed")]
    public event EventHandler DefaultYearChanged;

    protected virtual void OnDefaultYearChanged()
    {
      if (DefaultYearChanged != null)
        DefaultYearChanged(this, EventArgs.Empty);
    }

    #endregion

    #region Переопределяемые методы

    protected override void OnVisibleChanged(EventArgs args)
    {
      base.OnVisibleChanged(args);
      if (Visible)
        InitText(); // 14.01.2019 - предотвращаем "уезд" текста при разрешении 150dpi.
      // Одной установки MainControl.Text= MainControl.Text не достаточно. Сначала нужен вызов MainControl.Text=""
    }

    protected override void OnLeave(EventArgs args)
    {
      // Перед выходом из элемента преобразуем текущее значение в нормальный вид
      InitText();

      base.OnLeave(args);
    }

    protected override Size DefaultSize
    {
      get
      {
        Size sz = base.DefaultSize;
        return new Size(120, sz.Height);
      }
    }

    #endregion

    #region Свойство Text

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string Text
    {
      get { return base.Text; }
      set { base.Text = value; }
    }

    private void InitText()
    {
      _InsideInitText = true;
      try
      {
        MainControl.Text = "";
        if (_Value.HasValue)
          MainControl.Text = EditableDateTimeFormatters.Date.ToString(_Value.Value);

        MainControl.SelectAll();
      }
      finally
      {
        _InsideInitText = false;
      }
    }

    private bool _InsideInitText;

    protected override void OnTextChanged(EventArgs args)
    {
      base.OnTextChanged(args);

      InitButtonsEnabled();

      if (_InsideInitText)
        return;
      // При каждом изменении текста пытаемся установить свойство Value
      DateTime? NewVal = TextToDate(MainControl.Text, DefaultYear);
      if (NewVal == _Value)
        return;

      // Свойство изменилось
      _Value = NewVal;
      OnValueChanged();
    }

    internal static DateTime? TextToDate(string text, int defaultYear)
    {
      DateTime value;
      if (EditableDateTimeFormatters.Date.TryParse(text, out value, defaultYear))
        return value;
      else
        return null;
    }

    /// <summary>
    /// Возвращает количество заполненных позиций в маске
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int AssignedEditPositionCount
    {
      get
      {
        if (MainControl.MaskedTextProvider == null)
        {
          if (Text == null)
            return 0;
          else
            return Text.Length;
        }
        else
          return MainControl.MaskedTextProvider.AssignedEditPositionCount;
      }
    }

    #endregion

    #region Кнопка вызова календаря

    #region Класс MyCalendar

    private class MyCalendar : MonthCalendar
    {
      public DateBox Owner;

      protected override void OnDateSelected(DateRangeEventArgs drevent)
      {
        try
        {
          base.OnDateSelected(drevent);
          Owner.Value = SelectionStart;
          Owner.MainControl.SelectAll();
          FindForm().Hide();
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "Ошибка вызова OnDateSelected");
        }
      }

      protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs args)
      {
        base.OnPreviewKeyDown(args);
        if (args.KeyCode == Keys.Tab || args.KeyCode == Keys.Escape)
        {
          if ((!args.Alt) && (!args.Control))
            args.IsInputKey = true;
        }
      }

      protected override void OnKeyDown(KeyEventArgs args)
      {
        try
        {
          switch (args.KeyCode)
          {
            case Keys.Return:
              Owner.Value = SelectionStart;
              Owner.MainControl.SelectAll();
              FindForm().Hide();
              args.Handled = true;
              return;
            case Keys.Tab:
              FindForm().Hide();
              args.Handled = true;
              Owner.FindForm().SelectNextControl(Owner, !args.Shift, true, true, true);
              return;
            case Keys.Escape:
              FindForm().Hide();
              return;
          }
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "Ошибка вызова OnDateSelected");
        }
        base.OnKeyDown(args);
      }
    }

    #endregion

    #region Класс MyCalendarForm

    private class MyCalendarForm : Form
    {
      #region Конструктор

      public MyCalendarForm(DateBox owner)
      {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        //TopMost = false;
        //TopLevel = false;
        //base.Owner = Owner.FindForm();
        FormButtonStub.AssignCancel(this);

        // Если используется тема Windows-XP, то MonthCalendar рисует нормальную рамку
        // Если используется тема Windows-98, то MonthCalendar не содержит никакой рамки
        // и получается некрасиво
        // Пусть лучше будет лишняя рамка
        Panel pan = new Panel();
        pan.BorderStyle = BorderStyle.Fixed3D;
        Controls.Add(pan);

        Calendar = new MyCalendar();
        Calendar.Owner = owner;
        Calendar.MaxSelectionCount = 1;
        Calendar.SizeChanged += new EventHandler(Calendar_SizeChanged);
        pan.Controls.Add(Calendar);
      }

      #endregion

      #region Свойства

      public readonly MyCalendar Calendar;

      #endregion

      #region Обработчики

      protected override bool ShowWithoutActivation
      {
        get { return false; }
      }

      protected override void OnDeactivate(EventArgs args)
      {
        base.OnDeactivate(args);

        // Закрытие формы при попытке щелкнуть мимо
        try
        {
          if (Visible)
            Hide();
        }
        catch /*(Exception e)*/
        {
          //MessageBox.Show(e.Message, "Ошибка при деактивации календаря");
        }
      }

      void Calendar_SizeChanged(object sender, EventArgs args)
      {
        try
        {
          Calendar.Parent.ClientSize = Calendar.Size;
          Size = Calendar.Parent.Size;
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "Ошибка изменения размеров календаря");
        }
      }

      #endregion
    }

    #endregion

    /// <summary>
    /// Форма календарика. Нельзя уничтожать форму изнутри обработчика календаря,
    /// т.к. будет ошибка обращения к удаленному объекту при нажатии мыши на
    /// кнопке "текущая дата" (видимо, дефект в MonthCalendar).
    /// Поэтому форма не уничтожается, а делается скрытой.
    /// </summary>
    private MyCalendarForm _CalendarForm;

    protected override void OnPopupClick(EventArgs args)
    {
      if (HasPopupClickHandler)
        base.OnPopupClick(args);
      else
        PopupCalendar();
    }

    /// <summary>
    /// Показываем календарик для выбора даты
    /// </summary>
    private void PopupCalendar()
    {
      try
      {
        if (WinFormsTools.IsNullOrDisposed(_CalendarForm))
          _CalendarForm = new MyCalendarForm(this);

        Point pt = new Point(3, Height);
        pt = PointToScreen(pt);
        _CalendarForm.Location = pt;
        if (Value.HasValue)
        {
          if (Value.Value >= _CalendarForm.Calendar.MinDate && Value.Value <= _CalendarForm.Calendar.MaxDate)
            _CalendarForm.Calendar.SelectionStart = Value.Value;
          else
            _CalendarForm.Calendar.SelectionStart = DateTime.Today; // 24.05.2019
        }
        else
          _CalendarForm.Calendar.SelectionStart = DateTime.Today;
        _CalendarForm.Calendar.SelectionEnd = _CalendarForm.Calendar.SelectionStart;

        _CalendarForm.Show();
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Ошибка вызова календаря");
      }
    }



    protected override void InitButtonsEnabled()
    {
      PopupButtonEnabled = !ReadOnly;
      ClearButtonEnabled = (!ReadOnly) && AssignedEditPositionCount > 0;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool PopupButton { get { return base.PopupButton; } set { base.PopupButton = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool PopupButtonEnabled { get { return base.PopupButtonEnabled; } set { base.PopupButtonEnabled = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new string PopupButtonToolTipText { get { return base.PopupButtonToolTipText; } set { base.PopupButtonToolTipText = value; } }

    #endregion

    #region Кнопка "Очистить"

    /// <summary>
    /// Выполняет очистку значения
    /// </summary>
    /// <param name="args"></param>
    protected override void OnClearClick(EventArgs args)
    {
      if (Value.HasValue)
        Value = null;
      else
        InitText(); // чтобы убрать возможные огрызки, введенные пользователем
    }
    /*
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Bindable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public new bool ClearButtonEnabled
        {
          get { return base.ClearButtonEnabled; }
          set 
          { 
          }
        }
      */

    /// <summary>
    /// Не используется, т.к. устанавливается автоматически
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool ClearButtonEnabled { get { return base.ClearButtonEnabled; } set { base.ClearButtonEnabled = value; } }

    /// <summary>
    /// Не используется, т.к. устанавливается автоматически
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new string ClearButtonToolTipText { get { return base.ClearButtonToolTipText; } set { base.ClearButtonToolTipText = value; } }

    #endregion

    #region Кнопка "Редактировать" (заглушки для свойств)

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool EditButton { get { return base.EditButton; } set { base.EditButton = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool EditButtonEnabled { get { return base.EditButtonEnabled; } set { base.EditButtonEnabled = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new UserComboBoxEditButtonKind EditButtonKind { get { return base.EditButtonKind; } set { base.EditButtonKind = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new string EditButtonToolTipText { get { return base.EditButtonToolTipText; } set { base.EditButtonToolTipText = value; } }

    #endregion
  }

  /// <summary>
  /// Закрывалка для формы по нажатию клавиши "Esc" вместо настоящей кнопки
  /// или "Enter"
  /// </summary>
  public class FormButtonStub : IButtonControl
  {
    #region Конструктор

    public FormButtonStub(Form form, DialogResult dialogResult)
    {
      _Form = form;
      _DialogResult = dialogResult;
    }
    #endregion

    #region Свойства

    private readonly Form _Form;

    #endregion

    #region IButtonControl Members

    public DialogResult DialogResult
    {
      get
      {
        return _DialogResult;
      }
      set
      {
        _DialogResult = value;
      }
    }
    private DialogResult _DialogResult;


    public void NotifyDefault(bool value)
    {
      // Ничего не делаем
    }

    public void PerformClick()
    {
      _Form.DialogResult = DialogResult;
      _Form.Close();
    }

    #endregion

    #region Статический метод

    /// <summary>
    /// Присоединение нового объекта к свойству Form.CancelButton.
    /// Если свойству уже присвоено значение, никаких действий не выполняется
    /// </summary>
    /// <param name="form"></param>
    /// <returns>true, если обработчик присоединен</returns>
    public static bool AssignCancel(Form form)
    {
#if DEBUG
      if (form == null)
        throw new ArgumentNullException("form");
#endif

      if (form.CancelButton == null)
      {
        FormButtonStub Stub = new FormButtonStub(form, DialogResult.Cancel);
        form.CancelButton = Stub;
        return true;
      }
      else
        return false;
    }

    public static bool AssignOK(Form form)
    {
#if DEBUG
      if (form == null)
        throw new ArgumentNullException("form");
#endif

      if (form.AcceptButton == null)
      {
        FormButtonStub Stub = new FormButtonStub(form, DialogResult.OK);
        form.AcceptButton = Stub;
        return true;
      }
      else
        return false;
    }

    #endregion
  }
}
