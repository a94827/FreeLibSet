// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using FreeLibSet.Core;


// �������������� ���������, ������������ ��������� IObjectWithCode
// 25.07.2019.
// ��������� �������, ����� ��� T ��� �������, � �� ����������.
// ���������� ����������� �� �������� ������������ � ��� ����� ���� �� �� �������, 
// �� ������������� �������� �������� � ����������� boxing'� ��������� ��� ������ ������ ����������, ��� �������� �������������.

namespace FreeLibSet.Collections
{
  #region ��������� IObjectWithCode

  /// <summary>
  /// ������, ���������� ���
  /// </summary>
  public interface IObjectWithCode
  {
    /// <summary>
    /// ���������� ��� �������.
    /// ��������� ����� ��������� ��� ������������ ������� �������� ����
    /// </summary>
    string Code { get; }
  }

  #endregion

  /// <summary>
  /// ������� ���������� ��� ���������� IObjectWithCode
  /// </summary>
  [Serializable]
  public class ObjectWithCode : IObjectWithCode
  {
    #region �����������

    /// <summary>
    /// ������� ����� ������ � �������� �����
    /// </summary>
    /// <param name="code">���. ������ ���� �����</param>
    public ObjectWithCode(string code)
    {
      if (String.IsNullOrEmpty(code))
        throw new ArgumentNullException("code");
      _Code = code;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���, �������� � ������������
    /// </summary>
    protected string Code { get { return _Code; } }
    private readonly string _Code;

    /// <summary>
    /// ��������� �������������, ������������ ���.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return _Code;
    }

    #endregion

    #region ��������������� ������

    string IObjectWithCode.Code
    {
      get { return _Code; }
    }

    /// <summary>
    /// ���-�������� ��� ���������. ���������� Code.GetHashCode()
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return _Code.GetHashCode();
    }

    #endregion
  }

  /// <summary>
  /// ������ ��� ���������� ��������, ����������� ��������� IObjectWithCode.
  /// ����������� ����������� ���������� �������� Ordinal ��� OrdinalIgnoreCase.
  /// </summary>
  public sealed class ObjectWithCodeComparer<T> : IComparer<T>
    where T:IObjectWithCode
  {
    #region ���������� �����������

    private ObjectWithCodeComparer(StringComparison comparisonType)
    {
      _ComparisonType = comparisonType;
    }

    #endregion

    #region ����

    private readonly StringComparison _ComparisonType;

    #endregion

    #region IComparer<IObjectWithCode> Members

    /// <summary>
    /// ��������� ���� ��������.
    /// ���������� String.Compare(x.Code, y.Code, ComparisonType)
    /// </summary>
    /// <param name="x">������ ������������ ������</param>
    /// <param name="y">������ ������������ ������</param>
    /// <returns>��������� ��������� ����� ��������</returns>
    public int Compare(T x, T y)
    {
      return String.Compare(x.Code, y.Code, _ComparisonType);
    }

    #endregion

    #region ����������� ����������

    /// <summary>
    /// �����������, ����������� ������� ��������
    /// </summary>
    public static readonly ObjectWithCodeComparer<T> Ordinal = new ObjectWithCodeComparer<T>(StringComparison.Ordinal);

    /// <summary>
    /// �����������, ������������ ������� ��������
    /// </summary>
    public static readonly ObjectWithCodeComparer<T> OrdinalIgnoreCase = new ObjectWithCodeComparer<T>(StringComparison.OrdinalIgnoreCase);

    #endregion
  }

#if OLD  
  /// <summary>
  /// ������ �������� ������������� ����, ������ � ������� ����� �������������� ��� ��
  /// ������� (��� � ������� ������ List), ��� � �� ����, ��� � Dictionary � ������ String.
  /// ������ �� ����� ��������� �������� null.
  /// ��������� ����� ���� �������������� ��� ���������������� � �������� ���� (�������� � ������������).
  /// ���� ����� �������� ���������������� ����� ��������� �������� ReadOnly.
  /// ���� ������ �����������, ������ �� �������� ����������������.
  /// </summary>
  /// <typeparam name="T">��� ��������, ���������� � ������, �������������� ��������� IObjectWithCode</typeparam>
  /// <remarks>
  /// ���� �������� �� ��������� ��������� IObjectWithCode, ����������� OrderSortedList.
  /// ���� �� ��������� ������ � �������� �� �������, ����������� ����� "������" ����� NamedCollection.
  /// </remarks>
  [Serializable]
  public class NamedList<T> : IEnumerable<T>, IList<T>, IReadOnlyObject, INamedValuesAccess
    where T : IObjectWithCode
  {
  #region ������������

    /// <summary>
    /// ������� ������ ������.
    /// ������� ���� �����������
    /// </summary>
    public NamedList()
      : this(false)
    {
    }


    /// <summary>
    /// ������� ������ ������
    /// </summary>
    /// <param name="IgnoreCase">���� �� ������������ ������� ����</param>
    public NamedList(bool IgnoreCase)
    {
      FList = new List<T>();
      FDict = new Dictionary<string, T>();
      FIgnoreCase = IgnoreCase;
    }

    /// <summary>
    /// ������� ������ ������ �������� �������.
    /// ������� ���� �����������.
    /// </summary>
    /// <param name="Capacity">��������� ������� ������</param>
    public NamedList(int Capacity)
      : this(Capacity, false)
    {
    }

    /// <summary>
    /// ������� ������ ������ �������� �������
    /// </summary>
    /// <param name="Capacity">��������� ������� ������</param>
    /// <param name="IgnoreCase">���� �� ������������ ������� ����</param>
    public NamedList(int Capacity, bool IgnoreCase)
    {
      FList = new List<T>(Capacity);
      FDict = new Dictionary<string, T>(Capacity);
      FIgnoreCase = IgnoreCase;
    }


    /// <summary>
    /// ������� ������, �������� ��� ���������� �� ���������.
    /// ������� ���� �����������.
    /// </summary>
    /// <param name="Src">������� ������ � ��������</param>
    public NamedList(IDictionary<string, T> Src)
    {
      FList = new List<T>(Src.Values);
      FDict = new Dictionary<string, T>(Src);
    }

    /// <summary>
    /// ������� ������, �������� ��� ���������� �� ���������.
    /// ������� ���� �����������.
    /// ���� ������ <paramref name="Src"/> �������� ������� � �������������� ������, ������� �������������
    /// </summary>
    /// <param name="Src">�������� ������ ��������</param>
    public NamedList(ICollection<T> Src)
      : this(Src, false)
    {
    }

    /// <summary>
    /// ������� ������, �������� ��� ���������� �� ���������.
    /// ���� ������ <paramref name="Src"/> �������� ������� � �������������� ������ (� ������ <paramref name="IgnoreCase"/>), ������� �������������
    /// </summary>
    /// <param name="Src">�������� ������ ��������</param>
    /// <param name="IgnoreCase">���� �� ������������ ������� ����</param>
    public NamedList(ICollection<T> Src, bool IgnoreCase)
      : this(Src.Count, IgnoreCase)
    {
      foreach (T Item in Src)
        Add(Item);
    }

    /// <summary>
    /// ������� ������, �������� ��� ���������� �� ���������.
    /// ������� ���� �����������.
    /// ���� ������ <paramref name="Src"/> �������� ������� � �������������� ������, ������� �������������
    /// </summary>
    /// <param name="Src">�������� ������ ��������</param>
    public NamedList(IEnumerable<T> Src)
      : this(Src, false)
    {
    }

    /// <summary>
    /// ������� ������, �������� ��� ���������� �� ���������.
    /// ���� ������ <paramref name="Src"/> �������� ������� � �������������� ������ (� ������ <paramref name="IgnoreCase"/>), ������� �������������
    /// </summary>
    /// <param name="Src">�������� ������ ��������</param>
    /// <param name="IgnoreCase">���� �� ������������ ������� ����</param>
    public NamedList(IEnumerable<T> Src, bool IgnoreCase)
      : this(IgnoreCase)
    {
      foreach (T Item in Src)
        Add(Item);
    }

  #endregion

  #region ������ � ���������

    /// <summary>
    /// �������� ������, ������������ ������� ���������
    /// </summary>
    private List<T> FList;

    /// <summary>
    /// ��������� �� �����.
    /// ���� IgnoreCase=true, �� ���� ������������ � �������� ��������
    /// </summary>
    [NonSerialized]
    private Dictionary<string, T> FDict;

    /// <summary>
    /// ������ �� �������
    /// </summary>
    /// <param name="Index">������ �������� � �������. ������ ���� � ��������� �� 0 �� Count-1</param>
    /// <returns></returns>
    public T this[int Index]
    {
      get { return FList[Index]; }
      set
      {
        CheckNotReadOnly();

        T OldItem = FList[Index];
        FDict.Remove(OldItem.Code);
        try
        {
          string NewCode = value.Code;
          if (FIgnoreCase)
            NewCode = NewCode.ToUpperInvariant();
          FDict.Add(NewCode, value);
        }
        catch
        {
          string OldItemCode = OldItem.Code;
          if (FIgnoreCase)
            OldItemCode = OldItemCode.ToUpperInvariant();
          FDict.Add(OldItemCode, OldItem);
          throw;
        }
        FList[Index] = value;
      }
    }

    /// <summary>
    /// ������ �� ����.
    /// ���� �������� �������������� ���, ������������ ������ �������
    /// </summary>
    /// <param name="Code">��� �������</param>
    /// <returns>������ ��� ������ ��������, ���� � ������ ��� ������� � ����� �����</returns>
    public T this[string Code]
    {
      get
      {
        T res;
        if (string.IsNullOrEmpty(Code))
          return default(T);

        if (IgnoreCase)
          Code = Code.ToUpperInvariant();

        if (FDict.TryGetValue(Code, out res))
          return res;
        else
          return default(T);
      }
    }

    /// <summary>
    /// ���������� ������� � �������� �����.
    /// � ������� �� ���������������� ������� �� ����, ���� �� ������ ������ � �������� �����, ������������ ����������
    /// </summary>
    /// <param name="Code">��� �������</param>
    /// <returns>������</returns>
    public T GetRequired(string Code)
    {
      T res;
      if (string.IsNullOrEmpty(Code))
        throw new ArgumentNullException("Code");

      if (IgnoreCase)
        Code = Code.ToUpperInvariant();

      if (FDict.TryGetValue(Code, out res))
        return res;
      else
        throw new ArgumentException("� ������ ��� �������� � ����� \"" + Code + "\"");
    }

    /// <summary>
    /// ���������� ���������� ��������� � ������
    /// </summary>
    public int Count { get { return FList.Count; } }

    /// <summary>
    /// ��������� ������������� "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

  #endregion

  #region IgnoreCase

    /// <summary>
    /// ���� ����������� � true, �� ��� ������ ��������� ����� �������������� �������.
    /// ���� �������� ����������� � false (�� ���������), �� ������� �������� ����������
    /// �������� ��������������� � ������������
    /// </summary>
    public bool IgnoreCase { get { return FIgnoreCase; } }
    private bool FIgnoreCase;

  #endregion

  #region ������ ������ ��� ������

    /// <summary>
    /// ���������� true, ���� ������ ��� ��������� � ����� "������ ������"
    /// </summary>
    public bool IsReadOnly { get { return FIsReadOnly; } }
    private bool FIsReadOnly;

    /// <summary>
    /// ���������� ����� ��� �������� ������ � ����� "������ ������"
    /// </summary>
    internal protected void SetReadOnly()
    {
      FIsReadOnly = true;
    }

    /// <summary>
    /// ���������� ����������, ���� IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (FIsReadOnly)
        throw new ObjectReadOnlyException();
    }

  #endregion

  #region IEnumerable<T> Members

    /// <summary>
    /// ���������� ������������� �������� � ������
    /// </summary>
    /// <returns>�������������</returns>
    public IEnumerator<T> GetEnumerator()
    {
      return FList.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return FList.GetEnumerator();
    }

  #endregion

  #region IList<T> Members

    /// <summary>
    /// ���������� ������ ������� � ������ ��� (-1), ���� ������ �� ������.
    /// ����� �������� ���������.
    /// ���� ��������� ������ ��������� ������� �������� � ����� �����, ������������� ������������
    /// ����� Contains(), ����������� ��������� ���
    /// </summary>
    /// <param name="item">������ ��� ������</param>
    /// <returns>������ �������.</returns>
    public int IndexOf(T item)
    {
      return FList.IndexOf(item);
    }

    /// <summary>
    /// ��������� ������� � �������� ������� ������.
    /// ���� � ������ ��� ���� ������� � ����� ����� (� ������ IgnoreCase), ������������ ����������
    /// </summary>
    /// <param name="index">������� ��� ����������</param>
    /// <param name="item">����������� ������</param>
    public void Insert(int index, T item)
    {
      CheckNotReadOnly();

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      FDict.Add(ItemCode, item);
      try
      {
        FList.Insert(index, item);
      }
      catch
      {
        FDict.Remove(ItemCode);
        throw;
      }
    }

    /// <summary>
    /// ������� ������� �� ��������� ������� ������.
    /// </summary>
    /// <param name="index">������ ��������</param>
    public void RemoveAt(int index)
    {
      CheckNotReadOnly();

      T item = FList[index];
      FList.RemoveAt(index);

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      FDict.Remove(ItemCode);
    }

  #endregion

  #region ICollection<T> Members

    /// <summary>
    /// ��������� ������� � ����� ������.
    /// ���� � ������ ��� ���� ������� � ����� ����� (� ������ IgnoreCase), ������������ ����������
    /// </summary>
    /// <param name="item">����������� �������</param>
    public void Add(T item)
    {
      CheckNotReadOnly();

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      FDict.Add(ItemCode, item);
      try
      {
        FList.Add(item);
      }
      catch
      {
        FDict.Remove(ItemCode);
        throw;
      }
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      FList.Clear();
      FDict.Clear();
    }

    /// <summary>
    /// ��������� ������� ����� � ������ �������� � ����� �����.
    /// ������������� ������������ ����������, ����������� ��������� ���.
    /// </summary>
    /// <param name="item">������� �������</param>
    /// <returns>true, ���� ������� ������</returns>
    public bool Contains(T item)
    {
      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();
      // return FDict.ContainsKey(ItemCode);

      // 21.02.2018
      // ���� ��� � �� ��������� ���������
      T ResItem;
      if (FDict.TryGetValue(ItemCode, out ResItem))
        return item.Equals(ResItem);
      else
        return false;
    }

    /// <summary>
    /// �������� ������ � ������
    /// </summary>
    /// <param name="array">����������� ������</param>
    /// <param name="arrayIndex">��������� ������ � ������� ��� ����������</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      FList.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// ������� �������� ������� �� ������
    /// </summary>
    /// <param name="item">��������� �������</param>
    /// <returns>true, ���� ������� ��� ������ � ������</returns>
    public bool Remove(T item)
    {
      CheckNotReadOnly();

      if (FList.Remove(item))
      {
        string ItemCode = item.Code;
        if (IgnoreCase)
          ItemCode = ItemCode.ToUpperInvariant();

        FDict.Remove(ItemCode);
        return true;
      }
      else
        return false;
    }

  #endregion

  #region �������������� ������

    /// <summary>
    /// ��������� ����� �� ���� (� ������ IgnoreCase).
    /// ���������� ������ ���������� �������� ��� (-1)
    /// ���� ��������� ������ ���������� ������������� �������� � �������� �����, �����������
    /// Contains(), ����������� ��������� ��������.
    /// </summary>
    /// <param name="Code">������� ���</param>
    /// <returns>������ �������</returns>
    public int IndexOf(string Code)
    {
      if (IgnoreCase)
      {
        for (int i = 0; i < FList.Count; i++)
        {
          if (String.Equals(FList[i].Code, Code, StringComparison.OrdinalIgnoreCase))
            return i;
        }
      }
      else
      {
        for (int i = 0; i < FList.Count; i++)
        {
          if (FList[i].Code == Code)
            return i;
        }
      }
      return -1;
    }

    /// <summary>
    /// ������� ����� ���� (� ������ IgnoreCase).
    /// ���������� true, ���� � ������ ���� ������� � �������� �����
    /// </summary>
    /// <param name="Code">������� ���</param>
    /// <returns>������� �������� � ������</returns>
    public bool Contains(string Code)
    {
      if (String.IsNullOrEmpty(Code))
        return false;

      if (IgnoreCase)
        Code = Code.ToUpperInvariant();

      return FDict.ContainsKey(Code);
    }

    /// <summary>
    /// ������� ����� ���� (� ������ IgnoreCase).
    /// ���������� true, ���� � ������ ���� ������� � �������� �����.
    /// ��� ���� ����� ������������ � �������.
    /// ���� � ������ ��� �������� � ����� �����, ������������ false, 
    /// � Value �������� ������ ��������
    /// </summary>
    /// <param name="Code">������� ���</param>
    /// <param name="Value">���� ���������� ��������� ��������</param>
    /// <returns>������� �������� � ������</returns>
    public bool TryGetValue(string Code, out T Value)
    {
      if (String.IsNullOrEmpty(Code))
      {
        Value = default(T);
        return false;
      }

      if (IgnoreCase)
        Code = Code.ToUpperInvariant();

      return FDict.TryGetValue(Code, out Value);
    }

    /// <summary>
    /// �������� ������ � ������
    /// </summary>
    /// <returns>����� ������</returns>
    public T[] ToArray()
    {
      return FList.ToArray();
    }

    /// <summary>
    /// ��������� ��������� ��������� � ������.
    /// ������������ ����������������� ������ ������ Add()
    /// </summary>
    /// <param name="Collection">������ ����������� ���������</param>
    public void AddRange(IEnumerable<T> Collection)
    {
      CheckNotReadOnly();
      foreach (T Item in Collection)
        Add(Item);
    }

    /// <summary>
    /// ������� ������� � �������� ����� (� ������ IgnoreCase)
    /// </summary>
    /// <param name="Code">��� ���������� ��������</param>
    /// <returns>true, ���� ������ ��� ������ � ������</returns>
    public bool Remove(string Code)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(Code))
        return false;
      if (IgnoreCase)
        Code = Code.ToUpperInvariant();

      int p = IndexOf(Code);
      if (p >= 0)
      {
        FList.RemoveAt(p);
        FDict.Remove(Code);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// ���������� ��������� ������ � ������ ���������.
    /// ������� ������������� ������������ ��������� � ������.
    /// ������� �������� �� ��������, ���� ���� IgnoreCase=true.
    /// </summary>
    /// <returns></returns>
    public string[] GetCodes()
    {
      string[] a = new string[FList.Count];
      for (int i = 0; i < FList.Count; i++)
        a[i] = FList[i].Code;

      return a;
    }

  #endregion

  #region ��������������

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      FDict = new Dictionary<string, T>(FList.Count);
      for (int i = 0; i < FList.Count; i++)
      {
        string ItemCode = FList[i].Code;
        if (IgnoreCase)
          ItemCode = ItemCode.ToUpperInvariant();

        FDict.Add(ItemCode, FList[i]);
      }
    }

  #endregion

  #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string Name)
    {
      return this[Name];
    }

    string[] INamedValuesAccess.GetNames()
    {
      return GetCodes();
    }

  #endregion
  }
