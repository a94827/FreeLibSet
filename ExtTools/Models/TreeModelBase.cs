// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
//
// Original TreeViewAdv component from Aga.Controls.dll
// Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
// http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
// http://sourceforge.net/projects/treeviewadv/

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Models.Tree
{
  /// <summary>
  /// Интерфейс модели иерархической структуры для построения деревьев.
  /// Содержит методы для работы с путем и события, возникающие при изменении стуктуры
  /// </summary>
  public interface ITreeModel
  {
    // Интерфейс ITreeModel взят из Aga.Controls

    #region Методы

    /// <summary>
    /// Возвращает перечислитель для узлов, являющихся дочерними для заданного родителького узла.
    /// Если <paramref name="treePath"/>.IsEmpty=true, возвращает список узлов верхнего уровня.
    /// Перечислимыми объектами являются теги узлов (TreeNodeAdv.Tag), а не объекты TreePath.
    /// Для получения дочерних объектов TreePath используйте конструктор TreePath, принимающий родительский узел и дочерний Tag.
    /// </summary>
    /// <param name="treePath">Путь к родительскому узлу</param>
    /// <returns>Перечислитель</returns>
    IEnumerable GetChildren(TreePath treePath);

    /// <summary>
    /// Возвращает признак, что данный узел дерева является конечным, то есть не может содержать дочерних узлов
    /// </summary>
    /// <param name="treePath">Путь к проверяемому узлу</param>
    /// <returns>true, если узел является "листом"</returns>
    bool IsLeaf(TreePath treePath);

    #endregion

    #region События

    /// <summary>
    /// Событие вызывается при изменении узлов, когда структура дерева не меняется.
    /// Если изменение приводит к перестройке дерева, вместо этого события вызывается StructureChanged.
    /// </summary>
    event EventHandler<TreeModelEventArgs> NodesChanged;

    /// <summary>
    /// Событие вызывается при добавлении узлов
    /// </summary>
    event EventHandler<TreeModelEventArgs> NodesInserted;

    /// <summary>
    /// Событие вызывается при удалении узлов
    /// </summary>
    event EventHandler<TreeModelEventArgs> NodesRemoved;

    /// <summary>
    /// Событие вызывается при "глобальном" изменении иерархии, начиная с заданного узла,
    /// задаваемого свойством TreePathEventArgs.Path. Если задан пустой путь, то дерево изменено полностью.
    /// </summary>
    event EventHandler<TreePathEventArgs> StructureChanged;

    #endregion
  }

  /// <summary>
  /// Расширение базовой модели дерева методами, позволяющими вызвать события обновления
  /// </summary>
  public interface IRefreshableTreeModel : ITreeModel
  {
    #region Методы

    /// <summary>
    /// Вызывает событие NodesChanged для заданного узла.
    /// Дочерние узлы не обновляются.
    /// </summary>
    /// <param name="path">Путь к узлу, который требуется обновить. Не может быть пустым</param>
    void RefreshNode(TreePath path);

    /// <summary>
    /// Обновление всей структуры. Вызывает событие StructureChanged.
    /// </summary>
    void Refresh();

    #endregion
  }


  /// <summary>
  /// Путь в модели дерева.
  /// Содержит массив объектов (тегов), образующих иерархию.
  /// Тип объектов определяется моделью. В пределах одного дерева могут быть разнотипные объекты.
  /// Объекты должны быть сравниваемыми, то есть в пределах одного родительского (или корневого) узла разным дочерним узлам должны соответствовать теги, для которых Object.Equals() возвращает false.
  /// Это ограничение не распространяется на все узлы дерева: у узлов, относящихся к разным родителям, могут быть одинаковые теги
  /// Ссылки на саму модель ITreeModel в объекте TreePath нет.
  /// Является объектом "однократной записи".
  /// </summary>
  public class TreePath : IEquatable<TreePath>
  {
    // Класс TreePath взят из Aga.Controls (с изменениями)

    #region Конструкторы

    /// <summary>
    /// Создает пустой путь.
    /// Используйте статическое свойство Empty.
    /// </summary>
    public TreePath()
    {
      _FullPath = new object[0];
    }

    /// <summary>
    /// Создает путь с единственным узлом
    /// </summary>
    /// <param name="node">Объект узла. Тип объекта определяется моделью</param>
    public TreePath(object node)
    {
      _FullPath = new object[] { node };
    }

    /// <summary>
    /// Создает пусть из массива узлов.
    /// </summary>
    /// <param name="path">Объекты узлов. Тип объектов определяется моделью</param>
    public TreePath(object[] path)
    {
      _FullPath = path;
    }

    /// <summary>
    /// Создает путь, расширяющий существующий на один уровень
    /// </summary>
    /// <param name="parent">Родительский путь</param>
    /// <param name="node">Добавляемый узел. Тип объектов определяется моделью. Этот узел будет возвращаться свойством LastNode</param>
    public TreePath(TreePath parent, object node)
    {
      _FullPath = new object[parent.FullPath.Length + 1];
      for (int i = 0; i < _FullPath.Length - 1; i++)
        _FullPath[i] = parent.FullPath[i];
      _FullPath[_FullPath.Length - 1] = node;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает путь в виде массива объектов.
    /// Тип объектов зависит от модели.
    /// </summary>
    public object[] FullPath { get { return _FullPath; } }
    private readonly object[] _FullPath;

    /// <summary>
    /// Возвращает первый узел пути (объект верхнего уровня)
    /// Тип объекта зависит от модели.
    /// Возвращает null, если путь пустой.
    /// </summary>
    public object FirstNode
    {
      get
      {
        if (_FullPath.Length > 0)
          return _FullPath[0];
        else
          return null;
      }
    }

    /// <summary>
    /// Возвращает последний узел пути (объект нижнего уровня)
    /// Тип объекта зависит от модели.
    /// Возвращает null, если путь пустой.
    /// </summary>
    public object LastNode
    {
      get
      {
        if (_FullPath.Length > 0)
          return _FullPath[_FullPath.Length - 1];
        else
          return null;
      }
    }

    /// <summary>
    /// Возвращает путь, содержащий на один уровень меньше, чем текущий.
    /// Если текущий путь пустой, то также возвращается пустой путь, 
    /// чтобы не генерировать исключение.
    /// </summary>
    public TreePath Parent
    {
      // Этого свойства не было в оригинале в TreeViewAdv

      get
      {
        if (_FullPath.Length < 2)
          return Empty;
        else
        {
          object[] a = new object[_FullPath.Length - 1];
          Array.Copy(_FullPath, a, a.Length);
          return new TreePath(a);
        }
      }
    }

    /// <summary>
    /// Возвращает true, если путь пустой (не содержит объектов)
    /// </summary>
    /// <returns>FullPath.Length=0</returns>
    public bool IsEmpty
    {
      get { return (_FullPath.Length == 0); }
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (IsEmpty)
        return "Empty";
      else
        return "Level=" + _FullPath.Length.ToString() + ", " + LastNode.ToString();
    }

    #endregion

    #region Сравнение путей

    /// <summary>
    /// Сравнение с другим путем
    /// </summary>
    /// <param name="other">Второй путь</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(TreePath other)
    {
      if (Object.ReferenceEquals(other, null))
        return false;

      if (Object.ReferenceEquals(other, this))
        return true;

      if (other._FullPath.Length != this._FullPath.Length)
        return false;

      for (int i = 0; i < this._FullPath.Length; i++)
      {
        if (!Object.Equals(this._FullPath[i], other._FullPath[i]))
          return false;
      }

      return true;
    }

    /// <summary>
    /// Сравнение с другим путем
    /// </summary>
    /// <param name="other">Второй путь</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      TreePath other = obj as TreePath;
      return Equals(other);
    }

    /// <summary>
    /// Сравнение двух путей
    /// </summary>
    /// <param name="a">Первый путь</param>
    /// <param name="b">Второй путь</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(TreePath a, TreePath b)
    {
      if (Object.ReferenceEquals(a, null))
        return Object.ReferenceEquals(b, null);
      else
        return a.Equals(b);
    }

    /// <summary>
    /// Сравнение двух путей
    /// </summary>
    /// <param name="a">Первый путь</param>
    /// <param name="b">Второй путь</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(TreePath a, TreePath b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Хэш-код.
    /// Объекты TreePath обычно не хранятся в коллекциях
    /// </summary>
    /// <returns>Хэш-код</returns>
    public override int GetHashCode()
    {
      if (IsEmpty)
        return 0;
      else
        return LastNode.GetHashCode();
    }

    /// <summary>
    /// Находит часть пути, являющегося общим для обоих путей.
    /// Если пути не имеют общей части, возвращается TreePath.Empty.
    /// </summary>
    /// <param name="a">Первый путь</param>
    /// <param name="b">Второй путь</param>
    /// <returns>Общий путь</returns>
    public static TreePath GetCommonPath(TreePath a, TreePath b)
    {
      int n = Math.Min(a.FullPath.Length, b.FullPath.Length);
      int common = 0;
      for (int i = 0; i < n; i++)
      {
        if (Object.Equals(a.FullPath[i], b.FullPath[i]))
          common++;
        else
          break;
      }
      if (common == 0)
        return Empty;

      // Стараемся избежать создания нового массива
      if (common == a.FullPath.Length)
        return a;
      if (common == b.FullPath.Length)
        return b;

      // Создаем новый объект
      object[] path = new object[common];
      Array.Copy(a.FullPath, path, common);
      return new TreePath(path);
    }

    #endregion

    #region Статические свойства

    /// <summary>
    /// Пустой путь.
    /// </summary>
    //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly TreePath Empty = new TreePath();

    #endregion
  }

  /// <summary>
  /// Аргументы события ITreeModel.StructureChanged.
  /// Также является базовым классом для TreeModelEventArgs
  /// </summary>
  public class TreePathEventArgs : EventArgs
  {
    // Класс TreePathEventArgs взят из Aga.Controls

    #region Конструкторы

    /// <summary>
    /// Создает объект с пустым путем
    /// </summary>
    public TreePathEventArgs()
    {
      _Path = new TreePath();
    }

    /// <summary>
    /// Создает объект с заданным путем
    /// </summary>
    /// <param name="path">Путь</param>
    public TreePathEventArgs(TreePath path)
    {
      if (path == null)
        throw new ArgumentNullException("path");

      _Path = path;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Путь к узлу дерева.
    /// Для события ITreeModel.StructureChanged задает узел, дочерние узлы которого полностью изменились.
    /// Если при этом задан пустой путь, предполагается полное перестроение дерева.
    /// Для событий NodesChanged, NodesInserted и NodesRemoved задает корневой узел, дочерние узлы
    /// которого поменялись. Пустой путь означает изменение узлов верхнего уровня
    /// </summary>
    public TreePath Path { get { return _Path; } }
    private TreePath _Path;

    /// <summary>
    /// Статический экземпляр объекта с пустым TreePath
    /// </summary>
    public static readonly new TreePathEventArgs Empty = new TreePathEventArgs();

    #endregion
  }

  /// <summary>
  /// Аргумент событий ITreeModel.NodesChanged, NodesInserted и NodesRemoved, 
  /// генерируемых моделью иерархической структуры, реализующей интерфейс ITreeModel
  /// </summary>
  public class TreeModelEventArgs : TreePathEventArgs
  {
    // Класс TreePathEventArgs взят из Aga.Controls

    #region Конструкторы

    /// <summary>
    /// Создает новый объект без индексов узлов
    /// </summary>
    /// <param name="parent">Path to a parent node</param>
    /// <param name="children">Child nodes</param>
    public TreeModelEventArgs(TreePath parent, object[] children)
      : this(parent, null, children)
    {
    }

    /// <summary>
    /// Создает новый объект с индексами узлов
    /// </summary>
    /// <param name="parent">Path to a parent node</param>
    /// <param name="indices">Indices of children in parent nodes collection</param>
    /// <param name="children">Child nodes</param>
    public TreeModelEventArgs(TreePath parent, int[] indices, object[] children)
      : base(parent)
    {
      if (children == null)
        throw new ArgumentNullException("children");

      if (indices != null && indices.Length != children.Length)
        throw new ArgumentException("indices and children arrays must have the same length");

      _Indices = indices;
      _Children = children;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объекты дочерних узлов.
    /// Тип объектов зависит от модели.
    /// Дочерние узлы задаются относительно родительского узла, задаваемого свойством TreePathEventArgs.Path
    /// </summary>
    public object[] Children { get { return _Children; } }
    private object[] _Children;

    /// <summary>
    /// Индексы дочерних узлов в дереве.
    /// Массив соответствует свойству Children.
    /// Может быть null, если индексы узлов не определены.
    /// </summary>
    public int[] Indices { get { return _Indices; } }
    private int[] _Indices;

    #endregion
  }

  /// <summary>
  /// Абстрактный класс, реализующий интерфейс ITreeModel
  /// </summary>
  public abstract class TreeModelBase : IRefreshableTreeModel
  {
    #region Абстрактные методы

    /// <summary>
    /// Возвращает перечислитель для узлов, являющихся дочерними для заданного родителького узла.
    /// Если treePath.IsEmpty=true, возвращает список узлов верхнего уровня
    /// </summary>
    /// <param name="treePath">Путь к родительскому узлу</param>
    /// <returns>Перечислитель</returns>
    public abstract System.Collections.IEnumerable GetChildren(TreePath treePath);

    /// <summary>
    /// Возвращает признак, что данный узел дерева является конечным, то есть не может содержать дочерних узлов
    /// </summary>
    /// <param name="treePath">Путь к проверяемому узлу</param>
    /// <returns>true, если узел является "листом"</returns>
    public abstract bool IsLeaf(TreePath treePath);

    #endregion

    #region События

    /// <summary>
    /// Событие вызывается при изменении узлов
    /// </summary>
    public event EventHandler<TreeModelEventArgs> NodesChanged;

    /// <summary>
    /// Вызов обработчика события NodesChanged
    /// </summary>
    /// <param name="args">Аргументы, передаваемыме обработчику</param>
    protected void OnNodesChanged(TreeModelEventArgs args)
    {
      if (NodesChanged != null)
        NodesChanged(this, args);
    }

    /// <summary>
    /// Событие вызывается при "глобальном" изменении дерева, начиная с узла TreePathEventArgs.Path
    /// </summary>
    public event EventHandler<TreePathEventArgs> StructureChanged;

    /// <summary>
    /// Вызов обработчика события StructureChanged
    /// </summary>
    /// <param name="args">Аргументы, передаваемыме обработчику</param>
    protected void OnStructureChanged(TreePathEventArgs args)
    {
      if (StructureChanged != null)
        StructureChanged(this, args);
    }

    /// <summary>
    /// Событие вызывается при добавлении узлов
    /// </summary>
    public event EventHandler<TreeModelEventArgs> NodesInserted;

    /// <summary>
    /// Вызов обработчика события NodesInserted
    /// </summary>
    /// <param name="args">Аргументы, передаваемыме обработчику</param>
    protected void OnNodesInserted(TreeModelEventArgs args)
    {
      if (NodesInserted != null)
        NodesInserted(this, args);
    }

    /// <summary>
    /// Событие вызывается при удалении узлов
    /// </summary>
    public event EventHandler<TreeModelEventArgs> NodesRemoved;

    /// <summary>
    /// Вызов обработчика события NodesRemoved
    /// </summary>
    /// <param name="args">Аргументы, передаваемыме обработчику</param>
    protected void OnNodesRemoved(TreeModelEventArgs args)
    {
      if (NodesRemoved != null)
        NodesRemoved(this, args);
    }

    /// <summary>
    /// Полное обновление модели
    /// </summary>
    public virtual void Refresh()
    {
      OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
    }

    /// <summary>
    /// Вызывает событие NodesChanged для заданного узла.
    /// Дочерние узлы не обновляются.
    /// </summary>
    /// <param name="path">Путь к узлу, который требуется обновить. Не может быть пустым</param>
    public void RefreshNode(TreePath path)
    {
      if (path == null)
        throw new ArgumentNullException("path");
      if (path.IsEmpty)
        throw new ArgumentException("Узел должен быть задан", "path");
      TreeModelEventArgs args = new TreeModelEventArgs(path.Parent, new object[] { path.LastNode });
      OnNodesChanged(args);
    }

    /// <summary>
    /// Определяет, является ли один из узлов <paramref name="path1"/> и <paramref name="path2"/> частью другого.
    /// Если да, то вызывается OnStructureChanged() для узла, который ближе к корню.
    /// Иначе дважды вызывается OnStructureChanged() для обоих узлов.
    /// Если любой из узлов является пустым, то выполняется полное обновление.
    /// </summary>
    /// <param name="path1">Первый путь</param>
    /// <param name="path2">Второй путь</param>
    protected void CallStructureChanged(TreePath path1, TreePath path2)
    {
      if (path1.IsEmpty || path2.IsEmpty)
      {
        OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
        return;
      }

      TreePath commonPath = TreePath.GetCommonPath(path1, path2);
      if (commonPath.FullPath.Length == path1.FullPath.Length)
        OnStructureChanged(new TreePathEventArgs(path1));
      else if (commonPath.FullPath.Length == path2.FullPath.Length)
        OnStructureChanged(new TreePathEventArgs(path2));
      else
      {
        OnStructureChanged(new TreePathEventArgs(path1));
        OnStructureChanged(new TreePathEventArgs(path2));
      }
    }

    #endregion
  }

  /// <summary>
  /// Добавление сортировки узлов дерева в пределах уровня иерархии для произвольной модели.
  /// Определяет метод GetChildren(), остальные вызовы переадресуются базовой модели.
  /// </summary>
  public class SortedTreeModel : TreeModelBase
  {
    #region Конструктор

    /// <summary>
    /// Создает модель
    /// </summary>
    /// <param name="innerModel">Базовая модель, которой переадресуются все вызовы</param>
    public SortedTreeModel(ITreeModel innerModel)
    {
      _InnerModel = innerModel;
      _InnerModel.NodesChanged += new EventHandler<TreeModelEventArgs>(InnerModel_NodesChanged);
      _InnerModel.NodesInserted += new EventHandler<TreeModelEventArgs>(InnerModel_NodesInserted);
      _InnerModel.NodesRemoved += new EventHandler<TreeModelEventArgs>(InnerModel_NodesRemoved);
      _InnerModel.StructureChanged += new EventHandler<TreePathEventArgs>(InnerModel_StructureChanged);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Базовая модель, которой переадресуются все вызовы.
    /// Задается в конструкторе.
    /// </summary>
    public ITreeModel InnerModel { get { return _InnerModel; } }
    private ITreeModel _InnerModel;

    /// <summary>
    /// Интерфейс сортировки узлов.
    /// Пока не задан (по умолчанию), узлы не сортируются и возвращаются в том порядке,
    /// в котором определены в базовой модели.
    /// Свойство должно быть установлено, иначе использование класса SortedTreeModel не имеет смысла
    /// </summary>
    public IComparer Comparer
    {
      get { return _Comparer; }
      set
      {
        _Comparer = value;
        OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
      }
    }
    private IComparer _Comparer;

    #endregion

    #region Методы ITreeModel

    /// <summary>
    /// Возвращает перечислимый объект для дочерних узлов.
    /// Если свойство Comparer установлено, список узлов извлекается из базовой модели и сортируются
    /// с помощью интерфейса IComparer. Отсортированный список возвращается.
    /// Если свойство Comparer не установлено, то вызов передается базовой модели для получения перечислимого объекта.
    /// </summary>
    /// <param name="treePath">Путь к узлу, для которого извлекаются дочерние узлы</param>
    /// <returns>Перечислимый объект</returns>
    public override IEnumerable GetChildren(TreePath treePath)
    {
      if (Comparer != null)
      {
        ArrayList list = new ArrayList();
        IEnumerable res = InnerModel.GetChildren(treePath);
        if (res != null)
        {
          foreach (object obj in res)
            list.Add(obj);
          list.Sort(Comparer);
          return list;
        }
        else
          return null;
      }
      else
        return InnerModel.GetChildren(treePath);
    }

    /// <summary>
    /// Вызов передается базовой модели.
    /// </summary>
    /// <param name="treePath">Путь к узлу</param>
    /// <returns>true, если узел является "листом" и не содержит дочерних узлов</returns>
    public override bool IsLeaf(TreePath treePath)
    {
      return InnerModel.IsLeaf(treePath);
    }

    #endregion

    #region Передача событий от базовой модели

    void InnerModel_StructureChanged(object sender, TreePathEventArgs args)
    {
      OnStructureChanged(args);
    }

    void InnerModel_NodesRemoved(object sender, TreeModelEventArgs args)
    {
      OnStructureChanged(new TreePathEventArgs(args.Path));
    }

    void InnerModel_NodesInserted(object sender, TreeModelEventArgs args)
    {
      OnStructureChanged(new TreePathEventArgs(args.Path));
    }

    void InnerModel_NodesChanged(object sender, TreeModelEventArgs args)
    {
      OnStructureChanged(new TreePathEventArgs(args.Path));
    }

    #endregion
  }

}
