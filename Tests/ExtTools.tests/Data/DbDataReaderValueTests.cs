using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Tests;
using FreeLibSet.Data;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class DbDataReaderValueTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Index()
    {
      DataTable table = CreateTestTable();
      DataTableReader reader = new DataTableReader(table);

      DbDataReaderValue sut = new DbDataReaderValue(reader, 1);

      Assert.AreEqual("FInt16", sut.ColumnName);
    }

    [Test]
    public void Constructor_Name()
    {
      DataTable table = CreateTestTable();
      DataTableReader reader = new DataTableReader(table);

      DbDataReaderValue sut = new DbDataReaderValue(reader, "FInt16");

      Assert.AreEqual("FInt16", sut.ColumnName);
    }

    [Test]
    public void CreateDict()
    {
      DataTable table = CreateTestTable();
      DataTableReader reader = new DataTableReader(table);

      IDictionary<string, DbDataReaderValue> res = DbDataReaderValue.CreateDict(reader);

      Assert.AreEqual(table.Columns.Count, res.Count, "Count");
      foreach (DataColumn col in table.Columns)
        Assert.AreEqual(col.ColumnName, res[col.ColumnName].ColumnName, col.ColumnName);
      Assert.AreEqual("FInt16", res["fint16"].ColumnName, "Case ignoring");
    }

    #endregion

    #region Типизированные значения

    [TestCase("FInt32", 1, -2, 0, 0)]
    [TestCase("FInt16", 1, -2, 0, 0)]
    [TestCase("FInt64", 1, -2, 0, 0)]
    [TestCase("FSingle", 1, -2, 0, 0)]
    [TestCase("FDouble", 1, -2, 0, 0)]
    [TestCase("FDecimal", 1, -2, 0, 0)]
    [TestCase("FBoolean", 1, 1, 0, 0)]
    public void AsInt32(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsInt32", wantedValues);
    }

    [TestCase("FInt32", 1, -2, 0, null)]
    [TestCase("FInt16", 1, -2, 0, null)]
    [TestCase("FInt64", 1, -2, 0, null)]
    [TestCase("FSingle", 1, -2, 0, null)]
    [TestCase("FDouble", 1, -2, 0, null)]
    [TestCase("FDecimal", 1, -2, 0, null)]
    [TestCase("FBoolean", 1, 1, 0, null)]
    public void AsNullableInt32(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsNullableInt32", wantedValues);
    }

    [TestCase("FInt32", 1L, -2L, 0L, 0L)]
    [TestCase("FInt16", 1L, -2L, 0L, 0L)]
    [TestCase("FInt64", 1L, -2L, 0L, 0L)]
    [TestCase("FSingle", 1L, -2L, 0L, 0L)]
    [TestCase("FDouble", 1L, -2L, 0L, 0L)]
    [TestCase("FDecimal", 1L, -2L, 0L, 0L)]
    [TestCase("FBoolean", 1L, 1L, 0L, 0L)]
    public void AsInt64(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsInt64", wantedValues);
    }

    [TestCase("FInt32", 1L, -2L, 0L, null)]
    [TestCase("FInt16", 1L, -2L, 0L, null)]
    [TestCase("FInt64", 1L, -2L, 0L, null)]
    [TestCase("FSingle", 1L, -2L, 0L, null)]
    [TestCase("FDouble", 1L, -2L, 0L, null)]
    [TestCase("FDecimal", 1L, -2L, 0L, null)]
    [TestCase("FBoolean", 1L, 1L, 0L, null)]
    public void AsNullableInt64(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsNullableInt64", wantedValues);
    }

    [TestCase("FInt32", 1f, -2f, 0f, 0f)]
    [TestCase("FInt16", 1f, -2f, 0f, 0f)]
    [TestCase("FInt64", 1f, -2f, 0f, 0f)]
    [TestCase("FSingle", 1f, -1.5f, 0f, 0f)]
    [TestCase("FDouble", 1f, -1.5f, 0f, 0f)]
    [TestCase("FDecimal", 1f, -1.5f, 0f, 0f)]
    [TestCase("FBoolean", 1f, 1f, 0f, 0f)]
    [TestCase("FString", 1f, -1.5f, 0f, 0f)]
    public void AsSingle(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsSingle", wantedValues);
    }

    [TestCase("FInt32", 1f, -2f, 0f, null)]
    [TestCase("FInt16", 1f, -2f, 0f, null)]
    [TestCase("FInt64", 1f, -2f, 0f, null)]
    [TestCase("FSingle", 1f, -1.5f, 0f, null)]
    [TestCase("FDouble", 1f, -1.5f, 0f, null)]
    [TestCase("FDecimal", 1f, -1.5f, 0f, null)]
    [TestCase("FBoolean", 1f, 1f, 0f, null)]
    [TestCase("FString", 1f, -1.5f, null, null)]
    public void AsNullableSingle(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsNullableSingle", wantedValues);
    }

    [TestCase("FInt32", 1.0, -2.0, 0.0, 0.0)]
    [TestCase("FInt16", 1.0, -2.0, 0.0, 0.0)]
    [TestCase("FInt64", 1.0, -2.0, 0.0, 0.0)]
    [TestCase("FSingle", 1.0, -1.5, 0.0, 0.0)]
    [TestCase("FDouble", 1.0, -1.5, 0.0, 0.0)]
    [TestCase("FDecimal", 1.0, -1.5, 0.0, 0.0)]
    [TestCase("FBoolean", 1.0, 1.0, 0.0, 0.0)]
    [TestCase("FString", 1.0, -1.5, 0.0, 0.0)]
    public void AsDouble(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsDouble", wantedValues);
    }

    [TestCase("FInt32", 1.0, -2.0, 0.0, null)]
    [TestCase("FInt16", 1.0, -2.0, 0.0, null)]
    [TestCase("FInt64", 1.0, -2.0, 0.0, null)]
    [TestCase("FSingle", 1.0, -1.5, 0.0, null)]
    [TestCase("FDouble", 1.0, -1.5, 0.0, null)]
    [TestCase("FDecimal", 1.0, -1.5, 0.0, null)]
    [TestCase("FBoolean", 1.0, 1.0, 0.0, null)]
    [TestCase("FString", 1.0, -1.5, null, null)]
    public void AsNullableDouble(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsNullableDouble", wantedValues);
    }

    [TestCase("FInt32", 1.0, -2.0, 0.0, 0.0)]
    [TestCase("FInt16", 1.0, -2.0, 0.0, 0.0)]
    [TestCase("FInt64", 1.0, -2.0, 0.0, 0.0)]
    [TestCase("FSingle", 1.0, -1.5, 0.0, 0.0)]
    [TestCase("FDouble", 1.0, -1.5, 0.0, 0.0)]
    [TestCase("FDecimal", 1.0, -1.5, 0.0, 0.0)]
    [TestCase("FBoolean", 1.0, 1.0, 0.0, 0.0)]
    [TestCase("FString", 1.0, -1.5, 0.0, 0.0)]
    public void AsDecimal(string columnName, params double?[] dWantedValues)
    {
      object[] wantedValues = ToDecimal(dWantedValues);
      DoTest(columnName, "AsDecimal", wantedValues);
    }

    private static object[] ToDecimal(double?[] dWantedValues)
    {
      object[] wanted = new object[dWantedValues.Length];
      for (int i = 0; i < dWantedValues.Length; i++)
      {
        if (dWantedValues[i].HasValue)
          wanted[i] = Convert.ToDecimal(dWantedValues[i]);
        else
          wanted[i] = null;
      }
      return wanted;
    }

    [TestCase("FInt32", 1.0, -2.0, 0.0, null)]
    [TestCase("FInt16", 1.0, -2.0, 0.0, null)]
    [TestCase("FInt64", 1.0, -2.0, 0.0, null)]
    [TestCase("FSingle", 1.0, -1.5, 0.0, null)]
    [TestCase("FDouble", 1.0, -1.5, 0.0, null)]
    [TestCase("FDecimal", 1.0, -1.5, 0.0, null)]
    [TestCase("FBoolean", 1.0, 1.0, 0.0, null)]
    [TestCase("FString", 1.0, -1.5, null, null)]
    public void AsNullableDecimal(string columnName, params double?[] dWantedValues)
    {
      object[] wantedValues = ToDecimal(dWantedValues);
      DoTest(columnName, "AsNullableDecimal", wantedValues);
    }

    [TestCase("FInt32", "1", "-2", "0", "")]
    [TestCase("FInt16", "1", "-2", "0", "")]
    [TestCase("FInt64", "1", "-2", "0", "")]
    [TestCase("FSingle", "1", "-1.5", "0", "")]
    [TestCase("FDouble", "1", "-1.5", "0", "")]
    [TestCase("FDecimal", "1", "-1.5", "0", "")]
    [TestCase("FBoolean", "True", "True", "False", "")]
    [TestCase("FString", "1", "-1.5", "", "")]
    public void AsString(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsString", wantedValues);
    }

    [TestCase("FInt32", true, true, false, false)]
    [TestCase("FInt16", true, true, false, false)]
    [TestCase("FInt64", true, true, false, false)]
    [TestCase("FSingle", true, true, false, false)]
    [TestCase("FDouble", true, true, false, false)]
    [TestCase("FDecimal", true, true, false, false)]
    [TestCase("FBoolean", true, true, false, false)]
    public void AsBoolean(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsBoolean", wantedValues);
    }

    [TestCase("FDateTime", StrDateTime1, StrDateTime2, StrDateTime3, "00010101")]
    public void AsDateTime(string columnName, params string[] sWantedValues)
    {
      object[] wantedValues = new object[sWantedValues.Length];
      for (int i = 0; i < wantedValues.Length; i++)
        wantedValues[i] = Creators.NDateTime(sWantedValues[i]);
      DoTest(columnName, "AsDateTime", wantedValues);
    }

    [TestCase("FDateTime", StrDateTime1, StrDateTime2, StrDateTime3, "")]
    public void AsNullableDateTime(string columnName, params string[] sWantedValues)
    {
      object[] wantedValues = new object[sWantedValues.Length];
      for (int i = 0; i < wantedValues.Length; i++)
        wantedValues[i] = Creators.NDateTime(sWantedValues[i]);
      DoTest(columnName, "AsNullableDateTime", wantedValues);
    }

    [TestCase("FTimeSpan", StrTimeSpan1, StrTimeSpan2, StrTimeSpan3, "00:00:00")]
    public void AsTimeSpan(string columnName, params string[] sWantedValues)
    {
      object[] wantedValues = new object[sWantedValues.Length];
      for (int i = 0; i < wantedValues.Length; i++)
        wantedValues[i] = Creators.NTimeSpan(sWantedValues[i]);
      DoTest(columnName, "AsTimeSpan", wantedValues);
    }

    [TestCase("FTimeSpan", StrTimeSpan1, StrTimeSpan2, StrTimeSpan3, "")]
    public void AsNullableTimeSpan(string columnName, params string[] sWantedValues)
    {
      object[] wantedValues = new object[sWantedValues.Length];
      for (int i = 0; i < wantedValues.Length; i++)
        wantedValues[i] = Creators.NTimeSpan(sWantedValues[i]);
      DoTest(columnName, "AsNullableTimeSpan", wantedValues);
    }

    [TestCase("FGuid", StrGuid1, StrGuid2, StrGuid3, StrGuidEmpty)]
    [TestCase("FGuidString", StrGuid1, StrGuid2, StrGuid3, StrGuidEmpty)]
    [TestCase("FGuidBytes", StrGuid1, StrGuid2, StrGuid3, StrGuidEmpty)]
    public void AsGuid(string columnName, params string[] sWantedValues)
    {
      object[] wantedValues = new object[sWantedValues.Length];
      for (int i = 0; i < wantedValues.Length; i++)
        wantedValues[i] = Creators.NGuid(sWantedValues[i]);
      DoTest(columnName, "AsGuid", wantedValues);
    }

    [TestCase("FGuid", StrGuid1, StrGuid2, StrGuid3, "")]
    [TestCase("FGuidString", StrGuid1, StrGuid2, StrGuid3, "")]
    [TestCase("FGuidBytes", StrGuid1, StrGuid2, StrGuid3, "")]
    public void AsNullableGuid(string columnName, params string[] sWantedValues)
    {
      object[] wantedValues = new object[sWantedValues.Length];
      for (int i = 0; i < wantedValues.Length; i++)
        wantedValues[i] = Creators.NGuid(sWantedValues[i]);
      DoTest(columnName, "AsNullableGuid", wantedValues);
    }

    #endregion

    #region Прочие свойства

    [TestCase("FInt32", false, false, false, true)]
    [TestCase("FString", false, false, false, true)]
    public void IsDBNull(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "IsDBNull", wantedValues);
    }


    [TestCase("FInt32", 1, -2, 0, null)]
    [TestCase("FString", "1", "-1.5", "", null)]
    public void Value(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "Value", wantedValues);
    }

    #endregion

    #region Реализация теста

    /// <summary>
    /// Выполнение теста.
    /// Так как объект <see cref="DbDataReader"/> является одноразовым, в одном тесте метод может вызываться только один раз.
    /// </summary>
    /// <typeparam name="TRes">Тип свойства <see cref="IDbDataReaderExtractor{T}.Value"/></typeparam>
    /// <param name="sut">Тестируемый объект</param>
    /// <param name="wantedValues">Список значений, который должен быть возвращен</param>
    private void DoTest(string columnName, string propertyName, params object[] wantedValues)
    {
      DataTable table = CreateTestTable();
      DbDataReader reader = table.CreateDataReader();

      DbDataReaderValue sut = new DbDataReaderValue(reader, columnName);
      PropertyInfo pi = typeof(DbDataReaderValue).GetProperty(propertyName);
      if (pi == null)
        throw new ArgumentException("Unknown property name", "propertyName");

      Assert.AreEqual(table.Rows.Count, wantedValues.Length, "WantedValues.Length");

      reader.NextResult();
      List<object> res = new List<object>();
      while (reader.Read())
      {
        object v = pi.GetValue(sut, null);
        res.Add(v);
      }

      CollectionAssert.AreEqual(wantedValues, res.ToArray(), sut.ColumnName);
    }

    const string StrDateTime1 = "20250527";
    const string StrDateTime2 = "00010101";
    const string StrDateTime3 = "99991231";

    const string StrTimeSpan1 = "12:34:56";
    const string StrTimeSpan2 = "-12:34:56";
    const string StrTimeSpan3 = "00:00:00";

    const string StrGuidEmpty = "{00000000-0000-0000-0000-000000000000}";
    const string StrGuid1 = "{55188A6E-E510-4AF4-A020-895E405AD2E6}";
    const string StrGuid2 = "{5D531E23-BD10-4E0C-ABA9-FAA94501DD2E}";
    const string StrGuid3 = StrGuidEmpty;
    private static DataTable CreateTestTable()
    {
      DataTable _Table = new DataTable();
      _Table.Columns.Add("FInt32", typeof(Int32));
      _Table.Columns.Add("FInt16", typeof(Int16));
      _Table.Columns.Add("FInt64", typeof(Int64));
      _Table.Columns.Add("FSingle", typeof(Single));
      _Table.Columns.Add("FDouble", typeof(Double));
      _Table.Columns.Add("FDecimal", typeof(Decimal));
      _Table.Columns.Add("FString", typeof(String));
      _Table.Columns.Add("FBoolean", typeof(Boolean));
      _Table.Columns.Add("FDateTime", typeof(DateTime));
      _Table.Columns.Add("FTimeSpan", typeof(TimeSpan));
      _Table.Columns.Add("FGuid", typeof(Guid));
      _Table.Columns.Add("FGuidString", typeof(String));
      _Table.Columns.Add("FGuidBytes", typeof(byte[]));

      _Table.Rows.Add(1, 1, 1L, 1f, 1.0, 1m, "1", true, Creators.DateTime(StrDateTime1), Creators.NTimeSpan(StrTimeSpan1), new Guid(StrGuid1), StrGuid1, Creators.GuidBytes(StrGuid1));
      _Table.Rows.Add(-2, -2, -2L, -1.5f, -1.5, -1.5m, "-1.5", true, Creators.DateTime(StrDateTime2), Creators.NTimeSpan(StrTimeSpan2), new Guid(StrGuid2), StrGuid2, Creators.GuidBytes(StrGuid2));
      _Table.Rows.Add(0, 0, 0L, 0f, 0.0, 0m, "", false, Creators.DateTime(StrDateTime3), Creators.NTimeSpan(StrTimeSpan3), new Guid(StrGuid3), StrGuid3, Creators.GuidBytes(StrGuid3));
      _Table.Rows.Add(); // DBNull
      return _Table;
    }

    #endregion
  }
}
