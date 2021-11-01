using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
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

namespace FreeLibSet.Win32
{
  /// <summary>
  /// ���� ������ ������������ ��� �������� ������� ������� RegistryTree.Enumerate()
  /// </summary>
  public sealed class EnumRegistryEntry
  {
    #region �����������

    internal EnumRegistryEntry(RegistryKey key, int enumKeyLevel, string valueName)
    {
      _Key = key;
      _EnumKeyLevel = enumKeyLevel;
      _ValueName = valueName;
      //System.Diagnostics.Trace.WriteLine("Init: " + this.ToString());
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� ������ �������
    /// </summary>
    public RegistryKey Key { get { return _Key; } }
    private RegistryKey _Key;

    /// <summary>
    /// ������� ������� Key ������������ �������, � �������� ������ ������������.
    /// ���� ������ ������������� ���������� ������, �������� ���������� 0, ���� ���� �� ���
    /// �������� ��������, �� 1, � �.�.
    /// </summary>
    public int EnumKeyLevel { get { return _EnumKeyLevel; } }
    private int _EnumKeyLevel;

    /// <summary>
    /// ��� �������� ��������.
    /// ������� ������������� ���������z ��� �������, ��� ���� �������� ���������� true.
    /// �����, ���� ������������ �������� ��������, ��� �������� ���������� ��� ���������� ��������.
    /// </summary>
    public string ValueName { get { return _ValueName; } }
    private string _ValueName;

    /// <summary>
    /// ���������� "Key.Name" ��� "Key.Name : Value", ���� ������ ������������ ��������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      //string s = Key.Name;
      if (String.IsNullOrEmpty(ValueName))
        return Key.Name;
      else
        return Key.Name + " : " + ValueName;
    }

    #endregion
  }

  /// <summary>
  /// �������� ��������� �������� RegistryKey.
  /// � ���������������� ���� ����������� ����� RegistryCfg ��� ������� � �������.
  /// </summary>
  public class RegistryTree : DisposableObject, IReadOnlyObject
  {
    #region ����������� � Dispose

    /// <summary>
    /// ������� ������ ������.
    /// ������ ����� �������� ��� ������.
    /// </summary>
    public RegistryTree()
      : this(false)
    {
    }

    /// <summary>
    /// ������� ������ ������.
    /// </summary>
    /// <param name="isReadOnly">true, ���� ������ ����� �������� ������ ��� ������</param>
    public RegistryTree(bool isReadOnly)
    {
      _Items = new Dictionary<string, RegistryKey>();
      _IsReadOnly = isReadOnly;
    }

    /// <summary>
    /// ��������� ��� �������� ������
    /// </summary>
    /// <param name="disposing">����� �� ������ Dispose()?</param>
    protected override void Dispose(bool disposing)
    {
      if (_Items != null)
      {
        Close();
        _Items = null;
      }
      base.Dispose(disposing);
    }

    #endregion

    #region ������ � �������� RegistryKey

    /// <summary>
    /// ���������� �������� ������ RegistryKey ��� ������� � ����� �������.
    /// ����� ���������� ���������� ��� ������������ ����� �������.
    /// ������� ������������ �� ���������� ������ � ����������� ��� ������ Close() ��� Dispose().
    /// ���������� null, ���� ����� ������� �� ����������.
    /// </summary>
    /// <param name="keyName">���� � ����� �������</param>
    /// <returns>������ ������� � ���� ������� ��� null</returns>
    public RegistryKey this[string keyName]
    {
      get
      {
        CheckNotDisposed();

        RegistryKey Item;
        if (_Items.TryGetValue(keyName, out Item))
          return Item; // ���� ��� ��� ������

        if (String.IsNullOrEmpty(keyName))
          throw new ArgumentNullException("keyName");

        int p = keyName.LastIndexOf('\\');
        if (p < 0)
        {
          RegistryKey Root = GetRootKey(keyName);
          if (Root == null)
            throw new ArgumentException("����������� ��������� ���� ������� \"" + keyName + "\"", "keyName");
          return Root;
        }

        // �������� ��������� ����
        string ParentKeyName = keyName.Substring(0, p);
        RegistryKey ParentKey = this[ParentKeyName]; // ����������� �����
        if (ParentKey == null)
          return null;

        string SubName = keyName.Substring(p + 1);
        Item = ParentKey.OpenSubKey(SubName, !IsReadOnly);

        if (Item == null && (!IsReadOnly))
          Item = ParentKey.CreateSubKey(SubName);

        // ��������� ���� � ���������
        _Items.Add(keyName, Item);
        return Item;
      }
    }

