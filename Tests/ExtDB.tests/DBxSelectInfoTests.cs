using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Collections;
using FreeLibSet.Remoting;
using FreeLibSet.Core;
using FreeLibSet.Calendar;

// Выполняется тестирование класса фильтров вне связи с табличными данными

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DBxSelectInfoTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DBxSelectInfo sut = new DBxSelectInfo();

      Assert.IsTrue(String.IsNullOrEmpty(sut.TableName), "TableName");
      Assert.AreEqual(0, sut.Expressions.Count, "Expressions");
      Assert.IsNull(sut.Where, "Where");
      Assert.AreEqual(0, sut.GroupBy.Count, "GroupBy");
      Assert.IsNull(sut.Having, "Having");
      Assert.IsNull(sut.OrderBy, "OrderBy");
      Assert.AreEqual(0, sut.MaxRecordCount, "MaxRecordCount");
      Assert.IsFalse(sut.Unique, "Unique");
    }

    #endregion

    #region InitGroupBy()

    [Test]
    public void InitGroupBy()
    {
      DBxSelectInfo sut = new DBxSelectInfo();
      sut.Expressions.Add("F1");
      sut.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Min, "F2"), "MINF2");
      sut.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Max, "F2"), "MAXF2");
      sut.Expressions.Add(new DBxFunction(DBxFunctionKind.Add,
        new DBxFunction(DBxFunctionKind.Multiply, "F3", "F4"),
        new DBxFunction(DBxFunctionKind.Multiply, "F5", "F6")), "F4567");

      sut.InitGroupBy();

      Assert.AreEqual(2, sut.GroupBy.Count, "Count");
      Assert.AreSame(sut.Expressions[0].Expression, sut.GroupBy[0], "[0]");
      Assert.AreSame(sut.Expressions[3].Expression, sut.GroupBy[1], "[1]");
    }

    #endregion

    #region GetColumnNames()

    [Test]
    public void GetColumnNames()
    {
      DBxSelectInfo sut = new DBxSelectInfo();
      sut.Expressions.Add("F1,F2");
      sut.Expressions.Add(new DBxFunction(DBxFunctionKind.Abs, new DBxColumn("F3")), "F4");
      sut.Expressions.Add(new DBxColumn("F5"), "F6");
      sut.Where = new ValueFilter("F7", 123);
      sut.GroupBy.Add(new DBxColumn("F8"));
      sut.GroupBy.Add(new DBxFunction(DBxFunctionKind.Add, new DBxColumn("F9"), new DBxColumn("F10")));
      sut.Having = new ValueFilter("F11", 456);
      sut.OrderBy = new DBxOrder("F12");

      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);

      CollectionAssert.AreEquivalent(new string[] { "F1", "F2", "F3", "F5", "F7", "F8", "F9", "F10", "F11", "F12" }, lst);
    }

    [Test]
    public void GetColumnNames_skipOrderByAliases()
    {
      DBxSelectInfo sut = new DBxSelectInfo();
      sut.Expressions.Add(new DBxFunction(DBxFunctionKind.Abs, new DBxColumn("F1")), "F2");
      sut.OrderBy = DBxOrder.FromDataViewSort("F2,F3");

      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);

      CollectionAssert.AreEquivalent(new string[] { "F1", "F3" }, lst);
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone()
    {
      DBxSelectInfo sut = new DBxSelectInfo();
      sut.TableName = "T1";
      sut.Expressions.Add("F1");
      sut.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, "F2"), "A2");
      sut.Where = new ValueFilter("F3", 123);
      sut.GroupBy.Add(new DBxColumn("F1"));
      sut.Having = new ValueFilter(new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, "F2"), 1000, CompareKind.LessThan);
      sut.OrderBy = DBxOrder.FromDataViewSort("A2");
      sut.Unique = true;
      sut.MaxRecordCount = 10;

      DBxSelectInfo res = sut.Clone();
      Assert.AreNotSame(sut, res, "Not same");

      Assert.AreEqual(sut.TableName, res.TableName);
      CollectionAssert.AreEqual(sut.Expressions, res.Expressions, "Expressions");
      Assert.AreEqual(sut.Where.ToString(), res.Where.ToString(), "Where");
      CollectionAssert.AreEqual(sut.GroupBy, res.GroupBy, "GroupBy");

      // TODO: Так не работает. DBxAggregateFunction не работает с DataView
      //Assert.AreEqual(sut.Having.ToString(), res.Having.ToString(), "Having");
      Assert.IsInstanceOf<ValueFilter>(res.Having, "Having");

      Assert.AreEqual(sut.OrderBy.ToString(), res.OrderBy.ToString(), "OrderBy");
      Assert.IsTrue(res.Unique, "Unique");
      Assert.AreEqual(sut.MaxRecordCount, res.MaxRecordCount, "MaxRecordCount");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DBxSelectInfo sut = new DBxSelectInfo();
      sut.TableName = "T1";
      sut.Expressions.Add("F1");
      sut.Expressions.Add(new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, "F2"), "A2");
      sut.Where = new ValueFilter("F3", 123);
      sut.GroupBy.Add(new DBxColumn("F1"));
      sut.Having = new ValueFilter(new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, "F2"), 1000, CompareKind.LessThan);
      sut.OrderBy = DBxOrder.FromDataViewSort("A2");
      sut.Unique = true;
      sut.MaxRecordCount = 10;

      byte[] b = SerializationTools.SerializeBinary(sut);
      DBxSelectInfo res = (DBxSelectInfo)(SerializationTools.DeserializeBinary(b));

      Assert.AreEqual(sut.TableName, res.TableName);
      CollectionAssert.AreEqual(sut.Expressions, res.Expressions, "Expressions");
      Assert.AreEqual(sut.Where.ToString(), res.Where.ToString(), "Where");
      CollectionAssert.AreEqual(sut.GroupBy, res.GroupBy, "GroupBy");

      // TODO: Так не работает. DBxAggregateFunction не работает с DataView
      //Assert.AreEqual(sut.Having.ToString(), res.Having.ToString(), "Having");
      Assert.IsInstanceOf<ValueFilter>(res.Having, "Having");

      Assert.AreEqual(sut.OrderBy.ToString(), res.OrderBy.ToString(), "OrderBy");
      Assert.IsTrue(res.Unique, "Unique");
      Assert.AreEqual(sut.MaxRecordCount, res.MaxRecordCount, "MaxRecordCount");
    }

    #endregion
  }
}
