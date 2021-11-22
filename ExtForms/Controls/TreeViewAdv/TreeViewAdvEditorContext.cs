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
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.Controls.TreeViewAdvNodeControls;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  public struct TreeViewAdvEditorContext
  {
    private TreeNodeAdv _currentNode;
    public TreeNodeAdv CurrentNode
    {
      get { return _currentNode; }
      set { _currentNode = value; }
    }

    private Control _editor;
    public Control Editor
    {
      get { return _editor; }
      set { _editor = value; }
    }

    private NodeControl _owner;
    public NodeControl Owner
    {
      get { return _owner; }
      set { _owner = value; }
    }

    private Rectangle _bounds;
    public Rectangle Bounds
    {
      get { return _bounds; }
      set { _bounds = value; }
    }

    private TreeViewAdvDrawContext _drawContext;
    public TreeViewAdvDrawContext DrawContext
    {
      get { return _drawContext; }
      set { _drawContext = value; }
    }
  }
}
