using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using NUnit.Framework;
using FreeLibSet.Core;
using FreeLibSet.Tests;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class DataToolsTests_DataRow
  {
    #region SetRowState()

    private DataRow CreateRowWithRowState(DataTable table, DataRowState state)
    {
      DataRow row = table.Rows.Add("AAA");

      #region Подготовка состояния строки

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
      table.Columns.Add("Text", typeof(string)); // непосредственно в тесте не используется
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
      table.Columns.Add("Text", typeof(string)); // непосредственно в тесте не используется
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

    [TestCase("FString", 0, "")]
    [TestCase("FString", 1, TestTable.Row1.VString)]
    [TestCase("FStringNN", 0, "")]
    [TestCase("FStringNN", 1, TestTable.Row1.VString)]
    [TestCase("FInt32", 0, "")]
    [TestCase("FInt32", 1, TestTable.Row1.StrInt32)]
    [TestCase("FInt32NN", 0, "0")]
    [TestCase("FInt32NN", 1, TestTable.Row1.StrInt32)]
    [TestCase("FInt64", 0, "")]
    [TestCase("FInt64", 1, TestTable.Row1.StrInt64)]
    [TestCase("FSingle", 0, "")]
    [TestCase("FSingle", 1, TestTable.Row1.StrSingle)]
    [TestCase("FDouble", 0, "")]
    [TestCase("FDouble", 1, TestTable.Row1.StrDouble)]
    [TestCase("FDecimal", 0, "")]
    [TestCase("FDecimal", 1, TestTable.Row1.StrDecimal)]
    [TestCase("FGuid", 0, "")]
    [TestCase("FGuid", 1, TestTable.Row1.StrGuid)]
    public void GetString(string columnName, int rowIndex, string wantedValue)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetString(row, columnName));
    }

    [TestCase("FString", 0, 0)]
    [TestCase("FStringNN", 0, 0)]
    [TestCase("FInt32", 0, 0)]
    [TestCase("FInt32", 1, TestTable.Row1.VInt32)]
    [TestCase("FInt32NN", 0, 0)]
    [TestCase("FInt32NN", 1, TestTable.Row1.VInt32)]
    [TestCase("FInt64", 0, 0)]
    [TestCase("FInt64", 1, (int)(TestTable.Row1.VInt64))]
    // Не тестируем поля с плавающей точкой с нецелыми значениями, т.к. Convert.ToInt32(Object) использует ужасные правила округления и, может быть, я это поменяю
    [TestCase("FSingle", 0, 0)]
    [TestCase("FSingle", 2, (int)(TestTable.Row2.VSingle))]
    [TestCase("FDouble", 0, 0)]
    [TestCase("FDouble", 2, (int)(TestTable.Row2.VDouble))]
    [TestCase("FDecimal", 0, 0)]
    [TestCase("FDecimal", 2, (int)(TestTable.Row2.VDecimal))]
    public void GetInt32(string columnName, int rowIndex, Int32 wantedValue)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetInt32(row, columnName));
    }


    [TestCase("FString", 0, 0L)]
    [TestCase("FStringNN", 0, 0L)]
    [TestCase("FInt32", 0, 0L)]
    [TestCase("FInt32", 1, (long)(TestTable.Row1.VInt32))]
    [TestCase("FInt32NN", 0, 0)]
    [TestCase("FInt32NN", 1, (long)(TestTable.Row1.VInt32))]
    [TestCase("FInt64", 0, 0L)]
    [TestCase("FInt64", 1, TestTable.Row1.VInt64)]
    // Не тестируем поля с плавающей точкой с нецелыми значениями, т.к. Convert.ToInt32(Object) использует ужасные правила округления и, может быть, я это поменяю
    [TestCase("FSingle", 0, 0L)]
    [TestCase("FSingle", 2, (long)(TestTable.Row2.VSingle))]
    [TestCase("FDouble", 0, 0L)]
    [TestCase("FDouble", 2, (long)(TestTable.Row2.VDouble))]
    [TestCase("FDecimal", 0, 0L)]
    [TestCase("FDecimal", 2, (long)(TestTable.Row2.VDecimal))]
    public void GetInt64(string columnName, int rowIndex, Int64 wantedValue)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetInt64(row, columnName));
    }

    [TestCase("FString", 0, 0f)]
    [TestCase("FStringNN", 0, 0f)]
    [TestCase("FInt32", 0, 0f)]
    [TestCase("FInt32", 1, (float)(TestTable.Row1.VInt32))]
    [TestCase("FInt32NN", 0, 0f)]
    [TestCase("FInt32NN", 1, (float)(TestTable.Row1.VInt32))]
    [TestCase("FInt64", 0, 0f)]
    [TestCase("FInt64", 1, (float)(TestTable.Row1.VInt64))]
    [TestCase("FSingle", 0, 0f)]
    [TestCase("FSingle", 1, TestTable.Row1.VSingle)]
    [TestCase("FDouble", 0, 0f)]
    [TestCase("FDouble", 1, (float)(TestTable.Row1.VDouble))]
    [TestCase("FDecimal", 0, 0f)]
    [TestCase("FDecimal", 1, (float)(TestTable.Row1.VDecimal))]
    public void GetSingle(string columnName, int rowIndex, float wantedValue)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetSingle(row, columnName));
    }

    [TestCase("FString", 0, 0.0)]
    [TestCase("FStringNN", 0, 0.0)]
    [TestCase("FInt32", 0, 0.0)]
    [TestCase("FInt32", 1, (double)(TestTable.Row1.VInt32))]
    [TestCase("FInt32NN", 0, 0.0)]
    [TestCase("FInt32NN", 1, (double)(TestTable.Row1.VInt32))]
    [TestCase("FInt64", 0, 0.0)]
    [TestCase("FInt64", 1, (double)(TestTable.Row1.VInt64))]
    [TestCase("FSingle", 0, 0.0)]
    [TestCase("FSingle", 1, (double)(TestTable.Row1.VSingle))]
    [TestCase("FDouble", 0, 0.0)]
    [TestCase("FDouble", 1, TestTable.Row1.VDouble)]
    [TestCase("FDecimal", 0, 0.0)]
    [TestCase("FDecimal", 1, (double)(TestTable.Row1.VDecimal))]
    public void GetDouble(string columnName, int rowIndex, double wantedValue)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetDouble(row, columnName));
    }

    [TestCase("FString", 0, "0")]
    [TestCase("FStringNN", 0, "0")]
    [TestCase("FInt32", 0, "0")]
    [TestCase("FInt32", 1, TestTable.Row1.StrInt32)]
    [TestCase("FInt32NN", 0, "0")]
    [TestCase("FInt32NN", 1, TestTable.Row1.StrInt32)]
    [TestCase("FInt64", 0, "0")]
    [TestCase("FInt64", 1, TestTable.Row1.StrInt64)]
    [TestCase("FSingle", 0, "0")]
    [TestCase("FSingle", 1, TestTable.Row1.StrSingle)]
    [TestCase("FDouble", 0, "0")]
    [TestCase("FDouble", 1, TestTable.Row1.StrDouble)]
    [TestCase("FDecimal", 0, "0")]
    [TestCase("FDecimal", 1, TestTable.Row1.StrDecimal)]
    // Не тестируем поля с плавающей точкой, т.к. Convert.ToInt64(Object) использует ужасные правила округления и, может быть, я это поменяю
    public void GetDecimal(string columnName, int rowIndex, string sWantedValue)
    {
      decimal wantedValue = StdConvert.ToDecimal(sWantedValue);

      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetDecimal(row, columnName));
    }

    [TestCase("FBoolean", 0, false)]
    [TestCase("FBoolean", 1, TestTable.Row1.VBoolean)]
    [TestCase("FBoolean", 2, TestTable.Row2.VBoolean)]
    [TestCase("FString", 0, false)]
    [TestCase("FInt32", 0, false)]
    [TestCase("FInt32", 1, true)]
    [TestCase("FInt64", 0, false)]
    [TestCase("FInt64", 1, true)]
    [TestCase("FSingle", 0, false)]
    [TestCase("FSingle", 1, true)]
    [TestCase("FDouble", 0, false)]
    [TestCase("FDouble", 1, true)]
    [TestCase("FDecimal", 0, false)]
    [TestCase("FDecimal", 1, true)]
    public void GetBoolean(string columnName, int rowIndex, bool wantedValue)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetBoolean(row, columnName));
    }

    [TestCase("FString", 0, Creators.StrDateMinValue)]
    [TestCase("FDateTime", 0, Creators.StrDateMinValue)]
    [TestCase("FDateTime", 1, TestTable.Row1.StrDateTime)]
    [TestCase("FDateTime", 2, TestTable.Row2.StrDateTime)]
    public void GetDateTime(string columnName, int rowIndex, string  sWantedValue)
    {
      DateTime wantedValue = Creators.DateTime(sWantedValue);

      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetDateTime(row, columnName));
    }

    [TestCase("FString", 0, Creators.StrTimeSpanZeroValue)]
    [TestCase("FTimeSpan", 0, Creators.StrTimeSpanZeroValue)]
    [TestCase("FTimeSpan", 1, TestTable.Row1.StrTimeSpan)]
    [TestCase("FTimeSpan", 2, TestTable.Row2.StrTimeSpan)]
    public void GetTimeSpan(string columnName, int rowIndex, string sWantedValue)
    {
      TimeSpan wantedValue = Creators.NTimeSpan(sWantedValue).Value;

      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetTimeSpan(row, columnName));
    }

    [TestCase("FString", 0, Creators.StrGuidEmpty)]
    [TestCase("FGuid", 0, Creators.StrGuidEmpty)]
    [TestCase("FGuid", 1, TestTable.Row1.StrGuid)]
    [TestCase("FGuid", 2, TestTable.Row2.StrGuid)]
    public void GetGuid(string columnName, int rowIndex, string sWantedValue)
    {
      Guid wantedValue = new Guid(sWantedValue);

      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetGuid(row, columnName));
    }


    [TestCase("FString", 0, TestEnum.Zero)]
    [TestCase("FStringNN", 0, TestEnum.Zero)]
    [TestCase("FEnumInt32", 0, TestEnum.Zero)]
    [TestCase("FEnumInt32", 1, TestTable.Row1.VEnum)]
    [TestCase("FEnumInt32", 2, TestTable.Row2.VEnum)]
    [TestCase("FEnumString", 1, TestTable.Row1.VEnum)]
    [TestCase("FEnumString", 2, TestTable.Row2.VEnum)]
    public void GetEnum(string columnName, int rowIndex, TestEnum wantedValue)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetEnum<TestEnum>(row, columnName));
    }


    [TestCase("FString", 0, null)]
    [TestCase("FStringNN", 0, null)]
    [TestCase("FInt32", 0, null)]
    [TestCase("FInt32", 1, TestTable.Row1.VInt32)]
    [TestCase("FInt32NN", 0, 0)]
    [TestCase("FInt32NN", 1, TestTable.Row1.VInt32)]
    [TestCase("FInt64", 0, null)]
    [TestCase("FInt64", 1, (int)(TestTable.Row1.VInt64))]
    // Не тестируем поля с плавающей точкой с нецелыми значениями, т.к. Convert.ToInt32(Object) использует ужасные правила округления и, может быть, я это поменяю
    [TestCase("FSingle", 0, null)]
    [TestCase("FSingle", 2, (int)(TestTable.Row2.VSingle))]
    [TestCase("FDouble", 0, null)]
    [TestCase("FDouble", 2, (int)(TestTable.Row2.VDouble))]
    [TestCase("FDecimal", 0, null)]
    [TestCase("FDecimal", 2, (int)(TestTable.Row2.VDecimal))]
    public void GetNullableInt32(string columnName, int rowIndex, Int32? wantedValue)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetNullableInt32(row, columnName));
    }


    [TestCase("FString", 0, null)]
    [TestCase("FStringNN", 0, null)]
    [TestCase("FInt32", 0, null)]
    [TestCase("FInt32", 1, (long)(TestTable.Row1.VInt32))]
    [TestCase("FInt32NN", 0, 0L)]
    [TestCase("FInt32NN", 1, (long)(TestTable.Row1.VInt32))]
    [TestCase("FInt64", 0, null)]
    [TestCase("FInt64", 1, TestTable.Row1.VInt64)]
    // Не тестируем поля с плавающей точкой с нецелыми значениями, т.к. Convert.ToInt32(Object) использует ужасные правила округления и, может быть, я это поменяю
    [TestCase("FSingle", 0, null)]
    [TestCase("FSingle", 2, (long)(TestTable.Row2.VSingle))]
    [TestCase("FDouble", 0, null)]
    [TestCase("FDouble", 2, (long)(TestTable.Row2.VDouble))]
    [TestCase("FDecimal", 0, null)]
    [TestCase("FDecimal", 2, (long)(TestTable.Row2.VDecimal))]
    public void GetNullableInt64(string columnName, int rowIndex, Int64? wantedValue)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetNullableInt64(row, columnName));
    }

    [TestCase("FString", 0, null)]
    [TestCase("FStringNN", 0, null)]
    [TestCase("FInt32", 0, null)]
    [TestCase("FInt32", 1, (float)(TestTable.Row1.VInt32))]
    [TestCase("FInt32NN", 0, 0f)]
    [TestCase("FInt32NN", 1, (float)(TestTable.Row1.VInt32))]
    [TestCase("FInt64", 0, null)]
    [TestCase("FInt64", 1, (float)(TestTable.Row1.VInt64))]
    [TestCase("FSingle", 0, null)]
    [TestCase("FSingle", 1, TestTable.Row1.VSingle)]
    [TestCase("FDouble", 0, null)]
    [TestCase("FDouble", 1, (float)(TestTable.Row1.VDouble))]
    [TestCase("FDecimal", 0, null)]
    [TestCase("FDecimal", 1, (float)(TestTable.Row1.VDecimal))]
    public void GetNullableSingle(string columnName, int rowIndex, float? wantedValue)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetNullableSingle(row, columnName));
    }

    [TestCase("FString", 0, null)]
    [TestCase("FStringNN", 0, null)]
    [TestCase("FInt32", 0, null)]
    [TestCase("FInt32", 1, (double)(TestTable.Row1.VInt32))]
    [TestCase("FInt32NN", 0, 0.0)]
    [TestCase("FInt32NN", 1, (double)(TestTable.Row1.VInt32))]
    [TestCase("FInt64", 0, null)]
    [TestCase("FInt64", 1, (double)(TestTable.Row1.VInt64))]
    [TestCase("FSingle", 0, null)]
    [TestCase("FSingle", 1, (double)(TestTable.Row1.VSingle))]
    [TestCase("FDouble", 0, null)]
    [TestCase("FDouble", 1, TestTable.Row1.VDouble)]
    [TestCase("FDecimal", 0, null)]
    [TestCase("FDecimal", 1, (double)(TestTable.Row1.VDecimal))]
    public void GetNullableDouble(string columnName, int rowIndex, double? wantedValue)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetNullableDouble(row, columnName));
    }

    [TestCase("FString", 0, "")]
    [TestCase("FStringNN", 0, "")]
    [TestCase("FInt32", 0, "")]
    [TestCase("FInt32", 1, TestTable.Row1.StrInt32)]
    [TestCase("FInt32NN", 0, "0")]
    [TestCase("FInt32NN", 1, TestTable.Row1.StrInt32)]
    [TestCase("FInt64", 0, "")]
    [TestCase("FInt64", 1, TestTable.Row1.StrInt64)]
    [TestCase("FSingle", 0, "")]
    [TestCase("FSingle", 1, TestTable.Row1.StrSingle)]
    [TestCase("FDouble", 0, "")]
    [TestCase("FDouble", 1, TestTable.Row1.StrDouble)]
    [TestCase("FDecimal", 0, "")]
    [TestCase("FDecimal", 1, TestTable.Row1.StrDecimal)]
    // Не тестируем поля с плавающей точкой, т.к. Convert.ToInt64(Object) использует ужасные правила округления и, может быть, я это поменяю
    public void GetNullableDecimal(string columnName, int rowIndex, string sWantedValue)
    {
      decimal? wantedValue=null;
      if (!String.IsNullOrEmpty(sWantedValue))
        wantedValue = StdConvert.ToDecimal(sWantedValue);

      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetNullableDecimal(row, columnName));
    }

    [TestCase("FString", 0, "")]
    [TestCase("FDateTime", 0, "")]
    [TestCase("FDateTime", 1, TestTable.Row1.StrDateTime)]
    [TestCase("FDateTime", 2, TestTable.Row2.StrDateTime)]
    public void GetNullableDateTime(string columnName, int rowIndex, string sWantedValue)
    {
      DateTime? wantedValue = Creators.NDateTime(sWantedValue);

      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[rowIndex];

      Assert.AreEqual(wantedValue, DataTools.GetNullableDateTime(row, columnName));
    }

    [TestCase(-3.5)]
    [TestCase(-2.5)]
    [TestCase(-1.5)]
    [TestCase(-0.6)]
    [TestCase(-0.5)]
    [TestCase(-0.4)]
    [TestCase(0.0)]
    [TestCase(-0.4)]
    [TestCase(0.5)]
    [TestCase(0.6)]
    [TestCase(1.5)]
    [TestCase(2.5)]
    [TestCase(3.5)]
    public void GetInt_RoundingTroubles(double value)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[0];

      row["FSingle"] = (float)value;
      row["FDouble"] = value;
      row["FDecimal"] = (decimal)value;

      Assert.AreEqual((float)value, DataTools.GetSingle(row, "FSingle"), "GetSingle()");
      Assert.AreEqual(value, DataTools.GetDouble(row, "FDouble"), "GetDouble()");
      Assert.AreEqual((decimal)value, DataTools.GetDecimal(row, "FDecimal"), "GetDecimal()");

      int wantedValueInt32 = (int)Math.Round(value, 0, MidpointRounding.AwayFromZero);
      long wantedValueInt64 = wantedValueInt32;

      Assert.AreEqual(wantedValueInt32, DataTools.GetInt32(row, "FSingle"), "GetInt32(FSingle)");
      Assert.AreEqual(wantedValueInt64, DataTools.GetInt64(row, "FSingle"), "GetInt64(FSingle)");

      Assert.AreEqual(wantedValueInt32, DataTools.GetInt32(row, "FDouble"), "GetInt32(FDouble)");
      Assert.AreEqual(wantedValueInt64, DataTools.GetInt64(row, "FDouble"), "GetInt64(FDouble)");

      Assert.AreEqual(wantedValueInt32, DataTools.GetInt32(row, "FDecimal"), "GetInt32(FDecimal)");
      Assert.AreEqual(wantedValueInt64, DataTools.GetInt64(row, "FDecimal"), "GetInt64(FDecimal)");

      Assert.AreEqual(wantedValueInt32, DataTools.GetNullableInt32(row, "FSingle"), "GetNullableInt32(FSingle)");
      Assert.AreEqual(wantedValueInt64, DataTools.GetNullableInt64(row, "FSingle"), "GetNullableInt64(FSingle)");

      Assert.AreEqual(wantedValueInt32, DataTools.GetNullableInt32(row, "FDouble"), "GetNullableInt32(FDouble)");
      Assert.AreEqual(wantedValueInt64, DataTools.GetNullableInt64(row, "FDouble"), "GetNullableInt64(FDouble)");

      Assert.AreEqual(wantedValueInt32, DataTools.GetNullableInt32(row, "FDecimal"), "GetNullableInt32(FDecimal)");
      Assert.AreEqual(wantedValueInt64, DataTools.GetNullableInt64(row, "FDecimal"), "GetNullableInt64(FDecimal)");
    }

    #endregion

    #region SetXXX()

    [TestCase("FString", "XXX", "XXX", false)]
    [TestCase("FString", "", "", true)]
    [TestCase("FStringNN", "", "", false)]
    [TestCase("FInt32", "123", "123", false)]
    [TestCase("FInt32", "0", "0", false)]
    [TestCase("FInt32", "", "", true)]
    [TestCase("FInt32NN", "", "0", false)]
    [TestCase("FInt64", "123", "123", false)]
    [TestCase("FInt64", "", "", true)]
    [TestCase("FSingle", "123.45", "123.45", false)]
    [TestCase("FSingle", "", "", true)]
    [TestCase("FDouble", "123.45", "123.45", false)]
    [TestCase("FDouble", "", "", true)]
    [TestCase("FDecimal", "123.45", "123.45", false)]
    [TestCase("FDecimal", "", "", true)]
    [TestCase("FBoolean", "1", "True", false)]
    [TestCase("FBoolean", "0", "False", false)]
    [TestCase("FBoolean", "True", "True", false)]
    [TestCase("FBoolean", "False", "False", false)]
    [TestCase("FBoolean", "", "", true)]
    [TestCase("FGuid", "{0614EA9D-BD4D-426B-9A62-B62E51CF5ED0}", "0614ea9d-bd4d-426b-9a62-b62e51cf5ed0", false)]
    [TestCase("FGuid", Creators.StrGuidEmpty, Creators.StrGuidEmpty, false)]
    [TestCase("FGuid", "", "", true)]
    [TestCase("FEnumString", "One", "One", false)]
    [TestCase("FEnumString", "Zero", "Zero", false)]
    [TestCase("FEnumString", "", "", true)]
    public void SetString(string columnName, string value, string wantedGetString, bool wantedIsNull)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[0];

      DataTools.SetString(row, columnName, value);

      Assert.AreEqual(wantedGetString, DataTools.GetString(row, columnName), "GetString()");
      Assert.AreEqual(wantedIsNull, row.IsNull(columnName), "IsNull()");
    }

    [TestCase("FString", 123, "123", false)]
    [TestCase("FString", 0, "", true)]
    [TestCase("FStringNN", 0, "", false)]
    [TestCase("FInt32", 123, "123", false)]
    [TestCase("FInt32", 0, "", true)]
    [TestCase("FInt32NN", 0, "0", false)]
    [TestCase("FInt64", 123, "123", false)]
    [TestCase("FInt64", 0, "", true)]
    [TestCase("FSingle", 123, "123", false)]
    [TestCase("FSingle", 0, "", true)]
    [TestCase("FDouble", 123, "123", false)]
    [TestCase("FDouble", 0, "", true)]
    [TestCase("FDecimal", 123, "123", false)]
    [TestCase("FDecimal", 0, "", true)]
    [TestCase("FBoolean", 123, "True", false)]
    [TestCase("FBoolean", 1, "True", false)]
    [TestCase("FBoolean", 0, "", true)]
    [TestCase("FBooleanNN", 1, "True", false)]
    [TestCase("FBooleanNN", 0, "False", false)]
    public void SetInt32(string columnName, int value, string wantedGetString, bool wantedIsNull)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[0];

      DataTools.SetInt32(row, columnName, value);

      Assert.AreEqual(wantedGetString, DataTools.GetString(row, columnName), "GetString()");
      Assert.AreEqual(wantedIsNull, row.IsNull(columnName), "IsNull()");
    }


    [TestCase("FString", 123L, "123", false)]
    [TestCase("FString", 0L, "", true)]
    [TestCase("FStringNN", 0L, "", false)]
    [TestCase("FInt32", 123L, "123", false)]
    [TestCase("FInt32", 0L, "", true)]
    [TestCase("FInt32NN", 0L, "0", false)]
    [TestCase("FInt64", 123L, "123", false)]
    [TestCase("FInt64", 0L, "", true)]
    [TestCase("FSingle", 123L, "123", false)]
    [TestCase("FSingle", 0L, "", true)]
    [TestCase("FDouble", 123L, "123", false)]
    [TestCase("FDouble", 0L, "", true)]
    [TestCase("FDecimal", 123L, "123", false)]
    [TestCase("FDecimal", 0, "", true)]
    [TestCase("FBoolean", 123L, "True", false)]
    [TestCase("FBoolean", 1L, "True", false)]
    [TestCase("FBoolean", 0L, "", true)]
    [TestCase("FBooleanNN", 1L, "True", false)]
    [TestCase("FBooleanNN", 0L, "False", false)]
    public void SetInt64(string columnName, long value, string wantedGetString, bool wantedIsNull)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[0];

      DataTools.SetInt64(row, columnName, value);

      Assert.AreEqual(wantedGetString, DataTools.GetString(row, columnName), "GetString()");
      Assert.AreEqual(wantedIsNull, row.IsNull(columnName), "IsNull()");
    }

    [TestCase("FString", 123.5f, "123.5", false)]
    [TestCase("FString", 0f, "", true)]
    [TestCase("FStringNN", 0f, "", false)]
    [TestCase("FInt32", 1.5f, "2", false)]
    [TestCase("FInt32", 2.5f, "3", false)]
    [TestCase("FInt32", 0f, "", true)]
    [TestCase("FInt32NN", 0f, "0", false)]
    [TestCase("FInt64", 3.5f, "4", false)]
    [TestCase("FInt64", 4.5f, "5", false)]
    [TestCase("FInt64", 0f, "", true)]
    [TestCase("FSingle", -1.5f, "-1.5", false)]
    [TestCase("FSingle", -2.5f, "-2.5", false)]
    [TestCase("FSingle", 0f, "", true)]
    [TestCase("FDouble", 10.5f, "10.5", false)]
    [TestCase("FDouble", 0f, "", true)]
    [TestCase("FDecimal", 1.5f, "1.5", false)]
    [TestCase("FDecimal", 0f, "", true)]
    [TestCase("FBoolean", 123.5f, "True", false)]
    [TestCase("FBoolean", 1f, "True", false)]
    [TestCase("FBoolean", 0f, "", true)]
    [TestCase("FBooleanNN", 1f, "True", false)]
    [TestCase("FBooleanNN", 0f, "False", false)]
    public void SetSingle(string columnName, float value, string wantedGetString, bool wantedIsNull)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[0];

      DataTools.SetSingle(row, columnName, value);

      Assert.AreEqual(wantedGetString, DataTools.GetString(row, columnName), "GetString()");
      Assert.AreEqual(wantedIsNull, row.IsNull(columnName), "IsNull()");
    }

    [TestCase("FString", 123.5, "123.5", false)]
    [TestCase("FString", 0.0, "", true)]
    [TestCase("FStringNN", 0.0, "", false)]
    [TestCase("FInt32", 1.5, "2", false)]
    [TestCase("FInt32", 2.5, "3", false)]
    [TestCase("FInt32", 0.0, "", true)]
    [TestCase("FInt32NN", 0.0, "0", false)]
    [TestCase("FInt64", 3.5, "4", false)]
    [TestCase("FInt64", 4.5, "5", false)]
    [TestCase("FInt64", 0.0, "", true)]
    [TestCase("FSingle", -1.5, "-1.5", false)]
    [TestCase("FSingle", -2.5, "-2.5", false)]
    [TestCase("FSingle", 0.0, "", true)]
    [TestCase("FDouble", 10.5, "10.5", false)]
    [TestCase("FDouble", 0.0, "", true)]
    [TestCase("FDecimal", 1.5, "1.5", false)]
    [TestCase("FDecimal", 0.0, "", true)]
    [TestCase("FBoolean", 123.5, "True", false)]
    [TestCase("FBoolean", 1.0, "True", false)]
    [TestCase("FBoolean", 0.0, "", true)]
    [TestCase("FBooleanNN", 1.0, "True", false)]
    [TestCase("FBooleanNN", 0.0, "False", false)]
    public void SetDouble(string columnName, double value, string wantedGetString, bool wantedIsNull)
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[0];

      DataTools.SetDouble(row, columnName, value);

      Assert.AreEqual(wantedGetString, DataTools.GetString(row, columnName), "GetString()");
      Assert.AreEqual(wantedIsNull, row.IsNull(columnName), "IsNull()");
    }

    [TestCase("FString", "123.5", "123.5", false)]
    [TestCase("FString", "0", "", true)]
    [TestCase("FStringNN", "0", "", false)]
    [TestCase("FInt32", "1.5", "2", false)]
    [TestCase("FInt32", "2.5", "3", false)]
    [TestCase("FInt32", "0", "", true)]
    [TestCase("FInt32NN", "0", "0", false)]
    [TestCase("FInt64", "3.5", "4", false)]
    [TestCase("FInt64", "4.5", "5", false)]
    [TestCase("FInt64", "0", "", true)]
    [TestCase("FSingle", "-1.5", "-1.5", false)]
    [TestCase("FSingle", "-2.5", "-2.5", false)]
    [TestCase("FSingle", "0", "", true)]
    [TestCase("FDouble", "10.5", "10.5", false)]
    [TestCase("FDouble", "0", "", true)]
    [TestCase("FDecimal", "1.5", "1.5", false)]
    [TestCase("FDecimal", "0", "", true)]
    [TestCase("FBoolean", "123.5", "True", false)]
    [TestCase("FBoolean", "1", "True", false)]
    [TestCase("FBoolean", "0", "", true)]
    [TestCase("FBooleanNN", "1", "True", false)]
    [TestCase("FBooleanNN", "0", "False", false)]
    public void SetDecimal(string columnName, string sValue, string wantedGetString, bool wantedIsNull)
    {
      decimal value = StdConvert.ToDecimal(sValue);
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[0];

      DataTools.SetDecimal(row, columnName, value);

      Assert.AreEqual(wantedGetString, DataTools.GetString(row, columnName), "GetString()");
      Assert.AreEqual(wantedIsNull, row.IsNull(columnName), "IsNull()");
    }

    [TestCase("FString", "20250625", "2025-06-25T00:00:00", false)]
    [TestCase("FString", "20250625123456", "2025-06-25T12:34:56", false)]
    [TestCase("FString", "", "", true)]
    [TestCase("FDateTime", "20250625", "2025-06-25T00:00:00", false)]
    [TestCase("FDateTime", "20250625123456", "2025-06-25T12:34:56", false)]
    [TestCase("FDateTime", "", "", true)]
    public void SetNullableDateTime(string columnName, string sValue, string wantedGetString, bool wantedIsNull)
    {
      DateTime? value = Creators.NDateTime(sValue);
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[0];

      DataTools.SetNullableDateTime(row, columnName, value);

      Assert.AreEqual(wantedGetString, DataTools.GetString(row, columnName), "GetString()");
      Assert.AreEqual(wantedIsNull, row.IsNull(columnName), "IsNull()");
    }

    [TestCase("FString", "12:34:56", "12:34:56", false)]
    [TestCase("FString", "00:00:00", "", true)]
    [TestCase("FTimeSpan", "12:34:56", "12:34:56", false)]
    [TestCase("FTimeSpan", "00:00:00", "", true)]
    public void SetTimeSpan(string columnName, string sValue, string wantedGetString, bool wantedIsNull)
    {
      TimeSpan value = Creators.NTimeSpan(sValue).Value;
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[0];

      DataTools.SetTimeSpan(row, columnName, value);

      Assert.AreEqual(wantedGetString, DataTools.GetString(row, columnName), "GetString()");
      Assert.AreEqual(wantedIsNull, row.IsNull(columnName), "IsNull()");
    }

    [TestCase("FString", "946d61c6-99f4-4f70-b037-3777740f96f3", false)]
    [TestCase("FString", Creators.StrGuidEmpty, true)]
    [TestCase("FGuid", "946d61c6-99f4-4f70-b037-3777740f96f3", false)]
    [TestCase("FGuid", Creators.StrGuidEmpty, true)]
    [TestCase("FBytes", "946d61c6-99f4-4f70-b037-3777740f96f3", false)]
    [TestCase("FBytes", Creators.StrGuidEmpty, true)]
    public void SetGuid(string columnName, string sValue, bool wantedIsNull)
    {
      Guid value = new Guid(sValue);
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[0];

      DataTools.SetGuid(row, columnName, value);

      Assert.AreEqual(value, DataTools.GetGuid(row, columnName), "GetGuid()");
      Assert.AreEqual(wantedIsNull, row.IsNull(columnName), "IsNull()");
    }


    #endregion

    #region IncXXX()

    [Test]
    public void IncXXX_delta_NotNull()
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[1];

      DataTools.IncInt32(row, "FInt32", 1);
      Assert.AreEqual(TestTable.Row1.VInt32 + 1, row["FInt32"], "IncInt32()");

      DataTools.IncInt64(row, "FInt64", 1L);
      Assert.AreEqual(TestTable.Row1.VInt64 + 1L, row["FInt64"], "IncInt64()");

      DataTools.IncSingle(row, "FSingle", 1f);
      Assert.AreEqual(TestTable.Row1.VSingle + 1f, row["FSingle"], "IncSingle()");

      DataTools.IncDouble(row, "FDouble", 1.0);
      Assert.AreEqual(TestTable.Row1.VDouble + 1.0, row["FDouble"], "IncDouble()");

      DataTools.IncDecimal(row, "FDecimal", 1m);
      Assert.AreEqual(TestTable.Row1.VDecimal + 1m, row["FDecimal"], "IncDecimal()");

      DataTools.IncTimeSpan(row, "FTimeSpan", new TimeSpan(1, 0, 0));
      Assert.AreEqual(TestTable.Row1.VTimeSpan + new TimeSpan(1, 0, 0), row["FTimeSpan"], "IncTimeSpan()");
    }

    [Test]
    public void IncXXX_delta_DBNull()
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[0];

      DataTools.IncInt32(row, "FInt32", 1);
      Assert.AreEqual(1, row["FInt32"], "IncInt32()");

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
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[1];
      DataRow row2 = tbl.Rows[2];

      DataTools.IncInt32(row2, row, "FInt32");
      Assert.AreEqual(TestTable.Row1.VInt32 + TestTable.Row2.VInt32, row["FInt32"], "IncInt32()");

      DataTools.IncInt64(row2, row, "FInt64");
      Assert.AreEqual(TestTable.Row1.VInt64 + TestTable.Row2.VInt64, row["FInt64"], "IncInt64()");

      DataTools.IncSingle(row2, row, "FSingle");
      Assert.AreEqual(TestTable.Row1.VSingle + TestTable.Row2.VSingle, row["FSingle"], "IncSingle()");

      DataTools.IncDouble(row2, row, "FDouble");
      Assert.AreEqual(TestTable.Row1.VDouble + TestTable.Row2.VDouble, row["FDouble"], "IncDouble()");

      DataTools.IncDecimal(row2, row, "FDecimal");
      Assert.AreEqual(TestTable.Row1.VDecimal + TestTable.Row2.VDecimal, row["FDecimal"], "IncDecimal()");

      DataTools.IncTimeSpan(row2, row, "FTimeSpan");
      Assert.AreEqual(TestTable.Row1.VTimeSpan + TestTable.Row2.VTimeSpan, row["FTimeSpan"], "IncTimeSpan()");
    }

    [Test]
    public void IncValue_delta()
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[1];

      DataTools.IncValue(row, "FInt32", 1);
      Assert.AreEqual(TestTable.Row1.VInt32 + 1, row["FInt32"], "Int32");

      DataTools.IncValue(row, "FInt64", 1L);
      Assert.AreEqual(TestTable.Row1.VInt64 + 1L, row["FInt64"], "Int64");

      DataTools.IncValue(row, "FSingle", 1f);
      Assert.AreEqual(TestTable.Row1.VSingle + 1f, row["FSingle"], "Single");

      DataTools.IncValue(row, "FDouble", 1.0);
      Assert.AreEqual(TestTable.Row1.VDouble + 1.0, row["FDouble"], "Double");

      DataTools.IncValue(row, "FDecimal", 1m);
      Assert.AreEqual(TestTable.Row1.VDecimal + 1m, row["FDecimal"], "Decimal");

      DataTools.IncValue(row, "FTimeSpan", new TimeSpan(1, 0, 0));
      Assert.AreEqual(TestTable.Row1.VTimeSpan + new TimeSpan(1, 0, 0), row["FTimeSpan"], "TimeSpan");
    }

    [Test]
    public void IncValue_RowRow()
    {
      DataTable tbl = TestTable.Create();
      DataRow row = tbl.Rows[1];
      DataRow row2 = tbl.Rows[2];

      DataTools.IncValue(row2, row, "FInt32");
      Assert.AreEqual(TestTable.Row1.VInt32 + TestTable.Row2.VInt32, row["FInt32"], "Int32");

      DataTools.IncValue(row2, row, "FInt64");
      Assert.AreEqual(TestTable.Row1.VInt64 + TestTable.Row2.VInt64, row["FInt64"], "Int64");

      DataTools.IncValue(row2, row, "FSingle");
      Assert.AreEqual(TestTable.Row1.VSingle + TestTable.Row2.VSingle, row["FSingle"], "Single");

      DataTools.IncValue(row2, row, "FDouble");
      Assert.AreEqual(TestTable.Row1.VDouble + TestTable.Row2.VDouble, row["FDouble"], "Double");

      DataTools.IncValue(row2, row, "FDecimal");
      Assert.AreEqual(TestTable.Row1.VDecimal + TestTable.Row2.VDecimal, row["FDecimal"], "Decimal");

      DataTools.IncValue(row2, row, "FTimeSpan");
      Assert.AreEqual(TestTable.Row1.VTimeSpan + TestTable.Row2.VTimeSpan, row["FTimeSpan"], "TimeSpan");
    }

    #endregion

    #region AddStrIfNotEmpty()

    [TestCase(null, null, null, "")]
    [TestCase(null, "", null, "")]
    [TestCase(null, null, "", "")]
    [TestCase(null, "", "", "")]
    [TestCase("", "", "", "")]
    [TestCase("A", "", "", "A")]
    [TestCase("", "B", "", "B")]
    [TestCase("", "", "C", "")]
    [TestCase("", "B", "C", "B")]
    [TestCase("A", "", "C", "A")]
    [TestCase("A", "B", "C", "ACB")]
    [TestCase("A", "B", "", "AB")]
    public void AddStrIfNotEmpty_DataRow(string orgVal, string addedStr, string separator, string wantedRes)
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string));

      DataRow row = table.Rows.Add();
      DataTools.SetString(row, "F1", orgVal);

      DataTools.AddStrIfNotEmpty(row, "F1", addedStr, separator);

      string res = DataTools.GetString(row, "F1");
      Assert.AreEqual(wantedRes, res);
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
