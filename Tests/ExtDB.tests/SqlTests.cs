using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.Remoting;
using System.Data;
using FreeLibSet.Data.SQLite;
using FreeLibSet.Data.SqlClient;
using FreeLibSet.Data.Npgsql;
using Npgsql;
#if !MONO
using FreeLibSet.Data.OleDb;
#endif
using FreeLibSet.Tests;

namespace ExtDB_tests.Data
{
  public abstract class SqlTestBase: FixtureWithSetUp
  {
    #region Конструктор

    protected SqlTestBase()
    {
      _TestStruct = new DBxStruct();
      DBxTableStruct ts;
      _TestData = new DataSet();
      DataTable tbl;

      #region Тестовая таблица 1 - фильтры и функции для полей разных типов

      const string Guid1 = "83e4ea91-6f0b-4ab6-9d58-c981418c26b7";
      const string Guid2 = "c84cc4da-6f1d-488b-b647-8bfa5642fe23";
      const string Guid0 = "00000000-0000-0000-0000-000000000000"; // Guid.Empty

      ts = _TestStruct.Tables.Add("Test1");
      ts.Columns.AddId();
      ts.Columns.AddString("ColS", 10, true);
      ts.Columns.AddString("ColS2", 10, true);
      ts.Columns.AddInt("ColI", true);
      ts.Columns.AddDouble("ColF", true);
      ts.Columns.AddDouble("ColF2", true);
      ts.Columns.AddBoolean("ColL");
      ts.Columns.AddDate("ColD", true);
      ts.Columns.AddDate("ColD2", true);
      ts.Columns.AddDateTime("ColDT", true);
      ts.Columns.AddTime("ColT", true);
      ts.Columns.AddGuid("ColG", true);

      DBNull NULL = DBNull.Value; // для большей наглядности таблицы


      tbl = ts.CreateDataTable();
      _TestData.Tables.Add(tbl);
      //          Id  ColS          ColS2   ColI  ColF ColF2 ColL   ColD                      ColD2                     ColDT                                 ColT                    ColG           
      tbl.Rows.Add(1, "AAA", "***", 10, 5.0, NULL, true, new DateTime(2023, 1, 1), new DateTime(2023, 1, 5), new DateTime(2023, 1, 1, 0, 0, 1), new TimeSpan(12, 34, 56), new Guid(Guid1));
      tbl.Rows.Add(2, "BcDe", "###", 20, NULL, -1.0, false, NULL, new DateTime(2023, 1, 5), new DateTime(2023, 1, 1, 0, 0, 0), TimeSpan.Zero, Guid.Empty);
      tbl.Rows.Add(3, NULL, "???", NULL, NULL, 2.0, false, NULL, NULL, NULL, NULL, NULL);
      tbl.Rows.Add(4, "", "%%%", 0, 0.0, 3.0, false, new DateTime(2001, 1, 1), NULL, new DateTime(2023, 1, 1, 23, 59, 59), new TimeSpan(12, 34, 56), new Guid(Guid2));
      tbl.Rows.Add(5, "abcdefghij", "*#?%", -10, -5.0, -3.0, true, new DateTime(2023, 1, 2), new DateTime(2023, 1, 2), new DateTime(2023, 1, 2, 12, 34, 56), new TimeSpan(12, 34, 57), new Guid(Guid1));
      tbl.AcceptChanges();

      #endregion

      #region Тесты для фильтров

      List<FilterTestInfo> filters = new List<FilterTestInfo>();

      #region CompareFilter

      filters.Add(new FilterTestInfo(new CompareFilter("Id", "ColI", CompareKind.GreaterThan, false, DBxColumnType.Int), 4, 5));
      filters.Add(new FilterTestInfo(new CompareFilter("Id", "ColI", CompareKind.GreaterThan, true, DBxColumnType.Int), 3, 4, 5));
      filters.Add(new FilterTestInfo(new CompareFilter("Id", "ColI", CompareKind.LessThan, false, DBxColumnType.Int), 1, 2));
      filters.Add(new FilterTestInfo(new CompareFilter("Id", "ColI", CompareKind.LessThan, true, DBxColumnType.Int), 1, 2));

      filters.Add(new FilterTestInfo(new CompareFilter("ColI", "ColF", CompareKind.Equal, false, DBxColumnType.Int), 4));
      filters.Add(new FilterTestInfo(new CompareFilter("ColI", "ColF", CompareKind.Equal, true, DBxColumnType.Int), 3, 4));

      filters.Add(new FilterTestInfo(new CompareFilter(new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("Id"), new DBxConst(10)), new DBxColumn("ColI"), CompareKind.Equal), 1, 2));
      filters.Add(new FilterTestInfo(new CompareFilter(new DBxFunction(DBxFunctionKind.Abs, new DBxColumn("ColI")), new DBxConst(10)), 1, 5));
      filters.Add(new FilterTestInfo(new CompareFilter(new DBxFunction(DBxFunctionKind.Abs, new DBxColumn("ColF")), new DBxConst(0.0), CompareKind.GreaterThan), 1, 5));

      #endregion

      #region ValueFilter

      filters.Add(new FilterTestInfo(new ValueFilter("ColI", 20), 2));
      filters.Add(new FilterTestInfo(new ValueFilter("Id", 3, CompareKind.Equal), 3));
      filters.Add(new FilterTestInfo(new ValueFilter("Id", 3, CompareKind.NotEqual), 1, 2, 4, 5));
      filters.Add(new FilterTestInfo(new ValueFilter("Id", 3, CompareKind.GreaterThan), 4, 5));
      filters.Add(new FilterTestInfo(new ValueFilter("Id", 3, CompareKind.GreaterOrEqualThan), 3, 4, 5));
      filters.Add(new FilterTestInfo(new ValueFilter("Id", 3, CompareKind.LessThan), 1, 2));
      filters.Add(new FilterTestInfo(new ValueFilter("Id", 3, CompareKind.LessOrEqualThan), 1, 2, 3));

      filters.Add(new FilterTestInfo(new ValueFilter("ColI", 0, CompareKind.Equal, DBxColumnType.Int), 3, 4)); // DBNull считается 0
      filters.Add(new FilterTestInfo(new ValueFilter("ColI", 0, CompareKind.NotEqual, DBxColumnType.Int), 1, 2, 5));
      filters.Add(new FilterTestInfo(new ValueFilter("ColI", 0, CompareKind.GreaterThan, DBxColumnType.Int), 1, 2));
      filters.Add(new FilterTestInfo(new ValueFilter("ColI", 0, CompareKind.GreaterOrEqualThan, DBxColumnType.Int), 1, 2, 3, 4));
      filters.Add(new FilterTestInfo(new ValueFilter("ColI", 0, CompareKind.LessThan, DBxColumnType.Int), 5));
      filters.Add(new FilterTestInfo(new ValueFilter("ColI", 0, CompareKind.LessOrEqualThan, DBxColumnType.Int), 3, 4, 5));

      filters.Add(new FilterTestInfo(new ValueFilter("ColF", 0.0, CompareKind.Equal), 2, 3, 4)); // DBNull считается 0
      filters.Add(new FilterTestInfo(new ValueFilter("ColF", 0.0, CompareKind.GreaterThan), 1));

      filters.Add(new FilterTestInfo(new ValueFilter("ColL", true, CompareKind.Equal, DBxColumnType.Boolean), 1, 5));
      filters.Add(new FilterTestInfo(new ValueFilter("ColL", true, CompareKind.NotEqual, DBxColumnType.Boolean), 2, 3, 4));

      filters.Add(new FilterTestInfo(new ValueFilter("ColD", new DateTime(2023, 1, 1), CompareKind.Equal, DBxColumnType.Date), 1));
      filters.Add(new FilterTestInfo(new ValueFilter("ColD", new DateTime(2023, 1, 1), CompareKind.NotEqual, DBxColumnType.Date), 2, 3, 4, 5));
      filters.Add(new FilterTestInfo(new ValueFilter("ColD", new DateTime(2023, 1, 1), CompareKind.GreaterThan, DBxColumnType.Date), 5));
      filters.Add(new FilterTestInfo(new ValueFilter("ColD", new DateTime(2023, 1, 1), CompareKind.GreaterOrEqualThan, DBxColumnType.Date), 1, 5));
      filters.Add(new FilterTestInfo(new ValueFilter("ColD", new DateTime(2023, 1, 1), CompareKind.LessThan, DBxColumnType.Date), 2, 3, 4));
      filters.Add(new FilterTestInfo(new ValueFilter("ColD", new DateTime(2023, 1, 1), CompareKind.LessOrEqualThan, DBxColumnType.Date), 1, 2, 3, 4));

      filters.Add(new FilterTestInfo(new ValueFilter("ColG", new Guid(Guid1), CompareKind.Equal, DBxColumnType.Guid), 1, 5));
      filters.Add(new FilterTestInfo(new ValueFilter("ColG", Guid.Empty, CompareKind.Equal, DBxColumnType.Guid), 2, 3));
      filters.Add(new FilterTestInfo(new ValueFilter("ColG", Guid1, CompareKind.Equal, DBxColumnType.Guid), 1, 5));
      filters.Add(new FilterTestInfo(new ValueFilter("ColG", Guid0, CompareKind.Equal, DBxColumnType.Guid), 2, 3));
      filters.Add(new FilterTestInfo(new ValueFilter("ColG", new Guid(Guid1), CompareKind.NotEqual, DBxColumnType.Guid), 2, 3, 4));
      filters.Add(new FilterTestInfo(new ValueFilter("ColG", Guid.Empty, CompareKind.NotEqual, DBxColumnType.Guid), 1, 4, 5));
      filters.Add(new FilterTestInfo(new ValueFilter("ColG", Guid1, CompareKind.NotEqual, DBxColumnType.Guid), 2, 3, 4));
      filters.Add(new FilterTestInfo(new ValueFilter("ColG", Guid0, CompareKind.NotEqual, DBxColumnType.Guid), 1, 4, 5));

