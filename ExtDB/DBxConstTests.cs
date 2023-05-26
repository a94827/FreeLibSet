using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.Remoting;
using System.Data;

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DBxConstTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_1()
    {
      DoConstrcuctor_1("ABC", DBxColumnType.String);
      DoConstrcuctor_1(1, DBxColumnType.Int);
      DoConstrcuctor_1(123.4, DBxColumnType.Float);
      DoConstrcuctor_1(123m, DBxColumnType.Money);
      DoConstrcuctor_1(true, DBxColumnType.Boolean);
      DoConstrcuctor_1(new DateTime(2023, 4, 27), DBxColumnType.Date);
      DoConstrcuctor_1(new DateTime(2023, 4, 27, 12, 34, 56), DBxColumnType.DateTime);
      DoConstrcuctor_1(new Guid("62a83186-d410-4c44-a420-d3e656f00188"), DBxColumnType.Guid);
    }

    private static void DoConstrcuctor_1(object value, DBxColumnType columnType)
    {
      DBxConst sut = new DBxConst(value);
      Assert.AreEqual(value, sut.Value, "Value - " + columnType.ToString());
      Assert.AreEqual(columnType, sut.ColumnType, "ColumnType");
    }

    [Test]
    public void Constructor_1_exception()
    {
      Assert.Catch<ArgumentException>(delegate () { new DBxConst(null); }, "null");
      Assert.Catch<ArgumentException>(delegate () { new DBxConst(DBNull.Value); }, "DBNull");
    }

    [Test]
    public void Constructor_2_normal()
    {
      DBxConst sut = new DBxConst("ABC", DBxColumnType.String);
      Assert.AreEqual("ABC", sut.Value, "Value");
      Assert.AreEqual(DBxColumnType.String, sut.ColumnType, "ColumnType");
    }

    [Test]
    public void Constructor_2_null()
    {
      DBxConst sut = new DBxConst(null, DBxColumnType.String);
      Assert.IsNull(sut.Value, "Value");
      Assert.AreEqual(DBxColumnType.String, sut.ColumnType, "ColumnType");
    }

    [Test]
    public void Constructor_2_DBNull()
    {
      DBxConst sut = new DBxConst(DBNull.Value, DBxColumnType.String);
      Assert.IsNull(sut.Value, "Value");
      Assert.AreEqual(DBxColumnType.String, sut.ColumnType, "ColumnType");
    }

    [Test]
    public void Constructor_2_UnknownType()
    {
      DBxConst sut = new DBxConst("ABC", DBxColumnType.Unknown);
      Assert.AreEqual("ABC", sut.Value, "Value");
      Assert.AreEqual(DBxColumnType.String, sut.ColumnType, "ColumnType");
    }

    [Test]
    public void Constructor_2_exception()
    {
      Assert.Catch(delegate () { new DBxConst(null, DBxColumnType.Unknown); }, "null");
      Assert.Catch(delegate () { new DBxConst(DBNull.Value, DBxColumnType.Unknown); }, "DBNull");
    }

    #endregion

    #region Методы

    [TestCase(123, 123, true)]
    [TestCase(123, 456, false)]
    public void Equals_sameType(int v1, int v2, bool wantedRes)
    {
      DoEquals(new DBxConst(v1, DBxColumnType.Int), new DBxConst(v2, DBxColumnType.Int), wantedRes);
    }

    [Test]
    public void Equals_diffType()
    {
      DoEquals(new DBxConst(0, DBxColumnType.Int), new DBxConst(0, DBxColumnType.Float), false);
    }

    private static void DoEquals(DBxConst obj1, DBxConst obj2, bool wantedRes)
    {
      Assert.AreEqual(wantedRes, obj1.Equals(obj2), "Equals");
      Assert.AreEqual(wantedRes, obj1 == obj2, "==");
      Assert.AreEqual(!wantedRes, obj1 != obj2, "!=");
    }


    [Test]
    public void GetColumnNames()
    {
      DBxConst sut = new DBxConst(123);
      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);
      Assert.AreEqual(0, lst.Count);
    }

    [Test]
    public void GetConst()
    {
      DBxConst sut = new DBxConst(123);
      Assert.AreEqual(sut, sut.GetConst());
    }

    [TestCase(123, false, 123)]
    [TestCase(123, true, 123)]
    [TestCase(null, false, null)]
    [TestCase(null, true, 0)]
    public void GetValue(object value, bool nullAsDefaultValue, object wantedValue)
    {
      DummyNamedValues dummy = new DummyNamedValues();
      DBxConst sut = new DBxConst(value, DBxColumnType.Int);
      object res = sut.GetValue(dummy, nullAsDefaultValue);
      Assert.AreEqual(wantedValue, res);
    }

    [Test]
    public void SetColumnNamePrefix()
    {
      DBxConst sut = new DBxConst(123);
      DBxExpression res = sut.SetColumnNamePrefix("AAA");
      Assert.AreSame(sut, res);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DBxConst sut = new DBxConst(123, DBxColumnType.Money);
      byte[] b = SerializationTools.SerializeBinary(sut);
      DBxConst res = (DBxConst)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(123, res.Value, "Value");
      Assert.AreEqual(DBxColumnType.Money, res.ColumnType, "ColumnType");
    }

    #endregion
  }
}
