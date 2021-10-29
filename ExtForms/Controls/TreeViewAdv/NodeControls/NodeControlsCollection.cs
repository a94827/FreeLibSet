using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Design;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;

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


namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  internal class NodeControlsCollection : Collection<NodeControl>
  {
    private TreeViewAdv _tree;

    public NodeControlsCollection(TreeViewAdv tree)
    {
      _tree = tree;
    }

    protected override void ClearItems()
    {
      _tree.BeginUpdate();
      try
      {
        while (this.Count != 0)
          this.RemoveAt(this.Count - 1);
      }
      finally
      {
        _tree.EndUpdate();
      }
    }

    protected override void InsertItem(int index, NodeControl item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      if (item.Parent != _tree)
      {
        if (item.Parent != null)
        {
          item.Parent.NodeControls.Remove(item);
        }
        base.InsertItem(index, item);
        item.AssignParent(_tree);
        _tree.FullUpdate();
      }
    }

    protected override void RemoveItem(int index)
    {
      NodeControl value = this[index];
      value.AssignParent(null);
      base.RemoveItem(index);
      _tree.FullUpdate();
    }

    protected override void SetItem(int index, NodeControl item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      _tree.BeginUpdate();
      try
      {
        RemoveAt(index);
        InsertItem(index, item);
      }
      finally
      {
        _tree.EndUpdate();
      }
    }
  }

  internal class NodeControlCollectionEditor : CollectionEditor
  {
    private Type[] _types;

    public NodeControlCollectionEditor(Type type)
      : base(type)
    {
      _types = new Type[] { typeof(NodeTextBox), typeof(NodeIntegerTextBox), typeof(NodeDecimalTextBox), 
				typeof(NodeComboBox), typeof(NodeCheckBox),
				typeof(NodeStateIcon), typeof(NodeIcon), typeof(NodeNumericUpDown), typeof(ExpandingIcon)  };
    }

    protected override System.Type[] CreateNewItemTypes()
    {
      return _types;
    }
  }
}
