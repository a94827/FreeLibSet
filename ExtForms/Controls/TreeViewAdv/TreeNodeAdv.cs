// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
//
// Original TreeViewAdv component from Aga.Controls.dll
// Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
// http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
// http://sourceforge.net/projects/treeviewadv/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Security.Permissions;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Узел в дереве <see cref="TreeViewAdv"/>
  /// </summary>
  [Serializable]
  public sealed class TreeNodeAdv : ISerializable
  {
    #region NodeCollection class

    private class NodeCollection : Collection<TreeNodeAdv>
    {
      private TreeNodeAdv _owner;

      public NodeCollection(TreeNodeAdv owner)
      {
        _owner = owner;
      }

      protected override void ClearItems()
      {
        while (this.Count != 0)
          this.RemoveAt(this.Count - 1);
      }

      protected override void InsertItem(int index, TreeNodeAdv item)
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
        }

        if (_owner.Tree != null && _owner.Tree.Model == null)
        {
          _owner.Tree.SmartFullUpdate();
        }
      }

      protected override void RemoveItem(int index)
      {
        TreeNodeAdv item = this[index];
        item._parent = null;
        item._index = -1;
        for (int i = index + 1; i < Count; i++)
          this[i]._index--;
        base.RemoveItem(index);

        if (_owner.Tree != null && _owner.Tree.Model == null)
        {
          _owner.Tree.UpdateSelection();
          _owner.Tree.SmartFullUpdate();
        }
      }

      protected override void SetItem(int index, TreeNodeAdv item)
      {
        if (item == null)
          throw new ArgumentNullException("item");
        RemoveAt(index);
        InsertItem(index, item);
      }
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Не знаю, зачем может понадобиться
    /// </summary>
    /// <param name="tag"></param>
    public TreeNodeAdv(object tag)
      : this(null, tag)
    {
    }

    internal TreeNodeAdv(TreeViewAdv tree, object tag)
    {
      _row = -1;
      _tree = tree;
      _nodes = new NodeCollection(this);
      _children = new ReadOnlyCollection<TreeNodeAdv>(_nodes);
      _tag = tag;
    }

    #endregion

    #region Events

    /// <summary>
    /// Вызывается перед свертыванием узла в дереве
    /// </summary>
    public event EventHandler<TreeViewAdvEventArgs> Collapsing;

    internal void OnCollapsing()
    {
      if (Collapsing != null)
        Collapsing(this, new TreeViewAdvEventArgs(this));
    }

    /// <summary>
    /// Вызывается после свертывания узла в дереве
    /// </summary>
    public event EventHandler<TreeViewAdvEventArgs> Collapsed;

    internal void OnCollapsed()
    {
      if (Collapsed != null)
        Collapsed(this, new TreeViewAdvEventArgs(this));
    }

    /// <summary>
    /// Вызывается перед развертыванием узла в дереве
    /// </summary>
    public event EventHandler<TreeViewAdvEventArgs> Expanding;

    internal void OnExpanding()
    {
      if (Expanding != null)
        Expanding(this, new TreeViewAdvEventArgs(this));
    }

    /// <summary>
    /// Вызывается поле развертыывания узла в дереве
    /// </summary>
    public event EventHandler<TreeViewAdvEventArgs> Expanded;

    internal void OnExpanded()
    {
      if (Expanded != null)
        Expanded(this, new TreeViewAdvEventArgs(this));
    }

    #endregion

    #region Properties

    /// <summary>
    /// Просмотр, к которому относится узел
    /// </summary>
    public TreeViewAdv Tree { get { return _tree; } }
    private readonly TreeViewAdv _tree;

    /// <summary>
    /// Индекс узла в пределах просмотра, с учетом только видимых узлов.
    /// Узлы, которые принадлежат свернутым родительским узлам, не учитываются.
    /// </summary>
    public int Row
    {
      get { return _row; }
      internal set { _row = value; }
    }
    private int _row;

    /// <summary>
    /// Индекс данного узла в пределах родительского
    /// </summary>
    public int Index { get { return _index; } }
    private int _index = -1;

    /// <summary>
    /// True, если данный узел является выбранным
    /// </summary>
    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        if (_isSelected != value)
        {
          if (Tree.IsMyNode(this))
          {
            //_tree.OnSelectionChanging
            if (value)
            {
              if (!_tree.Selection.Contains(this))
                _tree.Selection.Add(this);

              if (_tree.Selection.Count == 1)
                _tree.CurrentNode = this;
            }
            else
              _tree.Selection.Remove(this);
            _tree.UpdateView();
            _tree.OnSelectionChanged();
          }
          _isSelected = value;
        }
      }
    }
    private bool _isSelected;

    /// <summary>
    /// Returns true if all parent nodes of this node are expanded.
    /// </summary>
    internal bool IsVisible
    {
      get
      {
        TreeNodeAdv node = _parent;
        while (node != null)
        {
          if (!node.IsExpanded)
            return false;
          node = node.Parent;
        }
        return true;
      }

    }

    /// <summary>
    /// Возвращает true, если для этого узла нет дочерних узлов
    /// </summary>
    public bool IsLeaf
    {
      get { return _isLeaf; }
      internal set { _isLeaf = value; }
    }
    private bool _isLeaf;

    public bool IsExpandedOnce
    {
      get { return _isExpandedOnce; }
      internal set { _isExpandedOnce = value; }
    }
    private bool _isExpandedOnce;

    /// <summary>
    /// True, если узел развернут.
    /// Установка значения в true разворачивает список дочерних узлов, вызывая <see cref="Expand()"/>, а в false - сворачивает вызовом <see cref="Collapse()"/>.
    /// </summary>
    public bool IsExpanded
    {
      get { return _isExpanded; }
      set
      {
        if (value)
          Expand();
        else
          Collapse();
      }
    }
    private bool _isExpanded;

    internal void AssignIsExpanded(bool value)
    {
      _isExpanded = value;
    }

    /// <summary>
    /// Возвращает родительский узел.
    /// Для узла <see cref="TreeViewAdv.Root"/> возвращает null
    /// </summary>
    public TreeNodeAdv Parent { get { return _parent; } }
    private TreeNodeAdv _parent;

    /// <summary>
    /// Возвращает уровень уровень узла.
    /// Для узла <see cref="TreeViewAdv.Root"/> возвращает 0.
    /// </summary>
    public int Level
    {
      get
      {
        if (_parent == null)
          return 0;
        else
          return _parent.Level + 1;
      }
    }

    /// <summary>
    /// Возвращает предыдущий узел, относящийся к тому же родительскому узлу.
    /// Если текущий узел является первым, то свойство возвращает null
    /// </summary>
    public TreeNodeAdv PreviousNode
    {
      get
      {
        if (_parent != null)
        {
          int index = Index;
          if (index > 0)
            return _parent.Nodes[index - 1];
        }
        return null;
      }
    }

    /// <summary>
    /// Возвращает следующий узел, относящийся к тому же родительскому узлу.
    /// Если текущий узел является последним, то свойство возвращает null
    /// </summary>
    public TreeNodeAdv NextNode
    {
      get
      {
        if (_parent != null)
        {
          int index = Index;
          if (index < _parent.Nodes.Count - 1)
            return _parent.Nodes[index + 1];
        }
        return null;
      }
    }

    internal TreeNodeAdv BottomNode
    {
      get
      {
        TreeNodeAdv parent = this.Parent;
        if (parent != null)
        {
          if (parent.NextNode != null)
            return parent.NextNode;
          else
            return parent.BottomNode;
        }
        return null;
      }
    }

    internal TreeNodeAdv NextVisibleNode
    {
      get
      {
        if (IsExpanded && Nodes.Count > 0)
          return Nodes[0];
        else
        {
          TreeNodeAdv nn = NextNode;
          if (nn != null)
            return nn;
          else
            return BottomNode;
        }
      }
    }

    /// <summary>
    /// Возвращает true, если узел содержит дочерние узлы или это пока неизвестно.
    /// </summary>
    public bool CanExpand
    {
      get
      {
        return (Nodes.Count > 0 || (!IsExpandedOnce && !IsLeaf));
      }
    }

    /// <summary>
    /// Данные для узла (например, <see cref="System.Data.DataRow"/> для <see cref="FreeLibSet.Models.Tree.DataTableTreeModel"/>)
    /// </summary>
    public object Tag { get { return _tag; } }
    private object _tag;

    internal Collection<TreeNodeAdv> Nodes { get { return _nodes; } }
    private Collection<TreeNodeAdv> _nodes;

    /// <summary>
    /// Коллекция дочерних узлов, принадлежащих данному узлу (прямые потомки)
    /// </summary>
    public ReadOnlyCollection<TreeNodeAdv> Children { get { return _children; } }
    private ReadOnlyCollection<TreeNodeAdv> _children;

    internal int? RightBounds
    {
      get { return _rightBounds; }
      set { _rightBounds = value; }
    }
    private int? _rightBounds;

    internal int? Height
    {
      get { return _height; }
      set { _height = value; }
    }
    private int? _height;

    internal bool IsExpandingNow
    {
      get { return _isExpandingNow; }
      set { _isExpandingNow = value; }
    }
    private bool _isExpandingNow;

    public bool AutoExpandOnStructureChanged
    {
      get { return _autoExpandOnStructureChanged; }
      set { _autoExpandOnStructureChanged = value; }
    }
    private bool _autoExpandOnStructureChanged = false;

    #endregion

    #region Методы

    /// <summary>
    /// Возвращает текстовое представление для <see cref="Tag"/>.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (Tag != null)
        return Tag.ToString();
      else
        return base.ToString();
    }

    /// <summary>
    /// Сворачивает узел. Если узел уже свернут, никаких действий не выполняется.
    /// Дочерние узлы не сворачиваются.
    /// </summary>
    public void Collapse()
    {
      if (_isExpanded)
        Collapse(true);
    }

    /// <summary>
    /// Сворачивает узел и все дочерние узлы. Дочерние узлы сворачиваются, даже если текущий узел уже свернут.
    /// </summary>
    public void CollapseAll()
    {
      Collapse(false);
    }

    /// <summary>
    /// Сворачивает узел и, опционально, дочерние узлы
    /// </summary>
    /// <param name="ignoreChildren">true - свернуть только текущий узел (метод <see cref="Collapse()"/>). false - свернуть также и дочерние узлы (метод <see cref="CollapseAll()"/>)</param>
    public void Collapse(bool ignoreChildren)
    {
      SetIsExpanded(false, ignoreChildren);
    }

    /// <summary>
    /// Развернуть узел. Если узел уже развернут, никаких действий не выполняется.
    /// Дочерние узлы не разворачиваются.
    /// </summary>
    public void Expand()
    {
      if (!_isExpanded)
        Expand(true);
    }

    /// <summary>
    /// Развернуть узел и все дочерние узлы
    /// </summary>
    public void ExpandAll()
    {
      Expand(false);
    }

    /// <summary>
    /// Разворачивает узел и, опционально, дочерние узлы
    /// </summary>
    /// <param name="ignoreChildren">true - развернуть только текущий узел (метод <see cref="Expand()"/>). false - развернуть также и дочерние узлы (метод <see cref="ExpandAll()"/>)</param>
    public void Expand(bool ignoreChildren)
    {
      SetIsExpanded(true, ignoreChildren);
    }

    private void SetIsExpanded(bool value, bool ignoreChildren)
    {
      if (Tree == null)
        _isExpanded = value;
      else
        Tree.SetIsExpanded(this, value, ignoreChildren);
    }

    #endregion

    #region ISerializable Members

    private TreeNodeAdv(SerializationInfo info, StreamingContext context)
      : this(null, null)
    {
      int nodesCount = 0;
      nodesCount = info.GetInt32("NodesCount");
      _isExpanded = info.GetBoolean("IsExpanded");
      _tag = info.GetValue("Tag", typeof(object));

      for (int i = 0; i < nodesCount; i++)
      {
        TreeNodeAdv child = (TreeNodeAdv)info.GetValue("Child" + i, typeof(TreeNodeAdv));
        Nodes.Add(child);
      }

    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("IsExpanded", IsExpanded);
      info.AddValue("NodesCount", Nodes.Count);
      if ((Tag != null) && Tag.GetType().IsSerializable)
        info.AddValue("Tag", Tag, Tag.GetType());

      for (int i = 0; i < Nodes.Count; i++)
        info.AddValue("Child" + i, Nodes[i], typeof(TreeNodeAdv));

    }

    #endregion
  }
}
