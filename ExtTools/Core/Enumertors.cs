using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Microsoft.Win32;
using System.Data;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

namespace FreeLibSet.Core
{
  /// <summary>
  /// ������ �������������.
  /// ��������� ��������� IEnumerator ��� ���������, �� ���������� ���������
  /// </summary>
  /// <typeparam name="T">��� "������������" ��������</typeparam>
  [Serializable]
  public sealed class DummyEnumerator<T> : IEnumerator<T>
  {
    #region IEnumerator<T> Members

    T IEnumerator<T>.Current
    {
      get { return default(T); }
    }

    void IDisposable.Dispose()
    {
    }

    object IEnumerator.Current
    {
      get { return null; }
    }

    bool IEnumerator.MoveNext()
    {
      return false;
    }

    void IEnumerator.Reset()
    {
    }

    #endregion
  }

  /// <summary>
  /// ��������� ��������� IEnumerable ��� ���������, �� ���������� ���������
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable]
  public sealed class DummyEnumerable<T> : IEnumerable<T>
  {
    #region IEnumerable<T> Members

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new DummyEnumerator<T>();
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
      return new DummyEnumerator<T>();
    }

    #endregion
  }

  /// <summary>
  /// "�������������" ��� ������ ��������.
  /// �� ��������� ��������� ������� null, �� ����, ���� ������ ����� null, �� �� ��� ����� ����� ����������.
  /// ���� ��������� ������ �������������, ����������� DummyEnumerator.
  /// </summary>
  /// <typeparam name="T">��� ������������� �������</typeparam>
  [Serializable]
  public struct SingleObjectEnumerator<T> : IEnumerator<T>
  {
    #region �����������

    /// <summary>
    /// ������� ������������������� ��� ������ �������
    /// </summary>
    /// <param name="singleObject">������������ ������</param>
    public SingleObjectEnumerator(T singleObject)
    {
      _Object = singleObject;
      _Flag = false;
    }

    #endregion

    #region IEnumerator<T> Members

    private T _Object;
    private bool _Flag;

    /// <summary>
    /// ���������� ������� ��������
    /// </summary>
    public T Current { get { return _Object; } }

    /// <summary>
    /// ������ �� ������
    /// </summary>
    public void Dispose()
    {
    }

    object IEnumerator.Current
    {
      get { return _Object; }
    }

    /// <summary>
    /// ������� � ���������� ��������
    /// </summary>
    /// <returns>������� ��������</returns>
    public bool MoveNext()
    {
      if (_Flag)
        return false;
      else
      {
        _Flag = true;
        return true;
      }
    }

    void IEnumerator.Reset()
    {
      _Flag = false;
    }

    #endregion
  }


  /// <summary>
  /// "������������ ������" ��� ������ ��������.
  /// �� ��������� ��������� ������� null, �� ����, ���� ������ ����� null, �� �� ��� ����� ����� ����������.
  /// ���� ��������� ������ �������������, ����������� DummyEnumerable.
  /// </summary>
  /// <typeparam name="T">��� ������������� �������</typeparam>
  [Serializable]
  public sealed class SingleObjectEnumerable<T> : IEnumerable<T>
  {
    #region �����������

    /// <summary>
    /// ������� ������������������� ��� ������ �������
    /// </summary>
    /// <param name="singleObject">������������ ������</param>
    public SingleObjectEnumerable(T singleObject)
    {
      _Object = singleObject;
    }

    private T _Object;

    #endregion

    #region IEnumerable<T> Members

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new SingleObjectEnumerator<T>(_Object);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new SingleObjectEnumerator<T>(_Object);
    }

    #endregion
  }

  /// <summary>
  /// ����������� �������������, ������� �������� ������ �������������
  /// ������� �����.
  /// </summary>
  /// <typeparam name="T">��� ������������ ��������</typeparam>
  [Serializable]
  public abstract class GroupEnumerator<T> : IEnumerator<T>
  {
    #region �����������

    /// <summary>
    /// ����������� �������� ������
    /// </summary>
    public GroupEnumerator()
    {
      _GroupIndex = -1;
    }

    #endregion

    #region ����

    private int _GroupIndex;

    private IEnumerator<T> _CurrentGroup;

    #endregion

    #region IEnumerator<T> Members

    /// <summary>
    /// ���������� ������� ������
    /// </summary>
    public T Current
    {
      get
      {
        if (_CurrentGroup == null)
          return default(T);
        else
          return _CurrentGroup.Current;
      }
    }

    /// <summary>
    /// �������� Reset()
    /// </summary>
    public void Dispose()
    {
      Reset();
    }

    object IEnumerator.Current
    {
      get { return Current; }
    }

    /// <summary>
    /// ������� � ���������� ��������
    /// </summary>
    /// <returns>������� ��������</returns>
    public bool MoveNext()
    {
      while (true)
      {
        if (_CurrentGroup != null)
        {
          if (_CurrentGroup.MoveNext())
            return true;

          _CurrentGroup.Dispose();
          _CurrentGroup = null;
        }

        _GroupIndex++;
        _CurrentGroup = GetNextGroup(_GroupIndex);
        if (_CurrentGroup == null)
          return false;
      }
    }

    /// <summary>
    /// ���� ����� ������ ���� ������������� � ����������� ������.
    /// �� ������ ������� ��������� ������������� ��� null, ����� ������ ��� ��������������.
    /// </summary>
    /// <param name="groupIndex">������ ���������� �������������.
    /// ����� ��������������� ���������� � �������� 0,1,2,...</param>
    /// <returns>��������� ������������� ��� null</returns>
    protected abstract IEnumerator<T> GetNextGroup(int groupIndex);

    /// <summary>
    /// ��������� ������������� �������������
    /// </summary>
    public void Reset()
    {
      if (_CurrentGroup != null)
      {
        _CurrentGroup.Dispose();
        _CurrentGroup = null;
        _GroupIndex = -1;
      }
    }

    #endregion
  }

  /// <summary>
  /// ����������� �������������, ������� �� ������� �������� ������ �������������, �������� � �������.
  /// ���� ����� �������� ����� ��������, ��� GroupEnumerator, �.�. �������, ����� ��������� �������������
  /// ���� ������� �������.
  /// </summary>
  /// <typeparam name="T">��� ������������ ��������</typeparam>
  [Serializable]
  public sealed class GroupArrayEnumerator<T> : GroupEnumerator<T>
  {
    #region �����������

    /// <summary>
    /// ������� ������������� � �������� ������� ��������.
    /// </summary>
    /// <param name="groups">������������� ������ ��������������.
    /// ������ �� ����� ��������� �������� null, �.�. � ���� ������ ������������ ���������� ��������</param>
    public GroupArrayEnumerator(IEnumerator<T>[] groups)
    {
      if (groups == null)
        throw new ArgumentNullException("groups");
      _Groups = groups;
    }

    #endregion

    #region ������ �����

    private IEnumerator<T>[] _Groups;

    /// <summary>
    /// ���������� ��������� ������ �� ������
    /// </summary>
    /// <param name="groupIndex">������ ��������� ������</param>
    /// <returns>��������� ������������� ��� null, ���� ������ ����� ������</returns>
    protected override IEnumerator<T> GetNextGroup(int groupIndex)
    {
      if (groupIndex >= _Groups.Length)
        return null;
      else
        return _Groups[groupIndex];
    }

    #endregion
  }

  /// <summary>
  /// �������������� ������������� �� �������.
  /// ����� Array.GetEnumerator() ���������� ���������������� �������������
  /// </summary>
  /// <typeparam name="T">��� ��������� �������</typeparam>
  [Serializable]
  [StructLayout(LayoutKind.Auto)]
  public struct ArrayEnumerator<T> : IEnumerator<T>
  {
    #region �����������

    /// <summary>
    /// ������� ������������� ��� ����������� �������
    /// </summary>
    /// <param name="a">������</param>
    public ArrayEnumerator(T[] a)
    {
      if (a == null)
        throw new ArgumentNullException("a");
      _Array = a;
      _Index = -1;
    }

    private T[] _Array;
    private int _Index;

    #endregion

    #region IEnumerator<T> Members

    /// <summary>
    /// ���������� ������� ������� �������
    /// </summary>
    public T Current { get { return _Array[_Index]; } }

    /// <summary>
    /// ������ �� ������
    /// </summary>
    public void Dispose()
    {
    }

    object IEnumerator.Current { get { return _Array[_Index]; } }

    /// <summary>
    /// ��������� � ���������� �������� �������
    /// </summary>
    /// <returns></returns>
    public bool MoveNext()
    {
      _Index++;
      return _Index < _Array.Length;
    }

    /// <summary>
    /// ����� �������������
    /// </summary>
    void IEnumerator.Reset()
    {
      _Index = -1;
    }

    #endregion
  }

  /// <summary>
  /// ��������� ��������������� ������������� ��� �������
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public struct ArrayEnumerable<T> : IEnumerable<T>
  {
    #region �����������

    /// <summary>
    /// ������� ������ ��� �������
    /// </summary>
    /// <param name="a">������</param>
    public ArrayEnumerable(T[] a)
    {
      if (a == null)
        throw new ArgumentNullException("a");
      _Array = a;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������, �� �������� ����� ����������� ������������
    /// </summary>
    public T[] Array { get { return _Array; } }
    private T[] _Array;

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// ������� ����� ������������� ��� �������
    /// </summary>
    /// <returns>�������������</returns>
    public ArrayEnumerator<T> GetEnumerator()
    {
      return new ArrayEnumerator<T>(_Array);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new ArrayEnumerator<T>(_Array);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new ArrayEnumerator<T>(_Array);
    }

    #endregion
  }

  /// <summary>
  /// ������, ��� �������� ����� ������� �������� foreach.
  /// ���������� ������������� ��� �������� ��������� �������, ������ �� ������� ����� ������, �� ����������� ��������.
  /// ���������� ������������ �������� ��������� ArraySegment.
  /// ��� �������� ��� ��������, ����� ����������, ����� �������� ������.
  /// ���� ����� ������ ������� �����, ������������� �� ����� ������ �� ����.
  /// � ���� ����� �������� ����� ������ �������� ������������� �������.
  /// ���� ��� �������� ��������� ��������� ��������� ��� ������� ��������, ����������� ArrayBlockEnumerable.
  /// </summary>
  /// <typeparam name="T">��� ������, ���������� � �������</typeparam>
  [Serializable]
  public class ArraySegmentEnumerable<T> : IEnumerable<ArraySegment<T>>
  {
    #region �����������

    /// <summary>
    /// ������� ������������� ��� �������
    /// </summary>
    /// <param name="array">������������ ������</param>
    /// <param name="segmentSize">������ ��������. �� ����� ���� ������ 1</param>
    public ArraySegmentEnumerable(T[] array, int segmentSize)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      if (segmentSize < 1)
        throw new ArgumentOutOfRangeException("segmentSize");

      _Array = array;
      _SegmentSize = segmentSize;
    }

    #endregion

    #region ����

    private T[] _Array;
    private int _SegmentSize;

    #endregion

    #region IEnumerable<ArraySegment<T>> Members

    /// <summary>
    /// ������������� ��� �������� ��������� �������, ������ �� ������� ����� ������, �� ����������� ��������.
    /// ���������� ������������ �������� ��������� ArraySegment.
    /// ��� �������� ��� ��������, ����� ����������, ����� �������� ������.
    /// ���� ����� ������ ������� �����, ������������� �� ����� ������ �� ����.
    /// � ���� ����� �������� ����� ������ �������� ������������� �������.
    /// ���� ��� �������� ��������� ��������� ��������� ��� ������� ��������, ����������� ArrayBlockEnumerator.
    /// </summary>
    [Serializable]
    public struct Enumerator : IEnumerator<ArraySegment<T>>
    {
      #region �����������

      /// <summary>
      /// ������� ������������� ��� �������
      /// </summary>
      /// <param name="array">������������ ������</param>
      /// <param name="segmentSize">������ ��������. �� ����� ���� ������ 1</param>
      internal Enumerator(T[] array, int segmentSize)
      {

        _Array = array;
        _SegmentSize = segmentSize;
        _CurrentOffset = -1;
      }

      #endregion

      #region ����

      private T[] _Array;
      private int _SegmentSize;
      private int _CurrentOffset;

      #endregion

      #region IEnumerator<ArraySegment<T>> Members

      /// <summary>
      /// ���������� ������� �������
      /// </summary>
      public ArraySegment<T> Current
      {
        get
        {
          int count = Math.Min(_Array.Length - _CurrentOffset, _SegmentSize);
          return new ArraySegment<T>(_Array, _CurrentOffset, count);
        }
      }

      /// <summary>
      /// ������ �� ������
      /// </summary>
      public void Dispose()
      {
      }

      object IEnumerator.Current { get { return Current; } }

      /// <summary>
      /// ������� � ���������� ��������
      /// </summary>
      /// <returns>���� ������� Current</returns>
      public bool MoveNext()
      {
        if (_CurrentOffset < 0)
          _CurrentOffset = 0;
        else
          _CurrentOffset += _SegmentSize;

        return _CurrentOffset < _Array.Length;
      }

      /// <summary>
      /// ������������� ���������� ������������
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        _CurrentOffset = -1;
      }

      #endregion
    }

    /// <summary>
    /// ������� ������������� ArraySegmentEnumerator
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(_Array, _SegmentSize);
    }

    IEnumerator<ArraySegment<T>> IEnumerable<ArraySegment<T>>.GetEnumerator()
    {
      return new Enumerator(_Array, _SegmentSize);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(_Array, _SegmentSize);
    }

    #endregion
  }

  /// <summary>
  /// ������, ��� �������� ����� ������� �������� foreach.
  /// ���������� ������������� ��� �������� ������ �������, ������ �� ������� ����� ������, �� ����������� ��������.
  /// ���������� ������������ �������� ���������� ������� ������� �����.
  /// ��� �������� ��� ��������, ����� ����������, ����� �������� ������.
  /// ���� ����� ������ ������� �����, ������������� �� ����� ������ �� ����.
  /// � ���� ����� �������� ������ ������ �������� ������������� ����������.
  /// ���� ��� �������� �� ����� ��������� ��� ��������������� �������, ����� ���� �������� ������������ ArraySegmentEnumerator.
  /// </summary>
  /// <typeparam name="T">��� ������, ���������� � �������</typeparam>
  [Serializable]
  public class ArrayBlockEnumerable<T> : IEnumerable<T[]>
  {
    #region �����������

    /// <summary>
    /// ������� ������������� ��� �������
    /// </summary>
    /// <param name="array">������������ ������</param>
    /// <param name="segmentSize">������ ��������. �� ����� ���� ������ 1</param>
    public ArrayBlockEnumerable(T[] array, int segmentSize)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      if (segmentSize < 1)
        throw new ArgumentOutOfRangeException("segmentSize");

      _Array = array;
      _SegmentSize = segmentSize;
    }

    #endregion

    #region ����

    private T[] _Array;
    private int _SegmentSize;

    #endregion

    #region IEnumerable<ArraySegment<T>> Members

    /// <summary>
    /// ������������� ��� �������� ������ �������, ������ �� ������� ����� ������, �� ����������� ��������.
    /// ���������� ������������ �������� ���������� ������� ������� �����.
    /// ��� �������� ��� ��������, ����� ����������, ����� �������� ������.
    /// ���� ����� ������ ������� �����, ������������� �� ����� ������ �� ����.
    /// � ���� ����� �������� ������ ������ �������� ������������� ����������.
    /// ���� ��� �������� �� ����� ��������� ��� ��������������� �������, ����� ���� �������� ������������ ArraySegmentEnumerator.
    /// </summary>
    [Serializable]
    public struct Enumerator : IEnumerator<T[]>
    {
      #region �����������

      /// <summary>
      /// ������� ������������� ��� �������
      /// </summary>
      /// <param name="array">������������ ������</param>
      /// <param name="segmentSize">������ ��������. �� ����� ���� ������ 1</param>
      internal Enumerator(T[] array, int segmentSize)
      {

        _Array = array;
        _SegmentSize = segmentSize;
        _CurrentOffset = -1;
      }

      #endregion

      #region ����

      private T[] _Array;
      private int _SegmentSize;
      private int _CurrentOffset;

      #endregion

      #region IEnumerator<ArraySegment<T>> Members

      /// <summary>
      /// ���������� ������� �������
      /// </summary>
      public T[] Current
      {
        get
        {
          if (_CurrentOffset == 0 && _SegmentSize >= _Array.Length)
            return _Array;
          else
          {
            int count = Math.Min(_Array.Length - _CurrentOffset, _SegmentSize);
            T[] a = new T[count];
            Array.Copy(_Array, _CurrentOffset, a, 0, count);
            return a;
          }
        }
      }

      /// <summary>
      /// ������ �� ������
      /// </summary>
      public void Dispose()
      {
      }

      object IEnumerator.Current { get { return Current; } }

      /// <summary>
      /// ������� � ���������� ��������
      /// </summary>
      /// <returns>���� ������� Current</returns>
      public bool MoveNext()
      {
        if (_CurrentOffset < 0)
          _CurrentOffset = 0;
        else
          _CurrentOffset += _SegmentSize;

        return _CurrentOffset < _Array.Length;
      }

      /// <summary>
      /// ������������� ���������� ������������
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        _CurrentOffset = -1;
      }

      #endregion
    }

    /// <summary>
    /// ������� ������������� �� ������
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(_Array, _SegmentSize);
    }

    IEnumerator<T[]> IEnumerable<T[]>.GetEnumerator()
    {
      return new Enumerator(_Array, _SegmentSize);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(_Array, _SegmentSize);
    }

    #endregion
  }

  /// <summary>
  /// �������������, ����������� �������������� ���� ������.
  /// �������� �� ����� ���������������� ������������� � ���������� ��� ��� �������� ���������.
  /// ��� ������� ���������� �������� ����������� ��� �������������� � ��� <typeparamref name="T"/>
  /// � ������� ��������� "as". ���� �������������� ����������, ������� ������������ ������������
  /// </summary>
  /// <typeparam name="T">��� ������, ������� �������������. ������ ���� �������, � �� ����������</typeparam>
  [Serializable]
  public sealed class ConvertEnumerator<T> : IEnumerator<T>
    where T : class
  {
    #region �����������

    /// <summary>
    /// ������� �������������
    /// </summary>
    /// <param name="enumerator">���������������� �������������</param>
    public ConvertEnumerator(System.Collections.IEnumerator enumerator)
    {
      if (enumerator == null)
        throw new ArgumentNullException("enumerator");
      _Enumerator = enumerator;
    }

    #endregion

    #region ����

    /// <summary>
    /// ������������ �������������
    /// </summary>
    System.Collections.IEnumerator _Enumerator;

    /// <summary>
    /// ��������� ������� �������, ����� �������� ���������� ��������������
    /// </summary>
    T _Current;

    #endregion

    #region IEnumerator<T> Members

    /// <summary>
    /// ���������� ������� ������� ������������
    /// </summary>
    public T Current { get { return _Current; } }

    /// <summary>
    /// ���������� �������������
    /// </summary>
    public void Dispose()
    {
      IDisposable Disp = _Enumerator as IDisposable;
      if (Disp != null)
        Disp.Dispose();
      _Enumerator = null;
    }

    #endregion

    #region IEnumerator Members

    object IEnumerator.Current { get { return _Current; } }

    /// <summary>
    /// ������� � ���������� �������� ������������.
    /// </summary>
    /// <returns>true, ���� ���� ��������� �������</returns>
    public bool MoveNext()
    {
      while (_Enumerator.MoveNext())
      {
        _Current = _Enumerator.Current as T;
        if (_Current != null)
          return true;
      }
      return false;
    }

    /// <summary>
    /// ���������� �������������
    /// </summary>
    public void Reset()
    {
      _Enumerator.Reset();
    }

    #endregion
  }
}

