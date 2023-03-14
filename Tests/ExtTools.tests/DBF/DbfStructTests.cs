using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DBF;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Remoting;

namespace ExtTools_tests.DBF
{
  [TestFixture]
  public class DbfStructTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DbfStruct sut = new DbfStruct();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region Методы Add()

    [Test]
    public void Add()
    {
      DbfStruct sut = new DbfStruct();

      sut.Add(new DbfFieldInfo("F1", 'C', 10, 0));
      Assert.AreEqual(1, sut.Count, "Count #1");
      Assert.AreEqual("F1", sut[0].Name, "[0]");

      sut.Add(new DbfFieldInfo("F2", 'N', 3, 0));
      Assert.AreEqual(2, sut.Count, "Count #2");
      Assert.AreEqual("F2", sut[1].Name, "[1]");
    }

    [Test]
    public void AddString()
    {
      DbfStruct sut = new DbfStruct();
      sut.AddString("F1", 123);
      Assert.AreEqual(1, sut.Count, "Count");
      Assert.AreEqual("F1", sut[0].Name, "Name");
      Assert.AreEqual('C', sut[0].Type, "Type");
      Assert.AreEqual(123, sut[0].Length, "Length");
      Assert.AreEqual(0, sut[0].Precision, "Precision");
    }

    [Test]
    public void AddNum_1()
    {
      DbfStruct sut = new DbfStruct();
      sut.AddNum("F1", 3);
      Assert.AreEqual(1, sut.Count, "Count");
      Assert.AreEqual("F1", sut[0].Name, "Name");
      Assert.AreEqual('N', sut[0].Type, "Type");
      Assert.AreEqual(3, sut[0].Length, "Length");
      Assert.AreEqual(0, sut[0].Precision, "Precision");
    }

    [Test]
    public void AddNum_2()
    {
      DbfStruct sut = new DbfStruct();
      sut.AddNum("F1", 5, 2);
      Assert.AreEqual(1, sut.Count, "Count");
      Assert.AreEqual("F1", sut[0].Name, "Name");
      Assert.AreEqual('N', sut[0].Type, "Type");
      Assert.AreEqual(5, sut[0].Length, "Length");
      Assert.AreEqual(2, sut[0].Precision, "Precision");
    }

    [Test]
    public void AddBool()
    {
      DbfStruct sut = new DbfStruct();
      sut.AddBool("F1");
      Assert.AreEqual(1, sut.Count, "Count");
      Assert.AreEqual("F1", sut[0].Name, "Name");
      Assert.AreEqual('L', sut[0].Type, "Type");
      Assert.AreEqual(1, sut[0].Length, "Length");
      Assert.AreEqual(0, sut[0].Precision, "Precision");
    }

    [Test]
    public void AddDate()
    {
      DbfStruct sut = new DbfStruct();
      sut.AddDate("F1");
      Assert.AreEqual(1, sut.Count, "Count");
      Assert.AreEqual("F1", sut[0].Name, "Name");
      Assert.AreEqual('D', sut[0].Type, "Type");
      Assert.AreEqual(8, sut[0].Length, "Length");
      Assert.AreEqual(0, sut[0].Precision, "Precision");
    }

    [Test]
    public void AddMemo()
    {
      DbfStruct sut = new DbfStruct();
      sut.AddMemo("F1");
      Assert.AreEqual(1, sut.Count, "Count");
      Assert.AreEqual("F1", sut[0].Name, "Name");
      Assert.AreEqual('M', sut[0].Type, "Type");
      Assert.AreEqual(10, sut[0].Length, "Length");
      Assert.AreEqual(0, sut[0].Precision, "Precision");
    }

    #endregion

    #region Item

