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
  public class SubstringFilterTests
  {
    #region Конструкторы

    [TestCase(2, "AbC", false)]
    [TestCase(3, "AbC", true)]
    [TestCase(0, "AAA", false)]
    public void Constructor_Expression(int startIndex, string value, bool ignoreCase)
    {
      DBxExpression expr = new DBxColumn("F1");
      SubstringFilter sut = new SubstringFilter(expr, startIndex, value, ignoreCase);
      Assert.AreSame(expr, sut.Expression, "Expression");
      Assert.AreEqual(startIndex, sut.StartIndex, "StartIndex");
      Assert.AreEqual(value, sut.Value, "Value");
      Assert.AreEqual(ignoreCase, sut.IgnoreCase, "IgnoreCase");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_ColumnName_4args()
    {
      SubstringFilter sut = new SubstringFilter("F1", 2, "AbC", true);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression, "Expression.GetType()");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression)).ColumnName, "Expression.ColumnName");
      Assert.AreEqual(2, sut.StartIndex, "StartIndex");
      Assert.AreEqual("AbC", sut.Value, "Value");
      Assert.IsTrue(sut.IgnoreCase, "IgnoreCase");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_ColumnName_3args()
    {
      SubstringFilter sut = new SubstringFilter("F1", 1, "AbC");
      Assert.IsInstanceOf<DBxColumn>(sut.Expression, "Expression.GetType()");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression)).ColumnName, "Expression.ColumnName");
      Assert.AreEqual(1, sut.StartIndex, "StartIndex");
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
      SubstringFilter sut = new SubstringFilter(expr, 2, "ABC", true);

      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);

      CollectionAssert.AreEquivalent(new string[] { "F1" }, lst);
    }

    #endregion

    #region TestFilter()

    [TestCase(1, "bCd", false, "aBcDe", false)]
    [TestCase(1, "bCd", true, "aBcDe", true)]
    [TestCase(1, "BcD", false, "aBcDe", true)]
    [TestCase(0, "BcD", false, "aBcDe", false)]
    [TestCase(2, "BcD", false, "aBcDe", false)]
    [TestCase(4, "E", true, "aBcDe", true)]
    [TestCase(4, "eF", false, "aBcDe", false)]
    [TestCase(100, "XX", false, "aBcDe", false)]
    [TestCase(0, "X", true, "", false)]
    [TestCase(0, "X", true, null, false)]
    public void TestFilter(int startIndex, string filterValue, bool ignoreCase, object colValue, bool wantedRes)
    {
      SubstringFilter sut = new SubstringFilter("F1", startIndex, filterValue, ignoreCase);

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
      SubstringFilter sut = new SubstringFilter("F1", 1, "ABC", true);
      byte[] b = SerializationTools.SerializeBinary(sut);
      SubstringFilter res = (SubstringFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
      Assert.AreEqual(sut.StartIndex, res.StartIndex, "StartIndex");
      Assert.AreEqual(sut.Value, res.Value, "Value");
      Assert.AreEqual(sut.IgnoreCase, res.IgnoreCase, "IgnoreCase");
    }

    #endregion
  }
}
