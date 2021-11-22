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
  public class TreeViewAdvEventArgs : EventArgs
  {
    private TreeNodeAdv _node;

    public TreeNodeAdv Node
    {
      get { return _node; }
    }

    public TreeViewAdvEventArgs(TreeNodeAdv node)
    {
      _node = node;
    }
  }

  public class TreeViewAdvCancelEventArgs : TreeViewAdvEventArgs
  {
    private bool _cancel;

    public bool Cancel
    {
      get { return _cancel; }
      set { _cancel = value; }
    }

    public TreeViewAdvCancelEventArgs(TreeNodeAdv node)
      : base(node)
    {
    }

  }

  public class TreeViewAdvRowDrawEventArgs : PaintEventArgs
  {
    TreeNodeAdv _node;
    TreeViewAdvDrawContext _context;
    int _row;
    Rectangle _rowRect;

    public TreeViewAdvRowDrawEventArgs(Graphics graphics, Rectangle clipRectangle, TreeNodeAdv node, TreeViewAdvDrawContext context, int row, Rectangle rowRect)
      : base(graphics, clipRectangle)
    {
      _node = node;
      _context = context;
      _row = row;
      _rowRect = rowRect;
    }

    public TreeNodeAdv Node
    {
      get { return _node; }
    }

    public TreeViewAdvDrawContext Context
    {
      get { return _context; }
    }

    public int Row
    {
      get { return _row; }
    }

    public Rectangle RowRect
    {
      get { return _rowRect; }
    }
  }
  public class TreeNodeAdvMouseEventArgs : MouseEventArgs
  {
    private TreeNodeAdv _node;
    public TreeNodeAdv Node
    {
      get { return _node; }
      internal set { _node = value; }
    }

    private NodeControl _control;
    public NodeControl Control
    {
      get { return _control; }
      internal set { _control = value; }
    }

    private Point _viewLocation;
    public Point ViewLocation
    {
      get { return _viewLocation; }
      internal set { _viewLocation = value; }
    }

    private Keys _modifierKeys;
    public Keys ModifierKeys
    {
      get { return _modifierKeys; }
      internal set { _modifierKeys = value; }
    }

    private bool _handled;
    public bool Handled
    {
      get { return _handled; }
      set { _handled = value; }
    }

    private Rectangle _controlBounds;
    public Rectangle ControlBounds
    {
      get { return _controlBounds; }
      internal set { _controlBounds = value; }
    }

    public TreeNodeAdvMouseEventArgs(MouseEventArgs args)
      : base(args.Button, args.Clicks, args.X, args.Y, args.Delta)
    {
    }
  }

  public class TreeColumnEventArgs : EventArgs
  {
    private TreeColumn _column;
    public TreeColumn Column
    {
      get { return _column; }
    }

    public TreeColumnEventArgs(TreeColumn column)
    {
      _column = column;
    }
  }

  public class TreeViewAdvDropNodeValidatingEventArgs : EventArgs
  {
    Point _point;
    TreeNodeAdv _node;

    public TreeViewAdvDropNodeValidatingEventArgs(Point point, TreeNodeAdv node)
    {
      _point = point;
      _node = node;
    }

    public Point Point
    {
      get { return _point; }
    }

    public TreeNodeAdv Node
    {
      get { return _node; }
      set { _node = value; }
    }
  }

  public interface ITreeViewAdvToolTipProvider
  {
    string GetToolTip(TreeNodeAdv node, NodeControl nodeControl);
  }

}
