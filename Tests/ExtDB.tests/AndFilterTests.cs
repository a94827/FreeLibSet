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
  public class AndFilterTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Array()
    {
      DBxFilter[] filters = new DBxFilter[3];
      filters[0] = new ValueFilter("F1", 123);
      filters[1] = new ValueFilter("F2", 456);
      filters[2] = new ValueFilter("F3", 789);

      AndFilter sut = new AndFilter(filters);
      CollectionAssert.AreEqual(filters, sut.Filters, "Filters");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_2args_simple()
    {
      DBxFilter f1 = new ValueFilter("F1", 123);
      DBxFilter f2 = new ValueFilter("F2", 456);
      AndFilter sut = new AndFilter(f1, f2);
      CollectionAssert.AreEqual(new DBxFilter[2] { f1, f2}, sut.Filters, "Filters");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_2args_AND_Other()
    {
      DBxFilter f1 = new ValueFilter("F1", 111);
      DBxFilter f2 = new ValueFilter("F2", 222);
      DBxFilter f3 = new ValueFilter("F3", 333);
      DBxFilter f12 = new AndFilter(f1, f2);

      AndFilter sut = new AndFilter(f12, f3);
      CollectionAssert.AreEqual(new DBxFilter[3] { f1, f2, f3 }, sut.Filters, "Filters");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_2args_Other_AND()
    {
      DBxFilter f1 = new ValueFilter("F1", 111);
      DBxFilter f2 = new ValueFilter("F2", 222);
      DBxFilter f3 = new ValueFilter("F3", 333);
      DBxFilter f23 = new AndFilter(f2, f3);

      AndFilter sut = new AndFilter(f1, f23);
      CollectionAssert.AreEqual(new DBxFilter[3] { f1, f2, f3 }, sut.Filters, "Filters");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_2args_AND_AND()
    {
      DBxFilter f1 = new ValueFilter("F1", 111);
      DBxFilter f2 = new ValueFilter("F2", 222);
      DBxFilter f3 = new ValueFilter("F3", 333);
      DBxFilter f4 = new ValueFilter("F4", 444);
      DBxFilter f12 = new AndFilter(f1, f2);
      DBxFilter f34 = new AndFilter(f3, f4);

      AndFilter sut = new AndFilter(f12, f34);
      CollectionAssert.AreEqual(new DBxFilter[4] { f1, f2, f3, f4 }, sut.Filters, "Filters");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_exceptions()
    {
      Assert.Catch<ArgumentNullException>(delegate () { new AndFilter((DBxFilter[])null); }, "null");
      Assert.Catch<ArgumentException>(delegate () { new AndFilter((new DBxFilter[0])); }, "Length=0");
      DBxFilter f1 = new ValueFilter("F1", 123);
      Assert.Catch<ArgumentException>(delegate () { new AndFilter((new DBxFilter[1] { f1 })); }, "Length=1");

      Assert.Catch<ArgumentNullException>(delegate () { new AndFilter(f1, null); }, "First is null");
      Assert.Catch<ArgumentNullException>(delegate () { new AndFilter(null, f1); }, "Second is null");
    }

    #endregion

    #region FromList()/FromArray()

    #region Обычное использование

    [Test]
    public void FromList_FromArray_3_normal()
    {
      List<DBxFilter> filters = new List<DBxFilter>();
      filters.Add(new ValueFilter("F1", 1));
      filters.Add(new ValueFilter("F2", 2));
      filters.Add(new ValueFilter("F3", 3));

      DoTestAndFilter(AndFilter.FromList(filters), "FromList()", filters[0], filters[1], filters[2]);
      DoTestAndFilter(AndFilter.FromArray(filters.ToArray()), "FromArray()", filters[0], filters[1], filters[2]);
    }

    [Test]
    public void FromList_FromArray_2_normal()
    {
      List<DBxFilter> filters = new List<DBxFilter>();
      filters.Add(new ValueFilter("F1", 1));
      filters.Add(new ValueFilter("F2", 2));

      DoTestAndFilter(AndFilter.FromList(filters), "FromList()", filters[0], filters[1] );
      DoTestAndFilter(AndFilter.FromArray(filters.ToArray()), "FromArray()", filters[0], filters[1]);
    }

    private static void DoTestAndFilter(DBxFilter res, string messagePrefix, params DBxFilter[] wantedFilters)
    {
      Assert.IsInstanceOf<AndFilter>(res, messagePrefix + "-Type");

      DBxFilter[] wantedRes = new DBxFilter[wantedFilters.Length + 1];
      wantedRes[0] = res;
      Array.Copy(wantedFilters, 0, wantedRes, 1, wantedFilters.Length);

      List<DBxFilter> lst = new List<DBxFilter>();
      res.GetAllFilters(lst);
      CollectionAssert.AreEqual(wantedRes, lst, messagePrefix + "-Filters");
      Assert.AreEqual(DBxFilterDegeneration.None, res.Degeneration, messagePrefix + "-Degeneration");
    }

    [Test]
    public void FromList_FromArray_1_normal()
    {
      List<DBxFilter> filters = new List<DBxFilter>();
      filters.Add(new ValueFilter("F1", 1));

      DBxFilter res1 = AndFilter.FromList(filters);
      Assert.AreSame(filters[0], res1, "FromList()");

      DBxFilter res2 = AndFilter.FromArray(filters.ToArray());
      Assert.AreSame(filters[0], res2, "FromArray()");
    }

    [Test]
    public void FromList_FromArray_0_normal()
    {
      List<DBxFilter> filters = new List<DBxFilter>();

      DBxFilter res1 = AndFilter.FromList(filters);
      Assert.IsNull(res1, "FromList()");

      DBxFilter res2 = AndFilter.FromArray(filters.ToArray());
      Assert.IsNull(res2, "FromArray()");
    }

    #endregion

    #region null'ы

    [Test]
    public void FromList_FromArray_null()
    {
      List<DBxFilter> filters1 = null;
      DBxFilter res1 = AndFilter.FromList(filters1);
      Assert.IsNull(res1, "FromList()");

      DBxFilter[] filters2 = null;
      DBxFilter res2 = AndFilter.FromArray(filters2);
      Assert.IsNull(res2, "FromArray()");
    }

    #endregion

    #region Фильтры с Degeneration

    [Test]
    public void FromList_FromArray_1_True()
    {
      List<DBxFilter> filters = new List<DBxFilter>();
      filters.Add(new DummyFilter(true));

      DBxFilter res1 = AndFilter.FromList(filters);
      Assert.IsNull(res1, "FromList()");

      DBxFilter res2 = AndFilter.FromArray(filters.ToArray());
      Assert.IsNull(res2, "FromArray()");
    }

    [Test]
    public void FromList_FromArray_1_False()
    {
      List<DBxFilter> filters = new List<DBxFilter>();
      filters.Add(new DummyFilter(false));

      DoTestDummyFilterFalse(AndFilter.FromList(filters), "FromList()");
      DoTestDummyFilterFalse(AndFilter.FromArray(filters.ToArray()), "FromArray()");
    }

    private static void DoTestDummyFilterFalse(DBxFilter res, string messagePrefix)
    {
      Assert.IsInstanceOf<DummyFilter>(res, messagePrefix + "-Type");
      Assert.AreEqual(DBxFilterDegeneration.AlwaysFalse, res.Degeneration, messagePrefix + "-Degeneration");
    }

    [Test]
    public void FromList_FromArray_2_True_X()
    {
      List<DBxFilter> filters = new List<DBxFilter>();
      filters.Add(new DummyFilter(true));
      filters.Add(new ValueFilter("F2", 2));

      DBxFilter res1 = AndFilter.FromList(filters);
      Assert.AreSame(filters[1], res1);

      DBxFilter res2 = AndFilter.FromArray(filters.ToArray());
      Assert.AreSame(filters[1], res2);
    }

    [Test]
    public void FromList_FromArray_2_False_X()
    {
      List<DBxFilter> filters = new List<DBxFilter>();
      filters.Add(new DummyFilter(false));
      filters.Add(new ValueFilter("F2", 2));

      DoTestDummyFilterFalse(AndFilter.FromList(filters), "FromList()");
      DoTestDummyFilterFalse(AndFilter.FromArray(filters.ToArray()), "FromArray()");
    }

    [Test]
    public void FromList_FromArray_3_X_True_X()
    {
      List<DBxFilter> filters = new List<DBxFilter>();
      filters.Add(new ValueFilter("F1", 1));
      filters.Add(new DummyFilter(true));
      filters.Add(new ValueFilter("F3", 3));

      DoTestAndFilter(AndFilter.FromList(filters), "FromList()", filters[0], filters[2]);
      DoTestAndFilter(AndFilter.FromArray(filters.ToArray()), "FromArray()", filters[0], filters[2]);
    }

    [Test]
    public void FromList_FromArray_3_X_False_X()
    {
      List<DBxFilter> filters = new List<DBxFilter>();
      filters.Add(new ValueFilter("F1", 1));
      filters.Add(new DummyFilter(false));
      filters.Add(new ValueFilter("F3", 3));

      DoTestDummyFilterFalse(AndFilter.FromList(filters), "FromList()");
      DoTestDummyFilterFalse(AndFilter.FromArray(filters.ToArray()), "FromArray()");
    }

    #endregion

    #endregion

    #region Degeneration

    [TestCase(null, null, null, DBxFilterDegeneration.None)]
    [TestCase(false, null, null, DBxFilterDegeneration.AlwaysFalse)]
    [TestCase(null, false, null, DBxFilterDegeneration.AlwaysFalse)]
    [TestCase(null, null, false, DBxFilterDegeneration.AlwaysFalse)]
    [TestCase(null, true, false, DBxFilterDegeneration.AlwaysFalse)]
    [TestCase(null, true, true, DBxFilterDegeneration.None)]
    [TestCase(true, true, true, DBxFilterDegeneration.AlwaysTrue)]
    public void Degeneration(bool? f1, bool? f2, bool?f3, DBxFilterDegeneration wantedRes)
    {
      DBxFilter[] filters = new DBxFilter[3];
      filters[0] = CreateDegenerationTestFilter(f1);
      filters[1] = CreateDegenerationTestFilter(f2);
      filters[2] = CreateDegenerationTestFilter(f3);
      AndFilter sut = new AndFilter(filters);
      Assert.AreEqual(wantedRes, sut.Degeneration);
    }

    private static DBxFilter CreateDegenerationTestFilter(bool? f)
    {
      if (f.HasValue)
        return new DummyFilter(f.Value);
      else
        return new ValueFilter("F1", 1);
    }

    #endregion

    #region Прочие методы

    [Test]
    public void GetColumnNames()
    {
      DBxFilter f1 = new ValueFilter("F1", 111);
      DBxFilter f2 = new ValueFilter("F2", 222);

      AndFilter sut = new AndFilter(f1, f2);
      DBxColumnList res = new DBxColumnList();
      sut.GetColumnNames(res);
      CollectionAssert.AreEquivalent(new string[] { "F1", "F2" }, res.ToArray()); // порядок полей , вообще говоря, не определен
    }

    [Test]
    public void GetAllFilters()
    {
      DBxFilter f1 = new ValueFilter("F1", 111);
      DBxFilter f2 = new ValueFilter("F2", 222);
      AndFilter sut = new AndFilter(f1, f2);

      List<DBxFilter> res = new List<DBxFilter>();
      sut.GetAllFilters(res);
      CollectionAssert.AreEqual(new DBxFilter[3] { sut, f1, f2 }, res.ToArray());
    }

    #endregion

    #region TestFilter()

    [TestCase(1, 2, 3, true)]
    [TestCase(0, 2, 3, false)]
    [TestCase(1, 0, 3, false)]
    [TestCase(1, 2, 0, false)]
    [TestCase(0, 0, 0, false)]
    public void TestFilter(int v1, int v2, int v3, bool wantedRes)
    {
      DBxFilter[] filters = new DBxFilter[3];
      filters[0] = new ValueFilter("F1", 1);
      filters[1] = new ValueFilter("F2", 2);
      filters[2] = new ValueFilter("F3", 3);
      AndFilter sut = new AndFilter(filters);

      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", v1);
      vals.Add("F2", v2);
      vals.Add("F3", v3);

      bool res = sut.TestFilter(vals);
      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void TestFilter_Degeneration_False()
    {
      DBxFilter[] filters = new DBxFilter[3];
      filters[0] = new ValueFilter("F1", 1);
      filters[1] = new ValueFilter("F2", 2);
      filters[2] = new DummyFilter(false);
      AndFilter sut = new AndFilter(filters);
      Assert.AreEqual(DBxFilterDegeneration.AlwaysFalse, sut.Degeneration, "Degeneration");

      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", 1);
      vals.Add("F2", 2);

      bool res = sut.TestFilter(vals);
      Assert.IsFalse(res, "Result");
    }

    [Test]
    public void TestFilter_Degeneration_True()
    {
      DBxFilter[] filters = new DBxFilter[3];
      filters[0] = new DummyFilter(true);
      filters[1] = new DummyFilter(true);
      filters[2] = new DummyFilter(true);
      AndFilter sut = new AndFilter(filters);
      Assert.AreEqual(DBxFilterDegeneration.AlwaysTrue, sut.Degeneration, "Degeneration");

      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);

      bool res = sut.TestFilter(vals);
      Assert.IsTrue(res, "Result");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DBxFilter[] filters = new DBxFilter[2];
      filters[0] = new ValueFilter("F1", 1);
      filters[1] = new ValueFilter("F2", 2);
      AndFilter sut = new AndFilter(filters);
      byte[] b = SerializationTools.SerializeBinary(sut);
      AndFilter res = (AndFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
      // так нельзя проверять, т.к. у фильтров не определена операция сравнения // Assert.AreEqual(sut.Filters, res.Filters, "Filters");
    }

    #endregion
  }
}