namespace FreeLibSet.Data
{
  /// <summary>
  /// ����� ����� � ������� � ������������ ���������� �����, �������� � DataView.Sort.
  /// ��������� ������������� �� �������� ����� � ����������� ���������� �����.
  /// </summary>
  public sealed class DataViewRowPairEnumarable : IEnumerable<DataRow[]>
  {
    #region �����������

    /// <summary>
    /// ������� ������, ��� �������� ����� ������� �������� foreach.
    /// ����� ������������ ������ ������ � ������������ ���������� �����.
    /// ��� ��������� ����������� ��� �������, �������� � DataView.Sort
    /// </summary>
    /// <param name="dv">����� ������ � ������������� ��������� Sort</param>
    public DataViewRowPairEnumarable(DataView dv)
      : this(dv, 0, false)
    {
    }

    /// <summary>
    /// ������� ������, ��� �������� ����� ������� �������� foreach.
    /// </summary>
    /// <param name="dv">����� ������ � ������������� ��������� Sort</param>
    /// <param name="compareColumnCount">���������� ��������, ����������� � ���������.
    /// � ��������� ��������� ������ �������, �������� � DataView.Sort, �� ����� �������������� �� ��� �������.
    /// ������� �������� �������� ������������� ���� ��������.</param>
    public DataViewRowPairEnumarable(DataView dv, int compareColumnCount)
      : this(dv, compareColumnCount, false)
    {
    }

