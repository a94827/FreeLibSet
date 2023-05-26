using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Collections;
using FreeLibSet.Remoting;
using FreeLibSet.Core;
using System.Data;

// Выполняется тестирование класса фильтров вне связи с табличными данными

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DBxFilterTests
  {
    #region Операторы

    #region AND

    [Test]
    public void Operator_AND_X_X()
    {
      DBxFilter f1 = new ValueFilter("F1", 1);
      DBxFilter f2 = new ValueFilter("F2", 2);
      DBxFilter res = f1 & f2;
      Assert.IsInstanceOf<AndFilter>(res, "Type");
      CollectionAssert.AreEqual(new DBxFilter[2] { f1, f2 }, ((AndFilter)res).Filters);
    }

    [Test]
    public void Operator_AND_X_null()
    {
      DBxFilter f1 = new ValueFilter("F1", 1);
      DBxFilter f2 = null;
      DBxFilter res = f1 & f2;
      Assert.AreSame(f1, res);
    }

    [Test]
    public void Operator_AND_null_X()
    {
      DBxFilter f1 = null;
      DBxFilter f2 = new ValueFilter("F2", 2);
      DBxFilter res = f1 & f2;
      Assert.AreSame(f2, res);
    }

    [Test]
    public void Operator_AND_null_null()
    {
      DBxFilter f1 = null;
      DBxFilter f2 = null;
      DBxFilter res = f1 & f2;
      Assert.IsNull(res);
    }

    [Test]
    public void Operator_AND_X_True()
    {
      DBxFilter f1 = new ValueFilter("F1", 1);
      DBxFilter f2 = new DummyFilter(true);
      DBxFilter res = f1 & f2;
      Assert.AreSame(f1, res);
    }

    [Test]
    public void Operator_AND_True_X()
    {
      DBxFilter f1 = new DummyFilter(true);
      DBxFilter f2 = new ValueFilter("F2", 2);
      DBxFilter res = f1 & f2;
      Assert.AreSame(f2, res);
    }

    [Test]
    public void Operator_AND_True_True()
    {
      DBxFilter f1 = new DummyFilter(true);
      DBxFilter f2 = new DummyFilter(true);
      DBxFilter res = f1 & f2;
      Assert.IsNull(res);
    }

    [Test]
    public void Operator_AND_X_False()
    {
      DBxFilter f1 = new ValueFilter("F1", 1);
      DBxFilter f2 = new DummyFilter(false);
      DBxFilter res = f1 & f2;
      Assert.IsInstanceOf<DummyFilter>(res);
      Assert.AreEqual(DBxFilterDegeneration.AlwaysFalse, res.Degeneration);
    }

    [Test]
    public void Operator_AND_False_X()
    {
      DBxFilter f1 = new DummyFilter(false);
      DBxFilter f2 = new ValueFilter("F2", 2);
      DBxFilter res = f1 & f2;
      Assert.IsInstanceOf<DummyFilter>(res);
      Assert.AreEqual(DBxFilterDegeneration.AlwaysFalse, res.Degeneration);
    }

    [Test]
    public void Operator_AND_False_False()
    {
      DBxFilter f1 = new DummyFilter(false);
      DBxFilter f2 = new DummyFilter(false);
      DBxFilter res = f1 & f2;
      Assert.IsInstanceOf<DummyFilter>(res);
      Assert.AreEqual(DBxFilterDegeneration.AlwaysFalse, res.Degeneration);
    }

    [Test]
    public void Operator_AND_False_True()
    {
      DBxFilter f1 = new DummyFilter(false);
      DBxFilter f2 = new DummyFilter(true);
      DBxFilter res = f1 & f2;
      Assert.IsInstanceOf<DummyFilter>(res);
      Assert.AreEqual(DBxFilterDegeneration.AlwaysFalse, res.Degeneration);
    }

    [Test]
    public void Operator_AND_True_False()
    {
      DBxFilter f1 = new DummyFilter(true);
      DBxFilter f2 = new DummyFilter(false);
      DBxFilter res = f1 & f2;
      Assert.IsInstanceOf<DummyFilter>(res);
      Assert.AreEqual(DBxFilterDegeneration.AlwaysFalse, res.Degeneration);
    }

    #endregion

    #region OR

    [Test]
    public void Operator_OR_X_X()
    {
      DBxFilter f1 = new ValueFilter("F1", 1);
      DBxFilter f2 = new ValueFilter("F2", 2);
      DBxFilter res = f1 | f2;
      Assert.IsInstanceOf<OrFilter>(res, "Type");
      CollectionAssert.AreEqual(new DBxFilter[2] { f1, f2 }, ((OrFilter)res).Filters);
    }

    [Test]
    public void Operator_OR_X_null()
    {
      DBxFilter f1 = new ValueFilter("F1", 1);
      DBxFilter f2 = null;
      DBxFilter res = f1 | f2;
      Assert.IsNull(res);
    }

    [Test]
    public void Operator_OR_null_X()
    {
      DBxFilter f1 = null;
      DBxFilter f2 = new ValueFilter("F2", 2);
      DBxFilter res = f1 | f2;
      Assert.IsNull(res);
    }

    [Test]
    public void Operator_OR_null_null()
    {
      DBxFilter f1 = null;
      DBxFilter f2 = null;
      DBxFilter res = f1 | f2;
      Assert.IsNull(res);
    }

    [Test]
    public void Operator_OR_X_False()
    {
      DBxFilter f1 = new ValueFilter("F1", 1);
      DBxFilter f2 = new DummyFilter(false);
      DBxFilter res = f1 | f2;
      Assert.AreSame(f1, res);
    }

    [Test]
    public void Operator_OR_False_X()
    {
      DBxFilter f1 = new DummyFilter(false);
      DBxFilter f2 = new ValueFilter("F2", 2);
      DBxFilter res = f1 | f2;
      Assert.AreSame(f2, res);
    }

    [Test]
    public void Operator_OR_False_False()
    {
      DBxFilter f1 = new DummyFilter(false);
      DBxFilter f2 = new DummyFilter(false);
      DBxFilter res = f1 | f2;
      Assert.IsInstanceOf<DummyFilter>(res);
      Assert.AreEqual(DBxFilterDegeneration.AlwaysFalse, res.Degeneration);
    }

    [Test]
    public void Operator_OR_X_True()
    {
      DBxFilter f1 = new ValueFilter("F1", 1);
      DBxFilter f2 = new DummyFilter(true);
      DBxFilter res = f1 | f2;
      Assert.IsNull(res);
    }

    [Test]
    public void Operator_OR_True_X()
    {
      DBxFilter f1 = new DummyFilter(true);
      DBxFilter f2 = new ValueFilter("F2", 2);
      DBxFilter res = f1 | f2;
      Assert.IsNull(res);
    }

    [Test]
    public void Operator_OR_True_True()
    {
      DBxFilter f1 = new DummyFilter(true);
      DBxFilter f2 = new DummyFilter(true);
      DBxFilter res = f1 | f2;
      Assert.IsNull(res);
    }

    [Test]
    public void Operator_OR_False_True()
    {
      DBxFilter f1 = new DummyFilter(false);
      DBxFilter f2 = new DummyFilter(true);
      DBxFilter res = f1 | f2;
      Assert.IsNull(res);
    }

    [Test]
    public void Operator_OR_True_False()
    {
      DBxFilter f1 = new DummyFilter(true);
      DBxFilter f2 = new DummyFilter(false);
      DBxFilter res = f1 | f2;
      Assert.IsNull(res);
    }

    #endregion

    #region NOT

    [Test]
    public void Operator_NOT_normal()
    {
      DBxFilter f1 = new ValueFilter("F1", 1);
      DBxFilter res = !f1;
      Assert.IsInstanceOf<NotFilter>(res, "Type");
      Assert.AreSame(f1, ((NotFilter)res).BaseFilter);
    }

    [Test]
    public void Operator_NOT_twice()
    {
      DBxFilter f1 = new ValueFilter("F1", 1);
      DBxFilter f2 = !f1;
      DBxFilter res = !f2;
      Assert.AreSame(f1, res);
    }

    [Test]
    public void Operator_NOT_null()
    {
      DBxFilter f1 = null;
      DBxFilter res = !f1;
      Assert.IsInstanceOf<DummyFilter>(res, "Type");
      Assert.AreEqual(DBxFilterDegeneration.AlwaysFalse, res.Degeneration, "Degeneration");
    }

    [TestCase(true, DBxFilterDegeneration.AlwaysFalse)]
    [TestCase(false, DBxFilterDegeneration.AlwaysTrue)]
    public void Operator_NOT_degenaration(bool isTrue, DBxFilterDegeneration wantedDegeneration)
    {
      DBxFilter f1 = new DummyFilter(isTrue);
      DBxFilter res = !f1;
      Assert.AreEqual(wantedDegeneration, res.Degeneration);
    }

    #endregion

    #endregion

    #region SetColumnValues()

    [Test]
    public void SetColumnValues()
    {
      DataTable testTable = new DataTable();
      testTable.Columns.Add("F1", typeof(string));
      testTable.Columns.Add("F2", typeof(int));
      testTable.Rows.Add("AA", 1);
      testTable.Rows.Add("AB", 2);
      testTable.Rows.Add("BA", 3);
      testTable.Rows.Add("BB", 4);

      DBxFilter sut = new SubstringFilter("F1", 1, "A");
      sut.SetColumnValues(testTable, "F2", 123);
      int[] res = DataTools.GetValuesFromColumn<int>(testTable, "F2");
      Assert.AreEqual(new int[] { 123, 2, 123, 4 }, res);
    }

    #endregion
  }
}
