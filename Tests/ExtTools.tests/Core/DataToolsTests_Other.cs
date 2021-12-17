using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using NUnit.Framework;
using FreeLibSet.Core;

namespace ExtTools_tests.Core
{
  [TestFixture]
  class DataToolsTests_Other
  {
    #region CloneDataColumn

    [Test]
    public void CloneDataColumn()
    {
      DataColumn col1 = new DataColumn("AAA", typeof(string));

      DataColumn col2 = DataTools.CloneDataColumn(col1);

      PropertyAssert.ProperiesEqual(col1, col2);
    }

    #endregion

    #region TrimEnd()

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public void TrimEnd_DataTable(int rowCount)
    {
      DataTable table = new DataTable();
      table.Columns.Add("Col1", typeof(string)); // с пробелами
      table.Columns.Add("Col2", typeof(string)); // без пробелов
      for (int i = 0; i < rowCount; i++)
        table.Rows.Add(i.ToString(), // Col1
          i.ToString() + new string(' ', i + 2)); // Col2

      DataTools.TrimEnd(table);

      Assert.AreEqual(rowCount, table.Rows.Count, "All rows present");
      for (int i = 0; i < rowCount; i++)
      {
        string v1 = table.Rows[i]["Col1"].ToString();
        string v2 = table.Rows[i]["Col2"].ToString();
        Assert.AreEqual(i.ToString(), v1, "Unchanged Col1");
        Assert.AreEqual(i.ToString(), v2, "Trimmed Col2");
      }
    }

    #endregion

    #region SortDataRow()

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(100)]
    public void SortDataRow(int rowCount)
    {
      int[] TestValues = new int[] { 5, 1, 10, 4 }; // можно было бы использовать генератор случайных чисел
      DataTable table = new DataTable();
      table.Columns.Add("Col1", typeof(int));
      for (int i = 0; i < rowCount; i++)
      {
        int v = TestValues[i % TestValues.Length]; // перебираем по кругу
        table.Rows.Add(v);
      }
      DataRow[] a = DataTools.GetDataTableRows(table); // нужен массив

      DataTools.SortDataRows(a, "Col1");

      Assert.AreEqual(rowCount, a.Length, "Array length");
      int[] a2 = new int[rowCount];
      for (int i = 0; i < rowCount; i++)
        a2[i] = (int)(a[i]["Col1"]);

      CollectionAssert.IsOrdered(a2);
    }

    #endregion

    #region NamesAndValuesToPairs(), PairsToNamesAndValues()

    [Test]
    public void NamesAndValuesToPairs()
    {
      string[] colNames = new string[] { "F1", "F2", "F3" };
      object[] values = new object[] { "ABC", 2, 3.0 };

      Hashtable res = DataTools.NamesAndValuesToPairs(colNames, values);

      Assert.AreEqual(3, res.Count, "Count");
      Assert.AreEqual("ABC", res["F1"], "[0]");
      Assert.AreEqual(2, res["F2"], "[1]");
      Assert.AreEqual(3.0, res["F3"], "[2]");
    }

    [Test]
    public void PairsToNamesAndValues()
    {
      Dictionary<string, object> dict = new Dictionary<string, object>();
      dict.Add("F1", "ABC");
      dict.Add("F2", 2);
      dict.Add("F3", 3.0);


      string[] names;
      object[] values;
      DataTools.PairsToNamesAndValues(dict, out names, out values);

      Assert.AreEqual(3, names.Length, "Names.Length");
      Assert.AreEqual(3, values.Length, "Values.Length");

      Assert.AreEqual("ABC", values[Array.IndexOf<string>(names, "F1")], "[0]");
      Assert.AreEqual(2, values[Array.IndexOf<string>(names, "F2")], "[1]");
      Assert.AreEqual(3.0, values[Array.IndexOf<string>(names, "F3")], "[2]");
    }

    #endregion

    #region AreEnumerablesEqual()

    [TestCase("1,2,3", "1,2,3", true)]
    [TestCase("1,2,3", "2,1,3", false)]
    [TestCase("1,2,3", "1,2", false)]
    [TestCase("1,2,3", "", false)]
    [TestCase("", "", true)]
    public void AreEnumerablesEqual(string sArg1, string sArg2, bool wanted)
    {
      int[] a1 = StdConvert.ToInt32Array(sArg1);
      int[] a2 = StdConvert.ToInt32Array(sArg2);

      IEnumerable<int> en1 = a1;
      IEnumerable<int> en2 = a2;
      bool res1 = DataTools.AreEnumerablesEqual<int>(en1, en2);
      Assert.AreEqual(wanted, res1, "Typed #1");
      bool res2 = DataTools.AreEnumerablesEqual<int>(en2, en1);
      Assert.AreEqual(wanted, res2, "Typed #2");

      IEnumerable en3 = a1;
      IEnumerable en4 = a2;
      bool res3 = DataTools.AreEnumerablesEqual(en3, en4);
      Assert.AreEqual(wanted, res3, "Untyped #1");
      bool res4 = DataTools.AreEnumerablesEqual(en4, en3);
      Assert.AreEqual(wanted, res4, "Untyped #2");
    }

