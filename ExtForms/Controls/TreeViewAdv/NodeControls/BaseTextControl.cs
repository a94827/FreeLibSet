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
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using FreeLibSet.Controls.TreeViewAdvInternal;

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  /// <summary>
  /// Базовый класс для <see cref="NodeTextBox"/>, <see cref="NodeComboBox"/>, <see cref="NodeIntEditBox"/>, <see cref="NodeSingleEditBox"/>, <see cref="NodeDoubleEditBox"/>,<see cref="NodeDecimalEditBox"/>
  /// </summary>
  public abstract class BaseTextControl : EditableControl
  {
    private readonly TextFormatFlags _baseFormatFlags;
    private TextFormatFlags _formatFlags;
    private Pen _focusPen;
    private StringFormat _format;

    #region Конструктор и Dispose

    /// <summary>
    /// Защищенный конструктор
    /// </summary>
    protected BaseTextControl()
    {
      IncrementalSearchEnabled = true;
      _focusPen = new Pen(Color.Black); 
      _focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

      _format = new StringFormat(StringFormatFlags.LineLimit | StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox | StringFormatFlags.MeasureTrailingSpaces);
      _baseFormatFlags = TextFormatFlags.PreserveGraphicsClipping |
               TextFormatFlags.PreserveGraphicsTranslateTransform;

      _format.FormatFlags &= ~StringFormatFlags.LineLimit;
      _baseFormatFlags |= TextFormatFlags.EndEllipsis; // Агеев А.В., 16.01.2019

      SetFormatFlags();
      LeftMargin = 3;
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (disposing)
      {
        _focusPen.Dispose();
        _format.Dispose();
      }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Шрифт для прорисовки текста ячеек.
    /// По умолчанию используется шрифт <see cref="Control.DefaultFont"/>, зависящий от операционной системы.
    /// </summary>
    public Font Font
    {
      get
      {
        if (_font == null)
          return Control.DefaultFont;
        else
          return _font;
      }
      set
      {
        if (value == Control.DefaultFont)
          _font = null;
        else
          _font = value;
      }
    }
    private Font _font = null;

    /// <summary>
    /// Возвращает true, если задан нестандартный шрифт
    /// </summary>
    /// <returns></returns>
    protected bool ShouldSerializeFont()
    {
      return (_font != null);
    }

    [DefaultValue(3)] // Ageyev A.V.
    public new int LeftMargin
    {
      get { return base.LeftMargin; }
      set { base.LeftMargin = value; }
    }

    private HorizontalAlignment _textAlign = HorizontalAlignment.Left;
    /// <summary>
    /// Горизонтальное выравнивание для содержимого ячейки.
    /// По умолчанию используется выравнивание по левому краю.
    /// </summary>
    [DefaultValue(HorizontalAlignment.Left)]
    public HorizontalAlignment TextAlign
    {
      get { return _textAlign; }
      set
      {
        _textAlign = value;
        SetFormatFlags();
      }
    }

    //private StringTrimming _trimming = StringTrimming.None;
    //[DefaultValue(StringTrimming.None)]
    private StringTrimming _trimming = StringTrimming.EllipsisCharacter; // Агеев А.В., 16.01.2019
    /// <summary>
    /// Способ обрезки текста, если он не помещается в ячейку.
    /// По умолчанию используется StringTrimming.EllipsisCharacter, в отличие от оригинального компонента от Andrey Gliznetsov
    /// </summary>
    [DefaultValue(StringTrimming.EllipsisCharacter)]
    public StringTrimming Trimming
    {
      get { return _trimming; }
      set
      {
        _trimming = value;
        SetFormatFlags();
      }
    }

    [DefaultValue(true)]
    public bool DisplayHiddenContentInToolTip
    {
      get { return _displayHiddenContentInToolTip; }
      set { _displayHiddenContentInToolTip = value; }
    }
    private bool _displayHiddenContentInToolTip = true;

    [DefaultValue(false)]
    public bool UseCompatibleTextRendering
    {
      get { return _useCompatibleTextRendering; }
      set { _useCompatibleTextRendering = value; }
    }
    private bool _useCompatibleTextRendering = false;

    [DefaultValue(false)]
    public bool TrimMultiLine
    {
      get { return _TrimMultiLine; }
      set { _TrimMultiLine = value; }
    }
    private bool _TrimMultiLine;

    #endregion

    private void SetFormatFlags()
    {
      _format.Alignment = TextHelper.TranslateAligment(TextAlign);
      _format.Trimming = Trimming;

      _formatFlags = _baseFormatFlags | TextHelper.TranslateAligmentToFlag(TextAlign)
        | TextHelper.TranslateTrimmingToFlag(Trimming);
    }

    public override Size MeasureSize(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      return GetLabelSize(node, context);
    }

    protected Size GetLabelSize(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      return GetLabelSize(node, context, GetLabel(node));
    }

    protected Size GetLabelSize(TreeNodeAdv node, TreeViewAdvDrawContext context, string label)
    {
      CheckThread();
      Font font = GetDrawingFont(node, context, label);
      Size s = Size.Empty;
      if (UseCompatibleTextRendering)
        s = TextRenderer.MeasureText(label, font);
      else
      {
        SizeF sf = context.Graphics.MeasureString(label, font);
        s = new Size((int)Math.Ceiling(sf.Width), (int)Math.Ceiling(sf.Height));
      }

      if (!s.IsEmpty)
        return s;
      else
        return new Size(10, Font.Height);
    }

    protected Font GetDrawingFont(TreeNodeAdv node, TreeViewAdvDrawContext context, string label)
    {
      Font font = context.Font;
      if (DrawTextMustBeFired(node))
      {
        DrawEventArgs args = new DrawEventArgs(node, this, context, label);
        args.Font = context.Font;
        OnDrawText(args);
        font = args.Font;
      }
      return font;
    }

    /// <summary>
    /// Устанавливает свойство System.Windows.Forms.Control.Font
    /// </summary>
    /// <param name="control">Инициализируемый управляющий элемент Windows Forms</param>
    /// <param name="node">Узел дерева</param>
    protected void SetEditControlProperties(Control control, TreeNodeAdv node)
    {
      string label = GetLabel(node);
      TreeViewAdvDrawContext context = new TreeViewAdvDrawContext();
      context.Font = control.Font;
      control.Font = GetDrawingFont(node, context, label);
    }

    public override void Draw(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      if (context.CurrentEditorOwner == this && node == Parent.CurrentNode)
        return;

      string label = GetLabel(node);
      Rectangle bounds = GetBounds(node, context);
      Rectangle focusRect = new Rectangle(bounds.X, context.Bounds.Y,
        bounds.Width, context.Bounds.Height);

      Brush backgroundBrush;
      Color textColor;
      Font font;
      CreateBrushes(node, context, label, out backgroundBrush, out textColor, out font, ref label);

      if (backgroundBrush != null)
        context.Graphics.FillRectangle(backgroundBrush, focusRect);
      if (context.DrawFocus)
      {
        focusRect.Width--;
        focusRect.Height--;
        if (context.DrawSelection == TreeViewAdvDrawSelectionMode.None)
          _focusPen.Color = SystemColors.ControlText;
        else
          _focusPen.Color = SystemColors.InactiveCaption;
        context.Graphics.DrawRectangle(_focusPen, focusRect);
      }

      if (UseCompatibleTextRendering)
        TextRenderer.DrawText(context.Graphics, label, font, bounds, textColor, _formatFlags);
      else
        context.Graphics.DrawString(label, font, GetFrush(textColor), bounds, _format);
    }

    private static Dictionary<Color, Brush> _brushes = new Dictionary<Color, Brush>();
    private static Brush GetFrush(Color color)
    {
      Brush br;
      if (_brushes.ContainsKey(color))
        br = _brushes[color];
      else
      {
        br = new SolidBrush(color);
        _brushes.Add(color, br);
      }
      return br;
    }

    private void CreateBrushes(TreeNodeAdv node, TreeViewAdvDrawContext context, string text, out Brush backgroundBrush, out Color textColor, out Font font, ref string label)
    {
      //textColor = SystemColors.ControlText;
      textColor = Parent.ForeColor; // Ageyev A.V., 24.05.2015
      backgroundBrush = null;
      font = context.Font;
      if (context.DrawSelection == TreeViewAdvDrawSelectionMode.Active)
      {
        textColor = SystemColors.HighlightText;
        backgroundBrush = SystemBrushes.Highlight;
      }
      else if (context.DrawSelection == TreeViewAdvDrawSelectionMode.Inactive)
      {
        textColor = SystemColors.ControlText;
        backgroundBrush = SystemBrushes.InactiveBorder;
      }
      else if (context.DrawSelection == TreeViewAdvDrawSelectionMode.FullRowSelect)
        textColor = SystemColors.HighlightText;

      if (!context.Enabled)
        textColor = SystemColors.GrayText;

      if (DrawTextMustBeFired(node))
      {
        DrawEventArgs args = new DrawEventArgs(node, this, context, text);
        args.Text = label;
        args.TextColor = textColor;
        args.BackgroundBrush = backgroundBrush;
        args.Font = font;

        OnDrawText(args);

        textColor = args.TextColor;
        backgroundBrush = args.BackgroundBrush;
        font = args.Font;
        label = args.Text;
      }
    }

    public string GetLabel(TreeNodeAdv node)
    {
      if (node != null && node.Tag != null)
      {
        object obj = GetValue(node);
        if (obj != null)
          return FormatLabel(obj);
      }
      return string.Empty;
    }

    protected virtual string FormatLabel(object obj)
    {
      string res = obj.ToString();
      if (TrimMultiLine && res != null)
      {
        string[] parts = res.Split('\n');
        if (parts.Length > 1)
          return parts[0] + "...";
      }
      return res;
    }

    public void SetLabel(TreeNodeAdv node, string value)
    {
      SetValue(node, value);
    }

    /// <summary>
    /// Fires when control is going to draw a text. Can be used to change text or back color
    /// </summary>
    public event EventHandler<DrawEventArgs> DrawText;
    protected virtual void OnDrawText(DrawEventArgs args)
    {
      TreeViewAdv tree = args.Node.Tree;
      if (tree != null)
        tree.FireDrawControl(args);
      if (DrawText != null)
        DrawText(this, args);
    }

    protected virtual bool DrawTextMustBeFired(TreeNodeAdv node)
    {
      return DrawText != null || (node.Tree != null && node.Tree.DrawControlMustBeFired());
    }
  }
}
