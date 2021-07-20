using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

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

namespace AgeyevAV.ExtForms.NodeControls
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