      filters.Add(new FilterTestInfo(TestCompat.All | TestCompat.TimeSpan, new ValueFilter("ColT", new TimeSpan(12, 34, 56), CompareKind.Equal, DBxColumnType.Time), 1, 4));
      filters.Add(new FilterTestInfo(TestCompat.All | TestCompat.TimeSpan, new ValueFilter("ColT", TimeSpan.Zero, CompareKind.Equal, DBxColumnType.Time), 2, 3));
      filters.Add(new FilterTestInfo(TestCompat.All | TestCompat.TimeSpan, new ValueFilter("ColT", new TimeSpan(12, 34, 56), CompareKind.NotEqual, DBxColumnType.Time), 2, 3, 5));
      filters.Add(new FilterTestInfo(TestCompat.All | TestCompat.TimeSpan, new ValueFilter("ColT", TimeSpan.Zero, CompareKind.NotEqual, DBxColumnType.Time), 1, 4, 5));

      // сравнение с NULL. Для DataView не работает
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColS", null, CompareKind.Equal, DBxColumnType.String), 3));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColI", null, CompareKind.Equal, DBxColumnType.Int), 3));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColF", null, CompareKind.Equal, DBxColumnType.Float), 2, 3));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColD", null, CompareKind.Equal, DBxColumnType.Date), 2, 3));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColG", null, CompareKind.Equal, DBxColumnType.Guid), 3));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView | TestCompat.TimeSpan, new ValueFilter("ColT", null, CompareKind.Equal, DBxColumnType.Time), 3));

      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColS", null, CompareKind.NotEqual, DBxColumnType.String), 1, 2, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColI", null, CompareKind.NotEqual, DBxColumnType.Int), 1, 2, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColF", null, CompareKind.NotEqual, DBxColumnType.Float), 1, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColD", null, CompareKind.NotEqual, DBxColumnType.Date), 1, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColG", null, CompareKind.NotEqual, DBxColumnType.Guid), 1, 2, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView | TestCompat.TimeSpan, new ValueFilter("ColT", null, CompareKind.NotEqual, DBxColumnType.Time), 1, 2, 4, 5));

      #endregion

      #region IdsFilter

      filters.Add(new FilterTestInfo(new IdsFilter(3), 3));
      filters.Add(new FilterTestInfo(new IdsFilter(666)));
      filters.Add(new FilterTestInfo(new IdsFilter(new Int32[] { 4, 666 }), 4));
      filters.Add(new FilterTestInfo(new IdsFilter(IdList.FromArray(new Int32[] { 4, 666 })), 4));
      filters.Add(new FilterTestInfo(new IdsFilter("ColI", 10), 1));
      filters.Add(new FilterTestInfo(new IdsFilter("ColI", 666)));

      #endregion

      #region ValuesFilter

      // Нужно отличать случаи с одним значением в массиве, т.к. они форматируются без использования конструкции "IN"

      filters.Add(new FilterTestInfo(new ValuesFilter("ColS", new string[] { "abcdefghij", "AAA" }), 1, 5));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColS", new string[] { "AAA", "" }), 1, 3, 4));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColS", new string[] { "AAA" }), 1));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColS", new string[] { "" }), 3, 4));

      filters.Add(new FilterTestInfo(new ValuesFilter("Id", new Int32[] { 4, 3, 2 }), 2, 3, 4));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColI", new Int32[] { 10, 20 }), 1, 2));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColI", new Int32[] { 0, 10, 20 }), 1, 2, 3, 4));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColI", new Int32[] { 20 }), 2));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColI", new Int32[] { 0 }), 3, 4));

      filters.Add(new FilterTestInfo(new ValuesFilter("ColF", new double[] { -5.0, 5.0 }), 1, 5));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColF", new int[] { -5, 5 }, DBxColumnType.Float), 1, 5)); // Преобразование типа данных
      filters.Add(new FilterTestInfo(new ValuesFilter("ColF", new int[] { 0, 5 }, DBxColumnType.Float), 1, 2, 3, 4));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColF", new double[] { 5.0 }), 1));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColF", new int[] { 5 }, DBxColumnType.Float), 1)); // Преобразование типа данных
      filters.Add(new FilterTestInfo(new ValuesFilter("ColF", new int[] { 0 }, DBxColumnType.Float), 2, 3, 4));

      // Реально ValuesFilter не используется для Bool
      filters.Add(new FilterTestInfo(new ValuesFilter("ColL", new bool[] { false }), 2, 3, 4));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColL", new bool[] { true }), 1, 5));

      filters.Add(new FilterTestInfo(new ValuesFilter("ColD", new DateTime[] { new DateTime(2023, 1, 1), new DateTime(2023, 1, 2) }), 1, 5));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColD", new DateTime[] { new DateTime(2023, 1, 2) }), 5));

      filters.Add(new FilterTestInfo(new ValuesFilter("ColG", new Guid[] { new Guid(Guid1), new Guid(Guid2) }), 1, 4, 5));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColG", new string[] { Guid1, Guid2 }, DBxColumnType.Guid), 1, 4, 5));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColG", new Guid[] { new Guid(Guid1), new Guid(Guid0) }), 1, 2, 3, 5));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColG", new string[] { Guid1, Guid0 }, DBxColumnType.Guid), 1, 2, 3, 5));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColG", new Guid[] { new Guid(Guid1) }, DBxColumnType.Guid), 1, 5));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColG", new string[] { Guid1 }, DBxColumnType.Guid), 1, 5));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColG", new Guid[] { Guid.Empty }, DBxColumnType.Guid), 2, 3));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColG", new string[] { Guid0 }, DBxColumnType.Guid), 2, 3));

      filters.Add(new FilterTestInfo(TestCompat.All | TestCompat.TimeSpan, new ValuesFilter("ColT", new TimeSpan[] { new TimeSpan(12, 34, 56), new TimeSpan(12, 34, 57) }), 1, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.All | TestCompat.TimeSpan, new ValuesFilter("ColT", new TimeSpan[] { new TimeSpan(12, 34, 56), TimeSpan.Zero }), 1, 2, 3, 4));
      filters.Add(new FilterTestInfo(TestCompat.All | TestCompat.TimeSpan, new ValuesFilter("ColT", new TimeSpan[] { new TimeSpan(12, 34, 56) }), 1, 4));
      filters.Add(new FilterTestInfo(TestCompat.All | TestCompat.TimeSpan, new ValuesFilter("ColT", new TimeSpan[] { TimeSpan.Zero }), 2, 3));

      #endregion

      #region AndFilter

      filters.Add(new FilterTestInfo(new AndFilter(new ValueFilter("Id", 2, CompareKind.GreaterOrEqualThan),
        new ValueFilter("Id", 4, CompareKind.LessOrEqualThan)), 2, 3, 4));
      filters.Add(new FilterTestInfo(new AndFilter(new ValueFilter("ColL", true),
        new OrFilter(new ValueFilter("ColD", new DateTime(2023, 1, 1)), new IdsFilter(new Int32[] { 2, 3 }))), 1));
      filters.Add(new FilterTestInfo(AndFilter.FromArray(new DBxFilter[] {
        new ValueFilter("Id", 2, CompareKind.GreaterOrEqualThan),
        new ValueFilter("Id", 4, CompareKind.LessOrEqualThan),
        new ValueFilter("ColD", new DateTime(2001, 1, 1)) }),
        4));
      filters.Add(new FilterTestInfo(
        new ValueFilter("Id", 2, CompareKind.GreaterOrEqualThan) &
        new ValueFilter("Id", 4, CompareKind.LessOrEqualThan) &
        new ValueFilter("ColD", new DateTime(2001, 1, 1)),
        4));

      #endregion

      #region OrFilter

      filters.Add(new FilterTestInfo(new OrFilter(new ValueFilter("Id", 2, CompareKind.LessOrEqualThan),
        new ValueFilter("Id", 4, CompareKind.GreaterOrEqualThan)), 1, 2, 4, 5));
      filters.Add(new FilterTestInfo(new OrFilter(new ValueFilter("ColL", true),
        new AndFilter(new ValueFilter("ColD", new DateTime(2023, 1, 1)), new IdsFilter(new Int32[] { 1, 2 }))), 1, 5));
      filters.Add(new FilterTestInfo(OrFilter.FromArray(new DBxFilter[] {
        new ValueFilter("Id", 2, CompareKind.Equal),
        new ValueFilter("Id", 4, CompareKind.GreaterOrEqualThan),
        new ValueFilter("ColD", new DateTime(2023, 1, 1)) }),
        1, 2, 4, 5));
      filters.Add(new FilterTestInfo(
        new ValueFilter("Id", 2, CompareKind.Equal) |
        new ValueFilter("Id", 4, CompareKind.GreaterOrEqualThan) |
        new ValueFilter("ColD", new DateTime(2023, 1, 1)),
        1, 2, 4, 5));

      #endregion

      #region NotFilter

      filters.Add(new FilterTestInfo(new NotFilter(new IdsFilter(new Int32[] { 1, 4 })), 2, 3, 5));
      filters.Add(new FilterTestInfo(new NotFilter(new StartsWithFilter("ColS", "AA")), 2, 3, 4, 5));

      filters.Add(new FilterTestInfo(!new IdsFilter(new Int32[] { 1, 4 }), 2, 3, 5));
      filters.Add(new FilterTestInfo(!!new IdsFilter(new Int32[] { 1, 4 }), 1, 4));

      #endregion

      #region NumRangeFilter

      filters.Add(new FilterTestInfo(new NumRangeFilter("Id", 2, 4), 2, 3, 4));
      filters.Add(new FilterTestInfo(new NumRangeFilter("Id", 2, null), 2, 3, 4, 5));
      filters.Add(new FilterTestInfo(new NumRangeFilter("Id", null, 4), 1, 2, 3, 4));
      filters.Add(new FilterTestInfo(new NumRangeFilter("Id", null, null), 1, 2, 3, 4, 5));
      filters.Add(new FilterTestInfo(new NumRangeFilter("Id", 3, 2)));

      filters.Add(new FilterTestInfo(new NumRangeFilter("ColI", -10, 0), 3, 4, 5));

      filters.Add(new FilterTestInfo(new NumRangeFilter("ColF", 0.0, 5.0), 1, 2, 3, 4));
      filters.Add(new FilterTestInfo(new NumRangeFilter("ColF", -5.0, 0.0), 2, 3, 4, 5));

      #endregion

      #region NumRangeInclusionFilter

      filters.Add(new FilterTestInfo(new NumRangeInclusionFilter("ColF", "ColF2", 5.0), 1));
      filters.Add(new FilterTestInfo(new NumRangeInclusionFilter("ColF", "ColF2", 4.0)));
      filters.Add(new FilterTestInfo(new NumRangeInclusionFilter("ColF", "ColF2", 2.0), 3, 4));
      filters.Add(new FilterTestInfo(new NumRangeInclusionFilter("ColF", "ColF2", 0.0), 3, 4));
      filters.Add(new FilterTestInfo(new NumRangeInclusionFilter("ColF", "ColF2", -2.0), 2, 3));
      filters.Add(new FilterTestInfo(new NumRangeInclusionFilter("ColF", "ColF2", -3.0), 2, 3, 5));
      filters.Add(new FilterTestInfo(new NumRangeInclusionFilter("ColF", "ColF2", -5.0), 2, 3, 5));
      filters.Add(new FilterTestInfo(new NumRangeInclusionFilter("ColF", "ColF2", -5.1), 2, 3));

      #endregion

      #region NumRangeCrossFilter

      filters.Add(new FilterTestInfo(new NumRangeCrossFilter("ColF", "ColF2", 1.0, 2.0), 3, 4));
      filters.Add(new FilterTestInfo(new NumRangeCrossFilter("ColF", "ColF2", 6.0, 7.0), 1));
      filters.Add(new FilterTestInfo(new NumRangeCrossFilter("ColF", "ColF2", 0.0, 0.0), 3, 4));
      filters.Add(new FilterTestInfo(new NumRangeCrossFilter("ColF", "ColF2", -1.0, 0.0), 2, 3, 4));
      filters.Add(new FilterTestInfo(new NumRangeCrossFilter("ColF", "ColF2", null, 0.0), 2, 3, 4, 5));
      filters.Add(new FilterTestInfo(new NumRangeCrossFilter("ColF", "ColF2", null, -2.0), 2, 3, 5));
      filters.Add(new FilterTestInfo(new NumRangeCrossFilter("ColF", "ColF2", 0, null), 1, 3, 4));
      filters.Add(new FilterTestInfo(new NumRangeCrossFilter("ColF", "ColF2", (double?)null, (double?)null), 1, 2, 3, 4, 5));

      #endregion

      #region DateRangeFilter

      filters.Add(new FilterTestInfo(new DateRangeFilter("ColD", new DateTime(2001, 1, 1), new DateTime(2022, 12, 31)), 4));
      filters.Add(new FilterTestInfo(new DateRangeFilter("ColD", 2023, 1), 1, 5));
      filters.Add(new FilterTestInfo(new DateRangeFilter("ColD", new DateTime(2023, 1, 1), null), 1, 5));
      filters.Add(new FilterTestInfo(new DateRangeFilter("ColD", null, new DateTime(2023, 1, 1)), 1, 4));

      filters.Add(new FilterTestInfo(new DateRangeFilter("ColDT", 2023, 1, 1), 1, 2, 4));
      filters.Add(new FilterTestInfo(new DateRangeFilter("ColDT", null, new DateTime(2023, 1, 1)), 1, 2, 4));


      #endregion

      #region DateRangeInclusionFilter

      filters.Add(new FilterTestInfo(new DateRangeInclusionFilter("ColD", "ColD2", new DateTime(2022, 12, 31)), 2, 3, 4));
      filters.Add(new FilterTestInfo(new DateRangeInclusionFilter("ColD", "ColD2", new DateTime(2023, 1, 1)), 1, 2, 3, 4));
      filters.Add(new FilterTestInfo(new DateRangeInclusionFilter("ColD", "ColD2", new DateTime(2023, 1, 2)), 1, 2, 3, 4, 5));
      filters.Add(new FilterTestInfo(new DateRangeInclusionFilter("ColD", "ColD2", new DateTime(2023, 1, 6)), 3, 4));

      #endregion

      #region DateRangeCrossFilter

      filters.Add(new FilterTestInfo(new DateRangeCrossFilter("ColD", "ColD2", new DateTime(2022, 12, 31), new DateTime(2023, 1, 1)), 1, 2, 3, 4));
      filters.Add(new FilterTestInfo(new DateRangeCrossFilter("ColD", "ColD2", new DateTime(2023, 1, 2), new DateTime(2023, 1, 5)), 1, 2, 3, 4, 5));
      filters.Add(new FilterTestInfo(new DateRangeCrossFilter("ColD", "ColD2", new DateTime(2023, 1, 5), new DateTime(2023, 1, 5)), 1, 2, 3, 4));
      filters.Add(new FilterTestInfo(new DateRangeCrossFilter("ColD", "ColD2", new DateTime(2023, 1, 6), new DateTime(2023, 1, 6)), 3, 4));
      filters.Add(new FilterTestInfo(new DateRangeCrossFilter("ColD", "ColD2", null, new DateTime(2022, 12, 31)), 2, 3, 4));
      filters.Add(new FilterTestInfo(new DateRangeCrossFilter("ColD", "ColD2", null, new DateTime(2023, 1, 1)), 1, 2, 3, 4));
      filters.Add(new FilterTestInfo(new DateRangeCrossFilter("ColD", "ColD2", new DateTime(2023, 1, 5), null), 1, 2, 3, 4));
      filters.Add(new FilterTestInfo(new DateRangeCrossFilter("ColD", "ColD2", new DateTime(2023, 1, 6), null), 3, 4));
      filters.Add(new FilterTestInfo(new DateRangeCrossFilter("ColD", "ColD2", null, null), 1, 2, 3, 4, 5));

      #endregion

      #region NotNullFilter

      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new NotNullFilter("ColS", DBxColumnType.String), 1, 2, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new NotNullFilter("ColI", DBxColumnType.Int), 1, 2, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new NotNullFilter("ColF", DBxColumnType.Float), 1, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new NotNullFilter("ColD", DBxColumnType.Date), 1, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new NotNullFilter("ColDT", DBxColumnType.DateTime), 1, 2, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView | TestCompat.TimeSpan, new NotNullFilter("ColT", DBxColumnType.Time), 1, 2, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new NotNullFilter("ColG", DBxColumnType.Guid), 1, 2, 4, 5));

      #endregion

      #region StringValueFilter

      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StringValueFilter("ColS", "aaa", false)));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StringValueFilter("ColS", "aaa", true), 1));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StringValueFilter("ColS", "AA", false)));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StringValueFilter("ColS", "AAA", false), 1));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StringValueFilter("ColS", "", false), 3, 4));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StringValueFilter("ColS", "", true), 3, 4));

      #endregion

      #region StartsWithFilter

      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StartsWithFilter("ColS", "a", false), 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StartsWithFilter("ColS", "a", true), 1, 5));
      // Использование специальных символов
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StartsWithFilter("ColS2", "*", true), 1, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StartsWithFilter("ColS2", "**", true), 1));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StartsWithFilter("ColS2", "***", true), 1));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StartsWithFilter("ColS2", "****", false)));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StartsWithFilter("ColS2", "#", true), 2));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StartsWithFilter("ColS2", "##", true), 2));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StartsWithFilter("ColS2", "###", true), 2));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StartsWithFilter("ColS2", "####", false)));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StartsWithFilter("ColS2", "?", true), 3));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StartsWithFilter("ColS2", "??", true), 3));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new StartsWithFilter("ColS2", "???", true), 3));
      filters.Add(new FilterTestInfo(new StartsWithFilter("ColS2", "????", false)));
      filters.Add(new FilterTestInfo(new StartsWithFilter("ColS2", "%", true), 4));
      filters.Add(new FilterTestInfo(new StartsWithFilter("ColS2", "%%", true), 4));
      filters.Add(new FilterTestInfo(new StartsWithFilter("ColS2", "%%%", true), 4));
      filters.Add(new FilterTestInfo(new StartsWithFilter("ColS2", "%%%%", false)));
      filters.Add(new FilterTestInfo(new StartsWithFilter("ColS2", "*#", true), 5));
      filters.Add(new FilterTestInfo(new StartsWithFilter("ColS2", "*#?", true), 5));
      filters.Add(new FilterTestInfo(new StartsWithFilter("ColS2", "*#?%", true), 5));
      filters.Add(new FilterTestInfo(new StartsWithFilter("ColS2", "*#?%%", true)));

      #endregion

      #region SubstringFilter

      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new SubstringFilter("ColS", 1, "cd", false)));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new SubstringFilter("ColS", 1, "cd", true), 2));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new SubstringFilter("ColS", 5, "fg", true), 5));

      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new SubstringFilter("ColS2", 1, "**", true), 1));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new SubstringFilter("ColS2", 1, "##", true), 2));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new SubstringFilter("ColS2", 1, "??", true), 3));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new SubstringFilter("ColS2", 1, "%%", true), 4));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new SubstringFilter("ColS2", 0, "*", true), 1, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new SubstringFilter("ColS2", 1, "#", true), 2, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new SubstringFilter("ColS2", 2, "?", true), 3, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new SubstringFilter("ColS2", 3, "%", true), 5));

      #endregion

      #region DummyFilters

      filters.Add(new FilterTestInfo(new DummyFilter(false)));
      filters.Add(new FilterTestInfo(new DummyFilter(true), 1, 2, 3, 4, 5));

      #endregion

      _AllTestFilters = filters.ToArray();

      #endregion

      #region Тесты для функций

      List<FunctionTestInfo> functions = new List<FunctionTestInfo>();

      #region Математические операции

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Add, new DBxColumn("Id"), new DBxConst(10)), 11, 12, 13, 14, 15));
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Add, new DBxColumn("Id"), new DBxColumn("ColI")), 11, 22, null, 4, -5));
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Add, new DBxColumn("ColI"), new DBxColumn("ColF")), 15.0, null, null, 0.0, -15.0));
      //functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Add, new DBxColumn("ColD"), new DBxColumn("ColT")), 
      //  new DateTime(2023, 1, 1, 12, 34, 56), null, null, new DateTime(2001, 1, 1, 12, 34, 56), new DateTime(2023, 1, 2, 12, 34, 57)));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Substract, new DBxColumn("Id"), new DBxConst(10)), -9, -8, -7, -6, -5));
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Substract, new DBxColumn("Id"), new DBxColumn("ColI")), -9, -18, null, 4, 15));
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Substract, new DBxColumn("ColI"), new DBxColumn("ColF")), 5.0, null, null, 0.0, -5.0));
      //functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Substract, new DBxColumn("ColD"), new DBxColumn("ColT")),
      //  new DateTime(2022, 12, 31, 11, 25, 4), null, null, new DateTime(2000, 12, 31, 11, 25, 4), new DateTime(2023, 1, 1, 11, 25, 3)));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("Id"), new DBxConst(10)), 10, 20, 30, 40, 50));
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("Id"), new DBxColumn("ColI")), 10, 40, null, 0, -50));
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("ColI"), new DBxColumn("ColF")), 50.0, null, null, 0.0, 50.0));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Divide, new DBxColumn("ColF"), new DBxConst(2.0)), 2.5, null, null, 0.0, -2.5));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Neg, new DBxColumn("Id")), -1, -2, -3, -4, -5));
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Neg, new DBxColumn("ColI")), -10, -20, null, 0, 10));
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Neg, new DBxColumn("ColF")), -5.0, null, null, 0.0, 5.0));
      //functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Neg, new DBxColumn("ColT")), 
      //  new TimeSpan(-12, -34, -56), TimeSpan.Zero, null, new TimeSpan(-12, -34, -56), new TimeSpan(-12, -34, -57)));

      #endregion

      #region Операции сравнения

      #region Int-Int

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Equal,
        new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("Id"), new DBxConst(10)),
        new DBxColumn("ColI")),
        true, true, null, false, false));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.GreaterThan,
        new DBxColumn("Id"), new DBxColumn("ColI")),
        false, false, null, true, true));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.GreaterOrEqualThan,
        new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("Id"), new DBxConst(10)),
        new DBxColumn("ColI")),
        true, true, null, true, true));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.LessThan,
        new DBxColumn("Id"), new DBxColumn("ColI")),
        true, true, null, false, false));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.LessOrEqualThan,
        new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("Id"), new DBxConst(10)),
        new DBxColumn("ColI")),
        true, true, null, false, false));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.NotEqual,
        new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("Id"), new DBxConst(10)),
        new DBxColumn("ColI")),
        false, false, null, true, true));

      #endregion

      #region Int-Float

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Equal,
        new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("Id"), new DBxConst(5.0)),
        new DBxColumn("ColF")),
        true, null, null, false, false));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.GreaterThan,
        new DBxColumn("Id"), new DBxColumn("ColF")),
        false, null, null, true, true));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.GreaterOrEqualThan,
        new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("Id"), new DBxConst(5.0)),
        new DBxColumn("ColF")),
        true, null, null, true, true));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.LessThan,
        new DBxColumn("Id"), new DBxColumn("ColF")),
        true, null, null, false, false));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.LessOrEqualThan,
        new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("Id"), new DBxConst(5.0)),
        new DBxColumn("ColF")),
        true, null, null, false, false));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.NotEqual,
        new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("Id"), new DBxConst(5.0)),
        new DBxColumn("ColF")),
        false, null, null, true, true));

      #endregion

      #region Date

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Equal,
        new DBxColumn("ColD"), new DBxColumn("ColD2")),
        false, null, null, null, true));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.GreaterThan,
        new DBxColumn("ColD"), new DBxColumn("ColD2")),
        false, null, null, null, false));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.GreaterOrEqualThan,
        new DBxColumn("ColD"), new DBxColumn("ColD2")),
        false, null, null, null, true));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.LessThan,
        new DBxColumn("ColD"), new DBxColumn("ColD2")),
        true, null, null, null, false));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.LessOrEqualThan,
        new DBxColumn("ColD"), new DBxColumn("ColD2")),
        true, null, null, null, true));


      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.NotEqual,
        new DBxColumn("ColD"), new DBxColumn("ColD2")),
        true, null, null, null, false));

      #endregion

      #region Int-Length()

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Equal,
        new DBxColumn("Id"), new DBxFunction(DBxFunctionKind.Length, new DBxColumn("ColS2"))),
        false, false, true, false, false));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.LessThan,
        new DBxColumn("Id"), new DBxFunction(DBxFunctionKind.Length, new DBxColumn("ColS2"))),
        true, true, false, false, false));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.LessOrEqualThan,
        new DBxColumn("Id"), new DBxFunction(DBxFunctionKind.Length, new DBxColumn("ColS2"))),
        true, true, true, false, false));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.GreaterThan,
        new DBxColumn("Id"), new DBxFunction(DBxFunctionKind.Length, new DBxColumn("ColS2"))),
        false, false, false, true, true));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.GreaterOrEqualThan,
        new DBxColumn("Id"), new DBxFunction(DBxFunctionKind.Length, new DBxColumn("ColS2"))),
        false, false, true, true, true));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.NotEqual,
        new DBxColumn("Id"), new DBxFunction(DBxFunctionKind.Length, new DBxColumn("ColS2"))),
        true, true, false, true, true));

      #endregion

      #endregion

      #region Функции

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Abs, new DBxColumn("Id")), 1, 2, 3, 4, 5));
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Abs, new DBxColumn("ColI")), 10, 20, null, 0, 10));
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Abs, new DBxColumn("ColF")), 5.0, null, null, 0.0, 5.0));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ColS"), new DBxConst("XXX")),
        "AAA", "BcDe", "XXX", "", "abcdefghij"));
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ColI"), new DBxColumn("Id")), 10, 20, 3, 0, -10));
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ColF"), new DBxConst(1.5)), 5.0, 1.5, 1.5, 0.0, -5.0));
      // Для нечисловых типов функция COALESCE() в SQLite возвращает строку, а не типизированное значение
      functions.Add(new FunctionTestInfo(TestCompat.All & ~(TestCompat.SQLite),
        new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ColD"), new DBxConst(new DateTime(2023, 5, 23))),
        new DateTime(2023, 1, 1), new DateTime(2023, 5, 23), new DateTime(2023, 5, 23), new DateTime(2001, 1, 1), new DateTime(2023, 1, 2)));
      functions.Add(new FunctionTestInfo(TestCompat.All & ~(TestCompat.SQLite),
        new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ColG"), new DBxConst(Guid.Empty)),
        new Guid(Guid1), Guid.Empty, Guid.Empty, new Guid(Guid2), new Guid(Guid1)));
      functions.Add(new FunctionTestInfo(TestCompat.All & ~(TestCompat.SQLite),
        new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ColT"), new DBxConst(TimeSpan.Zero)),
        new TimeSpan(12, 34, 56), TimeSpan.Zero, TimeSpan.Zero, new TimeSpan(12, 34, 56), new TimeSpan(12, 34, 57)));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.IIf, new DBxColumn("ColL"), new DBxColumn("ColI"), new DBxColumn("Id")),
        10, 2, 3, 4, -10));
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.IIf,
        new DBxFunction(DBxFunctionKind.GreaterThan, new DBxColumn("Id"), new DBxConst(3)),
        new DBxColumn("ColI"), new DBxColumn("Id")),
        1, 2, 3, 0, -10));

      // Реализация функции с несколькими аргументами имеет свои особенности в BD Access
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ColF"),
        new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("ColI"), new DBxConst(3.0)),
        new DBxFunction(DBxFunctionKind.Multiply, new DBxColumn("Id"), new DBxConst(7.0))),
        5.0, 60.0, 21.0, 0.0, -5.0));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Length, new DBxColumn("ColS")), 3, 4, null, 0, 10));

      // Функции UPPER() и LOWER() не работают в DataColumn.Expression
      // Для SQLite для значения функции возвращают "", а не NULL, как предполагается по стандарту
      functions.Add(new FunctionTestInfo(TestCompat.All & ~(TestCompat.DataView | TestCompat.SQLite),
        new DBxFunction(DBxFunctionKind.Upper, new DBxColumn("ColS")), "AAA", "BCDE", null, "", "ABCDEFGHIJ"));
      functions.Add(new FunctionTestInfo(TestCompat.All & ~(TestCompat.DataView | TestCompat.SQLite),
        new DBxFunction(DBxFunctionKind.Lower, new DBxColumn("ColS")), "aaa", "bcde", null, "", "abcdefghij"));
      functions.Add(new FunctionTestInfo(TestCompat.SQLite,
        new DBxFunction(DBxFunctionKind.Upper, new DBxColumn("ColS")), "AAA", "BCDE", "", "", "ABCDEFGHIJ"));
      functions.Add(new FunctionTestInfo(TestCompat.SQLite,
        new DBxFunction(DBxFunctionKind.Lower, new DBxColumn("ColS")), "aaa", "bcde", "", "", "abcdefghij"));

      // Функция Susbtring для DataView возвращает null, а не "", когда индексы выходят за пределы строки.
      // Во избежание разночтений, следует использовать в сочетании с Coalesce.
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Coalesce, new DBxFunction(DBxFunctionKind.Substring, new DBxColumn("ColS"), new DBxConst(3), new DBxConst(2)), new DBxConst("")),
        "A", "De", "", "", "cd"));

      #endregion

      #region Комбинации функций

      // Функция(Функция())
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Abs, new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ColF"), new DBxConst(1.2))),
        5.0, 1.2, 1.2, 0, 5.0));
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Length, new DBxFunction(DBxFunctionKind.Coalesce, new DBxFunction(DBxFunctionKind.Substring, new DBxColumn("ColS"), new DBxConst(3), new DBxConst(2)), new DBxConst(""))),
        1, 2, 0, 0, 2));

      // Функция(Операция)
      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Coalesce, new DBxFunction(DBxFunctionKind.Substract, new DBxColumn("ColF"), new DBxConst(3.0)), new DBxConst(123.0)),
        2.0, 123.0, 123.0, -3.0, -8.0));

      #endregion

      _AllTestFunctions = functions.ToArray();

      #endregion

      #region Тестовая таблица 2 - запросы SELECT с фильтрами и группировками

      ts = _TestStruct.Tables.Add("Test2");
      ts.Columns.AddId();
      ts.Columns.AddString("ColS", 10, true);
      ts.Columns.AddInt("ColI", true);
      ts.Columns.AddInt("ColI2", false);

      tbl = ts.CreateDataTable();
      _TestData.Tables.Add(tbl);
      //           Id  ColS   ColI  ColI2
      tbl.Rows.Add(1, "AAA", 1, 1);
      tbl.Rows.Add(2, "CCC", 1, 2);
      tbl.Rows.Add(3, "BBB", 1, 3);
      tbl.Rows.Add(4, NULL, NULL, 1);
      tbl.Rows.Add(5, "AAA", 3, 2);
      tbl.Rows.Add(6, "AAA", 3, 3);
      tbl.Rows.Add(7, "BBB", NULL, 1);
      tbl.Rows.Add(8, "CCC", 2, 2);
      tbl.Rows.Add(9, NULL, 2, 3);
      tbl.Rows.Add(10, "AAA", 3, 1);
      tbl.Rows.Add(11, "CCC", 2, 2);
      tbl.Rows.Add(12, "CCC", 1, 3);
      tbl.Rows.Add(13, "AAA", 1, 1);
      tbl.Rows.Add(14, NULL, 1, 2);
      tbl.Rows.Add(15, "BBB", 2, 3);

      #endregion

      #region Тестовые таблицы 3 и 4 - связи между таблицами

      ts = _TestStruct.Tables.Add("Test3");
      ts.Columns.AddId();
      ts.Columns.AddString("Name", 10, false);
      ts.Columns.AddReference("ColR2", "Test2", false);

      tbl = ts.CreateDataTable();
      _TestData.Tables.Add(tbl);

      tbl.Rows.Add(1, "N301", 1);
      tbl.Rows.Add(2, "N302", 5);
      tbl.Rows.Add(3, "N303", 6);
      tbl.Rows.Add(4, "N304", 9);
      tbl.Rows.Add(5, "N305", 15);
      tbl.Rows.Add(6, "N306", 1);
      tbl.Rows.Add(7, "N307", 5);
      tbl.Rows.Add(8, "N308", 5);

      ts = _TestStruct.Tables.Add("Test4");
      ts.Columns.AddId();
      ts.Columns.AddString("Name", 10, false);
      ts.Columns.AddReference("ColR3", "Test3", true);

      tbl = ts.CreateDataTable();
      _TestData.Tables.Add(tbl);

      tbl.Rows.Add(1, "N401", 2);
      tbl.Rows.Add(2, "N402", 2);
      tbl.Rows.Add(3, "N403", 5);
      tbl.Rows.Add(4, "N404", NULL);
      tbl.Rows.Add(5, "N405", NULL);

      #endregion
    }

    #endregion

    #region Данные

    /// <summary>
    /// Структуры тестовых таблиц
    /// </summary>
    protected DBxStruct TestStruct { get { return _TestStruct; } }
    private DBxStruct _TestStruct;

    /// <summary>
    /// Набор таблиц тестовых данных
    /// </summary>
    protected DataSet TestData { get { return _TestData; } }
    private DataSet _TestData;

    #endregion

    #region Перечисление TestCompat

    /// <summary>
    /// Флаги совместимости с разными базами данных
    /// </summary>
    [Flags]
    public enum TestCompat
    {
      /// <summary>
      /// Тест совместим с DataView
      /// </summary>
      DataView = 0x1,

      /// <summary>
      /// Тест совместим с БД SQLite
      /// </summary>
      SQLite = 0x2,

      /// <summary>
      /// Тест совместим с MS SQL Server 2008R2 или новее
      /// </summary>
      MSSQL = 0x4,

      /// <summary>
      /// Тест совместим с PostgreSQL версии 8 и новее
      /// </summary>
      PostgreSQL = 0x8,

      /// <summary>
      /// Тест совместим с Access-2000
      /// </summary>
      OleDB = 0x10,

      /// <summary>
      /// Тест используется везде
      /// </summary>
      All = DataView | SQLite | MSSQL | PostgreSQL | OleDB,

      /// <summary>
      /// Тест используется везде, кроме DataView
      /// </summary>
      AllButDataView = All & (~DataView),

      /// <summary>
      /// Тест использует поля TimeSpan
      /// </summary>
      TimeSpan = 0x100,
    }

    #endregion

    #region Список тестов фильтров

    public struct FilterTestInfo
    {
      #region Конструкторы

      public FilterTestInfo(TestCompat compat, DBxFilter filter, params Int32[] wantedIds)
      {
        if (compat == 0)
          throw new ArgumentException("compat=0", "compat");
        if (filter == null)
          throw new ArgumentNullException("filter");
        _Compat = compat;
        _Filter = filter;
        _WantedIds = wantedIds;
      }

      public FilterTestInfo(DBxFilter filter, params Int32[] wantedIds)
        : this(TestCompat.All, filter, wantedIds)
      {
      }

      #endregion

      #region Основные свойства

      /// <summary>
      /// Тестируемый фильтр
      /// </summary>
      public DBxFilter Filter { get { return _Filter; } }
      private DBxFilter _Filter;

      /// <summary>
      /// Ожидаемые идентификаторы строк, которые проходят фильтр
      /// </summary>
      public Int32[] WantedIds { get { return _WantedIds; } }
      private Int32[] _WantedIds;

      /// <summary>
      /// Флаги совместимости
      /// </summary>
      public TestCompat Compat { get { return _Compat; } }
      private TestCompat _Compat;

      public override string ToString()
      {
        string sFilter;
        try
        {
          sFilter = _Filter.ToString();
        }
        catch (Exception e)
        {
          sFilter = "*** Ошибка *** " + e.Message;
        }
        return _Filter.GetType().Name + " {" + sFilter + "}, Ids=" + StdConvert.ToString(_WantedIds);
      }

      #endregion
    }

    /// <summary>
    /// Все варианты тестов фильтров.
    /// В классах тестов реализованы свои подмассивы
    /// </summary>
    private FilterTestInfo[] _AllTestFilters;

    protected FilterTestInfo[] GetTestFilters(TestCompat compat)
    {
      List<FilterTestInfo> lst = new List<FilterTestInfo>();
      foreach (FilterTestInfo ti in _AllTestFilters)
      {
        if (IsCompat(ti.Compat, compat))
          lst.Add(ti);
      }
      return lst.ToArray();
    }

    /// <summary>
    /// Проверка совместимости теста
    /// </summary>
    /// <param name="testCompat">Задано в тесте</param>
    /// <param name="realCompat">Совместимость для текущей базы данных</param>
    /// <returns>Тест доступен</returns>
    protected static bool IsCompat(TestCompat testCompat, TestCompat realCompat)
    {
      if ((testCompat & TestCompat.All & realCompat) == 0)
        return false;
      if ((testCompat & TestCompat.TimeSpan) != 0)
      {
        if ((realCompat & TestCompat.TimeSpan) == 0)
          return false;
      }
      return true;
    }

    #endregion

    #region Список тестов функций

    public struct FunctionTestInfo
    {
      #region Конструкторы

      public FunctionTestInfo(TestCompat compat, DBxFunction function, params object[] wantedResults)
      {
        //if (compat == 0)
        //  throw new ArgumentException("compat=0", "compat");
        if (function == null)
          throw new ArgumentNullException("function");
        _Compat = compat;
        _Function = function;
        _WantedResults = wantedResults;
      }

      public FunctionTestInfo(DBxFunction function, params object[] wantedResults)
        : this(TestCompat.All, function, wantedResults)
      {
      }

      #endregion

      #region Основные свойства

      /// <summary>
      /// Тестируемая функция
      /// </summary>
      public DBxFunction Function { get { return _Function; } }
      private DBxFunction _Function;

      /// <summary>
      /// Ожидаемые идентификаторы строк, которые проходят фильтр
      /// </summary>
      public object[] WantedResults { get { return _WantedResults; } }
      private object[] _WantedResults;

      /// <summary>
      /// Флаги совместимости
      /// </summary>
      public TestCompat Compat { get { return _Compat; } }
      private TestCompat _Compat;

      public override string ToString()
      {
        string sFunction;
        try
        {
          sFunction = _Function.ToString();
        }
        catch (Exception e)
        {
          sFunction = "*** Ошибка *** " + e.Message;
        }
        return _Function.Function.ToString() + " {" + sFunction + "}";
      }

      #endregion
    }

    /// <summary>
    /// Все варианты тестов функций.
    /// В классах тестов реализованы свои подмассивы
    /// </summary>
    private FunctionTestInfo[] _AllTestFunctions;

    protected FunctionTestInfo[] GetTestFunctions(TestCompat compat)
    {
      List<FunctionTestInfo> lst = new List<FunctionTestInfo>();
      foreach (FunctionTestInfo ti in _AllTestFunctions)
      {
        if (IsCompat(ti.Compat, compat))
          lst.Add(ti);
      }
      return lst.ToArray();
    }

    #endregion
  }

  [TestFixture]
  public class SqlTestDataView : SqlTestBase
  {
    public FilterTestInfo[] TestFilters { get { return GetTestFilters(TestCompat.DataView | TestCompat.TimeSpan); } }

    [TestCaseSource("TestFilters")]
    public void TestFilter_DataTable_Select(FilterTestInfo info)
    {
      DBxSqlBuffer buffer = new DBxSqlBuffer();
      buffer.FormatFilter(info.Filter);
      DataRow[] rows = TestData.Tables["Test1"].Select(buffer.SB.ToString());
      Int32[] res = DataTools.GetIds(rows);
      CollectionAssert.AreEquivalent(info.WantedIds, res);
    }

    [TestCaseSource("TestFilters")]
    public void TestFilter_DataView_RowFilter(FilterTestInfo info)
    {
      using (DataView dv = new DataView(TestData.Tables["Test1"]))
      {
        dv.RowFilter = info.Filter.ToString();
        Int32[] res1 = DataTools.GetIds(dv);
        CollectionAssert.AreEquivalent(info.WantedIds, res1, "ToString()");

        dv.RowFilter = "";
        info.Filter.AddToDataViewRowFilter(dv);
        Int32[] res2 = DataTools.GetIds(dv);
        CollectionAssert.AreEquivalent(info.WantedIds, res2, "AddToDataViewRowFilter(), empty");

        dv.RowFilter = "[Id]>2";
        info.Filter.AddToDataViewRowFilter(dv);
        Int32[] res3 = DataTools.GetIds(dv);
        IdList wantedIds3 = new IdList(info.WantedIds);
        wantedIds3.Remove(1);
        wantedIds3.Remove(2);
        CollectionAssert.AreEquivalent(wantedIds3, res3, "AddToDataViewRowFilter(), AND");
      }
    }

    public FunctionTestInfo[] TestFunctions { get { return GetTestFunctions(TestCompat.DataView | TestCompat.TimeSpan); } }

    [TestCaseSource("TestFunctions")]
    public void TestFunction(FunctionTestInfo info)
    {
      DBxSqlBuffer buffer = new DBxSqlBuffer();
      buffer.FormatExpression(info.Function, new DBxFormatExpressionInfo());

      Type resType = null;
      for (int i = 0; i < info.WantedResults.Length; i++)
      {
        if (info.WantedResults[i] != null)
        {
          resType = info.WantedResults[i].GetType();
          break;
        }
      }
      if (resType == null)
        throw new BugException("Wanted result type is unknown");

      Assert.AreEqual(info.WantedResults.Length, TestData.Tables["Test1"].Rows.Count, "ResultCount");

      DataTable table1 = TestData.Tables["Test1"];
      DataTable table2 = table1.Clone();
      table2.Columns.Add("Res", resType, buffer.SB.ToString());
      for (int i = 0; i < table1.Rows.Count; i++)
      {
        DataRow row = table2.Rows.Add(table1.Rows[i].ItemArray);
        object res = row["Res"];
        if (res is DBNull)
          res = null;

        Assert.AreEqual(info.WantedResults[i], res, "Строка Id=" + row["Id"].ToString());
      }
    }
  }

  public abstract class SqlTestBaseDB : SqlTestBase
  {
    #region Абстрактные методы

    protected abstract TestCompat Compat { get; }

    protected abstract DBxConBase Con { get; }

    /// <summary>
    /// Получить имя тестовой таблицы "Test1", "Test2", ...
    /// Переопределяется для временных таблиц MS SQL Server.
    /// </summary>
    /// <param name="tableName">"Test1", "Test2", ...</param>
    /// <returns>Скорректированное имя таблицы</returns>
    protected virtual string GetTestTableName(string tableName)
    {
      return tableName;
    }

    #endregion

    #region Тестирование

    #region Отладка запроса

    private bool _ConPrepared;

    private void PrepareCon()
    {
      if (!_ConPrepared)
      {
        _ConPrepared=true;
        Con.DB.SqlQueryStarted += new DBxSqlQueryStartedEventHandler(DB_SqlQueryStarted);
        Con.DB.SqlQueryFinished += new DBxSqlQueryFinishedEventHandler(DB_SqlQueryFinished);
      }
    }

    void DB_SqlQueryStarted(object sender, DBxSqlQueryStartedEventArgs args)
    {
      //Console.WriteLine("SQL: " + args.CmdText);
    }

    void DB_SqlQueryFinished(object sender, DBxSqlQueryFinishedEventArgs args)
    {
      DBx db = (DBx)sender;
      if (args.Exception != null)
      {
        Console.WriteLine("Bad SQL: " + args.CmdText);
        Console.WriteLine("Server version: " + db.ServerVersionText);
      }
    }

    #endregion

    #region Фильтры

    public FilterTestInfo[] TestFilters { get { return GetTestFilters(Compat); } }

    [TestCaseSource("TestFilters")]
    public void TestFilter(FilterTestInfo info)
    {
      PrepareCon();
      IdList res = Con.GetIds(GetTestTableName("Test1"), info.Filter);
      CollectionAssert.AreEquivalent(info.WantedIds, res);
    }

    #endregion

    #region Функции

    public FunctionTestInfo[] TestFunctions { get { return GetTestFunctions(Compat); } }

    [TestCaseSource("TestFunctions")]
    public void TestFunction(FunctionTestInfo info)
    {
      PrepareCon();
      DBxSelectInfo si = new DBxSelectInfo();
      si.TableName = GetTestTableName("Test1");
      si.Expressions.Add("Id");
      si.Expressions.Add(info.Function, "Res");
      si.OrderBy = new DBxOrder("Id");
      DataTable resTable = Con.FillSelect(si);
      Assert.AreEqual(info.WantedResults.Length, resTable.Rows.Count, "ResultCount");
      for (int i = 0; i < info.WantedResults.Length; i++)
      {
        object wanted = info.WantedResults[i];
        object res = resTable.Rows[i]["Res"];
        if (res is DBNull)
          res = null;

        // Некоторые провайдеры возвращают неправильные типы данных
        if (res != null)
        {
          if (wanted is Guid)
          {
            if (res is String)
            {
              if (((String)res).Length > 0)
                res = new Guid((String)res);
            }
          }
          if (wanted is Boolean)
          {
            if (DataTools.IsIntegerType(res.GetType()))
              res = DataTools.GetBool(res);
          }
          if (wanted is TimeSpan)
          {
            if (res is DateTime)
              res = DataTools.GetTimeSpan(res);
          }
        }

        Assert.AreEqual(wanted, res, "Строка Id=" + resTable.Rows[i]["Id"].ToString());
      }
    }

    #endregion

    #region SELECT

    [Test]
    public void Select_Where()
    {
      PrepareCon();
      DBxSelectInfo si = new DBxSelectInfo();
      si.TableName = GetTestTableName("Test2");
      si.Expressions.Add("Id");
      si.Where = new ValueFilter("ColI", 2);
      DataTable resTable = Con.FillSelect(si);
      Int32[] res = DataTools.GetIdsFromColumn(resTable, "Id");
      CollectionAssert.AreEquivalent(new Int32[] { 8, 9, 11, 15 }, res);
    }

    [Test]
    public void Select_Where_OrderBy()
    {
      PrepareCon();
      DBxSelectInfo si = new DBxSelectInfo();
      si.TableName = GetTestTableName("Test2");
      si.Expressions.Add("Id");
      si.Where = new ValueFilter("ColI", 2);
      si.OrderBy = DBxOrder.FromDataViewSort("Id DESC");
      DataTable resTable = Con.FillSelect(si);

      Int32[] res = DataTools.GetIdsFromColumn(resTable, "Id");
      CollectionAssert.AreEqual(new Int32[] { 15, 11, 9, 8 }, res);
    }

    [Test]
    public void Select_Where_OrderBy2()
    {
      PrepareCon();
      DBxSelectInfo si = new DBxSelectInfo();
      si.TableName = GetTestTableName("Test2");
      si.Expressions.Add("Id");
      si.Where = new ValueFilter("ColI", 2);
      si.OrderBy = DBxOrder.FromDataViewSort("ColS,Id");
      DataTable resTable = Con.FillSelect(si);

      Int32[] res = DataTools.GetIdsFromColumn(resTable, "Id");
      CollectionAssert.AreEqual(new Int32[] { 9, 15, 8, 11 }, res);
    }

    [Test]
    public void Select_GroupBy_OrderBy()
    {
      PrepareCon();
      DBxSelectInfo si = new DBxSelectInfo();
      si.TableName = GetTestTableName("Test2");
      si.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, "ColI"), "S1");
      si.Expressions.Add("ColS");
      si.GroupBy.Add(new DBxColumn("ColS"));
      si.OrderBy = new DBxOrder("ColS");
      DataTable resTable = Con.FillSelect(si);

      string[] res2 = DataTools.GetValuesFromColumn<string>(resTable, "ColS");
      Int32[] res1 = DataTools.GetValuesFromColumn<int>(resTable, "S1");
      CollectionAssert.AreEqual(new string[] { null, "AAA", "BBB", "CCC" }, res2, "ColS");
      CollectionAssert.AreEqual(new int[] { 
        0+2+1, // NULL
        1+3+3+3+1, // "AAA"
        1+0+2, // "BBB"
        1+2+2+1  // "CCC"
        }, res1, "S1");
    }

    [Test]
    public void Select_Where_GroupBy_OrderBy()
    {
      PrepareCon();
      DBxSelectInfo si = new DBxSelectInfo();
      si.TableName = GetTestTableName("Test2");
      si.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, "ColI"), "S1");
      si.Expressions.Add("ColS");
      si.Where = new ValueFilter("Id", 10, CompareKind.LessOrEqualThan);
      si.GroupBy.Add(new DBxColumn("ColS"));
      si.OrderBy = new DBxOrder("ColS");
      DataTable resTable = Con.FillSelect(si);

      string[] res2 = DataTools.GetValuesFromColumn<string>(resTable, "ColS");
      Int32[] res1 = DataTools.GetValuesFromColumn<int>(resTable, "S1");
      CollectionAssert.AreEqual(new string[] { null, "AAA", "BBB", "CCC" }, res2, "ColS");
      CollectionAssert.AreEqual(new int[] {
        0+2, // NULL
        1+3+3+3, // "AAA"
        1+0, // "BBB"
        1+2  // "CCC"
        }, res1, "S1");
    }

    [Test]
    public void Select_GroupBy_Having_OrderBy()
    {
      PrepareCon();
      DBxSelectInfo si = new DBxSelectInfo();
      si.TableName = GetTestTableName("Test2");
      si.Expressions.Add("ColS");
      si.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, "ColI"), "S1");
      si.Where = new ValueFilter("Id", 10, CompareKind.LessOrEqualThan);
      si.GroupBy.Add(new DBxColumn("ColS"));
      si.Having = new ValueFilter(new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, "ColI"), 5, CompareKind.LessThan);
      si.OrderBy = DBxOrder.FromDataViewSort("ColS");
      DataTable resTable = Con.FillSelect(si);

      string[] res2 = DataTools.GetValuesFromColumn<string>(resTable, "ColS");
      Int32[] res1 = DataTools.GetValuesFromColumn<int>(resTable, "S1");
      CollectionAssert.AreEqual(new string[] { null, "BBB", "CCC" }, res2, "ColS");
      CollectionAssert.AreEqual(new int[] { 0+2, // NULL
        1+0, // "BBB"
        1+2  // "CCC"
        }, res1, "S1");
    }

    [Test]
    public void Select_OrderBy_MaxRecordCount()
    {
      PrepareCon();
      DBxSelectInfo si = new DBxSelectInfo();
      si.TableName = GetTestTableName("Test2");
      si.Expressions.Add("Id");
      si.OrderBy = DBxOrder.FromDataViewSort("Id DESC");
      si.MaxRecordCount = 3;
      DataTable resTable = Con.FillSelect(si);

      Int32[] res = DataTools.GetIdsFromColumn(resTable, "Id");
      CollectionAssert.AreEqual(new Int32[] { 15, 14, 13 }, res);
    }

    [Test]
    public void Select_AggregateFunctions_Where()
    {
      PrepareCon();
      DBxSelectInfo si = new DBxSelectInfo();
      si.TableName = GetTestTableName("Test2");
      si.Expressions.Add(DBxAggregateFunction.Count, "A1");
      si.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, "ColI"), "A2");
      si.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Min, "ColI"), "A3");
      si.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Max, "ColI"), "A4");
      si.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Avg, "ColI"), "A5");
      si.Where = new ValueFilter("Id", 10, CompareKind.LessOrEqualThan);
      DataTable resTable = Con.FillSelect(si);

      Assert.AreEqual(1, resTable.Rows.Count, "RowCount");
      object[] res = resTable.Rows[0].ItemArray;
      Assert.AreEqual(10, res[0], "Count");
      Assert.AreEqual(1 + 1 + 1 + 3 + 3 + 2 + 2 + 3, res[1], "Sum");
      Assert.AreEqual(1, res[2], "Min");
      Assert.AreEqual(3, res[3], "Max");
      Assert.AreEqual((double)(1 + 1 + 1 + 3 + 3 + 2 + 2 + 3) / 8.0, DataTools.GetDouble(res[4]), "Avg");
    }

    [Test]
    public void Select_AggregateFunctions_GroupBy_OrderBy()
    {
      PrepareCon();
      DBxSelectInfo si = new DBxSelectInfo();
      si.TableName = GetTestTableName("Test2");
      si.Expressions.Add("ColS");
      si.Expressions.Add(DBxAggregateFunction.Count, "A1");
      si.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, "ColI"), "A2");
      si.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Min, "ColI"), "A3");
      si.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Max, "ColI"), "A4");
      si.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Avg, "ColI"), "A5");
      si.GroupBy.Add(new DBxColumn("ColS"));
      si.OrderBy = new DBxOrder("ColS");
      DataTable resTable = Con.FillSelect(si);
      string[] colS = DataTools.GetValuesFromColumn<string>(resTable, "ColS");
      Assert.AreEqual(new string[] { null, "AAA", "BBB", "CCC" }, colS, "ColS");
      Assert.AreEqual(new int[] { 
        2 + 1, 
        1 + 3 + 3 + 3 + 1, 
        1+2,
        1+2+2+1
      }, DataTools.GetValuesFromColumn<int>(resTable, "A2"), "Sum");
      Assert.AreEqual(new int[] { 1, 1, 1, 1 }, DataTools.GetValuesFromColumn<int>(resTable, "A3"), "Min");
      Assert.AreEqual(new int[] { 2, 3, 2, 2 }, DataTools.GetValuesFromColumn<int>(resTable, "A4"), "Max");

      // Для SQL Server почему-то среднее значение округляется до целого
      //if (Compat != TestCompat.MSSQL)
      //{
      Assert.AreEqual(new double[] {
        (double)(2+1)/2.0,
        (double)(1+3+3+3+1)/5.0,
        (double)(1+2)/2.0,
        (double)(1+2+2+1)/4.0
        //}, DataTools.GetValuesFromColumn<double>(resTable, "A5"), "Avg");
        }, GetDoubleValuesFromColumn(resTable, "A5"), "Avg");
      //}
    }

    private static double[] GetDoubleValuesFromColumn(DataTable table, string colName)
    {
      double[] a = new double[table.Rows.Count];
      for (int i = 0; i < a.Length; i++)
        a[i] = DataTools.GetDouble(table.Rows[i], colName);
      return a;
    }

    #endregion

    #region SELECT-2

    public struct SelectTestInfo
    {
      #region Конструкторы

      public SelectTestInfo(TestCompat compat, string displayName)
      {
        if (compat == 0)
          throw new ArgumentException("compat=0", "compat");
        _Compat = compat;
        _DisplayName = displayName;

        _Sel = new DBxSelectInfo();
        _WantedColResults = new List<object>();
      }

      public SelectTestInfo(string tableName)
        : this(TestCompat.All, tableName)
      {
      }

      #endregion

      #region Основные свойства

      /// <summary>
      /// Тестируемый запрос
      /// </summary>
      public DBxSelectInfo Sel { get { return _Sel; } }
      private DBxSelectInfo _Sel;

      /// <summary>
      /// Флаги совместимости
      /// </summary>
      public TestCompat Compat { get { return _Compat; } }
      private TestCompat _Compat;

      /// <summary>
      /// Результаты, которые требуется получить.
      /// Индексы списка соответствуют столбцам в результирующей таблице.
      /// Элементами списка являются массивы произвольного типа. Количество элементов в массиве должно быть равно ожидаемому количеству строк в выборке
      /// </summary>
      public List<object> WantedColResults { get { return _WantedColResults; } }
      private List<object> _WantedColResults;

      public string DisplayName { get { return _DisplayName; } }
      private string _DisplayName;

      public override string ToString()
      {
        return _DisplayName;
      }

      #endregion
    }

    public SelectTestInfo[] AllTestSelect { get { return _AllTestSelect; } }
    private SelectTestInfo[] _AllTestSelect = CreateAllTestSelect();

    private static SelectTestInfo[] CreateAllTestSelect()
    {
      List<SelectTestInfo> lst = new List<SelectTestInfo>();

      SelectTestInfo obj;

      obj = new SelectTestInfo("Simple Ref");
      obj.Sel.TableName = "Test3";
      obj.Sel.Expressions.Add("Id");
      obj.Sel.Expressions.Add("ColR2.ColS");
      obj.Sel.OrderBy = DBxOrder.FromDataViewSort("Id");
      obj.WantedColResults.Add(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 });
      obj.WantedColResults.Add(new string[] { "AAA", "AAA", "AAA", null, "BBB", "AAA", "AAA", "AAA" });
      lst.Add(obj);

      obj = new SelectTestInfo("2-Level Ref");
      obj.Sel.TableName = "Test4";
      obj.Sel.Expressions.Add("Id");
      obj.Sel.Expressions.Add("ColR3");
      obj.Sel.Expressions.Add("ColR3.Name");
      obj.Sel.Expressions.Add("ColR3.ColR2");
      obj.Sel.Expressions.Add("ColR3.ColR2.ColS");
      obj.Sel.OrderBy = DBxOrder.FromDataViewSort("Id");
      obj.WantedColResults.Add(new int[] { 1, 2, 3, 4, 5 }); // Id
      obj.WantedColResults.Add(new int?[] { 2, 2, 5, null, null }); // ColR3
      obj.WantedColResults.Add(new string[] { "N302", "N302", "N305", null, null }); // ColR3.Name
      obj.WantedColResults.Add(new int?[] { 5, 5, 15, null, null }); // ColR3.ColR2
      obj.WantedColResults.Add(new string[] { "AAA", "AAA", "BBB", null, null }); // ColR3.ColR2.ColS
      lst.Add(obj);


      obj = new SelectTestInfo("Ref with complex OrderBy and Where");
      obj.Sel.TableName = "Test3";
      obj.Sel.Expressions.Add("Id");
      obj.Sel.Expressions.Add("ColR2.ColS");
      obj.Sel.Where = new ValueFilter("ColR2.ColI2", 2, CompareKind.NotEqual);
      obj.Sel.OrderBy = DBxOrder.FromDataViewSort("ColR2.ColI DESC,Id");
      obj.WantedColResults.Add(new int[] { 3, 4, 5, 1, 6 }); // Id
      obj.WantedColResults.Add(new string[] { "AAA", null, "BBB", "AAA", "AAA" });
      lst.Add(obj);

      obj = new SelectTestInfo("Ref with GroupBy");
      obj.Sel.TableName = "Test3";
      obj.Sel.Expressions.Add("ColR2.ColS");
      obj.Sel.Expressions.Add(DBxAggregateFunction.Count, "A1");
      obj.Sel.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, "ColR2.ColI"), "A2");
      obj.Sel.GroupBy.Add(new DBxColumn("ColR2.ColS"));
      obj.Sel.OrderBy = DBxOrder.FromDataViewSort("ColR2.ColS");
      obj.WantedColResults.Add(new string[] { null, "AAA", "BBB" }); // ColR2.ColS
      obj.WantedColResults.Add(new int[] { 1, 6, 1 }); // A1
      obj.WantedColResults.Add(new int[] { 2, 1 + 3 + 3 + 1 + 3 + 3, 2 }); // A2
      lst.Add(obj);

      return lst.ToArray();
    }

    [TestCaseSource("AllTestSelect")]
    public void TestSelect(SelectTestInfo info)
    {
      PrepareCon();
      int wantedRowCount = -1;
      foreach (object obj in info.WantedColResults)
      {
        Array a = obj as Array;
        if (a == null)
          continue;
        if (wantedRowCount < 0)
          wantedRowCount = a.Length;
        else if (a.Length != wantedRowCount)
          throw new BugException("Invalid WantedColResults item length");
      }
      if (wantedRowCount < 0)
        throw new BugException("No WantedColResults");

      DBxSelectInfo sel2 = info.Sel.Clone();
      sel2.TableName = GetTestTableName(sel2.TableName);

      DataTable resTable = Con.FillSelect(sel2);
      Assert.AreEqual(wantedRowCount, resTable.Rows.Count, "RowCount");
      for (int i = 0; i < info.WantedColResults.Count; i++)
      {
        Array a = info.WantedColResults[i] as Array;
        if (a == null)
          continue;

        for (int j = 0; j < wantedRowCount; j++)
        {
          object wanted = a.GetValue(j);
          object res = resTable.Rows[j][i];
          if (res is DBNull)
            res = null;
          if (res is String)
            res = ((String)res).TrimEnd();
          Assert.AreEqual(wanted, res, "RowIndex=" + j.ToString() + ", ColumnIndex=" + i.ToString() + " (" + resTable.Columns[i].ColumnName + ")");
        }
      }
    }

    #endregion

    #endregion
  }
}

