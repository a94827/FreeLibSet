using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using NUnit.Framework;
using FreeLibSet.Core;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class DataToolsTests_DataRow
  {
    #region SetRowState()

    private DataRow CreateRowWithRowState(DataTable table, DataRowState state)
    {
      DataRow row = table.Rows.Add("AAA");

      #region ѕодготовка состо€ни€ строки

      switch (state)
      {
        case DataRowState.Added:
          break;
        case DataRowState.Modified:
          table.AcceptChanges();
          row["Text"] = "BBB";
          break;
        case DataRowState.Deleted:
          table.AcceptChanges();
          row.Delete();
          break;
        case DataRowState.Unchanged:
          table.AcceptChanges();
          break;
        case DataRowState.Detached:
          table.AcceptChanges();
          table.Rows.Remove(row);
          break;
      }

      #endregion

      Assert.AreEqual(state, row.RowState, "Original state");

      return row;
    }

    [Test]
    public void SetRowState(
      [Values(
        DataRowState.Unchanged,
        DataRowState.Added,
        DataRowState.Modified,
        DataRowState.Deleted)] DataRowState oldState,
      [Values(
        DataRowState.Unchanged,
        DataRowState.Added,
        DataRowState.Modified,
        DataRowState.Deleted)] DataRowState newState)
    {
      DataTable table = new DataTable();
      table.Columns.Add("Text", typeof(string)); // непосредственно в тесте не используетс€
      DataRow row = CreateRowWithRowState(table, oldState);

      DataTools.SetRowState(row, newState);

      Assert.AreEqual(newState, row.RowState, "New state");
    }

    #endregion

    #region GetRowValues()

    [TestCase(DataRowState.Unchanged, DataRowVersion.Current, "AAA")]
    [TestCase(DataRowState.Added, DataRowVersion.Current, "AAA")]
    [TestCase(DataRowState.Modified, DataRowVersion.Original, "AAA")]
    [TestCase(DataRowState.Modified, DataRowVersion.Current, "BBB")]
    [TestCase(DataRowState.Deleted, DataRowVersion.Original, "AAA")]
    public void GetRowValues(DataRowState state, DataRowVersion rowVersion, string result)
    {
      DataTable table = new DataTable();
      table.Columns.Add("Text", typeof(string)); // непосредственно в тесте не используетс€
      DataRow row = CreateRowWithRowState(table, state);

      object[] res1 = DataTools.GetRowValues(row, rowVersion);
      Assert.AreEqual(1, res1.Length);
      Assert.AreEqual(result, DataTools.GetString(res1[0]));

      IDictionary<string, object> res2 = DataTools.GetRowValueDictionary(row, rowVersion);
      Assert.AreEqual(1, res2.Count);
      Assert.AreEqual(result, DataTools.GetString(res2["Text"]));
    }

    #endregion

    #region GetXXX()

    [Test]
    public void GetXXX_NotNull()
    {
      DataTable tbl = Creators.CreateTestDataTable();
      DataRow row = tbl.Rows[1];

      Assert.AreEqual(Creators.Row1.VString, DataTools.GetString(row, "FString"), "GetString()");
      Assert.AreEqual(Creators.Row1.VInt32, DataTools.GetInt(row, "FInt32"), "GetInt()");
      Assert.AreEqual(Creators.Row1.VInt64, DataTools.GetInt64(row, "FInt64"), "GetInt64()");
      Assert.AreEqual(Creators.Row1.VSingle, DataTools.GetSingle(row, "FSingle"), "GetSingle()");
      Assert.AreEqual(Creators.Row1.VDouble, DataTools.GetDouble(row, "FDouble"), "GetDouble()");
      Assert.AreEqual(Creators.Row1.VDecimal, DataTools.GetDecimal(row, "FDecimal"), "GetDecimal()");
      Assert.AreEqual(Creators.Row1.VBool, DataTools.GetBool(row, "FBoolean"), "GetBoolean");
      Assert.AreEqual(Creators.Row1.VDateTime, DataTools.GetDateTime(row, "FDateTime"), "GetDateTime()");
      Assert.AreEqual(Creators.Row1.VTimeSpan, DataTools.GetTimeSpan(row, "FTimeSpan"), "GetTimeSpan()");
      Assert.AreEqual(Creators.Row1.VGuid, DataTools.GetGuid(row, "FGuid"), "GetGuid()");
      Assert.AreEqual(Creators.Row1.VEnum, DataTools.GetEnum<Creators.TestEnum>(row, "FEnum"), "GetEnum<>()");
    }

    [Test]
    public void GetXXX_DBNull()
    {
      DataTable tbl = Creators.CreateTestDataTable();
      DataRow row = tbl.Rows[0];

      Assert.AreEqual("", DataTools.GetString(row, "FString"), "GetString()");
      Assert.AreEqual(0, DataTools.GetInt(row, "FInt32"), "GetInt()");
      Assert.AreEqual(0L, DataTools.GetInt64(row, "FInt64"), "GetInt64()");
      Assert.AreEqual(0f, DataTools.GetSingle(row, "FSingle"), "GetSingle()");
      Assert.AreEqual(0.0, DataTools.GetDouble(row, "FDouble"), "GetDouble()");
      Assert.AreEqual(0m, DataTools.GetDecimal(row, "FDecimal"), "GetDecimal()");
      Assert.AreEqual(false, DataTools.GetBool(row, "FBoolean"), "GetBoolean");
      Assert.AreEqual(DateTime.MinValue, DataTools.GetDateTime(row, "FDateTime"), "GetDateTime()");
      Assert.AreEqual(TimeSpan.Zero, DataTools.GetTimeSpan(row, "FTimeSpan"), "GetTimeSpan()");
      Assert.AreEqual(Guid.Empty, DataTools.GetGuid(row, "FGuid"), "GetGuid()");
      Assert.AreEqual(Creators.TestEnum.Zero, DataTools.GetEnum<Creators.TestEnum>(row, "FEnum"), "GetEnum<>()");
    }

    [Test]
    public void GetNullableXXX_NotNull()
    {
      DataTable tbl = Creators.CreateTestDataTable();
      DataRow row = tbl.Rows[1];

      // ѕока дл€ DataRow есть только GetNullableDateTime

      // Assert.AreEqual(1, DataTools.GetNullableInt(row, "FInt32"), "GetNullableInt()");
      //Assert.AreEqual(1L, DataTools.GetNullableInt64(row, "FInt64"), "GetNullableInt64()");
      //Assert.AreEqual(1f, DataTools.GetNullableSingle(row, "FSingle"), "GetNullableSingle()");
      //Assert.AreEqual(1.0, DataTools.GetNullableDouble(row, "FDouble"), "GetNullableDouble()");
      //Assert.AreEqual(1m, DataTools.GetNullableDecimal(row, "FDecimal"), "GetNullableDecimal()");
      Assert.AreEqual(Creators.Row1.VDateTime, DataTools.GetNullableDateTime(row, "FDateTime"), "GetNullableDateTime()");
      //Assert.AreEqual(new TimeSpan(1, 2, 3), DataTools.GetGetNullableTimeSpan(row, "FTimeSpan"), "GetNullableTimeSpan()");
      //Assert.AreEqual(TestGuid, DataTools.GetGetNullableGuid(row, "FGuid"), "GetNullableGuid()");
      //Assert.AreEqual(TestEnum.Two, DataTools.GetGetNullableEnum<TestEnum>(row, "FEnum"), "GetNullableEnum<>()");
    }

    [Test]
    public void GetNullableXXX_DBNull()
    {
      DataTable tbl = Creators.CreateTestDataTable();
      DataRow row = tbl.Rows[0];

      // ѕока дл€ DataRow есть только GetNullableDateTime

      // Assert.IsNull(DataTools.GetNullableInt(row, "FInt32"), "GetNullableInt()");
      //Assert.IsNull(DataTools.GetNullableInt64(row, "FInt64"), "GetNullableInt64()");
      //Assert.IsNull(DataTools.GetNullableSingle(row, "FSingle"), "GetNullableSingle()");
      //Assert.IsNull(DataTools.GetNullableDouble(row, "FDouble"), "GetNullableDouble()");
      //Assert.IsNull(DataTools.GetNullableDecimal(row, "FDecimal"), "GetNullableDecimal()");
      Assert.IsNull(DataTools.GetNullableDateTime(row, "FDateTime"), "GetNullableDateTime()");
      //Assert.IsNull(DataTools.GetGetNullableTimeSpan(row, "FTimeSpan"), "GetNullableTimeSpan()");
      //Assert.IsNull(DataTools.GetGetNullableGuid(row, "FGuid"), "GetNullableGuid()");
      //Assert.IsNull(DataTools.GetGetNullableEnum<TestEnum>(row, "FEnum"), "GetNullableEnum<>()");
    }

    #endregion

    #region SetXXX()

    [Test]
    public void SetXXX_NotZero()
    {
      DataTable tbl = Creators.CreateTestDataTable();
      DataRow row = tbl.Rows[1];

      DataTools.SetString(row, "FString", "XXX");
      Assert.AreEqual("XXX", row["FString"], "SetString()");

      DataTools.SetInt(row, "FInt32", 201);
      Assert.AreEqual(201, row["FInt32"], "SetInt()");

      DataTools.SetSingle(row, "FSingle", 202f);
      Assert.AreEqual(202f, row["FSingle"], "SetSingle()");

      DataTools.SetDouble(row, "FDouble", 203.0);
      Assert.AreEqual(203.0, row["FDouble"], "SetDouble()");

      DataTools.SetDecimal(row, "FDecimal", 204m);
      Assert.AreEqual(204m, row["FDecimal"], "SetDecimal()");

      DataTools.SetNullableDateTime(row, "FDateTime", new DateTime(2020, 2, 29));
      Assert.AreEqual(new DateTime(2020, 2, 29), row["FDateTime"], "SetNullableDateTime()");

      DataTools.SetTimeSpan(row, "FTimeSpan", new TimeSpan(6, 6, 6, 6));
      Assert.AreEqual(new TimeSpan(6, 6, 6, 6), row["FTimeSpan"], "SetTimeSpan()");

      Guid g = Guid.NewGuid();
      DataTools.SetGuid(row, "FGuid", g);
      Assert.AreEqual(g, row["FGuid"], "SetGuid()");
    }

    [Test]
    public void SetXXX_Zero()
    {
      DataTable tbl = Creators.CreateTestDataTable();
      DataRow row = tbl.Rows[1];

      DataTools.SetString(row, "FString", String.Empty);
      Assert.IsTrue(row.IsNull("FString"), "SetString()");

      DataTools.SetInt(row, "FInt32", 0);
      Assert.IsTrue(row.IsNull("FInt32"), "SetInt()");

      DataTools.SetSingle(row, "FSingle", 0f);
      Assert.IsTrue(row.IsNull("FSingle"), "SetSingle()");

      DataTools.SetDouble(row, "FDouble", 0.0);
      Assert.IsTrue(row.IsNull("FDouble"), "SetDouble()");

      DataTools.SetDecimal(row, "FDecimal", 0m);
      Assert.IsTrue(row.IsNull("FDecimal"), "SetDecimal()");

      DataTools.SetNullableDateTime(row, "FDateTime", null);
      Assert.IsTrue(row.IsNull("FDateTime"), "SetNullableDateTime()");

      DataTools.SetTimeSpan(row, "FTimeSpan", TimeSpan.Zero);
      Assert.IsTrue(row.IsNull("FTimeSpan"), "SetTimeSpan()");

      DataTools.SetGuid(row, "FGuid", Guid.Empty);
      Assert.IsTrue(row.IsNull("FGuid"), "SetGuid()");
    }

    #endregion

    #region IncXXX()

    [Test]
    public void IncXXX_delta_NotNull()
    {
      DataTable tbl = Creators.CreateTestDataTable();
      DataRow row = tbl.Rows[1];

      DataTools.IncInt(row, "FInt32", 1);
      Assert.AreEqual(Creators.Row1.VInt32 + 1, row["FInt32"], "IncInt()");

      DataTools.IncInt64(row, "FInt64", 1L);
      Assert.AreEqual(Creators.Row1.VInt64 + 1L, row["FInt64"], "IncInt64()");

      DataTools.IncSingle(row, "FSingle", 1f);
      Assert.AreEqual(Creators.Row1.VSingle + 1f, row["FSingle"], "IncSingle()");

      DataTools.IncDouble(row, "FDouble", 1.0);
      Assert.AreEqual(Creators.Row1.VDouble + 1.0, row["FDouble"], "IncDouble()");

      DataTools.IncDecimal(row, "FDecimal", 1m);
      Assert.AreEqual(Creators.Row1.VDecimal + 1m, row["FDecimal"], "IncDecimal()");

      DataTools.IncTimeSpan(row, "FTimeSpan", new TimeSpan(1, 0, 0));
      Assert.AreEqual(Creators.Row1.VTimeSpan + new TimeSpan(1, 0, 0), row["FTimeSpan"], "IncTimeSpan()");
    }

    [Test]
    public void IncXXX_delta_DBNull()
    {
      DataTable tbl = Creators.CreateTestDataTable();
      DataRow row = tbl.Rows[0];

      DataTools.IncInt(row, "FInt32", 1);
      Assert.AreEqual(1, row["FInt32"], "IncInt()");

      DataTools.IncInt64(row, "FInt64", 1);
      Assert.AreEqual(1L, row["FInt64"], "IncInt64()");

      DataTools.IncSingle(row, "FSingle", 1f);
      Assert.AreEqual(1.0f, row["FSingle"], "IncSingle()");

      DataTools.IncDouble(row, "FDouble", 1.0);
      Assert.AreEqual(1.0, row["FDouble"], "IncDouble()");

      DataTools.IncDecimal(row, "FDecimal", 1m);
      Assert.AreEqual(1.0m, row["FDecimal"], "IncDecimal()");

      DataTools.IncTimeSpan(row, "FTimeSpan", new TimeSpan(1, 0, 0));
      Assert.AreEqual(new TimeSpan(1, 0, 0), row["FTimeSpan"], "IncTimeSpan()");
    }

    [Test]
    public void IncXXX_RowRow()
    {
      DataTable tbl = Creators.CreateTestDataTable();
      DataRow row = tbl.Rows[1];
      DataRow row2 = tbl.Rows[2];

      DataTools.IncInt(row2, row, "FInt32");
      Assert.AreEqual(Creators.Row1.VInt32 + Creators.Row2.VInt32, row["FInt32"], "IncInt()");

      DataTools.IncInt64(row2, row, "FInt64");
      Assert.AreEqual(Creators.Row1.VInt64 + Creators.Row2.VInt64, row["FInt64"], "IncInt64()");

      DataTools.IncSingle(row2, row, "FSingle");
      Assert.AreEqual(Creators.Row1.VSingle + Creators.Row2.VSingle, row["FSingle"], "IncSingle()");

      DataTools.IncDouble(row2, row, "FDouble");
      Assert.AreEqual(Creators.Row1.VDouble + Creators.Row2.VDouble, row["FDouble"], "IncDouble()");

      DataTools.IncDecimal(row2, row, "FDecimal");
      Assert.AreEqual(Creators.Row1.VDecimal + Creators.Row2.VDecimal, row["FDecimal"], "IncDecimal()");

      DataTools.IncTimeSpan(row2, row, "FTimeSpan");
      Assert.AreEqual(Creators.Row1.VTimeSpan + Creators.Row2.VTimeSpan, row["FTimeSpan"], "IncTimeSpan()");
    }

    [Test]
    public void IncValue_delta()
    {
      DataTable tbl = Creators.CreateTestDataTable();
      DataRow row = tbl.Rows[1];

      DataTools.IncValue(row, "FInt32", 1);
      Assert.AreEqual(Creators.Row1.VInt32 + 1, row["FInt32"], "Int32");

      DataTools.IncValue(row, "FInt64", 1L);
      Assert.AreEqual(Creators.Row1.VInt64 + 1L, row["FInt64"], "Int64");

      DataTools.IncValue(row, "FSingle", 1f);
      Assert.AreEqual(Creators.Row1.VSingle + 1f, row["FSingle"], "Single");

      DataTools.IncValue(row, "FDouble", 1.0);
      Assert.AreEqual(Creators.Row1.VDouble + 1.0, row["FDouble"], "Double");

      DataTools.IncValue(row, "FDecimal", 1m);
      Assert.AreEqual(Creators.Row1.VDecimal + 1m, row["FDecimal"], "Decimal");

      DataTools.IncValue(row, "FTimeSpan", new TimeSpan(1, 0, 0));
      Assert.AreEqual(Creators.Row1.VTimeSpan + new TimeSpan(1, 0, 0), row["FTimeSpan"], "TimeSpan");
    }

    [Test]
    public void IncValue_RowRow()
    {
      DataTable tbl = Creators.CreateTestDataTable();
      DataRow row = tbl.Rows[1];
      DataRow row2 = tbl.Rows[2];

      DataTools.IncValue(row2, row, "FInt32");
      Assert.AreEqual(Creators.Row1.VInt32 + Creators.Row2.VInt32, row["FInt32"], "Int32");

      DataTools.IncValue(row2, row, "FInt64");
      Assert.AreEqual(Creators.Row1.VInt64 + Creators.Row2.VInt64, row["FInt64"], "Int64");

      DataTools.IncValue(row2, row, "FSingle");
      Assert.AreEqual(Creators.Row1.VSingle + Creators.Row2.VSingle, row["FSingle"], "Single");

      DataTools.IncValue(row2, row, "FDouble");
      Assert.AreEqual(Creators.Row1.VDouble + Creators.Row2.VDouble, row["FDouble"], "Double");

      DataTools.IncValue(row2, row, "FDecimal");
      Assert.AreEqual(Creators.Row1.VDecimal + Creators.Row2.VDecimal, row["FDecimal"], "Decimal");

      DataTools.IncValue(row2, row, "FTimeSpan");
      Assert.AreEqual(Creators.Row1.VTimeSpan + Creators.Row2.VTimeSpan, row["FTimeSpan"], "TimeSpan");
    }

    #endregion

    #region GetValues()

    [Test]
    public void GetValues()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(string));
      tbl.Columns.Add("F2", typeof(int));
      tbl.Columns.Add("F3", typeof(bool));
      DataRow row = tbl.Rows.Add("AAA", DBNull.Value, true);

      object[] res = DataTools.GetValues(row, "F3,F2,F1");
      Assert.AreEqual(new object[] { true, null, "AAA" }, res);
    }

    #endregion
  }
}
