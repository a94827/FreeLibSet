// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Значок для MessageBox
  /// </summary>
  [DefaultProperty("Icon")]
  [DefaultEvent("IconChanged")]
  [Description("Значок для MessageBox")]
  [ToolboxBitmap(typeof(MessageBoxIconBox), "MessageBoxIconBox.bmp")]
  public class MessageBoxIconBox : Control
  {
    #region Конструктор

    public MessageBoxIconBox()
    {
      InitializeComponent();

      SetStyle(ControlStyles.UserPaint, true);
      SetStyle(ControlStyles.SupportsTransparentBackColor, true);
      SetStyle(ControlStyles.Selectable, false);
      SetStyle(ControlStyles.ResizeRedraw, true);
      _Icon = MessageBoxIcon.Information;
      _IconSize = MessageBoxIconSize.Small;
      base.BackColor = Color.Transparent;
      base.SetAutoSizeMode(AutoSizeMode.GrowAndShrink);
    }

    #endregion

    #region Дизайнер

    private void InitializeComponent()
    {
    }

    #endregion

    #region Свойство Icon

    [Description("Значок")]
    [Category("Appearance")]
    [DefaultValue(MessageBoxIcon.Information)]
    [TypeConverter(typeof(MessageBoxIconEnumConverter))]
    public MessageBoxIcon Icon
    {
      get { return _Icon; }
      set
      {
        if (value == _Icon)
          return;
        _Icon = value;
        OnIconChanged(EventArgs.Empty);
        Invalidate();
      }
    }
    private MessageBoxIcon _Icon;

    #endregion

    #region Свойство IconSize

    [Description("Размер значка")]
    [Category("Appearance")]
    [DefaultValue(MessageBoxIconSize.Small)]
    public MessageBoxIconSize IconSize
    {
      get { return _IconSize; }
      set
      {
        if (value == _IconSize)
          return;
        _IconSize = value;

        Size sz1 = GetImageSize(); // 32,32
        if (value == MessageBoxIconSize.Large && (Size.Width < sz1.Width || Size.Height < sz1.Height))
          this.Size = new Size(Math.Max(Size.Width, sz1.Width), Math.Max(Size.Height, sz1.Height));

        OnIconChanged(EventArgs.Empty);
        Invalidate();
      }
    }
    private MessageBoxIconSize _IconSize;

    #endregion

    #region Событие IconChanged

    [Description("Вызывается при изменении свойств Icon и IconSize")]
    [Category("Property Changed")]
    public event EventHandler IconChanged;

    protected virtual void OnIconChanged(EventArgs args)
    {
      if (IconChanged != null)
        IconChanged(this, args);
    }

    #endregion

    #region OnPaint

    protected override void OnPaint(PaintEventArgs args)
    {
      Size sz = GetImageSize();

      Rectangle rc1 = ClientRectangle;
      rc1 = new Rectangle(rc1.Left + Padding.Left, rc1.Top + Padding.Top, rc1.Width - Padding.Horizontal, rc1.Height - Padding.Vertical);
      Rectangle rc2 = new Rectangle(rc1.Left + (rc1.Width - sz.Width) / 2,
        rc1.Top + (rc1.Height - sz.Height) / 2,
        sz.Width,
        sz.Height);

      if (Rectangle.Intersect(rc2, args.ClipRectangle).IsEmpty)
        return; // не надо рисовать

      if (IconSize == MessageBoxIconSize.Large)
      {
        Icon ic;
        switch (_Icon)
        {
          case MessageBoxIcon.Information: ic = SystemIcons.Information; break;
          case MessageBoxIcon.Error: ic = SystemIcons.Error; break;
          case MessageBoxIcon.Warning: ic = SystemIcons.Warning; break;
          case MessageBoxIcon.Question: ic = SystemIcons.Question; break;
          default:
            return; // Нечего рисовать
        }

        args.Graphics.DrawIcon(ic, rc2);
      }
      else
      {
        Image img;
        switch (_Icon)
        {
          case MessageBoxIcon.Information: img = MainImagesResource.Information; break;
          case MessageBoxIcon.Error: img = MainImagesResource.Error; break;
          case MessageBoxIcon.Warning: img = MainImagesResource.Warning; break;
          case MessageBoxIcon.Question: img = MainImagesResource.UnknownState; break;
          default:
            return; // Нечего рисовать
        }
#if DEBUG
        if (img == null)
          throw new BugException("img==null");
#endif
        args.Graphics.DrawImage(img, rc2);
      }
    }

    #endregion

    #region Ограничение размеров

    private Size GetImageSize()
    {
      if (IconSize == MessageBoxIconSize.Small)
        return SystemInformation.SmallIconSize;
      else
        return SystemInformation.IconSize;
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
      return GetImageSize() + Padding.Size;
    }

    protected override Size DefaultSize
    {
      get
      {
        // Метод может вызываться до вызова конструктора
        return GetImageSize() + Padding.Size;
      }
    }

    protected override Size DefaultMinimumSize
    {
      get
      {
        return GetImageSize() + Padding.Size;
      }
    }

    #endregion

    #region BackColor

    [DefaultValue(typeof(Color), "Transparent")]
    public override Color BackColor
    {
      get { return base.BackColor; }
      set { base.BackColor = value; }
    }

    public override void ResetBackColor()
    {
      BackColor = Color.Transparent;
    }

    #endregion

    #region Свойство AutoSize

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public override bool AutoSize
    {
      get
      {
        return base.AutoSize;
      }
      set
      {
        base.AutoSize = value;
      }
    }

    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public new event EventHandler AutoSizeChanged
    {
      add { base.AutoSizeChanged += value; }
      remove { base.AutoSizeChanged -= value; }
    }

    protected override void OnAutoSizeChanged(EventArgs args)
    {
      base.OnAutoSizeChanged(args);

      if (base.AutoSize)
        Size = GetImageSize() + Padding.Size;
      SetStyle(ControlStyles.FixedWidth, !base.AutoSize);
      SetStyle(ControlStyles.FixedWidth, !base.AutoSize);
      UpdateStyles();
    }


    #endregion

    #region Заглушки для свойств

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool AllowDrop
    {
      get { return base.AllowDrop; }
      set { base.AllowDrop = value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new bool CausesValidation
    {
      get { return base.CausesValidation; }
      set { base.CausesValidation = value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override Font Font
    {
      get { return base.Font; }
      set { base.Font = value; }
    }
    //
    // Сводка:
    //     Переопределяет свойство System.Windows.Forms.Control.ForeColor.
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override Color ForeColor
    {
      get { return base.ForeColor; }
      set { base.ForeColor = value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new ImeMode ImeMode
    {
      get { return base.ImeMode; }
      set { base.ImeMode = value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override RightToLeft RightToLeft
    {
      get { return base.RightToLeft; }
      set { base.RightToLeft = value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new int TabIndex
    {
      get { return base.TabIndex; }
      set { base.TabIndex = value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new bool TabStop
    {
      get { return base.TabStop; }
      set { base.TabStop = value; }
    }

    [Bindable(false)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string Text
    {
      get { return base.Text; }
      set { base.Text = value; }
    }

    #endregion

    #region Заглушки для событий

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event EventHandler CausesValidationChanged
    {
      add { base.CausesValidationChanged += value; }
      remove { base.CausesValidationChanged -= value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event EventHandler Enter
    {
      add { base.Enter += value; }
      remove { base.Enter -= value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event EventHandler FontChanged
    {
      add { base.FontChanged += value; }
      remove { base.FontChanged -= value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event EventHandler ForeColorChanged
    {
      add { base.ForeColorChanged += value; }
      remove { base.ForeColorChanged -= value; }
    }

    //
    // Сводка:
    //     Происходит при изменении значения свойства System.Windows.Forms.PictureBox.ImeMode.
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event EventHandler ImeModeChanged
    {
      add { base.ImeModeChanged += value; }
      remove { base.ImeModeChanged -= value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event KeyEventHandler KeyDown
    {
      add { base.KeyDown += value; }
      remove { base.KeyDown -= value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event KeyPressEventHandler KeyPress
    {
      add { base.KeyPress += value; }
      remove { base.KeyPress -= value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event KeyEventHandler KeyUp
    {
      add { base.KeyUp += value; }
      remove { base.KeyUp -= value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event EventHandler Leave
    {
      add { base.Leave += value; }
      remove { base.Leave -= value; }
    }


    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event EventHandler RightToLeftChanged
    {
      add { base.RightToLeftChanged += value; }
      remove { base.RightToLeftChanged -= value; }
    }


    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event EventHandler TabIndexChanged
    {
      add { base.TabIndexChanged += value; }
      remove { base.TabIndexChanged -= value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event EventHandler TabStopChanged
    {
      add { base.TabStopChanged += value; }
      remove { base.TabStopChanged -= value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event EventHandler TextChanged
    {
      add { base.TextChanged += value; }
      remove { base.TextChanged -= value; }
    }


    #endregion
  }

  #region Перечисление MessageBoxIconSize

  public enum MessageBoxIconSize
  {
    /// <summary>
    /// Значок 16х16
    /// </summary>
    Small,

    /// <summary>
    /// Значок 32х32
    /// </summary>
    Large
  }

  #endregion


  /// <summary>
  /// Конвертер для свойства Enum.
  /// Назначение:
  /// 1. Ограничить список значений (без повторов)
  /// 2. Выбирать из альтернативных названий перечисления тот, который мне больше нравится
  /// </summary>
  internal class MessageBoxIconEnumConverter : EnumConverter
  {
    #region Конструктор

    public MessageBoxIconEnumConverter()
      : base(typeof(MessageBoxIcon))
    {
      MessageBoxIcon[] aValues = new MessageBoxIcon[]{
          MessageBoxIcon.Information, 
          MessageBoxIcon.Warning, 
          MessageBoxIcon.Error, 
          MessageBoxIcon.Question,
          MessageBoxIcon.None};

      base.Values = new StandardValuesCollection(aValues);
    }

    #endregion

    #region Переопределенные методы

    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
    {
      if ((value is MessageBoxIcon) && destinationType == typeof(string))
      {
        switch ((MessageBoxIcon)value)
        {
          case MessageBoxIcon.Information: return "Information";
          case MessageBoxIcon.Warning: return "Warning";
          case MessageBoxIcon.Error: return "Error";
          case MessageBoxIcon.Question: return "Question";
          case MessageBoxIcon.None: return "None";
        }
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }

    #endregion
  }

}
