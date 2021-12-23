using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Data;
using FreeLibSet.Data;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class DataRowValueArrayTests
  {
    [Test]
    public void Constructor()
    {
      DataRowValueArray sut = new DataRowValueArray();
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
      Assert.AreEqual(0, sut.GetNames().Length, "GetNames()");
    }


    private static DataTable CreateTestTable1()
    {
      DataTable tbl = new DataTable("T1");
      tbl.Columns.Add("F1", typeof(int));
      tbl.Columns.Add("F2", typeof(string));
      tbl.Rows.Add(1, "AAA");
      tbl.Rows.Add(2, "BBB");
      return tbl;
    }

    private static DataTable CreateTestTable2()
    {
      DataTable tbl = new DataTable("T2");
      tbl.Columns.Add("F2", typeof(string));
      tbl.Columns.Add("F3", typeof(int));
      tbl.Rows.Add("CCC", 3);
      tbl.Rows.Add("DDD", 4);
      return tbl;
    }

    [Test]
    public void GetNames()
    {
      DataTable tbl1 = CreateTestTable1();
      DataTable tbl2 = CreateTestTable2();
      DataRowValueArray sut = new DataRowValueArray();

      sut.CurrentRow = tbl1.Rows[0];
      string[] res1 = sut.GetNames();
      Assert.AreEqual(new string[] { "F1", "F2" }, res1, "#1");

      sut.CurrentRow = tbl2.Rows[0];
      string[] res2 = sut.GetNames();
      Assert.AreEqual(new string[] { "F2", "F3" }, res2, "#2");
    }

    [Test]
    public void Contains()
    {
      DataTable tbl1 = CreateTestTable1();
      DataTable tbl2 = CreateTestTable2();
      DataRowValueArray sut = new DataRowValueArray();
      Assert.IsFalse(sut.Contains("F1"), "#1");

      sut.CurrentRow = tbl1.Rows[0];
      Assert.IsTrue(sut.Contains("F1"), "#2");

      sut.CurrentRow = tbl2.Rows[0];
      Assert.IsFalse(sut.Contains("F1"), "#3");
    }

    [Test]
    public void GetValue()
    {
      DataTable tbl1 = CreateTestTable1();
      DataTable tbl2 = CreateTestTable2();
      DataRowValueArray sut = new DataRowValueArray();

      sut.CurrentRow = tbl1.Rows[0];
      Assert.AreEqual("AAA", sut.GetValue("F2"), "#1");
      sut.CurrentRow = tbl1.Rows[1];
      Assert.AreEqual("BBB", sut.GetValue("F2"), "#2");
      sut.CurrentRow = tbl2.Rows[0];
      Assert.AreEqual("CCC", sut.GetValue("F2"), "#3");
      sut.CurrentRow = tbl2.Rows[1];
      Assert.AreEqual("DDD", sut.GetValue("F2"), "#4");
    }
  }
}
