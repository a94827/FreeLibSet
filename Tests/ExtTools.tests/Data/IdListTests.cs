using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using FreeLibSet.Data;
using FreeLibSet.Remoting;
using NUnit.Framework;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class IdListTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Empty()
    {
      IdList sut = new IdList();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, sut.SingleId, "SingleId");
      Assert.AreEqual(DataTools.EmptyIds, sut.ToArray(), "ToArray()");
    }

    [Test]
    public void Constructor_Collection()
    {
      ICollection<Int32> src = new Int32[] { 1, 3, 5, 3, 0 };
      IdList sut = new IdList(src);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, sut.SingleId, "SingleId");

      Int32[] a = sut.ToArray(); // порядок элементов не гарантирован
      Array.Sort<Int32>(a);
      Assert.AreEqual(new Int32[] { 1, 3, 5 }, a, "ToArray()");
    }

    [Test]
    public void Constructor_Enumarable()
    {
      IEnumerable<Int32> src = new ArrayEnumerable<Int32>(new Int32[] { 0, 3, 2, 1, 0, 1, 2, 3 });
      IdList sut = new IdList(src);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, sut.SingleId, "SingleId");

      Int32[] a = sut.ToArray(); // порядок элементов не гарантирован
      Array.Sort<Int32>(a);
      Assert.AreEqual(new Int32[] { 1, 2, 3 }, a, "ToArray()");
    }

    #endregion

    #region Contains()

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(2, false)]
    [TestCase(3, true)]
    public void Contains(Int32 id, bool wantedRes)
    {
      IdList sut = new IdList(new Int32[] { 5, 1, 3 });
      Assert.AreEqual(wantedRes, sut.Contains(id));
    }

    #endregion

    #region CopyTo() / ToArray()

    [Test]
    public void CopyTo()
    {
      IdList sut = new IdList(new Int32[] { 5, 1, 3 });
      Int32[] a = new Int32[sut.Count];
      sut.CopyTo(a, 0);

      Array.Sort<Int32>(a);
      Assert.AreEqual(new Int32[] { 1, 3, 5 }, a);
    }

    [Test]
    public void ToArray()
    {
      IdList sut = new IdList(new Int32[] { 5, 1, 3 });
      Int32[] a1 = sut.ToArray();
      Array.Sort<Int32>(a1);
      Assert.AreEqual(new Int32[] { 1, 3, 5 }, a1, "#1");

      sut.Remove(3);
      Int32[] a2 = sut.ToArray();
      Array.Sort<Int32>(a2);
      Assert.AreEqual(new Int32[] { 1, 5 }, a2, "#2");

      sut.Clear();
      Int32[] a3 = sut.ToArray();
      Array.Sort<Int32>(a3);
      Assert.AreEqual(new Int32[] { }, a3, "#3");
    }

    #endregion

    #region Add() / Remove() / Clear()

    [TestCase(0, "1,3,5")]
    [TestCase(1, "1,3,5")]
    [TestCase(2, "1,2,3,5")]
    public void Add_Id(Int32 id, string sWantedRes)
    {
      Int32[] wantedRes = StdConvert.ToInt32Array(sWantedRes);

      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      sut.Add(id);

      Int32[] res = sut.ToArray();
      Array.Sort<Int32>(res);
      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void Add_IdList()
    {
      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      IdList src = new IdList(new Int32[] { 3, 4, 5, 2 });
      sut.Add(src);

      Int32[] a = sut.ToArray();
      Array.Sort<Int32>(a);
      Assert.AreEqual(a, new Int32[] { 1, 2, 3, 4, 5 });
    }

    [Test]
    public void Add_IdList_Empty()
    {
      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      IdList src = new IdList();
      sut.Add(src);

      Int32[] a = sut.ToArray();
      Array.Sort<Int32>(a);
      Assert.AreEqual(a, new Int32[] { 1, 3, 5 });
    }

    [Test]
    public void Add_IdList_ToEmpty()
    {
      IdList sut = new IdList();
      IdList src = new IdList(new Int32[] { 1, 3, 5 });
      sut.Add(src);

      Int32[] a = sut.ToArray();
      Array.Sort<Int32>(a);
      Assert.AreEqual(a, new Int32[] { 1, 3, 5 });
    }

    [Test]
    public void Add_Enumerable()
    {
      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      IEnumerable<Int32> src = new ArrayEnumerable<Int32>(new Int32[] { 1, 2, 3, 1, 2, 3, 0, 0 });
      sut.Add(src);

      Int32[] a = sut.ToArray();
      Array.Sort<Int32>(a);
      Assert.AreEqual(a, new Int32[] { 1, 2, 3, 5 });
    }


    [TestCase(0, "1,3,5")]
    [TestCase(1, "3,5")]
    [TestCase(2, "1,3,5")]
    public void Remove_Id(Int32 id, string sWantedRes)
    {
      Int32[] wantedRes = StdConvert.ToInt32Array(sWantedRes);

      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      sut.Add(id);

      Int32[] res = sut.ToArray();
      Array.Sort<Int32>(res);
      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void Remove_IdList()
    {
      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      IdList arg = new IdList(new Int32[] { 3, 4, 5 });
      sut.Remove(arg);

      Int32[] a = sut.ToArray();
      Array.Sort<Int32>(a);
      Assert.AreEqual(a, new Int32[] { 1 });
    }

    [Test]
    public void Remove_IdList_Empty()
    {
      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      IdList arg = new IdList();
      sut.Remove(arg);

      Int32[] a = sut.ToArray();
      Array.Sort<Int32>(a);
      Assert.AreEqual(a, new Int32[] { 1, 3, 5 });
    }

    [Test]
    public void Remove_Enumerable()
    {
      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      IEnumerable<Int32> arg = new ArrayEnumerable<Int32>(new Int32[] { 1, 2, 3, 1, 2, 3, 0, 0 });
      sut.Remove(arg);

      Int32[] a = sut.ToArray();
      Array.Sort<Int32>(a);
      Assert.AreEqual(a, new Int32[] { 5 });
    }

    [Test]
    public void RemoveOthers_IdList()
    {
      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      IdList arg = new IdList(new Int32[] { 3, 4, 5 });
      sut.Remove(arg);

      Int32[] a = sut.ToArray();
      Array.Sort<Int32>(a);
      Assert.AreEqual(a, new Int32[] { 3, 5 });
    }

    [Test]
    public void RemoveOthers_IdList_Empty()
    {
      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      IdList arg = new IdList();
      sut.Remove(arg);

      Int32[] a = sut.ToArray();
      Array.Sort<Int32>(a);
      Assert.AreEqual(a, new Int32[] { });
    }

    [Test]
    public void RemoveOthers_Enumerable()
    {
      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      IEnumerable<Int32> arg = new ArrayEnumerable<Int32>(new Int32[] { 1, 2, 3, 1, 2, 3, 0, 0 });
      sut.Remove(arg);

      Int32[] a = sut.ToArray();
      Array.Sort<Int32>(a);
      Assert.AreEqual(a, new Int32[] { 1, 3 });
    }


    [Test]
    public void Clear()
    {
      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      sut.Clear();
      Assert.AreEqual(0, sut.Count);
    }

    #endregion

    #region GetEnumerator()

    [Test]
    public void GetEnumerator()
    {
      IdList sut = new IdList(new Int32[] { 3, 1, 5 });

      List<Int32> lst = new List<Int32>();
      foreach (Int32 id in sut)
        lst.Add(id);

      lst.Sort();
      Assert.AreEqual(new Int32[] { 1, 3, 5 }, lst.ToArray());
    }


    [Test]
    public void GetEnumerator_Empty()
    {
      IdList sut = new IdList();
      List<Int32> lst = new List<Int32>();
      foreach (Int32 id in sut)
        lst.Add(id);

      Assert.AreEqual(0, lst.Count);
    }

    !!!!

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      IdList sut = new IdList();
      sut.Add(1);
      sut.Add(3);
      sut.Add(5);

      byte[] b = SerializationTools.SerializeBinary(sut);

      IdList res = (IdList)(SerializationTools.DeserializeBinary(b));
      Int32[] a = res.ToArray(); // порядок элементов не гарантирован
      Array.Sort<Int32>(a);
      Assert.AreEqual(new Int32[] { 1, 3, 5 }, a, "ToArray()");
    }

    #endregion
  }
}
