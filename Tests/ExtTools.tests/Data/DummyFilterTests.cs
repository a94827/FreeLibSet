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
  public class DummyFilterTests
  {
    #region Конструктор

    [TestCase(true, DBxFilterDegeneration.AlwaysTrue)]
    [TestCase(false, DBxFilterDegeneration.AlwaysFalse)]
    public void Constructor(bool isTrue, DBxFilterDegeneration wantedDegeneration)
    {
      DummyFilter sut = new DummyFilter(isTrue);
      Assert.AreEqual(isTrue, sut.IsTrue, "IsTrue");
      Assert.AreEqual(wantedDegeneration, sut.Degeneration, "Degeneration");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void GetColumnNames()
    {
      DummyFilter sut = new DummyFilter(true);
      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);
      Assert.AreEqual(0, lst.Count);
    }

    [Test]
    public void GetAllFilters()
    {
      DummyFilter sut = new DummyFilter(true);
      List<DBxFilter> lst = new List<DBxFilter>();
      sut.GetAllFilters(lst);
      CollectionAssert.AreEqual(new DBxFilter[1] { sut }, lst);
    }

    #endregion

    #region TestFilter()

    [TestCase(true)]
    [TestCase(false)]
    public void TestFilter(bool isTrue)
    {
      DummyFilter sut = new DummyFilter(isTrue);

      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);

      bool res = sut.TestFilter(vals);
      Assert.AreEqual(isTrue, res);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DummyFilter sut = new DummyFilter(true);
      byte[] b = SerializationTools.SerializeBinary(sut);
      DummyFilter res = (DummyFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
    }

    #endregion
  }
}
