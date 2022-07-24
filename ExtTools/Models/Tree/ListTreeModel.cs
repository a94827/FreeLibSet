// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Models.Tree
{

  /// <summary>
  /// Простейшая модель "дерева", содержащая узлы только одного уровня.
  /// Узлы задаются списком IList.
  /// Содержит методы для добавления, удаления и изменения узлов.
  /// Если список предназначен только для просмотра, используйте класс SimpleListTreeModel
  /// </summary>
  public class ListTreeModel : TreeModelBase
  {
    #region Конструкторы

    /// <summary>
    /// Создает модель, первоначально не содержащую ни одного узла
    /// </summary>
    public ListTreeModel()
    {
      _list = new List<object>();
    }

    /// <summary>
    /// Создает модель с заданным списком узлов.
    /// Передаваемый список <paramref name="list"/> является "рабочим".
    /// Методы изменения узлов в ListModel будут вносить изменения в этот список.
    /// </summary>
    /// <param name="list">Список</param>
    public ListTreeModel(IList list)
    {
      _list = list;
    }
    private IList _list;

    #endregion

    #region Методы ITreeModel

    /// <summary>
    /// Возвращает рабочий список в качестве перечислителя
    /// </summary>
    /// <param name="treePath">Игнорируется</param>
    /// <returns>Список</returns>
    public override IEnumerable GetChildren(TreePath treePath)
    {
      return _list;
    }

    /// <summary>
    /// Возвращает true, так как "дерево" не содержит узлов, кроме верхнего уровня.
    /// </summary>
    /// <param name="treePath">Игнорируется</param>
    /// <returns>true</returns>
    public override bool IsLeaf(TreePath treePath)
    {
      return true;
    }

    #endregion

    #region Методы ICollection

    /// <summary>
    /// Возвращает число узлов в списке
    /// </summary>
    public int Count
    {
      get { return _list.Count; }
    }

    /// <summary>
    /// Добавляет несколько узлов в конец списка
    /// </summary>
    /// <param name="items">Добавляемые узлы</param>
    public void AddRange(IEnumerable items)
    {
      foreach (object obj in items)
        _list.Add(obj);
      OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
    }

    /// <summary>
    /// Возвращает один узел в конец списка
    /// </summary>
    /// <param name="item">Добавляемый узел</param>
    public void Add(object item)
    {
      _list.Add(item);
      OnNodesInserted(new TreeModelEventArgs(TreePath.Empty, new int[] { _list.Count - 1 }, new object[] { item }));
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      _list.Clear();
      OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
    }

    #endregion
  }

  /// <summary>
  /// Простейший переходник для списка, чтобы отображать его в дереве.
  /// Принимает интерфейс IEnumerable.
  /// Список предназначен только для просмотра, изменение списка не предусмотрено.
  /// </summary>
  public sealed class SimpleListTreeModel : TreeModelBase
  {
    // Original TreeViewAdv component from Aga.Controls.dll
    // Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
    // http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
    // http://sourceforge.net/projects/treeviewadv/
    //
    // В оригинале - класс TreeListAdapter

    #region Конструктор

    /// <summary>
    /// Создает "модель" - переходник
    /// </summary>
    /// <param name="list">Исходный список</param>
    public SimpleListTreeModel(System.Collections.IEnumerable list)
    {
      if (list == null)
        throw new ArgumentNullException();
      _List = list;
    }

    private System.Collections.IEnumerable _List;

    #endregion

    #region ITreeModel Members

    /// <summary>
    /// Возращает список для TreePath.Empty и фиктивный перечислитель для непустого пути
    /// </summary>
    /// <param name="treePath">Путь к корневому узлу</param>
    /// <returns>Перечислитель</returns>
    public override System.Collections.IEnumerable GetChildren(TreePath treePath)
    {
      if (treePath.IsEmpty)
        return _List;
      else
        return new DummyEnumerable<object>();
    }

    /// <summary>
    /// Всегда возвращает true
    /// </summary>
    /// <param name="treePath">Игнорируется</param>
    /// <returns>true</returns>
    public override bool IsLeaf(TreePath treePath)
    {
      return true;
    }

    #endregion
  }
}
