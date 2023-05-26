using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.Remoting;

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DBxColumnValueArrayTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_1()
    {
      DBxColumns columns = new DBxColumns("F1,F2,F3");
      DBxColumnValueArray sut = new DBxColumnValueArray(columns);
      Assert.AreEqual(columns.AsString, sut.Columns.AsString, "Columns");
      Assert.AreEqual(new object[3], sut.Values, "Values");
      Assert.IsNull(sut["F1"], "[F1]");
      Assert.IsNull(sut["F2"], "[F2]");
      Assert.IsNull(sut["F3"], "[F3]");
    }

    [Test]
    public void Constructor_2()
    {
      DBxColumns columns = new DBxColumns("F1,F2,F3");
      object[] values = new object[] { "AAA", 222, true };
      DBxColumnValueArray sut = new DBxColumnValueArray(columns, values);
      Assert.AreEqual(columns.AsString, sut.Columns.AsString, "Columns");
      Assert.AreEqual(values, sut.Values, "Values");
      Assert.AreEqual("AAA", sut["F1"], "[F1]");
      Assert.AreEqual(222, sut["F2"], "[F2]");
      Assert.AreEqual(true, sut["F3"], "[F3]");
    }

    #endregion

    #region Свойство Values

    [Test]
    public void Values()
    {
      DBxColumns columns = new DBxColumns("F1,F2,F3");
      DBxColumnValueArray sut = new DBxColumnValueArray(columns);
      object[] values1 = new object[] { "AAA", 222, true };
      sut.Values = values1;
      Assert.AreEqual(values1, sut.Values, "#1");

      object[] values2 = new object[] { "BBB", 333, false };
      sut.Values = values2;
      Assert.AreEqual(values2, sut.Values, "#2");
    }

    #endregion

    #region INamedValuesAccess members

    [Test]
    public void INamedValuesAccess_GetNames()
    {
      DBxColumns columns = new DBxColumns("F1,F2,F3");
      DBxColumnValueArray sut = new DBxColumnValueArray(columns);
      string[] res = ((INamedValuesAccess)sut).GetNames();
      Assert.AreEqual(columns.AsArray, res);
    }

    [Test]
    public void INamedValuesAccess_Contains()
    {
      DBxColumns columns = new DBxColumns("F1,F2,F3");
      DBxColumnValueArray sut = new DBxColumnValueArray(columns);
      Assert.IsTrue(((INamedValuesAccess)sut).Contains("F2"), "F2");
      Assert.IsFalse(((INamedValuesAccess)sut).Contains("F4"), "F4");
    }

    [Test]
    public void INamedValuesAccess_GetValue()
    {
      DBxColumns columns = new DBxColumns("F1,F2,F3");
      object[] values = new object[] { "AAA", 222, true };
      DBxColumnValueArray sut = new DBxColumnValueArray(columns, values);
      Assert.AreEqual(222, ((INamedValuesAccess)sut).GetValue("F2"), "F2");
    }

    #endregion
  }
}
