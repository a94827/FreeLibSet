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
  /// Расширение базовой модели дерева методами, позволяющими вызвать события обновления
  /// </summary>
  public interface IRefreshableTreeModel : ITreeModel
  {
    #region Методы

    /// <summary>
    /// Вызывает событие <see cref="ITreeModel.NodesChanged"/>  для заданного узла.
    /// Дочерние узлы не обновляются.
    /// </summary>
    /// <param name="path">Путь к узлу, который требуется обновить. Не может быть пустым</param>
    void RefreshNode(TreePath path);

    /// <summary>
    /// Обновление всей структуры. Вызывает событие <see cref="ITreeModel.StructureChanged"/>, передавая путь <see cref="TreePath.Empty"/>.
    /// </summary>
    void Refresh();

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
    /// Если <paramref name="treePath"/>.IsEmpty=true, возвращает список узлов верхнего уровня.
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
    /// <seealso cref="ITreeModel.NodesChanged"/>
    public event EventHandler<TreeModelEventArgs> NodesChanged;

    /// <summary>
    /// Вызов обработчика события <seealso cref="NodesChanged"/>
    /// </summary>
    /// <param name="args">Аргументы, передаваемыме обработчику</param>
    protected void OnNodesChanged(TreeModelEventArgs args)
    {
#if DEBUG
#endif
      if (NodesChanged != null)
        NodesChanged(this, args);
    }

    /// <summary>
    /// Событие вызывается при "глобальном" изменении дерева, начиная с узла <see cref="TreePathEventArgs.Path"/>
    /// </summary>
    /// <seealso cref="ITreeModel.StructureChanged"/>
    public event EventHandler<TreePathEventArgs> StructureChanged;

    /// <summary>
    /// Вызов обработчика события <seealso cref="StructureChanged"/>
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
    /// <seealso cref="ITreeModel.NodesInserted"/>
    public event EventHandler<TreeModelEventArgs> NodesInserted;

    /// <summary>
    /// Вызов обработчика события <seealso cref="NodesInserted"/>
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
    /// <seealso cref="ITreeModel.NodesRemoved"/>
    public event EventHandler<TreeModelEventArgs> NodesRemoved;

    /// <summary>
    /// Вызов обработчика события <seealso cref="NodesRemoved"/>
    /// </summary>
    /// <param name="args">Аргументы, передаваемыме обработчику</param>
    protected void OnNodesRemoved(TreeModelEventArgs args)
    {
      if (NodesRemoved != null)
        NodesRemoved(this, args);
    }

    /// <summary>
    /// Полное обновление модели.
    /// Вызывает <seealso cref="OnStructureChanged(TreePathEventArgs)"/>
    /// </summary>
    public virtual void Refresh()
    {
      OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
    }

    /// <summary>
    /// Вызывает событие <seealso cref="NodesChanged"/> для заданного узла.
    /// Дочерние узлы не обновляются.
    /// </summary>
    /// <param name="path">Путь к узлу, который требуется обновить. Не может быть пустым</param>
    public void RefreshNode(TreePath path)
    {
      if (path.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("path");

      TreeModelEventArgs args = new TreeModelEventArgs(path.Parent, new object[] { path.LastNode });
      OnNodesChanged(args);
    }

    /// <summary>
    /// Определяет, является ли один из узлов <paramref name="path1"/> и <paramref name="path2"/> частью другого.
    /// Если да, то вызывается <see cref="OnStructureChanged(TreePathEventArgs)"/> для узла, который ближе к корню.
    /// Иначе дважды вызывается <see cref="OnStructureChanged(TreePathEventArgs)"/> для обоих узлов.
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
  /// Определяет метод <see cref="TreeModelBase.GetChildren(TreePath)"/>, остальные вызовы переадресуются базовой модели.
  /// Передает события <seealso cref="ITreeModel"/> из базовой модели в текущий объект, учитывая несоответствие индексов узов в свойстве <seealso cref="TreeModelEventArgs.Indices"/>.
  /// </summary>
  public class SortedTreeModel : TreeModelBase
  {
    #region Конструктор

    /// <summary>
    /// Создает модель
    /// </summary>
    /// <param name="innerModel">Базовая модель, которой переадресуются все вызовы. Не может быть null</param>
    public SortedTreeModel(ITreeModel innerModel)
    {
      if (innerModel == null)
        throw new ArgumentNullException("innerModel");

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
    private readonly ITreeModel _InnerModel;

    /// <summary>
    /// Интерфейс сортировки узлов.
    /// Пока не задан (по умолчанию), узлы не сортируются и возвращаются в том порядке,
    /// в котором определены в базовой модели.
    /// Свойство должно быть установлено, иначе использование класса <see cref="SortedTreeModel"/> не имеет смысла.
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
    /// Если свойство <seealso cref="Comparer"/> установлено, список узлов извлекается из базовой модели и сортируются
    /// с помощью интерфейса <seealso cref="IComparer"/>. Отсортированный список возвращается.
    /// Если свойство <seealso cref="Comparer"/> не установлено, то вызов передается базовой модели для получения перечислимого объекта.
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

    // При добавлении/изменении/удалении узла в базовой модели, если передаются индексы узлов, то они недействительны после сортировки.
    // Такие события заменяются на OnStructureChanged()

    void InnerModel_NodesInserted(object sender, TreeModelEventArgs args)
    {
      // Наличие индексов узлов события является обязательным.
      // Всегда заменяем событие
      OnStructureChanged(new TreePathEventArgs(args.Path));
    }

    void InnerModel_NodesRemoved(object sender, TreeModelEventArgs args)
    {
      if (args.Indices != null)
      {
        if (args.Children == null)
          OnStructureChanged(new TreePathEventArgs(args.Path));
        else
        {
          TreeModelEventArgs args2 = new TreeModelEventArgs(args.Path, args.Children);
          OnNodesRemoved(args2); // 19.07.2023
        }
      }
      else
        OnNodesRemoved(args);
    }

    void InnerModel_NodesChanged(object sender, TreeModelEventArgs args)
    {
      //if (args.Indices != null)
      //{
      //  if (args.Children == null)
      //    OnStructureChanged(new TreePathEventArgs(args.Path));
      //  else
      //  {
      //    TreeModelEventArgs args2 = new TreeModelEventArgs(args.Path, args.Children);
      //    OnNodesChanged(args2);
      //  }
      //}
      //else
      //  OnNodesChanged(args);

      // Для узла могло измениться свойство/поле, по которому выполняется сортировка.
      // Нет способа узнать, так ли это.
      OnStructureChanged(new TreePathEventArgs(args.Path));
    }

    #endregion
  }

  /// <summary>
  /// Рекурсивный перечислитель по модели дерева.
  /// Элементами перечисления являются пути <seealso cref="TreePath"/>.
  /// Этот перечислитель следует применять с осторожностью, так как могут быть иерархические модели с бесконечным вложением узлов.
  /// </summary>
  public struct TreePathEnumerable : IEnumerable<TreePath>
  {
    #region Конструкторы

    /// <summary>
    /// Создает перечислитель, который перебирает все пути, которые являются дочерними (включая наследников) по отношению к заданному.
    /// Сам путь <paramref name="parentPath"/> не входит в перечисление.
    /// </summary>
    /// <param name="model">Модель</param>
    /// <param name="parentPath">Корневой узел</param>
    public TreePathEnumerable(ITreeModel model, TreePath parentPath)
    {
      if (model == null)
        throw new ArgumentNullException("model");
      _Model = model;
      _ParentPath = parentPath;
    }

    /// <summary>
    /// Создает перечислитель, который перебирает все пути модели дерева.
    /// </summary>
    /// <param name="model">Модель</param>
    public TreePathEnumerable(ITreeModel model)
      : this(model, TreePath.Empty)
    {
    }

    #endregion

    #region Поля

    private readonly ITreeModel _Model;
    private readonly TreePath _ParentPath;

    #endregion

    /// <summary>
    /// Состояние для одного уровня
    /// Должно быть классом, а не структурой, чтобы можно было обновлять поле CurrentIndex.
    /// </summary>
    private class OneLevelInfo
    {
      #region Поля

      /// <summary>
      /// Узлы, возвращаемые ITreeModel.GetChildren().
      /// Можно было бы просто хранить ITreeModel.GetChildren().GetEnumerator(), но тогда придется выполнять вызов Dispose()
      /// </summary>
      public object[] Nodes;

      public int CurrentIndex;

      public TreePath ParentPath;

      #endregion
    }

    /// <summary>
    /// Структура перечислителя
    /// </summary>
    public struct Enumerator : IEnumerator<TreePath>
    {
      #region Конструктор

      /// <summary>
      /// Конструктор перечислителя
      /// </summary>
      /// <param name="model">Модель</param>
      /// <param name="parentPath">Корневой узел</param>
      public Enumerator(ITreeModel model, TreePath parentPath)
      {
        _Model = model;
        _ParentPath = parentPath;
        _Stack = new Stack<OneLevelInfo>();
        _GetChildRequired = false; // чтобы компилятор не ругался
      }

      #endregion

      #region Поля

      private readonly ITreeModel _Model;
      private readonly TreePath _ParentPath;
      private /*readonly*/ Stack<OneLevelInfo> _Stack;

      /// <summary>
      /// Переключение между переходом к следующему или к дочернему узлу.
      /// Сохраняет состояние между вызовами GetNext.
      /// Если true, то очередной вызов должен вызвать ITreeModel.GetChildren().
      /// Если false, то требуется переход к следующему узлу в пределах текущего
      /// </summary>
      private bool _GetChildRequired;

      #endregion

      #region IEnumerator<TreePath> Members

      /// <summary>
      /// Переход к следующему узлу дерева
      /// </summary>
      /// <returns>true, если есть очередной узел</returns>
      public bool MoveNext()
      {
        if (_Stack == null)
          return false; // уже закончили перечислять

        if (_Stack.Count == 0)
          PushNodes(_ParentPath);

        while (_Stack.Count > 0)
        {
          if (_GetChildRequired)
            PushNodes(Current);

          OneLevelInfo level = _Stack.Peek();

          level.CurrentIndex++;
          if (level.CurrentIndex >= level.Nodes.Length)
            _Stack.Pop();
          else
          {
            _GetChildRequired = true;
            return true;
          }
        }
        
        // Перебор закончен
        _Stack = null;
        return false;
      }

      private void PushNodes(TreePath parentPath)
      {
        OneLevelInfo level = new OneLevelInfo();
        level.ParentPath = parentPath;
        level.CurrentIndex = -1;

        // Для TreePath.Empty нельзя вызывать IsLeaf().
        bool isLeaf;
        if (parentPath.IsEmpty)
          isLeaf = false;
        else
          isLeaf = _Model.IsLeaf(parentPath);

        if (isLeaf)
          level.Nodes = EmptyArray<object>.Empty;
        else
          level.Nodes = ArrayTools.CreateObjectArray(_Model.GetChildren(parentPath));
        _Stack.Push(level);
        _GetChildRequired = false;
      }

      /// <summary>
      /// Возвращает очередной узел
      /// </summary>
      public TreePath Current
      {
        get
        {
          if (_Stack == null)
            return TreePath.Empty; // можно было бы выбросить исключение

          OneLevelInfo level = _Stack.Peek();
          return new TreePath(level.ParentPath, level.Nodes[level.CurrentIndex]);
        }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object IEnumerator.Current
      {
        get { return Current; }
      }

      void IEnumerator.Reset()
      {
        _Stack = new Stack<OneLevelInfo>();
      }

      #endregion
    }

    #region IEnumerable<TreePath> Members

    /// <summary>
    /// Создает перечислитель
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(_Model, _ParentPath);
    }

    IEnumerator<TreePath> IEnumerable<TreePath>.GetEnumerator()
    {
      return new Enumerator(_Model, _ParentPath);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(_Model, _ParentPath);
    }

    #endregion
  }
}