    [Test]
    public void Item_index()
    {
      DbfStruct sut = CreateTestObject();
      Assert.AreEqual("F1", sut[0].Name, "[0]");
      Assert.AreEqual("F2", sut[1].Name, "[1]");
      Assert.AreEqual("F3", sut[2].Name, "[2]");

      DbfFieldInfo dummy;
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[-1]; }, "[-1]");
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[3]; }, "[3]");
    }

    private static DbfStruct CreateTestObject()
    {
      DbfStruct sut = new DbfStruct();
      sut.AddString("F1", 20);
      sut.AddNum("F2", 5);
      sut.AddBool("F3");
      Assert.AreEqual(3, sut.Count);
      return sut;
    }

    [Test]
    public void Item_name()
    {
      DbfStruct sut = CreateTestObject();
      Assert.AreEqual("F1", sut["F1"].Name, "[F1]");
      Assert.AreEqual("F2", sut["F2"].Name, "[F2]");
      Assert.AreEqual("F3", sut["F3"].Name, "[F3]");

      Assert.IsTrue(sut["F4"].IsEmpty, "[F4]");
      Assert.IsTrue(sut[""].IsEmpty, "[]");
    }

    #endregion

    #region RecordSize

    [Test]
    public void RecordSize()
    {
      DbfStruct sut = CreateTestObject();
      Assert.AreEqual(1+20+5+1, sut.RecordSize, "#1");

      sut.AddMemo("F4");
      Assert.AreEqual(1 + 20 + 5 + 1+10, sut.RecordSize, "#2");
    }

    #endregion

    #region HasMemo

    [Test]
    public void HasMemo()
    {
      DbfStruct sut = CreateTestObject();
      Assert.IsFalse(sut.HasMemo, "#1");

      sut.AddMemo("F4");
      Assert.IsTrue(sut.HasMemo, "#2");
    }

    #endregion

    #region Прочие методы

    [Test]
    public void IndexOf()
    {
      DbfStruct sut = CreateTestObject();
      Assert.AreEqual(1, sut.IndexOf("F2"), "F2");
      Assert.AreEqual(-1, sut.IndexOf("F4"), "F4");
      Assert.AreEqual(-1, sut.IndexOf(""), "Empty string");
    }

    [Test]
    public void CreateTable()
    {
      DbfStruct sut = CreateTestObject();
      DataTable res = sut.CreateTable();

      Assert.AreEqual(3, res.Columns.Count, "Columns.Count");
      Assert.AreEqual("F1", res.Columns[0].ColumnName, "ColumnName[0]");
      Assert.AreEqual(typeof(String), res.Columns[0].DataType, "DataType[0]");
      Assert.AreEqual("F2", res.Columns[1].ColumnName, "ColumnName[1]");
      Assert.AreEqual(typeof(Int32), res.Columns[1].DataType, "DataType[1]");
      Assert.AreEqual("F3", res.Columns[2].ColumnName, "ColumnName[2]");
      Assert.AreEqual(typeof(Boolean), res.Columns[2].DataType, "DataType[2]");
    }

    [Test]
    public void TestFormat()
    {
      DbfStruct sut = CreateTestObject();
      Assert.IsTrue(sut.TestFormat(DbfFileFormat.dBase2), "dBase2 #1");
      Assert.IsTrue(sut.TestFormat(DbfFileFormat.dBase3), "dBase3 #1");
      Assert.IsTrue(sut.TestFormat(DbfFileFormat.dBase4), "dBase4 #1");

      sut.AddMemo("F4");
      Assert.IsFalse(sut.TestFormat(DbfFileFormat.dBase2), "dBase2 #2");
      Assert.IsTrue(sut.TestFormat(DbfFileFormat.dBase3), "dBase3 #2");
      Assert.IsTrue(sut.TestFormat(DbfFileFormat.dBase4), "dBase4 #2");
    }

    #endregion

    #region SetReadOnly()

    [Test]
    public void SetReadOnly()
    {
      DbfStruct sut = CreateTestObject();
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly #1");

      sut.SetReadOnly();
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly #2");

      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.AddString("F5", 10); }, "AddString()");
    }

    #endregion

    #region Перечисление полей

    [Test]
    public void GetEnumerator()
    {
      DbfStruct sut = CreateTestObject();
      List<string> lst = new List<string>();
      foreach (DbfFieldInfo field in sut)
        lst.Add(field.Name);

      Assert.AreEqual(new string[] { "F1", "F2", "F3" }, lst.ToArray());
    }

    #endregion

    #region Сериализация

    [TestCase(false)]
    [TestCase(true)]
    public void Serialization(bool setReadOnly)
    {
      DbfStruct obj1 = CreateTestObject();
      if (setReadOnly)
        obj1.SetReadOnly();
      byte[] b = SerializationTools.SerializeBinary(obj1);

      DbfStruct obj2 = (DbfStruct)(SerializationTools.DeserializeBinary(b));

      Assert.AreEqual(3, obj2.Count, "Count");
      for (int i = 0; i < 3; i++)
      {
        Assert.AreEqual(obj1[i].Name, obj2[i].Name, "Name");
        Assert.AreEqual(obj1[i].Type, obj2[i].Type, "Type");
        Assert.AreEqual(obj1[i].Length, obj2[i].Length, "Length");
        Assert.AreEqual(obj1[i].Precision, obj2[i].Precision, "Precision");
      }
      Assert.AreEqual(setReadOnly, obj2.IsReadOnly, "IsReadOnly");
    }

    #endregion
  }
}
