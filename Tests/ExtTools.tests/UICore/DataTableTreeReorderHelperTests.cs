using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.UICore;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Models.Tree;

namespace ExtTools_tests.UICore
{
  [TestFixture]
  public class DataTableTreeReorderHelperTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DataTableTreeReorderHelper sut = CreateTestObject();

      Assert.AreEqual("Order", sut.OrderColumnName, "OrderColumnName"); // а не "Text"
    }

    [Test]
    public void Constructor_Exceptions()
    {
      DataTableTreeReorderHelper sut;

      Assert.Catch(delegate() { sut = new DataTableTreeReorderHelper(null, "Order"); }, "Model=null");

      DataTable tbl = CreateTestTable();
      DataTableTreeModel model = new DataTableTreeModel(tbl, "Id", "ParentId");
      Assert.Catch(delegate() { sut = new DataTableTreeReorderHelper(model, "Order2"); }, "Unknown OrderColumnName");
    }

    #endregion

    #region MoveUp()/MoveDown()

    [TestCase("AAA", true, "DDD,EEE,FFF,GGG,HHH,AAA,BBB,CCC")]
    [TestCase("BBB", true, "AAA,CCC,BBB,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("CCC", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("DDD", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("EEE", true, "AAA,BBB,CCC,DDD,GGG,EEE,FFF,HHH")]
    [TestCase("FFF", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("GGG", true, "AAA,BBB,CCC,DDD,EEE,FFF,HHH,GGG")]
    [TestCase("HHH", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("AAA,DDD", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("BBB,CCC", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("EEE,GGG", true, "AAA,BBB,CCC,DDD,HHH,EEE,FFF,GGG")]
    [TestCase("EEE,HHH", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("GGG,HHH", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    // Не проверяем возможность сдвига для разноуровневых узлов, т.к. может в будущей версии будет разрешено
    //[TestCase("BBB,EEE", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    public void MoveDown(string sRowValues, bool wantedRes, string sWantedValues)
    {
      // Перемещение для нескольких строк должно работать независимо от порядка задания выбранных строк
      for (int i = 1; i <= 2; i++)
      {
        DataTableTreeReorderHelper sut = CreateTestObject();

        DataRow[] rows = GetTableRows(sut.Model.Table, sRowValues);
        if (i == 2)
          Array.Reverse(rows);

        bool res = sut.MoveDown(rows);

        Assert.AreEqual(wantedRes, res, "Result #" + i.ToString());

        string sResValues = GetOrderedText(sut.Model.Table);
        Assert.AreEqual(sWantedValues, sResValues, "Rows #" + i.ToString());
      }
    }

    [TestCase("AAA", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("BBB", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("CCC", true, "AAA,CCC,BBB,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("DDD", true, "DDD,EEE,FFF,GGG,HHH,AAA,BBB,CCC")]
    [TestCase("EEE", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("FFF", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("GGG", true, "AAA,BBB,CCC,DDD,GGG,EEE,FFF,HHH")]
    [TestCase("HHH", true, "AAA,BBB,CCC,DDD,EEE,FFF,HHH,GGG")]
    [TestCase("AAA,DDD", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("BBB,CCC", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("EEE,GGG", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("EEE,HHH", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("GGG,HHH", true, "AAA,BBB,CCC,DDD,GGG,HHH,EEE,FFF")]
    [TestCase("", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    public void MoveUp(string sRowValues, bool wantedRes, string sWantedValues)
    {
      // Перемещение для нескольких строк должно работать независимо от порядка задания выбранных строк
      for (int i = 1; i <= 2; i++)
      {
        DataTableTreeReorderHelper sut = CreateTestObject();

        DataRow[] rows = GetTableRows(sut.Model.Table, sRowValues);
        if (i == 2)
          Array.Reverse(rows);

        bool res = sut.MoveUp(rows);

        Assert.AreEqual(wantedRes, res, "Result #" + i.ToString());

        string sResValues = GetOrderedText(sut.Model.Table);
        Assert.AreEqual(sWantedValues, sResValues, "Rows #" + i.ToString());
      }
    }

    #endregion

    #region InitRows()

    [TestCase("AAA", "AAA", true, "DDD,EEE,FFF,GGG,HHH,AAA,BBB,CCC")]
    [TestCase("AAA", "", false, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("DDD", "DDD", true, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")] // хотя порядок и не поменялся
    [TestCase("AAA,DDD", "AAA,DDD", true, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase("DDD,AAA", "AAA,DDD", true, "DDD,EEE,FFF,GGG,HHH,AAA,BBB,CCC")]
    [TestCase("AAA,EEE", "AAA,EEE", true, "DDD,GGG,HHH,EEE,FFF,AAA,BBB,CCC")]
    [TestCase("AAA,EEE", "EEE", true, "AAA,BBB,CCC,DDD,GGG,HHH,EEE,FFF")]
    public void InitRows(string sRowValues, string sZeroRowValues, bool wantedRes, string sWantedValues)
    {
      DataTableTreeReorderHelper sut = CreateTestObject();
      DataRow[] rows = GetTableRows(sut.Model.Table, sRowValues);

      // Обнуляем некоторые строки
      DataRow[] zeroRows = GetTableRows(sut.Model.Table, sZeroRowValues);
      for (int i = 0; i < zeroRows.Length; i++)
        zeroRows[i]["Order"] = DBNull.Value;
      
      bool otherRowChanged;
      bool res = sut.InitRows(rows, out otherRowChanged);
      Assert.AreEqual(wantedRes, res, "Result");
      Assert.AreEqual(sWantedValues, GetOrderedText(sut.Model.Table), "Rows");

      // Не проверяем otherRowChanged, т.к. непредсказуемо
      // Assert.IsFalse(otherRowChanged, "otherRowChanged");
    }

    #endregion

    #region Reorder()

    [Test]
    public void Reorder_Simple()
    {
      DataTableTreeReorderHelper sut = CreateTestObject();


      bool res1 = sut.Reorder();
      Assert.IsTrue(res1, "Result #1");
      Assert.AreEqual("AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH", GetOrderedText(sut.Model.Table), "Rows #1"); // ничего не поменялось

      bool res2 = sut.Reorder();
      Assert.IsFalse(res2, "Result #2");
      Assert.AreEqual("AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH", GetOrderedText(sut.Model.Table), "Rows #2"); // ничего не поменялось
    }

    [TestCase("AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH", "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")] // не меняем
    [TestCase("FFF,BBB,AAA,DDD,CCC,EEE,GGG,HHH", "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")] // относительно групп порядок сохранился
    [TestCase("DDD,BBB,CCC,AAA,EEE,FFF,GGG,HHH", "DDD,EEE,FFF,GGG,HHH,AAA,BBB,CCC")] // перестановка "AAA"-"DDD"
    [TestCase("HHH,GGG,FFF,EEE,DDD,CCC,BBB,AAA", "DDD,HHH,GGG,EEE,FFF,AAA,CCC,BBB")] // обратный порядок
    public void Reorder_ByRows(string sRowValues, string sWantedValues)
    {
      DataTableTreeReorderHelper sut = CreateTestObject();
      DataRow[] rows = GetTableRows(sut.Model.Table, sRowValues);

      bool res1 = sut.Reorder(rows);
      Assert.IsTrue(res1, "Result #1");
      Assert.AreEqual(sWantedValues, GetOrderedText(sut.Model.Table), "Rows #1");

      bool res2 = sut.Reorder(rows);
      // В текущей реализации и при повторном вызове может вернуть true.
      //Assert.IsFalse(res2, "Result #2");
      Assert.AreEqual(sWantedValues, GetOrderedText(sut.Model.Table), "Rows #2"); // ничего не поменялось
    }

    #endregion

    #region Тестовая таблица

    private static DataTable CreateTestTable()
    {
      // Дерево:
      // AAA
      //   BBB
      //   CCC
      // DDD
      //   EEE
      //     FFF
      //   GGG
      //   HHH

      // Нельзя использовать значения не по порядку для поля "Order"
      // Модель работать будет, но тестовый метод GetOrderedText() будет возвращать неправильные результаты

      DataTable tbl = new DataTable("TestTable");
      tbl.Columns.Add("Text", typeof(string));
      tbl.Columns.Add("Order", typeof(Int16));
      tbl.Columns.Add("Id", typeof(Int32));
      tbl.Columns.Add("ParentId", typeof(Int32));
      tbl.Rows.Add("AAA", 10, 1, DBNull.Value); // [0]
      tbl.Rows.Add("BBB", 20, 2, 1); // [1]
      tbl.Rows.Add("CCC", 30,3, 1); // [2]
      tbl.Rows.Add("DDD", 40, 4, DBNull.Value); // [3]
      tbl.Rows.Add("EEE", 50, 5, 4); // [4]
      tbl.Rows.Add("FFF", 60, 6, 5); // [5]
      tbl.Rows.Add("GGG", 70, 7, 4); // [6]
      tbl.Rows.Add("HHH", 80, 8, 4); // [7]
      tbl.DefaultView.Sort = "Order";

      return tbl;
    }

    private static DataTableTreeReorderHelper CreateTestObject()
    {
      DataTable tbl = CreateTestTable();
      DataTableTreeModel model = new DataTableTreeModel(tbl, "Id", "ParentId");
      
      // проверяем корректность модели
      Assert.AreEqual("Order", model.Sort, "DataTableTreeModel.Sort");
      List<string> lst = new List<string>();
      foreach (TreePath path in new TreePathEnumerable(model))
      {
        DataRow row = model.TreePathToDataRow(path);
        lst.Add(row["Text"].ToString());
      }
      string s = String.Join(",", lst.ToArray());
      Assert.AreEqual("AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH", s, "DataTableTreeModel rows");

      return new DataTableTreeReorderHelper(model, "Order");
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

