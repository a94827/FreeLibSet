using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class DBxTableColumnListTests
  {
    [Test]
    public void Constructor()
    {
      DBxTableColumnList sut = new DBxTableColumnList();

      Assert.AreEqual(0, sut.Count, "Count=0");
      Assert.AreEqual(0, sut.Tables.Count, "Tables.Count=0");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly=false");
    }

    [Test]
    public void Tables_Add()
    {
      DBxTableColumnList sut = new DBxTableColumnList();

      sut.Tables["Tab1"].AddRange(new DBxColumnList(new string[] { "F1", "F2" }));
      sut.Tables["Tab2"].Add("F8,F9"); // другая таблица
      sut.Tables["Tab1"].AddRange(new DBxColumnList(new string[] { "F3" }));
      sut.Tables["Tab1"].AddRange(new DBxColumnList(new string[] { "F2" })); // повторное добавление

      DBxTableColumnName[] wanted = new DBxTableColumnName[]{
        new DBxTableColumnName("Tab1", "F1"),
        new DBxTableColumnName("Tab1", "F2"),
        new DBxTableColumnName("Tab1", "F3"),
        new DBxTableColumnName("Tab2", "F8"),
        new DBxTableColumnName("Tab2", "F9"),
      };
      Assert.AreEqual(wanted, sut.ToArray());
    }


    [Test]
    public void Tables_ContainsKey()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].AddRange(new DBxColumnList(new string[] { "F1", "F2" }));

      Assert.IsTrue(sut.Tables.ContainsKey("Tab1"));
      Assert.IsFalse(sut.Tables.ContainsKey("Tab2"));
    }

    [Test]
    public void Tables_Keys()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].AddRange(new DBxColumnList(new string[] { "F1", "F2" }));
      sut.Tables["Tab2"].Add("F9");

      string[] res = new string[sut.Tables.Keys.Count];
      sut.Tables.Keys.CopyTo(res, 0);

      Assert.AreEqual(new string[] { "Tab1", "Tab2" }, res);
    }

    [Test]
    public void Tables_Remove()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");
      sut.Tables["Tab2"].Add("F3,F4"); // другая таблица

      sut.Tables.Remove("Tab1");

      Assert.IsFalse(sut["Tab1", "F1"], "table must be deleted");
      Assert.IsTrue(sut["Tab2", "F3"], "table must remain");
    }

    [Test]
    public void Tables_TryGetValue()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");

      DBxColumnList list;
      bool res = sut.Tables.TryGetValue("Tab1", out list);

      Assert.IsTrue(res);
      Assert.AreEqual(2, list.Count);
    }

    [Test]
    public void Tables_Item_get_exist()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");

      DBxColumnList list = sut.Tables["Tab1"];

      Assert.AreEqual(2, list.Count);
    }

    [Test]
    public void Tables_Item_get_empty()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");

      DBxColumnList list = sut.Tables["Tab2"];

      Assert.IsNotNull(list);
      Assert.AreEqual(0, list.Count);
    }

    [TestCase("F1,F2", "F2,F3", Result="F2,F3")]
    [TestCase("F1,F2", "", Result = "")]
    [TestCase("", "F2,F3", Result = "F2,F3")]
    [TestCase("", "", Result = "")]
    public string Tables_Item_set_same_table(string original, string arg)
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add(original);

      sut.Tables["Tab1"] = new DBxColumnList(arg);

      return sut.Tables["Tab1"].AsString;
    }

    [Test]
    public void Tables_Item_other_table()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");

      sut.Tables["Tab2"] = new DBxColumnList("F2,F3");

      Assert.AreEqual(4, sut.Count);
    }


    [Test]
    public void Tables_CopyTo()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");

      KeyValuePair<string, DBxColumnList>[] a = new KeyValuePair<string, DBxColumnList>[sut.Tables.Count];
      sut.Tables.CopyTo(a, 0);

      Assert.AreEqual(1, a.Length, "array length");
      Assert.AreEqual("Tab1", a[0].Key, "table name");
      Assert.AreEqual(2, a[0].Value.Count, "column count");
    }

    [Test]
    public void Tables_Count()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");
      sut.Tables["Tab2"].Add("F3");

      Assert.AreEqual(2, sut.Tables.Count, "before removing");

      sut["Tab2", "F3"] = false; // now Tab2 is empty, but Tables.Count is not defined
      sut.SetReadOnly();

      Assert.AreEqual(1, sut.Tables.Count, "after removing");
    }

    [Test]
    public void Tables_GetEnumerator()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");
      sut.Tables["Tab2"].Add("F3");

      List<KeyValuePair<string, DBxColumnList>> lst = new List<KeyValuePair<string, DBxColumnList>>();
      foreach (KeyValuePair<string, DBxColumnList> pair in sut.Tables)
        lst.Add(pair);

      Assert.AreEqual(2, lst.Count, "iteration count");
      Assert.AreEqual("Tab1", lst[0].Key, "first table name");
      Assert.AreEqual(2, lst[0].Value.Count, "first table column count");
      Assert.AreEqual("Tab2", lst[1].Key, "second table name");
      Assert.AreEqual(1, lst[1].Value.Count, "second table column count");
    }

    [Test]
    public void IsReadOnly()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");

      Assert.IsFalse(sut.IsReadOnly, "before SetReadOnly");

      sut.SetReadOnly();

      Assert.IsTrue(sut.IsReadOnly, "after SetReadOnly");
      Assert.IsTrue(sut.Tables["Tab1"].IsReadOnly, "DBxColumnList.IsReadOnly");
    }

    [Test]
    public void SetReadOnly_saved_count()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");
      sut.Tables["Tab2"].Add("F3");
      sut["Tab2", "F3"] = false;
      Assert.AreEqual(2, sut.Count, "before SetReadOnly()");

      sut.SetReadOnly();
      Assert.AreEqual(2, sut.Count, "after SetReadOnly()");
    }

    [Test]
    public void SetReadOnly_remove_empty()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1");
      sut["Tab1", "F1"] = false;
      sut.SetReadOnly();

      Assert.AreEqual(0, sut.Count);
    }

    [Test]
    public void IsReadOnly_exceptions()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1");
      sut.SetReadOnly();

      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.CheckNotReadOnly(); });

      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Tables["Tab1"].Add("F3"); });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Tables.Remove("Tab1"); });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Tables["Tab1"] = new DBxColumnList("F3"); });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Tables["Tab2"] = new DBxColumnList("F4"); });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Tables.Clear(); });

      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Add(new DBxTableColumnName("Tab1", "F3")); });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Remove(new DBxTableColumnName("Tab1", "F1")); });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut.Clear(); });

      Assert.Throws<ObjectReadOnlyException>(delegate() { sut[new DBxTableColumnName("Tab1", "F1")] = true; });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut[new DBxTableColumnName("Tab1", "F1")] = false; }); // действия для true и false могут выполняться по разному

      Assert.Throws<ObjectReadOnlyException>(delegate() { sut["Tab1", "F1"] = true; });
      Assert.Throws<ObjectReadOnlyException>(delegate() { sut["Tab1", "F1"] = false; }); // действия для true и false могут выполняться по разному
    }

    [Test]
    public void Pairs_Add()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1");

      sut.Add(new DBxTableColumnName("Tab1", "F2"));

      Assert.AreEqual(2, sut.Count, "Count");
      Assert.AreEqual(2, sut.Tables["Tab1"].Count, "Tables.Count");
    }


    [Test]
    public void Pairs_AddRange()
    {
      DBxTableColumnList sut = new DBxTableColumnList();

      DBxTableColumnName[] a = new DBxTableColumnName[]{
        new DBxTableColumnName("Tab1", "F1"),
        new DBxTableColumnName("Tab2", "F2"),
        new DBxTableColumnName("Tab1", "F3")};
      ArrayEnumerable<DBxTableColumnName> ae = new ArrayEnumerable<DBxTableColumnName>(a);

      sut.AddRange(ae);

      Assert.AreEqual(3, sut.Count);
      Assert.AreEqual(2, sut.Tables.Count);
    }


    [Test]
    public void Pairs_AddRange_exception()
    {
      DBxTableColumnList sut = new DBxTableColumnList();

      Assert.Throws<ArgumentException>(delegate() { sut.AddRange(sut); });
    }

    [Test]
    public void Pairs_Remove()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");

      bool res1 = sut.Remove(new DBxTableColumnName("Tab1", "F1"));
      Assert.IsTrue(res1, "item has been found");

      bool res2 = sut.Remove(new DBxTableColumnName("Tab1", "F9"));
      Assert.IsFalse(res2, "no such column");

      bool res3 = sut.Remove(new DBxTableColumnName("Tab2", "F2"));
      Assert.IsFalse(res2, "no such table");

      Assert.AreEqual(1, sut.Count);
    }

    [Test]
    public void Clear()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");

      sut.Clear();

      Assert.AreEqual(0, sut.Count);
    }

    [Test]
    public void Pairs_Contains()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");

      Assert.IsTrue(sut[new DBxTableColumnName("Tab1", "F1")], "column exists");
      Assert.IsFalse(sut[new DBxTableColumnName("Tab1", "F3")], "column does not exist");
      Assert.IsFalse(sut[new DBxTableColumnName("Tab2", "F1")], "table does not exist");
    }


    [Test]
    public void Count()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut["Tab1", "F1"] = true;
      sut["Tab1", "F2"] = true;
      sut["Tab2", "F3"] = true;

      Assert.AreEqual(3, sut.Count);
    }

    [Test]
    public void Pairs_CopyTo()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");
      sut.Tables["Tab2"].Add("F3");

      DBxTableColumnName[] a = new DBxTableColumnName[sut.Count];
      sut.CopyTo(a, 0);

      // Порядок элементов является предопределенным
      Assert.AreEqual("Tab1.F1,Tab1.F2,Tab2.F3", GetTestedArrayString(a));
    }

    [Test]
    public void Pairs_ToArray()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");
      sut.Tables["Tab2"].Add("F3");

      DBxTableColumnName[] a = sut.ToArray();

      // Порядок элементов является предопределенным
      Assert.AreEqual("Tab1.F1,Tab1.F2,Tab2.F3", GetTestedArrayString(a));
    }


    [Test]
    public void Pairs_GetEnumerator()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");
      sut.Tables["Tab2"].Add("F3");

      List<DBxTableColumnName> lst = new List<DBxTableColumnName>();
      foreach (DBxTableColumnName item in sut)
        lst.Add(item);

      // Порядок элементов является предопределенным
      Assert.AreEqual("Tab1.F1,Tab1.F2,Tab2.F3", GetTestedArrayString(lst.ToArray()));
    }

    private static string GetTestedArrayString(DBxTableColumnName[] a)
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < a.Length; i++)
      {
        if (i > 0)
          sb.Append(',');
        sb.Append(a[i].TableName);
        sb.Append('.');
        sb.Append(a[i].ColumnName);
      }
      return sb.ToString();
    }

    [Test]
    public void Pairs_BoolItem_get()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");
      sut.SetReadOnly();

      Assert.IsTrue(sut["Tab1", "F2"], "column exists");
      Assert.IsFalse(sut["Tab1", "F3"], "column does not exist");
      Assert.IsFalse(sut["Tab2", "F1"], "table does not exist");
    }

    [Test]
    public void Pairs_BoolItem_set()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");

      sut["Tab1", "F3"] = true;
      sut["Tab1", "F1"] = false;

      Assert.AreEqual(2, sut.Count);
    }

    [Test]
    public void Clone()
    {
      DBxTableColumnList sut = new DBxTableColumnList();
      sut.Tables["Tab1"].Add("F1,F2");
      sut.Tables["Tab2"].Add("F3");
      sut.SetReadOnly();

      DBxTableColumnList res = sut.Clone();
      Assert.AreEqual(3, res.Count);
      Assert.IsFalse(res.IsReadOnly);
    }

    [Test]
    public void Empty()
    {
      Assert.AreEqual(0, DBxTableColumnList.Empty.Count);
      Assert.IsTrue(DBxTableColumnList.Empty.IsReadOnly);
    }
  }
}
