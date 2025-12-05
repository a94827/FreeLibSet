using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using NUnit.Framework;
using FreeLibSet.Core;
using System.ComponentModel;
using FreeLibSet.Tests;
using FreeLibSet.Data;

namespace ExtTools_tests.Core
{
  [TestFixture]
  class DataToolsTests_Other
  {
    #region GetXXX() (не связанные с DataTable)

    #region GetString()

    public class DummyToStringTestObject
    {
      public override string ToString()
      {
        return "XXX";
      }
    }

    public static readonly TestPair[] GetStringTests = new TestPair[] {
      new TestPair(null, String.Empty),
      new TestPair(DBNull.Value, String.Empty),
      new TestPair(String.Empty, String.Empty),
      new TestPair("Abc ", "Abc"), // с удалением пробелов
      new TestPair(false, Boolean.FalseString),
      new TestPair(true, Boolean.TrueString),
      new TestPair(123, "123"),
      new TestPair(-123456789L, "-123456789"),
      new TestPair(-1.2f, "-1.2"),
      new TestPair(-1.2, "-1.2"),
      new TestPair(-1.2m, "-1.2"),
      new TestPair(new DateTime(2023, 5, 17, 12, 34, 56), "2023-05-17T12:34:56"),
      new TestPair(new TimeSpan(12,34,56), "12:34:56"),
      new TestPair(Guid.Empty, Guid.Empty.ToString()),
      new TestPair(new DummyToStringTestObject(), "XXX")};

