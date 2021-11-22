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

#pragma warning disable 1591


namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  public class NodeEventArgs : EventArgs
  {
    private TreeNodeAdv _node;
    public TreeNodeAdv Node
    {
      get { return _node; }
    }

    public NodeEventArgs(TreeNodeAdv node)
    {
      _node = node;
    }
  }
}
