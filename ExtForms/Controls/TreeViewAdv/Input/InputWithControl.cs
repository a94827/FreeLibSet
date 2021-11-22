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
  internal class InputWithControl : NormalInputState
  {
    public InputWithControl(TreeViewAdv tree)
      : base(tree)
    {
    }

    protected override void DoMouseOperation(TreeNodeAdvMouseEventArgs args)
    {
      if (Tree.SelectionMode == TreeViewAdvSelectionMode.Single)
      {
        base.DoMouseOperation(args);
      }
      else if (CanSelect(args.Node))
      {
        args.Node.IsSelected = !args.Node.IsSelected;
        Tree.SelectionStart = args.Node;
      }
    }

    protected override void MouseDownAtEmptySpace(TreeNodeAdvMouseEventArgs args)
    {
    }
  }
}
