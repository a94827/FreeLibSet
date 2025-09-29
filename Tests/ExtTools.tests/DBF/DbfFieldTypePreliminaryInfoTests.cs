using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DBF;

namespace ExtTools_tests.DBF
{
  class DbfFieldTypePreliminaryInfoTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_default()
    {
      DbfFieldTypePreliminaryInfo sut = new DbfFieldTypePreliminaryInfo();
      Assert.AreEqual(' ', sut.Type, "Type");
      Assert.AreEqual(0, sut.Length, "Length");
      Assert.AreEqual(0, sut.Precision, "Precision");
      Assert.IsFalse(sut.LengthIsDefined, "LengthIsDefined");
      Assert.IsFalse(sut.PrecisionIsDefined, "PrecisionIsDefined");
    }

    [Test]
    public void Constructor_DbfFieldInfo_isEmpty()
    {
      DbfFieldInfo fi = new DbfFieldInfo();
      DbfFieldTypePreliminaryInfo sut = new DbfFieldTypePreliminaryInfo(fi);
      Assert.AreEqual(' ', sut.Type, "Type");
      Assert.AreEqual(0, sut.Length, "Length");
      Assert.AreEqual(0, sut.Precision, "Precision");
      Assert.IsFalse(sut.LengthIsDefined, "LengthIsDefined");
      Assert.IsFalse(sut.PrecisionIsDefined, "PrecisionIsDefined");
    }

    [Test]
    public void Constructor_DbfFieldInfo_C()
    {
      DbfFieldInfo fi = DbfFieldInfo.CreateString("F1", 123);
      DbfFieldTypePreliminaryInfo sut = new DbfFieldTypePreliminaryInfo(fi);
      Assert.AreEqual('C', sut.Type, "Type");
      Assert.AreEqual(123, sut.Length, "Length");
      Assert.IsTrue(sut.LengthIsDefined, "LengthIsDefined");
    }

    [Test]
    public void Constructor_DbfFieldInfo_N()
    {
      DbfFieldInfo fi = DbfFieldInfo.CreateNum("F1", 12, 2);
      DbfFieldTypePreliminaryInfo sut = new DbfFieldTypePreliminaryInfo(fi);
      Assert.AreEqual('N', sut.Type, "Type");
      Assert.AreEqual(12, sut.Length, "Length");
      Assert.AreEqual(2, sut.Precision, "Precision");
      Assert.IsTrue(sut.LengthIsDefined, "LengthIsDefined");
      Assert.IsTrue(sut.PrecisionIsDefined, "PrecisionIsDefined");
    }

    [Test]
    public void Constructor_DbfFieldInfo_D()
    {
      DbfFieldInfo fi = DbfFieldInfo.CreateDate("F1");
      DbfFieldTypePreliminaryInfo sut = new DbfFieldTypePreliminaryInfo(fi);
      Assert.AreEqual('D', sut.Type, "Type");
    }

    [Test]
    public void Constructor_DbfFieldInfo_L()
    {
      DbfFieldInfo fi = DbfFieldInfo.CreateBoolean("F1");
      DbfFieldTypePreliminaryInfo sut = new DbfFieldTypePreliminaryInfo(fi);
      Assert.AreEqual('L', sut.Type, "Type");
    }

    [Test]
    public void Constructor_DbfFieldInfo_M()
    {
      DbfFieldInfo fi = DbfFieldInfo.CreateMemo("F1");
      DbfFieldTypePreliminaryInfo sut = new DbfFieldTypePreliminaryInfo(fi);
      Assert.AreEqual('M', sut.Type, "Type");
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone_CopyTo([Values(false,true)]bool lengthIsDefined,
    [Values(false, true)] bool precisionIsDefined)
    {
      DbfFieldTypePreliminaryInfo sut = new DbfFieldTypePreliminaryInfo();
      sut.Type = 'N';
      sut.Length = 12;
      sut.LengthIsDefined = lengthIsDefined;
      sut.PrecisionIsDefined = precisionIsDefined;

      DbfFieldTypePreliminaryInfo res1 = sut.Clone();
      Assert.AreNotSame(sut, res1, "Not same");
      DoCompare(sut, res1);

      DbfFieldTypePreliminaryInfo res2 = new DbfFieldTypePreliminaryInfo();
      sut.CopyTo(res2);
      DoCompare(sut, res2);
    }

    private static void DoCompare(DbfFieldTypePreliminaryInfo wanted, DbfFieldTypePreliminaryInfo actual)
    {
      Assert.AreEqual(wanted.Type, actual.Type, "Type");
      Assert.AreEqual(wanted.Length, actual.Length, "Length");
      Assert.AreEqual(wanted.Precision, actual.Precision, "Precision");
      Assert.AreEqual(wanted.LengthIsDefined, actual.LengthIsDefined, "LengthIsDefined");
      Assert.AreEqual(wanted.PrecisionIsDefined, actual.PrecisionIsDefined, "PrecisionIsDefined");
    }

    #endregion
  }
}
