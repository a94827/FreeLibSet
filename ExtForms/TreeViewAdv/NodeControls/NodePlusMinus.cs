using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
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


namespace AgeyevAV.ExtForms.NodeControls
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
