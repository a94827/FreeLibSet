using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.Remoting;
using System.Data;

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DBxAggregateFunctionTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_DBxExpression()
    {
      DBxColumn col1 = new DBxColumn("F1");
      DBxColumn col2 = new DBxColumn("F2");
      DBxExpression expr = new DBxFunction(DBxFunctionKind.Multiply, col1, col2);
      DBxAggregateFunction sut = new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, expr);
      Assert.AreEqual(DBxAggregateFunctionKind.Sum, sut.Function, "Function");
      Assert.AreSame(expr, sut.Argument, "Argument");
    }

    [Test]
    public void Constructor_ColumnName()
    {
      DBxAggregateFunction sut = new DBxAggregateFunction(DBxAggregateFunctionKind.Max, "F1");
      Assert.AreEqual(DBxAggregateFunctionKind.Max, sut.Function, "Function");
      Assert.IsInstanceOf<DBxColumn>(sut.Argument, "Argument type");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Argument)).ColumnName, "ColumnName");
    }

    [Test]
    public void Constructor_Count_NoArg()
    {
      DBxAggregateFunction sut = new DBxAggregateFunction(DBxAggregateFunctionKind.Count, "");
      Assert.AreEqual(DBxAggregateFunctionKind.Count, sut.Function, "Function");
      Assert.IsNull(sut.Argument, "Argument");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void Equals()
    {
      DBxAggregateFunction sut = new DBxAggregateFunction(DBxAggregateFunctionKind.Min, "F1");

      DBxAggregateFunction res1 = new DBxAggregateFunction(DBxAggregateFunctionKind.Min, "F1");
      Assert.IsTrue(res1 == sut, "Equal");

      DBxAggregateFunction res2 = new DBxAggregateFunction(DBxAggregateFunctionKind.Max, "F1");
      Assert.IsFalse(res2 == sut, "Different function kind");

      DBxAggregateFunction res3 = new DBxAggregateFunction(DBxAggregateFunctionKind.Max, "F2");
      Assert.IsFalse(res3 == sut, "Different argument");
    }

    [Test]
    public void Equals_noArg()
    {
      DBxAggregateFunction sut = new DBxAggregateFunction(DBxAggregateFunctionKind.Count, "");

      DBxAggregateFunction res1 = new DBxAggregateFunction(DBxAggregateFunctionKind.Count, "");
      Assert.IsTrue(res1 == sut, "Equal");

      DBxAggregateFunction res3 = new DBxAggregateFunction(DBxAggregateFunctionKind.Count, "F2");
      Assert.IsFalse(res3 == sut, "Different argument");
    }

    [Test]
    public void GetColumnNames()
    {
      DBxColumn col1 = new DBxColumn("F1");
      DBxColumn col2 = new DBxColumn("F2");
      DBxExpression expr = new DBxFunction(DBxFunctionKind.Multiply, col1, col2);
      DBxAggregateFunction sut = new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, expr);
      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);
      Assert.AreEqual("F1,F2", lst.AsString);
    }

    [Test]
    public void GetColumnNames_noArg()
    {
      DBxAggregateFunction sut = new DBxAggregateFunction(DBxAggregateFunctionKind.Count, "");
      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);
      Assert.AreEqual(0, lst.Count);
    }

    // GetConst() и GetValue() не тестируем

      [Test]
    public void SetColumnNamePrefix()
    {
      DBxColumn col1 = new DBxColumn("F1");
      DBxColumn col2 = new DBxColumn("F2");
      DBxFunction expr = new DBxFunction(DBxFunctionKind.Multiply, col1, col2);
      DBxAggregateFunction sut = new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, expr);

      DBxAggregateFunction res = (DBxAggregateFunction)(sut.SetColumnNamePrefix("ABC"));

      Assert.AreEqual(DBxAggregateFunctionKind.Sum, res.Function, "Function");
      DBxFunction resexpr = res.Argument as DBxFunction;
      Assert.AreEqual(DBxFunctionKind.Multiply, resexpr.Function, "Argument.Function");
      DBxColumn rescol1 = resexpr.Arguments[0] as DBxColumn;
      DBxColumn rescol2 = resexpr.Arguments[1] as DBxColumn;
      Assert.AreEqual("ABCF1", rescol1.ColumnName);
      Assert.AreEqual("ABCF2", rescol2.ColumnName);
    }


    [Test]
    public void GetAllExpressions()
    {
      DBxColumn col1 = new DBxColumn("F1");
      DBxColumn col2 = new DBxColumn("F2");
      DBxFunction expr = new DBxFunction(DBxFunctionKind.Multiply, col1, col2);
      DBxAggregateFunction sut = new DBxAggregateFunction(DBxAggregateFunctionKind.Sum, expr);

      List<DBxExpression> lst = new List<DBxExpression>();
      sut.GetAllExpressions(lst);
      CollectionAssert.AreEqual(new DBxExpression[] { sut, expr, col1, col2 }, lst);
    }
    #endregion
  }
}
