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
  public class StringValueFilterTests
  {
    #region Конструкторы

    [TestCase("AbC", false)]
    [TestCase("AbC", true)]
    [TestCase("", false)]
    public void Constructor_Expression(string value, bool ignoreCase)
    {
      DBxExpression expr = new DBxColumn("F1");
      StringValueFilter sut = new StringValueFilter(expr, value, ignoreCase);
      Assert.AreSame(expr, sut.Expression, "Expression");
      Assert.AreEqual(value, sut.Value, "Value");
      Assert.AreEqual(ignoreCase, sut.IgnoreCase, "IgnoreCase");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_ColumnName_3args()
    {
      StringValueFilter sut = new StringValueFilter("F1", "AbC", true);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression, "Expression.GetType()");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression)).ColumnName, "Expression.ColumnName");
      Assert.AreEqual("AbC", sut.Value, "Value");
      Assert.IsTrue(sut.IgnoreCase, "IgnoreCase");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_ColumnName_2args()
    {
      StringValueFilter sut = new StringValueFilter("F1", "AbC");
      Assert.IsInstanceOf<DBxColumn>(sut.Expression, "Expression.GetType()");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression)).ColumnName, "Expression.ColumnName");
      Assert.AreEqual("AbC", sut.Value, "Value");
      Assert.IsFalse(sut.IgnoreCase, "IgnoreCase");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void GetColumnNames()
    {
      DBxExpression expr = new DBxColumn("F1");
      StringValueFilter sut = new StringValueFilter(expr, "ABC", true);

      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);

      CollectionAssert.AreEquivalent(new string[] { "F1" }, lst);
    }

    #endregion

    #region TestFilter()

    [TestCase("AbC", false, "aBc", false)]
    [TestCase("AbC", true, "aBc", true)]
    [TestCase("AbC", false, "", false)]
    [TestCase("AbC", false, null, false)]
    [TestCase("", false, "aBc", false)]
    [TestCase("", false, "", true)]
    [TestCase("", false, null, true)]
    public void TestFilter(string filterValue, bool ignoreCase, object colValue, bool wantedRes)
    {
      StringValueFilter sut = new StringValueFilter("F1", filterValue, ignoreCase);

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
      StringValueFilter sut = new StringValueFilter("F1", "ABC", true);
      byte[] b = SerializationTools.SerializeBinary(sut);
      StringValueFilter res = (StringValueFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
      Assert.AreEqual(sut.Value, res.Value, "Value");
      Assert.AreEqual(sut.IgnoreCase, res.IgnoreCase, "IgnoreCase");
    }

    #endregion
  }
}
