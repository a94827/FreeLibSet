using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using System.Collections;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  public class DictionaryWithMRUTests
  {
    #region Конструкторы

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

    #endregion

    #region MaxCapacity

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

    #endregion

    #region Add()

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

    #endregion

    #region ContainsKey()

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

    #endregion

    #region Remove()

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

    #endregion

    #region Доступ к элементам

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

    #endregion

    #region Keys & Values

    /// <summary>
    /// Создает тестовый элемент с ключами "AAA", "CCC", "DDD" и значениями 1,3,4 соотвественно.
    /// Причем к ключу "DDD" прикосновение было последнимм
    /// </summary>
    /// <returns></returns>
    private static DictionaryWithMRU<string, int> CreateTestObject()
    {
      DictionaryWithMRU<string, int> sut = new DictionaryWithMRU<string, int>();
      sut.Add("AAA", 1);
      sut.Add("BBB", 2);
      sut.Add("CCC", 3);
      sut.Add("DDD", 4);
      sut.Remove("BBB");
      return sut;
    }

    [Test]
    public void Keys_Count()
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();
      Assert.AreEqual(3, sut.Keys.Count);
    }

    [TestCase("AAA", true)]
    [TestCase("BBB", false)]
    [TestCase("CCC", true)]
    [TestCase("DDD", true)]
    public void Keys_Contains(string key, bool wantedRes)
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();
      Assert.AreEqual(wantedRes, sut.Keys.Contains(key));
    }

    [Test]
    public void Keys_ToArray()
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();
      string[] wanted = new string[] { "DDD", "CCC", "AAA" };
      Assert.AreEqual(wanted, sut.Keys.ToArray());
    }

    [Test]
    public void Keys_GetEnumerable()
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();

      List<string> lst = new List<string>();
      foreach (string x in sut.Keys)
        lst.Add(x);

      string[] wanted = new string[] { "DDD", "CCC", "AAA" };
      CollectionAssert.AreEqual(wanted, lst.ToArray());
    }

    [Test]
    public void Keys_CopyTo()
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();
      string[] res = new string[5];
      ArrayTools.FillArray<string>(res, "XXX");
      sut.Keys.CopyTo(res, 1);

      string[] wanted = new string[] { "XXX", "DDD", "CCC", "AAA", "XXX" };
      CollectionAssert.AreEqual(wanted, res);
    }

    [Test]
    public void Keys_ICollection_CopyTo()
    {
      DictionaryWithMRU<int, int> sut = new DictionaryWithMRU<int, int>();
      sut.Add(10, 1);
      sut.Add(20, 2);
      double[] res = new double[5];
      ((ICollection)(sut.Keys)).CopyTo(res, 1);

      double[] wanted = new double[] { 0.0, 20.0, 10.0, 0.0, 0.0 };
      CollectionAssert.AreEqual(wanted, res);
    }

    [Test]
    public void Values_Count()
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();
      Assert.AreEqual(3, sut.Values.Count);
    }

    [TestCase(1, true)]
    [TestCase(2, false)]
    [TestCase(3, true)]
    [TestCase(4, true)]
    public void Values_Contains(int value, bool wantedRes)
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();
      Assert.AreEqual(wantedRes, sut.Values.Contains(value));
    }

    [Test]
    public void Values_ToArray()
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();
      int[] wanted = new int[] { 4, 3, 1 };
      Assert.AreEqual(wanted, sut.Values.ToArray());
    }

    [Test]
    public void Values_GetEnumerable()
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();

      List<int> lst = new List<int>();
      foreach (int x in sut.Values)
        lst.Add(x);

      int[] wanted = new int[] { 4, 3, 1 };
      CollectionAssert.AreEqual(wanted, lst.ToArray());
    }

    [Test]
    public void Values_CopyTo()
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();
      int[] res = new int[5];
      ArrayTools.FillArray<int>(res, 666);
      sut.Values.CopyTo(res, 1);

      int[] wanted = new int[] { 666, 4, 3, 1, 666 };
      CollectionAssert.AreEqual(wanted, res);
    }

    [Test]
    public void Values_ICollection_CopyTo()
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();
      float[] res = new float[6];
      ((ICollection)(sut.Values)).CopyTo(res, 2); // метод должен уметь преобразовывать значения

      float[] wanted = new float[] { 0f, 0f, 4f, 3f, 1f, 0f };
      CollectionAssert.AreEqual(wanted, res);
    }

    #endregion

    #region Clear()

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

    #endregion

    #region Перечислитель

    [Test]
    public void GetEnumerator()
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();
      string[] wantedKeys = new string[] { "DDD", "CCC", "AAA" };
      int[] wantedValues = new int[] { 4, 3, 1 };

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
    public void IDictionary_GetEnumerator()
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();
      string[] wantedKeys = new string[] { "DDD", "CCC", "AAA" };
      int[] wantedValues = new int[] { 4, 3, 1 };

      List<string> realKeys = new List<string>();
      List<int> realValues = new List<int>();
      foreach (object x in (IDictionary)sut)
      {
        Assert.IsInstanceOf<DictionaryEntry>(x, "DictionaryEntry type");
        DictionaryEntry de = (DictionaryEntry)x; 
        Assert.IsInstanceOf<string>(de.Key, "DictionaryEntry.Key type");
        Assert.IsInstanceOf<int>(de.Value, "DictionaryEntry.Value type");

        realKeys.Add((string)(de.Key));
        realValues.Add((int)(de.Value));
      }

      CollectionAssert.AreEqual(wantedKeys, realKeys, "Keys");
      CollectionAssert.AreEqual(wantedValues, realValues, "Values");
    }

    #endregion

    #region CopyTo()

    [Test]
    public void CopyTo()
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();
      KeyValuePair<string, int>[] a = new KeyValuePair<string, int>[6];
      ((IDictionary<string, int>)sut).CopyTo(a, 2);

      string[] wantedKeys = new string[6] { null, null, "DDD", "CCC", "AAA", null };
      int[] wantedValues = new int[6] { 0, 0, 4, 3, 1, 0 };

      for (int i = 0; i < a.Length; i++)
      {
        Assert.AreEqual(wantedKeys[i], a[i].Key, "Key[" + i.ToString() + "]");
        Assert.AreEqual(wantedValues[i], a[i].Value, "Value[" + i.ToString() + "]");
      }
    }

    [Test]
    public void ICollection_CopyTo()
    {
      DictionaryWithMRU<string, int> sut = CreateTestObject();
      DictionaryEntry[] a = new DictionaryEntry[6];
      ((IDictionary)sut).CopyTo(a, 2);

      string[] wantedKeys = new string[6] { null, null, "DDD", "CCC", "AAA", null };
      int?[] wantedValues = new int?[6] { null, null, 4, 3, 1, null };

      for (int i = 0; i < a.Length; i++)
      {
        Assert.AreEqual(wantedKeys[i], a[i].Key, "Key[" + i.ToString() + "]");
        Assert.AreEqual(wantedValues[i], a[i].Value, "Value[" + i.ToString() + "]");
      }
    }

    #endregion

    #region Touch()

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

    #endregion
  }
}
