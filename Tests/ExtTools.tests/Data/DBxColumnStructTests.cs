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
  public class DBxColumnStructTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      Assert.AreEqual("F1", sut.ColumnName, "ColumnName");
      Assert.AreEqual(DBxColumnType.Unknown, sut.ColumnType, "ColumnType");
      Assert.IsNull(sut.DataType, "DataType");
      Assert.IsTrue(sut.Nullable, "Nullable");
      Assert.IsNull(sut.DefaultValue, "DefaultValue");
      Assert.IsNull(sut.DefaultExpression, "DefaultExpression");
      Assert.AreEqual(0, sut.MaxLength, "MaxLength");
      Assert.AreEqual(0.0, sut.MinValue, "MinValue");
      Assert.AreEqual(0.0, sut.MaxValue, "MaxValue");
      Assert.AreEqual(DBxRefType.Disallow, sut.RefType, "RefType");
      Assert.AreEqual("", sut.MasterTableName, "MasterTableName");
      Assert.AreEqual("", sut.Comment, "Comment");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.DoesNotThrow(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
    }

    #endregion

    #region ColumnType

    [TestCase(DBxColumnType.String, typeof(String))]
    [TestCase(DBxColumnType.Int, typeof(Int32))]
    [TestCase(DBxColumnType.Float, typeof(Double))]
    [TestCase(DBxColumnType.Decimal, typeof(Decimal))]
    [TestCase(DBxColumnType.Boolean, typeof(Boolean))]
    [TestCase(DBxColumnType.Date, typeof(DateTime))]
    [TestCase(DBxColumnType.DateTime, typeof(DateTime))]
    [TestCase(DBxColumnType.Time, typeof(TimeSpan))]
    [TestCase(DBxColumnType.Guid, typeof(Guid))]
    [TestCase(DBxColumnType.Memo, typeof(String))]
    [TestCase(DBxColumnType.Xml, typeof(String))]
    [TestCase(DBxColumnType.Binary, typeof(byte[]))]
    [TestCase(DBxColumnType.Unknown, null)]
    public void ColumnType(DBxColumnType columnType, Type wantedDataType)
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      sut.ColumnType = columnType;

      Assert.AreEqual(columnType, sut.ColumnType, "ColumnType");
      Assert.AreEqual(wantedDataType, sut.DataType);
    }

    #endregion

    #region Другие свойства

    [Test]
    public void MaxLength()
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      sut.MaxLength = 123;
      Assert.AreEqual(123, sut.MaxLength);
    }

    [Test]
    public void MinValue()
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      sut.MinValue = 123.0;
      Assert.AreEqual(123.0, sut.MinValue);
    }

    [Test]
    public void MaxValue()
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      sut.MaxValue = 123.0;
      Assert.AreEqual(123.0, sut.MaxValue);
    }

    [Test]
    public void Nullable([Values(false, true)]bool value)
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      sut.Nullable = value;
      Assert.AreEqual(value, sut.Nullable, "Nullable");
      Assert.IsNull(sut.DefaultValue, "DefaultValue");
    }

    [Test]
    public void DefaultValue()
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      sut.ColumnType = DBxColumnType.Int;
      sut.DefaultValue = 123;
      Assert.AreEqual(123, sut.DefaultValue, "DefaultValue");
      Assert.IsFalse(sut.Nullable, "Nullable");
      Assert.IsNotNull(sut.DefaultExpression, "DefaultExpression");
      Assert.AreEqual(123, sut.DefaultExpression.Value, "DefaultExpression.Value");
    }

    [Test]
    public void RefType([Values(DBxRefType.Disallow, DBxRefType.Delete, DBxRefType.Clear, DBxRefType.Emulation)] DBxRefType refType)
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      sut.RefType = refType;
      Assert.AreEqual(refType, sut.RefType);
    }

    [Test]
    public void MasterTableName()
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      sut.MasterTableName = "T1";
      Assert.AreEqual("T1", sut.MasterTableName);
    }


    [Test]
    public void Comment()
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      sut.Comment = "XXX";
      Assert.AreEqual("XXX", sut.Comment);
    }

    #endregion

    #region SetDefaultyValue()

    [TestCase(DBxColumnType.String, "")]
    [TestCase(DBxColumnType.Int, 0)]
    [TestCase(DBxColumnType.Float, 0.0)]
    [TestCase(DBxColumnType.Boolean, false)]
    // Остальные значения не являются константными
    public void SetDefaultValue(DBxColumnType colType, object wantedValue)
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      sut.ColumnType = colType;

      sut.SetDefaultValue();
      Assert.AreEqual(wantedValue, sut.DefaultValue, "DefaultValue");
      Assert.IsFalse(sut.Nullable, "Nullable");
    }

    #endregion

    #region SetReadOnly()

    #endregion

    #region DataType

    // Основная часть протестирована в ColumnType()

    [TestCase(0, 0, typeof(Int32))]
    [TestCase(-128, 127, typeof(SByte))]
    [TestCase(0, 128, typeof(Byte))]
    [TestCase(0, 255, typeof(Byte))]
    [TestCase(0, 256, typeof(Int16))]
    [TestCase(Int16.MinValue, Int16.MaxValue, typeof(Int16))]
    [TestCase(0, 65535, typeof(UInt16))]
    [TestCase(0, 65536, typeof(Int32))]
    [TestCase(Int32.MinValue, Int32.MaxValue, typeof(Int32))]
    [TestCase(0, 4000000000.0, typeof(UInt32))]
    [TestCase(0, 40000000000.0, typeof(Int64))]
    public void DataType_Int(double minValue, double maxValue, Type wantedDataType)
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      sut.ColumnType = DBxColumnType.Int;
      sut.MinValue = minValue;
      sut.MaxValue = maxValue;
      Assert.AreEqual(wantedDataType, sut.DataType);
    }

    [TestCase(0, 0, typeof(Double))]
    [TestCase(0, 1000, typeof(Single))]
    [TestCase(0, 1e35, typeof(Single))]
    [TestCase(0, 1e40, typeof(Double))]
    public void DataType_Float(double minValue, double maxValue, Type wantedDataType)
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      sut.ColumnType = DBxColumnType.Float;
      sut.MinValue = minValue;
      sut.MaxValue = maxValue;
      Assert.AreEqual(wantedDataType, sut.DataType);
    }

    #endregion

    #region CreateDataColumn()

    // TODO: !!!

    //[Test]
    //public void CreateDataColumn()
    //{
    //  DBxColumnStruct sut = new DBxColumnStruct("F1");
    //  sut.ColumnType = DBxColumnType.Float;
    //}

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      byte[] b = SerializationTools.SerializeBinary(sut);
      DBxColumnStruct res = (DBxColumnStruct)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ColumnName, res.ColumnName);
    }

    #endregion
  }
}
