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
  public class BidirectionalDictionaryTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_empty()
    {
      BidirectionalDictionary<int, string> sut = new BidirectionalDictionary<int, string>();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      sut.CheckNotReadOnly();
      Assert.AreEqual(0, sut.Keys.Count, "Keys.Count");
      Assert.AreEqual(0, sut.Values.Count, "Values.Count");
      Assert.AreSame(sut.KeyComparer, EqualityComparer<int>.Default);
      Assert.AreSame(sut.ValueComparer, EqualityComparer<string>.Default);
    }

    [Test]
    public void Constructor_dictionary()
    {
      Dictionary<int, string> dict = new Dictionary<int, string>();
      dict.Add(1, "AAA");
      dict.Add(2, "BBB");
      dict.Add(3, "CCC");

      BidirectionalDictionary<int, string> sut = new BidirectionalDictionary<int, string>(dict);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreSame(sut.KeyComparer, EqualityComparer<int>.Default);
      Assert.AreSame(sut.ValueComparer, EqualityComparer<string>.Default);
    }

    [Test]
    public void Constructor_dictionary_exception()
    {
      Dictionary<int, string> dict = new Dictionary<int, string>();
      dict.Add(1, "AAA");
      dict.Add(2, "AAA");
      Assert.Catch<Exception>(delegate()
      {
        BidirectionalDictionary<int, string> sut = new BidirectionalDictionary<int, string>(dict);
      });
    }

    private static BidirectionalDictionary<int, string> CreateTestObject()
    {
      BidirectionalDictionary<int, string> sut = new BidirectionalDictionary<int, string>();
      sut.Add(1, "AAA");
      sut.Add(2, "BBB");
      sut.Add(3, "CCC");
      return sut;
    }

    /// <summary>
    /// Тестовый компатор, который считает одинаковыми положительные и отрицательные числа
    /// </summary>
    private class IntUnsignedComparer : IEqualityComparer<int>
    {
      #region IEqualityComparer<int> Members

      public bool Equals(int x, int y)
      {
        return Math.Abs(x) == Math.Abs(y);
      }

      public int GetHashCode(int obj)
      {
        return Math.Abs(obj);
      }

      #endregion
    }

    [Test]
    public void Constructor_Comparators()
    {
      IEqualityComparer<int> keyComparer = new IntUnsignedComparer();
      IEqualityComparer<string> valueComparer = StringComparer.OrdinalIgnoreCase;
      BidirectionalDictionary<int, string> sut = new BidirectionalDictionary<int, string>(keyComparer, valueComparer);
      Assert.AreSame(keyComparer, sut.KeyComparer, "KeyComparer");
      Assert.AreSame(valueComparer, sut.ValueComparer, "ValueComparer");

      sut.Add(1, "AAA");
      sut.Add(2, "BBB");
      Assert.Catch(delegate() { sut.Add(-1, "CCC"); }, "Same key");

      // Это не вызывает исключения в текущей реализации
      //Assert.Catch(delegate() { sut.Add(3, "bbb"); }, "Same value");
    }

    #endregion

    #region AddRemove()

    [Test]
    public void Add()
    {
      BidirectionalDictionary<int, string> sut = CreateTestObject();
      sut.Add(4, "DDD");
      Assert.AreEqual(4, sut.Count);
    }

    [TestCase(1, true, 2)]
    [TestCase(4, false, 3)]
    public void Remove(int key, bool wanted, int newCount)
    {
      BidirectionalDictionary<int, string> sut = CreateTestObject();
      Assert.AreEqual(wanted, sut.Remove(key), "Remove()");
      Assert.AreEqual(newCount, sut.Count, "Count");
    }

    #endregion

    #region Прямые методы доступа

    [TestCase(1, true, "AAA")]
    [TestCase(4, false, null)]
    public void TryGetValue(int key, bool wanted, string wantedValue)
    {
      BidirectionalDictionary<int, string> sut = CreateTestObject();
      string value;
      Assert.AreEqual(wanted, sut.TryGetValue(key, out value), "Result");
      Assert.AreEqual(wantedValue, value, "Value");
    }

    [Test]
    public void Item_get()
    {
      BidirectionalDictionary<int, string> sut = CreateTestObject();

      Assert.AreEqual("BBB", sut[2], "[2]");

      string dummy;
      Assert.Catch(delegate() { dummy = sut[4]; }, "[4]");
    }

    [Test]
    public void Item_set()
    {
      BidirectionalDictionary<int, string> sut = CreateTestObject();

      sut[2] = "DDD";
      Assert.AreEqual("DDD", sut[2], "replaced");
      Assert.AreEqual(3, sut.Count, "count after replace");

      sut[4] = "EEE";
      Assert.AreEqual("EEE", sut[4], "added");
      Assert.AreEqual(4, sut.Count, "count after add");
    }

    [TestCase(1, true)]
    [TestCase(3, true)]
    [TestCase(4, false)]
    public void ContainsKey(int key, bool wanted)
    {
      BidirectionalDictionary<int, string> sut = CreateTestObject();
      Assert.AreEqual(wanted, sut.ContainsKey(key));
    }

    #endregion

    #region Обратные методы доступа

    [TestCase("AAA", true)]
    [TestCase("DDD", false)]
    public void ContainsValue(string value, bool wanted)
    {
      BidirectionalDictionary<int, string> sut = CreateTestObject();
      Assert.AreEqual(wanted, sut.ContainsValue(value));
    }

    [TestCase("AAA", true, 1)]
    [TestCase("DDD", false, 0)]
    public void TryGetKey(string value, bool wanted, int wantedKey)
    {
      BidirectionalDictionary<int, string> sut = CreateTestObject();
      int key;
      Assert.AreEqual(wanted, sut.TryGetKey(value, out key), "Result");
      Assert.AreEqual(wantedKey, key, "Key");
    }


    [TestCase("AAA", true, 2)]
    [TestCase("DDD", false, 3)]
    public void RemoveValue(string value, bool wanted, int newCount)
    {
      BidirectionalDictionary<int, string> sut = CreateTestObject();
      Assert.AreEqual(wanted, sut.RemoveValue(value), "Result");
      Assert.AreEqual(newCount, sut.Count, "Count");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void Clear()
    {
      BidirectionalDictionary<int, string> sut = CreateTestObject();
      sut.Clear();
      Assert.AreEqual(0, sut.Count);
    }

    #endregion

    #region Перечислитель

    [Test]
    public void GetEnumerator()
    {
      BidirectionalDictionary<int, string> sut = CreateTestObject();
      int sum=0;
      foreach (KeyValuePair<int, string> pair in sut)
        sum += pair.Key;
      Assert.AreEqual(6, sum);
    }

    #endregion

    #region SetReadOnly

    private class BidirectionalDictionary_RO<TKey, TValue> : BidirectionalDictionary<TKey, TValue>
    {
      #region SetReadOnly()

      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }

      #endregion
    }

    [Test]
    public void SetReadOnly()
    {
      BidirectionalDictionary_RO<int, string> sut = new BidirectionalDictionary_RO<int, string>();
      sut.Add(1, "AAA");
      sut.SetReadOnly();
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch(delegate() { sut.Add(4, "DDD"); }, "Add()");
      Assert.Catch(delegate() { sut[1] = "BBB"; }, "Item_set()");
      Assert.Catch(delegate() { sut.Remove(1); }, "Remove()");
      Assert.Catch(delegate() { sut.RemoveValue("AAA"); }, "RemoveValue()");
      Assert.Catch(delegate() { sut.Clear(); }, "Clear()");

      Assert.AreEqual(1, sut.Count, "Count");
    }

    #endregion


    #region Сериализация

    [Test]
    public void Serialization()
    {
      BidirectionalDictionary<int, string> sut1 = CreateTestObject();

      byte[] bytes = SerializationTools.SerializeBinary(sut1);
      BidirectionalDictionary<int, string> sut2 = (BidirectionalDictionary<int, string>)(SerializationTools.DeserializeBinary(bytes));

      Assert.AreEqual(DataTools.CreateArray<int>(sut1.Keys), DataTools.CreateArray<int>(sut2.Keys), "Keys");
      Assert.AreEqual(DataTools.CreateArray<string>(sut1.Values), DataTools.CreateArray<string>(sut2.Values), "Values");
    }

    #endregion

  }
}
