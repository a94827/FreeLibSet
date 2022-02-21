using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Models.Tree;
using System.Data;
using FreeLibSet.Core;

namespace ExtTools_tests.Models.Tree
{
  [TestFixture]
  public class DataTableTreeModelTests
  {
    #region Конструктор

    [Test]
    public void Constructor_Ok()
    {
      DataTable table = new DataTable();
      table.Columns.Add("Id", typeof(Int32));
      table.Columns.Add("ParentId", typeof(Int32));

      DataTableTreeModel sut = new DataTableTreeModel(table, "Id", "ParentId");
      Assert.AreSame(table, sut.Table, "Table");
      Assert.AreEqual("Id", sut.IdColumnName, "IdColumnName");
      Assert.AreEqual("ParentId", sut.ParentColumnName, "ParentColumnName");
      Assert.AreEqual("", sut.Sort, "Sort");
      Assert.AreEqual(0, sut.IsNullDefaultValue, "IsNullDefaultValue");
      Assert.IsFalse(sut.UsePrimaryKey, "UsePrimaryKey");
    }

    [Test]
    public void Constructor_Fail()
    {
      DataTable table1 = new DataTable();
      table1.Columns.Add("Id", typeof(Int32));
      table1.Columns.Add("ParentId", typeof(Int32));

      Assert.Catch(delegate() { new DataTableTreeModel(table1, "XXX", "ParentId"); }, "unknown idColumnName");
      Assert.Catch(delegate() { new DataTableTreeModel(table1, "Id", "XXX"); }, "unknown parentColumnName");

      DataTable table2 = new DataTable();
      table2.Columns.Add("Id", typeof(Int32));
      table2.Columns.Add("ParentId", typeof(Int64));
      Assert.Catch(delegate() { new DataTableTreeModel(table2, "Id", "ParentId"); }, "DataType different");
    }

    #endregion

    #region Sort

