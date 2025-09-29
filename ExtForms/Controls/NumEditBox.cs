// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.Formatting;
using System.ComponentModel.Design.Serialization;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  [Designer(typeof(FreeLibSet.Controls.Design.NumEditBoxBaseDesigner))]
  public abstract class NumEditBoxBase<T> : UserControl, IMinMaxSource<T?>
    where T : struct, IFormattable, IComparable<T>
  {
    #region Конструктор

    public NumEditBoxBase()
    {
      _Format = String.Empty;
      SetStyle(ControlStyles.FixedHeight, true);
      base.ForeColor = SystemColors.WindowText;
      base.BackColor = SystemColors.Window;
      base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;

      InitMainControl(false);
      _MainControl.Text = String.Empty;
      _TextIsValid = true;
    }

    #endregion

    #region Основной управляющий элемент

    private class InternalUpDown : UpDownBase
    {
      #region Конструктор

      internal InternalUpDown(NumEditBoxBase<T> owner)
      {
        _Owner = owner;
        if (base.Controls.Count != 2)
          throw new BugException("Invalid realization of UpDownBase control");
        _ScrollPart = base.Controls[0];
        _MainPart = (TextBox)(base.Controls[1]);

        // 28.08.2023
        // В Mono (v.6.12.0, без Wine) почему-то не работет размещение элементов в UpDownBase.
        // Выполняем размещение самостоятельно в методе OnSizeChanged()

        if (EnvironmentTools.IsMono)
        {
          _ScrollPart.Dock = DockStyle.None;
          _MainPart.Dock = DockStyle.None;
        }
      }

      private NumEditBoxBase<T> _Owner;

      #endregion

      #region Свойства

      /// <summary>
      /// Основной элемент - TextBox.
      /// См. исходный текст класса net framework UpDownBase.
      /// </summary>
      public TextBox MainPart { get { return _MainPart; } }
      private readonly TextBox _MainPart;
      
      /// <summary>
      /// Элемент со стрелочками
      /// </summary>
      private readonly Control _ScrollPart;

      protected override void OnSizeChanged(EventArgs args)
      {
        base.OnSizeChanged(args);

        // Метод может вызываться из конструктора базового класса.
        // В этом случае ничего не делаем

        if (_MainPart != null && EnvironmentTools.IsMono)
        {
          Rectangle r = ClientRectangle;
          int w = SystemInformation.VerticalScrollBarWidth;
          _ScrollPart.SetBounds(r.Width - w, r.Top, w, r.Height);
          _MainPart.SetBounds(r.Left, r.Top, r.Width - w, r.Height);
        }
      }

      #endregion

      #region Переопределенные методы UpDownBase

      public override void UpButton()
      {
        _Owner.PerformIncrement(+1);
      }

      public override void DownButton()
      {
        _Owner.PerformIncrement(-1);
      }

      protected override void UpdateEditText()
      {
        // Этого не надо
        //_Owner.InitControlText();
      }

      #endregion

      #region Исправление прокрутки колесиком мыши

      /// <summary>
      /// Событие MouseWheel не вызывается
      /// </summary>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [Bindable(false)]
      [EditorBrowsable(EditorBrowsableState.Never)]
      [Browsable(false)]
      public new event MouseEventHandler MouseWheel
      {
        add { base.MouseWheel += value; }
        remove { base.MouseWheel -= value; }
      }

      /// <summary>
      /// Упрощенная реализация прокрутки колесиком мыши.
      /// В классе UpDownBase выполнется прокрутка, зависящая от системных настроек.
      /// Обычно, за один щелчок колесика выполняется прокрутка на 3 единицы, а не 1, как при нажатии на стрелочки.
      /// Реализуем прокрутку на 1 единицу.
      /// 
      /// К сожалению, событие MouseWheel больше не удается вызвать
      /// </summary>
      /// <param name="args">Аргументы события</param>
      protected override void OnMouseWheel(MouseEventArgs args)
      {
        // base.OnMouseWheel(args);

        // Взято из UpDownBase.OnMouseWheel()
        if ((ModifierKeys & (Keys.Shift | Keys.Alt)) != 0 || MouseButtons != MouseButtons.None)
          return; // Do not scroll when Shift or Alt key is down, or when a mouse button is down.

        if (args.Delta == 0)
          return;

        if (args.Delta > 0)
          UpButton();
        else
          DownButton();
      }

      #endregion
    }

    private Control _MainControl;

    private bool IsUpDown { get { return _MainControl is InternalUpDown; } }

    private bool _InsideInitMainControl;

    private void InitMainControl(bool upDown)
    {
      if (_InsideInitMainControl)
        throw new ReenteranceException();

      _InsideInitMainControl = true;
      try
      {
        // Запоминаем свойства
        HorizontalAlignment oldTextAlign = this.TextAlign;
        string oldText = this.Text;
        bool oldReadOnly = this.ReadOnly;

        bool hasOldControl = false;
        // Удаляем старый элемент
        if (_MainControl != null)
        {
          hasOldControl = true;
          base.Controls.Remove(_MainControl);
          _MainControl.Dispose();
        }

        // Создаем новый элемент
        if (upDown)
          _MainControl = new InternalUpDown(this);
        else
          _MainControl = new TextBox();
        _MainControl.Dock = DockStyle.Fill;

        // Восстанавливаем свойства
        this.TextAlign = oldTextAlign;
        this.Text = oldText;
        this.ReadOnly = oldReadOnly;

        // Текущие значения свойств
        if (!this.ReadOnly)
        {
          _MainControl.BackColor = this.BackColor;
          _MainControl.ForeColor = this.ForeColor;
        }
        _MainControl.Font = this.Font;
        //_MainControl.ContextMenu = this.ContextMenu;
        //_MainControl.ContextMenuStrip = this.ContextMenuStrip;

        base.Controls.Add(_MainControl);

        // После этого присоединяем обработчики
        _MainControl.TextChanged += MainControl_TextChanged;

        _MainControl.KeyDown += new System.Windows.Forms.KeyEventHandler(MainControl_KeyDown);
        _MainControl.KeyUp += new KeyEventHandler(MainControl_KeyUp);
        _MainControl.KeyPress += new KeyPressEventHandler(MainControl_KeyPress);

        _MainControl.MouseDown += new MouseEventHandler(MainControl_MouseDown);
        _MainControl.MouseUp += new MouseEventHandler(MainControl_MouseUp);
        _MainControl.MouseClick += new MouseEventHandler(MainControl_MouseClick);
        _MainControl.MouseDoubleClick += new MouseEventHandler(MainControl_MouseDoubleClick);
        _MainControl.MouseWheel += new MouseEventHandler(MainControl_MouseWheel);
        _MainControl.Click += new EventHandler(MainControl_Click);
        _MainControl.DoubleClick += new EventHandler(MainControl_DoubleClick);

        _MainControl.SizeChanged += MainControl_SizeChanged;

        if (hasOldControl)
          Invalidate();
      }
      finally
      {
        _InsideInitMainControl = false;
      }
    }

    void MainControl_SizeChanged(object sender, EventArgs args)
    {
      this.Height = _MainControl.Height;
    }

    #endregion

    #region Свойства Value/NValue

    private bool _InsideValueChanged;

    [Bindable(true)]
    [DefaultValue(null)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("Current value with null support")]
    [Category("Appearance")]
    public T? NValue
    {
      get { return _NValue; }
      set
      {
        if (value.Equals(_NValue))
          return;

        if (_InsideValueChanged)
          return;
        _InsideValueChanged = true;
        try
        {
          if (value.HasValue)
            _NValue = GetRoundedValue(value.Value);
          else
            _NValue = null; // 23.11.2021
          if (!_InsideMainControl_TextChanged)
          {
            InitControlText();
            _TextIsValid = true;
          }
          OnValueChanged(EventArgs.Empty);
        }
        finally
        {
          _InsideValueChanged = false;
        }
      }
    }
    private T? _NValue; // текущее значение

    [Bindable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [RefreshProperties(RefreshProperties.All)]
    [Description("Current value without null")]
    [Category("Appearance")]
    [Browsable(false)]
    public T Value
    {
      get { return NValue ?? default(T); }
      set { NValue = value; }
    }

    protected virtual void OnValueChanged(EventArgs args)
    {
      if (ValueChanged != null)
        ValueChanged(this, args);
    }

    [Description("Called when NValue and Value property changed")]
    [Category("Property Changed")]
    public event EventHandler ValueChanged;

    /// <summary>
    /// Переопределенный метод должен выполнить округление с учетом количества десятичных знаков в формате
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected abstract T GetRoundedValue(T value);

    #endregion

    #region Свойство Increment

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IUpDownHandler<T?> UpDownHandler
    {
      get { return _UpDownHandler; }
      set
      {
        if (Object.ReferenceEquals(value, _UpDownHandler))
          return;
        _UpDownHandler = value;

        bool newUpDown = (value != null);
        if (newUpDown != IsUpDown)
          InitMainControl(newUpDown);
      }
    }
    private IUpDownHandler<T?> _UpDownHandler;

    [Bindable(true)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("Increment value. If 0, there are no arrow button presented. Positive value allows scrolling current value.")]
    [Category("Appearance")]
    [DefaultValue(0)]
    public T Increment
    {
      get
      {
        IncrementUpDownHandler<T> incObj = UpDownHandler as IncrementUpDownHandler<T>;
        if (incObj == null)
          return default(T);
        else
          return incObj.Increment;
      }
      set
      {
        if (value.Equals(this.Increment))
          return;

        if (value.CompareTo(default(T)) < 0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        if (value.CompareTo(default(T)) == 0)
          UpDownHandler = null;
        else
          UpDownHandler = IncrementUpDownHandler<T>.Create(value, this);
      }
    }

    internal void PerformIncrement(int sign)
    {
      if (ReadOnly)
        return;
      if (!TextIsValid)
        return;
      try
      {
        bool hasNext, hasPrev;
        T? nextValue, prevValue;
        UpDownHandler.GetUpDown(NValue, out hasNext, out nextValue, out hasPrev, out prevValue);

        bool has = sign > 0 ? hasNext : hasPrev;
        T? value = sign > 0 ? nextValue : prevValue;

        if (has)
          NValue = value;
      }
      catch { } // Перехват OvertflowException

    }

    #endregion

    #region Свойства Minimum и Maximum

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("Minimum allowed value for scrolling, if any")]
    [Category("Appearance")]
    [DefaultValue(null)]
    public T? Minimum
    {
      get { return _Minimum; }
      set { _Minimum = value; }
    }
    private T? _Minimum;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("Maximum allowed value for scrolling, if any")]
    [Category("Appearance")]
    [DefaultValue(null)]
    public T? Maximum
    {
      get { return _Maximum; }
      set { _Maximum = value; }
    }
    private T? _Maximum;

    #endregion

    #region Свойство Format

    [Bindable(true)]
    [DefaultValue("")]
    [Description("Number format")]
    [RefreshProperties(RefreshProperties.All)]
    [Category("Appearance")]
    public string Format
    {
      get { return _Format; }
      set
      {
        if (value == null)
          value = String.Empty;
        if (String.Equals(value, _Format, StringComparison.Ordinal))
          return;

        // Проверяем корректность формата
        // Используем InvariantCulture во избежание неожиданностей от национальных настроек
        default(T).ToString(value, CultureInfo.InvariantCulture); // может произойти FormatException

        _Format = value;
        OnFormatChanged();
      }
    }
    private string _Format;

    private void OnFormatChanged()
    {
      InitControlText();
    }

    /// <summary>
    /// Форматировщик для числового значения
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IFormatProvider FormatProvider
    {
      get
      {
        if (_FormatProvider == null)
          return CultureInfo.CurrentCulture;
        else
          return _FormatProvider;
      }
      set
      {
        _FormatProvider = value;
      }
    }
    private IFormatProvider _FormatProvider;

    /// <summary>
    /// Вспомогательное свойство.
    /// Возвращает количество десятичных разрядов для числа с плавающей точкой, которое определено в свойстве Format
    /// </summary>
    [DefaultValue("")]
    [Description("Number of digits after the decimal point. Alternative to Format property")]
    [RefreshProperties(RefreshProperties.All)]
    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual int DecimalPlaces
    {
      get { return FormatStringTools.DecimalPlacesFromNumberFormat(Format); }
      set { Format = FormatStringTools.DecimalPlacesToNumberFormat(value); }
    }

    #endregion

    #region Свойство ReadOnly

    [Description("View only mode")]
    [Category("Appearance")]
    [DefaultValue(false)]
    public bool ReadOnly
    {
      get
      {
        if (_MainControl == null)
          return false;
        if (IsUpDown)
          return ((InternalUpDown)_MainControl).ReadOnly;
        else
          return ((TextBox)_MainControl).ReadOnly;
      }
      set
      {
        if (value == this.ReadOnly)
          return;

        if (IsUpDown)
          ((InternalUpDown)_MainControl).ReadOnly = value;
        else
          ((TextBox)_MainControl).ReadOnly = value;

        if (!_InsideInitMainControl)
          OnReadOnlyChanged(EventArgs.Empty);

        CopyColors();
      }
    }

    [Description("Called when ReadOnly property changed")]
    [Category("PropertyChanged")]
    public event EventHandler ReadOnlyChanged;

    protected virtual void OnReadOnlyChanged(EventArgs args)
    {
      if (ReadOnlyChanged != null)
        ReadOnlyChanged(this, args);
    }

    #endregion

    #region Текстовое представление

    private void InitControlText()
    {
      if (_NValue.HasValue)
      {
        {
          try
          {
            _MainControl.Text = _NValue.Value.ToString(Format, FormatProvider);
          }
          catch
          {
            _MainControl.ToString();
          }
        }
      }
      else
        _MainControl.Text = String.Empty;
    }

    /// <summary>
    /// Свойство возвращает true, если текущий введенный текст может быть преобразован в число
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [Browsable(false)]
    public bool TextIsValid { get { return _TextIsValid; } }
    private bool _TextIsValid;

    private bool _InsideMainControl_TextChanged;

    void MainControl_TextChanged(object sender, EventArgs args)
    {
      if ((!_InsideValueChanged) && (!_InsideMainControl_TextChanged))
      {
        _InsideMainControl_TextChanged = true;
        try
        {
          if (String.IsNullOrEmpty(_MainControl.Text))
          {
            NValue = null;
            _TextIsValid = true; // 23.11.2021
          }
          else
          {
            string s = _MainControl.Text;
            UITools.CorrectNumberString(ref s, this.FormatProvider); // замена точки и запятой

            T value;
            _TextIsValid = TryParseText(s, out value);
            if (_TextIsValid)
              NValue = value;
          }
        }
        finally
        {
          _InsideMainControl_TextChanged = false;
        }
      }

      OnTextChanged(EventArgs.Empty); // 23.11.2021
    }

    protected abstract bool TryParseText(string s, out T value);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public override string Text
    {
      get
      {
        if (_MainControl == null)
          return String.Empty;
        else
          return _MainControl.Text;
      }
      set { _MainControl.Text = value; }
    }

    [Description("Horizontal text alignment")]
    [Localizable(true)]
    [DefaultValue(HorizontalAlignment.Right)]
    [Category("Appearance")]
    public HorizontalAlignment TextAlign
    {
      get
      {
        if (_MainControl == null)
          return HorizontalAlignment.Right;
        if (IsUpDown)
          return ((InternalUpDown)_MainControl).TextAlign;
        else
          return ((TextBox)_MainControl).TextAlign;
      }
      set
      {
        if (IsUpDown)
          ((InternalUpDown)_MainControl).TextAlign = value;
        else
          ((TextBox)_MainControl).TextAlign = value;
      }
    }

    #endregion

    #region Шрифт и цвет

    protected override void OnFontChanged(EventArgs args)
    {
      base.OnFontChanged(args);
      if (_MainControl != null)
        _MainControl.Font = this.Font;
    }

    [DefaultValue(typeof(Color), "Window")]
    public override Color BackColor
    {
      get { return base.BackColor; }
      set { base.BackColor = value; }
    }

    [DefaultValue(typeof(Color), "WindowText")]
    public override Color ForeColor
    {
      get { return base.ForeColor; }
      set { base.ForeColor = value; }
    }

    public override void ResetBackColor()
    {
      base.BackColor = SystemColors.Window;
    }

    public override void ResetForeColor()
    {
      base.ForeColor = SystemColors.WindowText;
    }

    protected override void OnForeColorChanged(EventArgs args)
    {
      base.OnForeColorChanged(args);
      CopyColors();
    }

    protected override void OnBackColorChanged(EventArgs args)
    {
      base.OnBackColorChanged(args);
      CopyColors();
    }

    private void CopyColors()
    {
      if (_MainControl == null)
        return;

      if (Enabled)
      {
        if (ReadOnly)
        {
          _MainControl.ForeColor = SystemColors.ControlText;
          _MainControl.BackColor = SystemColors.Control;
        }
        else
        {
          _MainControl.ForeColor = this.ForeColor;
          _MainControl.BackColor = this.BackColor;
        }
      }
      else
      {
        _MainControl.ForeColor = SystemColors.GrayText;
        _MainControl.BackColor = SystemColors.Control;
      }
    }

    protected override void OnEnabledChanged(EventArgs args)
    {
      base.OnEnabledChanged(args);
      if (_MainControl != null)
        _MainControl.Enabled = this.Enabled;
      CopyColors();
    }

    #endregion

    #region Свойства TextBox

    [Browsable(false)]
    [DesignerSerializationVisibility(0)]
    public int SelectionStart
    {
      get
      {
        if (_MainControl == null)
          return 0;
        if (IsUpDown)
          return ((InternalUpDown)_MainControl).MainPart.SelectionStart;
        else
          return ((TextBox)_MainControl).SelectionStart;
      }
      set
      {
        if (IsUpDown)
          ((InternalUpDown)_MainControl).MainPart.SelectionStart = value;
        else
          ((TextBox)_MainControl).SelectionStart = value;
      }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(0)]
    public int SelectionLength
    {
      get
      {
        if (_MainControl == null)
          return 0;
        if (IsUpDown)
          return ((InternalUpDown)_MainControl).MainPart.SelectionLength;
        else
          return ((TextBox)_MainControl).SelectionLength;
      }
      set
      {
        if (IsUpDown)
          ((InternalUpDown)_MainControl).MainPart.SelectionLength = value;
        else
          ((TextBox)_MainControl).SelectionLength = value;
      }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(0)]
    public string SelectedText
    {
      get
      {
        if (_MainControl == null)
          return String.Empty;
        if (IsUpDown)
          return ((InternalUpDown)_MainControl).MainPart.SelectedText;
        else
          return ((TextBox)_MainControl).SelectedText;
      }
      set
      {
        if (IsUpDown)
          ((InternalUpDown)_MainControl).MainPart.SelectedText = value;
        else
          ((TextBox)_MainControl).SelectedText = value;
      }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(0)]
    public void Select(int start, int length)
    {
      if (IsUpDown)
        ((InternalUpDown)_MainControl).Select(start, length);
      else
        ((TextBox)_MainControl).Select(start, length);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(0)]
    public void SelectAll()
    {
      if (IsUpDown)
        ((InternalUpDown)_MainControl).MainPart.SelectAll();
      else
        ((TextBox)_MainControl).SelectAll();
    }

    #endregion

    #region Передача других событий от основного элемента

    void MainControl_Click(object sender, EventArgs args)
    {
      OnClick(args);
    }

    void MainControl_DoubleClick(object sender, EventArgs args)
    {
      OnDoubleClick(args);
    }

    void MainControl_MouseClick(object sender, MouseEventArgs args)
    {
      OnMouseClick(args);
    }

    void MainControl_MouseDoubleClick(object sender, MouseEventArgs args)
    {
      OnMouseDoubleClick(args);
    }

    void MainControl_MouseUp(object sender, MouseEventArgs args)
    {
      OnMouseUp(args);
    }

    void MainControl_MouseDown(object sender, MouseEventArgs args)
    {
      OnMouseDown(args);
    }

    void MainControl_MouseWheel(object sender, MouseEventArgs args)
    {
      OnMouseWheel(args);
    }

    void MainControl_KeyPress(object sender, KeyPressEventArgs args)
    {
      OnKeyPress(args);
    }


    private void MainControl_KeyDown(object sender, KeyEventArgs args)
    {
      OnKeyDown(args);
    }


    void MainControl_KeyUp(object sender, KeyEventArgs args)
    {
      OnKeyUp(args);
    }

    #endregion

    #region Другие переопределенные свойства и методы

    protected override Size DefaultSize
    {
      get
      {
        if (_MainControl == null)
          return new Size(150, 20);
        else
          return new Size(150, _MainControl.Size.Height);
      }
    }

    /// <summary>
    /// При выходе из элемента выполняем форматирование текста
    /// </summary>
    /// <param name="args"></param>
    protected override void OnLeave(EventArgs args)
    {
      if (_TextIsValid)
        InitControlText();

      base.OnLeave(args);
    }

    protected override void Select(bool directed, bool forward)
    {
      base.Select(directed, forward);
      _MainControl.Select();
      SelectAll();
    }

    //protected override void OnContextMenuChanged(EventArgs args)
    //{
    //  base.OnContextMenuChanged(args);
    //  if (_MainControl != null)
    //    _MainControl.ContextMenu = this.ContextMenu;
    //}

    //protected override void OnContextMenuStripChanged(EventArgs args)
    //{
    //  base.OnContextMenuStripChanged(args);
    //  if (_MainControl != null)
    //    _MainControl.ContextMenuStrip = this.ContextMenuStrip;
    //}

    #endregion

    #region Заглушки для свойств

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new BorderStyle BorderStyle { get { return base.BorderStyle; } set { base.BorderStyle = value; } }

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

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool AutoSize { get { return base.AutoSize; } set { base.AutoSize = value; } }


    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new AutoSizeMode AutoSizeMode { get { return base.AutoSizeMode; } set { base.AutoSizeMode = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new Padding Padding { get { return base.Padding; } set { base.Padding = value; } }

    #endregion
  }


  /// <summary>
  /// Поле ввода числового значения типа Double
  /// </summary>
  [Description("Text box for input an integer value with null support")]
  [ToolboxBitmap(typeof(Int32EditBox), "NumEditBox.bmp")]
  [ToolboxItem(true)]
  [DesignerSerializer("System.Windows.Forms.Design.ControlCodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
  public class Int32EditBox : NumEditBoxBase<Int32>
  {
    #region Переопределенные методы

    /// <summary>
    /// Вызывает метод Int32.TryParse() с соответствующими флагами
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="result">Числовое значение</param>
    /// <returns>true, если преобразование выполнено</returns>
    protected override bool TryParseText(string s, out int result)
    {
      return Int32.TryParse(s, NumberStyles.Integer | NumberStyles.AllowParentheses | NumberStyles.AllowThousands, FormatProvider, out result);
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="value">Значение до округления</param>
    /// <returns>Округленное значение</returns>
    protected override int GetRoundedValue(int value)
    {
      return value;
    }

    /// <summary>
    /// Возвращает 0
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override int DecimalPlaces { get { return 0; } }

    #endregion
  }

  /// <summary>
  /// Поле ввода числового значения типа Double
  /// </summary>
  [Description("Text box for input a floating point value of type Single with null support")]
  [ToolboxBitmap(typeof(SingleEditBox), "NumEditBox.bmp")]
  [ToolboxItem(true)]
  public class SingleEditBox : NumEditBoxBase<Single>
  {
    #region Переопределенные методы

    /// <summary>
    /// Вызывает метод Single.TryParse() с соответствующими флагами
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="result">Числовое значение</param>
    /// <returns>true, если преобразование выполнено</returns>
    protected override bool TryParseText(string s, out float result)
    {
      return Single.TryParse(s, NumberStyles.Float | NumberStyles.AllowParentheses | NumberStyles.AllowThousands, FormatProvider, out result);
    }

    /// <summary>
    /// Выполняет округление до числа разрядов, определяемых свойством DecimalPlaces
    /// </summary>
    /// <param name="value">Значение до округления</param>
    /// <returns>Округленное значение</returns>
    protected override float GetRoundedValue(float value)
    {
      int dp = this.DecimalPlaces;
      if (dp >= 0)
        // Нет Math.Round() для float.
        return (float)Math.Round((double)value, dp, MidpointRounding.AwayFromZero);
      else
        return value;
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода числового значения типа Double
  /// </summary>
  [Description("Text box for input a floating point value of type Double with null support")]
  [ToolboxBitmap(typeof(DoubleEditBox), "NumEditBox.bmp")]
  [ToolboxItem(true)]
  public class DoubleEditBox : NumEditBoxBase<Double>
  {
    #region Переопределенные методы

    /// <summary>
    /// Вызывает метод Double.TryParse() с соответствующими флагами
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="result">Числовое значение</param>
    /// <returns>true, если преобразование выполнено</returns>
    protected override bool TryParseText(string s, out double result)
    {
      return Double.TryParse(s, NumberStyles.Float | NumberStyles.AllowParentheses | NumberStyles.AllowThousands, FormatProvider, out result);
    }

    /// <summary>
    /// Выполняет округление до числа разрядов, определяемых свойством DecimalPlaces
    /// </summary>
    /// <param name="value">Значение до округления</param>
    /// <returns>Округленное значение</returns>
    protected override double GetRoundedValue(double value)
    {
      int dp = this.DecimalPlaces;
      if (dp >= 0)
        return Math.Round(value, dp, MidpointRounding.AwayFromZero);
      else
        return value;
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода числового значения типа Double
  /// </summary>
  [Description("Text box for input a floating point value of type Decimal with null support")]
  [ToolboxBitmap(typeof(DecimalEditBox), "NumEditBox.bmp")]
  [ToolboxItem(true)]
  public class DecimalEditBox : NumEditBoxBase<Decimal>
  {
    #region Переопределенные методы

    /// <summary>
    /// Вызывает метод Decimal.TryParse() с соответствующими флагами
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="result">Числовое значение</param>
    /// <returns>true, если преобразование выполнено</returns>
    protected override bool TryParseText(string s, out decimal result)
    {
      return Decimal.TryParse(s, NumberStyles.Float | NumberStyles.AllowParentheses | NumberStyles.AllowThousands, FormatProvider, out result);
    }

    /// <summary>
    /// Выполняет округление до числа разрядов, определяемых свойством DecimalPlaces
    /// </summary>
    /// <param name="value">Значение до округления</param>
    /// <returns>Округленное значение</returns>
    protected override decimal GetRoundedValue(decimal value)
    {
      int dp = this.DecimalPlaces;
      if (dp >= 0)
        return Math.Round(value, dp, MidpointRounding.AwayFromZero);
      else
        return value;
    }

    #endregion
  }
}