#else
  /// <summary>
  /// ������ �������� ������������� ����, ������ � ������� ����� �������������� ��� ��
  /// ������� (��� � ������� ������ List), ��� � �� ����, ��� � Dictionary � ������ String.
  /// ������ �� ����� ��������� �������� null.
  /// ��������� ����� ���� �������������� ��� ���������������� � �������� ���� (�������� � ������������).
  /// ���� ����� �������� ���������������� ����� ��������� �������� ReadOnly.
  /// ���� ������ �����������, ������ �� �������� ����������������.
  /// </summary>
  /// <typeparam name="T">��� ��������, ���������� � ������, �������������� ��������� IObjectWithCode</typeparam>
  /// <remarks>
  /// ���� �������� �� ��������� ��������� IObjectWithCode, ����������� OrderSortedList.
  /// ���� �� ��������� ������ � �������� �� �������, ����������� ����� "������" ����� NamedCollection.
  /// �������� � ������ �������� � ������� ����������. ��� �������������, ����������� ����� Sort().
  /// </remarks>
  [Serializable]
  public class NamedList<T> : IEnumerable<T>, IList<T>, IReadOnlyObject, INamedValuesAccess
    where T : class, IObjectWithCode
  {
    #region ������������

    /// <summary>
    /// ������� ������ ������.
    /// ������� ���� �����������
    /// </summary>
    public NamedList()
      : this(false)
    {
    }


    /// <summary>
    /// ������� ������ ������
    /// </summary>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    public NamedList(bool ignoreCase)
    {
      _List = new List<T>();
      _Dict = new Dictionary<string, int>();
      _DictIsValid = true;
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ������ ������ �������� �������.
    /// ������� ���� �����������.
    /// </summary>
    /// <param name="capacity">��������� ������� ������</param>
    public NamedList(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// ������� ������ ������ �������� �������
    /// </summary>
    /// <param name="capacity">��������� ������� ������</param>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    public NamedList(int capacity, bool ignoreCase)
    {
      _List = new List<T>(capacity);
      _Dict = new Dictionary<string, int>(capacity);
      _DictIsValid = true;
      _IgnoreCase = ignoreCase;
    }


    /// <summary>
    /// ������� ������, �������� ��� ���������� �� ���������.
    /// ������� ���� �����������.
    /// </summary>
    /// <param name="srcDictionary">������� ������ � ��������</param>
    public NamedList(IDictionary<string, T> srcDictionary)
      :this(srcDictionary.Count)
    {
      AddRange(srcDictionary.Values);
    }

    /// <summary>
    /// ������� ������, �������� ��� ���������� �� ���������.
    /// ������� ���� �����������.
    /// ���� ������ <paramref name="srcCollection"/> �������� ������� � �������������� ������, ������� �������������
    /// </summary>
    /// <param name="srcCollection">�������� ������ ��������</param>
    public NamedList(ICollection<T> srcCollection)
      : this(srcCollection, false, false)
    {
    }

    /// <summary>
    /// ������� ������, �������� ��� ���������� �� ���������.
    /// ���� ������ <paramref name="srcCollection"/> �������� ������� � �������������� ������ (� ������ <paramref name="ignoreCase"/>), ������� �������������
    /// </summary>
    /// <param name="srcCollection">�������� ������ ��������</param>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    public NamedList(ICollection<T> srcCollection, bool ignoreCase)
      : this(srcCollection, ignoreCase, false)
    {
    }

    /// <summary>
    /// ������� ������, �������� ��� ���������� �� ���������.
    /// ���� ������ <paramref name="srcCollection"/> �������� ������� � �������������� ������ (� ������ <paramref name="ignoreCase"/>), ������� �������������
    /// </summary>
    /// <param name="srcCollection">�������� ������ ��������</param>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    /// <param name="isReadOnly">���� true, �� ������ ����� ����������� � ����� "������ ������"</param>
    public NamedList(ICollection<T> srcCollection, bool ignoreCase, bool isReadOnly)
      : this(srcCollection.Count, ignoreCase)
    {
      foreach (T item in srcCollection)
        Add(item);

      _IsReadOnly = isReadOnly;
    }

    /// <summary>
    /// ������� ������, �������� ��� ���������� �� ���������.
    /// ������� ���� �����������.
    /// ���� ������ <paramref name="srcCollection"/> �������� ������� � �������������� ������, ������� �������������
    /// </summary>
    /// <param name="srcCollection">�������� ������ ��������</param>
    public NamedList(IEnumerable<T> srcCollection)
      : this(srcCollection, false, false)
    {
    }

    /// <summary>
    /// ������� ������, �������� ��� ���������� �� ���������.
    /// ���� ������ <paramref name="srcCollection"/> �������� ������� � �������������� ������ (� ������ <paramref name="ignoreCase"/>), ������� �������������
    /// </summary>
    /// <param name="srcCollection">�������� ������ ��������</param>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    public NamedList(IEnumerable<T> srcCollection, bool ignoreCase)
      : this(srcCollection, ignoreCase, false)
    {
    }

    /// <summary>
    /// ������� ������, �������� ��� ���������� �� ���������.
    /// ���� ������ <paramref name="srcCollection"/> �������� ������� � �������������� ������ (� ������ <paramref name="ignoreCase"/>), ������� �������������
    /// </summary>
    /// <param name="srcCollection">�������� ������ ��������</param>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    /// <param name="isReadOnly">���� true, �� ������ ����� ����������� � ����� "������ ������"</param>
    public NamedList(IEnumerable<T> srcCollection, bool ignoreCase, bool isReadOnly)
      : this(ignoreCase)
    {
      foreach (T item in srcCollection)
        Add(item);

      _IsReadOnly = isReadOnly;
    }

    #endregion

    #region ������ � ���������

    /// <summary>
    /// �������� ������, ������������ ������� ���������
    /// </summary>
    private readonly List<T> _List;

    /// <summary>
    /// ��������� �� �����.
    /// ���� IgnoreCase=true, �� ���� ������������ � �������� ��������
    /// ��������� �������� ������ �������� � ������ FList.
    /// ����� FDictIsValid=false, �������� � FDict ���������������. ����� ������������� ������
    /// </summary>
    [NonSerialized]
    private Dictionary<string, int> _Dict;

    /// <summary>
    /// ���� true, �� ������� _Dict �������� ���������� ��������
    /// </summary>
    [NonSerialized] // ����� �������������� ������ ��������������� �������
    private bool _DictIsValid;

    /// <summary>
    /// ��������� ������� _Dict � ���������� ���������.
    /// ������������� ��������, �����������, ��� ����� - ������.
    /// </summary>
    /// <returns></returns>
    private void ValidateDict()
    {
#if DEBUG
      CheckDictCount();
#endif

      if (_DictIsValid)
        return;

      for (int i = 0; i < _List.Count; i++)
      {
        string code = _List[i].Code;
        if (_IgnoreCase)
          code = code.ToUpperInvariant();
        _Dict[code] = i;
      }

#if DEBUG
      CheckDictCount(); // ����� � ������� ���� ���������� �����. ����� ������ ����� FDict.Count > FList.Count
#endif

      _DictIsValid = true;
    }

#if DEBUG

    private void CheckDictCount()
    {
      if (_Dict.Count != _List.Count)
        throw new BugException("����� ������ (" + _List.Count.ToString() + ") �� ��������� � ������ ������� �������� (" + _Dict.Count.ToString() + ")");
    }

#endif

    /// <summary>
    /// ������ �� �������
    /// </summary>
    /// <param name="index">������ �������� � �������. ������ ���� � ��������� �� 0 �� Count-1</param>
    /// <returns>�������</returns>
    public T this[int index]
    {
      get { return _List[index]; }
      set
      {
        CheckNotReadOnly();
#if DEBUG
        if (Object.Equals(value, default(T)))
          throw new ArgumentNullException();
#endif
        string newCode = value.Code;
        if (String.IsNullOrEmpty(newCode))
          throw new ArgumentException("value.Code ������");
        if (_IgnoreCase)
          newCode = newCode.ToUpperInvariant();


        T oldItem = _List[index];
        _Dict.Remove(oldItem.Code);
        try
        {
          _Dict.Add(newCode, index);
        }
        catch
        {
          string oldItemCode = oldItem.Code;
          if (_IgnoreCase)
            oldItemCode = oldItemCode.ToUpperInvariant();
          _Dict.Add(oldItemCode, index);
          _DictIsValid = false;
          throw;
        }
        _List[index] = value;
      }
    }

    /// <summary>
    /// ������ �� ����.
    /// ���� �������� �������������� ���, ������������ ������ �������
    /// </summary>
    /// <param name="code">��� �������</param>
    /// <returns>������ ��� ������ ��������, ���� � ������ ��� ������� � ����� �����</returns>
    public T this[string code]
    {
      get
      {
        int index = IndexOf(code); // ��������������� �������
        if (index >= 0)
          return _List[index];
        else
          return default(T);
      }
    }

    /// <summary>
    /// ���������� ������� � �������� �����.
    /// � ������� �� ���������������� ������� �� ����, ���� �� ������ ������ � �������� �����, ������������ ����������
    /// </summary>
    /// <param name="code">��� �������</param>
    /// <returns>������</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">���� �� ������ ������ � �������� �����</exception>
    public T GetRequired(string code)
    {
      int index = IndexOf(code); // ��������������� �������
      if (index >= 0)
        return _List[index];
      else if (string.IsNullOrEmpty(code))
        throw new ArgumentNullException("code");
      else
        throw new KeyNotFoundException("� ������ ��� �������� � ����� \"" + code + "\"");
    }

    /// <summary>
    /// ���������� ���������� ��������� � ������
    /// </summary>
    public int Count { get { return _List.Count; } }

    /// <summary>
    /// ��������� ������������� "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// ���� ����������� � true, �� ��� ������ ��������� ����� �������������� �������.
    /// ���� �������� ����������� � false (�� ���������), �� ������� �������� ����������
    /// �������� ��������������� � ������������
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private readonly bool _IgnoreCase;

    #endregion

    #region ������ ������ ��� ������

    /// <summary>
    /// ���������� true, ���� ������ ��� ��������� � ����� "������ ������"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// ���������� ����� ��� �������� ������ � ����� "������ ������"
    /// </summary>
    internal protected void SetReadOnly()
    {
      if (_IsReadOnly)
        return; // ��������� ����� ������������

      ValidateDict();
      _IsReadOnly = true;
    }

    /// <summary>
    /// ���������� ����������, ���� IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// ���������� ������������� �������� � ������.
    /// 
    /// ��� ������������� �������� ����� ���������� � �������, 
    /// ������������� ������ ���������� ���������� �������������.
    /// ������� � ���������� ���� ����� ������ �������������� ������������� ��� ������������� � ��������� foreach.
    /// </summary>
    /// <returns>�������������</returns>
    public List<T>.Enumerator GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    #endregion

    #region IList<T> Members

    /// <summary>
    /// ���������� ������ ������� � ������ ��� (-1), ���� ������ �� ������.
    /// ���� ��������� ������ ��������� ������� �������� � ����� �����, ������������� ������������
    /// ����� Contains(), ����������� ��������� ���.
    /// ����� ������������� ������������ ���������� ������ IndexOf(), ����������� ���, ����� �������� ������� ��������� ��������, ������� ������ �� �����.
    /// </summary>
    /// <param name="item">������ ��� ������</param>
    /// <returns>������ �������.</returns>
    public int IndexOf(T item)
    {
      if (Object.Equals(item, default(T)))
        return -1;

      int p = IndexOf(item.Code);
      if (p < 0)
        return -1;

      if (_List[p].Equals(item))
        return p;
      else
        return -1; // ���� ���������, � ������� - ������
    }

    /// <summary>
    /// ��������� ������� � �������� ������� ������.
    /// ���� � ������ ��� ���� ������� � ����� ����� (� ������ IgnoreCase), ������������ ����������
    /// </summary>
    /// <param name="index">������� ��� ����������</param>
    /// <param name="item">����������� ������</param>
    public void Insert(int index, T item)
    {
      CheckNotReadOnly();
#if DEBUG
      if (Object.Equals(item, default(T)))
        throw new ArgumentNullException("item");
#endif
      if (index > _List.Count)
        throw new ArgumentOutOfRangeException("index", index, "������ ������ ���� � ��������� �� 0 �� " + _List.Count.ToString());

      string itemCode = item.Code;
      if (String.IsNullOrEmpty(itemCode))
        throw new ArgumentException("������ Item.Code", "item");
      if (IgnoreCase)
        itemCode = itemCode.ToUpperInvariant();

      if (index < _List.Count)
        _DictIsValid = false; // ����� ��� ������� ����� Add()

      _Dict.Add(itemCode, index); // ����� ��������� ����������, ���� ��� ���� ������� � ����� �����
      try
      {
        _List.Insert(index, item);
      }
      catch
      {
        _DictIsValid = false;
        _Dict.Remove(itemCode);
        throw;
      }
    }

    /// <summary>
    /// ������� ������� �� ��������� ������� ������.
    /// </summary>
    /// <param name="index">������ ��������</param>
    public void RemoveAt(int index)
    {
      CheckNotReadOnly();

      if (index < (_List.Count - 1))
        _DictIsValid = false; // ����� ��������� ��������� ������� � ������� �������� ��������

      T item = _List[index];
      _List.RemoveAt(index);

      string itemCode = item.Code;
      if (IgnoreCase)
        itemCode = itemCode.ToUpperInvariant();

      _Dict.Remove(itemCode);
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// ��������� ������� � ����� ������.
    /// ���� � ������ ��� ���� ������� � ����� ����� (� ������ IgnoreCase), ������������ ����������
    /// </summary>
    /// <param name="item">����������� �������</param>
    public void Add(T item)
    {
      CheckNotReadOnly();
#if DEBUG
      if (Object.Equals(item, default(T)))
        throw new ArgumentNullException("item");
#endif

      string itemCode = item.Code;
      if (String.IsNullOrEmpty(itemCode))
        throw new ArgumentException("������ Item.Code", "item");
      if (IgnoreCase)
        itemCode = itemCode.ToUpperInvariant();

      _Dict.Add(itemCode, _List.Count); // ����� ��������� ����������, ���� ��� ���� ������� � ����� �����
      try
      {
        _List.Add(item);
      }
      catch
      {
        _DictIsValid = false;
        _Dict.Remove(itemCode);
        throw;
      }

      // ������� �������� ��������, ���� �� ��� �������� �� �����
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _List.Clear();
      _Dict.Clear();
      _DictIsValid = true; // ������ ������� ����� ��������
    }

    /// <summary>
    /// ��������� ������� ����� � ������ �������� � ����� �����.
    /// ������������� ������������ ����������, ����������� ��������� ���.
    /// </summary>
    /// <param name="item">������� �������</param>
    /// <returns>true, ���� ������� ������</returns>
    public bool Contains(T item)
    {
      if (Object.Equals(item, default(T)))
        return false;

      string itemCode = item.Code;
      if (IgnoreCase)
        itemCode = itemCode.ToUpperInvariant();
      if (!_Dict.ContainsKey(itemCode))
        return false;

      // 21.02.2018
      // ���� ��� � �� ��������� ���������
      return item.Equals(this[itemCode]);
    }

    /// <summary>
    /// �������� ���� ������ � ������.
    /// </summary>
    /// <param name="array">����������� ������</param>
    public void CopyTo(T[] array)
    {
      _List.CopyTo(array);
    }

    /// <summary>
    /// �������� ���� ������ � ������.
    /// </summary>
    /// <param name="array">����������� ������</param>
    /// <param name="arrayIndex">��������� ������ � ������� ��� ����������</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      _List.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// �������� ����� ������ � ������.
    /// </summary>
    /// <param name="index">������ ������� �������� � ������� ������, � �������� �������� �����������</param>
    /// <param name="array">����������� ������</param>
    /// <param name="arrayIndex">��������� ������ � ������� ��� ����������</param>
    /// <param name="count">���������� ���������, ������� ����� �����������</param>
    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
      _List.CopyTo(index, array, arrayIndex, count);
    }

    /// <summary>
    /// ������� �������� ������� �� ������
    /// </summary>
    /// <param name="item">��������� �������</param>
    /// <returns>true, ���� ������� ��� ������ � ������</returns>
    public bool Remove(T item)
    {
      CheckNotReadOnly();

      int p = IndexOf(item); // � ��� ����� � ��������� ���������, � �� ������ ����
      if (p >= 0)
      {
        RemoveAt(p);
        return true;
      }
      else
        return false;
    }

    #endregion

    #region �������������� ������

    /// <summary>
    /// ����� �� ���� (� ������ IgnoreCase).
    /// ���������� ������ ���������� �������� ��� (-1).
    /// ���� ��������� ������ ���������� ������������� �������� � �������� �����, �����������
    /// Contains(), ����������� ��������� ��������.
    /// </summary>
    /// <param name="code">������� ���</param>
    /// <returns>������ �������</returns>
    public int IndexOf(string code)
    {
      if (String.IsNullOrEmpty(code))
        return -1;

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      ValidateDict();
      int p;
      if (_Dict.TryGetValue(code, out p))
        return p;
      else
        return -1;
    }

    /// <summary>
    /// ��������� ����� �� ���� (� ������ IgnoreCase).
    /// ���������� ������ ���������� �������� ��� (-1)
    /// ���� ��������� ������ ���������� ������������� �������� � �������� �����, �����������
    /// Contains(), ����������� ��������� ��������.
    /// </summary>
    /// <param name="code">������� ���</param>
    /// <returns>������ �������</returns>
    private int SlowIndexOf(string code)
    {
      if (IgnoreCase)
      {
        for (int i = 0; i < _List.Count; i++)
        {
          if (String.Equals(_List[i].Code, code, StringComparison.OrdinalIgnoreCase))
            return i;
        }
      }
      else
      {
        for (int i = 0; i < _List.Count; i++)
        {
          if (_List[i].Code == code)
            return i;
        }
      }
      return -1;
    }

    /// <summary>
    /// ������� ����� ���� (� ������ IgnoreCase).
    /// ���������� true, ���� � ������ ���� ������� � �������� �����
    /// </summary>
    /// <param name="code">������� ���</param>
    /// <returns>������� �������� � ������</returns>
    public bool Contains(string code)
    {
      if (String.IsNullOrEmpty(code))
        return false;

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      // �������, ����� ������� ��� ���

      return _Dict.ContainsKey(code);
    }

    /// <summary>
    /// ������� ����� ���� (� ������ IgnoreCase).
    /// ���������� true, ���� � ������ ���� ������� � �������� �����.
    /// ��� ���� ����� ������������ � �������.
    /// ���� � ������ ��� �������� � ����� �����, ������������ false, 
    /// � Value �������� ������ ��������
    /// </summary>
    /// <param name="code">������� ���</param>
    /// <param name="value">���� ���������� ��������� ��������</param>
    /// <returns>������� �������� � ������</returns>
    public bool TryGetValue(string code, out T value)
    {
      if (String.IsNullOrEmpty(code))
      {
        value = default(T);
        return false;
      }

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      ValidateDict();
      int p;
      if (_Dict.TryGetValue(code, out p))
      {
        value = _List[p];
        return true;
      }
      else
      {
        value = default(T);
        return false;
      }
    }

    /// <summary>
    /// �������� ������ � ������
    /// </summary>
    /// <returns>����� ������</returns>
    public T[] ToArray()
    {
      return _List.ToArray();
    }

    /// <summary>
    /// ��������� ��������� ��������� � ������.
    /// ������������ ����������������� ������ ������ Add()
    /// </summary>
    /// <param name="collection">������ ����������� ���������</param>
    public void AddRange(IEnumerable<T> collection)
    {
      CheckNotReadOnly();
#if DEBUG
      if (collection==null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("������ �������� �������� �� ������ ����", "collection");

      foreach (T item in collection)
        Add(item);
    }

    /// <summary>
    /// ������� ������� � �������� ����� (� ������ IgnoreCase)
    /// </summary>
    /// <param name="code">��� ���������� ��������</param>
    /// <returns>true, ���� ������ ��� ������ � ������</returns>
    public bool Remove(string code)
    {
      CheckNotReadOnly();

      int p;
      if (_DictIsValid)
        p = IndexOf(code);
      else // �� �������� ������ ������� ������ ���, ����� �� ���������.
        p = SlowIndexOf(code);
      if (p >= 0)
      {
        RemoveAt(p);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// ���������� ��������� ������ � ������ ���������.
    /// ������� ������������� ������������ ��������� � ������.
    /// ������� �������� �� ��������, ���� ���� IgnoreCase=true.
    /// </summary>
    /// <returns></returns>
    public string[] GetCodes()
    {
      string[] a = new string[_List.Count];
      for (int i = 0; i < _List.Count; i++)
        a[i] = _List[i].Code;

      return a;
    }


    /// <summary>
    /// ���������� ������ �����.
    /// ��� ���������� ������� �������� ����������� ��� ������������, � ����������� �� �������� IgnoreCase
    /// </summary>
    public void Sort()
    {
      CheckNotReadOnly();

      _DictIsValid = false;

      if (_IgnoreCase)
        _List.Sort(ObjectWithCodeComparer<T>.OrdinalIgnoreCase);
      else
        _List.Sort(ObjectWithCodeComparer<T>.Ordinal);
    }

    /// <summary>
    /// �������� ������� ��������� �� ��������
    /// </summary>
    public void Reverse()
    {
      CheckNotReadOnly();

      _DictIsValid = false;

      _List.Reverse();
    }

    #endregion

    #region ��������������

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      _Dict = new Dictionary<string, int>(_List.Count);
      for (int i = 0; i < _List.Count; i++)
      {
        string itemCode = _List[i].Code;
        if (IgnoreCase)
          itemCode = itemCode.ToUpperInvariant();

        _Dict.Add(itemCode, i);
      }
      _DictIsValid = true;
    }

    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    string[] INamedValuesAccess.GetNames()
    {
      return GetCodes();
    }

    #endregion
  }
#endif

  /// <summary>
  /// ��������� ������� NamedListWithNotifications.BeforeAdd, AfterAdd, BeforeRemove � AfterRemove
  /// </summary>
  /// <typeparam name="T">��� ���������, ���������� � ������ NamedListWithNotifications</typeparam>
  public sealed class NamedListItemEventArgs<T> : EventArgs
    where T : IObjectWithCode
  {
    #region �����������

    /// <summary>
    /// ������� ��������� �������
    /// </summary>
    /// <param name="item">����������� ��� ��������� ������� ���������</param>
    public NamedListItemEventArgs(T item)
    {
      _Item = item;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����������� ��� ��������� ������� ���������
    /// </summary>
    public T Item { get { return _Item; } }
    private readonly T _Item;

    #endregion
  }

  /// <summary>
  /// ������� ������� NamedListWithNotifications.ItemAdded � ItemRemoved
  /// </summary>
  /// <typeparam name="T">��� ���������, ���������� � ������ NamedListWithNotifications</typeparam>
  /// <param name="sender">������ ������</param>
  /// <param name="args">��������� �������</param>
  public delegate void NamedListItemEventHandler<T>(object sender, NamedListItemEventArgs<T> args)
    where T : IObjectWithCode;

#if OLD
  /// <summary>
  /// ������ �������� ������������� ����, ������ � ������� ����� �������������� ��� ��
  /// ������� (��� � ������� ������ List), ��� � �� ����, ��� � Dictionary � ������ String.
  /// ������ �� ����� ��������� �������� null.
  /// � ������� �� NamedList, ��� ���������� � �������� ��������� ������ ���������� ����������� ������ � �������,
  /// ������� ���� ������ �������� ���������.
  /// � ������� �� NamedList, ���� ����� �� �������� ����������������.
  /// </summary>
  /// <typeparam name="T">��� ��������, ���������� � ������, �������������� ��������� IObjectWithCode</typeparam>
  [Serializable]
  public class NamedListWithNotifications<T> : IEnumerable<T>, IList<T>, IReadOnlyObject, INamedValuesAccess
    where T : IObjectWithCode
  {
    #region ������������

    // � ������� �� NamedList, � ����� ������ ��� �������������, ����������� ��������� ������.
    // �������������� ������� �����-���� ��������� � ����������� ������ ��� ������������� ������������ �������.

    /// <summary>
    /// ������� ������ ������.
    /// ������� ���� �����������
    /// </summary>
    public NamedListWithNotifications()
      : this(false)
    {
    }

    /// <summary>
    /// ������� ������ ������
    /// </summary>
    /// <param name="IgnoreCase">���� �� ������������ ������� ����</param>
    public NamedListWithNotifications(bool IgnoreCase)
    {
      FList = new List<T>();
      FDict = new Dictionary<string, T>();
      FIgnoreCase = IgnoreCase;
    }

    /// <summary>
    /// ������� ������ ������ �������� �������.
    /// ������� ���� �����������.
    /// </summary>
    /// <param name="Capacity">��������� ������� ������</param>
    public NamedListWithNotifications(int Capacity)
      : this(Capacity, false)
    {
    }

    /// <summary>
    /// ������� ������ ������ �������� �������
    /// </summary>
    /// <param name="Capacity">��������� ������� ������</param>
    /// <param name="IgnoreCase">���� �� ������������ ������� ����</param>
    public NamedListWithNotifications(int Capacity, bool IgnoreCase)
    {
      FList = new List<T>(Capacity);
      FDict = new Dictionary<string, T>(Capacity);
      FIgnoreCase = IgnoreCase;
    }

    #endregion

    #region ������ � ���������

    /// <summary>
    /// �������� ������, ������������ ������� ���������
    /// </summary>
    private List<T> FList;

    /// <summary>
    /// ��������� �� �����.
    /// ���� IgnoreCase=true, �� ���� ������������ � �������� ��������
    /// </summary>
    [NonSerialized]
    private Dictionary<string, T> FDict;

    /// <summary>
    /// ������ �� �������
    /// </summary>
    /// <param name="Index">������ �������� � �������. ������ ���� � ��������� �� 0 �� Count-1</param>
    /// <returns></returns>
    public T this[int Index]
    {
      get { return FList[Index]; }
      set
      {
        CheckNotReadOnly();

        T OldItem = FList[Index];

        OnBeforeAdd(value);
        OnBeforeRemove(OldItem);

        FDict.Remove(OldItem.Code);
        try
        {
          string NewCode = value.Code;
          if (FIgnoreCase)
            NewCode = NewCode.ToUpperInvariant();
          FDict.Add(NewCode, value);
        }
        catch
        {
          string OldItemCode = OldItem.Code;
          if (FIgnoreCase)
            OldItemCode = OldItemCode.ToUpperInvariant();
          FDict.Add(OldItemCode, OldItem);
          throw;
        }
        FList[Index] = value;

        OnAfterAdd(value);
        OnAfterRemove(OldItem);

        CallListChanged(ListChangedType.ItemChanged, Index);
      }
    }

    /// <summary>
    /// ������ �� ����.
    /// ���� �������� �������������� ���, ������������ ������ �������
    /// </summary>
    /// <param name="Code">��� �������</param>
    /// <returns>������ ��� ������ ��������, ���� � ������ ��� ������� � ����� �����</returns>
    public T this[string Code]
    {
      get
      {
        T res;
        if (string.IsNullOrEmpty(Code))
          return default(T);

        if (IgnoreCase)
          Code = Code.ToUpperInvariant();

        if (FDict.TryGetValue(Code, out res))
          return res;
        else
          return default(T);
      }
    }

    /// <summary>
    /// ���������� ������� � �������� �����.
    /// � ������� �� ���������������� ������� �� ����, ���� �� ������ ������ � �������� �����, ������������ ����������
    /// </summary>
    /// <param name="Code">��� �������</param>
    /// <returns>������</returns>
    public T GetRequired(string Code)
    {
      T res;
      if (string.IsNullOrEmpty(Code))
        throw new ArgumentNullException("Code");

      if (IgnoreCase)
        Code = Code.ToUpperInvariant();

      if (FDict.TryGetValue(Code, out res))
        return res;
      else
        throw new ArgumentException("� ������ ��� �������� � ����� \"" + Code + "\"");
    }

    /// <summary>
    /// ���������� ���������� ��������� � ������
    /// </summary>
    public int Count { get { return FList.Count; } }

    /// <summary>
    /// ��������� ������������� "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// ���� ����������� � true, �� ��� ������ ��������� ����� �������������� �������.
    /// ���� �������� ����������� � false (�� ���������), �� ������� �������� ����������
    /// �������� ��������������� � ������������
    /// </summary>
    public bool IgnoreCase { get { return FIgnoreCase; } }
    private bool FIgnoreCase;

    #endregion

    #region ������ ������ ��� ������

    /// <summary>
    /// ���������� true, ���� ������ ��� ��������� � ����� "������ ������"
    /// </summary>
    public bool IsReadOnly { get { return FIsReadOnly; } }
    private bool FIsReadOnly;

    /// <summary>
    /// ���������� ����� ��� �������� ������ � ����� "������ ������"
    /// </summary>
    internal protected void SetReadOnly()
    {
      FIsReadOnly = true;
    }

    /// <summary>
    /// ���������� ����������, ���� IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (FIsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// ���������� ������������� �������� � ������
    /// </summary>
    /// <returns>�������������</returns>
    public IEnumerator<T> GetEnumerator()
    {
      return FList.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return FList.GetEnumerator();
    }

    #endregion

    #region IList<T> Members

    /// <summary>
    /// ���������� ������ ������� � ������ ��� (-1), ���� ������ �� ������.
    /// ����� �������� ���������.
    /// ���� ��������� ������ ��������� ������� �������� � ����� �����, ������������� ������������
    /// ����� Contains(), ����������� ��������� ���
    /// </summary>
    /// <param name="item">������ ��� ������</param>
    /// <returns>������ �������.</returns>
    public int IndexOf(T item)
    {
      return FList.IndexOf(item);
    }

    /// <summary>
    /// ��������� ������� � �������� ������� ������.
    /// ���� � ������ ��� ���� ������� � ����� ����� (� ������ IgnoreCase), ������������ ����������
    /// </summary>
    /// <param name="index">������� ��� ����������</param>
    /// <param name="item">����������� ������</param>
    public void Insert(int index, T item)
    {
      CheckNotReadOnly();

      OnBeforeAdd(item);

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      FDict.Add(ItemCode, item);
      try
      {
        FList.Insert(index, item);
      }
      catch
      {
        FDict.Remove(ItemCode);
        throw;
      }

      OnAfterAdd(item);
      CallListChanged(ListChangedType.ItemAdded, index);
    }

    /// <summary>
    /// ������� ������� �� ��������� ������� ������.
    /// </summary>
    /// <param name="index">������ ��������</param>
    public void RemoveAt(int index)
    {
      CheckNotReadOnly();

      T item = FList[index];

      OnBeforeRemove(item);

      FList.RemoveAt(index);

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      FDict.Remove(ItemCode);

      OnAfterRemove(item);
      ListChangedEventArgs Args = new ListChangedEventArgs(ListChangedType.ItemDeleted, index);
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// ��������� ������� � ����� ������.
    /// ���� � ������ ��� ���� ������� � ����� ����� (� ������ IgnoreCase), ������������ ����������
    /// </summary>
    /// <param name="item">����������� �������</param>
    public void Add(T item)
    {
      CheckNotReadOnly();

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      OnBeforeAdd(item);

      FDict.Add(ItemCode, item);
      try
      {
        FList.Add(item);
      }
      catch
      {
        FDict.Remove(ItemCode);
        throw;
      }

      OnAfterAdd(item);
      CallListChanged(ListChangedType.ItemAdded, FList.Count - 1);
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      if (Count == 0)
        return;

      T[] Items = ToArray();
      for (int i = 0; i < Items.Length; i++)
        OnBeforeRemove(Items[i]);

      FList.Clear();
      FDict.Clear();

      for (int i = 0; i < Items.Length; i++)
        OnAfterRemove(Items[i]);

      CallListChanged(ListChangedType.Reset, -1);
    }

    /// <summary>
    /// ��������� ������� ����� � ������ �������� � ����� �����.
    /// ������������� ������������ ����������, ����������� ��������� ���.
    /// </summary>
    /// <param name="item">������� �������</param>
    /// <returns>true, ���� ������� ������</returns>
    public bool Contains(T item)
    {
      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();
      // return FDict.ContainsKey(ItemCode);

      // 21.02.2018
      // ���� ��� � �� ��������� ���������
      T ResItem;
      if (FDict.TryGetValue(ItemCode, out ResItem))
        return item.Equals(ResItem);
      else
        return false;
    }

    /// <summary>
    /// �������� ������ � ������
    /// </summary>
    /// <param name="array">����������� ������</param>
    /// <param name="arrayIndex">��������� ������ � ������� ��� ����������</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      FList.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// ������� �������� ������� �� ������
    /// </summary>
    /// <param name="item">��������� �������</param>
    /// <returns>true, ���� ������� ��� ������ � ������</returns>
    public bool Remove(T item)
    {
      CheckNotReadOnly();

      if (!Contains(item)) // ������� ��������
        return false;

      int p = IndexOf(item);
      RemoveAt(p);
      return true;
    }

    #endregion

    #region �������������� ������

    /// <summary>
    /// ��������� ����� �� ���� (� ������ IgnoreCase).
    /// ���������� ������ ���������� �������� ��� (-1)
    /// ���� ��������� ������ ���������� ������������� �������� � �������� �����, �����������
    /// Contains(), ����������� ��������� ��������.
    /// </summary>
    /// <param name="Code">������� ���</param>
    /// <returns>������ �������</returns>
    public int IndexOf(string Code)
    {
      if (IgnoreCase)
      {
        for (int i = 0; i < FList.Count; i++)
        {
          if (String.Equals(FList[i].Code, Code, StringComparison.OrdinalIgnoreCase))
            return i;
        }
      }
      else
      {
        for (int i = 0; i < FList.Count; i++)
        {
          if (FList[i].Code == Code)
            return i;
        }
      }
      return -1;
    }

    /// <summary>
    /// ������� ����� ���� (� ������ IgnoreCase).
    /// ���������� true, ���� � ������ ���� ������� � �������� �����
    /// </summary>
    /// <param name="Code">������� ���</param>
    /// <returns>������� �������� � ������</returns>
    public bool Contains(string Code)
    {
      if (String.IsNullOrEmpty(Code))
        return false;

      if (IgnoreCase)
        Code = Code.ToUpperInvariant();

      return FDict.ContainsKey(Code);
    }

    /// <summary>
    /// ������� ����� ���� (� ������ IgnoreCase).
    /// ���������� true, ���� � ������ ���� ������� � �������� �����.
    /// ��� ���� ����� ������������ � �������.
    /// ���� � ������ ��� �������� � ����� �����, ������������ false, 
    /// � Value �������� ������ ��������
    /// </summary>
    /// <param name="Code">������� ���</param>
    /// <param name="Value">���� ���������� ��������� ��������</param>
    /// <returns>������� �������� � ������</returns>
    public bool TryGetValue(string Code, out T Value)
    {
      if (String.IsNullOrEmpty(Code))
      {
        Value = default(T);
        return false;
      }

      if (IgnoreCase)
        Code = Code.ToUpperInvariant();

      return FDict.TryGetValue(Code, out Value);
    }

    /// <summary>
    /// �������� ������ � ������
    /// </summary>
    /// <returns>����� ������</returns>
    public T[] ToArray()
    {
      return FList.ToArray();
    }

    /// <summary>
    /// ��������� ��������� ��������� � ������.
    /// ������������ ����������������� ������ ������ Add()
    /// </summary>
    /// <param name="Collection">������ ����������� ���������</param>
    public void AddRange(IEnumerable<T> Collection)
    {
      CheckNotReadOnly();
#if DEBUG
      if (collection==null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("������ �������� �������� �� ������ ����", "collection");

      BeginUpdate();
      try
      {
        foreach (T Item in Collection)
          Add(Item);
      }
      finally
      {
        EndUpdate();
      }
    }

    /// <summary>
    /// ������� ������� � �������� ����� (� ������ IgnoreCase)
    /// </summary>
    /// <param name="Code">��� ���������� ��������</param>
    /// <returns>true, ���� ������ ��� ������ � ������</returns>
    public bool Remove(string Code)
    {
      CheckNotReadOnly();

      if (!Contains(Code)) // ������� ��������
        return false;

      int p = IndexOf(Code);
      RemoveAt(p);
      return true;
    }

    /// <summary>
    /// ���������� ��������� ������ � ������ ���������.
    /// ������� ������������� ������������ ��������� � ������.
    /// ������� �������� �� ��������, ���� ���� IgnoreCase=true.
    /// </summary>
    /// <returns></returns>
    public string[] GetCodes()
    {
      string[] a = new string[FList.Count];
      for (int i = 0; i < FList.Count; i++)
        a[i] = FList[i].Code;

      return a;
    }

    #endregion

  #region ��������� ��� ���������� � ������

    /// <summary>
    /// ������� ���������� ����� ����������� ��������.
    /// ���� ���������� ������� �������� ����������, ������ ��������� � ���������� � ������������� ���������.
    /// ������� �� ����������, ���� ���� �������� ����� BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> BeforeAdd;

    /// <summary>
    /// �������� ������� BeforeAdd, ���� ��� ��������� BeginUpdate().
    /// </summary>
    /// <param name="Item">����������� �������</param>
    protected virtual void OnBeforeAdd(T Item)
    {
      if (FUpdateCount == 0 && BeforeAdd != null)
      {
        NamedListItemEventArgs<T> Args = new NamedListItemEventArgs<T>(Item);
        BeforeAdd(this, Args);
      }
    }

    /// <summary>
    /// ������� ���������� ����� ���������� ��������.
    /// ���� ���������� ������� �������� ����������, ������ �������� � ��������������� ��������� � �� ����� �������������� ������.
    /// ������� �� ����������, ���� ���� �������� ����� BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> AfterAdd;

    /// <summary>
    /// �������� ������� AfterAdd, ���� ��� ��������� BeginUpdate().
    /// </summary>
    /// <param name="Item">����������� �������</param>
    protected virtual void OnAfterAdd(T Item)
    {
      if (FUpdateCount == 0 && AfterAdd != null)
      {
        NamedListItemEventArgs<T> Args = new NamedListItemEventArgs<T>(Item);
        AfterAdd(this, Args);
      }
    }

    /// <summary>
    /// ������� ���������� ����� ��������� ��������.
    /// ���� ���������� ������� �������� ����������, ������ ��������� � ���������� � ������������� ���������.
    /// ������� �� ����������, ���� ���� �������� ����� BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> BeforeRemove;

    /// <summary>
    /// �������� ������� BeforeRemove, ���� ��� ��������� BeginUpdate().
    /// </summary>
    /// <param name="Item">��������� �������</param>
    protected virtual void OnBeforeRemove(T Item)
    {
      if (FUpdateCount == 0 && BeforeRemove != null)
      {
        NamedListItemEventArgs<T> Args = new NamedListItemEventArgs<T>(Item);
        BeforeRemove(this, Args);
      }
    }

    /// <summary>
    /// ������� ���������� ����� �������� ��������.
    /// ���� ���������� ������� �������� ����������, ������ �������� � ��������������� ��������� � �� ����� �������������� ������.
    /// ������� �� ����������, ���� ���� �������� ����� BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> AfterRemove;

    /// <summary>
    /// �������� ������� AfterRemove, ���� ��� ��������� BeginUpdate().
    /// </summary>
    /// <param name="Item">����������� �������</param>
    protected virtual void OnAfterRemove(T Item)
    {
      if (FUpdateCount == 0 && AfterRemove != null)
      {
        NamedListItemEventArgs<T> Args = new NamedListItemEventArgs<T>(Item);
        AfterRemove(this, Args);
      }
    }

    /// <summary>
    /// ������� ���������� ��� ���������� � ������.
    /// </summary>
    public event ListChangedEventHandler ListChanged;

    /// <summary>
    /// �������� ������� ListChanged, ���� ��� ��������� ������ BeginUpdate()
    /// </summary>
    /// <param name="Args">��������� �������</param>
    protected void OnListChanged(ListChangedEventArgs Args)
    {
#if DEBUG
      if (Args == null)
        throw new ArgumentNullException("Args");
#endif

      if (InsideOnListChanged)
        throw new ReenterException("��������� ����� ListChanged");

      if (FUpdateCount > 0)
        DelayedListChanged = true; // ��������� �� �������
      else if (ListChanged != null)
      {
        InsideOnListChanged = true;
        try
        {
          ListChanged(this, Args);
        }
        finally
        {
          InsideOnListChanged = false;
        }
      }
    }

    private bool InsideOnListChanged;

    private void CallListChanged(ListChangedType ListChangedType, int NewIndex)
    {
      ListChangedEventArgs Args = new ListChangedEventArgs(ListChangedType, NewIndex);
      OnListChanged(Args);
    }

    /// <summary>
    /// �������� ������� ListChanged � ListChangedType=ItemChanged
    /// </summary>
    /// <param name="Index">������ ��������. ������ ���� � ��������� �� 0 �� (Count-1)</param>
    public void NotifyItemChanged(int Index)
    {
      if (Index < 0 || Index >= Count)
        throw new ArgumentOutOfRangeException("Index", Index, "������ �������� ��� ���������");

      CallListChanged(ListChangedType.ItemChanged, Index);
    }

    #endregion

  #region ������������ �������� ���������

    /// <summary>
    /// ����� ������ ������ ��������� ���������� ��������� BeforeAdd, AfterAdd, BeforeRemove � AfterRemove.
    /// ������ ����������� ����������� ������ ������� EndUpdate().
    /// </summary>
    public virtual void BeginUpdate()
    {
      if (FUpdateCount == 0)
        DelayedListChanged = false; // �� ����, ������� �� ������ �����������
      FUpdateCount++;
    }

    /// <summary>
    /// ��������� ���������� ������.
    /// ����� ������ ���� ������, �� ��������� � BeginUpdate()
    /// </summary>
    public virtual void EndUpdate()
    {
      if (FUpdateCount <= 0)
        throw new InvalidOperationException("�������� ����� EndUpdate()");

      FUpdateCount--;
      if (FUpdateCount == 0 && DelayedListChanged)
      {
        CallListChanged(ListChangedType.Reset, -1);
        DelayedListChanged = false;
      }
    }

    /// <summary>
    /// ���������� true, ���� ��� �������� ����� ������ BeginUpdate
    /// </summary>
    public bool IsUpdating { get { return FUpdateCount > 0; } }
    private int FUpdateCount;

    /// <summary>
    /// ��������������� � true ��� ����� ���������� � ������, ���� ��� �������� ����� BeginUpdate().
    /// � ���� ������, ����� EndUpdate() ���������� ������ � ������ ���������� ������
    /// </summary>
    private bool DelayedListChanged;

    #endregion

  #region ��������������

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      FDict = new Dictionary<string, T>(FList.Count);
      for (int i = 0; i < FList.Count; i++)
      {
        string ItemCode = FList[i].Code;
        if (IgnoreCase)
          ItemCode = ItemCode.ToUpperInvariant();

        FDict.Add(ItemCode, FList[i]);
      }
    }

    #endregion

  #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string Name)
    {
      return this[Name];
    }

    string[] INamedValuesAccess.GetNames()
    {
      return GetCodes();
    }

    #endregion
  }
#else

  /// <summary>
  /// ������ �������� ������������� ����, ������ � ������� ����� �������������� ��� ��
  /// ������� (��� � ������� ������ List), ��� � �� ����, ��� � Dictionary � ������ String.
  /// ������ �� ����� ��������� �������� null.
  /// � ������� �� NamedList, ��� ���������� � �������� ��������� ������ ���������� ����������� ������ � �������,
  /// ������� ���� ������ �������� ���������.
  /// � ������� �� NamedList, ���� ����� �� �������� ����������������.
  /// </summary>
  /// <typeparam name="T">��� ��������, ���������� � ������, �������������� ��������� IObjectWithCode</typeparam>
  [Serializable]
  public class NamedListWithNotifications<T> : IEnumerable<T>, IList<T>, IReadOnlyObject, INamedValuesAccess
    where T : class, IObjectWithCode
  {
    #region ������������

    // � ������� �� NamedList, � ����� ������ ��� �������������, ����������� ��������� ������.
    // �������������� ������� �����-���� ��������� � ����������� ������ ��� ������������� ������������ �������.

    /// <summary>
    /// ������� ������ ������.
    /// ������� ���� �����������
    /// </summary>
    public NamedListWithNotifications()
      : this(false)
    {
    }

    /// <summary>
    /// ������� ������ ������
    /// </summary>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    public NamedListWithNotifications(bool ignoreCase)
    {
      _List = new List<T>();
      _Dict = new Dictionary<string, int>();
      _DictIsValid = true;
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ������ ������ �������� �������.
    /// ������� ���� �����������.
    /// </summary>
    /// <param name="capacity">��������� ������� ������</param>
    public NamedListWithNotifications(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// ������� ������ ������ �������� �������
    /// </summary>
    /// <param name="capacity">��������� ������� ������</param>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    public NamedListWithNotifications(int capacity, bool ignoreCase)
    {
      _List = new List<T>(capacity);
      _Dict = new Dictionary<string, int>(capacity);
      _DictIsValid = true;
      _IgnoreCase = ignoreCase;
    }

    #endregion

    #region ������ � ���������

    /// <summary>
    /// �������� ������, ������������ ������� ���������
    /// </summary>
    private readonly List<T> _List;

    /// <summary>
    /// ��������� �� �����.
    /// ���� IgnoreCase=true, �� ���� ������������ � �������� ��������
    /// ��������� �������� ������ �������� � ������ FList.
    /// ����� FDictIsValid=false, �������� � FDict ���������������. ����� ������������� ������
    /// </summary>
    [NonSerialized]
    private Dictionary<string, int> _Dict;

    /// <summary>
    /// ���� true, �� ������� FDict �������� ���������� ��������
    /// </summary>
    [NonSerialized] // ����� �������������� ������ ��������������� �������
    private bool _DictIsValid;

    /// <summary>
    /// ��������� ������� FDict � ���������� ���������.
    /// ������������� ��������, �����������, ��� ����� - ������.
    /// </summary>
    /// <returns></returns>
    private void ValidateDict()
    {
#if DEBUG
      CheckDictCount();
#endif

      if (_DictIsValid)
        return;

      for (int i = 0; i < _List.Count; i++)
      {
        string code = _List[i].Code;
        if (_IgnoreCase)
          code = code.ToUpperInvariant();
        _Dict[code] = i;
      }

#if DEBUG
      CheckDictCount(); // ����� � ������� ���� ���������� �����. ����� ������ ����� FDict.Count > FList.Count
#endif

      _DictIsValid = true;
    }

#if DEBUG

    private void CheckDictCount()
    {
      if (_Dict.Count != _List.Count)
        throw new BugException("����� ������ (" + _List.Count.ToString() + ") �� ��������� � ������ ������� �������� (" + _Dict.Count.ToString() + ")");
    }

#endif


    /// <summary>
    /// ������ �� �������
    /// </summary>
    /// <param name="index">������ �������� � �������. ������ ���� � ��������� �� 0 �� Count-1</param>
    /// <returns></returns>
    public T this[int index]
    {
      get { return _List[index]; }
      set
      {
        CheckNotReadOnly();
#if DEBUG
        if (Object.Equals(value, default(T)))
          throw new ArgumentNullException();
#endif
        string newCode = value.Code;
        if (String.IsNullOrEmpty(newCode))
          throw new ArgumentException("value.Code ������");
        if (_IgnoreCase)
          newCode = newCode.ToUpperInvariant();

        T oldItem = _List[index];

        OnBeforeAdd(value);
        OnBeforeRemove(oldItem);

        _Dict.Remove(oldItem.Code);
        try
        {
          _Dict.Add(newCode, index);
        }
        catch
        {
          string oldItemCode = oldItem.Code;
          if (_IgnoreCase)
            oldItemCode = oldItemCode.ToUpperInvariant();
          _Dict.Add(oldItemCode, index);
          _DictIsValid = false;
          throw;
        }
        _List[index] = value;

        OnAfterAdd(value);
        OnAfterRemove(oldItem);

        CallListChanged(ListChangedType.ItemChanged, index);
      }
    }

    /// <summary>
    /// ������ �� ����.
    /// ���� �������� �������������� ���, ������������ ������ �������
    /// </summary>
    /// <param name="code">��� �������</param>
    /// <returns>������ ��� ������ ��������, ���� � ������ ��� ������� � ����� �����</returns>
    public T this[string code]
    {
      get
      {
        int index = IndexOf(code); // ��������������� �������
        if (index >= 0)
          return _List[index];
        else
          return default(T);
      }
    }

    /// <summary>
    /// ���������� ������� � �������� �����.
    /// � ������� �� ���������������� ������� �� ����, ���� �� ������ ������ � �������� �����, ������������ ����������
    /// </summary>
    /// <param name="code">��� �������</param>
    /// <returns>������</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">���� �� ������ ������ � �������� �����</exception>
    public T GetRequired(string code)
    {
      int index = IndexOf(code); // ��������������� �������
      if (index >= 0)
        return _List[index];
      else if (string.IsNullOrEmpty(code))
        throw new ArgumentNullException("code");
      else
        throw new KeyNotFoundException("� ������ ��� �������� � ����� \"" + code + "\"");
    }

    /// <summary>
    /// ���������� ���������� ��������� � ������
    /// </summary>
    public int Count { get { return _List.Count; } }

    /// <summary>
    /// ��������� ������������� "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// ���� ����������� � true, �� ��� ������ ��������� ����� �������������� �������.
    /// ���� �������� ����������� � false (�� ���������), �� ������� �������� ����������
    /// �������� ��������������� � ������������
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private readonly bool _IgnoreCase;

    #endregion

    #region ������ ������ ��� ������

    /// <summary>
    /// ���������� true, ���� ������ ��� ��������� � ����� "������ ������"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// ���������� ����� ��� �������� ������ � ����� "������ ������"
    /// </summary>
    internal protected void SetReadOnly()
    {
      if (_IsReadOnly)
        return; // ��������� ����� ������������

      ValidateDict();
      _IsReadOnly = true;
    }

    /// <summary>
    /// ���������� ����������, ���� IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// ���������� ������������� �������� � ������
    /// 
    /// ��� ������������� �������� ����� ���������� � �������, 
    /// ������������� ������ ���������� ���������� �������������.
    /// ������� � ���������� ���� ����� ������ �������������� ������������� ��� ������������� � ��������� foreach.
    /// </summary>
    /// <returns>�������������</returns>
    public List<T>.Enumerator GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    #endregion

    #region IList<T> Members

    /// <summary>
    /// ���������� ������ ������� � ������ ��� (-1), ���� ������ �� ������.
    /// ���� ��������� ������ ��������� ������� �������� � ����� �����, ������������� ������������
    /// ����� Contains(), ����������� ��������� ���.
    /// ����� ������������� ������������ ���������� ������ IndexOf(), ����������� ���, ����� �������� ������� ��������� ��������, ������� ������ �� �����.
    /// </summary>
    /// <param name="item">������ ��� ������</param>
    /// <returns>������ �������.</returns>
    public int IndexOf(T item)
    {
      if (Object.Equals(item, default(T)))
        return -1;

      int p = IndexOf(item.Code);
      if (p < 0)
        return -1;

      if (_List[p].Equals(item))
        return p;
      else
        return -1; // ���� ���������, � ������� - ������
    }

    /// <summary>
    /// ��������� ������� � �������� ������� ������.
    /// ���� � ������ ��� ���� ������� � ����� ����� (� ������ IgnoreCase), ������������ ����������
    /// </summary>
    /// <param name="index">������� ��� ����������</param>
    /// <param name="item">����������� ������</param>
    public void Insert(int index, T item)
    {
      CheckNotReadOnly();
#if DEBUG
      if (Object.Equals(item, default(T)))
        throw new ArgumentNullException("item");
#endif
      if (index > _List.Count)
        throw new ArgumentOutOfRangeException("index", index, "������ ������ ���� � ��������� �� 0 �� " + _List.Count.ToString());

      string itemCode = item.Code;
      if (String.IsNullOrEmpty(itemCode))
        throw new ArgumentException("������ Item.Code", "item");
      if (IgnoreCase)
        itemCode = itemCode.ToUpperInvariant();

      OnBeforeAdd(item);

      if (index < _List.Count)
        _DictIsValid = false; // ����� ��� ������� ����� Add()

      _Dict.Add(itemCode, index);
      try
      {
        _List.Insert(index, item);// ����� ��������� ����������, ���� ��� ���� ������� � ����� �����
      }
      catch
      {
        _DictIsValid = false;
        _Dict.Remove(itemCode);
        throw;
      }

      OnAfterAdd(item);
      CallListChanged(ListChangedType.ItemAdded, index);
    }

    /// <summary>
    /// ������� ������� �� ��������� ������� ������.
    /// </summary>
    /// <param name="index">������ ��������</param>
    public void RemoveAt(int index)
    {
      CheckNotReadOnly();

      if (index < (_List.Count - 1))
        _DictIsValid = false; // ����� ��������� ��������� ������� � ������� �������� ��������

      T item = _List[index];

      OnBeforeRemove(item);

      _List.RemoveAt(index);

      string itemCode = item.Code;
      if (IgnoreCase)
        itemCode = itemCode.ToUpperInvariant();

      _Dict.Remove(itemCode);

      OnAfterRemove(item);
      CallListChanged(ListChangedType.ItemDeleted, index);
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// ��������� ������� � ����� ������.
    /// ���� � ������ ��� ���� ������� � ����� ����� (� ������ IgnoreCase), ������������ ����������
    /// </summary>
    /// <param name="item">����������� �������</param>
    public void Add(T item)
    {
      CheckNotReadOnly();
#if DEBUG
      if (Object.Equals(item, default(T)))
        throw new ArgumentNullException("item");
#endif

      string itemCode = item.Code;
      if (String.IsNullOrEmpty(itemCode))
        throw new ArgumentException("������ Item.Code", "item");
      if (IgnoreCase)
        itemCode = itemCode.ToUpperInvariant();

      OnBeforeAdd(item);

      _Dict.Add(itemCode, _List.Count); // ����� ��������� ����������, ���� ��� ���� ������� � ����� �����
      try
      {
        _List.Add(item);
      }
      catch
      {
        _DictIsValid = false;
        _Dict.Remove(itemCode);
        throw;
      }

      // ������� �������� ��������, ���� �� ��� �������� �� �����

      OnAfterAdd(item);
      CallListChanged(ListChangedType.ItemAdded, _List.Count - 1);
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      if (Count == 0)
        return;

      T[] items = ToArray();
      for (int i = 0; i < items.Length; i++)
        OnBeforeRemove(items[i]);

      _List.Clear();
      _Dict.Clear();
      _DictIsValid = true; // ������ ������� ����� ��������

      for (int i = 0; i < items.Length; i++)
        OnAfterRemove(items[i]);

      CallListChanged(ListChangedType.Reset, -1);
    }

    /// <summary>
    /// ��������� ������� ����� � ������ �������� � ����� �����.
    /// ������������� ������������ ����������, ����������� ��������� ���.
    /// </summary>
    /// <param name="item">������� �������</param>
    /// <returns>true, ���� ������� ������</returns>
    public bool Contains(T item)
    {
      if (Object.Equals(item, default(T)))
        return false;

      string itemCode = item.Code;
      if (IgnoreCase)
        itemCode = itemCode.ToUpperInvariant();
      if (!_Dict.ContainsKey(itemCode))
        return false;

      // ���� ��� � �� ��������� ���������
      return item.Equals(this[itemCode]);
    }

    /// <summary>
    /// �������� ���� ������ � ������.
    /// </summary>
    /// <param name="array">����������� ������</param>
    public void CopyTo(T[] array)
    {
      _List.CopyTo(array);
    }

    /// <summary>
    /// �������� ���� ������ � ������.
    /// </summary>
    /// <param name="array">����������� ������</param>
    /// <param name="arrayIndex">��������� ������ � ������� ��� ����������</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      _List.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// �������� ����� ������ � ������.
    /// </summary>
    /// <param name="index">������ ������� �������� � ������� ������, � �������� �������� �����������</param>
    /// <param name="array">����������� ������</param>
    /// <param name="arrayIndex">��������� ������ � ������� ��� ����������</param>
    /// <param name="count">���������� ���������, ������� ����� �����������</param>
    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
      _List.CopyTo(index, array, arrayIndex, count);
    }

    /// <summary>
    /// ������� �������� ������� �� ������
    /// </summary>
    /// <param name="item">��������� �������</param>
    /// <returns>true, ���� ������� ��� ������ � ������</returns>
    public bool Remove(T item)
    {
      CheckNotReadOnly();

      int p = IndexOf(item); // � ��� ����� � ��������� ���������, � �� ������ ����
      if (p >= 0)
      {
        RemoveAt(p);
        return true;
      }
      else
        return false;
    }

    #endregion

    #region �������������� ������

    /// <summary>
    /// ��������� ����� �� ���� (� ������ IgnoreCase).
    /// ���������� ������ ���������� �������� ��� (-1).
    /// ���� ��������� ������ ���������� ������������� �������� � �������� �����, �����������
    /// Contains(), ����������� ��������� ��������.
    /// </summary>
    /// <param name="code">������� ���</param>
    /// <returns>������ �������</returns>
    public int IndexOf(string code)
    {
      if (String.IsNullOrEmpty(code))
        return -1;

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      ValidateDict();
      int p;
      if (_Dict.TryGetValue(code, out p))
        return p;
      else
        return -1;
    }

    /// <summary>
    /// ��������� ����� �� ���� (� ������ IgnoreCase).
    /// ���������� ������ ���������� �������� ��� (-1)
    /// ���� ��������� ������ ���������� ������������� �������� � �������� �����, �����������
    /// Contains(), ����������� ��������� ��������.
    /// </summary>
    /// <param name="code">������� ���</param>
    /// <returns>������ �������</returns>
    private int SlowIndexOf(string code)
    {
      if (IgnoreCase)
      {
        for (int i = 0; i < _List.Count; i++)
        {
          if (String.Equals(_List[i].Code, code, StringComparison.OrdinalIgnoreCase))
            return i;
        }
      }
      else
      {
        for (int i = 0; i < _List.Count; i++)
        {
          if (_List[i].Code == code)
            return i;
        }
      }
      return -1;
    }

    /// <summary>
    /// ������� ����� ���� (� ������ IgnoreCase).
    /// ���������� true, ���� � ������ ���� ������� � �������� �����
    /// </summary>
    /// <param name="code">������� ���</param>
    /// <returns>������� �������� � ������</returns>
    public bool Contains(string code)
    {
      if (String.IsNullOrEmpty(code))
        return false;

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      // �������, ����� ������� ��� ���

      return _Dict.ContainsKey(code);
    }

    /// <summary>
    /// ������� ����� ���� (� ������ IgnoreCase).
    /// ���������� true, ���� � ������ ���� ������� � �������� �����.
    /// ��� ���� ����� ������������ � �������.
    /// ���� � ������ ��� �������� � ����� �����, ������������ false, 
    /// � Value �������� ������ ��������
    /// </summary>
    /// <param name="code">������� ���</param>
    /// <param name="value">���� ���������� ��������� ��������</param>
    /// <returns>������� �������� � ������</returns>
    public bool TryGetValue(string code, out T value)
    {
      if (String.IsNullOrEmpty(code))
      {
        value = default(T);
        return false;
      }

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      ValidateDict();
      int p;
      if (_Dict.TryGetValue(code, out p))
      {
        value = _List[p];
        return true;
      }
      else
      {
        value = default(T);
        return false;
      }
    }

    /// <summary>
    /// �������� ������ � ������
    /// </summary>
    /// <returns>����� ������</returns>
    public T[] ToArray()
    {
      return _List.ToArray();
    }

    /// <summary>
    /// ��������� ��������� ��������� � ������.
    /// ������������ ����������������� ������ ������ Add()
    /// </summary>
    /// <param name="collection">������ ����������� ���������</param>
    public void AddRange(IEnumerable<T> collection)
    {
      CheckNotReadOnly();
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("������ �������� �������� �� ������ ����", "collection");

      BeginUpdate();
      try
      {
        foreach (T item in collection)
          Add(item);
      }
      finally
      {
        EndUpdate();
      }
    }

    /// <summary>
    /// ������� ������� � �������� ����� (� ������ IgnoreCase)
    /// </summary>
    /// <param name="code">��� ���������� ��������</param>
    /// <returns>true, ���� ������ ��� ������ � ������</returns>
    public bool Remove(string code)
    {
      CheckNotReadOnly();

      int p;
      if (_DictIsValid)
        p = IndexOf(code);
      else // �� �������� ������ ������� ������ ���, ����� �� ���������.
        p = SlowIndexOf(code);
      if (p >= 0)
      {
        RemoveAt(p);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// ���������� ��������� ������ � ������ ���������.
    /// ������� ������������� ������������ ��������� � ������.
    /// ������� �������� �� ��������, ���� ���� IgnoreCase=true.
    /// </summary>
    /// <returns></returns>
    public string[] GetCodes()
    {
      string[] a = new string[_List.Count];
      for (int i = 0; i < _List.Count; i++)
        a[i] = _List[i].Code;

      return a;
    }

    /// <summary>
    /// ���������� ������ �����.
    /// ��� ���������� ������� �������� ����������� ��� ������������, � ����������� �� �������� IgnoreCase.
    /// ����� ���������� ���������� ������� ListChanged � ������ Reset.
    /// </summary>
    public void Sort()
    {
      CheckNotReadOnly();

      _DictIsValid = false;

      if (_IgnoreCase)
        _List.Sort(ObjectWithCodeComparer<T>.OrdinalIgnoreCase);
      else
        _List.Sort(ObjectWithCodeComparer<T>.Ordinal);

      CallListChanged(ListChangedType.Reset, -1);
    }

    /// <summary>
    /// �������� ������� ��������� �� ��������.
    /// ����� ���������� ���������� ������� ListChanged � ������ Reset.
    /// </summary>
    public void Reverse()
    {
      CheckNotReadOnly();

      _DictIsValid = false;

      _List.Reverse();

      CallListChanged(ListChangedType.Reset, -1);
    }

    #endregion

    #region ��������� ��� ���������� � ������

    /// <summary>
    /// ������� ���������� ����� ����������� ��������.
    /// ���� ���������� ������� �������� ����������, ������ ��������� � ���������� � ������������� ���������.
    /// ������� �� ����������, ���� ���� �������� ����� BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> BeforeAdd;

    /// <summary>
    /// �������� ������� BeforeAdd, ���� ��� ��������� BeginUpdate().
    /// </summary>
    /// <param name="item">����������� �������</param>
    protected virtual void OnBeforeAdd(T item)
    {
      if (_UpdateCount == 0 && BeforeAdd != null)
      {
        NamedListItemEventArgs<T> args = new NamedListItemEventArgs<T>(item);
        BeforeAdd(this, args);
      }
    }

    /// <summary>
    /// ������� ���������� ����� ���������� ��������.
    /// ���� ���������� ������� �������� ����������, ������ �������� � ��������������� ��������� � �� ����� �������������� ������.
    /// ������� �� ����������, ���� ���� �������� ����� BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> AfterAdd;

    /// <summary>
    /// �������� ������� AfterAdd, ���� ��� ��������� BeginUpdate().
    /// </summary>
    /// <param name="item">����������� �������</param>
    protected virtual void OnAfterAdd(T item)
    {
      if (_UpdateCount == 0 && AfterAdd != null)
      {
        NamedListItemEventArgs<T> args = new NamedListItemEventArgs<T>(item);
        AfterAdd(this, args);
      }
    }

    /// <summary>
    /// ������� ���������� ����� ��������� ��������.
    /// ���� ���������� ������� �������� ����������, ������ ��������� � ���������� � ������������� ���������.
    /// ������� �� ����������, ���� ���� �������� ����� BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> BeforeRemove;

    /// <summary>
    /// �������� ������� BeforeRemove, ���� ��� ��������� BeginUpdate().
    /// </summary>
    /// <param name="item">��������� �������</param>
    protected virtual void OnBeforeRemove(T item)
    {
      if (_UpdateCount == 0 && BeforeRemove != null)
      {
        NamedListItemEventArgs<T> args = new NamedListItemEventArgs<T>(item);
        BeforeRemove(this, args);
      }
    }

    /// <summary>
    /// ������� ���������� ����� �������� ��������.
    /// ���� ���������� ������� �������� ����������, ������ �������� � ��������������� ��������� � �� ����� �������������� ������.
    /// ������� �� ����������, ���� ���� �������� ����� BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> AfterRemove;

    /// <summary>
    /// �������� ������� AfterRemove, ���� ��� ��������� BeginUpdate().
    /// </summary>
    /// <param name="item">����������� �������</param>
    protected virtual void OnAfterRemove(T item)
    {
      if (_UpdateCount == 0 && AfterRemove != null)
      {
        NamedListItemEventArgs<T> args = new NamedListItemEventArgs<T>(item);
        AfterRemove(this, args);
      }
    }

    /// <summary>
    /// ������� ���������� ��� ���������� � ������.
    /// </summary>
    public event ListChangedEventHandler ListChanged;

    /// <summary>
    /// �������� ������� ListChanged, ���� ��� ��������� ������ BeginUpdate()
    /// </summary>
    /// <param name="args">��������� �������</param>
    protected void OnListChanged(ListChangedEventArgs args)
    {
#if DEBUG
      if (args == null)
        throw new ArgumentNullException("args");
#endif

      if (InsideOnListChanged)
        throw new ReenteranceException("��������� ����� ListChanged");

      if (_UpdateCount > 0)
        DelayedListChanged = true; // ��������� �� �������
      else if (ListChanged != null)
      {
        InsideOnListChanged = true;
        try
        {
          ListChanged(this, args);
        }
        finally
        {
          InsideOnListChanged = false;
        }
      }
    }

    private bool InsideOnListChanged;

    private void CallListChanged(ListChangedType listChangedType, int newIndex)
    {
      ListChangedEventArgs args = new ListChangedEventArgs(listChangedType, newIndex);
      OnListChanged(args);
    }

    /// <summary>
    /// �������� ������� ListChanged � ListChangedType=ItemChanged
    /// </summary>
    /// <param name="index">������ ��������. ������ ���� � ��������� �� 0 �� (Count-1)</param>
    public void NotifyItemChanged(int index)
    {
      if (index < 0 || index >= Count)
        throw new ArgumentOutOfRangeException("index", index, "������ �������� ��� ���������");

      CallListChanged(ListChangedType.ItemChanged, index);
    }

    #endregion

    #region ������������ �������� ���������

    /// <summary>
    /// ����� ������ ������ ��������� ���������� ��������� BeforeAdd, AfterAdd, BeforeRemove � AfterRemove.
    /// ������ ����������� ����������� ������ ������� EndUpdate().
    /// </summary>
    public virtual void BeginUpdate()
    {
      if (_UpdateCount == 0)
        DelayedListChanged = false; // �� ����, ������� �� ������ �����������
      _UpdateCount++;
    }

    /// <summary>
    /// ��������� ���������� ������.
    /// ����� ������ ���� ������, �� ��������� � BeginUpdate()
    /// </summary>
    public virtual void EndUpdate()
    {
      if (_UpdateCount <= 0)
        throw new InvalidOperationException("�������� ����� EndUpdate()");

      _UpdateCount--;
      if (_UpdateCount == 0 && DelayedListChanged)
      {
        CallListChanged(ListChangedType.Reset, -1);
        DelayedListChanged = false;
      }
    }

    /// <summary>
    /// ���������� true, ���� ��� �������� ����� ������ BeginUpdate
    /// </summary>
    public bool IsUpdating { get { return _UpdateCount > 0; } }
    private int _UpdateCount;

    /// <summary>
    /// ��������������� � true ��� ����� ���������� � ������, ���� ��� �������� ����� BeginUpdate().
    /// � ���� ������, ����� EndUpdate() ���������� ������ � ������ ���������� ������
    /// </summary>
    private bool DelayedListChanged;

    #endregion

    #region ��������������

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      _Dict = new Dictionary<string, int>(_List.Count);
      for (int i = 0; i < _List.Count; i++)
      {
        string itemCode = _List[i].Code;
        if (IgnoreCase)
          itemCode = itemCode.ToUpperInvariant();

        _Dict.Add(itemCode, i);
      }
      _DictIsValid = true;
    }

    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    string[] INamedValuesAccess.GetNames()
    {
      return GetCodes();
    }

    #endregion
  }

#endif

  /// <summary>
  /// ��������� ��������, �������������� ��������� IObjectWithCode.
  /// � ������� �� NamedList, ������� ��������� �������� ��������������.
  /// ���������� ������ ������ �� ����, �� �� �� �������.
  /// � ������� �� ����������� ��������� Dictionary, ������� ����������� �� �� KeyValuePair,
  /// � ��������������� �� ��������.
  /// </summary>
  /// <typeparam name="T">��� ��������, ���������� � ������, �������������� ��������� IObjectWithCode</typeparam>
  [Serializable]
  public class NamedCollection<T> : IEnumerable<T>, ICollection<T>, IReadOnlyObject, INamedValuesAccess
    where T : class, IObjectWithCode
  {
    #region ������������

    /// <summary>
    /// ������� ������ ���������.
    /// ������� ����� �����������
    /// </summary>
    public NamedCollection()
      : this(false)
    {
    }

    /// <summary>
    /// ������� ������ ���������
    /// </summary>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    public NamedCollection(bool ignoreCase)
    {
      _Dict = new Dictionary<string, T>();
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ������ ���������.
    /// ������� ����� �����������
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    public NamedCollection(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// ������� ������ ���������
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    public NamedCollection(int capacity, bool ignoreCase)
    {
      _Dict = new Dictionary<string, T>(capacity);
      _IgnoreCase = ignoreCase;
    }


    /// <summary>
    /// ������� ��������� � ��������� �� ���������� �� ������ ���������.
    /// ������� ����� �����������.
    /// </summary>
    /// <param name="srcDictionary">�������� ���������, ������ ������� ��������. ��������� ������ ����� ���� �� ����</param>
    public NamedCollection(IDictionary<string, T> srcDictionary)
    {
      _Dict = new Dictionary<string, T>(srcDictionary);
    }

    /// <summary>
    /// ������� ��������� � ��������� �� ���������� �� ������.
    /// ������� ����� �����������
    /// </summary>
    /// <param name="srcCollection">�������� ���������</param>
    public NamedCollection(ICollection<T> srcCollection)
      : this(srcCollection, false)
    {
    }

    /// <summary>
    /// ������� ��������� � ��������� �� ���������� �� ������
    /// </summary>
    /// <param name="srcCollection">�������� ���������</param>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    public NamedCollection(ICollection<T> srcCollection, bool ignoreCase)
      : this(srcCollection.Count, ignoreCase)
    {
      foreach (T item in srcCollection)
        Add(item);
    }


    /// <summary>
    /// ������� ��������� � ��������� �� ���������� �� ������.
    /// ������� ����� �����������
    /// </summary>
    /// <param name="srcCollection">�������� ���������</param>
    public NamedCollection(IEnumerable<T> srcCollection)
      : this(srcCollection, false)
    {
    }

    /// <summary>
    /// ������� ��������� � ��������� �� ���������� �� ������
    /// </summary>
    /// <param name="srcCollection">�������� ���������</param>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    public NamedCollection(IEnumerable<T> srcCollection, bool ignoreCase)
      : this(ignoreCase)
    {
      foreach (T item in srcCollection)
        Add(item);
    }

    #endregion

    #region ������ � ���������

    /// <summary>
    /// ��������� �� �����.
    /// ���� IgnoreCase=true, �� ���� ������������ � �������� ��������
    /// </summary>
    private readonly Dictionary<string, T> _Dict;

    /// <summary>
    /// ������ �� ����.
    /// ���� �������� �������������� ���, ������������ ������ �������
    /// </summary>
    /// <param name="code">��� ��������</param>
    /// <returns>��������� ������� ��� ������ ��������</returns>
    public T this[string code]
    {
      get
      {
        T res;
        if (string.IsNullOrEmpty(code))
          return default(T);

        if (IgnoreCase)
          code = code.ToUpperInvariant();

        if (_Dict.TryGetValue(code, out res))
          return res;
        else
          return default(T);
      }
    }

    /// <summary>
    /// ������ �� ����.
    /// � ������� �� ������� �� ���������������� ��������, ���� �������� �������������� ���, ������������ ����������
    /// </summary>
    /// <param name="code">��� ��������</param>
    /// <returns>��������� ������� ��� ������ ��������</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">���� �� ������ ������ � �������� �����</exception>
    public T GetRequired(string code)
    {
      T res;
      if (string.IsNullOrEmpty(code))
        throw new ArgumentNullException("code");

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      if (_Dict.TryGetValue(code, out res))
        return res;
      else
        throw new KeyNotFoundException("� ��������� ��� �������� � ����� \"" + code + "\"");
    }

    /// <summary>
    /// ���������� ���������� ��������� � ���������
    /// </summary>
    public int Count { get { return _Dict.Count; } }

    /// <summary>
    /// ��������� ������������� "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// ���� ����������� � true, �� ��� ������ ��������� ����� �������������� �������.
    /// ���� �������� ����������� � false (�� ���������), �� ������� �������� ����������.
    /// �������� ��������������� � ������������.
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private readonly bool _IgnoreCase;

    #endregion

    #region ������ ������ ��� ������

    /// <summary>
    /// ���������� true, ���� ��������� ��������� ���������
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// ������������� ��������� �� ���������
    /// </summary>
    internal protected void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    /// <summary>
    /// ���������� ���������� ObjectReadOnlyException, ���� ��������� �� ����� ���� ��������������
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// ���������� ������������� �� �������� ���������.
    /// 
    /// ��� ������������� �������� ����� ���������� � �������, 
    /// ������������� ������ ���������� ���������� �������������.
    /// ������� � ���������� ���� ����� ������ �������������� ������������� ��� ������������� � ��������� foreach.
    /// </summary>
    /// <returns>�������������</returns>
    public Dictionary<string, T>.ValueCollection.Enumerator GetEnumerator()
    {
      return _Dict.Values.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return _Dict.Values.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _Dict.Values.GetEnumerator();
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// ��������� ������� � ���������.
    /// ���� � ��������� ��� ���� ������� � ����� ����� (� ������ IgnoreCase), ������������ ����������.
    /// </summary>
    /// <param name="item">����������� �������</param>
    public void Add(T item)
    {
      CheckNotReadOnly();

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      _Dict.Add(ItemCode, item);
    }

    /// <summary>
    /// ������� ���������
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _Dict.Clear();
    }

    /// <summary>
    /// ���������� true, ���� ������� ���� � ���������.
    /// ������������� ������������ �����, ����������� ��������� ���
    /// </summary>
    /// <param name="item">�������, ������� �������� �����������</param>
    /// <returns></returns>
    public bool Contains(T item)
    {
      string itemCode = item.Code;
      if (IgnoreCase)
        itemCode = itemCode.ToUpperInvariant();
      //return FDict.ContainsKey(ItemCode);

      // 22.02.2018. ������������� ��������� �� ���������
      T resItem;
      if (_Dict.TryGetValue(itemCode, out resItem))
        return item.Equals(resItem);
      else
        return false;
    }

    /// <summary>
    /// �������� ��������� � ������
    /// </summary>
    /// <param name="array">����������� ������</param>
    public void CopyTo(T[] array)
    {
      _Dict.Values.CopyTo(array, 0);
    }

    /// <summary>
    /// �������� ��������� � ������
    /// </summary>
    /// <param name="array">����������� ������</param>
    /// <param name="arrayIndex">��������� ������ � ����������� �������</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      _Dict.Values.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// ������� ������� �� ���������
    /// </summary>
    /// <param name="item">��������� �������</param>
    /// <returns>true, ���� ������� ��� ������ � ������</returns>
    public bool Remove(T item)
    {
      CheckNotReadOnly();

      string itemCode = item.Code;
      if (IgnoreCase)
        itemCode = itemCode.ToUpperInvariant();

      return _Dict.Remove(itemCode);
    }

    #endregion

    #region �������������� ������

    /// <summary>
    /// ������� �����.
    /// ���������� true, ���� ������� � �������� ����� (� ������ IgnoreCase) ���� � ���������.
    /// ��� - ������������� ����� ������.
    /// </summary>
    /// <param name="code">����������� ���</param>
    /// <returns>������� �������� � �����</returns>
    public bool Contains(string code)
    {
      if (String.IsNullOrEmpty(code))
        return false;

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      return _Dict.ContainsKey(code);
    }

    /// <summary>
    /// ������� �����.
    /// ���������� true, ���� ������� � �������� ����� (� ������ IgnoreCase) ���� � ���������.
    /// ��������� ��������� � �������� this, �� ��������� �������� �������� ������� ��� ���������� ��������.
    /// </summary>
    /// <param name="code">����������� ���</param>
    /// <param name="value">���� ���������� ��������� ��������</param>
    /// <returns>������� �������� � �����</returns>
    public bool TryGetValue(string code, out T value)
    {
      if (String.IsNullOrEmpty(code))
      {
        value = default(T);
        return false;
      }

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      return _Dict.TryGetValue(code, out value);
    }

    /// <summary>
    /// ������� ������ � ���������� �� ���������
    /// </summary>
    /// <returns>����� ������</returns>
    public T[] ToArray()
    {
      T[] a = new T[_Dict.Count];
      _Dict.Values.CopyTo(a, 0);
      return a;
    }

    /// <summary>
    /// ��������� �������� �� ������.
    /// ������������ ���������� ������ ������ Add()
    /// </summary>
    /// <param name="collection">�������� ������ ���������</param>
    public void AddRange(IEnumerable<T> collection)
    {
      CheckNotReadOnly();
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("������ �������� �������� �� ������ ����", "collection");

      foreach (T item in collection)
        Add(item);
    }

    /// <summary>
    /// ������� ������� � �������� �����
    /// </summary>
    /// <param name="code">��� ���������� ��������</param>
    /// <returns>true, ���� ������� ��� ������ � ������</returns>
    public bool Remove(string code)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(code))
        return false;
      if (IgnoreCase)
        code = code.ToUpperInvariant();

      return _Dict.Remove(code);
    }

    /// <summary>
    /// ���������� ��������� ������ � ������ ���������
    /// ������� �������� �� ��������, ���� ���� IgnoreCase=true
    /// </summary>
    /// <returns>������ �����</returns>
    public string[] GetCodes()
    {
      string[] a = new string[_Dict.Count];
      int cnt = 0;
      foreach (T item in _Dict.Values)
      {
        a[cnt] = item.Code;
        cnt++;
      }

      return a;
    }

    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    string[] INamedValuesAccess.GetNames()
    {
      return GetCodes();
    }

    #endregion
  }

  /// <summary>
  /// ���������������� ���������� NamedCollection, ������� ������ ������. 
  /// ���� ������ NamedCollection ��� ��������� � ����� ReadOnly, ����� ������������ ������������ �����, �.�. � ���� ������ ��
  /// ��� �������� ����������������.
  /// ������ �������� ������������� ����, ������ � ������� ����� �������������� �� �����, ��� � Dictionary � 
  /// ������ String
  /// </summary>
  /// <typeparam name="T">��� ��������, ���������� � ������, �������������� ��������� IObjectWithCode</typeparam>
  public class SyncNamedCollection<T> : SyncCollection<T>, IReadOnlyObject, INamedValuesAccess
    where T : class, IObjectWithCode
  {
    #region ������������

    /// <summary>
    /// �������� ������������������ �������� ��� ������������ �������.
    /// </summary>
    /// <param name="source">������� ������</param>
    public SyncNamedCollection(NamedCollection<T> source)
      : base(source)
    {
    }

    /// <summary>
    /// ������� ������ ���������.
    /// ������� �������� ���� �����������.
    /// </summary>
    public SyncNamedCollection()
      : this(new NamedCollection<T>())
    {
    }

    /// <summary>
    /// ������� ������ ���������.
    /// </summary>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    public SyncNamedCollection(bool ignoreCase)
      : this(new NamedCollection<T>(ignoreCase))
    {
    }


    /// <summary>
    /// ������� ������ ���������.
    /// ������� �������� ���� �����������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    public SyncNamedCollection(int capacity)
      : this(new NamedCollection<T>(capacity))
    {
    }

    /// <summary>
    /// ������� ������ ���������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    public SyncNamedCollection(int capacity, bool ignoreCase)
      : this(new NamedCollection<T>(capacity, ignoreCase))
    {
    }

    /// <summary>
    /// ������� ��������� � ��������� �� ���������� �� ������ ���������.
    /// ������� �������� ���� �����������.
    /// ��� ������� ������������ � IgnoreCase
    /// </summary>
    /// <param name="srcDictionary">�������� ���������. ������ ����� ���� �� ���� ���������</param>
    public SyncNamedCollection(IDictionary<string, T> srcDictionary)
      : this(new NamedCollection<T>(srcDictionary))
    {
    }


    /// <summary>
    /// ������� ��������� � ��������� �� ���������� �� ������� ������.
    /// ������� �������� ���� �����������.
    /// </summary>
    /// <param name="srcCollection">�������� ���������. ������ ����� ���� �� ���� ���������</param>
    public SyncNamedCollection(ICollection<T> srcCollection)
      : this(new NamedCollection<T>(srcCollection))
    {
    }

    /// <summary>
    /// ������� ��������� � ��������� �� ���������� �� ������� ������.
    /// </summary>
    /// <param name="srcCollection">�������� ���������. ������ ����� ���� �� ���� ���������</param>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    public SyncNamedCollection(ICollection<T> srcCollection, bool ignoreCase)
      : this(new NamedCollection<T>(srcCollection, ignoreCase))
    {
    }


    /// <summary>
    /// ������� ��������� � ��������� �� ���������� �� ������� ������.
    /// ������� �������� ���� �����������.
    /// </summary>
    /// <param name="srcCollection">�������� ���������. ������ ����� ���� �� ���� ���������</param>
    public SyncNamedCollection(IEnumerable<T> srcCollection)
      : this(new NamedCollection<T>(srcCollection))
    {
    }

    /// <summary>
    /// ������� ��������� � ��������� �� ���������� �� ������� ������.
    /// </summary>
    /// <param name="srcCollection">�������� ���������. ������ ����� ���� �� ���� ���������</param>
    /// <param name="ignoreCase">���� �� ������������ ������� ����</param>
    public SyncNamedCollection(IEnumerable<T> srcCollection, bool ignoreCase)
      : this(new NamedCollection<T>(srcCollection, ignoreCase))
    {
    }

    #endregion

    #region ������ � ���������

    /// <summary>
    /// �������� ������ ������
    /// </summary>
    protected new NamedCollection<T> Source { get { return (NamedCollection<T>)(base.Source); } }

    /// <summary>
    /// ������ �� ����.
    /// ���� �������� �������������� ���, ������������ ������ �������
    /// </summary>
    /// <param name="code">��� ��������</param>
    /// <returns>��������� ������� ��� ������ ��������</returns>
    public T this[string code]
    {
      get
      {
        lock (SyncRoot)
        {
          return Source[code];
        }
      }
    }

    /// <summary>
    /// ������ �� ����.
    /// � ������� �� ������� �� ���������������� ��������, ���� �������� �������������� ���, ������������ ����������
    /// </summary>
    /// <param name="code">��� ��������</param>
    /// <returns>��������� �������</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">���� �� ������ ������ � �������� �����</exception>
    public T GetRequired(string code)
    {
      lock (SyncRoot)
      {
        return Source.GetRequired(code);
      }
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// ���������� true, ���� ������� �������� ������������ ��� ������ �� �����.
    /// False, ���� �����������. �������� � ������������
    /// </summary>
    public bool IgnoreCase { get { return Source.IgnoreCase; } }

    #endregion

    #region ������ ������ ��� ������

    /// <summary>
    /// ���������� true, ���� ��������� ��������� � ������ "������ ������"
    /// </summary>
    public new bool IsReadOnly
    {
      get
      {
        lock (SyncRoot)
        {
          return Source.IsReadOnly;
        }
      }
    }

    /// <summary>
    /// ���������� ����� ��� �������� ��������� � ����� "������ ������"
    /// </summary>
    protected void SetReadOnly()
    {
      lock (SyncRoot)
      {
        Source.SetReadOnly();
      }
    }

    /// <summary>
    /// ���������� ����������, ���� ��������� ��������� � ������ "������ ������"
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region �������������� ������

    /// <summary>
    /// ������� �����.
    /// ���������� true, ���� ��������� �������� ������� � ����� ����� (� ������ IgnoreCase).
    /// ������������� ����� ������.
    /// </summary>
    /// <param name="code">����������� ���</param>
    /// <returns>������� ��������</returns>
    public bool Contains(string code)
    {
      lock (SyncRoot)
      {
        return Source.Contains(code);
      }
    }

    /// <summary>
    /// ������� �����.
    /// ���������� true, ���� ��������� �������� ������� � ����� ����� (� ������ IgnoreCase).
    /// ������������� ����� ������.
    /// </summary>
    /// <param name="code">����������� ���</param>
    /// <param name="value">���� ���������� ��������� ��������</param>
    /// <returns>������� ��������</returns>
    public bool TryGetValue(string code, out T value)
    {
      lock (SyncRoot)
      {
        return Source.TryGetValue(code, out value);
      }
    }

    /// <summary>
    /// ��������� �������� �� ������� ������.
    /// </summary>
    /// <param name="collection">������ ��������� ��� ����������</param>
    public new void AddRange(IEnumerable<T> collection)
    {
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("������ �������� �������� �� ������ ����", "collection");
      if (Object.ReferenceEquals(collection, Source))
        throw new ArgumentException("������ �������� �������� �� �������� ������", "collection");

      lock (SyncRoot)
      {
        ResetCopyArray();
        Source.AddRange(collection);
      }
    }

    /// <summary>
    /// ���������� ������ ���� ����� � ���������.
    /// </summary>
    /// <returns>������ �����</returns>
    public string[] GetCodes()
    {
      lock (SyncRoot)
      {
        return Source.GetCodes();
      }
    }

    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    string[] INamedValuesAccess.GetNames()
    {
      return GetCodes();
    }

    #endregion
  }
}