    [Test]
    public void Sort([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");
      Assert.AreEqual("2,3", GetTestStringIds(sut.GetChildRows(table.Rows[0])), "default sort");

      sut.Sort = "F1 DESC";
      Assert.AreEqual("3,2", GetTestStringIds(sut.GetChildRows(table.Rows[0])), "F1 DESC");

      sut.Sort = "F1 ASC";
      Assert.AreEqual("2,3", GetTestStringIds(sut.GetChildRows(table.Rows[0])), "F1 ASC");
    }

    #endregion

    #region IsNullDefaultValue

    [Test]
    public void IsNullDefaultValue([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");
      Assert.AreEqual("", sut.IsNullDefaultValue, "#1 IsNullDefaultValue");
      Assert.AreEqual("1", GetTestStringIds(sut.GetChildRows(null)), "#1 root rows");
      Assert.AreEqual("2,3", GetTestStringIds(sut.GetChildRows(table.Rows[0])), "#1 child rows");

      sut.IsNullDefaultValue = "ZZZ";
      Assert.AreEqual("1", GetTestStringIds(sut.GetChildRows(null)), "#2 root rows");
      Assert.AreEqual("2,3", GetTestStringIds(sut.GetChildRows(table.Rows[0])), "#2 child rows");

      Assert.Catch(delegate() { sut.IsNullDefaultValue = 0; }, "#3 invalid value");
    }

    #endregion

    #region GetChildren(), IsLeaf()

    [Test]
    public void GetChildren([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");

      List<object> lst = new List<object>();
      foreach (object tag in sut.GetChildren(TreePath.Empty))
        lst.Add(tag);
      Assert.AreEqual(1, lst.Count, "#1 Count");
      Assert.AreEqual(table.Rows[0], lst[0], "#1 Items[0]");

      TreePath path2 = new TreePath(lst[0]);
      lst.Clear();
      foreach (object tag in sut.GetChildren(path2))
        lst.Add(tag);
      Assert.AreEqual(2, lst.Count, "#2 Count");
      Assert.AreEqual(table.Rows[1], lst[0], "#1 Items[0]");
      Assert.AreEqual(table.Rows[2], lst[1], "#1 Items[1]");

      TreePath path3 = new TreePath(path2, lst[0]);
      lst.Clear();
      foreach (object tag in sut.GetChildren(path3))
        lst.Add(tag);
      Assert.AreEqual(0, lst.Count, "#3 Count");
    }


    [Test]
    public void IsLeaf([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");

      TreePath path1 = new TreePath(table.Rows[0]);
      Assert.IsFalse(sut.IsLeaf(path1), "#1");

      TreePath path2 = new TreePath(path1, table.Rows[1]);
      Assert.IsTrue(sut.IsLeaf(path2), "#2");
    }

    #endregion

    #region TreePath comparision

    [Test]
    public void TreePath_compare()
    {
      DataTable table = CreateTestTableWithStringIds(false);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");

      TreePath path1 = new TreePath(table.Rows[0]);
      TreePath path2 = new TreePath(path1, table.Rows[1]);

    }

    #endregion

    #region TreePathTo/FromDataRow()

    [Test]
    public void TreePathToDataRow([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");

      TreePath path1 = new TreePath(table.Rows[0]);
      TreePath path2 = new TreePath(path1, table.Rows[1]);
      TreePath path3 = new TreePath(path1, table.Rows[2]);

      Assert.AreEqual(table.Rows[0], sut.TreePathToDataRow(path1), "#1");
      Assert.AreEqual(table.Rows[1], sut.TreePathToDataRow(path2), "#2");
      Assert.AreEqual(table.Rows[2], sut.TreePathToDataRow(path3), "#3");
      Assert.IsNull(sut.TreePathToDataRow(TreePath.Empty), "Empty");
    }

    [Test]
    public void TreePathFromDataRow([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");

      TreePath path1 = new TreePath(table.Rows[0]);
      TreePath path2 = new TreePath(path1, table.Rows[1]);
      TreePath path3 = new TreePath(path1, table.Rows[2]);

      Assert.AreEqual(path1, sut.TreePathFromDataRow(table.Rows[0]), "#1");
      Assert.AreEqual(path2, sut.TreePathFromDataRow(table.Rows[1]), "#2");
      Assert.AreEqual(path3, sut.TreePathFromDataRow(table.Rows[2]), "#3");
      Assert.AreEqual(TreePath.Empty, sut.TreePathFromDataRow(null), "null");
    }

    #endregion

    #region GetChildRows()

    [Test]
    public void GetChildRows([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");

      DataRow[] wanted1=new DataRow[1]{table.Rows[0]};
      Assert.AreEqual(wanted1, sut.GetChildRows(null), "null");

      DataRow[] wanted2 = new DataRow[2] { table.Rows[1], table.Rows[2] };
      Assert.AreEqual(wanted2, sut.GetChildRows(table.Rows[0]), "1");

      DataRow[] wanted3 = new DataRow[0];
      Assert.AreEqual(wanted3, sut.GetChildRows(table.Rows[1]), "2");
    }

    #endregion

    #region TreePathTo/FromKey()

    [Test]
    public void TreePathToKey([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");

      TreePath path1 = new TreePath(table.Rows[0]);
      TreePath path2 = new TreePath(path1, table.Rows[1]);
      TreePath path3 = new TreePath(path1, table.Rows[2]);

      Assert.AreEqual("1", sut.TreePathToKey(path1), "#1");
      Assert.AreEqual("2", sut.TreePathToKey(path2), "#2");
      Assert.AreEqual("3", sut.TreePathToKey(path3), "#3");
      Assert.AreEqual(DBNull.Value, sut.TreePathToKey(TreePath.Empty), "Empty");
    }

    [Test]
    public void TreePathFromKey([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");

      TreePath path1 = new TreePath(table.Rows[0]);
      TreePath path2 = new TreePath(path1, table.Rows[1]);
      TreePath path3 = new TreePath(path1, table.Rows[2]);

      Assert.AreEqual(path1, sut.TreePathFromKey("1"), "#1");
      Assert.AreEqual(path2, sut.TreePathFromKey("2"), "#2");
      Assert.AreEqual(path3, sut.TreePathFromKey("3"), "#3");
      Assert.AreEqual(TreePath.Empty, sut.TreePathFromKey(DBNull.Value), "DBNull");
      Assert.AreEqual(TreePath.Empty, sut.TreePathFromKey(null), "null");
    }

    #endregion

    #region UsePrimaryKey

    [Test]
    public void UsePrimaryKey([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");
      Assert.AreEqual(usePK, sut.UsePrimaryKey);
    }

    #endregion

    #region События

    /// <summary>
    /// Тестировщик событий для ITreeModel
    /// </summary>
    private class EventTester
    {
      #region Конструктор

      public EventTester(ITreeModel model)
      {
        model.NodesChanged += model_NodesChanged;
        model.NodesInserted += model_NodesInserted;
        model.NodesRemoved += model_NodesRemoved;
        model.StructureChanged += model_StructureChanged;

        NodesChangedPaths = new List<TreePath>();
        NodesChangedChildren = new List<object[]>();
        NodesChangedIndices = new List<int[]>();

        NodesInsertedPaths = new List<TreePath>();
        NodesInsertedChildren = new List<object[]>();
        NodesInsertedIndices = new List<int[]>();

        NodesRemovedPaths = new List<TreePath>();
        NodesRemovedChildren = new List<object[]>();
        NodesRemovedIndices = new List<int[]>();

        StructureChangedPaths = new List<TreePath>();
      }

      #endregion

      #region NodesChanged

      public List<TreePath> NodesChangedPaths;
      public List<object[]> NodesChangedChildren;
      public List<int[]> NodesChangedIndices;

      void model_NodesChanged(object sender, TreeModelEventArgs args)
      {
        NodesChangedPaths.Add(args.Path);
        NodesChangedChildren.Add(args.Children);
        NodesChangedIndices.Add(args.Indices);
      }

      #endregion

      #region NodesInserted

      public List<TreePath> NodesInsertedPaths;
      public List<object[]> NodesInsertedChildren;
      public List<int[]> NodesInsertedIndices;

      void model_NodesInserted(object sender, TreeModelEventArgs args)
      {
        NodesInsertedPaths.Add(args.Path);
        NodesInsertedChildren.Add(args.Children);
        NodesInsertedIndices.Add(args.Indices);
      }

      #endregion

      #region NodesRemoved

      public List<TreePath> NodesRemovedPaths;
      public List<object[]> NodesRemovedChildren;
      public List<int[]> NodesRemovedIndices;

      void model_NodesRemoved(object sender, TreeModelEventArgs args)
      {
        NodesRemovedPaths.Add(args.Path);
        NodesRemovedChildren.Add(args.Children);
        NodesRemovedIndices.Add(args.Indices);
      }

      #endregion

      #region StructureChanged

      public List<TreePath> StructureChangedPaths;

      void model_StructureChanged(object sender, TreePathEventArgs args)
      {
        StructureChangedPaths.Add(args.Path);
      }

      #endregion
    }

    [Test]
    public void NodesChanged_topRow([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");
      EventTester tester = new EventTester(sut);
      table.Rows[0]["F3"] = "XXX";

      Assert.AreEqual(1, tester.NodesChangedPaths.Count, "NodesChanged count");
      Assert.AreEqual(1, tester.NodesChangedChildren[0].Length, "NodesChanged Children.Length");
      Assert.AreEqual(null, tester.NodesChangedPaths[0].LastNode, "path");
      Assert.AreEqual(table.Rows[0], tester.NodesChangedChildren[0][0], "node");

      Assert.AreEqual(0, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(0, tester.NodesRemovedPaths.Count, "NodesRemoved count");
      Assert.AreEqual(0, tester.StructureChangedPaths.Count, "StructureChanged count");
    }

    [Test]
    public void NodesChanged_leafRow([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");
      EventTester tester = new EventTester(sut);
      table.Rows[1]["F3"] = "XXX";

      Assert.AreEqual(1, tester.NodesChangedPaths.Count, "NodesChanged count");
      Assert.AreEqual(1, tester.NodesChangedChildren[0].Length, "NodesChanged Children.Length");
      Assert.AreEqual(table.Rows[0], tester.NodesChangedPaths[0].LastNode, "path");
      Assert.AreEqual(table.Rows[1], tester.NodesChangedChildren[0][0], "node");
    }

    [Test]
    public void NodesInserted_topRow([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");
      EventTester tester = new EventTester(sut);
      table.Rows.Add("4", DBNull.Value, "XXX");

      Assert.AreEqual(1, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(1, tester.NodesInsertedChildren[0].Length, "NodesInserted Children.Length");
      Assert.AreEqual(null, tester.NodesInsertedPaths[0].LastNode, "path");
      Assert.AreEqual(table.Rows[3], tester.NodesInsertedChildren[0][0], "node");

      Assert.AreEqual(0, tester.NodesChangedPaths.Count, "NodesChanged count");
      Assert.AreEqual(0, tester.NodesRemovedPaths.Count, "NodesRemoved count");
      Assert.AreEqual(0, tester.StructureChangedPaths.Count, "StructureChanged count");
    }

    [Test]
    public void NodesInserted_leafRow([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");
      EventTester tester = new EventTester(sut);
      table.Rows.Add("4", "2", "XXX");

      Assert.AreEqual(1, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(1, tester.NodesInsertedChildren[0].Length, "NodesInserted Children.Length");
      Assert.AreEqual(table.Rows[1], tester.NodesInsertedPaths[0].LastNode, "path");
      Assert.AreEqual(table.Rows[3], tester.NodesInsertedChildren[0][0], "node");

      TreePath path1 = new TreePath(table.Rows[0]);
      TreePath path2 = new TreePath(path1, table.Rows[1]);
      Assert.IsFalse(sut.IsLeaf(path2), "Row 2 IsLeaf()");
      Assert.AreEqual("4", GetTestStringIds(sut.GetChildRows(table.Rows[1])), "Row 2 GetChildren()");
      TreePath path4 = new TreePath(path2, table.Rows[3]);
      Assert.IsTrue(sut.IsLeaf(path4), "Row 4 IsLeaf()");
    }

    [Test]
    public void NodesRemoved([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");
      EventTester tester = new EventTester(sut);
      DataRow delRow = table.Rows[1];
      delRow.Delete();

      Assert.AreEqual(1, tester.NodesRemovedPaths.Count, "NodesRemoved count");
      Assert.AreEqual(1, tester.NodesRemovedChildren[0].Length, "NodesRemoved Children.Length");
      Assert.AreEqual(table.Rows[0], tester.NodesRemovedPaths[0].LastNode, "path");
      Assert.AreEqual(delRow, tester.NodesRemovedChildren[0][0], "node");

      Assert.AreEqual(0, tester.NodesChangedPaths.Count, "NodesChanged count");
      Assert.AreEqual(0, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(0, tester.StructureChangedPaths.Count, "StructureChanged count");
    }

    [Test]
    public void StructureChanged([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTableWithStringIds(usePK);
      DataTableTreeModel sut = new DataTableTreeModel(table, "F1", "F2");
      // Текущая структура: 1 <-2 , 1 <- 3
      Assert.AreEqual("2,3", GetTestStringIds(sut.GetChildRows(table.Rows[0])), "#0 GetChildren(1)");
      Assert.AreEqual("", GetTestStringIds(sut.GetChildRows(table.Rows[1])), "#0 GetChildren(2)");
      Assert.AreEqual("", GetTestStringIds(sut.GetChildRows(table.Rows[2])), "#0 GetChildren(3)");

      // 1. Переносим узел 2: 1 <- 3 <- 2
      EventTester tester1 = new EventTester(sut);
      table.Rows[1]["F2"] = "3";
      Assert.AreEqual("3", GetTestStringIds(sut.GetChildRows(table.Rows[0])), "#1 GetChildren(1)");
      Assert.AreEqual("", GetTestStringIds(sut.GetChildRows(table.Rows[1])), "#1 GetChildren(2)");
      Assert.AreEqual("2", GetTestStringIds(sut.GetChildRows(table.Rows[2])), "#1 GetChildren(3)");
      // Событие должно было быть вызвано для узла 1, а для узла 3 - не обязательно
      TreePath path1 = new TreePath(table.Rows[0]);
      Assert.IsTrue(tester1.StructureChangedPaths.Contains(path1), "#1 StructureChanged(1)");

      // 2. Переносим узел 3 в корень: 1, 3 <- 2
      EventTester tester2 = new EventTester(sut);
      table.Rows[2]["F2"] = DBNull.Value;
      Assert.AreEqual("", GetTestStringIds(sut.GetChildRows(table.Rows[0])), "#2 GetChildren(1)");
      Assert.AreEqual("", GetTestStringIds(sut.GetChildRows(table.Rows[1])), "#2 GetChildren(2)");
      Assert.AreEqual("2", GetTestStringIds(sut.GetChildRows(table.Rows[2])), "#2 GetChildren(3)");
      // Поменялась вся структура дерева
      Assert.IsTrue(tester2.StructureChangedPaths.Contains(TreePath.Empty), "#2 StructureChanged(all)");

      // 3. Снова переносим узел 2: 1<-2, 3
      EventTester tester3 = new EventTester(sut);
      table.Rows[1]["F2"] = "1";
      Assert.AreEqual("2", GetTestStringIds(sut.GetChildRows(table.Rows[0])), "#3 GetChildren(1)");
      Assert.AreEqual("", GetTestStringIds(sut.GetChildRows(table.Rows[1])), "#3 GetChildren(2)");
      Assert.AreEqual("", GetTestStringIds(sut.GetChildRows(table.Rows[2])), "#3 GetChildren(3)");
      // Поменялись узлы 1 и 3

      Assert.IsTrue(tester3.StructureChangedPaths.Contains(path1), "#3 StructureChanged(1)");
      TreePath path3 = new TreePath(table.Rows[2]); // а не TreePath1+row, путь уже поменялся
      Assert.IsTrue(tester3.StructureChangedPaths.Contains(path3), "#3 StructureChanged(3)");
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Создает тестовую таблицу-дерево с ключевым полем типа String. Корневым узлом является идеентификатор "1",
    /// из него выходят два дочерних узла "2" и "3".
    /// </summary>
    /// <returns></returns>
    private static DataTable CreateTestTableWithStringIds(bool usePK)
    {
      DataTable table = new DataTable();
      DataColumn col = table.Columns.Add("F1", typeof(string));
      col.AllowDBNull = false;
      col = table.Columns.Add("F2", typeof(string));
      col.AllowDBNull = true;
      table.Columns.Add("F3", typeof(string)); // Дополнительное поле
      if (usePK)
        DataTools.SetPrimaryKey(table, "F1");

      table.Rows.Add("1", DBNull.Value, "AAA");
      table.Rows.Add("2", "1", "BBB");
      table.Rows.Add("3", "1", "CCC");

      return table;
    }

    private string GetTestStringIds(DataRow[] rows)
    {
      string[] a = new string[rows.Length];
      for (int i = 0; i < a.Length; i++)
        a[i] = rows[i]["F1"].ToString();
      return String.Join(",", a);
    }

    #endregion
  }
}