    [TestCaseSource("GetStringTests")]
    public void GetString(TestPair info)
    {
      string res = DataTools.GetString(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    #endregion

    #region GetInt32(), GetNullableInt32()

    public static readonly TestPair[] GetInt32Tests = new TestPair[] {
      new TestPair(null, 0),
      new TestPair("", 0),
      new TestPair("-123", -123),
      new TestPair(123, 123),
      new TestPair(123L, 123),
      new TestPair(123f, 123),
      new TestPair(123.0, 123),
      new TestPair(123m, 123),
      new TestPair(false, 0),
      new TestPair(true, 1) };
    [TestCaseSource("GetInt32Tests")]
    public void GetInt32(TestPair info)
    {
      int res = DataTools.GetInt32(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    public static readonly object[] GetInt32ExceptionTests = new object[] {
      new DateTime(2023, 5, 17),
      TimeSpan.Zero,
      "ABC",
      999999999999L,
      999999999999.0,
      999999999999m,
      Guid.Empty,
      EmptyArray<byte>.Empty };

    [TestCaseSource("GetInt32ExceptionTests")]
    public void GetInt32_exceptions(object source)
    {
      Assert.Catch<SystemException>(delegate () { DataTools.GetInt32(source); });
    }


    public static readonly TestPair[] GetNullableInt32Tests = new TestPair[] {
      new TestPair(null, null),
      new TestPair("", null),
      new TestPair("-123", -123),
      new TestPair(123, 123),
      new TestPair(123L, 123),
      new TestPair(123f, 123),
      new TestPair(123.0, 123),
      new TestPair(123m, 123),
      new TestPair(false, 0),
      new TestPair(true, 1) };
    [TestCaseSource("GetNullableInt32Tests")]
    public void GetNullableInt32(TestPair info)
    {
      int? res = DataTools.GetNullableInt32(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    [TestCaseSource("GetInt32ExceptionTests")]
    public void GetNullableInt32_exceptions(object source)
    {
      Assert.Catch<SystemException>(delegate () { DataTools.GetNullableInt32(source); });
    }

    #endregion

    #region GetInt64(), GetNullableInt64()

    public static readonly TestPair[] GetInt64Tests = new TestPair[] {
      new TestPair(null, 0L),
      new TestPair("", 0L),
      new TestPair("-123", -123L),
      new TestPair(123, 123L),
      new TestPair(123L, 123L),
      new TestPair(123f, 123L),
      new TestPair(123.0, 123L),
      new TestPair(123m, 123L),
      new TestPair(false, 0L),
      new TestPair(true, 1L) };
    [TestCaseSource("GetInt64Tests")]
    public void GetInt64(TestPair info)
    {
      long res = DataTools.GetInt64(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    public static readonly object[] GetInt64ExceptionTests = new object[] {
      new DateTime(2023, 5, 17),
      TimeSpan.Zero,
      "ABC",
      Guid.Empty,
      Double.MaxValue,
      Decimal.MaxValue,
      EmptyArray<byte>.Empty };

    [TestCaseSource("GetInt64ExceptionTests")]
    public void GetInt64_exceptions(object source)
    {
      Assert.Catch<SystemException>(delegate () { DataTools.GetInt64(source); });
    }

    public static readonly TestPair[] GetNullableInt64Tests = new TestPair[] {
      new TestPair(null, null),
      new TestPair("", null),
      new TestPair("-123", -123L),
      new TestPair(123, 123L),
      new TestPair(123L, 123L),
      new TestPair(123f, 123L),
      new TestPair(123.0, 123L),
      new TestPair(123m, 123L),
      new TestPair(false, 0L),
      new TestPair(true, 1L) };
    [TestCaseSource("GetNullableInt64Tests")]
    public void GetNullableInt64(TestPair info)
    {
      long? res = DataTools.GetNullableInt64(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    [TestCaseSource("GetInt64ExceptionTests")]
    public void GetNullableInt64_exceptions(object source)
    {
      Assert.Catch<SystemException>(delegate () { DataTools.GetNullableInt64(source); });
    }

    #endregion

    #region GetSingle(), GetNullableSingle()

    public static readonly TestPair[] GetSingleTests = new TestPair[] {
      new TestPair(null, 0f),
      new TestPair("", 0f),
      new TestPair("-123", -123f),
      new TestPair(123, 123f),
      new TestPair(123L, 123f),
      new TestPair(123f, 123f),
      new TestPair(123.0, 123f),
      new TestPair(123m, 123f),
      new TestPair(false, 0f),
      new TestPair(true, 1f) };
    [TestCaseSource("GetSingleTests")]
    public void GetSingle(TestPair info)
    {
      float res = DataTools.GetSingle(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    public static readonly object[] GetSingleExceptionTests = new object[] {
      new DateTime(2023, 5, 17),
      TimeSpan.Zero,
      "ABC",
      //Double.MaxValue, Не возникает исключения. Возвращается Single.Infinity
      Guid.Empty,
      EmptyArray<byte>.Empty };

    [TestCaseSource("GetSingleExceptionTests")]
    public void GetSingle_exceptions(object source)
    {
      Assert.Catch<SystemException>(delegate () { DataTools.GetSingle(source); });
    }

    public static readonly TestPair[] GetNullableSingleTests = new TestPair[] {
      new TestPair(null, null),
      new TestPair("", null),
      new TestPair("-123", -123f),
      new TestPair(123, 123f),
      new TestPair(123L, 123f),
      new TestPair(123f, 123f),
      new TestPair(123.0, 123f),
      new TestPair(123m, 123f),
      new TestPair(false, 0f),
      new TestPair(true, 1f) };
    [TestCaseSource("GetNullableSingleTests")]
    public void GetNullableSingle(TestPair info)
    {
      float? res = DataTools.GetNullableSingle(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    [TestCaseSource("GetSingleExceptionTests")]
    public void GetNullableSingle_exceptions(object source)
    {
      float? dummy;
      Assert.Catch<SystemException>(delegate () { dummy = DataTools.GetNullableSingle(source); });
    }

    #endregion

    #region GetDouble() / GetNullableDouble()

    public static readonly TestPair[] GetDoubleTests = new TestPair[] {
      new TestPair(null, 0.0),
      new TestPair("", 0.0),
      new TestPair("-123", -123.0),
      new TestPair(123, 123.0),
      new TestPair(123L, 123.0),
      new TestPair(123f, 123.0),
      new TestPair(123.0, 123.0),
      new TestPair(123m, 123.0),
      new TestPair(false, 0.0),
      new TestPair(true, 1.0) };
    [TestCaseSource("GetDoubleTests")]
    public void GetDouble(TestPair info)
    {
      double res = DataTools.GetDouble(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    public static readonly object[] GetDoubleExceptionTests = new object[] {
      new DateTime(2023, 5, 17),
      TimeSpan.Zero,
      "ABC",
      Guid.Empty,
      EmptyArray<byte>.Empty };

    [TestCaseSource("GetDoubleExceptionTests")]
    public void GetDouble_exceptions(object source)
    {
      Assert.Catch<SystemException>(delegate () { DataTools.GetDouble(source); });
    }

    public static readonly TestPair[] GetNullableDoubleTests = new TestPair[] {
      new TestPair(null, null),
      new TestPair("", null),
      new TestPair("-123", -123.0),
      new TestPair(123, 123.0),
      new TestPair(123L, 123.0),
      new TestPair(123f, 123.0),
      new TestPair(123.0, 123.0),
      new TestPair(123m, 123.0),
      new TestPair(false, 0.0),
      new TestPair(true, 1.0) };
    [TestCaseSource("GetNullableDoubleTests")]
    public void GetNullableDouble(TestPair info)
    {
      double? res = DataTools.GetNullableDouble(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    [TestCaseSource("GetDoubleExceptionTests")]
    public void GetNullableDouble_exceptions(object source)
    {
      Assert.Catch<SystemException>(delegate () { DataTools.GetNullableDouble(source); });
    }

    #endregion

    #region GetDecimal(), GetNullableDecimal()

    public static readonly TestPair[] GetDecimalTests = new TestPair[] {
      new TestPair(null, 0m),
      new TestPair("", 0m),
      new TestPair("-123", -123m),
      new TestPair(123, 123m),
      new TestPair(123L, 123m),
      new TestPair(123f, 123m),
      new TestPair(123.0, 123m),
      new TestPair(123m, 123m),
      new TestPair(false, 0m),
      new TestPair(true, 1m) };
    [TestCaseSource("GetDecimalTests")]
    public void GetDecimal(TestPair info)
    {
      decimal res = DataTools.GetDecimal(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    public static readonly object[] GetDecimalExceptionTests = new object[] {
      new DateTime(2023, 5, 17),
      TimeSpan.Zero,
      "ABC",
      Guid.Empty,
      EmptyArray<byte>.Empty };

    [TestCaseSource("GetDecimalExceptionTests")]
    public void GetDecimal_exceptions(object source)
    {
      Assert.Catch<SystemException>(delegate () { DataTools.GetDecimal(source); });
    }

    public static readonly TestPair[] GetNullableDecimalTests = new TestPair[] {
      new TestPair(null, null),
      new TestPair("", null),
      new TestPair("-123", -123m),
      new TestPair(123, 123m),
      new TestPair(123L, 123m),
      new TestPair(123f, 123m),
      new TestPair(123.0, 123m),
      new TestPair(123m, 123m),
      new TestPair(false, 0m),
      new TestPair(true, 1m) };
    [TestCaseSource("GetNullableDecimalTests")]
    public void GetNullableDecimal(TestPair info)
    {
      decimal? res = DataTools.GetNullableDecimal(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    [TestCaseSource("GetDecimalExceptionTests")]
    public void GetNullableDecimal_exceptions(object source)
    {
      Assert.Catch<SystemException>(delegate () { DataTools.GetNullableDecimal(source); });
    }

    #endregion

    #region GetBoolean()

    public static readonly TestPair[] GetBooleanTests = new TestPair[] {
      new TestPair(null, false),
      new TestPair("", false),
      new TestPair("false", false),
      new TestPair("true", true),
      new TestPair("0", false),
      new TestPair("1", true),
      new TestPair(0, false),
      new TestPair(1, true),
      new TestPair(123, true),
      new TestPair(-123, true),
      new TestPair(0L, false),
      new TestPair(123L, true),
      new TestPair(-123L, true),
      new TestPair(123f, true),
      new TestPair(123.0, true),
      new TestPair(123m, true),
      new TestPair(false, false),
      new TestPair(true, true) };
    [TestCaseSource("GetBooleanTests")]
    public void GetBoolean(TestPair info)
    {
      bool res = DataTools.GetBoolean(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    public static readonly object[] GetBooleanExceptionTests = new object[] {
      new DateTime(2023, 5, 17),
      TimeSpan.Zero,
      "ABC",
      Guid.Empty,
      EmptyArray<byte>.Empty };

    [TestCaseSource("GetBooleanExceptionTests")]
    public void GetBoolean_exceptions(object source)
    {
      // Может быть InvalidCastException или FormatException
      Assert.Catch<SystemException>(delegate () { DataTools.GetBoolean(source); });
    }

    #endregion

    #region GetDateTime(), GetNullableDateTime()

    public static readonly TestPair[] GetDateTime_ObjectTests = new TestPair[] {
      new TestPair(null, DateTime.MinValue),
      new TestPair("", DateTime.MinValue),
      new TestPair("2023-05-19", new DateTime(2023, 5, 19)),
      new TestPair("2023-05-19T12:34:56", new DateTime(2023, 5, 19, 12, 34, 56)),
      new TestPair(new DateTime(2023, 5, 19), new DateTime(2023, 5, 19)),
      new TestPair(TimeSpan.Zero, DateTime.MinValue),
      new TestPair(new TimeSpan(12, 34, 56), DateTime.MinValue + new TimeSpan(12, 34, 56)),
      new TestPair(new TimeSpan(-1, -2, -3), DateTime.MinValue + new TimeSpan(22, 57, 57))};
    [TestCaseSource("GetDateTime_ObjectTests")]
    public void GetDateTime_Object(TestPair info)
    {
      DateTime res = DataTools.GetDateTime(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    public static readonly TestPair[] GetDateTime_TimeSpanTests = new TestPair[] {
      new TestPair(new TimeSpan(12, 34, 56), DateTime.MinValue + new TimeSpan(12, 34, 56)),
      new TestPair(new TimeSpan(-1, -2, -3), DateTime.MinValue + new TimeSpan(22, 57, 57)),
      new TestPair(new TimeSpan(10, 12, 34, 56), DateTime.MinValue + new TimeSpan(12, 34, 56)),
      new TestPair(new TimeSpan(-10, -1, -2, -3), DateTime.MinValue + new TimeSpan(22, 57, 57))};
    [TestCaseSource("GetDateTime_TimeSpanTests")]
    public void GetDateTime_TimeSpan(TestPair info)
    {
      TimeSpan ts = (TimeSpan)(info.Source);

      DateTime res = DataTools.GetDateTime(ts);
      Assert.AreEqual(info.Result, res);
    }

    [TestCaseSource("GetDateTime_TimeSpanTests")]
    public void GetDateTime_TimeSpan_Date(TestPair info)
    {
      TimeSpan ts = (TimeSpan)(info.Source);

      DateTime res1 = DataTools.GetDateTime(ts, new DateTime(2023, 5, 31, 12, 34, 56));
      Assert.AreEqual(new DateTime(2023, 5, 31), res1.Date, "Date");
      TimeSpan res2 = res1 - new DateTime(2023, 5, 31);
      Assert.AreEqual(((DateTime)(info.Result)).TimeOfDay, res2, "Time");
    }

    public static readonly object[] GetDateTimeExceptionTests = new object[] {
      0,
      1000,
      "ABC",
      Guid.Empty,
      EmptyArray<byte>.Empty };

    [TestCaseSource("GetDateTimeExceptionTests")]
    public void GetDateTime_exceptions(object source)
    {
      Assert.Catch<SystemException>(delegate () { DataTools.GetDateTime(source); });
    }

    public static readonly TestPair[] GetNullableDateTimeTests = new TestPair[] {
      new TestPair(null, null),
      new TestPair("", null),
      new TestPair("2023-05-19", new DateTime(2023, 5, 19)),
      new TestPair("2023-05-19T12:34:56", new DateTime(2023, 5, 19, 12, 34, 56)),
      new TestPair(new DateTime(2023, 5, 19), new DateTime(2023, 5, 19)),
      new TestPair(TimeSpan.Zero, DateTime.MinValue),
      new TestPair(new TimeSpan(12, 34, 56), DateTime.MinValue + new TimeSpan(12, 34, 56)) };
    [TestCaseSource("GetNullableDateTimeTests")]
    public void GetNullableDateTime(TestPair info)
    {
      DateTime? res = DataTools.GetNullableDateTime(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    [TestCaseSource("GetDateTimeExceptionTests")] // исключения такие же
    public void GetNullableDateTime_exceptions(object source)
    {
      Assert.Catch<SystemException>(delegate () { DataTools.GetNullableDateTime(source); });
    }

    #endregion

    #region GetTimeSpan()

    public static readonly TestPair[] GetTimeSpanTests = new TestPair[] {
      new TestPair(null, TimeSpan.Zero),
      new TestPair("", TimeSpan.Zero),
      new TestPair("12:34:56", new TimeSpan(12, 34, 56)),
      new TestPair("-1:2:3", new TimeSpan(-1, -2, -3)), // отрицательные значения не сворачиваются
      new TestPair("3.4:5:6", new TimeSpan(3, 4, 5, 6)), // переход на следующий день не сворачивается
      new TestPair(new TimeSpan(-1,-2,-3,-4), new TimeSpan(-1,-2,-3,-4)),
      new TestPair(new DateTime(2023,5,19,12,34,56), new TimeSpan(12,34,56)) };
    [TestCaseSource("GetTimeSpanTests")]
    public void GetTimeSpan(TestPair info)
    {
      TimeSpan res = DataTools.GetTimeSpan(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    public static readonly object[] GetTimeSpanExceptionTests = new object[] {
      0,
      false,
      "ABC",
      Guid.Empty,
      EmptyArray<byte>.Empty };

    [TestCaseSource("GetTimeSpanExceptionTests")]
    public void GetTimeSpan_exceptions(object source)
    {
      Assert.Catch<SystemException>(delegate () { DataTools.GetTimeSpan(source); });
    }

    #endregion

    #region GetGuid()

    private static readonly Guid TestGuid = new Guid("83e4ea91-6f0b-4ab6-9d58-c981418c26b7");

    public static readonly TestPair[] GetGuidTests = new TestPair[] {
      new TestPair(null, Guid.Empty),
      new TestPair("", Guid.Empty),
      new TestPair(Guid.Empty.ToString(), Guid.Empty),
      new TestPair(TestGuid.ToString("N"), TestGuid),
      new TestPair(TestGuid.ToString("D"), TestGuid),
      new TestPair(TestGuid.ToString("B"), TestGuid),
      new TestPair(TestGuid.ToString("P"), TestGuid),
      new TestPair(TestGuid.ToByteArray(), TestGuid)
    };
    [TestCaseSource("GetGuidTests")]
    public void GetGuid(TestPair info)
    {
      Guid res = DataTools.GetGuid(info.Source);
      Assert.AreEqual(info.Result, res);
    }

    public static readonly object[] GetGuidExceptionTests = new object[] {
      0,
      false,
      "ABC",
      new DateTime(2023, 05, 19)};

    [TestCaseSource("GetGuidExceptionTests")]
    public void GetGuid_exceptions(object source)
    {
      Assert.Catch<SystemException>(delegate () { DataTools.GetGuid(source); });
    }

    #endregion

    #endregion

    #region CloneDataColumn

    [Test]
    public void CloneDataColumn()
    {
      DataColumn col1 = new DataColumn("AAA", typeof(string));

      DataColumn col2 = DataTools.CloneDataColumn(col1);

      PropertyAssert.AreEqual(col1, col2);
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

      Assert.Catch(delegate () { DataTools.SetPrimaryKey(tbl, "F4"); }, "Unknown column name");
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

      DataTools.SetPrimaryKey(tbl, EmptyArray<string>.Empty);
      Assert.AreEqual(0, tbl.PrimaryKey.Length, "#3.Length");

      Assert.Catch(delegate () { DataTools.SetPrimaryKey(tbl, new string[] { "F4" }); }, "Unknown column name");
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
      Assert.Catch(delegate () { DataTools.FindOrAddPrimaryKeyRow(tbl, keys); }, "extra keys");

      keys = new object[] { };
      Assert.Catch(delegate () { DataTools.FindOrAddPrimaryKeyRow(tbl, keys); }, "less keys #1");

      DataTools.SetPrimaryKey(tbl, "F1,F2");
      Assert.Catch(delegate () { DataTools.FindOrAddPrimaryKeyRow(tbl, 1); }, "less keys #2");
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
      DataSet ds = new DataSet();
      DataTable tbl01 = CreateTestTable123();
      tbl01.TableName = "T1";
      ds.Tables.Add(tbl01);
      DataTable tbl02 = CreateTestTable123();
      tbl02.TableName = "T2";
      ds.Tables.Add(tbl02);

      DataTable tbl = new DataTable("T1");
      tbl.Columns.Add("F1", typeof(string));
      DataTools.AddTableToDataSet(ds, tbl);
      Assert.AreSame(ds, tbl.DataSet, "DataSet");
      Assert.AreEqual(2, ds.Tables.Count, "Tables.Count");

      Assert.IsNull(tbl01.DataSet, "Old table #1 has been removed");
      Assert.AreSame(ds, tbl02.DataSet, "Old table #2 stays in dataset");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void AddTableToDataSet_DefaultView_Props(bool replace)
    {
      DataSet ds = new DataSet();
      if (replace)
      {
        DataTable tblOld = CreateTestTable123();
        tblOld.TableName = "T1";
        ds.Tables.Add(tblOld);
      }

      DataTable tbl = CreateTestTable123();
      tbl.TableName = "T1";
      tbl.AcceptChanges();
      tbl.Rows.Add(10, 11, 12); // в состоянии Added
      Assert.AreEqual(4, tbl.Rows.Count, "DataTable.RowCount");

      tbl.DefaultView.Sort = "F1 DESC";
      tbl.DefaultView.RowFilter = "F3>3"; // первая строка отключена
      tbl.DefaultView.RowStateFilter = DataViewRowState.Unchanged; // последняя строка отключена

      Assert.AreEqual(2, tbl.DefaultView.Count, "Count before");
      Assert.AreSame(tbl.Rows[2], tbl.DefaultView[0].Row, "Row1 before");
      Assert.AreSame(tbl.Rows[1], tbl.DefaultView[1].Row, "Row2 before");

      DataTools.AddTableToDataSet(ds, tbl);
      Assert.AreEqual(2, tbl.DefaultView.Count, "Count after");
      Assert.AreSame(tbl.Rows[2], tbl.DefaultView[0].Row, "Row1 after");
      Assert.AreSame(tbl.Rows[1], tbl.DefaultView[1].Row, "Row2 after");
    }

    #endregion

    #region GetDataTableRows()

    [Test]
    public void GetDataTableRows_DataTable()
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


    [Test]
    public void GetDataTableRows_DataTable_null()
    {
      DataTable tbl = null;

      DataRow[] rows = DataTools.GetDataTableRows(tbl);
      Assert.AreEqual(0, rows.Length, "Length");
    }


    //[Test]
    //public void GetDataTableRows_DataView()
    //{
    //  DataTable tbl = CreateTestTable123();
    //  DataView dv = new DataView(tbl);
    //  dv.Sort = "F1 DESC";

    //  DataRow[] rows = DataTools.GetDataTableRows(dv);
    //  Assert.AreEqual(3, rows.Length, "Length");
    //  Assert.AreSame(tbl.Rows[2], rows[0], "[0]");
    //  Assert.AreSame(tbl.Rows[1], rows[1], "[1]");
    //  Assert.AreSame(tbl.Rows[0], rows[2], "[2]");
    //}

    #endregion

    #region GetDataRowEnumerable()

    [Test]
    public void GetDataRowEnumerable()
    {
      DataTable tbl = CreateTestTable123();
      DoTestGetDataRowEnumerable(tbl, "1,4,7", "DataTable");
      DoTestGetDataRowEnumerable(tbl.DefaultView, "1,4,7", "DataView");
      DoTestGetDataRowEnumerable(DataTools.GetDataTableRows(tbl), "1,4,7", "DataRow[]");
      DoTestGetDataRowEnumerable(new DataRowView[] { tbl.DefaultView[1], tbl.DefaultView[2] }, "4,7", "DataRowView[]");

      // Пока не поддерживается
      //DataSet ds = new DataSet();
      //ds.Tables.Add(tbl);
      //DoTestGetDataRowEnumerable(ds, "1,4,7", "DataSet");

      DoTestGetDataRowEnumerable(null, "", "Null");
    }

    private static void DoTestGetDataRowEnumerable(object x, string wanted, string message)
    {
      List<int> lst = new List<int>();
      foreach (DataRow row in DataTools.GetDataRowEnumerable(x))
        lst.Add(DataTools.GetInt32(row, "F1"));

      string sRes = StdConvert.ToString(lst.ToArray());

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

    #region GetUniqueXXXValues()

    #region GetUniqueInt32Values()

    [Test]
    public void GetUniqueInt32Values_skipNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Rows.Add(1);
      tbl.Rows.Add(2);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(2);
      tbl.Rows.Add(DBNull.Value);

      int[] wanted = new int[] { 1, 2 };

      int[] res1 = DataTools.GetUniqueInt32Values(tbl, "F1", true);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      int[] res2 = DataTools.GetUniqueInt32Values(tbl.DefaultView, "F1", true);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      int[] res3 = DataTools.GetUniqueInt32Values(DataTools.GetDataTableRows(tbl), "F1", true);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    [Test]
    public void GetUniqueInt32Values_withNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Rows.Add(1);
      tbl.Rows.Add(2);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(2);
      tbl.Rows.Add(DBNull.Value);

      int[] wanted = new int[] { 0, 1, 2 };

      int[] res1 = DataTools.GetUniqueInt32Values(tbl, "F1", false);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      int[] res2 = DataTools.GetUniqueInt32Values(tbl.DefaultView, "F1", false);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      int[] res3 = DataTools.GetUniqueInt32Values(DataTools.GetDataTableRows(tbl), "F1", false);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    #endregion

    #region GetUniqueInt64Values()

    [Test]
    public void GetUniqueInt64Values_skipNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(long));
      tbl.Rows.Add(1L);
      tbl.Rows.Add(2L);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(2L);
      tbl.Rows.Add(DBNull.Value);

      long[] wanted = new long[] { 1L, 2L };

      long[] res1 = DataTools.GetUniqueInt64Values(tbl, "F1", true);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      long[] res2 = DataTools.GetUniqueInt64Values(tbl.DefaultView, "F1", true);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      long[] res3 = DataTools.GetUniqueInt64Values(DataTools.GetDataTableRows(tbl), "F1", true);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    [Test]
    public void GetUniqueInt64Values_withNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(long));
      tbl.Rows.Add(1L);
      tbl.Rows.Add(2L);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(2L);
      tbl.Rows.Add(DBNull.Value);

      long[] wanted = new long[] { 0L, 1L, 2L };

      long[] res1 = DataTools.GetUniqueInt64Values(tbl, "F1", false);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      long[] res2 = DataTools.GetUniqueInt64Values(tbl.DefaultView, "F1", false);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      long[] res3 = DataTools.GetUniqueInt64Values(DataTools.GetDataTableRows(tbl), "F1", false);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    #endregion

    #region GetUniqueSingleValues()

    [Test]
    public void GetUniqueSingleValues_skipNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(float));
      tbl.Rows.Add(1f);
      tbl.Rows.Add(2f);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(2f);
      tbl.Rows.Add(DBNull.Value);

      float[] wanted = new float[] { 1f, 2f };

      float[] res1 = DataTools.GetUniqueSingleValues(tbl, "F1", true);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      float[] res2 = DataTools.GetUniqueSingleValues(tbl.DefaultView, "F1", true);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      float[] res3 = DataTools.GetUniqueSingleValues(DataTools.GetDataTableRows(tbl), "F1", true);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    [Test]
    public void GetUniqueSingleValues_withNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(float));
      tbl.Rows.Add(1f);
      tbl.Rows.Add(2f);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(2f);
      tbl.Rows.Add(DBNull.Value);

      float[] wanted = new float[] { 0f, 1f, 2f };

      float[] res1 = DataTools.GetUniqueSingleValues(tbl, "F1", false);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      float[] res2 = DataTools.GetUniqueSingleValues(tbl.DefaultView, "F1", false);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      float[] res3 = DataTools.GetUniqueSingleValues(DataTools.GetDataTableRows(tbl), "F1", false);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    #endregion

    #region GetUniqueDoubleValues()

    [Test]
    public void GetUniqueDoubleValues_skipNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(double));
      tbl.Rows.Add(1.0);
      tbl.Rows.Add(2.0);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(2.0);
      tbl.Rows.Add(DBNull.Value);

      double[] wanted = new double[] { 1.0, 2.0 };

      double[] res1 = DataTools.GetUniqueDoubleValues(tbl, "F1", true);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      double[] res2 = DataTools.GetUniqueDoubleValues(tbl.DefaultView, "F1", true);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      double[] res3 = DataTools.GetUniqueDoubleValues(DataTools.GetDataTableRows(tbl), "F1", true);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    [Test]
    public void GetUniqueDoubleValues_withNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(double));
      tbl.Rows.Add(1.0);
      tbl.Rows.Add(2.0);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(2.0);
      tbl.Rows.Add(DBNull.Value);

      double[] wanted = new double[] { 0.0, 1.0, 2.0 };

      double[] res1 = DataTools.GetUniqueDoubleValues(tbl, "F1", false);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      double[] res2 = DataTools.GetUniqueDoubleValues(tbl.DefaultView, "F1", false);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      double[] res3 = DataTools.GetUniqueDoubleValues(DataTools.GetDataTableRows(tbl), "F1", false);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    #endregion

    #region GetUniqueDecimalValues()

    [Test]
    public void GetUniqueDecimalValues_skipNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(decimal));
      tbl.Rows.Add(1m);
      tbl.Rows.Add(2m);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(2m);
      tbl.Rows.Add(DBNull.Value);

      decimal[] wanted = new decimal[] { 1m, 2m };

      decimal[] res1 = DataTools.GetUniqueDecimalValues(tbl, "F1", true);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      decimal[] res2 = DataTools.GetUniqueDecimalValues(tbl.DefaultView, "F1", true);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      decimal[] res3 = DataTools.GetUniqueDecimalValues(DataTools.GetDataTableRows(tbl), "F1", true);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    [Test]
    public void GetUniqueDecimalValues_withNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(decimal));
      tbl.Rows.Add(1m);
      tbl.Rows.Add(2m);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(2m);
      tbl.Rows.Add(DBNull.Value);

      decimal[] wanted = new decimal[] { 1m, 2m };

      decimal[] res1 = DataTools.GetUniqueDecimalValues(tbl, "F1", true);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      decimal[] res2 = DataTools.GetUniqueDecimalValues(tbl.DefaultView, "F1", true);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      decimal[] res3 = DataTools.GetUniqueDecimalValues(DataTools.GetDataTableRows(tbl), "F1", true);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    #endregion

    #region GetUniqueDateTimeValues()

    [Test]
    public void GetUniqueDateTimeValues()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(DateTime));
      tbl.Rows.Add(new DateTime(2021, 12, 21));
      tbl.Rows.Add(new DateTime(2021, 12, 20));
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(new DateTime(2021, 12, 21));
      tbl.Rows.Add(DBNull.Value);

      DateTime[] wanted = new DateTime[] { new DateTime(2021, 12, 20), new DateTime(2021, 12, 21) };

      DateTime[] res1 = DataTools.GetUniqueDateTimeValues(tbl, "F1");
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      DateTime[] res2 = DataTools.GetUniqueDateTimeValues(tbl.DefaultView, "F1");
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      DateTime[] res3 = DataTools.GetUniqueDateTimeValues(DataTools.GetDataTableRows(tbl), "F1");
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    #endregion

    #region GetUniqueTimeSpans()

    [Test]
    public void GetUniqueTimeSpanValues_skipNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(TimeSpan));
      tbl.Rows.Add(new TimeSpan(1, 2, 3));
      tbl.Rows.Add(new TimeSpan(4, 5, 6));
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(new TimeSpan(4, 5, 6));
      tbl.Rows.Add(DBNull.Value);

      TimeSpan[] wanted = new TimeSpan[] { new TimeSpan(1, 2, 3), new TimeSpan(4, 5, 6) };

      TimeSpan[] res1 = DataTools.GetUniqueTimeSpanValues(tbl, "F1", true);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      TimeSpan[] res2 = DataTools.GetUniqueTimeSpanValues(tbl.DefaultView, "F1", true);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      TimeSpan[] res3 = DataTools.GetUniqueTimeSpanValues(DataTools.GetDataTableRows(tbl), "F1", true);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    [Test]
    public void GetUniqueTimeSpanValues_withNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(TimeSpan));
      tbl.Rows.Add(new TimeSpan(1, 2, 3));
      tbl.Rows.Add(new TimeSpan(4, 5, 6));
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(new TimeSpan(4, 5, 6));
      tbl.Rows.Add(DBNull.Value);

      TimeSpan[] wanted = new TimeSpan[] { TimeSpan.Zero, new TimeSpan(1, 2, 3), new TimeSpan(4, 5, 6) };

      TimeSpan[] res1 = DataTools.GetUniqueTimeSpanValues(tbl, "F1", false);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      TimeSpan[] res2 = DataTools.GetUniqueTimeSpanValues(tbl.DefaultView, "F1", false);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      TimeSpan[] res3 = DataTools.GetUniqueTimeSpanValues(DataTools.GetDataTableRows(tbl), "F1", false);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    #endregion

    #region GetUniqueGuidValues()

    [Test]
    public void GetUniqueGuidValues_skipNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(Guid));
      tbl.Rows.Add(TestTable.Row1.VGuid);
      tbl.Rows.Add(TestTable.Row2.VGuid);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(TestTable.Row1.VGuid);
      tbl.Rows.Add(DBNull.Value);

      Guid[] wanted = new Guid[] { TestTable.Row1.VGuid, TestTable.Row2.VGuid };
      Array.Sort(wanted); // не уверен насчет порядка тестовых GUID.

      Guid[] res1 = DataTools.GetUniqueGuidValues(tbl, "F1", true);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      Guid[] res2 = DataTools.GetUniqueGuidValues(tbl.DefaultView, "F1", true);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      Guid[] res3 = DataTools.GetUniqueGuidValues(DataTools.GetDataTableRows(tbl), "F1", true);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    [Test]
    public void GetUniqueGuidValues_withNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(Guid));
      tbl.Rows.Add(TestTable.Row1.VGuid);
      tbl.Rows.Add(TestTable.Row2.VGuid);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(TestTable.Row1.VGuid);
      tbl.Rows.Add(DBNull.Value);

      Guid[] wanted = new Guid[] { Guid.Empty, TestTable.Row1.VGuid, TestTable.Row2.VGuid };
      Array.Sort(wanted); // не уверен насчет порядка тестовых GUID.

      Guid[] res1 = DataTools.GetUniqueGuidValues(tbl, "F1", false);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      Guid[] res2 = DataTools.GetUniqueGuidValues(tbl.DefaultView, "F1", false);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      Guid[] res3 = DataTools.GetUniqueGuidValues(DataTools.GetDataTableRows(tbl), "F1", false);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    #endregion

    #region GetUniqueEnumValues()

    [Test]
    public void GetUniqueEnumValues_skipNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Rows.Add((int)TestEnum.One);
      tbl.Rows.Add((int)TestEnum.Two);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add((int)TestEnum.One);
      tbl.Rows.Add(DBNull.Value);

      TestEnum[] wanted = new TestEnum[] { TestEnum.One, TestEnum.Two };

      TestEnum[] res1 = DataTools.GetUniqueEnumValues<TestEnum>(tbl, "F1", true);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      TestEnum[] res2 = DataTools.GetUniqueEnumValues<TestEnum>(tbl.DefaultView, "F1", true);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      TestEnum[] res3 = DataTools.GetUniqueEnumValues<TestEnum>(DataTools.GetDataTableRows(tbl), "F1", true);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    [Test]
    public void GetUniqueEnumValues_withNulls()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Rows.Add((int)TestEnum.One);
      tbl.Rows.Add((int)TestEnum.Two);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add((int)TestEnum.One);
      tbl.Rows.Add(DBNull.Value);

      TestEnum[] wanted = new TestEnum[] { TestEnum.Zero, TestEnum.One, TestEnum.Two };

      TestEnum[] res1 = DataTools.GetUniqueEnumValues<TestEnum>(tbl, "F1", false);
      Array.Sort(res1);
      Assert.AreEqual(wanted, res1, "DataTable");

      TestEnum[] res2 = DataTools.GetUniqueEnumValues<TestEnum>(tbl.DefaultView, "F1", false);
      Array.Sort(res2);
      Assert.AreEqual(wanted, res2, "DataView");

      TestEnum[] res3 = DataTools.GetUniqueEnumValues<TestEnum>(DataTools.GetDataTableRows(tbl), "F1", false);
      Array.Sort(res3);
      Assert.AreEqual(wanted, res3, "IEnumerable<DataRow>");
    }

    #endregion

    #endregion

    #region GetColumnNames, GetTableNames

    [Test]
    public void GetColumnNames()
    {
      DataTable tbl = CreateTestTable123();
      string[] res = DataTools.GetColumnNames(tbl);
      Assert.AreEqual(new string[] { "F1", "F2", "F3" }, res);
    }

    [Test]
    public void GetTableNames()
    {
      DataSet ds = new DataSet();
      ds.Tables.Add("T1");
      ds.Tables.Add("T2");

      string[] res = DataTools.GetTableNames(ds);
      Assert.AreEqual(new string[] { "T1", "T2" }, res);
    }

    #endregion

    #region AreColumnNamesEqual(), AreTableNamesEqual()

    [TestCase("F1,F2,F3", "F1,F2,F3", false, true)]
    [TestCase("F1,F2,F3", "F1,F2,F3", true, true)]
    [TestCase("F1,F2", "F1,F2,F3", false, false)]
    [TestCase("F1,F2", "F1,F2,F3", true, false)]
    [TestCase("", "", false, true)]
    [TestCase("F1,F2,F3", "F1,F3,F2", false, false)]
    [TestCase("F1,F2,F3", "F1,F3,F2", true, true)]
    [TestCase("F1,F2", "F1,f2", false, true)]
    public void AreColumnNamesEqual(string sColNames1, string sColNames2, bool ignoreOrder, bool wantedRes)
    {
      DataTable tbl1 = AreColumnNamesEqual_CreateTestTable(sColNames1);
      DataTable tbl2 = AreColumnNamesEqual_CreateTestTable(sColNames2);

      bool res1 = DataTools.AreColumnNamesEqual(tbl1, tbl2, ignoreOrder);
      Assert.AreEqual(wantedRes, res1, "#1");

      bool res2 = DataTools.AreColumnNamesEqual(tbl2, tbl1, ignoreOrder);
      Assert.AreEqual(wantedRes, res2, "#2");
    }

    private static DataTable AreColumnNamesEqual_CreateTestTable(string sColNames)
    {
      DataTable tbl = new DataTable();
      if (sColNames.Length > 0)
      {
        string[] aColNames = sColNames.Split(',');
        for (int i = 0; i < aColNames.Length; i++)
        {
          Type dataType = i % 2 == 0 ? typeof(string) : typeof(int);
          DataColumn col = new DataColumn(aColNames[i], dataType);
          tbl.Columns.Add(col);
        }
      }
      return tbl;
    }

    [TestCase("T1,T2,T3", "T1,T2,T3", false, true)]
    [TestCase("T1,T2,T3", "T1,T2,T3", true, true)]
    [TestCase("T1,T2", "T1,T2,T3", false, false)]
    [TestCase("T1,T2", "T1,T2,T3", true, false)]
    [TestCase("", "", false, true)]
    [TestCase("T1,T2,T3", "T1,T3,T2", false, false)]
    [TestCase("T1,T2,T3", "T1,T3,T2", true, true)]
    [TestCase("T1,T2", "T1,t2", false, true)]
    public void AreTableNamesEqual(string sTableNames1, string sTableNames2, bool ignoreOrder, bool wantedRes)
    {
      DataSet ds1 = AreTableNamesEqual_CreateTestDS(sTableNames1);
      DataSet ds2 = AreTableNamesEqual_CreateTestDS(sTableNames2);

      bool res1 = DataTools.AreTableNamesEqual(ds1, ds2, ignoreOrder);
      Assert.AreEqual(wantedRes, res1, "#1");

      bool res2 = DataTools.AreTableNamesEqual(ds2, ds1, ignoreOrder);
      Assert.AreEqual(wantedRes, res2, "#2");
    }

    private static DataSet AreTableNamesEqual_CreateTestDS(string sTableNames)
    {
      DataSet ds = new DataSet();
      if (sTableNames.Length > 0)
      {
        string[] aTableNames = sTableNames.Split(',');
        for (int i = 0; i < aTableNames.Length; i++)
          ds.Tables.Add(aTableNames[i]);
      }
      return ds;
    }


    #endregion

    #region SetBoundariesFlags(), SetGroupSequenceNumber()

    #region SetBoundariesFlags()

    [Test]
    public void SetBoundariesFlags_DataTable_1col()
    {
      DataTable tbl = SetBoundariesFlags_CreateTestTable();
      DataTools.SetBoundariesFlags(tbl, "F1", "FLAG1", "FLAG2");
      SetBoundariesFlags_CheckResult_1col(tbl);
    }

    [Test]
    public void SetBoundariesFlags_DataView_1col()
    {
      DataTable tbl = SetBoundariesFlags_CreateTestTable();
      DataTools.SetBoundariesFlags(tbl.DefaultView, "F1", "FLAG1", "FLAG2");
      SetBoundariesFlags_CheckResult_1col(tbl);
    }
    [Test]
    public void SetBoundariesFlags_DataTable_2cols()
    {
      DataTable tbl = SetBoundariesFlags_CreateTestTable();
      DataTools.SetBoundariesFlags(tbl, "F1,F2", "FLAG1", "FLAG2");
      SetBoundariesFlags_CheckResult_2cols(tbl);
    }

    [Test]
    public void SetBoundariesFlags_DataView_2cols()
    {
      DataTable tbl = SetBoundariesFlags_CreateTestTable();
      DataTools.SetBoundariesFlags(tbl.DefaultView, "F1,F2", "FLAG1", "FLAG2");
      SetBoundariesFlags_CheckResult_2cols(tbl);
    }


    private void SetBoundariesFlags_CheckResult_1col(DataTable tbl)
    {
      bool[] res1 = DataTools.GetValuesFromColumn<bool>(tbl, "FLAG1");
      Assert.AreEqual(new bool[] { true, true, false, true, false, false }, res1, "Begin flags");

      bool[] res2 = DataTools.GetValuesFromColumn<bool>(tbl, "FLAG2");
      Assert.AreEqual(new bool[] { true, false, true, false, false, true }, res2, "End flags");
    }

    private void SetBoundariesFlags_CheckResult_2cols(DataTable tbl)
    {
      bool[] res1 = DataTools.GetValuesFromColumn<bool>(tbl, "FLAG1");
      Assert.AreEqual(new bool[] { true, true, true, true, false, true }, res1, "Begin flags");

      bool[] res2 = DataTools.GetValuesFromColumn<bool>(tbl, "FLAG2");
      Assert.AreEqual(new bool[] { true, true, true, false, true, true }, res2, "End flags");
    }

    #endregion

    #region SetGroupSequenceNumber()

    [Test]
    public void SetGroupSequenceNumber_DataTable_1col()
    {
      DataTable tbl = SetBoundariesFlags_CreateTestTable();
      DataTools.SetGroupSequenceNumber(tbl, "F1", "ORDER");
      SetGroupSequenceNumber_CheckResult_1col(tbl);
    }

    [Test]
    public void SetGroupSequenceNumber_DataView_1col()
    {
      DataTable tbl = SetBoundariesFlags_CreateTestTable();
      DataTools.SetGroupSequenceNumber(tbl.DefaultView, "F1", "ORDER");
      SetGroupSequenceNumber_CheckResult_1col(tbl);
    }
    [Test]
    public void SetGroupSequenceNumber_DataTable_2cols()
    {
      DataTable tbl = SetBoundariesFlags_CreateTestTable();
      DataTools.SetGroupSequenceNumber(tbl, "F1,F2", "ORDER");
      SetGroupSequenceNumber_CheckResult_2cols(tbl);
    }

    [Test]
    public void SetGroupSequenceNumber_DataView_2cols()
    {
      DataTable tbl = SetBoundariesFlags_CreateTestTable();
      DataTools.SetGroupSequenceNumber(tbl.DefaultView, "F1,F2", "ORDER");
      SetGroupSequenceNumber_CheckResult_2cols(tbl);
    }


    private void SetGroupSequenceNumber_CheckResult_1col(DataTable tbl)
    {
      int[] res = DataTools.GetValuesFromColumn<int>(tbl, "ORDER");
      Assert.AreEqual(new int[] { 1, 2, 2, 3, 3, 3 }, res, "Order");
    }

    private void SetGroupSequenceNumber_CheckResult_2cols(DataTable tbl)
    {
      int[] res = DataTools.GetValuesFromColumn<int>(tbl, "ORDER");
      Assert.AreEqual(new int[] { 1, 2, 3, 4, 4, 5 }, res, "Order");
    }

    #endregion

    #region Тестовая таблица

    private static DataTable SetBoundariesFlags_CreateTestTable()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Columns.Add("F2", typeof(string));
      tbl.Columns.Add("FLAG1", typeof(bool));
      tbl.Columns.Add("FLAG2", typeof(bool));
      tbl.Columns.Add("ORDER", typeof(int));

      tbl.Rows.Add(1, "AAA");
      tbl.Rows.Add(2, "AAA");
      tbl.Rows.Add(2, "BBB");
      tbl.Rows.Add(3, "AAA");
      tbl.Rows.Add(3, "AAA");
      tbl.Rows.Add(3, "BBB");

      return tbl;
    }

    #endregion

    #endregion

    #region AreValuesEqual()

    [TestCase(1, 1, true)]
    [TestCase(1, 2, false)]
    [TestCase(1, null, false)]
    [TestCase(0, null, false)]
    [TestCase(null, null, true)]
    public void AreValuesEqual(object v1, object v2, bool wanted)
    {
      if (v1 == null)
        v1 = DBNull.Value;
      if (v2 == null)
        v2 = DBNull.Value;

      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Rows.Add(v1);
      tbl.Rows.Add(v2);

      Assert.AreEqual(wanted, DataTools.AreValuesEqual(tbl.Rows[0], tbl.Rows[1], "F1"), "DataRow with ColumnName");
      Assert.AreEqual(wanted, DataTools.AreValuesEqual(tbl.Rows[0], tbl.Rows[1], 0), "DataRow with ColumnPos");
      Assert.AreEqual(wanted, DataTools.AreValuesEqual(tbl.DefaultView[0], tbl.DefaultView[1], "F1"), "DataRowView with ColumnName");
      Assert.AreEqual(wanted, DataTools.AreValuesEqual(tbl.DefaultView[0], tbl.DefaultView[1], 0), "DataRowView with ColumnPos");
      Assert.AreEqual(wanted, DataTools.AreValuesEqual(v1, v2), "Object");
    }

    #endregion

    #region CopyRowsToRows()

    [Test]
    public void CopyRowsToRows_DataSet()
    {
      DataSet dsSrc = new DataSet();
      dsSrc.ExtendedProperties["Test1"] = "AAA";

      DataTable tbl1 = CreateTestTable123(); // эта таблица копируется
      tbl1.TableName = "T1";
      tbl1.Rows[0]["F1"] = 1000;
      tbl1.ExtendedProperties["Test2"] = "BBB";
      dsSrc.Tables.Add(tbl1);

      DataTable tbl2 = dsSrc.Tables.Add("T2"); // эта таблица не копируется
      tbl2.Columns.Add("F4", typeof(int));
      tbl2.Rows.Add(123);

      DataSet dsRes = new DataSet();
      dsRes.ExtendedProperties["Test3"] = "CCC";

      DataTable tbl1res = CreateTestTable123(); // эта таблица переписывается
      tbl1res.TableName = "T1";
      tbl1res.ExtendedProperties["Test4"] = "DDD";
      dsRes.Tables.Add(tbl1res);

      dsRes.Tables.Add("T3"); // эта таблица сохраняется

      DataTools.CopyRowsToRows(dsSrc, dsRes);

      Assert.AreEqual(new string[] { "T1", "T2" }, DataTools.GetTableNames(dsSrc), "Source TableNames");
      Assert.AreEqual(new string[] { "T1", "T3" }, DataTools.GetTableNames(dsRes), "Result TableNames");

      Assert.AreEqual(new int[] { 1000, 4, 7 }, DataTools.GetValuesFromColumn<int>(tbl1res, "F1"), "F1");

      Assert.AreEqual("AAA", DataTools.GetString(dsRes.ExtendedProperties["Test1"]), "ExtendedProperties #1");
      Assert.AreEqual("BBB", DataTools.GetString(dsRes.Tables["T1"].ExtendedProperties["Test2"]), "ExtendedProperties #2");
      Assert.AreEqual("CCC", DataTools.GetString(dsRes.ExtendedProperties["Test3"]), "ExtendedProperties #3");
      Assert.AreEqual("DDD", DataTools.GetString(dsRes.Tables["T1"].ExtendedProperties["Test4"]), "ExtendedProperties #4");
    }

    [Test]
    public void CopyRowsToRows_DataTable_replace()
    {
      DataTable tblSrc = CreateTestTable123();
      tblSrc.Rows[0]["F1"] = 1000;
      tblSrc.ExtendedProperties["Test1"] = "AAA";
      tblSrc.Columns["F1"].ExtendedProperties["Test3"] = "CCC";

      DataTable tblRes = CreateTestTable123();
      tblRes.ExtendedProperties["Test2"] = "BBB";

      DataTools.CopyRowsToRows(tblSrc, tblRes, false, false);
      Assert.AreEqual(new int[] { 1000, 4, 7 }, DataTools.GetValuesFromColumn<int>(tblRes, "F1"), "F1");

      Assert.AreEqual("AAA", DataTools.GetString(tblRes.ExtendedProperties["Test1"]), "ExtendedProperties #1");
      Assert.AreEqual("BBB", DataTools.GetString(tblRes.ExtendedProperties["Test2"]), "ExtendedProperties #2");
      Assert.AreEqual("CCC", DataTools.GetString(tblRes.Columns["F1"].ExtendedProperties["Test3"]), "ExtendedProperties #3");
    }

    [Test]
    public void CopyRowsToRows_DataTable_add()
    {
      DataTable tblSrc = CreateTestTable123();
      tblSrc.Rows[0]["F1"] = 1000;

      DataTable tblRes = CreateTestTable123();

      DataTools.CopyRowsToRows(tblSrc, tblRes, false, true);
      Assert.AreEqual(new int[] { 1, 4, 7, 1000, 4, 7 }, DataTools.GetValuesFromColumn<int>(tblRes, "F1"), "F1");
    }

    [Test]
    public void CopyRowsToRows_DataTable_useColumnNames()
    {
      DataTable tblSrc = CreateTestTable123();
      tblSrc.Rows[0]["F1"] = 1000;

      DataTable tblRes = new DataTable();
      tblRes.Columns.Add("F2", typeof(int));
      tblRes.Columns.Add("F1", typeof(int));

      DataTools.CopyRowsToRows(tblSrc, tblRes, true, true);
      Assert.AreEqual(new int[] { 1000, 4, 7 }, DataTools.GetValuesFromColumn<int>(tblRes, "F1"), "F1");
    }

    #endregion

    #region CopyRowValues()

    [Test]
    public void CopyRowValues_useColumnNames()
    {
      DataTable tblSrc = CreateTestTable123();
      DataRow rowSrc = tblSrc.Rows[0];

      DataTable tblRes = new DataTable();
      tblRes.Columns.Add("F2", typeof(int));
      tblRes.Columns.Add("F1", typeof(int));
      DataRow rowRes = tblRes.NewRow();

      DataTools.CopyRowValues(rowSrc, rowRes, true);
      Assert.AreEqual(new object[] { 2, 1 }, rowRes.ItemArray);
    }

    [Test]
    public void CopyRowValues_useColumnPositions()
    {
      DataTable tblSrc = CreateTestTable123();
      DataRow rowSrc = tblSrc.Rows[0];

      DataTable tblRes = tblSrc.Clone();
      DataRow rowRes = tblRes.NewRow();

      DataTools.CopyRowValues(rowSrc, rowRes, false);
      Assert.AreEqual(new object[] { 1, 2, 3 }, rowRes.ItemArray);
    }


    [Test]
    public void CopyRowValues_with_columnNames_1()
    {
      DataTable tblSrc = CreateTestTable123();
      DataRow rowSrc = tblSrc.Rows[0];

      DataTable tblRes = tblSrc.Clone();
      DataRow rowRes = tblRes.NewRow();

      DataTools.CopyRowValues(rowSrc, rowRes, new string[] { "F1", "F3" });
      Assert.AreEqual(new object[] { 1, DBNull.Value, 3 }, rowRes.ItemArray);
    }

    [Test]
    public void CopyRowValues_with_columnNames_2()
    {
      DataTable tblSrc = CreateTestTable123();
      DataRow rowSrc = tblSrc.Rows[0];

      DataTable tblRes = tblSrc.Clone();
      DataRow rowRes = tblRes.NewRow();

      DataTools.CopyRowValues(rowSrc, rowRes, "F3,F2");
      Assert.AreEqual(new object[] { DBNull.Value, 2, 3 }, rowRes.ItemArray);
    }

    #endregion

    #region CopyRowValuesForPrefix()

    [Test]
    public void CopyRowValuesForPrefix()
    {
      DataTable tblSrc = CreateTestTable123();
      DataRow rowSrc = tblSrc.Rows[0];

      DataTable tblRes = new DataTable();
      tblRes.Columns.Add("A3", typeof(int));
      tblRes.Columns.Add("A2", typeof(int));
      tblRes.Columns.Add("A4", typeof(int)); // нее будет найдено
      DataRow rowRes = tblRes.NewRow();

      DataTools.CopyRowValuesForPrefix(rowSrc, rowRes, "F", "A");
      Assert.AreEqual(new object[] { 3, 2, DBNull.Value }, rowRes.ItemArray);
    }

    #endregion

    #region ReplaceNulls()

    [Test]
    public void ReplaceNulls_DataTable()
    {
      DataTable tbl = ReplaceNulls_CreateTestTable();
      DataTools.ReplaceNulls(tbl, "F1", 100);
      Assert.AreEqual(new int[] { 1, 100, 3, 100, 5 }, DataTools.GetValuesFromColumn<int>(tbl, "F1"));
    }

    [Test]
    public void ReplaceNulls_DataView()
    {
      DataTable tbl = ReplaceNulls_CreateTestTable();
      DataTools.ReplaceNulls(tbl.DefaultView, "F1", 100);
      Assert.AreEqual(new int[] { 1, 100, 3, 100, 5 }, DataTools.GetValuesFromColumn<int>(tbl, "F1"));
    }

    [Test]
    public void ReplaceNulls_DataRowArray()
    {
      DataTable tbl = ReplaceNulls_CreateTestTable();
      DataRow[] a = new DataRow[] { tbl.Rows[2], tbl.Rows[3] };
      DataTools.ReplaceNulls(a, "F1", 100);
      Assert.AreEqual(new int[] { 1, 0, 3, 100, 5 }, DataTools.GetValuesFromColumn<int>(tbl, "F1"));
    }

    private DataTable ReplaceNulls_CreateTestTable()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Rows.Add(1);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(3);
      tbl.Rows.Add(DBNull.Value);
      tbl.Rows.Add(5);
      return tbl;
    }

    #endregion

    #region StrTrim()

    [Test]
    public void StrTrim_DataTable_ColumnName()
    {
      DataTable tbl = StrTrim_CreateTestTable();
      DataTools.StrTrim(tbl, "F2");
      Assert.AreEqual(new object[] { "AAA ", " XXX", " ", "EEE", null }, DataTools.GetValuesFromColumn<string>(tbl, "F1"), "Not changed");
      Assert.AreEqual(new object[] { "BBB", "DDD", null, "FFF", null }, DataTools.GetValuesFromColumn<string>(tbl, "F2"), "Changed");
    }

    [Test]
    public void StrTrim_DataTable_ColumnIndex()
    {
      DataTable tbl = StrTrim_CreateTestTable();
      DataTools.StrTrim(tbl, 0);
      Assert.AreEqual(new object[] { "AAA", "XXX", null, "EEE", null }, DataTools.GetValuesFromColumn<string>(tbl, "F1"), "Changed");
      Assert.AreEqual(new object[] { "BBB", "DDD ", null, " FFF", " " }, DataTools.GetValuesFromColumn<string>(tbl, "F2"), "Not changed");
    }

    [Test]
    public void StrTrim_DataTable_AllColumns()
    {
      DataTable tbl = StrTrim_CreateTestTable();
      DataTools.StrTrim(tbl);
      Assert.AreEqual(new object[] { "AAA", "XXX", null, "EEE", null }, DataTools.GetValuesFromColumn<string>(tbl, "F1"), "#1");
      Assert.AreEqual(new object[] { "BBB", "DDD", null, "FFF", null }, DataTools.GetValuesFromColumn<string>(tbl, "F2"), "#2");
    }

    [Test]
    public void StrTrim_DataSet()
    {
      DataSet ds = new DataSet();
      DataTable tbl = StrTrim_CreateTestTable();
      ds.Tables.Add(tbl);

      DataTools.StrTrim(tbl);
      Assert.AreEqual(new object[] { "AAA", "XXX", null, "EEE", null }, DataTools.GetValuesFromColumn<string>(tbl, "F1"), "#1");
      Assert.AreEqual(new object[] { "BBB", "DDD", null, "FFF", null }, DataTools.GetValuesFromColumn<string>(tbl, "F2"), "#2");
    }

    private DataTable StrTrim_CreateTestTable()
    {
      // В тестовой таблице не должно быть значений String.Empty, т.к. не четко определено,
      // должны ли они заменяться на DBNull

      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(string));
      tbl.Columns.Add("F2", typeof(string));

      tbl.Rows.Add("AAA ", "BBB"); // [0]
      tbl.Rows.Add(" XXX", "DDD "); // [1]
      tbl.Rows.Add(" ", DBNull.Value); // [2]
      tbl.Rows.Add("EEE", " FFF"); // [3]
      tbl.Rows.Add(DBNull.Value, " "); // [4]
      return tbl;
    }

    #endregion

    #region UpdateXXXByPrimaryKey()

    #region UpdateRowsByPrimaryKey()

    [Test]
    public void UpdateRowsByPrimaryKey_useColumnPos()
    {
      DataTable tblSrc = UpdateXXXByPrimaryKey_CreateTable();

      DataTable tblRes = tblSrc.Clone();
      DataTools.SetPrimaryKey(tblRes, "F1,F2");
      tblRes.Rows.Add(1, "AAA", 100);
      tblRes.Rows.Add(4, "BBB", 100); // Такой строки нет
      tblRes.Rows.Add(2, "BBB", 100);

      DataTools.UpdateRowsByPrimaryKey(tblSrc, tblRes, false);
      Assert.AreEqual(new int[] { 1, 100, 2 }, DataTools.GetValuesFromColumn<int>(tblRes, "F3"), "F3");
    }

    [Test]
    public void UpdateRowsByPrimaryKey_useColumnName()
    {
      DataTable tblSrc = UpdateXXXByPrimaryKey_CreateTable();

      DataTable tblRes = new DataTable();
      tblRes.Columns.Add("F3", typeof(int));
      tblRes.Columns.Add("F4", typeof(int));
      tblRes.Columns.Add("F1", typeof(int));
      tblRes.Columns.Add("F2", typeof(string));
      DataTools.SetPrimaryKey(tblRes, "F1,F2");
      tblRes.Rows.Add(100, 100, 1, "AAA");
      tblRes.Rows.Add(100, 100, 4, "BBB"); // Такой строки нет
      tblRes.Rows.Add(100, 100, 2, "BBB");

      DataTools.UpdateRowsByPrimaryKey(tblSrc, tblRes, true);
      Assert.AreEqual(new int[] { 1, 100, 2 }, DataTools.GetValuesFromColumn<int>(tblRes, "F3"), "F3");
      Assert.AreEqual(new int[] { 100, 100, 100 }, DataTools.GetValuesFromColumn<int>(tblRes, "F4"), "F4");
    }

    #endregion

    #region UpdateTableByPrimaryKey()

    [Test]
    public void UpdateTableByPrimaryKey_useColumnPos()
    {
      DataTable tblSrc = UpdateXXXByPrimaryKey_CreateTable();

      DataTable tblRes = tblSrc.Clone();
      DataTools.SetPrimaryKey(tblRes, "F1,F2");
      tblRes.Rows.Add(1, "AAA", 100);
      tblRes.Rows.Add(4, "BBB", 100); // Такой строки нет
      tblRes.Rows.Add(2, "BBB", 100);

      DataTools.UpdateTableByPrimaryKey(tblSrc, tblRes, false);

      // Порядок строк в таблице tblRes не определен. Надо отсортировать вручную
      tblRes.DefaultView.Sort = "F1,F2";
      tblRes = tblRes.DefaultView.ToTable();

      Assert.AreEqual(new int[] { 1, 2, 3 }, DataTools.GetValuesFromColumn<int>(tblRes, "F1"), "F1");
      Assert.AreEqual(new string[] { "AAA", "BBB", "CCC" }, DataTools.GetValuesFromColumn<string>(tblRes, "F2"), "F2");
      Assert.AreEqual(new int[] { 1, 2, 3 }, DataTools.GetValuesFromColumn<int>(tblRes, "F3"), "F3");
    }

    [Test]
    public void UpdateTableByPrimaryKey_useColumnName()
    {
      DataTable tblSrc = UpdateXXXByPrimaryKey_CreateTable();

      DataTable tblRes = new DataTable();
      tblRes.Columns.Add("F3", typeof(int));
      tblRes.Columns.Add("F4", typeof(int));
      tblRes.Columns.Add("F1", typeof(int));
      tblRes.Columns.Add("F2", typeof(string));
      DataTools.SetPrimaryKey(tblRes, "F1,F2");
      tblRes.Rows.Add(10, 100, 1, "AAA");
      tblRes.Rows.Add(20, 200, 123, "BBB"); // Такой строки нет
      tblRes.Rows.Add(30, 300, 2, "BBB");

      DataTools.UpdateTableByPrimaryKey(tblSrc, tblRes, true);

      // Порядок строк в таблице tblRes не определен. Надо отсортировать вручную
      tblRes.DefaultView.Sort = "F1,F2";
      tblRes = tblRes.DefaultView.ToTable();

      Assert.AreEqual(new int[] { 1, 2, 3 }, DataTools.GetValuesFromColumn<int>(tblRes, "F1"), "F1");
      Assert.AreEqual(new string[] { "AAA", "BBB", "CCC" }, DataTools.GetValuesFromColumn<string>(tblRes, "F2"), "F2");
      Assert.AreEqual(new int[] { 1, 2, 3 }, DataTools.GetValuesFromColumn<int>(tblRes, "F3"), "F3"); // обновленные значения
      Assert.AreEqual(new int[] { 100, 300, 0 }, DataTools.GetValuesFromColumn<int>(tblRes, "F4"), "F4"); // необновленные значения
    }

    #endregion

    #region Вспомогательные методы

    private static DataTable UpdateXXXByPrimaryKey_CreateTable()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Columns.Add("F2", typeof(string));
      tbl.Columns.Add("F3", typeof(int));
      tbl.Rows.Add(1, "AAA", 1);
      tbl.Rows.Add(2, "BBB", 2);
      tbl.Rows.Add(3, "CCC", 3);
      return tbl;
    }

    #endregion

    #endregion

    #region FindDataRowView()/FindDataRowViewIndex()

    [TestCase(5, 3, 2)]
    [TestCase(5, -3, -1)]
    [TestCase(5, 6, -1)]
    [TestCase(200, 3, 2)]
    [TestCase(200, -3, -1)]
    [TestCase(200, 201, -1)]
    public void FindDataRowView_FindDataRowViewIndex(int nRows, int value, int wantedIndex)
    {
      // Реализация отличается для отсортированных DataView, содержащих больше 50 строк

      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      DataRow row = null;
      for (int i = 1; i <= nRows; i++)
      {
        DataRow row1 = tbl.Rows.Add(i); // Эти строки попадут в DataView
        if (i == value)
          row = row1;
        DataRow row2 = tbl.Rows.Add(-i); // Эти строки не пройдут фильтр
        if ((-i) == value)
          row = row2;
      }
      tbl.DefaultView.Sort = "F1";
      tbl.DefaultView.RowFilter = "F1>0";

      int res1 = DataTools.FindDataRowViewIndex(tbl.DefaultView, row);
      Assert.AreEqual(wantedIndex, res1, "FindDataRowViewIndex()");

      DataRowView res2 = DataTools.FindDataRowView(tbl.DefaultView, row);
      if (wantedIndex >= 0)
      {
        Assert.IsNotNull(res2, "FindDataRowView() return not null");
        Assert.AreEqual(value, res2.Row["F1"], "FindDataRowView() row");
      }
      else
        Assert.IsNull(res2, "FindDataRowView() return null");
    }

    #endregion

    #region GetDataViewRows()

    [Test]
    public void GetDataViewRows_ok()
    {
      DataTable tbl = CreateTestTable123();
      tbl.DefaultView.Sort = "F1 DESC";
      tbl.DefaultView.RowFilter = "F3>=6";

      DataRow[] res = DataTools.GetDataViewRows(tbl.DefaultView);
      Assert.AreEqual(new DataRow[] { tbl.Rows[2], tbl.Rows[1] }, res);
    }

    [Test]
    public void GetDataViewRows_emptyArray()
    {
      DataTable tbl = CreateTestTable123();
      tbl.DefaultView.RowFilter = "F1>=100";

      DataRow[] res = DataTools.GetDataViewRows(tbl.DefaultView);
      Assert.AreEqual(new DataRow[] { }, res);
    }

    [Test]
    public void GetDataViewRows_null()
    {
      DataRow[] res = DataTools.GetDataViewRows(null);
      Assert.AreEqual(new DataRow[] { }, res);
    }

    #endregion

    #region GetDataRowViewRows()

    [Test]
    public void GetDataRowViewRows_ok()
    {
      DataTable tbl = CreateTestTable123();
      tbl.DefaultView.Sort = "F1";
      DataRowView[] drvs = tbl.DefaultView.FindRows(4);

      DataRow[] res = DataTools.GetDataRowViewRows(drvs);
      Assert.AreEqual(new DataRow[] { tbl.Rows[1] }, res);
    }

    [Test]
    public void GetDataRowViewRows_null()
    {
      DataRow[] res = DataTools.GetDataRowViewRows(null);
      Assert.AreEqual(new DataRow[] { }, res);
    }


    [Test]
    public void GetDataRowViewRows_emptyArray()
    {
      DataRow[] res = DataTools.GetDataRowViewRows(new DataRowView[0]);
      Assert.AreEqual(new DataRow[] { }, res);
    }

    #endregion

    #region GetDataViewIdIndexDictionary()

    [Test]
    public void GetDataViewIdIndexDictionary()
    {
      DataTable tbl = IdTools.TableFromIds<Int32>(new int[] { 1, 3, 5, 2, 4, 6 }, "Id");
      tbl.DefaultView.Sort = "Id DESC";
      tbl.DefaultView.RowFilter = "Id>=3"; // 6,5,4,3

      IDictionary<int, int> res = DataTools.GetDataViewIdIndexDictionary(tbl.DefaultView);
      Assert.AreEqual(4, res.Count, "Count");
      Assert.AreEqual(0, res[6], "[6]");
      Assert.AreEqual(1, res[5], "[5]");
      Assert.AreEqual(2, res[4], "[4]");
      Assert.AreEqual(3, res[3], "[3]");
    }

    #endregion

    #region GetDataViewSortColumnNames/SingleColumnName/Sort()

    [Test]
    public void GetDataViewSortColumnNames_simple()
    {
      Assert.AreEqual(new string[] { "F1", "F2" }, DataTools.GetDataViewSortColumnNames("F1,F2 DESC"));
    }

    [Test]
    public void GetDataViewSortColumnNames_with_brackets()
    {
      Assert.AreEqual(new string[] { "F1", "F2" }, DataTools.GetDataViewSortColumnNames("[F1],[F2] DESC"));
    }

    [Test]
    public void GetDataViewSortColumnNames_empty()
    {
      Assert.AreEqual(new string[] { }, DataTools.GetDataViewSortColumnNames(""));
    }

    [Test]
    public void GetDataViewSortColumnNames_full()
    {
      string[] columnNames;
      ListSortDirection[] directions;
      DataTools.GetDataViewSortColumnNames("F1,F2 DESC", out columnNames, out directions);

      Assert.AreEqual(new string[] { "F1", "F2" }, columnNames, "ColumnNames");
      Assert.AreEqual(new ListSortDirection[] { ListSortDirection.Ascending, ListSortDirection.Descending }, directions, "Directions");
    }

    [Test]
    public void GetDataViewSortSingleColumnName_simple()
    {
      Assert.AreEqual("F1", DataTools.GetDataViewSortSingleColumnName("F1 DESC"));
    }

    [Test]
    public void GetDataViewSortSingleColumnName_empty()
    {
      Assert.AreEqual("", DataTools.GetDataViewSortSingleColumnName(""));
    }

    [Test]
    public void GetDataViewSortSingleColumnName_full()
    {
      string columnName;
      ListSortDirection direction;
      DataTools.GetDataViewSortSingleColumnName("F1 DESC", out columnName, out direction);

      Assert.AreEqual("F1", columnName, "ColumnName");
      Assert.AreEqual(ListSortDirection.Descending, direction, "Direction");
    }

    [Test]
    public void GetDataViewSortSingleColumnName_with_brackets()
    {
      Assert.AreEqual("F1", DataTools.GetDataViewSortSingleColumnName("[F1] DESC"));
    }

    [Test]
    public void GetDataViewSortSingleColumnName_exception()
    {
      Assert.Catch(delegate () { DataTools.GetDataViewSortSingleColumnName("F1,F2"); });
    }

    [Test]
    public void GetDataViewSort_normal()
    {
      string res = DataTools.GetDataViewSort(new string[] { "F1", "F2" },
        new ListSortDirection[] { ListSortDirection.Ascending, ListSortDirection.Descending });

      // Чтобы тест не был хрупким
      res = res.Replace("[", "");
      res = res.Replace("]", "");
      res = res.Replace(" ASC", "");
      res = res.Replace(", ", ",");

      Assert.AreEqual("F1,F2 DESC", res);
    }

    [Test]
    public void GetDataViewSort_empty()
    {
      string res = DataTools.GetDataViewSort(new string[] { },
        new ListSortDirection[] { });

      Assert.AreEqual("", res);
    }

    [Test]
    public void GetDataViewSort_exceptions()
    {
      Assert.Catch(delegate () { DataTools.GetDataViewSort(new string[] { "F1", "F2" }, new ListSortDirection[] { ListSortDirection.Ascending }); }, "missed direction");
      Assert.Catch(delegate () { DataTools.GetDataViewSort(new string[] { }, new ListSortDirection[] { ListSortDirection.Ascending }); }, "extra direction");
      Assert.Catch(delegate () { DataTools.GetDataViewSort(new string[] { "" }, new ListSortDirection[] { ListSortDirection.Ascending }); }, "empty column name");
    }

    #endregion

    #region GetDataViewLikeExpressionString()

    [TestCase("ABC", @"LIKE 'ABC*'")]
    [TestCase("A*B%C", @"LIKE 'A[*]B[%]C*'")]
    [TestCase("A[B]C", @"LIKE 'A[[]B[]]C*'")]
    public void GetDataViewLikeExpressionString(string str, string wanted)
    {
      string res = DataTools.GetDataViewLikeExpressionString(str);
      Assert.AreEqual(wanted, res);
    }

    #endregion

    #region FindOrAddDataRow()

    [TestCase(4, false)]
    [TestCase(10, true)]
    public void FindOrAddDataRow_1_3arg(int value, bool wanted)
    {
      DataTable tbl = CreateTestTable123();
      tbl.DefaultView.Sort = "F1";

      DataRow resRow;
      bool res = DataTools.FindOrAddDataRow(tbl.DefaultView, value, out resRow);
      Assert.AreEqual(wanted, res, "result");
      Assert.AreEqual(wanted ? 4 : 3, tbl.Rows.Count, "Rows.Count");
      Assert.AreEqual(value, resRow["F1"], "row value");
    }

    [TestCase(4, false)]
    [TestCase(10, true)]
    public void FindOrAddDataRow_1_2arg(int value, bool wanted)
    {
      DataTable tbl = CreateTestTable123();
      tbl.DefaultView.Sort = "F1";

      DataRow resRow = DataTools.FindOrAddDataRow(tbl.DefaultView, value);
      Assert.AreEqual(wanted ? 4 : 3, tbl.Rows.Count, "Rows.Count");
      Assert.AreEqual(value, resRow["F1"], "row value");
    }

    [TestCase(4, 5, false)]
    [TestCase(10, 11, true)]
    public void FindOrAddDataRow_multi_3arg(int value1, int value2, bool wanted)
    {
      DataTable tbl = CreateTestTable123();
      tbl.DefaultView.Sort = "F1,F2";

      object[] keys = new object[] { value1, value2 };
      DataRow resRow;
      bool res = DataTools.FindOrAddDataRow(tbl.DefaultView, keys, out resRow);
      Assert.AreEqual(wanted, res, "result");
      Assert.AreEqual(wanted ? 4 : 3, tbl.Rows.Count, "Rows.Count");
      Assert.AreEqual(value1, resRow["F1"], "row value #1");
      Assert.AreEqual(value2, resRow["F2"], "row value #2");
    }

    [TestCase(4, 5, false)]
    [TestCase(10, 11, true)]
    public void FindOrAddDataRow_multi_2arg(int value1, int value2, bool wanted)
    {
      DataTable tbl = CreateTestTable123();
      tbl.DefaultView.Sort = "F1,F2";

      object[] keys = new object[] { value1, value2 };
      DataRow resRow = DataTools.FindOrAddDataRow(tbl.DefaultView, keys);
      Assert.AreEqual(wanted ? 4 : 3, tbl.Rows.Count, "Rows.Count");
      Assert.AreEqual(value1, resRow["F1"], "row value #1");
      Assert.AreEqual(value2, resRow["F2"], "row value #2");
    }

    #endregion

    #region CreateUniqueTable()

    public void CreateUniqueTable()
    {
      DataTable tbl = CreateTestTable123();
      tbl.Rows.Add(1, 2, 10); // повтор по полям F1,F2

      int[] wantedF3 = new int[] { 3, 6, 9 };

      DataTable res1 = DataTools.CreateUniqueTable(tbl, new string[] { "F1", "F2" });
      Assert.AreEqual(wantedF3, DataTools.GetValuesFromColumn<int>(res1, "F3"), "DataTable, string[]");

      DataTable res2 = DataTools.CreateUniqueTable(tbl, "F1,F2");
      Assert.AreEqual(wantedF3, DataTools.GetValuesFromColumn<int>(res2, "F3"), "DataTable, CSV-string");

      DataTable res3 = DataTools.CreateUniqueTable(tbl.DefaultView, new string[] { "F1", "F2" });
      Assert.AreEqual(wantedF3, DataTools.GetValuesFromColumn<int>(res3, "F3"), "DataView, string[]");

      DataTable res4 = DataTools.CreateUniqueTable(tbl.DefaultView, "F1,F2");
      Assert.AreEqual(wantedF3, DataTools.GetValuesFromColumn<int>(res4, "F3"), "DataView, CSV-string");
    }

    #endregion

    #region GetKeys()/GetValues()

    [Test]
    public void GetKeys_GetValues()
    {
      Dictionary<int, string> dict = new Dictionary<int, string>();
      dict.Add(2, "AAA");
      dict.Add(1, "BBB");

      int[] keys = DataTools.GetKeys<int, string>(dict);
      string[] values = DataTools.GetValues<int, string>(dict);

      // порядок возвращаемых ключей, строго говоря, не определен
      Assert.AreEqual(2, keys.Length, "keys length");
      Assert.AreEqual(2, values.Length, "values length");

      int p1 = Array.IndexOf<int>(keys, 1);
      int p2 = Array.IndexOf<int>(keys, 2);
      Assert.IsTrue(p1 >= 0, "keys[1]");
      Assert.IsTrue(p2 >= 0, "keys[2]");

      Assert.AreEqual("BBB", values[p1], "values[1]");
      Assert.AreEqual("AAA", values[p2], "values[2]");
    }

    #endregion

    #region FormatDataValue()

    [TestCase(null, "NULL")]
    [TestCase("", "\'\'")]
    [TestCase("ABC", "\'ABC\'")]
    [TestCase("A'BC", "\'A\'\'BC\'")]
    [TestCase(123, "123")]
    [TestCase(1.23, "1.23")]
    [TestCase(1.23f, "1.23")]
    [TestCase(true, "TRUE")]
    [TestCase(false, "FALSE")]
    [TestCase('a', "\'a\'")]
    [TestCase(' ', "\' \'")]
    [TestCase('\'', "\'\'\'\'")]
    public void FormatDataValue_baseTypes(object value, string wanted)
    {
      Assert.AreEqual(wanted, DataTools.FormatDataValue(value));
    }

    [Test]
    public void FormatDataValue_DateTime()
    {
      DateTime dt1 = new DateTime(2022, 2, 18);
      Assert.AreEqual("#2/18/2022#", DataTools.FormatDataValue(dt1), "Date only");

      DateTime dt2 = new DateTime(2022, 2, 18, 19, 50, 21);
      Assert.AreEqual("#2/18/2022 19:50:21#", DataTools.FormatDataValue(dt2), "Date and time");
    }

    [Test]
    public void FormatDataValue_Guid()
    {
      Guid g = new Guid("2c0dfea6-8326-44e4-ba55-a994e0bedd10");
      Assert.AreEqual("\'2c0dfea6-8326-44e4-ba55-a994e0bedd10\'", DataTools.FormatDataValue(g));
    }

    /// <summary>
    /// Тестируем практическое применение метода для построения выражений
    /// </summary>
    [Test]
    public void FormatDataValue_Select()
    {
      // Перебираем все возможные типы. См. справку для свойства DataColumn.DataType
      DoFormatDataValue_Select<Boolean>(new Boolean[] { true, false });
      DoFormatDataValue_Select<Byte>(new Byte[] { 1, 10, 0 });
      DoFormatDataValue_Select<Char>(new Char[] { ' ', '1', 'x' });
      DoFormatDataValue_Select<DateTime>(new DateTime[] { new DateTime(2021, 2, 18), new DateTime(2020, 2, 29) });
      DoFormatDataValue_Select<Decimal>(new Decimal[] { 1m, -10.5m, 0m });
      DoFormatDataValue_Select<Double>(new Double[] { 1.0, -10.5, 0.0 });
      DoFormatDataValue_Select<Int16>(new Int16[] { 1, -10, 0 });
      DoFormatDataValue_Select<Int32>(new Int32[] { 1, -10, 0 });
      DoFormatDataValue_Select<Int64>(new Int64[] { 1L, 10L, 0L });
      DoFormatDataValue_Select<SByte>(new SByte[] { -11, 10, 0 });
      DoFormatDataValue_Select<Single>(new Single[] { 1f, -10.5f, 0f });
      DoFormatDataValue_Select<String>(new String[] { "ABC", "" });

      // Для TimeSpan не поддерживаются операции сравнения. Select() не работает.
      // DoFormatDataValue_Select<TimeSpan>(new TimeSpan[] { TimeSpan.Zero, new TimeSpan(1,2,3), new TimeSpan(-5,2,6,1,456)});

      DoFormatDataValue_Select<UInt16>(new UInt16[] { 1, 10, 0 });
      DoFormatDataValue_Select<UInt32>(new UInt32[] { 1, 10, 0 });
      DoFormatDataValue_Select<UInt64>(new UInt64[] { 1, 10, 0 });

      // Тоже не работает
      //DoFormatDataValue_Select<byte[]>(new byte[][] { new byte[] { 1, 10, 0 }, DataTools.EmptyBytes });

      // Для этого типа нет стандартной поддержки в DataTable
      DoFormatDataValue_Select<Guid>(new Guid[] { Guid.Empty, new Guid("2c0dfea6-8326-44e4-ba55-a994e0bedd10") });
    }

    private static void DoFormatDataValue_Select<T>(T[] a)
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(T));
      table.Rows.Add(); // F1=null
      for (int i = 0; i < a.Length; i++)
        table.Rows.Add(a[i]);
      table.Rows.Add(); // F1=null

      for (int i = 0; i < a.Length; i++)
      {
        string expr = "F1=" + DataTools.FormatDataValue(a[i]);
        DataRow[] rows = table.Select(expr);
        Assert.AreEqual(1, rows.Length, "Select for value " + a[i].ToString() + " (" + typeof(T).ToString() + ")");
        int pRow = table.Rows.IndexOf(rows[0]);
        Assert.AreEqual(i + 1, pRow, "Row position for value " + a[i].ToString() + " (" + typeof(T).ToString() + ")");
      }
    }

    #endregion

    #region IsEmptyValue() / GetEmptyValue()

    [Test]
    public void IsEmptyValue()
    {
      Assert.IsTrue(DataTools.IsEmptyValue(null), "null");
      Assert.IsTrue(DataTools.IsEmptyValue(DBNull.Value), "DBNull");
      Assert.IsTrue(DataTools.IsEmptyValue(String.Empty), "String.Empty");
      Assert.IsTrue(DataTools.IsEmptyValue(0), "0");
      Assert.IsFalse(DataTools.IsEmptyValue(1), "1");

      Assert.IsTrue(DataTools.IsEmptyValue(EmptyArray<Int32>.Empty), "Array[0]");
      Assert.IsFalse(DataTools.IsEmptyValue(new int[] { 0 }), "Array[0]");

      int? nv1 = 1;
      Assert.IsFalse(DataTools.IsEmptyValue(nv1), "int?=1");
      int? nvnull = null;
      Assert.IsTrue(DataTools.IsEmptyValue(nvnull), "int?=null");
    }

    [Test]
    public void GetEmptyValue()
    {
      Assert.IsNull(DataTools.GetEmptyValue(null));
      Assert.AreEqual(0, DataTools.GetEmptyValue(typeof(int)));
      Assert.AreEqual("", DataTools.GetEmptyValue(typeof(string)));
      Assert.AreEqual(DateTime.MinValue, DataTools.GetEmptyValue(typeof(DateTime)));
      Assert.IsNull(DataTools.GetEmptyValue(typeof(int?)));
    }

    #endregion

    #region AreAllDecimalZeros()

    [Test]
    public void AreAllDecimalZeros_DataRow_ColumnNames()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(decimal));
      tbl.Columns.Add("F2", typeof(decimal));
      tbl.Columns.Add("F3", typeof(decimal));
      tbl.Rows.Add(1m, 0m, DBNull.Value); // 0
      Assert.IsFalse(DataTools.AreAllDecimalZeros(tbl.Rows[0], "F1"), "F1");
      Assert.IsTrue(DataTools.AreAllDecimalZeros(tbl.Rows[0], "F2"), "F2");
      Assert.IsTrue(DataTools.AreAllDecimalZeros(tbl.Rows[0], "F3"), "F3");
      Assert.IsFalse(DataTools.AreAllDecimalZeros(tbl.Rows[0], "F1,F2"), "F1,F2");
      Assert.IsTrue(DataTools.AreAllDecimalZeros(tbl.Rows[0], "F2,F3"), "F2,F3");
    }


    [Test]
    public void AreAllDecimalZeros_DataRow_whole()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(decimal));
      tbl.Columns.Add("F2", typeof(decimal));
      tbl.Columns.Add("F3", typeof(decimal));
      tbl.Rows.Add(1m, 0m, DBNull.Value); // 0
      tbl.Rows.Add(0m, 0m, DBNull.Value); // 1
      Assert.IsFalse(DataTools.AreAllDecimalZeros(tbl.Rows[0]), "[0]");
      Assert.IsTrue(DataTools.AreAllDecimalZeros(tbl.Rows[1]), "[1]");
    }

    [Test]
    public void AreAllDecimalZeros_DataTable_ColumnNames()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(decimal));
      tbl.Columns.Add("F2", typeof(decimal));
      tbl.Columns.Add("F3", typeof(decimal));
      tbl.Rows.Add(1m, 0m, DBNull.Value); // 0
      tbl.Rows.Add(0m, 0m, DBNull.Value); // 1
      Assert.IsFalse(DataTools.AreAllDecimalZeros(tbl, "F1"), "F1");
      Assert.IsTrue(DataTools.AreAllDecimalZeros(tbl, "F2"), "F2");
      Assert.IsTrue(DataTools.AreAllDecimalZeros(tbl, "F3"), "F3");
      Assert.IsFalse(DataTools.AreAllDecimalZeros(tbl, "F1,F2"), "F1,F2");
      Assert.IsTrue(DataTools.AreAllDecimalZeros(tbl, "F2,F3"), "F2,F3");
    }


    [Test]
    public void AreAllDecimalZeros_DataTable_whole()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(decimal));
      tbl.Columns.Add("F2", typeof(decimal));
      tbl.Columns.Add("F3", typeof(decimal));
      tbl.Rows.Add(); // 0
      tbl.Rows.Add(); // 1

      Assert.IsTrue(DataTools.AreAllDecimalZeros(tbl), "#1");

      tbl.Rows[1]["F1"] = 0m;
      Assert.IsTrue(DataTools.AreAllDecimalZeros(tbl), "#2");

      tbl.Rows[1]["F1"] = 1m;
      Assert.IsFalse(DataTools.AreAllDecimalZeros(tbl), "#3");
    }

    #endregion

    #region GetEnumRange()

    [Test]
    public void GetEnumRange()
    {
      MinMax<int> r = DataTools.GetEnumRange(typeof(TestEnum));
      Assert.AreEqual(0, r.MinValue, "MinValue");
      Assert.AreEqual(3, r.MaxValue, "MaxValue");
    }


    [Test]
    public void Swap()
    {
      int v1 = 1;
      int v2 = 2;
      DataTools.Swap<int>(ref v1, ref v2);

      Assert.AreEqual(2, v1, "v1");
      Assert.AreEqual(1, v2, "v2");
    }

    #endregion

    #region Dispose()

    [Test]
    public void Dispose()
    {
      DisposableObject obj = new DisposableObject();
      DisposableObject refValue = obj;

      DataTools.Dispose<DisposableObject>(ref refValue);
      Assert.IsNull(refValue, "RefValue #1");
      Assert.IsTrue(obj.IsDisposed, "IdDisposed #1");

      // Вызываем еще раз
      DataTools.Dispose<DisposableObject>(ref refValue);
      Assert.IsNull(refValue, "RefValue #2");
    }

    #endregion

    #region GetValueMatrix()

    [Test]
    public void GetValueMatrix_DataTable()
    {
      // Не будем использовать TestTable, так как там могут добавляться новые столбцы
      DataTable table = new DataTable();
      table.Columns.Add("FString", typeof(string));
      table.Columns.Add("FInt32", typeof(Int32));
      table.Columns.Add("FBoolean", typeof(Boolean));

      table.Rows.Add("AAA", DBNull.Value, false);
      table.Rows.Add("BBB", 123, DBNull.Value);
      table.Rows.Add("CCC", 456, DBNull.Value);
      table.Rows.Add(DBNull.Value, 789, true);
      table.Rows.Add(DBNull.Value, DBNull.Value, DBNull.Value);

      // Удаленные строки должны пропрускаться
      table.Rows[2].Delete();

      object[,] res = DataTools.GetValueMatrix(table);

      object[,] wantedRes = ArrayTools.MatrixFromRows(
        new object[3] { "AAA", null, false },
        new object[3] { "BBB", 123, null },
        new object[3] { null, 789, true },
        new object[3] { null, null, null });

      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void GetValueMatrix_DataTable_empty()
    {
      DataTable table = new DataTable();
      object[,] res = DataTools.GetValueMatrix(table);

      Assert.AreEqual(new object[0, 0], res);
    }

    [Test]
    public void GetValueMatrix_DataTable_noRows()
    {
      DataTable table = TestTable.Create().Clone();
      object[,] res = DataTools.GetValueMatrix(table);

      object[,] wantedRes = new object[0, table.Columns.Count];
      Assert.AreEqual(wantedRes, res);
    }


    [Test]
    public void GetValueMatrix_DataView()
    {
      // Не будем использовать TestTable, так как там могут добавляться новые столбцы
      DataTable table = new DataTable();
      table.Columns.Add("FString", typeof(string));
      table.Columns.Add("FInt32", typeof(Int32));
      table.Columns.Add("FBoolean", typeof(Boolean));

      table.Rows.Add("AAA", DBNull.Value, false);
      table.Rows.Add("BBB", 123, DBNull.Value);
      table.Rows.Add("CCC", 456, DBNull.Value);
      table.Rows.Add(DBNull.Value, 789, true);
      table.Rows.Add(DBNull.Value, DBNull.Value, DBNull.Value);
      table.DefaultView.RowFilter = "FInt32>0";
      table.DefaultView.Sort = "FString DESC";

      object[,] res = DataTools.GetValueMatrix(table.DefaultView);

      object[,] wantedRes = ArrayTools.MatrixFromRows(
        new object[3] { "CCC", 456, null },
        new object[3] { "BBB", 123, null },
        new object[3] { null, 789, true});

      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void GetValueMatrix_DataView_empty()
    {
      DataTable table = new DataTable();
      object[,] res = DataTools.GetValueMatrix(table.DefaultView);

      Assert.AreEqual(new object[0, 0], res);
    }

    [Test]
    public void GetValueMatrix_DataView_noRows()
    {
      DataTable table = TestTable.Create();
      table.DefaultView.RowFilter = "FInt32=666";
      object[,] res = DataTools.GetValueMatrix(table.DefaultView);

      object[,] wantedRes = new object[0, table.Columns.Count];
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region MergeTables()

    [Test]
    public void MergeTables_0()
    {
      DataTable res = DataTools.MergeTables();

      Assert.AreEqual(0, res.Columns.Count, "ColumnCount");
      Assert.AreEqual(0, res.Rows.Count, "RowCount");
      Assert.AreEqual("", res.TableName, "TableName");
      Assert.IsNull(res.DataSet, "DataSet");
    }

    [Test]
    public void MergeTables_1()
    {
      DataTable srcTable1 = TestTable.Create();
      DataTools.SetPrimaryKey(srcTable1, "FInt32NN");
      srcTable1.TableName = "Table1";
      DataSet ds = new DataSet();
      ds.Tables.Add(srcTable1);

      DataTable res = DataTools.MergeTables(srcTable1);

      Assert.AreNotSame(srcTable1, res, "Not same");
      Assert.AreEqual(DataTools.GetColumnNames(srcTable1), DataTools.GetColumnNames(res), "Column names");
      Assert.AreEqual(DataTools.GetValueMatrix(srcTable1), DataTools.GetValueMatrix(res), "Value matrix");

      Assert.AreEqual("", res.TableName, "TableName");
      Assert.IsNull(res.DataSet, "DataSet");
      Assert.AreEqual("", DataTools.GetPrimaryKey(res), "PrimaryKey");
    }

    [Test]
    public void MergeTables_2()
    {
      DataTable srcTable1 = new DataTable("Table1");
      srcTable1.Columns.Add("F1", typeof(string));
      srcTable1.Columns.Add("F2", typeof(Int32));
      DataTools.SetPrimaryKey(srcTable1, "F1");
      srcTable1.Rows.Add("AAA", 123);
      srcTable1.Rows.Add("BBB", 456);
      srcTable1.Rows.Add("CCC", DBNull.Value);
      srcTable1.Rows.Add("DDD", 789);
      srcTable1.Rows[3].Delete(); // Удаленные строки пропускаются

      DataTable srcTable2 = new DataTable("Table2");
      srcTable2.Columns.Add("F3", typeof(Int64));
      srcTable2.Columns.Add("F2", typeof(Int32));
      srcTable2.Rows.Add(3L, 666);
      srcTable2.Rows.Add(1L, 123);
      srcTable2.Rows.Add(2L, DBNull.Value);
      srcTable2.Rows[0].Delete(); // Удаленные строки пропускаются

      DataTable res = DataTools.MergeTables(srcTable1, srcTable2);

      Assert.AreEqual(new string[3] { "F1", "F2", "F3" }, DataTools.GetColumnNames(res), "Column names");

      object[,] wantedMatrix = ArrayTools.MatrixFromRows<object>(
        new object[3] { "AAA", 123, null},
        new object[3] { "BBB", 456, null },
        new object[3] { "CCC", null, null },
        new object[3] { null, 123, 1L },
        new object[3] { null, null, 2L }
        );
      Assert.AreEqual(wantedMatrix, DataTools.GetValueMatrix(res), "Value matrix");
      Assert.AreEqual("", res.TableName, "TableName");
      Assert.IsNull(res.DataSet, "DataSet");
      Assert.AreEqual("", DataTools.GetPrimaryKey(res), "PrimaryKey");
    }


    [Test]
    public void MergeTables_3()
    {
      DataTable srcTable1 = new DataTable("Table1");
      srcTable1.Columns.Add("F1", typeof(string));
      srcTable1.Columns.Add("F2", typeof(Int32));
      DataTools.SetPrimaryKey(srcTable1, "F1");
      srcTable1.Rows.Add("AAA", 123);
      srcTable1.Rows.Add("BBB", 456);

      DataTable srcTable2 = new DataTable("Table2");
      srcTable2.Columns.Add("F3", typeof(Int64));
      srcTable2.Columns.Add("F2", typeof(Int32));
      srcTable2.Rows.Add(1L, 123);
      srcTable2.Rows.Add(2L, DBNull.Value);

      DataTable srcTable3 = new DataTable("Table3");
      srcTable3.Columns.Add("F2", typeof(Int32));
      srcTable3.Columns.Add("F1", typeof(string));
      srcTable3.Rows.Add(789, "CCC");


      DataTable res = DataTools.MergeTables(srcTable1, srcTable2, srcTable3);

      Assert.AreEqual(new string[3] { "F1", "F2", "F3" }, DataTools.GetColumnNames(res), "Column names");

      object[,] wantedMatrix = ArrayTools.MatrixFromRows<object>(
        new object[3] { "AAA", 123, null },
        new object[3] { "BBB", 456, null },
        new object[3] { null, 123, 1L },
        new object[3] { null, null, 2L },
        new object[3] { "CCC", 789, null }
        );
      Assert.AreEqual(wantedMatrix, DataTools.GetValueMatrix(res), "Value matrix");
      Assert.AreEqual("", res.TableName, "TableName");
      Assert.IsNull(res.DataSet, "DataSet");
      Assert.AreEqual("", DataTools.GetPrimaryKey(res), "PrimaryKey");
    }

    #endregion
  }
}
