// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
//
// Original TreeViewAdv component from Aga.Controls.dll
// Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
// http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
// http://sourceforge.net/projects/treeviewadv/

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using System.Drawing.Imaging;
using FreeLibSet.Drawing;
using FreeLibSet.Controls.TreeViewAdvInternal;
using FreeLibSet.Core;

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Столбец иерахического просмотра в режиме <see cref="TreeViewAdv.UseColumns"/>=true.
  /// В столбец может входить один (обычно) или несколько элементов <see cref="FreeLibSet.Controls.TreeViewAdvNodeControls.NodeControl"/>.
  /// Привязка элемента к столбцу выполняется с помощью свойства <see cref="FreeLibSet.Controls.TreeViewAdvNodeControls.NodeControl.ParentColumn"/>.
  /// Из объекта <see cref="TreeColumn"/> нельзя получить список элементов, которые в него входят.
  /// </summary>
  [TypeConverter(typeof(TreeColumn.TreeColumnConverter)), DesignTimeVisible(false), ToolboxItem(false)]
  public class TreeColumn : Component
  {
    #region Константы

    private class TreeColumnConverter : ComponentConverter
    {
      public TreeColumnConverter()
        : base(typeof(TreeColumn))
      {
      }

      public override bool GetPropertiesSupported(ITypeDescriptorContext context)
      {
        return false;
      }
    }

    private const int HeaderLeftMargin = 5;
    private const int HeaderRightMargin = 5;
    private const int SortOrderMarkMargin = 8;

    private TextFormatFlags _headerFlags;
    private readonly TextFormatFlags _baseHeaderFlags = TextFormatFlags.NoPadding |
                                               TextFormatFlags.EndEllipsis |
                                               TextFormatFlags.VerticalCenter |
                                               TextFormatFlags.PreserveGraphicsTranslateTransform;

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает столбец без текста заголовка шириной 50 пикселей
    /// </summary>
    public TreeColumn()
    : this(string.Empty, 50)
    {
    }


    /// <summary>
    /// Создает столбец с заданным текстом заголовка и шириной
    /// </summary>
    /// <param name="header">Текст заголовка</param>
    /// <param name="width">Ширина в пикселях</param>
    public TreeColumn(string header, int width)
    {
      _header = header;
      _width = width;
      _headerFlags = _baseHeaderFlags | TextFormatFlags.Left;
    }

    #endregion

    #region Properties

    internal TreeColumnCollection Owner
    {
      get { return _owner; }
      set { _owner = value; }
    }
    private TreeColumnCollection _owner;

    /// <summary>
    /// Возвращает индекс столбца в просмотре.
    /// Возвращает (-1), если столбец еще не добавлен в просмотр
    /// </summary>
    [Browsable(false)]
    public int Index
    {
      get
      {
        if (Owner != null)
          return Owner.IndexOf(this);
        else
          return -1;
      }
    }

    /// <summary>
    /// Текст заголовка столбца
    /// </summary>
    [Localizable(true)]
    public string Header
    {
      get { return _header; }
      set
      {
        _header = value;
        OnHeaderChanged();
      }
    }
    private string _header;

    /// <summary>
    /// Всплывающая подсказка, которая появляется при наведении курсора мыши на заголовок столбца
    /// </summary>
    [Localizable(true)]
    public string TooltipText
    {
      get { return _tooltipText; }
      set { _tooltipText = value; }
    }
    private string _tooltipText;

    /// <summary>
    /// Ширина столбца в пикселях
    /// </summary>
    [DefaultValue(50), Localizable(true)]
    public int Width
    {
      get
      {
        return _width;
      }
      set
      {
        if (_width != value)
        {
          _width = Math.Max(MinColumnWidth, value);
          if (_maxColumnWidth > 0)
          {
            _width = Math.Min(_width, MaxColumnWidth);
          }
          OnWidthChanged();
        }
      }
    }
    private int _width;

    /// <summary>
    /// Минимальная ширина столбца в пикселях
    /// </summary>
    [DefaultValue(0)]
    public int MinColumnWidth
    {
      get { return _minColumnWidth; }
      set
      {
        if (value < 0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        _minColumnWidth = value;
        Width = Math.Max(value, Width);
      }
    }
    private int _minColumnWidth;

    /// <summary>
    /// Максимальная ширина столбца в пикселях.
    /// Нулевое значение означает отсутствие ограничений.
    /// </summary>
    [DefaultValue(0)]
    public int MaxColumnWidth
    {
      get { return _maxColumnWidth; }
      set
      {
        if (value < 0)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, null);

        _maxColumnWidth = value;
        if (value > 0)
          Width = Math.Min(value, _width);
      }
    }
    private int _maxColumnWidth;

    /// <summary>
    /// Видимость столбца
    /// </summary>
    [DefaultValue(true)]
    public bool IsVisible
    {
      get { return _visible; }
      set
      {
        _visible = value;
        OnIsVisibleChanged();
      }
    }
    private bool _visible = true;

    /// <summary>
    /// Горизонтальное выравнивание для заголовка столбца.
    /// Не влияет на выравнивание содержимого столбца, которое задается в <see cref="FreeLibSet.Controls.TreeViewAdvNodeControls.BaseTextControl.TextAlign"/>.
    /// </summary>
    [DefaultValue(HorizontalAlignment.Left)]
    public HorizontalAlignment TextAlign
    {
      get { return _textAlign; }
      set
      {
        if (value != _textAlign)
        {
          _textAlign = value;
          _headerFlags = _baseHeaderFlags | TextHelper.TranslateAligmentToFlag(value);
          OnHeaderChanged();
        }
      }
    }
    private HorizontalAlignment _textAlign = HorizontalAlignment.Left;

    /// <summary>
    /// Если true, то пользователь может сортировать столбец, нажимая на заголовок столбца.
    /// </summary>
    [DefaultValue(false)]
    public bool Sortable
    {
      get { return _sortable; }
      set { _sortable = value; }
    }
    private bool _sortable = false;

    /// <summary>
    /// Текущий порядок сортировки: Нет, По возрастанию, По убыванию.
    /// </summary>
    public SortOrder SortOrder
    {
      get { return _sort_order; }
      set
      {
        if (value == _sort_order)
          return;
        _sort_order = value;
        OnSortOrderChanged();
      }
    }
    private SortOrder _sort_order = SortOrder.None;

    /// <summary>
    /// Размер значка сортировки в пикселях
    /// </summary>
    public Size SortMarkSize
    {
      get
      {
        if (Application.RenderWithVisualStyles)
          return new Size(9, 5);
        else
          return new Size(7, 4);
      }
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Возвращает текст заголовка столбца, если он задан
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (string.IsNullOrEmpty(Header))
        return GetType().Name;
      else
        return Header;
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
    }

    #endregion

    #region Draw

    private static VisualStyleRenderer _normalRenderer;
    private static VisualStyleRenderer _pressedRenderer;
    private static VisualStyleRenderer _hotRenderer;

    private static void CreateRenderers()
    {
      if (Application.RenderWithVisualStyles && _normalRenderer == null)
      {
        _normalRenderer = new VisualStyleRenderer(VisualStyleElement.Header.Item.Normal);
        _pressedRenderer = new VisualStyleRenderer(VisualStyleElement.Header.Item.Pressed);
        _hotRenderer = new VisualStyleRenderer(VisualStyleElement.Header.Item.Hot);
      }
    }

    internal Bitmap CreateGhostImage(Rectangle bounds, Font font)
    {
      Bitmap b = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
      Graphics gr = Graphics.FromImage(b);
      gr.FillRectangle(SystemBrushes.ControlDark, bounds);
      DrawContent(gr, bounds, font);
      ImagingTools.SetAlphaChannelValue(b, 150);
      return b;
    }

    /// <summary>
    /// Рисование заголовка столбца
    /// </summary>
    /// <param name="gr"></param>
    /// <param name="bounds"></param>
    /// <param name="font"></param>
    /// <param name="pressed"></param>
    /// <param name="hot"></param>
    internal void Draw(Graphics gr, Rectangle bounds, Font font, bool pressed, bool hot)
    {
      DrawBackground(gr, bounds, pressed, hot);
      DrawContent(gr, bounds, font);
    }

    private void DrawContent(Graphics gr, Rectangle bounds, Font font)
    {
      Rectangle innerBounds = new Rectangle(bounds.X + HeaderLeftMargin, bounds.Y,
                             bounds.Width - HeaderLeftMargin - HeaderRightMargin,
                             bounds.Height);

      if (SortOrder != SortOrder.None)
        innerBounds.Width -= (SortMarkSize.Width + SortOrderMarkMargin);

      Size maxTextSize = TextRenderer.MeasureText(gr, Header, font, innerBounds.Size, TextFormatFlags.NoPadding);
      Size textSize = TextRenderer.MeasureText(gr, Header, font, innerBounds.Size, _baseHeaderFlags);

      if (SortOrder != SortOrder.None)
      {
        int tw = Math.Min(textSize.Width, innerBounds.Size.Width);

        int x = 0;
        if (TextAlign == HorizontalAlignment.Left)
          x = innerBounds.X + tw + SortOrderMarkMargin;
        else if (TextAlign == HorizontalAlignment.Right)
          x = innerBounds.Right + SortOrderMarkMargin;
        else
          x = innerBounds.X + tw + (innerBounds.Width - tw) / 2 + SortOrderMarkMargin;
        DrawSortMark(gr, bounds, x);
      }

      if (textSize.Width < maxTextSize.Width)
        TextRenderer.DrawText(gr, Header, font, innerBounds, SystemColors.ControlText, _baseHeaderFlags | TextFormatFlags.Left);
      else
        TextRenderer.DrawText(gr, Header, font, innerBounds, SystemColors.ControlText, _headerFlags);
    }

    private void DrawSortMark(Graphics gr, Rectangle bounds, int x)
    {
      int y = bounds.Y + bounds.Height / 2 - 2;
      x = Math.Max(x, bounds.X + SortOrderMarkMargin);

      int w2 = SortMarkSize.Width / 2;
      if (SortOrder == SortOrder.Ascending)
      {
        Point[] points = new Point[] { new Point(x, y), new Point(x + SortMarkSize.Width, y), new Point(x + w2, y + SortMarkSize.Height) };
        gr.FillPolygon(SystemBrushes.ControlDark, points);
      }
      else if (SortOrder == SortOrder.Descending)
      {
        Point[] points = new Point[] { new Point(x - 1, y + SortMarkSize.Height), new Point(x + SortMarkSize.Width, y + SortMarkSize.Height), new Point(x + w2, y - 1) };
        gr.FillPolygon(SystemBrushes.ControlDark, points);
      }
    }

    internal static void DrawDropMark(Graphics gr, Rectangle rect)
    {
      gr.FillRectangle(SystemBrushes.HotTrack, rect.X - 1, rect.Y, 2, rect.Height);
    }

    internal static void DrawBackground(Graphics gr, Rectangle bounds, bool pressed, bool hot)
    {
      if (Application.RenderWithVisualStyles)
      {
        CreateRenderers();
        if (pressed)
          _pressedRenderer.DrawBackground(gr, bounds);
        else if (hot)
          _hotRenderer.DrawBackground(gr, bounds);
        else
          _normalRenderer.DrawBackground(gr, bounds);
      }
      else
      {
        gr.FillRectangle(SystemBrushes.Control, bounds);
        Pen p1 = SystemPens.ControlLightLight;
        Pen p2 = SystemPens.ControlDark;
        Pen p3 = SystemPens.ControlDarkDark;
        if (pressed)
          gr.DrawRectangle(p2, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        else
        {
          gr.DrawLine(p1, bounds.X, bounds.Y, bounds.Right, bounds.Y);
          gr.DrawLine(p3, bounds.X, bounds.Bottom, bounds.Right, bounds.Bottom);
          gr.DrawLine(p3, bounds.Right - 1, bounds.Y, bounds.Right - 1, bounds.Bottom - 1);
          gr.DrawLine(p1, bounds.Left, bounds.Y + 1, bounds.Left, bounds.Bottom - 2);
          gr.DrawLine(p2, bounds.Right - 2, bounds.Y + 1, bounds.Right - 2, bounds.Bottom - 2);
          gr.DrawLine(p2, bounds.X, bounds.Bottom - 1, bounds.Right - 2, bounds.Bottom - 1);
        }
      }
    }

    #endregion

    #region Events

    /// <summary>
    /// Вызывается при изменении текста заголовка
    /// </summary>
    public event EventHandler HeaderChanged;
    private void OnHeaderChanged()
    {
      if (HeaderChanged != null)
        HeaderChanged(this, EventArgs.Empty);
    }

    /// <summary>
    /// Вызывается при изменении свойства <see cref="SortOrder"/>
    /// </summary>
    public event EventHandler SortOrderChanged;
    private void OnSortOrderChanged()
    {
      if (SortOrderChanged != null)
        SortOrderChanged(this, EventArgs.Empty);
    }

    /// <summary>
    /// Вызывается при изменении видимости столбца
    /// </summary>
    public event EventHandler IsVisibleChanged;
    private void OnIsVisibleChanged()
    {
      if (IsVisibleChanged != null)
        IsVisibleChanged(this, EventArgs.Empty);
    }

    /// <summary>
    /// Вызывается при изменении ширины столбца
    /// </summary>
    public event EventHandler WidthChanged;
    private void OnWidthChanged()
    {
      if (WidthChanged != null)
        WidthChanged(this, EventArgs.Empty);
    }

    #endregion
  }
}
