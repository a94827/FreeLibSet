using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

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

    [Description("Текущий цвет. Значение по умолчанию: Color.White")]
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
          throw new ArgumentException("Системные цвета не поддерживаются");
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

    [Description("Цвета в выпадающем списке")]
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

    [Description("Имена цветов в выпадающем списке")]
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

    [Description("Цвет изображения для значения Color.Empty. "+
    "По умолчанию - Color.Empty, при этом внутренность квадратика не заполняется")]
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

    [Description("Вызывается при изменении значения свойства Color")]
    [Category("Property Changed")]
    public event EventHandler ColorChanged;

    #endregion

    #region Прорисовка

    private void InitPopupItems()
    {
      Color clr = _Color;
      // Инициализация выпадающего списка
      base.Items.Clear();
      base.Items.Add("Выбрать");
      if (_PopupColors != null)
      { 
        object[] Items=new object[_PopupColors.Length];
        for (int i = 0; i < _PopupColors.Length; i++)
          Items[i] = _PopupColors[i];
        base.Items.AddRange(Items);
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
      string ItemText;
      if (args.Index < 1)
      {
        clr = Color;
        if (args.Index == 0)
          ItemText = "Выбрать ...";
        else
          ItemText = MyGetColorText(Color);
      }
      else
      {
        int PopIdx = args.Index - 1;
        if (PopupColors == null || PopIdx >= PopupColors.Length)
        {
          clr = Color.Red;
          ItemText = "???";
        }
        else
        {
          clr = PopupColors[PopIdx];
          ItemText=MyGetColorText(clr);
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

      args.Graphics.DrawString(ItemText, base.Font, SystemBrushes.ControlText, rc2);

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
            return "Вне диапазона";
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
        int PopIdx = base.SelectedIndex - 1;
        if (PopIdx < _PopupColors.Length)
        {
          Color = _PopupColors[PopIdx];
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
