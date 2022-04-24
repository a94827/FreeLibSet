using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Collections;
using FreeLibSet.Remoting;
using FreeLibSet.Core;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  public class SingleScopeSortedListTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_simple()
    {
      SingleScopeSortedList<int> sut = new SingleScopeSortedList<int>();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(Comparer<int>.Default, sut.Comparer, "Comparer");
    }

    [Test]
    public void Constructor_collection()
    {
      SingleScopeSortedList<int> sut = new SingleScopeSortedList<int>(new int[] { 3, 1, 2 });
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(new int[] { 1, 2, 3 }, sut.ToArray(), "ToArray()");
    }

    /// <summary>
    /// Класс сравнения целых чисел по модулю
    /// </summary>
    private class IntAbsComparer : IComparer<int>
    {
      #region IComparer<int> Members

      public int Compare(int x, int y)
      {
        return Math.Abs(x).CompareTo(Math.Abs(y));
      }

      #endregion
    }

    [Test]
    public void Constructor_comparer()
    {
      IComparer<int> comparer = new IntAbsComparer();
      SingleScopeSortedList<int> sut = new SingleScopeSortedList<int>(comparer);
      Assert.AreSame(comparer, sut.Comparer, "Comparer");

      sut.Add(2);
      sut.Add(1);
      sut.Add(-1);
      Assert.AreEqual(2, sut.Count);
      Assert.AreEqual(new int[]{1,2}, sut.ToArray());
    }

    #endregion

    #region Item

    private static SingleScopeSortedList<int> CreateTestObject(bool useComparer)
    {
      SingleScopeSortedList<int> sut;
      if (useComparer)
        sut = new SingleScopeSortedList<int>(new IntAbsComparer());
      else
        sut = new SingleScopeSortedList<int>();

      sut.Add(2);
      sut.Add(1);
      sut.Add(3);

      return sut;
    }

    [Test]
    public void Item_get()
    {
      SingleScopeSortedList<int> sut = CreateTestObject(false);
      Assert.AreEqual(1, sut[0], "0");
      Assert.AreEqual(2, sut[1], "1");
      Assert.AreEqual(3, sut[2], "2");

      int dummy;
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[-1]; }, "-1");
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[3]; }, "3");
    }

    //[Test]
    //public void Item_set()
    //{
    //  SingleScopeSortedList<int> sut = CreateTestObject(false);
    //  Assert.Catch(delegate() { sut[1] = 10; });
    //}

    #endregion

    #region Add/Remove()

    [Test]
    public void Add()
    {
      SingleScopeSortedList<int> sut = CreateTestObject(true);

      sut.Add(-4);
      Assert.AreEqual(new int[] { 1, 2, 3, -4 }, sut.ToArray(), "#1 added");

      sut.Add(-1);
      Assert.AreEqual(new int[] { 1, 2, 3, -4 }, sut.ToArray(), "#2 unchanged");
    }


    [Test]
    public void AddRange()
    {
      SingleScopeSortedList<int> sut = CreateTestObject(true);
      sut.AddRange(new int[] { 1, -4, -1 });
      Assert.AreEqual(new int[] { 1, 2, 3, -4 }, sut.ToArray());
    }

    [TestCase(1, false, true, "2,3")]
    [TestCase(-1, false, false, "1,2,3")]
    [TestCase(-1, true, true, "2,3")]
    public void Remove(int value, bool useComparer, bool wantedRes, string sWantedArray)
    {
      SingleScopeSortedList<int> sut = CreateTestObject(useComparer);
      bool res = sut.Remove(value);
      Assert.AreEqual(wantedRes, res, "Result");
      Assert.AreEqual(StdConvert.ToInt32Array(sWantedArray), sut.ToArray(), "ToArray()");
    }

    [Test]
    public void RemoveAt()
    {
      SingleScopeSortedList<int> sut = CreateTestObject(false);
      sut.RemoveAt(1);
      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual(new int[] { 1, 3 }, sut.ToArray(), "ToArray() #1");

      Assert.Catch<ArgumentOutOfRangeException>(delegate() { sut.RemoveAt(2); }, "out of range");
      Assert.AreEqual(new int[] { 1, 3 }, sut.ToArray(), "ToArray() #2");
    }

    #endregion

    #region Contains()

    [TestCase(2, false, 1)]
    [TestCase(-2, false, -1)]
    [TestCase(-2, true, 1)]
    public void IndexOf_Contains(int value, bool useComparer, int wanted)
    {
      SingleScopeSortedList<int> sut = CreateTestObject(useComparer);

      int res1 = sut.IndexOf(value);
      Assert.AreEqual(wanted, res1, "IndexOf()");

      bool res2 = sut.Contains(value);
      Assert.AreEqual(wanted >= 0, res2, "Contains()");
    }

    [TestCase("1,2,3", false, true)]
    [TestCase("1,2,4", false, false)]
    [TestCase("2,-1", false, false)]
    [TestCase("2,-1", true, true)]
    [TestCase("", false, true)]
    public void ContainsAll(string sValues, bool useComparer, bool wanted)
    {
      SingleScopeSortedList<int> sut = CreateTestObject(useComparer);
      int[] values = StdConvert.ToInt32Array(sValues);
      bool res = sut.ContainsAll(values);
      Assert.AreEqual(wanted, res);
    }

    [Test]
    public void ContainsAll_EmptyLlst()
    {
      SingleScopeSortedList<int> sut = new SingleScopeSortedList<int>();
      Assert.IsTrue(sut.ContainsAll(DataTools.EmptyInts), "{}");
      Assert.IsFalse(sut.ContainsAll(new int[] { 1 }), "{1}");
    }

    [TestCase("1,4", false, true)]
    [TestCase("-2,4,-4", false, false)]
    [TestCase("-2,4,-4", true, true)]
    [TestCase("", false, false)]
    public void ContainsAny(string sValues, bool useComparer, bool wanted)
    {
      SingleScopeSortedList<int> sut = CreateTestObject(useComparer);
      int[] values = StdConvert.ToInt32Array(sValues);
      bool res = sut.ContainsAny(values);
      Assert.AreEqual(wanted, res);
    }

    [Test]
    public void ContainsAny_EmptyLlst()
    {
      SingleScopeSortedList<int> sut = new SingleScopeSortedList<int>();
      Assert.IsFalse(sut.ContainsAny(DataTools.EmptyInts), "{}");
      Assert.IsFalse(sut.ContainsAny(new int[] { 1 }), "{1}");
    }

    [TestCase("3,2,1", false, true)]
    [TestCase("-2,1,3", false, false)]
    [TestCase("-2,1,3", true, true)]
    [TestCase("", false, false)]
    public void ContainsSame(string sValues, bool useComparer, bool wanted)
    {
      SingleScopeSortedList<int> sut = CreateTestObject(useComparer);
      int[] values = StdConvert.ToInt32Array(sValues);
      bool res = sut.ContainsSame(values);
      Assert.AreEqual(wanted, res);
    }

    [Test]
    public void ContainsSame_EmptyLlst()
    {
      SingleScopeSortedList<int> sut = new SingleScopeSortedList<int>();
      Assert.IsTrue(sut.ContainsSame(DataTools.EmptyInts), "{}");
      Assert.IsFalse(sut.ContainsSame(new int[] { 1 }), "{1}");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void Clear()
    {
      SingleScopeSortedList<int> sut = CreateTestObject(false);
      sut.Clear();
      Assert.AreEqual(0, sut.Count);
    }

    [Test]
    public void CopyTo()
    {
      SingleScopeSortedList<int> sut = CreateTestObject(false);
      int[] a = new int[5];
      sut.CopyTo(a, 1);
      Assert.AreEqual(new int[] { 0, 1, 2, 3, 0 }, a);
    }

    [Test]
    public void ToArray()
    {
      SingleScopeSortedList<int> sut = new SingleScopeSortedList<int>(new int[] { 1, -4, 2, 4 }, new IntAbsComparer());
      int[] a = sut.ToArray();
      Assert.AreEqual(new int[] { 1, 2, -4 }, a);
    }

    #endregion

    #region Перечислитель

    [Test]
    public void GetEnumerator()
    {
      SingleScopeSortedList<int> sut = CreateTestObject(false);
      List<int> lst = new List<int>();
      foreach (int item in sut)
        lst.Add(item);
      Assert.AreEqual(new int[] { 1, 2, 3 }, lst.ToArray());
    }


    #endregion

    #region SetReadOnly()

    private class SingleScopeSortedList_RO<T> : SingleScopeSortedList<T>
    {
      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }
    }

    [Test]
    public void SetReadOnly()
    {
      SingleScopeSortedList_RO<string> sut = new SingleScopeSortedList_RO<string>();
      sut.Add("AAA");
      sut.Add("BBB");
      sut.CheckNotReadOnly();

      sut.SetReadOnly();

      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch(delegate() { sut.Add("DDD"); }, "Add()");
      Assert.Catch(delegate() { sut.AddRange(new string[] { "DDD", "EEE" }); }, "AddRange()");
      Assert.Catch(delegate() { sut.RemoveAt(1); }, "RemoveAt()");
      Assert.Catch(delegate() { sut.Remove("AAA"); }, "Remove()");
      Assert.Catch(delegate() { sut.Clear(); }, "Clear()");

      Assert.AreEqual(2, sut.Count, "Count");
    }

    #endregion
  }
}
