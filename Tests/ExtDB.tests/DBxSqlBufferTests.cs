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
  public class DBxSqlBufferTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_default()
    {
      DBxSqlBuffer sut = new DBxSqlBuffer();
      Assert.IsInstanceOf<DataViewDBxSqlFormatter>(sut.Formatter, "Formatter");
      Assert.AreEqual(0, sut.SB.Length, "SB.Length");
    }

    [Test]
    public void Constructor_formatter()
    {
      DataViewDBxSqlFormatter formatter = new DataViewDBxSqlFormatter();
      DBxSqlBuffer sut = new DBxSqlBuffer(formatter);
      Assert.AreSame(formatter, sut.Formatter, "Formatter");
      Assert.AreEqual(0, sut.SB.Length, "SB.Length");
    }

    #endregion
  }
}
