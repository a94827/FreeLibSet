using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.UICore;
using System.Data;
using FreeLibSet.Core;

namespace ExtTools_tests.UICore
{
  [TestFixture]
  public class DataTableReorderHelperTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DataTable tbl = CreateTestTable();
      tbl.DefaultView.Sort = "Text";

      DataTableReorderHelper sut = new DataTableReorderHelper(tbl.DefaultView, "Order");

      Assert.AreSame(tbl.DefaultView, sut.DV, "DV");
      Assert.AreEqual("Order", sut.OrderColumnName, "OrderColumnName"); // а не "Text"
      Assert.AreEqual("AAA,DDD,CCC,BBB", GetOrderedText(tbl), "Order unchanged");
    }

    [Test]
    public void Constructor_Exceptions()
    {
      DataTableReorderHelper sut;

      Assert.Catch(delegate() { sut = new DataTableReorderHelper(null, "Order"); }, "DV=null");

      DataTable tbl = CreateTestTable();
      Assert.Catch(delegate() { sut = new DataTableReorderHelper(tbl.DefaultView, "Order2"); }, "Unknown OrderColumnName");
    }

    #endregion

    #region MoveUp()/MoveDown()

    [TestCase("AAA", true, "DDD,AAA,CCC,BBB")]
    [TestCase("BBB", false, "AAA,DDD,CCC,BBB")]
    [TestCase("CCC", true, "AAA,DDD,BBB,CCC")]
    [TestCase("DDD", true, "AAA,CCC,DDD,BBB")]
    [TestCase("AAA,BBB", false, "AAA,DDD,CCC,BBB")]
    [TestCase("BBB,CCC", false, "AAA,DDD,CCC,BBB")]
    [TestCase("AAA,DDD", true, "CCC,AAA,DDD,BBB")]
    [TestCase("AAA,CCC", true, "DDD,AAA,BBB,CCC")]
    [TestCase("", false, "AAA,DDD,CCC,BBB")]
    public void MoveDown(string sRowValues, bool wantedRes, string sWantedValues)
    {
      // Перемещение для нескольких строк должно работать независимо от порядка задания выбранных строк
      for (int i = 1; i <= 2; i++)
      {
        DataTable tbl = CreateTestTable();
        DataTableReorderHelper sut = new DataTableReorderHelper(tbl.DefaultView, "Order");

        DataRow[] rows = GetTableRows(tbl, sRowValues);
        if (i == 2)
          Array.Reverse(rows);

        bool res = sut.MoveDown(rows);

        Assert.AreEqual(wantedRes, res, "Result #" + i.ToString());

        string sResValues = GetOrderedText(tbl);
        Assert.AreEqual(sWantedValues, sResValues, "Rows #" + i.ToString());
      }
    }

    [TestCase("AAA", false, "AAA,DDD,CCC,BBB")]
    [TestCase("BBB", true, "AAA,DDD,BBB,CCC")]
    [TestCase("CCC", true, "AAA,CCC,DDD,BBB")]
    [TestCase("DDD", true, "DDD,AAA,CCC,BBB")]
    [TestCase("AAA,BBB", false, "AAA,DDD,CCC,BBB")]
    [TestCase("BBB,CCC", true, "AAA,CCC,BBB,DDD")]
    [TestCase("AAA,DDD", false, "AAA,DDD,CCC,BBB")]
    [TestCase("AAA,CCC", false, "AAA,DDD,CCC,BBB")]
    [TestCase("", false, "AAA,DDD,CCC,BBB")]
    public void MoveUp(string sRowValues, bool wantedRes, string sWantedValues)
    {
      // Перемещение для нескольких строк должно работать независимо от порядка задания выбранных строк
      for (int i = 1; i <= 2; i++)
      {
        DataTable tbl = CreateTestTable();
        DataTableReorderHelper sut = new DataTableReorderHelper(tbl.DefaultView, "Order");

        DataRow[] rows = GetTableRows(tbl, sRowValues);
        if (i == 2)
          Array.Reverse(rows);

        bool res = sut.MoveUp(rows);

        Assert.AreEqual(wantedRes, res, "Result #" + i.ToString());

        string sResValues = GetOrderedText(tbl);
        Assert.AreEqual(sWantedValues, sResValues, "Rows #" + i.ToString());
      }
    }

    #endregion

    #region InitRows()

    [Test]
    public void InitRows_Ok()
    {
      DataTable tbl = CreateTestTable();
      DataTableReorderHelper sut = new DataTableReorderHelper(tbl.DefaultView, "Order");
      DataRow rowCCC = tbl.Rows[2]; // "CCC"
      rowCCC["Order"] = DBNull.Value;
      bool otherRowChanged;
      bool res = sut.InitRows(new DataRow[] { rowCCC }, out otherRowChanged);
      Assert.IsTrue(res, "Result");
      Assert.IsFalse(otherRowChanged, "otherRowChanged");
      Assert.AreEqual("AAA,DDD,BBB,CCC", GetOrderedText(tbl), "Rows");
    }

    [Test]
    public void InitRows_None()
    {
      DataTable tbl = CreateTestTable();
      DataTableReorderHelper sut = new DataTableReorderHelper(tbl.DefaultView, "Order");
      DataRow rowCCC = tbl.Rows[2]; // "CCC"
      bool otherRowChanged;
      bool res = sut.InitRows(new DataRow[] { rowCCC }, out otherRowChanged);
      Assert.IsFalse(res, "Result");
      Assert.IsFalse(otherRowChanged, "otherRowChanged");
      Assert.AreEqual("AAA,DDD,CCC,BBB", GetOrderedText(tbl), "Rows"); // ничего не поменялось
    }

    [Test]
    public void InitRows_Mixed()
    {
      DataTable tbl = CreateTestTable();
      DataTableReorderHelper sut = new DataTableReorderHelper(tbl.DefaultView, "Order");
      DataRow rowDDD = tbl.Rows[1]; // "DDD"
      DataRow rowCCC = tbl.Rows[2]; // "CCC"
      rowCCC["Order"] = DBNull.Value;
      bool otherRowChanged;
      bool res = sut.InitRows(new DataRow[] { rowDDD, rowCCC }, out otherRowChanged);
      Assert.IsTrue(res, "Result");
      Assert.IsFalse(otherRowChanged, "otherRowChanged");
      Assert.AreEqual("AAA,DDD,BBB,CCC", GetOrderedText(tbl), "Rows"); // только "CCC" переместилось
    }

    #endregion

    #region Reorder()

    [Test]
    public void Reorder_Simple()
    {
      DataTable tbl = CreateTestTable();
      DataTableReorderHelper sut = new DataTableReorderHelper(tbl.DefaultView, "Order");

      bool res1 = sut.Reorder();
      Assert.IsTrue(res1, "Result #1");
      Assert.AreEqual("AAA,DDD,CCC,BBB", GetOrderedText(tbl), "Rows #1"); // ничего не поменялось

      bool res2 = sut.Reorder();
      Assert.IsFalse(res2, "Result #2");
      Assert.AreEqual("AAA,DDD,CCC,BBB", GetOrderedText(tbl), "Rows #2"); // ничего не поменялось
    }

    [Test]
    public void Reorder_ByRows()
    {
      DataTable tbl = CreateTestTable();
      DataTableReorderHelper sut = new DataTableReorderHelper(tbl.DefaultView, "Order");

      DataRow rowDDD = tbl.Rows[3]; // "DDD"
      DataRow rowCCC = tbl.Rows[2]; // "CCC"
      DataRow rowBBB = tbl.Rows[1]; // "BBB"
      DataRow rowAAA = tbl.Rows[0]; // "AAA"
      DataRow[] rows = new DataRow[] { rowDDD, rowCCC, rowBBB, rowAAA };

      bool res1 = sut.Reorder(rows);
      Assert.IsTrue(res1, "Result #1");
      Assert.AreEqual("DDD,CCC,BBB,AAA", GetOrderedText(tbl), "Rows #1");

      bool res2 = sut.Reorder(rows);
      Assert.IsFalse(res2, "Result #2");
      Assert.AreEqual("DDD,CCC,BBB,AAA", GetOrderedText(tbl), "Rows #2"); // ничего не поменялось
    }

    #endregion

    #region Тестовая таблица

    private static DataTable CreateTestTable()
    {
      DataTable tbl = new DataTable("TestTable");
      tbl.Columns.Add("Text", typeof(string));
      tbl.Columns.Add("Order", typeof(Int16));
      tbl.Rows.Add("AAA", 10); // [0]
      tbl.Rows.Add("BBB", 40); // [1]
      tbl.Rows.Add("CCC", 30); // [2]
      tbl.Rows.Add("DDD", 20); // [3]
      tbl.DefaultView.Sort = "Order";

      // Текущий порядок сортировки: "AAA,DDD,CCC,BBB" (10,20,30,40)
      return tbl;
    }

    private static DataRow[] GetTableRows(DataTable tbl, string sRowValues)
    {
      if (String.IsNullOrEmpty(sRowValues))
        return new DataRow[0];

      string[] orgRowValues = new string[tbl.Rows.Count];
      for (int i = 0; i < tbl.Rows.Count; i++)
        orgRowValues[i] = tbl.Rows[i]["Text"].ToString();

      string[] aValues = sRowValues.Split(',');
      DataRow[] rows = new DataRow[aValues.Length];
      for (int i = 0; i < aValues.Length; i++)
      {
        int p = Array.IndexOf<string>(orgRowValues, aValues[i]);
        if (p < 0)
          throw new BugException("Значения \"" + aValues[i] + "\" нет в таблице");
        rows[i] = tbl.Rows[p];
      }
      return rows;
    }

    private static string GetOrderedText(DataTable tbl)
    {
      StringBuilder sb = new StringBuilder();
      using (DataView dv = new DataView(tbl))
      {
        dv.Sort = "Order";
        for (int i = 0; i < dv.Count; i++)
        {
          if (i > 0)
            sb.Append(',');
          string text = dv[i].Row["Text"].ToString();
          sb.Append(text);
        }
      }
      return sb.ToString();
    }

    #endregion
  }
}

