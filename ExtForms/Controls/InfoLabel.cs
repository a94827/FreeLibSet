// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Значок для MessageBox
  /// </summary>
  [DefaultProperty("Text")]
  [DefaultBindingProperty("Text")]
  [DefaultEvent("TextChanged")]
  [Description("Label for showing helpful information with an icon if required")]
  [ToolboxBitmap(typeof(InfoLabel), "InfoLabel.bmp")]
  public class InfoLabel : ContainerControl
  {
    #region Конструктор

    public InfoLabel()
    {
      base.BackColor = SystemColors.Info;
      base.ForeColor = SystemColors.InfoText;

      SetStyle(ControlStyles.SupportsTransparentBackColor, true);
      SetStyle(ControlStyles.ContainerControl, true);
      SetStyle(ControlStyles.Selectable, false); // 05.10.2018
      base.TabStop = false; // 05.10.2018

      _TheLabel = new Label();
      _TheLabel.AutoSize = false;
      _TheLabel.Dock = DockStyle.Fill;
      _TheLabel.BackColor = Color.Transparent;
      _TheLabel.ForeColor = SystemColors.InfoText;
      _TheLabel.UseMnemonic = false;
      _TheLabel.TextAlign = ContentAlignment.MiddleLeft;
      _TheLabel.AutoEllipsis = true; // 13.05.2019
      _TheLabel.Resize += new EventHandler(TheLabel_Resize);
      base.Controls.Add(_TheLabel);
    }

    #endregion

    #region Составные части

    /// <summary>
    /// Основная часть метки
    /// </summary>
    private Label _TheLabel;

    /// <summary>
    /// Значок
    /// </summary>
    private MessageBoxIconBox _TheIconBox;

    private void GetReadyIconBox()
    {
      if (_TheIconBox != null)
        return;

      _TheIconBox = new MessageBoxIconBox();
      _TheIconBox.Icon = MessageBoxIcon.None;
      _TheIconBox.IconSize = MessageBoxIconSize.Small;
      _TheIconBox.AutoSize = true;
      _TheIconBox.Dock = DockStyle.Left;
      InitIconPadding();
      _TheIconBox.Visible = false; // Соответствует MessageBoxIcon.None
      base.Controls.Add(_TheIconBox);
    }

    #endregion

    #region Запрет табуляции

    /// <summary>
    /// Метка не должна получать фокус ввода по табуляцмм
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DefaultValue(false)]
    [Browsable(false)]
    public new bool TabStop
    {
      get { return base.TabStop; }
      set { base.TabStop = value; }
    }

    #endregion

    #region BackColor и ForeColor

    [DefaultValue(typeof(Color), "Info")]
    public override Color BackColor
    {
      get { return base.BackColor; }
      set { base.BackColor = value; }
    }

    public override void ResetBackColor()
    {
      BackColor = SystemColors.Info;
    }

    [DefaultValue(typeof(Color), "InfoText")]
    public override Color ForeColor
    {
      get
      {
        return base.ForeColor;
      }
      set
      {
        base.ForeColor = value;
        if (_TheLabel != null)
          _TheLabel.ForeColor = value;
      }
    }

    public override void ResetForeColor()
    {
      ForeColor = SystemColors.InfoText; // испр. 09.09.2021
    }

    #endregion

    #region Text

    [Editor("System.ComponentModel.Design.MultilineStringEditor", typeof(UITypeEditor))]
    public override string Text
    {
      get
      {
        return base.Text;
      }
      set
      {
        base.Text = value;
        _TheLabel.Text = value;
      }
    }

    protected override void OnTextChanged(EventArgs args)
    {
      base.OnTextChanged(args);
      AdjustSize();
    }

    #endregion

    #region TextAlign

    [Description("Text alignment in the label")]
    [Category("Appearance")]
    [DefaultValue(ContentAlignment.MiddleLeft)]
    public virtual ContentAlignment TextAlign
    {
      get { return _TheLabel.TextAlign; }
      set
      {
        if (value == _TheLabel.TextAlign)
          return;
        _TheLabel.TextAlign = value;
        OnTextAlignChanged(EventArgs.Empty);
      }
    }

    [Description("Called when TextAlign changed")]
    [Category("Property Changed")]
    public event EventHandler TextAlignChanged;

    protected virtual void OnTextAlignChanged(EventArgs args)
    {
      if (TextAlignChanged != null)
        TextAlignChanged(this, args);
    }

    #endregion

    #region Свойство Icon

    [Description("Icon (none, error, warning or information)")]
    [Category("Appearance")]
    [DefaultValue(MessageBoxIcon.None)]
    [TypeConverter(typeof(MessageBoxIconEnumConverter))]
    public MessageBoxIcon Icon
    {
      get
      {
        if (_TheIconBox == null)
          return MessageBoxIcon.None;
        else
          return _TheIconBox.Icon;
      }
      set
      {
        if (value == this.Icon)
          return;

        GetReadyIconBox();
        _TheIconBox.Icon = value;
        _TheIconBox.Visible = (value != MessageBoxIcon.None);
        OnIconChanged(EventArgs.Empty);

        Invalidate();
      }
    }

    #endregion

    #region Свойство IconSize

    [Description("Icon size when the icon property is set")]
    [Category("Appearance")]
    [DefaultValue(MessageBoxIconSize.Small)]
    public MessageBoxIconSize IconSize
    {
      get
      {
        if (_TheIconBox == null)
          return MessageBoxIconSize.Small;
        else
          return _TheIconBox.IconSize;
      }
      set
      {
        if (value == this.IconSize)
          return;
        GetReadyIconBox();
        _TheIconBox.IconSize = value;
        _TheIconBox.Size = _TheIconBox.MinimumSize;
        OnIconChanged(EventArgs.Empty);
      }
    }

    #endregion

    #region Событие IconChanged

    [Description("Called when Icon or IconSize property changed")]
    [Category("Property Changed")]
    public event EventHandler IconChanged;

    protected virtual void OnIconChanged(EventArgs args)
    {
      if (IconChanged != null)
        IconChanged(this, args);


      AdjustSize();
    }

    #endregion

    #region Padding

    // TODO: В дизайнере, если после установки Padding выполнить "Сброс" свойства, то у дочерних элементов свойство Padding не меняется

    // ReSharper disable once RedundantOverriddenMember
    protected override Padding DefaultPadding
    {
      get
      {
        return base.DefaultPadding; // 13.05.2019
        //return new Padding(3);
      }
    }

    protected override void OnPaddingChanged(EventArgs args)
    {
      base.OnPaddingChanged(args);

      _TheLabel.Padding = base.Padding;
      if (_TheIconBox != null)
        InitIconPadding();

      AdjustSize();
    }

    private void InitIconPadding()
    {
      _TheIconBox.Padding = new Padding(base.Padding.Left, base.Padding.Top, 0, base.Padding.Bottom);
      _TheIconBox.Size = _TheIconBox.MinimumSize;
    }

    #endregion

    #region Свойство AutoSize

    /// <summary>
    /// Автоматический подбор размеров метки под содержимое
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Always)]
    [DefaultValue(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[Localizable(true)]
    [Browsable(true)]
    [RefreshProperties(RefreshProperties.All)]
    public override bool AutoSize
    {
      get { return base.AutoSize; }
      set
      {
        if (value == base.AutoSize)
          return;

        // Не устанавливаем Label.AutoSize, т.к. иначе длинный текст будет вылазить за пределы окна, а перенос по словам не будет выполняться
        // TheLabel.AutoSize = value;

        base.AutoSize = value;
      }
    }

    protected override void OnAutoSizeChanged(EventArgs args)
    {
      base.OnAutoSizeChanged(args);
      AdjustSize();
    }

    /// <summary>
    /// Событие вызывается при изменении свойства AutoSize
    /// </summary>
    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public new event EventHandler AutoSizeChanged
    {
      // Событие Control.AutoSizeChanged существует, но его не видно в IntelliSense
      // Никаких действий, кроме, как сделать событие видимым, не требуется.
      // См. исходный код класса Label. Там так же

      add { base.AutoSizeChanged += value; }
      remove { base.AutoSizeChanged -= value; }
    }

    void TheLabel_Resize(object sender, EventArgs args)
    {
      //AdjustSize();
    }

    #endregion

    #region Font

    protected override void OnFontChanged(EventArgs args)
    {
      base.OnFontChanged(args);

      AdjustSize();
    }

    #endregion

    #region Ограничение размеров

    public override Size GetPreferredSize(Size proposedSize)
    {
      Size sz1;
      if (Icon == MessageBoxIcon.None)
        sz1 = new Size(0, 0);
      else
      {
        sz1 = _TheIconBox.GetPreferredSize(proposedSize);
        proposedSize = new Size(Math.Max(proposedSize.Width - sz1.Width, 1), proposedSize.Height); // 26.03.2016
      }
      Size sz2 = _TheLabel.GetPreferredSize(proposedSize);
      return new Size(sz1.Width + sz2.Width, Math.Max(sz1.Height, sz2.Height));
    }

    // ReSharper disable once RedundantOverriddenMember
    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
      //if (AutoSize)
      //{
      //  Size sz = PreferredSize;
      //  if ((specified & BoundsSpecified.Width) != BoundsSpecified.None)
      //    width = sz.Width;
      //  if ((specified & BoundsSpecified.Height) != BoundsSpecified.None)
      //    height = sz.Height;
      //}

      base.SetBoundsCore(x, y, width, height, specified);
    }

    protected override Size DefaultSize
    {
      get
      {
        // Метод может вызываться до вызова конструктора
        return new Size(120, AutoSize ? PreferredSize.Height : 24);
      }
    }

    protected override Size DefaultMinimumSize
    {
      get
      {
        Size sz1;
        if (Icon == MessageBoxIcon.None)
          sz1 = new Size(0, 0);
        else
          sz1 = _TheIconBox.MinimumSize;
        Size sz2;
        if (_TheLabel == null)
          sz2 = new Size(0, 0);
        else
          sz2 = _TheLabel.MinimumSize;
        return new Size(sz1.Width + sz2.Width, Math.Max(sz1.Height, sz2.Height));
      }
    }

    protected override void OnSizeChanged(EventArgs args)
    {
      base.OnSizeChanged(args);
      //AdjustSize();
      //if (Parent != null)
      //  Parent.PerformLayout();
    }

    private bool _InsideAdjustSize;

    /// <summary>
    /// 26.03.2016
    /// Установка желаемых размеров в режиме AutoSize
    /// </summary>
    private void AdjustSize()
    {
      if (_InsideAdjustSize)
        return;
      _InsideAdjustSize = true;
      try
      {
        if (AutoSize)
        {
          Size sz = PreferredSize;
          this.Size = sz;
        }
      }
      finally
      {
        _InsideAdjustSize = false;
      }
    }

    protected override void OnHandleCreated(EventArgs args)
    {
      base.OnHandleCreated(args);
      PerformLayout(); // 13.05.2019
    }

    #endregion
  }
}