    /// <summary>
    /// ���������� �������� ���� �� �����.
    /// </summary>
    /// <param name="keyName">���, ��������, "HKEY_CLASSES_ROOT"</param>
    /// <returns>����������� �������� �� ������ Registry</returns>
    public static RegistryKey GetRootKey(string keyName)
    {
      // ���������� �������� ����
      switch (keyName)
      {
        case "HKEY_CLASSES_ROOT": return Registry.ClassesRoot;
        case "HKEY_CURRENT_USER": return Registry.CurrentUser;
        case "HKEY_LOCAL_MACHINE": return Registry.LocalMachine;
        case "HKEY_USERS": return Registry.Users;
        case "HKEY_CURRENT_CONFIG": return Registry.CurrentConfig;
        default: return null;
      }
    }

    /// <summary>
    /// ������ �������� ����� �������. �������� ���� �� ��������.
    /// ����� ���� �������� null ��� �������������� ����� � ������ IsReadOnly
    /// </summary>
    private Dictionary<string, RegistryKey> _Items;

    #endregion

    #region ��������������� ������ � ��������

    /// <summary>
    /// ����� "������ ��� ������". �������� � ������������.
    /// ����� ����������� ��������� � ��������������� ���� �������, ��� IsReadOnly=true ������������ null,
    /// � ��� false - ��������� ����� ����
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private readonly bool _IsReadOnly;

    /// <summary>
    /// ���������� ����������, ���� IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new InvalidOperationException("��������� ����� ������� ������� ������ ��� ������");
    }

    /// <summary>
    /// ��������� ��� �������� ������, ������� RegistryKey.Close()
    /// </summary>
    public void Close()
    {
      if (IsDisposed)
        return;

      foreach (RegistryKey Item in _Items.Values)
      {
        try
        {
          if (Item != null)
            Item.Close();
        }
        catch { }
      }

      _Items.Clear();
    }

    /// <summary>
    /// ���������� true, ���� ���������� ��������� ���� � ����� �������
    /// </summary>
    /// <param name="keyName">���� � ���� �������</param>
    /// <returns>������������� �����</returns>
    public bool Exists(string keyName)
    {
      CheckNotDisposed();

      if (String.IsNullOrEmpty(keyName))
        return false;

      if (_Items.ContainsKey(keyName))
        return true;

      int p = keyName.LastIndexOf('\\');
      if (p < 0)
        return GetRootKey(keyName) != null;

      string ParentKeyName = keyName.Substring(0, p);

      if (!Exists(ParentKeyName)) // ����������� �����
        return false;

      // ����� ������������ ����
      RegistryKey ParentKey = this[ParentKeyName];
      if (ParentKey == null)
        return false; // 21.02.2020
      string SubName = keyName.Substring(p + 1);

      RegistryKey SubKey = ParentKey.OpenSubKey(SubName, !IsReadOnly);
      if (SubKey == null)
        return false;

      _Items.Add(keyName, SubKey);
      return true;
    }

    #endregion

    #region ������ ������ ��������

    /// <summary>
    /// �������� ��������.
    /// ���� ���� ���, ������������ null.
    /// </summary>
    /// <param name="keyName">���� � ���� �������</param>
    /// <param name="valueName">��� ��������. ��� ��������� �������� �� ��������� ������� ������ �����</param>
    /// <returns>��������</returns>
    public object GetValue(string keyName, string valueName)
    {
      RegistryKey Key = this[keyName];
      if (Key == null)
        return null;
      else
        return Key.GetValue(valueName);
    }