namespace ExtDB_tests.Data_SQLite
{
  [TestFixture]
  public class SqlTest_SQLite : ExtDB_tests.Data.SqlTestBaseDB
  {
    #region База данных в памяти

    protected override void OnOneTimeSetUp()
    {
      base.OnOneTimeSetUp();
      _DB = new SQLiteDBx();
      DBxStruct dbs = new DBxStruct();
      _DB.Struct = TestStruct;
      _DB.UpdateStruct();

      _Con = _DB.MainEntry.CreateCon();
      _Con.AddRecords(TestData);
    }

    protected override void OnOneTimeTearDown()
    {
      if (_Con != null)
        _Con.Dispose();
      if (_DB != null)
        _DB.Dispose();
      base.OnOneTimeTearDown();
    }

    private SQLiteDBx _DB;

    private DBxConBase _Con;

    #endregion

    #region Переопределенные методы

    protected override TestCompat Compat { get { return TestCompat.SQLite |TestCompat.TimeSpan; } }

    protected override DBxConBase Con { get { return _Con; } }

    #endregion
  }
}

namespace ExtDB_tests.Data_SqlClient
{
  //[Category("MSSQL")]
  public class SqlTest_MSSQL : ExtDB_tests.Data.SqlTestBaseDB
  {
    #region База данных в памяти

