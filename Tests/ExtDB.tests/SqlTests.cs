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

namespace ExtDB_tests.Data
{
  public abstract class SqlTestBase
  {
    protected SqlTestBase()
    {
      #region Тестовая таблица

      const string Guid1 = "83e4ea91-6f0b-4ab6-9d58-c981418c26b7";
      const string Guid2 = "c84cc4da-6f1d-488b-b647-8bfa5642fe23";
      const string Guid0 = "00000000-0000-0000-0000-000000000000"; // Guid.Empty

      _TableStruct = new DBxTableStruct("Test");
      _TableStruct.Columns.AddId();
      _TableStruct.Columns.AddString("ColS", 10, true);
      _TableStruct.Columns.AddInt("ColI", true);
      _TableStruct.Columns.AddDouble("ColF", true);
      _TableStruct.Columns.AddBoolean("ColL", true);
      _TableStruct.Columns.AddDate("ColD", true);
      _TableStruct.Columns.AddGuid("ColG", true);
      _TableStruct.Columns.AddTime("ColT", true);

      DBNull NULL = DBNull.Value; // для большей наглядности таблицы

      _TestTable = _TableStruct.CreateDataTable();
      //                 Id  ColS          ColI  ColF  ColL   ColD                    ColG             ColT
      _TestTable.Rows.Add(1, "AAA"       , 10  , 5.0 , true , new DateTime(2023,1,1), new Guid(Guid1), new TimeSpan(12,34,56));
      _TestTable.Rows.Add(2, "BcDe"      , 20  , NULL, NULL , NULL                  , Guid.Empty     , TimeSpan.Zero);
      _TestTable.Rows.Add(3, NULL        , NULL, NULL, NULL , NULL                  , NULL           , NULL);
      _TestTable.Rows.Add(4, ""          , 0   , 0.0 , false, new DateTime(2001,1,1), new Guid(Guid2), new TimeSpan(12,34,56));
      _TestTable.Rows.Add(5, "abcdefghij", -10 , -5.0, true , new DateTime(2023,1,2), new Guid(Guid1), new TimeSpan(12,34,57));
      _TestTable.AcceptChanges();

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

      filters.Add(new FilterTestInfo(new ValueFilter("ColL", false, CompareKind.Equal, DBxColumnType.Boolean), 2, 3, 4)); // DBNull считается false
      filters.Add(new FilterTestInfo(new ValueFilter("ColL", true, CompareKind.Equal, DBxColumnType.Boolean), 1, 5));
      filters.Add(new FilterTestInfo(new ValueFilter("ColL", false, CompareKind.NotEqual, DBxColumnType.Boolean), 1, 5));
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

      filters.Add(new FilterTestInfo(new ValueFilter("ColT", new TimeSpan(12, 34, 56), CompareKind.Equal, DBxColumnType.Time), 1, 4));
      filters.Add(new FilterTestInfo(new ValueFilter("ColT", TimeSpan.Zero, CompareKind.Equal, DBxColumnType.Time), 2, 3));
      filters.Add(new FilterTestInfo(new ValueFilter("ColT", new TimeSpan(12, 34, 56), CompareKind.NotEqual, DBxColumnType.Time), 2, 3, 5));
      filters.Add(new FilterTestInfo(new ValueFilter("ColT", TimeSpan.Zero, CompareKind.NotEqual, DBxColumnType.Time), 1, 4, 5));

      // сравнение с NULL. Для DataView не работает
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColS", null, CompareKind.Equal, DBxColumnType.String), 3));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColI", null, CompareKind.Equal, DBxColumnType.Int), 3));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColF", null, CompareKind.Equal, DBxColumnType.Float), 2, 3));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColL", null, CompareKind.Equal, DBxColumnType.Boolean), 2, 3));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColD", null, CompareKind.Equal, DBxColumnType.Date), 2, 3));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColG", null, CompareKind.Equal, DBxColumnType.Guid), 3));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColT", null, CompareKind.Equal, DBxColumnType.Time), 3));

      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColS", null, CompareKind.NotEqual, DBxColumnType.String), 1, 2, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColI", null, CompareKind.NotEqual, DBxColumnType.Int), 1, 2, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColF", null, CompareKind.NotEqual, DBxColumnType.Float), 1, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColL", null, CompareKind.NotEqual, DBxColumnType.Boolean), 1, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColD", null, CompareKind.NotEqual, DBxColumnType.Date), 1, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColG", null, CompareKind.NotEqual, DBxColumnType.Guid), 1, 2, 4, 5));
      filters.Add(new FilterTestInfo(TestCompat.AllButDataView, new ValueFilter("ColT", null, CompareKind.NotEqual, DBxColumnType.Time), 1, 2, 4, 5));

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

      filters.Add(new FilterTestInfo(new ValuesFilter("ColT", new TimeSpan[] { new TimeSpan(12, 34, 56), new TimeSpan(12, 34, 57) }), 1, 4, 5));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColT", new TimeSpan[] { new TimeSpan(12, 34, 56), TimeSpan.Zero }), 1, 2, 3, 4));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColT", new TimeSpan[] { new TimeSpan(12, 34, 56) }), 1, 4));
      filters.Add(new FilterTestInfo(new ValuesFilter("ColT", new TimeSpan[] { TimeSpan.Zero }), 2, 3));

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
        new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ColD"), new DBxConst(new DateTime(2023,5,23))), 
        new DateTime(2023,1,1), new DateTime(2023,5,23), new DateTime(2023,5,23), new DateTime(2001,1,1), new DateTime(2023,1,2)));
      functions.Add(new FunctionTestInfo(TestCompat.All & ~(TestCompat.SQLite), 
        new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ColG"), new DBxConst(Guid.Empty)),
        new Guid(Guid1), Guid.Empty, Guid.Empty, new Guid(Guid2), new Guid(Guid1)));
      functions.Add(new FunctionTestInfo(TestCompat.All & ~(TestCompat.SQLite), 
        new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("ColT"), new DBxConst(TimeSpan.Zero)),
        new TimeSpan(12, 34, 56), TimeSpan.Zero, TimeSpan.Zero, new TimeSpan(12, 34, 56), new TimeSpan(12, 34, 57)));

      functions.Add(new FunctionTestInfo(new DBxFunction(DBxFunctionKind.Length, new DBxColumn("ColS")), 3, 4, null, 0, 10));

      // Функции UPPER() и LOWER() не работают в DataColumn.Expression
      // Для SQLite для значения функции возвращают "", а не NULL, как предполагается по стандарту
      functions.Add(new FunctionTestInfo(TestCompat.All & ~(TestCompat.DataView| TestCompat.SQLite), 
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
    }

    protected DBxTableStruct TableStruct { get { return _TableStruct; } }
    private DBxTableStruct _TableStruct;

    protected DataTable TestTable { get { return _TestTable; } }
    private DataTable _TestTable;

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

      All = DataView | SQLite | MSSQL,
      AllButDataView = All & (~DataView)
    }

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
        if ((ti.Compat & compat) != 0)
          lst.Add(ti);
      }
      return lst.ToArray();
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
        if ((ti.Compat & compat) != 0)
          lst.Add(ti);
      }
      return lst.ToArray();
    }

    #endregion
  }

  [TestFixture]
  public class SqlTestDataView : SqlTestBase
  {
    public FilterTestInfo[] TestFilters { get { return GetTestFilters(TestCompat.DataView); } }

    [TestCaseSource("TestFilters")]
    public void TestFilter_DataTable_Select(FilterTestInfo info)
    {
      DBxSqlBuffer buffer = new DBxSqlBuffer();
      buffer.FormatFilter(info.Filter);
      DataRow[] rows = TestTable.Select(buffer.SB.ToString());
      Int32[] res = DataTools.GetIds(rows);
      CollectionAssert.AreEquivalent(info.WantedIds, res);
    }

    [TestCaseSource("TestFilters")]
    public void TestFilter_DataView_RowFilter(FilterTestInfo info)
    {
      using (DataView dv = new DataView(TestTable))
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

    public FunctionTestInfo[] TestFunctions { get { return GetTestFunctions(TestCompat.DataView); } }

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

      Assert.AreEqual(info.WantedResults.Length, TestTable.Rows.Count, "ResultCount");
      DataTable table2 = TestTable.Clone();
      table2.Columns.Add("Res", resType, buffer.SB.ToString());
      for (int i = 0; i < TestTable.Rows.Count; i++)
      {
        DataRow row = table2.Rows.Add(TestTable.Rows[i].ItemArray);
        object res = row["Res"];
        if (res is DBNull)
          res = null;

        Assert.AreEqual(info.WantedResults[i], res, "Строка Id=" + row["Id"].ToString());
      }
    }
  }

  public abstract class SqlTestBaseDB: SqlTestBase
  {
    #region Абстрактные методы

    protected abstract TestCompat Compat { get; }

    protected abstract DBxConBase Con { get; }

    /// <summary>
    /// Имя тестовой таблицы в базе данных. Переопределяется для MS SQL Server
    /// </summary>
    protected virtual string TestTableName { get { return "Test"; } }

    #endregion

    #region Тестирование

    public FilterTestInfo[] TestFilters { get { return GetTestFilters(Compat); } }

    [TestCaseSource("TestFilters")]
    public void TestFilter(FilterTestInfo info)
    {
      IdList res = Con.GetIds(TestTableName, info.Filter);
      CollectionAssert.AreEquivalent(info.WantedIds, res);
    }

    public FunctionTestInfo[] TestFunctions { get { return GetTestFunctions(Compat); } }

    [TestCaseSource("TestFunctions")]
    public void TestFunction(FunctionTestInfo info)
    {
      DBxSelectInfo si = new DBxSelectInfo();
      si.TableName = TestTableName;
      si.Expressions.Add("Id");
      si.Expressions.Add(info.Function, "Res");
      si.OrderBy = new DBxOrder("Id");
      DataTable resTable = Con.FillSelect(si);
      Assert.AreEqual(info.WantedResults.Length, resTable.Rows.Count, "ResultCount");
      for (int i = 0; i < info.WantedResults.Length; i++)
      {
        object res = resTable.Rows[i]["Res"];
        if (res is DBNull)
          res = null;
        Assert.AreEqual(info.WantedResults[i], res, "Строка Id=" + resTable.Rows[i]["Id"].ToString());
      }
    }

    #endregion

  }

  [TestFixture]
  public class SqlTestSQLite : SqlTestBaseDB
  {
    #region База данных в памяти

    [OneTimeSetUp]
    public void SetUp()
    {
      _DB = new SQLiteDBx();
      DBxStruct dbs = new DBxStruct();
      dbs.Tables.Add(TableStruct);
      _DB.Struct = dbs;
      _DB.UpdateStruct();

      _Con = _DB.MainEntry.CreateCon();
      _Con.AddRecords(TestTable);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
      if (_Con != null)
        _Con.Dispose();
      if (_DB != null)
        _DB.Dispose();
    }

    private SQLiteDBx _DB;

    private DBxConBase _Con;

    #endregion

    #region Переопределенные методы

    protected override TestCompat Compat { get { return TestCompat.SQLite; } }

    protected override DBxConBase Con { get { return _Con; } }

    #endregion
  }


  [Category("MSSQL")]
  public class SqlTestMSSQL : SqlTestBaseDB
  {
    #region База данных в памяти

    [OneTimeSetUp]
    public void SetUp()
    {
      System.Data.SqlClient.SqlConnectionStringBuilder csb = new System.Data.SqlClient.SqlConnectionStringBuilder();
      csb.DataSource = @".\SQLEXPRESS";
      csb.IntegratedSecurity = true;
      csb.InitialCatalog = "master";
      _DB = new SqlDBx(csb);

      _Con = (SqlDBxCon)(_DB.MainEntry.CreateCon());
      _Con.NameCheckingEnabled = false; // имя таблицы начинается с "#"

      _TestTableName = _Con.CreateTempTableInternal(TableStruct);
      _Con.AddRecords(_TestTableName, TestTable);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
      if (_Con != null)
        _Con.Dispose();
      if (_DB != null)
        _DB.Dispose();
    }

    private SqlDBx _DB;

    private SqlDBxCon _Con;

    #endregion

    #region Переопределенные методы

    protected override TestCompat Compat { get { return TestCompat.MSSQL; } }

    protected override DBxConBase Con { get { return _Con; } }

    protected override string TestTableName { get { return _TestTableName; } }
    private string _TestTableName;

    #endregion
  }
}
