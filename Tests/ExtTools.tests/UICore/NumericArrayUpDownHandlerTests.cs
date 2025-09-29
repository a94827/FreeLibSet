using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.UICore;

namespace ExtTools_tests.UICore
{
  [TestFixture]
  class NumericArrayUpDownHandlerTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Array_NoSort()
    {
      int[] a = new int[] { 1, 2, 5, 10 };
      NumericArrayUpDownHandler<int> sut = new NumericArrayUpDownHandler<int>(a, false);
      CollectionAssert.AreEqual(a, sut.Items, "Items");
    }

    [Test]
    public void Constructor_Array_Sort()
    {
      int[] a = new int[] { 5, 2, 10, 1 };
      NumericArrayUpDownHandler<int> sut = new NumericArrayUpDownHandler<int>(a, true);

      int[] wanted = new int[] { 1, 2, 5, 10 };
      CollectionAssert.AreEqual(wanted, sut.Items, "Items");
    }

    private class ReverseComparer : IComparer<int>
    {
      public int Compare(int x, int y)
      {
        return -x.CompareTo(y);
      }
    }

    [Test]
    public void Constructor_Array_Comparer()
    {
      int[] a = new int[] { 5, 2, 10, 1 };
      ReverseComparer comparer = new ReverseComparer();
      NumericArrayUpDownHandler<int> sut = new NumericArrayUpDownHandler<int>(a, true, comparer);

      int[] wanted = new int[] { 10, 5, 2, 1 };
      CollectionAssert.AreEqual(wanted, sut.Items, "Items");
      Assert.AreSame(comparer, sut.Comparer, "Comparer");
    }

    [Test]
    public void Constructor_Enumarable()
    {
      List<int> lst= new List<int>();
      lst.Add(5);
      lst.Add(2);
      lst.Add(10);
      lst.Add(1);
      NumericArrayUpDownHandler<int> sut = new NumericArrayUpDownHandler<int>(lst);

      int[] wanted = new int[] { 1, 2, 5, 10 };
      CollectionAssert.AreEqual(wanted, sut.Items, "Items");
    }

    [Test]
    public void Constructor_Enumarable_Comparer()
    {
      List<int> lst = new List<int>();
      lst.Add(5);
      lst.Add(2);
      lst.Add(10);
      lst.Add(1);
      ReverseComparer comparer = new ReverseComparer();
      NumericArrayUpDownHandler<int> sut = new NumericArrayUpDownHandler<int>(lst, comparer);

      int[] wanted = new int[] { 10, 5, 2, 1 };
      CollectionAssert.AreEqual(wanted, sut.Items, "Items");
      Assert.AreSame(comparer, sut.Comparer, "Comparer");
    }

    #endregion

    #region GetUpDown()

    [TestCase(0f, null, 1f)]
    [TestCase(1f, null, 2f)]
    [TestCase(1.5f, 1f, 2f)]
    [TestCase(20f, 10f, 50f)]
    [TestCase(50f, 20f, null)]
    [TestCase(51f, 50f, null)]
    public void GetUpDown_float(float? current, float? wantedPrev, float? wantedNext)
    {
      float[] a = new float[] { 1f, 2f, 5f, 10f, 20f, 50f };
      NumericArrayUpDownHandler<float> sut = new NumericArrayUpDownHandler<float>(a, false);

      bool hasNext, hasPrev;
      float? nextValue, prevValue;
      sut.GetUpDown(current, out hasNext, out nextValue, out hasPrev, out prevValue);

      Assert.AreEqual(wantedNext, nextValue, "NextValue");
      Assert.AreEqual(wantedPrev, prevValue, "PrevValue");
      Assert.AreEqual(wantedNext.HasValue, hasNext, "HasNext");
      Assert.AreEqual(wantedPrev.HasValue, hasPrev, "HasPrev");
    }

    private struct TestType
    {
      public TestType(int value)
      {
        _Value = value;
      }

      public int Value { get { return _Value; } }
      private int _Value;
    }

    private class TestComparer : IComparer<TestType>
    {
      public int Compare(TestType x, TestType y)
      {
        return x.Value.CompareTo(y.Value);
      }
    }

    [TestCase(0, null, 1)]
    [TestCase(1, null, 5)]
    [TestCase(2, 1, 5)]
    [TestCase(5, 1, 10)]
    [TestCase(9, 5, 10)]
    [TestCase(10, 5, null)]
    [TestCase(11, 10, null)]
    public void GetUpDown_userType(int? current, int? wantedPrev, int? wantedNext)
    {
      TestType[] a = new TestType[] { new TestType(1), new TestType(5), new TestType(10) };
      TestComparer comparer = new TestComparer();

      NumericArrayUpDownHandler<TestType> sut = new NumericArrayUpDownHandler<TestType>(a, false, comparer);

      TestType? current2 = GetTestType(current);
      bool hasNext, hasPrev;
      TestType? nextValue, prevValue;
      sut.GetUpDown(current2, out hasNext, out nextValue, out hasPrev, out prevValue);

      Assert.AreEqual(wantedNext, GetIntValue(nextValue), "NextValue");
      Assert.AreEqual(wantedPrev, GetIntValue(prevValue), "PrevValue");
      Assert.AreEqual(wantedNext.HasValue, hasNext, "HasNext");
      Assert.AreEqual(wantedPrev.HasValue, hasPrev, "HasPrev");
    }

    private TestType? GetTestType(int? v)
    {
      if (v.HasValue)
        return new TestType(v.Value);
      else
        return null;
    }

    private int? GetIntValue(TestType? v)
    {
      if (v.HasValue)
        return v.Value.Value;
      else
        return null;
    }


    #endregion
  }
}
