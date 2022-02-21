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
using System.Collections.ObjectModel;
using FreeLibSet.Models.Tree;
using System.Windows.Forms;
#pragma warning disable 1591

namespace FreeLibSet.Models.Tree
{
  /// <summary>
  /// Provides a simple ready to use implementation of <see cref="ITreeModel"/>. Warning: this class is not optimized 
  /// to work with big amount of data. In this case create you own implementation of <c>ITreeModel</c>, and pay attention
  /// on GetChildren and IsLeaf methods.
  /// </summary>
  public class TreeModel : ITreeModel
  {
    #region Вложенные классы

    public class Node
    {
      #region Конструкторы

      public Node()
        : this(string.Empty)
      {
      }

      public Node(string text)
      {
        _text = text;
        _nodes = new NodeCollection(this);
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
      public Collection<Node> Nodes { get { return _nodes; } }

      internal Node _parent;
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

      internal int _index = -1;
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

      #region Методы

      public override string ToString()
      {
        return Text;
      }

      internal TreeModel FindModel()
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

      #endregion
    }

    private class NodeCollection : Collection<Node>
    {
      #region Конструктор

      public NodeCollection(Node owner)
      {
        _owner = owner;
      }

      #endregion

      #region Свойства

      private Node _owner;

      #endregion

      #region Методы

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

      #endregion
    }

    #endregion


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
      if (path.IsEmpty)
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

}
