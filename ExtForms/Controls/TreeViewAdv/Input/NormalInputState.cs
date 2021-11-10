using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

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
  internal class NormalInputState : InputState
  {
    private bool _mouseDownFlag = false;

    public NormalInputState(TreeViewAdv tree)
      : base(tree)
    {
    }

    public override void KeyDown(KeyEventArgs args)
    {
      if (Tree.CurrentNode == null && Tree.Root.Nodes.Count > 0)
        Tree.CurrentNode = Tree.Root.Nodes[0];

      if (Tree.CurrentNode != null)
      {
        switch (args.KeyCode)
        {
          case Keys.Right:
            if (!Tree.CurrentNode.IsExpanded)
              Tree.CurrentNode.IsExpanded = true;
            else if (Tree.CurrentNode.Nodes.Count > 0)
              Tree.SelectedNode = Tree.CurrentNode.Nodes[0];
            args.Handled = true;
            break;
          case Keys.Left:
            if (Tree.CurrentNode.IsExpanded)
              Tree.CurrentNode.IsExpanded = false;
            else if (Tree.CurrentNode.Parent != Tree.Root)
              Tree.SelectedNode = Tree.CurrentNode.Parent;
            args.Handled = true;
            break;
          case Keys.Down:
            NavigateForward(1);
            args.Handled = true;
            break;
          case Keys.Up:
            NavigateBackward(1);
            args.Handled = true;
            break;
          case Keys.PageDown:
            NavigateForward(Math.Max(1, Tree.CurrentPageSize - 1));
            args.Handled = true;
            break;
          case Keys.PageUp:
            NavigateBackward(Math.Max(1, Tree.CurrentPageSize - 1));
            args.Handled = true;
            break;
          case Keys.Home:
            if (Tree.RowMap.Count > 0)
              FocusRow(Tree.RowMap[0]);
            args.Handled = true;
            break;
          case Keys.End:
            if (Tree.RowMap.Count > 0)
              FocusRow(Tree.RowMap[Tree.RowMap.Count - 1]);
            args.Handled = true;
            break;
          case Keys.Subtract:
            Tree.CurrentNode.Collapse();
            args.Handled = true;
            args.SuppressKeyPress = true;
            break;
          case Keys.Add:
            Tree.CurrentNode.Expand();
            args.Handled = true;
            args.SuppressKeyPress = true;
            break;
          case Keys.Multiply:
            Tree.CurrentNode.ExpandAll();
            args.Handled = true;
            args.SuppressKeyPress = true;
            break;
          case Keys.A:
            if (args.Modifiers == Keys.Control)
              Tree.SelectAllNodes();
            break;
        }
      }
    }

    public override void MouseDown(TreeNodeAdvMouseEventArgs args)
    {
      if (args.Node != null)
      {
        Tree.ItemDragMode = true;
        Tree.ItemDragStart = args.Location;

        if (args.Button == MouseButtons.Left || args.Button == MouseButtons.Right)
        {
          Tree.BeginUpdate();
          try
          {
            Tree.CurrentNode = args.Node;
            if (args.Node.IsSelected)
              _mouseDownFlag = true;
            else
            {
              _mouseDownFlag = false;
              DoMouseOperation(args);
            }
          }
          finally
          {
            Tree.EndUpdate();
          }
        }

      }
      else
      {
        Tree.ItemDragMode = false;
        MouseDownAtEmptySpace(args);
      }
    }

    public override void MouseUp(TreeNodeAdvMouseEventArgs args)
    {
      Tree.ItemDragMode = false;
      if (_mouseDownFlag && args.Node != null)
      {
        if (args.Button == MouseButtons.Left)
          DoMouseOperation(args);
        else if (args.Button == MouseButtons.Right)
          Tree.CurrentNode = args.Node;
      }
      _mouseDownFlag = false;
    }


    private void NavigateBackward(int n)
    {
      int row = Math.Max(Tree.CurrentNode.Row - n, 0);
      if (row != Tree.CurrentNode.Row)
        FocusRow(Tree.RowMap[row]);
    }

    private void NavigateForward(int n)
    {
      int row = Math.Min(Tree.CurrentNode.Row + n, Tree.RowCount - 1);
      if (row != Tree.CurrentNode.Row)
        FocusRow(Tree.RowMap[row]);
    }

    protected virtual void MouseDownAtEmptySpace(TreeNodeAdvMouseEventArgs args)
    {
      Tree.ClearSelection();
    }

    protected virtual void FocusRow(TreeNodeAdv node)
    {
      Tree.SuspendSelectionEvent = true;
      try
      {
        Tree.ClearSelectionInternal();
        Tree.CurrentNode = node;
        Tree.SelectionStart = node;
        node.IsSelected = true;
        Tree.ScrollTo(node);
      }
      finally
      {
        Tree.SuspendSelectionEvent = false;
      }
    }

    protected bool CanSelect(TreeNodeAdv node)
    {
      if (Tree.SelectionMode == TreeViewAdvSelectionMode.MultiSameParent)
      {
        return (Tree.SelectionStart == null || node.Parent == Tree.SelectionStart.Parent);
      }
      else
        return true;
    }

    protected virtual void DoMouseOperation(TreeNodeAdvMouseEventArgs args)
    {
      if (Tree.SelectedNodes.Count == 1 && args.Node != null && args.Node.IsSelected)
        return;

      Tree.SuspendSelectionEvent = true;
      try
      {
        Tree.ClearSelectionInternal();
        if (args.Node != null)
          args.Node.IsSelected = true;
        Tree.SelectionStart = args.Node;
      }
      finally
      {
        Tree.SuspendSelectionEvent = false;
      }
    }
  }
}