    protected override void OnOneTimeSetUp()
    {
      base.OnOneTimeSetUp();

      _DB = CreateDB();

      _Con = (SqlDBxCon)(_DB.MainEntry.CreateCon());
      _Con.NameCheckingEnabled = false; // имя таблицы начинается с "#"

      _TestTableNames = new Dictionary<string, string>();

      foreach (DBxTableStruct ts in TestStruct.Tables)
      {
        DBxTableStruct ts2 = ts.Clone();
        foreach (DBxColumnStruct col in ts2.Columns)
        {
          if (!String.IsNullOrEmpty(col.MasterTableName))
            col.MasterTableName = GetTestTableName(col.MasterTableName);
        }
        _TestTableNames.Add(ts.TableName, _Con.CreateTempTableInternal(ts2));
        DataTable tbl = TestData.Tables[ts.TableName];
        _Con.AddRecords(_TestTableNames[ts.TableName], tbl);
      }
    }

    private SqlDBx CreateDB()
    {
      System.Data.SqlClient.SqlConnectionStringBuilder csb = new System.Data.SqlClient.SqlConnectionStringBuilder();
      csb.DataSource = @".\SQLEXPRESS";
      csb.IntegratedSecurity = true;
      csb.InitialCatalog = "master";
      return new SqlDBx(csb);
    }