    [TestCase("1,11,21", "2,12,22", true)]
    [TestCase("1,11,21", "2,12,42", false)]
    [TestCase("1,2,3", "", false)]
    [TestCase("", "", true)]
    public void AreEnumerablesEqual_EqualityComparer(string sArg1, string sArg2, bool wanted)
    {
      int[] a1 = StdConvert.ToInt32Array(sArg1);
      int[] a2 = StdConvert.ToInt32Array(sArg2);

      IEnumerable<int> en1 = a1;
      IEnumerable<int> en2 = a2;
      bool res1 = DataTools.AreEnumerablesEqual<int>(en1, en2, new TestEqualityComparer());
      Assert.AreEqual(wanted, res1, "#1");
      bool res2 = DataTools.AreEnumerablesEqual<int>(en2, en1, new TestEqualityComparer());
      Assert.AreEqual(wanted, res2, "#2");
    }

    /// <summary>
    /// Тестовый компаратор, который считает, что числа одинаковые с точностью до 10
    /// </summary>
    private class TestEqualityComparer : IEqualityComparer<int>
    {
      public bool Equals(int x, int y)
      {
        return (x / 10) == (y / 10);
      }

      public int GetHashCode(int obj)
      {
        return obj / 10;
      }
    }


    [Test]
    public void AreEnumerablesEqual_StringComparison()
    {
      string[] a1 = new string[] { "AAA", "BBB" };
      string[] a2 = new string[] { "aaa", "BBB" };

      Assert.IsFalse(DataTools.AreEnumerablesEqual(a1, a2, StringComparison.Ordinal), "Ordinal #1");
      Assert.IsFalse(DataTools.AreEnumerablesEqual(a2, a1, StringComparison.Ordinal), "Ordinal #2");

      Assert.IsTrue(DataTools.AreEnumerablesEqual(a1, a2, StringComparison.OrdinalIgnoreCase), "OrdinalIgnoreCase #1");
      Assert.IsTrue(DataTools.AreEnumerablesEqual(a2, a1, StringComparison.OrdinalIgnoreCase), "OrdinalIgnoreCase #1");
    }

    #endregion

    #region AreDictionariesEqual()

    [TestCase("1,2,3", "10,20,30", "1,2,3", "10,20,30", true)]
    [TestCase("1,2,3", "10,20,30", "1,2,3", "10,20,31", false)]
    [TestCase("1,2,3", "10,20,30", "1,2,4", "10,20,30", false)]
    [TestCase("1,2,3", "10,20,30", "1,2", "10,20", false)]
    [TestCase("1,2,3", "10,20,30", "", "", false)]
    [TestCase("", "", "", "", true)]
    public void AreDictionariesEqual(string sKeys1, string sValues1, string sKeys2, string sValues2, bool wanted)
    {
      Dictionary<int, int> dict1 = CreateTestDictionary(sKeys1, sValues1);
      Dictionary<int, int> dict2 = CreateTestDictionary(sKeys2, sValues2);

      IDictionary<int, int> arg1 = dict1;
      IDictionary<int, int> arg2 = dict2;
      Assert.AreEqual(wanted, DataTools.AreDictionariesEqual<int, int>(arg1, arg2), "Typed #1");
      Assert.AreEqual(wanted, DataTools.AreDictionariesEqual<int, int>(arg2, arg1), "Typed #2");

      IDictionary arg3 = dict1;
      IDictionary arg4 = dict2;
      Assert.AreEqual(wanted, DataTools.AreDictionariesEqual(arg3, arg4), "Untyped #1");
      Assert.AreEqual(wanted, DataTools.AreDictionariesEqual(arg4, arg3), "Untyped #2");
    }

    private static Dictionary<int, int> CreateTestDictionary(string sKeys, string sValues)
    {
      int[] keys = StdConvert.ToInt32Array(sKeys);
      int[] values = StdConvert.ToInt32Array(sValues);
      if (values.Length != keys.Length)
        throw new ArgumentException();
      Dictionary<int, int> dict = new Dictionary<int, int>(keys.Length);
      for (int i = 0; i < keys.Length; i++)
        dict.Add(keys[i], values[i]);
      return dict;
    }

