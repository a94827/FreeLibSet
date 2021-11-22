// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.DependedValues;

namespace FreeLibSet.Forms.Docs
{
  #region ��������� IDocEditItem

  /// <summary>
  /// ��������� ������ ��������, ������������ �������������� ���� ��� �����
  /// � ��������� ���������
  /// </summary>
  public interface IDocEditItem
  {
    /// <summary>
    /// ���������� ����� �������� ReadValues() ��� ���� ���������. ����� ����� 
    /// �������� ��������� ������������� ��������� �����
    /// </summary>
    void BeforeReadValues();

    /// <summary>
    /// ���������� ��� ������������� ���������
    /// </summary>
    void ReadValues();

    /// <summary>
    /// ���������� ����� ����, ��� ����� ReadValues() ��� ������� ��� ���� ��������
    /// � ������. ������ �������� ��������, ����������� BeforeReadValues()
    /// </summary>
    void AfterReadValues();

    /// <summary>
    /// ���������� ��� ������ ��������
    /// </summary>
    void WriteValues();

    /// <summary>
    /// ������, � ������� �������� DocEditItem ����� ��������� �������� �� ����������
    /// </summary>
    DepChangeInfo ChangeInfo { get; }
  }

  #endregion

  /// <summary>
  /// ������ �������� IDocEditItem � ��������������� �������� ������ � ������ ��������
  /// </summary>
  public class DocEditItemList : List<IDocEditItem>
  {
    #region ������ � ������ ��������

    /// <summary>
    /// ����� ������� IDocEditItem.BeforeReadValues(), ReadValues() � AfterReadValues() � ���������� ������
    /// </summary>
    public void ReadValues()
    {
      foreach (IDocEditItem Item in this)
        Item.BeforeReadValues();

      foreach (IDocEditItem Item in this)
      {
        try
        {
          Item.ReadValues();
        }
        catch (Exception e)
        {
          string DisplayName;
          if (Item.ChangeInfo == null)
            DisplayName = Item.ToString();
          else
            DisplayName = Item.ChangeInfo.DisplayName;
          EFPApp.ShowException(e, "������ ��� ���������� �������� \"" + DisplayName + "\"");
        }
      }
      foreach (IDocEditItem Item in this)
      {
        Item.AfterReadValues();
        //Item.ChangeInfo = new DepChangeInfoItem(FChangeInfo);
        //Item.ChangeInfo.DisplayName = Item.DisplayName;
      }
    }

    /// <summary>
    /// ����� ������ WriteValues() ��� ���� �������� � ������
    /// </summary>
    public void WriteValues()
    {
      foreach (IDocEditItem Item in this)
        Item.WriteValues();
    }

    #endregion
  }

  /// <summary>
  /// ������� �����, ����������� ��������� IDocEditItem
  /// ���� ���������������� ��� ������ ������������� ����������� ���������� ������
  /// � �� ��������� ������������ �� ������� ������, ����� ����������� ���� �����
  /// </summary>
  public abstract class DocEditItem : IDocEditItem
  {
    #region �����������

    /// <summary>
    /// ������� ������ � DepChangeInfoItem
    /// </summary>
    public DocEditItem()
    {
      _ChangeInfo = new DepChangeInfoItem();
    }

    #endregion

    #region IDocEditItem Members

    /// <summary>
    /// ���������� ����� �������� ReadValues() ��� ���� ���������. ����� ����� 
    /// �������� ��������� ������������� ��������� �����.
    /// ������������������ ����� ������ �� ������.
    /// </summary>
    public virtual void BeforeReadValues()
    {
    }

    /// <summary>
    /// ���������� ��� ������������� ���������.
    /// </summary>
    public abstract void ReadValues();

    /// <summary>
    /// ���������� ����� ����, ��� ����� ReadValues() ��� ������� ��� ���� ��������
    /// � ������. ������ �������� ��������, ����������� BeforeReadValues()
    /// ������������������ ����� ������ �� ������.
    /// </summary>
    public virtual void AfterReadValues()
    {
    }

    /// <summary>
    /// ���������� ��� ������ ��������
    /// </summary>
    public abstract void WriteValues();


    /// <summary>
    /// ������, � ������� �������� ����������� ����� ����� ��������� �������� �� ����������.
    /// </summary>
    protected DepChangeInfoItem ChangeInfo { get { return _ChangeInfo; } }
    private DepChangeInfoItem _ChangeInfo;

    DepChangeInfo IDocEditItem.ChangeInfo { get { return _ChangeInfo; } }

    #endregion

    #region �������������

    /// <summary>
    /// ���������� DisplayName
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return _ChangeInfo.DisplayName;
    }

