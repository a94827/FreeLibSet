using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  public class DictionaryWithMRUTests
  {
    [Test]
    public void Constructor_Empty()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();

      Assert.AreEqual(0, sut.Count, "Count");
      Assert.AreEqual(int.MaxValue, sut.MaxCapacity, "MaxCapacity");
      Assert.AreEqual(0, sut.Keys.Count, "Keys.Count");
      Assert.AreEqual(0, sut.Values.Count, "Values.Count");
      Assert.IsNull(sut.MRUFirstKey, "MRUFirstKey");
      Assert.IsNull(sut.MRULastKey, "MRULastKey");
    }

    [Test]
    public void Constructor_Dictionary()
    {
      Dictionary<string, int> src = new Dictionary<string, int>();
      src.Add("AAA", 1);
      src.Add("BBB", 2);
      src.Add("CCC", 3);

      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>(src);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(int.MaxValue, sut.MaxCapacity, "MaxCapacity");
      Assert.AreEqual(3, sut.Keys.Count, "Keys.Count");
      Assert.AreEqual(3, sut.Values.Count, "Values.Count");

      // Порядок MRU для добавленных элементов не определен
      Assert.IsNotNull(sut.MRUFirstKey, "MRUFirstKey");
      Assert.IsNotNull(sut.MRULastKey, "MRULastKey");
    }

    [Test]
    public void MaxCapacity()
    {
      Dictionary<string, int> src = new Dictionary<string, int>();
      src.Add("AAA", 1);
      src.Add("BBB", 2);
      src.Add("CCC", 3);

      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>(src);

      sut.MaxCapacity = 4;
      Assert.AreEqual(4, sut.MaxCapacity, "MaxCapacity-1");
      Assert.AreEqual(3, sut.Count, "Count-1");

      sut.MaxCapacity = 3;
      Assert.AreEqual(3, sut.MaxCapacity, "MaxCapacity-2");
      Assert.AreEqual(3, sut.Count, "Count-2");

      sut.MaxCapacity = 2;
      Assert.AreEqual(2, sut.MaxCapacity, "MaxCapacity-3");
      Assert.AreEqual(2, sut.Count, "Count-3");

      Assert.Catch<ArgumentException>(delegate() { sut.MaxCapacity = 0; });
      Assert.AreEqual(2, sut.MaxCapacity, "MaxCapacity-4");
      Assert.AreEqual(2, sut.Count, "Count-4");

      Assert.Catch<ArgumentException>(delegate() { sut.MaxCapacity = -1; });
      Assert.AreEqual(2, sut.MaxCapacity, "MaxCapacity-5");
      Assert.AreEqual(2, sut.Count, "Count-5");
    }

    [Test]
    public void Add_One()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);

      Assert.AreEqual(1, sut.Count, "Count");
      Assert.AreEqual(1, sut.Keys.Count, "Keys.Count");
      Assert.AreEqual(1, sut.Values.Count, "Values.Count");
      Assert.AreEqual("AAA", sut.MRUFirstKey, "MRUFirstKey");
      Assert.AreEqual("AAA", sut.MRULastKey, "MRULastKey");
      Assert.AreEqual(1, sut["AAA"], "Item[AAA]");
    }

    [Test]
    public void Add_Two()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);

      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual(2, sut.Keys.Count, "Keys.Count");
      Assert.AreEqual(2, sut.Values.Count, "Values.Count");
      Assert.AreEqual("BBB", sut.MRUFirstKey, "MRUFirstKey");
      Assert.AreEqual("AAA", sut.MRULastKey, "MRULastKey");
      Assert.AreEqual(1, sut["AAA"], "Item[AAA]");
      Assert.AreEqual(2, sut["BBB"], "Item[BBB]");
    }

    [Test]
    public void Add_UpToMaxCapacity()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.MaxCapacity = 3;
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);
      sut.Add("CCC", 3);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(3, sut.Keys.Count, "Keys.Count");
      Assert.AreEqual(3, sut.Values.Count, "Values.Count");
      Assert.AreEqual("CCC", sut.MRUFirstKey, "MRUFirstKey");
      Assert.AreEqual("AAA", sut.MRULastKey, "MRULastKey");
    }

    [Test]
    public void Add_OverMaxCapacity()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.MaxCapacity = 3;
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);
      sut.Add("CCC", 3);

      sut.Add("DDD", 4);
      Assert.AreEqual(3, sut.Count, "Count-1");
      Assert.AreEqual(3, sut.Keys.Count, "Keys.Count-1");
      Assert.AreEqual(3, sut.Values.Count, "Values.Count-1");
      Assert.AreEqual("DDD", sut.MRUFirstKey, "MRUFirstKey-1");
      Assert.AreEqual("BBB", sut.MRULastKey, "MRULastKey-1");

      sut.Add("EEE", 5);
      Assert.AreEqual(3, sut.Count, "Count-2");
      Assert.AreEqual(3, sut.Keys.Count, "Keys.Count-2");
      Assert.AreEqual(3, sut.Values.Count, "Values.Count-2");
      Assert.AreEqual("EEE", sut.MRUFirstKey, "MRUFirstKey-2");
      Assert.AreEqual("CCC", sut.MRULastKey, "MRULastKey-2");

      sut.Add("FFF", 6);
      Assert.AreEqual(3, sut.Count, "Count-3");
      Assert.AreEqual(3, sut.Keys.Count, "Keys.Count-3");
      Assert.AreEqual(3, sut.Values.Count, "Values.Count-3");
      Assert.AreEqual("FFF", sut.MRUFirstKey, "MRUFirstKey-3");
      Assert.AreEqual("DDD", sut.MRULastKey, "MRULastKey-3");
    }

    [Test]
    public void Add_ArgumentNullException()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);

      Assert.Catch(delegate() { sut.Add(null, 4); });

      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual("BBB", sut.MRUFirstKey, "MRUFirstKey");
      Assert.AreEqual("AAA", sut.MRULastKey, "MRULastKey");
    }

    [Test]
    public void Add_KeyExistsException()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);

      Assert.Catch(delegate() { sut.Add("BBB", 4); });

      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual("BBB", sut.MRUFirstKey, "MRUFirstKey");
      Assert.AreEqual("AAA", sut.MRULastKey, "MRULastKey");
    }

    [Test]
    public void ContainsKey()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);

      Assert.IsTrue(sut.ContainsKey("AAA"), "AAA");
      Assert.IsTrue(sut.ContainsKey("BBB"), "BBB");
      Assert.IsFalse(sut.ContainsKey("CCC"), "CCC");
    }

    [Test]
    public void Remove_True()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);

      Assert.IsTrue(sut.Remove("AAA"), "Remove()");
      Assert.AreEqual(1, sut.Count, "Count");
      Assert.AreEqual(1, sut.Keys.Count, "Keys.Count");
      Assert.AreEqual(1, sut.Values.Count, "Values.Count");
      Assert.AreEqual("BBB", sut.MRUFirstKey, "MRUFirstKey");
      Assert.AreEqual("BBB", sut.MRULastKey, "MRULastKey");
    }

    [Test]
    public void Remove_False()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);

      Assert.IsFalse(sut.Remove("CCC"), "Remove()");
      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual(2, sut.Keys.Count, "Keys.Count");
      Assert.AreEqual(2, sut.Values.Count, "Values.Count");
      Assert.AreEqual("BBB", sut.MRUFirstKey, "MRUFirstKey");
      Assert.AreEqual("AAA", sut.MRULastKey, "MRULastKey");
    }

    [Test]
    public void TryGetValue_True()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);

      int value;
      Assert.IsTrue(sut.TryGetValue("BBB", out value), "TryGetValue()");
      Assert.AreEqual(2, value, "value");
    }

    [Test]
    public void TryGetValue_False()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);

      int value;
      Assert.IsFalse(sut.TryGetValue("CCC", out value), "TryGetValue()");
      Assert.AreEqual(0, value, "value");
    }

    [Test]
    public void Item_Get()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);

      Assert.AreEqual(1, sut["AAA"], "AAA");
      Assert.AreEqual(2, sut["BBB"], "BBB");
      int dummy;
      Assert.Catch(delegate() { dummy = sut["CCC"]; }, "CCC");
    }

    [Test]
    public void Item_Set_Replace()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);

      sut["AAA"] = 3;

      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual(3, sut["AAA"], "AAA");
      Assert.AreEqual(2, sut["BBB"], "BBB");
    }

    [Test]
    public void Item_Set_Add()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);

      sut["CCC"] = 3;

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(1, sut["AAA"], "AAA");
      Assert.AreEqual(2, sut["BBB"], "BBB");
      Assert.AreEqual(3, sut["CCC"], "CCC");
    }

    [Test]
    public void Keys()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);
      sut.Add("CCC", 3);
      sut.Remove("BBB");

      Assert.AreEqual(2, sut.Keys.Count, "Keys.Count");
      Assert.IsTrue(sut.Keys.Contains("AAA"), "Contains(AAA)");
      Assert.IsFalse(sut.Keys.Contains("BBB"), "Contains(BBB)");
      Assert.IsTrue(sut.Keys.Contains("CCC"), "Contains(CCC)");

      string[] wantedKeys = new string[] { "CCC", "AAA" };

      string[] res1 = new string[2];
      sut.Keys.CopyTo(res1, 0);
      CollectionAssert.AreEqual(wantedKeys, res1, "CopyTo()");

      string[] res2 = sut.Keys.ToArray();
      CollectionAssert.AreEqual(wantedKeys, res2, "ToArray()");

      List<string> res3 = new List<string>();
      foreach (string x in sut.Keys)
        res3.Add(x);

      CollectionAssert.AreEqual(wantedKeys, res3, "GetEnumerator()");
    }

    [Test]
    public void Values()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);
      sut.Add("CCC", 3);
      sut.Remove("BBB");

      Assert.AreEqual(2, sut.Values.Count, "Values.Count");
      Assert.IsTrue(sut.Values.Contains(1), "Contains(1)");
      Assert.IsFalse(sut.Values.Contains(2), "Contains(2)");
      Assert.IsTrue(sut.Values.Contains(3), "Contains(3)");

      int[] wantedValues = new int[] { 3, 1 };

      int[] res1 = new int[2];
      sut.Values.CopyTo(res1, 0);
      CollectionAssert.AreEqual(wantedValues, res1, "CopyTo()");

      int[] res2 = sut.Values.ToArray();
      CollectionAssert.AreEqual(wantedValues, res2, "ToArray()");

      List<int> res3 = new List<int>();
      foreach (int x in sut.Values)
        res3.Add(x);

      CollectionAssert.AreEqual(wantedValues, res3, "GetEnumerator()");
    }

    [Test]
    public void Clear()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.MaxCapacity = 100;
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);

      sut.Clear();

      Assert.AreEqual(0, sut.Count, "Count");
      Assert.AreEqual(100, sut.MaxCapacity, "MaxCapacity");
      Assert.AreEqual(0, sut.Keys.Count, "Keys.Count");
      Assert.AreEqual(0, sut.Values.Count, "Values.Count");
      Assert.IsNull(sut.MRUFirstKey, "MRUFirstKey");
      Assert.IsNull(sut.MRULastKey, "MRULastKey");
      Assert.IsFalse(sut.ContainsKey("AAA"), "ContainsKey()");
    }

    [Test]
    public void GetEnumerator()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);
      sut.Add("CCC", 3);
      sut.Remove("BBB");

      string[] wantedKeys = new string[] { "CCC", "AAA" };
      int[] wantedValues = new int[] { 3, 1 };

      List<string> realKeys = new List<string>();
      List<int> realValues = new List<int>();
      foreach (KeyValuePair<string, int> pair in sut)
      {
        realKeys.Add(pair.Key);
        realValues.Add(pair.Value);
      }

      CollectionAssert.AreEqual(wantedKeys, realKeys, "Keys");
      CollectionAssert.AreEqual(wantedValues, realValues, "Values");
    }

    [Test]
    public void Touch_True()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);
      sut.Add("CCC", 3);

      bool res = sut.Touch("BBB");

      Assert.IsTrue(res, "Touch()");
      string[] wantedKeys = new string[] { "BBB", "CCC", "AAA" };
      int[] wantedValues = new int[] { 2, 3, 1 };
      CollectionAssert.AreEqual(wantedKeys, sut.Keys.ToArray(), "Keys");
      CollectionAssert.AreEqual(wantedValues, sut.Values.ToArray(), "Values");
    }

    [Test]
    public void Touch_False()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);
      sut.Add("CCC", 3);

      bool res = sut.Touch("DDD");

      Assert.IsFalse(res, "Touch()");
      string[] wantedKeys = new string[] { "CCC", "BBB", "AAA" };
      int[] wantedValues = new int[] { 3, 2, 1 };
      CollectionAssert.AreEqual(wantedKeys, sut.Keys.ToArray(), "Keys");
      CollectionAssert.AreEqual(wantedValues, sut.Values.ToArray(), "Values");
    }
  }
}
