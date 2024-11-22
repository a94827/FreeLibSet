using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Collections;
using FreeLibSet.Remoting;
using FreeLibSet.Core;

// Выполняется тестирование класса фильтров вне связи с табличными данными

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class StartsWithFilterTests
  {
    #region Конструкторы

    [TestCase("AbC", false, DBxFilterDegeneration.None)]
    [TestCase("AbC", true, DBxFilterDegeneration.None)]
    [TestCase("", false, DBxFilterDegeneration.AlwaysTrue)]
    public void Constructor_Expression(string value, bool ignoreCase, DBxFilterDegeneration wantedDegeneration)
    {
      DBxExpression expr = new DBxColumn("F1");
      StartsWithFilter sut = new StartsWithFilter(expr, value, ignoreCase);
      Assert.AreSame(expr, sut.Expression, "Expression");
      Assert.AreEqual(value, sut.Value, "Value");
      Assert.AreEqual(ignoreCase, sut.IgnoreCase, "IgnoreCase");
      Assert.AreEqual(wantedDegeneration, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_ColumnName_3args()
    {
      StartsWithFilter sut = new StartsWithFilter("F1", "AbC", true);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression, "Expression.GetType()");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression)).ColumnName, "Expression.ColumnName");
      Assert.AreEqual("AbC", sut.Value, "Value");
      Assert.IsTrue(sut.IgnoreCase, "IgnoreCase");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_ColumnName_2args()
    {
      StartsWithFilter sut = new StartsWithFilter("F1", "AbC");
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
      StartsWithFilter sut = new StartsWithFilter(expr, "ABC", true);

      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);

      CollectionAssert.AreEquivalent(new string[] { "F1" }, lst);
    }

    #endregion

    #region TestFilter()

    [TestCase("AbC", false, "aBcd", false)]
    [TestCase("AbC", true, "aBcd", true)]
    [TestCase("AbC", false, "aBc", false)]
    [TestCase("AbC", true, "aBc", true)]
    [TestCase("AbC", false, "", false)]
    [TestCase("AbC", false, null, false)]
    [TestCase("", false, "aBc", true)]
    [TestCase("", false, "", true)]
    [TestCase("", false, null, true)]
    public void TestFilter(string filterValue, bool ignoreCase, object colValue, bool wantedRes)
    {
      StartsWithFilter sut = new StartsWithFilter("F1", filterValue, ignoreCase);

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
      StartsWithFilter sut = new StartsWithFilter("F1", "ABC", true);
      byte[] b = SerializationTools.SerializeBinary(sut);
      StartsWithFilter res = (StartsWithFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
      Assert.AreEqual(sut.Value, res.Value, "Value");
      Assert.AreEqual(sut.IgnoreCase, res.IgnoreCase, "IgnoreCase");
    }

    #endregion
  }
}
