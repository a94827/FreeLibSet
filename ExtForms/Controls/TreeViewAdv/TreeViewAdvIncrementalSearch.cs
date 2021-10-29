using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using System.ComponentModel;
using System.Drawing;
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


namespace FreeLibSet.Controls
{
  // TODO: Убрать это безобразие

  internal class TreeViewAdvIncrementalSearch
  {
    private const int SearchTimeout = 300; //end of incremental search timeot in msec

    private TreeViewAdv _tree;
    private TreeNodeAdv _currentNode;
    private string _searchString = "";
    private DateTime _lastKeyPressed = DateTime.Now;

    public TreeViewAdvIncrementalSearch(TreeViewAdv tree)
    {
      _tree = tree;
    }

    public void Search(Char value)
    {
      if (!Char.IsControl(value))
      {
        Char ch = Char.ToLowerInvariant(value);
        DateTime dt = DateTime.Now;
        TimeSpan ts = dt - _lastKeyPressed;
        _lastKeyPressed = dt;
        if (ts.TotalMilliseconds < SearchTimeout)
        {
          if (_searchString == value.ToString())
            FirstCharSearch(ch);
          else
            ContinuousSearch(ch);
        }
        else
        {
          FirstCharSearch(ch);
        }
      }
    }

    private void ContinuousSearch(Char value)
    {
      if (value == ' ' && String.IsNullOrEmpty(_searchString))
        return; //Ingnore leading space

      _searchString += value;
      DoContinuousSearch();
    }

    private void FirstCharSearch(Char value)
    {
      if (value == ' ')
        return;

      _searchString = value.ToString();
      TreeNodeAdv node = null;
      if (_tree.SelectedNode != null)
        node = _tree.SelectedNode.NextVisibleNode;
      if (node == null)
        node = _tree.Root.NextVisibleNode;

      if (node != null)
        foreach (string label in IterateNodeLabels(node))
        {
          if (label.StartsWith(_searchString))
          {
            _tree.SelectedNode = _currentNode;
            return;
          }
        }
    }

    public virtual void EndSearch()
    {
      _currentNode = null;
      _searchString = "";
    }

    protected IEnumerable<string> IterateNodeLabels(TreeNodeAdv start)
    {
      _currentNode = start;
      while (_currentNode != null)
      {
        foreach (string label in GetNodeLabels(_currentNode))
          yield return label;

        _currentNode = _currentNode.NextVisibleNode;
        if (_currentNode == null)
          _currentNode = _tree.Root;

        if (start == _currentNode)
          break;
      }
    }

    private IEnumerable<string> GetNodeLabels(TreeNodeAdv node)
    {
      foreach (NodeControl nc in _tree.NodeControls)
      {
        BindableControl bc = nc as BindableControl;
        if (bc != null && bc.IncrementalSearchEnabled)
        {
          object obj = bc.GetValue(node);
          if (obj != null)
            yield return obj.ToString().ToLowerInvariant();
        }
      }
    }

    private bool DoContinuousSearch()
    {
      bool found = false;
      if (!String.IsNullOrEmpty(_searchString))
      {
        TreeNodeAdv node = null;
        if (_tree.SelectedNode != null)
          node = _tree.SelectedNode;
        if (node == null)
          node = _tree.Root.NextVisibleNode;

        foreach (string label in IterateNodeLabels(node))
        {
          if (label.StartsWith(_searchString))
          {
            found = true;
            _tree.SelectedNode = _currentNode;
            break;
          }
        }
      }
      return found;
    }

  }
}