    protected override void OnOneTimeTearDown()
    {
      if (_Con != null)
        _Con.Dispose();
      if (_DB != null)
        _DB.Dispose();
      base.OnOneTimeTearDown();
    }

    private SqlDBx _DB;

    private SqlDBxCon _Con;

    #endregion

    #region Переопределенные методы

    protected override TestCompat Compat 
    { 
      get 
      {
        if (_Compat == 0)
        {
          using (SqlDBx db = CreateDB())
          {
            _Compat = TestCompat.MSSQL;
            if (db.IsSqlServer2008orNewer)
              _Compat |= TestCompat.TimeSpan;
          }
        }
        return _Compat; 
      }
    }
    private TestCompat _Compat;

    protected override DBxConBase Con { get { return _Con; } }

    protected override string GetTestTableName(string tableName)
    {
      return _TestTableNames[tableName];
    }
    private Dictionary<string, string> _TestTableNames;

    #endregion
  }
}

namespace ExtDB_tests.Data_Npgsql
{
  public class SqlTest_Npgsql : ExtDB_tests.Data.SqlTestBaseDB
  {
    #region База данных в памяти

    protected override void OnOneTimeSetUp()
    {
      base.OnOneTimeSetUp();

      NpgsqlConnectionStringBuilder csb = new NpgsqlConnectionStringBuilder();
      csb.Host = "127.0.0.1";
      csb.Database = "test";
      csb.UserName = "postgres";
      csb.Password = "123";
      _DB = new NpgsqlDBx(csb);
      //_DB.DropDatabaseIfExists();
      _DB.CreateIfRequired();
      _DB.Struct = TestStruct;
      _DB.UpdateStruct();

      _Con = (NpgsqlDBxCon)(_DB.MainEntry.CreateCon());

      //foreach(DBxTableStruct ts in TestStruct.Tables)
      //  _Con.DeleteAll(ts.TableName);
      // Чистить надо в обратном порядке
      for (int i = TestStruct.Tables.Count - 1; i >= 0; i--)
      {
        //_Con.DeleteAll(TestStruct.Tables[i].TableName);
        _Con.Delete(TestStruct.Tables[i].TableName, DummyFilter.AlwaysTrue);
      }

      _Con.AddRecords(TestData);
    }