    /// <summary>
    /// �������� ��������� ��������.
    /// ���� ���� ���, ������������ ������ ������.
    /// </summary>
    /// <param name="keyName">���� � ���� �������</param>
    /// <param name="valueName">��� ��������. ��� ��������� �������� �� ��������� ������� ������ �����</param>
    /// <returns>��������</returns>
    public string GetString(string keyName, string valueName)
    {
      return DataTools.GetString(GetValue(keyName, valueName));
    }

    /// <summary>
    /// �������� �������� ��������.
    /// ���� ���� ��� ��� �������� �������� ������ �������, ������������ 0.
    /// ���� �������� �������� ������ ������������� � �����, ������������ ����������
    /// </summary>
    /// <param name="keyName">���� � ���� �������</param>
    /// <param name="valueName">��� ��������. ��� ��������� �������� �� ��������� ������� ������ �����</param>
    /// <returns>��������</returns>
    public int GetInt(string keyName, string valueName)
    {
      string s = GetString(keyName, valueName);
      if (s.Length == 0)
        return 0;
      return StdConvert.ToInt32(s);
    }

    /// <summary>
    /// �������� �������� ��������.
    /// ���� ���� ��� ��� �������� �������� ������ �������, ������������ 0.
    /// ���� �������� �������� ������ ������������� � �����, ������������ ����������
    /// </summary>
    /// <param name="keyName">���� � ���� �������</param>
    /// <param name="valueName">��� ��������. ��� ��������� �������� �� ��������� ������� ������ �����</param>
    /// <returns>��������</returns>
    public long GetInt64(string keyName, string valueName)
    {
      string s = GetString(keyName, valueName);
      if (s.Length == 0)
        return 0;
      return StdConvert.ToInt64(s);
    }

    /// <summary>
    /// �������� �������� ��������.
    /// ���� ���� ��� ��� �������� �������� ������ �������, ������������ 0.
    /// ���� �������� �������� ������ ������������� � �����, ������������ ����������
    /// </summary>
    /// <param name="keyName">���� � ���� �������</param>
    /// <param name="valueName">��� ��������. ��� ��������� �������� �� ��������� ������� ������ �����</param>
    /// <returns>��������</returns>
    public float GetSingle(string keyName, string valueName)
    {
      string s = GetString(keyName, valueName);
      if (s.Length == 0)
        return 0f;
      return StdConvert.ToSingle(s);
    }

    /// <summary>
    /// �������� �������� ��������.
    /// ���� ���� ��� ��� �������� �������� ������ �������, ������������ 0.
    /// ���� �������� �������� ������ ������������� � �����, ������������ ����������
    /// </summary>
    /// <param name="keyName">���� � ���� �������</param>
    /// <param name="valueName">��� ��������. ��� ��������� �������� �� ��������� ������� ������ �����</param>
    /// <returns>��������</returns>
    public double GetDouble(string keyName, string valueName)
    {
      string s = GetString(keyName, valueName);
      if (s.Length == 0)
        return 0.0;
      return StdConvert.ToDouble(s);
    }

    /// <summary>
    /// �������� �������� ��������.
    /// ���� ���� ��� ��� �������� �������� ������ �������, ������������ 0.
    /// ���� �������� �������� ������ ������������� � �����, ������������ ����������
    /// </summary>
    /// <param name="keyName">���� � ���� �������</param>
    /// <param name="valueName">��� ��������. ��� ��������� �������� �� ��������� ������� ������ �����</param>
    /// <returns>��������</returns>
    public decimal GetDecimal(string keyName, string valueName)
    {
      string s = GetString(keyName, valueName);
      if (s.Length == 0)
        return 0m;
      return StdConvert.ToDecimal(s);
    }

    /// <summary>
    /// �������� ���������� ��������.
    /// ���� ���� ��� ��� �������� �������� ������ �������, ������������ false.
    /// ���� �������� �������� ������ ������������� � �����, ������������ ����������
    /// </summary>
    /// <param name="keyName">���� � ���� �������</param>
    /// <param name="valueName">��� ��������. ��� ��������� �������� �� ��������� ������� ������ �����</param>
    /// <returns>��������</returns>
    public bool GetBool(string keyName, string valueName)
    {
      return GetInt(keyName, valueName) != 0;
    }

