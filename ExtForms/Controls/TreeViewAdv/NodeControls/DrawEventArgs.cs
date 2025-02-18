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

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  public class DrawEventArgs : NodeEventArgs
  {
    public DrawEventArgs(TreeNodeAdv node, EditableControl control, TreeViewAdvDrawContext context, string text)
      : base(node)
    {
      _control = control;
      _context = context;
      _text = text;
    }

    public TreeViewAdvDrawContext Context { get { return _context; } }
    private readonly TreeViewAdvDrawContext _context;

    [Obsolete("Use TextColor")]
    public Brush TextBrush
    {
      get { return _textBrush; }
      set { _textBrush = value; }
    }
    private Brush _textBrush;

    public Brush BackgroundBrush
    {
      get { return _backgroundBrush; }
      set { _backgroundBrush = value; }
    }
    private Brush _backgroundBrush;

    public Font Font
    {
      get { return _font; }
      set { _font = value; }
    }
    private Font _font;

    public Color TextColor
    {
      get { return _textColor; }
      set { _textColor = value; }
    }
    private Color _textColor;

    public string Text
    {
      get { return _text; }
      set { _text = value; }
    }
    private string _text;

    public EditableControl Control
    {
      get { return _control; }
    }
    private readonly EditableControl _control;
  }
}
