using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using NUnit.Framework;
using FreeLibSet.Remoting;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  public class HistoryListTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_array()
    {
      HistoryList sut = new HistoryList(new string[] { "AAA", "BBB", "CCC" });
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual("AAA", sut.Top, "Top");
    }

    [Test]
    public void Constructor_collection()
    {
      List<string> lst = new List<string>();
      lst.Add("AAA");
      lst.Add("BBB");
      lst.Add("CCC");
      HistoryList sut = new HistoryList((ICollection<string>)lst);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual("AAA", sut.Top, "Top");
    }

    [Test]
    public void Constructor_string()
    {
      HistoryList sut = new HistoryList("AAA");
      Assert.AreEqual(1, sut.Count, "Count");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual("AAA", sut.Top, "Top");
    }

    [Test]
    public void Constructor_empty_string()
    {
      HistoryList sut = new HistoryList("");
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual("", sut.Top, "Top");
    }

    [Test]
    public void Constructor_default()
    {
      HistoryList sut = new HistoryList();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual("", sut.Top, "Top");
    }

    #endregion

    [Test]
    public void Item_get()
    {
      HistoryList sut = new HistoryList(new string[] { "AAA", "BBB", "CCC" });
      Assert.AreEqual("AAA", sut[0], "[0]");
      Assert.AreEqual("BBB", sut[1], "[1]");
      Assert.AreEqual("CCC", sut[2], "[2]");
    }

    [Test]
    public void Add_1arg()
    {
      HistoryList sut = new HistoryList();

      HistoryList res1 = sut.Add("AAA");
      Assert.AreEqual(new string[] { "AAA" }, res1.ToArray());

      HistoryList res2 = res1.Add("BBB");
      Assert.AreEqual(new string[] { "BBB", "AAA" }, res2.ToArray());
    }

    [Test]
    public void Add_2arg()
    {
      HistoryList sut = new HistoryList();

      HistoryList res1 = sut.Add("AAA", 3);
      Assert.AreEqual(new string[] { "AAA" }, res1.ToArray(), "#1");

      HistoryList res2 = res1.Add("BBB", 3);
      Assert.AreEqual(new string[] { "BBB", "AAA" }, res2.ToArray(), "#2");

      HistoryList res3 = res2.Add("CCC", 3);
      Assert.AreEqual(new string[] { "CCC", "BBB", "AAA" }, res3.ToArray(), "#3");

      HistoryList res4 = res3.Add("DDD", 3);
      Assert.AreEqual(new string[] { "DDD", "CCC", "BBB" }, res4.ToArray(), "#4");
    }

    [Test]
    public void Add_exists()
    {
      HistoryList sut = new HistoryList(new string[] { "AAA", "BBB", "CCC" });

      HistoryList res1 = sut.Add("BBB");
      Assert.AreEqual(new string[] { "BBB", "AAA", "CCC" }, res1.ToArray());
    }

    [Test]
    public void SetLimit()
    {
      HistoryList sut = new HistoryList(new string[] { "AAA", "BBB", "CCC" });
      HistoryList res1 = sut.SetLimit(3);
      Assert.AreEqual(new string[] { "AAA", "BBB", "CCC" }, res1.ToArray(), "#1");
      HistoryList res2 = sut.SetLimit(2);
      Assert.AreEqual(new string[] { "AAA", "BBB" }, res2.ToArray(), "#2");
    }

    [TestCase("AAA", 0)]
    [TestCase("CCC", 2)]
    [TestCase("DDD", -1)]
    [TestCase("aaa", -1)]
    [TestCase("", -1)]
    public void IndexOf(string s, int wanted)
    {
      HistoryList sut = new HistoryList(new string[] { "AAA", "BBB", "CCC" });
      int res1 = sut.IndexOf(s);
      Assert.AreEqual(wanted, res1, "IndexOf()");

      bool res2 = sut.Contains(s);
      Assert.AreEqual(wanted >= 0, res2, "Contains()");
    }

    [Test]
    public void CopyTo_2arg()
    {
      HistoryList sut = new HistoryList(new string[] { "AAA", "BBB", "CCC" });
      string[] a = new string[sut.Count];
      sut.CopyTo(a, 0);
      Assert.AreEqual(new string[] { "AAA", "BBB", "CCC" }, a);
    }

    [Test]
    public void CopyTo_4arg()
    {
      HistoryList sut = new HistoryList(new string[] { "AAA", "BBB", "CCC" });
      string[] a = new string[2];
      sut.CopyTo(1, a, 0, 2);
      Assert.AreEqual(new string[] { "BBB", "CCC" }, a);
    }

    [Test]
    public void ToArray()
    {
      HistoryList sut = new HistoryList(new string[] { "AAA", "BBB", "CCC" });
      string[] res = sut.ToArray();
      Assert.AreEqual(new string[] { "AAA", "BBB", "CCC" }, res);
    }

    [Test]
    public void ToArray_empty()
    {
      HistoryList sut = new HistoryList();
      string[] res = sut.ToArray();
      Assert.AreEqual(new string[0], res);
    }

    [Test]
    public void RemoveAt()
    {
      HistoryList sut = new HistoryList(new string[] { "AAA", "BBB", "CCC" });
      HistoryList res = sut.RemoveAt(1);
      Assert.AreEqual(new string[] { "AAA", "CCC" }, res.ToArray());
    }

    [Test]
    public void Remove()
    {
      HistoryList sut = new HistoryList(new string[] { "AAA", "BBB", "CCC" });
      HistoryList res1 = sut.Remove("BBB");
      Assert.AreEqual(new string[] { "AAA", "CCC" }, res1.ToArray(), "#1");
      HistoryList res2 = sut.Remove("DDD");
      Assert.AreEqual(new string[] { "AAA", "BBB", "CCC" }, res2.ToArray(), "#2");
    }

    [Test]
    public void Enumerator()
    {
      HistoryList sut = new HistoryList(new string[] { "AAA", "BBB", "CCC" });
      StringBuilder sb = new StringBuilder();
      foreach (string item in sut)
        sb.Append(item);

      Assert.AreEqual("AAABBBCCC", sb.ToString());
    }

    [Test]
    public void Enumerator_empty()
    {
      HistoryList sut = new HistoryList();
      StringBuilder sb = new StringBuilder();
      foreach (string item in sut)
        sb.Append(item);

      Assert.AreEqual("", sb.ToString());
    }

    [Test]
    public void Merge()
    {
      HistoryList list1 = new HistoryList(new string[] { "AAA", "BBB", "CCC" });
      HistoryList list2 = new HistoryList();
      HistoryList list3 = new HistoryList(new string[] { "BBB", "CCC", "DDD" });

      HistoryList res = HistoryList.Merge(list1, list2, list3);

      Assert.AreEqual(new string[] { "AAA", "BBB", "CCC", "DDD" }, res.ToArray());
    }

    [Test]
    public void Empty()
    {
      HistoryList sut = HistoryList.Empty;
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
    }

    #region Сериализация

    [Test]
    public void Serialization()
    {
      HistoryList sut1 = new HistoryList(new string[] { "AAA", "BBB", "CCC" });

      byte[] bytes = SerializationTools.SerializeBinary(sut1);
      HistoryList sut2 = (HistoryList)(SerializationTools.DeserializeBinary(bytes));

      Assert.AreEqual(sut1.ToArray(), sut2.ToArray());
    }

    #endregion
  }
}
