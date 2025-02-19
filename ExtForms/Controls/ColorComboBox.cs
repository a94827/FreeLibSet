﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Комбоблок для выбора цвета
  /// </summary>
  [Description("Комбоблок для выбора цвета")]
  [ToolboxBitmap(typeof(ColorComboBox), "ColorComboBox.bmp")]
  [ToolboxItem(true)]
  public class ColorComboBox : ComboBox
  {
    #region Конструктор

    public ColorComboBox()
    {
      base.DrawMode = DrawMode.OwnerDrawFixed;
      DrawItem += new DrawItemEventHandler(TheCB_DrawItem);
      //      base.ItemHeight = 20;
      base.DropDownStyle = ComboBoxStyle.DropDownList;
      InitPopupItems();
      base.SelectedIndexChanged += new EventHandler(ColorComboBox_SelectedIndexChanged);
      _EmptyColor = Color.Empty;
      _Color = Color.White;
    }

    #endregion

    #region Свойства

    [Description("Current color. Default value is Color.White")]
    [Category("Appearance")]
    [DefaultValue(typeof(Color), "White")]
    public Color Color
    {
      get { return _Color; }
      set
      {
        if (value == _Color)
          return;
        if (value.IsSystemColor)
          throw new ArgumentException(Res.ColorComboBox_Arg_SystemColorNotSupported, "value");
        _Color = value;
        Invalidate();
        if (ColorChanged != null)
          ColorChanged(this, EventArgs.Empty);
        if (PopupColors != null)
        {
          int p = Array.IndexOf<Color>(PopupColors, _Color);
          if (p >= 0)
          {
            p = p + 1;
            if (p < base.Items.Count)
              base.SelectedIndex = p;
          }
        }
      }
    }
    private Color _Color;

    [Description("Colors presented in the drop-down list")]
    [Category("Behavior")]
    [DefaultValue(null)]
    public Color[] PopupColors
    {
      get { return _PopupColors; }
      set
      {
        if (value == _PopupColors)
          return;
        _PopupColors = value;
        InitPopupItems();
      }
    }
    private Color[] _PopupColors;

    [Description("Color names in the drop-down list")]
    [Category("Behavior")]
    [DefaultValue(null)]
    public string[] PopupColorNames
    {
      get { return _PopupColorNames; }
      set
      {
        if (value == _PopupColorNames)
          return;
        _PopupColorNames = value;
      }
    }
    private string[] _PopupColorNames;

    [Description("Image color for the value of Color.Empty. " +
    "Default value is Color.Empty, the rectangle shape has no filling")]
    [Category("Appearance")]
    [DefaultValue(typeof(Color), "Empty")]
    public Color EmptyColor
    {
      get { return _EmptyColor; }
      set { _EmptyColor = value; }
    }
    private Color _EmptyColor;

    #endregion

    #region События

    [Description("Event called when the Color changed")]
    [Category("Property Changed")]
    public event EventHandler ColorChanged;

    #endregion

    #region Прорисовка

    private void InitPopupItems()
    {
      Color clr = _Color;
      // Инициализация выпадающего списка
      base.Items.Clear();
      base.Items.Add(Res.ColorComboBox_Msg_SelectWithDialogListItem);
      if (_PopupColors != null)
      {
        object[] items = new object[_PopupColors.Length];
        for (int i = 0; i < _PopupColors.Length; i++)
          items[i] = _PopupColors[i];
        base.Items.AddRange(items);
      }
      Color = clr;
    }

    void TheCB_DrawItem(object sender, DrawItemEventArgs args)
    {
      args.DrawBackground();

      // Образец цвета
      Rectangle rc1 = args.Bounds;
      rc1.Width = 30;
      rc1.Inflate(-1, -1);
      args.Graphics.DrawRectangle(Pens.Black, rc1);
      rc1.Inflate(-1, -1);
      args.Graphics.DrawRectangle(Pens.Black, rc1);
      rc1.Inflate(-1, -1);
      args.Graphics.DrawRectangle(Pens.White, rc1);
      rc1.Inflate(-1, -1);

      Color clr;
      string itemText;
      if (args.Index < 1)
      {
        clr = Color;
        if (args.Index == 0)
          itemText = Res.ColorComboBox_Msg_SelectWithDialogListItem;
        else
          itemText = MyGetColorText(Color);
      }
      else
      {
        int popIdx = args.Index - 1;
        if (PopupColors == null || popIdx >= PopupColors.Length)
        {
          clr = Color.Red;
          itemText = "???";
        }
        else
        {
          clr = PopupColors[popIdx];
          itemText = MyGetColorText(clr);
        }
      }

      if (clr.IsEmpty)
        clr = EmptyColor;
      if (!clr.IsEmpty)
      {
        Brush Brush = new SolidBrush(clr);
        try
        {
          args.Graphics.FillRectangle(Brush, rc1);
        }
        finally
        {
          Brush.Dispose();
        }
      }

      Rectangle rc2 = args.Bounds;
      rc2.X += 32;
      rc2.Width -= 32;

      args.Graphics.DrawString(itemText, base.Font, SystemBrushes.ControlText, rc2);

      args.DrawFocusRectangle();
    }

    /// <summary>
    /// Текст для списка
    /// </summary>
    /// <param name="clr"></param>
    /// <returns></returns>
    private string MyGetColorText(Color clr)
    {
      if (PopupColorNames != null && PopupColors != null)
      {
        int p = Array.IndexOf<Color>(PopupColors, clr);
        if (p >= 0)
        {
          if (p < PopupColorNames.Length)
            return PopupColorNames[p];
          else
            return Res.ColorComboBox_Msg_NotInPopupColorsListItem;
        }
      }
      return clr.Name;
    }

    #endregion

    #region Выбор из списка

    private bool _InsideSelectedIndexChanged = false;

    void ColorComboBox_SelectedIndexChanged(object sender, EventArgs args)
    {
      if (_InsideSelectedIndexChanged)
        return;
      _InsideSelectedIndexChanged = true;
      try
      {
        DoSelectedIndex();
      }
      finally
      {
        _InsideSelectedIndexChanged = false;
      }
    }

    private void DoSelectedIndex()
    {
      if (base.SelectedIndex < 0)
        return; // вложенный вызов

      if (base.SelectedIndex == 0)
      {
        // Вызов диалога
        SelectFromDialog();
        base.SelectedIndex = -1;
        return;
      }
      if (_PopupColors != null)
      {
        int popIdx = base.SelectedIndex - 1;
        if (popIdx < _PopupColors.Length)
        {
          Color = _PopupColors[popIdx];
          return;
        }
      }
      base.SelectedIndex = -1;
    }

    private void SelectFromDialog()
    {
      ColorDialog dlg = new ColorDialog();
      dlg.AllowFullOpen = true;
      dlg.Color = Color;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      Color = dlg.Color;
    }

    #endregion

    #region Заглушки для свойств, чтобы их не было видно в дизайнере

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new AutoCompleteStringCollection AutoCompleteCustomSource
    {
      get { return base.AutoCompleteCustomSource; }
      set { }
    }

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new AutoCompleteMode AutoCompleteMode
    {
      get { return base.AutoCompleteMode; }
      set { }
    }

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new AutoCompleteSource AutoCompleteSource
    {
      get { return base.AutoCompleteSource; }
      set { }
    }

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new object DataSource
    {
      get { return null; }
      set { }
    }

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new string DisplayMember
    {
      get { return null; }
      set { }
    }

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new DrawMode DrawMode
    {
      get { return base.DrawMode; }
      set { }
    }

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new ComboBoxStyle DropDownStyle
    {
      get { return base.DropDownStyle; }
      set { }
    }

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new string FormatString
    {
      get { return null; }
      set { }
    }

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool FormattingEnabled
    {
      get { return false; }
      set { }
    }

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new int ItemHeight
    {
      get { return base.ItemHeight; }
      set { }
    }

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new ObjectCollection Items
    {
      get { return base.Items; }
    }

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new int MaxLength
    {
      get { return 0; }
    }

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool Sorted
    {
      get { return false; }
    }

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string Text
    {
      get { return base.Text; }
      set { }
    }

    /// <summary>
    /// Не используется
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new string ValueMember
    {
      get { return null; }
      set { }
    }

    #endregion

    #region Статические свойства

    public static readonly Color[] DefaultPopupColors = new Color[]{
            Color.Black, Color.DarkGray, Color.Maroon, Color.Brown,
            Color.Green, Color.DarkCyan, Color.Navy, Color.DarkMagenta,
            Color.White, Color.LightGray, Color.Red, Color.Yellow,
            Color.LightGreen, Color.LightCyan,Color.Blue, Color.Magenta}; // ???

    #endregion
  }
}
