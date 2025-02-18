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

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  public class EditEventArgs : NodeEventArgs
  {

    public EditEventArgs(TreeNodeAdv node, Control control)
      : base(node)
    {
      _control = control;
    }

    public Control Control
    {
      get { return _control; }
    }
    private readonly Control _control;
  }
}
