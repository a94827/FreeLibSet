using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using AgeyevAV.ExtDB;
using AgeyevAV;

namespace ExtDB.tests
{
  [TestFixture]
  public class DBxColumnListTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_without_args()
    {
      DBxColumnList sut = new DBxColumnList();

      Assert.AreEqual(0, sut.Count, "Count=0");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly=false");
      Assert.AreEqual("", sut.AsString, "AsString");
    }

    [Test]
    public void Constructor_with_collection()
    {
      ICollection<string> col = new string[] { "F1", "F2", "F3" };

      DBxColumnList sut = new DBxColumnList(col);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly=false");
      Assert.AreEqual("F1,F2,F3", sut.AsString, "AsString");
    }

    [Test]
    public void Constructor_with_enumerable()
    {
      ArrayEnumerable<string> en = new ArrayEnumerable<string>(new string[] { "F1", "F2", "F3" });

      DBxColumnList sut = new DBxColumnList(en);

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly=false");
      Assert.AreEqual("F1,F2,F3", sut.AsString, "AsString");
    }

    [Test]
    public void Constructor_with_string()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      Assert.AreEqual(3, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly=false");
      Assert.AreEqual("F1,F2,F3", sut.AsString, "AsString");
    }

    [Test]
    public void Constructor_with_empty_string()
    {
      DBxColumnList sut = new DBxColumnList(String.Empty);

      Assert.AreEqual(0, sut.Count, "Count=0");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly=false");
      Assert.AreEqual("", sut.AsString, "AsString");
    }

    #endregion

    #region Add()/Remove()

    [TestCase("F1,F2", "F3", "F1,F2,F3")]
    [TestCase("F1,F2", "F1", "F1,F2")]
    [TestCase("F1,F2", "F3,F3", "F1,F2,F3")]
    [TestCase("F1,F2", "F3,F1", "F1,F2,F3")]
    [TestCase("", "F1,F2", "F1,F2")]
    [TestCase("F1,F2", "", "F1,F2")]
    [TestCase("", "", "")]
    public void Add(string original, string removed, string expected)
    {
      // Проверяем сразу 3 перегрузки
      DBxColumnList sut1 = new DBxColumnList(original);
      sut1.Add(removed);
      Assert.AreEqual(expected, sut1.AsString, "String");

      DBxColumnList sut2 = new DBxColumnList(original);
      sut2.Add(new DBxColumns(removed));
      Assert.AreEqual(expected, sut2.AsString, "DBxColumns");

      DBxColumnList sut3 = new DBxColumnList(original);
      sut3.Add(new DBxColumnList(removed));
      Assert.AreEqual(expected, sut3.AsString, "DBxColumnList");
    }



    [TestCase("F1,F2", "F1", "F2")]
    [TestCase("F1,F2", "F2,F3", "F1")]
    [TestCase("F1,F2", "F3,F1", "F2")]
    [TestCase("F1,F2", "F3,F1,F2",  "")]
    [TestCase("", "F1", "")]
    [TestCase("", "F1,F1", "")]
    [TestCase("F1,F2", "", "F1,F2")]
    [TestCase("", "", "")]
    public void Remove(string original, string removed, string expected)
    {
      // Проверяем сразу 3 перегрузки
      DBxColumnList sut1 = new DBxColumnList(original);
      sut1.Remove(removed);
      Assert.AreEqual(expected, sut1.AsString, "String");

      DBxColumnList sut2 = new DBxColumnList(original);
      sut2.Remove(new DBxColumns(removed));
      Assert.AreEqual(expected, sut2.AsString, "DBxColumns");

      DBxColumnList sut3 = new DBxColumnList(original);
      sut3.Remove(new DBxColumnList(removed));
      Assert.AreEqual(expected, sut3.AsString, "DBxColumnList");
    }

    #endregion

    #region Clear()

