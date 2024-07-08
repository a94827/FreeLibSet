// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
//
// Original TreeViewAdv component from Aga.Controls.dll
// Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
// http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
// http://sourceforge.net/projects/treeviewadv/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Models.Tree
{
  /// <summary>
  /// Интерфейс модели иерархической структуры для построения деревьев.
  /// Содержит методы для работы с путем и события, возникающие при изменении стуктуры.
  /// </summary>
  public interface ITreeModel
  {
    // Интерфейс ITreeModel взят из Aga.Controls

    #region Методы

    /// <summary>
    /// Возвращает перечислитель для узлов, являющихся дочерними для заданного родителького узла.
    /// Если <paramref name="treePath"/>.IsEmpty=true, возвращает список узлов верхнего уровня.
    /// Перечислимыми объектами являются теги узлов (TreeNodeAdv.Tag), а не объекты TreePath.
    /// Для получения дочерних объектов <see cref="TreePath"/> используйте конструктор TreePath, принимающий родительский узел и дочерний Tag.
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
    /// Событие вызывается при "глобальном" изменении иерархии, начиная с заданного узла,
    /// задаваемого свойством <see cref="TreePathEventArgs.Path"/>. Если задан пустой путь <see cref="TreePath.Empty"/>, то дерево изменено полностью.
    /// </summary>
    event EventHandler<TreePathEventArgs> StructureChanged;

    /// <summary>
    /// Событие вызывается при изменении узлов, когда структура дерева не меняется.
    /// В аргументах события может задаваться свойство <see cref="TreeModelEventArgs.Indices"/> (предпочтительно).
    /// Должно быть задано свойство <see cref="TreeModelEventArgs.Children"/>.
    /// Свойство <see cref="TreePathEventArgs.Path"/> задает родительский узел, в котором находятся изменившееся узлы 
    /// (<see cref="TreePath.Empty"/> задает корневой узел дерева).
    /// Если изменение приводит к перестройке дерева, вместо этого события вызывается <see cref="StructureChanged"/>.
    /// </summary>
    event EventHandler<TreeModelEventArgs> NodesChanged;

    /// <summary>
    /// Событие вызывается при добавлении узлов.
    /// В аргументах события должны быть заданы оба свойства <see cref="TreeModelEventArgs.Children"/> и <see cref="TreeModelEventArgs.Indices"/>.
    /// Свойство <see cref="TreePathEventArgs.Path"/> задает родительский узел, в который добавляются узлы 
    /// (<see cref="TreePath.Empty"/> задает корневой узел дерева).
    /// </summary>
    event EventHandler<TreeModelEventArgs> NodesInserted;

    /// <summary>
    /// Событие вызывается при удалении узлов.
    /// В аргументах события может задаваться свойство <see cref="TreeModelEventArgs.Indices"/> (предпочтительно).
    /// Должно быть задано свойство <see cref="TreeModelEventArgs.Children"/>.
    /// Свойство <see cref="TreePathEventArgs.Path"/> задает родительский узел, из которого удаляются узлы 
    /// (<see cref="TreePath.Empty"/> задает корневой узел дерева).
    /// </summary>
    event EventHandler<TreeModelEventArgs> NodesRemoved;

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
  public struct TreePath : IEquatable<TreePath>
  {
    // Класс TreePath взят из Aga.Controls (с изменениями)

    #region Конструкторы

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
    public object[] FullPath
    {
      get
      {
        if (Object.ReferenceEquals(_FullPath, null))
          return DataTools.EmptyObjects;
        else
          return _FullPath;
      }
    }
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
        if (!Object.ReferenceEquals(_FullPath, null))
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
        if (!Object.ReferenceEquals(_FullPath, null))
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
        if (FullPath.Length < 2)
          return Empty;
        else
        {
          object[] a = new object[FullPath.Length - 1];
          Array.Copy(FullPath, a, a.Length);
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
      get { return Object.ReferenceEquals(_FullPath, null); }
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
        return "Level=" + FullPath.Length.ToString() + ", " + LastNode.ToString();
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
      // Испр. 25.04.2024
      //if (Object.ReferenceEquals(other, null))
      //  return false;
      //if (Object.ReferenceEquals(other, this))
      //  return true;
      if (Object.ReferenceEquals(other.FullPath, this.FullPath))
        return true;

      if (other.FullPath.Length != this.FullPath.Length)
        return false;

      for (int i = 0; i < this.FullPath.Length; i++)
      {
        if (!Object.Equals(this.FullPath[i], other.FullPath[i]))
          return false;
      }

      return true;
    }

    /// <summary>
    /// Сравнение с другим путем
    /// </summary>
    /// <param name="obj">Второй путь</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      if (obj is TreePath)
        return Equals((TreePath)obj);
      else
        return false;
    }

    /// <summary>
    /// Сравнение двух путей
    /// </summary>
    /// <param name="a">Первый путь</param>
    /// <param name="b">Второй путь</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(TreePath a, TreePath b)
    {
      // Испр. 25.04.2024
      //if (Object.ReferenceEquals(a, null))
      //  return Object.ReferenceEquals(b, null);
      //else
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

    /// <summary>
    /// Возвращает true, если текущий путь является прямым потомком (дочерним узлом) заданного.
    /// Если <paramref name="ansector"/> совпадает с текущим путем, возвращается false.
    /// Если текущий путь - пустой (IsEmpty=true), возвращается false.
    /// </summary>
    /// <param name="ansector">Кандидат в предки</param>
    /// <returns>True, если текущий объект - потомок</returns>
    public bool IsChildOf(TreePath ansector)
    {
      object[] aThis = this.FullPath;
      object[] aAnsector = ansector.FullPath;

      if (aThis.Length != (aAnsector.Length + 1))
        return false;

      for (int i = 0; i < aAnsector.Length; i++)
      {
        if (!Object.Equals(aThis[i], aAnsector[i]))
          return false;
      }
      return true;
    }


    /// <summary>
    /// Возвращает true, если текущий путь является прямым предком (родителем) заданного.
    /// Если <paramref name="descendant"/> совпадает с текущим путем, возвращается false.
    /// Если текущий путь - пустой (IsEmpty=true), возвращается true, если <paramref name="descendant"/>.IsEmpty=false.
    /// </summary>
    /// <param name="descendant">Кандидат в потомки</param>
    /// <returns>True, если текущий объект - предок</returns>
    public bool IsParentOf(TreePath descendant)
    {
      object[] aThis = this.FullPath;
      object[] aDescendant = descendant.FullPath;

      if (aThis.Length != (aDescendant.Length - 1))
        return false;

      for (int i = 0; i < aThis.Length; i++)
      {
        if (!Object.Equals(aThis[i], aDescendant[i]))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если текущий путь является потомком заданного.
    /// Если <paramref name="ansector"/> совпадает с текущим путем, возвращается false.
    /// Если текущий путь - пустой (IsEmpty=true), возвращается false.
    /// </summary>
    /// <param name="ansector">Кандидат в предки</param>
    /// <returns>True, если текущий объект - потомок</returns>
    public bool IsDescendantOf(TreePath ansector)
    {
      object[] aThis = this.FullPath;
      object[] aAnsector = ansector.FullPath;

      if (aThis.Length <= aAnsector.Length)
        return false;

      for (int i = 0; i < aAnsector.Length; i++)
      {
        if (!Object.Equals(aThis[i], aAnsector[i]))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если текущий путь является предком заданного.
    /// Если <paramref name="descendant"/> совпадает с текущим путем, возвращается false.
    /// Если текущий путь - пустой (IsEmpty=true), возвращается true, если <paramref name="descendant"/>.IsEmpty=false.
    /// </summary>
    /// <param name="descendant">Кандидат в потомки</param>
    /// <returns>True, если текущий объект - предок</returns>
    public bool IsAncestorOf(TreePath descendant)
    {
      object[] aThis = this.FullPath;
      object[] aDescendant = descendant.FullPath;

      if (aThis.Length >= aDescendant.Length)
        return false;

      for (int i = 0; i < aThis.Length; i++)
      {
        if (!Object.Equals(aThis[i], aDescendant[i]))
          return false;
      }
      return true;
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
      _Path = path;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Путь к узлу дерева.
    /// Для события <see cref="ITreeModel.StructureChanged"/> задает узел, дочерние узлы которого полностью изменились.
    /// Если при этом задан пустой путь, предполагается полное перестроение дерева.
    /// Для событий <see cref="ITreeModel.NodesChanged"/>, <see cref="ITreeModel.NodesInserted"/> и <see cref="ITreeModel.NodesRemoved"/> задает корневой узел, 
    /// дочерние узлы которого поменялись. Пустой путь означает изменение узлов верхнего уровня.
    /// </summary>
    public TreePath Path { get { return _Path; } }
    private TreePath _Path;

    /// <summary>
    /// Статический экземпляр объекта с пустым <see cref="TreePath"/>
    /// </summary>
    public static readonly new TreePathEventArgs Empty = new TreePathEventArgs();

    #endregion
  }

  /// <summary>
  /// Аргумент событий <see cref="ITreeModel.NodesChanged"/>, <see cref="ITreeModel.NodesInserted"/> и <see cref="ITreeModel.NodesRemoved"/>, 
  /// генерируемых моделью иерархической структуры, реализующей интерфейс <see cref="ITreeModel"/>.
  /// </summary>
  public class TreeModelEventArgs : TreePathEventArgs
  {
    // Класс TreePathEventArgs взят из Aga.Controls

    #region Конструкторы

    /// <summary>
    /// Создает новый объект без индексов узлов.
    /// Этот конструктор может использоваться для событий <see cref="ITreeModel.NodesChanged"/> и <see cref="ITreeModel.NodesRemoved"/>, когда идексы изменяемых/удаляемых узлов неизвестны.
    /// </summary>
    /// <param name="parent">Path to a parent node</param>
    /// <param name="children">Child nodes. Cannot be null</param>
    public TreeModelEventArgs(TreePath parent, object[] children)
      : this(parent, null, children)
    {
    }

    /// <summary>
    /// Создает новый объект с индексами узлов.
    /// Этот конструктор должен использоваться для события <see cref="ITreeModel.NodesInserted"/> и может использоваться для остальных событий.
    /// </summary>
    /// <param name="parent">Path to a parent node</param>
    /// <param name="indices">Indices of children in parent nodes collection. Can be null except for <see cref="ITreeModel.NodesInserted"/> event</param>
    /// <param name="children">Child nodes. Cannot be null</param>
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
    /// Дочерние узлы задаются относительно родительского узла, задаваемого свойством <see cref="TreePathEventArgs.Path"/>.
    /// </summary>
    public object[] Children { get { return _Children; } }
    private object[] _Children;

    /// <summary>
    /// Индексы дочерних узлов в дереве.
    /// Массив соответствует свойству <see cref="Children"/>.
    /// Может быть null, если индексы узлов не определены (кроме события <see cref="ITreeModel.NodesInserted"/>, где наличие индексов обязательно).
    /// </summary>
    public int[] Indices { get { return _Indices; } }
    private int[] _Indices;

    #endregion
  }

}
