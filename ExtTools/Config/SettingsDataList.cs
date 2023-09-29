using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;

namespace FreeLibSet.Config
{
  #region ������������ SettingsPart

  /// <summary>
  /// ������ ������������ ��� �������� ����������
  /// </summary>
  [Flags]
  public enum SettingsPart
  {
    /// <summary>
    /// �������� ������ ������, ����������� � ������������
    /// </summary>
    User = 0x1,

    /// <summary>
    /// ����� ������ ��������� ���� ��� ������, �� ������� �������� �����.
    /// ����� ������ �� �������� � ������� ������� ��� ���������������� �������,
    /// � ����������� ������ ��������� ��������
    /// </summary>
    NoHistory = 0x2,

    /// <summary>
    /// ����� ������ ��������� ������ �� ����� � ��������, ����������� �� ���������� ������������.
    /// ����� ������ ������������� � ������������ � ����������. ��� ����������, ���������� ��������
    /// ������������� ���� ������ �� ����� ������. ��� ������� ���������� ��� �����, ���� ������������
    /// ����� ������� � ������ �����������, � ��������� ������������ �������� � ���� ������
    /// </summary>
    Machine = 0x4,
  }

  #endregion

  /// <summary>
  /// ��������� �������, ������������ � �������� ��������� ������, ������� ����� �������� � ��������� �� ������ ������������
  /// </summary>
  public interface ISettingsDataItem /*: IObjectWithCode*/
  {
    // ��� ������ ������������ ���� � NamedList, ��� ��� �� ����� ���� ���� ���������� �������� � ����� SettingsDataList

    /// <summary>
    /// ���������� ����� ���� �������� ������, ������������ ���� �������.
    /// ����� ���� ���������� ���������� �� ���������� ������.
    /// </summary>
    SettingsPart UsedParts { get; }

    /// <summary>
    /// �������� ������ � ������ ������������.
    /// ����� ����������� ������ ��� ������, ������������ <see cref="UsedParts"/>.
    /// </summary>
    /// <param name="cfg">������������ ������</param>
    /// <param name="part">������� �������� ������. �� ���� ����� ����� ���� ����� ������ ���� ����.</param>
    void WriteConfig(CfgPart cfg, SettingsPart part);

    /// <summary>
    /// ��������� ������ �� ������ ������������.
    /// ����� ����������� ������ ��� ������, ������������ <see cref="UsedParts"/>.
    /// </summary>
    /// <param name="cfg">������ � �������</param>
    /// <param name="part">������� �������� ������. �� ���� ����� ����� ���� ����� ������ ���� ����.</param>
    void ReadConfig(CfgPart cfg, SettingsPart part);
  }

  /// <summary>
  /// ����������� ���������� ���������� <see cref="ISettingsDataItem"/>
  /// </summary>
  public abstract class SettingsDataItem : ISettingsDataItem
  {
    /// <summary>
    /// ������������������ �������� ���������� <see cref="SettingsPart.User"/>
    /// </summary>
    public virtual SettingsPart UsedParts { get { return SettingsPart.User; } }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="part"></param>
    public abstract void ReadConfig(CfgPart cfg, SettingsPart part);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="part"></param>
    public abstract void WriteConfig(CfgPart cfg, SettingsPart part);
  }

  /// <summary>
  /// ������-�������� ��� ���������� ���������� <see cref="ISettingsDataItem"/>
  /// </summary>
  public sealed class DummySettingsDataItem : SettingsDataItem
  {
    /// <summary>
    /// ���������� 0
    /// </summary>
    public override SettingsPart UsedParts { get { return (SettingsPart)0; } }

    /// <summary>
    /// ������ �� ������
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="part"></param>
    public override void WriteConfig(CfgPart cfg, SettingsPart part)
    {
    }

    /// <summary>
    /// ������ �� ������
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="part"></param>
    public override void ReadConfig(CfgPart cfg, SettingsPart part)
    {
    }
  }

  /// <summary>
  /// ��������� ������� ������ <see cref="ISettingsDataItem"/>.
  /// � ��������� ����� �������������� ������ ������� ������ �������.
  /// </summary>
  public class SettingsDataList : ICollection<ISettingsDataItem>
  {
    #region �����������

    /// <summary>
    /// ������� ������ ������
    /// </summary>
    public SettingsDataList()
    {
      _Items = new List<ISettingsDataItem>();
    }

    #endregion

    #region ICollection

    private List<ISettingsDataItem> _Items;

    /// <summary>
    /// ���������� ���������� ���������
    /// </summary>
    public int Count { get { return _Items.Count; } }

