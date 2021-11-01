using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

// ���������, ������������ ������ (� �������� �����).
// ������������ ����� IgnoreCase

namespace FreeLibSet.Collections
{
  /// <summary>
  /// ������ ����� � ����������� ����������.
  /// �������������� �������� � ������ � ��� ����� ��������.
  /// � �������� ��� ����� ��������, �������� ������� �������� �����������.
  /// �������� null �� �����������.
  /// ����� ��������� �������� ReadOnly=true, ������ ���������� ����������������.
  /// ������ �������� � ������ � ������� ����������. ����������� ����� Sort() ��� ���������� ������
  /// </summary>
  [Serializable]
  public class SingleScopeStringList : IList<string>, IReadOnlyObject
  {
    #region ������������

    /// <summary>
    /// ������� ������ ������
    /// </summary>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public SingleScopeStringList(bool ignoreCase)
    {
      _List = new List<string>();
      _Dict = new Dictionary<string, string>();
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ������ ������ �������� �������.
    /// ����������� ���� �����������, ���� �������� ����� ��������� � ��������� �������� � ������� ����� �����������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public SingleScopeStringList(int capacity, bool ignoreCase)
    {
      _List = new List<string>(capacity);
      _Dict = new Dictionary<string, string>(capacity);
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ������ � ��������� ��� ��������� ����������.
    /// </summary>
    /// <param name="src">���������, ������ ������� ����� ������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public SingleScopeStringList(ICollection<string> src, bool ignoreCase)
      : this(src.Count, ignoreCase)
    {
      foreach (string Item in src)
        Add(Item);
    }

    /// <summary>
    /// ������� ������ � ��������� ��� ��������� ����������.
    /// </summary>
    /// <param name="src">���������, ������ ������� ����� ������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public SingleScopeStringList(IEnumerable<string> src, bool ignoreCase)
      : this(ignoreCase)
    {
      foreach (string Item in src)
        Add(Item);
    }

    #endregion

    #region ������ � ���������

    /// <summary>
    /// �������� ������.
    /// ������ �������� � ��� ��������, � ������� ���� ��������
    /// </summary>
    private List<string> _List;

    /// <summary>
    /// ���� - ������. ���� IgnoreCase=true, �� ������ ����������� � ������� �������.
    /// ��������� �������� �� �� ������, �� � ������ ��������
    /// </summary>
    [NonSerialized]
    private Dictionary<string, string> _Dict;

    /// <summary>
    /// ������ �� �������
    /// </summary>
    /// <param name="index">������ �������� � ������</param>
    /// <returns>��������</returns>
    public string this[int index]
    {
      get { return _List[index]; }
      set
      {
        CheckNotReadOnly();

        string value2 = PrepareValue(value);

        if (_Dict.ContainsKey(value2))
          throw new InvalidOperationException("�������� " + value.ToString() + " ��� ���� � ������");

        string OldItem = _List[index];
        _Dict.Remove(PrepareValue(OldItem));
        try
        {
          _Dict.Add(value2, value);
        }
        catch
        {
          _Dict.Add(PrepareValue(OldItem), OldItem);
          throw;
        }
        _List[index] = value;
      }
    }

    /// <summary>
    /// ���������� ���������� ����� � ������.
    /// </summary>
    public int Count { get { return _List.Count; } }

    /// <summary>
    /// ��������� ������������� "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string s = "Count=" + Count.ToString();
      if (IsReadOnly)
        s += " (ReadOnly)";
      return s;
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// ���� true, �� ������� ����� �� �����������.
    /// �������� �������� � ������������
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    private string PrepareValue(string value)
    {
      if (value == null)
        //return null;
        throw new ArgumentNullException("value"); // 27.12.2020

      if (_IgnoreCase)
        return value.ToUpperInvariant();
      else
        return value;
    }

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
    protected void SetReadOnly()
    {
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

    #region IEnumerable<string> Members

    /// <summary>
    /// ���������� ������������� �� ������� ������.
    /// 
    /// ��� ������������� �������� ����� ���������� � �������, 
    /// ������������� ������ ���������� ���������� �������������.
    /// ������� � ���������� ���� ����� ������ �������������� ������������� ��� ������������� � ��������� foreach.
    /// </summary>
    /// <returns>�������������</returns>
    public List<string>.Enumerator GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    #endregion

    #region IList<string> Members

    /// <summary>
    /// ���������� ������ �������� ������ � ������.
    /// ��� ������ ������ ����������� �������� IgnoreCase.
    /// ����� �������� ���������. ���� ��������� ������ ���������� ���� ������� ������, ����������� Contains().
    /// </summary>
    /// <param name="item">������� ������</param>
    /// <returns>������ ��������� ������ ��� (-1), ���� ������ �� �������</returns>
    public int IndexOf(string item)
    {
      StringComparison Flags = IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

      for (int i = 0; i < _List.Count; i++)
      {
        if (String.Equals(item, _List[i], Flags))
          return i;
      }
      return -1;
    }

    /// <summary>
    /// ��������� ������ � �������� �������.
    /// ���� ����� ������ (� ������ IgnoreCase) ��� ���� � ������, ������� �������� �� �����������.
    /// </summary>
    /// <param name="index">������ � ������, ���� ������ ���� ��������� ������</param>
    /// <param name="item">����������� ������</param>
    public void Insert(int index, string item)
    {
      CheckNotReadOnly();

      string value2 = PrepareValue(item);

      if (_Dict.ContainsKey(value2))
        return;

      _Dict.Add(value2, item);
      try
      {
        _List.Insert(index, item);
      }
      catch
      {
        _Dict.Remove(value2);
        throw;
      }
    }

    /// <summary>
    /// ������� ������ �� ������ � �������� �������.
    /// </summary>
    /// <param name="index">������ ������ ��� ��������</param>
    public void RemoveAt(int index)
    {
      CheckNotReadOnly();

      string item = _List[index];
      _List.RemoveAt(index);
      _Dict.Remove(PrepareValue(item));
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// ��������� ������ � ������.
    /// ���� ����� ������ ��� ���� � ������ (� ������ IgnoreCase), ������� �������� �� �����������.
    /// </summary>
    /// <param name="item">����������� ������</param>
    public void Add(string item)
    {
      CheckNotReadOnly();

      string value2 = PrepareValue(item);

      if (_Dict.ContainsKey(value2))
        return;

      _Dict.Add(value2, item);
      try
      {
        _List.Add(item);
      }
      catch
      {
        _Dict.Remove(value2);
      }
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _List.Clear();
      _Dict.Clear();
    }

    /// <summary>
    /// ���������� true, ���� � ������ ���� ����� ������ (� ������ IgnoreCase).
    /// � ������� �� IndexOf(), ���� ����� ����������� ������.
    /// </summary>
    /// <param name="item">������� ������</param>
    /// <returns>������� ������ � ������</returns>
    public bool Contains(string item)
    {
      return _Dict.ContainsKey(PrepareValue(item));
    }

    /// <summary>
    /// �������� ���� ������ � ������
    /// </summary>
    /// <param name="array">����������� ������</param>
    public void CopyTo(string[] array)
    {
      _List.CopyTo(array);
    }

    /// <summary>
    /// �������� ���� ������ � ������
    /// </summary>
    /// <param name="array">����������� ������</param>
    /// <param name="arrayIndex">������ � �������, ������� � �������� �� �����������</param>
    public void CopyTo(string[] array, int arrayIndex)
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
    public void CopyTo(int index, string[] array, int arrayIndex, int count)
    {
      _List.CopyTo(index, array, arrayIndex, count);
    }

    /// <summary>
    /// ������� �������� ������ �� ������ (� ������ IgnoreCase).
    /// ���� ������ ��� � ������, ������� �������� �� �����������.
    /// </summary>
    /// <param name="item">��������� ������</param>
    /// <returns>true, ���� ������ ���� ������� � �������. 
    /// false, ���� ������ �� ������� � ������.</returns>
    public bool Remove(string item)
    {
      CheckNotReadOnly();

      int p = IndexOf(item);
      if (p >= 0)
      {
        _List.RemoveAt(p);
        string value2 = PrepareValue(item);
        _Dict.Remove(value2);
        return true;
      }
      else
        return false;
    }

    #endregion

    #region �������������� ������

    /// <summary>
    /// ������� ������ ����� �� ������
    /// </summary>
    /// <returns>����� ������</returns>
    public string[] ToArray()
    {
      return _List.ToArray();
    }

    /// <summary>
    /// ��������� ���������� ��������� ������
    /// � �������� ��������� ����� ���� ���������� ��������, ������� ������������
    /// </summary>
    /// <param name="collection"></param>
    public void AddRange(IEnumerable<string> collection)
    {
      CheckNotReadOnly();
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("������ �������� �������� �� ������ ����", "collection");

      foreach (string Item in collection)
        Add(Item);
    }

    /// <summary>
    /// ���������� ������ �����.
    /// ��� ���������� ������� �������� ����������� ��� ������������, � ����������� �� �������� IgnoreCase
    /// </summary>
    public void Sort()
    {
      CheckNotReadOnly();

      if (_IgnoreCase)
        _List.Sort(StringComparer.OrdinalIgnoreCase);
      else
        _List.Sort(StringComparer.Ordinal);
    }

    /// <summary>
    /// �������� ������� ��������� �� ��������
    /// </summary>
    public void Reverse()
    {
      CheckNotReadOnly();

      _List.Reverse();
    }

    #endregion

    #region ������������

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      _Dict = new Dictionary<string, string>(_List.Count);
      for (int i = 0; i < _List.Count; i++)
      {
        string value2 = PrepareValue(_List[i]);
        _Dict.Add(value2, _List[i]);
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������������� ���������, � ������� ������ �������� ������, � �������� ����� �������� ���.
  /// � ������� �� ������� ��������� Dictionary, ����� ���� �� ������������� � �������� �����
  /// </summary>
  /// <typeparam name="TValue">��� ���������� ��������</typeparam>
  [Serializable]
  public class TypedStringDictionary<TValue> : IDictionary<string, TValue>, IReadOnlyObject, INamedValuesAccess
  {
    #region ������������

    /// <summary>
    /// ������� ������ ���������.
    /// </summary>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public TypedStringDictionary(bool ignoreCase)
    {
      _IgnoreCase = ignoreCase;
      _MainDict = new Dictionary<string, TValue>();
    }

    /// <summary>
    /// ������� ������ ��������� �������� �������
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public TypedStringDictionary(int capacity, bool ignoreCase)
    {
      _IgnoreCase = ignoreCase;
      _MainDict = new Dictionary<string, TValue>(capacity);
    }

    /// <summary>
    /// ������� �������� � ��������� �� ����������
    /// </summary>
    /// <param name="dictionary">�������� ���������, ������ ������� �������� ��� ����������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public TypedStringDictionary(IDictionary<string, TValue> dictionary, bool ignoreCase)
      : this(dictionary.Count, ignoreCase)
    {
      foreach (KeyValuePair<string, TValue> Pair in dictionary)
        Add(Pair.Key, Pair.Value);
    }

    #endregion

    #region ������ � ���������

    // ������������ ��� ��������� Dictionary

    /// <summary>
    /// �������� ���������.
    /// �������� ����� � �������� ��������, ���������� �� IgnoreCase
    /// </summary>
    private Dictionary<string, TValue> _MainDict;

    /// <summary>
    /// �������������� ���������.
    /// ����������, ����� IgnoreCase=true. 
    /// ��������� ��� �������������. � ���������, ��������� �� �������������
    /// ����: - �����, ����������� � �������� ��������
    /// ��������: ����� � �������� ��������.
    /// </summary>
    [NonSerialized]
    private Dictionary<string, string> _AuxDict;

    /// <summary>
    /// ��� IgnoreCase ��������� ���� � ��� �������, ������� ��� ����� ��� ������ ���������
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string PrepareKey(string key)
    {
      if (!IgnoreCase)
        return key;

      // ���������� ����� ������� �������� �� ������ ������������ �������
      if (_AuxDict == null)
        PrepareAuxDict();

      string Key2 = key.ToUpperInvariant();
      string Key3;
      if (_AuxDict.TryGetValue(Key2, out Key3))
        return Key3;
      else
        return key;
    }

    private void PrepareAuxDict()
    {
      // ����������� Dictionary �� ��������� ������������� capacity=0.
      // ���� � ��������� ��� ���� ��������, ��, ��������, ���� ����� ������ ����� ��������������.
      // ���������� MainDict.Count � �������� ��������� �������
      Dictionary<string, string> AuxDict2 = new Dictionary<string, string>(_MainDict.Count);
      foreach (KeyValuePair<string, TValue> Pair in _MainDict)
      {
        AuxDict2.Add(Pair.Key.ToUpperInvariant(), Pair.Key);
      }

      // ���� ��� ������, ������������� �������� ����
      _AuxDict = AuxDict2;
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// ���� true, �� ������� ����� �� �����������.
    /// �������� �������� � ������������
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    #endregion

    #region IDictionary<string,TValue> Members

    /// <summary>
    /// ��������� ���� "����-��������" � ���������.
    /// � ������ IgnoreCase=true ��������� ����������, ���� � ��������� ��� ���� ������� ����, ������������ ������ ���������
    /// </summary>
    /// <param name="key">����</param>
    /// <param name="value">��������</param>
    public void Add(string key, TValue value)
    {
      CheckNotReadOnly();
      key = PrepareKey(key);
      _MainDict.Add(key, value);
      if (_AuxDict != null)
        _AuxDict.Add(key.ToUpperInvariant(), key);
    }

    /// <summary>
    /// ���������� true, ���� ��������� �������� ��������� ����
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(string key)
    {
      return _MainDict.ContainsKey(PrepareKey(key));
    }

    /// <summary>
    /// ������ � ������ ���������.
    /// ������������ ��������� ������������� ������ ��� ���������.
    /// </summary>
    public ICollection<string> Keys { get { return _MainDict.Keys; } }

    /// <summary>
    /// ������� ���� �� ���������
    /// </summary>
    /// <param name="key">����</param>
    /// <returns>true, ���� ������� ��� ������ �� ���������</returns>
    public bool Remove(string key)
    {
      CheckNotReadOnly();

      key = PrepareKey(key);
      if (_MainDict.Remove(key))
      {
        if (_AuxDict != null)
          _AuxDict.Remove(key.ToUpperInvariant());
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// ������� ������� ������� � �������� ������ �� ���������
    /// </summary>
    /// <param name="key">����</param>
    /// <param name="value">���� ������������ ��������</param>
    /// <returns>true, ���� ������� � ������ ���� � ���������</returns>
    public bool TryGetValue(string key, out TValue value)
    {
      key = PrepareKey(key);
      return _MainDict.TryGetValue(key, out value);
    }

    /// <summary>
    /// ��������� ��������.
    /// ������������ ��������� ������������� ������� ��� ���������.
    /// </summary>
    public ICollection<TValue> Values { get { return _MainDict.Values; } }

    /// <summary>
    /// ���������� ��� ������ �������� � ������.
    /// ���� ��� ������ �������� �������������� ����, ������������ ����������.
    /// ��� ��������� ��������, ���� ����������� ����� ������, ���� ���������� ������������ � ����� ������.
    /// </summary>
    /// <param name="key">����</param>
    /// <returns>��������</returns>
    public TValue this[string key]
    {
      get
      {
        key = PrepareKey(key);
        return _MainDict[key];
      }
      set
      {
        key = PrepareKey(key);
        _MainDict[key] = value;
        // ��������� RegDict �� ��������
      }
    }

    #endregion

    #region ICollection<KeyValuePair<string,TValue>> Members

    void ICollection<KeyValuePair<string, TValue>>.Add(KeyValuePair<string, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    /// <summary>
    /// �������� ���������
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _MainDict.Clear();
      _AuxDict = null; // ������������, ���� ������������
    }

    bool ICollection<KeyValuePair<string, TValue>>.Contains(KeyValuePair<string, TValue> item)
    {
      if (_IgnoreCase)
        item = new KeyValuePair<string, TValue>(PrepareKey(item.Key), item.Value);
      return ((ICollection<KeyValuePair<string, TValue>>)_MainDict).Contains(item);
    }

    void ICollection<KeyValuePair<string, TValue>>.CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
    {
      ((ICollection<KeyValuePair<string, TValue>>)_MainDict).CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// ���������� ���������� ��������� � ���������
    /// </summary>
    public int Count { get { return _MainDict.Count; } }

    bool ICollection<KeyValuePair<string, TValue>>.Remove(KeyValuePair<string, TValue> item)
    {
      return Remove(item.Key);
    }

    #endregion

    #region IEnumerable<KeyValuePair<string,TValue>> Members

    /// <summary>
    /// ���������� ������������� ���������.
    /// ���������� ������������ �������� ��������� KeyValuePair.
    /// ���� � ���� ����� ��� �������, ������� ������������� ��� ���������� �������� � ���������.
    /// 
    /// ��� ������������� �������� ����� ���������� � �������, 
    /// ������������� ������ ���������� ���������� �������������.
    /// ������� � ���������� ���� ����� ������ �������������� ������������� ��� ������������� � ��������� foreach.
    /// </summary>
    /// <returns>�������������</returns>
    public Dictionary<string, TValue>.Enumerator GetEnumerator()
    {
      return _MainDict.GetEnumerator();
    }

    IEnumerator<KeyValuePair<string, TValue>> IEnumerable<KeyValuePair<string, TValue>>.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region ������ ������ ��� ������

    /// <summary>
    /// ���������� true, ���� ��������� ���� ���������� � ����� "������ ������"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// ���������� ����� ��� �������� ��������� � ����� "������ ������"
    /// </summary>
    protected void SetReadOnly()
    {
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

    #region ��������������

    // ���������� ����������� ��������������

    #endregion

    #region ������

    /// <summary>
    /// ���������� ������ ���� "Count=XXX"
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    bool INamedValuesAccess.Contains(string name)
    {
      return ContainsKey(name);
    }

    string[] INamedValuesAccess.GetNames()
    {
      string[] a = new string[Count];
      Keys.CopyTo(a, 0);
      return a;
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������������� ���������, � ������� ������ �������� ������, � �������� ����� �������� ���.
  /// ������������ ���������, � ������� ����� �������� �� ������ �������� ��� �����, �� � ���� ��� ��������.
  /// � ������� �� ������� ��������� Dictionary, ���� ����� ���� �� ������������ � �������� �����.
  /// ���� ��������� <typeparamref name="TValue"/> �������� ������, �� ��� ������ �������� �������������� � ��������
  /// </summary>
  /// <typeparam name="TValue">��� ���������� ��������</typeparam>
  [Serializable]
  public class BidirectionalTypedStringDictionary<TValue> : IDictionary<string, TValue>, IReadOnlyObject, INamedValuesAccess
  {
    #region ������������

    /// <summary>
    /// ������� ������ ���������.
    /// </summary>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public BidirectionalTypedStringDictionary(bool ignoreCase)
    {
      _IgnoreCase = ignoreCase;
      _MainDict = new Dictionary<string, TValue>();
    }

    /// <summary>
    /// ������� ������ ��������� �������� �������
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public BidirectionalTypedStringDictionary(int capacity, bool ignoreCase)
    {
      _IgnoreCase = ignoreCase;
      _MainDict = new Dictionary<string, TValue>(capacity);
    }

    /// <summary>
    /// ������� �������� � ��������� �� ����������
    /// </summary>
    /// <param name="dictionary">�������� ���������, ������ ������� �������� ��� ����������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public BidirectionalTypedStringDictionary(IDictionary<string, TValue> dictionary, bool ignoreCase)
      : this(dictionary.Count, ignoreCase)
    {
      foreach (KeyValuePair<string, TValue> Pair in dictionary)
        Add(Pair.Key, Pair.Value);
    }

    #endregion

    #region ������ � ���������

    // ������������ ��� ��������� Dictionary

    /// <summary>
    /// �������� ���������.
    /// �������� ����� � �������� ��������, ���������� �� IgnoreCase
    /// </summary>
    private Dictionary<string, TValue> _MainDict;

    /// <summary>
    /// �������������� ���������.
    /// ����������, ����� IgnoreCase=true. 
    /// ��������� ��� �������������. � ���������, ��������� �� �������������
    /// ����: - �����, ����������� � �������� ��������
    /// ��������: ����� � �������� ��������.
    /// </summary>
    [NonSerialized]
    private Dictionary<string, string> _AuxDict;

    /// <summary>
    /// ��� IgnoreCase ��������� ���� � ��� �������, ������� ��� ����� ��� ������ ���������
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string PrepareKey(string key)
    {
      if (!IgnoreCase)
        return key;

      // ���������� ����� ������� �������� �� ������ ������������ �������
      if (_AuxDict == null)
        PrepareAuxDict();

      string Key2 = key.ToUpperInvariant();
      string Key3;
      if (_AuxDict.TryGetValue(Key2, out Key3))
        return Key3;
      else
        return key;
    }

    private void PrepareAuxDict()
    {
      // ����������� Dictionary �� ��������� ������������� capacity=0.
      // ���� � ��������� ��� ���� ��������, ��, ��������, ���� ����� ������ ����� ��������������.
      // ���������� MainDict.Count � �������� ��������� �������
      Dictionary<string, string> AuxDict2 = new Dictionary<string, string>(_MainDict.Count);
      foreach (KeyValuePair<string, TValue> Pair in _MainDict)
      {
        AuxDict2.Add(Pair.Key.ToUpperInvariant(), Pair.Key);
      }

      // ���� ��� ������, ������������� �������� ����
      _AuxDict = AuxDict2;
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// ���� true, �� ������� ����� �� �����������.
    /// �������� �������� � ������������
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    #endregion

    #region IDictionary<string,TValue> Members

    /// <summary>
    /// ��������� ���� "����-��������" � ���������.
    /// � ������ IgnoreCase=true ��������� ����������, ���� � ��������� ��� ���� ������� ����, ������������ ������ ���������
    /// </summary>
    /// <param name="key">����</param>
    /// <param name="value">��������</param>
    public void Add(string key, TValue value)
    {
      CheckNotReadOnly();
      key = PrepareKey(key);
      _MainDict.Add(key, value);
      try
      {
        if (_AuxDict != null)
          _AuxDict.Add(key.ToUpperInvariant(), key);
        if (_ReversedDict != null)
          _ReversedDict.Add(value, key);
      }
      catch
      {
        _MainDict.Remove(key);
        _AuxDict = null;
        _ReversedDict = null;
        throw;
      }
    }

    /// <summary>
    /// ���������� true, ���� ��������� �������� ��������� ����
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(string key)
    {
      return _MainDict.ContainsKey(PrepareKey(key));
    }

    /// <summary>
    /// ������ � ������ ���������.
    /// ������������ ��������� ������������� ������ ��� ���������.
    /// </summary>
    public ICollection<string> Keys { get { return _MainDict.Keys; } }

    /// <summary>
    /// ������� ���� �� ���������
    /// </summary>
    /// <param name="key">����</param>
    /// <returns>true, ���� ������� ��� ������ �� ���������</returns>
    public bool Remove(string key)
    {
      CheckNotReadOnly();

      key = PrepareKey(key);

      TValue Value;
      if (_MainDict.TryGetValue(key, out Value))
      {
        _MainDict.Remove(key);
        if (_AuxDict != null)
          _AuxDict.Remove(key.ToUpperInvariant());
        if (_ReversedDict != null)
          _ReversedDict.Remove(Value);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// ������� ������� ������� � �������� ������ �� ���������
    /// </summary>
    /// <param name="key">����</param>
    /// <param name="value">���� ������������ ��������</param>
    /// <returns>true, ���� ������� � ������ ���� � ���������</returns>
    public bool TryGetValue(string key, out TValue value)
    {
      key = PrepareKey(key);
      return _MainDict.TryGetValue(key, out value);
    }

    /// <summary>
    /// ��������� ��������.
    /// ������������ ��������� ������������� ������� ��� ���������.
    /// </summary>
    public ICollection<TValue> Values { get { return _MainDict.Values; } }

    /// <summary>
    /// ���������� ��� ������ �������� � ������.
    /// ���� ��� ������ �������� �������������� ����, ������������ ����������.
    /// ��� ��������� ��������, ���� ����������� ����� ������, ���� ���������� ������������ � ����� ������.
    /// </summary>
    /// <param name="key">����</param>
    /// <returns>��������</returns>
    public TValue this[string key]
    {
      get
      {
        key = PrepareKey(key);
        return _MainDict[key];
      }
      set
      {
        if (ContainsKey(key))
        {
          TValue OldValue = this[key];
          Remove(key);
          try
          {
            Add(key, value);
          }
          catch
          {
            Add(key, OldValue);
            throw;
          }
        }
        else
        {
          // ������ ��������� ����
          Add(key, value);
        }
      }
    }

    #endregion

    #region ICollection<KeyValuePair<string,TValue>> Members

    void ICollection<KeyValuePair<string, TValue>>.Add(KeyValuePair<string, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    /// <summary>
    /// �������� ���������
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _MainDict.Clear();
      _AuxDict = null; // ������������, ���� �����������
      _ReversedDict = null;
    }

    bool ICollection<KeyValuePair<string, TValue>>.Contains(KeyValuePair<string, TValue> item)
    {
      if (_IgnoreCase)
        item = new KeyValuePair<string, TValue>(PrepareKey(item.Key), item.Value);
      return ((ICollection<KeyValuePair<string, TValue>>)_MainDict).Contains(item);
    }

    void ICollection<KeyValuePair<string, TValue>>.CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
    {
      ((ICollection<KeyValuePair<string, TValue>>)_MainDict).CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// ���������� ���������� ��������� � ���������
    /// </summary>
    public int Count { get { return _MainDict.Count; } }

    bool ICollection<KeyValuePair<string, TValue>>.Remove(KeyValuePair<string, TValue> item)
    {
      return Remove(item.Key);
    }

    #endregion

    #region IEnumerable<KeyValuePair<string,TValue>> Members

    /// <summary>
    /// ���������� ������������� ���������.
    /// ���������� ������������ �������� ��������� KeyValuePair.
    /// ���� � ���� ����� ��� �������, ������� ������������� ��� ���������� �������� � ���������.
    /// 
    /// ��� ������������� �������� ����� ���������� � �������, 
    /// ������������� ������ ���������� ���������� �������������.
    /// ������� � ���������� ���� ����� ������ �������������� ������������� ��� ������������� � ��������� foreach.
    /// </summary>
    /// <returns>�������������</returns>
    public Dictionary<string, TValue>.Enumerator GetEnumerator()
    {
      return _MainDict.GetEnumerator();
    }

    IEnumerator<KeyValuePair<string, TValue>> IEnumerable<KeyValuePair<string, TValue>>.GetEnumerator()
    {
      return _MainDict.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _MainDict.GetEnumerator();
    }

    #endregion

    #region �������� ���������

    /// <summary>
    /// �������� ���������.
    /// ��������� ������ �� �������������.
    /// ��������� �������� ���� �������� ��������� � ������������ ��������
    /// </summary>
    [NonSerialized]
    private Dictionary<TValue, string> _ReversedDict;

    private void PrepareReversed()
    {
      if (_ReversedDict != null)
        return;

      Dictionary<TValue, string> r2 = new Dictionary<TValue, string>(_MainDict.Count);
      foreach (KeyValuePair<string, TValue> Pair in _MainDict)
        r2.Add(Pair.Value, Pair.Key);
      _ReversedDict = r2;
    }

    /// <summary>
    /// ���������� true, ���� � ��������� ���������� ��������� ��������
    /// </summary>
    /// <param name="value">�������� ��� ������ � �������� ���������</param>
    /// <returns>true, ���� �������� ����������</returns>
    public bool ContainsValue(TValue value)
    {
      PrepareReversed();
      return _ReversedDict.ContainsKey(value);
    }

    /// <summary>
    /// ������� �������� ���� �� ��������.
    /// ���� �������� <paramref name="value"/> ����������, ���������� true � �� ������ <paramref name="key"/>
    /// ������������ ���������� ��������.
    /// ���� �������� <paramref name="value"/> �� ����������, ������������ false, � � ��� ������ ������������
    /// ������ ��������
    /// </summary>
    /// <param name="value">�������� ��� ������ � �������� ���������</param>
    /// <param name="key">����, ��������������� ��������</param>
    /// <returns>true, ���� �������� ����������</returns>
    public bool TryGetKey(TValue value, out string key)
    {
      PrepareReversed();
      return _ReversedDict.TryGetValue(value, out key);
    }

    /// <summary>
    /// ������� �������� �� ���������
    /// </summary>
    /// <param name="value">�������� ��� ������ � ��������</param>
    /// <returns>true, ���� �������� ���� ������� � �������� ���������</returns>
    public bool RemoveValue(TValue value)
    {
      CheckNotReadOnly();
      PrepareReversed();
      string key;
      if (_ReversedDict.TryGetValue(value, out key))
      {
        bool Res = Remove(key);
        if (!Res)
          throw new BugException("������ ������������� �������� � �������� ���������");
        return true;
      }
      else
        return false;
    }

    #endregion

    #region ������ ������ ��� ������

    /// <summary>
    /// ���������� true, ���� ��������� ���� ���������� � ����� "������ ������"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// ���������� ����� ��� �������� ��������� � ����� "������ ������"
    /// </summary>
    protected void SetReadOnly()
    {
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

    #region ��������������

    // ���������� ����������� ��������������

    #endregion

    #region ������

    /// <summary>
    /// ���������� ������ ���� "Count=XXX"
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    bool INamedValuesAccess.Contains(string name)
    {
      return ContainsKey(name);
    }

    string[] INamedValuesAccess.GetNames()
    {
      string[] a = new string[Count];
      Keys.CopyTo(a, 0);
      return a;
    }

    #endregion
  }


  /// <summary>
  /// ������� �����, ����������� ������� ����� ��������� � ������� �����.
  /// �������� ������ Contains � IndexOf.
  /// ������������ ������������� ��������
  /// �������� ������ ������ ��������� � �������� ����� ���������: �������� ������ ���� �����������,
  /// �������� null �����������.
  /// �� �������� ��������� �������.
  /// ���� ����� �� �������� �������������, �.�. ����� ����� ���� ���������.
  /// ��������� ��������� ��������� IComparer ��� ���������� ������ �������� � ������� (����� Compare()).
  /// ����� �������� ����������������.
  /// </summary>
  public sealed class StringArrayIndexer : IComparer<string>
  {
    #region ������������
    /// <summary>
    /// ������� ���������� ��� �������.
    /// ��� ������ ������������� ��������� ������� ��������.
    /// </summary>
    /// <param name="source">������������� ������</param>
    public StringArrayIndexer(string[] source)
      :this(source, false)
    { 
    }

    /// <summary>
    /// ������� ���������� ��� �������.
    /// </summary>
    /// <param name="source">������������� ������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public StringArrayIndexer(string[] source, bool ignoreCase)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      _Dict = new Dictionary<string, int>(source.Length);
      for (int i = 0; i < source.Length; i++)
      {
        if (ignoreCase)
          _Dict.Add(source[i].ToUpperInvariant(), i);
        else
          _Dict.Add(source[i], i);
      }

      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ���������� ��� ��������� �����.
    /// ��� ������ ������������� ��������� ������� ��������.
    /// </summary>
    /// <param name="source">������������� ���������</param>
    public StringArrayIndexer(ICollection<string> source)
      :this(source, false)
    { 
    }

    /// <summary>
    /// ������� ���������� ��� ��������� �����.
    /// </summary>
    /// <param name="source">������������� ���������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public StringArrayIndexer(ICollection<string> source, bool ignoreCase)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      _Dict = new Dictionary<string, int>(source.Count);
      int cnt = 0;
      foreach (string Item in source)
      {
        if (ignoreCase)
          _Dict.Add(Item.ToUpperInvariant(), cnt);
        else
          _Dict.Add(Item, cnt);
        cnt++;
      }
    }

    private StringArrayIndexer()
      : this(DataTools.EmptyStrings, false)
    {
      _IsReadOnly = true;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������ �������� ������, ����������� � �������� ���������, ���� IgnoreCase=true
    /// </summary>
    private Dictionary<string, int> _Dict;

    /// <summary>
    /// ���������� ��������� � �������
    /// </summary>
    public int Count { get { return _Dict.Count; } }

    /// <summary>
    /// ���� true, �� ������� ����� �� �����������.
    /// �������� �������� � ������������
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    #endregion

    #region ������

    /// <summary>
    /// ��������� ������������� "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + _Dict.Count.ToString();
    }

    /// <summary>
    /// ���������� ������ �������� � �������.
    /// � ������� �� Array.IndexOf(), ����������� ������
    /// </summary>
    /// <param name="item">������� ������</param>
    /// <returns>������� � ������</returns>
    public int IndexOf(string item)
    {
      if (_IgnoreCase)
        item = item.ToUpperInvariant();

      int p;
      if (_Dict.TryGetValue(item, out p))
        return p;
      else
        return -1;
    }

    /// <summary>
    /// ���������� true, ���� � ��������������� ���������/������� ���� ��������� ������ (� ������ IgnoreCase)
    /// </summary>
    /// <param name="item">������� ������</param>
    /// <returns>������� ������� ������</returns>
    public bool Contains(string item)
    {
      if (_IgnoreCase)
        item = item.ToUpperInvariant();

      return _Dict.ContainsKey(item);
    }


    /// <summary>
    /// ���������� true, ���� � ������ ���������� ��� ��������, �� ���� ���� Contains() ���������� true ��� ������� ��������.
    /// ���� ����������� ������ ������, ���������� true.
    /// </summary>
    /// <param name="items">����������� ������ ���������</param>
    /// <returns>������� ���������</returns>
    public bool ContainsAll(IEnumerable<string> items)
    {
      foreach (string item in items)
      {
        if (!Contains(item))
          return false;
      }
      return true;
    }

    /// <summary>
    /// ���������� true, ���� � ������ ���� ���� �� ���� �������, �� ���� ���� Contains() ���������� true ��� ������-���� ��������
    /// ���� ����������� ������ ������, ���������� false.
    /// </summary>
    /// <param name="items">����������� ������ ���������</param>
    /// <returns>������� ���������</returns>
    public bool ContainsAny(IEnumerable<string> items)
    {
      foreach (string item in items)
      {
        if (Contains(item))
          return true;
      }
      return false;
    }

    #endregion

    #region IComparer<T> members

    /// <summary>
    /// ���������� ����, ������������� ��� ������ Empty
    /// </summary>
    private bool _IsReadOnly;

    /// <summary>
    /// ��������� ����������� ��������� ��� ���������� � ������� ������ Compare().
    /// �� ��������� - First - ����������� �������� ������������� � ������ ������.
    /// </summary>
    public UnknownItemPosition UnknownItemPosition
    {
      get { return _UnknownItemPosition; }
      set
      {
        if (_IsReadOnly)
          throw new ObjectReadOnlyException();

        switch (value)
        {
          case FreeLibSet.Collections.UnknownItemPosition.First:
          case FreeLibSet.Collections.UnknownItemPosition.Last:
            _UnknownItemPosition = value;
            break;
          default:
            throw new ArgumentException();
        }
      }
    }
    private UnknownItemPosition _UnknownItemPosition;

    /// <summary>
    /// ��������� ��������� ���� ���������.
    /// ����� ����� ���� ����������� ��� ���������� ������������ ������� � ��������, �����
    /// ������������� �� � ������������ � �������� ��������� � ������� ������� StringArrayIndexer.
    /// ������������ ��������� ��������� � ������� �������, � �� ������.
    /// ���� �����-���� �������� ����������� � ������� �������, �� ��� ����� ����������� �
    /// ������ ��� � ����� ������, � ����������� �� �������� UnknownItemPosition.
    /// 
    /// ����� ���������� ������������� ��������, ���� <paramref name="x"/> ������������� �����
    /// � ������ ������, ��� <paramref name="y"/>. ������������� �������� ������������, ����
    /// <paramref name="x"/> ������������� ����� � ����� ������, ��� <paramref name="y"/>. 
    /// ���� ����� �������� ��� � ������� ������, �� ������������ ��������� ��������� �����.
    /// /// </summary>
    /// <param name="x">������ ������������ ��������</param>
    /// <param name="y">������ ������������ ��������</param>
    /// <returns>��������� ��������� �������</returns>
    public int Compare(string x, string y)
    {
      int px = IndexOf(x);
      int py = IndexOf(y);

      if (px < 0 && py < 0)
      {
        // ���� ����� ��������� ��� � ������, ���������� ��������
        return String.Compare(x, y, IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
      }

      if (UnknownItemPosition == FreeLibSet.Collections.UnknownItemPosition.Last)
      {
        if (px < 0)
          px = int.MaxValue;
        if (py < 0)
          py = int.MaxValue;
      }

      return px.CompareTo(py);
    }

    #endregion

    #region ����������� ������

    /// <summary>
    /// ������ ������ - ����������
    /// </summary>
    public static readonly StringArrayIndexer Empty = new StringArrayIndexer();

    #endregion
  }

  /// <summary>
  /// ������� �����, ����������� ������� ����� ��������.
  /// �������� ������ Contains() � IndexOf().
  /// �������� ������ ������ ��������� � �������� ����� ���������: �������� ������ ���� �����������,
  /// �������� null �����������.
  /// �� �������� ��������� �������.
  /// ���� ����� �� �������� �������������, �.�. ����� ����� ���� ���������.
  /// ��������� ArrayIndexer of Char �� ����������� ������������� �� ������.
  /// ����� ������������ ����� � �������������� ��������
  /// ��������� ��������� ��������� IComparer ��� ���������� ������ �������� � ������� (����� Compare()).
  /// ����� �������� ����������������.
  /// </summary>
  public sealed class CharArrayIndexer : IComparer<char>
  {
    #region ������������

    /// <summary>
    /// ������� ���������� ��� �������.
    /// � ������� �� ArrayIndexer of Char, ����������� ������� � <paramref name="source"/> ������������� ��������, ������� �������������.
    /// ��������� ����� �������������� � �������� ��������.
    /// </summary>
    /// <param name="source">������ ��������</param>
    public CharArrayIndexer(char[] source)
      :this(source, false)
    {
    }

    /// <summary>
    /// ������� ���������� ��� �������.
    /// � ������� �� ArrayIndexer of Char, ����������� ������� � <paramref name="source"/> ������������� ��������, ������� �������������
    /// </summary>
    /// <param name="source">������ ��������</param>
    /// <param name="ignoreCase">���� true, �� ����� �������������� ������� ��������</param>
    public CharArrayIndexer(char[] source, bool ignoreCase)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      _IgnoreCase = ignoreCase;

      _Dict = new Dictionary<char, int>(source.Length);
      for (int i = 0; i < source.Length; i++)
      {
        char ch = source[i];
        if (ignoreCase)
          ch = Char.ToUpperInvariant(ch);
        _Dict[ch] = i;
      }
    }


    /// <summary>
    /// ������� ���������� ��� ������ ��������
    /// ����������� ������� � <paramref name="source"/> ������������� ��������, ������� �������������
    /// ��������� ����� �������������� � �������� ��������.
    /// </summary>
    /// <param name="source">������ ��������</param>
    public CharArrayIndexer(string source)
      :this(source, false)
    {
    }

    /// <summary>
    /// ������� ���������� ��� ������ ��������
    /// ����������� ������� � <paramref name="source"/> ������������� ��������, ������� �������������
    /// </summary>
    /// <param name="source">������ ��������</param>
    /// <param name="ignoreCase">���� true, �� ����� �������������� ������� ��������</param>
    public CharArrayIndexer(string source, bool ignoreCase)
    {
      if (source == null)
        source = String.Empty;

      _IgnoreCase = ignoreCase;

      if (ignoreCase)
        source = source.ToUpperInvariant();

      _Dict = new Dictionary<char, int>(source.Length);
      for (int i = 0; i < source.Length; i++)
        _Dict[source[i]] = i;
    }

    private CharArrayIndexer()
      : this(String.Empty, false)
    {
      _IsReadOnly = true;
    }

    #endregion

    #region ��������

    private Dictionary<char, int> _Dict;

    /// <summary>
    /// ���������� ��������� � �������
    /// </summary>
    public int Count { get { return _Dict.Count; } }

    /// <summary>
    /// ���� true, �� ������� ����� �� �����������.
    /// �������� �������� � ������������
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    #endregion

    #region ������

    /// <summary>
    /// ��������� ������������� ��� �������
    /// </summary>
    /// <returns>������ ���� "Count=XXX"</returns>
    public override string ToString()
    {
      return "Count=" + _Dict.Count.ToString();
    }

    /// <summary>
    /// ���������� ������ �������� � �������.
    /// � ������� �� Array.IndexOf(), ����������� ������
    /// </summary>
    /// <param name="item">������ ��� ������</param>
    /// <returns>������ ��������</returns>
    public int IndexOf(char item)
    {
      if (_IgnoreCase)
        item = Char.ToUpperInvariant(item);

      int p;
      if (_Dict.TryGetValue(item, out p))
        return p;
      else
        return -1;
    }

    /// <summary>
    /// ���������� ������ ������ ������� �� ������ <paramref name="s"/>, ���� �� ���� � ������� �������.
    /// ���� � ������� ������� ��� �� ������ ������� �� ������ <paramref name="s"/>, ������������ (-1)
    /// </summary>
    /// <param name="s">������� ��� ������</param>
    /// <returns>������ ������� ���������� �������</returns>
    public int IndexOfAny(string s)
    {
      if (String.IsNullOrEmpty(s))
        return -1;

      if (_IgnoreCase)
        s = s.ToUpperInvariant();

      int p;
      for (int i = 0; i < s.Length; i++)
      {
        if (_Dict.TryGetValue(s[i], out p))
          return p;
      }
      return -1;
    }

    /// <summary>
    /// ���������� true, ���� ������� ���� � �������� �������
    /// </summary>
    /// <param name="item">������ ��� ������</param>
    /// <returns>true, ���� ������ ���� � ������</returns>
    public bool Contains(char item)
    {
      if (_IgnoreCase)
        item = Char.ToUpperInvariant(item);

      return _Dict.ContainsKey(item);
    }

    /// <summary>
    /// ���������� true, ���� � ������� ����������� ���� ���� �� ���� ������ �� ������ <paramref name="s"/>
    /// </summary>
    /// <param name="s">����������� ������ ��������</param>
    /// <returns>��������� ������</returns>
    public bool ContainsAny(string s)
    {
      if (String.IsNullOrEmpty(s))
        return false;

      if (_IgnoreCase)
        s = s.ToUpperInvariant();

      for (int i = 0; i < s.Length; i++)
      {
        if (_Dict.ContainsKey(s[i]))
          return true;
      }
      return false;
    }

    /// <summary>
    /// ���������� true, ���� � ������� ����������� ���� ��� ������� �� ������ <paramref name="s"/>
    /// </summary>
    /// <param name="s">����������� ������ ��������</param>
    /// <returns>��������� ������</returns>
    public bool ContainsAll(string s)
    {
      if (String.IsNullOrEmpty(s))
        return true;

      if (_IgnoreCase)
        s = s.ToUpperInvariant();

      for (int i = 0; i < s.Length; i++)
      {
        if (!_Dict.ContainsKey(s[i]))
          return false;
      }
      return true;
    }

    /// <summary>
    /// ���������� true, ���� � ������ ���������� ��� ��������, �� ���� ���� Contains() ���������� true ��� ������� ��������.
    /// ���� ����������� ������ ������, ���������� true.
    /// </summary>
    /// <param name="items">����������� ������ ���������</param>
    /// <returns>������� ���������</returns>
    public bool ContainsAll(IEnumerable<char> items)
    {
      foreach (char item in items)
      {
        if (!Contains(item))
          return false;
      }
      return true;
    }

    /// <summary>
    /// ���������� true, ���� � ������ ���� ���� �� ���� �������, �� ���� ���� Contains() ���������� true ��� ������-���� ��������
    /// ���� ����������� ������ ������, ���������� false.
    /// </summary>
    /// <param name="items">����������� ������ ���������</param>
    /// <returns>������� ���������</returns>
    public bool ContainsAny(IEnumerable<char> items)
    {
      foreach (char item in items)
      {
        if (Contains(item))
          return true;
      }
      return false;
    }

    #endregion

    #region IComparer<T> members

    /// <summary>
    /// ��������������� ��� ������ Empty
    /// </summary>
    private bool _IsReadOnly;

    /// <summary>
    /// ��������� ����������� ��������� ��� ���������� � ������� ������ Compare().
    /// �� ��������� - First - ����������� �������� ������������� � ������ ������.
    /// </summary>
    public UnknownItemPosition UnknownItemPosition
    {
      get { return _UnknownItemPosition; }
      set
      {
        if (_IsReadOnly)
          throw new ObjectReadOnlyException();

        switch (value)
        {
          case FreeLibSet.Collections.UnknownItemPosition.First:
          case FreeLibSet.Collections.UnknownItemPosition.Last:
            _UnknownItemPosition = value;
            break;
          default:
            throw new ArgumentException();
        }
      }
    }
    private UnknownItemPosition _UnknownItemPosition;

    /// <summary>
    /// ��������� ��������� ���� ��������.
    /// ����� ����� ���� ����������� ��� ���������� ������������ ������� � ��������, �����
    /// ������������� �� � ������������ � �������� ��������� � ������� ������� CharArrayIndexer.
    /// ������������ ��������� ��������� � ������� �������, � �� ���� ��������.
    /// ���� �����-���� �������� ����������� � ������� �������, �� ��� ����� ����������� �
    /// ������ ��� � ����� ������, � ����������� �� �������� UnknownItemPosition.
    /// ���� ����� �������� ��� � ������� ������, �� ������������ ��������� ��������� ��������.
    /// 
    /// ����� ���������� ������������� ��������, ���� <paramref name="x"/> ������������� �����
    /// � ������ ������, ��� <paramref name="y"/>. ������������� �������� ������������, ����
    /// <paramref name="x"/> ������������� ����� � ����� ������, ��� <paramref name="y"/>. 
    /// </summary>
    /// <param name="x">������ ������������ ��������</param>
    /// <param name="y">������ ������������ ��������</param>
    /// <returns>��������� ��������� �������</returns>
    public int Compare(char x, char y)
    {
      int px = IndexOf(x);
      int py = IndexOf(y);

      if (px < 0 && py < 0)
      {
        // ���� ����� ��������� ��� � ������, ���������� ��������
        return x.CompareTo(y);
      }

      if (UnknownItemPosition == FreeLibSet.Collections.UnknownItemPosition.Last)
      {
        if (px < 0)
          px = int.MaxValue;
        if (py < 0)
          py = int.MaxValue;
      }

      return px.CompareTo(py);
    }

    #endregion

    #region ����������� ������

    /// <summary>
    /// ������ ������ - ����������
    /// </summary>
    public static readonly CharArrayIndexer Empty = new CharArrayIndexer();

    #endregion
  }

  #region �������� ��������� �� ��������� ������

  /// <summary>
  /// ������� �������� �������� �� ��������� ������.
  /// ������� ����� ���� �� ������������ � �������� ����� (������������ � ������������).
  /// </summary>
  [Serializable]
  public class IntNamedDictionary : TypedStringDictionary<int>
  {
    #region ������������

    /// <summary>
    /// �������� ������ ������������.
    /// ������� ������� � ������, �������������� � ��������
    /// </summary>
    public IntNamedDictionary()
      : this(false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������
    /// </summary>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public IntNamedDictionary(bool ignoreCase)
      : base(ignoreCase)
    {
    }

    /// <summary>
    /// ������� ������� � ������, �������������� � ��������
    /// ��� ������ ������� ������������, ���� ������� ��������, ������� ����� ��������� � �������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    public IntNamedDictionary(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������.
    /// ��� ������ ������� ������������, ���� ������� ��������, ������� ����� ��������� � �������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public IntNamedDictionary(int capacity, bool ignoreCase)
      : base(capacity, ignoreCase)
    {
    }

    /// <summary>
    /// ������� ������� � ������, �������������� � �������� � ��������� ��� ����������.
    /// </summary>
    /// <param name="dictionary">��������, ������ ������� ��������</param>
    public IntNamedDictionary(IDictionary<string, int> dictionary)
      : this(dictionary, false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������, � ��������� ��� ����������.
    /// </summary>
    /// <param name="dictionary">��������, ������ ������� ��������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public IntNamedDictionary(IDictionary<string, int> dictionary, bool ignoreCase)
      : base(dictionary, ignoreCase)
    {
    }

    #endregion

    #region ������ � ���������

    /// <summary>
    /// ��������� ��� ��������� ��������.
    /// ���� � ������� ��� �������� � ��������� ������, ������������ 0
    /// (��� ������� ����������, ��� ��� ������� � ����������� ����������)
    /// </summary>
    /// <param name="key">����</param>
    /// <returns>��������</returns>
    public new int this[string key]
    {
      get
      {
        int v;
        if (base.TryGetValue(key, out v))
          return v;
        else
          return 0;
      }
      set
      {
        base[key] = value;
      }
    }

    #endregion

    #region �������� � ���������

    /// <summary>
    /// �������� � ������� ��������� �������� �� ������ ���������.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="source">������� "���-��������", ������ ������� ����������� ��������.
    /// ���� null, �� ������� �������� �� �����������</param>
    public void Add(IDictionary<string, int> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, int> Pair in source)
        checked { this[Pair.Key] += Pair.Value; }
    }

    /// <summary>
    /// ������� �� ������� ��������� �������� �� ������ ���������.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="source">������� "���-��������", ������ ������� ���������� ��������.
    /// ���� null, �� ������� �������� �� �����������</param>
    public void Substract(IDictionary<string, int> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, int> Pair in source)
        checked { this[Pair.Key] -= Pair.Value; }
    }

    /// <summary>
    /// �������� ���� ���������.
    /// ��� ���������� ��������� ������ ��������� ����� ���������, ������� ������ ����������� ������������
    /// ������������� ����� Add(), ������� ���������� ������������ ���������.
    /// �������� IgnoreCase ����� ��������� ��������������� � ������������ �� ��������� �������� ������ ���������.
    /// </summary>
    /// <param name="a">������ �������� ���������</param>
    /// <param name="b">������ �������� ���������</param>
    /// <returns>����� ���������</returns>
    public static IntNamedDictionary operator +(IntNamedDictionary a, IDictionary<string, int> b)
    {
      IntNamedDictionary Res = new IntNamedDictionary(a, a.IgnoreCase);
      Res.Add(b);
      return Res;
    }

    /// <summary>
    /// ��������� ����� ��������� �� ������.
    /// ��� ���������� ��������� ������ ��������� ����� ���������, ������� ������ ����������� ������������
    /// ������������� ����� Substract(), ������� ���������� ������������ ���������.
    /// �������� IgnoreCase ����� ��������� ��������������� � ������������ �� ��������� �������� ������ ���������.
    /// </summary>
    /// <param name="a">������ �������� ���������</param>
    /// <param name="b">������ �������� ���������</param>
    /// <returns>����� ���������</returns>
    public static IntNamedDictionary operator -(IntNamedDictionary a, IDictionary<string, int> b)
    {
      IntNamedDictionary Res = new IntNamedDictionary(a, a.IgnoreCase);
      Res.Substract(b);
      return Res;
    }

    #endregion

    #region ��������� � �������

    /// <summary>
    /// ��������� ���� �������� ��������� �� �������� �����.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="m">���������</param>
    public void Multiply(int m)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        checked { this[Codes[i]] *= m; }
    }

    /// <summary>
    /// ������� ���� �������� ��������� �� �������� �����.
    /// ������� ����������� � ����������� ���������� �� �������� ��� ����� �����.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="d">��������. �� ����� ���� ����� 0</param>
    public void Divide(int d)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        checked { this[Codes[i]] /= d; }
    }

    #endregion
  }

  /// <summary>
  /// ������� �������� �������� �� ��������� ������.
  /// ������� ����� ���� �� ������������ � �������� ����� (������������ � ������������).
  /// </summary>
  [Serializable]
  public class Int64NamedDictionary : TypedStringDictionary<long>
  {
    #region ������������

    /// <summary>
    /// �������� ������ ������������.
    /// ������� ������� � ������, �������������� � ��������
    /// </summary>
    public Int64NamedDictionary()
      : this(false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������
    /// </summary>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public Int64NamedDictionary(bool ignoreCase)
      : base(ignoreCase)
    {
    }

    /// <summary>
    /// ������� ������� � ������, �������������� � ��������
    /// ��� ������ ������� ������������, ���� ������� ��������, ������� ����� ��������� � �������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    public Int64NamedDictionary(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������.
    /// ��� ������ ������� ������������, ���� ������� ��������, ������� ����� ��������� � �������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public Int64NamedDictionary(int capacity, bool ignoreCase)
      : base(capacity, ignoreCase)
    {
    }

    /// <summary>
    /// ������� ������� � ������, �������������� � �������� � ��������� ��� ����������.
    /// </summary>
    /// <param name="dictionary">��������, ������ ������� ��������</param>
    public Int64NamedDictionary(IDictionary<string, long> dictionary)
      : this(dictionary, false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������, � ��������� ��� ����������.
    /// </summary>
    /// <param name="dictionary">��������, ������ ������� ��������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public Int64NamedDictionary(IDictionary<string, long> dictionary, bool ignoreCase)
      : base(dictionary, ignoreCase)
    {
    }

    #endregion

    #region ������ � ���������

    /// <summary>
    /// ��������� ��� ��������� ��������.
    /// ���� � ������� ��� �������� � ��������� ������, ������������ 0
    /// (��� ������� ����������, ��� ��� ������� � ����������� ����������)
    /// </summary>
    /// <param name="key">����</param>
    /// <returns>��������</returns>
    public new long this[string key]
    {
      get
      {
        long v;
        if (base.TryGetValue(key, out v))
          return v;
        else
          return 0L;
      }
      set
      {
        base[key] = value;
      }
    }

    #endregion

    #region �������� � ���������

    /// <summary>
    /// �������� � ������� ��������� �������� �� ������ ���������.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="source">������� "���-��������", ������ ������� ����������� ��������.
    /// ���� null, �� ������� �������� �� �����������</param>
    public void Add(IDictionary<string, long> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, long> Pair in source)
        checked { this[Pair.Key] += Pair.Value; }
    }

    /// <summary>
    /// ������� �� ������� ��������� �������� �� ������ ���������.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="source">������� "���-��������", ������ ������� ���������� ��������.
    /// ���� null, �� ������� �������� �� �����������</param>
    public void Substract(IDictionary<string, long> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, long> Pair in source)
        checked { this[Pair.Key] -= Pair.Value; }
    }

    /// <summary>
    /// �������� ���� ���������.
    /// ��� ���������� ��������� ������ ��������� ����� ���������, ������� ������ ����������� ������������
    /// ������������� ����� Add(), ������� ���������� ������������ ���������.
    /// �������� IgnoreCase ����� ��������� ��������������� � ������������ �� ��������� �������� ������ ���������.
    /// </summary>
    /// <param name="a">������ �������� ���������</param>
    /// <param name="b">������ �������� ���������</param>
    /// <returns>����� ���������</returns>
    public static Int64NamedDictionary operator +(Int64NamedDictionary a, IDictionary<string, long> b)
    {
      Int64NamedDictionary Res = new Int64NamedDictionary(a, a.IgnoreCase);
      Res.Add(b);
      return Res;
    }

    /// <summary>
    /// ��������� ����� ��������� �� ������.
    /// ��� ���������� ��������� ������ ��������� ����� ���������, ������� ������ ����������� ������������
    /// ������������� ����� Substract(), ������� ���������� ������������ ���������.
    /// �������� IgnoreCase ����� ��������� ��������������� � ������������ �� ��������� �������� ������ ���������.
    /// </summary>
    /// <param name="a">������ �������� ���������</param>
    /// <param name="b">������ �������� ���������</param>
    /// <returns>����� ���������</returns>
    public static Int64NamedDictionary operator -(Int64NamedDictionary a, IDictionary<string, long> b)
    {
      Int64NamedDictionary Res = new Int64NamedDictionary(a, a.IgnoreCase);
      Res.Substract(b);
      return Res;
    }

    #endregion

    #region ��������� � �������

    /// <summary>
    /// ��������� ���� �������� ��������� �� �������� �����.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="m">���������</param>
    public void Multiply(long m)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        checked { this[Codes[i]] *= m; }
    }

    /// <summary>
    /// ������� ���� �������� ��������� �� �������� �����.
    /// ������� ����������� � ����������� ���������� �� �������� ��� ����� �����.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="d">��������. �� ����� ���� ����� 0</param>
    public void Divide(long d)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        checked { this[Codes[i]] /= d; }
    }

    #endregion
  }

  /// <summary>
  /// ������� �������� �������� �� ��������� ������.
  /// ������� ����� ���� �� ������������ � �������� ����� (������������ � ������������).
  /// </summary>
  [Serializable]
  public class SingleNamedDictionary : TypedStringDictionary<float>
  {
    #region ������������

    /// <summary>
    /// �������� ������ ������������.
    /// ������� ������� � ������, �������������� � ��������
    /// </summary>
    public SingleNamedDictionary()
      : this(false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������
    /// </summary>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public SingleNamedDictionary(bool ignoreCase)
      : base(ignoreCase)
    {
    }

    /// <summary>
    /// ������� ������� � ������, �������������� � ��������
    /// ��� ������ ������� ������������, ���� ������� ��������, ������� ����� ��������� � �������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    public SingleNamedDictionary(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������.
    /// ��� ������ ������� ������������, ���� ������� ��������, ������� ����� ��������� � �������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public SingleNamedDictionary(int capacity, bool ignoreCase)
      : base(capacity, ignoreCase)
    {
    }

    /// <summary>
    /// ������� ������� � ������, �������������� � �������� � ��������� ��� ����������.
    /// </summary>
    /// <param name="dictionary">��������, ������ ������� ��������</param>
    public SingleNamedDictionary(IDictionary<string, float> dictionary)
      : this(dictionary, false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������, � ��������� ��� ����������.
    /// </summary>
    /// <param name="dictionary">��������, ������ ������� ��������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public SingleNamedDictionary(IDictionary<string, float> dictionary, bool ignoreCase)
      : base(dictionary, ignoreCase)
    {
    }

    #endregion

    #region ������ � ���������

    /// <summary>
    /// ��������� ��� ��������� ��������.
    /// ���� � ������� ��� �������� � ��������� ������, ������������ 0
    /// (��� ������� ����������, ��� ��� ������� � ����������� ����������)
    /// </summary>
    /// <param name="key">����</param>
    /// <returns>��������</returns>
    public new float this[string key]
    {
      get
      {
        float v;
        if (base.TryGetValue(key, out v))
          return v;
        else
          return 0f;
      }
      set
      {
        base[key] = value;
      }
    }

    #endregion

    #region �������� � ���������

    /// <summary>
    /// �������� � ������� ��������� �������� �� ������ ���������.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="source">������� "���-��������", ������ ������� ����������� ��������.
    /// ���� null, �� ������� �������� �� �����������</param>
    public void Add(IDictionary<string, float> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, float> Pair in source)
        this[Pair.Key] += Pair.Value;
    }

    /// <summary>
    /// ������� �� ������� ��������� �������� �� ������ ���������.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="source">������� "���-��������", ������ ������� ���������� ��������.
    /// ���� null, �� ������� �������� �� �����������</param>
    public void Substract(IDictionary<string, float> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, float> Pair in source)
        this[Pair.Key] -= Pair.Value;
    }

    /// <summary>
    /// �������� ���� ���������.
    /// ��� ���������� ��������� ������ ��������� ����� ���������, ������� ������ ����������� ������������
    /// ������������� ����� Add(), ������� ���������� ������������ ���������.
    /// �������� IgnoreCase ����� ��������� ��������������� � ������������ �� ��������� �������� ������ ���������.
    /// </summary>
    /// <param name="a">������ �������� ���������</param>
    /// <param name="b">������ �������� ���������</param>
    /// <returns>����� ���������</returns>
    public static SingleNamedDictionary operator +(SingleNamedDictionary a, IDictionary<string, float> b)
    {
      SingleNamedDictionary Res = new SingleNamedDictionary(a, a.IgnoreCase);
      Res.Add(b);
      return Res;
    }

    /// <summary>
    /// ��������� ����� ��������� �� ������.
    /// ��� ���������� ��������� ������ ��������� ����� ���������, ������� ������ ����������� ������������
    /// ������������� ����� Substract(), ������� ���������� ������������ ���������.
    /// �������� IgnoreCase ����� ��������� ��������������� � ������������ �� ��������� �������� ������ ���������.
    /// </summary>
    /// <param name="a">������ �������� ���������</param>
    /// <param name="b">������ �������� ���������</param>
    /// <returns>����� ���������</returns>
    public static SingleNamedDictionary operator -(SingleNamedDictionary a, IDictionary<string, float> b)
    {
      SingleNamedDictionary Res = new SingleNamedDictionary(a, a.IgnoreCase);
      Res.Substract(b);
      return Res;
    }

    #endregion

    #region ��������� � �������

    /// <summary>
    /// ��������� ���� �������� ��������� �� �������� �����.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="m">���������</param>
    public void Multiply(float m)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] *= m;
    }

    /// <summary>
    /// ������� ���� �������� ��������� �� �������� �����.
    /// ������� ����������� ��� ����������.
    /// ����������� ����� Round() ����� ���������� �������
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="d">��������. �� ����� ���� ����� 0</param>
    public void Divide(float d)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] /= d;
    }

    #endregion

    #region ����������

    /// <summary>
    /// ��������� ���������� ���� ��������� ��������� �� ��������� ����� ������ ����� �������.
    /// ������������ ������� ��������������� ����������.
    /// </summary>
    /// <param name="decimals">����� ������ ����� �������</param>
    public void Round(int decimals)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] = (float)(Math.Round((double)(this[Codes[i]]), decimals, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// ��������� ���������� ���� ��������� ��������� �� ����� ��������.
    /// ������������ ������� ��������������� ����������.
    /// </summary>
    public void Round()
    {
      Round(0);
    }

    #endregion
  }

  /// <summary>
  /// ������� �������� �������� �� ��������� ������.
  /// ������� ����� ���� �� ������������ � �������� ����� (������������ � ������������).
  /// </summary>
  [Serializable]
  public class DoubleNamedDictionary : TypedStringDictionary<double>
  {
    #region ������������

    /// <summary>
    /// �������� ������ ������������.
    /// ������� ������� � ������, �������������� � ��������
    /// </summary>
    public DoubleNamedDictionary()
      : this(false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������
    /// </summary>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public DoubleNamedDictionary(bool ignoreCase)
      : base(ignoreCase)
    {
    }

    /// <summary>
    /// ������� ������� � ������, �������������� � ��������
    /// ��� ������ ������� ������������, ���� ������� ��������, ������� ����� ��������� � �������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    public DoubleNamedDictionary(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������.
    /// ��� ������ ������� ������������, ���� ������� ��������, ������� ����� ��������� � �������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public DoubleNamedDictionary(int capacity, bool ignoreCase)
      : base(capacity, ignoreCase)
    {
    }

    /// <summary>
    /// ������� ������� � ������, �������������� � �������� � ��������� ��� ����������.
    /// </summary>
    /// <param name="dictionary">��������, ������ ������� ��������</param>
    public DoubleNamedDictionary(IDictionary<string, double> dictionary)
      : this(dictionary, false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������, � ��������� ��� ����������.
    /// </summary>
    /// <param name="dictionary">��������, ������ ������� ��������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public DoubleNamedDictionary(IDictionary<string, double> dictionary, bool ignoreCase)
      : base(dictionary, ignoreCase)
    {
    }

    #endregion

    #region ������ � ���������

    /// <summary>
    /// ��������� ��� ��������� ��������.
    /// ���� � ������� ��� �������� � ��������� ������, ������������ 0
    /// (��� ������� ����������, ��� ��� ������� � ����������� ����������)
    /// </summary>
    /// <param name="key">����</param>
    /// <returns>��������</returns>
    public new double this[string key]
    {
      get
      {
        double v;
        if (base.TryGetValue(key, out v))
          return v;
        else
          return 0.0;
      }
      set
      {
        base[key] = value;
      }
    }

    #endregion

    #region �������� � ���������

    /// <summary>
    /// �������� � ������� ��������� �������� �� ������ ���������.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="source">������� "���-��������", ������ ������� ����������� ��������.
    /// ���� null, �� ������� �������� �� �����������</param>
    public void Add(IDictionary<string, double> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, double> Pair in source)
        this[Pair.Key] += Pair.Value;
    }

    /// <summary>
    /// ������� �� ������� ��������� �������� �� ������ ���������.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="source">������� "���-��������", ������ ������� ���������� ��������.
    /// ���� null, �� ������� �������� �� �����������</param>
    public void Substract(IDictionary<string, double> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, double> Pair in source)
        this[Pair.Key] -= Pair.Value;
    }

    /// <summary>
    /// �������� ���� ���������.
    /// ��� ���������� ��������� ������ ��������� ����� ���������, ������� ������ ����������� ������������
    /// ������������� ����� Add(), ������� ���������� ������������ ���������.
    /// �������� IgnoreCase ����� ��������� ��������������� � ������������ �� ��������� �������� ������ ���������.
    /// </summary>
    /// <param name="a">������ �������� ���������</param>
    /// <param name="b">������ �������� ���������</param>
    /// <returns>����� ���������</returns>
    public static DoubleNamedDictionary operator +(DoubleNamedDictionary a, IDictionary<string, double> b)
    {
      DoubleNamedDictionary Res = new DoubleNamedDictionary(a, a.IgnoreCase);
      Res.Add(b);
      return Res;
    }

    /// <summary>
    /// ��������� ����� ��������� �� ������.
    /// ��� ���������� ��������� ������ ��������� ����� ���������, ������� ������ ����������� ������������
    /// ������������� ����� Substract(), ������� ���������� ������������ ���������.
    /// �������� IgnoreCase ����� ��������� ��������������� � ������������ �� ��������� �������� ������ ���������.
    /// </summary>
    /// <param name="a">������ �������� ���������</param>
    /// <param name="b">������ �������� ���������</param>
    /// <returns>����� ���������</returns>
    public static DoubleNamedDictionary operator -(DoubleNamedDictionary a, IDictionary<string, double> b)
    {
      DoubleNamedDictionary Res = new DoubleNamedDictionary(a, a.IgnoreCase);
      Res.Substract(b);
      return Res;
    }

    #endregion

    #region ��������� � �������

    /// <summary>
    /// ��������� ���� �������� ��������� �� �������� �����.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="m">���������</param>
    public void Multiply(double m)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] *= m;
    }

    /// <summary>
    /// ������� ���� �������� ��������� �� �������� �����.
    /// ������� ����������� ��� ����������.
    /// ����������� ����� Round() ����� ���������� �������
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="d">��������. �� ����� ���� ����� 0</param>
    public void Divide(double d)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] /= d;
    }

    #endregion

    #region ����������

    /// <summary>
    /// ��������� ���������� ���� ��������� ��������� �� ��������� ����� ������ ����� �������.
    /// ������������ ������� ��������������� ����������.
    /// </summary>
    /// <param name="decimals">����� ������ ����� �������</param>
    public void Round(int decimals)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] = Math.Round(this[Codes[i]], decimals, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// ��������� ���������� ���� ��������� ��������� �� ����� ��������.
    /// ������������ ������� ��������������� ����������.
    /// </summary>
    public void Round()
    {
      Round(0);
    }

    #endregion
  }

  /// <summary>
  /// ������� �������� �������� �� ��������� ������.
  /// ������� ����� ���� �� ������������ � �������� ����� (������������ � ������������).
  /// </summary>
  [Serializable]
  public class DecimalNamedDictionary : TypedStringDictionary<decimal>
  {
    #region ������������

    /// <summary>
    /// �������� ������ ������������.
    /// ������� ������� � ������, �������������� � ��������
    /// </summary>
    public DecimalNamedDictionary()
      : this(false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������
    /// </summary>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public DecimalNamedDictionary(bool ignoreCase)
      : base(ignoreCase)
    {
    }

    /// <summary>
    /// ������� ������� � ������, �������������� � ��������
    /// ��� ������ ������� ������������, ���� ������� ��������, ������� ����� ��������� � �������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    public DecimalNamedDictionary(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������.
    /// ��� ������ ������� ������������, ���� ������� ��������, ������� ����� ��������� � �������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public DecimalNamedDictionary(int capacity, bool ignoreCase)
      : base(capacity, ignoreCase)
    {
    }

    /// <summary>
    /// ������� ������� � ������, �������������� � �������� � ��������� ��� ����������.
    /// </summary>
    /// <param name="dictionary">��������, ������ ������� ��������</param>
    public DecimalNamedDictionary(IDictionary<string, decimal> dictionary)
      : this(dictionary, false)
    {
    }

    /// <summary>
    /// ������� ������� � ��������� ���������������� � ��������, � ��������� ��� ����������.
    /// </summary>
    /// <param name="dictionary">��������, ������ ������� ��������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public DecimalNamedDictionary(IDictionary<string, decimal> dictionary, bool ignoreCase)
      : base(dictionary, ignoreCase)
    {
    }

    #endregion

    #region ������ � ���������

    /// <summary>
    /// ��������� ��� ��������� ��������.
    /// ���� � ������� ��� �������� � ��������� ������, ������������ 0
    /// (��� ������� ����������, ��� ��� ������� � ����������� ����������)
    /// </summary>
    /// <param name="key">����</param>
    /// <returns>��������</returns>
    public new decimal this[string key]
    {
      get
      {
        decimal v;
        if (base.TryGetValue(key, out v))
          return v;
        else
          return 0m;
      }
      set
      {
        base[key] = value;
      }
    }

    #endregion

    #region �������� � ���������

    /// <summary>
    /// �������� � ������� ��������� �������� �� ������ ���������.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="source">������� "���-��������", ������ ������� ����������� ��������.
    /// ���� null, �� ������� �������� �� �����������</param>
    public void Add(IDictionary<string, decimal> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, decimal> Pair in source)
        this[Pair.Key] += Pair.Value;
    }

    /// <summary>
    /// ������� �� ������� ��������� �������� �� ������ ���������.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="source">������� "���-��������", ������ ������� ���������� ��������.
    /// ���� null, �� ������� �������� �� �����������</param>
    public void Substract(IDictionary<string, decimal> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, decimal> Pair in source)
        this[Pair.Key] -= Pair.Value;
    }

    /// <summary>
    /// �������� ���� ���������.
    /// ��� ���������� ��������� ������ ��������� ����� ���������, ������� ������ ����������� ������������
    /// ������������� ����� Add(), ������� ���������� ������������ ���������.
    /// �������� IgnoreCase ����� ��������� ��������������� � ������������ �� ��������� �������� ������ ���������.
    /// </summary>
    /// <param name="a">������ �������� ���������</param>
    /// <param name="b">������ �������� ���������</param>
    /// <returns>����� ���������</returns>
    public static DecimalNamedDictionary operator +(DecimalNamedDictionary a, IDictionary<string, decimal> b)
    {
      DecimalNamedDictionary Res = new DecimalNamedDictionary(a, a.IgnoreCase);
      Res.Add(b);
      return Res;
    }

    /// <summary>
    /// ��������� ����� ��������� �� ������.
    /// ��� ���������� ��������� ������ ��������� ����� ���������, ������� ������ ����������� ������������
    /// ������������� ����� Substract(), ������� ���������� ������������ ���������.
    /// �������� IgnoreCase ����� ��������� ��������������� � ������������ �� ��������� �������� ������ ���������.
    /// </summary>
    /// <param name="a">������ �������� ���������</param>
    /// <param name="b">������ �������� ���������</param>
    /// <returns>����� ���������</returns>
    public static DecimalNamedDictionary operator -(DecimalNamedDictionary a, IDictionary<string, decimal> b)
    {
      DecimalNamedDictionary Res = new DecimalNamedDictionary(a, a.IgnoreCase);
      Res.Substract(b);
      return Res;
    }

    #endregion

    #region ��������� � �������

    /// <summary>
    /// ��������� ���� �������� ��������� �� �������� �����.
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="m">���������</param>
    public void Multiply(decimal m)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] *= m;
    }

    /// <summary>
    /// ������� ���� �������� ��������� �� �������� �����.
    /// ������� ����������� ��� ����������.
    /// ����������� ����� Round() ����� ���������� �������
    /// ������� ��������� ������ ���� �������� ��� ������ (IsReadOnly=false).
    /// </summary>
    /// <param name="d">��������. �� ����� ���� ����� 0</param>
    public void Divide(decimal d)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] /= d;
    }

    #endregion

    #region ����������

    /// <summary>
    /// ��������� ���������� ���� ��������� ��������� �� ��������� ����� ������ ����� �������.
    /// ������������ ������� ��������������� ����������.
    /// </summary>
    /// <param name="decimals">����� ������ ����� �������</param>
    public void Round(int decimals)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] = Math.Round(this[Codes[i]], decimals, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// ��������� ���������� ���� ��������� ��������� �� ����� ��������.
    /// ������������ ������� ��������������� ����������.
    /// </summary>
    public void Round()
    {
      Round(0);
    }

    #endregion
  }

  #endregion
}