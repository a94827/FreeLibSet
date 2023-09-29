using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.Remoting;
using System.Collections;
using FreeLibSet.Tests;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  public class DisposableDictionaryTests
  {
    #region Тестовый класс значения

    private class TestValue : IDisposable
    {
      public TestValue(int value)
      {
        _Value = value;
      }

      public void Dispose()
      {
        _IsDisposed = true;
      }

      public int Value { get { return _Value; } }
      private int _Value;

      public bool IsDisposed { get { return _IsDisposed; } }
      private bool _IsDisposed;

      public override string ToString()
      {
        return Value.ToString();
      }
    }

    private static readonly TestValue[] EmptyTestValues = new TestValue[0];

    #endregion

    #region Конструкторы

    [Test]
    public void Constructor_default()
    {
      DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>();
      DoTestConstructor_default(sut);
    }

    [Test]
    public void Constructor_capacity()
    {
      DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>(10);
      DoTestConstructor_default(sut);
    }

    private static void DoTestConstructor_default(DisposableDictionary<int, TestValue> sut)
    {
      Assert.AreEqual(0, sut.Count, "Count");
      //Assert.IsFalse(sut.DisposeOnDestuction, "DisposeOnDestruction");
      CollectionAssert.AreEqual(DataTools.EmptyInts, sut.Keys, "Keys");
      CollectionAssert.AreEqual(EmptyTestValues, sut.Values, "Values");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.DoesNotThrow(delegate () { sut.CheckNotDisposed(); }, "CheckNotDisposed");
      Assert.DoesNotThrow(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly");
    }

    private class TestIntComparer : IEqualityComparer<int>
    {
      public bool Equals(int x, int y)
      {
        return (x % 10) == (y % 10);
      }

      public int GetHashCode(int obj)
      {
        return obj % 10;
      }
    }

    [Test]
    public void Constructor_comparer()
    {
      DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>(new TestIntComparer());
      DoTestConstructor_comparer(sut);
    }

    [Test]
    public void Constructor_capacity_comparer()
    {
      DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>(5, new TestIntComparer());
      DoTestConstructor_comparer(sut);
    }

    private static void DoTestConstructor_comparer(DisposableDictionary<int, TestValue> sut)
    {
      DoTestConstructor_default(sut);

      sut[1] = new TestValue(123);
      sut[2] = new TestValue(456);
      sut[11] = new TestValue(789); // заменяет значение 1

      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual(789, sut[21].Value, "[21]");
      Assert.AreEqual(456, sut[22].Value, "[22]");
    }

    [Test]
    public void Constructor_dict()
    {
      Dictionary<int, TestValue> dict = new Dictionary<int, TestValue>();
      dict.Add(1, new TestValue(123));
      dict.Add(2, new TestValue(456));

      DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>(dict);
      DoTestConstructor_dict(sut, false);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Constructor_dict_isReadOnly(bool isReadOnly)
    {
      Dictionary<int, TestValue> dict = new Dictionary<int, TestValue>();
      dict.Add(1, new TestValue(123));
      dict.Add(2, new TestValue(456));

      DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>(dict, isReadOnly);
      DoTestConstructor_dict(sut, isReadOnly);
    }

    private static void DoTestConstructor_dict(DisposableDictionary<int, TestValue> sut, bool isReadOnly)
    {
      Assert.AreEqual(2, sut.Count, "Count");
      //Assert.IsFalse(sut.DisposeOnDestuction, "DisposeOnDestruction");
      CollectionAssert.AreEquivalent(new int[] { 1, 2 }, sut.Keys, "Keys");
      Assert.AreEqual(isReadOnly, sut.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region Dispose()

    public void Dispose()
    {
      DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>();

      TestValue v1 = new TestValue(123);
      TestValue v2 = new TestValue(456);
      sut.Add(1, v1);
      sut.Add(1, v2);

      sut.Dispose();

      Assert.IsTrue(v1.IsDisposed, "#1");
      Assert.IsTrue(v2.IsDisposed, "#2");

      Assert.Catch<ObjectDisposedException>(delegate () { sut.Add(1, new TestValue(111)); }, "Add()");
      Assert.Catch<ObjectDisposedException>(delegate () { sut[1] = new TestValue(111); }, "Item set");
      Assert.Catch<ObjectDisposedException>(delegate () { sut.Remove(1); }, "Remove()");
      Assert.Catch<ObjectDisposedException>(delegate () { sut.Clear(); }, "Clear()");
    }

    #endregion

    //#region DisposingOnDestruction

    //[TestCase(false)]
    //[TestCase(true)]
    //public void DisposeOnDestruction(bool disposeOnDestruction)
    //{

    //  TestValue v1 = new TestValue(123);
    //  TestValue v2 = new TestValue(456);
    //  DisposeOnDestruction_CreateObj(disposeOnDestruction, v1, v2);
    //  TestTools.GCCollect(); // Destructor must be called

    //  Assert.AreEqual(disposeOnDestruction, v1.IsDisposed, "#1");
    //  Assert.AreEqual(disposeOnDestruction, v2.IsDisposed, "#2");
    //}

    //private static void DisposeOnDestruction_CreateObj(bool disposeOnDestruction, TestValue v1, TestValue v2)
    //{
    //  DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>();
    //  sut.DisposeOnDestuction = disposeOnDestruction;
    //  sut.Add(1, v1);
    //  sut.Add(2, v2);

    //  Assert.AreEqual(disposeOnDestruction, sut.DisposeOnDestuction, "DisposeOnDestruction");
    //}

    //#endregion

    #region Item

    public void Item()
    {
      DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>();
      TestValue v1 = new TestValue(111);
      sut[1] = v1;
      TestValue v2 = new TestValue(222);
      sut[2] = v2;
      TestValue v3 = new TestValue(333);
      sut[1] = v3; // замена

      Assert.AreEqual(2, sut.Count, "Count");
      CollectionAssert.AreEquivalent(new int[] { 1, 2 }, sut.Keys, "Keys");
      CollectionAssert.AreEquivalent(new TestValue[] { v2, v3 }, sut.Values, "Values");
      Assert.AreEqual(v3, sut[1], "[1]");
      Assert.AreEqual(v2, sut[2], "[2]");

      Assert.IsTrue(v1.IsDisposed, "Disposed() #1");
      Assert.IsFalse(v2.IsDisposed, "Disposed() #2");
      Assert.IsFalse(v3.IsDisposed, "Disposed() #3");
    }

    #endregion

    #region Add()/Remove()

    [Test]
    public void Add()
    {
      DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>();
      TestValue v1 = new TestValue(111);
      sut.Add(1, v1);
      TestValue v2 = new TestValue(222);
      sut.Add(2, v2);

      TestValue v3 = new TestValue(333);
      Assert.Catch(delegate () { sut.Add(1, v3); }, "Adding same key");

      Assert.AreEqual(2, sut.Count, "Count");
      CollectionAssert.AreEquivalent(new int[] { 1, 2 }, sut.Keys, "Keys");
      CollectionAssert.AreEquivalent(new TestValue[] { v1, v2 }, sut.Values, "Values");
      Assert.AreEqual(v1, sut[1], "[1]");
      Assert.AreEqual(v2, sut[2], "[2]");

      Assert.IsFalse(v1.IsDisposed, "Disposed() #1");
      Assert.IsFalse(v2.IsDisposed, "Disposed() #2");
      Assert.IsFalse(v3.IsDisposed, "Disposed() #3");
    }

    [Test]
    public void Remove()
    {
      DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>();
      TestValue v1 = new TestValue(111);
      sut.Add(1, v1);
      TestValue v2 = new TestValue(222);
      sut.Add(2, v2);

      bool res1 = sut.Remove(1);
      Assert.IsTrue(res1, "Result #1");
      bool res2 = sut.Remove(3);
      Assert.IsTrue(res1, "Result #2");

      Assert.AreEqual(1, sut.Count, "Count");
      CollectionAssert.AreEquivalent(new int[] { 2 }, sut.Keys, "Keys");
      CollectionAssert.AreEquivalent(new TestValue[] { v2 }, sut.Values, "Values");
      Assert.AreEqual(v2, sut[2], "[2]");

      Assert.IsTrue(v1.IsDisposed, "Disposed() #1");
      Assert.IsFalse(v2.IsDisposed, "Disposed() #2");
    }

    #endregion

    #region Clear()

    public void Clear()
    {
      DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>();
      TestValue v1 = new TestValue(111);
      sut.Add(1, v1);
      TestValue v2 = new TestValue(222);
      sut.Add(2, v2);

      sut.Clear();

      Assert.AreEqual(0, sut.Count, "Count");
      CollectionAssert.AreEqual(DataTools.EmptyInts, sut.Keys, "Keys");
      CollectionAssert.AreEquivalent(EmptyTestValues, sut.Values, "Values");

      Assert.IsTrue(v1.IsDisposed, "Disposed() #1");
      Assert.IsTrue(v2.IsDisposed, "Disposed() #2");
    }

    #endregion

    #region Прочие методы

    [TestCase(1, true, 111)]
    [TestCase(2, true, 222)]
    [TestCase(3, false, 0)]
    public void ContainsKey_TryGetValue(int key, bool wantedRes, int wantedValue)
    {
      DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>();
      TestValue v1 = new TestValue(111);
      sut.Add(1, v1);
      TestValue v2 = new TestValue(222);
      sut.Add(2, v2);

      bool res1 = sut.ContainsKey(key);
      Assert.AreEqual(wantedRes, res1, "ContainsKey() result");

      TestValue val2;
      bool res2 = sut.TryGetValue(key, out val2);
      Assert.AreEqual(wantedRes, res2, "TryGetValue() result");
      if (wantedRes)
        Assert.AreEqual(wantedValue, val2.Value, "TryGetValue() value");

      Assert.IsFalse(v1.IsDisposed, "Disposed() #1");
      Assert.IsFalse(v2.IsDisposed, "Disposed() #2");
    }

    private class KeyValuePairComparer : IComparer<KeyValuePair<int, TestValue>>
    {
      public int Compare(KeyValuePair<int, TestValue> x, KeyValuePair<int, TestValue> y)
      {
        return x.Key.CompareTo(y.Key);
      }
    }

    [Test]
    public void CopyTo()
    {
      DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>();
      TestValue v1 = new TestValue(111);
      sut.Add(1, v1);
      TestValue v2 = new TestValue(222);
      sut.Add(2, v2);

      KeyValuePair<int, TestValue>[] a = new KeyValuePair<int, TestValue>[5];
      sut.CopyTo(a, 2);

      Array.Sort<KeyValuePair<int, TestValue>>(a, 2, 2, new KeyValuePairComparer()); // для определенности порядка сортировки

      KeyValuePair<int, TestValue>[] wanted = new KeyValuePair<int, TestValue>[5];
      wanted[2] = new KeyValuePair<int, TestValue>(1, v1);
      wanted[3] = new KeyValuePair<int, TestValue>(2, v2);
      CollectionAssert.AreEqual(wanted, a, "Array");

      Assert.IsFalse(v1.IsDisposed, "Disposed() #1");
      Assert.IsFalse(v2.IsDisposed, "Disposed() #2");
    }

    #endregion

    #region GetEnumerator()

    [Test]
    public void GetEnumerator()
    {
      DisposableDictionary<int, TestValue> sut = new DisposableDictionary<int, TestValue>();
      TestValue v1 = new TestValue(111);
      sut.Add(1, v1);
      TestValue v2 = new TestValue(222);
      sut.Add(2, v2);

      List<KeyValuePair<int, TestValue>> lst = new List<KeyValuePair<int, TestValue>>();
      foreach (KeyValuePair<int, TestValue> pair in sut)
        lst.Add(pair);

      KeyValuePair<int, TestValue>[] wanted = new KeyValuePair<int, TestValue>[2];
      wanted[0] = new KeyValuePair<int, TestValue>(1, v1);
      wanted[1] = new KeyValuePair<int, TestValue>(2, v2);
      CollectionAssert.AreEqual(wanted, lst, "List");

      Assert.IsFalse(v1.IsDisposed, "Disposed() #1");
      Assert.IsFalse(v2.IsDisposed, "Disposed() #2");
    }

    #endregion

    #region SetReadOnly()

    private class TestDictionary : DisposableDictionary<int, TestValue>
    {
      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }
    }

    [Test]
    public void SetReadOnly()
    {
      TestDictionary sut = new TestDictionary();

      TestValue v1 = new TestValue(111);
      sut.Add(1, v1);
      TestValue v2 = new TestValue(222);
      sut.Add(2, v2);

      sut.SetReadOnly();

      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.DoesNotThrow(delegate () { sut.SetReadOnly(); }, "SetReadOnly() again");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Add(1, new TestValue(111)); }, "Add()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut[1] = new TestValue(111); }, "Item set");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Remove(1); }, "Remove()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Clear(); }, "Clear()");

      Assert.AreEqual(2, sut.Count, "Count");
      CollectionAssert.AreEquivalent(new int[] { 1, 2 }, sut.Keys, "Keys");
      CollectionAssert.AreEquivalent(new TestValue[] { v1, v2 }, sut.Values, "Values");
      //Assert.IsFalse(sut.DisposeOnDestuction, "DisposeOnDestruction");

      Assert.IsFalse(v1.IsDisposed, "Disposed() #1 before");
      Assert.IsFalse(v2.IsDisposed, "Disposed() #2 before");

      sut.Dispose();

      Assert.IsTrue(v1.IsDisposed, "Disposed() #1 after");
      Assert.IsTrue(v2.IsDisposed, "Disposed() #2 after");
    }

    #endregion

    //#region Сериализация

    //[Test]
    //public void Serialization()
    //{
    //  DisposableDictionary<int, TestValue> sut1 = new DisposableDictionary<int, TestValue>();
    //  TestValue v1 = new TestValue(111);
    //  sut1.Add(1, v1);
    //  TestValue v2 = new TestValue(222);
    //  sut1.Add(2, v2);

    //  byte[] bytes = SerializationTools.SerializeBinary(sut1);

    //  DisposableDictionary<int, TestValue> sut2 = (DisposableDictionary<int, TestValue>)(SerializationTools.DeserializeBinary(bytes));
    //  Assert.AreEqual(sut1.Count, sut2.Count, "Count");
    //  CollectionAssert.AreEquivalent(new int[] { 1, 2 }, sut2.Keys, "Keys");
    //  CollectionAssert.AreEquivalent(new TestValue[] { v1, v2 }, sut2.Values, "Values");
    //}

    //#endregion
  }
}
