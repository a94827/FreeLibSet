using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;
using System.Data;
using FreeLibSet.Data.SQLite;
using FreeLibSet.Remoting;

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DBxTableStructTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      Assert.AreEqual("Test1", sut.TableName, "TableName");
      Assert.AreEqual(0, sut.Columns.Count, "Columns");
      Assert.AreEqual(0, sut.AllColumnNames.Length, "AllColumnNames");
      Assert.IsTrue(sut.PrimaryKey.IsEmpty, "PrimaryKey");
      Assert.AreEqual(0, sut.PrimaryKeyColumns.Length, "PrimaryKeyColumns");
      Assert.IsTrue(sut.AutoPrimaryKey, "AutoPrimaryKey");
      Assert.AreEqual(0, sut.Indexes.Count, "Indexes");
      Assert.AreEqual("", sut.Comment, "Comment");
      Assert.IsFalse(sut.HasComments, "HasComments");
      Assert.IsTrue(sut.AutoCreate, "AutoCreate");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.DoesNotThrow(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
    }

    #endregion

    #region Тестовый объект

    private static DBxTableStruct CreateTestObject()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      sut.Columns.AddId();
      sut.Columns.AddString("F1", 10, false);
      sut.Columns.AddInt("F2", true);

      Assert.AreEqual(3, sut.Columns.Count, "Columns.Count");

      return sut;
    }

    #endregion

    #region Доступ к столбцам

    [Test]
    public void Columns_Item_GetByName()
    {
      DBxTableStruct sut = CreateTestObject();

      DBxColumnStruct res1 = sut.Columns["Id"];
      Assert.AreEqual("Id", res1.ColumnName, "ColumnName #1");
      Assert.AreEqual(DBxColumnType.Int, res1.ColumnType, "ColumnType #1");
      Assert.IsFalse(res1.Nullable, "Nullable #1");
      Assert.IsNull(res1.DefaultValue, "DefaultValue #1");

      DBxColumnStruct res2 = sut.Columns["F1"];
      Assert.AreEqual("F1", res2.ColumnName, "ColumnName #2");
      Assert.AreEqual(DBxColumnType.String, res2.ColumnType, "ColumnType #2");
      Assert.IsFalse(res2.Nullable, "Nullable #2");
      Assert.IsNull(res2.DefaultValue, "DefaultValue #2");

      DBxColumnStruct res3 = sut.Columns["XXX"];
      Assert.IsNull(res3, "#3");

      DBxColumnStruct res4 = sut.Columns[""];
      Assert.IsNull(res4, "#4");
    }

    [Test]
    public void Columns_Item_GetByIndex()
    {
      DBxTableStruct sut = CreateTestObject();

      DBxColumnStruct res1 = sut.Columns[0];
      Assert.AreEqual("Id", res1.ColumnName, "ColumnName #1");
      Assert.AreEqual(DBxColumnType.Int, res1.ColumnType, "ColumnType #1");
      Assert.IsFalse(res1.Nullable, "Nullable #1");
      Assert.IsNull(res1.DefaultValue, "DefaultValue #1");

      DBxColumnStruct res2 = sut.Columns[1];
      Assert.AreEqual("F1", res2.ColumnName, "ColumnName #2");
      Assert.AreEqual(DBxColumnType.String, res2.ColumnType, "ColumnType #2");
      Assert.IsFalse(res2.Nullable, "Nullable #2");
      Assert.IsNull(res2.DefaultValue, "DefaultValue #2");

      DBxColumnStruct res3;
      Assert.Catch(delegate () { res3 = sut.Columns[3]; }, "#3");

      DBxColumnStruct res4;
      Assert.Catch(delegate () { res4 = sut.Columns[-1]; }, "#4");
    }

    [Test]
    public void Columns_GetRequired()
    {
      DBxTableStruct sut = CreateTestObject();

      DBxColumnStruct res1 = sut.Columns.GetRequired("Id");
      Assert.AreEqual("Id", res1.ColumnName, "ColumnName #1");

      DBxColumnStruct res2 = sut.Columns.GetRequired("F1");
      Assert.AreEqual("F1", res2.ColumnName, "ColumnName #2");

      DBxColumnStruct res3;
      Assert.Catch(delegate () { res3 = sut.Columns.GetRequired("XXX"); }, "#3");

      DBxColumnStruct res4;
      Assert.Catch(delegate () { res4 = sut.Columns.GetRequired(""); }, "#4");
    }

    [Test]
    public void Columns_LastAdded()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      Assert.IsNull(sut.Columns.LastAdded, "#1");

      DBxColumnStruct col = sut.Columns.AddId();
      Assert.AreSame(col, sut.Columns.LastAdded, "#2");
    }

    [Test]
    public void AllColumnNames()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      CollectionAssert.AreEqual(new string[] { }, sut.AllColumnNames, "#0");

      sut.Columns.AddId();
      CollectionAssert.AreEqual(new string[] { "Id" }, sut.AllColumnNames, "#1");

      sut.Columns.AddString("F1", 10, false);
      CollectionAssert.AreEqual(new string[] { "Id", "F1" }, sut.AllColumnNames, "#2");
    }

    #endregion

    #region Методы добавления столбцов

    [Test]
    public void Columns_AddId()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddId();
      Assert.AreEqual("Id", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.IsFalse(res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Int32), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddId_withColumnName()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddId("F1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.IsFalse(res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Int32), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddString([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddString("F1", 123, nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.String, res.ColumnType, "ColumnType");
      Assert.AreEqual(123, res.MaxLength, "MaxLength");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(String), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddDate([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddDate("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Date, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(DateTime), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddDateTime([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddDateTime("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.DateTime, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(DateTime), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddTime([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddTime("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Time, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(TimeSpan), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddInt()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt("F1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.IsTrue(res.Nullable, "Nullable");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(0.0, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Int32), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddInt_nullable([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(0.0, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Int32), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddInt_int_int()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt("F1", 0, 255);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.IsTrue(res.Nullable, "Nullable");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(255.0, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(byte), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddInt_int_int_nullable([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt("F1", 0, 127, nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(127.0, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(byte), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddInt_MinMaxInt()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt("F1", new MinMax<int>(0, 255));
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.IsTrue(res.Nullable, "Nullable");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(255, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(byte), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddInt_MinMaxInt_nullable([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt("F1", new MinMax<int>(0, 255), nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(255, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(byte), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddInt_long_long()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt("F1", 0L, 10000000000L);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.IsTrue(res.Nullable, "Nullable");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(1e10, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(long), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddInt_long_long_nullable([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt("F1", 0L, 10000000000L, nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(1e10, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(long), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddInt16()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt16("F1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.IsTrue(res.Nullable, "Nullable");
      Assert.AreEqual(-32768.0, res.MinValue, "MinValue");
      Assert.AreEqual(32767.0, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Int16), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddInt16_nullable([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt16("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.AreEqual(-32768.0, res.MinValue, "MinValue");
      Assert.AreEqual(32767.0, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Int16), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddInt64()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt64("F1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.IsTrue(res.Nullable, "Nullable");
      Assert.AreEqual((double)(Int64.MinValue), res.MinValue, "MinValue");
      Assert.AreEqual((double)(Int64.MaxValue), res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Int64), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddInt64_nullable([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt64("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.AreEqual((double)(Int64.MinValue), res.MinValue, "MinValue");
      Assert.AreEqual((double)(Int64.MaxValue), res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Int64), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddMoney()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddMoney("F1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Money, res.ColumnType, "ColumnType");
      Assert.IsTrue(res.Nullable, "Nullable");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(0.0, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Decimal), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddMoney_nullable([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddMoney("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Money, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(0.0, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Decimal), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddSingle()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddSingle("F1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Float, res.ColumnType, "ColumnType");
      Assert.IsTrue(res.Nullable, "Nullable");
      Assert.AreEqual((double)(Single.MinValue), res.MinValue, "MinValue");
      Assert.AreEqual((double)(Single.MaxValue), res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Single), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddSingle_nullable([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddSingle("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Float, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.AreEqual((double)(Single.MinValue), res.MinValue, "MinValue");
      Assert.AreEqual((double)(Single.MaxValue), res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Single), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddDouble()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddDouble("F1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Float, res.ColumnType, "ColumnType");
      Assert.IsTrue(res.Nullable, "Nullable");
      Assert.AreEqual(Double.MinValue, res.MinValue, "MinValue");
      Assert.AreEqual(Double.MaxValue, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Double), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddDouble_nullable([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddDouble("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Float, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.AreEqual(Double.MinValue, res.MinValue, "MinValue");
      Assert.AreEqual(Double.MaxValue, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Double), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddBoolean()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddBoolean("F1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Boolean, res.ColumnType, "ColumnType");
      Assert.IsFalse(res.Nullable, "Nullable");
      Assert.AreEqual(false, res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Boolean), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddReference()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddReference("F1", "T1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.AreEqual("T1", res.MasterTableName);
      Assert.IsFalse(res.Nullable, "Nullable");
      Assert.AreEqual(DBxRefType.Disallow, res.RefType, "RefType");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(0.0, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(int), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddReference_nullable([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddReference("F1", "T1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.AreEqual("T1", res.MasterTableName);
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.AreEqual(DBxRefType.Disallow, res.RefType, "RefType");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(0.0, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(int), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    // Не все сочетания разрешены
    [TestCase(true, DBxRefType.Disallow)]
    [TestCase(true, DBxRefType.Delete)]
    [TestCase(true, DBxRefType.Clear)]
    [TestCase(true, DBxRefType.Emulation)]
    [TestCase(false, DBxRefType.Disallow)]
    [TestCase(false, DBxRefType.Emulation)]
    public void Columns_AddReference_nullable_refType(bool nullable, DBxRefType refType)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddReference("F1", "T1", nullable, refType);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int, res.ColumnType, "ColumnType");
      Assert.AreEqual("T1", res.MasterTableName);
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.AreEqual(refType, res.RefType, "RefType");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(0.0, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(int), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
    }

    [Test]
    public void Columns_AddMemo()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddMemo("F1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Memo, res.ColumnType, "ColumnType");
      Assert.IsTrue(res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(String), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsTrue(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddXml()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddXml("F1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Xml, res.ColumnType, "ColumnType");
      Assert.IsTrue(res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(String), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsTrue(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddXmlConfig()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddXmlConfig("F1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Xml, res.ColumnType, "ColumnType");
      Assert.IsTrue(res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(String), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsTrue(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddBinary()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddBinary("F1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Binary, res.ColumnType, "ColumnType");
      Assert.IsTrue(res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(byte[]), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsTrue(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddGuid()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddGuid("F1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Guid, res.ColumnType, "ColumnType");
      Assert.IsTrue(res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Guid), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddGuid_nullable([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddGuid("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Guid, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Guid), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    #endregion

    #region PrimaryKey

    [Test]
    public void PrimaryKey_get()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DoTestPrimaryKey(sut, "", "#0");

      sut.Columns.AddId();
      DoTestPrimaryKey(sut, "Id", "#1");
      Assert.AreEqual("Id", sut.PrimaryKey.AsString, "#1");

      sut.Columns.AddString("F1", 10, false);
      DoTestPrimaryKey(sut, "Id", "#2");
    }

    private static void DoTestPrimaryKey(DBxTableStruct sut, string colNames, string message)
    {
      Assert.AreEqual(colNames, sut.PrimaryKey.AsString, message + "-PrimaryKey");

      string[] aNames = DataTools.EmptyStrings;
      if (colNames.Length > 0)
        aNames = colNames.Split(',');
      DBxColumnStruct[] wanted = new DBxColumnStruct[aNames.Length];
      for (int i = 0; i < aNames.Length; i++)
        wanted[i] = sut.Columns.GetRequired(aNames[i]);
      CollectionAssert.AreEqual(wanted, sut.PrimaryKeyColumns, message + "-PrimaryKeyColumns");
    }

    [Test]
    public void PrimaryKey_set()
    {
      DBxTableStruct sut = CreateTestObject();
      DoTestPrimaryKey(sut, "Id", "#0");

      sut.PrimaryKey = new DBxColumns("Id,F1");
      DoTestPrimaryKey(sut, "Id,F1", "#1");

      sut.PrimaryKey = DBxColumns.Empty;
      DoTestPrimaryKey(sut, "", "#2");

      Assert.Catch<ArgumentException>(delegate () { sut.PrimaryKey = new DBxColumns("XXX"); }, "#3");
      DoTestPrimaryKey(sut, "", "#3");
    }

    [Test]
    public void PrimaryKeyColumns_set()
    {
      DBxTableStruct sut = CreateTestObject();
      DoTestPrimaryKey(sut, "Id", "#0");

      sut.PrimaryKeyColumns = new DBxColumnStruct[2] { sut.Columns.GetRequired("Id"), sut.Columns.GetRequired("F1") };
      DoTestPrimaryKey(sut, "Id,F1", "#1");

      sut.PrimaryKeyColumns = new DBxColumnStruct[0];
      DoTestPrimaryKey(sut, "", "#2");
    }

    #endregion

    #region AutoPrimaryKey

    [Test]
    public void AutoPrimaryKey([Values(false, true)]bool value)
    {
      DBxTableStruct sut = CreateTestObject();
      sut.AutoPrimaryKey = value;
      Assert.AreEqual(value, sut.AutoPrimaryKey);
    }

    #endregion

    #region Доступ к индексам

    [Test]
    public void Indexes_Item_GetByName()
    {
      DBxTableStruct sut = CreateTestObject();
      DBxIndexStruct ind = sut.Indexes.Add("F1");

      DBxIndexStruct res1 = sut.Indexes[ind.IndexName];
      Assert.AreSame(ind, res1, "#1");

      DBxIndexStruct res2 = sut.Indexes["XXX"];
      Assert.IsNull(res2, "#2");

      DBxIndexStruct res3 = sut.Indexes[""];
      Assert.IsNull(res3, "#3");
    }

    [Test]
    public void Indexes_Item_GetByIndex()
    {
      DBxTableStruct sut = CreateTestObject();
      DBxIndexStruct ind = sut.Indexes.Add("F1");

      DBxIndexStruct res1 = sut.Indexes[0];
      Assert.AreSame(ind, res1, "#1");

      DBxIndexStruct res2;
      Assert.Catch<ArgumentOutOfRangeException>(delegate () { res2 = sut.Indexes[1]; }, "#2");

      DBxIndexStruct res3;
      Assert.Catch<ArgumentOutOfRangeException>(delegate () { res3 = sut.Indexes[-1]; }, "#4");
    }

    [Test]
    public void Indexes_GetRequired()
    {
      DBxTableStruct sut = CreateTestObject();
      DBxIndexStruct ind = sut.Indexes.Add("F1");

      DBxIndexStruct res1 = sut.Indexes.GetRequired(ind.IndexName);
      Assert.AreSame(ind, res1, "#1");

      DBxIndexStruct res2;
      Assert.Catch(delegate () { res2 = sut.Indexes.GetRequired("XXX"); }, "#2");

      DBxIndexStruct res3;
      Assert.Catch(delegate () { res3 = sut.Indexes.GetRequired(null); }, "#4");
    }

    [Test]
    public void Indexes_LastAdded()
    {
      DBxTableStruct sut = CreateTestObject();
      Assert.IsNull(sut.Indexes.LastAdded, "#1");

      DBxIndexStruct ind = sut.Indexes.Add("F1");
      Assert.AreSame(ind, sut.Indexes.LastAdded, "#2");
    }

    [Test]
    public void Indexes_FindByColumns()
    {
      DBxTableStruct sut = CreateTestObject();
      DBxIndexStruct ind1 = sut.Indexes.Add("F1");
      DBxIndexStruct ind2 = sut.Indexes.Add("F2,F1");

      Assert.AreSame(ind1, sut.Indexes.FindByColumns(new DBxColumns("F1")), "F1");
      Assert.AreSame(ind2, sut.Indexes.FindByColumns(new DBxColumns("F2,F1")), "F2,F1");
      Assert.IsNull(sut.Indexes.FindByColumns(new DBxColumns("F2")), "F2");
      Assert.IsNull(sut.Indexes.FindByColumns(new DBxColumns("F1,F2")), "F1,F2");
      Assert.IsNull(sut.Indexes.FindByColumns(DBxColumns.Empty), "DBxColumns.Empty");
      Assert.IsNull(sut.Indexes.FindByColumns((DBxColumns)null), "null");
    }

    #endregion

    #region Методы добавления индексов

    // Основная часть методов реализуется классом NamedList, а не IndexCollection

    [Test]
    public void Indexes_Add_DBxColumns()
    {
      DBxTableStruct sut = CreateTestObject();
      DBxIndexStruct res1 = sut.Indexes.Add(new DBxColumns("F1,F2"));
      DBxIndexStruct res2 = sut.Indexes.Add(new DBxColumns("F2"));

      Assert.AreEqual(2, sut.Indexes.Count, "IndexCount");
      CollectionAssert.AreEqual(new DBxIndexStruct[] { res1, res2 }, sut.Indexes, "Indexes");

      Assert.AreEqual("F1,F2", res1.Columns.AsString, "Columns #1");
      Assert.AreEqual("Index1", res1.IndexName, "IndexName #1");

      Assert.AreEqual("F2", res2.Columns.AsString, "Columns #2");
      Assert.AreEqual("Index2", res2.IndexName, "IndexName #2");
    }


    [Test]
    public void Indexes_Add_String()
    {
      DBxTableStruct sut = CreateTestObject();
      DBxIndexStruct res1 = sut.Indexes.Add("F1,F2");
      DBxIndexStruct res2 = sut.Indexes.Add("F2");

      Assert.AreEqual(2, sut.Indexes.Count, "IndexCount");

      Assert.AreEqual("F1,F2", res1.Columns.AsString, "Columns #1");
      Assert.AreEqual("Index1", res1.IndexName, "IndexName #1");

      Assert.AreEqual("F2", res2.Columns.AsString, "Columns #2");
      Assert.AreEqual("Index2", res2.IndexName, "IndexName #2");
    }

    [Test]
    public void Indexes_Add_exceptions()
    {
      DBxTableStruct sut = CreateTestObject();
      Assert.Catch(delegate () { sut.Indexes.Add(""); }, "Empty string");
      Assert.Catch(delegate () { sut.Indexes.Add(DBxColumns.Empty); }, "DBxColumns.Empty");
      DBxColumns nullColumns = null;
      Assert.Catch(delegate () { sut.Indexes.Add(nullColumns); }, "null");
    }

    #endregion

    #region Comment, HasComments

    [Test]
    public void Comment()
    {
      DBxTableStruct sut = CreateTestObject();
      Assert.AreEqual("", sut.Comment, "Comment #0");
      Assert.IsFalse(sut.HasComments, "HasComments #0");

      sut.Comment = "XXX";
      Assert.AreEqual("XXX", sut.Comment, "Comment #1");
      Assert.IsTrue(sut.HasComments, "HasComments #1");
    }

    // Свойство DBxTableStruct.Comment уже протестировано

    [Test]
    public void HasComments_Column()
    {
      DBxTableStruct sut = CreateTestObject();
      Assert.IsFalse(sut.HasComments, "#0");

      sut.Columns["F1"].Comment = "XXX";
      Assert.IsTrue(sut.HasComments, "#1");
    }

    [Test]
    public void HasComments_Index()
    {
      DBxTableStruct sut = CreateTestObject();
      sut.Indexes.Add("F1");
      Assert.IsFalse(sut.HasComments, "#0");

      sut.Indexes[0].Comment = "XXX";
      Assert.IsTrue(sut.HasComments, "#1");
    }

    #endregion

    #region AutoPrimaryKey

    [Test]
    public void AutoCreate([Values(false, true)]bool value)
    {
      DBxTableStruct sut = CreateTestObject();
      sut.AutoCreate = value;
      Assert.AreEqual(value, sut.AutoCreate);
    }

    #endregion

    #region CreateDataTable()

    [Test]
    public void CreateDataTable()
    {
      DBxTableStruct sut = CreateTestObject();
      sut.SetReadOnly();

      DataTable res = sut.CreateDataTable();
      Assert.AreEqual("Test1", res.TableName, "TableName");
      CollectionAssert.AreEqual(new string[] { "Id", "F1", "F2" }, DataTools.GetColumnNames(res), "ColumnNames");
      Assert.AreEqual(0, res.Rows.Count, "RowCount");

      Assert.AreEqual(typeof(Int32), res.Columns[0].DataType, "Id DataType");
      Assert.IsFalse(res.Columns[0].AllowDBNull, "Id AllowDBNull");

      Assert.AreEqual(typeof(string), res.Columns[1].DataType, "F1 DataType");
      Assert.IsFalse(res.Columns[1].AllowDBNull, "F1 AllowDBNull");
      Assert.AreEqual(10, res.Columns[1].MaxLength, "F1 MaxLength");

      Assert.AreEqual(typeof(Int32), res.Columns[2].DataType, "F2 DataType");
      Assert.IsTrue(res.Columns[2].AllowDBNull, "F2 AllowDBNull");
    }

    #endregion

    #region SetReadOnly()

    [Test]
    public void SetReadOnly()
    {
      DBxTableStruct sut = CreateTestObject();
      sut.Indexes.Add("F1");

      sut.SetReadOnly();

      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.DoesNotThrow(delegate () { sut.SetReadOnly(); }, "SetReadOnly() again");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.AutoCreate = false; }, "AutoCreate");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.PrimaryKey = DBxColumns.Empty; }, "PrimaryKey");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.PrimaryKeyColumns = new DBxColumnStruct[0]; }, "PrimaryKeyColumns");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.AutoPrimaryKey = false; }, "AutoPrimaryKey");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Comment = "XXX"; }, "Comment");

      Assert.IsTrue(sut.Columns.IsReadOnly, "Columns.IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Columns.AddBoolean("F99"); }, "Columns.Add()");
      Assert.IsTrue(sut.Columns[0].IsReadOnly, "DBxColumnStruct.IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Columns[0].Comment="XXX"; }, "DBxColumnStruct.Comment");

      Assert.IsTrue(sut.Columns.IsReadOnly, "Indexes.IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Indexes.Add("F2"); }, "Indexes.Add()");
      Assert.IsTrue(sut.Indexes[0].IsReadOnly, "DBxIndexStruct.IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Indexes[0].Comment = "XXX"; }, "DBxIndexStruct.Comment");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DBxTableStruct sut = CreateTestObject();
      sut.Indexes.Add("F1");
      byte[] b = SerializationTools.SerializeBinary(sut);
      DBxTableStruct res = (DBxTableStruct)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.TableName, res.TableName);
    }

    #endregion
  }
}
