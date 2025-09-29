using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;
using System.Data;
using FreeLibSet.Remoting;

namespace ExtTools_tests.Data
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
      sut.Columns.AddInt32("Id", false);
      sut.Columns.AddString("F1", 10, false);
      sut.Columns.AddInt32("F2", true);

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
      Assert.AreEqual(DBxColumnType.Int32, res1.ColumnType, "ColumnType #1");
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
      Assert.AreEqual(DBxColumnType.Int32, res1.ColumnType, "ColumnType #1");
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

      DBxColumnStruct col = sut.Columns.AddInt32("Id", false);
      Assert.AreSame(col, sut.Columns.LastAdded, "#2");
    }

    [Test]
    public void AllColumnNames()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      CollectionAssert.AreEqual(new string[] { }, sut.AllColumnNames, "#0");

      sut.Columns.AddInt32("Id", false);
      CollectionAssert.AreEqual(new string[] { "Id" }, sut.AllColumnNames, "#1");

      sut.Columns.AddString("F1", 10, false);
      CollectionAssert.AreEqual(new string[] { "Id", "F1" }, sut.AllColumnNames, "#2");
    }

    #endregion

    #region Методы добавления столбцов

    #region AddIntXX()

    [Test]
    public void Columns_AddInt16([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt16("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int16, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Int16), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddInt32([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt32("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int32, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(0.0, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Int32), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddInt64([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInt64("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Int64, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Int64), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    #endregion

    #region AddInteger()

    [TestCase(0, 1, DBxColumnType.Byte, typeof(byte))]
    [TestCase(0, 255, DBxColumnType.Byte, typeof(byte))]
    [TestCase(-1, 100, DBxColumnType.SByte, typeof(sbyte))]
    [TestCase(-128, 127, DBxColumnType.SByte, typeof(sbyte))]
    [TestCase(0, 256, DBxColumnType.UInt16, typeof(ushort))]
    [TestCase(0, 65535, DBxColumnType.UInt16, typeof(ushort))]
    [TestCase(-1, 256, DBxColumnType.Int16, typeof(short))]
    [TestCase(-32768, 32767, DBxColumnType.Int16, typeof(short))]
    public void Columns_AddInteger_Int32(int min, int max, DBxColumnType wantedColumnType, Type wantedDataType)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");

      DBxColumnStruct res1 = sut.Columns.AddInteger("F1", min, max, true);
      DBxColumnStruct res2 = sut.Columns.AddInteger("F2", new MinMax<Int32>(min, max), true);

      Assert.AreEqual("F1", res1.ColumnName, "ColumnName");
      Assert.AreEqual("F2", res2.ColumnName, "ColumnName");
      DoTest_Columns_AddInteger_Int32(res1, min, max, wantedColumnType, wantedDataType);
      DoTest_Columns_AddInteger_Int32(res1, min, max, wantedColumnType, wantedDataType);

      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    private void DoTest_Columns_AddInteger_Int32(DBxColumnStruct res, int min, int max, DBxColumnType wantedColumnType, Type wantedDataType)
    {

      Assert.AreEqual(wantedColumnType, res.ColumnType, "ColumnType");
      Assert.AreEqual((double)min, res.MinValue, "MinValue");
      Assert.AreEqual((double)max, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(wantedDataType, res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
    }

    [TestCase(0L, 1, DBxColumnType.Byte, typeof(byte))]
    [TestCase(0L, 255, DBxColumnType.Byte, typeof(byte))]
    [TestCase(-1L, 100, DBxColumnType.SByte, typeof(sbyte))]
    [TestCase(-128L, 127L, DBxColumnType.SByte, typeof(sbyte))]
    [TestCase(0L, 256L, DBxColumnType.UInt16, typeof(ushort))]
    [TestCase(0L, 65535L, DBxColumnType.UInt16, typeof(ushort))]
    [TestCase(-1L, 256L, DBxColumnType.Int16, typeof(short))]
    [TestCase(-32768L, 32767L, DBxColumnType.Int16, typeof(short))]
    [TestCase(0L, 65536L, DBxColumnType.UInt32, typeof(uint))]
    [TestCase(0L, 4294967295L, DBxColumnType.UInt32, typeof(uint))]
    [TestCase(-1L, 65536L, DBxColumnType.Int32, typeof(int))]
    [TestCase(-2147483648L, 2147483647L, DBxColumnType.Int32, typeof(int))]
    [TestCase(0L, 4294967296L, DBxColumnType.UInt64, typeof(ulong))]
    [TestCase(-1L, 2147483648L, DBxColumnType.Int64, typeof(long))]
    public void Columns_AddInteger_Int64(long min, long max, DBxColumnType wantedColumnType, Type wantedDataType)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInteger("F1", min, max, true);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(wantedColumnType, res.ColumnType, "ColumnType");
      Assert.AreEqual(min, (Int64)(res.MinValue), "MinValue");
      Assert.AreEqual(max, (Int64)(res.MaxValue), "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(wantedDataType, res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }


    [Test]
    public void Columns_AddInteger_Int32_Int32_nullable([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInteger("F1", 0, 127, nullable);
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
    }

    [Test]
    public void Columns_AddInteger_MinMaxInt32_nullable([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInteger("F1", new MinMax<int>(0, 255), nullable);
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
    }

    [Test]
    public void Columns_AddInteger_Int64_Int64_nullable([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddInteger("F1", 0L, 10000000000L, nullable);
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
    }

    #endregion

    #region Числа с плавающей точкой

    [Test]
    public void Columns_AddSingle([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddSingle("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Single, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Single), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddDouble([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddDouble("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Double, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Double), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    [Test]
    public void Columns_AddDecimal([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddDecimal("F1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Decimal, res.ColumnType, "ColumnType");
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.AreEqual(typeof(Decimal), res.DataType, "DataType");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      Assert.IsFalse(sut.Columns.ContainsBlob(), "ContainsBlob()");
    }

    #endregion

    #region Дата/время

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

    #endregion

    #region Прочие

    [Test]
    public void Columns_AddId()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddId("F1");
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Unknown, res.ColumnType, "ColumnType");
      Assert.IsFalse(res.Nullable, "Nullable");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.IsNull(res.DataType, "DataType");
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
    public void Columns_AddReference([Values(false, true)]bool nullable)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddReference("F1", "T1", nullable);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Unknown, res.ColumnType, "ColumnType");
      Assert.AreEqual("T1", res.MasterTableName);
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.AreEqual(DBxRefType.Disallow, res.RefType, "RefType");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(0.0, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.IsNull(res.DataType, "DataType");
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
    public void Columns_AddReference_refType(bool nullable, DBxRefType refType)
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DBxColumnStruct res = sut.Columns.AddReference("F1", "T1", nullable, refType);
      Assert.AreEqual("F1", res.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Unknown, res.ColumnType, "ColumnType");
      Assert.AreEqual("T1", res.MasterTableName);
      Assert.AreEqual(nullable, res.Nullable, "Nullable");
      Assert.AreEqual(refType, res.RefType, "RefType");
      Assert.AreEqual(0.0, res.MinValue, "MinValue");
      Assert.AreEqual(0.0, res.MaxValue, "MaxValue");
      Assert.IsNull(res.DefaultValue, "DefaultValue");
      Assert.IsNull(res.DataType, "DataType");
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
    public void Columns_AddGuid([Values(false, true)]bool nullable)
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

    #endregion

    #region PrimaryKey

    [Test]
    public void PrimaryKey_get()
    {
      DBxTableStruct sut = new DBxTableStruct("Test1");
      DoTestPrimaryKey(sut, "", "#0");

      sut.Columns.AddId("Id");
      DoTestPrimaryKey(sut, "Id", "#1");
      Assert.AreEqual("Id", sut.PrimaryKey.AsString, "#1");

      sut.Columns.AddString("F1", 10, false);
      DoTestPrimaryKey(sut, "Id", "#2");
    }

    private static void DoTestPrimaryKey(DBxTableStruct sut, string colNames, string message)
    {
      Assert.AreEqual(colNames, sut.PrimaryKey.AsString, message + "-PrimaryKey");

      string[] aNames = EmptyArray<string>.Empty;
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
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Columns[0].Comment = "XXX"; }, "DBxColumnStruct.Comment");

      Assert.IsTrue(sut.Columns.IsReadOnly, "Indexes.IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Indexes.Add("F2"); }, "Indexes.Add()");
      Assert.IsTrue(sut.Indexes[0].IsReadOnly, "DBxIndexStruct.IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Indexes[0].Comment = "XXX"; }, "DBxIndexStruct.Comment");
    }


    [Test]
    public void DBxColumnStruct_SetReadOnly() // этот тест нельзя поместить в DBxColumnStructTests
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      // Метод SetReadOnly является защищенным. Используем DBxTableStruct
      DBxTableStruct ts = new DBxTableStruct("T1");
      ts.Columns.Add(sut);
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly #1");

      ts.SetReadOnly();
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly #2");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.ColumnType = DBxColumnType.String; }, "ColumnType");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Nullable = false; }, "Nullable");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.DefaultValue = 123; }, "DefaultValue");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.SetDefaultValue(); }, "SetDefaultValue()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.MaxLength = 10; }, "MaxLength");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.MinValue = 1.0; }, "MinValue");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.MaxValue = 1.0; }, "MaxValue");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.RefType = DBxRefType.Delete; }, "RefType");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.MasterTableName = "T1"; }, "MasterTableName");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Comment = "XXX"; }, "Comment");
    }

    #endregion

    #region Clone()

    [TestCase("STRING")]
    [TestCase("NOT NULL")]
    [TestCase("DEFAULT")]
    [TestCase("REF")]
    public void DBxColumnStruct_Clone(string mode) // этот тест нельзя поместить в DBxColumnStructTests
    {
      // За один раз нельзя протестировать все свойства
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      if (mode == "STRING")
        sut.ColumnType = DBxColumnType.String;
      else
        sut.ColumnType = DBxColumnType.Int32;
      switch (mode)
      {
        case "NOT NULL":
          sut.Nullable = false;
          break;
        case "DEFAULT":
          sut.DefaultValue = 123;
          sut.MinValue = 10.0;
          sut.MaxValue = 20.0;
          break;
        case "STRING":
          sut.MaxLength = 30;
          break;
        case "REF":
          sut.RefType = DBxRefType.Delete;
          sut.MasterTableName = "T1";
          break;
      }
      sut.Comment = "XXX";

      DBxTableStruct ts = new DBxTableStruct("T1");
      ts.Columns.Add(sut);
      ts.SetReadOnly();

      DBxColumnStruct res1 = sut.Clone();
      Assert.AreEqual("F1", res1.ColumnName, "#1-ColumnName");
      DoTestClone(sut, res1, "#1");

      DBxColumnStruct res2 = sut.Clone("F2");
      Assert.AreEqual("F2", res2.ColumnName, "#2-ColumnName");
      DoTestClone(sut, res2, "#2");
    }

    private static void DoTestClone(DBxColumnStruct sut, DBxColumnStruct res, string messagePrefix)
    {
      Assert.IsFalse(res.IsReadOnly, messagePrefix + "-IsReadOnly");
      Assert.DoesNotThrow(delegate () { res.CheckNotReadOnly(); }, messagePrefix + "-CheckNotReadOnly()");
      Assert.AreEqual(sut.ColumnType, res.ColumnType, messagePrefix + "-ColumnType");
      Assert.AreEqual(sut.DataType, res.DataType, messagePrefix + "-DataType");
      Assert.AreEqual(sut.Nullable, res.Nullable, messagePrefix + "-Nullable");
      Assert.AreEqual(sut.DefaultValue, res.DefaultValue, messagePrefix + "-DefaultValue");
      Assert.AreEqual(sut.MaxLength, res.MaxLength, messagePrefix + "-MaxLength");
      Assert.AreEqual(sut.MinValue, res.MinValue, messagePrefix + "-MinValue");
      Assert.AreEqual(sut.MaxValue, res.MaxValue, messagePrefix + "-MaxValue");
      Assert.AreEqual(sut.RefType, res.RefType, messagePrefix + "-RefType");
      Assert.AreEqual(sut.MasterTableName, res.MasterTableName, messagePrefix + "-MasterTableName");
      Assert.AreEqual(sut.Comment, res.Comment, messagePrefix + "-Comment");
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