    [Test]
    public void Clear()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      sut.Clear();
      Assert.AreEqual(0, sut.Count);
    }

    #endregion

    #region Contains()

    [Test]
    public void Contains_simple_string()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2");

      bool res1 = sut.Contains("F1");
      Assert.IsTrue(res1, "column found");

      bool res2 = sut.Contains("F3");
      Assert.IsFalse(res2, "column not found");
    }

    [Test]
    public void Contains_empty_string()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2");

      bool res1 = sut.Contains(String.Empty);
      Assert.IsTrue(res1, "empty string");

      bool res2 = sut.Contains((String)null);
      Assert.IsTrue(res2, "null string");
    }

    [Test]
    public void Contains_empty_string_in_empty_list()
    {
      DBxColumnList sut = new DBxColumnList();


      bool res1 = sut.Contains(String.Empty);
      Assert.IsTrue(res1, "empty string");

      bool res2 = sut.Contains((String)null);
      Assert.IsTrue(res2, "null string");
    }

    [Test]
    public void Contains_string_with_commas()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      bool res1 = sut.Contains("F1,F3");
      Assert.IsTrue(res1, "all columns found");

      bool res2 = sut.Contains("F1,F4");
      Assert.IsFalse(res2, "some columns not found");

      bool res3 = sut.Contains("F4,F5");
      Assert.IsFalse(res3, "none columns found");
    }

    [Test]
    public void Contains_DBxColumns()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      bool res1 = sut.Contains(new DBxColumns("F1,F3"));
      Assert.IsTrue(res1, "all columns found");

      bool res2 = sut.Contains(new DBxColumns("F3,F4"));
      Assert.IsFalse(res2, "some columns not found");

      bool res3 = sut.Contains(new DBxColumns("F4,F5"));
      Assert.IsFalse(res3, "none columns found");
    }


    [Test]
    public void Contains_DBxColumns_empty()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      bool res1 = sut.Contains(DBxColumns.Empty);
      Assert.IsTrue(res1, "true if list is empty");

      bool res2 = sut.Contains((DBxColumns)null);
      Assert.IsTrue(res2, "true if list is null");
    }

    [Test]
    public void Contains_DBxColumnList()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      bool res1 = sut.Contains(new DBxColumnList("F1,F3"));
      Assert.IsTrue(res1, "all columns found");

      bool res2 = sut.Contains(new DBxColumnList("F3,F4"));
      Assert.IsFalse(res2, "some columns not found");

      bool res3 = sut.Contains(new DBxColumnList("F4,F5"));
      Assert.IsFalse(res3, "none columns found");
    }


    [Test]
    public void Contains_DBxColumnList_empty()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      bool res1 = sut.Contains(DBxColumnList.Empty);
      Assert.IsTrue(res1, "true if list is empty");

      bool res2 = sut.Contains((DBxColumnList)null);
      Assert.IsTrue(res2, "true if list is null");
    }

    #endregion

    #region ContainsAny()

    [Test]
    public void ContainsAny_simple_string()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2");

      bool res1 = sut.ContainsAny("F1");
      Assert.IsTrue(res1, "column found");

      bool res2 = sut.ContainsAny("F3");
      Assert.IsFalse(res2, "column not found");
    }

    [Test]
    public void ContainsAny_empty_string()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2");

      bool res1 = sut.ContainsAny(String.Empty);
      Assert.IsFalse(res1, "empty string");

      bool res2 = sut.ContainsAny((String)null);
      Assert.IsFalse(res2, "null string");
    }

    [Test]
    public void ContainsAny_empty_string_in_empty_list()
    {
      DBxColumnList sut = new DBxColumnList();


      bool res1 = sut.ContainsAny(String.Empty);
      Assert.IsFalse(res1, "empty string");

      bool res2 = sut.ContainsAny((String)null);
      Assert.IsFalse(res2, "null string");
    }

    [Test]
    public void ContainsAny_string_with_commas()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      bool res1 = sut.ContainsAny("F1,F3");
      Assert.IsTrue(res1, "all columns found");

      bool res2 = sut.ContainsAny("F1,F4");
      Assert.IsTrue(res2, "some columns found");

      bool res3 = sut.ContainsAny("F4,F5");
      Assert.IsFalse(res3, "none columns found");
    }

    [Test]
    public void ContainsAny_DBxColumns()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      bool res1 = sut.ContainsAny(new DBxColumns("F1,F3"));
      Assert.IsTrue(res1, "all columns found");

      bool res2 = sut.ContainsAny(new DBxColumns("F3,F4"));
      Assert.IsTrue(res2, "some columns found");

      bool res3 = sut.ContainsAny(new DBxColumns("F4,F5"));
      Assert.IsFalse(res3, "none columns found");
    }

    [Test]
    public void ContainsAny_DBxColumns_empty()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      bool res1 = sut.ContainsAny(DBxColumns.Empty);
      Assert.IsFalse(res1, "false if list is empty");

      bool res2 = sut.ContainsAny((DBxColumns)null);
      Assert.IsFalse(res2, "false if list is null");
    }

    [Test]
    public void ContainsAny_DBxColumnList()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      bool res1 = sut.ContainsAny(new DBxColumnList("F1,F3"));
      Assert.IsTrue(res1, "all columns found");

      bool res2 = sut.ContainsAny(new DBxColumnList("F3,F4"));
      Assert.IsTrue(res2, "some columns found");

      bool res3 = sut.ContainsAny(new DBxColumnList("F4,F5"));
      Assert.IsFalse(res3, "none columns found");
    }


    [Test]
    public void ContainsAny_DBxColumnList_empty()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      bool res1 = sut.ContainsAny(DBxColumnList.Empty);
      Assert.IsFalse(res1, "false if list is empty");

      bool res2 = sut.ContainsAny((DBxColumnList)null);
      Assert.IsFalse(res2, "false if list is null");
    }

    #endregion

    #region ContainsStartedWith()

    [Test]
    public void ContainsStartedWith()
    {
      DBxColumnList sut = new DBxColumnList("A.1,A.2,B.3");

      bool res1 = sut.ContainsStartedWith("A.");
      Assert.IsTrue(res1, "found");

      bool res2 = sut.ContainsStartedWith("C.");
      Assert.IsFalse(res2, "not found");
    }

    [Test]
    public void ContainsStartedWith_empty_prefix()
    {
      DBxColumnList sut = new DBxColumnList("A.1,A.2,B.3");
      bool res = sut.ContainsStartedWith(String.Empty);
      Assert.IsTrue(res);
    }

    [Test]
    public void ContainsStartedWith_empty_prefix_for_empty_list()
    {
      DBxColumnList sut = new DBxColumnList();
      bool res = sut.ContainsStartedWith(String.Empty);
      Assert.IsFalse(res);
    }

    #endregion

    #region HasMoreThan()

    [Test]
    public void HasMoreThan_DBxColumns()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      bool res1 = sut.HasMoreThan(new DBxColumns("F1,F3"));
      Assert.IsTrue(res1, "there are extra columns");

      bool res2 = sut.HasMoreThan(new DBxColumns("F1,F2,F3"));
      Assert.IsFalse(res2, "there are no extra columns");

      bool res3 = sut.HasMoreThan(new DBxColumns("F1,F2,F3,F4"));
      Assert.IsFalse(res3, "there are no extra columns too");
    }

    [Test]
    public void HasMoreThan_DBxColumns_empty()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      bool res1 = sut.HasMoreThan(DBxColumns.Empty);
      Assert.IsTrue(res1, "empty other columns");

      bool res2 = sut.HasMoreThan((DBxColumns)null);
      Assert.IsTrue(res2, "null other columns");
    }

    [Test]
    public void HasMoreThan_DBxColumns_empty_for_empty_list()
    {
      DBxColumnList sut = new DBxColumnList();

      bool res1 = sut.HasMoreThan(DBxColumns.Empty);
      Assert.IsFalse(res1, "empty other columns");

      bool res2 = sut.HasMoreThan((DBxColumns)null);
      Assert.IsFalse(res2, "null other columns");
    }


    [Test]
    public void HasMoreThan_DBxColumnList()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      bool res1 = sut.HasMoreThan(new DBxColumnList("F1,F3"));
      Assert.IsTrue(res1, "there are extra columns");

      bool res2 = sut.HasMoreThan(new DBxColumnList("F1,F2,F3"));
      Assert.IsFalse(res2, "there are no extra columns");

      bool res3 = sut.HasMoreThan(new DBxColumnList("F1,F2,F3,F4"));
      Assert.IsFalse(res3, "there are no extra columns too");
    }

    [Test]
    public void HasMoreThan_DBxColumnList_empty()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");

      bool res1 = sut.HasMoreThan(DBxColumnList.Empty);
      Assert.IsTrue(res1, "empty other columns");

      bool res2 = sut.HasMoreThan((DBxColumnList)null);
      Assert.IsTrue(res2, "null other columns");
    }

    [Test]
    public void HasMoreThan_DBxColumnList_empty_for_empty_list()
    {
      DBxColumnList sut = new DBxColumnList();

      bool res1 = sut.HasMoreThan(DBxColumnList.Empty);
      Assert.IsFalse(res1, "empty other columns");

      bool res2 = sut.HasMoreThan((DBxColumnList)null);
      Assert.IsFalse(res2, "null other columns");
    }

    [Test]
    public void HasMoreThan_DBxColumnList_static()
    {
      bool res1 = DBxColumnList.HasMoreThan((DBxColumnList)null, (DBxColumnList)null);
      Assert.IsFalse(res1, "null and null");

      bool res2 = DBxColumnList.HasMoreThan((DBxColumnList)null, new DBxColumnList("F2"));
      Assert.IsFalse(res2, "null and not null");

      bool res3 = DBxColumnList.HasMoreThan(new DBxColumnList("F1"), (DBxColumnList)null);
      Assert.IsTrue(res3, "not null and null");

      bool res4 = DBxColumnList.HasMoreThan(new DBxColumnList("F1"), new DBxColumnList("F2"));
      Assert.IsTrue(res4, "not null and not null - normal operation");
    }

    #endregion

    #region ContainsDots()

    [Test]
    public void ContainsDots()
    {
      DBxColumnList sut1 = new DBxColumnList("A,B.C,D");
      Assert.IsTrue(sut1.ContainsDots, "contains");

      DBxColumnList sut2 = new DBxColumnList("A,B,C,D");
      Assert.IsFalse(sut2.ContainsDots, "no dots");

      DBxColumnList sut3 = new DBxColumnList("");
      Assert.IsFalse(sut3.ContainsDots, "is empty");
    }

    #endregion

    #region IsReadOnly

    [Test]
    public void SetReadOnly()
    {
      DBxColumnList sut = new DBxColumnList();
      sut.SetReadOnly();
      Assert.IsTrue(sut.IsReadOnly);
    }

    [Test]
    public void SetReadOnly_exceptions()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");
      sut.SetReadOnly();

      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.CheckNotReadOnly(); });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Add("F4"); });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Add(new DBxColumns("F4")); });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Add(new DBxColumnList("F4")); });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Remove("F1"); });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Remove(new DBxColumns("F1")); });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Remove(new DBxColumnList("F1")); });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Clear(); });
    }

    #endregion

    #region CloneXXX()

    [Test]
    public void Clone()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");
      sut.SetReadOnly();

      DBxColumnList res = sut.Clone();
      Assert.AreEqual("F1,F2,F3", res.AsString, "result");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly=false");
    }

    [Test]
    public void CloneWithPrefix()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");
      sut.SetReadOnly();

      DBxColumnList res = sut.CloneWithPrefix("A.");
      Assert.AreEqual("A.F1,A.F2,A.F3", res.AsString, "result");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly=false");
    }

    [Test]
    public void CloneWithSuffix()
    {
      DBxColumnList sut = new DBxColumnList("F1,F2,F3");
      sut.SetReadOnly();

      DBxColumnList res = sut.CloneWithSuffix(".B");
      Assert.AreEqual("F1.B,F2.B,F3.B", res.AsString, "result");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly=false");
    }

    #endregion

    #region Empty

    [Test]
    public void Empty()
    {
      Assert.AreEqual(0, DBxColumnList.Empty.Count, "Count=0");
      Assert.IsTrue(DBxColumnList.Empty.IsReadOnly, "IsReadOnly=true");
    }

    #endregion
  }
}
