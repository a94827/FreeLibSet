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
  /// ���������� ������ "������", ���������� ���� ������ ������ ������.
  /// ���� �������� ������� IList.
  /// �������� ������ ��� ����������, �������� � ��������� �����.
  /// ���� ������ ������������ ������ ��� ���������, ����������� ����� SimpleListTreeModel
  /// </summary>
  public class ListTreeModel : TreeModelBase
  {
    #region �����������

    /// <summary>
    /// ������� ������, ������������� �� ���������� �� ������ ����
    /// </summary>
    public ListTreeModel()
    {
      _list = new List<object>();
    }

    /// <summary>
    /// ������� ������ � �������� ������� �����.
    /// ������������ ������ <paramref name="list"/> �������� "�������".
    /// ������ ��������� ����� � ListModel ����� ������� ��������� � ���� ������.
    /// </summary>
    /// <param name="list">������</param>
    public ListTreeModel(IList list)
    {
      _list = list;
    }
    private IList _list;

    #endregion

    #region ������ ITreeModel

    /// <summary>
    /// ���������� ������� ������ � �������� �������������
    /// </summary>
    /// <param name="treePath">������������</param>
    /// <returns>������</returns>
    public override IEnumerable GetChildren(TreePath treePath)
    {
      return _list;
    }

    /// <summary>
    /// ���������� true, ��� ��� "������" �� �������� �����, ����� �������� ������.
    /// </summary>
    /// <param name="treePath">������������</param>
    /// <returns>true</returns>
    public override bool IsLeaf(TreePath treePath)
    {
      return true;
    }

    #endregion

    #region ������ ICollection

    /// <summary>
    /// ���������� ����� ����� � ������
    /// </summary>
    public int Count
    {
      get { return _list.Count; }
    }

    /// <summary>
    /// ��������� ��������� ����� � ����� ������
    /// </summary>
    /// <param name="items">����������� ����</param>
    public void AddRange(IEnumerable items)
    {
      foreach (object obj in items)
        _list.Add(obj);
      OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
    }

    /// <summary>
    /// ���������� ���� ���� � ����� ������
    /// </summary>
    /// <param name="item">����������� ����</param>
    public void Add(object item)
    {
      _list.Add(item);
      OnNodesInserted(new TreeModelEventArgs(TreePath.Empty, new int[] { _list.Count - 1 }, new object[] { item }));
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    public void Clear()
    {
      _list.Clear();
      OnStructureChanged(new TreePathEventArgs(TreePath.Empty));
    }

    #endregion
  }

  /// <summary>
  /// ���������� ���������� ��� ������, ����� ���������� ��� � ������.
  /// ��������� ��������� IEnumerable.
  /// ������ ������������ ������ ��� ���������, ��������� ������ �� �������������.
  /// </summary>
  public sealed class SimpleListTreeModel : TreeModelBase
  {
    // Original TreeViewAdv component from Aga.Controls.dll
    // Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
    // http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
    // http://sourceforge.net/projects/treeviewadv/
    //
    // � ��������� - ����� TreeListAdapter

    #region �����������

    /// <summary>
    /// ������� "������" - ����������
    /// </summary>
    /// <param name="list">�������� ������</param>
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
    /// ��������� ������ ��� TreePath.Empty � ��������� ������������� ��� ��������� ����
    /// </summary>
    /// <param name="treePath">���� � ��������� ����</param>
    /// <returns>�������������</returns>
    public override System.Collections.IEnumerable GetChildren(TreePath treePath)
    {
      if (treePath.IsEmpty)
        return _List;
      else
        return new DummyEnumerable<object>();
    }

    /// <summary>
    /// ������ ���������� true
    /// </summary>
    /// <param name="treePath">������������</param>
    /// <returns>true</returns>
    public override bool IsLeaf(TreePath treePath)
    {
      return true;
    }

    #endregion
  }
}