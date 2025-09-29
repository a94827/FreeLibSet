using FreeLibSet.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class DataToolsTests_AggregateFunctions
  {
    #region SumXXX

    #region DataTable

    [Test]
    public void SumXXX_DataTable()
    {
      DataTable tbl = TestTable.Create();

      Assert.AreEqual(TestTable.Row1.VInt32 + TestTable.Row2.VInt32, DataTools.SumInt32(tbl, "FInt32"), "SumInt32()");
      Assert.AreEqual(TestTable.Row1.VInt64 + TestTable.Row2.VInt64, DataTools.SumInt64(tbl, "FInt64"), "SumInt64()");
      Assert.AreEqual(TestTable.Row1.VSingle + TestTable.Row2.VSingle, DataTools.SumSingle(tbl, "FSingle"), "SumSingle()");
      Assert.AreEqual(TestTable.Row1.VDouble + TestTable.Row2.VDouble, DataTools.SumDouble(tbl, "FDouble"), "SumDouble()");
      Assert.AreEqual(TestTable.Row1.VDecimal + TestTable.Row2.VDecimal, DataTools.SumDecimal(tbl, "FDecimal"), "SumDecimal()");
      Assert.AreEqual(TestTable.Row1.VTimeSpan + TestTable.Row2.VTimeSpan, DataTools.SumTimeSpan(tbl, "FTimeSpan"), "SumTimeSpan()");
    }

    [Test]
    public void SumValue_DataTable()
    {
      DataTable tbl = TestTable.Create();

      Assert.AreEqual(TestTable.Row1.VInt32 + TestTable.Row2.VInt32, DataTools.SumValue(tbl, "FInt32"), "Int32");
      Assert.AreEqual(TestTable.Row1.VInt64 + TestTable.Row2.VInt64, DataTools.SumValue(tbl, "FInt64"), "Int64");
      Assert.AreEqual(TestTable.Row1.VSingle + TestTable.Row2.VSingle, DataTools.SumValue(tbl, "FSingle"), "Single");
      Assert.AreEqual(TestTable.Row1.VDouble + TestTable.Row2.VDouble, DataTools.SumValue(tbl, "FDouble"), "Double");
      Assert.AreEqual(TestTable.Row1.VDecimal + TestTable.Row2.VDecimal, DataTools.SumValue(tbl, "FDecimal"), "Decimal");
      Assert.AreEqual(TestTable.Row1.VTimeSpan + TestTable.Row2.VTimeSpan, DataTools.SumValue(tbl, "FTimeSpan"), "TimeSpan");
    }

    #endregion

    #region DataView

    [Test]
    public void SumXXX_DataView()
    {
      DataView dv = TestTable.Create().DefaultView;

      Assert.AreEqual(TestTable.Row1.VInt32 + TestTable.Row2.VInt32, DataTools.SumInt32(dv, "FInt32"), "SumInt32()");
      Assert.AreEqual(TestTable.Row1.VInt64 + TestTable.Row2.VInt64, DataTools.SumInt64(dv, "FInt64"), "SumInt64()");
      Assert.AreEqual(TestTable.Row1.VSingle + TestTable.Row2.VSingle, DataTools.SumSingle(dv, "FSingle"), "SumSingle()");
      Assert.AreEqual(TestTable.Row1.VDouble + TestTable.Row2.VDouble, DataTools.SumDouble(dv, "FDouble"), "SumDouble()");
      Assert.AreEqual(TestTable.Row1.VDecimal + TestTable.Row2.VDecimal, DataTools.SumDecimal(dv, "FDecimal"), "SumDecimal()");
      Assert.AreEqual(TestTable.Row1.VTimeSpan + TestTable.Row2.VTimeSpan, DataTools.SumTimeSpan(dv, "FTimeSpan"), "SumTimeSpan()");
    }

    [Test]
    public void SumValue_DataView()
    {
      DataView dv = TestTable.Create().DefaultView;

      Assert.AreEqual(TestTable.Row1.VInt32 + TestTable.Row2.VInt32, DataTools.SumValue(dv, "FInt32"), "Int32");
      Assert.AreEqual(TestTable.Row1.VInt64 + TestTable.Row2.VInt64, DataTools.SumValue(dv, "FInt64"), "Int64");
      Assert.AreEqual(TestTable.Row1.VSingle + TestTable.Row2.VSingle, DataTools.SumValue(dv, "FSingle"), "Single");
      Assert.AreEqual(TestTable.Row1.VDouble + TestTable.Row2.VDouble, DataTools.SumValue(dv, "FDouble"), "Double");
      Assert.AreEqual(TestTable.Row1.VDecimal + TestTable.Row2.VDecimal, DataTools.SumValue(dv, "FDecimal"), "Decimal");
      Assert.AreEqual(TestTable.Row1.VTimeSpan + TestTable.Row2.VTimeSpan, DataTools.SumValue(dv, "FTimeSpan"), "TimeSpan");
    }

    #endregion

    #region DataRowEnumerable

    [Test]
    public void SumXXX_DataRowEnumerable()
    {
      IEnumerable<DataRow> en = DataTools.GetDataTableRows(TestTable.Create());

      Assert.AreEqual(TestTable.Row1.VInt32 + TestTable.Row2.VInt32, DataTools.SumInt32(en, "FInt32"), "SumInt32()");
      Assert.AreEqual(TestTable.Row1.VInt64 + TestTable.Row2.VInt64, DataTools.SumInt64(en, "FInt64"), "SumInt64()");
      Assert.AreEqual(TestTable.Row1.VSingle + TestTable.Row2.VSingle, DataTools.SumSingle(en, "FSingle"), "SumSingle()");
      Assert.AreEqual(TestTable.Row1.VDouble + TestTable.Row2.VDouble, DataTools.SumDouble(en, "FDouble"), "SumDouble()");
      Assert.AreEqual(TestTable.Row1.VDecimal + TestTable.Row2.VDecimal, DataTools.SumDecimal(en, "FDecimal"), "SumDecimal()");
      Assert.AreEqual(TestTable.Row1.VTimeSpan + TestTable.Row2.VTimeSpan, DataTools.SumTimeSpan(en, "FTimeSpan"), "SumTimeSpan()");
    }

    [Test]
    public void SumValue_DataRowEnumerable()
    {
      IEnumerable<DataRow> en = DataTools.GetDataTableRows(TestTable.Create());

      Assert.AreEqual(TestTable.Row1.VInt32 + TestTable.Row2.VInt32, DataTools.SumValue(en, "FInt32"), "Int32");
      Assert.AreEqual(TestTable.Row1.VInt64 + TestTable.Row2.VInt64, DataTools.SumValue(en, "FInt64"), "Int64");
      Assert.AreEqual(TestTable.Row1.VSingle + TestTable.Row2.VSingle, DataTools.SumValue(en, "FSingle"), "Single");
      Assert.AreEqual(TestTable.Row1.VDouble + TestTable.Row2.VDouble, DataTools.SumValue(en, "FDouble"), "Double");
      Assert.AreEqual(TestTable.Row1.VDecimal + TestTable.Row2.VDecimal, DataTools.SumValue(en, "FDecimal"), "Decimal");
      Assert.AreEqual(TestTable.Row1.VTimeSpan + TestTable.Row2.VTimeSpan, DataTools.SumValue(en, "FTimeSpan"), "TimeSpan");
    }

    #endregion

    #region TotalRow

    [Test]
    public void SumXXX_TotalRow()
    {
      DataRow row = TestTable.Create().NewRow();

      DataTools.SumInt32(row, "FInt32");
      Assert.AreEqual(TestTable.Row1.VInt32 + TestTable.Row2.VInt32, row["FInt32"], "SumInt32()");

      DataTools.SumInt64(row, "FInt64");
      Assert.AreEqual(TestTable.Row1.VInt64 + TestTable.Row2.VInt64, row["FInt64"], "SumInt64()");

      DataTools.SumSingle(row, "FSingle");
      Assert.AreEqual(TestTable.Row1.VSingle + TestTable.Row2.VSingle, row["FSingle"], "SumSingle()");

      DataTools.SumDouble(row, "FDouble");
      Assert.AreEqual(TestTable.Row1.VDouble + TestTable.Row2.VDouble, row["FDouble"], "SumDouble()");

      DataTools.SumDecimal(row, "FDecimal");
      Assert.AreEqual(TestTable.Row1.VDecimal + TestTable.Row2.VDecimal, row["FDecimal"], "SumDecimal()");

      DataTools.SumTimeSpan(row, "FTimeSpan");
      Assert.AreEqual(TestTable.Row1.VTimeSpan + TestTable.Row2.VTimeSpan, row["FTimeSpan"], "SumTimeSpan()");
    }

    [Test]
    public void SumValue_TotalRow()
    {
      DataRow row = TestTable.Create().NewRow();

      DataTools.SumValue(row, "FInt32");
      Assert.AreEqual(TestTable.Row1.VInt32 + TestTable.Row2.VInt32, row["FInt32"], "Int32");

      DataTools.SumValue(row, "FInt64");
      Assert.AreEqual(TestTable.Row1.VInt64 + TestTable.Row2.VInt64, row["FInt64"], "Int64");

      DataTools.SumValue(row, "FSingle");
      Assert.AreEqual(TestTable.Row1.VSingle + TestTable.Row2.VSingle, row["FSingle"], "Single()");

      DataTools.SumValue(row, "FDouble");
      Assert.AreEqual(TestTable.Row1.VDouble + TestTable.Row2.VDouble, row["FDouble"], "Double()");

      DataTools.SumValue(row, "FDecimal");
      Assert.AreEqual(TestTable.Row1.VDecimal + TestTable.Row2.VDecimal, row["FDecimal"], "Decimal()");

      DataTools.SumValue(row, "FTimeSpan");
      Assert.AreEqual(TestTable.Row1.VTimeSpan + TestTable.Row2.VTimeSpan, row["FTimeSpan"], "TimeSpan()");
    }

    #endregion

    #region Enumerable

    [Test]
    public void SumInt32_Enumerable()
    {
      int[] a = new int[] { 1, 2, 3, 4 };
      Assert.AreEqual(10, DataTools.SumInt32(a));
    }

    [Test]
    public void SumInt64_Enumerable()
    {
      long[] a = new long[] { 1L, 2L, 3L, 4L };
      Assert.AreEqual(10L, DataTools.SumInt64(a));
    }

    [Test]
    public void SumSingle_Enumerable()
    {
      float[] a = new float[] { 1, 2, 3, 4 };
      Assert.AreEqual(10f, DataTools.SumSingle(a));
    }

    [Test]
    public void SumDouble_Enumerable()
    {
      double[] a = new double[] { 1, 2, 3, 4 };
      Assert.AreEqual(10.0, DataTools.SumDouble(a));
    }

    [Test]
    public void SumDecimal_Enumerable()
    {
      decimal[] a = new decimal[] { 1, 2, 3, 4 };
      Assert.AreEqual(10m, DataTools.SumDecimal(a));
    }

    [Test]
    public void SumTimeSpan_Enumerable()
    {
      TimeSpan[] a = new TimeSpan[] { new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0), new TimeSpan(3, 0, 0) };
      Assert.AreEqual(new TimeSpan(6, 0, 0), DataTools.SumTimeSpan(a));
    }

    [Test]
    public void SumValue_Enumerable()
    {
      object[] a1 = new object[] { 1, 2, 3, 4 };
      Assert.AreEqual(10, DataTools.SumValue(a1), "SumValue(IEnumerable)");

      object[,] a2 = new object[,] { { 1, 2 }, { 3, 4 } };
      Assert.AreEqual(10, DataTools.SumValue(a2), "SumValue([,])");

      object[][] a3 = new object[][] { new object[] { 1, 2 }, null, new object[] { 3, 4 } };
      Assert.AreEqual(10, DataTools.SumValue(a3), "SumValue([][])");

      // jagged-массив с переменной размерностью
      object[] a4 = new object[3];
      a4[0] = new object[,] { { 1, 2 }, { 3, 4 } };
      a4[1] = new object[] { 5 };
      a4[2] = new object[2];
      ((object[])a4[2])[1] = new object[] { 6, 7 };
      Assert.AreEqual(1 + 2 + 3 + 4 + 5 + 6 + 7, DataTools.SumValue(a4), "SumValue(...)");
    }

    #endregion

    #endregion

    #region Min

    #region DataTable

    [Test]
    public void MinXXX_DataTable_skipNulls()
    {
      DataTable tbl = TestTable.Create();

      Assert.AreEqual(TestTable.Row1.VInt32, DataTools.MinInt32(tbl, "FInt32", true), "MinInt32()");
      Assert.AreEqual(TestTable.Row1.VInt64, DataTools.MinInt64(tbl, "FInt64", true), "MinInt64()");
      Assert.AreEqual(TestTable.Row1.VSingle, DataTools.MinSingle(tbl, "FSingle", true), "MinSingle()");
      Assert.AreEqual(TestTable.Row1.VDouble, DataTools.MinDouble(tbl, "FDouble", true), "MinDouble()");
      Assert.AreEqual(TestTable.Row1.VDecimal, DataTools.MinDecimal(tbl, "FDecimal", true), "MinDecimal()");
      Assert.AreEqual(TestTable.Row1.VDateTime, DataTools.MinDateTime(tbl, "FDateTime"), "MinDateTime()");
      Assert.AreEqual(TestTable.Row1.VTimeSpan, DataTools.MinTimeSpan(tbl, "FTimeSpan", true), "MinTimeSpan()");
    }

    [Test]
    public void MinXXX_DataTable_withNulls()
    {
      DataTable tbl = TestTable.Create();

      Assert.AreEqual(0, DataTools.MinInt32(tbl, "FInt32", false), "MinInt32()");
      Assert.AreEqual(0L, DataTools.MinInt64(tbl, "FInt64", false), "MinInt64()");
      Assert.AreEqual(0f, DataTools.MinSingle(tbl, "FSingle", false), "MinSingle()");
      Assert.AreEqual(0.0, DataTools.MinDouble(tbl, "FDouble", false), "MinDouble()");
      Assert.AreEqual(0m, DataTools.MinDecimal(tbl, "FDecimal", false), "MinDecimal()");
      Assert.AreEqual(TestTable.Row1.VDateTime, DataTools.MinDateTime(tbl, "FDateTime"), "MinDateTime()"); // повтор
      Assert.AreEqual(TimeSpan.Zero, DataTools.MinTimeSpan(tbl, "FTimeSpan", false), "MinTimeSpan()");
    }

    [Test]
    public void MinValue_DataTable_skipNulls()
    {
      DataTable tbl = TestTable.Create();

      Assert.AreEqual(TestTable.Row1.VInt32, DataTools.MinValue(tbl, "FInt32", true), "Int32");
      Assert.AreEqual(TestTable.Row1.VInt64, DataTools.MinValue(tbl, "FInt64", true), "Int64");
      Assert.AreEqual(TestTable.Row1.VSingle, DataTools.MinValue(tbl, "FSingle", true), "Single");
      Assert.AreEqual(TestTable.Row1.VDouble, DataTools.MinValue(tbl, "FDouble", true), "Double");
      Assert.AreEqual(TestTable.Row1.VDecimal, DataTools.MinValue(tbl, "FDecimal", true), "Decimal");
      Assert.AreEqual(TestTable.Row1.VDateTime, DataTools.MinValue(tbl, "FDateTime", true), "DateTime");
      Assert.AreEqual(TestTable.Row1.VTimeSpan, DataTools.MinValue(tbl, "FTimeSpan", true), "TimeSpan");
    }

    [Test]
    public void MinValue_DataTable_withNulls()
    {
      DataTable tbl = TestTable.Create();

      Assert.AreEqual(0, DataTools.MinValue(tbl, "FInt32", false), "Int32");
      Assert.AreEqual(0L, DataTools.MinValue(tbl, "FInt64", false), "Int64");
      Assert.AreEqual(0f, DataTools.MinValue(tbl, "FSingle", false), "Single");
      Assert.AreEqual(0.0, DataTools.MinValue(tbl, "FDouble", false), "Double");
      Assert.AreEqual(0m, DataTools.MinValue(tbl, "FDecimal", false), "Decimal");
      Assert.AreEqual(TestTable.Row1.VDateTime, DataTools.MinValue(tbl, "FDateTime", false), "DateTime");
      Assert.AreEqual(TimeSpan.Zero, DataTools.MinTimeSpan(tbl, "FTimeSpan", false), "TimeSpan");
    }

    #endregion

    #region DataView

    [Test]
    public void MinXXX_DataView_skipNulls()
    {
      DataView dv = TestTable.Create().DefaultView;

      Assert.AreEqual(TestTable.Row1.VInt32, DataTools.MinInt32(dv, "FInt32", true), "MinInt32()");
      Assert.AreEqual(TestTable.Row1.VInt64, DataTools.MinInt64(dv, "FInt64", true), "MinInt64()");
      Assert.AreEqual(TestTable.Row1.VSingle, DataTools.MinSingle(dv, "FSingle", true), "MinSingle()");
      Assert.AreEqual(TestTable.Row1.VDouble, DataTools.MinDouble(dv, "FDouble", true), "MinDouble()");
      Assert.AreEqual(TestTable.Row1.VDecimal, DataTools.MinDecimal(dv, "FDecimal", true), "MinDecimal()");
      Assert.AreEqual(TestTable.Row1.VDateTime, DataTools.MinDateTime(dv, "FDateTime"), "MinDateTime()");
      Assert.AreEqual(TestTable.Row1.VTimeSpan, DataTools.MinTimeSpan(dv, "FTimeSpan", true), "MinTimeSpan()");
    }

    [Test]
    public void MinXXX_DataView_withNulls()
    {
      DataView dv = TestTable.Create().DefaultView;

      Assert.AreEqual(0, DataTools.MinInt32(dv, "FInt32", false), "MinInt32()");
      Assert.AreEqual(0L, DataTools.MinInt64(dv, "FInt64", false), "MinInt64()");
      Assert.AreEqual(0f, DataTools.MinSingle(dv, "FSingle", false), "MinSingle()");
      Assert.AreEqual(0.0, DataTools.MinDouble(dv, "FDouble", false), "MinDouble()");
      Assert.AreEqual(0m, DataTools.MinDecimal(dv, "FDecimal", false), "MinDecimal()");
      Assert.AreEqual(TestTable.Row1.VDateTime, DataTools.MinDateTime(dv, "FDateTime"), "MinDateTime()"); // повтор
      Assert.AreEqual(TimeSpan.Zero, DataTools.MinTimeSpan(dv, "FTimeSpan", false), "MinTimeSpan()");
    }


    [Test]
    public void MinValue_DataView_skipNulls()
    {
      DataView dv = TestTable.Create().DefaultView;

      Assert.AreEqual(TestTable.Row1.VInt32, DataTools.MinValue(dv, "FInt32", true), "Int32");
      Assert.AreEqual(TestTable.Row1.VInt64, DataTools.MinValue(dv, "FInt64", true), "Int64");
      Assert.AreEqual(TestTable.Row1.VSingle, DataTools.MinValue(dv, "FSingle", true), "Single");
      Assert.AreEqual(TestTable.Row1.VDouble, DataTools.MinValue(dv, "FDouble", true), "Double");
      Assert.AreEqual(TestTable.Row1.VDecimal, DataTools.MinValue(dv, "FDecimal", true), "Decimal");
      Assert.AreEqual(TestTable.Row1.VDateTime, DataTools.MinValue(dv, "FDateTime", true), "DateTime");
      Assert.AreEqual(TestTable.Row1.VTimeSpan, DataTools.MinValue(dv, "FTimeSpan", true), "TimeSpan");
    }

    [Test]
    public void MinValue_DataView_withNulls()
    {
      DataView dv = TestTable.Create().DefaultView;

      Assert.AreEqual(0, DataTools.MinValue(dv, "FInt32", false), "Int32");
      Assert.AreEqual(0L, DataTools.MinValue(dv, "FInt64", false), "Int64");
      Assert.AreEqual(0f, DataTools.MinValue(dv, "FSingle", false), "Single");
      Assert.AreEqual(0.0, DataTools.MinValue(dv, "FDouble", false), "Double");
      Assert.AreEqual(0m, DataTools.MinValue(dv, "FDecimal", false), "Decimal");
      Assert.AreEqual(TestTable.Row1.VDateTime, DataTools.MinValue(dv, "FDateTime", false), "DateTime");
      Assert.AreEqual(TimeSpan.Zero, DataTools.MinTimeSpan(dv, "FTimeSpan", false), "TimeSpan");
    }

    #endregion

    #region DataRowEnumerable

    [Test]
    public void MinXXX_DataRowEnumerable_skipNulls()
    {
      IEnumerable<DataRow> en = DataTools.GetDataTableRows(TestTable.Create());

      Assert.AreEqual(TestTable.Row1.VInt32, DataTools.MinInt32(en, "FInt32", true), "MinInt32()");
      Assert.AreEqual(TestTable.Row1.VInt64, DataTools.MinInt64(en, "FInt64", true), "MinInt64()");
      Assert.AreEqual(TestTable.Row1.VSingle, DataTools.MinSingle(en, "FSingle", true), "MinSingle()");
      Assert.AreEqual(TestTable.Row1.VDouble, DataTools.MinDouble(en, "FDouble", true), "MinDouble()");
      Assert.AreEqual(TestTable.Row1.VDecimal, DataTools.MinDecimal(en, "FDecimal", true), "MinDecimal()");
      Assert.AreEqual(TestTable.Row1.VDateTime, DataTools.MinDateTime(en, "FDateTime"), "MinDateTime()");
      Assert.AreEqual(TestTable.Row1.VTimeSpan, DataTools.MinTimeSpan(en, "FTimeSpan", true), "MinTimeSpan()");
    }

    [Test]
    public void MinXXX_DataRowEnumerable_withNulls()
    {
      IEnumerable<DataRow> en = DataTools.GetDataTableRows(TestTable.Create());

      Assert.AreEqual(0, DataTools.MinInt32(en, "FInt32", false), "MinInt32()");
      Assert.AreEqual(0L, DataTools.MinInt64(en, "FInt64", false), "MinInt64()");
      Assert.AreEqual(0f, DataTools.MinSingle(en, "FSingle", false), "MinSingle()");
      Assert.AreEqual(0.0, DataTools.MinDouble(en, "FDouble", false), "MinDouble()");
      Assert.AreEqual(0m, DataTools.MinDecimal(en, "FDecimal", false), "MinDecimal()");
      Assert.AreEqual(TestTable.Row1.VDateTime, DataTools.MinDateTime(en, "FDateTime"), "MinDateTime()"); // повтор
      Assert.AreEqual(TimeSpan.Zero, DataTools.MinTimeSpan(en, "FTimeSpan", false), "MinTimeSpan()");
    }

    [Test]
    public void MinValue_DataRowEnumerable_skipNulls()
    {
      IEnumerable<DataRow> en = DataTools.GetDataTableRows(TestTable.Create());

      Assert.AreEqual(TestTable.Row1.VInt32, DataTools.MinValue(en, "FInt32", true), "Int32");
      Assert.AreEqual(TestTable.Row1.VInt64, DataTools.MinValue(en, "FInt64", true), "Int64");
      Assert.AreEqual(TestTable.Row1.VSingle, DataTools.MinValue(en, "FSingle", true), "Single");
      Assert.AreEqual(TestTable.Row1.VDouble, DataTools.MinValue(en, "FDouble", true), "Double");
      Assert.AreEqual(TestTable.Row1.VDecimal, DataTools.MinValue(en, "FDecimal", true), "Decimal");
      Assert.AreEqual(TestTable.Row1.VDateTime, DataTools.MinValue(en, "FDateTime", true), "DateTime");
      Assert.AreEqual(TestTable.Row1.VTimeSpan, DataTools.MinValue(en, "FTimeSpan", true), "TimeSpan");
    }

    [Test]
    public void MinValue_DataRowEnumerable_withNulls()
    {
      IEnumerable<DataRow> en = DataTools.GetDataTableRows(TestTable.Create());

      Assert.AreEqual(0, DataTools.MinValue(en, "FInt32", false), "Int32");
      Assert.AreEqual(0L, DataTools.MinValue(en, "FInt64", false), "Int64");
      Assert.AreEqual(0f, DataTools.MinValue(en, "FSingle", false), "Single");
      Assert.AreEqual(0.0, DataTools.MinValue(en, "FDouble", false), "Double");
      Assert.AreEqual(0m, DataTools.MinValue(en, "FDecimal", false), "Decimal");
      Assert.AreEqual(TestTable.Row1.VDateTime, DataTools.MinValue(en, "FDateTime", false), "DateTime");
      Assert.AreEqual(TimeSpan.Zero, DataTools.MinTimeSpan(en, "FTimeSpan", false), "TimeSpan");
    }

    #endregion

    #region TotalRow

    [Test]
    public void MinXXX_TotalRow_skipNulls()
    {
      DataRow row = TestTable.Create().NewRow();

      DataTools.MinInt32(row, "FInt32", true);
      Assert.AreEqual(TestTable.Row1.VInt32, row["FInt32"], "MinInt32()");

      DataTools.MinInt64(row, "FInt64", true);
      Assert.AreEqual(TestTable.Row1.VInt64, row["FInt64"], "MinInt64()");

      DataTools.MinSingle(row, "FSingle", true);
      Assert.AreEqual(TestTable.Row1.VSingle, row["FSingle"], "MinSingle()");

      DataTools.MinDouble(row, "FDouble", true);
      Assert.AreEqual(TestTable.Row1.VDouble, row["FDouble"], "MinDouble()");

      DataTools.MinDecimal(row, "FDecimal", true);
      Assert.AreEqual(TestTable.Row1.VDecimal, row["FDecimal"], "MinDecimal()");

      DataTools.MinDateTime(row, "FDateTime");
      Assert.AreEqual(TestTable.Row1.VDateTime, row["FDateTime"], "MinDateTime()");

      DataTools.MinTimeSpan(row, "FTimeSpan", true);
      Assert.AreEqual(TestTable.Row1.VTimeSpan, row["FTimeSpan"], "MinTimeSpan()");
    }

    [Test]
    public void MinXXX_TotalRow_withNulls()
    {
      DataRow row = TestTable.Create().NewRow();

      DataTools.MinInt32(row, "FInt32", false);
      Assert.AreEqual(0, row["FInt32"], "MinInt32()");

      DataTools.MinInt64(row, "FInt64", false);
      Assert.AreEqual(0L, row["FInt64"], "MinInt64()");

      DataTools.MinSingle(row, "FSingle", false);
      Assert.AreEqual(0f, row["FSingle"], "MinSingle()");

      DataTools.MinDouble(row, "FDouble", false);
      Assert.AreEqual(0.0, row["FDouble"], "MinDouble()");

      DataTools.MinDecimal(row, "FDecimal", false);
      Assert.AreEqual(0m, row["FDecimal"], "MinDecimal()");

      DataTools.MinDateTime(row, "FDateTime");
      Assert.AreEqual(TestTable.Row1.VDateTime, row["FDateTime"], "MinDateTime()"); // повтор

      DataTools.MinTimeSpan(row, "FTimeSpan", false);
      Assert.AreEqual(TimeSpan.Zero, row["FTimeSpan"], "MinTimeSpan()");
    }

    [Test]
    public void MinValue_TotalRow_skipNulls()
    {
      DataRow row = TestTable.Create().NewRow();

      DataTools.MinValue(row, "FInt32", true);
      Assert.AreEqual(TestTable.Row1.VInt32, row["FInt32"], "Int32");

      DataTools.MinValue(row, "FInt64", true);
      Assert.AreEqual(TestTable.Row1.VInt64, row["FInt64"], "Int64");

      DataTools.MinValue(row, "FSingle", true);
      Assert.AreEqual(TestTable.Row1.VSingle, row["FSingle"], "Single");

      DataTools.MinValue(row, "FDouble", true);
      Assert.AreEqual(TestTable.Row1.VDouble, row["FDouble"], "Double");

      DataTools.MinValue(row, "FDecimal", true);
      Assert.AreEqual(TestTable.Row1.VDecimal, row["FDecimal"], "Decimal");

      DataTools.MinValue(row, "FDateTime", true);
      Assert.AreEqual(TestTable.Row1.VDateTime, row["FDateTime"], "DateTime");

      DataTools.MinValue(row, "FTimeSpan", true);
      Assert.AreEqual(TestTable.Row1.VTimeSpan, row["FTimeSpan"], "TimeSpan");
    }

    [Test]
    public void MinValue_TotalRow_withNulls()
    {
      DataRow row = TestTable.Create().NewRow();

      DataTools.MinValue(row, "FInt32", false);
      Assert.AreEqual(0, row["FInt32"], "Int32");

      DataTools.MinValue(row, "FInt64", false);
      Assert.AreEqual(0L, row["FInt64"], "Int64");

      DataTools.MinValue(row, "FSingle", false);
      Assert.AreEqual(0f, row["FSingle"], "Single");

      DataTools.MinValue(row, "FDouble", false);
      Assert.AreEqual(0.0, row["FDouble"], "Double");

      DataTools.MinValue(row, "FDecimal", false);
      Assert.AreEqual(0m, row["FDecimal"], "Decimal");

      DataTools.MinValue(row, "FDateTime", false);
      Assert.AreEqual(TestTable.Row1.VDateTime, row["FDateTime"], "DateTime");

      DataTools.MinTimeSpan(row, "FTimeSpan", false);
      Assert.AreEqual(TimeSpan.Zero, row["FTimeSpan"], "TimeSpan");
    }

    #endregion

    #region Enumerable

    [Test]
    public void MinInt32_Enumerable()
    {
      int[] a = new int[] { 1, 2, 3, 4 };
      Assert.AreEqual(1, DataTools.MinInt32(a));
    }

    [Test]
    public void MinInt64_Enumerable()
    {
      long[] a = new long[] { 1L, 2L, 3L, 4L };
      Assert.AreEqual(1L, DataTools.MinInt64(a));
    }

    [Test]
    public void MinSingle_Enumerable()
    {
      float[] a = new float[] { 1, 2, 3, 4 };
      Assert.AreEqual(1f, DataTools.MinSingle(a));
    }

    [Test]
    public void MinDouble_Enumerable()
    {
      double[] a = new double[] { 1, 2, 3, 4 };
      Assert.AreEqual(1.0, DataTools.MinDouble(a));
    }

    [Test]
    public void MinDecimal_Enumerable()
    {
      decimal[] a = new decimal[] { 1, 2, 3, 4 };
      Assert.AreEqual(1m, DataTools.MinDecimal(a));
    }

    [Test]
    public void MinDateTime_Enumerable()
    {
      DateTime[] a = new DateTime[] { new DateTime(2021, 12, 15), new DateTime(2020, 2, 29) };
      Assert.AreEqual(new DateTime(2020, 2, 29), DataTools.MinDateTime(a));
    }

    [Test]
    public void MinTimeSpan_Enumerable()
    {
      TimeSpan[] a = new TimeSpan[] { new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0), new TimeSpan(3, 0, 0) };
      Assert.AreEqual(new TimeSpan(1, 0, 0), DataTools.MinTimeSpan(a));
    }

    [Test]
    public void MinValue_Enumerable()
    {
      object[] a1 = new object[] { 1, 2, 3, 4 };
      Assert.AreEqual(1, DataTools.MinValue(a1), "MinValue(IEnumerable)");

      object[,] a2 = new object[,] { { 1, 2 }, { 3, 4 } };
      Assert.AreEqual(1, DataTools.MinValue(a2), "MinValue([,])");

      object[][] a3 = new object[][] { new object[] { 1, 2 }, null, new object[] { 3, 4 } };
      Assert.AreEqual(1, DataTools.MinValue(a3), "MinValue([][])");

      // jagged-массив с переменной размерностью
      object[] a4 = new object[3];
      a4[0] = new object[,] { { 1, 2 }, { 3, 4 } };
      a4[1] = new object[] { 5 };
      a4[2] = new object[2];
      ((object[])a4[2])[1] = new object[] { 6, 7 };
      Assert.AreEqual(1, DataTools.MinValue(a4), "MinValue(...)");
    }

    #endregion

    #endregion

    #region Max

    // В тестах функции MaxXXX() будут одинаковые результаты, независимо от аргумента skipNulls.

    #region DataTable

    [TestCase(true)]
    [TestCase(false)]
    public void MaxXXX_DataTable(bool skipNulls)
    {
      DataTable tbl = TestTable.Create();

      Assert.AreEqual(TestTable.Row2.VInt32, DataTools.MaxInt32(tbl, "FInt32", skipNulls), "MaxInt32()");
      Assert.AreEqual(TestTable.Row2.VInt64, DataTools.MaxInt64(tbl, "FInt64", skipNulls), "MaxInt64()");
      Assert.AreEqual(TestTable.Row2.VSingle, DataTools.MaxSingle(tbl, "FSingle", skipNulls), "MaxSingle()");
      Assert.AreEqual(TestTable.Row2.VDouble, DataTools.MaxDouble(tbl, "FDouble", skipNulls), "MaxDouble()");
      Assert.AreEqual(TestTable.Row2.VDecimal, DataTools.MaxDecimal(tbl, "FDecimal", skipNulls), "MaxDecimal()");
      Assert.AreEqual(TestTable.Row2.VDateTime, DataTools.MaxDateTime(tbl, "FDateTime"), "MaxDateTime()");
      Assert.AreEqual(TestTable.Row2.VTimeSpan, DataTools.MaxTimeSpan(tbl, "FTimeSpan", skipNulls), "MaxTimeSpan()");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void MaxValue_DataTable(bool skipNulls)
    {
      DataTable tbl = TestTable.Create();

      Assert.AreEqual(TestTable.Row2.VInt32, DataTools.MaxValue(tbl, "FInt32", skipNulls), "Int32");
      Assert.AreEqual(TestTable.Row2.VInt64, DataTools.MaxValue(tbl, "FInt64", skipNulls), "Int64");
      Assert.AreEqual(TestTable.Row2.VSingle, DataTools.MaxValue(tbl, "FSingle", skipNulls), "Single");
      Assert.AreEqual(TestTable.Row2.VDouble, DataTools.MaxValue(tbl, "FDouble", skipNulls), "Double");
      Assert.AreEqual(TestTable.Row2.VDecimal, DataTools.MaxValue(tbl, "FDecimal", skipNulls), "Decimal");
      Assert.AreEqual(TestTable.Row2.VDateTime, DataTools.MaxValue(tbl, "FDateTime", skipNulls), "DateTime");
      Assert.AreEqual(TestTable.Row2.VTimeSpan, DataTools.MaxValue(tbl, "FTimeSpan", skipNulls), "TimeSpan");
    }

    #endregion

    #region DataView

    [TestCase(true)]
    [TestCase(false)]
    public void MaxXXX_DataView(bool skipNulls)
    {
      DataView dv = TestTable.Create().DefaultView;

      Assert.AreEqual(TestTable.Row2.VInt32, DataTools.MaxInt32(dv, "FInt32", skipNulls), "MaxInt32()");
      Assert.AreEqual(TestTable.Row2.VInt64, DataTools.MaxInt64(dv, "FInt64", skipNulls), "MaxInt64()");
      Assert.AreEqual(TestTable.Row2.VSingle, DataTools.MaxSingle(dv, "FSingle", skipNulls), "MaxSingle()");
      Assert.AreEqual(TestTable.Row2.VDouble, DataTools.MaxDouble(dv, "FDouble", skipNulls), "MaxDouble()");
      Assert.AreEqual(TestTable.Row2.VDecimal, DataTools.MaxDecimal(dv, "FDecimal", skipNulls), "MaxDecimal()");
      Assert.AreEqual(TestTable.Row2.VDateTime, DataTools.MaxDateTime(dv, "FDateTime"), "MaxDateTime()");
      Assert.AreEqual(TestTable.Row2.VTimeSpan, DataTools.MaxTimeSpan(dv, "FTimeSpan", skipNulls), "MaxTimeSpan()");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void MaxValue_DataView(bool skipNulls)
    {
      DataView dv = TestTable.Create().DefaultView;

      Assert.AreEqual(TestTable.Row2.VInt32, DataTools.MaxValue(dv, "FInt32", skipNulls), "Int32");
      Assert.AreEqual(TestTable.Row2.VInt64, DataTools.MaxValue(dv, "FInt64", skipNulls), "Int64");
      Assert.AreEqual(TestTable.Row2.VSingle, DataTools.MaxValue(dv, "FSingle", skipNulls), "Single");
      Assert.AreEqual(TestTable.Row2.VDouble, DataTools.MaxValue(dv, "FDouble", skipNulls), "Double");
      Assert.AreEqual(TestTable.Row2.VDecimal, DataTools.MaxValue(dv, "FDecimal", skipNulls), "Decimal");
      Assert.AreEqual(TestTable.Row2.VDateTime, DataTools.MaxValue(dv, "FDateTime", skipNulls), "DateTime");
      Assert.AreEqual(TestTable.Row2.VTimeSpan, DataTools.MaxValue(dv, "FTimeSpan", skipNulls), "TimeSpan");
    }

    #endregion

    #region DataRowEnumerable

    [TestCase(true)]
    [TestCase(false)]
    public void MaxXXX_DataRowEnumerable(bool skipNulls)
    {
      IEnumerable<DataRow> en = DataTools.GetDataTableRows(TestTable.Create());

      Assert.AreEqual(TestTable.Row2.VInt32, DataTools.MaxInt32(en, "FInt32", skipNulls), "MaxInt32()");
      Assert.AreEqual(TestTable.Row2.VInt64, DataTools.MaxInt64(en, "FInt64", skipNulls), "MaxInt64()");
      Assert.AreEqual(TestTable.Row2.VSingle, DataTools.MaxSingle(en, "FSingle", skipNulls), "MaxSingle()");
      Assert.AreEqual(TestTable.Row2.VDouble, DataTools.MaxDouble(en, "FDouble", skipNulls), "MaxDouble()");
      Assert.AreEqual(TestTable.Row2.VDecimal, DataTools.MaxDecimal(en, "FDecimal", skipNulls), "MaxDecimal()");
      Assert.AreEqual(TestTable.Row2.VDateTime, DataTools.MaxDateTime(en, "FDateTime"), "MaxDateTime()");
      Assert.AreEqual(TestTable.Row2.VTimeSpan, DataTools.MaxTimeSpan(en, "FTimeSpan", skipNulls), "MaxTimeSpan()");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void MaxValue_DataRowEnumerable(bool skipNulls)
    {
      IEnumerable<DataRow> en = DataTools.GetDataTableRows(TestTable.Create());

      Assert.AreEqual(TestTable.Row2.VInt32, DataTools.MaxValue(en, "FInt32", skipNulls), "Int32");
      Assert.AreEqual(TestTable.Row2.VInt64, DataTools.MaxValue(en, "FInt64", skipNulls), "Int64");
      Assert.AreEqual(TestTable.Row2.VSingle, DataTools.MaxValue(en, "FSingle", skipNulls), "Single");
      Assert.AreEqual(TestTable.Row2.VDouble, DataTools.MaxValue(en, "FDouble", skipNulls), "Double");
      Assert.AreEqual(TestTable.Row2.VDecimal, DataTools.MaxValue(en, "FDecimal", skipNulls), "Decimal");
      Assert.AreEqual(TestTable.Row2.VDateTime, DataTools.MaxValue(en, "FDateTime", skipNulls), "DateTime");
      Assert.AreEqual(TestTable.Row2.VTimeSpan, DataTools.MaxValue(en, "FTimeSpan", skipNulls), "TimeSpan");
    }

    #endregion

    #region TotalRow

    [TestCase(true)]
    [TestCase(false)]
    public void MaxXXX_TotalRow(bool skipNulls)
    {
      DataRow row = TestTable.Create().NewRow();

      DataTools.MaxInt32(row, "FInt32", skipNulls);
      Assert.AreEqual(TestTable.Row2.VInt32, row["FInt32"], "MaxInt32()");

      DataTools.MaxInt64(row, "FInt64", skipNulls);
      Assert.AreEqual(TestTable.Row2.VInt64, row["FInt64"], "MaxInt64()");

      DataTools.MaxSingle(row, "FSingle", skipNulls);
      Assert.AreEqual(TestTable.Row2.VSingle, row["FSingle"], "MaxSingle()");

      DataTools.MaxDouble(row, "FDouble", skipNulls);
      Assert.AreEqual(TestTable.Row2.VDouble, row["FDouble"], "MaxDouble()");

      DataTools.MaxDecimal(row, "FDecimal", skipNulls);
      Assert.AreEqual(TestTable.Row2.VDecimal, row["FDecimal"], "MaxDecimal()");

      DataTools.MaxDateTime(row, "FDateTime");
      Assert.AreEqual(TestTable.Row2.VDateTime, row["FDateTime"], "MaxDateTime()");

      DataTools.MaxTimeSpan(row, "FTimeSpan", skipNulls);
      Assert.AreEqual(TestTable.Row2.VTimeSpan, row["FTimeSpan"], "MaxTimeSpan()");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void MaxValue_TotalRow(bool skipNulls)
    {
      DataRow row = TestTable.Create().NewRow();

      DataTools.MaxValue(row, "FInt32", skipNulls);
      Assert.AreEqual(TestTable.Row2.VInt32, row["FInt32"], "Int32");

      DataTools.MaxValue(row, "FInt64", skipNulls);
      Assert.AreEqual(TestTable.Row2.VInt64, row["FInt64"], "Int64");

      DataTools.MaxValue(row, "FSingle", skipNulls);
      Assert.AreEqual(TestTable.Row2.VSingle, row["FSingle"], "Single");

      DataTools.MaxValue(row, "FDouble", skipNulls);
      Assert.AreEqual(TestTable.Row2.VDouble, row["FDouble"], "Double");

      DataTools.MaxValue(row, "FDecimal", skipNulls);
      Assert.AreEqual(TestTable.Row2.VDecimal, row["FDecimal"], "Decimal");

      DataTools.MaxValue(row, "FDateTime", skipNulls);
      Assert.AreEqual(TestTable.Row2.VDateTime, row["FDateTime"], "DateTime");

      DataTools.MaxValue(row, "FTimeSpan", skipNulls);
      Assert.AreEqual(TestTable.Row2.VTimeSpan, row["FTimeSpan"], "TimeSpan");
    }

    #endregion

    #region Enumerable

    [Test]
    public void MaxInt32_Enumerable()
    {
      int[] a = new int[] { 1, 2, 3, 4 };
      Assert.AreEqual(4, DataTools.MaxInt32(a));
    }

    [Test]
    public void MaxInt64_Enumerable()
    {
      long[] a = new long[] { 1L, 2L, 3L, 4L };
      Assert.AreEqual(4L, DataTools.MaxInt64(a));
    }

    [Test]
    public void MaxSingle_Enumerable()
    {
      float[] a = new float[] { 1, 2, 3, 4 };
      Assert.AreEqual(4f, DataTools.MaxSingle(a));
    }

    [Test]
    public void MaxDouble_Enumerable()
    {
      double[] a = new double[] { 1, 2, 3, 4 };
      Assert.AreEqual(4.0, DataTools.MaxDouble(a));
    }

    [Test]
    public void MaxDecimal_Enumerable()
    {
      decimal[] a = new decimal[] { 1, 2, 3, 4 };
      Assert.AreEqual(4m, DataTools.MaxDecimal(a));
    }

    [Test]
    public void MaxDateTime_Enumerable()
    {
      DateTime[] a = new DateTime[] { new DateTime(2021, 12, 15), new DateTime(2020, 2, 29) };
      Assert.AreEqual(new DateTime(2021, 12, 15), DataTools.MaxDateTime(a));
    }

    [Test]
    public void MaxTimeSpan_Enumerable()
    {
      TimeSpan[] a = new TimeSpan[] { new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0), new TimeSpan(3, 0, 0) };
      Assert.AreEqual(new TimeSpan(3, 0, 0), DataTools.MaxTimeSpan(a));
    }

    [Test]
    public void MaxValue_Enumerable()
    {
      object[] a1 = new object[] { 1, 2, 3, 4 };
      Assert.AreEqual(4, DataTools.MaxValue(a1), "MaxValue(IEnumerable)");

      object[,] a2 = new object[,] { { 1, 2 }, { 3, 4 } };
      Assert.AreEqual(4, DataTools.MaxValue(a2), "MaxValue([,])");

      object[][] a3 = new object[][] { new object[] { 1, 2 }, null, new object[] { 3, 4 } };
      Assert.AreEqual(4, DataTools.MaxValue(a3), "MaxValue([][])");

      // jagged-массив с переменной размерностью
      object[] a4 = new object[3];
      a4[0] = new object[,] { { 1, 2 }, { 3, 4 } };
      a4[1] = new object[] { 5 };
      a4[2] = new object[2];
      ((object[])a4[2])[1] = new object[] { 6, 7 };
      Assert.AreEqual(7, DataTools.MaxValue(a4), "MaxValue(...)");
    }

    #endregion

    #endregion

    #region MinMax

    #region DataTable

    [Test]
    public void MinMaxXXX_DataTable_skipNulls()
    {
      DataTable tbl = TestTable.Create();

      Assert.AreEqual(new MinMax<Int32>(TestTable.Row1.VInt32, TestTable.Row2.VInt32), DataTools.MinMaxInt32(tbl, "FInt32", true), "MinInt32()");
      Assert.AreEqual(new MinMax<Int64>(TestTable.Row1.VInt64, TestTable.Row2.VInt64), DataTools.MinMaxInt64(tbl, "FInt64", true), "MinMaxInt64()");
      Assert.AreEqual(new MinMax<Single>(TestTable.Row1.VSingle, TestTable.Row2.VSingle), DataTools.MinMaxSingle(tbl, "FSingle", true), "MinMaxSingle()");
      Assert.AreEqual(new MinMax<Double>(TestTable.Row1.VDouble, TestTable.Row2.VDouble), DataTools.MinMaxDouble(tbl, "FDouble", true), "MinMaxDouble()");
      Assert.AreEqual(new MinMax<Decimal>(TestTable.Row1.VDecimal, TestTable.Row2.VDecimal), DataTools.MinMaxDecimal(tbl, "FDecimal", true), "MinMaxDecimal()");
      Assert.AreEqual(new MinMax<DateTime>(TestTable.Row1.VDateTime, TestTable.Row2.VDateTime), DataTools.MinMaxDateTime(tbl, "FDateTime"), "MinMaxDateTime()");
      Assert.AreEqual(new MinMax<TimeSpan>(TestTable.Row1.VTimeSpan, TestTable.Row2.VTimeSpan), DataTools.MinMaxTimeSpan(tbl, "FTimeSpan", true), "MinMaxTimeSpan()");
    }

    [Test]
    public void MinMaxXXX_DataTable_withNulls()
    {
      DataTable tbl = TestTable.Create();

      Assert.AreEqual(new MinMax<Int32>(0, TestTable.Row2.VInt32), DataTools.MinMaxInt32(tbl, "FInt32", false), "MinInt32()");
      Assert.AreEqual(new MinMax<Int64>(0, TestTable.Row2.VInt64), DataTools.MinMaxInt64(tbl, "FInt64", false), "MinMaxInt64()");
      Assert.AreEqual(new MinMax<Single>(0, TestTable.Row2.VSingle), DataTools.MinMaxSingle(tbl, "FSingle", false), "MinMaxSingle()");
      Assert.AreEqual(new MinMax<Double>(0, TestTable.Row2.VDouble), DataTools.MinMaxDouble(tbl, "FDouble", false), "MinMaxDouble()");
      Assert.AreEqual(new MinMax<Decimal>(0, TestTable.Row2.VDecimal), DataTools.MinMaxDecimal(tbl, "FDecimal", false), "MinMaxDecimal()");
      Assert.AreEqual(new MinMax<DateTime>(TestTable.Row1.VDateTime, TestTable.Row2.VDateTime), DataTools.MinMaxDateTime(tbl, "FDateTime"), "MinMaxDateTime()");
      Assert.AreEqual(new MinMax<TimeSpan>(TimeSpan.Zero, TestTable.Row2.VTimeSpan), DataTools.MinMaxTimeSpan(tbl, "FTimeSpan", false), "MinMaxTimeSpan()");
    }

    #endregion

    #region DataView

    [Test]
    public void MinMaxXXX_DataView_skipNulls()
    {
      DataView dv = TestTable.Create().DefaultView;

      Assert.AreEqual(new MinMax<Int32>(TestTable.Row1.VInt32, TestTable.Row2.VInt32), DataTools.MinMaxInt32(dv, "FInt32", true), "MinMaxInt32()");
      Assert.AreEqual(new MinMax<Int64>(TestTable.Row1.VInt64, TestTable.Row2.VInt64), DataTools.MinMaxInt64(dv, "FInt64", true), "MinMaxInt64()");
      Assert.AreEqual(new MinMax<Single>(TestTable.Row1.VSingle, TestTable.Row2.VSingle), DataTools.MinMaxSingle(dv, "FSingle", true), "MinMaxSingle()");
      Assert.AreEqual(new MinMax<Double>(TestTable.Row1.VDouble, TestTable.Row2.VDouble), DataTools.MinMaxDouble(dv, "FDouble", true), "MinMaxDouble()");
      Assert.AreEqual(new MinMax<Decimal>(TestTable.Row1.VDecimal, TestTable.Row2.VDecimal), DataTools.MinMaxDecimal(dv, "FDecimal", true), "MinMaxDecimal()");
      Assert.AreEqual(new MinMax<DateTime>(TestTable.Row1.VDateTime, TestTable.Row2.VDateTime), DataTools.MinMaxDateTime(dv, "FDateTime"), "MinMaxDateTime()");
      Assert.AreEqual(new MinMax<TimeSpan>(TestTable.Row1.VTimeSpan, TestTable.Row2.VTimeSpan), DataTools.MinMaxTimeSpan(dv, "FTimeSpan", true), "MinMaxTimeSpan()");
    }

    [Test]
    public void MinMaxXXX_DataView_withNulls()
    {
      DataView dv = TestTable.Create().DefaultView;

      Assert.AreEqual(new MinMax<Int32>(0, TestTable.Row2.VInt32), DataTools.MinMaxInt32(dv, "FInt32", false), "MinMaxInt32()");
      Assert.AreEqual(new MinMax<Int64>(0, TestTable.Row2.VInt64), DataTools.MinMaxInt64(dv, "FInt64", false), "MinMaxInt64()");
      Assert.AreEqual(new MinMax<Single>(0, TestTable.Row2.VSingle), DataTools.MinMaxSingle(dv, "FSingle", false), "MinMaxSingle()");
      Assert.AreEqual(new MinMax<Double>(0, TestTable.Row2.VDouble), DataTools.MinMaxDouble(dv, "FDouble", false), "MinMaxDouble()");
      Assert.AreEqual(new MinMax<Decimal>(0, TestTable.Row2.VDecimal), DataTools.MinMaxDecimal(dv, "FDecimal", false), "MinMaxDecimal()");
      Assert.AreEqual(new MinMax<DateTime>(TestTable.Row1.VDateTime, TestTable.Row2.VDateTime), DataTools.MinMaxDateTime(dv, "FDateTime"), "MinMaxDateTime()");
      Assert.AreEqual(new MinMax<TimeSpan>(TimeSpan.Zero, TestTable.Row2.VTimeSpan), DataTools.MinMaxTimeSpan(dv, "FTimeSpan", false), "MinMaxTimeSpan()");
    }

    #endregion

    #region DataRowEnumerable

    [Test]
    public void MinMaxXXX_DataRowEnumerable_skipNulls()
    {
      IEnumerable<DataRow> en = DataTools.GetDataTableRows(TestTable.Create());

      Assert.AreEqual(new MinMax<Int32>(TestTable.Row1.VInt32, TestTable.Row2.VInt32), DataTools.MinMaxInt32(en, "FInt32", true), "MinMaxInt32()");
      Assert.AreEqual(new MinMax<Int64>(TestTable.Row1.VInt64, TestTable.Row2.VInt64), DataTools.MinMaxInt64(en, "FInt64", true), "MinMaxInt64()");
      Assert.AreEqual(new MinMax<Single>(TestTable.Row1.VSingle, TestTable.Row2.VSingle), DataTools.MinMaxSingle(en, "FSingle", true), "MinMaxSingle()");
      Assert.AreEqual(new MinMax<Double>(TestTable.Row1.VDouble, TestTable.Row2.VDouble), DataTools.MinMaxDouble(en, "FDouble", true), "MinMaxDouble()");
      Assert.AreEqual(new MinMax<Decimal>(TestTable.Row1.VDecimal, TestTable.Row2.VDecimal), DataTools.MinMaxDecimal(en, "FDecimal", true), "MinMaxDecimal()");
      Assert.AreEqual(new MinMax<DateTime>(TestTable.Row1.VDateTime, TestTable.Row2.VDateTime), DataTools.MinMaxDateTime(en, "FDateTime"), "MinMaxDateTime()");
      Assert.AreEqual(new MinMax<TimeSpan>(TestTable.Row1.VTimeSpan, TestTable.Row2.VTimeSpan), DataTools.MinMaxTimeSpan(en, "FTimeSpan", true), "MinMaxTimeSpan()");
    }

    [Test]
    public void MinMaxXXX_DataRowEnumerable_withNulls()
    {
      IEnumerable<DataRow> en = DataTools.GetDataTableRows(TestTable.Create());

      Assert.AreEqual(new MinMax<Int32>(0, TestTable.Row2.VInt32), DataTools.MinMaxInt32(en, "FInt32", false), "MinInt32()");
      Assert.AreEqual(new MinMax<Int64>(0, TestTable.Row2.VInt64), DataTools.MinMaxInt64(en, "FInt64", false), "MinMaxInt64()");
      Assert.AreEqual(new MinMax<Single>(0, TestTable.Row2.VSingle), DataTools.MinMaxSingle(en, "FSingle", false), "MinMaxSingle()");
      Assert.AreEqual(new MinMax<Double>(0, TestTable.Row2.VDouble), DataTools.MinMaxDouble(en, "FDouble", false), "MinMaxDouble()");
      Assert.AreEqual(new MinMax<Decimal>(0, TestTable.Row2.VDecimal), DataTools.MinMaxDecimal(en, "FDecimal", false), "MinMaxDecimal()");
      Assert.AreEqual(new MinMax<DateTime>(TestTable.Row1.VDateTime, TestTable.Row2.VDateTime), DataTools.MinMaxDateTime(en, "FDateTime"), "MinMaxDateTime()");
      Assert.AreEqual(new MinMax<TimeSpan>(TimeSpan.Zero, TestTable.Row2.VTimeSpan), DataTools.MinMaxTimeSpan(en, "FTimeSpan", false), "MinMaxTimeSpan()");
    }

    #endregion

    #endregion

    #region Average

    // В текущей реализации нет среднего по DateTime, хотя его можно было бы сделать.

    #region DataTable

    [Test]
    public void AverageXXX_DataTable_skipNulls()
    {
      DataTable tbl = TestTable.Create();

      Assert.AreEqual(TestTable.AvgRes2.VInt32, DataTools.AverageInt32(tbl, "FInt32", true), "AverageInt()");
      Assert.AreEqual(TestTable.AvgRes2.VInt64, DataTools.AverageInt64(tbl, "FInt64", true), "AverageInt64()");
      Assert.AreEqual(TestTable.AvgRes2.VSingle, DataTools.AverageSingle(tbl, "FSingle", true), "AverageSingle()");
      Assert.AreEqual(TestTable.AvgRes2.VDouble, DataTools.AverageDouble(tbl, "FDouble", true), "AverageDouble()");
      Assert.AreEqual(TestTable.AvgRes2.VDecimal, DataTools.AverageDecimal(tbl, "FDecimal", true), "AverageDecimal()");
      //Assert.AreEqual(TestTable.AvgRes2.VDateTime, DataTools.AverageDateTime(tbl, "FDateTime"), "AverageDateTime()");
      Assert.AreEqual(TestTable.AvgRes2.VTimeSpan, DataTools.AverageTimeSpan(tbl, "FTimeSpan", true), "AverageTimeSpan()");
    }

    [Test]
    public void AverageXXX_DataTable_withNulls()
    {
      DataTable tbl = TestTable.Create();

      Assert.AreEqual(TestTable.AvgRes3.VInt32, DataTools.AverageInt32(tbl, "FInt32", false), "AverageInt()");
      Assert.AreEqual(TestTable.AvgRes3.VInt64, DataTools.AverageInt64(tbl, "FInt64", false), "AverageInt64()");
      Assert.AreEqual(TestTable.AvgRes3.VSingle, DataTools.AverageSingle(tbl, "FSingle", false), "AverageSingle()");
      Assert.AreEqual(TestTable.AvgRes3.VDouble, DataTools.AverageDouble(tbl, "FDouble", false), "AverageDouble()");
      Assert.AreEqual(TestTable.AvgRes3.VDecimal, DataTools.AverageDecimal(tbl, "FDecimal", false), "AverageDecimal()");
      //Assert.AreEqual(TestTable.AvgRes3.VDateTime, DataTools.AverageDateTime(tbl, "FDateTime"), "AverageDateTime()");
      Assert.AreEqual(TestTable.AvgRes3.VTimeSpan, DataTools.AverageTimeSpan(tbl, "FTimeSpan", false), "AverageTimeSpan()");
    }

    [Test]
    public void AverageValue_DataTable_skipNulls()
    {
      DataTable tbl = TestTable.Create();

      Assert.AreEqual(TestTable.AvgRes2.VInt32, DataTools.AverageValue(tbl, "FInt32", true), "Int32");
      Assert.AreEqual(TestTable.AvgRes2.VInt64, DataTools.AverageValue(tbl, "FInt64", true), "Int64");
      Assert.AreEqual(TestTable.AvgRes2.VSingle, DataTools.AverageValue(tbl, "FSingle", true), "Single");
      Assert.AreEqual(TestTable.AvgRes2.VDouble, DataTools.AverageValue(tbl, "FDouble", true), "Double");
      Assert.AreEqual(TestTable.AvgRes2.VDecimal, DataTools.AverageValue(tbl, "FDecimal", true), "Decimal");
      //Assert.AreEqual(TestTable.AvgRes2.VDateTime, DataTools.AverageValue(tbl, "FDateTime"), "DateTime");
      Assert.AreEqual(TestTable.AvgRes2.VTimeSpan, DataTools.AverageValue(tbl, "FTimeSpan", true), "TimeSpan");
    }

    [Test]
    public void AverageValue_DataTable_withNulls()
    {
      DataTable tbl = TestTable.Create();

      Assert.AreEqual(TestTable.AvgRes3.VInt32, DataTools.AverageValue(tbl, "FInt32", false), "Int32");
      Assert.AreEqual(TestTable.AvgRes3.VInt64, DataTools.AverageValue(tbl, "FInt64", false), "Int64");
      Assert.AreEqual(TestTable.AvgRes3.VSingle, DataTools.AverageValue(tbl, "FSingle", false), "Single");
      Assert.AreEqual(TestTable.AvgRes3.VDouble, DataTools.AverageValue(tbl, "FDouble", false), "Double");
      Assert.AreEqual(TestTable.AvgRes3.VDecimal, DataTools.AverageValue(tbl, "FDecimal", false), "Decimal");
      //Assert.AreEqual(TestTable.AvgRes3.VDateTime, DataTools.AverageValue(tbl, "FDateTime"), "DateTime");
      Assert.AreEqual(TestTable.AvgRes3.VTimeSpan, DataTools.AverageValue(tbl, "FTimeSpan", false), "TimeSpan");
    }

    #endregion

    #region DataView

    [Test]
    public void AverageXXX_DataView_skipNulls()
    {
      DataView dv = TestTable.Create().DefaultView;

      Assert.AreEqual(TestTable.AvgRes2.VInt32, DataTools.AverageInt32(dv, "FInt32", true), "AverageInt()");
      Assert.AreEqual(TestTable.AvgRes2.VInt64, DataTools.AverageInt64(dv, "FInt64", true), "AverageInt64()");
      Assert.AreEqual(TestTable.AvgRes2.VSingle, DataTools.AverageSingle(dv, "FSingle", true), "AverageSingle()");
      Assert.AreEqual(TestTable.AvgRes2.VDouble, DataTools.AverageDouble(dv, "FDouble", true), "AverageDouble()");
      Assert.AreEqual(TestTable.AvgRes2.VDecimal, DataTools.AverageDecimal(dv, "FDecimal", true), "AverageDecimal()");
      //Assert.AreEqual(TestTable.AvgRes2.VDateTime, DataTools.AverageDateTime(tbl, "FDateTime"), "AverageDateTime()");
      Assert.AreEqual(TestTable.AvgRes2.VTimeSpan, DataTools.AverageTimeSpan(dv, "FTimeSpan", true), "AverageTimeSpan()");
    }

    [Test]
    public void AverageXXX_DataView_withNulls()
    {
      DataView dv = TestTable.Create().DefaultView;

      Assert.AreEqual(TestTable.AvgRes3.VInt32, DataTools.AverageInt32(dv, "FInt32", false), "AverageInt()");
      Assert.AreEqual(TestTable.AvgRes3.VInt64, DataTools.AverageInt64(dv, "FInt64", false), "AverageInt64()");
      Assert.AreEqual(TestTable.AvgRes3.VSingle, DataTools.AverageSingle(dv, "FSingle", false), "AverageSingle()");
      Assert.AreEqual(TestTable.AvgRes3.VDouble, DataTools.AverageDouble(dv, "FDouble", false), "AverageDouble()");
      Assert.AreEqual(TestTable.AvgRes3.VDecimal, DataTools.AverageDecimal(dv, "FDecimal", false), "AverageDecimal()");
      //Assert.AreEqual(TestTable.AvgRes3.VDateTime, DataTools.AverageDateTime(tbl, "FDateTime"), "AverageDateTime()");
      Assert.AreEqual(TestTable.AvgRes3.VTimeSpan, DataTools.AverageTimeSpan(dv, "FTimeSpan", false), "AverageTimeSpan()");
    }

    [Test]
    public void AverageValue_DataView_skipNulls()
    {
      DataView dv = TestTable.Create().DefaultView;

      Assert.AreEqual(TestTable.AvgRes2.VInt32, DataTools.AverageValue(dv, "FInt32", true), "Int32");
      Assert.AreEqual(TestTable.AvgRes2.VInt64, DataTools.AverageValue(dv, "FInt64", true), "Int64");
      Assert.AreEqual(TestTable.AvgRes2.VSingle, DataTools.AverageValue(dv, "FSingle", true), "Single");
      Assert.AreEqual(TestTable.AvgRes2.VDouble, DataTools.AverageValue(dv, "FDouble", true), "Double");
      Assert.AreEqual(TestTable.AvgRes2.VDecimal, DataTools.AverageValue(dv, "FDecimal", true), "Decimal");
      //Assert.AreEqual(TestTable.AvgRes2.VDateTime, DataTools.AverageValue(tbl, "FDateTime"), "DateTime");
      Assert.AreEqual(TestTable.AvgRes2.VTimeSpan, DataTools.AverageValue(dv, "FTimeSpan", true), "TimeSpan");
    }

    [Test]
    public void AverageValue_DataView_withNulls()
    {
      DataView dv = TestTable.Create().DefaultView;

      Assert.AreEqual(TestTable.AvgRes3.VInt32, DataTools.AverageValue(dv, "FInt32", false), "Int32");
      Assert.AreEqual(TestTable.AvgRes3.VInt64, DataTools.AverageValue(dv, "FInt64", false), "Int64");
      Assert.AreEqual(TestTable.AvgRes3.VSingle, DataTools.AverageValue(dv, "FSingle", false), "Single");
      Assert.AreEqual(TestTable.AvgRes3.VDouble, DataTools.AverageValue(dv, "FDouble", false), "Double");
      Assert.AreEqual(TestTable.AvgRes3.VDecimal, DataTools.AverageValue(dv, "FDecimal", false), "Decimal");
      //Assert.AreEqual(TestTable.AvgRes3.VDateTime, DataTools.AverageValue(tbl, "FDateTime"), "DateTime");
      Assert.AreEqual(TestTable.AvgRes3.VTimeSpan, DataTools.AverageValue(dv, "FTimeSpan", false), "TimeSpan");
    }

    #endregion

    #region EnumerableDataRow

    [Test]
    public void AverageXXX_EnumerableDataRow_skipNulls()
    {
      IEnumerable<DataRow> en = DataTools.GetDataTableRows(TestTable.Create());

      Assert.AreEqual(TestTable.AvgRes2.VInt32, DataTools.AverageInt32(en, "FInt32", true), "AverageInt()");
      Assert.AreEqual(TestTable.AvgRes2.VInt64, DataTools.AverageInt64(en, "FInt64", true), "AverageInt64()");
      Assert.AreEqual(TestTable.AvgRes2.VSingle, DataTools.AverageSingle(en, "FSingle", true), "AverageSingle()");
      Assert.AreEqual(TestTable.AvgRes2.VDouble, DataTools.AverageDouble(en, "FDouble", true), "AverageDouble()");
      Assert.AreEqual(TestTable.AvgRes2.VDecimal, DataTools.AverageDecimal(en, "FDecimal", true), "AverageDecimal()");
      //Assert.AreEqual(TestTable.AvgRes2.VDateTime, DataTools.AverageDateTime(tbl, "FDateTime"), "AverageDateTime()");
      Assert.AreEqual(TestTable.AvgRes2.VTimeSpan, DataTools.AverageTimeSpan(en, "FTimeSpan", true), "AverageTimeSpan()");
    }

    [Test]
    public void AverageXXX_EnumerableDataRow_withNulls()
    {
      IEnumerable<DataRow> en = DataTools.GetDataTableRows(TestTable.Create());

      Assert.AreEqual(TestTable.AvgRes3.VInt32, DataTools.AverageInt32(en, "FInt32", false), "AverageInt32()");
      Assert.AreEqual(TestTable.AvgRes3.VInt64, DataTools.AverageInt64(en, "FInt64", false), "AverageInt64()");
      Assert.AreEqual(TestTable.AvgRes3.VSingle, DataTools.AverageSingle(en, "FSingle", false), "AverageSingle()");
      Assert.AreEqual(TestTable.AvgRes3.VDouble, DataTools.AverageDouble(en, "FDouble", false), "AverageDouble()");
      Assert.AreEqual(TestTable.AvgRes3.VDecimal, DataTools.AverageDecimal(en, "FDecimal", false), "AverageDecimal()");
      //Assert.AreEqual(TestTable.AvgRes3.VDateTime, DataTools.AverageDateTime(tbl, "FDateTime"), "AverageDateTime()");
      Assert.AreEqual(TestTable.AvgRes3.VTimeSpan, DataTools.AverageTimeSpan(en, "FTimeSpan", false), "AverageTimeSpan()");
    }

    [Test]
    public void AverageValue_EnumerableDataRow_skipNulls()
    {
      IEnumerable<DataRow> en = DataTools.GetDataTableRows(TestTable.Create());

      Assert.AreEqual(TestTable.AvgRes2.VInt32, DataTools.AverageValue(en, "FInt32", true), "Int32");
      Assert.AreEqual(TestTable.AvgRes2.VInt64, DataTools.AverageValue(en, "FInt64", true), "Int64");
      Assert.AreEqual(TestTable.AvgRes2.VSingle, DataTools.AverageValue(en, "FSingle", true), "Single");
      Assert.AreEqual(TestTable.AvgRes2.VDouble, DataTools.AverageValue(en, "FDouble", true), "Double");
      Assert.AreEqual(TestTable.AvgRes2.VDecimal, DataTools.AverageValue(en, "FDecimal", true), "Decimal");
      //Assert.AreEqual(TestTable.AvgRes2.VDateTime, DataTools.AverageValue(tbl, "FDateTime"), "DateTime");
      Assert.AreEqual(TestTable.AvgRes2.VTimeSpan, DataTools.AverageValue(en, "FTimeSpan", true), "TimeSpan");
    }

    [Test]
    public void AverageValue_EnumerableDataRow_withNulls()
    {
      IEnumerable<DataRow> en = DataTools.GetDataTableRows(TestTable.Create());

      Assert.AreEqual(TestTable.AvgRes3.VInt32, DataTools.AverageValue(en, "FInt32", false), "Int32");
      Assert.AreEqual(TestTable.AvgRes3.VInt64, DataTools.AverageValue(en, "FInt64", false), "Int64");
      Assert.AreEqual(TestTable.AvgRes3.VSingle, DataTools.AverageValue(en, "FSingle", false), "Single");
      Assert.AreEqual(TestTable.AvgRes3.VDouble, DataTools.AverageValue(en, "FDouble", false), "Double");
      Assert.AreEqual(TestTable.AvgRes3.VDecimal, DataTools.AverageValue(en, "FDecimal", false), "Decimal");
      //Assert.AreEqual(TestTable.AvgRes3.VDateTime, DataTools.AverageValue(tbl, "FDateTime"), "DateTime");
      Assert.AreEqual(TestTable.AvgRes3.VTimeSpan, DataTools.AverageValue(en, "FTimeSpan", false), "TimeSpan");
    }

    #endregion

    #region TotalRow

    [Test]
    public void AverageXXX_TotalRow_skipNulls()
    {
      DataRow row = TestTable.Create().NewRow();

      DataTools.AverageInt32(row, "FInt32", true);
      Assert.AreEqual(TestTable.AvgRes2.VInt32, row["FInt32"], "AverageInt32()");

      DataTools.AverageInt64(row, "FInt64", true);
      Assert.AreEqual(TestTable.AvgRes2.VInt64, row["FInt64"], "AverageInt64()");

      DataTools.AverageSingle(row, "FSingle", true);
      Assert.AreEqual(TestTable.AvgRes2.VSingle, row["FSingle"], "AverageSingle()");

      DataTools.AverageDouble(row, "FDouble", true);
      Assert.AreEqual(TestTable.AvgRes2.VDouble, row["FDouble"], "AverageDouble()");

      DataTools.AverageDecimal(row, "FDecimal", true);
      Assert.AreEqual(TestTable.AvgRes2.VDecimal, row["FDecimal"], "AverageDecimal()");

      //DataTools.AverageDateTime(row, "FDateTime");
      //Assert.AreEqual(TestTable.AvgRes2.VDateTime, row["FDateTime"], "AverageDateTime()");

      DataTools.AverageTimeSpan(row, "FTimeSpan", true);
      Assert.AreEqual(TestTable.AvgRes2.VTimeSpan, row["FTimeSpan"], "AverageTimeSpan()");
    }

    [Test]
    public void AverageXXX_TotalRow_withNulls()
    {
      DataRow row = TestTable.Create().NewRow();

      DataTools.AverageInt32(row, "FInt32", false);
      Assert.AreEqual(TestTable.AvgRes3.VInt32, row["FInt32"], "AverageInt()");

      DataTools.AverageInt64(row, "FInt64", false);
      Assert.AreEqual(TestTable.AvgRes3.VInt64, row["FInt64"], "AverageInt64()");

      DataTools.AverageSingle(row, "FSingle", false);
      Assert.AreEqual(TestTable.AvgRes3.VSingle, row["FSingle"], "AverageSingle()");

      DataTools.AverageDouble(row, "FDouble", false);
      Assert.AreEqual(TestTable.AvgRes3.VDouble, row["FDouble"], "AverageDouble()");

      DataTools.AverageDecimal(row, "FDecimal", false);
      Assert.AreEqual(TestTable.AvgRes3.VDecimal, row["FDecimal"], "AverageDecimal()");

      //DataTools.AverageDateTime(row, "FDateTime");
      //Assert.AreEqual(TestTable.AvgRes3.VDateTime, row["FDateTime"], "AverageDateTime()"); 

      DataTools.AverageTimeSpan(row, "FTimeSpan", false);
      Assert.AreEqual(TestTable.AvgRes3.VTimeSpan, row["FTimeSpan"], "AverageTimeSpan()");
    }

    [Test]
    public void AverageValue_TotalRow_skipNulls()
    {
      DataRow row = TestTable.Create().NewRow();

      DataTools.AverageValue(row, "FInt32", true);
      Assert.AreEqual(TestTable.AvgRes2.VInt32, row["FInt32"], "Int32");

      DataTools.AverageValue(row, "FInt64", true);
      Assert.AreEqual(TestTable.AvgRes2.VInt64, row["FInt64"], "Int64");

      DataTools.AverageValue(row, "FSingle", true);
      Assert.AreEqual(TestTable.AvgRes2.VSingle, row["FSingle"], "Single");

      DataTools.AverageValue(row, "FDouble", true);
      Assert.AreEqual(TestTable.AvgRes2.VDouble, row["FDouble"], "Double");

      DataTools.AverageValue(row, "FDecimal", true);
      Assert.AreEqual(TestTable.AvgRes2.VDecimal, row["FDecimal"], "Decimal");

      //DataTools.AverageValue(row, "FDateTime", true);
      //Assert.AreEqual(TestTable.AvgRes2.VDateTime, row["FDateTime"], "DateTime");

      DataTools.AverageValue(row, "FTimeSpan", true);
      Assert.AreEqual(TestTable.AvgRes2.VTimeSpan, row["FTimeSpan"], "TimeSpan");
    }

    [Test]
    public void AverageValue_TotalRow_withNulls()
    {
      DataRow row = TestTable.Create().NewRow();

      DataTools.AverageValue(row, "FInt32", false);
      Assert.AreEqual(TestTable.AvgRes3.VInt32, row["FInt32"], "Int32");

      DataTools.AverageValue(row, "FInt64", false);
      Assert.AreEqual(TestTable.AvgRes3.VInt64, row["FInt64"], "Int64");

      DataTools.AverageValue(row, "FSingle", false);
      Assert.AreEqual(TestTable.AvgRes3.VSingle, row["FSingle"], "Single");

      DataTools.AverageValue(row, "FDouble", false);
      Assert.AreEqual(TestTable.AvgRes3.VDouble, row["FDouble"], "Double");

      DataTools.AverageValue(row, "FDecimal", false);
      Assert.AreEqual(TestTable.AvgRes3.VDecimal, row["FDecimal"], "Decimal");

      //DataTools.AverageValue(row, "FDateTime", false);
      //Assert.AreEqual(TestTable.AvgRes3.VDateTime, row["FDateTime"], "DateTime");

      DataTools.AverageTimeSpan(row, "FTimeSpan", false);
      Assert.AreEqual(TestTable.AvgRes3.VTimeSpan, row["FTimeSpan"], "TimeSpan");
    }

    #endregion

    #region Enumerable

    [Test]
    public void AverageInt32_Enumerable()
    {
      int[] a = new int[] { 1, 2, 3, 4 };
      Assert.AreEqual(3, DataTools.AverageInt32(a));
    }

    [Test]
    public void AverageInt64_Enumerable()
    {
      long[] a = new long[] { 1L, 2L, 3L, 4L };
      Assert.AreEqual(3L, DataTools.AverageInt64(a));
    }

    [Test]
    public void AverageSingle_Enumerable()
    {
      float[] a = new float[] { 1, 2, 3, 4 };
      Assert.AreEqual(2.5f, DataTools.AverageSingle(a));
    }

    [Test]
    public void AverageDouble_Enumerable()
    {
      double[] a = new double[] { 1, 2, 3, 4 };
      Assert.AreEqual(2.5, DataTools.AverageDouble(a));
    }

    [Test]
    public void AverageDecimal_Enumerable()
    {
      decimal[] a = new decimal[] { 1, 2, 3, 4 };
      Assert.AreEqual(2.5m, DataTools.AverageDecimal(a));
    }

    //[Test]
    //public void AverageDateTime_Enumerable()
    //{
    //  DateTime[] a = new DateTime[] { new DateTime(2021, 12, 15), new DateTime(2020, 2, 29) };
    //  Assert.AreEqual(new DateTime(2020, 2, 29), DataTools.AverageDateTime(a));
    //}

    [Test]
    public void AverageTimeSpan_Enumerable()
    {
      TimeSpan[] a = new TimeSpan[] { new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0), new TimeSpan(3, 0, 0) };
      Assert.AreEqual(new TimeSpan(2, 0, 0), DataTools.AverageTimeSpan(a));
    }

    [Test]
    public void AverageValue_Enumerable()
    {
      object[] a1 = new object[] { 1, 2, 3, 4 };
      Assert.AreEqual(3, DataTools.AverageValue(a1), "AverageValue(IEnumerable)");

      object[,] a2 = new object[,] { { 1, 2 }, { 3, 4 } };
      Assert.AreEqual(3, DataTools.AverageValue(a2), "AverageValue([,])");

      object[][] a3 = new object[][] { new object[] { 1, 2 }, null, new object[] { 3, 4 } };
      Assert.AreEqual(3, DataTools.AverageValue(a3), "AverageValue([][])");

      // jagged-массив с переменной размерностью
      object[] a4 = new object[3];
      a4[0] = new object[,] { { 1, 2 }, { 3, 4 } };
      a4[1] = new object[] { 5 };
      a4[2] = new object[2];
      ((object[])a4[2])[1] = new object[] { 6, 7 };

      int wanted = 4; // (1+2+3+4+5+6+7)/7
      Assert.AreEqual(wanted, DataTools.AverageValue(a4), "AverageValue(...)");
    }

    #endregion

    #endregion
  }
}
