// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Collections
{
  /// <summary>
  /// ������ ������� ��� ������.
  /// �������� ����������� ����� OnChanged() � ������� Changed.
  /// ���� ����� �� �������� ����������������
  /// </summary>
  [Serializable]
  public class SelectionFlagList
  {
    #region ������������

    /// <summary>
    /// ������� ������ � �������� ������ ���������.
    /// � �������� ��������� ������ �� �����������.
    /// </summary>
    /// <param name="count">���������� �������</param>
    public SelectionFlagList(int count)
    {
      _Flags = new bool[count];
    }

    /// <summary>
    /// ������� ������ �������, ������� ������ �� ���������.
    /// ����� ���������� ������������ ����� � �������� ���������� �� �����������.
    /// </summary>
    /// <param name="source">���������� ��������� �������</param>
    public SelectionFlagList(ICollection<bool> source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif
      _Flags = new bool[source.Count];
      source.CopyTo(_Flags, 0);
    }

    #endregion

    #region ������

    private bool[] _Flags;

    /// <summary>
    /// ��������� / ��������� �������� ������ ������ �� �������.
    /// </summary>
    /// <param name="index">������ ������</param>
    /// <returns>�������� ������</returns>
    public bool this[int index]
    {
      get { return _Flags[index]; }
      set
      {
        if (value == _Flags[index])
          return;
        _Flags[index] = value;
        OnChanged();
      }
    }

    /// <summary>
    /// ���������� ������� � �������. ������������ � ������������ � �� ����� ���� ��������.
    /// </summary>
    public int Count { get { return _Flags.Length; } }

    /// <summary>
    /// ���������� ���������� ��������� ���������
    /// </summary>
    public int SelectedCount
    {
      get
      {
        int cnt = 0;
        for (int i = 0; i < _Flags.Length; i++)
        {
          if (_Flags[i])
            cnt++;
        }
        return cnt;
      }
    }

    /// <summary>
    /// ��������� ������������� ��� �������.
    /// ���������� ������ ������ Count �������� "0" � "1". ��� ������������� ������� �������� "1", ��� �� ������������� - "0"
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder(_Flags.Length);
      for (int i = 0; i < _Flags.Length; i++)
        sb.Append(_Flags[i] ? '1' : '0');

      return sb.ToString();
    }

    #endregion

    #region ������� Changed

    /// <summary>
    /// ������� ���������� ��� ��������� ������ ������
    /// </summary>
    public event EventHandler Changed;

    /// <summary>
    /// ���������� ��� ��������� ����� ������.
    /// �������� ������� Changed.
    /// </summary>
    protected virtual void OnChanged()
    {
      if (Changed != null)
        Changed(this, EventArgs.Empty);
    }

    #endregion

    #region �������������� � ������

    /// <summary>
    /// �������� ������ � ������.
    /// </summary>
    /// <returns>����� ������ �������</returns>
    public bool[] ToArray()
    {
      return (bool[])(_Flags.Clone());
    }

    /// <summary>
    /// ��������� ���������� ���� �������.
    /// ����� ������� <paramref name="value"/> ������ ��������� � ������ ������� Count.
    /// ������� Changed ���������� ���� ��� � �����, ������ ���� �������� ������-���� ������ ����������.
    /// </summary>
    /// <param name="value">������ ����� ��������</param>
    public void FromArray(bool[] value)
    {
      if (value == null)
        throw new ArgumentNullException();
      if (value.Length != _Flags.Length)
        throw new ArgumentException("������������ ����� �������");

      bool Changed = false;
      for (int i = 0; i < _Flags.Length; i++)
      {
        if (value[i] != _Flags[i])
        {
          _Flags[i] = value[i];
          Changed = true;
        }
      }
      if (Changed)
        OnChanged();
    }

    #endregion

    #region ����� ���� ����� � ������ ������

    /// <summary>
    /// ������������� ��� ������ � �������� true.
    /// ������� Changed ���������� ���� ��� � �����, ������ ���� �������� ������-���� ������ ����������
    /// </summary>
    public void SelectAll()
    {
      bool Changed = false;
      for (int i = 0; i < _Flags.Length; i++)
      {
        if (!_Flags[i])
        {
          _Flags[i] = true;
          Changed = true;
        }
      }
      if (Changed)
        OnChanged();
    }

    /// <summary>
    /// ������������� ��� ������ � �������� false.
    /// ������� Changed ���������� ���� ��� � �����, ������ ���� �������� ������-���� ������ ����������
    /// </summary>
    public void UnselectAll()
    {
      bool Changed = false;
      for (int i = 0; i < _Flags.Length; i++)
      {
        if (_Flags[i])
        {
          _Flags[i] = false;
          Changed = true;
        }
      }
      if (Changed)
        OnChanged();
    }

    /// <summary>
    /// ������������� ��� ������ � ��������������� ��������.
    /// ������� Changed ���������� ���� ��� � �����, ���� ����� ������� ������ 0.
    /// </summary>
    public void InvertAll()
    {
      for (int i = 0; i < _Flags.Length; i++)
        _Flags[i] = !_Flags[i];
      if (_Flags.Length > 0)
        OnChanged();
    }

    /// <summary>
    /// ���������� true, ���� ��� ������ ����� �������� true.
    /// ���� Count=0, ������������ true.
    /// </summary>
    public bool AreAllSelected
    {
      get
      {
        for (int i = 0; i < _Flags.Length; i++)
        {
          if (!_Flags[i])
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// ���������� true, ���� ��� ������ ����� �������� false.
    /// ���� Count=0, ������������ true.
    /// </summary>
    public bool AreAllUnselected
    {
      get
      {
        for (int i = 0; i < _Flags.Length; i++)
        {
          if (_Flags[i])
            return false;
        }
        return true;
      }
    }

    #endregion

    #region SelectedIndices

    /// <summary>
    /// ��������� �������� � ���� ������� �������� ���������.
    /// ������ � ������ �������� �������� ������������ ����������, ������� ������� �������� ���������
    /// � �������� � �����.
    /// ��� ������ � ������� ����� ���� �������� (-1), ������� ������������
    /// </summary>
    public int[] SelectedIndices
    {
      get
      {
        List<int> lst = new List<int>();
        for (int i = 0; i < _Flags.Length; i++)
        {
          if (_Flags[i])
            lst.Add(i);
        }
        return lst.ToArray();
      }
      set
      {
        if (value != null)
        {
          bool[] a = new bool[_Flags.Length];
          for (int i = 0; i < value.Length; i++)
          {
            if (value[i] >= 0)
              a[value[i]] = true;
          }
          FromArray(a);
        }
        else
          UnselectAll();
      }
    }

    #endregion
  }
}