    #endregion

    #region �������������

    /// <summary>
    /// ���� ������� ��� ������������ ������.
    /// ����� ������� � ���� ������ ��� ����� ��� �������� ��������, ��� � ����.
    /// ����� ����� ������� ��������, ��� ��� ��������� ���������� � Enumerate() ��������� �������
    /// </summary>
    private class EnumInfo
    {
      #region �����������

      public EnumInfo(RegistryKey currKey, bool enumerateValues)
      {
#if DEBUG
        if (currKey == null)
          throw new ArgumentNullException("currKey");
#endif
        _CurrKey = currKey;

        _SubKeyNames = currKey.GetSubKeyNames();
        SubKeyIndex = -1;

        if (enumerateValues)
          _ValueNames = currKey.GetValueNames();
        ValueIndex = -1;

        // System.Diagnostics.Trace.WriteLine("EnumInfo: " + CurrKey.Name + ", SubKeyNames: " + String.Join(", ", SubKeyNames) +
        //   ", ValueNames: " + String.Join(", ", ValueNames));
      }

      #endregion

      #region ����

      /// <summary>
      /// ������ �������, ������� ������ ������������
      /// </summary>
      public RegistryKey CurrKey { get { return _CurrKey; } }
      private RegistryKey _CurrKey;

      /// <summary>
      /// ������ ���� �������� ��������
      /// </summary>
      public string[] SubKeyNames { get { return _SubKeyNames; } }
      private string[] _SubKeyNames;

      /// <summary>
      /// ������ �������� ��������� ����
      /// </summary>
      public int SubKeyIndex;

      public string[] ValueNames { get { return _ValueNames; } }
      private string[] _ValueNames;

      public int ValueIndex;

      public override string ToString()
      {
        return CurrKey.Name;
      }

      #endregion
    }

    /// <summary>
    /// ����������� ������������ �� �������.
    /// ������������ ���������� � ��������� ���� �������, ����� ������������� ��� ��������
    /// (����� "�����������" �������� �� ���������), ����� ���������� ������������� �������� �������.
    /// ���� ������� <paramref name="keyName"/> ��� � �������, ������������� �� ���� �� ����������.
    /// ��� ������������ ������������ ������ EnumRegistryEntry.
    /// ��� ������������ ��������� �������� ������� Windows � ������������ ����� �� ���� ���� �� ������.
    /// ����������� RegistryTree � ������ IsReadOnly=true.
    /// ��� ������������ ������������ ����� ���� �������� ������������ ����������� ����� StaticEnumerate(),
    /// ������� �� ������� ������ �������� � �������� ������� RegistryTree.
    /// </summary>
    /// <param name="keyName">���� � ������� �������, ������� ����� �����������</param>
    /// <returns>������ ��� ������������� � ����� foreach</returns>
    public IEnumerable<EnumRegistryEntry> Enumerate(string keyName)
    {
      return Enumerate(keyName, true);
    }

