using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.Remoting;
using System.Data;

namespace ExtTools_tests.Data
{
  [TestFixture]
  class DBxColumnTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DBxColumn sut = new DBxColumn("F1");
      Assert.AreEqual("F1", sut.ColumnName, "ColumnName");
    }

    [Test]
    public void Constructor_exception()
    {
      Assert.Catch<ArgumentException>(delegate () { new DBxColumn(""); }, "Empty string");
      Assert.Catch<ArgumentException>(delegate () { new DBxColumn("F1,F2"); }, "Comma");
    }

    #endregion

    #region Методы

    [TestCase("F1", "F1", true)]
    [TestCase("F1", "F2", false)]
    public void Equals(string name1, string name2, bool wantedRes)
    {
      DBxColumn col1 = new DBxColumn(name1);
      DBxColumn col2 = new DBxColumn(name2);
      Assert.AreEqual(wantedRes, col1.Equals(col2), "Equals");
      Assert.AreEqual(wantedRes, col1 == col2, "==");
      Assert.AreEqual(!wantedRes, col1 != col2, "!=");
    }

    [Test]
    public void GetColumnNames()
    {
      DBxColumn sut = new DBxColumn("F1");
      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);
      Assert.AreEqual("F1", lst.AsString);
    }

    [Test]
    public void GetConst()
    {
      DBxColumn sut = new DBxColumn("F1");
      Assert.IsNull(sut.GetConst());
    }

    [TestCase("F1", false, "AAA")]
    [TestCase("F1", true, null)]
    [TestCase("F2", false, 222)]
    [TestCase("F2", true, null)]
    [TestCase("F3", false, true)]
    [TestCase("F3", true, null)]
    public void GetValue(string columnName, bool srcNull, object wantedValue)
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string));
      table.Columns.Add("F2", typeof(int));
      table.Columns.Add("F3", typeof(bool));
      table.Rows.Add("AAA", 222, true);
      table.Rows.Add();
      DataTableValues va = new DataTableValues(table);
      va.CurrentRow = table.Rows[srcNull ? 1 : 0];

      DBxColumn sut = new DBxColumn(columnName);
      object res = sut.GetValue(va);
      Assert.AreEqual(wantedValue, res);
    }

    [Test]
    public void SetColumnNamePrefix()
    {
      DBxColumn sut = new DBxColumn("F2");
      DBxColumn res = (DBxColumn)(sut.SetColumnNamePrefix("F1."));
      Assert.AreEqual("F1.F2", res.ColumnName);
    }

    [Test]
    public void GetAllExpressions()
    {
      DBxColumn sut = new DBxColumn("F2");
      List<DBxExpression> lst = new List<DBxExpression>();
      sut.GetAllExpressions(lst);
      CollectionAssert.AreEqual(new DBxExpression[1] { sut }, lst);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DBxColumn sut = new DBxColumn("F1");
      byte[] b = SerializationTools.SerializeBinary(sut);
      DBxColumn res = (DBxColumn)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual("F1", res.ColumnName);
    }

    #endregion
  }
}
