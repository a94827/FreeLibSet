using FreeLibSet.Core;
using FreeLibSet.Data;
using FreeLibSet.Remoting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class IdArrayTests
  {
    #region Конструкторы

    // Хотя имеется только одна перегрузка конструктора с аргументом IEnumerable<T>, 
    // требуются отдельные тесты, так как реализация отличается для IEnumerable<T>, IIdSet<T> и другого IdArray<T>

    [Test]
    public void Constructor_Enumarable()
    {
      IEnumerable<Int32> src = new ArrayEnumerable<Int32>(new Int32[] { 0, 3, 2, 1, 0, 1, 2, 3 });
      IdArray<Int32> sut = new IdArray<Int32>(src);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsTrue(((IReadOnlyObject)sut).IsReadOnly, "IsReadOnly");
      CollectionAssert.AreEqual(new Int32[] { 3, 2, 1 }, sut.ToArray(), "ToArray()");
      Assert.AreEqual(0, sut.SingleId, "SingleId");
    }

    [Test]
    public void Constructor_IIdSet()
    {
      IdCollection<Int32> src = new IdCollection<Int32>(new Int32[] { 5, 1, 3 });
      IdArray<Int32> sut = new IdArray<Int32>(src);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsTrue(((IReadOnlyObject)sut).IsReadOnly, "IsReadOnly");
      CollectionAssert.AreEquivalent(new Int32[] { 5, 1, 3 }, sut.ToArray(), "ToArray()");
      Assert.AreEqual(0, sut.SingleId, "SingleId");
    }

    [Test]
    public void Constructor_IdArray()
    {
      IdArray<Int32> src = new IdArray<Int32>(new Int32[] { 5, 1, 3 });
      IdArray<Int32> sut = new IdArray<Int32>(src);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsTrue(((IReadOnlyObject)sut).IsReadOnly, "IsReadOnly");
      CollectionAssert.AreEqual (new Int32[] { 5, 1, 3 }, sut.ToArray(), "ToArray()");
      Assert.AreEqual(0, sut.SingleId, "SingleId");
    }

    [Test]
    public void Constructor_Null()
    {
      IdArray<Int32> sut = new IdArray<Int32>((IEnumerable<Int32>)null);
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsTrue(((IReadOnlyObject)sut).IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, sut.SingleId, "SingleId");
      Assert.AreEqual(EmptyArray<Int32>.Empty, sut.ToArray(), "ToArray()");
    }

    #endregion

    #region Contains(), ContainsAll() / ConatinsAny()

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(2, false)]
    [TestCase(3, true)]
    public void Contains(Int32 id, bool wantedRes)
    {
      IdArray<Int32> sut = new IdArray<Int32>(new Int32[] { 5, 1, 3 });
      Assert.AreEqual(wantedRes, sut.Contains(id));
    }

    [TestCase("1,3,5", "2,3,4", true, "3")]
    [TestCase("1,3,5", "3,4,5", true, "3,5")]
    [TestCase("1,3,5", "2,4,6", false, "")]
    [TestCase("1,3,5", "", false, "")]
    [TestCase("", "1,3,5", false, "")]
    [TestCase("", "", false, "")]
    public void ContainsAny(string sSUT, string sOther, bool wantedRes, string sFirstMatched)
    {
      IdArray<Int32> sut = new IdArray<Int32>(StdConvert.ToInt32Array(sSUT));
      Int32[] other1 = StdConvert.ToInt32Array(sOther);
      IIdSet<Int32> other2 = new IdList<Int32>(StdConvert.ToInt32Array(sOther));

      bool res1 = sut.ContainsAny(other1);
      Assert.AreEqual(wantedRes, res1, "Result #1");

      Int32 firstMatchedId2;
      bool res2 = sut.ContainsAny(other1, out firstMatchedId2);
      Assert.AreEqual(wantedRes, res2, "Result #2");
      TestFirstMatchedId(firstMatchedId2, sFirstMatched, "FirstMatchedId #2");

      bool res3 = sut.ContainsAny(other2);
      Assert.AreEqual(wantedRes, res3, "Result #3");

      Int32 firstMatchedId4;
      bool res4 = sut.ContainsAny(other2, out firstMatchedId4);
      Assert.AreEqual(wantedRes, res4, "Result #4");
      TestFirstMatchedId(firstMatchedId4, sFirstMatched, "FirstMatchedId #4");
    }

    [TestCase("1,3,5", "3", true)]
    [TestCase("1,3,5", "2", false)]
    [TestCase("1,3,5", "1,3,5", true)]
    [TestCase("1,3,5", "1,2,3", false)]
    [TestCase("1,3,5", "", true)]
    [TestCase("", "1,3,5", false)]
    [TestCase("", "", true)]
    public void ContainsAll(string sSUT, string sOther, bool wantedRes)
    {
      IdArray<Int32> sut = new IdArray<Int32>(StdConvert.ToInt32Array(sSUT));
      IIdSet<Int32> other1 = new IdList<Int32>(StdConvert.ToInt32Array(sOther));
      Int32[] other2 = StdConvert.ToInt32Array(sOther);

      bool res1 = sut.ContainsAll(other1);
      Assert.AreEqual(wantedRes, res1, "Result #1");

      bool res2 = sut.ContainsAll(other2);
      Assert.AreEqual(wantedRes, res2, "Result #2");
    }

    private void TestFirstMatchedId(Int32 firstMatchedId, string sFirstMatched, string prefix)
    {
      if (sFirstMatched.Length == 0)
        Assert.AreEqual(0, firstMatchedId, prefix);
      else
      {
        IdArray<Int32> lst = new IdArray<Int32>(StdConvert.ToInt32Array(sFirstMatched));
        Assert.IsTrue(lst.Contains(firstMatchedId), prefix);
      }
    }

    #endregion

    #region Доступ по индексу

    [Test]
    public void Item()
    {
      IdArray<Int32> sut = new IdArray<Int32>(new Int32[] { 1, 5, 3 });
      Assert.AreEqual(1, sut[0], "[0]");
      Assert.AreEqual(5, sut[1], "[1]");
      Assert.AreEqual(3, sut[2], "[2]");
      int dummy;
      //IndexOutOfRangeException
      Assert.Catch(delegate () { dummy = sut[-1]; }, "[-1]");
      Assert.Catch(delegate () { dummy = sut[3]; }, "[3]");
    }

    [TestCase(1, 0)]
    [TestCase(3, 2)]
    [TestCase(2, -1)]
    [TestCase(0, -1)]
    public void IndexOf(Int32 id, int wantedRes)
    {
      IdArray<Int32> sut = new IdArray<Int32>(new Int32[] { 1, 5, 3 });
      int res = sut.IndexOf(id);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region CopyTo() / ToArray()

    [Test]
    public void CopyTo()
    {
      IdArray<Int32> sut = new IdArray<Int32>(new Int32[] { 5, 1, 3 });
      Int32[] a = new Int32[sut.Count + 3];
      sut.CopyTo(a, 2);

      CollectionAssert.AreEqual(new Int32[] { 0, 0, 5, 1, 3, 0 }, a);
    }

    [Test]
    public void ToArray()
    {
      IdArray<Int32> sut = new IdArray<Int32>(new Int32[] { 5, 1, 3 });

      Int32[] res1 = sut.ToArray();
      CollectionAssert.AreEqual(new Int32[] { 5, 1, 3 }, res1, "#1");

      Int32[] res2 = sut.ToArray();
      Assert.AreNotSame(res1, res2, "#2");
    }

    #endregion

    #region GetEnumerator()

    [Test]
    public void GetEnumerator()
    {
      IdArray<Int32> sut = new IdArray<Int32>(new Int32[] { 3, 1, 5 });

      List<Int32> lst = new List<Int32>();
      foreach (Int32 id in sut)
        lst.Add(id);

      CollectionAssert.AreEqual(new Int32[] { 3, 1, 5 }, lst.ToArray());
    }


    [Test]
    public void GetEnumerator_Empty()
    {
      IdArray<Int32> sut = new IdArray<Int32>(null);
      List<Int32> lst = new List<Int32>();
      foreach (Int32 id in sut)
        lst.Add(id);

      Assert.AreEqual(0, lst.Count);
    }

    #endregion

    #region SetReadOnly()

    [Test]
    public void SetReadOnly()
    {
      IdArray<Int32> sut = new IdArray<Int32>(null);
      Assert.IsTrue(((IReadOnlyObject)sut).IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { ((IReadOnlyObject)sut).CheckNotReadOnly(); }, "CheckNotReadOnly()");
    }

    #endregion

    #region FromId()

    [TestCase(1, "1")]
    [TestCase(0, "")]
    public void FromId(Int32 id, string sWantedRes)
    {
      IdArray<Int32> sut = IdArray<Int32>.FromId(id);
      CollectionAssert.AreEqual(StdConvert.ToInt32Array(sWantedRes), sut.ToArray(), "ToArray()");
      Assert.AreEqual(id, sut.SingleId, "SingleId");
    }

    #endregion

    #region Empty

    [Test]
    public void Empty()
    {
      Assert.AreEqual(0, IdArray<Int32>.Empty.Count, "Count");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      IdArray<Int32> sut = new IdArray<Int32>(new Int32[] { 1,3,5});
      
      byte[] b = SerializationTools.SerializeBinary(sut);

      IdArray<Int32> res = (IdArray<Int32>)(SerializationTools.DeserializeBinary(b));
      CollectionAssert.AreEqual(new Int32[] { 1, 3, 5 }, res.ToArray(), "ToArray()");
    }

    #endregion
  }
}
