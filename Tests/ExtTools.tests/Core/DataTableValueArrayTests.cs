using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using NUnit.Framework;
using System.Data;
using FreeLibSet.Data;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class DataTableValueArrayTests
  {
    [Test]
    public void Constructor_ok()
    {
      DataTable tbl = CreateTestTable();
      DataTableValueArray sut = new DataTableValueArray(tbl);
      Assert.AreSame(tbl, sut.Table, "Table");
      Assert.IsNull(sut.CurrentRow, "CurrentRow");
    }

    private static DataTable CreateTestTable()
    {
      DataTable tbl = new DataTable("T1");
      tbl.Columns.Add("F1", typeof(int));
      tbl.Columns.Add("F2", typeof(string));
      tbl.Rows.Add(1, "AAA");
      tbl.Rows.Add(2, "BBB");
      return tbl;
    }

    [Test]
    public void Constructor_exception()
    {
      Assert.Catch(delegate() { new DataTableValueArray(null); });
    }

    [Test]
    public void GetNames()
    {
      DataTableValueArray sut = new DataTableValueArray(CreateTestTable());
      string[] res = sut.GetNames();
      Assert.AreEqual(new string[] { "F1", "F2" }, res);
    }

    [TestCase("F1", true)]
    [TestCase("F2", true)]
    [TestCase("F0", false)]
    [TestCase("", false)]
    public void Contains(string name, bool wanted)
    {
      DataTableValueArray sut = new DataTableValueArray(CreateTestTable());
      bool res = sut.Contains(name);
      Assert.AreEqual(wanted, res);
    }

    [Test]
    public void GetValue()
    {
      DataTable tbl=CreateTestTable();
      DataTableValueArray sut = new DataTableValueArray(tbl);
      sut.CurrentRow = tbl.Rows[0];
      Assert.AreEqual(1, sut.GetValue("F1"), "#1");
      Assert.AreEqual("AAA", sut.GetValue("F2"), "#2");

      sut.CurrentRow = tbl.Rows[1];
      Assert.AreEqual("BBB", sut.GetValue("F2"), "#3");
    }
  }
}