    #endregion

    #region Get/Set/CheckPrimaryKey()

    [Test]
    public void SetPrimaryKey_string()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Columns.Add("F2", typeof(int));
      tbl.Columns.Add("F3", typeof(int));

      DataTools.SetPrimaryKey(tbl, "F1");
      Assert.AreEqual(1, tbl.PrimaryKey.Length, "#1.Length");
      Assert.AreEqual("F1", tbl.PrimaryKey[0].ColumnName, "#1[0]");

      DataTools.SetPrimaryKey(tbl, "F3,F2");
      Assert.AreEqual(2, tbl.PrimaryKey.Length, "#2.Length");
      Assert.AreEqual("F3", tbl.PrimaryKey[0].ColumnName, "#2[0]");
      Assert.AreEqual("F2", tbl.PrimaryKey[1].ColumnName, "#2[1]");

      DataTools.SetPrimaryKey(tbl, "");
      Assert.AreEqual(0, tbl.PrimaryKey.Length, "#3.Length");

      Assert.Catch(delegate() { DataTools.SetPrimaryKey(tbl, "F4"); }, "Unknown column name");
    }

    [Test]
    public void SetPrimaryKey_array()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Columns.Add("F2", typeof(int));
      tbl.Columns.Add("F3", typeof(int));

      DataTools.SetPrimaryKey(tbl, new string[] { "F1" });
      Assert.AreEqual(1, tbl.PrimaryKey.Length, "#1.Length");
      Assert.AreEqual("F1", tbl.PrimaryKey[0].ColumnName, "#1[0]");

      DataTools.SetPrimaryKey(tbl, new string[] { "F3", "F2" });
      Assert.AreEqual(2, tbl.PrimaryKey.Length, "#2.Length");
      Assert.AreEqual("F3", tbl.PrimaryKey[0].ColumnName, "#2[0]");
      Assert.AreEqual("F2", tbl.PrimaryKey[1].ColumnName, "#2[1]");

      DataTools.SetPrimaryKey(tbl, DataTools.EmptyStrings);
      Assert.AreEqual(0, tbl.PrimaryKey.Length, "#3.Length");

      Assert.Catch(delegate() { DataTools.SetPrimaryKey(tbl, new string[] { "F4" }); }, "Unknown column name");
    }

    [Test]
    public void GetPrimaryKey()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Columns.Add("F2", typeof(int));
      tbl.Columns.Add("F3", typeof(int));

      tbl.PrimaryKey = new DataColumn[] { tbl.Columns[0] };
      Assert.AreEqual("F1", DataTools.GetPrimaryKey(tbl), "#1");

      tbl.PrimaryKey = new DataColumn[] { tbl.Columns[2], tbl.Columns[1] };
      Assert.AreEqual("F3,F2", DataTools.GetPrimaryKey(tbl), "#2");

      tbl.PrimaryKey = new DataColumn[] { };
      Assert.AreEqual("", DataTools.GetPrimaryKey(tbl), "#3");
    }

    [Test]
    public void CheckPrimaryKey()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Columns.Add("F2", typeof(int));
      tbl.Columns.Add("F3", typeof(int));

      DataTools.CheckPrimaryKey(tbl, "F1");
      Assert.AreEqual("F1", DataTools.GetPrimaryKey(tbl), "#1");

      DataTools.CheckPrimaryKey(tbl, "F1");
      Assert.AreEqual("F1", DataTools.GetPrimaryKey(tbl), "#2. Nothing changed");
    }

    #endregion

    #region GetPrimaryKeyValues()

    [Test]
    public void GetPrimaryKeyValues_row()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Columns.Add("F2", typeof(int));
      tbl.Columns.Add("F3", typeof(int));
      DataRow row = tbl.Rows.Add(1, 2, 3);

      DataTools.SetPrimaryKey(tbl, "F1");
      Assert.AreEqual(new object[] { 1 }, DataTools.GetPrimaryKeyValues(row), "#1");

      DataTools.SetPrimaryKey(tbl, "F3,F2");
      Assert.AreEqual(new object[] { 3, 2 }, DataTools.GetPrimaryKeyValues(row), "#2");

      // Поведение не определено при отсутствии первичного ключа
      //DataTools.SetPrimaryKey(tbl, "");
      //Assert.AreEqual(new object[] { }, DataTools.GetPrimaryKeyValues(row), "#3");
    }

    [Test]
    public void GetPrimaryKeyValues_row_another_table()
    {
      DataTable tbl1 = new DataTable();
      tbl1.Columns.Add("F1", typeof(int));
      tbl1.Columns.Add("F2", typeof(int));
      tbl1.Columns.Add("F3", typeof(int));

      DataTable tbl2 = tbl1.Clone();
      DataRow row2 = tbl2.Rows.Add(1, 2, 3);

      DataTools.SetPrimaryKey(tbl1, "F1");
      Assert.AreEqual(new object[] { 1 }, DataTools.GetPrimaryKeyValues(row2, tbl1));
    }

    [Test]
    public void GetPrimaryKeyValues_rows()
    {
      DataTable tbl = CreateTestTable123();

      DataRow[] rows = new DataRow[] { tbl.Rows[2], tbl.Rows[0] };

      DataTools.SetPrimaryKey(tbl, "F1");
      Assert.AreEqual(new object[,] { { 7 }, { 1 } }, DataTools.GetPrimaryKeyValues(tbl, rows), "#1");

      DataTools.SetPrimaryKey(tbl, "F3,F2");
      Assert.AreEqual(new object[,] { { 9, 8 }, { 3, 2 } }, DataTools.GetPrimaryKeyValues(tbl, rows), "#2");
    }

    #endregion

    #region GetPrimaryKeyRows()

    [Test]
    public void GetPrimaryKeyRows()
    {
      DataTable tbl = CreateTestTable123();

      DataTools.SetPrimaryKey(tbl, "F3,F2");

      object[,] arg1 = new object[,] { { 9, 8 }, { 3, 2 }, { 100, 100 } };
      DataRow[] res = DataTools.GetPrimaryKeyRows(tbl, arg1);
      Assert.AreEqual(3, res.Length, "Length");
      Assert.AreSame(tbl.Rows[2], res[0], "[0]");
      Assert.AreSame(tbl.Rows[0], res[1], "[1]");
      Assert.IsNull(res[2], "[2]");


      object[,] arg2 = new object[,] { { 9 }, { 3 } };
      Assert.Catch(delegate { DataTools.GetPrimaryKeyRows(tbl, arg2); }, "PK length missmatch");
    }

    /// <summary>
    /// Тестовая таблица из 3 строк
    /// </summary>
    /// <returns></returns>
    private static DataTable CreateTestTable123()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Columns.Add("F2", typeof(int));
      tbl.Columns.Add("F3", typeof(int));
      tbl.Rows.Add(1, 2, 3);
      tbl.Rows.Add(4, 5, 6);
      tbl.Rows.Add(7, 8, 9);
      return tbl;
    }

    #endregion

    #region FindOrAddPrimaryKeyRow()

    [TestCase(4, false)]
    [TestCase(10, true)]
    public void FindOrAddPrimaryKeyRow_1_3arg(int value, bool wanted)
    {
      DataTable tbl = CreateTestTable123();
      DataTools.SetPrimaryKey(tbl, "F1");

      DataRow resRow;
      bool res = DataTools.FindOrAddPrimaryKeyRow(tbl, value, out resRow);
      Assert.AreEqual(wanted, res, "result");
      Assert.AreEqual(wanted ? 4 : 3, tbl.Rows.Count, "Rows.Count");
      Assert.AreEqual(value, resRow["F1"], "row value");
    }

    [TestCase(4, false)]
    [TestCase(10, true)]
    public void FindOrAddPrimaryKeyRow_1_2arg(int value, bool wanted)
    {
      DataTable tbl = CreateTestTable123();
      DataTools.SetPrimaryKey(tbl, "F1");

      DataRow resRow = DataTools.FindOrAddPrimaryKeyRow(tbl, value);
      Assert.AreEqual(wanted ? 4 : 3, tbl.Rows.Count, "Rows.Count");
      Assert.AreEqual(value, resRow["F1"], "row value");
    }

    [TestCase(4, 5, false)]
    [TestCase(10, 11, true)]
    public void FindOrAddPrimaryKeyRow_multi_3arg(int value1, int value2, bool wanted)
    {
      DataTable tbl = CreateTestTable123();
      DataTools.SetPrimaryKey(tbl, "F1,F2");

      object[] keys = new object[] { value1, value2 };
      DataRow resRow;
      bool res = DataTools.FindOrAddPrimaryKeyRow(tbl, keys, out resRow);
      Assert.AreEqual(wanted, res, "result");
      Assert.AreEqual(wanted ? 4 : 3, tbl.Rows.Count, "Rows.Count");
      Assert.AreEqual(value1, resRow["F1"], "row value #1");
      Assert.AreEqual(value2, resRow["F2"], "row value #2");
    }

    [TestCase(4, 5, false)]
    [TestCase(10, 11, true)]
    public void FindOrAddPrimaryKeyRow_multi_2arg(int value1, int value2, bool wanted)
    {
      DataTable tbl = CreateTestTable123();
      DataTools.SetPrimaryKey(tbl, "F1,F2");

      object[] keys = new object[] { value1, value2 };
      DataRow resRow = DataTools.FindOrAddPrimaryKeyRow(tbl, keys);
      Assert.AreEqual(wanted ? 4 : 3, tbl.Rows.Count, "Rows.Count");
      Assert.AreEqual(value1, resRow["F1"], "row value #1");
      Assert.AreEqual(value2, resRow["F2"], "row value #2");
    }

    [Test]
    public void FindOrAddPrimaryKeyRow_exception()
    {
      DataTable tbl = CreateTestTable123();
      DataTools.SetPrimaryKey(tbl, "F1");

      object[] keys = new object[] { 1, 2 };
      Assert.Catch(delegate() { DataTools.FindOrAddPrimaryKeyRow(tbl, keys); }, "extra keys");

      keys = new object[] { };
      Assert.Catch(delegate() { DataTools.FindOrAddPrimaryKeyRow(tbl, keys); }, "less keys #1");

      DataTools.SetPrimaryKey(tbl, "F1,F2");
      Assert.Catch(delegate() { DataTools.FindOrAddPrimaryKeyRow(tbl, 1); }, "less keys #2");
    }

    #endregion

    #region AddRelation()

    [Test]
    public void AddRelation()
    {
      DataSet ds = new DataSet();
      DataTable tbl1 = ds.Tables.Add("T1");
      tbl1.Columns.Add("Id1", typeof(Int32));
      tbl1.Columns.Add("Text1", typeof(string));
      DataTools.SetPrimaryKey(tbl1, "Id1");

      DataTable tbl2 = ds.Tables.Add("T2");
      tbl2.Columns.Add("Id2", typeof(Int32));
      tbl2.Columns.Add("Ref2", typeof(Int32)); // ссылочное поле
      tbl2.Columns.Add("Text2", typeof(string));
      DataTools.SetPrimaryKey(tbl2, "Id2");

      DataTools.AddRelation(tbl1, tbl2, "Ref2", "Rel_1");

      Assert.AreEqual(1, ds.Relations.Count, "Relations.Count");
      DataRelation rel = ds.Relations[0];
      Assert.AreEqual("Rel_1", rel.RelationName, "RelationName");
      Assert.AreSame(tbl1, rel.ParentTable, "ParentTable");
      Assert.AreEqual(1, rel.ParentColumns.Length, "ParentColumns.Length");
      Assert.AreEqual("Id1", rel.ParentColumns[0].ColumnName, "ParentColumns[0].ColumnName");
      Assert.AreSame(tbl2, rel.ChildTable, "ChildTable");
      Assert.AreEqual(1, rel.ChildColumns.Length, "ChildColumns.Length");
      Assert.AreEqual("Ref2", rel.ChildColumns[0].ColumnName, "ChildColumns[0].ColumnName");
    }

    #endregion

    #region CloneTableForSelectedRows()

    [Test]
    public void CloneTableForSelectedRows()
    {
      DataTable tbl = CreateTestTable123();
      bool[] flags = new bool[] { true, false, true };

      DataTable res = DataTools.CloneTableForSelectedRows(tbl, flags);
      Assert.AreEqual(2, res.Rows.Count, "Rows.Count");
      Assert.AreEqual(tbl.Rows[0].ItemArray, res.Rows[0].ItemArray, "[0]");
      Assert.AreEqual(tbl.Rows[2].ItemArray, res.Rows[1].ItemArray, "[1]");
    }

    [Test]
    public void CloneOrSameTableForSelectedRows()
    {
      DataTable tbl = CreateTestTable123();

      bool[] flags1 = new bool[] { true, false, true };
      DataTable res1 = DataTools.CloneOrSameTableForSelectedRows(tbl, flags1);
      Assert.AreNotSame(res1, tbl, "Clone");

      bool[] flags2 = new bool[] { true, true, true };
      DataTable res2 = DataTools.CloneOrSameTableForSelectedRows(tbl, flags2);
      Assert.AreSame(res2, tbl, "Same");
    }

    [Test]
    public void CloneTableForSelectedIds()
    {
      DataTable tbl = CreateTestTable123();
      DataTools.SetPrimaryKey(tbl, "F1");
      int[] ids = new int[] { 1, 7 };

      DataTable res = DataTools.CloneTableForSelectedIds(tbl, ids);
      Assert.AreEqual(2, res.Rows.Count, "Rows.Count");
      Assert.AreEqual(tbl.Rows[0].ItemArray, res.Rows[0].ItemArray, "[0]");
      Assert.AreEqual(tbl.Rows[2].ItemArray, res.Rows[1].ItemArray, "[1]");
      Assert.AreEqual("F1", DataTools.GetPrimaryKey(res), "PrimaryKey");
    }


    [Test]
    public void CloneOrSameTableForSelectedIds()
    {
      DataTable tbl = CreateTestTable123();
      DataTools.SetPrimaryKey(tbl, "F1");

      int[] ids1 = new int[] { 1, 7 };
      DataTable res1 = DataTools.CloneOrSameTableForSelectedIds(tbl, ids1);
      Assert.AreNotSame(res1, tbl, "Clone");
      Assert.AreEqual("F1", DataTools.GetPrimaryKey(res1), "PrimaryKey");

      int[] ids2 = new int[] { 1, 4, 7 };
      DataTable res2 = DataTools.CloneOrSameTableForSelectedIds(tbl, ids2);
      Assert.AreSame(res2, tbl, "Same");
    }

    #endregion

    #region GetRowCount()

    [Test]
    public void GetRowCount()
    {
      DataSet ds = new DataSet();
      DataTable tbl1 = CreateTestTable123();
      tbl1.TableName = "T1";
      ds.Tables.Add(tbl1);
      DataTable tbl2 = CreateTestTable123();
      tbl2.TableName = "T2";
      ds.Tables.Add(tbl2);

      Assert.AreEqual(6, DataTools.GetRowCount(ds));
    }

    #endregion

    #region AddTableToDataSet()

    [Test]
    public void AddTableToDataSet_noname()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(string));

      DataSet ds1 = new DataSet();
      ds1.Tables.Add(CreateTestTable123());

      DataTools.AddTableToDataSet(ds1, tbl);
      Assert.AreSame(ds1, tbl.DataSet, "DataSet #1");
      Assert.AreEqual(2, ds1.Tables.Count, "Tables.Count #1");

      DataSet ds2 = new DataSet();
      ds2.Tables.Add(CreateTestTable123());

      DataTools.AddTableToDataSet(ds2, tbl);
      Assert.AreSame(ds2, tbl.DataSet, "DataSet #2");
      Assert.AreEqual(1, ds1.Tables.Count, "Tables.Count #2 - old");
      Assert.AreEqual(2, ds2.Tables.Count, "Tables.Count #2 - new");
    }

    [Test]
    public void AddTableToDataSet_named()
    {
      DataTable tbl = new DataTable("T1");
      tbl.Columns.Add("F1", typeof(string));

      DataSet ds = new DataSet();
      DataTable tbl01 = CreateTestTable123();
      tbl01.TableName = "T1";
      ds.Tables.Add(tbl01);
      DataTable tbl02 = CreateTestTable123();
      tbl02.TableName = "T2";
      ds.Tables.Add(tbl02);

      DataTools.AddTableToDataSet(ds, tbl);
      Assert.AreSame(ds, tbl.DataSet, "DataSet");
      Assert.AreEqual(2, ds.Tables.Count, "Tables.Count");

      Assert.IsNull(tbl01.DataSet, "Old table #1 has been removed");
      Assert.AreSame(ds, tbl02.DataSet, "Old table #2 stays in dataset");
    }

    #endregion

    #region GetIdsFromColumn()

    [Test]
    public void GetIdsFromColumn()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(Int32));
      tbl.Rows.Add(1);
      tbl.Rows.Add(0);
      tbl.Rows.Add(2);
      tbl.Rows.Add(3);
      tbl.Rows.Add(2);

      int[] wanted = new Int32[] { 1, 2, 3 };

      Int32[] res1 = DataTools.GetIdsFromColumn(tbl, "F1");
      Array.Sort<Int32>(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      Int32[] res2 = DataTools.GetIdsFromColumn(tbl.DefaultView, "F1");
      Array.Sort<Int32>(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      Int32[] res3 = DataTools.GetIdsFromColumn(DataTools.GetDataTableRows(tbl), "F1");
      Array.Sort<Int32>(res3);
      Assert.AreEqual(wanted, res3, "DataRow[]");

      DataRowView[] drvs = new DataRowView[] { tbl.DefaultView[0], tbl.DefaultView[1], tbl.DefaultView[2], tbl.DefaultView[3], tbl.DefaultView[4] };
      Int32[] res4 = DataTools.GetIdsFromColumn(drvs, "F1");
      Array.Sort<Int32>(res4);
      Assert.AreEqual(wanted, res4, "DataRowView[]");
    }

    #endregion

    #region GetIds()

    [Test]
    public void GetIds()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Id", typeof(Int32));
      tbl.Rows.Add(2);
      tbl.Rows.Add(3);
      tbl.Rows.Add(1);


      int[] wanted = new Int32[] { 2, 3, 1 };

      Int32[] res1 = DataTools.GetIds(tbl);
      Assert.AreEqual(wanted, res1, "DataTable");

      Int32[] res2 = DataTools.GetIds(tbl.DefaultView);
      Assert.AreEqual(wanted, res2, "DataView");

      Int32[] res3 = DataTools.GetIds(DataTools.GetDataTableRows(tbl));
      Assert.AreEqual(wanted, res3, "DataRow[]");

      DataRowView[] drvs = new DataRowView[] { tbl.DefaultView[0], tbl.DefaultView[1], tbl.DefaultView[2] };
      Int32[] res4 = DataTools.GetIds(drvs);
      Assert.AreEqual(wanted, res4, "DataRowView[]");
    }

    #endregion

    #region GetFirstId()/GetLastId()

    [Test]
    public void GetFirstId()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Id", typeof(Int32));
      tbl.Rows.Add(2);
      tbl.Rows.Add(3);
      tbl.Rows.Add(1);

      Assert.AreEqual(2, DataTools.GetFirstId(tbl), "DataTable");
      Assert.AreEqual(2, DataTools.GetFirstId(tbl.DefaultView), "DataView");
      Assert.AreEqual(2, DataTools.GetFirstId(DataTools.GetDataTableRows(tbl)), "DataRow[]");
    }

    [Test]
    public void GetLastId()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Id", typeof(Int32));
      tbl.Rows.Add(2);
      tbl.Rows.Add(3);
      tbl.Rows.Add(1);

      Assert.AreEqual(1, DataTools.GetLastId(tbl), "DataTable");
      Assert.AreEqual(1, DataTools.GetLastId(tbl.DefaultView), "DataView");
      Assert.AreEqual(1, DataTools.GetLastId(DataTools.GetDataTableRows(tbl)), "DataRow[]");
    }

    #endregion

    #region GetBlockedIds()

    [Test]
    public void GetBlockedIds()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Id", typeof(Int32));
      tbl.Rows.Add(2);
      tbl.Rows.Add(3);
      tbl.Rows.Add(1);
      tbl.Rows.Add(7);
      tbl.Rows.Add(6);
      tbl.Rows.Add(5);
      tbl.Rows.Add(4);

      int[][] wanted = new int[][] { new int[] { 2, 3, 1 }, new int[] { 7, 6, 5 }, new int[] { 4 } };

      Assert.AreEqual(wanted, DataTools.GetBlockedIds(tbl, 3), "DataTable");
      Assert.AreEqual(wanted, DataTools.GetBlockedIds(tbl.DefaultView, 3), "DataView");
      Assert.AreEqual(wanted, DataTools.GetBlockedIds(DataTools.GetDataTableRows(tbl), 3), "DataRow[]");
    }

    #endregion

    #region GetRowsFromIds()

    // Требуются отдельные тесты для наличия первичного ключа и его отсутствия.
    // Также важно количество элементов. Пустая таблица и таблица с одной строкой - особые случаи.
    // См. реализацию

    [TestCase(10, true, "1,5,10,0,11")]
    [TestCase(10, false, "1,5,10,0,11")]
    [TestCase(1, true, "1,0")]
    [TestCase(1, false, "1,0")]
    [TestCase(0, true, "1")]
    [TestCase(0, false, "1")]
    public void GetRowsFromIds(int nRows, bool usePK, string sIds)
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Id", typeof(Int32));
      for (int i = 1; i <= nRows; i++)
        tbl.Rows.Add(i.ToString());
      if (usePK)
        DataTools.SetPrimaryKey(tbl, "Id");

      Int32[] ids = StdConvert.ToInt32Array(sIds);

      DataRow[] rows = DataTools.GetRowsFromIds(tbl, ids);
      Assert.AreEqual(ids.Length, rows.Length, "Length");

      for (int i = 0; i < ids.Length; i++)
      {
        if (ids[i] > 0 && ids[i] <= nRows)
        {
          Assert.IsNotNull(rows[i], "HasRow (" + ids[i] + ")");
          Assert.AreEqual(ids[i], rows[i]["Id"], "Id=" + ids[i].ToString());
        }
        else
          Assert.IsNull(rows[i], "Null (" + ids[i] + ")");
      }

      // Проверяем, что первичный ключ таблицы не испортился
      Assert.AreEqual(usePK ? "Id" : "", DataTools.GetPrimaryKey(tbl), "Primary key");
    }

    #endregion

    #region TableFromIds()

    [Test]
    public void TableFromIds()
    {
      DataTable tbl = DataTools.TableFromIds(new Int32[]{3,1,2});

      Assert.AreEqual(1, tbl.Columns.Count, "Columns.Count");
      Assert.AreEqual("Id", tbl.Columns[0].ColumnName, "ColumnName");
      Assert.AreEqual(typeof(Int32), tbl.Columns[0].DataType, "DataType");

      Assert.AreEqual(3, tbl.Rows.Count, "Rows.Count");
      Assert.AreEqual(3, tbl.Rows[0]["Id"], "[0]");
      Assert.AreEqual(1, tbl.Rows[1]["Id"], "[1]");
      Assert.AreEqual(2, tbl.Rows[2]["Id"], "[2]");
    }

    #endregion

    #region GetDataTableRows()

    [Test]
    public void GetDataTableRows()
    {
      DataTable tbl = CreateTestTable123();

      DataRow[] rows1 = DataTools.GetDataTableRows(tbl);
      Assert.AreEqual(3, rows1.Length, "Length #1");
      Assert.AreSame(tbl.Rows[0], rows1[0], "[0] #1");
      Assert.AreSame(tbl.Rows[1], rows1[1], "[1] #1");
      Assert.AreSame(tbl.Rows[2], rows1[2], "[2] #1");

      DataRow[] rows2 = DataTools.GetDataTableRows(tbl, 1, 2);
      Assert.AreEqual(2, rows2.Length, "Length #2");
      Assert.AreSame(tbl.Rows[1], rows2[0], "[0] #2");
      Assert.AreSame(tbl.Rows[2], rows2[1], "[1] #2");
    }

    #endregion

    #region GetDataRowEnumerable()

    [Test]
    public void GetDataRowEnumerable()
    {
      DataTable tbl = CreateTestTable123();
      DoTestGetDataRowEnumerable(tbl, "1,4,7", "DataTable");
      DoTestGetDataRowEnumerable(tbl.DefaultView, "1,4,7", "DataView");
      DoTestGetDataRowEnumerable(DataTools.GetDataTableRows(tbl), "1,4,7", "DataRow[]");
      DoTestGetDataRowEnumerable(new DataRowView[]{tbl.DefaultView[1], tbl.DefaultView[2]}, "4,7", "DataRowView[]");

      // Пока не поддерживается
      //DataSet ds = new DataSet();
      //ds.Tables.Add(tbl);
      //DoTestGetDataRowEnumerable(ds, "1,4,7", "DataSet");

      DoTestGetDataRowEnumerable(null, "", "Null");
    }

    private static void DoTestGetDataRowEnumerable(object x, string wanted, string message)
    {
      List<int> lst=new List<int>();
      foreach(DataRow row in DataTools.GetDataRowEnumerable(x))
        lst.Add(DataTools.GetInt(row, "F1"));

      string sRes=StdConvert.ToString(lst.ToArray());

      Assert.AreEqual(wanted, sRes, message);
    }

    #endregion

    #region GetStringsFromColumn()

    [Test]
    public void GetStringsFromColumn()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(string));
      tbl.Rows.Add("AAA");
      tbl.Rows.Add("CCC");
      tbl.Rows.Add(); // DBNull
      tbl.Rows.Add(""); // пустая строка
      tbl.Rows.Add("BBB");
      tbl.Rows.Add("AAA"); // повтор


      string[] wanted = new string[] { "AAA", "BBB", "CCC" };

      Assert.AreEqual(wanted, DataTools.GetStringsFromColumn(tbl, "F1"), "DataTable");
      Assert.AreEqual(wanted, DataTools.GetStringsFromColumn(tbl.DefaultView, "F1"), "DataView");
      Assert.AreEqual(wanted, DataTools.GetStringsFromColumn(DataTools.GetDataTableRows(tbl), "F1"), "DataRow[]");
    }

    #endregion

    #region GetValuesFromColumn()

    [Test]
    public void GetValuesFromColumn()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Rows.Add(2);
      tbl.Rows.Add(1);
      tbl.Rows.Add(); // DBNull
      tbl.Rows.Add(2); // повтор

      int[] wanted = new int[] { 2, 1, 0, 2 };

      Assert.AreEqual(wanted, DataTools.GetValuesFromColumn<int>(tbl, "F1"), "DataTable");
      Assert.AreEqual(wanted, DataTools.GetValuesFromColumn<int>(tbl.DefaultView, "F1"), "DataView");
      Assert.AreEqual(wanted, DataTools.GetValuesFromColumn<int>(DataTools.GetDataTableRows(tbl), "F1"), "DataRow[]");
    }

    #endregion
  }
}
