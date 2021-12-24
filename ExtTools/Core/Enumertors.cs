// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace FreeLibSet.Core
{
  /// <summary>
  /// ��������� ��������� IEnumerable ��� ���������, �� ���������� ���������
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable]
  public sealed class DummyEnumerable<T> : IEnumerable<T>
  {
    /// <summary>
    /// ������ �������������.
    /// ��������� ��������� IEnumerator ��� ���������, �� ���������� ���������
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
      #region IEnumerator<T> Members

      /// <summary>
      /// ���������� ������ ��������
      /// </summary>
      public T Current
      {
        get { return default(T); }
      }

      /// <summary>
      /// ������ �� ������
      /// </summary>
      public void Dispose()
      {
      }

      object IEnumerator.Current
      {
        get { return null; }
      }

      /// <summary>
      /// ������ ���������� false
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        return false;
      }

      void IEnumerator.Reset()
      {
      }

      #endregion
    }

    #region IEnumerable<T> Members

    /// <summary>
    /// ���������� ��������� �������������
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new Enumerator();
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator();
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

    /// <summary>
    /// "�������������" ��� ������ ��������.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
      #region �����������

      /// <summary>
      /// ������� ������������������� ��� ������ �������
      /// </summary>
      /// <param name="singleObject">������������ ������</param>
      public Enumerator(T singleObject)
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
      /// ������� � ���������� ��������.
      /// ��� ������ ������ ���������� true, ��� ������ - false.
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

    #region IEnumerable<T> Members

    /// <summary>
    /// ������� ����� ������������� �� �������
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(_Object);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new Enumerator(_Object);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(_Object);
    }

    #endregion
  }

  /// <summary>
  /// ���������� ������� ��� ��������, ����������� ��������� IEnumerator.
  /// ��������� ��������� IEnumerable, ����� ����� ���� ��������� �������� foreach.
  /// ��������� �������� "�����������", ��� ��� ��������� �������� foreach ������� �� ��� �� ��������� �������������.
  /// </summary>
  /// <typeparam name="T">��� �������������</typeparam>
  [Serializable]
  public struct EnumerableWrapper<T> : IEnumerable<T>
  {
    #region �����������

    /// <summary>
    /// �������������� ������
    /// </summary>
    /// <param name="enumerator">�������������. �� ����� ���� null</param>
    public EnumerableWrapper(IEnumerator<T> enumerator)
    {
      if (enumerator == null)
        throw new ArgumentNullException("enumerator");
      _Enumerator = enumerator;
    }

    #endregion

    #region GetEnumerator()

    private IEnumerator<T> _Enumerator;

    /// <summary>
    /// ���������� �������������, ���������� ������������.
    /// ��������� ����� �������� ����������.
    /// </summary>
    /// <returns>�������������</returns>
    public IEnumerator<T> GetEnumerator()
    {
      if (_Enumerator == null)
        throw new InvalidOperationException("��������� ����� ������������� �� �����������");
      IEnumerator<T> res = _Enumerator;
      _Enumerator = null;
      return res;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
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
    public virtual void Dispose()
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
  public sealed class GroupArrayEnumerable<T> : IEnumerable<T>
  {
    #region �����������

    public GroupArrayEnumerable(IEnumerable<T>[] groups)
    {
      if (groups == null)
        throw new ArgumentNullException("groups");
      _Groups = groups;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������ �����
    /// </summary>
    public IEnumerable<T>[] Groups { get { return _Groups; } }
    private IEnumerable<T>[] _Groups;

    #endregion

    /// <summary>
    /// ����������� �������������, ������� �� ������� �������� ������ �������������, �������� � �������.
    /// ���� ����� �������� ����� ��������, ��� GroupEnumerator, �.�. �������, ����� ��������� �������������
    /// ���� ������� �������.
    /// </summary>
    [Serializable]
    public sealed class Enumerator : GroupEnumerator<T>
    {
      #region �����������

      /// <summary>
      /// ������� ������������� � �������� ������� ��������.
      /// </summary>
      /// <param name="groups">������������� ������ ��������������.
      /// ������ �� ����� ��������� �������� null, �.�. � ���� ������ ������������ ���������� ��������</param>
      public Enumerator(IEnumerator<T>[] groups)
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

      /// <summary>
      /// �������� Dispose() ��� ���� ��������������
      /// </summary>
      public override void Dispose()
      {
        base.Dispose();
        for (int i = 0; i < _Groups.Length; i++)
          _Groups[i].Dispose(); // ����� ���� ��������� �����
      }

      #endregion
    }

    #region GetEnumerator()

    public Enumerator GetEnumerator()
    {
      IEnumerator<T>[] a = new IEnumerator<T>[_Groups.Length];
      for (int i = 0; i < _Groups.Length; i++)
        a[i] = _Groups[i].GetEnumerator();

      return new Enumerator(a);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
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

    /// <summary>
    /// �������������� ������������� �� �������.
    /// ����� Array.GetEnumerator() ���������� ���������������� �������������
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
      #region �����������

      /// <summary>
      /// ������� ������������� ��� ����������� �������
      /// </summary>
      /// <param name="a">������</param>
      public Enumerator(T[] a)
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

    #region IEnumerable<T> Members

    /// <summary>
    /// ������� ����� ������������� ��� �������
    /// </summary>
    /// <returns>�������������</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(_Array);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new Enumerator(_Array);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(_Array);
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
