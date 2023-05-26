using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.Remoting;

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DBxColumnsTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Collection()
    {
      List<string> src = new List<string>();
      src.Add("CCC");
      src.Add("AAA");
      src.Add("BBB");

      DBxColumns sut = new DBxColumns(src);
      Assert.AreEqual("CCC,AAA,BBB", sut.AsString, "AsString");
      Assert.AreEqual(src.ToArray(), sut.AsArray, "AsArray");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual(3, sut.Count, "Count");
    }

    [Test]
    public void Constructor_Collection_empty()
    {
      List<string> src = new List<string>();
      DBxColumns sut = new DBxColumns(src);
      Assert.AreEqual("", sut.AsString, "AsString");
      Assert.AreEqual(DataTools.EmptyStrings, sut.AsArray, "AsArray");
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual(0, sut.Count, "Count");
    }

    [Test]
    public void Constructor_Collection_null()
    {
      List<string> src = null;
      DBxColumns sut = new DBxColumns(src);
      Assert.AreEqual("", sut.AsString);
    }

    [Test]
    public void Constructor_string_simple()
    {
      DBxColumns sut = new DBxColumns("AAA");
      Assert.AreEqual("AAA", sut.AsString);
    }

    [Test]
    public void Constructor_string_commas()
    {
      DBxColumns sut = new DBxColumns("CCC,AAA,BBB");
      Assert.AreEqual("CCC,AAA,BBB", sut.AsString);
    }

    [Test]
    public void Constructor_string_empty()
    {
      DBxColumns sut = new DBxColumns("");
      Assert.AreEqual(0, sut.Count);
    }

    [Test]
    public void Constructor_string_null()
    {
      DBxColumns sut = new DBxColumns((string)null);
      Assert.AreEqual(0, sut.Count);
    }

    [Test]
    public void Constructor_stringArray()
    {
      string[] src = new string[] { "AAA", "BBB", "CCC" };
      DBxColumns sut = new DBxColumns(src);
      Assert.AreEqual("AAA,BBB,CCC", sut.AsString, "AsString");
      Assert.AreEqual(src, sut.AsArray, "AsArray");
      Assert.AreNotSame(src, sut.AsArray, "AsArray is not same");
    }

    [TestCase("AAA,AAA")]
    [TestCase("AAA,BBB,AAA")]
    [TestCase("AAA,BBB,CCC,AAA")]
    [TestCase("AAA,BBB,AAA,CCC")]
    public void Constructor_repeats(string aNames)
    {
      string[] src2 = aNames.Split(',');
      ICollection<string> src3 = new List<string>(src2);

      Assert.Catch<ArgumentException>(delegate() { new DBxColumns(aNames); }, "string");
      Assert.Catch<ArgumentException>(delegate() { new DBxColumns(src2); }, "string[]");
      Assert.Catch<ArgumentException>(delegate() { new DBxColumns(src3); }, "ICollection<string>");
    }

    #endregion

    #region FromNames()

    [Test]
    public void FromNames_string()
    {
      DBxColumns sut1 = DBxColumns.FromNames("AAA,BBB,CCC");
      Assert.AreEqual("AAA,BBB,CCC", sut1.AsString, "#1");

      DBxColumns sut2 = DBxColumns.FromNames("");
      Assert.IsNull(sut2, "#2");
    }

    [Test]
    public void FromNames_stringArray_1()
    {
      DBxColumns sut1 = DBxColumns.FromNames("AAA,BBB,CCC".Split(','));
      Assert.AreEqual("AAA,BBB,CCC", sut1.AsString, "#1");

      DBxColumns sut2 = DBxColumns.FromNames(DataTools.EmptyStrings);
      Assert.IsNull(sut2, "#2");
    }

    [Test]
    public void FromNames_stringArray_2_copy()
    {
      string[] src1 = "AAA,BBB,CCC".Split(',');
      DBxColumns sut1 = DBxColumns.FromNames(src1, true);
      Assert.AreEqual(src1, sut1.AsArray, "Equal #1");
      Assert.AreNotSame(src1, sut1.AsArray, "Same #1");

      DBxColumns sut2 = DBxColumns.FromNames(DataTools.EmptyStrings, true);
      Assert.IsNull(sut2, "#2");
    }

    [Test]
    public void FromNames_stringArray_2_dontcopy()
    {
      string[] src1 = "AAA,BBB,CCC".Split(',');
      DBxColumns sut1 = DBxColumns.FromNames(src1, false);
      Assert.AreEqual(src1, sut1.AsArray, "Equal #1");
      Assert.AreSame(src1, sut1.AsArray, "Same #1");

      DBxColumns sut2 = DBxColumns.FromNames(DataTools.EmptyStrings, false);
      Assert.IsNull(sut2, "#2");
    }

    #endregion

    #region FromColumns()

    [Test]
    public void FromColumns_1_normal()
    {
      DataTable src = new DataTable();
      src.Columns.Add("F1", typeof(string));
      src.Columns.Add("F2", typeof(int));
      src.Columns.Add("F3", typeof(bool));

      DBxColumns sut = DBxColumns.FromColumns(src.Columns);
      Assert.AreEqual("F1,F2,F3", sut.AsString);
    }

    [Test]
    public void FromColumns_1_emptyTable()
    {
      DataTable src = new DataTable();
      DBxColumns sut = DBxColumns.FromColumns(src.Columns);
      Assert.IsNull(sut);
    }

    [Test]
    public void FromColumns_1_null()
    {
      DBxColumns sut = DBxColumns.FromColumns((DataColumnCollection)null);
      Assert.IsNull(sut);
    }

    [TestCase("AA1,BA2,AA3,AA4", "A", false, "AA1,AA3,AA4")]
    [TestCase("AA1,BA2,AA3,AA4", "A", true, "A1,A3,A4")]
    [TestCase("AA1,BA2,AA3,AA4", "B", false, "BA2")]
    [TestCase("AA1,BA2,AA3,AA4", "B", true, "A2")]
    [TestCase("AA1,BA2,AA3,AA4", "", false, "AA1,BA2,AA3,AA4")]
    [TestCase("AA1,BA2,AA3,AA4", "", true, "AA1,BA2,AA3,AA4")]
    public void FromColumns_3_normal(string srcColumns, string prefix, bool stripPrefix, string wantedNames)
    {
      DataTable src = new DataTable();
      foreach (string srcName in srcColumns.Split(','))
        src.Columns.Add(srcName, typeof(string));

      DBxColumns sut = DBxColumns.FromColumns(src.Columns, prefix, stripPrefix);
      Assert.AreEqual(wantedNames, sut.AsString);
    }

    [Test]
    public void FromColumns_3_emptyTable()
    {
      DataTable src = new DataTable();
      DBxColumns sut = DBxColumns.FromColumns(src.Columns, "A", false);
      Assert.IsNull(sut);
    }

    [Test]
    public void FromColumns_3_null()
    {
      DBxColumns sut = DBxColumns.FromColumns((DataColumnCollection)null, "A", false);
      Assert.IsNull(sut);
    }

    #endregion

    #region FromDataViewSort()

    [TestCase("F1", "F1")]
    [TestCase("F1,F2", "F1,F2")]
    [TestCase("F1 DESC,F2 ASC", "F1,F2")]
    [TestCase("[F1],[F2]", "F1,F2")]
    [TestCase("[F1] DESC", "F1")]
    public void FromDataViewSort_normal(string sort, string wantedNames)
    {
      DBxColumns sut = DBxColumns.FromDataViewSort(sort);
      Assert.AreEqual(wantedNames, sut.AsString);
    }

    [Test]
    public void FromDataViewSort_emptyString()
    {
      DBxColumns sut = DBxColumns.FromDataViewSort("");
      Assert.IsNull(sut);
    }

    #endregion

    #region FromDataReader()

    [Test]
    public void FromDataReader()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(string));
      tbl.Columns.Add("F2", typeof(int));
      using (DataTableReader rdr = new DataTableReader(tbl))
      {
        DBxColumns sut = DBxColumns.FromDataReader(rdr);
        Assert.AreEqual("F1,F2", sut.AsString);
      }
    }

    #endregion

    #region Item

    [Test]
    public void Item()
    {
      DBxColumns sut = new DBxColumns("F1,F2,F3");
      Assert.AreEqual("F1", sut[0], "[0]");
      Assert.AreEqual("F2", sut[1], "[1]");
      Assert.AreEqual("F3", sut[2], "[2]");
    }

    #endregion

    #region ContainsDots()

    [TestCase("F1,F2", false)]
    [TestCase("F1.F2", true)]
    [TestCase("F1,F2.F3", true)]
    [TestCase("", false)]
    public void ContainsDots(string names, bool wantedRes)
    {
      DBxColumns sut = new DBxColumns(names);
      Assert.AreEqual(wantedRes, sut.ContainsDots);
    }

    #endregion

    #region Contains(), ContainsAny(), ContainsStartedWith()

    [TestCase("F1,F2", "F1", true)]
    [TestCase("F1,F2", "f1", false)]
    [TestCase("F1,F2", "F1,F2", true)]
    [TestCase("F1,F2", "F1,F3", false)]
    [TestCase("F1", "F3", false)]
    [TestCase("", "F1", false)]
    [TestCase("F1", "", true)]
    [TestCase("F1,F2,F3,F4,F5", "F1,F6", false)] // использование внутреннего индексатора
    [TestCase("F1,F2,F3,F4,F5", "f1", false)]
    public void Contains(string names, string testNames, bool wantedRes)
    {
      DBxColumns sut = new DBxColumns(names);
      Assert.AreEqual(wantedRes, sut.Contains(testNames), "string");
      Assert.AreEqual(wantedRes, sut.Contains(new DBxColumns(testNames)), "DBxColumns");
    }

    [TestCase("F1,F2", "F1", true)]
    [TestCase("F1,F2", "f1", false)]
    [TestCase("F1,F2", "F1,F2", true)]
    [TestCase("F1,F2", "F1,F3", true)]
    [TestCase("F1", "F3", false)]
    [TestCase("", "F1", false)]
    [TestCase("F1", "", false)]
    [TestCase("F1,F2,F3,F4,F5", "F1,F6", true)] // использование внутреннего индексатора
    [TestCase("F1,F2,F3,F4,F5", "f1", false)]
    public void ContainsAny(string names, string testNames, bool wantedRes)
    {
      DBxColumns sut = new DBxColumns(names);
      Assert.AreEqual(wantedRes, sut.ContainsAny(testNames), "string");
      Assert.AreEqual(wantedRes, sut.ContainsAny(new DBxColumns(testNames)), "DBxColumns");
    }

    [TestCase("F1,F2", "F", true)]
    [TestCase("F1,F2", "f", false)]
    [TestCase("F1,F2", "", true)]
    [TestCase("", "F", false)]
    [TestCase("", "", false)]
    public void ContainsStartedWith(string names, string testPrefix, bool wantedRes)
    {
      DBxColumns sut = new DBxColumns(names);
      Assert.AreEqual(wantedRes, sut.ContainsStartedWith(testPrefix));
    }

    #endregion

    #region TableContains() / TableContainsAny()

    [TestCase("F1,F2", "f1", true)]
    [TestCase("F1,F2", "F1,F2", true)]
    [TestCase("F1,F2", "F1,F3", false)]
    [TestCase("F1,F2", "F3,F4", false)]
    [TestCase("F1,F2", "", true)]
    [TestCase("", "F1", false)]
    [TestCase("", "", true)]
    public void TableContains(string tableColumnNames, string testNames, bool wantedRes)
    {
      DataTable table = new DataTable();
      foreach (string colName in tableColumnNames.Split(','))
        table.Columns.Add(colName, typeof(string));

      Assert.AreEqual(wantedRes, DBxColumns.TableContains(table, new DBxColumns(testNames)));
    }

    [TestCase("F1,F2", "f1", true)]
    [TestCase("F1,F2", "F1,F2", true)]
    [TestCase("F1,F2", "F1,F3", true)]
    [TestCase("F1,F2", "F3,F4", false)]
    [TestCase("F1,F2", "", false)]
    [TestCase("", "F1", false)]
    [TestCase("", "", false)]
    public void TableContainsAny(string tableColumnNames, string testNames, bool wantedRes)
    {
      DataTable table = new DataTable();
      foreach (string colName in tableColumnNames.Split(','))
        table.Columns.Add(colName, typeof(string));

      Assert.AreEqual(wantedRes, DBxColumns.TableContainsAny(table, new DBxColumns(testNames)));
    }

    #endregion

    #region IndexOf()

    [TestCase("F1,F2,F3", "F2", 1)]
    [TestCase("F1,F2,F3", "f2", -1)]
    [TestCase("F1", "", -1)]
    [TestCase("", "F1", -1)]
    [TestCase("", "", -1)]
    public void IndexOf(string names, string testName, int wantedRes)
    {
      DBxColumns sut = new DBxColumns(names);
      Assert.AreEqual(wantedRes, sut.IndexOf(testName));
    }

    #endregion

    #region HasMoreThan()

    [TestCase("F1,F2", "F1", true)]
    [TestCase("F1,F2", "F1,F2", false)]
    [TestCase("F1,F2", "F1,F2,F3", false)]
    [TestCase("F1", "F2", true)]
    [TestCase("F1", "", true)]
    [TestCase("", "F1", false)]
    [TestCase("", "", false)]
    public void HasMoreThan(string thisNames, string otherNames, bool wantedRes)
    {
      HasMoreThan_test1(new DBxColumns(thisNames), otherNames, wantedRes);
      if (String.IsNullOrEmpty(thisNames))
        HasMoreThan_test1(null, otherNames, wantedRes);
    }

    private static void HasMoreThan_test1(DBxColumns thisNames, string otherNames, bool wantedRes)
    {
      HasMoreThan_test2(thisNames, new DBxColumns(otherNames), wantedRes);
    }

    private static void HasMoreThan_test2(DBxColumns thisNames, DBxColumns otherNames, bool wantedRes)
    {
      Assert.AreEqual(wantedRes, DBxColumns.HasMoreThan(thisNames, otherNames), "static");
      if (!Object.ReferenceEquals(thisNames, null))
        Assert.AreEqual(wantedRes, thisNames.HasMoreThan(otherNames), "non-static");
    }

    #endregion

    #region CloneWithPrefix/Suffix()

    [TestCase("F1,F2,F3", "ABC", "ABCF1,ABCF2,ABCF3")]
    [TestCase("F1,F2,F3", "", "F1,F2,F3")]
    [TestCase("", "ABC", "")]
    [TestCase("", "", "")]
    public void CloneWithPrefix(string names, string prefix, string wantedNames)
    {
      DBxColumns sut = new DBxColumns(names);
      DBxColumns res = sut.CloneWithPrefix(prefix);
      Assert.AreEqual(wantedNames, res.AsString);
    }

    [TestCase("F1,F2,F3", "ABC", "F1ABC,F2ABC,F3ABC")]
    [TestCase("F1,F2,F3", "", "F1,F2,F3")]
    [TestCase("", "ABC", "")]
    [TestCase("", "", "")]
    public void CloneWithSuffix(string names, string prefix, string wantedNames)
    {
      DBxColumns sut = new DBxColumns(names);
      DBxColumns res = sut.CloneWithSuffix(prefix);
      Assert.AreEqual(wantedNames, res.AsString);
    }

    #endregion

    #region ExtractColumnValues()

    [Test]
    public void ExtractColumnValues_normal()
    {
      DBxColumns sut = new DBxColumns("F1,F2,F3");

      DBxColumns columnNames = new DBxColumns("F2,F4,F1,F3");
      object[] values = new object[] { "222", "444", "111", "333" };
      object[] res = sut.ExtractColumnValues(columnNames, values);
      Assert.AreEqual(new object[] { "111", "222", "333" }, res);
    }

    [Test]
    public void ExtractColumnValues_missing()
    {
      DBxColumns sut = new DBxColumns("F1,F2,F3");

      DBxColumns columnNames = new DBxColumns("F2,F4,F5,F3");
      object[] values = new object[] { "222", "444", "555", "333" };
      object[] res = sut.ExtractColumnValues(columnNames, values);
      Assert.IsNull(res);
    }

    #endregion

    #region AddColumns()

    [Test]
    public void AddColumns_normal()
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string));
      table.Columns.Add("F2", typeof(int));

      DBxColumns sut = new DBxColumns("F3,F4,F5");
      sut.AddColumns(table.Columns, typeof(bool));

      Assert.AreEqual(5, table.Columns.Count, "Count");
      Assert.AreEqual("F1", table.Columns[0].ColumnName, "ColumnName[0]");
      Assert.AreEqual(typeof(string), table.Columns[0].DataType, "DataType[0]");
      Assert.AreEqual("F2", table.Columns[1].ColumnName, "ColumnName[1]");
      Assert.AreEqual(typeof(int), table.Columns[1].DataType, "DataType[1]");
      Assert.AreEqual("F3", table.Columns[2].ColumnName, "ColumnName[2]");
      Assert.AreEqual(typeof(bool), table.Columns[2].DataType, "DataType[2]");
      Assert.AreEqual("F4", table.Columns[3].ColumnName, "ColumnName[3]");
      Assert.AreEqual(typeof(bool), table.Columns[3].DataType, "DataType[3]");
      Assert.AreEqual("F5", table.Columns[4].ColumnName, "ColumnName[4]");
      Assert.AreEqual(typeof(bool), table.Columns[4].DataType, "DataType[4]");
    }

    [Test]
    public void AddColumns_empty()
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string));
      table.Columns.Add("F2", typeof(int));

      DBxColumns.Empty.AddColumns(table.Columns, typeof(bool));
      Assert.AreEqual(2, table.Columns.Count);
    }

    #endregion

    #region AddContainedColumns()

    [TestCase(false)]
    [TestCase(true)]
    public void AddContainedColumns_columnNames(bool useArray)
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string));
      table.Columns.Add("F2", typeof(int));

      DBxColumns sut = new DBxColumns("F3,F4,F5");
      if (useArray)
        sut.AddContainedColumns(table.Columns, "F4,F5,F6".Split(','), typeof(bool));
      else
        sut.AddContainedColumns(table.Columns, "F4,F5,F6", typeof(bool));

      Assert.AreEqual(4, table.Columns.Count, "Count");
      Assert.AreEqual("F1", table.Columns[0].ColumnName, "ColumnName[0]");
      Assert.AreEqual(typeof(string), table.Columns[0].DataType, "DataType[0]");
      Assert.AreEqual("F2", table.Columns[1].ColumnName, "ColumnName[1]");
      Assert.AreEqual(typeof(int), table.Columns[1].DataType, "DataType[1]");
      Assert.AreEqual("F4", table.Columns[2].ColumnName, "ColumnName[2]");
      Assert.AreEqual(typeof(bool), table.Columns[2].DataType, "DataType[2]");
      Assert.AreEqual("F5", table.Columns[3].ColumnName, "ColumnName[3]");
      Assert.AreEqual(typeof(bool), table.Columns[3].DataType, "DataType[3]");
    }

    [Test]
    public void AddContainedColumns_DuplicateNameException()
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string));
      table.Columns.Add("F2", typeof(int));

      DBxColumns sut = new DBxColumns("F3,F1");
      Assert.Throws<DuplicateNameException>(delegate() { sut.AddContainedColumns(table.Columns, "F3,F1", typeof(bool)); });
    }

    [Test]
    public void AddContainedColumns_sourceTable()
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string));
      table.Columns.Add("F2", typeof(int));

      DataTable table2 = new DataTable();
      table2.Columns.Add("F4", typeof(bool));
      table2.Columns.Add("F3", typeof(DateTime));
      table2.Columns.Add("F6", typeof(double));

      DBxColumns sut = new DBxColumns("F3,F4");
      sut.AddContainedColumns(table.Columns, table2.Columns);
      Assert.AreEqual(4, table.Columns.Count, "Count #1");
      Assert.AreEqual(3, table2.Columns.Count, "Count #2");
      Assert.AreEqual("F1", table.Columns[0].ColumnName, "ColumnName[0]");
      Assert.AreEqual(typeof(string), table.Columns[0].DataType, "DataType[0]");
      Assert.AreEqual("F2", table.Columns[1].ColumnName, "ColumnName[1]");
      Assert.AreEqual(typeof(int), table.Columns[1].DataType, "DataType[1]");
      Assert.AreEqual("F3", table.Columns[2].ColumnName, "ColumnName[2]");
      Assert.AreEqual(typeof(DateTime), table.Columns[2].DataType, "DataType[2]");
      Assert.AreEqual("F4", table.Columns[3].ColumnName, "ColumnName[3]");
      Assert.AreEqual(typeof(bool), table.Columns[3].DataType, "DataType[3]");
    }

    [Test]
    public void AddContainedColumns_exception_no_column_in_source_table()
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string));
      table.Columns.Add("F2", typeof(int));

      DataTable table2 = new DataTable();
      table2.Columns.Add("F4", typeof(bool));
      table2.Columns.Add("F3", typeof(DateTime));
      table2.Columns.Add("F6", typeof(double));

      DBxColumns sut = new DBxColumns("F3,F5");
      Assert.Catch(delegate() { sut.AddContainedColumns(table.Columns, table2.Columns); });
    }

    #endregion

    #region CreateSubTable()

    [Test]
    public void CreateSubTable()
    {
      DataTable table = CreateTestDataTable();
      DBxColumns sut = new DBxColumns("F3,F1");
      DataTable res = sut.CreateSubTable(table);

      Assert.AreEqual(new string[] { "F3", "F1" }, DataTools.GetColumnNames(res), "ColumnNames");
      Assert.AreEqual(new bool[] { true, false, true, false }, DataTools.GetValuesFromColumn<bool>(res, "F3"), "F3");
      Assert.AreEqual(new string[] { "AAA", "BBB", "CCC", "DDD" }, DataTools.GetValuesFromColumn<string>(res, "F1"), "F1");
    }

    private DataTable CreateTestDataTable()
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string));
      table.Columns.Add("F2", typeof(int));
      table.Columns.Add("F3", typeof(bool));

      table.Rows.Add("AAA", 1, true);
      table.Rows.Add("BBB", 2, false);
      table.Rows.Add("CCC", 3, true);
      table.Rows.Add("DDD", 4, false);

      return table;
    }

    [Test]
    public void CreateSubTable_exception()
    {
      DataTable table = CreateTestDataTable();
      DBxColumns sut = new DBxColumns("F4,F1");
      Assert.Catch(delegate() { sut.CreateSubTable(table); });
    }

    #endregion

    #region Get/SetRowValues()

    [Test]
    public void GetRowValues_normal()
    {
      DataTable table = CreateTestDataTable();
      DBxColumns sut = new DBxColumns("F3,F1");
      object[] res1 = sut.GetRowValues(table.Rows[1]);
      Assert.AreEqual(new object[] { false, "BBB" }, res1, "#1");

      object[] res2 = sut.GetRowValues(table.Rows[1], false);
      Assert.AreEqual(res1, res2, "#2");

      object[] res3 = sut.GetRowValues(table.Rows[1], true);
      Assert.AreEqual(res1, res3, "#3");
    }

    [Test]
    public void GetRowValues_nocolumn()
    {
      DataTable table = CreateTestDataTable();
      DBxColumns sut = new DBxColumns("F4,F1");
      object[] res1 = sut.GetRowValues(table.Rows[1], false);
      Assert.AreEqual(new object[] { null, "BBB" }, res1, "#1");
      Assert.Catch(delegate() { sut.GetRowValues(table.Rows[1], true); });
    }

    [Test]
    public void SetRowValues_normal()
    {
      DataTable table = CreateTestDataTable();
      DBxColumns sut = new DBxColumns("F3,F1");
      sut.SetRowValues(table.Rows[1], new object[] { true, "XXX" });
      Assert.AreEqual(new object[] { "XXX", 2, true }, table.Rows[1].ItemArray);
    }

    //[Test]
    //public void SetRowValues_nocolumn()
    //{
    //  DataTable table = CreateTestDataTable();
    //  DBxColumns sut = new DBxColumns("F4,F1");
    //  Assert.Catch(delegate() { sut.SetRowValues(table.Rows[1], new object[] { true, "XXX" }); });
    //}

    #endregion

    #region Операторы

    [TestCase("F1", "F2", "F1,F2")]
    [TestCase("F1,F2", "F3", "F1,F2,F3")]
    [TestCase("F1", "F2,F3", "F1,F2,F3")]
    [TestCase("F1,F2,F3", "F2,F3,F4", "F1,F2,F3,F4")]
    [TestCase("F1,F2", "", "F1,F2")]
    [TestCase("", "F1,F2", "F1,F2")]
    [TestCase("", "", "")]
    public void Operator_Add(string names1, string names2, string wantedRes)
    {
      DBxColumns arg1 = DBxColumns.FromNames(names1);
      DBxColumns arg2 = DBxColumns.FromNames(names2);

      DBxColumns res1 = arg1 + arg2;
      Assert.AreEqual(wantedRes, res1.AsString, "DBxColumns");

      DBxColumns res2 = arg1 + names2;
      Assert.AreEqual(wantedRes, res2.AsString, "string");

      string[] aNames2 = String.IsNullOrEmpty(names2) ? DataTools.EmptyStrings : names2.Split(',');
      DBxColumns res3 = arg1 + aNames2;
      Assert.AreEqual(wantedRes, res3.AsString, "string[]");
    }

    [TestCase("F1", "F2", "F1")]
    [TestCase("F1", "F1", "")]
    [TestCase("F1,F2,F3", "F3,F1,F4", "F2")]
    [TestCase("F1,F2", "F3", "F1,F2")]
    [TestCase("F1,F2", "", "F1,F2")]
    [TestCase("", "F1", "")]
    [TestCase("", "", "")]
    public void Operator_Substract(string names1, string names2, string wantedRes)
    {
      DBxColumns arg1 = DBxColumns.FromNames(names1);
      DBxColumns arg2 = DBxColumns.FromNames(names2);

      DBxColumns res1 = arg1 - arg2;
      Assert.AreEqual(wantedRes, res1.AsString, "DBxColumns");

      DBxColumns res2 = arg1 - names2;
      Assert.AreEqual(wantedRes, res2.AsString, "string");

      string[] aNames2 = String.IsNullOrEmpty(names2) ? DataTools.EmptyStrings : names2.Split(',');
      DBxColumns res3 = arg1 - aNames2;
      Assert.AreEqual(wantedRes, res3.AsString, "string[]");
    }

    [TestCase("F1", "F1", "F1")]
    [TestCase("F1", "F2", "")]
    [TestCase("F1,F2,F3", "F3,F2,F4,F5", "F2,F3")]
    [TestCase("F1", "", "")]
    [TestCase("", "F1", "")]
    [TestCase("", "", "")]
    public void Operator_And(string names1, string names2, string wantedRes)
    {
      DBxColumns arg1 = DBxColumns.FromNames(names1);
      DBxColumns arg2 = DBxColumns.FromNames(names2);

      DBxColumns res1 = arg1 & arg2;
      Assert.AreEqual(wantedRes, res1.AsString, "DBxColumns");

      DBxColumns res2 = arg1 & names2;
      Assert.AreEqual(wantedRes, res2.AsString, "string");

      string[] aNames2 = String.IsNullOrEmpty(names2) ? DataTools.EmptyStrings : names2.Split(',');
      DBxColumns res3 = arg1 & aNames2;
      Assert.AreEqual(wantedRes, res3.AsString, "string[]");
    }

    #endregion

    #region Перечислитель

    [Test]
    public void Enumerator()
    {
      DBxColumns sut = new DBxColumns("F1,F2,F3");
      List<string> lst = new List<string>();
      foreach (string name in sut)
        lst.Add(name);

      Assert.AreEqual(new string[] { "F1", "F2", "F3" }, lst.ToArray());
    }

    #endregion

    #region Empty, Id

    [Test]
    public void Empty()
    {
      Assert.AreEqual(0, DBxColumns.Empty.Count, "Count");
      Assert.IsTrue(DBxColumns.Empty.IsEmpty, "IsEmpty");
      Assert.AreEqual("", DBxColumns.Empty.AsString, "AsString");
      Assert.AreEqual(DataTools.EmptyStrings, DBxColumns.Empty.AsArray, "AsArray");

      int cnt = 0;
      foreach (string name in DBxColumns.Empty)
        cnt++;
      Assert.AreEqual(0, cnt, "GetEnumerator()");
    }

    [Test]
    public void Id()
    {
      Assert.AreEqual(1, DBxColumns.Id.Count, "Count");
      Assert.IsFalse(DBxColumns.Id.IsEmpty, "IsEmpty");
      Assert.AreEqual("Id", DBxColumns.Id.AsString, "AsString");
    }

    #endregion

    #region Сериализация

    [TestCase(false)]
    [TestCase(true)]
    public void Serialization(bool isLong)
    {
      string colNames = isLong ? "F1,F2,F3,F4,F5" : "F1,F2";
      DBxColumns sut = new DBxColumns(colNames);
      byte[] b = SerializationTools.SerializeBinary(sut);
      DBxColumns res = (DBxColumns)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(colNames, res.AsString, "AsString");
      for (int i = 0; i < sut.Count; i++)
      {
        int resPos = res.IndexOf(sut[i]);
        Assert.AreEqual(resPos, i, "IndexOf(" + sut[i] + ")");
      }
      Assert.AreEqual(-1, res.IndexOf("F666"), "IndexOf(F666)");
    }

    #endregion
  }
}
