using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.Remoting;
using System.Collections;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  public class OrderSortedListTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_simple()
    {
      OrderSortedList<int, string> sut = new OrderSortedList<int, string>();

      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, sut.Keys.Count, "Keys.Count");
      Assert.AreEqual(0, sut.Values.Count, "Keys.Count");
      Assert.AreSame(EqualityComparer<int>.Default, sut.Comparer, "Comparer");
    }

    [Test]
    public void Constructor_dictionary()
    {
      Dictionary<string, int> dict = new Dictionary<string, int>();
      dict.Add("BBB", 777);
      dict.Add("AAA", 222);
      dict.Add("aaa", 333);

      OrderSortedList<string, int> sut = new OrderSortedList<string, int>(dict);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(new string[] { "BBB", "AAA", "aaa" }, DataTools.CreateArray<string>(sut.Keys), "Keys");
      Assert.AreEqual(new int[] { 777, 222, 333 }, DataTools.CreateArray<int>(sut.Values), "Values");
    }

    [Test]
    public void Constructor_dictionary_comparer()
    {
      Dictionary<string, int> dict = new Dictionary<string, int>();
      dict.Add("BBB", 777);
      dict.Add("AAA", 222);

      OrderSortedList<string, int> sut = new OrderSortedList<string, int>(dict, StringComparer.OrdinalIgnoreCase);
      Assert.AreSame(StringComparer.OrdinalIgnoreCase, sut.Comparer, "Comparer");

      Assert.Catch(delegate() { sut.Add("aaa", 333); });

      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual(new string[] { "BBB", "AAA" }, DataTools.CreateArray<string>(sut.Keys), "Keys");
      Assert.AreEqual(new int[] { 777, 222 }, DataTools.CreateArray<int>(sut.Values), "Values");
    }

    #endregion

    #region Доступ к значениям

    [Test]
    public void Item_get([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);

      Assert.AreEqual(333, sut["CCC"], "#1");
      Assert.AreEqual(222, sut["Bbb"], "#2");
      Assert.AreEqual(111, sut["aaa"], "#3");

      int dummy;
      Assert.Catch<KeyNotFoundException>(delegate() { dummy = sut["DDD"]; }, "#4");
      Assert.Catch(delegate() { dummy = sut[null]; }, "#5");
    }

    [TestCase(false, "aaa", true, 111)]
    [TestCase(false, "bbb", false, 0)]
    [TestCase(true, "bbb", true, 222)]
    [TestCase(false, "DDD", false, 0)]
    [TestCase(true, "DDD", false, 0)]
    public void TryGetValue(bool useComparer, string key, bool wantedResult, int wantedValue)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);

      int value;
      bool res = sut.TryGetValue(key, out value);

      Assert.AreEqual(wantedResult, res, "Result");
      Assert.AreEqual(wantedValue, value, "Value");
    }

    [Test]
    public void Item_set_replace_noComparer()
    {
      OrderSortedList<string, int> sut = CreateTestObject(false);
      sut["Bbb"] = 444;
      Assert.AreEqual(new string[] { "CCC", "Bbb", "aaa" }, DataTools.CreateArray<string>(sut.Keys), "Keys");
      Assert.AreEqual(new int[] { 333, 444, 111 }, DataTools.CreateArray<int>(sut.Values), "Values");
    }

    [Test]
    public void Item_set_replace_withComparer()
    {
      OrderSortedList<string, int> sut = CreateTestObject(true);
      sut["BBB"] = 444;
      Assert.AreEqual(new string[] { "CCC", "Bbb", "aaa" }, DataTools.CreateArray<string>(sut.Keys), "Keys");
      Assert.AreEqual(new int[] { 333, 444, 111 }, DataTools.CreateArray<int>(sut.Values), "Values");
    }

    [Test]
    public void Item_set_new([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      sut["DDD"] = 444;
      Assert.AreEqual(new string[] { "CCC", "Bbb", "aaa", "DDD" }, DataTools.CreateArray<string>(sut.Keys), "Keys");
      Assert.AreEqual(new int[] { 333, 222, 111, 444 }, DataTools.CreateArray<int>(sut.Values), "Values");
    }

    [Test]
    public void Item_set_exception([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      Assert.Catch(delegate() { sut[null] = 444; });
    }

    private static OrderSortedList<string, int> CreateTestObject(bool useComparer)
    {
      OrderSortedList<string, int> sut;
      if (useComparer)
        sut = new OrderSortedList<string, int>(StringComparer.OrdinalIgnoreCase);
      else
        sut = new OrderSortedList<string, int>();

      sut.Add("CCC", 333);
      sut.Add("Bbb", 222);
      sut.Add("aaa", 111);

      return sut;
    }

    #endregion

    #region Keys

    [Test]
    public void Keys_Count([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      Assert.AreEqual(3, sut.Keys.Count);
    }

    [Test]
    public void Keys_Item_get([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      Assert.AreEqual("CCC", sut.Keys[0], "[0]");
      Assert.AreEqual("Bbb", sut.Keys[1], "[1]");
      Assert.AreEqual("aaa", sut.Keys[2], "[2]");
    }

    [TestCase(false, "BBB", -1)]
    //[TestCase(true, "BBB", 1)] // этот вариант не работает
    [TestCase(false, "CCC", 0)]
    [TestCase(true, "CCC", 0)]
    public void Keys_IndexOf_Contains([Values(false, true)]bool useComparer, string key, int wanted)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      int res1 = sut.Keys.IndexOf(key);
      Assert.AreEqual(wanted, res1, "IndexOf()");

      bool res2 = sut.Keys.Contains(key);
      Assert.AreEqual(wanted >= 0, res2, "Contains()");
    }

    [Test]
    public void Keys_CopyTo([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      string[] a = new string[4];
      sut.Keys.CopyTo(a, 1);
      Assert.AreEqual(new string[] { null, "CCC", "Bbb", "aaa" }, a);
    }


    [Test]
    public void Keys_GetEnumerator([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      List<string> lst = new List<string>();
      foreach (string s in sut.Keys)
        lst.Add(s);
      Assert.AreEqual(new string[] { "CCC", "Bbb", "aaa" }, lst.ToArray());
    }

    [Test]
    public void Keys_ToArray([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      Assert.AreEqual(new string[] { "CCC", "Bbb", "aaa" }, sut.Keys.ToArray());
    }

    #endregion

    #region Values

    [Test]
    public void Values_Count([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      Assert.AreEqual(3, sut.Values.Count);
    }

    [Test]
    public void Values_Item_get([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      Assert.AreEqual(333, sut.Values[0], "[0]");
      Assert.AreEqual(222, sut.Values[1], "[1]");
      Assert.AreEqual(111, sut.Values[2], "[2]");
    }

    [TestCase(false, 444, -1)]
    [TestCase(true, 444, -1)]
    [TestCase(false, 333, 0)]
    [TestCase(true, 333, 0)]
    public void Values_IndexOf_Contains([Values(false, true)]bool useComparer, int value, int wanted)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      int res1 = sut.Values.IndexOf(value);
      Assert.AreEqual(wanted, res1, "IndexOf()");

      bool res2 = sut.Values.Contains(value);
      Assert.AreEqual(wanted >= 0, res2, "Contains()");
    }

    [Test]
    public void Values_CopyTo([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      int[] a = new int[4];
      sut.Values.CopyTo(a, 1);
      Assert.AreEqual(new int[] { 0, 333, 222, 111 }, a);
    }

    [Test]
    public void Values_GetEnumerator([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      List<int> lst = new List<int>();
      foreach (int v in sut.Values)
        lst.Add(v);
      Assert.AreEqual(new int[] { 333, 222, 111 }, lst.ToArray());
    }

    [Test]
    public void Values_ToArray([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      Assert.AreEqual(new int[] { 333, 222, 111 }, sut.Values.ToArray());
    }

    #endregion

    #region Add()/Remove()

    [Test]
    public void Add_success([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      sut.Add("DDD", 111);
      Assert.AreEqual(new string[] { "CCC", "Bbb", "aaa", "DDD" }, sut.Keys.ToArray(), "Keys");
      Assert.AreEqual(new int[] { 333, 222, 111, 111 }, sut.Values.ToArray(), "Keys");
    }

    [Test]
    public void Add_fail_keyexists([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      Assert.Catch(delegate() { sut.Add("CCC", 444); });
    }

    [Test]
    public void Add_fail_comparer()
    {
      OrderSortedList<string, int> sut = CreateTestObject(true);
      Assert.Catch(delegate() { sut.Add("bbb", 444); });
    }

    [Test]
    public void Add_fail_nullkey([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      Assert.Catch(delegate() { sut.Add(null, 444); });
    }

    [TestCase(false, "aaa", true, "CCC,Bbb", "333,222")]
    [TestCase(true, "aaa", true, "CCC,Bbb", "333,222")]
    [TestCase(false, "ccc", false, "CCC,Bbb,aaa", "333,222,111")]
    [TestCase(true, "ccc", true, "Bbb,aaa", "222,111")]
    public void Remove([Values(false, true)]bool useComparer, string key, bool wantedRes, string sWantedKeys, string sWantedValues)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      bool res = sut.Remove(key);

      Assert.AreEqual(wantedRes, res, "Result");
      Assert.AreEqual(sWantedKeys.Split(','), sut.Keys.ToArray(), "Keys");

      int[] wantedValues = StdConvert.ToInt32Array(sWantedValues);
      Assert.AreEqual(wantedValues, sut.Values.ToArray(), "Values");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void Clear([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      sut.Clear();

      Assert.AreEqual(0, sut.Count, "Count");
      Assert.AreEqual(DataTools.EmptyStrings, sut.Keys.ToArray(), "Keys");
      Assert.AreEqual(DataTools.EmptyInts, sut.Values.ToArray(), "Values");
    }

    [TestCase(false, "bbb", false)]
    [TestCase(true, "bbb", true)]
    public void ContainsKey([Values(false, true)]bool useComparer, string key, bool wanted)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      bool res = sut.ContainsKey(key);
      Assert.AreEqual(wanted, res);
    }

    [Test]
    public void CopyTo([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);

      KeyValuePair<string, int>[] a = new KeyValuePair<string, int>[4];
      sut.CopyTo(a, 1);

      Assert.AreEqual(null, a[0].Key, "[0].Key");
      Assert.AreEqual(0, a[0].Value, "[0].Value");
      Assert.AreEqual("CCC", a[1].Key, "[1].Key");
      Assert.AreEqual(333, a[1].Value, "[1].Value");
      Assert.AreEqual("Bbb", a[2].Key, "[2].Key");
      Assert.AreEqual(222, a[2].Value, "[2].Value");
      Assert.AreEqual("aaa", a[3].Key, "[3].Key");
      Assert.AreEqual(111, a[3].Value, "[3].Value");
    }

    [Test]
    public void ToArray([Values(false, true)]bool useComparer)
    {
      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      KeyValuePair<string, int>[] a = sut.ToArray();

      Assert.AreEqual(3, a.Length, "Length");
      for (int i = 0; i < sut.Count; i++)
      {
        Assert.AreEqual(sut.Keys[i], a[i].Key, "[" + i.ToString() + "].Key");
        Assert.AreEqual(sut.Values[i], a[i].Value, "[" + i.ToString() + "].Value");
      }
    }

    #endregion

    #region GetEnumerator()

    [Test]
    public void GetEnumerator([Values(false, true)]bool useComparer)
    {
      List<string> lst1 = new List<string>();
      List<int> lst2 = new List<int>();

      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      foreach (KeyValuePair<string, int> pair in sut)
      {
        lst1.Add(pair.Key);
        lst2.Add(pair.Value);
      }

      Assert.AreEqual(new string[] { "CCC", "Bbb", "aaa" }, lst1.ToArray(), "Keys");
      Assert.AreEqual(new int[] { 333, 222, 111 }, lst2.ToArray(), "Values");
    }

    [Test]
    public void IDictionary_GetEnumerator([Values(false, true)]bool useComparer)
    {
      List<string> lst1 = new List<string>();
      List<int> lst2 = new List<int>();

      OrderSortedList<string, int> sut = CreateTestObject(useComparer);
      foreach (object x in (IDictionary)sut)
      {
        Assert.IsInstanceOf<DictionaryEntry>(x, "DictionaryEntry type");
        DictionaryEntry de = (DictionaryEntry)x;

        Assert.IsInstanceOf<string>(de.Key, "Key type");
        Assert.IsInstanceOf<int>(de.Value, "Value type");

        lst1.Add((string)(de.Key));
        lst2.Add((int)(de.Value));
      }

      Assert.AreEqual(new string[] { "CCC", "Bbb", "aaa" }, lst1.ToArray(), "Keys");
      Assert.AreEqual(new int[] { 333, 222, 111 }, lst2.ToArray(), "Values");
    }

    #endregion

    #region SetReadOnly()

    private class OrderSortedList_RO<TKey, TValue> : OrderSortedList<TKey, TValue>
    {
      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }
    }

    [Test]
    public void SetReadOnly()
    {
      OrderSortedList_RO<int, int> sut = new OrderSortedList_RO<int, int>();
      sut.Add(2, 12);
      sut.Add(1, 11);
      sut.SetReadOnly();

      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch(delegate() { sut.Add(3, 13); }, "Add()");
      Assert.Catch(delegate() { sut.Clear(); }, "Clear()");
      Assert.Catch(delegate() { sut[2] = 22; }, "Item set");

      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual(new int[] { 2, 1 }, sut.Keys.ToArray(), "Keys");
      Assert.AreEqual(new int[] { 12, 11 }, sut.Values.ToArray(), "Values");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization([Values(true, false)] bool useComparer)
    {
      OrderSortedList<string, int> sut1 = CreateTestObject(useComparer);
      byte[] bytes = SerializationTools.SerializeBinary(sut1);
      OrderSortedList<string, int> sut2 = (OrderSortedList<string, int>)(SerializationTools.DeserializeBinary(bytes));

      Assert.AreEqual(sut1.ToArray(), sut2.ToArray(), "ToArray()");
    }

    #endregion
  }
}
