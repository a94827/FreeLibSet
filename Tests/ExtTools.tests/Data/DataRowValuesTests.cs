using FreeLibSet.Core;
using FreeLibSet.Data;
using FreeLibSet.Tests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Reflection;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class DataRowValuesTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DataRowValues sut = new DataRowValues();
      Assert.IsNull(sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.AreEqual(new string[0], sut.GetNames(), "GetNames()");
      Assert.IsFalse(sut.TableIsFixed, "TableIsFixed");
    }

    #endregion

    #region Свойства

    [Test]
    public void Table()
    {
      DataRowValues sut = new DataRowValues();

      DataTable tbl = TestTable.Create();
      sut.Table = tbl;

      Assert.AreSame(tbl, sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
    }


    [Test]
    public void CurrentRow()
    {

      DataTable tbl1 = TestTable.Create();
      DataTable tbl2 = TestTable.Create();
      tbl2.Rows[2].Delete();

      DataRowValues sut = new DataRowValues();

      sut.CurrentRow = tbl1.Rows[0];
      Assert.AreSame(sut.CurrentRow, tbl1.Rows[0], "CurrentRow #1");
      Assert.AreSame(tbl1, sut.Table, "Table #1");
      Assert.AreEqual(DataRowVersion.Default, sut.RowVersion, "RowVersion #1");

      sut.CurrentRow = tbl2.Rows[0];
      Assert.AreSame(sut.CurrentRow, tbl2.Rows[0], "CurrentRow #2");
      Assert.AreSame(tbl2, sut.Table, "Table #2");
      Assert.AreEqual(DataRowVersion.Default, sut.RowVersion, "RowVersion #2");

      sut.CurrentRow = tbl2.Rows[2];
      Assert.AreSame(sut.CurrentRow, tbl2.Rows[2], "CurrentRow #3");
      Assert.AreSame(tbl2, sut.Table, "Table #2");
      Assert.AreEqual(DataRowVersion.Original, sut.RowVersion, "RowVersion #3");

      sut.CurrentRow = null;
      Assert.IsNull(sut.CurrentRow, "CurrentRow #4");
      Assert.AreSame(tbl2, sut.Table, "Table #4");
    }

    [Test]
    public void SetCurrentRow()
    {
      DataTable tbl1 = TestTable.Create();
      tbl1.Rows[1]["FInt32"] = 1234;
      DataTable tbl2 = TestTable.Create();

      DataRowValues sut = new DataRowValues();

      sut.SetCurrentRow(tbl1.Rows[1], DataRowVersion.Default);
      Assert.AreSame(sut.CurrentRow, tbl1.Rows[1], "CurrentRow #1");
      Assert.AreSame(tbl1, sut.Table, "Table #1");
      Assert.AreEqual(DataRowVersion.Default, sut.RowVersion, "RowVersion #1");
      Assert.AreEqual(1234, sut.GetValue("FInt32"), "FInt32 current value #1");

      sut.SetCurrentRow(tbl1.Rows[1], DataRowVersion.Original);
      Assert.AreSame(sut.CurrentRow, tbl1.Rows[1], "CurrentRow #2");
      Assert.AreSame(tbl1, sut.Table, "Table #2");
      Assert.AreEqual(DataRowVersion.Original, sut.RowVersion, "RowVersion #2");
      Assert.AreEqual(TestTable.Row1.VInt32, sut.GetValue("FInt32"), "FInt32 default value #2");

      sut.SetCurrentRow(null, DataRowVersion.Default);
      Assert.IsNull(sut.CurrentRow, "CurrentRow #3");
      Assert.AreSame(tbl1, sut.Table, "Table #3");
      Assert.IsNull(sut.GetValue("FInt32"), "FInt32 value #3");

      sut.SetCurrentRow(tbl2.Rows[1], DataRowVersion.Default);
      Assert.AreSame(sut.CurrentRow, tbl2.Rows[1], "CurrentRow #4");
      Assert.AreSame(tbl2, sut.Table, "Table #4");
      Assert.AreEqual(DataRowVersion.Default, sut.RowVersion, "RowVersion #4");
      Assert.AreEqual(TestTable.Row1.VInt32, sut.GetValue("FInt32"), "FInt32 value #4");
    }

    #endregion

    #region Доступ к столбцам

    [Test]
    public void Item_ByColumnIndex()
    {
      DataTable tbl = TestTable.Create();

      DataRowValues sut = new DataRowValues();
      sut.Table = tbl;

      DataColumnValue vInt32 = sut[tbl.Columns.IndexOf("FInt32")];
      Assert.AreEqual("FInt32", vInt32.ColumnName);
    }

    [Test]
    public void Item_ByColumnIndex_OutOfRange()
    {
      DataTable tbl = TestTable.Create();

      DataRowValues sut = new DataRowValues();
      sut.Table = tbl;

      DataColumnValue dummy;
      Assert.Catch<ArgumentOutOfRangeException>(delegate () { dummy = sut[-1]; });
      Assert.Catch<ArgumentOutOfRangeException>(delegate () { dummy = sut[tbl.Columns.Count]; });
    }

    [Test]
    public void Item_ByColumnName()
    {
      DataTable tbl = TestTable.Create();

      DataRowValues sut = new DataRowValues();
      sut.Table = tbl;

      DataColumnValue vInt32 = sut["FInt32"];
      Assert.AreEqual("FInt32", vInt32.ColumnName);
    }


    [Test]
    public void Item_ByColumnName_UnknownName()
    {
      DataTable tbl = TestTable.Create();

      DataRowValues sut = new DataRowValues();
      sut.Table = tbl;

      DataColumnValue dummy;
      Assert.Catch<ArgumentException>(delegate () { dummy = sut["XXX"]; });
      Assert.Catch<ArgumentException>(delegate () { dummy = sut[""]; });
    }

    #endregion

    #region INamedValuesAccess

    [Test]
    public void GetNames()
    {
      DataRowValues sut = new DataRowValues();

      DataTable tbl1 = TestTable.Create();
      sut.Table = tbl1;
      CollectionAssert.AreEqual(TestTable.ColumnNames, sut.GetNames(), "#1. Original test table");

      DataTable tbl2 = new DataTable();
      tbl2.Columns.Add("F1", typeof(string));
      sut.Table = tbl2;
      CollectionAssert.AreEqual(new string[1] { "F1" }, sut.GetNames(), "#2. Table has been changed");
    }

    [Test]
    public void Contains()
    {
      DataRowValues sut = new DataRowValues();
      DataTable tbl = TestTable.Create();
      sut.Table = tbl;

      foreach (DataColumn col in tbl.Columns)
      {
        bool res1 = ((INamedValuesAccess)sut).Contains(col.ColumnName);
        Assert.IsTrue(res1, col.ColumnName);
      }

      bool res2 = ((INamedValuesAccess)sut).Contains("");
      Assert.IsFalse(res2, "Empty string");

      bool res3 = ((INamedValuesAccess)sut).Contains("XXX");
      Assert.IsFalse(res3, "Unknown column name");
    }

    [Test]
    public void GetValue()
    {
      DataRowValues sut = new DataRowValues();
      DataTable tbl = TestTable.Create();
      sut.Table = tbl;

      foreach (DataColumn col in tbl.Columns)
      {
        object res1 = sut.GetValue(col.ColumnName);
        Assert.IsNull(res1, "#1");
      }

      sut.CurrentRow = tbl.Rows[0];
      foreach (DataColumn col in tbl.Columns)
      {
        object res2 = sut.GetValue(col.ColumnName);
        object wantedValue = tbl.Rows[0][col];
        Assert.AreEqual(wantedValue, res2, "#2");
      }
    }

    #endregion
  }

  [TestFixture]
  public class DataTableValuesTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DataTable tbl = TestTable.Create();
      DataTableValues sut = new DataTableValues(tbl);

      Assert.AreSame(tbl, sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.IsTrue(sut.TableIsFixed, "TableIsFixed");
    }

    #endregion

    #region Методы

    [Test]
    public void Read()
    {
      DataTable tbl = TestTable.Create();
      DataTableValues sut = new DataTableValues(tbl);

      bool[] wantedRes = new bool[] { true, true, true, false };
      DataRow[] wantedRows = new DataRow[] { tbl.Rows[0], tbl.Rows[1], tbl.Rows[2], null };
      object[] wantedValues = new object[] { DBNull.Value, TestTable.Row1.VString, TestTable.Row2.VString, null };

      for (int i = 0; i < wantedRes.Length; i++)
      {
        bool res = sut.Read();
        Assert.AreEqual(wantedRes[i], res, "Result #" + (i + 1).ToString());
        Assert.AreSame(wantedRows[i], sut.CurrentRow, "CurrentRow #" + (i + 1).ToString());
        Assert.AreEqual(wantedValues[i], sut.GetValue("FString"), "Value #" + (i + 1).ToString());
      }
    }

    [Test]
    public void ResetRead()
    {
      DataTable tbl = TestTable.Create();
      DataTableValues sut = new DataTableValues(tbl);
      while (sut.Read()) { }

      sut.ResetReading();

      bool[] wantedRes = new bool[] { true, true, true, false };
      for (int i = 0; i < wantedRes.Length; i++)
        Assert.AreEqual(wantedRes[i], sut.Read(), "Result #" + (i + 1).ToString());
    }

    #endregion

    #region Свойства

    [Test]
    public void Table_CannotBeChanged()
    {
      DataTable tbl1 = TestTable.Create();
      DataTable tbl2 = TestTable.Create();
      DataTableValues sut = new DataTableValues(tbl1);

      Assert.DoesNotThrow(delegate () { sut.Table = tbl1; }, "Our table");
      Assert.Catch(delegate () { sut.Table = tbl2; }, "Another table");
    }

    #endregion
  }

  [TestFixture]
  public class DataViewValuesTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DataTable tbl = TestTable.Create();
      DataViewValues sut = new DataViewValues(tbl.DefaultView);

      Assert.AreSame(tbl.DefaultView, sut.DataView, "DataView");
      Assert.AreSame(tbl, sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.IsTrue(sut.TableIsFixed, "TableIsFixed");
    }

    #endregion

    #region Методы

    [Test]
    public void Read()
    {
      DataTable tbl = TestTable.Create();
      // В DataView будут строки с индексами #2 и #1
      tbl.DefaultView.Sort = "FInt32 DESC";
      tbl.DefaultView.RowFilter = "ISNULL(FInt32, 0)<>0";
      DataViewValues sut = new DataViewValues(tbl.DefaultView);

      bool[] wantedRes = new bool[] { true, true, false };
      DataRow[] wantedRows = new DataRow[] { tbl.Rows[2], tbl.Rows[1], null };
      object[] wantedValues = new object[] { TestTable.Row2.VString, TestTable.Row1.VString, null };

      for (int i = 0; i < wantedRes.Length; i++)
      {
        bool res = sut.Read();
        Assert.AreEqual(wantedRes[i], res, "Result #" + (i + 1).ToString());
        Assert.AreSame(wantedRows[i], sut.CurrentRow, "CurrentRow #" + (i + 1).ToString());
        Assert.AreEqual(wantedValues[i], sut.GetValue("FString"), "Value #" + (i + 1).ToString());
      }
    }

    [Test]
    public void ResetRead()
    {
      DataTable tbl = TestTable.Create();
      tbl.DefaultView.Sort = "FInt32 DESC";
      tbl.DefaultView.RowFilter = "ISNULL(FInt32, 0)<>0";
      DataViewValues sut = new DataViewValues(tbl.DefaultView);
      while (sut.Read()) { }

      sut.ResetReading();

      bool[] wantedRes = new bool[] { true, true, false };
      for (int i = 0; i < wantedRes.Length; i++)
        Assert.AreEqual(wantedRes[i], sut.Read(), "Result #" + (i + 1).ToString());
    }

    #endregion

    #region Свойства

    [Test]
    public void Table_CannotBeChanged()
    {
      DataTable tbl1 = TestTable.Create();
      DataTable tbl2 = TestTable.Create();
      DataViewValues sut = new DataViewValues(tbl1.DefaultView);

      Assert.DoesNotThrow(delegate () { sut.Table = tbl1; }, "Our table");
      Assert.Catch(delegate () { sut.Table = tbl2; }, "Another table");
    }

    #endregion
  }

  [TestFixture]
  public class DataRowArrayValuesTests
  {
    #region Конструкторы

    #region DataRow[]

    [Test]
    public void Constructor_DataRowArray_WithTable()
    {
      DataTable tbl0 = TestTable.Create();
      DataTable tbl1 = TestTable.Create();
      DataTable tbl2 = TestTable.Create();
      DataRow[] a = new DataRow[] { tbl1.Rows[0], tbl2.Rows[2], tbl1.Rows[2] };
      DataRowArrayValues sut = new DataRowArrayValues(a, tbl0);

      CollectionAssert.AreEqual(a, sut.Rows, "Rows");
      Assert.AreSame(tbl0, sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.IsFalse(sut.TableIsFixed, "TableIsFixed");
    }

    [Test]
    public void Constructor_DataRowArray_WithoutTable()
    {
      DataTable tbl1 = TestTable.Create();
      DataTable tbl2 = TestTable.Create();
      DataRow[] a = new DataRow[] { tbl1.Rows[0], tbl2.Rows[2], tbl1.Rows[2] };
      DataRowArrayValues sut = new DataRowArrayValues(a, null);

      CollectionAssert.AreEqual(a, sut.Rows, "Rows");
      Assert.AreSame(tbl1, sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.IsFalse(sut.TableIsFixed, "TableIsFixed");
    }

    [Test]
    public void Constructor_DataRowArray_Empty()
    {
      DataRow[] a = EmptyArray<DataRow>.Empty;
      DataRowArrayValues sut = new DataRowArrayValues(a, null);

      Assert.AreEqual(0, sut.Rows.Length, "Rows");
      Assert.IsNull(sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.IsFalse(sut.TableIsFixed, "TableIsFixed");
    }

    #endregion

    #region IEnumerable<DataRow>

    [Test]
    public void Constructor_DataRowEnumerable_WithTable()
    {
      DataTable tbl0 = TestTable.Create();
      DataTable tbl1 = TestTable.Create();
      DataTable tbl2 = TestTable.Create();
      List<DataRow> rows = new List<DataRow>();
      rows.Add(tbl1.Rows[0]);
      rows.Add(tbl2.Rows[2]);
      rows.Add(tbl1.Rows[2]);
      DataRowArrayValues sut = new DataRowArrayValues(rows, tbl0);

      CollectionAssert.AreEqual(rows, sut.Rows, "Rows");
      Assert.AreSame(tbl0, sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.IsFalse(sut.TableIsFixed, "TableIsFixed");
    }

    [Test]
    public void Constructor_DataRowEnumerable_WithoutTable()
    {
      DataTable tbl1 = TestTable.Create();
      DataTable tbl2 = TestTable.Create();
      List<DataRow> rows = new List<DataRow>();
      rows.Add(tbl1.Rows[0]);
      rows.Add(tbl2.Rows[2]);
      rows.Add(tbl1.Rows[2]);
      DataRowArrayValues sut = new DataRowArrayValues(rows, null);

      CollectionAssert.AreEqual(rows, sut.Rows, "Rows");
      Assert.AreSame(tbl1, sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.IsFalse(sut.TableIsFixed, "TableIsFixed");
    }

    [Test]
    public void Constructor_DataRowEnumerable_Empty()
    {
      List<DataRow> lst = new List<DataRow>();
      DataRowArrayValues sut = new DataRowArrayValues(lst, null);

      Assert.AreEqual(0, sut.Rows.Length, "Rows");
      Assert.IsNull(sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.IsFalse(sut.TableIsFixed, "TableIsFixed");
    }

    #endregion

    #region DataRowView[]

    [Test]
    public void Constructor_DataRowViewArray_WithTable()
    {
      DataTable tbl0 = TestTable.Create();
      DataTable tbl1 = TestTable.Create();
      DataTable tbl2 = TestTable.Create();
      DataRowView[] a1 = new DataRowView[] { tbl1.DefaultView[0], tbl2.DefaultView[2], tbl1.DefaultView[2] };
      DataRow[] a2 = new DataRow[] { tbl1.Rows[0], tbl2.Rows[2], tbl1.Rows[2] };
      DataRowArrayValues sut = new DataRowArrayValues(a1, tbl0);

      CollectionAssert.AreEqual(a2, sut.Rows, "Rows");
      Assert.AreSame(tbl0, sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.IsFalse(sut.TableIsFixed, "TableIsFixed");
    }

    [Test]
    public void Constructor_DataRowViewArray_WithoutTable()
    {
      DataTable tbl1 = TestTable.Create();
      DataTable tbl2 = TestTable.Create();
      DataRowView[] a1 = new DataRowView[] { tbl1.DefaultView[0], tbl2.DefaultView[2], tbl1.DefaultView[2] };
      DataRow[] a2 = new DataRow[] { tbl1.Rows[0], tbl2.Rows[2], tbl1.Rows[2] };
      DataRowArrayValues sut = new DataRowArrayValues(a1, null);

      CollectionAssert.AreEqual(a2, sut.Rows, "Rows");
      Assert.AreSame(tbl1, sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.IsFalse(sut.TableIsFixed, "TableIsFixed");
    }

    [Test]
    public void Constructor_DataRowViewArray_Empty()
    {
      DataRowView[] a = EmptyArray<DataRowView>.Empty;
      DataRowArrayValues sut = new DataRowArrayValues(a, null);

      Assert.AreEqual(0, sut.Rows.Length, "Rows");
      Assert.IsNull(sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.IsFalse(sut.TableIsFixed, "TableIsFixed");
    }

    #endregion

    #region IEnumerable<DataRowView>

    [Test]
    public void Constructor_DataRowViewEnumerable_WithTable()
    {
      DataTable tbl0 = TestTable.Create();
      DataTable tbl1 = TestTable.Create();
      DataTable tbl2 = TestTable.Create();
      List<DataRowView> lst1 = new List<DataRowView>();
      lst1.Add(tbl1.DefaultView[0]);
      lst1.Add(tbl2.DefaultView[2]);
      lst1.Add(tbl1.DefaultView[2]);
      DataRow[] a2 = new DataRow[] { tbl1.Rows[0], tbl2.Rows[2], tbl1.Rows[2] };
      DataRowArrayValues sut = new DataRowArrayValues(lst1, tbl0);

      CollectionAssert.AreEqual(a2, sut.Rows, "Rows");
      Assert.AreSame(tbl0, sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.IsFalse(sut.TableIsFixed, "TableIsFixed");
    }

    [Test]
    public void Constructor_DataRowViewEnumerable_WithoutTable()
    {
      DataTable tbl1 = TestTable.Create();
      DataTable tbl2 = TestTable.Create();
      List<DataRowView> lst1 = new List<DataRowView>();
      lst1.Add(tbl1.DefaultView[0]);
      lst1.Add(tbl2.DefaultView[2]);
      lst1.Add(tbl1.DefaultView[2]);
      DataRow[] a2 = new DataRow[] { tbl1.Rows[0], tbl2.Rows[2], tbl1.Rows[2] };
      DataRowArrayValues sut = new DataRowArrayValues(lst1, null);

      CollectionAssert.AreEqual(a2, sut.Rows, "Rows");
      Assert.AreSame(tbl1, sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.IsFalse(sut.TableIsFixed, "TableIsFixed");
    }

    [Test]
    public void Constructor_DataRowViewEnumerable_Empty()
    {
      List<DataRowView> lst = new List<DataRowView>();
      DataRowArrayValues sut = new DataRowArrayValues(lst, null);

      Assert.AreEqual(0, sut.Rows.Length, "Rows");
      Assert.IsNull(sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.IsFalse(sut.TableIsFixed, "TableIsFixed");
    }

    #endregion

    #region Методы

    [Test]
    public void Read()
    {
      DataTable tbl1 = TestTable.Create();
      DataTable tbl2 = TestTable.Create();
      DataRow[] a = new DataRow[] { tbl1.Rows[0], tbl2.Rows[2], tbl1.Rows[2] };
      DataRowArrayValues sut = new DataRowArrayValues(a, null);

      bool[] wantedRes = new bool[] { true, true, true, false };
      DataRow[] wantedRows = new DataRow[] { tbl1.Rows[0], tbl2.Rows[2], tbl1.Rows[2], null };
      object[] wantedValues = new object[] { DBNull.Value,  TestTable.Row2.VString, TestTable.Row2.VString, null };

      for (int i = 0; i < wantedRes.Length; i++)
      {
        bool res = sut.Read();
        Assert.AreEqual(wantedRes[i], res, "Result #" + (i + 1).ToString());
        Assert.AreSame(wantedRows[i], sut.CurrentRow, "CurrentRow #" + (i + 1).ToString());
        Assert.AreEqual(wantedValues[i], sut.GetValue("FString"), "Value #" + (i + 1).ToString());
      }
    }

    [Test]
    public void ResetRead()
    {
      DataTable tbl = TestTable.Create();
      DataRow[] a = DataTools.GetDataTableRows(tbl);
      DataRowArrayValues sut = new DataRowArrayValues(a, null);
      while (sut.Read()) { }

      sut.ResetReading();

      bool[] wantedRes = new bool[] { true, true, true, false };
      for (int i = 0; i < wantedRes.Length; i++)
        Assert.AreEqual(wantedRes[i], sut.Read(), "Result #" + (i + 1).ToString());
    }

    #endregion

    #endregion

    #region Свойства

    [Test]
    public void Table_CanBeChanged()
    {
      DataTable tbl1 = TestTable.Create();
      DataTable tbl2 = TestTable.Create();
      DataRow[] a1 = DataTools.GetDataTableRows(tbl1);
      DataRow[] a2 = DataTools.GetDataTableRows(tbl2);
      DataRow[] a = ArrayTools.MergeArrays<DataRow>(a1, a2);

      DataRowArrayValues sut = new DataRowArrayValues(a, null);

      Assert.DoesNotThrow(delegate () { sut.Table = tbl1; }, "#1");
      Assert.DoesNotThrow(delegate () { sut.Table = tbl2; }, "#2");
    }

    #endregion
  }

  [TestFixture]
  public class DataColumnValueTests
  {
    #region Основные свойства

    [Test]
    public void Value_get()
    {
      DoTestGet("FInt32", "Value", DBNull.Value, TestTable.Row1.VInt32, TestTable.Row2.VInt32);
    }

    [Test]
    public void Value_get_NoRow()
    {
      DataTableValues vals = new DataTableValues(TestTable.Create());
      DataColumnValue sut = vals["FInt32"];

      Assert.IsNull(sut.Value);
    }

    [Test]
    public void Value_set()
    {
      DataTable tbl = TestTable.Create();
      tbl.Rows[2].Delete();
      DataTableValues vals = new DataTableValues(tbl);

      DataRow row = vals.Table.Rows[0];
      vals.CurrentRow = row;
      DataColumnValue sut = vals["FInt32"];

      sut.Value = 1234;
      Assert.AreEqual(1234, row["FInt32"], "#1. Normal value");

      sut.Value = DBNull.Value;
      Assert.AreEqual(DBNull.Value, row["FInt32"], "#2. DBNull");

      sut.Value = null;
      Assert.AreEqual(DBNull.Value, row["FInt32"], "#3. Null as DBNull");

      Assert.Catch(delegate { sut.Value = "ABC"; }, "#4. Wrong value type");

      vals.CurrentRow = vals.Table.Rows[2];
      Assert.Catch(delegate { sut.Value = 123; }, "#5. Row is deleted");

      vals.CurrentRow = null;
      Assert.Catch(delegate { sut.Value = 123; }, "#6. CurrentRow is null");
    }

    [Test]
    public void IsNull()
    {
      DoTestGet("FInt32", "IsNull", true, false, false);
    }

    #endregion

    #region Реализация тестов

    private void DoTestGet(string columnName, string propName, params object[] wantedValues)
    {
      DataTable tbl = TestTable.Create();
      if (wantedValues.Length != tbl.Rows.Count)
        throw ExceptionFactory.ArgWrongCollectionCount("wantedValues", wantedValues, 4);

      DataTableValues vals = new DataTableValues(tbl);
      DataColumnValue sut = vals[columnName];
      Assert.AreEqual(columnName, sut.ColumnName, "ColumnName");

      PropertyInfo pi = sut.GetType().GetProperty(propName);
      if (pi == null)
        throw ExceptionFactory.ArgUnknownValue("propName", propName);

      for (int i = 0; i < wantedValues.Length; i++)
      {
        if (!vals.Read())
          throw new BugException("Can't read table row");

        object value = pi.GetValue(sut, null);
        Assert.AreEqual(wantedValues[i], value, "Row#" + (i + 1).ToString() + ", ColumnName=" + columnName);
      }
    }

    private void DoTestSet(string columnName, string propName, object setValue, object wantedGetValue, bool wantedIsNull)
    {
      DataTableValues vals = new DataTableValues(TestTable.Create());
      vals.CurrentRow = vals.Table.Rows[0]; // строка с DBNull
      DataColumnValue sut = vals[columnName];
      Assert.AreEqual(columnName, sut.ColumnName, "ColumnName");

      PropertyInfo pi = sut.GetType().GetProperty(propName);
      if (pi == null)
        throw ExceptionFactory.ArgUnknownValue("propName", propName);

      pi.SetValue(sut, setValue, null);

      object res = pi.GetValue(sut, null);
      Assert.AreEqual(wantedGetValue, res, "get value, ColumnName=" + columnName + ", PropName=" + propName);

      Assert.AreEqual(wantedIsNull, sut.IsNull, "IsNull");
    }

    #endregion

    #region Форматированный доступ

    public struct GetTestData
    {
      #region Конструктор

      public GetTestData(string columnName, object v0, object v1, object v2)
      {
        _ColumnName = columnName;
        _WantedValues = new object[] { v0, v1, v2 };
      }

      #endregion

      #region Свойства

      public string ColumnName { get { return _ColumnName; } }
      private readonly string _ColumnName;

      public object[] WantedValues { get { return _WantedValues; } }
      private readonly object[] _WantedValues;

      public override string ToString()
      {
        return _ColumnName;
      }

      #endregion
    }

    #region String

    public readonly GetTestData[] AsString_get_Tests = new GetTestData[] {
      new GetTestData("FString", "", TestTable.Row1.VString, TestTable.Row2.VString),
      new GetTestData("FStringNN", "", TestTable.Row1.VString, TestTable.Row2.VString),
      new GetTestData("FInt32", "", TestTable.Row1.StrInt32, TestTable.Row2.StrInt32),
      new GetTestData("FInt32NN", "0", TestTable.Row1.StrInt32, TestTable.Row2.StrInt32),
      new GetTestData("FInt64", "", TestTable.Row1.StrInt64, TestTable.Row2.StrInt64),
      new GetTestData("FSingle", "", TestTable.Row1.StrSingle, TestTable.Row2.StrSingle),
      new GetTestData("FDouble", "", TestTable.Row1.StrDouble, TestTable.Row2.StrDouble),
      new GetTestData("FDecimal", "", TestTable.Row1.StrDecimal, TestTable.Row2.StrDecimal),
      new GetTestData("FBoolean", "", TestTable.Row1.VBoolean.ToString(), TestTable.Row2.VBoolean.ToString()),
      new GetTestData("FDateTime", "", StdConvert.ToString(TestTable.Row1.VDateTime, true), StdConvert.ToString(TestTable.Row2.VDateTime, true)),
      new GetTestData("FTimeSpan", "", StdConvert.ToString(TestTable.Row1.VTimeSpan), StdConvert.ToString(TestTable.Row2.VTimeSpan)),
      new GetTestData("FGuid", "", TestTable.Row1.StrGuid, TestTable.Row2.StrGuid),
      new GetTestData("FEnumString", "", TestTable.Row1.VEnum.ToString(), TestTable.Row2.VEnum.ToString()),
      new GetTestData("FEnumInt32", "", TestTable.Row1.StrEnumInt32, TestTable.Row2.StrEnumInt32),
    };
    [TestCaseSource("AsString_get_Tests")]
    public void AsString_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsString", data.WantedValues);
    }

    [TestCase("FString", "ABCDEF", "ABCDEF", false)]
    [TestCase("FString", "", "", true)]
    [TestCase("FString", null, "", true)]
    [TestCase("FStringNN", "ABCDEF", "ABCDEF", false)]
    [TestCase("FStringNN", "", "", false)]
    [TestCase("FStringNN", null, "", false)]
    [TestCase("FInt32", "12345", "12345", false)]
    [TestCase("FInt32", "", "", true)]
    [TestCase("FInt32NN", "12345", "12345", false)]
    [TestCase("FInt32NN", "", "0", false)]
    [TestCase("FInt64", "12345", "12345", false)]
    [TestCase("FInt64", "", "", true)]
    [TestCase("FSingle", "123", "123", false)]
    [TestCase("FSingle", "", "", true)]
    [TestCase("FDouble", "123", "123", false)]
    [TestCase("FDouble", "", "", true)]
    [TestCase("FDecimal", "123", "123", false)]
    [TestCase("FDecimal", "", "", true)]
    [TestCase("FBoolean", "False", "False", false)]
    [TestCase("FBoolean", "True", "True", false)]
    [TestCase("FBoolean", "0", "False", false)]
    [TestCase("FBoolean", "1", "True", false)]
    [TestCase("FBoolean", "", "", true)]
    [TestCase("FGuid", "", "", true)]
    [TestCase("FGuid", "00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000", false)]
    [TestCase("FGuid", "{946D61C6-99F4-4F70-B037-3777740F96F3}", "946d61c6-99f4-4f70-b037-3777740f96f3", false)]
    [TestCase("FEnumString", "", "", true)]
    [TestCase("FEnumString", "Zero", "Zero", false)]
    [TestCase("FEnumString", "One", "One", false)]
    public void AsString_set(string columnName, string setValue, string wantedGetValue, bool wantedIsNull)
    {
      DoTestSet(columnName, "AsString", setValue, wantedGetValue, wantedIsNull);
    }

    #endregion

    #region Int32

    public readonly GetTestData[] AsInt32_get_Tests = new GetTestData[] {
      new GetTestData("FInt32", 0, TestTable.Row1.VInt32, TestTable.Row2.VInt32),
      new GetTestData("FInt32NN", 0, TestTable.Row1.VInt32, TestTable.Row2.VInt32),
      new GetTestData("FInt64", 0, (int)(TestTable.Row1.VInt64), (int)(TestTable.Row2.VInt64)),
      new GetTestData("FSingle", 0, TestTable.Row1.RoundedSingle, TestTable.Row2.RoundedSingle),
      new GetTestData("FDouble", 0, TestTable.Row1.RoundedDouble, TestTable.Row2.RoundedDouble),
      new GetTestData("FDecimal", 0, TestTable.Row1.RoundedDecimal, TestTable.Row2.RoundedDecimal),
      new GetTestData("FBoolean", 0, TestTable.Row1.VBoolean?1:0, TestTable.Row2.VBoolean?1:0),
    };
    [TestCaseSource("AsInt32_get_Tests")]
    public void AsInt32_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsInt32", data.WantedValues);
    }

    [TestCase("FString", 123, 123, false)]
    [TestCase("FString", 0, 0, true)]
    [TestCase("FStringNN", 0, 0, false)]
    [TestCase("FInt32", 123, 123, false)]
    [TestCase("FInt32", 0, 0, true)]
    [TestCase("FInt32NN", 0, 0, false)]
    [TestCase("FInt64", 123, 123, false)]
    [TestCase("FInt64", 0, 0, true)]
    [TestCase("FSingle", 123, 123, false)]
    [TestCase("FSingle", 0, 0, true)]
    [TestCase("FDouble", 123, 123, false)]
    [TestCase("FDouble", 0, 0, true)]
    [TestCase("FDecimal", 123, 123, false)]
    [TestCase("FDecimal", 0, 0, true)]
    [TestCase("FBoolean", 123, 1, false)]
    [TestCase("FBoolean", 0, 0, true)]
    [TestCase("FBooleanNN", 0, 0, false)]
    public void AsInt32_set(string columnName, int setValue, int wantedGetValue, bool wantedIsNull)
    {
      DoTestSet(columnName, "AsInt32", setValue, wantedGetValue, wantedIsNull);
    }


    public readonly GetTestData[] AsNullableInt32_get_Tests = new GetTestData[] {
      new GetTestData("FInt32", null, TestTable.Row1.VInt32, TestTable.Row2.VInt32),
      new GetTestData("FInt32NN", 0, TestTable.Row1.VInt32, TestTable.Row2.VInt32),
      new GetTestData("FInt64", null, (int)(TestTable.Row1.VInt64), (int)(TestTable.Row2.VInt64)),
      new GetTestData("FSingle", null, TestTable.Row1.RoundedSingle, TestTable.Row2.RoundedSingle),
      new GetTestData("FDouble", null, TestTable.Row1.RoundedDouble, TestTable.Row2.RoundedDouble),
      new GetTestData("FDecimal", null, TestTable.Row1.RoundedDecimal, TestTable.Row2.RoundedDecimal),
      new GetTestData("FBoolean", null, TestTable.Row1.VBoolean?1:0, TestTable.Row2.VBoolean?1:0),
    };
    [TestCaseSource("AsNullableInt32_get_Tests")]
    public void AsNullableInt32_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsNullableInt32", data.WantedValues);
    }

    [TestCase("FString", 123, 123, false)]
    [TestCase("FString", 0, 0, false)]
    [TestCase("FString", null, null, true)]
    [TestCase("FStringNN", null, null, false)]
    [TestCase("FInt32", 123, 123, false)]
    [TestCase("FInt32", 0, 0, false)]
    [TestCase("FInt32", null, null, true)]
    [TestCase("FInt32NN", null, 0, false)]
    [TestCase("FInt64", 123, 123, false)]
    [TestCase("FInt64", 0, 0, false)]
    [TestCase("FInt64", null, null, true)]
    [TestCase("FSingle", 123, 123, false)]
    [TestCase("FSingle", 0, 0, false)]
    [TestCase("FSingle", null, null, true)]
    [TestCase("FDouble", 123, 123, false)]
    [TestCase("FDouble", 0, 0, false)]
    [TestCase("FDouble", null, null, true)]
    [TestCase("FDecimal", 123, 123, false)]
    [TestCase("FDecimal", 0, 0, false)]
    [TestCase("FDecimal", null, null, true)]
    [TestCase("FBoolean", 123, 1, false)]
    [TestCase("FBoolean", 0, 0, false)]
    [TestCase("FBoolean", null, null, true)]
    [TestCase("FBooleanNN", null, 0, false)]
    public void AsNullableInt32_set(string columnName, int? setValue, int? wantedGetValue, bool wantedIsNull)
    {
      DoTestSet(columnName, "AsNullableInt32", setValue, wantedGetValue, wantedIsNull);
    }

    #endregion

    #region Int64

    public readonly GetTestData[] AsInt64_get_Tests = new GetTestData[] {
      new GetTestData("FInt32", 0L, (long)(TestTable.Row1.VInt32), (long)(TestTable.Row2.VInt32)),
      new GetTestData("FInt32NN", 0L, (long)(TestTable.Row1.VInt32), (long)(TestTable.Row2.VInt32)),
      new GetTestData("FInt64", 0L, TestTable.Row1.VInt64, TestTable.Row2.VInt64),
      new GetTestData("FSingle", 0L, (long)(TestTable.Row1.RoundedSingle), (long)(TestTable.Row2.RoundedSingle)),
      new GetTestData("FDouble", 0L, (long)(TestTable.Row1.RoundedDouble), (long)(TestTable.Row2.RoundedDouble)),
      new GetTestData("FDecimal", 0L, (long)(TestTable.Row1.RoundedDecimal), (long)(TestTable.Row2.RoundedDecimal)),
      new GetTestData("FBoolean", 0L, TestTable.Row1.VBoolean?1L:0L, TestTable.Row2.VBoolean?1L:0L),
    };
    [TestCaseSource("AsInt64_get_Tests")]
    public void AsInt64_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsInt64", data.WantedValues);
    }

    [TestCase("FString", 123L, 123L, false)]
    [TestCase("FString", 0L, 0L, true)]
    [TestCase("FStringNN", 0L, 0L, false)]
    [TestCase("FInt32", 123L, 123L, false)]
    [TestCase("FInt32", 0L, 0L, true)]
    [TestCase("FInt32NN", 0L, 0L, false)]
    [TestCase("FInt64", 123L, 123L, false)]
    [TestCase("FInt64", 0L, 0L, true)]
    [TestCase("FSingle", 123L, 123L, false)]
    [TestCase("FSingle", 0L, 0L, true)]
    [TestCase("FDouble", 123L, 123L, false)]
    [TestCase("FDouble", 0L, 0L, true)]
    [TestCase("FDecimal", 123L, 123L, false)]
    [TestCase("FDecimal", 0L, 0L, true)]
    [TestCase("FBoolean", 123L, 1L, false)]
    [TestCase("FBoolean", 0L, 0L, true)]
    [TestCase("FBooleanNN", 0L, 0L, false)]
    public void AsInt64_set(string columnName, long setValue, long wantedGetValue, bool wantedIsNull)
    {
      DoTestSet(columnName, "AsInt64", setValue, wantedGetValue, wantedIsNull);
    }


    public readonly GetTestData[] AsNullableInt64_get_Tests = new GetTestData[] {
      new GetTestData("FInt32", null, (long)(TestTable.Row1.VInt32), (long)(TestTable.Row2.VInt32)),
      new GetTestData("FInt32NN", 0, (long)(TestTable.Row1.VInt32), (long)(TestTable.Row2.VInt32)),
      new GetTestData("FInt64", null, TestTable.Row1.VInt64, TestTable.Row2.VInt64),
      new GetTestData("FSingle", null, (long)(TestTable.Row1.RoundedSingle), (long)(TestTable.Row2.RoundedSingle)),
      new GetTestData("FDouble", null, (long)(TestTable.Row1.RoundedDouble), (long)(TestTable.Row2.RoundedDouble)),
      new GetTestData("FDecimal", null, (long)(TestTable.Row1.RoundedDecimal), (long)(TestTable.Row2.RoundedDecimal)),
      new GetTestData("FBoolean", null, TestTable.Row1.VBoolean?1L:0L, TestTable.Row2.VBoolean?1L:0L),
    };
    [TestCaseSource("AsNullableInt64_get_Tests")]
    public void AsNullableInt64_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsNullableInt64", data.WantedValues);
    }

    [TestCase("FString", 123L, 123L, false)]
    [TestCase("FString", 0L, 0L, false)]
    [TestCase("FString", null, null, true)]
    [TestCase("FStringNN", null, null, false)]
    [TestCase("FInt32", 123L, 123L, false)]
    [TestCase("FInt32", 0L, 0L, false)]
    [TestCase("FInt32", null, null, true)]
    [TestCase("FInt32NN", null, 0L, false)]
    [TestCase("FInt64", 123L, 123L, false)]
    [TestCase("FInt64", 0L, 0L, false)]
    [TestCase("FInt64", null, null, true)]
    [TestCase("FSingle", 123L, 123L, false)]
    [TestCase("FSingle", 0L, 0L, false)]
    [TestCase("FSingle", null, null, true)]
    [TestCase("FDouble", 123L, 123L, false)]
    [TestCase("FDouble", 0L, 0L, false)]
    [TestCase("FDouble", null, null, true)]
    [TestCase("FDecimal", 123L, 123L, false)]
    [TestCase("FDecimal", 0L, 0L, false)]
    [TestCase("FDecimal", null, null, true)]
    [TestCase("FBoolean", 123L, 1L, false)]
    [TestCase("FBoolean", 0L, 0L, false)]
    [TestCase("FBoolean", null, null, true)]
    [TestCase("FBooleanNN", null, 0L, false)]
    public void AsNullableInt64_set(string columnName, long? setValue, long? wantedGetValue, bool wantedIsNull)
    {
      DoTestSet(columnName, "AsNullableInt64", setValue, wantedGetValue, wantedIsNull);
    }

    #endregion

    #region Single

    public readonly GetTestData[] AsSingle_get_Tests = new GetTestData[] {
      new GetTestData("FInt32", 0f, (float)(TestTable.Row1.VInt32), (float)(TestTable.Row2.VInt32)),
      new GetTestData("FInt32NN", 0f, (float)(TestTable.Row1.VInt32), (float)(TestTable.Row2.VInt32)),
      new GetTestData("FInt64", 0f, (float)(TestTable.Row1.VInt64), (float)(TestTable.Row2.VInt64)),
      new GetTestData("FSingle", 0f, TestTable.Row1.VSingle, TestTable.Row2.VSingle),
      new GetTestData("FDouble", 0f, (float)(TestTable.Row1.VDouble), (float)(TestTable.Row2.VDouble)),
      new GetTestData("FDecimal", 0f, (float)(TestTable.Row1.VDecimal), (float)(TestTable.Row2.VDecimal)),
      new GetTestData("FBoolean", 0f, TestTable.Row1.VBoolean?1f:0f, TestTable.Row2.VBoolean?1f:0f),
    };
    [TestCaseSource("AsSingle_get_Tests")]
    public void AsSingle_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsSingle", data.WantedValues);
    }

    [TestCase("FString", 123.5f, 123.5f, false)]
    [TestCase("FString", 0f, 0f, true)]
    [TestCase("FStringNN", 0f, 0f, false)]
    [TestCase("FInt32", 1.5f, 2f, false)]
    [TestCase("FInt32", 2.5f, 3f, false)]
    [TestCase("FInt32", 3.5f, 4f, false)]
    [TestCase("FInt32", 0f, 0f, true)]
    [TestCase("FInt32NN", 0f, 0f, false)]
    [TestCase("FInt64", -1.5f, -2f, false)]
    [TestCase("FInt64", -2.5f, -3f, false)]
    [TestCase("FInt64", -3.5f, -4f, false)]
    [TestCase("FInt64", 0f, 0f, true)]
    [TestCase("FSingle", 12.5f, 12.5f, false)]
    [TestCase("FSingle", 0f, 0f, true)]
    [TestCase("FDouble", 11.5f, 11.5f, false)]
    [TestCase("FDouble", 0f, 0f, true)]
    [TestCase("FDecimal", 1.5f, 1.5f, false)]
    [TestCase("FDecimal", 0f, 0f, true)]
    [TestCase("FBoolean", 1f, 1f, false)]
    [TestCase("FBoolean", 0f, 0f, true)]
    [TestCase("FBooleanNN", 0f, 0f, false)]
    public void AsSingle_set(string columnName, float setValue, float wantedGetValue, bool wantedIsNull)
    {
      DoTestSet(columnName, "AsSingle", setValue, wantedGetValue, wantedIsNull);
    }


    public readonly GetTestData[] AsNullableSingle_get_Tests = new GetTestData[] {
      new GetTestData("FInt32", null, (float)(TestTable.Row1.VInt32), (float)(TestTable.Row2.VInt32)),
      new GetTestData("FInt32NN", 0f, (float)(TestTable.Row1.VInt32), (float)(TestTable.Row2.VInt32)),
      new GetTestData("FInt64", null, (float)(TestTable.Row1.VInt64), (float)(TestTable.Row2.VInt64)),
      new GetTestData("FSingle", null, TestTable.Row1.VSingle, TestTable.Row2.VSingle),
      new GetTestData("FDouble", null, (float)(TestTable.Row1.VDouble), (float)(TestTable.Row2.VDouble)),
      new GetTestData("FDecimal", null, (float)(TestTable.Row1.VDecimal), (float)(TestTable.Row2.VDecimal)),
      new GetTestData("FBoolean", null, TestTable.Row1.VBoolean?1f:0f, TestTable.Row2.VBoolean?1f:0f),
    };
    [TestCaseSource("AsNullableSingle_get_Tests")]
    public void AsNullableSingle_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsNullableSingle", data.WantedValues);
    }

    [TestCase("FString", 123.5f, 123.5f, false)]
    [TestCase("FString", 0f, 0f, false)]
    [TestCase("FString", null, null, true)]
    [TestCase("FStringNN", null, null, false)]
    [TestCase("FInt32", 1.5f, 2f, false)]
    [TestCase("FInt32", 2.5f, 3f, false)]
    [TestCase("FInt32", 3.5f, 4f, false)]
    [TestCase("FInt32", 0f, 0f, false)]
    [TestCase("FInt32", null, null, true)]
    [TestCase("FInt32NN", null, 0f, false)]
    [TestCase("FInt64", -1.5f, -2f, false)]
    [TestCase("FInt64", -2.5f, -3f, false)]
    [TestCase("FInt64", -3.5f, -4f, false)]
    [TestCase("FInt64", 0f, 0f, false)]
    [TestCase("FInt64", null, null, true)]
    [TestCase("FSingle", 12.5f, 12.5f, false)]
    [TestCase("FSingle", 0f, 0f, false)]
    [TestCase("FSingle", null, null, true)]
    [TestCase("FDouble", 11.5f, 11.5f, false)]
    [TestCase("FDouble", 0f, 0f, false)]
    [TestCase("FDouble", null, null, true)]
    [TestCase("FDecimal", 1.5f, 1.5f, false)]
    [TestCase("FDecimal", 0f, 0f, false)]
    [TestCase("FDecimal", null, null, true)]
    [TestCase("FBoolean", 1f, 1f, false)]
    [TestCase("FBoolean", 0f, 0f, false)]
    [TestCase("FBoolean", null, null, true)]
    [TestCase("FBooleanNN", null, 0f, false)]
    public void AsNullableSingle_set(string columnName, float? setValue, float? wantedGetValue, bool wantedIsNull)
    {
      DoTestSet(columnName, "AsNullableSingle", setValue, wantedGetValue, wantedIsNull);
    }

    #endregion

    #region Double

    public readonly GetTestData[] AsDouble_get_Tests = new GetTestData[] {
      new GetTestData("FInt32", 0.0, (double)(TestTable.Row1.VInt32), (float)(TestTable.Row2.VInt32)),
      new GetTestData("FInt32NN", 0.0, (double)(TestTable.Row1.VInt32), (float)(TestTable.Row2.VInt32)),
      new GetTestData("FInt64", 0.0, (double)(TestTable.Row1.VInt64), (float)(TestTable.Row2.VInt64)),
      new GetTestData("FSingle", 0.0, (double)(TestTable.Row1.VSingle), (double)(TestTable.Row2.VSingle)),
      new GetTestData("FDouble", 0.0, TestTable.Row1.VDouble, TestTable.Row2.VDouble),
      new GetTestData("FDecimal", 0.0, (double)(TestTable.Row1.VDecimal), (double)(TestTable.Row2.VDecimal)),
      new GetTestData("FBoolean", 0.0, TestTable.Row1.VBoolean?1.0:0.0, TestTable.Row2.VBoolean?1.0:0.0),
    };
    [TestCaseSource("AsDouble_get_Tests")]
    public void AsDouble_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsDouble", data.WantedValues);
    }

    [TestCase("FString", 123.5, 123.5, false)]
    [TestCase("FString", 0.0, 0.0, true)]
    [TestCase("FStringNN", 0.0, 0.0, false)]
    [TestCase("FInt32", 1.5, 2.0, false)]
    [TestCase("FInt32", 2.5, 3.0, false)]
    [TestCase("FInt32", 3.5, 4.0, false)]
    [TestCase("FInt32", 0.0, 0.0, true)]
    [TestCase("FInt32NN", 0.0, 0.0, false)]
    [TestCase("FInt64", -1.5, -2.0, false)]
    [TestCase("FInt64", -2.5, -3.0, false)]
    [TestCase("FInt64", -3.5, -4.0, false)]
    [TestCase("FInt64", 0.0, 0.0, true)]
    [TestCase("FSingle", 12.5, 12.5, false)]
    [TestCase("FSingle", 0.0, 0.0, true)]
    [TestCase("FDouble", 11.5, 11.5, false)]
    [TestCase("FDouble", 0.0, 0.0, true)]
    [TestCase("FDecimal", 1.5, 1.5, false)]
    [TestCase("FDecimal", 0.0, 0.0, true)]
    [TestCase("FBoolean", 1.0, 1.0, false)]
    [TestCase("FBoolean", 0.0, 0.0, true)]
    [TestCase("FBooleanNN", 0f, 0f, false)]
    public void AsDouble_set(string columnName, double setValue, double wantedGetValue, bool wantedIsNull)
    {
      DoTestSet(columnName, "AsDouble", setValue, wantedGetValue, wantedIsNull);
    }


    public readonly GetTestData[] AsNullableDouble_get_Tests = new GetTestData[] {
      new GetTestData("FInt32", null, (double)(TestTable.Row1.VInt32), (double)(TestTable.Row2.VInt32)),
      new GetTestData("FInt32NN", 0.0, (double)(TestTable.Row1.VInt32), (double)(TestTable.Row2.VInt32)),
      new GetTestData("FInt64", null, (double)(TestTable.Row1.VInt64), (double)(TestTable.Row2.VInt64)),
      new GetTestData("FSingle", null, (double)(TestTable.Row1.VSingle), (double)(TestTable.Row2.VSingle)),
      new GetTestData("FDouble", null, TestTable.Row1.VDouble, TestTable.Row2.VDouble),
      new GetTestData("FDecimal", null, (double)(TestTable.Row1.VDecimal), (double)(TestTable.Row2.VDecimal)),
      new GetTestData("FBoolean", null, TestTable.Row1.VBoolean?1.0:0.0, TestTable.Row2.VBoolean?1.0:0.0),
    };
    [TestCaseSource("AsNullableDouble_get_Tests")]
    public void AsNullableDouble_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsNullableDouble", data.WantedValues);
    }

    [TestCase("FString", 123.5, 123.5, false)]
    [TestCase("FString", 0.0, 0.0, false)]
    [TestCase("FString", null, null, true)]
    [TestCase("FStringNN", null, null, false)]
    [TestCase("FInt32", 1.5, 2.0, false)]
    [TestCase("FInt32", 2.5, 3.0, false)]
    [TestCase("FInt32", 3.5, 4.0, false)]
    [TestCase("FInt32", 0.0, 0.0, false)]
    [TestCase("FInt32", null, null, true)]
    [TestCase("FInt32NN", null, 0.0, false)]
    [TestCase("FInt64", -1.5, -2.0, false)]
    [TestCase("FInt64", -2.5, -3.0, false)]
    [TestCase("FInt64", -3.5, -4.0, false)]
    [TestCase("FInt64", 0.0, 0.0, false)]
    [TestCase("FInt64", null, null, true)]
    [TestCase("FSingle", 12.5, 12.5, false)]
    [TestCase("FSingle", 0.0, 0.0, false)]
    [TestCase("FSingle", null, null, true)]
    [TestCase("FDouble", 11.5, 11.5, false)]
    [TestCase("FDouble", 0.0, 0.0, false)]
    [TestCase("FDouble", null, null, true)]
    [TestCase("FDecimal", 1.5, 1.5, false)]
    [TestCase("FDecimal", 0.0, 0.0, false)]
    [TestCase("FDecimal", null, null, true)]
    [TestCase("FBoolean", 1.0, 1.0, false)]
    [TestCase("FBoolean", 0.0, 0.0, false)]
    [TestCase("FBoolean", null, null, true)]
    [TestCase("FBooleanNN", null, 0.0, false)]
    public void AsNullableDouble_set(string columnName, double? setValue, double? wantedGetValue, bool wantedIsNull)
    {
      DoTestSet(columnName, "AsNullableDouble", setValue, wantedGetValue, wantedIsNull);
    }

    #endregion

    #region Decimal

    public readonly GetTestData[] AsDecimal_get_Tests = new GetTestData[] {
      new GetTestData("FInt32", 0m, (decimal)(TestTable.Row1.VInt32), (decimal)(TestTable.Row2.VInt32)),
      new GetTestData("FInt32NN", 0m, (decimal)(TestTable.Row1.VInt32), (decimal)(TestTable.Row2.VInt32)),
      new GetTestData("FInt64", 0m, (decimal)(TestTable.Row1.VInt64), (decimal)(TestTable.Row2.VInt64)),
      new GetTestData("FSingle", 0m, (decimal)(TestTable.Row1.VSingle), (decimal)(TestTable.Row2.VSingle)),
      new GetTestData("FDouble", 0m, (decimal)(TestTable.Row1.VDouble), (decimal)(TestTable.Row2.VDouble)),
      new GetTestData("FDecimal", 0m, TestTable.Row1.VDecimal, TestTable.Row2.VDecimal),
      new GetTestData("FBoolean", 0m, TestTable.Row1.VBoolean?1m:0m, TestTable.Row2.VBoolean?1m:0m),
    };
    [TestCaseSource("AsDecimal_get_Tests")]
    public void AsDecimal_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsDecimal", data.WantedValues);
    }

    [TestCase("FString", "123.5", "123.5", false)]
    [TestCase("FString", "0", "0", true)]
    [TestCase("FStringNN", "0", "0", false)]
    [TestCase("FInt32", "1.5", "2", false)]
    [TestCase("FInt32", "2.5", "3", false)]
    [TestCase("FInt32", "3.5", "4", false)]
    [TestCase("FInt32", "0", "0", true)]
    [TestCase("FInt32NN", "0", "0", false)]
    [TestCase("FInt64", "-1.5", "-2", false)]
    [TestCase("FInt64", "-2.5", "-3", false)]
    [TestCase("FInt64", "-3.5", "-4", false)]
    [TestCase("FInt64", "0", "0", true)]
    [TestCase("FSingle", "12.5", "12.5", false)]
    [TestCase("FSingle", "0", "0", true)]
    [TestCase("FDouble", "11.5", "11.5", false)]
    [TestCase("FDouble", "0", "0", true)]
    [TestCase("FDecimal", "1.5", "1.5", false)]
    [TestCase("FDecimal", "0", "0", true)]
    [TestCase("FBoolean", "1", "1", false)]
    [TestCase("FBoolean", "0", "0", true)]
    [TestCase("FBooleanNN", "0", "0", false)]
    public void AsDecimal_set(string columnName, string sSetValue, string sWantedGetValue, bool wantedIsNull)
    {
      decimal setValue = StdConvert.ToDecimal(sSetValue);
      decimal wantedGetValue = StdConvert.ToDecimal(sWantedGetValue);

      DoTestSet(columnName, "AsDecimal", setValue, wantedGetValue, wantedIsNull);
    }


    public readonly GetTestData[] AsNullableDecimal_get_Tests = new GetTestData[] {
      new GetTestData("FInt32", null, (decimal)(TestTable.Row1.VInt32), (decimal)(TestTable.Row2.VInt32)),
      new GetTestData("FInt32NN", 0f, (decimal)(TestTable.Row1.VInt32), (decimal)(TestTable.Row2.VInt32)),
      new GetTestData("FInt64", null, (decimal)(TestTable.Row1.VInt64), (decimal)(TestTable.Row2.VInt64)),
      new GetTestData("FSingle", null, (decimal)(TestTable.Row1.VSingle), (decimal)(TestTable.Row2.VSingle)),
      new GetTestData("FDouble", null, (decimal)(TestTable.Row1.VDouble), (decimal)(TestTable.Row2.VDouble)),
      new GetTestData("FDecimal", null, TestTable.Row1.VDecimal, TestTable.Row2.VDecimal),
      new GetTestData("FBoolean", null, TestTable.Row1.VBoolean?1m:0m, TestTable.Row2.VBoolean?1m:0m),
    };
    [TestCaseSource("AsNullableDecimal_get_Tests")]
    public void AsNullableDecimal_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsNullableDecimal", data.WantedValues);
    }

    [TestCase("FString", "123.5", "123.5", false)]
    [TestCase("FString", "0", "0", false)]
    [TestCase("FString", null, null, true)]
    [TestCase("FStringNN", null, null, false)]
    [TestCase("FInt32", "1.5", "2", false)]
    [TestCase("FInt32", "2.5", "3", false)]
    [TestCase("FInt32", "3.5", "4", false)]
    [TestCase("FInt32", "0", "0", false)]
    [TestCase("FInt32", null, null, true)]
    [TestCase("FInt32NN", null, "0", false)]
    [TestCase("FInt64", "-1.5", "-2", false)]
    [TestCase("FInt64", "-2.5", "-3", false)]
    [TestCase("FInt64", "-3.5", "-4", false)]
    [TestCase("FInt64", "0", "0", false)]
    [TestCase("FInt64", null, null, true)]
    [TestCase("FSingle", "12.5", "12.5", false)]
    [TestCase("FSingle", "0", "0", false)]
    [TestCase("FSingle", null, null, true)]
    [TestCase("FDouble", "11.5", "11.5", false)]
    [TestCase("FDouble", "0", "0", false)]
    [TestCase("FDouble", null, null, true)]
    [TestCase("FDecimal", "1.5", "1.5", false)]
    [TestCase("FDecimal", "0", "0", false)]
    [TestCase("FDecimal", null, null, true)]
    [TestCase("FBoolean", "1", "1", false)]
    [TestCase("FBoolean", "0", "0", false)]
    [TestCase("FBoolean", null, null, true)]
    [TestCase("FBooleanNN", null, "0", false)]
    public void AsNullableDecimal_set(string columnName, string sSetValue, string sWantedGetValue, bool wantedIsNull)
    {
      decimal? setValue = Creators.NDecimal(sSetValue);
      decimal? wantedGetValue = Creators.NDecimal(sWantedGetValue);

      DoTestSet(columnName, "AsNullableDecimal", setValue, wantedGetValue, wantedIsNull);
    }

    #endregion

    #region Boolean

    public readonly GetTestData[] AsBoolean_get_Tests = new GetTestData[] {
      new GetTestData("FInt32", false, TestTable.Row1.VInt32!=0, TestTable.Row2.VInt32!=0),
      new GetTestData("FInt32NN", false, TestTable.Row1.VInt32!=0, TestTable.Row2.VInt32!=0),
      new GetTestData("FInt64", false, TestTable.Row1.VInt64!=0L, TestTable.Row2.VInt64!=0L),
      new GetTestData("FSingle", false, TestTable.Row1.VSingle!=0f, TestTable.Row2.VSingle!=0f),
      new GetTestData("FDouble", false, TestTable.Row1.VDouble!=0.0, TestTable.Row2.VDouble!=0.0),
      new GetTestData("FDecimal", false, TestTable.Row1.VDecimal!=0m, TestTable.Row2.VDecimal!=0m),
      new GetTestData("FBoolean", false, TestTable.Row1.VBoolean, TestTable.Row2.VBoolean),
      new GetTestData("FBooleanNN", false, TestTable.Row1.VBoolean, TestTable.Row2.VBoolean),
    };
    [TestCaseSource("AsBoolean_get_Tests")]
    public void AsBoolean_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsBoolean", data.WantedValues);
    }

    [TestCase("FString", true, true, false)]
    [TestCase("FString", false, false, true)]
    [TestCase("FStringNN", false, false, false)]
    [TestCase("FInt32", true, true, false)]
    [TestCase("FInt32", false, false, true)]
    [TestCase("FInt32NN", false, false, false)]
    [TestCase("FInt64", true, true, false)]
    [TestCase("FInt64", false, false, true)]
    [TestCase("FSingle", true, true, false)]
    [TestCase("FSingle", false, false, true)]
    [TestCase("FDouble", true, true, false)]
    [TestCase("FDouble", false, false, true)]
    [TestCase("FDecimal", true, true, false)]
    [TestCase("FDecimal", false, false, true)]
    [TestCase("FBoolean", true, true, false)]
    [TestCase("FBoolean", false, false, true)]
    [TestCase("FBooleanNN", false, false, false)]
    public void AsBoolean_set(string columnName, bool setValue, bool wantedGetValue, bool wantedIsNull)
    {
      DoTestSet(columnName, "AsBoolean", setValue, wantedGetValue, wantedIsNull);
    }

    #endregion

    #region DateTime

    public readonly GetTestData[] AsDateTime_get_Tests = new GetTestData[] {
      new GetTestData("FDateTime", DateTime.MinValue, TestTable.Row1.VDateTime, TestTable.Row2.VDateTime),
    };
    [TestCaseSource("AsDateTime_get_Tests")]
    public void AsDateTime_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsDateTime", data.WantedValues);
    }

    [TestCase("FString", "20250626", "20250626", false)]
    [TestCase("FString", Creators.StrDateMinValue, Creators.StrDateMinValue, true)]
    [TestCase("FStringNN", Creators.StrDateMinValue, Creators.StrDateMinValue, false)]
    [TestCase("FDateTime", "20250626", "20250626", false)]
    [TestCase("FDateTime", Creators.StrDateMinValue, Creators.StrDateMinValue, true)]
    public void AsDateTime_set(string columnName, string sSetValue, string sWantedGetValue, bool wantedIsNull)
    {
      DateTime? setValue = Creators.NDateTime(sSetValue);
      DateTime? wantedGetValue = Creators.NDateTime(sWantedGetValue);

      DoTestSet(columnName, "AsDateTime", setValue, wantedGetValue, wantedIsNull);
    }


    public readonly GetTestData[] AsNullableDateTime_get_Tests = new GetTestData[] {
      new GetTestData("FDateTime", null, TestTable.Row1.VDateTime, TestTable.Row2.VDateTime),
    };
    [TestCaseSource("AsNullableDateTime_get_Tests")]
    public void AsNullableDateTime_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsNullableDateTime", data.WantedValues);
    }

    [TestCase("FString", "20250626", "20250626", false)]
    [TestCase("FString", Creators.StrDateMinValue, Creators.StrDateMinValue, false)]
    [TestCase("FString", "", "", true)]
    [TestCase("FStringNN", "", "", false)]
    [TestCase("FDateTime", "20250626", "20250626", false)]
    [TestCase("FDateTime", Creators.StrDateMinValue, Creators.StrDateMinValue, false)]
    [TestCase("FDateTime", "", "", true)]
    public void AsNullableDateTime_set(string columnName, string sSetValue, string sWantedGetValue, bool wantedIsNull)
    {
      DateTime? setValue = Creators.NDateTime(sSetValue);
      DateTime? wantedGetValue = Creators.NDateTime(sWantedGetValue);

      DoTestSet(columnName, "AsNullableDateTime", setValue, wantedGetValue, wantedIsNull);
    }

    #endregion

    #region TimeSpan

    public readonly GetTestData[] AsTimeSpan_get_Tests = new GetTestData[] {
      new GetTestData("FTimeSpan", TimeSpan.Zero, TestTable.Row1.VTimeSpan, TestTable.Row2.VTimeSpan),
    };
    [TestCaseSource("AsTimeSpan_get_Tests")]
    public void AsTimeSpan_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsTimeSpan", data.WantedValues);
    }

    [TestCase("FString", "12:34:56", "12:34:56", false)]
    [TestCase("FString", Creators.StrTimeSpanZeroValue, Creators.StrTimeSpanZeroValue, true)]
    [TestCase("FStringNN", Creators.StrTimeSpanZeroValue, Creators.StrTimeSpanZeroValue, false)]
    [TestCase("FTimeSpan", "12:34:56", "12:34:56", false)]
    [TestCase("FTimeSpan", Creators.StrTimeSpanZeroValue, Creators.StrTimeSpanZeroValue, true)]
    public void AsTimeSpan_set(string columnName, string sSetValue, string sWantedGetValue, bool wantedIsNull)
    {
      TimeSpan? setValue = Creators.NTimeSpan(sSetValue);
      TimeSpan? wantedGetValue = Creators.NTimeSpan(sWantedGetValue);

      DoTestSet(columnName, "AsTimeSpan", setValue, wantedGetValue, wantedIsNull);
    }

    #endregion

    #region Guid

    public readonly GetTestData[] AsGuid_get_Tests = new GetTestData[] {
      new GetTestData("FGuid", Guid.Empty, TestTable.Row1.VGuid, TestTable.Row2.VGuid),
    };
    [TestCaseSource("AsGuid_get_Tests")]
    public void AsGuid_get(GetTestData data)
    {
      DoTestGet(data.ColumnName, "AsGuid", data.WantedValues);
    }

    [TestCase("FString", "{D39919A4-7336-4434-A7C4-0166EDFFEFDB}", "{D39919A4-7336-4434-A7C4-0166EDFFEFDB}", false)]
    [TestCase("FString", Creators.StrGuidEmpty, Creators.StrGuidEmpty, true)]
    [TestCase("FStringNN", Creators.StrGuidEmpty, Creators.StrGuidEmpty, false)]
    [TestCase("FGuid", "{D39919A4-7336-4434-A7C4-0166EDFFEFDB}", "{D39919A4-7336-4434-A7C4-0166EDFFEFDB}", false)]
    [TestCase("FGuid", Creators.StrGuidEmpty, Creators.StrGuidEmpty, true)]
    [TestCase("FBytes", "{D39919A4-7336-4434-A7C4-0166EDFFEFDB}", "{D39919A4-7336-4434-A7C4-0166EDFFEFDB}", false)]
    [TestCase("FBytes", Creators.StrGuidEmpty, Creators.StrGuidEmpty, true)]
    public void AsGuid_set(string columnName, string sSetValue, string sWantedGetValue, bool wantedIsNull)
    {
      Guid setValue = new Guid(sSetValue);
      Guid wantedGetValue = new Guid(sWantedGetValue);

      DoTestSet(columnName, "AsGuid", setValue, wantedGetValue, wantedIsNull);
    }

    #endregion

    #region Enum

    public readonly GetTestData[] GetEnum_Tests = new GetTestData[] {
      new GetTestData("FEnumInt32", TestEnum.Zero, TestTable.Row1.VEnum, TestTable.Row2.VEnum),
      new GetTestData("FEnumString", TestEnum.Zero, TestTable.Row1.VEnum, TestTable.Row2.VEnum),
    };
    [TestCaseSource("GetEnum_Tests")]
    public void GetEnum(GetTestData data)
    {
      DataTable tbl = TestTable.Create();
      if (data.WantedValues.Length != tbl.Rows.Count)
        throw ExceptionFactory.ArgWrongCollectionCount("wantedValues", data.WantedValues, 4);

      DataTableValues vals = new DataTableValues(tbl);
      DataColumnValue sut = vals[data.ColumnName];
      Assert.AreEqual(data.ColumnName, sut.ColumnName, "ColumnName");

      for (int i = 0; i < data.WantedValues.Length; i++)
      {
        if (!vals.Read())
          throw new BugException("Can't read table row");

        object value = sut.GetEnum<TestEnum>();
        Assert.AreEqual(data.WantedValues[i], value, "Row#" + (i + 1).ToString() + ", ColumnName=" + data.ColumnName);
      }
    }


    [TestCase("FString", TestEnum.One, TestEnum.One, false)]
    [TestCase("FString", TestEnum.Zero, TestEnum.Zero, true)]
    [TestCase("FStringNN", TestEnum.Zero, TestEnum.Zero, false)]
    [TestCase("FInt32", TestEnum.One, TestEnum.One, false)]
    [TestCase("FInt32", TestEnum.Zero, TestEnum.Zero, true)]
    [TestCase("FInt32NN", TestEnum.Zero, TestEnum.Zero, false)]
    public void SetEnum(string columnName, TestEnum setValue, TestEnum wantedGetValue, bool wantedIsNull)
    {
      DataTableValues vals = new DataTableValues(TestTable.Create());
      vals.CurrentRow = vals.Table.Rows[0]; // строка с DBNull
      DataColumnValue sut = vals[columnName];
      Assert.AreEqual(columnName, sut.ColumnName, "ColumnName");


      sut.SetEnum<TestEnum>(setValue);

      object res = sut.GetEnum<TestEnum>();
      Assert.AreEqual(wantedGetValue, res, "get value, ColumnName=" + columnName);

      Assert.AreEqual(wantedIsNull, sut.IsNull, "IsNull");
    }


    #endregion

    #endregion

    #region Инкремент значений

    #region IncInt32()

    [TestCase("FString", "", 123, 123)]
    [TestCase("FString", "123", 123, 246)]
    [TestCase("FString", "", 0, 0)]
    [TestCase("FString", "123", 0, 123)]
    [TestCase("FInt32", "-2", 3, 1)]
    [TestCase("FInt32", "", 3, 3)]
    [TestCase("FInt32NN", "", 3, 3)]
    [TestCase("FInt64", "5", -7, -2)]
    [TestCase("FInt64", "", -2, -2)]
    [TestCase("FSingle", "2.5", 3, 6)] // с округлением
    [TestCase("FDouble", "3.5", -3, 1)]
    [TestCase("FDecimal", "4.5", -5, 0)]
    public void IncInt32(string columnName, string sOrgValue, int delta, int wantedRes)
    {
      DataTable tbl = TestTable.Create();
      DataTableValues vals = new DataTableValues(tbl);
      vals.CurrentRow = tbl.Rows[0];
      DataColumnValue sut = vals[columnName];
      sut.AsString = sOrgValue;

      sut.IncInt32(delta);

      int res = sut.AsInt32;
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("FInt32", "", 1)]
    [TestCase("FInt32", "2", 3)]
    [TestCase("FInt32", "-1", 0)]
    public void IncInt32_By1(string columnName, string sOrgValue, int wantedRes)
    {
      DataTable tbl = TestTable.Create();
      DataTableValues vals = new DataTableValues(tbl);
      vals.CurrentRow = tbl.Rows[0];
      DataColumnValue sut = vals[columnName];
      sut.AsString = sOrgValue;

      sut.IncInt32();

      int res = sut.AsInt32;
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("FInt32", Int32.MaxValue, 1)]
    [TestCase("FInt32", Int32.MinValue, -1)]
    [TestCase("FInt64", Int32.MaxValue, 1)]
    [TestCase("FInt64", Int32.MinValue, -1)]
    [TestCase("FDecimal", Int32.MaxValue, 1)]
    [TestCase("FDecimal", Int32.MinValue, -1)]
    public void IncInt32_overflow(string columnName, int orgValue, int delta)
    {
      DataTable tbl = TestTable.Create();
      DataTableValues vals = new DataTableValues(tbl);
      vals.CurrentRow = tbl.Rows[0];
      DataColumnValue sut = vals[columnName];
      sut.AsInt32 = orgValue;

      Assert.Catch<OverflowException>(delegate () { sut.IncInt32(delta); }, "overflow");

      Assert.AreEqual(orgValue, sut.AsInt32, "do not changed");
    }

    #endregion

    #region IncInt64()

    [TestCase("FString", "", 123L, 123L)]
    [TestCase("FString", "123", 123L, 246L)]
    [TestCase("FString", "", 0L, 0L)]
    [TestCase("FString", "123", 0L, 123L)]
    [TestCase("FInt32", "-2", 3L, 1L)]
    [TestCase("FInt32", "", 3L, 3L)]
    [TestCase("FInt32NN", "", 3L, 3L)]
    [TestCase("FInt64", "5", -7L, -2L)]
    [TestCase("FInt64", "", -2L, -2L)]
    [TestCase("FSingle", "2.5", 3L, 6L)] // с округлением
    [TestCase("FDouble", "3.5", -3L, 1L)]
    [TestCase("FDecimal", "4.5", -5L, 0L)]
    public void IncInt64(string columnName, string sOrgValue, long delta, long wantedRes)
    {
      DataTable tbl = TestTable.Create();
      DataTableValues vals = new DataTableValues(tbl);
      vals.CurrentRow = tbl.Rows[0];
      DataColumnValue sut = vals[columnName];
      sut.AsString = sOrgValue;

      sut.IncInt64(delta);

      long res = sut.AsInt64;
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("FInt32", "", 1L)]
    [TestCase("FInt32", "2", 3L)]
    [TestCase("FInt32", "-1", 0L)]
    public void IncInt64_By1(string columnName, string sOrgValue, long wantedRes)
    {
      DataTable tbl = TestTable.Create();
      DataTableValues vals = new DataTableValues(tbl);
      vals.CurrentRow = tbl.Rows[0];
      DataColumnValue sut = vals[columnName];
      sut.AsString = sOrgValue;

      sut.IncInt64();

      long res = sut.AsInt64;
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("FInt32", Int32.MaxValue, 1L)]
    [TestCase("FInt32", Int32.MinValue, -1L)]
    [TestCase("FInt64", Int64.MaxValue, 1L)]
    [TestCase("FInt64", Int64.MinValue, -1L)]
    [TestCase("FDecimal", Int64.MaxValue, 1L)]
    [TestCase("FDecimal", Int64.MinValue, -1L)]
    public void IncInt64_overflow(string columnName, long orgValue, long delta)
    {
      DataTable tbl = TestTable.Create();
      DataTableValues vals = new DataTableValues(tbl);
      vals.CurrentRow = tbl.Rows[0];
      DataColumnValue sut = vals[columnName];
      sut.AsInt64 = orgValue;

      Assert.Catch<OverflowException>(delegate () { sut.IncInt64(delta); }, "overflow");

      Assert.AreEqual(orgValue, sut.AsInt64, "do not changed");
    }

    #endregion

    #region IncSingle()

    [TestCase("FString", "", 1.25f, 1.25f)]
    [TestCase("FString", "1.5", 1.5f, 3f)]
    [TestCase("FString", "", 0f, 0f)]
    [TestCase("FString", "-1.5", 0f, -1.5f)]
    [TestCase("FInt32", "-2", 2.5f, 1f)]
    [TestCase("FInt32", "", 3.5f, 4f)]
    [TestCase("FInt32NN", "", 3f, 3f)]
    [TestCase("FInt64", "5", -7f, -2f)]
    [TestCase("FInt64", "", -2.4f, -2f)]
    [TestCase("FSingle", "2.5", 3f, 5.5f)]
    [TestCase("FDouble", "3.5", -3.0f, 0.5f)]
    [TestCase("FDecimal", "4.5", -5f, -0.5f)]
    public void IncSingle(string columnName, string sOrgValue, float delta, float wantedRes)
    {
      DataTable tbl = TestTable.Create();
      DataTableValues vals = new DataTableValues(tbl);
      vals.CurrentRow = tbl.Rows[0];
      DataColumnValue sut = vals[columnName];
      sut.AsString = sOrgValue;

      sut.IncSingle(delta);

      float res = sut.AsSingle;
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region IncDouble()

    [TestCase("FString", "", 1.25, 1.25)]
    [TestCase("FString", "1.5", 1.5, 3.0)]
    [TestCase("FString", "", 0.0, 0.0)]
    [TestCase("FString", "-1.5", 0.0, -1.5)]
    [TestCase("FInt32", "-2", 2.5, 1.0)]
    [TestCase("FInt32", "", 3.5, 4.0)]
    [TestCase("FInt32NN", "", 3.0, 3.0)]
    [TestCase("FInt64", "5", -7.0, -2.0)]
    [TestCase("FInt64", "", -2.4, -2.0)]
    [TestCase("FSingle", "2.5", 3.0, 5.5)]
    [TestCase("FDouble", "3.5", -3.0, 0.5)]
    [TestCase("FDecimal", "4.5", -5.0, -0.5)]
    public void IncDouble(string columnName, string sOrgValue, double delta, double wantedRes)
    {
      DataTable tbl = TestTable.Create();
      DataTableValues vals = new DataTableValues(tbl);
      vals.CurrentRow = tbl.Rows[0];
      DataColumnValue sut = vals[columnName];
      sut.AsString = sOrgValue;

      sut.IncDouble(delta);

      double res = sut.AsDouble;
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region IncDecimal()

    [TestCase("FString", "", "1.25", "1.25")]
    [TestCase("FString", "1.5", "1.5", "3")]
    [TestCase("FString", "", "0", "0")]
    [TestCase("FString", "-1.5", "0", "-1.5")]
    [TestCase("FInt32", "-2", "2.5", "1")]
    [TestCase("FInt32", "", "3.5", "4")]
    [TestCase("FInt32NN", "", "3", "3")]
    [TestCase("FInt64", "5", "-7", "-2")]
    [TestCase("FInt64", "", "-2.4", "-2")]
    [TestCase("FSingle", "2.5", "3", "5.5")]
    [TestCase("FDouble", "3.5", "-3", "0.5")]
    [TestCase("FDecimal", "4.5", "-5", "-0.5")]
    public void IncDecimal(string columnName, string sOrgValue, string sDelta, string sWantedRes)
    {
      decimal delta = StdConvert.ToDecimal(sDelta);
      decimal wantedRes = StdConvert.ToDecimal(sWantedRes);

      DataTable tbl = TestTable.Create();
      DataTableValues vals = new DataTableValues(tbl);
      vals.CurrentRow = tbl.Rows[0];
      DataColumnValue sut = vals[columnName];
      sut.AsString = sOrgValue;

      sut.IncDecimal(delta);

      decimal res = sut.AsDecimal;
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #endregion
  }
}
