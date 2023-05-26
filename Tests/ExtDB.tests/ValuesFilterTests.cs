using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Collections;
using FreeLibSet.Remoting;
using FreeLibSet.Core;

// Выполняется тестирование класса фильтров вне связи с табличными данными

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class ValuesFilterTests
  {
    #region Конструкторы с DBxExpression

    [Test]
    public void Constructor_expr_3args()
    {
      DBxExpression expr = new DBxColumn("F1");
      int[] values = new int[] { 10, 20, 30 };
      ValuesFilter sut = new ValuesFilter(expr, values, DBxColumnType.Float);
      Assert.AreSame(expr, sut.Expression, "Expression");
      Assert.AreSame(values, sut.Values, "Values");
      Assert.AreEqual(DBxColumnType.Float, sut.ColumnType, "ColumnType");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_expr_2args()
    {
      DBxExpression expr = new DBxColumn("F1");
      int[] values = new int[] { 10, 20, 0 };
      ValuesFilter sut = new ValuesFilter(expr, values);
      Assert.AreSame(expr, sut.Expression, "Expression");
      Assert.AreSame(values, sut.Values, "Values");
      // Зависит от реализации Assert.AreEqual(DBxColumnType.Int, sut.ColumnType, "ColumnType");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    #endregion

    #region Конструкторы с именем поля

    [Test]
    public void Constructor_colName_3args()
    {
      int[] values = new int[] { 10, 20, 0 };
      ValuesFilter sut = new ValuesFilter("F1", values, DBxColumnType.Money);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression, "Expression type");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression)).ColumnName, "ColumnName");
      Assert.AreSame(values, sut.Values, "Values");
      Assert.AreEqual(DBxColumnType.Money, sut.ColumnType, "ColumnType");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_colName_2args()
    {
      int[] values = new int[] { 20 };
      ValuesFilter sut = new ValuesFilter("F1", values);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression, "Expression type");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression)).ColumnName, "ColumnName");
      Assert.AreSame(values, sut.Values, "Values");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    #endregion

    #region TestFilter()

    [TestCase("10,20", 10, true)]
    [TestCase("10,20", 1, false)]
    [TestCase("10,20", null, false)]
    [TestCase("0,10,20", 10, true)]
    [TestCase("0,10,20", 11, false)]
    [TestCase("0,10,20", 0, true)]
    [TestCase("0,10,20", null, true)]
    [TestCase("10", 10, true)]
    [TestCase("0", 1, false)]
    [TestCase("0", 0, true)]
    [TestCase("0", null, true)]
    public void TestFilter_int(string sListValues, object testValue, bool wantedRes)
    {
      int[] listValues = StdConvert.ToInt32Array(sListValues);
      ValuesFilter sut = new ValuesFilter("F1", listValues);

      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", testValue);

      bool res = sut.TestFilter(vals);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("ABC,DEF", "DEF", true)]
    [TestCase("ABC,DEF", "abc", false)]
    [TestCase("ABC,DEF", "", false)]
    [TestCase("ABC,DEF", null, false)]
    [TestCase("ABC,,DEF", "DEF", true)]
    [TestCase("ABC,,DEF", "", true)]
    [TestCase("ABC,,DEF", null, true)]
    public void TestFilter_string(string sListValues, object testValue, bool wantedRes)
    {
      string[] listValues = sListValues.Split(',');
      ValuesFilter sut = new ValuesFilter("F1", listValues);

      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", testValue);

      bool res = sut.TestFilter(vals);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("2023-05-16,2023-05-18", "2023-05-18", true)]
    [TestCase("2023-05-16,2023-05-18", "2023-05-17", false)]
    [TestCase("2023-05-16,2023-05-18", "", false)]
    public void TestFilter_date(string sListValues, string sTestValue, bool wantedRes)
    {
      string[] aValues2 = sListValues.Split(',');
      DateTime[] listValues = new DateTime[aValues2.Length]; // В списке не может быть значений null
      for (int i = 0; i < aValues2.Length; i++)
        listValues[i] = StdConvert.ToDateTime(aValues2[i], false);


      DateTime? testValue = null;
      if (sTestValue.Length > 0)
        testValue = StdConvert.ToDateTime(sTestValue, false);

      ValuesFilter sut = new ValuesFilter("F1", listValues);

      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", testValue);

      bool res = sut.TestFilter(vals);
      Assert.AreEqual(wantedRes, res);
    }

    const string Guid1 = "83e4ea91-6f0b-4ab6-9d58-c981418c26b7";
    const string Guid2 = "c84cc4da-6f1d-488b-b647-8bfa5642fe23";
    const string Guid3 = "d099f8c4-0ed3-4e72-9d29-173ed46de461";
    const string Guid0 = "00000000-0000-0000-0000-000000000000"; // Guid.Empty

    [TestCase(Guid1 + "," + Guid2, Guid1, true)]
    [TestCase(Guid1 + "," + Guid2, Guid3, false)]
    [TestCase(Guid1 + "," + Guid2, Guid0, false)]
    [TestCase(Guid1 + "," + Guid2, null, false)]
    [TestCase(Guid0 + "," + Guid1, Guid1, true)]
    [TestCase(Guid0 + "," + Guid1, Guid0, true)]
    [TestCase(Guid0 + "," + Guid1, null, true)]
    public void TestFilter_diffTypes_String_Guid(string sListValues, string testValue, bool wantedRes)
    {
      string[] listValues1 = sListValues.Split(',');
      Guid[] listValues2 = new Guid[listValues1.Length];
      for (int i = 0; i < listValues1.Length; i++)
        listValues2[i] = new Guid(listValues1[i]);

      Guid testValue2 = DataTools.GetGuid(testValue);
      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", testValue2);
      vals.Add("F2", testValue); // вывертуто

      ValuesFilter sut1 = new ValuesFilter("F1", listValues1);
      ValuesFilter sut2 = new ValuesFilter("F2", listValues2);

      bool res1 = sut1.TestFilter(vals);
      Assert.AreEqual(wantedRes, res1, "Guid in string array");

      bool res2 = sut2.TestFilter(vals);
      Assert.AreEqual(wantedRes, res2, "string in Guid array");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      ValuesFilter sut = new ValuesFilter("F1", new int[] { 1, 3, 5 }, DBxColumnType.Money);
      byte[] b = SerializationTools.SerializeBinary(sut);
      ValuesFilter res = (ValuesFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
      Assert.AreEqual(sut.Expression, res.Expression, "Expression");
      Assert.AreEqual(sut.Values, res.Values, "Values");
    }

    #endregion
  }
}
