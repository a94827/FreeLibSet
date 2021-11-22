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
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  internal class NodePlusMinus : NodeControl
  {
    public const int ImageSize = 9;
    public const int Width = 16;
    private Bitmap _plus;
    private Bitmap _minus;

    private VisualStyleRenderer _openedRenderer;
    private VisualStyleRenderer OpenedRenderer
    {
      get
      {
        if (_openedRenderer == null)
          _openedRenderer = new VisualStyleRenderer(VisualStyleElement.TreeView.Glyph.Opened);
        return _openedRenderer;

      }
    }

    private VisualStyleRenderer _closedRenderer;
    private VisualStyleRenderer ClosedRenderer
    {
      get
      {
        if (_closedRenderer == null)
          _closedRenderer = new VisualStyleRenderer(VisualStyleElement.TreeView.Glyph.Closed);
        return _closedRenderer;
      }
    }

    public NodePlusMinus()
    {
      _plus = TreeViewAdvRes.TreeViewAdvResources.plus;
      _minus = TreeViewAdvRes.TreeViewAdvResources.minus;
    }

    public override Size MeasureSize(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      return new Size(Width, Width);
    }

    public override void Draw(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      if (node.CanExpand)
      {
        Rectangle r = context.Bounds;
        int dy = (int)Math.Round((float)(r.Height - ImageSize) / 2);
        if (Application.RenderWithVisualStyles)
        {
          VisualStyleRenderer renderer;
          if (node.IsExpanded)
            renderer = OpenedRenderer;
          else
            renderer = ClosedRenderer;
          renderer.DrawBackground(context.Graphics, new Rectangle(r.X, r.Y + dy, ImageSize, ImageSize));
        }
        else
        {
          Image img;
          if (node.IsExpanded)
            img = _minus;
          else
            img = _plus;
          context.Graphics.DrawImageUnscaled(img, new Point(r.X, r.Y + dy));
        }
      }
    }

    public override void MouseDown(TreeNodeAdvMouseEventArgs args)
    {
      if (args.Button == MouseButtons.Left)
      {
        args.Handled = true;
        if (args.Node.CanExpand)
          args.Node.IsExpanded = !args.Node.IsExpanded;
      }
    }

    public override void MouseDoubleClick(TreeNodeAdvMouseEventArgs args)
    {
      args.Handled = true; // Supress expand/collapse when double click on plus/minus
    }
  }
}
