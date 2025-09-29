using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using FreeLibSet.Data;
using FreeLibSet.Remoting;
using NUnit.Framework;
using System.Data;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class IdCollectionTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Empty()
    {
      IdCollection<Int32> sut = new IdCollection<Int32>();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, sut.SingleId, "SingleId");
      Assert.AreEqual(EmptyArray<Int32>.Empty, sut.ToArray(), "ToArray()");
      Assert.AreEqual(0, sut.SingleId, "SingleId");
      Assert.AreEqual(IdSetKind.Collection, ((IIdSet)sut).Kind, "Kind");
      Assert.AreEqual(typeof(Int32), ((IIdSet)sut).IdType, "IdType");
    }

    // Хотя имеется только одна перегрузка конструктора с аргументом IEnumerable<T>, 
    // требуются отдельные тесты, так как реализация отличается для IEnumerable<T>, ICollection<T> и IIdSet<T>

    [Test]
    public void Constructor_Collection()
    {
      ICollection<Int32> src = new Int32[] { 1, 3, 5, 3, 0 };
      IdCollection<Int32> sut = new IdCollection<Int32>(src);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, sut.SingleId, "SingleId");
      CollectionAssert.AreEquivalent(new Int32[] { 1, 3, 5 }, sut.ToArray(), "ToArray()"); // порядок элементов не гарантирован
    }

    [Test]
    public void Constructor_Enumarable()
    {
      IEnumerable<Int32> src = new ArrayEnumerable<Int32>(new Int32[] { 0, 3, 2, 1, 0, 1, 2, 3 });
      IdCollection<Int32> sut = new IdCollection<Int32>(src);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, sut.SingleId, "SingleId");
      CollectionAssert.AreEquivalent(new Int32[] { 1, 2, 3 }, sut.ToArray(), "ToArray()");
    }

    [Test]
    public void Constructor_IIdSet()
    {
      IdArray<Int32> src = new IdArray<Int32>(new Int32[] { 5, 1, 3 });
      IdList<Int32> sut = new IdList<Int32>(src);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, sut.SingleId, "SingleId");
      CollectionAssert.AreEquivalent(new Int32[] { 1, 3, 5 }, sut.ToArray(), "ToArray()");
    }

    [Test]
    public void Constructor_Null()
    {
      IdCollection<Int32> sut = new IdCollection<Int32>((IEnumerable<Int32>)null);
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, sut.SingleId, "SingleId");
      Assert.AreEqual(EmptyArray<Int32>.Empty, sut.ToArray(), "ToArray()");
    }

    #endregion

    #region Contains() / ConatinsAny()

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(2, false)]
    [TestCase(3, true)]
    public void Contains(Int32 id, bool wantedRes)
    {
      IdCollection<Int32> sut = new IdCollection<Int32>(new Int32[] { 5, 1, 3 });
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
      IdCollection<Int32> sut = new IdCollection<Int32>(StdConvert.ToInt32Array(sSUT));
      Int32[] other1 = StdConvert.ToInt32Array(sOther);
      IIdSet<Int32> other2 = new IdArray<Int32>(StdConvert.ToInt32Array(sOther));

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

    private void TestFirstMatchedId(Int32 firstMatchedId, string sFirstMatched, string prefix)
    {
      if (sFirstMatched.Length == 0)
        Assert.AreEqual(0, firstMatchedId, prefix);
      else
      {
        IdCollection<Int32> coll = new IdCollection<Int32>(StdConvert.ToInt32Array(sFirstMatched));
        Assert.IsTrue(coll.Contains(firstMatchedId), prefix);
      }
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
      IdCollection<Int32> sut = new IdCollection<Int32>(StdConvert.ToInt32Array(sSUT));
      IIdSet<Int32> other1 = new IdArray<Int32>(StdConvert.ToInt32Array(sOther));
      Int32[] other2 = StdConvert.ToInt32Array(sOther);

      bool res1 = sut.ContainsAll(other1);
      Assert.AreEqual(wantedRes, res1, "Result #1");

      bool res2 = sut.ContainsAll(other2);
      Assert.AreEqual(wantedRes, res2, "Result #2");
    }

    #endregion

    #region CopyTo() / ToArray()

    [Test]
    public void CopyTo()
    {
      IdCollection<Int32> sut = new IdCollection<Int32>(new Int32[] { 5, 1, 3 });
      Int32[] a = new Int32[sut.Count + 3];
      sut.CopyTo(a, 2);

      CollectionAssert.AreEquivalent(new Int32[] { 0, 0, 1, 3, 5, 0 }, a);
    }

    [Test]
    public void ToArray()
    {
      IdCollection<Int32> sut = new IdCollection<Int32>(new Int32[] { 5, 1, 3 });
      CollectionAssert.AreEquivalent(new Int32[] { 1, 3, 5 }, sut.ToArray(), "#1");

      sut.Remove(3);
      CollectionAssert.AreEquivalent(new Int32[] { 1, 5 }, sut.ToArray(), "#2");

      sut.Clear();
      CollectionAssert.AreEquivalent(new Int32[] { }, sut.ToArray(), "#3");
    }

    #endregion

    #region Add() / Remove() / Clear()

    [TestCase(0, "1,3,5")]
    [TestCase(1, "1,3,5")]
    [TestCase(2, "1,2,3,5")]
    public void Add(Int32 id, string sWantedRes)
    {
      Int32[] wantedRes = StdConvert.ToInt32Array(sWantedRes);

      IdCollection<Int32> sut = new IdCollection<Int32>(new Int32[] { 1, 3, 5 });
      sut.Add(id);

      CollectionAssert.AreEquivalent(wantedRes, sut.ToArray(), "ToArray()");
      Assert.AreEqual(wantedRes.Length, sut.Count, "Count");
    }

    // Есть только одна перегрузка метода AddRange(), но она имеет разную реализацию для IIdSet<T> при Count=0 и для IEnumerable<T>

    [TestCase("1,3,5", "3,5,3,2,0", "1,3,5,2")]
    [TestCase("1,3,5", "", "1,3,5")]
    [TestCase("", "3,5,3,2,0", "3,5,2")]
    [TestCase("", "", "")]
    public void AddRange(string sSutSrc, string sArgSrc, string sWantedRes)
    {
      Int32[] sutSrc = StdConvert.ToInt32Array(sSutSrc);
      Int32[] argSrc = StdConvert.ToInt32Array(sArgSrc);
      Int32[] wantedRes = StdConvert.ToInt32Array(sWantedRes);

      IdCollection<Int32> sut1 = new IdCollection<Int32>(sutSrc);
      IEnumerable<Int32> arg1 = new ArrayEnumerable<Int32>(argSrc);
      sut1.AddRange(arg1);
      CollectionAssert.AreEquivalent(wantedRes, sut1.ToArray(), "ToArray() IEnumerable");
      Assert.AreEqual(wantedRes.Length, sut1.Count, "Count IEnumerable");

      IdCollection<Int32> sut2 = new IdCollection<Int32>(sutSrc);
      IdArray<Int32> arg2 = new IdArray<Int32>(argSrc);
      sut2.AddRange(arg2);
      CollectionAssert.AreEquivalent(wantedRes, sut2.ToArray(), "ToArray() IIdSet");
      Assert.AreEqual(wantedRes.Length, sut2.Count, "Count IIdSet");
    }

    [Test]
    public void AddRange_null()
    {
      IdCollection<Int32> sut = new IdCollection<Int32>(new Int32[] { 1, 3, 5 });
      sut.AddRange(null);

      CollectionAssert.AreEquivalent(new Int32[] { 1, 3, 5 }, sut.ToArray());
    }

    [TestCase(0, "1,3,5")]
    [TestCase(1, "3,5")]
    [TestCase(2, "1,3,5")]
    public void Remove(Int32 id, string sWantedRes)
    {
      Int32[] wantedRes = StdConvert.ToInt32Array(sWantedRes);

      IdCollection<Int32> sut = new IdCollection<Int32>(new Int32[] { 1, 3, 5 });
      sut.Remove(id);

      CollectionAssert.AreEquivalent(wantedRes, sut.ToArray(), "ToArray()");
      Assert.AreEqual(wantedRes.Length, sut.Count, "Count");
    }

    [TestCase("1,3,5", "3,5,3,2,0", "1")]
    [TestCase("1,3,5", "", "1,3,5")]
    [TestCase("", "3,5,3,2,0", "")]
    [TestCase("", "", "")]
    public void RemoveRange(string sSutSrc, string sArgSrc, string sWantedRes)
    {
      Int32[] sutSrc = StdConvert.ToInt32Array(sSutSrc);
      Int32[] argSrc = StdConvert.ToInt32Array(sArgSrc);
      Int32[] wantedRes = StdConvert.ToInt32Array(sWantedRes);

      IdCollection<Int32> sut1 = new IdCollection<Int32>(sutSrc);
      IEnumerable<Int32> arg1 = new ArrayEnumerable<Int32>(argSrc);
      sut1.RemoveRange(arg1);
      CollectionAssert.AreEquivalent(wantedRes, sut1.ToArray(), "ToArray() IEnumerable");
      Assert.AreEqual(wantedRes.Length, sut1.Count, "Count IEnumerable");

      IdCollection<Int32> sut2 = new IdCollection<Int32>(sutSrc);
      IdArray<Int32> arg2 = new IdArray<Int32>(argSrc);
      sut2.RemoveRange(arg2);
      CollectionAssert.AreEquivalent(wantedRes, sut2.ToArray(), "ToArray() IIdSet");
      Assert.AreEqual(wantedRes.Length, sut2.Count, "Count IIdSet");
    }

    [Test]
    public void RemoveRange_null()
    {
      IdCollection<Int32> sut = new IdCollection<Int32>(new Int32[] { 1, 3, 5 });
      sut.RemoveRange(null);

      CollectionAssert.AreEquivalent(new Int32[] { 1, 3, 5 }, sut.ToArray());
    }

    [TestCase("1,3,5", "3,5,3,2,0", "3,5")]
    [TestCase("1,3,5", "", "")]
    [TestCase("", "3,5,3,2,0", "")]
    [TestCase("", "", "")]
    public void RemoveOthers(string sSutSrc, string sArgSrc, string sWantedRes)
    {
      Int32[] sutSrc = StdConvert.ToInt32Array(sSutSrc);
      Int32[] argSrc = StdConvert.ToInt32Array(sArgSrc);
      Int32[] wantedRes = StdConvert.ToInt32Array(sWantedRes);

      IdCollection<Int32> sut1 = new IdCollection<Int32>(sutSrc);
      IEnumerable<Int32> arg1 = new ArrayEnumerable<Int32>(argSrc);
      sut1.RemoveOthers(arg1);
      CollectionAssert.AreEquivalent(wantedRes, sut1.ToArray(), "ToArray() IEnumerable");
      Assert.AreEqual(wantedRes.Length, sut1.Count, "Count IEnumerable");

      IdCollection<Int32> sut2 = new IdCollection<Int32>(sutSrc);
      IdArray<Int32> arg2 = new IdArray<Int32>(argSrc);
      sut2.RemoveOthers(arg2);
      CollectionAssert.AreEquivalent(wantedRes, sut2.ToArray(), "ToArray() IIdSet");
      Assert.AreEqual(wantedRes.Length, sut2.Count, "Count IIdSet");
    }

    [Test]
    public void RemoveOthers_null()
    {
      IdCollection<Int32> sut = new IdCollection<Int32>(new Int32[] { 1, 3, 5 });
      sut.RemoveOthers(null);

      CollectionAssert.AreEquivalent(new Int32[] { }, sut.ToArray());
    }

    [Test]
    public void Clear()
    {
      IdCollection<Int32> sut = new IdCollection<Int32>(new Int32[] { 1, 3, 5 });
      sut.Clear();
      Assert.AreEqual(0, sut.Count);
    }

    #endregion

    #region GetEnumerator()

    [Test]
    public void GetEnumerator()
    {
      IdCollection<Int32> sut = new IdCollection<Int32>(new Int32[] { 3, 1, 5 });

      List<Int32> lst = new List<Int32>();
      foreach (Int32 id in sut)
        lst.Add(id);

      CollectionAssert.AreEquivalent(new Int32[] { 1, 3, 5 }, lst.ToArray());
    }


    [Test]
    public void GetEnumerator_Empty()
    {
      IdCollection<Int32> sut = new IdCollection<Int32>();
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
      IdCollection<Int32> sut = new IdCollection<Int32>();
      sut.Add(1);
      sut.Add(2);
      Assert.DoesNotThrow(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly() #1");

      sut.SetReadOnly();
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly() #2");

      Assert.AreEqual(2, sut.Count, "Count #1");
      CollectionAssert.AreEquivalent(new Int32[] { 1, 2 }, sut.ToArray(), "ToArray()");

      Assert.Catch(delegate () { sut.Add(3); }, "Add(id)");
      Assert.Catch(delegate () { sut.AddRange(EmptyArray<Int32>.Empty); }, "AddRange(Enumarable)");
      Assert.Catch(delegate () { sut.Remove(3); }, "Remove(id)");
      Assert.Catch(delegate () { sut.RemoveRange(EmptyArray<Int32>.Empty); }, "RemoveRange(Enumarable)");
      Assert.Catch(delegate () { sut.RemoveOthers(EmptyArray<Int32>.Empty); }, "RemoveOthers(Enumarable)");
      Assert.Catch(delegate () { sut.Clear(); }, "Clear()");
      Assert.AreEqual(2, sut.Count, "Count #2"); // ничего не испортилось
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone()
    {
      IdCollection<Int32> sut = new IdCollection<Int32>(new Int32[] { 1, 3, 5 });
      sut.SetReadOnly();

      IdCollection<Int32> res = sut.Clone();
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      res.Add(6);
      Assert.AreEqual(4, res.Count, "Clone Count");
      Assert.AreEqual(3, sut.Count, "Original count"); // не изменилось
    }

    [Test]
    public void CloneIfReadOnly()
    {
      IdCollection<Int32> sut = new IdCollection<Int32>(new Int32[] { 1, 3, 5 });
      IdCollection<Int32> res1 = sut.CloneIfReadOnly();
      Assert.AreSame(sut, res1, "#1");

      sut.SetReadOnly();
      IdCollection<Int32> res2 = sut.CloneIfReadOnly();
      Assert.AreNotSame(sut, res2, "#2");
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region Сравнение

#if XXX
    [TestCase("1,3,5", "3,5,1", true)]
    [TestCase("1,3,5", "1,3,6", false)]
    [TestCase("1,3,5", "1,3,5,7", false)]
    [TestCase("1,3,5", "", false)]
    [TestCase("", "", true)]
    public void Operator_Eq_NotEq(string sSUT1, string sSUT2, bool wantedEq)
    {
      IdCollection<Int32> sut1 = CreateTestObject(sSUT1);
      IdCollection<Int32> sut2 = CreateTestObject(sSUT2);

      bool res1 = sut1 == sut2;
      Assert.AreEqual(wantedEq, res1, "operator ==");

      bool res2 = sut2 == sut1;
      Assert.AreEqual(wantedEq, res2, "inverted operator ==");

      bool res3 = sut1.Equals(sut2);
      Assert.AreEqual(wantedEq, res3, "Equals()");

      bool res4 = sut2.Equals(sut1);
      Assert.AreEqual(wantedEq, res4, "inverted Equals()");

      bool res5 = sut1 != sut2;
      Assert.AreEqual(!wantedEq, res5, "operator !=");

      bool res6 = sut2 != sut1;
      Assert.AreEqual(!wantedEq, res6, "inverted operator !=");
    }
#endif

    #endregion

    #region Сложение и вычитание

#if XXX
    [TestCase("1,3,5", "2,3,4", "1,2,3,4,5")]
    [TestCase("1,3", "3,1", "1,3")]
    [TestCase("1,3", "", "1,3")]
    [TestCase("", "", "")]
    public void Operator_Plus_Or(string sSUT1, string sSUT2, string sWantedRes)
    {
      IdCollection<Int32> sut1 = CreateTestObject(sSUT1);
      sut1.SetReadOnly();
      IdCollection<Int32> sut2 = CreateTestObject(sSUT2);
      sut2.SetReadOnly();

      IdCollection<Int32> wantedRes = CreateTestObject(sWantedRes);
      // Нельзя использовать для сравнения AssertEquals(wantedRes, res), т.к. для IdCollection выполняется сравнение как для коллекций, а не вызывается метод Equals().

      IdCollection<Int32> res1 = sut1 + sut2;
      Assert.IsTrue(wantedRes == res1, "operator +");
      Assert.IsFalse(res1.IsReadOnly, "IsReadOnly #1");

      IdCollection<Int32> res2 = sut2 + sut1;
      Assert.IsTrue(wantedRes == res2, "inverted operator +");
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly #2");

      IdCollection<Int32> res3 = sut1 | sut2;
      Assert.IsTrue(wantedRes == res3, "operator |");
      Assert.IsFalse(res3.IsReadOnly, "IsReadOnly #3");

      IdCollection<Int32> res4 = sut2 | sut1;
      Assert.IsTrue(wantedRes == res4, "inverted operator |");
      Assert.IsFalse(res4.IsReadOnly, "IsReadOnly #4");
    }

    [TestCase("1,3,5", "5,3,1", "1,3,5")]
    [TestCase("1,3,5", "1,2,3", "1,3")]
    [TestCase("1,3,5", "2,4,6", "")]
    [TestCase("1,3,5", "", "")]
    [TestCase("", "", "")]
    public void Operator_And(string sSUT1, string sSUT2, string sWantedRes)
    {
      IdCollection<Int32> sut1 = CreateTestObject(sSUT1);
      sut1.SetReadOnly();
      IdCollection<Int32> sut2 = CreateTestObject(sSUT2);
      sut2.SetReadOnly();

      IdCollection<Int32> wantedRes = CreateTestObject(sWantedRes);

      IdCollection<Int32> res1 = sut1 & sut2;
      Assert.IsTrue(wantedRes == res1, "operator &");
      Assert.IsFalse(res1.IsReadOnly, "IsReadOnly #1");

      IdCollection<Int32> res2 = sut2 & sut1;
      Assert.IsTrue(wantedRes == res2, "inverted operator &");
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly #2");
    }

    [TestCase("1,3,5", "1,3,6", "5")]
    [TestCase("1,3,5", "1,3,5", "")]
    [TestCase("1,3,5", "2,4,6", "1,3,5")]
    [TestCase("1,3,5", "", "1,3,5")]
    [TestCase("", "1,3,5", "")]
    [TestCase("", "", "")]
    public void Operator_Substract(string sSUT1, string sSUT2, string sWantedRes)
    {
      IdCollection<Int32> sut1 = CreateTestObject(sSUT1);
      sut1.SetReadOnly();
      IdCollection<Int32> sut2 = CreateTestObject(sSUT2);
      sut2.SetReadOnly();

      IdCollection<Int32> wantedRes = CreateTestObject(sWantedRes);

      IdCollection<Int32> res = sut1 - sut2;
      Assert.IsTrue(wantedRes == res, "operator -");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
    }
#endif

    #endregion

    #region SingleId

    [TestCase("1", 1)]
    [TestCase("1,3", 0)]
    [TestCase("", 0)]
    public void SingleId(string sSUT, Int32 wantedRes)
    {
      IdCollection<Int32> sut = new IdCollection<Int32>(StdConvert.ToInt32Array(sSUT));
      Assert.AreEqual(wantedRes, sut.SingleId);
    }

    #endregion

    #region FromId()

    [TestCase(1, "1")]
    [TestCase(0, "")]
    public void FromId(Int32 id, string sWantedRes)
    {
      IdCollection<Int32> sut = IdCollection<Int32>.FromId(id);
      CollectionAssert.AreEquivalent(StdConvert.ToInt32Array(sWantedRes), sut.ToArray(), "ToArray()");
      Assert.AreEqual(id, sut.SingleId, "SingleId");
    }

    #endregion

    #region FromIds(), FromColumn()
#if XXX
    [Test]
    public void FromIds()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Id", typeof(Int32));
      tbl.Rows.Add("1");
      tbl.Rows.Add("3");
      tbl.Rows.Add("5");

      IdCollection<Int32> wantedRes = new IdCollection<Int32>(new Int32[] { 1, 3, 5 });

      IdCollection<Int32> sut1 = IdCollection<Int32>.FromIds(tbl);
      Assert.AreEqual(wantedRes, sut1, "DataTable");

      IdCollection<Int32> sut2 = IdCollection<Int32>.FromIds(tbl.DefaultView);
      Assert.AreEqual(wantedRes, sut2, "DataView");

      //IdCollection<Int32> sut3 = IdCollection<Int32>.FromIds(DataTools.GetDataTableRows(tbl));
      //Assert.AreEqual(wantedRes, sut3, "DataRow[]");

      //DataRowView[] a4 = new DataRowView[tbl.DefaultView.Count];
      //tbl.DefaultView.CopyTo(a4, 0);
      //IdCollection<Int32> sut4 = IdCollection<Int32>.FromIds(a4);
      //Assert.AreEqual(wantedRes, sut4, "DataRowView[]");
    }

    [Test]
    public void FromIds_empty()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Id", typeof(Int32));

      IdCollection<Int32> sut1 = IdCollection<Int32>.FromIds(tbl);
      Assert.AreEqual(0, sut1.Count, "DataTable");

      IdCollection<Int32> sut2 = IdCollection<Int32>.FromIds(tbl.DefaultView);
      Assert.AreEqual(0, sut2.Count, "DataView");

      //IdCollection<Int32> sut3 = IdCollection<Int32>.FromIds(new DataRow[0]);
      //Assert.AreEqual(0, sut3.Count, "DataRow[]");

      //IdCollection<Int32> sut4 = IdCollection<Int32>.FromIds(new DataRowView[0]);
      //Assert.AreEqual(0, sut4.Count, "DataRowView[]");
    }
#endif

#if XXX

    [Test]
    public void FromColumn()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(Int32));
      tbl.Rows.Add("1");
      tbl.Rows.Add("3");
      tbl.Rows.Add("5");
      tbl.Rows.Add("3"); // повтор
      tbl.Rows.Add("0");
      tbl.Rows.Add(DBNull.Value);

      IdCollection<Int32> wantedRes = new IdCollection<Int32>(new Int32[] { 1, 3, 5 });

      IdCollection<Int32> sut1 = IdCollection<Int32>.FromColumn(tbl, "F1");
      Assert.AreEqual(wantedRes, sut1, "DataTable");

      IdCollection<Int32> sut2 = IdCollection<Int32>.FromColumn(tbl.DefaultView, "F1");
      Assert.AreEqual(wantedRes, sut2, "DataView");

      IdCollection<Int32> sut3 = IdCollection<Int32>.FromColumn(DataTools.GetDataTableRows(tbl), "F1");
      Assert.AreEqual(wantedRes, sut3, "DataRow[]");

      DataRowView[] a4 = new DataRowView[tbl.DefaultView.Count];
      tbl.DefaultView.CopyTo(a4, 0);
      IdCollection<Int32> sut4 = IdCollection<Int32>.FromColumn(a4, "F1");
      Assert.AreEqual(wantedRes, sut4, "DataRowView[]");
    }

    [Test]
    public void FromColumn_zeros()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(Int32));
      tbl.Rows.Add(0);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(0);
      tbl.Rows.Add(DBNull.Value);

      IdCollection<Int32> sut1 = IdCollection<Int32>.FromColumn(tbl, "F1");
      Assert.AreEqual(0, sut1.Count, "DataTable");

      IdCollection<Int32> sut2 = IdCollection<Int32>.FromColumn(tbl.DefaultView, "F1");
      Assert.AreEqual(0, sut2.Count, "DataView");

      IdCollection<Int32> sut3 = IdCollection<Int32>.FromColumn(DataTools.GetDataTableRows(tbl), "F1");
      Assert.AreEqual(0, sut3.Count, "DataRow[]");

      DataRowView[] a4 = new DataRowView[tbl.DefaultView.Count];
      tbl.DefaultView.CopyTo(a4, 0);
      IdCollection<Int32> sut4 = IdCollection<Int32>.FromColumn(a4, "F1");
      Assert.AreEqual(0, sut4.Count, "DataRowView[]");
    }

    [Test]
    public void FromColumn_empty()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(Int32));

      IdCollection<Int32> sut1 = IdCollection<Int32>.FromColumn(tbl, "F1");
      Assert.AreEqual(0, sut1.Count, "DataTable");

      IdCollection<Int32> sut2 = IdCollection<Int32>.FromColumn(tbl.DefaultView, "F1");
      Assert.AreEqual(0, sut2.Count, "DataView");

      IdCollection<Int32> sut3 = IdCollection<Int32>.FromColumn(new DataRow[0], "F1");
      Assert.AreEqual(0, sut3.Count, "DataRow[]");

      IdCollection<Int32> sut4 = IdCollection<Int32>.FromColumn(new DataRowView[0], "F1");
      Assert.AreEqual(0, sut4.Count, "DataRowView[]");
    }
#endif
    #endregion

    #region Empty

    [Test]
    public void Empty()
    {
      Assert.AreEqual(0, IdCollection<Int32>.Empty.Count, "Count");
      Assert.IsTrue(IdCollection<Int32>.Empty.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization([Values(false, true)] bool isReadOnly)
    {
      IdCollection<Int32> sut = new IdCollection<Int32>();
      sut.Add(1);
      sut.Add(3);
      sut.Add(5);
      if (isReadOnly)
        sut.SetReadOnly();

      byte[] b = SerializationTools.SerializeBinary(sut);

      IdCollection<Int32> res = (IdCollection<Int32>)(SerializationTools.DeserializeBinary(b));
      CollectionAssert.AreEquivalent(new Int32[] { 1, 3, 5 }, res.ToArray(), "ToArray()");
      Assert.AreEqual(isReadOnly, res.IsReadOnly, "IsReadOnly");
    }

    #endregion
  }
}
