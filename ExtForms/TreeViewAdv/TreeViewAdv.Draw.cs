using System;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
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
  public partial class TreeViewAdv
  {
    public void AutoSizeColumn(TreeColumn column)
    {
      if (!Columns.Contains(column))
        throw new ArgumentException("column");

      TreeViewAdvDrawContext context = new TreeViewAdvDrawContext();
      context.Graphics = Graphics.FromImage(new Bitmap(1, 1));
      context.Font = this.Font;
      int res = 0;
      for (int row = 0; row < RowCount; row++)
      {
        if (row < RowMap.Count)
        {
          int w = 0;
          TreeNodeAdv node = RowMap[row];
          foreach (NodeControl nc in NodeControls)
          {
            if (nc.ParentColumn == column)
              w += nc.GetActualSize(node, _measureContext).Width;
          }
          res = Math.Max(res, w);
        }
      }

      if (res > 0)
        column.Width = res;
    }

    private void CreatePens()
    {
      CreateLinePen();
      CreateMarkPen();
    }

    private void CreateMarkPen()
    {
      GraphicsPath path = new GraphicsPath();
      path.AddLines(new Point[] { new Point(0, 0), new Point(1, 1), new Point(-1, 1), new Point(0, 0) });
      CustomLineCap cap = new CustomLineCap(null, path);
      cap.WidthScale = 1.0f;

      _markPen = new Pen(_dragDropMarkColor, _dragDropMarkWidth);
      _markPen.CustomStartCap = cap;
      _markPen.CustomEndCap = cap;
    }

    private void CreateLinePen()
    {
      _linePen = new Pen(_lineColor);
      _linePen.DashStyle = DashStyle.Dot;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      TreeViewAdvDrawContext context = new TreeViewAdvDrawContext();
      context.Graphics = e.Graphics;
      context.Font = this.Font;
      context.Enabled = Enabled;

      // Ageyev A.V., 24.05.2015
      // Under windows 98, if we don't fill the background, it gets the "Control" color instead of actual value in BackColor property
      //e.Graphics.FillRectangle(new SolidBrush(BackColor), e.ClipRectangle);
            
      int y = 0;
      int gridHeight = 0;

      if (UseColumns)
      {
        DrawColumnHeaders(e.Graphics);
        y += ColumnHeaderHeight;
        if (Columns.Count == 0 || e.ClipRectangle.Height <= y)
          return;
      }

      int firstRowY = _rowLayout.GetRowBounds(FirstVisibleRow).Y;
      y -= firstRowY;

      e.Graphics.ResetTransform();
      e.Graphics.TranslateTransform(-OffsetX, y);
      Rectangle displayRect = DisplayRectangle;
      for (int row = FirstVisibleRow; row < RowCount; row++)
      {
        Rectangle rowRect = _rowLayout.GetRowBounds(row);
        gridHeight += rowRect.Height;
        if (rowRect.Y + y > displayRect.Bottom)
          break;
        else
          DrawRow(e, ref context, row, rowRect);
      }

      if ((GridLineStyle & TreeViewAdvGridLineStyle.Vertical) == TreeViewAdvGridLineStyle.Vertical && UseColumns)
        DrawVerticalGridLines(e.Graphics, firstRowY);

      if (_dropPosition.Node != null && DragMode && HighlightDropPosition)
        DrawDropMark(e.Graphics);

      e.Graphics.ResetTransform();
      DrawScrollBarsBox(e.Graphics);

      if (DragMode && _dragBitmap != null)
        e.Graphics.DrawImage(_dragBitmap, PointToClient(MousePosition));
             
    }

    private void DrawRow(PaintEventArgs e, ref TreeViewAdvDrawContext context, int row, Rectangle rowRect)
    {
      TreeNodeAdv node = RowMap[row];
      context.DrawSelection = TreeViewAdvDrawSelectionMode.None;
      context.CurrentEditorOwner = CurrentEditorOwner;
      if (DragMode)
      {
        if ((_dropPosition.Node == node) && _dropPosition.Position == TreeViewAdvNodePosition.Inside && HighlightDropPosition)
          context.DrawSelection = TreeViewAdvDrawSelectionMode.Active;
      }
      else
      {
        if (node.IsSelected && Focused)
          context.DrawSelection = TreeViewAdvDrawSelectionMode.Active;
        else if (node.IsSelected && !Focused && !HideSelection)
          context.DrawSelection = TreeViewAdvDrawSelectionMode.Inactive;
      }
      context.DrawFocus = Focused && CurrentNode == node;

      OnRowDraw(e, node, context, row, rowRect);

      if (FullRowSelect)
      {
        context.DrawFocus = false;
        if (context.DrawSelection == TreeViewAdvDrawSelectionMode.Active || context.DrawSelection == TreeViewAdvDrawSelectionMode.Inactive)
        {
          Rectangle focusRect = new Rectangle(OffsetX, rowRect.Y, ClientRectangle.Width, rowRect.Height);
          if (context.DrawSelection == TreeViewAdvDrawSelectionMode.Active)
          {
            e.Graphics.FillRectangle(SystemBrushes.Highlight, focusRect);
            context.DrawSelection = TreeViewAdvDrawSelectionMode.FullRowSelect;
          }
          else
          {
            e.Graphics.FillRectangle(SystemBrushes.InactiveBorder, focusRect);
            context.DrawSelection = TreeViewAdvDrawSelectionMode.None;
          }
        }
      }

      if ((GridLineStyle & TreeViewAdvGridLineStyle.Horizontal) == TreeViewAdvGridLineStyle.Horizontal)
        e.Graphics.DrawLine(SystemPens.InactiveBorder, 0, rowRect.Bottom, e.Graphics.ClipBounds.Right, rowRect.Bottom);

      if (ShowLines)
        DrawLines(e.Graphics, node, rowRect);

      DrawNode(node, context);
    }

    private void DrawVerticalGridLines(Graphics gr, int y)
    {
      int x = 0;
      foreach (TreeColumn c in Columns)
      {
        if (c.IsVisible)
        {
          x += c.Width;
          gr.DrawLine(SystemPens.InactiveBorder, x - 1, y, x - 1, gr.ClipBounds.Bottom);
        }
      }
    }

    private void DrawColumnHeaders(Graphics gr)
    {
      ReorderColumnState reorder = Input as ReorderColumnState;
      int x = 0;
      TreeColumn.DrawBackground(gr, new Rectangle(0, 0, ClientRectangle.Width + 2, ColumnHeaderHeight - 1), false, false);
      gr.TranslateTransform(-OffsetX, 0);
      foreach (TreeColumn c in Columns)
      {
        if (c.IsVisible)
        {
          if (x >= OffsetX && x - OffsetX < this.Bounds.Width)// skip invisible columns
          {
            Rectangle rect = new Rectangle(x, 0, c.Width, ColumnHeaderHeight - 1);
            gr.SetClip(rect);
            bool pressed = ((Input is ClickColumnState || reorder != null) && ((Input as ColumnState).Column == c));
            c.Draw(gr, rect, Font, pressed, _hotColumn == c);
            gr.ResetClip();

            if (reorder != null && reorder.DropColumn == c)
              TreeColumn.DrawDropMark(gr, rect);
          }
          x += c.Width;
        }
      }

      if (reorder != null)
      {
        if (reorder.DropColumn == null)
          TreeColumn.DrawDropMark(gr, new Rectangle(x, 0, 0, ColumnHeaderHeight));
        gr.DrawImage(reorder.GhostImage, new Point(reorder.Location.X + +reorder.DragOffset, reorder.Location.Y));
      }
    }

    public void DrawNode(TreeNodeAdv node, TreeViewAdvDrawContext context)
    {
      foreach (NodeControlInfo item in GetNodeControls(node))
      {
        if (item.Bounds.Right >= OffsetX && item.Bounds.X - OffsetX < this.Bounds.Width)// skip invisible nodes
        {
          context.Bounds = item.Bounds;
          context.Graphics.SetClip(context.Bounds);
          item.Control.Draw(node, context);
          context.Graphics.ResetClip();
        }
      }
    }

    private void DrawScrollBarsBox(Graphics gr)
    {
      Rectangle r1 = DisplayRectangle;
      Rectangle r2 = ClientRectangle;
      gr.FillRectangle(SystemBrushes.Control,
        new Rectangle(r1.Right, r1.Bottom, r2.Width - r1.Width, r2.Height - r1.Height));
    }

    private void DrawDropMark(Graphics gr)
    {
      if (_dropPosition.Position == TreeViewAdvNodePosition.Inside)
        return;

      Rectangle rect = GetNodeBounds(_dropPosition.Node);
      int right = DisplayRectangle.Right - LeftMargin + OffsetX;
      int y = rect.Y;
      if (_dropPosition.Position == TreeViewAdvNodePosition.After)
        y = rect.Bottom;
      gr.DrawLine(_markPen, rect.X, y, right, y);
    }

    private void DrawLines(Graphics gr, TreeNodeAdv node, Rectangle rowRect)
    {
      if (UseColumns && Columns.Count > 0)
        gr.SetClip(new Rectangle(0, rowRect.Y, Columns[0].Width, rowRect.Bottom));

      TreeNodeAdv curNode = node;
      while (curNode != _root && curNode != null)
      {
        int level = curNode.Level;
        int x = (level - 1) * _indent + NodePlusMinus.ImageSize / 2 + LeftMargin;
        int width = NodePlusMinus.Width - NodePlusMinus.ImageSize / 2;
        int y = rowRect.Y;
        int y2 = y + rowRect.Height;

        if (curNode == node)
        {
          int midy = y + rowRect.Height / 2;
          gr.DrawLine(_linePen, x, midy, x + width, midy);
          if (curNode.NextNode == null)
            y2 = y + rowRect.Height / 2;
        }

        if (node.Row == 0)
          y = rowRect.Height / 2;
        if (curNode.NextNode != null || curNode == node)
          gr.DrawLine(_linePen, x, y, x, y2);

        curNode = curNode.Parent;
      }

      gr.ResetClip();
    }
  }
}
