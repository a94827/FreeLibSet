using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Remoting;
using FreeLibSet.Core;
using FreeLibSet.Data;
using System.ComponentModel;

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DBxOrderTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_PartArray()
    {
      DBxOrderPart p1 = new DBxOrderPart("F1");
      DBxOrderPart p2 = new DBxOrderPart("F2", ListSortDirection.Descending);
      DBxOrderPart p3 = new DBxOrderPart("F3");
      DBxOrder sut = new DBxOrder(p1, p2, p3);
      Assert.AreEqual(3, sut.Parts.Length, "Length");
      Assert.AreEqual(p1, sut.Parts[0], "Parts[0]");
      Assert.AreEqual(p2, sut.Parts[1], "Parts[1]");
      Assert.AreEqual(p3, sut.Parts[2], "Parts[2]");
    }

    [Test]
    public void Constructor_PartCollection()
    {
      List<DBxOrderPart> lst = new List<DBxOrderPart>();
      lst.Add(new DBxOrderPart("F1"));
      lst.Add(new DBxOrderPart("F2", ListSortDirection.Descending));
      lst.Add(new DBxOrderPart("F3"));
      DBxOrder sut = new DBxOrder(lst);

      CollectionAssert.AreEqual(lst, sut.Parts);
    }

    [Test]
    public void Constructor_ExpressionCollection()
    {
      List<DBxExpression> lst = new List<DBxExpression>();
      lst.Add(new DBxColumn("F1"));
      lst.Add(new DBxColumn("F2"));
      DBxOrder sut = new DBxOrder(lst);

      List<DBxOrderPart> wanted = new List<DBxOrderPart>();
      wanted.Add(new DBxOrderPart("F1"));
      wanted.Add(new DBxOrderPart("F2"));
      CollectionAssert.AreEqual(wanted, sut.Parts);
    }

    [Test]
    public void Constructor_ExpressionArray()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      DBxExpression expr2 = new DBxColumn("F2");
      DBxOrder sut = new DBxOrder(expr1, expr2);

      List<DBxOrderPart> wanted = new List<DBxOrderPart>();
      wanted.Add(new DBxOrderPart("F1"));
      wanted.Add(new DBxOrderPart("F2"));
      CollectionAssert.AreEqual(wanted, sut.Parts);
    }

    [Test]
    public void Constructor_ColName_Direction()
    {
      DBxOrder sut = new DBxOrder("F1", ListSortDirection.Descending);

      List<DBxOrderPart> wanted = new List<DBxOrderPart>();
      wanted.Add(new DBxOrderPart("F1", ListSortDirection.Descending));
      CollectionAssert.AreEqual(wanted, sut.Parts);
    }

    [Test]
    public void Constructor_ColName()
    {
      DBxOrder sut = new DBxOrder("F1");

      List<DBxOrderPart> wanted = new List<DBxOrderPart>();
      wanted.Add(new DBxOrderPart("F1", ListSortDirection.Ascending));
      CollectionAssert.AreEqual(wanted, sut.Parts);
    }


    [Test]
    public void Constructor_Expression_Direction()
    {
      DBxExpression expr = new DBxColumn("F1");
      DBxOrder sut = new DBxOrder(expr, ListSortDirection.Descending);

      List<DBxOrderPart> wanted = new List<DBxOrderPart>();
      wanted.Add(new DBxOrderPart("F1", ListSortDirection.Descending));
      CollectionAssert.AreEqual(wanted, sut.Parts);
    }

    [Test]
    public void Constructor_Expression()
    {
      DBxExpression expr = new DBxColumn("F1");
      DBxOrder sut = new DBxOrder(expr);

      List<DBxOrderPart> wanted = new List<DBxOrderPart>();
      wanted.Add(new DBxOrderPart("F1", ListSortDirection.Ascending));
      CollectionAssert.AreEqual(wanted, sut.Parts);
    }

    #endregion

    #region GetFirstColumnInfo()

    [TestCase("F1", "F1", ListSortDirection.Ascending)]
    [TestCase("F1 DESC, F2", "F1", ListSortDirection.Descending)]
    public void GetFirstColumnInfo(string sortText, string wantedColumnName, ListSortDirection wantedSortOrder)
    {
      DBxOrder sut = DBxOrder.FromDataViewSort(sortText);

      string columnName;
      ListSortDirection sortOrder;
      sut.GetFirstColumnInfo(out columnName, out sortOrder);

      Assert.AreEqual(wantedColumnName, columnName, "ColumnName");
      Assert.AreEqual(wantedSortOrder, sortOrder, "SortOrder");
    }

    [Test]
    public void GetColumnNames()
    {
      DBxOrder sut = DBxOrder.FromDataViewSort("F1 DESC,F2");

      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);

      CollectionAssert.AreEqual(new string[] { "F1", "F2" }, lst);
    }

    [Test]
    public void SetColumnNamePrefix()
    {
      DBxOrder sut = DBxOrder.FromDataViewSort("F1 DESC,F2");
      DBxOrder res = sut.SetColumnNamePrefix("ABC");

      List<DBxOrderPart> wantedParts = new List<DBxOrderPart>();
      wantedParts.Add(new DBxOrderPart("ABCF1", ListSortDirection.Descending));
      wantedParts.Add(new DBxOrderPart("ABCF2", ListSortDirection.Ascending));
      CollectionAssert.AreEquivalent(wantedParts, res.Parts);
    }

    #endregion

    #region FromDataViewSort()

    [Test]
    public void FromDataViewSort_1arg()
    {
      DBxOrder sut = DBxOrder.FromDataViewSort("F1 DESC,F2");

      List<DBxOrderPart> wantedParts = new List<DBxOrderPart>();
      wantedParts.Add(new DBxOrderPart("F1", ListSortDirection.Descending));
      wantedParts.Add(new DBxOrderPart("F2", ListSortDirection.Ascending));
      CollectionAssert.AreEquivalent(wantedParts, sut.Parts);
    }

    [Test]
    public void FromDataViewSort_2args()
    {
      DBxOrder sut = DBxOrder.FromDataViewSort("F1 DESC,F2", "ABC");

      List<DBxOrderPart> wantedParts = new List<DBxOrderPart>();
      wantedParts.Add(new DBxOrderPart("ABC.F1", ListSortDirection.Descending));
      wantedParts.Add(new DBxOrderPart("ABC.F2", ListSortDirection.Ascending));
      CollectionAssert.AreEquivalent(wantedParts, sut.Parts);
    }

    [Test]
    public void FromDataViewSort_emptyString()
    {
      Assert.IsNull(DBxOrder.FromDataViewSort(""), "1 arg");
      Assert.IsNull(DBxOrder.FromDataViewSort("", "ABC"), "2 args");
    }

    #endregion

    #region FromColumns

    [Test]
    public void FromColumns_normal()
    {
      DBxColumns columns = new DBxColumns("F1,F2,F3");
      DBxOrder res = DBxOrder.FromColumns(columns);
      Assert.AreEqual(3, res.Parts.Length, "Length");

      Assert.IsInstanceOf<DBxColumn>(res.Parts[0].Expression, "Expression #1");
      Assert.AreEqual("F1", ((DBxColumn)(res.Parts[0].Expression)).ColumnName, "ColumnName #1");
      Assert.AreEqual(ListSortDirection.Ascending, res.Parts[0].SortOrder, "SortOrder #1");

      Assert.IsInstanceOf<DBxColumn>(res.Parts[1].Expression, "Expression #2");
      Assert.AreEqual("F2", ((DBxColumn)(res.Parts[1].Expression)).ColumnName, "ColumnName #2");
      Assert.AreEqual(ListSortDirection.Ascending, res.Parts[1].SortOrder, "SortOrder #2");

      Assert.IsInstanceOf<DBxColumn>(res.Parts[2].Expression, "Expression #3");
      Assert.AreEqual("F3", ((DBxColumn)(res.Parts[2].Expression)).ColumnName, "ColumnName #3");
      Assert.AreEqual(ListSortDirection.Ascending, res.Parts[2].SortOrder, "SortOrder #3");
    }

    [Test]
    public void FromColumns_null()
    {
      Assert.IsNull(DBxOrder.FromColumns(DBxColumns.Empty), "DBxColumns.Empty");

      DBxColumns columns2 = null;
      Assert.IsNull(DBxOrder.FromColumns(columns2), "null");
    }

    #endregion

      #region ById

    [Test]
    public void ById()
    {
      List<DBxOrderPart> wantedParts = new List<DBxOrderPart>();
      wantedParts.Add(new DBxOrderPart("Id", ListSortDirection.Ascending));
      CollectionAssert.AreEquivalent(wantedParts, DBxOrder.ById.Parts);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DBxOrder sut = DBxOrder.FromDataViewSort("F1,F2 DESC");
      byte[] b = SerializationTools.SerializeBinary(sut);
      DBxOrder res = (DBxOrder)(SerializationTools.DeserializeBinary(b));
      CollectionAssert.AreEqual(sut.Parts, res.Parts);
    }

    #endregion
  }
}
