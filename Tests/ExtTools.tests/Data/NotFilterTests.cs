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
  public class NotFilterTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      ValueFilter baseFilter = new ValueFilter("F1", 1);
      NotFilter sut = new NotFilter(baseFilter);
      Assert.AreSame(baseFilter, sut.BaseFilter, "BaseFilter");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    #endregion

    #region Degeneration

    [TestCase(true, DBxFilterDegeneration.AlwaysFalse)]
    [TestCase(false, DBxFilterDegeneration.AlwaysTrue)]
    public void Degeneration(bool isTrue, DBxFilterDegeneration wantedRes)
    {
      DummyFilter baseFilter = new DummyFilter(isTrue);
      NotFilter sut = new NotFilter(baseFilter);
      Assert.AreEqual(wantedRes, sut.Degeneration);
    }

    #endregion

    #region Прочие методы

    [Test]
    public void GetColumnNames()
    {
      DBxFilter baseFilter = new AndFilter(new ValueFilter("F1", 1), new ValueFilter("F2", 2));
      NotFilter sut = new NotFilter(baseFilter);

      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);

      CollectionAssert.AreEquivalent(new string[] { "F1", "F2" }, lst);
    }

    [Test]
    public void GetAllFilters()
    {
      DBxFilter f1 = new ValueFilter("F1", 1);
      DBxFilter f2 = new ValueFilter("F2", 2);
      DBxFilter baseFilter = new AndFilter(f1, f2);
      NotFilter sut = new NotFilter(baseFilter);

      List<DBxFilter> lst = new List<DBxFilter>();
      sut.GetAllFilters(lst);

      CollectionAssert.AreEqual(new DBxFilter[] { sut, baseFilter, f1, f2 }, lst);
    }

    #endregion

    #region TestFilter

    [TestCase(0, false)]
    [TestCase(1, true)]
    public void TestFilter(int v1, bool wantedRes)
    {
      ValueFilter baseFilter = new ValueFilter("F1", "0");
      NotFilter sut = new NotFilter(baseFilter);

      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", v1);

      bool res = sut.TestFilter(vals);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      ValueFilter baseFilter = new ValueFilter("F1", "0");
      NotFilter sut = new NotFilter(baseFilter);
      byte[] b = SerializationTools.SerializeBinary(sut);
      NotFilter res = (NotFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
    }

    #endregion
  }
}
