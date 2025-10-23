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
  internal abstract class ColumnState : InputState
  {
    private readonly TreeColumn _column;
    public TreeColumn Column
    {
      get { return _column; }
    }

    public ColumnState(TreeViewAdv tree, TreeColumn column)
      : base(tree)
    {
      _column = column;
    }
  }
}
