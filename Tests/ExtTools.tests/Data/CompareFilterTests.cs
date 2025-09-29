using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Collections;
using FreeLibSet.Remoting;

// Выполняется тестирование класса фильтров вне связи с табличными данными

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class CompareFilterTests
  {
    #region Конструкторы с выражениями

    [Test]
    public void Constructor_expr_5args()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      DBxExpression expr2 = new DBxColumn("F2");
      CompareFilter sut = new CompareFilter(expr1, expr2, CompareKind.GreaterThan, true, DBxColumnType.Int32);
      Assert.AreSame(expr1, sut.Expression1, "Expression1");
      Assert.AreSame(expr2, sut.Expression2, "Expression2");
      Assert.AreEqual(CompareKind.GreaterThan, sut.Kind, "Kind");
      Assert.IsTrue(sut.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.IsFalse(sut.ComparisionToNull, "ComparisionToNull");
      Assert.AreEqual(DBxColumnType.Int32, sut.ColumnType, "ColumnType");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "ColumnType");
    }

    [Test]
    public void Constructor_expr_4args()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      DBxExpression expr2 = new DBxColumn("F2");
      CompareFilter sut = new CompareFilter(expr1, expr2, CompareKind.GreaterThan, true);
      Assert.AreSame(expr1, sut.Expression1, "Expression1");
      Assert.AreSame(expr2, sut.Expression2, "Expression2");
      Assert.AreEqual(CompareKind.GreaterThan, sut.Kind, "Kind");
      Assert.IsTrue(sut.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.IsFalse(sut.ComparisionToNull, "ComparisionToNull");
      Assert.AreEqual(DBxColumnType.Unknown, sut.ColumnType, "ColumnType");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "ColumnType");
    }

    [Test]
    public void Constructor_expr_3args()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      DBxExpression expr2 = new DBxColumn("F2");
      CompareFilter sut = new CompareFilter(expr1, expr2, CompareKind.GreaterThan);
      Assert.AreSame(expr1, sut.Expression1, "Expression1");
      Assert.AreSame(expr2, sut.Expression2, "Expression2");
      Assert.AreEqual(CompareKind.GreaterThan, sut.Kind, "Kind");
      Assert.IsFalse(sut.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.IsFalse(sut.ComparisionToNull, "ComparisionToNull");
      Assert.AreEqual(DBxColumnType.Unknown, sut.ColumnType, "ColumnType");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "ColumnType");
    }

    [Test]
    public void Constructor_expr_2args()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      DBxExpression expr2 = new DBxColumn("F2");
      CompareFilter sut = new CompareFilter(expr1, expr2);
      Assert.AreSame(expr1, sut.Expression1, "Expression1");
      Assert.AreSame(expr2, sut.Expression2, "Expression2");
      Assert.AreEqual(CompareKind.Equal, sut.Kind, "Kind");
      Assert.IsFalse(sut.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.IsFalse(sut.ComparisionToNull, "ComparisionToNull");
      Assert.AreEqual(DBxColumnType.Unknown, sut.ColumnType, "ColumnType");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "ColumnType");
    }

    #endregion

    #region Конструкторы сравнения IS NULL/IS NOT NULL

    [Test]
    public void Constructor_NULL_1()
    {
      DoConstructor_NULL(new DBxConst(null, DBxColumnType.Int32), new DBxColumn("F1"));
    }

    [Test]
    public void Constructor_NULL_2()
    {
      DoConstructor_NULL(new DBxColumn("F1"), new DBxConst(null, DBxColumnType.Int32));
    }

    private void DoConstructor_NULL(DBxExpression expr1, DBxExpression expr2)
    {
      CompareFilter sut = null /* чтобы не ругался компилятор*/;
      Assert.DoesNotThrow(delegate () { sut = new CompareFilter(expr1, expr2, CompareKind.Equal); }, "Equal");
      Assert.IsTrue(sut.ComparisionToNull, "ComparisionToNull #1");

      Assert.DoesNotThrow(delegate () { sut = new CompareFilter(expr1, expr2, CompareKind.NotEqual); }, "NotEqual");
      Assert.IsTrue(sut.ComparisionToNull, "ComparisionToNull #2");

      Assert.Catch<ArgumentException>(delegate () { new CompareFilter(expr1, expr2, CompareKind.LessThan); }, "LessThan");
      Assert.Catch<ArgumentException>(delegate () { new CompareFilter(expr1, expr2, CompareKind.LessOrEqualThan); }, "LessOrEqualThan");
      Assert.Catch<ArgumentException>(delegate () { new CompareFilter(expr1, expr2, CompareKind.GreaterThan); }, "GreaterThan");
      Assert.Catch<ArgumentException>(delegate () { new CompareFilter(expr1, expr2, CompareKind.GreaterOrEqualThan); }, "GreaterOrEqualThan");

      Assert.DoesNotThrow(delegate () { sut = new CompareFilter(expr1, expr2, CompareKind.Equal, false); }, "NullAsDefaultValue=false");
      Assert.IsTrue(sut.ComparisionToNull, "ComparisionToNull #3");
      Assert.Catch<ArgumentException>(delegate () { new CompareFilter(expr1, expr2, CompareKind.Equal, true); }, "NullAsDefaultValue=true");
    }

    #endregion

    #region Конструкторы с именами полей

    [Test]
    public void Constructor_colName_5args()
    {
      CompareFilter sut = new CompareFilter("F1", "F2", CompareKind.GreaterThan, true, DBxColumnType.Int32);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression1, "Expression1 type");
      Assert.IsInstanceOf<DBxColumn>(sut.Expression2, "Expression2 type");
      Assert.AreSame("F1", ((DBxColumn)(sut.Expression1)).ColumnName, "ColumnName1");
      Assert.AreSame("F2", ((DBxColumn)(sut.Expression2)).ColumnName, "ColumnName2");
      Assert.AreEqual(CompareKind.GreaterThan, sut.Kind, "Kind");
      Assert.IsTrue(sut.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.IsFalse(sut.ComparisionToNull, "ComparisionToNull");
      Assert.AreEqual(DBxColumnType.Int32, sut.ColumnType, "ColumnType");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "ColumnType");
    }

    [Test]
    public void Constructor_colName_4args()
    {
      CompareFilter sut = new CompareFilter("F1", "F2", CompareKind.GreaterThan, true);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression1, "Expression1 type");
      Assert.IsInstanceOf<DBxColumn>(sut.Expression2, "Expression2 type");
      Assert.AreSame("F1", ((DBxColumn)(sut.Expression1)).ColumnName, "ColumnName1");
      Assert.AreSame("F2", ((DBxColumn)(sut.Expression2)).ColumnName, "ColumnName2");
      Assert.AreEqual(CompareKind.GreaterThan, sut.Kind, "Kind");
      Assert.IsTrue(sut.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.IsFalse(sut.ComparisionToNull, "ComparisionToNull");
      Assert.AreEqual(DBxColumnType.Unknown, sut.ColumnType, "ColumnType");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "ColumnType");
    }

    [Test]
    public void Constructor_colName_3args()
    {
      CompareFilter sut = new CompareFilter("F1", "F2", CompareKind.GreaterThan);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression1, "Expression1 type");
      Assert.IsInstanceOf<DBxColumn>(sut.Expression2, "Expression2 type");
      Assert.AreSame("F1", ((DBxColumn)(sut.Expression1)).ColumnName, "ColumnName1");
      Assert.AreSame("F2", ((DBxColumn)(sut.Expression2)).ColumnName, "ColumnName2");
      Assert.AreEqual(CompareKind.GreaterThan, sut.Kind, "Kind");
      Assert.IsFalse(sut.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.IsFalse(sut.ComparisionToNull, "ComparisionToNull");
      Assert.AreEqual(DBxColumnType.Unknown, sut.ColumnType, "ColumnType");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "ColumnType");
    }

    [Test]
    public void Constructor_colName_2args()
    {
      CompareFilter sut = new CompareFilter("F1", "F2");
      Assert.IsInstanceOf<DBxColumn>(sut.Expression1, "Expression1 type");
      Assert.IsInstanceOf<DBxColumn>(sut.Expression2, "Expression2 type");
      Assert.AreSame("F1", ((DBxColumn)(sut.Expression1)).ColumnName, "ColumnName1");
      Assert.AreSame("F2", ((DBxColumn)(sut.Expression2)).ColumnName, "ColumnName2");
      Assert.AreEqual(CompareKind.Equal, sut.Kind, "Kind");
      Assert.IsFalse(sut.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.IsFalse(sut.ComparisionToNull, "ComparisionToNull");
      Assert.AreEqual(DBxColumnType.Unknown, sut.ColumnType, "ColumnType");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "ColumnType");
    }

    #endregion

    #region TestFilter()

    [TestCase(2, 1, CompareKind.Equal, false)]
    [TestCase(2, 2, CompareKind.Equal, true)]
    [TestCase(2, 3, CompareKind.Equal, false)]
    [TestCase(2, 1, CompareKind.NotEqual, true)]
    [TestCase(2, 2, CompareKind.NotEqual, false)]
    [TestCase(2, 3, CompareKind.NotEqual, true)]
    [TestCase(2, 1, CompareKind.LessThan, false)]
    [TestCase(2, 2, CompareKind.LessThan, false)]
    [TestCase(2, 3, CompareKind.LessThan, true)]
    [TestCase(2, 1, CompareKind.LessOrEqualThan, false)]
    [TestCase(2, 2, CompareKind.LessOrEqualThan, true)]
    [TestCase(2, 3, CompareKind.LessOrEqualThan, true)]
    [TestCase(2, 1, CompareKind.GreaterThan, true)]
    [TestCase(2, 2, CompareKind.GreaterThan, false)]
    [TestCase(2, 3, CompareKind.GreaterThan, false)]
    [TestCase(2, 1, CompareKind.GreaterOrEqualThan, true)]
    [TestCase(2, 2, CompareKind.GreaterOrEqualThan, true)]
    [TestCase(2, 3, CompareKind.GreaterOrEqualThan, false)]

    [TestCase(false, false, CompareKind.Equal, true)]
    [TestCase(false, true, CompareKind.Equal, false)]
    [TestCase(true, false, CompareKind.Equal, false)]
    [TestCase(false, false, CompareKind.Equal, true)]
    [TestCase(false, false, CompareKind.NotEqual, false)]
    [TestCase(false, true, CompareKind.NotEqual, true)]
    [TestCase(true, false, CompareKind.NotEqual, true)]
    [TestCase(false, false, CompareKind.NotEqual, false)]

    public void TestFilter_CompareKind(object v1, object v2, CompareKind kind, bool wantedRes)
    {
      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", v1);
      vals.Add("F2", v2);
      CompareFilter sut = new CompareFilter("F1", "F2", kind);
      bool res = sut.TestFilter(vals);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase(null, 0, CompareKind.Equal, false, false)]
    [TestCase(null, 0, CompareKind.Equal, true, true)]
    [TestCase(null, 0, CompareKind.NotEqual, false, false)]
    [TestCase(null, 0, CompareKind.NotEqual, true, false)]
    [TestCase(null, -1, CompareKind.LessThan, false, false)]
    [TestCase(null, 0, CompareKind.LessThan, false, false)]
    [TestCase(null, 1, CompareKind.LessThan, false, false)]
    [TestCase(null, -1, CompareKind.LessThan, true, false)]
    [TestCase(null, 0, CompareKind.LessThan, true, false)]
    [TestCase(null, 1, CompareKind.LessThan, true, true)]
    [TestCase(-1, null, CompareKind.LessThan, false, false)]
    [TestCase(0, null, CompareKind.LessThan, false, false)]
    [TestCase(1, null, CompareKind.LessThan, false, false)]
    [TestCase(-1, null, CompareKind.LessThan, true, true)]
    [TestCase(0, null, CompareKind.LessThan, true, false)]
    [TestCase(1, null, CompareKind.LessThan, true, false)]
    public void TestFilter_NullAsDefaultValue(object v1, object v2, CompareKind kind, bool nullAsDefaultValue, bool wantedRes)
    {
      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", v1);
      vals.Add("F2", v2);
      CompareFilter sut = new CompareFilter("F1", "F2", kind, nullAsDefaultValue, DBxColumnType.Int32);
      bool res = sut.TestFilter(vals);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase(0, null, CompareKind.Equal, DBxColumnType.Unknown, false)]
    [TestCase(0, null, CompareKind.Equal, DBxColumnType.Int32, true)]
    public void TestFilter_CompareKind(object v1, object v2, CompareKind kind, DBxColumnType columnType, bool wantedRes)
    {
      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", v1);
      vals.Add("F2", v2);
      CompareFilter sut = new CompareFilter("F1", "F2", kind, true, columnType);
      bool res = sut.TestFilter(vals);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      CompareFilter sut = new CompareFilter("F1", "F2", CompareKind.GreaterThan, true, DBxColumnType.Int32);
      byte[] b = SerializationTools.SerializeBinary(sut);
      CompareFilter res = (CompareFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
      Assert.AreEqual(sut.Expression1, res.Expression1, "Expression1");
      Assert.AreEqual(sut.Expression2, res.Expression2, "Expression2");
      Assert.AreEqual(sut.Kind, res.Kind, "Kind");
      Assert.AreEqual(sut.NullAsDefaultValue, res.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.AreEqual(sut.ColumnType, res.ColumnType, "ColumnType");
    }

    #endregion
  }
}
