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
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FreeLibSet.Controls.TreeViewAdvInternal
{
  // TODO: Убрать это безобразие

  internal class TreeViewAdvIncrementalSearch
  {
    private const int SearchTimeout = 300; //end of incremental search timeot in msec

    private readonly TreeViewAdv _tree;
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
          if (label.StartsWith(_searchString, StringComparison.Ordinal))
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
          if (label.StartsWith(_searchString, StringComparison.Ordinal))
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
