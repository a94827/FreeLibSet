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
    [TestCase(DBxColumnType.Money, typeof(Decimal))]
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

    [Test]
    public void SetReadOnly()
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

    #region Clone()

    [TestCase("STRING")]
    [TestCase("NOT NULL")]
    [TestCase("DEFAULT")]
    [TestCase("REF")]
    public void Clone(string mode)
    {
      // За один раз нельзя протестировать все свойства
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      if (mode == "STRING")
        sut.ColumnType = DBxColumnType.String;
      else
        sut.ColumnType = DBxColumnType.Int;
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
      DBxColumnStruct sut = new DBxColumnStruct("F1");
      byte[] b = SerializationTools.SerializeBinary(sut);
      DBxColumnStruct res = (DBxColumnStruct)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ColumnName, res.ColumnName);
    }

    #endregion
  }
}
