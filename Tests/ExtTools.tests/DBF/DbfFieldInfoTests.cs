using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DBF;

namespace ExtTools_tests.DBF
{
  [TestFixture]
  public class DbfFieldInfoTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_main()
    {
      DbfFieldInfo sut = new DbfFieldInfo("F1", 'N', 10, 2);
      Assert.AreEqual("F1", sut.Name, "Name");
      Assert.AreEqual('N', sut.Type, "Type");
      Assert.AreEqual(10, sut.Length, "Length");
      Assert.AreEqual(2, sut.Precision, "Precision");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Test]
    public void Constructor_copy()
    {
      DbfFieldInfo field1 = new DbfFieldInfo("F1", 'N', 10, 2);
      DbfFieldInfo sut = new DbfFieldInfo("F2", field1);
      Assert.AreEqual("F2", sut.Name, "Name");
      Assert.AreEqual('N', sut.Type, "Type");
      Assert.AreEqual(10, sut.Length, "Length");
      Assert.AreEqual(2, sut.Precision, "Precision");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Test]
    public void Constructor_default()
    {
      DbfFieldInfo sut = new DbfFieldInfo();
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
    }

    [Test]
    public void CreateSting()
    {
      DbfFieldInfo sut = DbfFieldInfo.CreateString("F1", 20);
      Assert.AreEqual("F1", sut.Name, "Name");
      Assert.AreEqual('C', sut.Type, "Type");
      Assert.AreEqual(20, sut.Length, "Length");
      Assert.AreEqual(0, sut.Precision, "Precision");
    }

    [Test]
    public void CreateNum_1()
    {
      DbfFieldInfo sut = DbfFieldInfo.CreateNum("F1", 7);
      Assert.AreEqual("F1", sut.Name, "Name");
      Assert.AreEqual('N', sut.Type, "Type");
      Assert.AreEqual(7, sut.Length, "Length");
      Assert.AreEqual(0, sut.Precision, "Precision");
    }

    [Test]
    public void CreateNum_2()
    {
      DbfFieldInfo sut = DbfFieldInfo.CreateNum("F1", 8, 3);
      Assert.AreEqual("F1", sut.Name, "Name");
      Assert.AreEqual('N', sut.Type, "Type");
      Assert.AreEqual(8, sut.Length, "Length");
      Assert.AreEqual(3, sut.Precision, "Precision");
    }

    [Test]
    public void CreateBool()
    {
      DbfFieldInfo sut = DbfFieldInfo.CreateBool("F1");
      Assert.AreEqual("F1", sut.Name, "Name");
      Assert.AreEqual('L', sut.Type, "Type");
      Assert.AreEqual(1, sut.Length, "Length");
      Assert.AreEqual(0, sut.Precision, "Precision");
    }

    [Test]
    public void CreateDate()
    {
      DbfFieldInfo sut = DbfFieldInfo.CreateDate("F1");
      Assert.AreEqual("F1", sut.Name, "Name");
      Assert.AreEqual('D', sut.Type, "Type");
      Assert.AreEqual(8, sut.Length, "Length");
      Assert.AreEqual(0, sut.Precision, "Precision");
    }

    [Test]
    public void CreateMemo()
    {
      DbfFieldInfo sut = DbfFieldInfo.CreateMemo("F1");
      Assert.AreEqual("F1", sut.Name, "Name");
      Assert.AreEqual('M', sut.Type, "Type");
      Assert.AreEqual(10, sut.Length, "Length");
      Assert.AreEqual(0, sut.Precision, "Precision");
    }

    #endregion

    #region Mask

    [TestCase(8, 2, "0.00")]
    [TestCase(4, 2, "0.00")]
    [TestCase(3, 1, "0.0")]
    [TestCase(4, 0, "0")]
    [TestCase(8, 0, "0")]
    public void Mask_numeric(int length, int precision, string wantedRes)
    {
      DbfFieldInfo sut = DbfFieldInfo.CreateNum("F1", length, precision);
      Assert.AreEqual(wantedRes, sut.Mask);
    }

    [TestCase('C', 10)]
    [TestCase('L', 1)]
    [TestCase('D', 8)]
    [TestCase('M', 10)]
    public void Mask_otherTypes(char type, int length)
    {
      DbfFieldInfo sut = new DbfFieldInfo("F1", type, length, 0);
      Assert.AreEqual("", sut.Mask);
    }

    #endregion

    #region DataType

    [TestCase('C', 10, 0, typeof(String))]
    [TestCase('N', 10, 0, typeof(Int64))]
    [TestCase('N', 9, 0, typeof(Int32))]
    [TestCase('N', 1, 0, typeof(Int32))]
    [TestCase('L', 1, 0, typeof(Boolean))]
    [TestCase('D', 8, 0, typeof(DateTime))]
    [TestCase('M', 10, 0, typeof(String))]
    public void DataType(char type, int length, int precision, Type wantedRes)
    {
      DbfFieldInfo sut = new DbfFieldInfo("F1", type, length, precision);
      Assert.AreEqual(wantedRes, sut.DataType);
    }

    #endregion

    #region TestFormat()

    [TestCase('C', 10, 0, true, true, true)]
    [TestCase('N', 10, 0, true, true, true)]
    [TestCase('L', 1, 0, true, true, true)]
    [TestCase('D', 8, 0,  false, true, true)]
    [TestCase('M', 10, 0, false, true, true)]
    // Тип 'F' пока не проверяю
    public void TestFormat(char type, int length, int precision, bool wanted2, bool wanted3, bool wanted4)
    {
      DbfFieldInfo sut = new DbfFieldInfo("F1", type, length, precision);

      string errorText;
      Assert.AreEqual(wanted2, sut.TestFormat(DbfFileFormat.dBase2, out errorText), "dBase2");
      Assert.AreEqual(wanted3, sut.TestFormat(DbfFileFormat.dBase3, out errorText), "dBase3");
      Assert.AreEqual(wanted4, sut.TestFormat(DbfFileFormat.dBase4, out errorText), "dBase4");
    }

    #endregion

    #region IsValidFieldName()

    [TestCase("", false)]
    [TestCase("A", true)]
    [TestCase("A234567890", true)]
    [TestCase("A2345678901", false)]
    [TestCase("123", false)]
    [TestCase("_123", true)]
    [TestCase("_", true)]
    [TestCase("A-1", false)]
    [TestCase("A 1", false)]
    public void IsValidFieldName(string fieldName, bool wantedRes)
    {
      Assert.AreEqual(wantedRes, DbfFieldInfo.IsValidFieldName(fieldName));
    }

    #endregion
  }
}
