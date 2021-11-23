// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.ComponentModel;
using FreeLibSet.Formatting;
using FreeLibSet.Forms;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Поле ввода даты.
  /// Содержит выпадающий список для выбора даты из календаря
  /// </summary>
  [Description("Поле для ввода даты с выпадающим календариком")]
  [ToolboxBitmap(typeof(DateTimeBox), "DateTimeBox.bmp")]
  [ToolboxItem(true)]
  [DefaultProperty("Value")]
  [DefaultBindingProperty("Value")]
  [DefaultEvent("ValueChanged")]
  public class DateTimeBox : UserTextComboBoxBase
  {
    #region Конструктор и Dispose

    public DateTimeBox()
      : base(new MaskedTextBox())
    {
      MainControl.Mask = EditableDateTimeFormatters.Date.EditMask; // 29.04.2021
      _Formatter = EditableDateTimeFormatters.Date;

      PopupButton = true;
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

    #region Свойства Kind и Formatter

    [Description("Формат вывода даты и/иди времени")]
    [Category("Appearance")]
    [DefaultValue(typeof(EditableDateTimeFormatterKind), "Date")]
    [RefreshProperties(RefreshProperties.All)]
    public EditableDateTimeFormatterKind Kind
    {
      get { return _Formatter.Kind; }
      set { Formatter = EditableDateTimeFormatters.Get(value); }
    }

    /// <summary>
    /// Форматизатор вывода даты и/или времени
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public EditableDateTimeFormatter Formatter
    {
      get { return _Formatter; }
      set
      {
        DateTime? oldValue = NValue;
        if (value == null)
          throw new ArgumentNullException("value");
        _Formatter = value;
        NValue = oldValue;
        MainControl.Mask = _Formatter.EditMask; 
        InitText();
        PopupButton = _Formatter.ContainsDate;
      }
    }
    private EditableDateTimeFormatter _Formatter;

    #endregion

    #region Value/NValue

    [Description("Редактируемое значение с поддержкой значения null")]
    [Category("Appearance")]
    [Bindable(true)]
    //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [DefaultValue(typeof(DateTime?), "null")]
    [RefreshProperties(RefreshProperties.All)]
    public Nullable<DateTime> NValue
    {
      get
      {
        return _NValue;
      }
      set
      {
        // 26.08.2013, 23.11.2021
        // Отбрасываем время
        if (value.HasValue && (!_Formatter.ContainsTime))
          value = value.Value.Date;

        if (value == _NValue)
          return;
        _NValue = value;
        InitText();
        OnValueChanged();
      }
    }
    private Nullable<DateTime> _NValue;


    [Description("Редактируемое значение (свойство NValue) без поддежки значения null")]
    [Category("Appearance")]
    //[DefaultValue(null)]
    [Bindable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [RefreshProperties(RefreshProperties.All)]
    public DateTime Value
    {
      get
      {
        return NValue ?? DateTime.MinValue;
      }
      set
      {
        NValue = value;
      }
    }


    [Description("Вызывается при изменении даты (свойства Value или NValue)")]
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

        EditableDateTimeFormatter f = _Formatter;
        if (f == null)
          f = EditableDateTimeFormatters.Date;

        // TODO: Определение оптимальной ширины
        switch (f.Kind)
        { 
          case EditableDateTimeFormatterKind.DateTime:
          case EditableDateTimeFormatterKind.ShortDateTime:
            return new Size(200, sz.Height); 
          default:
            return new Size(120, sz.Height); // TODO: Определение оптимальной ширины
        }

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
        if (_NValue.HasValue)
          MainControl.Text = _Formatter.ToString(_NValue.Value);

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

      if (_Formatter == null)
        return; // режим разработки


      InitButtonsEnabled();

      if (_InsideInitText)
        return;

      // При каждом изменении текста пытаемся установить свойство NValue
      DateTime? NewVal = _Formatter.ToNValue(MainControl.Text, DefaultYear);
      if (NewVal == _NValue)
        return;

      // Свойство изменилось
      _NValue = NewVal;
      OnValueChanged();
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
      public DateTimeBox Owner;

      protected override void OnDateSelected(DateRangeEventArgs drevent)
      {
        try
        {
          base.OnDateSelected(drevent);
          SetResult();
          FindForm().Hide();
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "Ошибка вызова OnDateSelected");
        }
      }

      private void SetResult()
      {
        TimeSpan tod = TimeSpan.Zero;
        if (Owner.NValue.HasValue)
          tod = Owner.NValue.Value.TimeOfDay;
        Owner.NValue = SelectionStart.Date + tod;
        Owner.MainControl.SelectAll();
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
              SetResult();
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

      public MyCalendarForm(DateTimeBox owner)
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
        if (NValue.HasValue)
        {
          if (NValue.Value >= _CalendarForm.Calendar.MinDate && NValue.Value <= _CalendarForm.Calendar.MaxDate)
            _CalendarForm.Calendar.SelectionStart = NValue.Value;
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
      if (NValue.HasValue)
        NValue = null;
      else
        InitText(); // чтобы убрать возможные огрызки, введенные пользователем
    }

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