    /// <summary>
    /// ��������� ������ � ������.
    /// </summary>
    /// <param name="item">����������� ������</param>
    public void Add(ISettingsDataItem item)
    {
      if (item == null)
        throw new ArgumentNullException();
      foreach (ISettingsDataItem item2 in _Items)
      {
        if (item2.GetType() == item.GetType())
          throw new ArgumentException("� ������ ��� ���� ������ ������ ����");
      }
      _Items.Add(item);
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    public void Clear()
    {
      _Items.Clear();
    }

    bool ICollection<ISettingsDataItem>.Contains(ISettingsDataItem item)
    {
      return _Items.Contains(item);
    }

    /// <summary>
    /// �������� ������ � ������
    /// </summary>
    /// <param name="array">����������� ������</param>
    /// <param name="arrayIndex">������ � <paramref name="array"/></param>
    public void CopyTo(ISettingsDataItem[] array, int arrayIndex)
    {
      _Items.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// ������� ������ �� ����� ���������
    /// </summary>
    /// <returns>����� ������</returns>
    public ISettingsDataItem[] ToArray()
    {
      ISettingsDataItem[] a = new ISettingsDataItem[_Items.Count];
      _Items.CopyTo(a, 0);
      return a;
    }

    bool ICollection<ISettingsDataItem>.Remove(ISettingsDataItem item)
    {
      return _Items.Remove(item);
    }

    /// <summary>
    /// ������� ������������� �� ���� ��������� ������
    /// </summary>
    /// <returns></returns>
    public List<ISettingsDataItem>.Enumerator GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator<ISettingsDataItem> IEnumerable<ISettingsDataItem>.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    bool ICollection<ISettingsDataItem>.IsReadOnly
    {
      get
      {
        throw new NotImplementedException();
      }
    }
    #endregion

    #region ������

    /// <summary>
    /// ���������� ��� ����� ���� ���������� ������, ������������ ����������� ���������
    /// </summary>
    public SettingsPart UsedParts
    {
      get
      {
        SettingsPart res = 0;
        for (int i = 0; i < _Items.Count; i++)
          res |= _Items[i].UsedParts;
        return res;
      }
    }

    private static readonly SettingsPart[] _AllParts = new SettingsPart[] { SettingsPart.User, SettingsPart.Machine, SettingsPart.NoHistory };
    private const SettingsPart AllPartValue = SettingsPart.User | SettingsPart.Machine | SettingsPart.NoHistory;

    /// <summary>
    /// ���������� ������ � ������ ������������
    /// </summary>
    /// <param name="cfg">������������ ������</param>
    /// <param name="part">����� �������� ������. � ������� �� <see cref="ISettingsDataItem.WriteConfig(CfgPart, SettingsPart)"/>,
    /// ����������� �������� ���������� �� ���������� ������.</param>
    public void WriteConfig(CfgPart cfg, SettingsPart part)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        for (int j = 0; j < _AllParts.Length; j++)
        {
          if ((part & _AllParts[j]) != 0 && (_Items[i].UsedParts & _AllParts[j]) != 0)
            _Items[i].WriteConfig(cfg, _AllParts[j]);
        }
      }
    }

    /// <summary>
    /// ������ ������ �� ������ ������������
    /// </summary>
    /// <param name="cfg">������ ��� ������</param>
    /// <param name="part">����� �������� ������. � ������� �� <see cref="ISettingsDataItem.WriteConfig(CfgPart, SettingsPart)"/>,
    /// ����������� �������� ���������� �� ���������� ������.</param>
    public void ReadConfig(CfgPart cfg, SettingsPart part)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        for (int j = 0; j < _AllParts.Length; j++)
        {
          if ((part & _AllParts[j]) != 0 && (_Items[i].UsedParts & _AllParts[j]) != 0)
            _Items[i].ReadConfig(cfg, _AllParts[j]);
        }
      }
    }

    /// <summary>
    /// ���������� ������ � ������ ������������ ��� ���������� �� �����
    /// </summary>
    /// <param name="cfg">������������ ������</param>
    public void WriteConfig(CfgPart cfg)
    {
      WriteConfig(cfg, AllPartValue);
    }

    /// <summary>
    /// ������ ������ �� ������ ������������ ��� ��������� �� �����
    /// </summary>
    /// <param name="cfg">������ ��� ������</param>
    public void ReadConfig(CfgPart cfg)
    {
      ReadConfig(cfg, AllPartValue);
    }

    /// <summary>
    /// ���������� �� ������ ������ ��������� ����.
    /// ���������� null, ���� ������ �� ������
    /// </summary>
    /// <typeparam name="T">��� ������� � ������</typeparam>
    /// <returns>������</returns>
    public T GetItem<T>()
      where T : class, ISettingsDataItem
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        T item = _Items[i] as T;
        if (item != null)
          return item;
      }
      return null;
    }

    /// <summary>
    /// ���������� �� ������ ������ ��������� ����.
    /// ����������� ����������, ���� ������ �� ������
    /// </summary>
    /// <typeparam name="T">��� ������� � ������</typeparam>
    /// <returns>������</returns>
    public T GetRequired<T>()
      where T : class, ISettingsDataItem
    {
      T res = GetItem<T>();
      if (res == null)
        throw new InvalidOperationException("����� ������ �� �������� ������� ������ " + typeof(T).Name);
      return res;
    }

    /// <summary>
    /// ������� �� ������ ������ ��������� ����
    /// </summary>
    /// <typeparam name="T">��� ���������� �������</typeparam>
    /// <returns>True, ���� ������ ������ � ������</returns>
    public bool Remove<T>()
      where T : class, ISettingsDataItem
    {
      T item = GetItem<T>();
      if (item == null)
        return false;
      else
        return _Items.Remove(item); // ������ ������� true
    }

    #endregion
  }
}
