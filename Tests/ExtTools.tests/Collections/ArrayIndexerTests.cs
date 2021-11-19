using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.Text;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  public class ArrayIndexerTests
  {
    #region Основные методы

    [Test]
    public void Constructor_simple()
    {
      int[] a = new int[] { 1, 2, 3 };
      ArrayIndexer<int> sut = new ArrayIndexer<int>(a);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(UnknownItemPosition.First, sut.UnknownItemPosition, "UnknownItemPosition");
    }

    [TestCase("AAA", 0)]
    [TestCase("BBB", 1)]
    [TestCase("CCC", 2)]
    [TestCase("DDD", -1)]
    public void IndexOf_Contains_simple(string item, int wantedIndex)
    {
      string[] a = new string[] { "AAA", "BBB", "CCC" };
      ArrayIndexer<string> sut = new ArrayIndexer<string>(a);

      Assert.AreEqual(wantedIndex, sut.IndexOf(item), "IndexOf");

      Assert.AreEqual(wantedIndex >= 0, sut.Contains(item), "IndexOf");
    }

    [TestCase("AAA,CCC", true)]
    [TestCase("AAA,DDD", false)]
    [TestCase("BBB", true)]
    [TestCase("DDD", false)]
    [TestCase("", true)]
    [TestCase("BBB,BBB", true)]
    public void ContainsAll_simple(string sItems, bool wanted)
    {
      string[] a = new string[] { "AAA", "BBB", "CCC" };
      ArrayIndexer<string> sut = new ArrayIndexer<string>(a);

      string[] items = new CsvTextConvert().ToArray(sItems);
      if (items == null)
        items = DataTools.EmptyStrings;

      Assert.AreEqual(wanted, sut.ContainsAll(items));
    }

    [TestCase("AAA,CCC", true)]
    [TestCase("AAA,DDD", true)]
    [TestCase("BBB", true)]
    [TestCase("DDD", false)]
    [TestCase("", false)]
    [TestCase("BBB,BBB", true)]
    public void ContainsAny_simple(string sItems, bool wanted)
    {
      string[] a = new string[] { "AAA", "BBB", "CCC" };
      ArrayIndexer<string> sut = new ArrayIndexer<string>(a);

      string[] items = new CsvTextConvert().ToArray(sItems);
      if (items == null)
        items = DataTools.EmptyStrings;

      Assert.AreEqual(wanted, sut.ContainsAny(items));
    }

    [TestCase("First", "DDD,CCC,BBB")]
    [TestCase("Last", "CCC,BBB,DDD")]
    public void Compare_simple(string sUIP, string sWanted)
    {
      ArrayIndexer<string> sut = new ArrayIndexer<string>(new string[] { "CCC", "AAA", "BBB" });
      sut.UnknownItemPosition = StdConvert.ToEnum<UnknownItemPosition>(sUIP);

      string[] a = new string[] { "BBB", "CCC", "DDD" };

      Array.Sort(a, sut);
      Assert.AreEqual(sWanted, String.Join(",", a));
    }


    #endregion

    #region Внешний сравниватель

    #region Тестовый класс и компаратор

    private class TestObject
    {
      #region Конструктор

      public TestObject(int value)
      {
        _Value = value;
      }

      #endregion

      #region Свойства

      public int Value { get { return _Value; } }
      private int _Value;

      public override string ToString()
      {
        return StdConvert.ToString(_Value);
      }

      #endregion
    }

    private class TestComparer : IEqualityComparer<TestObject>
    {
      #region IEqualityComparer<TestObject> Members

      public bool Equals(TestObject x, TestObject y)
      {
        if (Object.ReferenceEquals(x, null) && Object.ReferenceEquals(y, null))
          return true;
        if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
          return false;
        return x.Value == y.Value;
      }

      public int GetHashCode(TestObject obj)
      {
        if (obj == null)
          return 0;
        else
          return obj.Value;
      }

      #endregion
    }

    private static TestObject[] CreateTestObjectArray(string values)
    {
      int[] a1 = StdConvert.ToInt32Array(values);
      TestObject[] a2 = new TestObject[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = new TestObject(a1[i]);
      return a2;
    }

    private static ArrayIndexer<TestObject> CreateTestObjectIndexer(string values)
    {
      TestObject[] a = CreateTestObjectArray(values);
      return new ArrayIndexer<TestObject>(a, new TestComparer());
    }

    #endregion

    [TestCase(1, 0)]
    [TestCase(3, 1)]
    [TestCase(2, 2)]
    [TestCase(4, -1)]
    public void IndexOf_Contains_comparer(int value, int wantedIndex)
    {
      ArrayIndexer<TestObject> sut = CreateTestObjectIndexer("1,3,2");

      TestObject item = new TestObject(value);

      Assert.AreEqual(wantedIndex, sut.IndexOf(item), "IndexOf");

      Assert.AreEqual(wantedIndex >= 0, sut.Contains(item), "IndexOf");
    }

    [TestCase("1,3", true)]
    [TestCase("1,4", false)]
    [TestCase("2", true)]
    [TestCase("4", false)]
    [TestCase("", true)]
    public void ContainsAll_comparer(string sItems, bool wanted)
    {
      ArrayIndexer<TestObject> sut = CreateTestObjectIndexer("1,2,3");
      TestObject[] items = CreateTestObjectArray(sItems);
      Assert.AreEqual(wanted, sut.ContainsAll(items));
    }

    [TestCase("1,3", true)]
    [TestCase("1,4", true)]
    [TestCase("2", true)]
    [TestCase("4", false)]
    [TestCase("", false)]
    public void ContainsAny_comparer(string sItems, bool wanted)
    {
      ArrayIndexer<TestObject> sut = CreateTestObjectIndexer("1,2,3");
      TestObject[] items = CreateTestObjectArray(sItems);
      Assert.AreEqual(wanted, sut.ContainsAny(items));
    }

    [TestCase("First", "4,1,3")]
    [TestCase("Last", "1,3,4")]
    public void Compare_comparer(string sUIP, string sWanted)
    {
      ArrayIndexer<TestObject> sut = CreateTestObjectIndexer("1,3,2");
      sut.UnknownItemPosition = StdConvert.ToEnum<UnknownItemPosition>(sUIP);

      TestObject[] a = CreateTestObjectArray("3,4,1");
      Array.Sort<TestObject>(a, sut);

      Assert.AreEqual(sWanted, DataTools.ToStringJoin<TestObject>(",", a));
    }

    #endregion
  }
}
