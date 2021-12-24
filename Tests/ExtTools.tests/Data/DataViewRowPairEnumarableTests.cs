using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using NUnit.Framework;
using System.Data;
using FreeLibSet.Core;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class DataViewRowPairEnumarableTests
  {
    #region Тестовая таблица

    private static DataTable CreateTestTable()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Columns.Add("F2", typeof(string));
      tbl.Columns.Add("F3", typeof(string));

      tbl.Rows.Add(1, "AAA", "a"); // 0
      tbl.Rows.Add(1, "BBB", "b"); // 3
      tbl.Rows.Add(2, "BBB", "c"); // 4
      tbl.Rows.Add(3, "CCC", "d"); // 6
      tbl.Rows.Add(1, "AAA", "e"); // 1
      tbl.Rows.Add(3, "CCC", "f"); // 7
      tbl.Rows.Add(4, "DDD", "g"); // 8
      tbl.Rows.Add(2, "BBB", "h"); // 5
      tbl.Rows.Add(1, "AAA", "i"); // 2

      tbl.DefaultView.Sort = "F1,F2";

      // Внутренняя проверка
      StringBuilder sb = new StringBuilder();
      foreach (DataRowView drv in tbl.DefaultView)
        sb.Append(drv.Row["F3"]);

      Assert.AreEqual("aeibchdfg", sb.ToString(), "CreateTestTable()");

      return tbl;
    }

    #endregion

    [Test]
    public void Constructor_simple()
    {
      DataTable tbl = CreateTestTable();
      DataViewRowPairEnumarable sut = new DataViewRowPairEnumarable(tbl.DefaultView);
      Assert.AreEqual("3aei2ch2df", TestEnum(sut));
    }

    [Test]
    public void Constructor_twocolumns()
    {
      DataTable tbl = CreateTestTable();
      DataViewRowPairEnumarable sut = new DataViewRowPairEnumarable(tbl.DefaultView, 2);
      Assert.AreEqual("3aei2ch2df", TestEnum(sut));
    }

    [Test]
    public void Constructor_onecolumn()
    {
      DataTable tbl = CreateTestTable();
      DataViewRowPairEnumarable sut = new DataViewRowPairEnumarable(tbl.DefaultView, 1);
      Assert.AreEqual("4aeib2ch2df", TestEnum(sut));
    }

    [Test]
    public void Constructor_twocolumns_nosinglerows()
    {
      DataTable tbl = CreateTestTable();
      DataViewRowPairEnumarable sut = new DataViewRowPairEnumarable(tbl.DefaultView, 2, false);
      Assert.AreEqual("3aei2ch2df", TestEnum(sut));
    }

    [Test]
    public void Constructor_twocolumns_enumsinglerows()
    {
      DataTable tbl = CreateTestTable();
      DataViewRowPairEnumarable sut = new DataViewRowPairEnumarable(tbl.DefaultView, 2, true);
      Assert.AreEqual("3aei1b2ch2df1g", TestEnum(sut));
    }

    [Test]
    public void Constructor_onecolumn_enumsinglerows()
    {
      DataTable tbl = CreateTestTable();
      DataViewRowPairEnumarable sut = new DataViewRowPairEnumarable(tbl.DefaultView, 1, true);
      Assert.AreEqual("4aeib2ch2df1g", TestEnum(sut));
    }

    [Test]
    public void Constructor_exceptions()
    {
      DataTable tbl = CreateTestTable();

      Assert.Catch(delegate { new DataViewRowPairEnumarable(null); }, "DataView=null");
      Assert.Catch(delegate { new DataViewRowPairEnumarable(tbl.DefaultView, -1); }, "ColumnCount=-1");
      Assert.Catch(delegate { new DataViewRowPairEnumarable(tbl.DefaultView, 3); }, "ColumnCount=3");
    }

    /// <summary>
    /// Создает строку вида "ДлинаГруппы1 F3[0] F3[1] ... ДлинаГруппы2 F3[0] F3[1] ... ..."
    /// </summary>
    /// <param name="sut"></param>
    /// <returns></returns>
    private static string TestEnum(DataViewRowPairEnumarable sut)
    {
      StringBuilder sb = new StringBuilder();
      foreach (DataRow[] rows in sut)
      {
        sb.Append(StdConvert.ToString(rows.Length));
        for (int j = 0; j < rows.Length; j++)
          sb.Append(rows[j]["F3"]);
      }
      return sb.ToString();
    }
  }
}