    protected override void OnOneTimeTearDown()
    {
      if (_Con != null)
        _Con.Dispose();
      if (_DB != null)
      {
        //_DB.DropDatabaseIfExists();
        _DB.Dispose();
      }

      base.OnOneTimeTearDown();
    }


    private NpgsqlDBx _DB;

    private NpgsqlDBxCon _Con;

    #endregion

    #region Переопределенные методы

    protected override TestCompat Compat { get { return TestCompat.PostgreSQL | TestCompat.TimeSpan; } }

    protected override DBxConBase Con { get { return _Con; } }

    #endregion
  }
}

#if !MONO

namespace ExtDB_tests.Data_OleDB
{
  //[Platform("Windows,X86")]
  public class SqlTest_OleDB : ExtDB_tests.Data.SqlTestBaseDB
  {
    // OleDbDBx пока не поддерживает программное создание структуры БД.
    // В случае расширения тестовых таблиц в конструкторе SqlTestBase, не забыть обновить структуру в SqlTestOleDB.mdb.

    #region База данных в памяти

    protected override void OnOneTimeSetUp()
    {
      base.OnOneTimeSetUp();

      _TempDir = new FreeLibSet.IO.TempDirectory();
      FreeLibSet.IO.AbsPath path = new FreeLibSet.IO.AbsPath(_TempDir.Dir, "test.mdb");
      System.IO.File.WriteAllBytes(path.Path, TestTablesResource.SqlTestOleDB);

      _DB = new OleDbDBx(path);
      _DB.Struct = base.TestStruct;
      _Con = (OleDbDBxCon)(_DB.MainEntry.CreateCon());
      _Con.AddRecords(TestData);
    }

    protected override void OnOneTimeTearDown()
    {
      if (_Con != null)
        _Con.Dispose();
      if (_DB != null)
      {
        _DB.Dispose();
      }

      if (_TempDir != null)
        _TempDir.Dispose();
      base.OnOneTimeTearDown();
    }

    private FreeLibSet.IO.TempDirectory _TempDir;

    private OleDbDBx _DB;

    private OleDbDBxCon _Con;

    #endregion

    #region Переопределенные методы

    protected override TestCompat Compat { get { return TestCompat.OleDB | TestCompat.TimeSpan; } }

    protected override DBxConBase Con { get { return _Con; } }

    #endregion
  }
}

#endif
