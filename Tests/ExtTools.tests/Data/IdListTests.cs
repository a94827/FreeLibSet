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

    #region Contains() / ConatinsAny()

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(2, false)]
    [TestCase(3, true)]
    public void Contains_Id(Int32 id, bool wantedRes)
    {
      IdList sut = new IdList(new Int32[] { 5, 1, 3 });
      Assert.AreEqual(wantedRes, sut.Contains(id));
    }

    [TestCase("1,3,5", "3", true)]
    [TestCase("1,3,5", "2", false)]
    [TestCase("1,3,5", "1,3,5", true)]
    [TestCase("1,3,5", "1,2,3", false)]
    [TestCase("1,3,5", "", true)]
    [TestCase("", "1,3,5", false)]
    [TestCase("", "", true)]
    public void Contains_list(string sSUT, string sOther, bool wantedRes)
    {
      IdList sut = CreateTestObject(sSUT);
      IdList other1 = CreateTestObject(sOther);
      bool res1 = sut.Contains(other1);
      Assert.AreEqual(wantedRes, res1, "Result #1");

      Int32[] other2 = other1.ToArray();
      bool res2 = sut.Contains(other2);
      Assert.AreEqual(wantedRes, res2, "Result #2");
    }

    [TestCase("1,3,5", "2,3,4", true, "3")]
    [TestCase("1,3,5", "3,4,5", true, "3,5")]
    [TestCase("1,3,5", "2,4,6", false, "")]
    [TestCase("1,3,5", "", false, "")]
    [TestCase("", "1,3,5", false, "")]
    [TestCase("", "", false, "")]
    public void ContainsAny(string sSUT, string sOther, bool wantedRes, string sFirstMatched)
    {
      IdList sut = CreateTestObject(sSUT);
      IdList other1 = CreateTestObject(sOther);
      bool res1 = sut.ContainsAny(other1);
      Assert.AreEqual(wantedRes, res1, "Result #1");

      Int32 firstMatchedId2;
      bool res2 = sut.ContainsAny(other1, out firstMatchedId2);
      Assert.AreEqual(wantedRes, res2, "Result #2");
      TestFirstMatchedId(firstMatchedId2, sFirstMatched, "FirstMatchedId #2");

      Int32[] other3 = other1.ToArray();
      bool res3 = sut.ContainsAny(other3);
      Assert.AreEqual(wantedRes, res3, "Result #3");

      Int32 firstMatchedId4;
      bool res4 = sut.ContainsAny(other3, out firstMatchedId4);
      Assert.AreEqual(wantedRes, res4, "Result #4");
      TestFirstMatchedId(firstMatchedId4, sFirstMatched, "FirstMatchedId #4");
    }

    private void TestFirstMatchedId(Int32 firstMatchedId, string sFirstMatched, string prefix)
    {
      if (sFirstMatched.Length == 0)
        Assert.AreEqual(0, firstMatchedId, prefix);
      else
      {
        IdList lst = CreateTestObject(sFirstMatched);
        Assert.IsTrue(lst.Contains(firstMatchedId), prefix);
      }
    }

    private static IdList CreateTestObject(string sSUT)
    {
      IdList res = new IdList();
      if (!String.IsNullOrEmpty(sSUT))
      {
        Int32[] ids = StdConvert.ToInt32Array(sSUT);
        res.Add(ids);
      }
      return res;
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
      sut.Remove(id);

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
      sut.RemoveOthers(arg);

      Int32[] a = sut.ToArray();
      Array.Sort<Int32>(a);
      Assert.AreEqual(a, new Int32[] { 3, 5 });
    }

    [Test]
    public void RemoveOthers_IdList_Empty()
    {
      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      IdList arg = new IdList();
      sut.RemoveOthers(arg);

      Int32[] a = sut.ToArray();
      Array.Sort<Int32>(a);
      Assert.AreEqual(a, new Int32[] { });
    }

    [Test]
    public void RemoveOthers_Enumerable()
    {
      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      IEnumerable<Int32> arg = new ArrayEnumerable<Int32>(new Int32[] { 1, 2, 3, 1, 2, 3, 0, 0 });
      sut.RemoveOthers(arg);

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

    #endregion

    #region SetReadOnly()

    [Test]
    public void SetReadOnly()
    {
      IdList sut = new IdList();
      sut.Add(1);
      sut.Add(2);
      Assert.DoesNotThrow(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly() #1");

      sut.SetReadOnly();
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly() #2");

      Assert.AreEqual(2, sut.Count, "Count #1");
      Int32[] a = sut.ToArray(); // порядок элементов не гарантирован
      Array.Sort<Int32>(a);
      Assert.AreEqual(new Int32[] { 1, 2 }, a, "ToArray()");

      Assert.Catch(delegate() { sut.Add(3); }, "Add(id)");
      Assert.Catch(delegate() { sut.Add(new IdList()); }, "Add(IdList)");
      Assert.Catch(delegate() { sut.Add(DataTools.EmptyIds); }, "Add(Enumarable)");

      Assert.Catch(delegate() { sut.Remove(3); }, "Remove(id)");
      Assert.Catch(delegate() { sut.Remove(new IdList()); }, "Remove(IdList)");
      Assert.Catch(delegate() { sut.Remove(DataTools.EmptyIds); }, "Remove(Enumarable)");

      Assert.Catch(delegate() { sut.RemoveOthers(new IdList()); }, "RemoveOthers(IdList)");
      Assert.Catch(delegate() { sut.RemoveOthers(DataTools.EmptyIds); }, "RemoveOthers(Enumarable)");

      Assert.Catch(delegate() { sut.Clear(); }, "Clear()");
      Assert.AreEqual(2, sut.Count, "Count #2"); // ничего не испортилось
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone()
    {
      IdList sut = new IdList(new Int32[] { 1, 3, 5 });
      sut.SetReadOnly();

      IdList res = sut.Clone();
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      res.Add(6);
      Assert.AreEqual(4, res.Count, "Clone Count");
      Assert.AreEqual(3, sut.Count, "Original count"); // не изменилось
    }

    #endregion

    #region Сравнение

    [TestCase("1,3,5", "3,5,1", true)]
    [TestCase("1,3,5", "1,3,6", false)]
    [TestCase("1,3,5", "1,3,5,7", false)]
    [TestCase("1,3,5", "", false)]
    [TestCase("", "", true)]
    public void Operator_Eq_NotEq(string sSUT1, string sSUT2, bool wantedEq)
    {
      IdList sut1 = CreateTestObject(sSUT1);
      IdList sut2 = CreateTestObject(sSUT2);

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

    #endregion

    #region Сложение и вычитание

    [TestCase("1,3,5", "2,3,4", "1,2,3,4,5")]
    [TestCase("1,3", "3,1", "1,3")]
    [TestCase("1,3", "", "1,3")]
    [TestCase("", "", "")]
    public void Operator_Plus_Or(string sSUT1, string sSUT2, string sWantedRes)
    {
      IdList sut1 = CreateTestObject(sSUT1);
      sut1.SetReadOnly();
      IdList sut2 = CreateTestObject(sSUT2);
      sut2.SetReadOnly();

      IdList wantedRes = CreateTestObject(sWantedRes);
      // Нельзя использовать для сравнения AssertEquals(wantedRes, res), т.к. для IdList выполняется сравнение как для коллекций, а не вызывается метод Equals().

      IdList res1 = sut1 + sut2;
      Assert.IsTrue(wantedRes == res1, "operator +");
      Assert.IsFalse(res1.IsReadOnly, "IsReadOnly #1");

      IdList res2 = sut2 + sut1;
      Assert.IsTrue(wantedRes == res2, "inverted operator +");
      Assert.IsFalse(res2.IsReadOnly, "IsReadOnly #2");

      IdList res3 = sut1 | sut2;
      Assert.IsTrue(wantedRes == res3, "operator |");
      Assert.IsFalse(res3.IsReadOnly, "IsReadOnly #3");

      IdList res4 = sut2 | sut1;
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
      IdList sut1 = CreateTestObject(sSUT1);
      sut1.SetReadOnly();
      IdList sut2 = CreateTestObject(sSUT2);
      sut2.SetReadOnly();

      IdList wantedRes = CreateTestObject(sWantedRes);

      IdList res1 = sut1 & sut2;
      Assert.IsTrue(wantedRes == res1, "operator &");
      Assert.IsFalse(res1.IsReadOnly, "IsReadOnly #1");

      IdList res2 = sut2 & sut1;
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
      IdList sut1 = CreateTestObject(sSUT1);
      sut1.SetReadOnly();
      IdList sut2 = CreateTestObject(sSUT2);
      sut2.SetReadOnly();

      IdList wantedRes = CreateTestObject(sWantedRes);

      IdList res = sut1 - sut2;
      Assert.IsTrue(wantedRes == res, "operator -");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region SingleId

    [TestCase("1", 1)]
    [TestCase("1,3", 0)]
    [TestCase("", 0)]
    public void SingleId(string sSUT, Int32 wantedRes)
    {
      IdList sut = CreateTestObject(sSUT);
      Assert.AreEqual(wantedRes, sut.SingleId);
    }

    #endregion

    #region FromId()

    [TestCase(1, "1")]
    [TestCase(0, "")]
    public void FromId(Int32 id, string sWantedRes)
    {
      IdList sut = IdList.FromId(id);
      Assert.AreEqual(CreateTestObject(sWantedRes), sut);
    }

    #endregion

    #region FromIds(), FromColumn()

    [Test]
    public void FromIds()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Id", typeof(Int32));
      tbl.Rows.Add("1");
      tbl.Rows.Add("3");
      tbl.Rows.Add("5");

      IdList wantedRes = new IdList(new Int32[] { 1, 3, 5 });

      IdList sut1 = IdList.FromIds(tbl);
      Assert.AreEqual(wantedRes, sut1, "DataTable");

      IdList sut2 = IdList.FromIds(tbl.DefaultView);
      Assert.AreEqual(wantedRes, sut2, "DataView");

      IdList sut3 = IdList.FromIds(DataTools.GetDataTableRows(tbl));
      Assert.AreEqual(wantedRes, sut3, "DataRow[]");

      DataRowView[] a4 = new DataRowView[tbl.DefaultView.Count];
      tbl.DefaultView.CopyTo(a4, 0);
      IdList sut4 = IdList.FromIds(a4);
      Assert.AreEqual(wantedRes, sut4, "DataRowView[]");
    }

    [Test]
    public void FromIds_empty()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Id", typeof(Int32));

      IdList sut1 = IdList.FromIds(tbl);
      Assert.AreEqual(0, sut1.Count, "DataTable");

      IdList sut2 = IdList.FromIds(tbl.DefaultView);
      Assert.AreEqual(0, sut2.Count, "DataView");

      IdList sut3 = IdList.FromIds(new DataRow[0]);
      Assert.AreEqual(0, sut3.Count, "DataRow[]");

      IdList sut4 = IdList.FromIds(new DataRowView[0]);
      Assert.AreEqual(0, sut4.Count, "DataRowView[]");
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

      IdList wantedRes = new IdList(new Int32[] { 1, 3, 5 });

      IdList sut1 = IdList.FromColumn(tbl, "F1");
      Assert.AreEqual(wantedRes, sut1, "DataTable");

      IdList sut2 = IdList.FromColumn(tbl.DefaultView, "F1");
      Assert.AreEqual(wantedRes, sut2, "DataView");

      IdList sut3 = IdList.FromColumn(DataTools.GetDataTableRows(tbl), "F1");
      Assert.AreEqual(wantedRes, sut3, "DataRow[]");

      DataRowView[] a4 = new DataRowView[tbl.DefaultView.Count];
      tbl.DefaultView.CopyTo(a4, 0);
      IdList sut4 = IdList.FromColumn(a4, "F1");
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

      IdList sut1 = IdList.FromColumn(tbl, "F1");
      Assert.AreEqual(0, sut1.Count, "DataTable");

      IdList sut2 = IdList.FromColumn(tbl.DefaultView, "F1");
      Assert.AreEqual(0, sut2.Count, "DataView");

      IdList sut3 = IdList.FromColumn(DataTools.GetDataTableRows(tbl), "F1");
      Assert.AreEqual(0, sut3.Count, "DataRow[]");

      DataRowView[] a4 = new DataRowView[tbl.DefaultView.Count];
      tbl.DefaultView.CopyTo(a4, 0);
      IdList sut4 = IdList.FromColumn(a4, "F1");
      Assert.AreEqual(0, sut4.Count, "DataRowView[]");
    }

    [Test]
    public void FromColumn_empty()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(Int32));

      IdList sut1 = IdList.FromColumn(tbl, "F1");
      Assert.AreEqual(0, sut1.Count, "DataTable");

      IdList sut2 = IdList.FromColumn(tbl.DefaultView, "F1");
      Assert.AreEqual(0, sut2.Count, "DataView");

      IdList sut3 = IdList.FromColumn(new DataRow[0], "F1");
      Assert.AreEqual(0, sut3.Count, "DataRow[]");

      IdList sut4 = IdList.FromColumn(new DataRowView[0], "F1");
      Assert.AreEqual(0, sut4.Count, "DataRowView[]");
    }

    #endregion

    #region FromArray()

    [Test]
    public void FromArray_1d()
    {
      Int32[] a = new Int32[] { 1, 3, 0, 3, 5 };
      IdList sut = IdList.FromArray(a);
      Assert.AreEqual(new IdList(new Int32[] { 1, 3, 5 }), sut);
    }

    [Test]
    public void FromArray_1d_empty()
    {
      IdList sut = IdList.FromArray(DataTools.EmptyIds);
      Assert.AreEqual(0, sut.Count);
    }

    [Test]
    public void FromArray_1d_null()
    {
      IdList sut = IdList.FromArray((Int32[])null);
      Assert.AreEqual(0, sut.Count);
    }


    [Test]
    public void FromArray_2d()
    {
      Int32[,] a = new Int32[,] { { 1, 3, 0 }, { 3, 5, 1 } };
      IdList sut = IdList.FromArray(a);
      Assert.AreEqual(new IdList(new Int32[] { 1, 3, 5 }), sut);
    }

    [Test]
    public void FromArray_2d_empty()
    {
      Int32[,] a = new Int32[0, 0];
      IdList sut = IdList.FromArray(a);
      Assert.AreEqual(0, sut.Count);
    }

    [Test]
    public void FromArray_2d_null()
    {
      IdList sut = IdList.FromArray((Int32[,])null);
      Assert.AreEqual(0, sut.Count);
    }


    [Test]
    public void FromArray_Jagged()
    {
      Int32[][] a = new Int32[][] {new Int32[] { 1, 3, 0 }, null, new Int32[]{ 3, 5, 1 } };
      IdList sut = IdList.FromArray(a);
      Assert.AreEqual(new IdList(new Int32[] { 1, 3, 5 }), sut);
    }

    [Test]
    public void FromArray_Jagged_empty()
    {
      Int32[][] a = new Int32[0][];
      IdList sut = IdList.FromArray(a);
      Assert.AreEqual(0, sut.Count);
    }

    [Test]
    public void FromArray_Jagged_null()
    {
      IdList sut = IdList.FromArray((Int32[][])null);
      Assert.AreEqual(0, sut.Count);
    }

    #endregion

    #region Empty

    [Test]
    public void Empty()
    {
      Assert.AreEqual(0, IdList.Empty.Count, "Count");
      Assert.IsTrue(IdList.Empty.IsReadOnly, "IsReadOnly");
    }

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
