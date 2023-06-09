using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;

// Тестируется только класс DBxDataWriterInfo, без выполнения реальной записи данных

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DBxDataWriterInfoTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DBxDataWriterInfo sut = new DBxDataWriterInfo();
      Assert.AreEqual(DBxDataWriterMode.Insert, sut.Mode, "Mode");
      //Assert.IsTrue(String.IsNullOrEmpty(sut.TableName), "TableName");
      Assert.AreEqual(0L, sut.ExpectedRowCount, "ExpectedRowCount");
      Assert.AreEqual(0, sut.TransactionPulseRowCount, "TransactionPulseRowCount");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.DoesNotThrow(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
    }

    #endregion

    #region SetReadOnly()

    [Test]
    public void SetReadOnly()
    {
      DBxDataWriterInfo sut = new DBxDataWriterInfo();
      sut.SetReadOnly();
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.TableName = "T1"; }, "TableName");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Columns = new DBxColumns("C1"); }, "Columns");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.SearchColumns = new DBxColumns("C1"); }, "SearchColumns");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Mode = DBxDataWriterMode.Update; }, "Mode");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.ExpectedRowCount = 1000; }, "ExpectedRowCount");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.TransactionPulseRowCount = 10000; }, "TransactionPulseRowCount");
      Assert.DoesNotThrow(delegate () { sut.SetReadOnly(); }, "Second SetReadOnly() call");
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone()
    {
      DBxDataWriterInfo sut = new DBxDataWriterInfo();
      sut.TableName = "T1";
      sut.Mode = DBxDataWriterMode.Update;
      sut.Columns = new DBxColumns("Id,Name");
      sut.SearchColumns = new DBxColumns("Id");
      sut.ExpectedRowCount = 1234567L;
      sut.TransactionPulseRowCount = 12345;
      sut.SetReadOnly();

      DBxDataWriterInfo res = sut.Clone();

      Assert.AreEqual("T1", res.TableName, "TableName");
      Assert.AreEqual(DBxDataWriterMode.Update, res.Mode, "Mode");
      Assert.AreEqual(new DBxColumns("Id,Name"), res.Columns, "Columns");
      Assert.AreEqual(new DBxColumns("Id"), res.SearchColumns, "SearchColumns");
      Assert.AreEqual(1234567L, res.ExpectedRowCount, "ExpectedRowCount");
      Assert.AreEqual(12345, res.TransactionPulseRowCount, "TransactionPulseRowCount");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
    }

    #endregion
  }
}
