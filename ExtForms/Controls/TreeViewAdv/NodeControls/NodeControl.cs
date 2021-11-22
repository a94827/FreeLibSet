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
using System.ComponentModel;
using System.Windows.Forms.VisualStyles;

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
