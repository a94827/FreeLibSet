using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.Remoting;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  public class NamedCollectionTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Simple()
    {
      NamedCollection<ObjectWithCode> sut = new NamedCollection<ObjectWithCode>();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.AreEqual(0, sut.GetCodes().Length, "GetCodes()");
      Assert.IsFalse(sut.IgnoreCase, "IgnoreCase");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Constructor_IgnoreCase([Values(false, true)]bool ignoreCase)
    {
      NamedCollection<ObjectWithCode> sut1 = new NamedCollection<ObjectWithCode>(ignoreCase);
      Assert.AreEqual(ignoreCase, sut1.IgnoreCase, "IgnoreCase #1");
      Assert.IsFalse(sut1.IsReadOnly, "IsReadOnly #1");

      NamedCollection<ObjectWithCode> sut2 = new NamedCollection<ObjectWithCode>(100, ignoreCase);
      Assert.AreEqual(ignoreCase, sut2.IgnoreCase, "IgnoreCase #2");
      Assert.IsFalse(sut2.IsReadOnly, "IsReadOnly #2");
    }

    [Test]
    public void Constructor_Collection_1()
    {
      ObjectWithCode[] a = new ObjectWithCode[3] { new ObjectWithCode("AAA"), new ObjectWithCode("bbB"), new ObjectWithCode("Ccc") };
      ICollection<ObjectWithCode> src = a;

      NamedCollection<ObjectWithCode> sut = new NamedCollection<ObjectWithCode>(src);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IgnoreCase, "IgnoreCase");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Constructor_Collection_2([Values(false, true)]bool ignoreCase)
    {
      ObjectWithCode[] a = new ObjectWithCode[3] { new ObjectWithCode("AAA"), new ObjectWithCode("bbB"), new ObjectWithCode("Ccc") };
      ICollection<ObjectWithCode> src = a;

      NamedCollection<ObjectWithCode> sut = new NamedCollection<ObjectWithCode>(src, ignoreCase);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(ignoreCase, sut.IgnoreCase, "IgnoreCase");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Constructor_Collection_3([Values(false, true)]bool ignoreCase, [Values(false, true)]bool isReadOnly)
    {
      ObjectWithCode[] a = new ObjectWithCode[3] { new ObjectWithCode("AAA"), new ObjectWithCode("bbB"), new ObjectWithCode("Ccc") };
      ICollection<ObjectWithCode> src = a;

      NamedCollection<ObjectWithCode> sut = new NamedCollection<ObjectWithCode>(src, ignoreCase, isReadOnly);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(ignoreCase, sut.IgnoreCase, "IgnoreCase");
      Assert.AreEqual(isReadOnly, sut.IsReadOnly, "IsReadOnly");
    }

    public void Constructor_Collection_RepeatedCodes([Values(false, true)]bool ignoreCase)
    {
      ObjectWithCode[] a = new ObjectWithCode[3] { new ObjectWithCode("AAA"), new ObjectWithCode("Aaa"), new ObjectWithCode("AAA") };
      ICollection<ObjectWithCode> src = a;

      NamedCollection<ObjectWithCode> sut = new NamedCollection<ObjectWithCode>(src, ignoreCase);
      Assert.AreEqual(ignoreCase ? 1 : 2, sut.Count);
    }

    [Test]
    public void Constructor_Enumerable_1()
    {
      ObjectWithCode[] a = new ObjectWithCode[3] { new ObjectWithCode("AAA"), new ObjectWithCode("bbB"), new ObjectWithCode("Ccc") };
      IEnumerable<ObjectWithCode> src = new ArrayEnumerable<ObjectWithCode>(a);

      NamedCollection<ObjectWithCode> sut = new NamedCollection<ObjectWithCode>(src);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IgnoreCase, "IgnoreCase");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Constructor_Enumerable_2([Values(false, true)]bool ignoreCase)
    {
      ObjectWithCode[] a = new ObjectWithCode[3] { new ObjectWithCode("AAA"), new ObjectWithCode("bbB"), new ObjectWithCode("Ccc") };
      IEnumerable<ObjectWithCode> src = new ArrayEnumerable<ObjectWithCode>(a);

      NamedCollection<ObjectWithCode> sut = new NamedCollection<ObjectWithCode>(src, ignoreCase);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(ignoreCase, sut.IgnoreCase, "IgnoreCase");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Constructor_Enumerable_3([Values(false, true)]bool ignoreCase, [Values(false, true)]bool isReadOnly)
    {
      ObjectWithCode[] a = new ObjectWithCode[3] { new ObjectWithCode("AAA"), new ObjectWithCode("bbB"), new ObjectWithCode("Ccc") };
      IEnumerable<ObjectWithCode> src = new ArrayEnumerable<ObjectWithCode>(a);

      NamedCollection<ObjectWithCode> sut = new NamedCollection<ObjectWithCode>(src, ignoreCase, isReadOnly);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(ignoreCase, sut.IgnoreCase, "IgnoreCase");
      Assert.AreEqual(isReadOnly, sut.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region Тестовый объект

    /// <summary>
    /// Можно было бы использовать класс ObjectWithCode, но он не разрешает пустые значения Code
    /// </summary>
    [Serializable]
    private class TestObj : IObjectWithCode
    {
      #region Конструктор

      public TestObj(string code)
      {
        _Code = code;
      }

      #endregion

      #region Свойства

      public string Code { get { return _Code; } }
      private string _Code;

      #endregion
    }

    [Serializable]
    private class TestCol : NamedCollection<TestObj>
    {
      public TestCol(ICollection<TestObj> src, bool ignoreCase)
        : base(src, ignoreCase)
      {
      }

      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }
    }

    private static TestCol CreateTestObject(bool ignoreCase)
    {
      TestObj[] a = new TestObj[3] { new TestObj("AAA"), new TestObj("bbB"), new TestObj("Ccc") };
      return new TestCol(a, ignoreCase);
    }

    #endregion

    #region Item, TryGetValue(), GetRequired()

    [TestCase("AAA", false)]
    [TestCase("Aaa", true)]
    public void Item_getByCode_TryGetValue_ok(string code, bool ignoreCase)
    {
      TestCol sut = CreateTestObject(ignoreCase);

      TestObj obj1 = sut[code];
      Assert.IsNotNull(obj1, "Item get() is not null");
      Assert.AreEqual(code.ToUpperInvariant(), obj1.Code.ToUpperInvariant(), "Code");

      TestObj obj2;
      bool res2 = sut.TryGetValue(code, out obj2);
      Assert.IsTrue(res2, "TryGetValue() result");
      Assert.AreSame(obj1, obj2, "Same object");
    }

    [TestCase("Aaa", false)]
    [TestCase("", false)]
    public void Item_getByCode_TryGetValue_notFound(string code, bool ignoreCase)
    {
      TestCol sut = CreateTestObject(ignoreCase);

      TestObj obj1 = sut[code];
      Assert.IsNull(obj1, "Item get()");

      TestObj obj2;
      bool res2 = sut.TryGetValue(code, out obj2);
      Assert.IsFalse(res2, "TryGetValue() result");
      Assert.IsNull(obj2, "TryGetValue() object");
    }

    [Test]
    public void GetRequired_ok()
    {
      TestCol sut = CreateTestObject(false);
      TestObj obj = sut.GetRequired("AAA");
      Assert.AreEqual("AAA", obj.Code);
    }

    [Test]
    public void GetRequired_notFound()
    {
      TestCol sut = CreateTestObject(false);
      TestObj obj;
      Assert.Catch<KeyNotFoundException>(delegate() { obj = sut.GetRequired("DDD"); });
    }

    [Test]
    public void GetRequired_emptyString()
    {
      TestCol sut = CreateTestObject(false);
      TestObj obj;
      Assert.Catch(delegate() { obj = sut.GetRequired(""); });
    }

    #endregion

    #region Contains()

    [TestCase("AAA", false, true)]
    [TestCase("Aaa", false, false)]
    [TestCase("Aaa", true, true)]
    [TestCase("", false, false)]
    [TestCase(null, false, false)]
    public void Contains_code(string code, bool ignoreCase, bool wantedRes)
    {
      TestCol sut = CreateTestObject(ignoreCase);
      bool res = sut.Contains(code);
      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void Contains_object()
    {
      TestCol sut = CreateTestObject(false);

      TestObj obj1 = sut["AAA"]; 
      Assert.IsTrue(sut.Contains(obj1), "Contains() #1");

      TestObj obj2 = new TestObj(obj1.Code);
      Assert.AreNotEqual(obj1, obj2, "Object #2 is not equal #1");

      Assert.IsFalse(sut.Contains(obj2), "Contains() #2");
    }

    [Test]
    public void Contains_object_null()
    {
      TestCol sut = CreateTestObject(false);
      TestObj obj = null;
      Assert.IsFalse(sut.Contains(obj));
    }

    #endregion

    #region GetCodes()

    [Test]
    public void GetCodes([Values(false, true)]bool ignoreCase)
    {
      TestCol sut = CreateTestObject(ignoreCase);
      string[] a = sut.GetCodes();
      CollectionAssert.AreEquivalent(new string[] { "AAA", "bbB", "Ccc" }, a);
    }

    #endregion

    #region Add()

    [TestCase("DDD", false)]
    [TestCase("BBB", false)]
    public void Add_ok(string code, bool ignoreCase)
    {
      TestCol sut = CreateTestObject(ignoreCase);
      TestObj obj = new TestObj(code);

      sut.Add(obj);
      Assert.AreEqual(4, sut.Count, "Count");
      Assert.IsTrue(sut.Contains(code), "Contains()");
    }

    [TestCase("bbB", false)]
    [TestCase("BBB", true)]
    [TestCase("CCC", true)]
    [TestCase("", false)]
    public void Add_exception(string code, bool ignoreCase)
    {
      TestCol sut = CreateTestObject(ignoreCase);
      TestObj obj = new TestObj(code);

      Assert.Catch(delegate() { sut.Add(obj); }, "Add()");
      Assert.AreEqual(3, sut.Count, "Count");
    }

    [TestCase("DDD,BBB,CCC", false)]
    [TestCase("DDD", true)]
    public void AddRange_ok(string codes, bool ignoreCase)
    {
      string[] aCodes = codes.Split(',');
      TestObj[] a = new TestObj[aCodes.Length];
      for (int i = 0; i < aCodes.Length; i++)
        a[i] = new TestObj(aCodes[i]);

      TestCol sut = CreateTestObject(ignoreCase);
      sut.AddRange(a);
      Assert.AreEqual(3 + aCodes.Length, sut.Count, "Count");
    }

    [TestCase("AAA", false)]
    [TestCase("bbB", true)]
    public void AddRange_exception(string codes, bool ignoreCase)
    {
      string[] aCodes = codes.Split(',');
      TestObj[] a = new TestObj[aCodes.Length];
      for (int i = 0; i < aCodes.Length; i++)
        a[i] = new TestObj(aCodes[i]);

      TestCol sut = CreateTestObject(ignoreCase);
      Assert.Catch(delegate() { sut.AddRange(a); });

      // Количество элементов не определено, т.к. метод мог выполниться частично
      // Assert.AreEqual(3, sut.Count, "Count");
    }

    [Test]
    public void AddRange_emptyCollection()
    {
      TestObj[] a = new TestObj[0];
      TestCol sut = CreateTestObject(false);
      Assert.DoesNotThrow(delegate() { sut.AddRange(a); });
    }

    #endregion

    #region Remove()

    [TestCase("AAA", false, true)]
    [TestCase("Aaa", false, false)]
    [TestCase("Aaa", true, true)]
    [TestCase("", false, false)]
    public void Remove_code(string code, bool ignoreCase, bool wantedRes)
    {
      TestCol sut = CreateTestObject(ignoreCase);
      bool res = sut.Remove(code);

      Assert.AreEqual(wantedRes, res, "Result");

      Assert.IsFalse(sut.Contains(code), "Contains()");
      Assert.AreEqual(wantedRes ? 2 : 3, sut.Count, "Count");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Remove_object(bool isOurs)
    {
      TestCol sut = CreateTestObject(false);
      TestObj obj = sut["AAA"];
      if (!isOurs)
        obj = new TestObj(obj.Code);

      bool res = sut.Remove(obj);

      Assert.AreEqual(isOurs, res);
    }

    [Test]
    public void Remove_object_null()
    {
      TestCol sut = CreateTestObject(false);
      TestObj obj = null;
      Assert.IsFalse(sut.Remove(obj));
    }

    #endregion

    #region Clear()

    [Test]
    public void Clear()
    {
      TestCol sut = CreateTestObject(false);
      Assert.IsTrue(sut.Contains("AAA"), "Contains() #1");
      sut.Clear();

      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.Contains("AAA"), "Contains() #2");
    }

    #endregion

    #region GetEnumerator()

    [Test]
    public void GetEnumerator([Values(false, true)]bool ignoreCase)
    {
      TestCol sut = CreateTestObject(ignoreCase);
      List<string> lst = new List<string>();
      foreach (TestObj item in sut)
        lst.Add(item.Code);

      CollectionAssert.AreEquivalent(new string[] { "AAA", "bbB", "Ccc" }, lst);
    }

    #endregion

    #region CopyTo() / ToArray()

    public void CopyTo_1([Values(false, true)]bool ignoreCase)
    {
      TestCol sut = CreateTestObject(ignoreCase);
      TestObj[] a = new TestObj[4];
      sut.CopyTo(a);
      CollectionAssert.AreEquivalent(a, new string[] { "AAA", "bbB", "Ccc", null }, "#1");
      Assert.IsNull(a[3], "#2");
    }

    public void CopyTo_2([Values(false, true)]bool ignoreCase)
    {
      TestCol sut = CreateTestObject(ignoreCase);
      TestObj[] a = new TestObj[4];
      sut.CopyTo(a, 1);
      CollectionAssert.AreEquivalent(a, new string[] { "AAA", "bbB", "Ccc", null }, "#1");
      Assert.IsNull(a[0], "#2");
    }

    public void ToArray([Values(false, true)]bool ignoreCase)
    {
      TestCol sut = CreateTestObject(ignoreCase);
      TestObj[] a = sut.ToArray();
      Assert.AreEqual(3, a.Length, "Length");
      CollectionAssert.AreEquivalent(a, new string[] { "AAA", "bbB", "Ccc"});
    }

    #endregion

    #region SetReadOnly()

    [Test]
    public void SetReadOnly()
    {
      TestCol sut = CreateTestObject(false);
      Assert.DoesNotThrow(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly() #1");

      sut.SetReadOnly();

      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadObly() #2");
      Assert.Catch(delegate() { sut.Add(new TestObj("DDD")); }, "Add()");
      Assert.Catch(delegate() { sut.AddRange(new TestObj[1] { new TestObj("DDD") }); }, "AddRange()");
      Assert.Catch(delegate() { sut.Remove("AAA"); }, "Remove(string)");
      Assert.Catch(delegate() { sut.Remove(sut["AAA"]); }, "Remove(<T>)");
      Assert.Catch(delegate() { sut.Clear(); }, "Clear()");
      Assert.AreEqual(3, sut.Count, "Count");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization([Values(false, true)]bool ignoreCase, [Values(false, true)]bool isReadOnly)
    {
      TestCol sut1 = CreateTestObject(ignoreCase);
      if (isReadOnly)
        sut1.SetReadOnly();

      byte[] b = SerializationTools.SerializeBinary(sut1);
      TestCol sut2 = SerializationTools.DeserializeBinary(b) as TestCol;

      Assert.AreEqual(3, sut2.Count, "Count");
      CollectionAssert.AreEquivalent(sut2.GetCodes(), new string[] { "AAA", "bbB", "Ccc" });
      Assert.AreEqual(ignoreCase, sut2.IgnoreCase, "IgnoreCase");
      Assert.AreEqual(isReadOnly, sut2.IsReadOnly, "IsReadOnly");
    }

    #endregion
  }
}