    /// <summary>
    /// ������� ������, ��� �������� ����� ������� �������� foreach.
    /// </summary>
    /// <param name="dv">����� ������ � ������������� ��������� Sort</param>
    /// <param name="compareColumnCount">���������� ��������, ����������� � ���������.
    /// � ��������� ��������� ������ �������, �������� � DataView.Sort, �� ����� �������������� �� ��� �������.
    /// ������� �������� �������� ������������� ���� ��������.</param>
    /// <param name="enumSingleRows">���� true, �� ����� ��������� ��� ������ � DataView, ������� ���������.
    /// ���� false, �� ����� ������������ ������ ������ � ������������ ���������� �����.</param>
    public DataViewRowPairEnumarable(DataView dv, int compareColumnCount, bool enumSingleRows)
    {
      if (dv == null)
        throw new ArgumentNullException("dv");
      if (String.IsNullOrEmpty(dv.Sort))
        throw new InvalidOperationException("�� ����������� �������� DataView.Sort");

      _DV = dv;
      string[] aColNames = DataTools.GetDataViewSortColumnNames(dv.Sort);

      if (compareColumnCount == 0)
        compareColumnCount = aColNames.Length;

      if (compareColumnCount < 1 || compareColumnCount >= aColNames.Length)
        throw new ArgumentOutOfRangeException("compareColumnCount", compareColumnCount, "���������� �������� ��� ��������� ������ ���� � ��������� �� 1 �� " + (aColNames.Length - 1).ToString());

      _ColPoss = new int[compareColumnCount];
      for (int i = 0; i < compareColumnCount; i++)
      {
        _ColPoss[i] = dv.Table.Columns.IndexOf(aColNames[i]);
        if (_ColPoss[i] < 0)
          throw new BugException("�� ������ ������� \"" + aColNames[i] + "\"");
      }
      _EnumSingleRows = enumSingleRows;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����� ������, �� �������� ����������� ������������
    /// </summary>
    public DataView DV { get { return _DV; } }
    private DataView _DV;

    /// <summary>
    /// ������ ������� ������� ��������, �������� � DataView.Sort.
    /// </summary>
    private int[] _ColPoss;

    /// <summary>
    /// ���� �� ���������� ��������� ������ (true) ��� ������ � ����������� ���������� (false).
    /// </summary>
    public bool EnumSingleRows { get { return _EnumSingleRows; } }
    private bool _EnumSingleRows;

    #endregion

    #region ���������� ������

    internal bool AreRowsEqual(DataRow row1, DataRow row2)
    {
      for (int i = 0; i < _ColPoss.Length; i++)
      {
        if (!AreValuesEqual(row1, row2, _ColPoss[i], !_DV.Table.CaseSensitive))
          //if (!DataTools.AreValuesEqual(row1, row2, _ColPoss[i]))
          return false;
      }
      return true;
    }
    /// <summary>
    /// ��������� �������� ������ ���� ��� ���� �����.
    /// ���������� �������� true, ���� �������� ���������. ���� ���� ������
    /// �������� DBNull, �� ������ ��������� �����������, ���� ��� ������ ��������
    /// DBNull
    /// </summary>
    /// <param name="row1">������ ������������ ������</param>
    /// <param name="row2">������ ������������ ������</param>
    /// <param name="columnPos">������� �������</param>
    /// <param name="ignoreCase">���� ������� ����� ��������� ���, �� ��������� ����������� ��� ����� ��������, ��� ��� ������ �������� ��� ������ � DataView</param>
    /// <returns>true, ���� �������� ���������</returns>
    private static bool AreValuesEqual(DataRow row1, DataRow row2, int columnPos, bool ignoreCase)
    {
      object x1 = row1[columnPos];
      object x2 = row2[columnPos];

      if (ignoreCase)
      {
        string s1 = x1 as String;
        string s2 = x2 as String;
        if (!(Object.ReferenceEquals(s1, null)) || Object.ReferenceEquals(s2, null))
          return String.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
      }

      return x1.Equals(x2);
    }

    #endregion

    #region �������������

    /// <summary>
    /// ���������� �������������.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct Enumerator : IEnumerator<DataRow[]>
    {
      #region �����������

      internal Enumerator(DataViewRowPairEnumarable owner)
      {
        _Owner = owner;
        _CurrIndex = -1;
        _CurrCount = 1;
      }

      #endregion

      #region ����

      /// <summary>
      /// ��������������� ����� ������
      /// </summary>
      private DataViewRowPairEnumarable _Owner;

      /// <summary>
      /// ������ ������ ������ � ����� ������������� �����
      /// </summary>
      private int _CurrIndex;

      /// <summary>
      /// ���������� ������������� �����. ������� 2, ���� �������
      /// </summary>
      private int _CurrCount;

      #endregion

      #region IEnumerator<DataRow[]> Members

      /// <summary>
      /// �������� �����.
      /// ��������� ����� ������������������ ���������� �����
      /// </summary>
      /// <returns>true, ���� ������� ��������� ���������� ������</returns>
      public bool MoveNext()
      {
        while ((_CurrIndex + _CurrCount) < _Owner._DV.Count)
        {
          _CurrIndex += _CurrCount;

          DataRow row0 = _Owner._DV[_CurrIndex].Row;
          _CurrCount = 1;

          while ((_CurrIndex + _CurrCount) < _Owner._DV.Count)
          {
            DataRow row2 = _Owner._DV[_CurrIndex + _CurrCount].Row;
            if (_Owner.AreRowsEqual(row0, row2))
              _CurrCount++;
            else
              break;
          }

          if (_CurrCount >= 2 || _Owner._EnumSingleRows)
            return true;
        }

        _CurrCount = 0;
        return false;
      }

      /// <summary>
      /// ���������� ������� ���� �� ���� ��� ����� ���������� �����, ���� EnumSingleRows=false.
      /// ��� EnumSingleRows=false ����� ������������ ������� �� ������ ��������.
      /// </summary>
      public DataRow[] Current
      {
        get
        {
          DataRow[] a = new DataRow[_CurrCount];
          for (int i = 0; i < a.Length; i++)
            a[i] = _Owner._DV[_CurrIndex + i].Row;
          return a;
        }
      }

      /// <summary>
      /// ������ �� ������
      /// </summary>
      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current { get { return Current; } }

      void System.Collections.IEnumerator.Reset()
      {
        _CurrIndex = -1;
        _CurrCount = 1;
      }

      #endregion
    }

    /// <summary>
    /// ���������� �������������
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<DataRow[]> IEnumerable<DataRow[]>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion
  }
}