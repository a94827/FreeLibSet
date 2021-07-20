using System;
using System.Collections.Generic;
using System.Text;

namespace AgeyevAV.ExtDB
{
  /// <summary>
  /// ��������� ��������������� ����� ������, ������� ������ ���� ������� � ���������� ��������� ��� �������� �����
  /// ���� ����� �� �������� ���������������� � ������ ������ (���� IsReadOnly=false).
  /// </summary>
  [Serializable]
  public sealed class DBxClearCacheInfo : IReadOnlyObject, IEnumerable<KeyValuePair<string, IdList>>
  {
    #region �����������

    public DBxClearCacheInfo()
    {
      FAreAllTables = false;
      FItems = new Dictionary<string, IdList>();
    }

    #endregion

    #region ���������� ������

    public void Add(string TableName, Int32[] Ids)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(TableName))
        throw new ArgumentNullException("TableName");

      if (AreAllTables)
        return;

      if (Ids.Length == 0)
        return;

      IdList lst;
      if (!FItems.TryGetValue(TableName, out lst))
      {
        lst = new IdList();
        FItems.Add(TableName, lst);
      }
      lst.Add(Ids);
    }

    public void Add(string TableName, Int32 Id)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(TableName))
        throw new ArgumentNullException("TableName");

      if (Id == 0)
        throw new ArgumentException("Id=0", "Id");

      if (AreAllTables)
        return;

      IdList lst;
      if (!FItems.TryGetValue(TableName, out lst))
      {
        lst = new IdList();
        FItems.Add(TableName, lst);
      }
      lst.Add(Id);
    }

    /// <summary>
    /// ����� ����� ������ ��������, ��� ��� ������������ ������ ������ ���� �������
    /// </summary>
    public void AddAllTables()
    {
      CheckNotReadOnly();

      if (FAreAllTables)
        return;

      FAreAllTables = true;
      FItems.Clear(); // ��������� ����������������� ������ �� �����
    }

    #endregion

    #region ��������

    private Dictionary<string, IdList> FItems;

    /// <summary>
    /// ���������� true, ��� ��� ����� AddAllTables()
    /// </summary>
    public bool AreAllTables { get { return FAreAllTables; } }
    private bool FAreAllTables;

    /// <summary>
    /// ���������� true, ���� � ������ �� ���� ��������� ���������
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return (!FAreAllTables) && FItems.Count == 0;
      }
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// ���������� true, ���� ����� ��������� � ������ ���������.
    /// � ���� ������ ������ ������� Add() �� �����������
    /// </summary>
    public bool IsReadOnly { get { return FIsReadOnly; } }
    private bool FIsReadOnly;

    /// <summary>
    /// �������� ������ IsReadOnly
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (FIsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// ��������� �������� IsReadOnly=true
    /// </summary>
    public void SetReadOnly()
    {
      FIsReadOnly = true;
    }

    #endregion

    #region IEnumerable<KeyValuePair<string,IdList>> Members

    public IEnumerator<KeyValuePair<string, IdList>> GetEnumerator()
    {
      return FItems.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return FItems.GetEnumerator();
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

    public DBxClearCacheHolder()
    {
      FCurrent = new DBxClearCacheInfo();
      SyncRoot = new object();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� ������������� ������ ���������
    /// </summary>
    private DBxClearCacheInfo FCurrent;

    /// <summary>
    /// ������ �������������
    /// </summary>
    private object SyncRoot;

    #endregion

    #region ������ ���������

    public void Add(string TableName, Int32[] Ids)
    {
      lock (SyncRoot)
      {
        FCurrent.Add(TableName, Ids);
      }
    }

    public void Add(string TableName, Int32 Id)
    {
      lock (SyncRoot)
      {
        FCurrent.Add(TableName, Id);
      }
    }

    public void AddAllTables()
    {
      lock (SyncRoot)
      {
        FCurrent.AddAllTables();
      }
    }

    #endregion

    #region ������������ �� ����� �����

    /// <summary>
    /// ������������� �� ����� ������ ���������.
    /// ��������� ���������� ������.
    /// ����� �� ��������� ������������ � ��������� null, ���� � ������� ������ �� ���������������� ���������
    /// </summary>
    /// <returns>���������� ������</returns>
    public DBxClearCacheInfo Swap()
    {
      DBxClearCacheInfo Prev;
      lock (SyncRoot)
      {
        if (FCurrent.IsEmpty)
          Prev = null;
        else
        {
          Prev = FCurrent;
          FCurrent = new DBxClearCacheInfo();
          Prev.SetReadOnly();
        }
      }
      return Prev;
    }

    #endregion
  }

  /// <summary>
  /// ������ ��������� ��������� DBxClearCacheInfo. ����� �������� ������ DBxClearCacheHolder.
  /// ����� �������� ����������������
  /// </summary>
  public sealed class DBxClearCacheBuffer
  {
    #region �����������

    /// <summary>
    /// ������� ��������� �����, ���������� <paramref name="BufferSize"/> ���������.
    /// </summary>
    /// <param name="BufferSize">���������� ��������� � ������</param>
    public DBxClearCacheBuffer(int BufferSize)
    {
      if (BufferSize < 2 || BufferSize > 1000)
        throw new ArgumentOutOfRangeException();

      FHolder = Holder;
      FItems = new RingBuffer<BufferItem>(BufferSize);
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� �������� ����������.
    /// ��� ����������� ������� ������� ������ ������ ���������� ��� ������ Add()
    /// </summary>
    public DBxClearCacheHolder Holder { get { return FHolder; } }
    private DBxClearCacheHolder FHolder;

    private struct BufferItem
    {
      #region ����

      public int Version;
      public DBxClearCacheInfo Info;

      #endregion
    }

    /// <summary>
    /// ��������� ����� ��� �������� �������
    /// </summary>
    private RingBuffer<BufferItem> FItems;

    #endregion
  }
}
