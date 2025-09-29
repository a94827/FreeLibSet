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
  public class IdListTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Empty()
    {
      IdList<Int32> sut = new IdList<Int32>();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(EmptyArray<Int32>.Empty, sut.ToArray(), "ToArray()");
      Assert.AreEqual(0, sut.SingleId, "SingleId");
      Assert.AreEqual(IdSetKind.List, ((IIdSet)sut).Kind, "Kind");
      Assert.AreEqual(typeof(Int32), ((IIdSet)sut).IdType, "IdType");
    }

    // Хотя имеется только одна перегрузка конструктора с аргументом IEnumerable<T>, 
    // требуются отдельные тесты, так как реализация отличается для IEnumerable<T>, ICollection<T> и IIdSet<T>

    [Test]
    public void Constructor_Collection()
    {
      ICollection<Int32> src = new Int32[] { 1, 3, 5, 3, 0 };
      IdList<Int32> sut = new IdList<Int32>(src);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      CollectionAssert.AreEqual(new Int32[] { 1, 3, 5 }, sut.ToArray(), "ToArray()"); // порядок элементов не гарантирован
      Assert.AreEqual(0, sut.SingleId, "SingleId");
    }

    [Test]
    public void Constructor_Enumarable()
    {
      IEnumerable<Int32> src = new ArrayEnumerable<Int32>(new Int32[] { 0, 3, 2, 1, 0, 1, 2, 3 });
      IdList<Int32> sut = new IdList<Int32>(src);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      CollectionAssert.AreEqual(new Int32[] { 3, 2, 1 }, sut.ToArray(), "ToArray()");
      Assert.AreEqual(0, sut.SingleId, "SingleId");
    }

    [Test]
    public void Constructor_IIdSet()
    {
      IdArray<Int32> src = new IdArray<Int32>(new Int32[] { 5, 1, 3 });
      IdList<Int32> sut = new IdList<Int32>(src);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      CollectionAssert.AreEqual(new Int32[] { 5, 1, 3 }, sut.ToArray(), "ToArray()");
      Assert.AreEqual(0, sut.SingleId, "SingleId");
    }

    [Test]
    public void Constructor_Null()
    {
      IdList<Int32> sut = new IdList<Int32>((IEnumerable<Int32>)null);
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
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
      IdList<Int32> sut = new IdList<Int32>(new Int32[] { 5, 1, 3 });
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
      IdList<Int32> sut = new IdList<Int32>(StdConvert.ToInt32Array(sSUT));
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

    [TestCase("1,3,5", "3", true)]
    [TestCase("1,3,5", "2", false)]
    [TestCase("1,3,5", "1,3,5", true)]
    [TestCase("1,3,5", "1,2,3", false)]
    [TestCase("1,3,5", "", true)]
    [TestCase("", "1,3,5", false)]
    [TestCase("", "", true)]
    public void ContainsAll(string sSUT, string sOther, bool wantedRes)
    {
      IdList<Int32> sut = new IdList<Int32>(StdConvert.ToInt32Array(sSUT));
      IIdSet<Int32> other1 = new IdArray<Int32>(StdConvert.ToInt32Array(sOther));
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
        IdList<Int32> lst = new IdList<Int32>(StdConvert.ToInt32Array(sFirstMatched));
        Assert.IsTrue(lst.Contains(firstMatchedId), prefix);
      }
    }

    #endregion

    #region Доступ по индексу

    [Test]
    public void Item()
    {
      IdList<Int32> sut = new IdList<Int32>(new Int32[] { 1, 5, 3 });
      Assert.AreEqual(1, sut[0], "[0]");
      Assert.AreEqual(5, sut[1], "[1]");
      Assert.AreEqual(3, sut[2], "[2]");
      int dummy;
      Assert.Catch<ArgumentOutOfRangeException>(delegate () { dummy = sut[-1]; }, "[-1]");
      Assert.Catch<ArgumentOutOfRangeException>(delegate () { dummy = sut[3]; }, "[3]");
    }

    [TestCase(1, 0)]
    [TestCase(3, 2)]
    [TestCase(2, -1)]
    [TestCase(0, -1)]
    public void IndexOf(Int32 id, int wantedRes)
    {
      IdList<Int32> sut = new IdList<Int32>(new Int32[] { 1, 5, 3 });
      int res = sut.IndexOf(id);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region CopyTo() / ToArray()

    [Test]
    public void CopyTo()
    {
      IdList<Int32> sut = new IdList<Int32>(new Int32[] { 5, 1, 3 });
      Int32[] a = new Int32[sut.Count + 3];
      sut.CopyTo(a, 2);

      CollectionAssert.AreEqual(new Int32[] { 0, 0, 5, 1, 3, 0 }, a);
    }

    [Test]
    public void ToArray()
    {
      IdList<Int32> sut = new IdList<Int32>(new Int32[] { 5, 1, 3 });
      CollectionAssert.AreEqual(new Int32[] { 5, 1, 3 }, sut.ToArray(), "#1");

      sut.Remove(3);
      CollectionAssert.AreEqual(new Int32[] { 5, 1 }, sut.ToArray(), "#2");

      sut.Clear();
      CollectionAssert.AreEqual(new Int32[] { }, sut.ToArray(), "#3");
    }

    #endregion

    #region Add() / Remove() / Clear()

    [TestCase(0, "1,3,5")]
    [TestCase(1, "1,3,5")]
    [TestCase(2, "1,3,5,2")]
    public void Add(Int32 id, string sWantedRes)
    {
      Int32[] wantedRes = StdConvert.ToInt32Array(sWantedRes);

      IdList<Int32> sut = new IdList<Int32>(new Int32[] { 1, 3, 5 });
      sut.Add(id);

      CollectionAssert.AreEqual(wantedRes, sut.ToArray(), "ToArray");
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

      IdList<Int32> sut1 = new IdList<Int32>(sutSrc);
      IEnumerable<Int32> arg1 = new ArrayEnumerable<Int32>(argSrc);
      sut1.AddRange(arg1);
      CollectionAssert.AreEqual(wantedRes, sut1.ToArray(), "ToArray() IEnumerable");
      Assert.AreEqual(wantedRes.Length, sut1.Count, "Count IEnumerable");

      IdList<Int32> sut2 = new IdList<Int32>(sutSrc);
      IdArray<Int32> arg2 = new IdArray<Int32>(argSrc);
      sut2.AddRange(arg2);
      CollectionAssert.AreEqual(wantedRes, sut2.ToArray(), "ToArray() IIdSet");
      Assert.AreEqual(wantedRes.Length, sut2.Count, "Count IIdSet");
    }

    [Test]
    public void AddRange_null()
    {
      IdList<Int32> sut = new IdList<Int32>(new Int32[] { 1, 3, 5 });
      sut.AddRange(null);

      CollectionAssert.AreEquivalent(new Int32[] { 1, 3, 5 }, sut.ToArray());
    }


    [TestCase(0, "1,3,5")]
    [TestCase(1, "3,5")]
    [TestCase(2, "1,3,5")]
    public void Remove(Int32 id, string sWantedRes)
    {
      Int32[] wantedRes = StdConvert.ToInt32Array(sWantedRes);

      IdList<Int32> sut = new IdList<Int32>(new Int32[] { 1, 3, 5 });
      sut.Remove(id);

      CollectionAssert.AreEqual(wantedRes, sut.ToArray(), "ToArray()");
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

      IdList<Int32> sut1 = new IdList<Int32>(sutSrc);
      IEnumerable<Int32> arg1 = new ArrayEnumerable<Int32>(argSrc);
      sut1.RemoveRange(arg1);
      CollectionAssert.AreEqual(wantedRes, sut1.ToArray(), "ToArray() IEnumerable");
      Assert.AreEqual(wantedRes.Length, sut1.Count, "Count IEnumerable");

      IdList<Int32> sut2 = new IdList<Int32>(sutSrc);
      IdArray<Int32> arg2 = new IdArray<Int32>(argSrc);
      sut2.RemoveRange(arg2);
      CollectionAssert.AreEqual(wantedRes, sut2.ToArray(), "ToArray() IIdSet");
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

      IdList<Int32> sut1 = new IdList<Int32>(sutSrc);
      IEnumerable<Int32> arg1 = new ArrayEnumerable<Int32>(argSrc);
      sut1.RemoveOthers(arg1);
      CollectionAssert.AreEqual(wantedRes, sut1.ToArray(), "ToArray() IEnumerable");
      Assert.AreEqual(wantedRes.Length, sut1.Count, "Count IEnumerable");

      IdList<Int32> sut2 = new IdList<Int32>(sutSrc);
      IdArray<Int32> arg2 = new IdArray<Int32>(argSrc);
      sut2.RemoveOthers(arg2);
      CollectionAssert.AreEqual(wantedRes, sut2.ToArray(), "ToArray() IIdSet");
      Assert.AreEqual(wantedRes.Length, sut2.Count, "Count IIdSet");
    }

    [Test]
    public void RemoveOthers_null()
    {
      IdList<Int32> sut = new IdList<Int32>(new Int32[] { 1, 3, 5 });
      sut.RemoveOthers(null);

      CollectionAssert.AreEquivalent(new Int32[] { }, sut.ToArray());
    }

    [Test]
    public void Clear()
    {
      IdList<Int32> sut = new IdList<Int32>(new Int32[] { 1, 3, 5 });
      sut.Clear();
      Assert.AreEqual(0, sut.Count);
    }

    #endregion

    #region GetEnumerator()

    [Test]
    public void GetEnumerator()
    {
      IdList<Int32> sut = new IdList<Int32>(new Int32[] { 3, 1, 5 });

      List<Int32> lst = new List<Int32>();
      foreach (Int32 id in sut)
        lst.Add(id);

      CollectionAssert.AreEqual(new Int32[] { 3, 1, 5 }, lst.ToArray());
    }


    [Test]
    public void GetEnumerator_Empty()
    {
      IdList<Int32> sut = new IdList<Int32>();
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
      IdList<Int32> sut = new IdList<Int32>();
      sut.Add(1);
      sut.Add(2);
      Assert.DoesNotThrow(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly() #1");

      sut.SetReadOnly();
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly() #2");

      Assert.AreEqual(2, sut.Count, "Count #1");
      CollectionAssert.AreEqual(new Int32[] { 1, 2 }, sut.ToArray(), "ToArray()");

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
      IdList<Int32> sut = new IdList<Int32>(new Int32[] { 1, 3, 5 });
      sut.SetReadOnly();

      IdList<Int32> res = sut.Clone();
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      res.Add(6);
      Assert.AreEqual(4, res.Count, "Clone Count");
      Assert.AreEqual(3, sut.Count, "Original count"); // не изменилось
    }

    [Test]
    public void CloneIfReadOnly()
    {
      IdList<Int32> sut = new IdList<Int32>(new Int32[] { 1, 3, 5 });
      IdList<Int32> res1 = sut.CloneIfReadOnly();
      Assert.AreSame(sut, res1, "#1");

      sut.SetReadOnly();
      IdList<Int32> res2 = sut.CloneIfReadOnly();
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
      IdList<Int32> sut1 = CreateTestObject(sSUT1);
      IdList<Int32> sut2 = CreateTestObject(sSUT2);

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
      IdList<Int32> sut1 = CreateTestObject(sSUT1);
      sut1.SetReadOnly();
      IdList<Int32> sut2 = CreateTestObject(sSUT2);
      sut2.SetReadOnly();

      IdList<Int32> wantedRes = CreateTestObject(sWantedRes);
      // Нельзя использовать для сравнения AssertEquals(wantedRes, res), т.к. для IdList выполняется сравнение как для коллекций, а не вызывается метод Equals().

      IdList<Int32> res1 = sut1 + sut2;
      Assert.IsTrue(wantedRes == res1, "operator +");
      Assert.IsFalse(res1.IsReadOnly, "IsReadOnly #1");

      IdList<Int32> res2 = sut2 + sut1;
      Assert.IsTrue(wantedRes == res2, "inverted operator +");
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly #2");

      IdList<Int32> res3 = sut1 | sut2;
      Assert.IsTrue(wantedRes == res3, "operator |");
      Assert.IsFalse(res3.IsReadOnly, "IsReadOnly #3");

      IdList<Int32> res4 = sut2 | sut1;
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
      IdList<Int32> sut1 = CreateTestObject(sSUT1);
      sut1.SetReadOnly();
      IdList<Int32> sut2 = CreateTestObject(sSUT2);
      sut2.SetReadOnly();

      IdList<Int32> wantedRes = CreateTestObject(sWantedRes);

      IdList<Int32> res1 = sut1 & sut2;
      Assert.IsTrue(wantedRes == res1, "operator &");
      Assert.IsFalse(res1.IsReadOnly, "IsReadOnly #1");

      IdList<Int32> res2 = sut2 & sut1;
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
      IdList<Int32> sut1 = CreateTestObject(sSUT1);
      sut1.SetReadOnly();
      IdList<Int32> sut2 = CreateTestObject(sSUT2);
      sut2.SetReadOnly();

      IdList<Int32> wantedRes = CreateTestObject(sWantedRes);

      IdList<Int32> res = sut1 - sut2;
      Assert.IsTrue(wantedRes == res, "operator -");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
    }
#endif

    #endregion

    //#region SingleId

    //[TestCase("1", 1)]
    //[TestCase("1,3", 0)]
    //[TestCase("", 0)]
    //public void SingleId(string sSUT, Int32 wantedRes)
    //{
    //  IdList<Int32> sut = CreateTestObject(sSUT);
    //  Assert.AreEqual(wantedRes, sut.SingleId);
    //}

    //#endregion

    #region FromId()

    [TestCase(1, "1")]
    [TestCase(0, "")]
    public void FromId(Int32 id, string sWantedRes)
    {
      IdList<Int32> sut = IdList<Int32>.FromId(id);
      CollectionAssert.AreEqual(StdConvert.ToInt32Array(sWantedRes), sut.ToArray(), "ToArray()");
      Assert.AreEqual(id, sut.SingleId, "SingleId");
    }

    #endregion
#if XXX

    #region FromIds(), FromColumn()

    [Test]
    public void FromIds()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Id", typeof(Int32));
      tbl.Rows.Add("1");
      tbl.Rows.Add("3");
      tbl.Rows.Add("5");

      IdList<Int32> wantedRes = new IdList<Int32>(new Int32[] { 1, 3, 5 });

      IdList<Int32> sut1 = IdList<Int32>.FromIds(tbl);
      Assert.AreEqual(wantedRes, sut1, "DataTable");

      IdList<Int32> sut2 = IdList<Int32>.FromIds(tbl.DefaultView);
      Assert.AreEqual(wantedRes, sut2, "DataView");

      //IdList<Int32> sut3 = IdList<Int32>.FromIds(DataTools.GetDataTableRows(tbl));
      //Assert.AreEqual(wantedRes, sut3, "DataRow[]");

      //DataRowView[] a4 = new DataRowView[tbl.DefaultView.Count];
      //tbl.DefaultView.CopyTo(a4, 0);
      //IdList<Int32> sut4 = IdList<Int32>.FromIds(a4);
      //Assert.AreEqual(wantedRes, sut4, "DataRowView[]");
    }

    [Test]
    public void FromIds_empty()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Id", typeof(Int32));

      IdList<Int32> sut1 = IdList<Int32>.FromIds(tbl);
      Assert.AreEqual(0, sut1.Count, "DataTable");

      IdList<Int32> sut2 = IdList<Int32>.FromIds(tbl.DefaultView);
      Assert.AreEqual(0, sut2.Count, "DataView");

      //IdList<Int32> sut3 = IdList<Int32>.FromIds(new DataRow[0]);
      //Assert.AreEqual(0, sut3.Count, "DataRow[]");

      //IdList<Int32> sut4 = IdList<Int32>.FromIds(new DataRowView[0]);
      //Assert.AreEqual(0, sut4.Count, "DataRowView[]");
    }

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

      IdList<Int32> wantedRes = new IdList<Int32>(new Int32[] { 1, 3, 5 });

      IdList<Int32> sut1 = IdList<Int32>.FromColumn(tbl, "F1");
      Assert.AreEqual(wantedRes, sut1, "DataTable");

      IdList<Int32> sut2 = IdList<Int32>.FromColumn(tbl.DefaultView, "F1");
      Assert.AreEqual(wantedRes, sut2, "DataView");

      IdList<Int32> sut3 = IdList<Int32>.FromColumn(DataTools.GetDataTableRows(tbl), "F1");
      Assert.AreEqual(wantedRes, sut3, "DataRow[]");

      DataRowView[] a4 = new DataRowView[tbl.DefaultView.Count];
      tbl.DefaultView.CopyTo(a4, 0);
      IdList<Int32> sut4 = IdList<Int32>.FromColumn(a4, "F1");
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

      IdList<Int32> sut1 = IdList<Int32>.FromColumn(tbl, "F1");
      Assert.AreEqual(0, sut1.Count, "DataTable");

      IdList<Int32> sut2 = IdList<Int32>.FromColumn(tbl.DefaultView, "F1");
      Assert.AreEqual(0, sut2.Count, "DataView");

      IdList<Int32> sut3 = IdList<Int32>.FromColumn(DataTools.GetDataTableRows(tbl), "F1");
      Assert.AreEqual(0, sut3.Count, "DataRow[]");

      DataRowView[] a4 = new DataRowView[tbl.DefaultView.Count];
      tbl.DefaultView.CopyTo(a4, 0);
      IdList<Int32> sut4 = IdList<Int32>.FromColumn(a4, "F1");
      Assert.AreEqual(0, sut4.Count, "DataRowView[]");
    }

    [Test]
    public void FromColumn_empty()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(Int32));

      IdList<Int32> sut1 = IdList<Int32>.FromColumn(tbl, "F1");
      Assert.AreEqual(0, sut1.Count, "DataTable");

      IdList<Int32> sut2 = IdList<Int32>.FromColumn(tbl.DefaultView, "F1");
      Assert.AreEqual(0, sut2.Count, "DataView");

      IdList<Int32> sut3 = IdList<Int32>.FromColumn(new DataRow[0], "F1");
      Assert.AreEqual(0, sut3.Count, "DataRow[]");

      IdList<Int32> sut4 = IdList<Int32>.FromColumn(new DataRowView[0], "F1");
      Assert.AreEqual(0, sut4.Count, "DataRowView[]");
    }

    #endregion

#endif

    #region Empty

    [Test]
    public void Empty()
    {
      Assert.AreEqual(0, IdList<Int32>.Empty.Count, "Count");
      Assert.IsTrue(IdList<Int32>.Empty.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization([Values(false, true)] bool isReadOnly)
    {
      IdList<Int32> sut = new IdList<Int32>();
      sut.Add(1);
      sut.Add(3);
      sut.Add(5);
      if (isReadOnly)
        sut.SetReadOnly();

      byte[] b = SerializationTools.SerializeBinary(sut);

      IdList<Int32> res = (IdList<Int32>)(SerializationTools.DeserializeBinary(b));
      CollectionAssert.AreEqual(new Int32[] { 1, 3, 5 }, res.ToArray(), "ToArray()");
      Assert.AreEqual(isReadOnly, res.IsReadOnly, "IsReadOnly");
    }

    #endregion
  }
}
