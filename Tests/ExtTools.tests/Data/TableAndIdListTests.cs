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
  public class TableAndIdListTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_simple()
    {
      TableAndIdList sut = new TableAndIdList();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");

      // должно быть последним
      DoTestCaseSensitivity(sut, false);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Constructor_IgnoreCase(bool ignoreCase)
    {
      TableAndIdList sut = new TableAndIdList(ignoreCase);
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");

      // должно быть последним
      DoTestCaseSensitivity(sut, ignoreCase);
    }

    private static void DoTestCaseSensitivity(TableAndIdList sut, bool ignoreCase)
    {
      sut["AAA"].Add(1);
      sut["aaa"].Add(1);

      if (ignoreCase)
        Assert.AreEqual(1, sut.Count, "IgnoreCase=true");
      else
        Assert.AreEqual(2, sut.Count, "IgnoreCase=false");
    }

    #endregion

    #region Item

    [Test]
    public void Item_IdList_get()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["BBB"].Add(1);
      sut["CCC"].Clear();
      sut["BBB"].Add(2);

      Assert.IsTrue(new IdList(new Int32[] { 1 }) == sut["AAA"], "AAA");
      Assert.IsTrue(new IdList(new Int32[] { 1, 2 }) == sut["BBB"], "BBB"); // проверяем, что возвращается та же самая коллекция
      Assert.AreEqual(0, sut["CCC"].Count, "CCC");
      Assert.AreEqual(0, sut["DDD"].Count, "DDD"); // с ней ничего не делали

      sut["BBB"].Remove(1);
      Assert.AreEqual(2, sut.Count);
    }

    [Test]
    public void Item_IdList_set_replace_ids()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["BBB"].Add(2);
      sut["BBB"].Add(3);

      sut["BBB"] = new IdList(new Int32[] { 3, 4 });

      Assert.AreEqual(3, sut.Count, "Count");
      CollectionAssert.AreEquivalent(new Int32[] { 1 }, sut["AAA"].ToArray(), "AAA");
      CollectionAssert.AreEquivalent(new Int32[] { 1 }, sut["AAA"].ToArray(), "BBB");
    }

    [Test]
    public void Item_IdList_set_add_table()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);

      sut["BBB"] = new IdList(new Int32[] { 3, 4 });

      Assert.AreEqual(3, sut.Count, "Count");
      CollectionAssert.AreEquivalent(new Int32[] { 1 }, sut["AAA"].ToArray(), "AAA");
      CollectionAssert.AreEquivalent(new Int32[] { 1 }, sut["AAA"].ToArray(), "BBB");
    }

    [Test]
    public void Item_IdList_set_clear_ids()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["BBB"].Add(2);
      sut["BBB"].Add(3);

      sut["BBB"] = IdList.Empty;

      Assert.AreEqual(1, sut.Count, "Count");
      CollectionAssert.AreEquivalent(new Int32[] { 1 }, sut["AAA"].ToArray(), "AAA");
    }

    [Test]
    public void Item_IdList_set_empty_no_action()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);

      sut["BBB"] = IdList.Empty;

      Assert.AreEqual(1, sut.Count, "Count");
      CollectionAssert.AreEquivalent(new Int32[] { 1 }, sut["AAA"].ToArray(), "AAA");
    }

    [Test]
    public void Item_IdList_set_not_readonly()
    {
      // Проверяем, что присоединение списка не переводит в режим ReadOnly

      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["BBB"].Add(2);
      sut["BBB"].Add(3);

      IdList arg1 = new IdList(new Int32[] { 3, 4 });
      arg1.SetReadOnly();
      sut["BBB"] = arg1;
      Assert.IsFalse(sut["BBB"].IsReadOnly, "#1");

      sut["BBB"] = IdList.Empty;
      Assert.IsFalse(sut["BBB"].IsReadOnly, "#2");

      sut["CCC"] = IdList.Empty;
      Assert.IsFalse(sut["CCC"].IsReadOnly, "#2");
    }


    [Test]
    public void Item_Table_Id_get_caseSensitive()
    {
      TableAndIdList sut = new TableAndIdList(false);
      sut["AAA"].Add(new Int32[] { 1, 2, 3 });
      sut["aaa"].Add(new Int32[] { 3, 4, 5 });
      Assert.AreEqual(6, sut.Count, "Count");
      Assert.AreEqual(3, sut["AAA"].Count, "[AAA].Count");
      Assert.AreEqual(3, sut["aaa"].Count, "[aaa].Count");

      Assert.IsTrue(sut["AAA", 1], "[AAA, 1]");
      Assert.IsFalse(sut["aaa", 1], "[aaa, 1]");
    }

    [Test]
    public void Item_Table_Id_get_ignoreCase()
    {
      TableAndIdList sut = new TableAndIdList(true);
      sut["AAA"].Add(new Int32[] { 1, 2, 3 });
      sut["aaa"].Add(new Int32[] { 3, 4, 5 });
      Assert.AreEqual(5, sut.Count, "Count");
      Assert.AreEqual(5, sut["AAA"].Count, "[AAA].Count");
      Assert.AreEqual(5, sut["aaa"].Count, "[aaa].Count");

      Assert.IsTrue(sut["AAA", 1], "[AAA, 1]");
      Assert.IsTrue(sut["aaa", 1], "[aaa, 1]");
    }

    [Test]
    public void Item_Table_Id_set_caseSensitive()
    {
      TableAndIdList sut = new TableAndIdList(false);
      sut["AAA", 1] = true;
      sut["aaa", 1] = true;
      Assert.AreEqual(2, sut.Count, "Count #1");
      Assert.AreEqual(1, sut["AAA"].Count, "[AAA].Count #1");
      Assert.AreEqual(1, sut["aaa"].Count, "[aaa].Count #1");

      sut["AAA", 1] = false;
      Assert.AreEqual(1, sut.Count, "Count #2");
      Assert.AreEqual(0, sut["AAA"].Count, "[AAA].Count #2");
      Assert.AreEqual(1, sut["aaa"].Count, "[aaa].Count #2");
    }

    [Test]
    public void Item_Table_Id_set_ignoreCase()
    {
      TableAndIdList sut = new TableAndIdList(true);
      sut["AAA", 1] = true;
      sut["aaa", 1] = true;
      Assert.AreEqual(1, sut.Count, "Count #1");
      Assert.AreEqual(1, sut["AAA"].Count, "[AAA].Count #1");
      Assert.AreEqual(1, sut["aaa"].Count, "[aaa].Count #1");

      sut["AAA", 1] = false;
      Assert.AreEqual(0, sut.Count, "Count #2");
      Assert.AreEqual(0, sut["AAA"].Count, "[AAA].Count #2");
      Assert.AreEqual(0, sut["aaa"].Count, "[aaa].Count #2");
    }

    #endregion

    #region IsEmpty

    [Test]
    public void IsEmpty()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      Assert.IsFalse(sut.IsEmpty, "#1");

      sut["BBB"].Add(2);
      Assert.IsFalse(sut.IsEmpty, "#2");

      sut["AAA"].Remove(1);
      Assert.IsFalse(sut.IsEmpty, "#3");

      sut["BBB"].Clear();
      Assert.IsTrue(sut.IsEmpty, "#4");
    }

    #endregion

    #region Contains()

    [TestCase("AAA", true)]
    [TestCase("BBB", false)]
    [TestCase("CCC", false)]
    [TestCase("DDD", false)]
    [TestCase("", false)]
    public void Contains(string tableName, bool wantedRes)
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["BBB"].Add(1);
      sut["BBB"].Remove(1); // пустой
      sut["CCC"].Clear(); // пустой

      bool res = sut.Contains(tableName);

      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region TryGetValue()

    [TestCase("AAA", true)]
    [TestCase("BBB", false)]
    [TestCase("CCC", false)]
    [TestCase("DDD", false)]
    [TestCase("", false)]
    public void TryGetvalue(string tableName, bool wantedRes)
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["BBB"].Add(2);
      sut["BBB"].Remove(2);
      sut["CCC"] = IdList.Empty;

      IdList value;
      bool res = sut.TryGetValue(tableName, out value);
      Assert.AreEqual(wantedRes, res, "Result");

      if (wantedRes)
        Assert.AreSame(sut[tableName], value, "Value");
      else
        Assert.IsNull(value, "Value");
    }

    #endregion

    #region GetTableNames()

    [Test]
    public void GetTableNames_caseSensitive()
    {
      TableAndIdList sut = new TableAndIdList(false);
      // Порядок возвращаемых таблиц не гарантируется
      CollectionAssert.AreEquivalent(DataTools.EmptyStrings, sut.GetTableNames(), "#1");

      sut["AAA"].Add(1);
      sut["AAA"].Add(2);
      sut["aaa"].Add(3);
      CollectionAssert.AreEquivalent(new string[] { "AAA", "aaa" }, sut.GetTableNames(), "#2");

      sut["AAA"].Remove(1);
      sut["AAA"].Remove(2);
      CollectionAssert.AreEquivalent(new string[] { "aaa" }, sut.GetTableNames(), "#3");

      sut["DDD", 4] = false; // ничего не поменялось
      CollectionAssert.AreEquivalent(new string[] { "aaa" }, sut.GetTableNames(), "#4");
    }

    [Test]
    public void GetTableNames_ignoreCase()
    {
      TableAndIdList sut = new TableAndIdList(true);
      CollectionAssert.AreEquivalent(DataTools.EmptyStrings, sut.GetTableNames(), "#1");

      sut["AAA"].Add(1);
      sut["AAA"].Add(2);
      sut["aaa"].Add(3);
      CollectionAssert.AreEquivalent(new string[] { "AAA" }, sut.GetTableNames(), "#2");
    }

    #endregion

    #region Add(), Remove(), Clear()

    [Test]
    public void Add()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["BBB"].Add(2);
      sut["BBB"].Add(3);
      sut["CCC"].Add(4);

      TableAndIdList arg = new TableAndIdList();
      arg["AAA"].Add(2);
      arg["BBB"].Add(3);
      arg["BBB"].Add(4);
      arg["DDD"].Add(5);
      arg.SetReadOnly();

      sut.Add(arg);

      Assert.AreEqual(7, sut.Count, "Count");
      CollectionAssert.AreEquivalent(new Int32[] { 1, 2 }, sut["AAA"].ToArray(), "AAA");
      CollectionAssert.AreEquivalent(new Int32[] { 2, 3, 4 }, sut["BBB"].ToArray(), "BBB");
      CollectionAssert.AreEquivalent(new Int32[] { 4 }, sut["CCC"].ToArray(), "CCC");
      CollectionAssert.AreEquivalent(new Int32[] { 5 }, sut["DDD"].ToArray(), "DDD");
    }

    [Test]
    public void Add_Empty()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["BBB"].Add(2);
      sut["BBB"].Add(3);
      sut["CCC"].Add(4);

      sut.Add(TableAndIdList.Empty);

      Assert.AreEqual(4, sut.Count, "Count");
      CollectionAssert.AreEquivalent(new Int32[] { 1 }, sut["AAA"].ToArray(), "AAA");
      CollectionAssert.AreEquivalent(new Int32[] { 2, 3 }, sut["BBB"].ToArray(), "BBB");
      CollectionAssert.AreEquivalent(new Int32[] { 4 }, sut["CCC"].ToArray(), "CCC");
    }


    [Test]
    public void Remove_TableName_true()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["BBB"].Add(2);
      sut["BBB"].Add(3);
      sut["CCC"].Add(4);

      bool res = sut.Remove("BBB");
      Assert.IsTrue(res, "Result");

      Assert.AreEqual(2, sut.Count, "Count");
      CollectionAssert.AreEquivalent(new Int32[] { 1 }, sut["AAA"].ToArray(), "AAA");
      CollectionAssert.AreEquivalent(new Int32[] { 4 }, sut["CCC"].ToArray(), "CCC");
    }


    [TestCase("BBB")]
    [TestCase("CCC")]
    [TestCase("DDD")]
    [TestCase("")]
    public void Remove_TableName_false(string tableName)
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["BBB"].Add(2);
      sut["BBB"].Remove(2); // пустой
      sut["CCC"] = IdList.Empty;

      bool res = sut.Remove("BBB");
      Assert.IsFalse(res, "Result");

      Assert.AreEqual(1, sut.Count, "Count");
      CollectionAssert.AreEquivalent(new Int32[] { 1 }, sut["AAA"].ToArray(), "AAA");
    }

    [Test]
    public void Remove_TableAndIdList()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["BBB"].Add(2);
      sut["BBB"].Add(3);
      sut["CCC"].Add(4);

      TableAndIdList arg = new TableAndIdList();
      arg["AAA"].Add(2);
      arg["BBB"].Add(3);
      arg["BBB"].Add(4);
      arg["DDD"].Add(5);
      arg.SetReadOnly();

      sut.Remove(arg);

      Assert.AreEqual(3, sut.Count, "Count");
      CollectionAssert.AreEquivalent(new Int32[] { 1 }, sut["AAA"].ToArray(), "AAA");
      CollectionAssert.AreEquivalent(new Int32[] { 2 }, sut["BBB"].ToArray(), "BBB");
      CollectionAssert.AreEquivalent(new Int32[] { 4 }, sut["CCC"].ToArray(), "CCC");
    }

    [Test]
    public void Remove_TableAndIdList_Empty()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["BBB"].Add(2);
      sut["BBB"].Add(3);
      sut["CCC"].Add(4);

      sut.Remove(TableAndIdList.Empty);

      Assert.AreEqual(4, sut.Count, "Count");
      CollectionAssert.AreEquivalent(new Int32[] { 1 }, sut["AAA"].ToArray(), "AAA");
      CollectionAssert.AreEquivalent(new Int32[] { 2, 3 }, sut["BBB"].ToArray(), "BBB");
      CollectionAssert.AreEquivalent(new Int32[] { 4 }, sut["CCC"].ToArray(), "CCC");
    }

    [Test]
    public void Clear()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["BBB"].Add(2);
      sut["BBB"].Add(3);
      sut["CCC"].Add(4);

      sut.Clear();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
    }

    #endregion

    #region Сравнение

    [TestCase("EQ", true)]
    [TestCase("DIFF", false)]
    [TestCase("EMPTY", false)]
    public void Operator_Eq_NotEq(string sMode2, bool wantedEq)
    {
      TableAndIdList sut1 = new TableAndIdList();
      sut1["AAA"].Add(1);
      sut1["BBB"].Add(2);
      sut1["BBB"].Add(3);
      sut1.SetReadOnly();

      TableAndIdList sut2 = new TableAndIdList();
      switch (sMode2)
      {
        case "EQ":
          sut2["BBB"].Add(2);
          sut2["BBB"].Add(3);
          sut2["AAA"].Add(1);
          break;
        case "DIFF":
          sut2["AAA"].Add(1);
          sut2["BBB"].Add(2);
          sut2["BBB"].Add(4);
          break;
        case "EMPTY":
          break;
      }
      sut2.SetReadOnly();

      bool res1 = (sut1 == sut2);
      Assert.AreEqual(wantedEq, res1, "operator ==");

      bool res2 = (sut2 == sut1);
      Assert.AreEqual(wantedEq, res2, "operator == inverted");

      bool res3 = sut1.Equals(sut2);
      Assert.AreEqual(wantedEq, res3, "Equals()");

      bool res4 = sut2.Equals(sut1);
      Assert.AreEqual(wantedEq, res4, "Equals() inverted");

      bool res5 = (sut1 != sut2);
      Assert.AreEqual(!wantedEq, res5, "operator !=");

      bool res6 = (sut2 != sut1);
      Assert.AreEqual(!wantedEq, res6, "operator != inverted");
    }

    #endregion

    #region Операторы сложения и вычитания

    [Test]
    public void Operator_Plus_Or()
    {
      TableAndIdList sut1 = new TableAndIdList();
      sut1["AAA"].Add(1);
      sut1["BBB"].Add(2);
      sut1["BBB"].Add(3);
      sut1["CCC"].Add(4);
      sut1.SetReadOnly();

      TableAndIdList sut2 = new TableAndIdList();
      sut2["AAA"].Add(2);
      sut2["BBB"].Add(3);
      sut2["BBB"].Add(4);
      sut2["DDD"].Add(5);
      sut2.SetReadOnly();

      TableAndIdList res1 = sut1 + sut2;
      Assert.AreEqual(7, res1.Count, "Count");
      CollectionAssert.AreEquivalent(new Int32[] { 1, 2 }, res1["AAA"].ToArray(), "AAA");
      CollectionAssert.AreEquivalent(new Int32[] { 2, 3, 4 }, res1["BBB"].ToArray(), "BBB");
      CollectionAssert.AreEquivalent(new Int32[] { 4 }, res1["CCC"].ToArray(), "CCC");
      CollectionAssert.AreEquivalent(new Int32[] { 5 }, res1["DDD"].ToArray(), "DDD");

      TableAndIdList res2 = sut2 + sut1;
      Assert.IsTrue(res2 == res1, "operator + inverted");

      TableAndIdList res3 = sut1 | sut2;
      Assert.IsTrue(res3 == res1, "operator |");

      TableAndIdList res4 = sut2 | sut1;
      Assert.IsTrue(res4 == res1, "operator | inverted");
    }

    [Test]
    public void Operator_And()
    {
      TableAndIdList sut1 = new TableAndIdList();
      sut1["AAA"].Add(1);
      sut1["BBB"].Add(2);
      sut1["BBB"].Add(3);
      sut1["CCC"].Add(4);
      sut1.SetReadOnly();

      TableAndIdList sut2 = new TableAndIdList();
      sut2["AAA"].Add(2);
      sut2["BBB"].Add(3);
      sut2["BBB"].Add(4);
      sut2["DDD"].Add(5);
      sut2.SetReadOnly();

      TableAndIdList res1 = sut1 & sut2;
      Assert.AreEqual(1, res1.Count, "Count");
      CollectionAssert.AreEquivalent(new Int32[] { 3 }, res1["BBB"].ToArray(), "BBB");

      TableAndIdList res2 = sut2 & sut1;
      Assert.IsTrue(res2 == res1, "operator & inverted");
    }

    [Test]
    public void Operator_Substract()
    {
      TableAndIdList sut1 = new TableAndIdList();
      sut1["AAA"].Add(1);
      sut1["BBB"].Add(2);
      sut1["BBB"].Add(3);
      sut1["CCC"].Add(4);
      sut1.SetReadOnly();

      TableAndIdList sut2 = new TableAndIdList();
      sut2["AAA"].Add(2);
      sut2["BBB"].Add(3);
      sut2["BBB"].Add(4);
      sut2["DDD"].Add(5);
      sut2.SetReadOnly();

      TableAndIdList res = sut1 - sut2;

      Assert.AreEqual(3, res.Count, "Count");
      CollectionAssert.AreEquivalent(new Int32[] { 1 }, res["AAA"].ToArray(), "AAA");
      CollectionAssert.AreEquivalent(new Int32[] { 2 }, res["BBB"].ToArray(), "BBB");
      CollectionAssert.AreEquivalent(new Int32[] { 4 }, res["CCC"].ToArray(), "CCC");
    }

    #endregion

    #region Перечисление

    [Test]
    public void GetEnumerator()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["BBB"].Add(1);
      sut["BBB"].Remove(1); // пустой
      sut["CCC"].Clear(); // пустой
      sut["DDD"] = new IdList(new Int32[] { 1, 2, 3 });

      Dictionary<string, IdList> dict = new Dictionary<string, IdList>();
      foreach (KeyValuePair<string, IdList> pair in sut)
        dict.Add(pair.Key, pair.Value);

      Assert.AreEqual(2, dict.Count, "TableCount");
      Assert.IsTrue(dict.ContainsKey("AAA"), "Contains(AAA)");
      Assert.IsTrue(dict.ContainsKey("DDD"), "Contains(DDD)");

      Assert.AreSame(sut["AAA"], dict["AAA"], "AAA");
      Assert.AreSame(sut["DDD"], dict["DDD"], "AAA");
    }

    #endregion

    #region SetReadOnly()

    [Test]
    public void SetReadOnly()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["AAA"].Add(2);
      Assert.DoesNotThrow(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly() #1");

      sut.SetReadOnly();

      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly() #2");
      Assert.IsTrue(sut["AAA"].IsReadOnly, "[AAA].IsReadOnly");
      Assert.Catch(delegate() { sut["AAA"].Add(3); }, "[AAA].Add()");

      Assert.AreEqual(0, sut["BBB"].Count, "[BBB].Count is accessible");
      Assert.IsTrue(sut["BBB"].IsReadOnly, "[BBB].IsReadOnly");

      Assert.Catch(delegate() { sut["AAA", 1] = false; }, "Item set [AAA, 1]");
      Assert.Catch(delegate() { sut["BBB", 1] = false; }, "Item set [BBB, 1]");
      Assert.Catch(delegate() { sut["CCC"] = IdList.Empty; }, "Item set [CCC]");

      Assert.Catch(delegate() { sut.Add(TableAndIdList.Empty); }, "Add()");
      Assert.Catch(delegate() { sut.Remove("AAA"); }, "Remove(tableName)");
      Assert.Catch(delegate() { sut.Remove(TableAndIdList.Empty); }, "Remove(TableAndIdList)");
      Assert.Catch(delegate() { sut.Clear(); }, "Clear()");

      Assert.AreEqual(2, sut.Count, "Count"); // ничего не испортилось
    }

    #endregion

    #region Empty

    [Test]
    public static void Empty()
    {
      Assert.AreEqual(0, TableAndIdList.Empty.Count, "Count");
      Assert.IsTrue(TableAndIdList.Empty.IsEmpty, "IsEmpty");
      Assert.IsTrue(TableAndIdList.Empty.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone()
    {
      TableAndIdList sut = new TableAndIdList();
      sut["AAA"].Add(1);
      sut["AAA"].Add(2);
      sut.SetReadOnly();

      TableAndIdList res = sut.Clone();

      Assert.AreEqual(2, res.Count, "Count");
      CollectionAssert.AreEquivalent(new Int32[] { 1, 2 }, res["AAA"].ToArray(), "ToArray()");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Clone_preserves_case_sensityivity(bool ignoreCase)
    {
      TableAndIdList sut = new TableAndIdList(ignoreCase);
      sut.SetReadOnly();

      TableAndIdList res = sut.Clone();

      DoTestCaseSensitivity(res, ignoreCase);
    }

    #endregion
  }
}
