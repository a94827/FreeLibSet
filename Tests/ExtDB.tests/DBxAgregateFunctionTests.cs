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
  public class DBxAgregateFunctionTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_DBxExpression()
    {
      DBxColumn col1 = new DBxColumn("F1");
      DBxColumn col2 = new DBxColumn("F2");
      DBxExpression expr = new DBxFunction(DBxFunctionKind.Multiply, col1, col2);
      DBxAgregateFunction sut = new DBxAgregateFunction(DBxAgregateFunctionKind.Sum, expr);
      Assert.AreEqual(DBxAgregateFunctionKind.Sum, sut.Function, "Function");
      Assert.AreSame(expr, sut.Argument, "Argument");
    }

    [Test]
    public void Constructor_ColumnName()
    {
      DBxAgregateFunction sut = new DBxAgregateFunction(DBxAgregateFunctionKind.Max, "F1");
      Assert.AreEqual(DBxAgregateFunctionKind.Max, sut.Function, "Function");
      Assert.IsInstanceOf<DBxColumn>(sut.Argument, "Argument type");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Argument)).ColumnName, "ColumnName");
    }

    [Test]
    public void Constructor_Count_NoArg()
    {
      DBxAgregateFunction sut = new DBxAgregateFunction(DBxAgregateFunctionKind.Count, "");
      Assert.AreEqual(DBxAgregateFunctionKind.Count, sut.Function, "Function");
      Assert.IsNull(sut.Argument, "Argument");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void Equals()
    {
      DBxAgregateFunction sut = new DBxAgregateFunction(DBxAgregateFunctionKind.Min, "F1");

      DBxAgregateFunction res1 = new DBxAgregateFunction(DBxAgregateFunctionKind.Min, "F1");
      Assert.IsTrue(res1 == sut, "Equal");

      DBxAgregateFunction res2 = new DBxAgregateFunction(DBxAgregateFunctionKind.Max, "F1");
      Assert.IsFalse(res2 == sut, "Different function kind");

      DBxAgregateFunction res3 = new DBxAgregateFunction(DBxAgregateFunctionKind.Max, "F2");
      Assert.IsFalse(res3 == sut, "Different argument");
    }

    [Test]
    public void Equals_noArg()
    {
      DBxAgregateFunction sut = new DBxAgregateFunction(DBxAgregateFunctionKind.Count, "");

      DBxAgregateFunction res1 = new DBxAgregateFunction(DBxAgregateFunctionKind.Count, "");
      Assert.IsTrue(res1 == sut, "Equal");

      DBxAgregateFunction res3 = new DBxAgregateFunction(DBxAgregateFunctionKind.Count, "F2");
      Assert.IsFalse(res3 == sut, "Different argument");
    }

    [Test]
    public void GetColumnNames()
    {
      DBxColumn col1 = new DBxColumn("F1");
      DBxColumn col2 = new DBxColumn("F2");
      DBxExpression expr = new DBxFunction(DBxFunctionKind.Multiply, col1, col2);
      DBxAgregateFunction sut = new DBxAgregateFunction(DBxAgregateFunctionKind.Sum, expr);
      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);
      Assert.AreEqual("F1,F2", lst.AsString);
    }

    [Test]
    public void GetColumnNames_noArg()
    {
      DBxAgregateFunction sut = new DBxAgregateFunction(DBxAgregateFunctionKind.Count, "");
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
      DBxAgregateFunction sut = new DBxAgregateFunction(DBxAgregateFunctionKind.Sum, expr);

      DBxAgregateFunction res = (DBxAgregateFunction)(sut.SetColumnNamePrefix("ABC"));

      Assert.AreEqual(DBxAgregateFunctionKind.Sum, res.Function, "Function");
      DBxFunction resexpr = res.Argument as DBxFunction;
      Assert.AreEqual(DBxFunctionKind.Multiply, resexpr.Function, "Argument.Function");
      DBxColumn rescol1 = resexpr.Arguments[0] as DBxColumn;
      DBxColumn rescol2 = resexpr.Arguments[1] as DBxColumn;
      Assert.AreEqual("ABCF1", rescol1.ColumnName);
      Assert.AreEqual("ABCF2", rescol2.ColumnName);
    }

    #endregion
  }
}
