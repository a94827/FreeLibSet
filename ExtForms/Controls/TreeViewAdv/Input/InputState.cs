// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
//
// Original TreeViewAdv component from Aga.Controls.dll
// Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
// http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
// http://sourceforge.net/projects/treeviewadv/

using System;
using System.Windows.Forms;

namespace FreeLibSet.Controls.TreeViewAdvInternal
{
  internal abstract class InputState
  {
    private TreeViewAdv _tree;

    public TreeViewAdv Tree
    {
      get { return _tree; }
    }

    public InputState(TreeViewAdv tree)
    {
      _tree = tree;
    }

    public abstract void KeyDown(System.Windows.Forms.KeyEventArgs args);
    public abstract void MouseDown(TreeNodeAdvMouseEventArgs args);
    public abstract void MouseUp(TreeNodeAdvMouseEventArgs args);

    /// <summary>
    /// handle OnMouseMove event
    /// </summary>
    /// <param name="args"></param>
    /// <returns>true if event was handled and should be dispatched</returns>
    public virtual bool MouseMove(MouseEventArgs args)
    {
      return false;
    }

    public virtual void MouseDoubleClick(TreeNodeAdvMouseEventArgs args)
    {
    }
  }
}
