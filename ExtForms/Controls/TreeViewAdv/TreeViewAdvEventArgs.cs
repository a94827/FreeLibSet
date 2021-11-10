using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.Controls.TreeViewAdvNodeControls;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
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

/*
 * Original TreeViewAdv component from Aga.Controls.dll
 * http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
 * http://sourceforge.net/projects/treeviewadv/
 */

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
