using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;

namespace ExtTools.tests
{
  [TestFixture]
  public class ListWithMRUTests
  {
    [Test]
    public void Constructor_IEnumerable()
    {
      ListWithMRU<string> sut = new ListWithMRU<string>(GetFiveItems());
      string[] a = sut.ToArray();
      Assert.AreEqual(GetFiveItems(), a);
    }

    private string[] GetFiveItems()
    {
      return new string[] { "One", "Two", "Three", "Four", "Five" };
    }

    [Test]
    public void MaxCapacity()
    {
      ListWithMRU<string> sut = new ListWithMRU<string>(GetFiveItems());
      sut.MaxCapacity = 3;

      Assert.AreEqual(new string[] { "One", "Two", "Three" }, sut.ToArray());
    }
    [Test]
    public void MaxCapacity_invalid()
    {
      ListWithMRU<string> sut = new ListWithMRU<string>();
      Assert.Throws(typeof(ArgumentOutOfRangeException), delegate() { sut.MaxCapacity = 0; });
    }

    [TestCase("One", Result = 0)]
    [TestCase("Five", Result = 4)]
    [TestCase("Zero", Result = -1)]
    public int IndexOf(string item)
    {
      ListWithMRU<string> sut = new ListWithMRU<string>(GetFiveItems());
      return sut.IndexOf(item);
    }

    [Test]
    public void Insert_unlimited()
    {
      ListWithMRU<string> sut = new ListWithMRU<string>(GetFiveItems());
      sut.Insert(2, "Zero");
      Assert.AreEqual(6, sut.Count);
    }

    [Test]
    public void Insert_limited()
    {
      ListWithMRU<string> sut = new ListWithMRU<string>(GetFiveItems());
      sut.MaxCapacity = 5;
      sut.Insert(2, "Zero");
      Assert.AreEqual(5, sut.Count);
    }

    [Test]
    public void RemoveAt()
    {
      ListWithMRU<string> sut = new ListWithMRU<string>(GetFiveItems());
      sut.RemoveAt(2);
      Assert.AreEqual(new string[] { "One", "Two", "Four", "Five" }, sut.ToArray());
    }

    [Test]
    public void Remove()
    {
      ListWithMRU<string> sut = new ListWithMRU<string>(GetFiveItems());
      bool res1 = sut.Remove("Two");
      bool res2 = sut.Remove("Zero");
      bool res3 = sut.Remove("Two");
      Assert.AreEqual(true, res1);
      Assert.AreEqual(false, res2);
      Assert.AreEqual(false, res3);
    }

    [Test]
    public void Item_get()
    {
      ListWithMRU<string> sut = new ListWithMRU<string>(GetFiveItems());
      Assert.AreEqual("Two", sut[1]);
    }

    [Test]
    public void Item_set()
    {
      ListWithMRU<string> sut = new ListWithMRU<string>(GetFiveItems());
      sut[2] = "Zero";
      Assert.AreEqual(new string[] { "One", "Two", "Zero", "Four", "Five" }, sut.ToArray());
    }

    [Test]
    public void Add_unlimited()
    {
      ListWithMRU<string> sut = new ListWithMRU<string>(GetFiveItems());
      sut.Add("Zero");
      Assert.AreEqual(new string[] { "One", "Two", "Three", "Four", "Five", "Zero"}, sut.ToArray());
    }

    public void Add_limited()
    {
      string[] a = GetFiveItems();
      ListWithMRU<string> sut = new ListWithMRU<string>();
      sut.MaxCapacity = 3;
      for (int i = 0; i < 5; i++)
        sut.Add(a[i]);

      Assert.AreEqual(new string[] { "Five", "Four", "Three" }, sut.MRU.ToArray());
      Assert.AreEqual("Five", sut.MRU.First);
      Assert.AreEqual("Three", sut.MRU.Last);
    }

    [Test]
    public void Clear()
    {
      ListWithMRU<string> sut = new ListWithMRU<string>(GetFiveItems());
      sut.Clear();
      Assert.AreEqual(0, sut.Count);
    }

    [TestCase("One", Result = true)]
    [TestCase("Zero", Result = false)]
    public bool Contains(string item)
    {
      ListWithMRU<string> sut = new ListWithMRU<string>(GetFiveItems());
      return sut.Contains(item);
    }

    [Test]
    public void Touch()
    {
      ListWithMRU<string> sut = new ListWithMRU<string>(GetFiveItems());
      bool res1 = sut.Touch("Two");
      string res2 = sut.MRU.First;
      Assert.AreEqual(true, res1);
      Assert.AreEqual("Two", res2);
    }

    public void MRUGetEnumerator()
    {
      string[] a = GetFiveItems();
      ListWithMRU<string> sut = new ListWithMRU<string>();
      sut.MaxCapacity = 3;
      for (int i = 0; i < 5; i++)
        sut.Add(a[i]);

      List<string> lst = new List<string>();
      foreach (string item in sut.MRU)
        lst.Add(item);

      Assert.AreEqual(new string[] { "Five", "Four", "Three" }, lst.ToArray());
    }
  }
}
