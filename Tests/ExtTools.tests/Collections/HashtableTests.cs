using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Collections;
using FreeLibSet.Remoting;
using FreeLibSet.Core;
using System.Collections;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  public class HashtableTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Simple()
    {
      Hashtable<string, int> sut = new Hashtable<string, int>();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.AreEqual(0, sut.Keys.Count, "Count");
      Assert.AreEqual(0, sut.Values.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreSame(EqualityComparer<string>.Default, sut.Comparer, "Comparer");
    }

    [Test]
    public void Constructor_dictionary([Values(true, false)] bool isReadOnly)
    {
      Dictionary<string, int> dict = new Dictionary<string, int>();
      dict.Add("AAA", 1);
      dict.Add("BBB", 2);
      Hashtable<string, int> sut = new Hashtable<string, int>(dict, isReadOnly);
      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual(2, sut.Keys.Count, "Count");
      Assert.AreEqual(2, sut.Values.Count, "Count");
      Assert.AreEqual(1, sut["AAA"], "#1");
      Assert.AreEqual(2, sut["BBB"], "#2");
      Assert.AreEqual(isReadOnly, sut.IsReadOnly);
    }

    [Test]
    public void Constructor_comparer()
    {
      Hashtable<string, int> sut = new Hashtable<string, int>(StringComparer.OrdinalIgnoreCase);
      Assert.AreSame(StringComparer.OrdinalIgnoreCase, sut.Comparer, "Comparer");

      sut["AAA"] = 1;
      sut["aaa"] = 2;

      Assert.AreEqual(1, sut.Count, "Count");
      Assert.AreEqual(2, sut["AAA"], "#1");
      Assert.AreEqual(2, sut["aaa"], "#2");
    }

    #endregion

    #region Метод Item

    private static Hashtable<string, int> CreateTestObject(bool ignoreCase)
    {
      Hashtable<string, int> sut;
      if (ignoreCase)
        sut = new Hashtable<string, int>(StringComparer.OrdinalIgnoreCase);
      else
        sut = new Hashtable<string, int>();

      sut.Add("Aaa", 1);
      sut.Add("Bbb", 2);
      return sut;
    }

    [Test]
    public void Item_get()
    {
      Hashtable<string, int> sut = CreateTestObject(false);
      Assert.AreEqual(2, sut["Bbb"], "#1");
      Assert.AreEqual(0, sut["BBB"], "#2");
    }

    [Test]
    public void Item_get_usecomparator()
    {
      Hashtable<string, int> sut = CreateTestObject(true);
      Assert.AreEqual(2, sut["Bbb"], "#1");
      Assert.AreEqual(2, sut["BBB"], "#2");
    }

    [Test]
    public void Item_set()
    {
      Hashtable<string, int> sut = CreateTestObject(false);
      sut["Aaa"] = 3;
      Assert.AreEqual(2, sut.Count, "Count #1");
      Assert.AreEqual(3, sut["Aaa"], "Value #1");

      sut["AAA"] = 4;
      Assert.AreEqual(3, sut.Count, "Count #2");
      Assert.AreEqual(4, sut["AAA"], "Value #2");
      Assert.AreEqual(3, sut["Aaa"], "Value #3");
    }

    [Test]
    public void Item_set_usecomparator()
    {
      Hashtable<string, int> sut = CreateTestObject(true);

      sut["AAA"] = 4;
      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual(4, sut["AAA"], "Value #1");
      Assert.AreEqual(4, sut["Aaa"], "Value #2");
    }

    #endregion

    #region Add/Remove()

    [Test]
    public void Add()
    {
      Hashtable<string, int> sut = CreateTestObject(false);
      sut.Add("AAA", 3);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(3, sut["AAA"], "Value");
    }

    [Test]
    public void Add_Fail_comparer()
    {
      Hashtable<string, int> sut = CreateTestObject(true);
      Assert.Catch(delegate() { sut.Add("AAA", 3); });
    }

    [Test]
    public void Remove()
    {
      Hashtable<string, int> sut = CreateTestObject(false);
      bool res1 = sut.Remove("AAA");
      Assert.IsFalse(res1, "Result #1");
      Assert.AreEqual(2, sut.Count, "Count #1");

      bool res2 = sut.Remove("Aaa");
      Assert.IsTrue(res2, "Result #2");
      Assert.AreEqual(1, sut.Count, "Count #2");

      Assert.AreEqual(0, sut["Aaa"], "Value #1");
      Assert.AreEqual(2, sut["Bbb"], "Value #2");
    }

    [Test]
    public void Remove_Comparer()
    {
      Hashtable<string, int> sut = CreateTestObject(true);
      bool res = sut.Remove("AAA");
      Assert.IsTrue(res, "Result");
      Assert.AreEqual(1, sut.Count, "Count");
      Assert.AreEqual(0, sut["Aaa"], "Value");
    }

    #endregion

    #region Keys/Values

    [Test]
    public void Keys_Values([Values(true, false)]bool useComparer)
    {
      Hashtable<string, int> sut = CreateTestObject(useComparer);
      Assert.AreEqual(2, sut.Keys.Count, "Keys.Count");
      Assert.AreEqual(2, sut.Values.Count, "Values.Count");

      List<string> keys = new List<string>();
      List<int> values = new List<int>();

      // Важно, что перечислители выполняются в одном и том же порядке
      foreach (string key in sut.Keys)
        keys.Add(key);
      foreach (int value in sut.Values)
        values.Add(value);

      ValidateTestKeysAndValues(keys, values);
    }

    private static void ValidateTestKeysAndValues(List<string> keys, List<int> values)
    {
      Assert.AreEqual(2, keys.Count, "Enum Keys count");
      Assert.AreEqual(2, values.Count, "Enum Values count");

      int p1 = keys.IndexOf("Aaa");
      Assert.IsTrue(p1 >= 0, "Key #1 found");
      Assert.AreEqual(1, values[p1], "Value #1");

      int p2 = keys.IndexOf("Bbb");
      Assert.IsTrue(p2 >= 0, "Key #2 found");
      Assert.AreEqual(2, values[p2], "Value #2");
    }

    [Test]
    public void Keys_CopyTo([Values(true, false)]bool useComparer)
    {
      Hashtable<string, int> sut = CreateTestObject(useComparer);
      List<string> keys = new List<string>();
      foreach (string key in sut.Keys)
        keys.Add(key);

      string[] res = new string[5];
      sut.Keys.CopyTo(res, 2);

      string[] wanted = new string[] { null, null, keys[0], keys[1], null };
      Assert.AreEqual(wanted, res);
    }

#if XXX // Свойства Keys и Values реализованы через класс Dictionary<TKey, TValue> и не поддерживают преобразование значений
    
    [Test]
    public void Keys_ICollection_CopyTo()
    {
      Hashtable<int, int> sut = new Hashtable<int, int>();
      sut.Add(1, 10);
      sut.Add(2, 20);
      List<int> keys = new List<int>();
      foreach (int key in sut.Keys)
        keys.Add(key);

      float[] res = new float[5];
      ((ICollection)(sut.Keys)).CopyTo(res, 2); // должен выполнять преобразование

      float[] wanted = new float[] { 0f, 0f, (float)(keys[0]), (float)(keys[1]), 0f };
      Assert.AreEqual(wanted, res);
    }

#endif

    [Test]
    public void Values_CopyTo([Values(true, false)]bool useComparer)
    {
      Hashtable<string, int> sut = CreateTestObject(useComparer);
      List<int> values = new List<int>();
      foreach (int value in sut.Values)
        values.Add(value);

      int[] res = new int[5];
      sut.Values.CopyTo(res, 2);

      int[] wanted = new int[] { 0, 0, values[0], values[1], 0};
      Assert.AreEqual(wanted, res);
    }

#if XXX // Свойства Keys и Values реализованы через класс Dictionary<TKey, TValue> и не поддерживают преобразование значений
   
    [Test]
    public void Values_ICollection_CopyTo()
    {
      Hashtable<string, int> sut = CreateTestObject(false);
      List<int> values = new List<int>();
      foreach (int value in sut.Values)
        values.Add(value);

      float[] res = new float[5];
      ((ICollection)(sut.Values)).CopyTo(res, 2); // должен выполнять преобразование

      float[] wanted = new float[] { 0f, 0f, (float)(values[0]), (float)(values[1]), 0f };
      Assert.AreEqual(wanted, res);
    }
    
#endif

    #endregion

    #region Прочие методы

    [Test]
    public void Clear()
    {
      Hashtable<string, int> sut = CreateTestObject(false);
      sut.Clear();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.AreEqual(0, sut.Keys.Count, "Keys.Count");
      Assert.AreEqual(0, sut.Values.Count, "Values.Count");
    }

    [Test]
    public void ContainsKey()
    {
      Hashtable<string, int> sut = CreateTestObject(false);
      Assert.IsTrue(sut.ContainsKey("Aaa"), "#1");
      Assert.IsFalse(sut.ContainsKey("AAA"), "#2");
    }

    [Test]
    public void CopyTo([Values(true, false)]bool useComparer)
    {
      Hashtable<string, int> sut = CreateTestObject(useComparer);
      KeyValuePair<string, int>[] a = new KeyValuePair<string, int>[3];
      sut.CopyTo(a, 1);
      Assert.IsNull(a[0].Key, "[0].Key");
      Assert.AreEqual(0, a[0].Value, "a[0].Value");

      // Порядок скопированных элементов не гарантирован
      for (int i = 1; i < a.Length; i++)
      {
        Assert.AreEqual(sut[a[i].Key], a[i].Value, "#" + i.ToString());
      }
    }

    [Test]
    public void IDictionary_CopyTo([Values(true, false)]bool useComparer)
    {
      Hashtable<string, int> sut = CreateTestObject(useComparer);
      DictionaryEntry[] a = new DictionaryEntry[3];
      ((IDictionary)sut).CopyTo(a, 1);
      Assert.IsNull(a[0].Key, "[0].Key");
      Assert.IsNull(a[0].Value, "a[0].Value");

      // Порядок скопированных элементов не гарантирован
      for (int i = 1; i < a.Length; i++)
      {
        Assert.IsInstanceOf<string>(a[i].Key, "Key type");
        Assert.IsInstanceOf<int>(a[i].Value, "Value type");

        string key = (string)(a[i].Key);
        Assert.AreEqual(sut[key], a[i].Value, "#" + i.ToString());
      }
    }

    [TestCase(false, "Aaa", true, 1)]
    [TestCase(false, "AAA", false, 0)]
    [TestCase(true, "AAA", true, 1)]
    public void TryGetValue([Values(true, false)]bool useComparer, string key, bool wantedRes, int wantedValue)
    {
      Hashtable<string, int> sut = CreateTestObject(useComparer);
      int value;
      bool res = sut.TryGetValue(key, out value);
      Assert.AreEqual(wantedRes, res, "Result");
      Assert.AreEqual(wantedValue, value, "Value");
    }

    #endregion

    #region Перечислитель

    [Test]
    public void GetEnumerator([Values(true, false)]bool useComparer)
    {
      Hashtable<string, int> sut = CreateTestObject(useComparer);
      List<string> keys = new List<string>();
      List<int> values = new List<int>();
      foreach (KeyValuePair<string, int> pair in sut)
      {
        keys.Add(pair.Key);
        values.Add(pair.Value);
      }

      ValidateTestKeysAndValues(keys, values);
    }

    [Test]
    public void IDictionary_GetEnumerator([Values(true, false)]bool useComparer)
    {
      Hashtable<string, int> sut = CreateTestObject(useComparer);
      List<string> keys = new List<string>();
      List<int> values = new List<int>();
      foreach (object x in (IDictionary)sut)
      {
        Assert.IsInstanceOf<DictionaryEntry>(x, "DictionaryEntry type");
        DictionaryEntry de = (DictionaryEntry)x;

        Assert.IsInstanceOf<string>(de.Key, "Key type");
        Assert.IsInstanceOf<int>(de.Value, "Value type");
        keys.Add((string)(de.Key));
        values.Add((int)(de.Value));
      }

      ValidateTestKeysAndValues(keys, values);
    }

    #endregion

    #region SetReadOnly()

    [Serializable]
    private class Hashtable_RO<TKey, TValue> : Hashtable<TKey, TValue>
    {
      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }
    }

    [Test]
    public void SetReadOnly()
    {
      Hashtable_RO<int, string> sut = new Hashtable_RO<int, string>();
      sut.Add(1, "AAA");
      sut.Add(2, "BBB");
      sut.SetReadOnly();

      Assert.IsTrue(sut.IsReadOnly);
      Assert.Catch(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch(delegate() { sut[2] = "CCC"; }, "Item_set");
      Assert.Catch(delegate() { sut.Add(3, "DDD"); }, "Add()");
      Assert.Catch(delegate() { sut.Remove(1); }, "Remove()");
      Assert.Catch(delegate() { sut.Clear(); }, "Clear()");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization([Values(false, true)] bool useReadOnly)
    {
      Hashtable_RO<int, string> sut1 = new Hashtable_RO<int, string>();
      sut1.Add(1, "AAA");
      sut1.Add(2, "BBB");
      if (useReadOnly)
        sut1.SetReadOnly();

      byte[] bytes = SerializationTools.SerializeBinary(sut1);
      Hashtable_RO<int, string> sut2 = (Hashtable_RO<int, string>)(SerializationTools.DeserializeBinary(bytes));

      // Вообще говоря, порядок элементов при сериализации не обязан сохраняться
      // Assert.AreEqual(DataTools.CreateArray<KeyValuePair<int, string>>(sut1), DataTools.CreateArray<KeyValuePair<int, string>>(sut2));
      Assert.AreEqual("AAA", sut2[1], "#1");
      Assert.AreEqual("BBB", sut2[2], "#2");
      Assert.AreEqual(useReadOnly, sut2.IsReadOnly);
    }

    #endregion
  }
}
