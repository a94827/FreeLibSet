using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Tests;
using FreeLibSet.Data;
using System.Data;
using System.Data.Common;
using System.Reflection;
using FreeLibSet.Core;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class DbDataReaderValueTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Index()
    {
      DataTable table = TestTable.Create();
      DataTableReader reader = new DataTableReader(table);

      DbDataReaderValue sut = new DbDataReaderValue(reader, 0);

      Assert.AreEqual("FString", sut.ColumnName);
    }

    [Test]
    public void Constructor_Name()
    {
      DataTable table = TestTable.Create();
      DataTableReader reader = new DataTableReader(table);

      DbDataReaderValue sut = new DbDataReaderValue(reader, "FInt32");

      Assert.AreEqual("FInt32", sut.ColumnName);
    }

    //[Test]
    //public void CreateDict()
    //{
    //  DataTable table = CreateTestTable();
    //  DataTableReader reader = new DataTableReader(table);

    //  IDictionary<string, DbDataReaderValue> res = DbDataReaderValue.CreateDict(reader);

    //  Assert.AreEqual(table.Columns.Count, res.Count, "Count");
    //  foreach (DataColumn col in table.Columns)
    //    Assert.AreEqual(col.ColumnName, res[col.ColumnName].ColumnName, col.ColumnName);
    //  Assert.AreEqual("FInt16", res["fint16"].ColumnName, "Case ignoring");
    //}

    #endregion

    #region Типизированные значения


    [TestCase("FString", "", TestTable.Row1.VString, TestTable.Row2.VString)]
    [TestCase("FStringNN", "", TestTable.Row1.VString, TestTable.Row2.VString)]
    [TestCase("FInt32", "", TestTable.Row1.StrInt32, TestTable.Row2.StrInt32)]
    [TestCase("FInt32NN", "0", TestTable.Row1.StrInt32, TestTable.Row2.StrInt32)]
    [TestCase("FInt64", "", TestTable.Row1.StrInt64, TestTable.Row2.StrInt64)]
    [TestCase("FSingle", "", TestTable.Row1.StrSingle, TestTable.Row2.StrSingle)]
    [TestCase("FDouble", "", TestTable.Row1.StrDouble, TestTable.Row2.StrDouble)]
    [TestCase("FDecimal", "", TestTable.Row1.StrDecimal, TestTable.Row2.StrDecimal)]
    [TestCase("FBoolean", "", TestTable.Row1.VBoolean ? "True" : "False", TestTable.Row2.VBoolean ? "True" : "False")]
    // сложно преобразовать[TestCase("FDateTime", "", TestTable.Row1.StrDateTime, TestTable.Row2.StrDateTime)]
    [TestCase("FTimeSpan", "", TestTable.Row1.StrTimeSpan, TestTable.Row2.StrTimeSpan)]
    [TestCase("FGuid", "", TestTable.Row1.StrGuid, TestTable.Row2.StrGuid)]
    [TestCase("FEnumInt32", "", TestTable.Row1.StrEnumInt32, TestTable.Row2.StrEnumInt32)]
    [TestCase("FEnumString", "", TestTable.Row1.StrEnumString, TestTable.Row2.StrEnumString)]
    public void AsString(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsString", wantedValues);
    }

    [TestCase("FInt32", 0, TestTable.Row1.VInt32, TestTable.Row2.VInt32)]
    [TestCase("FInt32NN", 0, TestTable.Row1.VInt32, TestTable.Row2.VInt32)]
    [TestCase("FInt64", 0, (int)TestTable.Row1.VInt64, (int)TestTable.Row2.VInt64)]
    [TestCase("FSingle", 0, TestTable.Row1.RoundedSingle, TestTable.Row2.RoundedSingle)]
    [TestCase("FDouble", 0, TestTable.Row1.RoundedDouble, TestTable.Row2.RoundedDouble)]
    [TestCase("FDecimal", 0, TestTable.Row1.RoundedDecimal, TestTable.Row2.RoundedDecimal)]
    [TestCase("FBoolean", 0, TestTable.Row1.VBoolean ? 1 : 0, TestTable.Row2.VBoolean ? 1 : 0)]
    public void AsInt32(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsInt32", wantedValues);
    }

    [TestCase("FInt32", null, TestTable.Row1.VInt32, TestTable.Row2.VInt32)]
    [TestCase("FInt32NN", 0, TestTable.Row1.VInt32, TestTable.Row2.VInt32)]
    [TestCase("FInt64", null, (int)TestTable.Row1.VInt64, (int)TestTable.Row2.VInt64)]
    [TestCase("FSingle", null, TestTable.Row1.RoundedSingle, TestTable.Row2.RoundedSingle)]
    [TestCase("FDouble", null, TestTable.Row1.RoundedDouble, TestTable.Row2.RoundedDouble)]
    [TestCase("FDecimal", null, TestTable.Row1.RoundedDecimal, TestTable.Row2.RoundedDecimal)]
    [TestCase("FBoolean", null, TestTable.Row1.VBoolean ? 1 : 0, TestTable.Row2.VBoolean ? 1 : 0)]
    public void AsNullableInt32(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsNullableInt32", wantedValues);
    }

    [TestCase("FInt32", 0L, (long)TestTable.Row1.VInt32, (long)TestTable.Row2.VInt32)]
    [TestCase("FInt32NN", 0L, (long)TestTable.Row1.VInt32, (long)TestTable.Row2.VInt32)]
    [TestCase("FInt64", 0L, TestTable.Row1.VInt64, TestTable.Row2.VInt64)]
    [TestCase("FSingle", 0L, (long)TestTable.Row1.RoundedSingle, (long)TestTable.Row2.RoundedSingle)]
    [TestCase("FDouble", 0L, (long)TestTable.Row1.RoundedDouble, (long)TestTable.Row2.RoundedDouble)]
    [TestCase("FDecimal", 0L, (long)TestTable.Row1.RoundedDecimal, (long)TestTable.Row2.RoundedDecimal)]
    [TestCase("FBoolean", 0L, TestTable.Row1.VBoolean ? 1L : 0L, TestTable.Row2.VBoolean ? 1L : 0L)]
    public void AsInt64(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsInt64", wantedValues);
    }

    [TestCase("FInt32", null, (long)TestTable.Row1.VInt32, (long)TestTable.Row2.VInt32)]
    [TestCase("FInt32NN", 0L, (long)TestTable.Row1.VInt32, (long)TestTable.Row2.VInt32)]
    [TestCase("FInt64", null, TestTable.Row1.VInt64, TestTable.Row2.VInt64)]
    [TestCase("FSingle", null, (long)TestTable.Row1.RoundedSingle, (long)TestTable.Row2.RoundedSingle)]
    [TestCase("FDouble", null, (long)TestTable.Row1.RoundedDouble, (long)TestTable.Row2.RoundedDouble)]
    [TestCase("FDecimal", null, (long)TestTable.Row1.RoundedDecimal, (long)TestTable.Row2.RoundedDecimal)]
    [TestCase("FBoolean", null, TestTable.Row1.VBoolean ? 1L : 0L, TestTable.Row2.VBoolean ? 1L : 0L)]
    public void AsNullableInt64(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsNullableInt64", wantedValues);
    }

    [TestCase("FInt32", 0f, (float)TestTable.Row1.VInt32, (float)TestTable.Row2.VInt32)]
    [TestCase("FInt32NN", 0f, (float)TestTable.Row1.VInt32, (float)TestTable.Row2.VInt32)]
    [TestCase("FInt64", 0f, (float)TestTable.Row1.VInt64, (float)TestTable.Row2.VInt64)]
    [TestCase("FSingle", 0f, TestTable.Row1.VSingle, TestTable.Row2.VSingle)]
    [TestCase("FDouble", 0f, (float)TestTable.Row1.VDouble, (float)TestTable.Row2.VDouble)]
    [TestCase("FDecimal", 0f, (float)TestTable.Row1.VDecimal, (float)TestTable.Row2.VDecimal)]
    [TestCase("FBoolean", 0f, TestTable.Row1.VBoolean ? 1f : 0f, TestTable.Row2.VBoolean ? 1f : 0f)]
    public void AsSingle(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsSingle", wantedValues);
    }

    [TestCase("FInt32", null, (float)TestTable.Row1.VInt32, (float)TestTable.Row2.VInt32)]
    [TestCase("FInt32NN", 0f, (float)TestTable.Row1.VInt32, (float)TestTable.Row2.VInt32)]
    [TestCase("FInt64", null, (float)TestTable.Row1.VInt64, (float)TestTable.Row2.VInt64)]
    [TestCase("FSingle", null, TestTable.Row1.VSingle, TestTable.Row2.VSingle)]
    [TestCase("FDouble", null, (float)TestTable.Row1.VDouble, (float)TestTable.Row2.VDouble)]
    [TestCase("FDecimal", null, (float)TestTable.Row1.VDecimal, (float)TestTable.Row2.VDecimal)]
    [TestCase("FBoolean", null, TestTable.Row1.VBoolean ? 1f : 0f, TestTable.Row2.VBoolean ? 1f : 0f)]
    public void AsNullableSingle(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsNullableSingle", wantedValues);
    }

    [TestCase("FInt32", 0.0, (double)TestTable.Row1.VInt32, (double)TestTable.Row2.VInt32)]
    [TestCase("FInt32NN", 0.0, (double)TestTable.Row1.VInt32, (double)TestTable.Row2.VInt32)]
    [TestCase("FInt64", 0.0, (double)TestTable.Row1.VInt64, (double)TestTable.Row2.VInt64)]
    [TestCase("FSingle", 0.0, (double)TestTable.Row1.VSingle, (double)TestTable.Row2.VSingle)]
    [TestCase("FDouble", 0.0, TestTable.Row1.VDouble, TestTable.Row2.VDouble)]
    [TestCase("FDecimal", 0.0, (double)TestTable.Row1.VDecimal, (double)TestTable.Row2.VDecimal)]
    [TestCase("FBoolean", 0.0, TestTable.Row1.VBoolean ? 1.0 : 0.0, TestTable.Row2.VBoolean ? 1.0 : 0.0)]
    public void AsDouble(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsDouble", wantedValues);
    }

    [TestCase("FInt32", null, (double)TestTable.Row1.VInt32, (double)TestTable.Row2.VInt32)]
    [TestCase("FInt32NN", 0.0, (double)TestTable.Row1.VInt32, (double)TestTable.Row2.VInt32)]
    [TestCase("FInt64", null, (double)TestTable.Row1.VInt64, (double)TestTable.Row2.VInt64)]
    [TestCase("FSingle", null, (double)TestTable.Row1.VSingle, (double)TestTable.Row2.VSingle)]
    [TestCase("FDouble", null, TestTable.Row1.VDouble, TestTable.Row2.VDouble)]
    [TestCase("FDecimal", null, (double)TestTable.Row1.VDecimal, (double)TestTable.Row2.VDecimal)]
    [TestCase("FBoolean", null, TestTable.Row1.VBoolean ? 1.0 : 0.0, TestTable.Row2.VBoolean ? 1.0 : 0.0)]
    public void AsNullableDouble(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsNullableDouble", wantedValues);
    }

    [TestCase("FInt32", 0.0, (double)TestTable.Row1.VInt32, (double)TestTable.Row2.VInt32)]
    [TestCase("FInt32NN", 0.0, (double)TestTable.Row1.VInt32, (double)TestTable.Row2.VInt32)]
    [TestCase("FInt64", 0.0, (double)TestTable.Row1.VInt64, (double)TestTable.Row2.VInt64)]
    [TestCase("FSingle", 0.0, (double)TestTable.Row1.VSingle, (double)TestTable.Row2.VSingle)]
    [TestCase("FDouble", 0.0, TestTable.Row1.VDouble, TestTable.Row2.VDouble)]
    [TestCase("FDecimal", 0.0, (double)TestTable.Row1.VDecimal, (double)TestTable.Row2.VDecimal)]
    [TestCase("FBoolean", 0.0, TestTable.Row1.VBoolean ? 1.0 : 0.0, TestTable.Row2.VBoolean ? 1.0 : 0.0)]
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

    [TestCase("FInt32", null, (double)TestTable.Row1.VInt32, (double)TestTable.Row2.VInt32)]
    [TestCase("FInt32NN", 0.0, (double)TestTable.Row1.VInt32, (double)TestTable.Row2.VInt32)]
    [TestCase("FInt64", null, (double)TestTable.Row1.VInt64, (double)TestTable.Row2.VInt64)]
    [TestCase("FSingle", null, (double)TestTable.Row1.VSingle, (double)TestTable.Row2.VSingle)]
    [TestCase("FDouble", null, TestTable.Row1.VDouble, TestTable.Row2.VDouble)]
    [TestCase("FDecimal", null, (double)TestTable.Row1.VDecimal, (double)TestTable.Row2.VDecimal)]
    [TestCase("FBoolean", null, TestTable.Row1.VBoolean ? 1.0 : 0.0, TestTable.Row2.VBoolean ? 1.0 : 0.0)]
    public void AsNullableDecimal(string columnName, params double?[] dWantedValues)
    {
      object[] wantedValues = ToDecimal(dWantedValues);
      DoTest(columnName, "AsNullableDecimal", wantedValues);
    }

    [TestCase("FInt32", false, TestTable.Row1.VInt32 != 0, TestTable.Row2.VInt32 != 0)]
    [TestCase("FInt32NN", false, TestTable.Row1.VInt32 != 0, TestTable.Row2.VInt32 != 0)]
    [TestCase("FInt64", false, TestTable.Row1.VInt64 != 0L, TestTable.Row2.VInt64 != 0L)]
    [TestCase("FSingle", false, TestTable.Row1.VSingle != 0f, TestTable.Row2.VSingle != 0f)]
    [TestCase("FDouble", false, TestTable.Row1.VDouble != 0.0, TestTable.Row2.VDouble != 0.0)]
    [TestCase("FDecimal", false, (double)TestTable.Row1.VDecimal != 0.0, (double)TestTable.Row2.VDecimal != 0.0)]
    [TestCase("FBoolean", false, TestTable.Row1.VBoolean, TestTable.Row2.VBoolean)]
    public void AsBoolean(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "AsBoolean", wantedValues);
    }

    [TestCase("FDateTime", Creators.StrDateMinValue, TestTable.Row1.StrDateTime, TestTable.Row2.StrDateTime)]
    public void AsDateTime(string columnName, params string[] sWantedValues)
    {
      object[] wantedValues = new object[sWantedValues.Length];
      for (int i = 0; i < wantedValues.Length; i++)
        wantedValues[i] = Creators.NDateTime(sWantedValues[i]);
      DoTest(columnName, "AsDateTime", wantedValues);
    }

    [TestCase("FDateTime", "", TestTable.Row1.StrDateTime, TestTable.Row2.StrDateTime)]
    public void AsNullableDateTime(string columnName, params string[] sWantedValues)
    {
      object[] wantedValues = new object[sWantedValues.Length];
      for (int i = 0; i < wantedValues.Length; i++)
        wantedValues[i] = Creators.NDateTime(sWantedValues[i]);
      DoTest(columnName, "AsNullableDateTime", wantedValues);
    }

    [TestCase("FTimeSpan", Creators.StrTimeSpanZeroValue, TestTable.Row1.StrTimeSpan, TestTable.Row2.StrTimeSpan)]
    public void AsTimeSpan(string columnName, params string[] sWantedValues)
    {
      object[] wantedValues = new object[sWantedValues.Length];
      for (int i = 0; i < wantedValues.Length; i++)
        wantedValues[i] = Creators.NTimeSpan(sWantedValues[i]);
      DoTest(columnName, "AsTimeSpan", wantedValues);
    }

    [TestCase("FGuid", Creators.StrGuidEmpty, TestTable.Row1.StrGuid, TestTable.Row2.StrGuid)]
    //[TestCase("FGuidString", StrGuid1, StrGuid2, StrGuid3, StrGuidEmpty)]
    //[TestCase("FGuidBytes", StrGuid1, StrGuid2, StrGuid3, StrGuidEmpty)]
    public void AsGuid(string columnName, params string[] sWantedValues)
    {
      object[] wantedValues = new object[sWantedValues.Length];
      for (int i = 0; i < wantedValues.Length; i++)
        wantedValues[i] = Creators.NGuid(sWantedValues[i]);
      DoTest(columnName, "AsGuid", wantedValues);
    }

    [TestCase("FEnumInt32", TestEnum.Zero, TestTable.Row1.VEnum, TestTable.Row2.VEnum)]
    [TestCase("FEnumString", TestEnum.Zero, TestTable.Row1.VEnum, TestTable.Row2.VEnum)]
    public void GetEnum(string columnName, params TestEnum[] wantedValues)
    {
      DataTable table = TestTable.Create();
      DbDataReader reader = table.CreateDataReader();

      DbDataReaderValue sut = new DbDataReaderValue(reader, columnName);

      Assert.AreEqual(table.Rows.Count, wantedValues.Length, "WantedValues.Length");

      reader.NextResult();
      List<object> res = new List<object>();
      while (reader.Read())
      {
        res.Add(sut.GetEnum<TestEnum>());
      }

      CollectionAssert.AreEqual(wantedValues, res.ToArray(), sut.ColumnName);
    }

    #endregion

    #region Прочие свойства

    [TestCase("FString", true, false, false)]
    [TestCase("FStringNN", false, false, false)]
    [TestCase("FInt32", true, false, false)]
    [TestCase("FInt32NN", false, false, false)]
    public void IsNull(string columnName, params object[] wantedValues)
    {
      DoTest(columnName, "IsNull", wantedValues);
    }


    [TestCase("FString", null, TestTable.Row1.VString, TestTable.Row2.VString)]
    [TestCase("FStringNN", "", TestTable.Row1.VString, TestTable.Row2.VString)]
    [TestCase("FInt32", null, TestTable.Row1.VInt32, TestTable.Row2.VInt32)]
    [TestCase("FInt32NN", 0, TestTable.Row1.VInt32, TestTable.Row2.VInt32)]
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
      DataTable table = TestTable.Create();
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
        if (v is DBNull)
          v = null;
        res.Add(v);
      }

      CollectionAssert.AreEqual(wantedValues, res.ToArray(), sut.ColumnName);
    }

    #endregion
  }

  [TestFixture]
  public class DbDataReaderValuesTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DataTable table = TestTable.Create();
      DataTableReader reader = new DataTableReader(table);

      DbDataReaderValues sut = new DbDataReaderValues(reader);
      Assert.AreSame(reader, sut.Reader, "Reader");
      CollectionAssert.AreEqual(TestTable.ColumnNames, sut.ColumnNames, "ColumnNames");
    }

    #endregion

    #region IDataValueAccess

    [TestCase("FInt32", true)]
    [TestCase("fiNT32", true)]
    [TestCase("FInt33", false)]
    [TestCase("", false)]
    public void Contains(string columnName, bool wantedRes)
    {
      DataTable table = TestTable.Create();
      DataTableReader reader = new DataTableReader(table);

      DbDataReaderValues sut = new DbDataReaderValues(reader);
      INamedValuesAccess sut2 = sut;
      bool res = sut2.Contains(columnName);
      Assert.AreEqual(wantedRes, res);
    }

    public struct GetValueTestData
    {
      #region Конструктор

      public GetValueTestData(int rowIndex, string columnName, object wantedValue)
      {
        _RowIndex = rowIndex;
        _ColumnName = columnName;
        _WantedValue = wantedValue;
      }

      #endregion

      #region Свойства

      public int RowIndex { get { return _RowIndex; } }
      private readonly int _RowIndex;

      public string ColumnName { get { return _ColumnName; } }
      private readonly string _ColumnName;

      public object WantedValue { get { return _WantedValue; } }
      private readonly object _WantedValue;

      public override string ToString()
      {
        return "RowIndex=" + RowIndex.ToString() + ", ColumnName=" + ColumnName;
      }

      #endregion
    }

    public static GetValueTestData[] GetValueSource
    {
      get
      {
        return new GetValueTestData[]
        {
          new GetValueTestData(0, "FString", DBNull.Value),
          new GetValueTestData(0, "FStringNN", ""),
          new GetValueTestData(0, "FInt32", DBNull.Value),
          new GetValueTestData(0, "FInt32NN", 0),
          new GetValueTestData(1, "FInt32", TestTable.Row1.VInt32),
          new GetValueTestData(2, "FInt32", TestTable.Row2.VInt32),
        };
      }
    }

    [TestCaseSource("GetValueSource")]
    public void GetValue(GetValueTestData source)
    {
      DataTable table = TestTable.Create();
      DataTableReader reader = new DataTableReader(table);
      for (int i = 0; i <= source.RowIndex; i++)
        reader.Read();

      DbDataReaderValues sut = new DbDataReaderValues(reader);
      object res = sut.GetValue(source.ColumnName);
      Assert.AreEqual(source.WantedValue, res);
    }

    #endregion

    #region Доступ к DbDataReaderValue

    [Test]
    public void Item_ColumnIndex()
    {
      DataTable table = TestTable.Create();
      DataTableReader reader = new DataTableReader(table);
      reader.Read();
      reader.Read();

      DbDataReaderValues sut = new DbDataReaderValues(reader);

      for (int i = 0; i < TestTable.ColumnNames.Length; i++)
      {
        DbDataReaderValue res = sut[i];

        Assert.AreEqual(TestTable.ColumnNames[i], res.ColumnName, "ColumnName");
        Assert.AreEqual(table.Rows[1][i], res.Value, "Value");
      }
    }

    [Test]
    public void Item_ColumnIndex_OutOfRangeException()
    {
      DataTable table = TestTable.Create();
      DataTableReader reader = new DataTableReader(table);

      DbDataReaderValues sut = new DbDataReaderValues(reader);
      DbDataReaderValue dummy;
      Assert.Catch<ArgumentOutOfRangeException>(delegate () { dummy = sut[-1]; });
      Assert.Catch<ArgumentOutOfRangeException>(delegate () { dummy = sut[TestTable.ColumnNames.Length]; });
    }


    [Test]
    public void Item_ColumnName()
    {
      DataTable table = TestTable.Create();
      DataTableReader reader = new DataTableReader(table);
      reader.Read();
      reader.Read();

      DbDataReaderValues sut = new DbDataReaderValues(reader);

      for (int i = 0; i < TestTable.ColumnNames.Length; i++)
      {
        DbDataReaderValue res = sut[TestTable.ColumnNames[i].ToLowerInvariant()];

        Assert.AreEqual(TestTable.ColumnNames[i], res.ColumnName, "ColumnName");
        Assert.AreEqual(table.Rows[1][i], res.Value, "Value");
      }
    }

    #endregion
  }
}
