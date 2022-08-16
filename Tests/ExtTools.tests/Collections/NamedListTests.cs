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
  public class NamedListTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Simple()
    {
      NamedList<ObjectWithCode> sut = new NamedList<ObjectWithCode>();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.AreEqual(0, sut.GetCodes().Length, "GetCodes()");
      Assert.IsFalse(sut.IgnoreCase, "IgnoreCase");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Constructor_IgnoreCase([Values(false, true)]bool ignoreCase)
    {
      NamedList<ObjectWithCode> sut1 = new NamedList<ObjectWithCode>(ignoreCase);
      Assert.AreEqual(ignoreCase, sut1.IgnoreCase, "IgnoreCase #1");
      Assert.IsFalse(sut1.IsReadOnly, "IsReadOnly #1");

      NamedList<ObjectWithCode> sut2 = new NamedList<ObjectWithCode>(100, ignoreCase);
      Assert.AreEqual(ignoreCase, sut2.IgnoreCase, "IgnoreCase #2");
      Assert.IsFalse(sut2.IsReadOnly, "IsReadOnly #2");
    }

    [Test]
    public void Constructor_Collection_1()
    {
      ObjectWithCode[] a = new ObjectWithCode[3] { new ObjectWithCode("AAA"), new ObjectWithCode("bbB"), new ObjectWithCode("Ccc") };
      ICollection<ObjectWithCode> src = a;

      NamedList<ObjectWithCode> sut = new NamedList<ObjectWithCode>(src);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IgnoreCase, "IgnoreCase");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Constructor_Collection_2([Values(false, true)]bool ignoreCase)
    {
      ObjectWithCode[] a = new ObjectWithCode[3] { new ObjectWithCode("AAA"), new ObjectWithCode("bbB"), new ObjectWithCode("Ccc") };
      ICollection<ObjectWithCode> src = a;

      NamedList<ObjectWithCode> sut = new NamedList<ObjectWithCode>(src, ignoreCase);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(ignoreCase, sut.IgnoreCase, "IgnoreCase");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Constructor_Collection_3([Values(false, true)]bool ignoreCase, [Values(false, true)]bool isReadOnly)
    {
      ObjectWithCode[] a = new ObjectWithCode[3] { new ObjectWithCode("AAA"), new ObjectWithCode("bbB"), new ObjectWithCode("Ccc") };
      ICollection<ObjectWithCode> src = a;

      NamedList<ObjectWithCode> sut = new NamedList<ObjectWithCode>(src, ignoreCase, isReadOnly);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(ignoreCase, sut.IgnoreCase, "IgnoreCase");
      Assert.AreEqual(isReadOnly, sut.IsReadOnly, "IsReadOnly");
    }

    public void Constructor_Collection_RepeatedCodes([Values(false, true)]bool ignoreCase)
    {
      ObjectWithCode[] a = new ObjectWithCode[3] { new ObjectWithCode("AAA"), new ObjectWithCode("Aaa"), new ObjectWithCode("AAA") };
      ICollection<ObjectWithCode> src = a;

      NamedList<ObjectWithCode> sut = new NamedList<ObjectWithCode>(src, ignoreCase);
      Assert.AreEqual(ignoreCase ? 1 : 2, sut.Count);
    }

    [Test]
    public void Constructor_Enumerable_1()
    {
      ObjectWithCode[] a = new ObjectWithCode[3] { new ObjectWithCode("AAA"), new ObjectWithCode("bbB"), new ObjectWithCode("Ccc") };
      IEnumerable<ObjectWithCode> src = new ArrayEnumerable<ObjectWithCode>(a);

      NamedList<ObjectWithCode> sut = new NamedList<ObjectWithCode>(src);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IgnoreCase, "IgnoreCase");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Constructor_Enumerable_2([Values(false, true)]bool ignoreCase)
    {
      ObjectWithCode[] a = new ObjectWithCode[3] { new ObjectWithCode("AAA"), new ObjectWithCode("bbB"), new ObjectWithCode("Ccc") };
      IEnumerable<ObjectWithCode> src = new ArrayEnumerable<ObjectWithCode>(a);

      NamedList<ObjectWithCode> sut = new NamedList<ObjectWithCode>(src, ignoreCase);
      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(ignoreCase, sut.IgnoreCase, "IgnoreCase");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Constructor_Enumerable_3([Values(false, true)]bool ignoreCase, [Values(false, true)]bool isReadOnly)
    {
      ObjectWithCode[] a = new ObjectWithCode[3] { new ObjectWithCode("AAA"), new ObjectWithCode("bbB"), new ObjectWithCode("Ccc") };
      IEnumerable<ObjectWithCode> src = new ArrayEnumerable<ObjectWithCode>(a);

      NamedList<ObjectWithCode> sut = new NamedList<ObjectWithCode>(src, ignoreCase, isReadOnly);
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
    private class TestList : NamedList<TestObj>
    {
      public TestList(ICollection<TestObj> src, bool ignoreCase)
        : base(src, ignoreCase)
      {
      }

      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }
    }

    private static TestList CreateTestObject(bool ignoreCase)
    {
      TestObj[] a = new TestObj[3] { new TestObj("AAA"), new TestObj("bbB"), new TestObj("Ccc") };
      return new TestList(a, ignoreCase);
    }

    #endregion

    #region Item, TryGetValue(), GetRequired()

    [Test]
    public void Item_getByIndex_ok()
    {
      TestList sut = CreateTestObject(false);
      TestObj res = sut[0];
      Assert.AreEqual("AAA", res.Code);
    }

    [Test]
    public void Item_getByIndex_out_of_range()
    {
      TestList sut = CreateTestObject(false);
      TestObj res;
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { res = sut[-1]; });
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { res = sut[3]; });
    }

    [TestCase(0, "AAA", false)]
    [TestCase(1, "DDD", true)]
    [TestCase(0, "BBB", false)]
    [TestCase(1, "bbB", true)]
    [TestCase(2, "Ccc", true)]
    public void Item_setByIndex_ok(int index, string code, bool ignoreCase)
    {
      TestObj value = new TestObj(code);
      TestList sut = CreateTestObject(ignoreCase);
      sut[index] = value;

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.AreEqual(index, sut.IndexOf(code), "IndexOf()");
    }

    [TestCase(0, "bbB", true)]
    [TestCase(1, "AAA", false)]
    public void Item_setByIndex_exception(int index, string code, bool ignoreCase)
    {
      TestObj value = new TestObj(code);
      TestList sut = CreateTestObject(ignoreCase);
      Assert.Catch(delegate() { sut[index] = value; });
      Assert.AreEqual(3, sut.Count, "Count");
    }

    [Test]
    public void Item_setByIndex_null_exception()
    {
      TestObj value = null;
      TestList sut = CreateTestObject(false);
      Assert.Catch<ArgumentNullException>(delegate() { sut[0] = value; });
      Assert.AreEqual(3, sut.Count, "Count");
    }

    [TestCase("AAA", false)]
    [TestCase("Aaa", true)]
    public void Item_getByCode_TryGetValue_ok(string code, bool ignoreCase)
    {
      TestList sut = CreateTestObject(ignoreCase);

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
      TestList sut = CreateTestObject(ignoreCase);

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
      TestList sut = CreateTestObject(false);
      TestObj obj = sut.GetRequired("AAA");
      Assert.AreEqual("AAA", obj.Code);
    }

    [Test]
    public void GetRequired_notFound()
    {
      TestList sut = CreateTestObject(false);
      TestObj obj;
      Assert.Catch<KeyNotFoundException>(delegate() { obj = sut.GetRequired("DDD"); });
    }

    [Test]
    public void GetRequired_emptyString()
    {
      TestList sut = CreateTestObject(false);
      TestObj obj;
      Assert.Catch(delegate() { obj = sut.GetRequired(""); });
    }

    #endregion

    #region Contains() / IndexOf()

    [TestCase("AAA", false, 0)]
    [TestCase("Aaa", false, -1)]
    [TestCase("Aaa", true, 0)]
    [TestCase("", false, -1)]
    [TestCase(null, false, -1)]
    public void IndexOf_Contains_code(string code, bool ignoreCase, int wantedRes)
    {
      TestList sut = CreateTestObject(ignoreCase);

      int res1 = sut.IndexOf(code);
      Assert.AreEqual(wantedRes, res1, "IndexOf()");

      bool res2 = sut.Contains(code);
      Assert.AreEqual(wantedRes >= 0, res2, "Contains()");
    }

    [Test]
    public void IndexOf_Contains_object()
    {
      TestList sut = CreateTestObject(false);

      TestObj obj1 = sut[0]; // "AAA"
      Assert.AreEqual(0, sut.IndexOf(obj1), "IndexOf() #1");
      Assert.IsTrue(sut.Contains(obj1), "Contains() #1");

      TestObj obj2 = new TestObj(obj1.Code);
      Assert.AreNotEqual(obj1, obj2, "Object #2 is not equal #1");

      Assert.AreEqual(-1, sut.IndexOf(obj2), "IndexOf() #2");
      Assert.IsFalse(sut.Contains(obj2), "Contains() #2");
    }

    [Test]
    public void IndexOf_Contains_object_null()
    {
      TestList sut = CreateTestObject(false);

      TestObj obj = null;
      Assert.AreEqual(-1, sut.IndexOf(obj), "IndexOf()");
      Assert.IsFalse(sut.Contains(obj), "Contains()");
    }

    #endregion

    #region GetCodes()

    [Test]
    public void GetCodes([Values(false, true)]bool ignoreCase)
    {
      TestList sut = CreateTestObject(ignoreCase);
      string[] a = sut.GetCodes();
      Assert.AreEqual(new string[] { "AAA", "bbB", "Ccc" }, a);
    }

    #endregion

    #region Add()/ Insert()

    [TestCase("DDD", false)]
    [TestCase("BBB", false)]
    public void Add_ok(string code, bool ignoreCase)
    {
      TestList sut = CreateTestObject(ignoreCase);
      TestObj obj = new TestObj(code);

      sut.Add(obj);
      Assert.AreEqual(4, sut.Count, "Count");
      Assert.AreEqual(3, sut.IndexOf(code), "IndexOf()");
    }

    [TestCase("bbB", false)]
    [TestCase("BBB", true)]
    [TestCase("CCC", true)]
    [TestCase("", false)]
    public void Add_exception(string code, bool ignoreCase)
    {
      TestList sut = CreateTestObject(ignoreCase);
      TestObj obj = new TestObj(code);

      Assert.Catch(delegate() { sut.Add(obj); }, "Add()");
      Assert.AreEqual(3, sut.Count, "Count");
    }


    [TestCase(0, "BBB", false)]
    [TestCase(2, "BBB", false)]
    [TestCase(3, "BBB", false)]
    public void Insert_ok(int index, string code, bool ignoreCase)
    {
      TestList sut = CreateTestObject(ignoreCase);
      TestObj obj = new TestObj(code);

      sut.Insert(index, obj);
      Assert.AreEqual(4, sut.Count, "Count");
      Assert.AreEqual(index, sut.IndexOf(code), "IndexOf()");
    }

    [TestCase(-1, "DDD", false)]
    [TestCase(4, "DDD", false)]
    [TestCase(0, "bbB", true)]
    [TestCase(0, "", false)]
    public void Insert_exception(int index, string code, bool ignoreCase)
    {
      TestList sut = CreateTestObject(ignoreCase);
      TestObj obj = new TestObj(code);

      Assert.Catch(delegate() { sut.Insert(index, obj); }, "Insert()");
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

      TestList sut = CreateTestObject(ignoreCase);
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

      TestList sut = CreateTestObject(ignoreCase);
      Assert.Catch(delegate() { sut.AddRange(a); });

      // Количество элементов не определено, т.к. метод мог выполниться частично
      // Assert.AreEqual(3, sut.Count, "Count");
    }

    [Test]
    public void AddRange_emptyCollection()
    {
      TestObj[] a = new TestObj[0];
      TestList sut = CreateTestObject(false);
      Assert.DoesNotThrow(delegate() { sut.AddRange(a); });
    }

    #endregion

    #region Remove() / RemoveAt()

    [TestCase("AAA", false, true)]
    [TestCase("Aaa", false, false)]
    [TestCase("Aaa", true, true)]
    [TestCase("", false, false)]
    public void Remove_code(string code, bool ignoreCase, bool wantedRes)
    {
      TestList sut = CreateTestObject(ignoreCase);
      bool res = sut.Remove(code);

      Assert.AreEqual(wantedRes, res, "Result");

      Assert.AreEqual(-1, sut.IndexOf(code), "IndexOf()");
      Assert.IsFalse(sut.Contains(code), "Contains()");
      Assert.AreEqual(wantedRes ? 2 : 3, sut.Count, "Count");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Remove_object(bool isOurs)
    {
      TestList sut = CreateTestObject(false);
      TestObj obj = sut[0];
      if (!isOurs)
        obj = new TestObj(obj.Code);

      bool res = sut.Remove(obj);

      Assert.AreEqual(isOurs, res);
    }

    [Test]
    public void Remove_object_null()
    {
      TestList sut = CreateTestObject(false);
      TestObj obj = null;
      Assert.IsFalse(sut.Remove(obj));
    }

    [TestCase(0)]
    [TestCase(2)]
    public void RemoveAt_ok(int index)
    {
      TestList sut = CreateTestObject(false);
      sut.RemoveAt(index);
      Assert.AreEqual(2, sut.Count);
    }

    [TestCase(-1)]
    [TestCase(3)]
    public void RemoveAt_exception(int index)
    {
      TestList sut = CreateTestObject(false);
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { sut.RemoveAt(index); }, "RemoveAt()");
      Assert.AreEqual(3, sut.Count, "Count");
    }

    #endregion

    #region Clear()

    [Test]
    public void Clear()
    {
      TestList sut = CreateTestObject(false);
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
      TestList sut = CreateTestObject(ignoreCase);
      sut.Insert(1, new TestObj("DDD")); // чтобы проверить порядок вывода
      List<string> lst = new List<string>();
      foreach (TestObj item in sut)
        lst.Add(item.Code);

      Assert.AreEqual(new string[] { "AAA", "DDD", "bbB", "Ccc" }, lst.ToArray());
    }

    #endregion

    #region CopyTo() / ToArray()

    public void CopyTo_1([Values(false, true)]bool ignoreCase)
    {
      TestList sut = CreateTestObject(ignoreCase);
      TestObj[] a = new TestObj[4];
      sut.CopyTo(a);
      CheckArrayCodes(a, "AAA", "bbB", "Ccc", null);
    }

    public void CopyTo_2([Values(false, true)]bool ignoreCase)
    {
      TestList sut = CreateTestObject(ignoreCase);
      TestObj[] a = new TestObj[4];
      sut.CopyTo(a, 1);
      CheckArrayCodes(a, null, "AAA", "bbB", "Ccc");
    }

    public void CopyTo_4([Values(false, true)]bool ignoreCase)
    {
      TestList sut = CreateTestObject(ignoreCase);
      TestObj[] a = new TestObj[4];
      sut.CopyTo(1, a, 2, 1);
      CheckArrayCodes(a, null, null, "bbB", null);
    }

    public void ToArray([Values(false, true)]bool ignoreCase)
    {
      TestList sut = CreateTestObject(ignoreCase);
      TestObj[] a = sut.ToArray();
      Assert.AreEqual(3, a.Length, "Length");
      CheckArrayCodes(a, "AAA", "bbB", "Ccc");
    }

    private static void CheckArrayCodes(TestObj[] a, params string[] wantedCodes)
    {
      if (a.Length != wantedCodes.Length)
        throw new ArgumentException("Invalid array length", "wantedCodes");

      for (int i = 0; i < a.Length; i++)
      {
        if (wantedCodes[i] == null)
          Assert.IsNull(a[i], "a[" + i.ToString() + "] expected to be null");
        else
        {
          Assert.IsNotNull(a[i], "a[" + i.ToString() + "] expected to exist");
          Assert.AreEqual(wantedCodes[i], a[i].Code, "a[" + i.ToString() + "].Code");
        }
      }
    }

    #endregion

    #region Reverse()

    [Test]
    public void Reverse([Values(false, true)]bool ignoreCase)
    {
      TestList sut = CreateTestObject(ignoreCase);

      sut.Reverse();

      Assert.AreEqual(3, sut.Count, "Count");
      CheckArrayCodes(sut.ToArray(), "Ccc", "bbB", "AAA");
    }

    #endregion

    #region Sort()

    [TestCase(false, "AAA", "Ccc", "bbB")]
    [TestCase(true, "AAA", "bbB", "Ccc")]
    public void Sort(bool ignoreCase, string code1, string code2, string code3)
    {
      TestList sut = CreateTestObject(ignoreCase);
      sut.Reverse();

      sut.Sort();

      Assert.AreEqual(3, sut.Count, "Count");
      CheckArrayCodes(sut.ToArray(), code1, code2, code3);
    }

    #endregion

    #region SetReadOnly()

    [Test]
    public void SetReadOnly()
    {
      TestList sut = CreateTestObject(false);
      Assert.DoesNotThrow(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly() #1");

      sut.SetReadOnly();

      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual("AAA", sut[0].Code, "Item get");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadObly() #2");
      Assert.Catch(delegate() { sut[0] = new TestObj("DDD"); }, "Item set");
      Assert.Catch(delegate() { sut.Add(new TestObj("DDD")); }, "Add()");
      Assert.Catch(delegate() { sut.AddRange(new TestObj[1] { new TestObj("DDD") }); }, "AddRange()");
      Assert.Catch(delegate() { sut.Insert(1, new TestObj("DDD")); }, "Insert()");
      Assert.Catch(delegate() { sut.Remove("AAA"); }, "Remove(string)");
      Assert.Catch(delegate() { sut.Remove(sut[0]); }, "Remove(<T>)");
      Assert.Catch(delegate() { sut.RemoveAt(0); }, "RemoveAt()");
      Assert.Catch(delegate() { sut.Clear(); }, "Clear()");
      Assert.Catch(delegate() { sut.Reverse(); }, "Reverse()");
      Assert.Catch(delegate() { sut.Sort(); }, "Sort()");
      Assert.AreEqual(3, sut.Count, "Count");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization([Values(false, true)]bool ignoreCase, [Values(false, true)]bool isReadOnly)
    {
      TestList sut1 = CreateTestObject(ignoreCase);
      if (isReadOnly)
        sut1.SetReadOnly();

      byte[] b = SerializationTools.SerializeBinary(sut1);
      TestList sut2 = SerializationTools.DeserializeBinary(b) as TestList;

      Assert.AreEqual(3, sut2.Count, "Count");
      CheckArrayCodes(sut2.ToArray(), "AAA", "bbB", "Ccc");
      Assert.AreEqual(ignoreCase, sut2.IgnoreCase, "IgnoreCase");
      Assert.AreEqual(isReadOnly, sut2.IsReadOnly, "IsReadOnly");
      Assert.IsTrue(sut2.Contains("AAA"), "Contains()"); // internal dictionary restore
    }

    #endregion

    #region Тестирование восстановления внутреннего словаря

    private static TestList CreateTestObjectWithInvalidDict()
    {
      TestList sut = CreateTestObject(true);
      sut.Insert(0, new TestObj("DDD")); // Устанавливает поле "_DictIsValid=false
      return sut;
    }

    [Test]
    public void InternalDict_Add_ok()
    {
      TestList sut = CreateTestObjectWithInvalidDict();
      sut.Add(new TestObj("EEE"));
      Assert.AreEqual(5, sut.Count);
    }

    [Test]
    public void InternalDict_Add_error()
    {
      TestList sut = CreateTestObjectWithInvalidDict();
      Assert.Catch(delegate() { sut.Add(new TestObj("BBB")); });
    }

    [Test]
    public void InternalDict_Insert_ok()
    {
      TestList sut = CreateTestObjectWithInvalidDict();
      sut.Insert(1, new TestObj("EEE"));
      Assert.AreEqual(5, sut.Count);
    }

    [Test]
    public void InternalDict_Insert_error()
    {
      TestList sut = CreateTestObjectWithInvalidDict();
      Assert.Catch(delegate() { sut.Insert(1, new TestObj("BBB")); });
    }

    [Test]
    public void InternalDict_ItemSet_ok()
    {
      TestList sut = CreateTestObjectWithInvalidDict();
      sut[1] = new TestObj("EEE");
      Assert.AreEqual(4, sut.Count);
    }

    [Test]
    public void InternalDict_ItemSet_error()
    {
      TestList sut = CreateTestObjectWithInvalidDict();
      Assert.Catch(delegate() { sut[1] = new TestObj("BBB"); });
    }

    [TestCase("AAA", 1)]
    [TestCase("CCC", 3)]
    [TestCase("DDD", 0)]
    [TestCase("EEE", -1)]
    public void InternalDict_IndexOf_code(string code, int wantedRes)
    {
      TestList sut = CreateTestObjectWithInvalidDict();
      Assert.AreEqual(wantedRes, sut.IndexOf(code));
    }

    // Нужен отдельный тест для Contains(), чтобы проверить восстановление словаря
    [TestCase("AAA", true)]
    [TestCase("CCC", true)]
    [TestCase("DDD", true)]
    [TestCase("EEE", false)]
    public void InternalDict_Contains_code(string code, bool wantedRes)
    {
      TestList sut = CreateTestObjectWithInvalidDict();
      Assert.AreEqual(wantedRes, sut.Contains(code));
    }

    [Test]
    public void InternalDict_Contains_object()
    {
      TestList sut = CreateTestObjectWithInvalidDict();
      TestObj obj = sut[1];
      Assert.IsTrue(sut.Contains(obj));
    }

    [Test]
    public void InternalDict_TryGetValue()
    {
      TestList sut = CreateTestObjectWithInvalidDict();
      TestObj obj;
      sut.TryGetValue("DDD", out obj);
      Assert.IsNotNull(obj);
      Assert.AreEqual("DDD", obj.Code);
    }

    #endregion
  }
}
