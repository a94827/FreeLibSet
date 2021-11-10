using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms.VisualStyles;

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

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  [DesignTimeVisible(false), ToolboxItem(false)]
  public abstract class NodeControl : Component
  {
    #region Properties

    private TreeViewAdv _parent;
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TreeViewAdv Parent
    {
      get { return _parent; }
      set
      {
        if (value != _parent)
        {
          if (_parent != null)
            _parent.NodeControls.Remove(this);

          if (value != null)
            value.NodeControls.Add(this);
        }
      }
    }

    private ITreeViewAdvToolTipProvider _toolTipProvider;
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ITreeViewAdvToolTipProvider ToolTipProvider
    {
      get { return _toolTipProvider; }
      set { _toolTipProvider = value; }
    }

    private TreeColumn _parentColumn;
    [DefaultValue(null)]
    public TreeColumn ParentColumn
    {
      get { return _parentColumn; }
      set
      {
        _parentColumn = value;
        if (_parent != null)
          _parent.FullUpdate();
      }
    }

    private VerticalAlignment _verticalAlign = VerticalAlignment.Center;
    [DefaultValue(VerticalAlignment.Center)]
    public VerticalAlignment VerticalAlign
    {
      get { return _verticalAlign; }
      set
      {
        _verticalAlign = value;
        if (_parent != null)
          _parent.FullUpdate();
      }
    }

    private int _leftMargin = 0;
    public int LeftMargin
    {
      get { return _leftMargin; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException();

        _leftMargin = value;
        if (_parent != null)
          _parent.FullUpdate();
      }
    }
    #endregion

    internal virtual void AssignParent(TreeViewAdv parent)
    {
      _parent = parent;
    }

    protected virtual Rectangle GetBounds(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      Rectangle r = context.Bounds;
      Size s = GetActualSize(node, context);
      Size bs = new Size(r.Width - LeftMargin, Math.Min(r.Height, s.Height));
      switch (VerticalAlign)
      {
        case VerticalAlignment.Top:
          return new Rectangle(new Point(r.X + LeftMargin, r.Y), bs);
        case VerticalAlignment.Bottom:
          return new Rectangle(new Point(r.X + LeftMargin, r.Bottom - s.Height), bs);
        default:
          return new Rectangle(new Point(r.X + LeftMargin, r.Y + (r.Height - s.Height) / 2), bs);
      }
    }

    protected void CheckThread()
    {
      if (Parent != null && Control.CheckForIllegalCrossThreadCalls)
        if (Parent.InvokeRequired)
          throw new InvalidOperationException("Cross-thread calls are not allowed");
    }

    public bool IsVisible(TreeNodeAdv node)
    {
      NodeControlValueEventArgs args = new NodeControlValueEventArgs(node);
      args.Value = true;
      OnIsVisibleValueNeeded(args);
      return Convert.ToBoolean(args.Value);
    }

    internal Size GetActualSize(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      if (IsVisible(node))
      {
        Size s = MeasureSize(node, context);
        return new Size(s.Width + LeftMargin, s.Height);
      }
      else
        return Size.Empty;
    }

    public abstract Size MeasureSize(TreeNodeAdv node, TreeViewAdvDrawContext context);

    public abstract void Draw(TreeNodeAdv node, TreeViewAdvDrawContext context);

    public virtual string GetToolTip(TreeNodeAdv node)
    {
      if (ToolTipProvider != null)
        return ToolTipProvider.GetToolTip(node, this);
      else
        return string.Empty;
    }

    public virtual void MouseDown(TreeNodeAdvMouseEventArgs args)
    {
    }

    public virtual void MouseUp(TreeNodeAdvMouseEventArgs args)
    {
    }

    public virtual void MouseDoubleClick(TreeNodeAdvMouseEventArgs args)
    {
    }

    public virtual void KeyDown(KeyEventArgs args)
    {
    }

    public virtual void KeyUp(KeyEventArgs args)
    {
    }

    public event EventHandler<NodeControlValueEventArgs> IsVisibleValueNeeded;
    protected virtual void OnIsVisibleValueNeeded(NodeControlValueEventArgs args)
    {
      if (IsVisibleValueNeeded != null)
        IsVisibleValueNeeded(this, args);
    }
  }
}
