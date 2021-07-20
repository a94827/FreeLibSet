using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
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


namespace AgeyevAV.ExtForms
{
  internal class TreeColumnCollection : Collection<TreeColumn>
  {
    private TreeViewAdv _treeView;

    public TreeColumnCollection(TreeViewAdv treeView)
    {
      _treeView = treeView;
    }

    protected override void InsertItem(int index, TreeColumn item)
    {
      base.InsertItem(index, item);
      BindEvents(item);
      _treeView.UpdateColumns();
    }

    protected override void RemoveItem(int index)
    {
      UnbindEvents(this[index]);
      base.RemoveItem(index);
      _treeView.UpdateColumns();
    }

    protected override void SetItem(int index, TreeColumn item)
    {
      UnbindEvents(this[index]);
      base.SetItem(index, item);
      item.Owner = this;
      BindEvents(item);
      _treeView.UpdateColumns();
    }

    protected override void ClearItems()
    {
      foreach (TreeColumn c in Items)
        UnbindEvents(c);
      Items.Clear();
      _treeView.UpdateColumns();
    }

    private void BindEvents(TreeColumn item)
    {
      item.Owner = this;
      item.HeaderChanged += HeaderChanged;
      item.IsVisibleChanged += IsVisibleChanged;
      item.WidthChanged += WidthChanged;
      item.SortOrderChanged += SortOrderChanged;
    }

    private void UnbindEvents(TreeColumn item)
    {
      item.Owner = null;
      item.HeaderChanged -= HeaderChanged;
      item.IsVisibleChanged -= IsVisibleChanged;
      item.WidthChanged -= WidthChanged;
      item.SortOrderChanged -= SortOrderChanged;
    }

    void SortOrderChanged(object sender, EventArgs args)
    {
      TreeColumn changed = sender as TreeColumn;
      //Only one column at a time can have a sort property set
      if (changed.SortOrder != SortOrder.None)
      {
        foreach (TreeColumn col in this)
        {
          if (col != changed)
            col.SortOrder = SortOrder.None;
        }
      }
      _treeView.UpdateHeaders();
    }

    void WidthChanged(object sender, EventArgs args)
    {
      _treeView.ChangeColumnWidth(sender as TreeColumn);
    }

    void IsVisibleChanged(object sender, EventArgs args)
    {
      _treeView.FullUpdate();
    }

    void HeaderChanged(object sender, EventArgs args)
    {
      _treeView.UpdateView();
    }
  }
}
