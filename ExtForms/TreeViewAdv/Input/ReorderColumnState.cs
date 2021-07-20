using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

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


namespace AgeyevAV.ExtForms
{
  internal class ReorderColumnState : ColumnState
  {
    #region Properties

    private Point _location;
    public Point Location
    {
      get { return _location; }
    }

    private Bitmap _ghostImage;
    public Bitmap GhostImage
    {
      get { return _ghostImage; }
    }

    private TreeColumn _dropColumn;
    public TreeColumn DropColumn
    {
      get { return _dropColumn; }
    }

    private int _dragOffset;
    public int DragOffset
    {
      get { return _dragOffset; }
    }

    #endregion

    public ReorderColumnState(TreeViewAdv tree, TreeColumn column, Point initialMouseLocation)
      : base(tree, column)
    {
      _location = new Point(initialMouseLocation.X + Tree.OffsetX, 0);
      _dragOffset = tree.GetColumnX(column) - initialMouseLocation.X;
      _ghostImage = column.CreateGhostImage(new Rectangle(0, 0, column.Width, tree.ColumnHeaderHeight), tree.Font);
    }

    public override void KeyDown(KeyEventArgs args)
    {
      args.Handled = true;
      if (args.KeyCode == Keys.Escape)
        FinishResize();
    }

    public override void MouseDown(TreeNodeAdvMouseEventArgs args)
    {
    }

    public override void MouseUp(TreeNodeAdvMouseEventArgs args)
    {
      FinishResize();
    }

    public override bool MouseMove(MouseEventArgs args)
    {
      _dropColumn = null;
      _location = new Point(args.X + Tree.OffsetX, 0);
      int x = 0;
      foreach (TreeColumn c in Tree.Columns)
      {
        if (c.IsVisible)
        {
          if (_location.X < x + c.Width / 2)
          {
            _dropColumn = c;
            break;
          }
          x += c.Width;
        }
      }
      Tree.UpdateHeaders();
      return true;
    }

    private void FinishResize()
    {
      Tree.ChangeInput();
      if (Column == DropColumn)
        Tree.UpdateView();
      else
      {
        Tree.Columns.Remove(Column);
        if (DropColumn == null)
          Tree.Columns.Add(Column);
        else
          Tree.Columns.Insert(Tree.Columns.IndexOf(DropColumn), Column);

        Tree.OnColumnReordered(Column);
      }
    }
  }
}