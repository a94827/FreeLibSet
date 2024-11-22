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
  public class IdsFilterTests
  {
    #region Конструкторы с DBxExpression

    [Test]
    public void Constructor_expr_IdList()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      IdList ids = new IdList();
      ids.Add(1);
      ids.Add(2);
      IdsFilter sut = new IdsFilter(expr1, ids);
      Assert.AreSame(expr1, sut.Expression, "Expression");
      Assert.AreSame(ids, sut.Ids, "Ids");
      Assert.IsTrue(sut.Ids.IsReadOnly, "Ids.IsReadOnly");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_expr_array()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      Int32[] ids = new Int32[] { 1, 2 };
      IdsFilter sut = new IdsFilter(expr1, ids);
      Assert.AreSame(expr1, sut.Expression, "Expression");
      Assert.AreEqual(2, sut.Ids.Count, "Ids.Count");
      Assert.IsTrue(sut.Ids.IsReadOnly, "Ids.IsReadOnly");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_expr_id()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      IdsFilter sut = new IdsFilter(expr1, 123);
      Assert.AreSame(expr1, sut.Expression, "Expression");
      Assert.AreEqual(1, sut.Ids.Count, "Ids.Count");
      Assert.AreEqual(123, sut.Ids.SingleId, "Ids[0]");
      Assert.IsTrue(sut.Ids.IsReadOnly, "Ids.IsReadOnly");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    #endregion

    #region Конструкторы с именем поля

    [Test]
    public void Constructor_colName_IdList()
    {
      IdList ids = new IdList();
      ids.Add(1);
      ids.Add(2);
      ids.Add(3);
      IdsFilter sut = new IdsFilter("F1", ids);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression, "Expression type");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression)).ColumnName, "ColumnName");
      Assert.AreSame(ids, sut.Ids, "Ids");
      Assert.IsTrue(sut.Ids.IsReadOnly, "Ids.IsReadOnly");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_colName_array()
    {
      Int32[] ids = new Int32[] { 1, 2 };
      IdsFilter sut = new IdsFilter("F1", ids);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression, "Expression type");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression)).ColumnName, "ColumnName");
      Assert.AreEqual(2, sut.Ids.Count, "Ids.Count");
      Assert.IsTrue(sut.Ids.IsReadOnly, "Ids.IsReadOnly");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_colName_id()
    {
      IdsFilter sut = new IdsFilter("F1", 123);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression, "Expression type");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression)).ColumnName, "ColumnName");
      Assert.AreEqual(1, sut.Ids.Count, "Ids.Count");
      Assert.AreEqual(123, sut.Ids.SingleId, "Ids[0]");
      Assert.IsTrue(sut.Ids.IsReadOnly, "Ids.IsReadOnly");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    #endregion

    #region Конструкторы для поля Id

    [Test]
    public void Constructor_colId_IdList()
    {
      IdList ids = new IdList();
      ids.Add(1);
      ids.Add(2);
      ids.Add(3);
      ids.Add(4);
      IdsFilter sut = new IdsFilter(ids);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression, "Expression type");
      Assert.AreEqual("Id", ((DBxColumn)(sut.Expression)).ColumnName, "ColumnName");
      Assert.AreSame(ids, sut.Ids, "Ids");
      Assert.IsTrue(sut.Ids.IsReadOnly, "Ids.IsReadOnly");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_colId_array()
    {
      Int32[] ids = new Int32[] { 1, 2, 3 };
      IdsFilter sut = new IdsFilter(ids);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression, "Expression type");
      Assert.AreEqual("Id", ((DBxColumn)(sut.Expression)).ColumnName, "ColumnName");
      Assert.AreEqual(3, sut.Ids.Count, "Ids.Count");
      Assert.IsTrue(sut.Ids.IsReadOnly, "Ids.IsReadOnly");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    [Test]
    public void Constructor_colId_id()
    {
      IdsFilter sut = new IdsFilter(123);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression, "Expression type");
      Assert.AreEqual("Id", ((DBxColumn)(sut.Expression)).ColumnName, "ColumnName");
      Assert.AreEqual(1, sut.Ids.Count, "Ids.Count");
      Assert.AreEqual(123, sut.Ids.SingleId, "Ids[0]");
      Assert.IsTrue(sut.Ids.IsReadOnly, "Ids.IsReadOnly");
      Assert.AreEqual(DBxFilterDegeneration.None, sut.Degeneration, "Degeneration");
    }

    #endregion

    #region Дополнительные проверки конструкторов

    [Test]
    public void Constructor_exceptions()
    {
      Assert.Catch(delegate () { new IdsFilter(IdList.Empty); }, "IdList.Empty");
      Assert.Catch(delegate () { new IdsFilter(DataTools.EmptyIds); }, "Int32[0]");
      Assert.Catch(delegate () { new IdsFilter(0); }, "id=0");
    }

    #endregion

    #region TestFilter()

    [TestCase(1, true)]
    [TestCase(2, false)]
    [TestCase(0, false)]
    [TestCase(null, false)]
    public void TestFilter(object value, bool wantedRes)
    {
      IdsFilter sut = new IdsFilter(new Int32[] { 1, 3, 5 });

      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("Id", value);
      bool res = sut.TestFilter(vals);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      IdsFilter sut = new IdsFilter("F1", new Int32[] { 1, 3, 5 });
      byte[] b = SerializationTools.SerializeBinary(sut);
      IdsFilter res = (IdsFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
      Assert.AreEqual(sut.Expression, res.Expression, "Expression");
      Assert.AreEqual(sut.Ids, res.Ids, "Ids");
      Assert.IsTrue(res.Ids.IsReadOnly, "Ids.IsReadOnly");
    }

    #endregion
  }
}
