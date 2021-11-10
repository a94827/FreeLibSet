﻿using System;
using System.Collections.Generic;
using System.Text;

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
