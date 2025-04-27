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

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  /// <summary>
  /// Аргументы события <see cref="BaseTextControl.DrawText"/>
  /// </summary>
  public class DrawEventArgs : NodeEventArgs
  {
    /// <summary>
    /// Используется в <see cref="BaseTextControl"/>
    /// </summary>
    /// <param name="node"></param>
    /// <param name="control"></param>
    /// <param name="context"></param>
    /// <param name="text"></param>
    public DrawEventArgs(TreeNodeAdv node, EditableControl control, TreeViewAdvDrawContext context, string text)
      : base(node)
    {
      _control = control;
      _context = context;
      _text = text;
    }

    /// <summary>
    /// Контекст рисования
    /// </summary>
    public TreeViewAdvDrawContext Context { get { return _context; } }
    private readonly TreeViewAdvDrawContext _context;

    /// <summary>
    /// Кисть для закрашивания фона
    /// </summary>
    public Brush BackgroundBrush
    {
      get { return _backgroundBrush; }
      set { _backgroundBrush = value; }
    }
    private Brush _backgroundBrush;

    /// <summary>
    /// Шрифт для вывода текста
    /// </summary>
    public Font Font
    {
      get { return _font; }
      set { _font = value; }
    }
    private Font _font;

    /// <summary>
    /// Цвет текста
    /// </summary>
    public Color TextColor
    {
      get { return _textColor; }
      set { _textColor = value; }
    }
    private Color _textColor;

    /// <summary>
    /// Выводимый текст
    /// </summary>
    public string Text
    {
      get { return _text; }
      set { _text = value; }
    }
    private string _text;

    /// <summary>
    /// Элемент, рисование которого выполняется
    /// </summary>
    public EditableControl Control
    {
      get { return _control; }
    }
    private readonly EditableControl _control;
  }
}
