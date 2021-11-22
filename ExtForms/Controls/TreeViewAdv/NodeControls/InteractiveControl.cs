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
using System.ComponentModel;

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  public abstract class InteractiveControl : BindableControl
  {
    private bool _editEnabled = false;
    [DefaultValue(false)]
    public bool EditEnabled
    {
      get { return _editEnabled; }
      set { _editEnabled = value; }
    }

    protected bool IsEditEnabled(TreeNodeAdv node)
    {
      if (EditEnabled)
      {
        NodeControlValueEventArgs args = new NodeControlValueEventArgs(node);
        args.Value = true;
        OnIsEditEnabledValueNeeded(args);
        return Convert.ToBoolean(args.Value);
      }
      else
        return false;
    }

    public event EventHandler<NodeControlValueEventArgs> IsEditEnabledValueNeeded;
    private void OnIsEditEnabledValueNeeded(NodeControlValueEventArgs args)
    {
      if (IsEditEnabledValueNeeded != null)
        IsEditEnabledValueNeeded(this, args);
    }
  }
}
