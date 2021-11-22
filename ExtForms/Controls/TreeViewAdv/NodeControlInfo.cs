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
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using System.Drawing;

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvInternal
{
  public struct NodeControlInfo
  {
    public static readonly NodeControlInfo Empty = new NodeControlInfo(null, Rectangle.Empty, null);

    private NodeControl _control;
    public NodeControl Control
    {
      get { return _control; }
    }

    private Rectangle _bounds;
    public Rectangle Bounds
    {
      get { return _bounds; }
    }

    private TreeNodeAdv _node;
    public TreeNodeAdv Node
    {
      get { return _node; }
    }

    public NodeControlInfo(NodeControl control, Rectangle bounds, TreeNodeAdv node)
    {
      _control = control;
      _bounds = bounds;
      _node = node;
    }
  }
}
