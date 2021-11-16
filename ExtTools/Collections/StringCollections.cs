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
  public class SingleScopeStringList : SingleScopeList<string>
  {
    #region ������������

    /// <summary>
    /// ������� ������ ������
    /// </summary>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public SingleScopeStringList(bool ignoreCase)
      : base(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ������ ������
    /// </summary>
    /// <param name="comparer">������ ��� ��������� �����</param>
    public SingleScopeStringList(StringComparer comparer)
      : base(comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    /// <summary>
    /// ������� ������ ������ �������� �������.
    /// ����������� ���� �����������, ���� �������� ����� ��������� � ��������� �������� � ������� ����� �����������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public SingleScopeStringList(int capacity, bool ignoreCase)
      : base(capacity, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ������ ������ �������� �������.
    /// ����������� ���� �����������, ���� �������� ����� ��������� � ��������� �������� � ������� ����� �����������.
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="comparer">������ ��� ��������� �����</param>
    public SingleScopeStringList(int capacity, StringComparer comparer)
      : base(capacity, comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    /// <summary>
    /// ������� ������ � ��������� ��� ��������� ����������.
    /// </summary>
    /// <param name="src">���������, ������ ������� ����� ������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public SingleScopeStringList(ICollection<string> src, bool ignoreCase)
      : base(src, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ������ � ��������� ��� ��������� ����������.
    /// </summary>
    /// <param name="src">���������, ������ ������� ����� ������</param>
    /// <param name="comparer">������ ��� ��������� �����</param>
    public SingleScopeStringList(ICollection<string> src, StringComparer comparer)
      : base(src, comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    /// <summary>
    /// ������� ������ � ��������� ��� ��������� ����������.
    /// </summary>
    /// <param name="src">���������, ������ ������� ����� ������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public SingleScopeStringList(IEnumerable<string> src, bool ignoreCase)
      : base(src, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ������ � ��������� ��� ��������� ����������.
    /// </summary>
    /// <param name="src">���������, ������ ������� ����� ������</param>
    /// <param name="comparer">������ ��� ��������� �����</param>
    public SingleScopeStringList(IEnumerable<string> src, StringComparer comparer)
      : base(src, comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���������� true, ���� ������ �� �������� �������������� � ��������
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    #endregion
  }

  /// <summary>
  /// ���������� �������������� ���������, � ������� ������ �������� ������, � �������� ����� �������� ���.
  /// � ������� �� ������� ��������� Dictionary, ����� ���� �� ������������� � �������� �����
  /// </summary>
  /// <typeparam name="TValue">��� ���������� ��������</typeparam>
  [Serializable]
  public class TypedStringDictionary<TValue> : DictionaryWithReadOnly<string, TValue>, INamedValuesAccess
  {
    #region ������������

    /// <summary>
    /// ������� ������ ���������.
    /// </summary>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public TypedStringDictionary(bool ignoreCase)
      : base(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ������ ���������.
    /// </summary>
    /// <param name="comparer">������ ��� ��������� �����</param>
    public TypedStringDictionary(StringComparer comparer)
      : base(comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    /// <summary>
    /// ������� ������ ��������� �������� �������
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public TypedStringDictionary(int capacity, bool ignoreCase)
      : base(capacity, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ������ ��������� �������� �������
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="comparer">������ ��� ��������� �����</param>
    public TypedStringDictionary(int capacity, StringComparer comparer)
      : base(capacity, comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
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

    /// <summary>
    /// ������� �������� � ��������� �� ����������
    /// </summary>
    /// <param name="dictionary">�������� ���������, ������ ������� �������� ��� ����������</param>
    /// <param name="comparer">������ ��� ��������� �����</param>
    public TypedStringDictionary(IDictionary<string, TValue> dictionary, StringComparer comparer)
      : this(dictionary.Count, comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);

      foreach (KeyValuePair<string, TValue> Pair in dictionary)
        Add(Pair.Key, Pair.Value);
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���������� true, ���� ��������� �� �������� �������������� � ��������
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

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
  public class BidirectionalTypedStringDictionary<TValue> : BidirectionalDictionary<string, TValue>, INamedValuesAccess
  {
    #region ������������

    /// <summary>
    /// ������� ������ ���������.
    /// </summary>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public BidirectionalTypedStringDictionary(bool ignoreCase)
      :base(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal, null)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ������ ���������.
    /// </summary>
    /// <param name="comparer">������ ��� ��������� �����</param>
    public BidirectionalTypedStringDictionary(StringComparer comparer)
      : base(comparer, null)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }
        
    /// <summary>
    /// ������� ������ ��������� �������� �������
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public BidirectionalTypedStringDictionary(int capacity, bool ignoreCase)
      :base(capacity, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal, null)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ������ ��������� �������� �������
    /// </summary>
    /// <param name="capacity">��������� ������� ���������</param>
    /// <param name="comparer">������ ��� ��������� �����</param>
    public BidirectionalTypedStringDictionary(int capacity, StringComparer comparer)
      : base(capacity, comparer, null)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
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

    /// <summary>
    /// ������� �������� � ��������� �� ����������
    /// </summary>
    /// <param name="dictionary">�������� ���������, ������ ������� �������� ��� ����������</param>
    /// <param name="comparer">������ ��� ��������� �����</param>
    public BidirectionalTypedStringDictionary(IDictionary<string, TValue> dictionary, StringComparer comparer)
      : this(dictionary.Count, comparer)
    {
      foreach (KeyValuePair<string, TValue> Pair in dictionary)
        Add(Pair.Key, Pair.Value);
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
  public sealed class StringArrayIndexer : ArrayIndexer<string>
  {
    #region ������������

    /// <summary>
    /// ������� ���������� ��� �������.
    /// ��� ������ ������������� ��������� ������� ��������.
    /// </summary>
    /// <param name="source">������������� ������</param>
    public StringArrayIndexer(string[] source)
      : this(source, false)
    {
    }

    /// <summary>
    /// ������� ���������� ��� �������.
    /// </summary>
    /// <param name="source">������������� ������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public StringArrayIndexer(string[] source, bool ignoreCase)
      :base(source, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ���������� ��� �������.
    /// </summary>
    /// <param name="source">������������� ������</param>
    /// <param name="comparer">������ ��� ��������� �����</param>
    public StringArrayIndexer(string[] source, StringComparer comparer)
      : base(source, comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    /// <summary>
    /// ������� ���������� ��� ��������� �����.
    /// ��� ������ ������������� ��������� ������� ��������.
    /// </summary>
    /// <param name="source">������������� ���������</param>
    public StringArrayIndexer(ICollection<string> source)
      : this(source, false)
    {
    }

    /// <summary>
    /// ������� ���������� ��� ��������� �����.
    /// </summary>
    /// <param name="source">������������� ���������</param>
    /// <param name="ignoreCase">����� �� ������������ ������� �����</param>
    public StringArrayIndexer(ICollection<string> source, bool ignoreCase)
      :base(source, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// ������� ���������� ��� ��������� �����.
    /// </summary>
    /// <param name="source">������������� ���������</param>
    /// <param name="comparer">������ ��� ��������� �����</param>
    public StringArrayIndexer(ICollection<string> source, StringComparer comparer)
      : base(source, comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���� true, �� ������� ����� �� �����������.
    /// �������� �������� � ������������
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    #endregion

    #region ����������� ������

    /// <summary>
    /// ������ ������ - ����������
    /// </summary>
    public static readonly StringArrayIndexer Empty = new StringArrayIndexer(DataTools.EmptyStrings, false);

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
    // ��������� ����������� ����������, � �� ����� ArrayIndexer, �.�. StringComparer �� ��������� IEqualityComparer ��� Char

    #region ������������

    /// <summary>
    /// ������� ���������� ��� �������.
    /// � ������� �� ArrayIndexer of Char, ����������� ������� � <paramref name="source"/> ������������� ��������, ������� �������������.
    /// ��������� ����� �������������� � �������� ��������.
    /// </summary>
    /// <param name="source">������ ��������</param>
    public CharArrayIndexer(char[] source)
      : this(source, false)
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
      : this(source, false)
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
