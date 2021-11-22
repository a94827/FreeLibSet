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
    private TreeViewAdvDrawContext _context;
    public TreeViewAdvDrawContext Context
    {
      get { return _context; }
    }

    private Brush _textBrush;
    [Obsolete("Use TextColor")]
    public Brush TextBrush
    {
      get { return _textBrush; }
      set { _textBrush = value; }
    }

    private Brush _backgroundBrush;
    public Brush BackgroundBrush
    {
      get { return _backgroundBrush; }
      set { _backgroundBrush = value; }
    }

    private Font _font;
    public Font Font
    {
      get { return _font; }
      set { _font = value; }
    }

    private Color _textColor;
    public Color TextColor
    {
      get { return _textColor; }
      set { _textColor = value; }
    }

    private string _text;
    public string Text
    {
      get { return _text; }
      set { _text = value; }
    }


    private EditableControl _control;
    public EditableControl Control
    {
      get { return _control; }
    }

    public DrawEventArgs(TreeNodeAdv node, EditableControl control, TreeViewAdvDrawContext context, string text)
      : base(node)
    {
      _control = control;
      _context = context;
      _text = text;
    }
  }
}
