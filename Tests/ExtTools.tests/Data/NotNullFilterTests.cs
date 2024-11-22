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

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class NotNullFilterTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_Expression()
    {
      DBxExpression expr = new DBxColumn("F1");
      NotNullFilter sut = new NotNullFilter(expr, DBxColumnType.Int);
      Assert.AreSame(expr, sut.Expression1, "Expression1");
      Assert.IsInstanceOf<DBxConst>(sut.Expression2, "Expression2.GetType()");
      Assert.IsNull(sut.Expression2.GetConst().Value, "Expression2.value");
      Assert.AreEqual(DBxColumnType.Int, sut.Expression2.GetConst().ColumnType, "Expression2.ColumnType");
      Assert.IsTrue(sut.ComparisionToNull, "ComparisionToNull");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
      Assert.IsFalse(sut.NullAsDefaultValue, "NullAsDefaultValue");
    }

    [Test]
    public void Constructor_ColumnName()
    {
      NotNullFilter sut = new NotNullFilter("F1", DBxColumnType.Int);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression1, "Expression1.GetType()");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression1)).ColumnName, "Expression1.ColumnName");
      Assert.IsInstanceOf<DBxConst>(sut.Expression2, "Expression2.GetType()");
      Assert.IsNull(sut.Expression2.GetConst().Value, "Expression2.value");
      Assert.AreEqual(DBxColumnType.Int, sut.Expression2.GetConst().ColumnType, "Expression2.ColumnType");
      Assert.IsTrue(sut.ComparisionToNull, "ComparisionToNull");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
      Assert.IsFalse(sut.NullAsDefaultValue, "NullAsDefaultValue");
    }

    [Test]
    public void Constructor_ColumnName_Type()
    {
      NotNullFilter sut = new NotNullFilter("F1", typeof(int));
      Assert.IsInstanceOf<DBxColumn>(sut.Expression1, "Expression1.GetType()");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression1)).ColumnName, "Expression1.ColumnName");
      Assert.IsInstanceOf<DBxConst>(sut.Expression2, "Expression2.GetType()");
      Assert.IsNull(sut.Expression2.GetConst().Value, "Expression2.value");
      Assert.AreEqual(DBxColumnType.Int, sut.Expression2.GetConst().ColumnType, "Expression2.ColumnType");
      Assert.IsTrue(sut.ComparisionToNull, "ComparisionToNull");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
      Assert.IsFalse(sut.NullAsDefaultValue, "NullAsDefaultValue");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void GetColumnNames()
    {
      DBxExpression expr = new DBxColumn("F1");
      NotNullFilter sut = new NotNullFilter(expr, DBxColumnType.Int);

      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);

      CollectionAssert.AreEquivalent(new string[] { "F1" }, lst);
    }

    #endregion

    #region TestFilter()

    [TestCase(123, true)]
    [TestCase(0, true, Description ="Zero is not NULL")]
    [TestCase(null, false)]
    public void TestFilter(object colValue, bool wantedRes)
    {
      NotNullFilter sut = new NotNullFilter("F1", DBxColumnType.Int);

      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", colValue);

      bool res = sut.TestFilter(vals);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      NotNullFilter sut = new NotNullFilter("F1", DBxColumnType.Int);
      byte[] b = SerializationTools.SerializeBinary(sut);
      NotNullFilter res = (NotNullFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
    }

    #endregion
  }
}
