using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;

namespace ExtDB.tests
{
  [TestFixture]
  public class DBxTableColumnNameTests
  {
    [Test]
    public void Constructor_ok()
    {
      DBxTableColumnName item = new DBxTableColumnName("A", "B");

      Assert.AreEqual("A", item.TableName, "table code");
      Assert.AreEqual("B", item.ColumnName, "column code");
      Assert.IsFalse(item.IsEmpty, "IsEmpty=false");
    }

    [Test]
    public void Constructor_exception()
    {
      Assert.Catch<ArgumentException>(delegate() { new DBxTableColumnName("A", String.Empty); });
      Assert.Catch<ArgumentException>(delegate() { new DBxTableColumnName("A", null); });
      Assert.Catch<ArgumentException>(delegate() { new DBxTableColumnName(String.Empty, "B"); });
      Assert.Catch<ArgumentException>(delegate() { new DBxTableColumnName(null, "B"); });
    }

    [Test]
    public void Equals()
    {
      DBxTableColumnName item1 = new DBxTableColumnName("A", "B");
      DBxTableColumnName item2 = new DBxTableColumnName("A", "B");
      DBxTableColumnName item3 = new DBxTableColumnName("A", "D");
      DBxTableColumnName item4 = new DBxTableColumnName("C", "B");

      Assert.IsTrue(item1.Equals(item2), "Equal to itself");
      Assert.IsTrue(item1 == item2, "operator == to itself");
      Assert.IsFalse(item1.Equals(item3), "Not equal to another table name");
      Assert.IsFalse(item1.Equals(item4), "Not equal to another column name");
    }

    [Test]
    public void Equals_IsEmpty()
    {
      DBxTableColumnName item1 = new DBxTableColumnName("A", "B");
      Assert.IsFalse(item1.Equals(DBxTableColumnName.Empty), "Not equal to empty");
      Assert.IsFalse(DBxTableColumnName.Empty.Equals(item1), "Empty not equal to filled");
      Assert.IsTrue(DBxTableColumnName.Empty.Equals(DBxTableColumnName.Empty), "Empty is equal to itself");
    }
  }
}