    /// <summary>
    /// ����������� ������������ �� �������.
    /// ������������ ���������� � ��������� ���� �������, �����, ���� <paramref name="enumerateValues"/>=true, ������������� ��� ��������
    /// (����� "�����������" �������� �� ���������), ����� ���������� ������������� �������� �������.
    /// ���� ������� <paramref name="keyName"/> ��� � �������, ������������� �� ���� �� ����������.
    /// ��� ������������ ������������ ������ EnumRegistryEntry.
    /// ��� ������������ ��������� �������� ������� Windows � ������������ ����� �� ���� ���� �� ������.
    /// ����������� RegistryTree � ������ IsReadOnly=true.
    /// ��� ������������ ������������ ����� ���� �������� ������������ ����������� ����� StaticEnumerate(),
    /// ������� �� ������� ������ �������� � �������� ������� RegistryTree.
    /// </summary>
    /// <param name="keyName">���� � ������� �������, ������� ����� �����������</param>
    /// <param name="enumerateValues">����� �� ����������� ����� ��� �������� � �������� (true),
    /// ��� ����������� ������ ������� (false)</param>
    /// <returns>������ ��� ������������� � ����� foreach</returns>
    public IEnumerable<EnumRegistryEntry> Enumerate(string keyName, bool enumerateValues)
    {
      RegistryKey StartKey = this[keyName];
      if (StartKey == null)
        yield break;

      Stack<EnumInfo> Stack = new Stack<EnumInfo>();
      Stack.Push(new EnumInfo(StartKey, enumerateValues));
      while (Stack.Count > 0)
      {
        EnumInfo CurrInfo = Stack.Peek();
        if (CurrInfo.SubKeyIndex < 0)
        {
          // ������ ���� �����

          // ��� ����� �������
          yield return new EnumRegistryEntry(CurrInfo.CurrKey, Stack.Count - 1, String.Empty);

          // ��� ��������
          if (enumerateValues)
          {
            while (true)
            {
              CurrInfo.ValueIndex++;
              if (CurrInfo.ValueIndex >= CurrInfo.ValueNames.Length)
                break;
              if (String.IsNullOrEmpty(CurrInfo.ValueNames[CurrInfo.ValueIndex]))
                continue; // �������� �� ��������� �� �����������
              yield return new EnumRegistryEntry(CurrInfo.CurrKey, Stack.Count - 1, CurrInfo.ValueNames[CurrInfo.ValueIndex]);

            }
          }
        }

        CurrInfo.SubKeyIndex++;
        if (CurrInfo.SubKeyIndex < CurrInfo.SubKeyNames.Length)
        {
          string nm = CurrInfo.SubKeyNames[CurrInfo.SubKeyIndex];
          string ChildKeyName = CurrInfo.CurrKey.Name + "\\" + nm;
          RegistryKey ChildKey = this[ChildKeyName];
          if (ChildKey != null) // ����� ��� ���� �������?
            Stack.Push(new EnumInfo(ChildKey, enumerateValues));
          continue;
        }

        // ������ ��� �������� ��������
        Stack.Pop();
      }
    }

    #endregion

    #region ����������� ������ �������������

    private class EnumeratorProxy : DisposableObject, IEnumerator<EnumRegistryEntry>
    {
      #region ����������� � Dispose

      public EnumeratorProxy(string keyName, bool enumerateValues)
      {
        _Tree = new RegistryTree(true);
        _En2 = _Tree.Enumerate(keyName, enumerateValues).GetEnumerator();
      }

      private RegistryTree _Tree;
      private IEnumerator<EnumRegistryEntry> _En2;

      protected override void Dispose(bool disposing)
      {
        if (disposing && _Tree != null)
        {
          _Tree.Dispose();
          _En2.Dispose();
        }
        _Tree = null;
        _En2 = null;

        base.Dispose(disposing);
      }

      #endregion

      #region IEnumerator<EnumRegistryEntry> Members

      public EnumRegistryEntry Current { get { return _En2.Current; } }

      #endregion

      #region IEnumerator Members

      object System.Collections.IEnumerator.Current { get { return _En2.Current; } }

      public bool MoveNext()
      {
#if DEBUG
        if (_En2.MoveNext())
        {
          if (Current == null)
            throw new BugException("������� ������� ����� null");
          return true;
        }
        else
          return false;
#else
        return _En2.MoveNext();
#endif
      }

      public void Reset()
      {
        _En2.Reset();
      }

      #endregion
    }

    // ���������� ���������� GetEnumerator(), � ��� foreach �� ��������
#if XXX
    /// <summary>
    /// ����������� ������������ �� �������.
    /// ������������ ���������� � ��������� ���� �������, ����� ������������� ��� ��������
    /// (����� "�����������" �������� �� ���������), ����� ���������� ������������� �������� �������.
    /// ���� ������� <paramref name="KeyName"/> ��� � �������, ������������� �� ���� �� ����������.
    /// ��� ������������ ������������ ������ EnumRegistryEntry.
    /// ����������� ������ ������� ��������� ������ RegistryTree.
    /// </summary>
    /// <param name="KeyName">���� � ������� �������, ������� ����� �����������</param>
    /// <returns>������ ��� ������������� � ����� foreach</returns>
    public static IEnumerator<EnumRegistryEntry> GetEnumerator(string KeyName)
    {
      return GetEnumerator(KeyName, true);
    }

