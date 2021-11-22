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

namespace FreeLibSet.Controls.TreeViewAdvInternal
{
  internal class InputWithShift : NormalInputState
  {
    public InputWithShift(TreeViewAdv tree)
      : base(tree)
    {
    }

    protected override void FocusRow(TreeNodeAdv node)
    {
      Tree.SuspendSelectionEvent = true;
      try
      {
        if (Tree.SelectionMode == TreeViewAdvSelectionMode.Single || Tree.SelectionStart == null)
          base.FocusRow(node);
        else if (CanSelect(node))
        {
          SelectAllFromStart(node);
          Tree.CurrentNode = node;
          Tree.ScrollTo(node);
        }
      }
      finally
      {
        Tree.SuspendSelectionEvent = false;
      }
    }

    protected override void DoMouseOperation(TreeNodeAdvMouseEventArgs args)
    {
      if (Tree.SelectionMode == TreeViewAdvSelectionMode.Single || Tree.SelectionStart == null)
      {
        base.DoMouseOperation(args);
      }
      else if (CanSelect(args.Node))
      {
        Tree.SuspendSelectionEvent = true;
        try
        {
          SelectAllFromStart(args.Node);
        }
        finally
        {
          Tree.SuspendSelectionEvent = false;
        }
      }
    }

    protected override void MouseDownAtEmptySpace(TreeNodeAdvMouseEventArgs args)
    {
    }

    private void SelectAllFromStart(TreeNodeAdv node)
    {
      Tree.ClearSelectionInternal();
      int a = node.Row;
      int b = Tree.SelectionStart.Row;
      for (int i = Math.Min(a, b); i <= Math.Max(a, b); i++)
      {
        if (Tree.SelectionMode == TreeViewAdvSelectionMode.Multi || Tree.RowMap[i].Parent == node.Parent)
          Tree.RowMap[i].IsSelected = true;
      }
    }
  }
}
