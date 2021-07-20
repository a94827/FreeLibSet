using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Drawing;
using AgeyevAV.Trees;

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

namespace AgeyevAV.ExtForms
{

  /// <summary>
  /// Provides a simple ready to use implementation of <see cref="ITreeModel"/>. Warning: this class is not optimized 
  /// to work with big amount of data. In this case create you own implementation of <c>ITreeModel</c>, and pay attention
  /// on GetChildren and IsLeaf methods.
  /// </summary>
  public class TreeModel : ITreeModel
  {
    private Node _root;
    public Node Root
    {
      get { return _root; }
    }

    public Collection<Node> Nodes
    {
      get { return _root.Nodes; }
    }

    public TreeModel()
    {
      _root = new Node();
      _root.Model = this;
    }

    public TreePath GetPath(Node node)
    {
      if (node == _root)
        return TreePath.Empty;
      else
      {
        Stack<object> stack = new Stack<object>();
        while (node != _root)
        {
          stack.Push(node);
          node = node.Parent;
        }
        return new TreePath(stack.ToArray());
      }
    }

    public Node FindNode(TreePath path)
    {
      if (path.IsEmpty())
        return _root;
      else
        return FindNode(_root, path, 0);
    }

    private Node FindNode(Node root, TreePath path, int level)
    {
      foreach (Node node in root.Nodes)
        if (node == path.FullPath[level])
        {
          if (level == path.FullPath.Length - 1)
            return node;
          else
            return FindNode(node, path, level + 1);
        }
      return null;
    }

    #region ITreeModel Members

    public System.Collections.IEnumerable GetChildren(TreePath treePath)
    {
      Node node = FindNode(treePath);
      if (node != null)
        foreach (Node n in node.Nodes)
          yield return n;
      else
        yield break;
    }

    public bool IsLeaf(TreePath treePath)
    {
      Node node = FindNode(treePath);
      if (node != null)
        return node.IsLeaf;
      else
        throw new ArgumentException("treePath");
    }

    public event EventHandler<TreeModelEventArgs> NodesChanged;
    internal void OnNodesChanged(TreeModelEventArgs args)
    {
      if (NodesChanged != null)
        NodesChanged(this, args);
    }

    public event EventHandler<TreePathEventArgs> StructureChanged;
    public void OnStructureChanged(TreePathEventArgs args)
    {
      if (StructureChanged != null)
        StructureChanged(this, args);
    }

    public event EventHandler<TreeModelEventArgs> NodesInserted;
    internal void OnNodeInserted(Node parent, int index, Node node)
    {
      if (NodesInserted != null)
      {
        TreeModelEventArgs args = new TreeModelEventArgs(GetPath(parent), new int[] { index }, new object[] { node });
        NodesInserted(this, args);
      }

    }

    public event EventHandler<TreeModelEventArgs> NodesRemoved;
    internal void OnNodeRemoved(Node parent, int index, Node node)
    {
      if (NodesRemoved != null)
      {
        TreeModelEventArgs args = new TreeModelEventArgs(GetPath(parent), new int[] { index }, new object[] { node });
        NodesRemoved(this, args);
      }
    }

    #endregion
  }
  public class Node
  {
    #region NodeCollection

    private class NodeCollection : Collection<Node>
    {
      private Node _owner;

      public NodeCollection(Node owner)
      {
        _owner = owner;
      }

      protected override void ClearItems()
      {
        while (this.Count != 0)
          this.RemoveAt(this.Count - 1);
      }

      protected override void InsertItem(int index, Node item)
      {
        if (item == null)
          throw new ArgumentNullException("item");

        if (item.Parent != _owner)
        {
          if (item.Parent != null)
            item.Parent.Nodes.Remove(item);
          item._parent = _owner;
          item._index = index;
          for (int i = index; i < Count; i++)
            this[i]._index++;
          base.InsertItem(index, item);

          TreeModel model = _owner.FindModel();
          if (model != null)
            model.OnNodeInserted(_owner, index, item);
        }
      }

      protected override void RemoveItem(int index)
      {
        Node item = this[index];
        item._parent = null;
        item._index = -1;
        for (int i = index + 1; i < Count; i++)
          this[i]._index--;
        base.RemoveItem(index);

        TreeModel model = _owner.FindModel();
        if (model != null)
          model.OnNodeRemoved(_owner, index, item);
      }

      protected override void SetItem(int index, Node item)
      {
        if (item == null)
          throw new ArgumentNullException("item");

        RemoveAt(index);
        InsertItem(index, item);
      }
    }

    #endregion

    #region Properties

    private TreeModel _model;
    internal TreeModel Model
    {
      get { return _model; }
      set { _model = value; }
    }

    private NodeCollection _nodes;
    public Collection<Node> Nodes
    {
      get { return _nodes; }
    }

    private Node _parent;
    public Node Parent
    {
      get { return _parent; }
      set
      {
        if (value != _parent)
        {
          if (_parent != null)
            _parent.Nodes.Remove(this);

          if (value != null)
            value.Nodes.Add(this);
        }
      }
    }

    private int _index = -1;
    public int Index
    {
      get
      {
        return _index;
      }
    }

    public Node PreviousNode
    {
      get
      {
        int index = Index;
        if (index > 0)
          return _parent.Nodes[index - 1];
        else
          return null;
      }
    }

    public Node NextNode
    {
      get
      {
        int index = Index;
        if (index >= 0 && index < _parent.Nodes.Count - 1)
          return _parent.Nodes[index + 1];
        else
          return null;
      }
    }

    private string _text;
    public virtual string Text
    {
      get { return _text; }
      set
      {
        if (_text != value)
        {
          _text = value;
          NotifyModel();
        }
      }
    }

    private CheckState _checkState;
    public virtual CheckState CheckState
    {
      get { return _checkState; }
      set
      {
        if (_checkState != value)
        {
          _checkState = value;
          NotifyModel();
        }
      }
    }

    private Image _image;
    public Image Image
    {
      get { return _image; }
      set
      {
        if (_image != value)
        {
          _image = value;
          NotifyModel();
        }
      }
    }

    private object _tag;
    public object Tag
    {
      get { return _tag; }
      set { _tag = value; }
    }

    public bool IsChecked
    {
      get
      {
        return CheckState != CheckState.Unchecked;
      }
      set
      {
        if (value)
          CheckState = CheckState.Checked;
        else
          CheckState = CheckState.Unchecked;
      }
    }

    public virtual bool IsLeaf
    {
      get
      {
        return false;
      }
    }

    #endregion

    public Node()
      : this(string.Empty)
    {
    }

    public Node(string text)
    {
      _text = text;
      _nodes = new NodeCollection(this);
    }

    public override string ToString()
    {
      return Text;
    }

    private TreeModel FindModel()
    {
      Node node = this;
      while (node != null)
      {
        if (node.Model != null)
          return node.Model;
        node = node.Parent;
      }
      return null;
    }

    protected void NotifyModel()
    {
      TreeModel model = FindModel();
      if (model != null && Parent != null)
      {
        TreePath path = model.GetPath(Parent);
        if (path != null)
        {
          TreeModelEventArgs args = new TreeModelEventArgs(path, new int[] { Index }, new object[] { this });
          model.OnNodesChanged(args);
        }
      }
    }
  }

}