    /// <summary>
    /// ����������� ������������ �� �������.
    /// ������������ ���������� � ��������� ���� �������, �����, ���� <paramref name="EnumerateValues"/>=true, ������������� ��� ��������
    /// (����� "�����������" �������� �� ���������), ����� ���������� ������������� �������� �������.
    /// ���� ������� <paramref name="KeyName"/> ��� � �������, ������������� �� ���� �� ����������.
    /// ��� ������������ ������������ ������ EnumRegistryEntry.
    /// ����������� ������ ������� ��������� ������ RegistryTree.
    /// </summary>
    /// <param name="KeyName">���� � ������� �������, ������� ����� �����������</param>
    /// <param name="EnumerateValues">����� �� ����������� ����� ��� �������� � �������� (true),
    /// ��� ����������� ������ ������� (false)</param>
    /// <returns>������ ��� ������������� � ����� foreach</returns>
    public static IEnumerator<EnumRegistryEntry> GetEnumerator(string KeyName, bool EnumerateValues)
    {
      return new EnumeratorProxy(KeyName, EnumerateValues);
    }

#endif


    private class EnumarableProxy : IEnumerable<EnumRegistryEntry>
    {
      #region ����

      public string KeyName;
      public bool EnumerateValues;

      #endregion

      #region IEnumerable<EnumRegistryEntry> Members

      public IEnumerator<EnumRegistryEntry> GetEnumerator()
      {
        return new EnumeratorProxy(KeyName, EnumerateValues);
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return new EnumeratorProxy(KeyName, EnumerateValues);
      }

      #endregion
    }

    /// <summary>
    /// ����������� ������������ �� �������.
    /// ������������ ���������� � ��������� ���� �������, ����� ������������� ��� ��������
    /// (����� "�����������" �������� �� ���������), ����� ���������� ������������� �������� �������.
    /// ���� ������� <paramref name="keyName"/> ��� � �������, ������������� �� ���� �� ����������.
    /// ��� ������������ ������������ ������ EnumRegistryEntry.
    /// ��� ������������ ��������� ���������� ������ RegistryTree. ���� ��������� ������������ ������������
    /// �������, �� ������� ������������ ������������� ������ ������ Enumerate().
    /// </summary>
    /// <param name="keyName">���� � ������� �������, ������� ����� �����������</param>
    /// <returns>������ ��� ������������� � ����� foreach</returns>
    public static IEnumerable<EnumRegistryEntry> StaticEnumerate(string keyName)
    {
      return StaticEnumerate(keyName, true);
    }

    /// <summary>
    /// ����������� ������������ �� �������.
    /// ������������ ���������� � ��������� ���� �������, �����, ���� <paramref name="enumerateValues"/>=true, ������������� ��� ��������
    /// (����� "�����������" �������� �� ���������), ����� ���������� ������������� �������� �������.
    /// ���� ������� <paramref name="keyName"/> ��� � �������, ������������� �� ���� �� ����������.
    /// ��� ������������ ������������ ������ EnumRegistryEntry.
    /// ��� ������������ ��������� ���������� ������ RegistryTree. ���� ��������� ������������ ������������
    /// �������, �� ������� ������������ ������������� ������ ������ Enumerate().
    /// </summary>
    /// <param name="keyName">���� � ������� �������, ������� ����� �����������</param>
    /// <param name="enumerateValues">����� �� ����������� ����� ��� �������� � �������� (true),
    /// ��� ����������� ������ ������� (false)</param>
    /// <returns>������ ��� ������������� � ����� foreach</returns>
    public static IEnumerable<EnumRegistryEntry> StaticEnumerate(string keyName, bool enumerateValues)
    {
      EnumarableProxy Proxy = new EnumarableProxy();
      Proxy.KeyName = keyName;
      Proxy.EnumerateValues = enumerateValues;
      return Proxy;
    }

    #endregion
  }
}