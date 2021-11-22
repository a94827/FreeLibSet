// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Data
{
  /// <summary>
  /// ������ ��������������� Int32 � ����������� ����������.
  /// �������� 0 �� �����������.
  /// ������������� �������� (� ExtDBDocs.dll ���������� ��� ��������� ��������������)
  /// �������������� ��� ������� ��������������.
  /// ������� ��������� �� �����������.
  /// ����� �������������� �������� == � ��������� � ���
  /// </summary>
  [Serializable]
  public sealed class IdList : ICollection<Int32>, IEnumerable<Int32>, IReadOnlyObject, ICloneable, ISerializable
  {
    #region ������������

    /// <summary>
    /// ������� ������ ������
    /// </summary>
    public IdList()
    {
      _Items = new Dictionary<Int32, object>();
    }

    /// <summary>
    /// ������� ������ � ��������� � ���� �������������� �� �������� ���������.
    /// ���� ��������� <paramref name="source"/> �������� ������������� ��� ������� ��������������,
    /// ��� �������������
    /// </summary>
    /// <param name="source">�������� ��������������� ��� ���������� ������</param>
    public IdList(ICollection<Int32> source)
    {
      _Items = new Dictionary<Int32, object>(source.Count);
      foreach (Int32 Id in source)
        Add(Id);
    }

    /// <summary>
    /// ������� ������ � ��������� � ���� �������������� �� ��������� �������������.
    /// ���� ��������� <paramref name="source"/> �������� ������������� ��� ������� ��������������,
    /// ��� �������������
    /// </summary>
    /// <param name="source">�������� ��������������� ��� ���������� ������</param>
    public IdList(IEnumerable<Int32> source)
      : this()
    {
      foreach (Int32 Id in source)
        Add(Id);
    }


    /// <summary>
    /// ��� ��������� ������������ � ���������� Capacity, ����� �� ������� � ����������, ��� ��� ��������� Id
    /// </summary>
    /// <param name="dummy">�������</param>
    /// <param name="capacity">�������</param>
    private IdList(bool dummy, int capacity)
    {
      _Items = new Dictionary<Int32, object>(capacity);
    }

    #endregion

    #region ISerializable Members

    private IdList(SerializationInfo info, StreamingContext context)
    {
      Int32[] Ids = (Int32[])(info.GetValue("Ids", typeof(Int32[])));
      _Items = new Dictionary<Int32, object>(Ids.Length);
      for (int i = 0; i < Ids.Length; i++)
        _Items.Add(Ids[i], null);
      _IsReadOnly = info.GetBoolean("IsReadOnly");
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Ids", ToArray());
      info.AddValue("IsReadOnly", IsReadOnly);
    }

    #endregion

    #region ������ ���������������

    /// <summary>
    /// ���������� ���������������, � ������� � �������� �������� ������������ null
    /// </summary>
    private Dictionary<Int32, object> _Items;

    #endregion

    #region ICollection<int> Members

    /// <summary>
    /// ���������� ���������� ��������������� � ������
    /// </summary>
    public int Count
    {
      get { return _Items.Count; }
    }

    /// <summary>
    /// ���������� true, ���� ������������� ���� � ������
    /// </summary>
    /// <param name="id">����������� �������������</param>
    /// <returns>true, ���� ������������� ���� � ������</returns>
    public bool Contains(Int32 id)
    {
      return _Items.ContainsKey(id);
    }

    /// <summary>
    /// �������� �������������� � ������.
    /// </summary>
    /// <param name="array">����������� ������</param>
    /// <param name="arrayIndex">������ ������ � �������, � �������� ������ ����������</param>
    public void CopyTo(Int32[] array, int arrayIndex)
    {
      _Items.Keys.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// ���������� ������ ���������������
    /// </summary>
    /// <returns>������ - �����</returns>
    public Int32[] ToArray()
    {
      Int32[] Ids = new Int32[_Items.Count];
      _Items.Keys.CopyTo(Ids, 0);
      return Ids;
    }

    /// <summary>
    /// ��������� ������������� � ������.
    /// ���� ������ �������� 0 ��� ����� ������������� ��� ���� � ������, ������� �������� �� �����������.
    /// </summary>
    /// <param name="id">����������� �������������</param>
    public void Add(Int32 id)
    {
      CheckNotReadOnly();

      if (id == 0)
        return;
      if (!_Items.ContainsKey(id))
        _Items.Add(id, null);
    }

    /// <summary>
    /// ��������� ��� �������������� �� ������� ������.
    /// ���� � ������� ������ ��� ���� ����� ��������������, �� ��� ������������
    /// </summary>
    /// <param name="source">����������� ������</param>
    public void Add(IdList source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      foreach (Int32 Id in source)
        Add(Id);
    }

    /// <summary>
    /// ��������� ��� �������������� �� �������������.
    /// ������� �������������� ������������.
    /// ���� � ������� ������ ��� ���� ����� ��������������, �� ��� ������������
    /// </summary>
    /// <param name="source">����������� ������</param>
    public void Add(IEnumerable<Int32> source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      foreach (Int32 Id in source)
        Add(Id);
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _Items.Clear();
    }

    /// <summary>
    /// ������� ������������� �� ������.
    /// ���� �������������� ��� � ������, ������ �������� �� �����������.
    /// </summary>
    /// <param name="id">��������� �������������.</param>
    /// <returns>true, ���� ������������� ������ � ������</returns>
    public bool Remove(Int32 id)
    {
      CheckNotReadOnly();

      return _Items.Remove(id);
    }

    /// <summary>
    /// ������� �� �������� ������ ��� ��������������, ������� ���� � ������ <paramref name="source"/>.
    /// </summary>
    /// <param name="source">������ ��������� ���������������</param>
    public void Remove(IdList source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      foreach (Int32 Id in source)
        Remove(Id);
    }

    /// <summary>
    /// ������� �� �������� ������ ��� ��������������, ������� ���� � ������ <paramref name="source"/>.
    /// </summary>
    /// <param name="source">������ ��������� ���������������</param>
    public void Remove(IEnumerable<Int32> source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      foreach (Int32 Id in source)
        Remove(Id);
    }

    /// <summary>
    /// ������� �� �������� ������ ��� ��������������, ������� ��� � ������ <paramref name="source"/>.
    /// </summary>
    /// <param name="source">������ ���������������, ������� ��������� �������</param>
    public void RemoveOthers(IdList source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      Int32[] a = ToArray();
      for (int i = 0; i < a.Length; i++)
      {
        if (!source.Contains(a[i]))
          Remove(a[i]);
      }
    }

    /// <summary>
    /// ������� �� �������� ������ ��� ��������������, ������� ��� � ������ <paramref name="source"/>.
    /// </summary>
    /// <param name="source">������ ���������������, ������� ��������� �������</param>
    public void RemoveOthers(IEnumerable<Int32> source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      // ������������ ������� ������ - ������� ��������� IdList
      IdList Source2 = new IdList();
      foreach (Int32 Id in source)
        Source2.Add(Id);


      RemoveOthers(Source2);
    }

    #endregion

    #region IEnumerable<int> Members

    /// <summary>
    /// ���������� ������������� �� ���� ��������������� � ������
    /// 
    /// ��� ������������� �������� ����� ���������� � �������, 
    /// ������������� ������ ���������� ���������� �������������.
    /// ������� � ���������� ���� ����� ������ �������������� ������������� ��� ������������� � ��������� foreach.
    /// </summary>
    /// <returns>�������������</returns>
    public Dictionary<Int32, object>.KeyCollection.Enumerator GetEnumerator()
    {
      return _Items.Keys.GetEnumerator();
    }

    IEnumerator<Int32> IEnumerable<Int32>.GetEnumerator()
    {
      return _Items.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _Items.Keys.GetEnumerator();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// ���������� true, ���� ������ �������� � ������ "������ ������".
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;


    /// <summary>
    /// ���������� ����������, ���� ������ �������� � ������ "������ ������".
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException("������ ������������� ��������� � ������ ReadOnly");
    }

    /// <summary>
    /// ��������� ������ � ����� "������ ������".
    /// ��������� ����� ������ �� ��������� ������� ��������.
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region ����� ����������

    /// <summary>
    /// ���������� true, ���� ���� ���� �� ���� ����������� �������������.
    /// ���� <paramref name="other"/> ������ ������ ������, �� ������������ false.
    /// </summary>
    /// <param name="other">������ ����������� ���������������</param>
    /// <returns>true ��� ������� �����������</returns>
    public bool ContainsAny(IdList other)
    {
#if DEBUG
      if (other == null)
        throw new ArgumentNullException("other");
#endif

      if (this.Count == 0 || other.Count == 0)
        return false;

      foreach (Int32 OtherId in other)
      {
        if (Contains(OtherId))
          return true;
      }
      return false;
    }

    /// <summary>
    /// ���������� true, ���� ���� ���� �� ���� ����������� �������������.
    /// ���� <paramref name="other"/> ������ ������ ������, �� ������������ false.
    /// </summary>
    /// <param name="other">������ ����������� ���������������</param>
    /// <param name="firstMatchedId">���� ���������� ������ ����������� �������������</param>
    /// <returns>true ��� ������� �����������</returns>
    public bool ContainsAny(IdList other, out Int32 firstMatchedId)
    {
#if DEBUG
      if (other == null)
        throw new ArgumentNullException("other");
#endif

      firstMatchedId = 0;
      if (this.Count == 0 || other.Count == 0)
        return false;

      foreach (Int32 OtherId in other)
      {
        if (Contains(OtherId))
        {
          firstMatchedId = OtherId;
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// ���������� true, ���� ���� ���� �� ���� ����������� �������������.
    /// </summary>
    /// <param name="ids">������ ����������� ���������������</param>
    /// <returns>true ��� ������� �����������</returns>
    public bool ContainsAny(Int32[] ids)
    {
      Int32 FirstMatchedId;
      return ContainsAny(ids, out FirstMatchedId);
    }

    /// <summary>
    /// ���������� true, ���� ���� ���� �� ���� ����������� �������������.
    /// �������� 0 � ������ <paramref name="ids"/> ������������.
    /// </summary>
    /// <param name="ids">������ ����������� ���������������</param>
    /// <param name="firstMatchedId">���� ���������� ������ ����������� �������������</param>
    /// <returns>true ��� ������� �����������</returns>
    public bool ContainsAny(Int32[] ids, out Int32 firstMatchedId)
    {
#if DEBUG
      if (ids == null)
        throw new ArgumentNullException("ids");
#endif

      firstMatchedId = 0;

      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] == 0)
          continue;
        if (Contains(ids[i]))
        {
          firstMatchedId = ids[i];
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// ���������� true, ���� � ������� ������ ���� ��� �������������� �� ������� ������.
    /// ���� <paramref name="other"/> ������ ������ ������, �� ������������ true.
    /// </summary>
    /// <param name="other">������ ����������� ���������������</param>
    /// <returns>true ��� ������� ���� ���������������</returns>
    public bool Contains(IdList other)
    {
#if DEBUG
      if (other == null)
        throw new ArgumentNullException("other");
#endif

      if (other.Count == 0)
        return true;

      if (this.Count == 0)
        return false;

      foreach (Int32 OtherId in other)
      {
        if (!Contains(OtherId))
          return false;
      }

      return true;
    }

    /// <summary>
    /// ���������� true, ���� � ������� ������ ���� ��� �������������� �� ������� ������.
    /// ���� <paramref name="ids"/> ������ ������ ������, �� ������������ true.
    /// �������� 0 � ������ <paramref name="ids"/> ������������.
    /// </summary>
    /// <param name="ids">������ ����������� ���������������</param>
    /// <returns>true ��� ������� ���� ���������������</returns>
    public bool Contains(Int32[] ids)
    {
#if DEBUG
      if (ids == null)
        throw new ArgumentNullException("ids");
#endif

      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] == 0)
          continue;

        if (!Contains(ids[i]))
          return false;
      }

      return true;
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// ������� ����� ������.
    /// � ����� ������ �������� IsReadOnly �� �����������.
    /// </summary>
    /// <returns>����� ������</returns>
    public IdList Clone()
    {
      return new IdList(this);
    }

    object ICloneable.Clone()
    {
      return new IdList(this);
    }

    #endregion

    #region ���������

    /// <summary>
    /// ���������� true, ���� ��� ������ �������� ���������� �������������� (��� ����� ������� ����������).
    /// ����� ���������� true, ���� ��� ������ ����� null
    /// </summary>
    /// <param name="a">������ ������ ��� ���������</param>
    /// <param name="b">������ ������ ��� ���������</param>
    /// <returns>��������� �������</returns>
    public static bool operator ==(IdList a, IdList b)
    {
      if (Object.ReferenceEquals(a, b))
        return true;
      if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null))
        return false;

      if (a.Count != b.Count)
        return false;

      return a.Contains(b);
    }

    /// <summary>
    /// ���������� true, ���� � ������� ���� �� ���� ������������� �� ���������� (��� ����� ������� ����������).
    /// ����� ���������� true, ���� ���� �� ������ ����� null, � ������ - ���
    /// </summary>
    /// <param name="a">������ ������ ��� ���������</param>
    /// <param name="b">������ ������ ��� ���������</param>
    /// <returns>��������� �������</returns>
    public static bool operator !=(IdList a, IdList b)
    {
      return !(a == b);
    }

    /// <summary>
    /// ���������� true, ���� <paramref name="obj"/> ��������� �� IdList � ������ ���������.
    /// </summary>
    /// <param name="obj">������������ ������</param>
    /// <returns>true, ���� ������ ���������</returns>
    public override bool Equals(object obj)
    {
      if (obj is IdList)
        return this == (IdList)obj;
      else
        return false;
    }

    /// <summary>
    /// ���-��� ������. ���������� �������� Count.
    /// </summary>
    /// <returns>���-���</returns>
    public override int GetHashCode()
    {
      return Count;
    }

    #endregion

    #region ��������� �������� / ���������

    /// <summary>
    /// ���������� ������, ���������� �������������� �� ����� �������
    /// � ��������������� ������ �������� IsReadOnly �� �����������
    /// ����, ��� � �������� "+"
    /// </summary>
    /// <param name="a">������ ������</param>
    /// <param name="b">������ ������</param>
    /// <returns>������������ ������</returns>
    public static IdList operator |(IdList a, IdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      IdList Res = a.Clone();
      foreach (Int32 Id in b)
        Res.Add(Id);
      return Res;
    }

    /// <summary>
    /// ���������� ������, ���������� ��������������, �������� � ��� ������
    /// � ��������������� ������ �������� IsReadOnly �� �����������
    /// </summary>
    /// <param name="a">������ ������</param>
    /// <param name="b">������ ������</param>
    /// <returns>������ � ������ ����������������</returns>
    public static IdList operator &(IdList a, IdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      IdList Res = new IdList();
      foreach (Int32 Id in a)
      {
        if (b.Contains(Id))
          Res.Add(Id);
      }
      return Res;
    }

    /// <summary>
    /// ���������� ������, ���������� �������������� �� ����� �������
    /// � ��������������� ������ �������� IsReadOnly �� �����������
    /// ����, ��� � �������� "|"
    /// </summary>
    /// <param name="a">������ ������</param>
    /// <param name="b">������ ������</param>
    /// <returns>������������ ������</returns>
    public static IdList operator +(IdList a, IdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      IdList Res = a.Clone();
      foreach (Int32 Id in b)
        Res.Add(Id);
      return Res;
    }

    /// <summary>
    /// ���������� ������, ���������� �������������� �� ������� ������, ������� ��� �� ������ ������
    /// � ��������������� ������ �������� IsReadOnly �� �����������
    /// </summary>
    /// <param name="a">������ ������ (�������)</param>
    /// <param name="b">������ ������ (����������)</param>
    /// <returns>���������� ������</returns>
    public static IdList operator -(IdList a, IdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      IdList Res = new IdList();
      foreach (Int32 Id in a)
      {
        if (!b.Contains(Id))
          Res.Add(Id);
      }
      return Res;
    }

    #endregion

    #region ������ ������ � ��������

    /// <summary>
    /// ���������� ������ ���������������, ����������� ��������
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return StdConvert.ToString(ToArray());
    }

    /// <summary>
    /// ���� ������ �������� ������������ �������������, �������� ���������� ���.
    /// ����� ������������ 0.
    /// �������� ����� ���� ������� ��� ����������� ����, ����� ������ �� ������ �������������� �������������� ������ �������
    /// </summary>
    public Int32 SingleId
    {
      get
      {
        if (_Items.Count == 1)
        {
          // ��� ����� ��������� �������
          Int32[] a1 = new Int32[1];
          _Items.Keys.CopyTo(a1, 0);
          return a1[0];
        }
        else
          return 0;
      }
    }

    #endregion

    #region ����������� ������ �������� �� ������

    #region FromIds

    /// <summary>
    /// ������� ������ �� ���� Id, ������� �������� ��������� ������ �������.
    /// ���������� DataTools.GetIds()
    /// </summary>
    /// <param name="table">������� � ����� "Id"</param>
    /// <returns>������ ���������������</returns>
    public static IdList FromIds(DataTable table)
    {
      if (table == null)
        return new IdList(); // 20.08.2019

      int ColPos = table.Columns.IndexOf("Id");
      if (ColPos < 0)
        throw new ArgumentException("�������\"" + table.TableName + "\" �� �������� ������� \"Id\"", "table");

      IdList Res = new IdList(false, table.Rows.Count);
      for (int i = 0; i < table.Rows.Count; i++)
      {
        if (table.Rows[i].RowState == DataRowState.Deleted)
          Res._Items.Add((Int32)(table.Rows[i][ColPos, DataRowVersion.Original]), null);
        else
          Res._Items.Add((Int32)(table.Rows[i][ColPos]), null);
      }
      return Res;
    }

    /// <summary>
    /// �������� ������ ��������������� ��� ��������� ���� "Id" � ������� ��� 
    /// �����, �������� � �������� DataView
    /// ������� ���������� ��������������� ������������� ������� ����� � ���������
    /// </summary>
    /// <param name="dv">�������� DataView</param>
    /// <returns>������ �������� ���������������</returns>
    public static IdList FromIds(DataView dv)
    {
      if (dv == null)
        return new IdList(); // 20.08.2019

      int ColPos = dv.Table.Columns.IndexOf("Id");
      if (ColPos < 0)
        throw new ArgumentException("�������\"" + dv.Table.TableName + "\" �� �������� ������� \"Id\"", "dv");

      IdList Res = new IdList(false, dv.Count);
      for (int i = 0; i < dv.Count; i++)
      {
        DataRow Row = dv[i].Row;
        if (Row.RowState == DataRowState.Deleted)
          Res._Items.Add((Int32)(Row[ColPos, DataRowVersion.Original]), null);
        else
          Res._Items.Add((Int32)(Row[ColPos]), null);
      }
      return Res;
    }

    /// <summary>
    /// ��������� �������� ���� "Id" �� ������� �����. � ������� �� FromField()
    /// �� ��������� ������� �������� � �� ��������� �������.
    /// </summary>
    /// <param name="rows">������ �����</param>
    /// <returns>������ ���������������</returns>
    public static IdList FromIds(ICollection<DataRow> rows)
    {
      if (rows == null)
        return new IdList(); // 20.08.2019

      IdList Res = new IdList(false, rows.Count);

      int ColPos = -1;
      foreach (DataRow Row in rows)
      {
        if (ColPos < 0)
        {
          ColPos = Row.Table.Columns.IndexOf("Id");
          if (ColPos < 0)
            throw new ArgumentException("�������\"" + Row.Table.TableName + "\" �� �������� ������� \"Id\"", "rows");
        }
        if (Row.RowState == DataRowState.Deleted)
          Res._Items.Add((Int32)(Row[ColPos, DataRowVersion.Original]), null);
        else
          Res._Items.Add((Int32)(Row[ColPos]), null);
      }
      return Res;
    }

    /// <summary>
    /// ��������� �������� ���� "Id" �� ������� ����� DataRowView. � ������� �� FromField()
    /// �� ��������� ������� �������� � �� ��������� �������
    /// </summary>
    /// <param name="rows">������ ����� ���� DataRowView</param>
    /// <returns>������ ���������������</returns>
    public static IdList FromIds(ICollection<DataRowView> rows)
    {
      if (rows == null)
        return new IdList(); // 20.08.2019

      IdList Res = new IdList(false, rows.Count);

      int ColPos = -1;
      foreach (DataRowView drv in rows)
      {
        if (ColPos < 0)
        {
          ColPos = drv.Row.Table.Columns.IndexOf("Id");
          if (ColPos < 0)
            throw new InvalidOperationException("�������\"" + drv.Row.Table.TableName + "\" �� �������� ������� \"Id\"");
        }
        if (drv.Row.RowState == DataRowState.Deleted)
          Res._Items.Add((Int32)(drv.Row[ColPos, DataRowVersion.Original]), null);
        else
          Res._Items.Add((Int32)(drv.Row[ColPos]), null);
      }

      return Res;
    }

    /// <summary>
    /// ���������� ������, ��������� �� ������ ��������������.
    /// ���� ������� ������� �������������, �� ������������ ������ ������, �������, ������, ����� �������������.
    /// </summary>
    /// <param name="id">�������������, ����������� � ������</param>
    /// <returns>������</returns>
    public static IdList FromId(Int32 id)
    {
      IdList List = new IdList();
      if (id != 0)
        List.Add(id);
      return List;
    }

    #endregion

    #region FromField

    /// <summary>
    /// ��������� ������ �������� �������� ���� (���������������), 
    /// ������� ��������� ��������� ���� � �������. 
    /// ������� �������� ������������� � ������� �������������
    /// </summary>
    /// <param name="table">������� ������</param>
    /// <param name="columnName">��� ��������� ���������� ����</param>
    /// <returns>������ ���������������</returns>
    public static IdList FromField(DataTable table, string columnName)
    {
      if (table == null)
        return new IdList(); // 20.08.2019

      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
      int ColPos = table.Columns.IndexOf(columnName);
      if (ColPos < 0)
        throw new ArgumentException("�������\"" + table.TableName + "\" �� �������� ������� \"" +
          columnName + "\"", "columnName");

      IdList Res = new IdList(); // �� ����� �������� �������, �.�. ������� ����� ���� ���� ������ ��������

      foreach (DataRow Row in table.Rows)
      {
        if (Row.IsNull(ColPos))
          continue;
        Int32 Id = (Int32)(Row[ColPos]);
        if (Id == 0)
          continue;

        if (!Res._Items.ContainsKey(Id))
          Res._Items.Add(Id, null);
      }

      return Res;
    }

    /// <summary>
    /// ��������� ������ �������� �������� ���� (���������������), 
    /// ������� ��������� ��������� ���� � ������� ��� �����, ����������� � 
    /// ������� DataView.
    /// ������� �������� ������������� � ������� �������������
    /// </summary>
    /// <param name="dv">��������� ����� ������� ������</param>
    /// <param name="columnName">��� ��������� ���������� ����</param>
    /// <returns>������ ���������������</returns>
    public static IdList FromField(DataView dv, string columnName)
    {
      if (dv == null)
        return new IdList(); // 20.08.2019

      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      int ColPos = dv.Table.Columns.IndexOf(columnName);
      if (ColPos < 0)
        throw new ArgumentException("�������\"" + dv.Table.TableName + "\" �� �������� ������� \"" +
          columnName + "\"", "columnName");

      IdList Res = new IdList(); // �� ����� �������� �������, �.�. ������� ����� ���� ���� ������ ��������

      for (int i = 0; i < dv.Count; i++)
      {
        if (dv[i].Row.IsNull(ColPos))
          continue;
        Int32 Id = (Int32)(dv[i].Row[ColPos]);
        if (Id == 0)
          continue;

        if (!Res._Items.ContainsKey(Id))
          Res._Items.Add(Id, null);
      }

      return Res;
    }

    /// <summary>
    /// ��������� ������ �������� �������� ���� (���������������), 
    /// ������� ��������� ��������� ���� ��� ����� ������� � �������. 
    /// ������� �������� ������������� � ������� �������������
    /// ������ � ������� ������ ���������� ���� � ����� �������, ���� � ��������,
    /// ������� ���������� ���������
    /// </summary>
    /// <param name="rows">������ ���������� �����</param>
    /// <param name="columnName">��� ��������� ���������� ����</param>
    /// <returns>������ ���������������</returns>
    public static IdList FromField(ICollection<DataRow> rows, string columnName)
    {
      if (rows == null)
        return new IdList(); // 20.08.2019

      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      IdList Res = new IdList(); // �� ����� �������� �������, �.�. ������� ����� ���� ���� ������ ��������
      int ColPos = -1;
      foreach (DataRow Row in rows)
      {
        if (ColPos < 0)
        {
          ColPos = Row.Table.Columns.IndexOf(columnName);
          if (ColPos < 0)
            throw new ArgumentException("�������\"" + Row.Table.TableName + "\" �� �������� ������� \"" +
              columnName + "\"", "columnName");
        }

        if (Row.IsNull(ColPos))
          continue;
        Int32 Id = (Int32)(Row[ColPos]);
        if (Id == 0)
          continue;
        if (!Res._Items.ContainsKey(Id))
          Res._Items.Add(Id, null);
      }

      return Res;
    }

    /// <summary>
    /// ��������� ������ �������� �������� ���� (���������������), 
    /// ������� ��������� ��������� ���� ��� ����� ������� � �������. 
    /// ������� �������� ������������� � ������� �������������
    /// ������ � ������� ������ ���������� ���� � ����� �������, ���� � ��������,
    /// ������� ���������� ���������
    /// </summary>
    /// <param name="rows">������ ���������� ����� ��� ��������� DataRowView</param>
    /// <param name="columnName">��� ��������� ���������� ����</param>
    /// <returns>������ ���������������</returns>
    public static IdList FromField(ICollection<DataRowView> rows, string columnName)
    {
      if (rows == null)
        return new IdList(); // 20.08.2019

      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      IdList Res = new IdList(); // �� ����� �������� �������, �.�. ������� ����� ���� ���� ������ ��������
      int ColPos = -1;
      foreach (DataRowView drv in rows)
      {
        if (ColPos < 0)
        {
          ColPos = drv.Row.Table.Columns.IndexOf(columnName);
          if (ColPos < 0)
            throw new ArgumentException("�������\"" + drv.Row.Table.TableName + "\" �� �������� ������� \"" +
              columnName + "\"", "columnName");
        }

        if (drv.Row.IsNull(ColPos))
          continue;
        Int32 Id = (Int32)(drv.Row[ColPos]);
        if (Id == 0)
          continue;
        if (!Res._Items.ContainsKey(Id))
          Res._Items.Add(Id, null);
      }

      return Res;
    }

    #endregion

    #region FromArray

    /// <summary>
    /// �������� ������ ��������������� �� ������� �������.
    /// ������������� � ������� �������������� ������������
    /// </summary>
    /// <param name="ids">������ ���������������</param>
    /// <returns></returns>
    public static IdList FromArray(Int32[] ids)
    {
      if (ids == null)
        return new IdList();

      IdList Res = new IdList(); // �� ����� �������� �������, �.�. ������� ����� ���� ���� ������ ��������

      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] == 0)
          continue;
        if (!Res._Items.ContainsKey(ids[i]))
          Res._Items.Add(ids[i], null);
      }
      return Res;
    }

    /// <summary>
    /// �������� ������ ��������������� �� ������� �������.
    /// ������������� � ������� �������������� ������������
    /// </summary>
    /// <param name="ids">������ ���������������</param>
    /// <returns></returns>
    public static IdList FromArray(Int32[,] ids)
    {
      if (ids == null)
        return new IdList();

      IdList Res = new IdList(); // �� ����� �������� �������, �.�. ������� ����� ���� ���� ������ ��������

      int n1 = ids.GetLowerBound(0);
      int n2 = ids.GetUpperBound(0);
      int m1 = ids.GetLowerBound(1);
      int m2 = ids.GetUpperBound(1);

      for (int i = n1; i <= n2; i++)
      {
        for (int j = m1; j <= m2; j++)
        {
          if (ids[i, j] == 0)
            continue;

          if (!Res._Items.ContainsKey(ids[i, j]))
            Res._Items.Add(ids[i, j], null);
        }
      }

      return Res;
    }


    /// <summary>
    /// �������� ������ ��������������� �� jagged-�������.
    /// ������������� � ������� �������������� ������������
    /// </summary>
    /// <param name="ids">������ ���������������</param>
    /// <returns></returns>
    public static IdList FromArray(Int32[][] ids)
    {
      if (ids == null)
        return new IdList();

      IdList Res = new IdList(); // �� ����� �������� �������, �.�. ������� ����� ���� ���� ������ ��������

      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] == null)
          continue;
        Res.Add(ids[i]);
      }
      return Res;
    }


    #endregion

    #endregion

    #region ����������� ���������

    /// <summary>
    /// ������ ������ ��������������� � ������������� ��������� IsReadOnly=true
    /// </summary>
    public static readonly IdList Empty = CreateEmpty();

    private static IdList CreateEmpty()
    {
      IdList Res = new IdList();
      Res.SetReadOnly();
      return Res;
    }

    #endregion
  }

  /// <summary>
  /// ����� ������� ��������������� ��� ���������� ������.
  /// ��������� �������, � ������� ������ �������� ��� �������, � ��������� - ������ ���������������.
  /// ����� �������� ���������������� ����� ������ SetReadOnly(). � �������� ���������� ����� �� �������� ����������������.
  /// ����� ��������� ����������������� � �������� ��� ���� ������.
  /// </summary>
  [Serializable]
  public sealed class TableAndIdList : ICloneable, IReadOnlyObject
  {
    #region ������������

    /// <summary>
    /// ������� �����
    /// </summary>
    /// <param name="ignoreCase">������ �� �������������� ������� �������� � ������ ������</param>
    public TableAndIdList(bool ignoreCase)
    {
      _Tables = new TableDict(ignoreCase);
    }

    /// <summary>
    /// ������� �����.
    /// ����� ������ ����� ������������� � ��������
    /// </summary>
    public TableAndIdList()
      : this(false)
    {
    }

    private TableAndIdList(int dummy)
      : this(false)
    {
      SetReadOnly();
    }

    #endregion

    #region ������� ������

    [Serializable]
    private class TableDict : TypedStringDictionary<IdList>
    {
      #region �����������

      public TableDict(bool ignoreCase)
        : base(ignoreCase)
      {
      }

      #endregion

      #region SetReadOnly

      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }

      #endregion
    }

    private TableDict _Tables;

    /// <summary>
    /// ������ � ������ ��������������� ��� ������� � �������� ������.
    /// ���� ������� ��� �� ���� � ������, ��, ���� �������� IsReadOnly=false (������ � ������ ����������), ���������
    /// ����� ������ ������ IdList, � ������� ����� ��������� ��������������. ���� IsReadOnly=true, �� ������������ ������ �� IdList.Empty
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <returns>������ ���������������</returns>
    public IdList this[string tableName]
    {
      get
      {
        IdList list;
        if (!_Tables.TryGetValue(tableName, out list))
        {
          if (String.IsNullOrEmpty(tableName))
            throw new ArgumentNullException("tableName");

          if (_Tables.IsReadOnly)
            return IdList.Empty;
          else
          {
            list = new IdList();
            _Tables.Add(tableName, list);
          }
        }
        return list;
      }
    }

    #endregion

    #region �������������� ������

    /// <summary>
    /// ���������� ������� �������������� ��� �������� ������� � ������.
    /// ��������� �������� ��������� ��� ������� ������������ �� ������, � ����������� �� ��������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="id">�������������</param>
    /// <returns>������� �������������� ��� ������</returns>
    public bool this[string tableName, Int32 id]
    {
      get
      {
        IdList list;
        if (_Tables.TryGetValue(tableName, out list))
          return list.Contains(id);
        else
          return false;
      }
      set
      {
        if (value)
          this[tableName].Add(id);
        else
          this[tableName].Remove(id);
      }
    }

    #endregion

    #region ����� ����������

    /// <summary>
    /// ���������� ����� ���������� ��������������� ��� ���� ������
    /// </summary>
    public int Count
    {
      get
      {
        int cnt = 0;
        foreach (KeyValuePair<string, IdList> pair in _Tables)
          cnt += pair.Value.Count;
        return cnt;
      }
    }

    /// <summary>
    /// ���������� true, ���� � ������ ��� �� ������ ��������������.
    /// ������� ��� ��������������� �� �����������
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        foreach (KeyValuePair<string, IdList> pair in _Tables)
        {
          if (pair.Value.Count > 0)
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// ������ ������ ��� ����������� ���������
    /// </summary>
    public static readonly TableAndIdList Empty = new TableAndIdList(0);

    /// <summary>
    /// ���������� ������ ���� ������, � ������� ���� ��������������
    /// </summary>
    /// <returns></returns>
    public string[] GetTableNames()
    {
      if (IsReadOnly)
      {
        // � ������ "������ ������" �� ����� ���� ������ ������� IdList

        string[] a = new string[_Tables.Count];
        _Tables.Keys.CopyTo(a, 0);
        return a;
      }
      else
      {
        List<string> lst = new List<string>();
        foreach (KeyValuePair<string, IdList> pair in _Tables)
        {
          if (pair.Value.Count > 0)
            lst.Add(pair.Key);
        }
        return lst.ToArray();
      }
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// ���������� true, ���� ����� ��������� � ������ ���������
    /// </summary>
    public bool IsReadOnly { get { return _Tables.IsReadOnly; } }

    /// <summary>
    /// ����������� ����������, ���� ����� ��������� � ������ ���������
    /// </summary>
    public void CheckNotReadOnly()
    {
      _Tables.CheckNotReadOnly();
    }

    /// <summary>
    /// ��������� ������ � ����� "������ ������"
    /// </summary>
    public void SetReadOnly()
    {
      // ��� ��� �������� �������, ������������� ����������� ����� ������
      lock (_Tables)
      {
        if (!_Tables.IsReadOnly)
        {
          #region �������� IdList.SetReadOnly() � ������� ������� ��� ���������������

          if (_Tables.Count > 0)
          {
            string[] allNames = new string[_Tables.Count];
            _Tables.Keys.CopyTo(allNames, 0);
            for (int i = 0; i < allNames.Length; i++)
            {
              if (_Tables[allNames[i]].Count == 0)
                _Tables.Remove(allNames[i]);
              else
                _Tables[allNames[i]].SetReadOnly();
            }
          }

          #endregion

          _Tables.SetReadOnly(); // �������� �������
        }
      }
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// ������� ����� ������, � �������� ������� IsReadOnly �� ����������
    /// </summary>
    /// <returns></returns>
    public TableAndIdList Clone()
    {
      TableAndIdList res = new TableAndIdList(_Tables.IgnoreCase);
      foreach (KeyValuePair<string, IdList> pair in _Tables)
        res._Tables.Add(pair.Key, pair.Value.Clone());
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region �������� � ���������

    #region ������ ����������� �������� ������

    /// <summary>
    /// ��������� ��� �������������� �� ������� ������.
    /// ���� � ������� ������ ��� ���� ����� ��������������, �� ��� ������������
    /// </summary>
    /// <param name="source">����������� ������</param>
    public void Add(TableAndIdList source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      foreach (string tableName in source.GetTableNames())
        this[tableName].Add(source[tableName]);
    }

    /// <summary>
    /// ������� �� �������� ������ ��������������, ������� ���� � ������ ������.
    /// ���� � ������� ������ ��� ��������� ���������������, �� ��� ������������.
    /// </summary>
    /// <param name="source">���������� ������</param>
    public void Remove(TableAndIdList source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();

      foreach (string tableName in source.GetTableNames())
      {
        IdList list;
        if (_Tables.TryGetValue(tableName, out list))
          list.Remove(source[tableName]);
      }
    }

    #endregion

    #region ���������

    /// <summary>
    /// ���������� ������, ���������� �������������� �� ����� �������
    /// � ��������������� ������ �������� IsReadOnly �� �����������
    /// ����, ��� � �������� "+"
    /// </summary>
    /// <param name="a">������ ������</param>
    /// <param name="b">������ ������</param>
    /// <returns>������������ ������</returns>
    public static TableAndIdList operator |(TableAndIdList a, TableAndIdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      TableAndIdList Res = a.Clone();
      Res.Add(b);
      return Res;
    }

    /// <summary>
    /// ���������� ������, ���������� �������������� �� ����� �������
    /// � ��������������� ������ �������� IsReadOnly �� �����������
    /// ����, ��� � �������� "+"
    /// </summary>
    /// <param name="a">������ ������</param>
    /// <param name="b">������ ������</param>
    /// <returns>������������ ������</returns>
    public static TableAndIdList operator +(TableAndIdList a, TableAndIdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      TableAndIdList Res = a.Clone();
      Res.Add(b);
      return Res;
    }

    /// <summary>
    /// ���������� ������, ���������� �������������� �� ������� ������, ������� ��� �� ������ ������
    /// � ��������������� ������ �������� IsReadOnly �� �����������
    /// </summary>
    /// <param name="a">������ ������ (�������)</param>
    /// <param name="b">������ ������ (����������)</param>
    /// <returns>���������� ������</returns>
    public static TableAndIdList operator -(TableAndIdList a, TableAndIdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      TableAndIdList Res = a.Clone();
      Res.Remove(b);
      return Res;
    }

    /// <summary>
    /// ���������� ������, ���������� ��������������, �������� � ��� ������
    /// � ��������������� ������ �������� IsReadOnly �� �����������
    /// </summary>
    /// <param name="a">������ ������</param>
    /// <param name="b">������ ������</param>
    /// <returns>������ � ������ ����������������</returns>
    public static TableAndIdList operator &(TableAndIdList a, TableAndIdList b)
    {
#if DEBUG
      if (a == null)
        throw new ArgumentNullException("a");
      if (b == null)
        throw new ArgumentNullException("b");
#endif

      TableAndIdList Res = new TableAndIdList();
      foreach (string tableName in a.GetTableNames())
      {
        IdList resList = a[tableName] & b[tableName];
        if (resList.Count > 0)
          Res[tableName].Add(resList);
      }
      return Res;
    }

    #endregion

    #endregion

    #region ���������

    /// <summary>
    /// ���������� true, ���� ��� ������ ��������� ���������
    /// </summary>
    /// <param name="a">������ ������������ ������</param>
    /// <param name="b">������ ������������ ������</param>
    /// <returns>��������� ���������</returns>
    public static bool operator ==(TableAndIdList a, TableAndIdList b)
    {
      if (Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null))
        return true;
      if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null))
        return false;

      string[] tableNamesA = a.GetTableNames();
      string[] tableNamesB = b.GetTableNames();
      if (tableNamesA.Length != tableNamesB.Length)
        return false; // �������� �����, ����� �� ��������� � ������ b ������, ������� ��� � ������ a.

      foreach (string tableName in tableNamesA)
      {
        IdList listA = a[tableName];
        IdList listB = b[tableName];
        if (listA != listB)
          return false;
      }

      return true;
    }


    /// <summary>
    /// ���������� true, ���� ��� ������ ����������
    /// </summary>
    /// <param name="a">������ ������������ ������</param>
    /// <param name="b">������ ������������ ������</param>
    /// <returns>��������� ���������</returns>
    public static bool operator !=(TableAndIdList a, TableAndIdList b)
    {
      return !(a == b);
    }

    /// <summary>
    /// ���������� true, ���� ������� ������ ��������� ��������� � <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">������ ������������ ������</param>
    /// <returns>��������� ���������</returns>
    public override bool Equals(object obj)
    {
      TableAndIdList b = obj as TableAndIdList;
      return (this == b);
    }

    /// <summary>
    /// ���-��� ��� ���������.
    /// ��� ��� TableAndNameList �� ������������ ��� ������������� � �������� ����� ���������,
    /// ����� ���������� 0.
    /// </summary>
    /// <returns>���-���</returns>
    public override int GetHashCode()
    {
      return 0;
    }

    #endregion

    #region ��������� �������������

    /// <summary>
    /// ��� �������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString() + (IsReadOnly ? " (ReadOnly)" : String.Empty);
    }

    #endregion
  }
}