    #endregion
  }

  /// <summary>
  /// ������� �����, ����������� ��������� IDocEditItem � ���������� �������� ��������
  /// ���� ���������������� ��� ������ ������������� ����������� ���������� ������
  /// � �� ��������� ������������ �� ������� ������, ����� ����������� ���� �����
  /// </summary>
  public abstract class DocEditItemWithChildren : IDocEditItem
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    public DocEditItemWithChildren()
    {
      _ChangeInfo = new DepChangeInfoList();

      _Children = new ChildList(this);
    }

    #endregion

    #region �������� ��������

    private class ChildList : ICollection<IDocEditItem>
    {
      #region �����������

      public ChildList(DocEditItemWithChildren owner)
      {
        _Owner = owner;
        _Items = new List<IDocEditItem>();
      }

      #endregion

      #region ��������

      private DocEditItemWithChildren _Owner;

      public List<IDocEditItem> Items { get { return _Items; } }
      private List<IDocEditItem> _Items;

      public override string ToString()
      {
        return Items.ToString();
      }

      #endregion

      #region ICollection<IDocEditItem> Members

      public void Add(IDocEditItem item)
      {
        if (item == null)
          throw new ArgumentNullException();

        _Items.Add(item);
        _Owner._ChangeInfo.Add(item.ChangeInfo);
      }

      public void Clear()
      {
        IDocEditItem[] a = _Items.ToArray();
        for (int i = 0; i < a.Length; i++)
          this.Remove(a[i]);
      }

      public bool Contains(IDocEditItem item)
      {
        return _Items.Contains(item);
      }

      public void CopyTo(IDocEditItem[] array, int arrayIndex)
      {
        _Items.CopyTo(array, arrayIndex);
      }

      public int Count
      {
        get { return _Items.Count; }
      }

      public bool IsReadOnly { get { return false; } }

      public bool Remove(IDocEditItem item)
      {
        if (item == null)
          return false;
        _Owner._ChangeInfo.Remove(item.ChangeInfo);
        return _Items.Remove(item);
      }

      public IEnumerator<IDocEditItem> GetEnumerator()
      {
        return _Items.GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return _Items.GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// �������� ����������� ��������, ������������ � ���������.
    /// ��� ������ ����������� ������������ � ����� ������ � �� � ��������� ���������������,
    /// ����� ����� ������������ ������� ������� ������� ������/������ � ��� �� ����� ����������
    /// </summary>
    public ICollection<IDocEditItem> Children { get { return _Children; } }
    private ChildList _Children;

    #endregion

    #region ������������ ���������

    /// <summary>
    /// ������ ���������. ����������� ��������� ��� � ����� ������, ��� � � �������� ���������
    /// </summary>
    public DepChangeInfo ChangeInfo { get { return _ChangeInfo; } }

    /// <summary>
    /// ������ � ChangeInfo ��� � ������
    /// </summary>
    protected DepChangeInfoList ChangeInfoList { get { return _ChangeInfo; } }
    private DepChangeInfoList _ChangeInfo;

    #endregion

    #region IDocEditItem Members

    /// <summary>
    /// �������� ����� ��� ���� �������� ��������
    /// </summary>
    public virtual void BeforeReadValues()
    {
      for (int i = 0; i < _Children.Items.Count; i++)
        _Children.Items[i].BeforeReadValues();
    }

    /// <summary>
    /// ������ ��������.
    /// ����� ������ ���� �������������. 
    /// �������� ������� ����� ������� ����� ���������� ����� ��������, ����� �������� ��������
    /// �������� ��� ����������� ��������
    /// </summary>
    public virtual void ReadValues()
    {
      for (int i = 0; i < _Children.Items.Count; i++)
        _Children.Items[i].ReadValues();
    }

    /// <summary>
    /// �������� ����� ��� ���� �������� ��������
    /// </summary>
    public virtual void AfterReadValues()
    {
      for (int i = 0; i < _Children.Items.Count; i++)
        _Children.Items[i].AfterReadValues();
    }

    /// <summary>
    /// ������ ��������.
    /// ����� ������ ���� �������������. 
    /// �������� ������� ����� ������� ����� ����������� ����� ��������, ����� �������� ��������
    /// ������������ ��������, ���������� ��������� ����������
    /// </summary>
    public virtual void WriteValues()
    {
      for (int i = 0; i < _Children.Items.Count; i++)
        _Children.Items[i].WriteValues();
    }
                                                         
    #endregion

    #region �������������

    /// <summary>
    /// ���������� DisplayName
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return _ChangeInfo.DisplayName;
    }

    #endregion
  }
}
