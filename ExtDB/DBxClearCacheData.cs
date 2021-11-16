using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using FreeLibSet.Core;
using FreeLibSet.Data;
using FreeLibSet.Collections;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace FreeLibSet.Data
{
  /// <summary>
  /// ��������� ��������������� ������ ����� ������� ������, ������� ������ ���� ������� � ���������� ��������� ��� �������� �����
  /// ���� ����� �� �������� ���������������� � ������ ������ (���� IsReadOnly=false).
  /// </summary>
  [Serializable]
  public sealed class DBxClearCacheData : IReadOnlyObject, IEnumerable<KeyValuePair<string, IdList>>
  {
    #region �����������

    /// <summary>
    /// ������� ������ ������ ��� �������
    /// </summary>
    public DBxClearCacheData()
    {
      _AreAllTables = false;
      _Items = new Dictionary<string, IdList>();
    }

    #endregion

    #region ���������� ������
                                                         
    /// <summary>
    /// ��������� �������������� ������� ��� ������� ����.
    /// ������� �������������� ������������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="ids">������ ���������������</param>
    public void Add(string tableName, Int32[] ids)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");

      if (AreAllTables)
        return;

      if (ids.Length == 0)
        return;

      IdList lst;
      if (!_Items.TryGetValue(tableName, out lst))
      {
        lst = new IdList();
        _Items.Add(tableName, lst);
      }
      else if (Object.ReferenceEquals(lst, null))
        return; // ��� ������� �������� �� ��������

      for (int i = 0; i < ids.Length; i++)
        DoAdd(lst, ids[i]);
    }

    private void DoAdd(IdList lst, Int32 Id)
    {
      if (Id <= 0) // 25.06.2021 - ��������� �������������� ������������
        return;

      Int32 FirstId = DBxTableCache.GetFirstPageId(Id);
      lst.Add(FirstId);
    }

    /// <summary>
    /// ��������� ������������� ������� ��� ������� ����.
    /// ������� �������������� ������������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="id">�������������</param>
    public void Add(string tableName, Int32 id)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");

      if (id == 0)
        return;

      if (AreAllTables)
        return;

      IdList lst;
      if (!_Items.TryGetValue(tableName, out lst))
      {
        lst = new IdList();
        _Items.Add(tableName, lst);
      }
      else if (Object.ReferenceEquals(lst, null))
        return; // ��� ������� �������� �� ��������

      DoAdd(lst, id);
    }

    /// <summary>
    /// �������� ������� ������� �� ����������
    /// </summary>
    /// <param name="tableName"></param>
    public void Add(string tableName)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");

      if (AreAllTables)
        return;

      IdList lst;
      if (!_Items.TryGetValue(tableName, out lst))
        _Items.Add(tableName, null);
      else
        _Items[tableName] = null;
    }

    /// <summary>
    /// ����� ����� ������ ��������, ��� ��� ������������ ������ ������ ���� �������
    /// </summary>
    public void AddAllTables()
    {
      CheckNotReadOnly();

      if (_AreAllTables)
        return;

      _AreAllTables = true;
      _Items.Clear(); // ��������� ����������������� ������ �� �����
    }

    #endregion

    #region ��������

    private Dictionary<string, IdList> _Items;

    /// <summary>
    /// ���������� true, ��� ��� ����� AddAllTables()
    /// </summary>
    public bool AreAllTables { get { return _AreAllTables; } }
    private bool _AreAllTables;

    /// <summary>
    /// ���������� true, ���� � ������ �� ���� ��������� ���������
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return (!_AreAllTables) && _Items.Count == 0;
      }
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// ���������� true, ���� ����� ��������� � ������ ���������.
    /// � ���� ������ ������ ������� Add() �� �����������
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// �������� ������ IsReadOnly
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// ��������� �������� IsReadOnly=true
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region IEnumerable<KeyValuePair<string,IdList>> Members

    /// <summary>
    /// ���������� �������������.
    /// �� ������������� ��� ������������� � ���������������� ����.
    /// </summary>
    /// <returns>�������������</returns>
    public Dictionary<string, IdList>.Enumerator GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator<KeyValuePair<string, IdList>> IEnumerable<KeyValuePair<string, IdList>>.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    #endregion

    #region ��������������� ������ � ��������

    /// <summary>
    /// ���������� true, ���� ���� ������ �� ������ ��� ���� �������.
    /// ����� ���������� true, ���� ��������� ������ ����� 
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <returns></returns>
    public bool ContainsTable(string tableName)
    {
      if (AreAllTables)
        return true;
      else
        return _Items.ContainsKey(tableName);
    }

    /// <summary>
    /// ���������� ������ ��������������� ������ ����� ������� ��� ������ �������.
    /// ���������� null, ���� ��������� ������ ����� �������.
    /// ���������� ������ ������, ���� ������� ��� � ������, ��� ��� ������� ���� ������ �������� ����� ����������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <returns>C����� ��������������� ��� null</returns>
    public IdList this[string tableName]
    {
      get
      {
        IdList Res;
        if (AreAllTables)
          Res = null;
        else if (!_Items.TryGetValue(tableName, out Res))
          Res = IdList.Empty;
        return Res;
      }
    }

    /// <summary>
    /// ���������� true, ���� ��������� ������������� ������ ������� ���� � ������ �� ����������.
    /// �����������, ��� � ������ �������� ������ �������������� ������ �������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="id">�������������</param>
    /// <returns>������� ��������������</returns>
    public bool this[string tableName, Int32 id]
    {
      get
      {
        if (id == 0)
          return false;
        IdList lst = this[tableName];
        if (Object.ReferenceEquals(lst, null))
          return true;
        Int32 FirstId = DBxTableCache.GetFirstPageId(id);
        return lst.Contains(FirstId);
      }
    }

    #endregion

    #region ��������� �������������

    /// <summary>
    /// ��� �������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      if (IsEmpty)
        return "Empty";
      if (AreAllTables)
        return "All tables";

      StringBuilder sb = new StringBuilder();
      foreach (KeyValuePair<string, IdList> Pair in _Items)
      {
        if (sb.Length > 0)
          sb.Append(", ");
        sb.Append(Pair.Key);
        sb.Append(": ");
        if (Object.ReferenceEquals(Pair.Value, null))
          sb.Append("all rows");
        else
        {
          Int32[] FirstIds = Pair.Value.ToArray();
          Array.Sort<Int32>(FirstIds);
          // ���������� ������������� �����
          // ����!
          for (int i = 0; i < FirstIds.Length; i++)
          {
            if (i > 0)
              sb.Append(",");
            Int32 LastId = FirstIds[i] + 99;
            sb.Append(FirstIds[i]);
            sb.Append("-");
            sb.Append(LastId);
          }
        }
      }

      return sb.ToString();
    }

    #endregion

    #region ����������� ���������

    /// <summary>
    /// ����������� ��������� �������, ��������� ������� ������� ���� ������
    /// </summary>
    public static readonly DBxClearCacheData AllTables = CreateAllTables();

    private static DBxClearCacheData CreateAllTables()
    {
      DBxClearCacheData Data = new DBxClearCacheData();
      Data.AddAllTables();
      Data.SetReadOnly();
      return Data;
    }

    #endregion
  }

  /// <summary>
  /// ��������� ������ �� ������� ����.
  /// ���� ����� �������� ����������������
  /// </summary>
  public sealed class DBxClearCacheHolder
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    public DBxClearCacheHolder()
    {
      _Current = new DBxClearCacheData();
      SyncRoot = new object();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� ������������� ������ ���������
    /// </summary>
    private DBxClearCacheData _Current;

    /// <summary>
    /// ������ �������������
    /// </summary>
    private object SyncRoot;

    #endregion

    #region ������ ���������

    /// <summary>
    /// �������� ��������� �������������� ������� �� ����������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="ids">������ ���������������</param>
    public void Add(string tableName, Int32[] ids)
    {
      lock (SyncRoot)
      {
        _Current.Add(tableName, ids);
      }
    }

    /// <summary>
    /// �������� ��������� ������������� ������� �� ����������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="id">�������������</param>
    public void Add(string tableName, Int32 id)
    {
      lock (SyncRoot)
      {
        _Current.Add(tableName, id);
      }
    }

    /// <summary>
    /// �������� ��������� ������� ������� �� ����������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    public void Add(string tableName)
    {
      lock (SyncRoot)
      {
        _Current.Add(tableName);
      }
    }

    /// <summary>
    /// �������� ��� ������� �� ����������
    /// </summary>
    public void AddAllTables()
    {
      lock (SyncRoot)
      {
        _Current.AddAllTables();
      }
    }

    #endregion

    #region ������������ �� ����� �����

    /// <summary>
    /// ������������� �� ����� ������ ���������.
    /// ���������� ���������� ������.
    /// ����� �� ��������� ������������ � ���������� null, ���� � ������� ������ �� ���������������� ���������
    /// </summary>
    /// <returns>���������� ������</returns>
    public DBxClearCacheData Swap()
    {
      DBxClearCacheData Prev;
      lock (SyncRoot)
      {
        if (_Current.IsEmpty)
          Prev = null;
        else
        {
          Prev = _Current;
          _Current = new DBxClearCacheData();
          Prev.SetReadOnly();
        }
      }
      return Prev;
    }

    #endregion
  }

  /// <summary>
  /// ������ ��������� ��������� DBxClearCacheData. ����� �������� ������ DBxClearCacheHolder.
  /// ����� �������� ����������������
  /// </summary>
  public sealed class DBxClearCacheBuffer
  {
    #region �����������

    /// <summary>
    /// ������� ��������� �����, ���������� <paramref name="bufferSize"/> ���������.
    /// </summary>
    /// <param name="bufferSize">���������� ��������� � ������</param>
    public DBxClearCacheBuffer(int bufferSize)
    {
      if (bufferSize < 2 || bufferSize > 1000)
        throw new ArgumentOutOfRangeException();

      _Holder = new DBxClearCacheHolder();
      _Items = new RingBuffer<BufferItem>(bufferSize);
      _DisplayName = "DBxClearCacheBuffer";
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� �������� ����������.
    /// ��� ����������� ������� ������� ������ ������ ���������� ��� ������ Add()
    /// </summary>
    public DBxClearCacheHolder Holder { get { return _Holder; } }
    private DBxClearCacheHolder _Holder;

    /// <summary>
    /// ������� ���������� ������
    /// </summary>
    private struct BufferItem
    {
      #region ����

      /// <summary>
      /// ������ ����� ����������
      /// </summary>
      public int Version;

      /// <summary>
      /// ������ �� ����������
      /// </summary>
      public DBxClearCacheData Data;

      #endregion
    }

    /// <summary>
    /// ��������� ����� ��� �������� �������.
    /// ���� ������ ������������ ����� ��� SycnRoot
    /// </summary>
    private RingBuffer<BufferItem> _Items;

    #endregion

    #region ������������ ������

    /// <summary>
    /// ��������� ������ ������ � ��������� ������
    /// </summary>
    public int LastVersion { get { return _LastVersion; } }
    /// <summary>
    /// ����� ���� �� ��������� ��������� ������ � �� ������ Items, �� ��� - �������.
    /// ���������� �����, � �����, �� �������.
    /// ��� �����, ��� ��� �������� ������������ ������ � ���������� �����.
    /// </summary>
    private int _LastVersion;

    /// <summary>
    /// ����������� �� ����� �����.
    /// ���������� ������� ���������� � ��������� �����.
    /// ���� � ������� ������ �� ���������������� ���������, ������� �������� �� �����������.
    /// ���� ����� ���������� �� ������� ������� �� �������, ��������, 1 ��� � ������
    /// </summary>
    /// <returns>True, ���� � ������� ����������� ������ ������ Swap() ���� ��������� � ������������ ���� ���������.
    /// ���������� false, ���� ����� �� �������� ������� ��������</returns>
    public bool Swap()
    {
      DBxClearCacheData PrevData;
      lock (_Items)
      {
        PrevData = Holder.Swap();
        if (PrevData != null)
        {
          checked { _LastVersion++; } // ������� � ������������� ��������� ����������
          BufferItem Item = new BufferItem();
          Item.Version = _LastVersion;
          Item.Data = PrevData;
          if (PrevData.AreAllTables)
            // ����� �������� ��������� �����
            _Items.Clear();
          _Items.Add(Item);
          if (DBxCache.TraceSwitch.Enabled)
            TraceSwap(PrevData);
        }
      }
      return PrevData != null;
    }

    /// <summary>
    /// ����� ����������� � ������ Swap()
    /// </summary>
    /// <param name="prevData"></param>
    private void TraceSwap(DBxClearCacheData prevData)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(DateTime.Now.ToString("G"));
      sb.Append(" ");
      sb.Append(DisplayName);
      sb.Append(". Item version=");
      sb.Append(_LastVersion);
      sb.Append(" added");
      Trace.WriteLine(sb.ToString());

      Trace.IndentLevel++;

      Trace.WriteLine("Tables:" + prevData.ToString());

      sb.Length = 0;
      sb.Append("Ring buffer items: ");
      for (int i = 0; i < _Items.Count; i++)
      {
        if (i > 0)
          sb.Append(",");
        sb.Append(_Items[i].Version);
      }
      Trace.WriteLine(sb.ToString());

      Trace.IndentLevel--;
    }

    #endregion

    #region ��������� ���������� ���������

    /// <summary>
    /// �������� ���������� � ����������� ������� �����������.
    /// ���� � ������� ���������� ������ �� ���� ������� �������� �� ������� ������, ��� ����� Swap() ��
    /// ���������, ������������ ������ ������.
    /// ����, ��������, ����� ����� �� ���� ������ GetData() � ����� "������", ������������ ������, ���������
    /// ������ ������� ������
    /// </summary>
    /// <param name="version">�� ����� ������ ����� ��������� ������, � ������� ��� ������ ���� �����.
    /// �� ������ �������� �������� LastVersion, ������� ������ ���� ������������ ��� ��������� ������</param>
    /// <returns>������ �������� �� ���������� ������, ���������� ���������� � ����������� �������</returns>
    public DBxClearCacheData[] GetData(ref int version)
    {
      DBxClearCacheData[] Res;
      lock (_Items)
      {
        int FirstVersion = 0;
        if (_Items.Count > 0)
          FirstVersion = _Items[0].Version;
#if DEBUG
        if (FirstVersion > _LastVersion)
          throw new BugException("FirstVersion > LastVersion");
#endif

        if (version < FirstVersion)
          // ���� ������� ����� �� ���� ������ GetData() � ����� "������",
          // ���� � ����� ����� ����� ������� ���� ������ � ���������� ����� ��� ������ �� �������������
          Res = AllTablesArray;
        else if (version == _LastVersion)
          Res = EmptyArray; // �� ���� ������� ��������� � ��������� ������
        else
        {
          if (version > _LastVersion)
            throw new ArgumentException("���������� �������� �������� Version=" + version.ToString() + " ��������� ��������� ������������ ������ ������");

          // ��� � �������.
          // � ��������� ������ ���� �������� � � ������� Version, � ����� �����
          int StartPos = -1;
          for (int i = 0; i < _Items.Count; i++)
          {
            if (_Items[i].Version == version)
            {
              StartPos = i;
              break;
            }
          }
          if (StartPos < 0)
            throw new BugException("� ��������� ������ �� ����� ������� � ������� " + version.ToString());

          Res = new DBxClearCacheData[_Items.Count - StartPos - 1];
          // ������ ������������ CopyTo(), �.�. � ������ � ��� ��������� BufferItem
          // FItems.CopyTo(StartPos+1, Res, 0, Res.Length);
          for (int i = 0; i < Res.Length; i++)
            Res[i] = _Items[StartPos + i + 1].Data;
        }

        // ����� ������ ��� ���������� ������
        version = _LastVersion;
      }

      return Res;
    }

    #region ����������� ������

    /// <summary>
    /// ������������ ��� ���������� ���������
    /// </summary>
    private static readonly DBxClearCacheData[] EmptyArray = new DBxClearCacheData[0];

    /// <summary>
    /// ������������ ��� ������������� ��������� ������ �������
    /// </summary>
    private static readonly DBxClearCacheData[] AllTablesArray = CreateAllTablesArray();

    private static DBxClearCacheData[] CreateAllTablesArray()
    {
      DBxClearCacheData Data = new DBxClearCacheData();
      Data.AddAllTables();
      Data.SetReadOnly();
      return new DBxClearCacheData[1] { Data };
    }

    #endregion

    #endregion

    #region �����������

    /// <summary>
    /// ������������ ��� ��� �����������
    /// </summary>
    public string DisplayName
    {
      get { lock (_Items) { return _DisplayName; } }
      set
      {
        lock (_Items)
        {
          if (String.IsNullOrEmpty(value))
            throw new ArgumentNullException();
          _DisplayName = value;
        }
      }
    }
    private string _DisplayName;

    #endregion
  }

  #region �������

  /// <summary>
  /// ������������ � ExtDBDocs
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="clearCacheData"></param>
  public delegate void DBxClearCacheEventHandler(object sender, DBxClearCacheData clearCacheData);

  #endregion
}
