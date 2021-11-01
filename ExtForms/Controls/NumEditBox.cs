using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Forms.Design.Behavior;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.Formatting;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  [Designer(typeof(NumEditBoxBaseDesigner))]
  public abstract class NumEditBoxBase<T> : UserControl
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
        foreach (Control c in base.Controls)
        {
          if (c is TextBox)
          {
            _MainPart = (TextBox)c;
            break;
          }
        }
        if (_MainPart == null)
          throw new BugException("Не найден TextBox");
      }

      private NumEditBoxBase<T> _Owner;

      #endregion

      #region Свойства

      /// <summary>
      /// Основной элемент - TextBox.
      /// См. исходный текст класса net framework UpDownBase.
      /// </summary>
      public TextBox MainPart { get { return _MainPart; } }
      private TextBox _MainPart;

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
    [Description("Текущее значение с выделением пустого значения")]
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
    [Description("Текущее значение без поддержки null")]
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

    [Description("Посылается после изменения свойств NValue/Value")]
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

    [Bindable(true)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("Инкремент. Если равно 0, то есть только поле ввода. Положительное значение приводит к появлению стрелочек для прокрутки значения")]
    [Category("Appearance")]
    [DefaultValue(0.0)]
    public T Increment
    {
      get { return _Increment; }
      set
      {
        if (value.Equals(_Increment))
          return;

        if (value.CompareTo(default(T)) < 0)
          throw new ArgumentOutOfRangeException("value", value, "Значение должно быть больше или равно 0");

        _Increment = value;

        bool newUpDown = !value.Equals(default(T));
        if (newUpDown != IsUpDown)
          InitMainControl(newUpDown);
      }
    }
    private T _Increment;

    internal void PerformIncrement(int sign)
    {
      if (ReadOnly)
        return;
      if (!TextIsValid)
        return;
      if (!NValue.HasValue)
        return;

      try
      {
        // Нельзя вычислить инкремент в шаблонном классе
        T newValue = GetIncrementedValue(sign);
        if (sign > 0)
        {
          if (Maximum.HasValue)
          {
            if (newValue.CompareTo(Maximum.Value) > 0)
              newValue = Maximum.Value;
          }
        }
        else
        {
          if (Minimum.HasValue)
          {
            if (newValue.CompareTo(Minimum.Value) < 0)
              newValue = Minimum.Value;
          }
        }
        this.Value = newValue;
      }
      catch { } // Перехват OvertflowException

    }

    protected abstract T GetIncrementedValue(int sign);

    #endregion

    #region Свойства Minimum и Maximum

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[RefreshProperties(RefreshProperties.All)]
    [Description("Минимальное значение, используемое для прокрутки")]
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
    [Description("Максимальное значение, используемое для прокрутки")]
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
    [Description("Форматирование текстового вывода")]
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
    [Description("Количество знаков после запятой. Альтернативная установка для свойства Format")]
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

    [Description("Режим \"Только для просмотра\"")]
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

    [Description("Изменилось свойство ReadOnly")]
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
      if (_InsideValueChanged)
        return;

      if (_InsideMainControl_TextChanged)
        return;

      _InsideMainControl_TextChanged = true;
      try
      {
        if (String.IsNullOrEmpty(_MainControl.Text))
        {
          NValue = null;
        }
        else
        {
          string s = _MainControl.Text;
          FreeLibSet.Forms.WinFormsTools.CorrectNumberString(ref s, this.FormatProvider); // замена точки и запятой

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

    [Description("Горизонтальное выравнивание (по умолчанию - по правому краю)")]
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
          return base.DefaultSize;
        else
          return _MainControl.Size;
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
  }


  public class NumEditBoxBaseDesigner : ControlDesigner
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
        Rules = Rules & (~(System.Windows.Forms.Design.SelectionRules.BottomSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable));
        return Rules;
      }
    }

    #endregion

    #region Snap lines

    // Добавляем "сиреневую" линию базовой линии текста для дизайнера формы
    // Линия берется из основного элемента
    // Взято из 
    // http://stackoverflow.com/questions/93541/baseline-snaplines-in-custom-winforms-controls
    //

    public override System.Collections.IList SnapLines
    {
      get
      {
        /* Code from above */
        System.Collections.IList snapLines = base.SnapLines;


        // *** This will need to be modified to match the item in your user control
        // This is the control in your UC that you want SnapLines for the entire UC
        IDesigner designer = TypeDescriptor.CreateDesigner(Control.Controls[0], typeof(IDesigner));
        if (designer == null)
          return snapLines;

        // *** This will need to be modified to match the item in your user control
        designer.Initialize(Control.Controls[0]);

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
                 line.Offset + Control.Controls[0].Top, // всегда 0
                 line.Filter, line.Priority));
              break;
            }
          }
        }

        return snapLines;
      }
    }

    #endregion

    //public override void InitializeNewComponent(IDictionary defaultValues)
    //{
    //  defaultValues.Remove("Text");
    //  base.InitializeNewComponent(defaultValues);
    //}
  }

  /// <summary>
  /// Поле ввода числового значения типа Double
  /// </summary>
  [Description("Поле ввода целого числа")]
  [ToolboxBitmap(typeof(IntEditBox), "NumEditBox.bmp")]
  [ToolboxItem(true)]
  public class IntEditBox : NumEditBoxBase<Int32>
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
    /// Вызывает UITools.GetIncrementedValue()
    /// </summary>
    /// <param name="sign">Знак инкремента</param>
    /// <returns>Новое значение</returns>
    protected override int GetIncrementedValue(int sign)
    {
      return UITools.GetIncrementedValue(Value, sign > 0 ? Increment : -Increment);
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
    public override int DecimalPlaces { get { return 0; } }

    #endregion
  }

  /// <summary>
  /// Поле ввода числового значения типа Double
  /// </summary>
  [Description("Поле ввода числового значения типа Single")]
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
    /// Вызывает UITools.GetIncrementedValue()
    /// </summary>
    /// <param name="sign">Знак инкремента</param>
    /// <returns>Новое значение</returns>
    protected override float GetIncrementedValue(int sign)
    {
      return UITools.GetIncrementedValue(Value, sign > 0 ? Increment : -Increment);
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
  [Description("Поле ввода числового значения типа Double")]
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
    /// Вызывает UITools.GetIncrementedValue()
    /// </summary>
    /// <param name="sign">Знак инкремента</param>
    /// <returns>Новое значение</returns>
    protected override double GetIncrementedValue(int sign)
    {
      return UITools.GetIncrementedValue(Value, sign > 0 ? Increment : -Increment);
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
  [Description("Поле ввода числового значения типа Decimal")]
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
    /// Вызывает UITools.GetIncrementedValue()
    /// </summary>
    /// <param name="sign">Знак инкремента</param>
    /// <returns>Новое значение</returns>
    protected override decimal GetIncrementedValue(int sign)
    {
      return UITools.GetIncrementedValue(Value, sign > 0 ? Increment : -Increment);
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
