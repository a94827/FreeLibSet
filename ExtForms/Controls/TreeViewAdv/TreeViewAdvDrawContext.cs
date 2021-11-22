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
using FreeLibSet.Controls.TreeViewAdvNodeControls;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  public struct TreeViewAdvDrawContext
  {
    private Graphics _graphics;
    public Graphics Graphics
    {
      get { return _graphics; }
      set { _graphics = value; }
    }

    private Rectangle _bounds;
    public Rectangle Bounds
    {
      get { return _bounds; }
      set { _bounds = value; }
    }

    private Font _font;
    public Font Font
    {
      get { return _font; }
      set { _font = value; }
    }

    private TreeViewAdvDrawSelectionMode _drawSelection;
    public TreeViewAdvDrawSelectionMode DrawSelection
    {
      get { return _drawSelection; }
      set { _drawSelection = value; }
    }

    private bool _drawFocus;
    public bool DrawFocus
    {
      get { return _drawFocus; }
      set { _drawFocus = value; }
    }

    private NodeControl _currentEditorOwner;
    public NodeControl CurrentEditorOwner
    {
      get { return _currentEditorOwner; }
      set { _currentEditorOwner = value; }
    }

    private bool _enabled;
    public bool Enabled
    {
      get { return _enabled; }
      set { _enabled = value; }
    }
  }
}
