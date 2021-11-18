using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using System.Threading;

namespace ExtTools.tests
{
  [TestFixture]
  public class WeakReferenceCollectionTests
  {
    private class TestObject
    {
      #region �����������

      public TestObject(int value)
      {
        _Value = value;
      }

      #endregion

      #region ��������

      public int Value { get { return _Value; } }
      private int _Value;

      public override string ToString()
      {
        return StdConvert.ToString(Value);
      }

      #endregion
    }

    [Test]
    public void Add()
    {
      WeakReferenceCollection<TestObject> sut = new WeakReferenceCollection<TestObject>();
      TestObject obj1 = new TestObject(1);
      TestObject obj2 = new TestObject(2);
      TestObject obj3 = new TestObject(3);
      sut.Add(obj1);
      sut.Add(obj2);
      sut.Add(obj3);

      GC.Collect();

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(3, sut.ToArray().Length, "ToArray.Length");
      int sum = 0;
      foreach (TestObject obj in sut)
        sum += obj.Value;
      Assert.AreEqual(6, sum, "Enumerable");


      GC.KeepAlive(obj1);
      GC.KeepAlive(obj2);
      GC.KeepAlive(obj3);
    }

    private class AsyncAddTester
    {
      #region �����������

      public const int ObjCount = 1000;

      public AsyncAddTester(WeakReferenceCollection<TestObject> sut, bool useGCCollect)
      {
        _SUT = sut;
        _UseGCCollect = useGCCollect;

        _Objs = new TestObject[ObjCount];
        for (int i = 0; i < _Objs.Length; i++)
          _Objs[i] = new TestObject(1);
      }

      #endregion

      #region ��������

      private WeakReferenceCollection<TestObject> _SUT;

      private bool _UseGCCollect;

      private TestObject[] _Objs;

      #endregion

      #region ����������

      public void Execute()
      {
        foreach (TestObject obj in _Objs)
          _SUT.Add(obj);

        if (_UseGCCollect)
          GC.Collect();
      }

      #endregion
    }

    [TestCase(false)]
    [TestCase(true)]
    [Repeat(50)]
    public void Add_async(bool useGCCollect)
    {
      WeakReferenceCollection<TestObject> sut = new WeakReferenceCollection<TestObject>();
      AsyncAddTester[] a = new AsyncAddTester[10];
      for (int i = 0; i < a.Length; i++)
        a[i] = new AsyncAddTester(sut, useGCCollect);

      // ����������� ����� Add()
      Thread[] trds = new Thread[a.Length];
      for (int i = 0; i < a.Length; i++)
        trds[i] = new Thread(a[i].Execute);
      for (int i = 0; i < a.Length; i++)
        trds[i].Start();

      // ������� ���������� ���� ��������
      for (int i = 0; i < a.Length; i++)
        trds[i].Join();

      Assert.AreEqual(a.Length * AsyncAddTester.ObjCount, sut.Count, "Count");
    }

    [Test]
    public void Clear()
    {
      WeakReferenceCollection<TestObject> sut = new WeakReferenceCollection<TestObject>();
      sut.Add(new TestObject(1));
      sut.Add(new TestObject(2));
      sut.Add(new TestObject(3));

      sut.Clear();
      Assert.AreEqual(0, sut.Count);
    }

    [Test]
    public void Remove()
    {
      WeakReferenceCollection<TestObject> sut = new WeakReferenceCollection<TestObject>();
      TestObject obj1 = new TestObject(1);
      TestObject obj2 = new TestObject(2);
      TestObject obj3 = new TestObject(3);

      sut.Add(obj1);
      sut.Add(obj2);
      sut.Add(obj3);

      bool res1 = sut.Remove(obj2);
      Assert.IsTrue(res1, "Remove ok");
      Assert.AreEqual(2, sut.Count, "Count");

      bool res2 = sut.Remove(obj2);
      Assert.IsFalse(res2, "Remove again");

      GC.KeepAlive(obj1);
      GC.KeepAlive(obj2);
      GC.KeepAlive(obj3);
    }

    [Test]
    public void Contains()
    {
      WeakReferenceCollection<TestObject> sut = new WeakReferenceCollection<TestObject>();
      TestObject obj1 = new TestObject(1);
      TestObject obj2 = new TestObject(2);
      sut.Add(obj1);

      Assert.IsTrue(sut.Contains(obj1), "Contained");
      Assert.IsFalse(sut.Contains(obj2), "Not contained");


      GC.KeepAlive(obj1);
      GC.KeepAlive(obj2);
    }

    [Test]
    public void Enumerable()
    {
      WeakReferenceCollection<TestObject> sut = new WeakReferenceCollection<TestObject>();
      TestObject[] a = new TestObject[10];
      int sum1 = 0;
      for (int i = 0; i < a.Length; i++)
      {
        a[i] = new TestObject(i);
        sum1 += a[i].Value;
        sut.Add(a[i]);
      }

      int sum2 = 0;
      foreach (TestObject obj in sut)
      {
        sum2 += obj.Value;
        if (obj.Value == 4)
          GC.Collect();
      }

      Assert.AreEqual(sum2, sum1);

      GC.KeepAlive(a);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [Repeat(10)]
    public void Vanish_reference(int checkedItem)
    {
      // ��������� ������������ ������ �� ������
      // ������ �������� ��� ������, ����� �����������.
      // ��� ������ WeakReferenceCollection.ToArray() ����������� �������������� ������� ������, ������� ������ ��������� ��������, ���������� �� ������� ��������� ���������

      WeakReferenceCollection<TestObject> sut = new WeakReferenceCollection<TestObject>();
      TestObject obj = DoAddRefs(sut, checkedItem); // ��������� �����, ����� ����� ���� �������� ������

      // ������ ��������
      GC.Collect();

      TestObject[] a = sut.ToArray();
      Assert.AreEqual(1, a.Length, "ToArray().Length");
      Assert.AreSame(obj, a[0], "Item");
    }

    private TestObject DoAddRefs(WeakReferenceCollection<TestObject> sut, int checkedItem)
    {
      TestObject res = null;
      for (int i = 0; i < 4; i++)
      {
        TestObject obj = new TestObject(i);
        sut.Add(obj);
        if (i == checkedItem)
          res = obj;
      }

      if (res == null)
        throw new ArgumentException();

      return res;
    }
  }
}
