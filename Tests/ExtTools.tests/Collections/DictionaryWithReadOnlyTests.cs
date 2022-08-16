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
  public class DictionaryWithReadOnlyTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_NoReadOnly()
    {
      DictionaryWithReadOnly<int, string> sut = new DictionaryWithReadOnly<int, string>();
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      sut.CheckNotReadOnly();
      sut.Add(1, "Hello");
      Assert.AreEqual(1, sut.Count, "Count #1");
      sut.Remove(1);
      sut.Clear();
      Assert.AreEqual(0, sut.Count, "Count #2");
    }

    [Test]
    public void Constructor_NoReadOnly_comparer()
    {
      DictionaryWithReadOnly<string, int> sut = new DictionaryWithReadOnly<string, int>(StringComparer.OrdinalIgnoreCase);
      sut.Add("AAA", 1);
      Assert.AreEqual(1, sut["Aaa"]);
    }

    [Test]
    public void Constructor_SetReadOnly()
    {
      Dictionary<string, int> dict = new Dictionary<string, int>();
      dict.Add("AAA", 1);
      dict.Add("BBB", 2);
      DictionaryWithReadOnly<string, int> sut = new DictionaryWithReadOnly<string, int>(dict, true);
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");

      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual(1, sut["AAA"], "Item");
    }

    #endregion

    #region Создание коллекции

    private class DictionaryWithReadOnly_RO : DictionaryWithReadOnly<string, int>
    {
      #region Конструктор

      public DictionaryWithReadOnly_RO()
        : base(CreateDictionary(), false)
      {
      }

      private static IDictionary<string, int> CreateDictionary()
      {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        dict.Add("aaa", 111);
        dict.Add("Bbb", 222);
        dict.Add("CCC", 333);
        return dict;
      }

      #endregion

      #region Методы

      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }

      #endregion
    }

    #endregion

    #region Методы, не меняющие коллекцию

    [Test]
    public void Item_get()
    {
      DictionaryWithReadOnly_RO sut = new DictionaryWithReadOnly_RO();
      sut.SetReadOnly();
      Assert.AreEqual(111, sut["aaa"]);
    }

    [Test]
    public void TryGetValue()
    {
      DictionaryWithReadOnly_RO sut = new DictionaryWithReadOnly_RO();
      sut.SetReadOnly();

      int value;
      Assert.IsTrue(sut.TryGetValue("CCC", out value));
      Assert.AreEqual(333, value);

      Assert.IsFalse(sut.TryGetValue("DDD", out value));
    }

    [Test]
    public void ContainsKey()
    {
      DictionaryWithReadOnly_RO sut = new DictionaryWithReadOnly_RO();
      sut.SetReadOnly();
      Assert.IsTrue(sut.ContainsKey("aaa"));
      Assert.IsFalse(sut.ContainsKey("DDD"));
    }

    [Test]
    public void CopyTo()
    {
      DictionaryWithReadOnly_RO sut = new DictionaryWithReadOnly_RO();
      sut.SetReadOnly();
      KeyValuePair<string, int>[] a = new KeyValuePair<string, int>[4];
      sut.CopyTo(a, 1);

      // Порядок элементов в массиве не определен
      Assert.IsNull(a[0].Key, "[0].Key");
      Assert.AreEqual(0, a[0].Value, "[0].Value");

      for (int i = 1; i < a.Length; i++)
      {
        int res = sut[a[i].Key];
        Assert.AreEqual(res, a[i].Value, "[" + i.ToString() + "]");
      }
    }

    [Test]
    public void ICollection_CopyTo()
    {
      DictionaryWithReadOnly_RO sut = new DictionaryWithReadOnly_RO();
      sut.SetReadOnly();
      KeyValuePair<string, int>[] a = new KeyValuePair<string, int>[4];
      ((ICollection)sut).CopyTo(a, 1);

      // Порядок элементов в массиве не определен
      Assert.IsNull(a[0].Key, "[0].Key");
      Assert.AreEqual(0, a[0].Value, "[0].Value");

      for (int i = 1; i < a.Length; i++)
      {
        int res = sut[a[i].Key];
        Assert.AreEqual(res, a[i].Value, "[" + i.ToString() + "]");
      }
    }

    [Test]
    public void Keys()
    {
      DictionaryWithReadOnly_RO sut = new DictionaryWithReadOnly_RO();
      sut.SetReadOnly();

      Assert.AreEqual(3, sut.Keys.Count, "Count");
      string[] a = new string[3];
      sut.Keys.CopyTo(a, 0);

      for (int i = 1; i < a.Length; i++)
        Assert.IsTrue(sut.ContainsKey(a[i]));
    }

    [Test]
    public void Values()
    {
      DictionaryWithReadOnly_RO sut = new DictionaryWithReadOnly_RO();
      sut.SetReadOnly();

      Assert.AreEqual(3, sut.Values.Count, "Count");
      int[] a = new int[3];
      sut.Values.CopyTo(a, 0);

      Assert.IsTrue(Array.IndexOf<int>(a, 111) >= 0);
      Assert.IsTrue(Array.IndexOf<int>(a, 222) >= 0);
      Assert.IsTrue(Array.IndexOf<int>(a, 333) >= 0);
    }

    #endregion

    #region Перечислитель

    [Test]
    public void GetEnumerator()
    {
      DictionaryWithReadOnly_RO sut = new DictionaryWithReadOnly_RO();
      sut.SetReadOnly();

      List<string> keys = new List<string>();
      List<int> values = new List<int>();

      foreach (KeyValuePair<string, int> pair in sut)
      {
        keys.Add(pair.Key);
        values.Add(pair.Value);
      }

      Assert.AreEqual(3, keys.Count, "Count");
      for (int i = 0; i < keys.Count; i++)
      {
        Assert.AreEqual(values[i], sut[keys[i]], "[" + i.ToString() + "]");
      }
    }

    [Test]
    public void IDictionary_GetEnumerator()
    {
      DictionaryWithReadOnly_RO sut = new DictionaryWithReadOnly_RO();
      sut.SetReadOnly();

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

      Assert.AreEqual(3, keys.Count, "Count");
      for (int i = 0; i < keys.Count; i++)
      {
        Assert.AreEqual(values[i], sut[keys[i]], "[" + i.ToString() + "]");
      }
    }

    #endregion

    #region Методы, изменяющие коллекцию

    [Test]
    public void Add()
    {
      DictionaryWithReadOnly_RO sut = new DictionaryWithReadOnly_RO();
      sut.Add("DDD", 444);
      sut.SetReadOnly();
      Assert.Catch(delegate() { sut.Add("EEE", 555); });
      Assert.AreEqual(4, sut.Count);
    }

    [Test]
    public void Remove()
    {
      DictionaryWithReadOnly_RO sut = new DictionaryWithReadOnly_RO();
      sut.Remove("aaa");
      sut.SetReadOnly();
      Assert.Catch(delegate() { sut.Remove("CCC"); });
      Assert.AreEqual(2, sut.Count);
    }

    [Test]
    public void Item_set()
    {
      DictionaryWithReadOnly_RO sut = new DictionaryWithReadOnly_RO();
      sut["aaa"] = 444;
      sut.SetReadOnly();
      Assert.Catch(delegate() { sut["aaa"] = 555; });
      Assert.AreEqual(444, sut["aaa"]);
    }

    [Test]
    public void Clear()
    {
      DictionaryWithReadOnly_RO sut = new DictionaryWithReadOnly_RO();
      sut.Clear();
      sut.SetReadOnly();
      Assert.Catch(delegate() { sut.Clear(); });
      Assert.AreEqual(0, sut.Count);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization([Values(true, false)] bool ignoreCase)
    {
      Dictionary<string, int> dict = new Dictionary<string, int>();
      dict.Add("AAA", 1);
      dict.Add("BBB", 2);
      DictionaryWithReadOnly<string, int> sut1 = new DictionaryWithReadOnly<string, int>(dict, true);

      byte[] bytes = SerializationTools.SerializeBinary(sut1);

      DictionaryWithReadOnly<string, int> sut2 = (DictionaryWithReadOnly<string, int>)(SerializationTools.DeserializeBinary(bytes));
      Assert.AreEqual(sut1.Count, sut2.Count, "Count");
      Assert.AreEqual(2, sut2["BBB"], "Item");
      Assert.IsTrue(sut2.IsReadOnly, "IsReadOnly");
    }

    #endregion
  }
}
