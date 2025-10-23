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
using System.Security.Permissions;
using System.Drawing;

namespace FreeLibSet.Controls.TreeViewAdvInternal
{
  internal class ResizeColumnState : ColumnState
  {
    private readonly Point _initLocation;
    private readonly int _initWidth;

    public ResizeColumnState(TreeViewAdv tree, TreeColumn column, Point p)
      : base(tree, column)
    {
      _initLocation = p;
      _initWidth = column.Width;
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

    private void FinishResize()
    {
      Tree.ChangeInput();
      Tree.FullUpdate();
      Tree.OnColumnWidthChanged(Column);
    }

    public override bool MouseMove(MouseEventArgs args)
    {
      Column.Width = _initWidth + args.Location.X - _initLocation.X;
      Tree.UpdateView();
      return true;
    }

    public override void MouseDoubleClick(TreeNodeAdvMouseEventArgs args)
    {
      Tree.AutoSizeColumn(Column);
    }
  }
}
