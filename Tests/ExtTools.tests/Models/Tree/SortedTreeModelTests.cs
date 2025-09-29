using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using NUnit.Framework;
using FreeLibSet.Models.Tree;
using FreeLibSet.Core;

namespace ExtTools_tests.Models.Tree
{
  [TestFixture]
  public class SortedTreeModelTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      DataTableTreeModel baseModel = CreateBaseModel();
      SortedTreeModel sut = new SortedTreeModel(baseModel);
      Assert.AreSame(baseModel, sut.InnerModel, "InnerModel");
      Assert.IsNull(sut.Comparer, "Comparer");
      TestGetChildren(sut, 0, new int[] { 1, 2, 3 });
      TestGetChildren(sut, 1, new int[] { });
      TestGetChildren(sut, 2, new int[] { 4, 5, 6 });

      TestIsLeaf(sut, 1, true);
      TestIsLeaf(sut, 2, false);
      TestIsLeaf(sut, 6, true);
    }

    private DataTableTreeModel CreateBaseModel()
    {
      DataTable table = new DataTable();
      table.Columns.Add("Id", typeof(int));
      table.Columns.Add("ParentId", typeof(int));
      table.Columns.Add("Name", typeof(string));
      DataTools.SetPrimaryKey(table, "Id");

      table.Rows.Add(1, DBNull.Value, "BBB");
      table.Rows.Add(2, DBNull.Value, "CCC");
      table.Rows.Add(3, DBNull.Value, "AAA");
      table.Rows.Add(4, 2, "FFF");
      table.Rows.Add(5, 2, "EEE");
      table.Rows.Add(6, 2, "DDD");

      DataTableTreeModel model = new DataTableTreeModel(table, "Id", "ParentId");
      return model;
    }

    private static void TestGetChildren(SortedTreeModel sut, int parentId, int[] wantedChildIds)
    {
      DataTableTreeModel baseModel = (DataTableTreeModel)(sut.InnerModel);
      TreePath parent = TreePath.Empty;
      if (parentId != 0)
        parent = baseModel.TreePathFromKey(parentId);

      List<Int32> lst = new List<int>();
      foreach (DataRow row in sut.GetChildren(parent))
        lst.Add(DataTools.GetInt32(row, "Id"));
      CollectionAssert.AreEqual(wantedChildIds, lst, "ParentId=" + parentId.ToString());
    }

    private static void TestIsLeaf(SortedTreeModel sut, int id, bool wantedIsLeaf)
    {
      if (id == 0)
        throw new ArgumentException();

      DataTableTreeModel baseModel = (DataTableTreeModel)(sut.InnerModel);
      TreePath path = baseModel.TreePathFromKey(id);

      bool res = sut.IsLeaf(path);
      Assert.AreEqual(wantedIsLeaf, res, "Id=" + id.ToString());
    }

    #endregion

    #region Comparer

    private class NameTestComparer : System.Collections.IComparer
    {
      public NameTestComparer(bool descend)
      {
        _Descend = descend;
      }

      private bool _Descend;

      public int Compare(object x, object y)
      {
        DataRow r1 = x as DataRow;
        DataRow r2 = y as DataRow;

        Assert.IsNotNull(r1, "Row 1");
        Assert.IsNotNull(r2, "Row 2");

        string name1 = DataTools.GetString(r1, "Name");
        string name2 = DataTools.GetString(r2, "Name");

        int res = String.CompareOrdinal(name1, name2);
        if (_Descend)
          res = -res;
        return res;
      }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Comparer(bool descend)
    {
      DataTableTreeModel baseModel = CreateBaseModel();
      SortedTreeModel sut = new SortedTreeModel(baseModel);

      NameTestComparer comparer = new NameTestComparer(descend);
      sut.Comparer = comparer;
      Assert.AreEqual(comparer, sut.Comparer, "Comparer");


      int[] wantedRes0 = new int[] { 3, 1, 2 };
      int[] wantedRes2 = new int[] { 6, 5, 4 };
      if (descend)
      {
        Array.Reverse(wantedRes0);
        Array.Reverse(wantedRes2);
      }

      TestGetChildren(sut, 0, wantedRes0);
      TestGetChildren(sut, 1, new int[] { });
      TestGetChildren(sut, 2, wantedRes2);

      TestIsLeaf(sut, 1, true);
      TestIsLeaf(sut, 2, false);
      TestIsLeaf(sut, 6, true);
    }

    #endregion

    #region Передача событий от базовой модели

    [Test]
    public void OnStructureChanged()
    {
      DataTableTreeModel baseModel = CreateBaseModel();
      SortedTreeModel sut = new SortedTreeModel(baseModel);
      sut.Comparer = new NameTestComparer(false);
      TreeModelEventTester tester = new TreeModelEventTester(sut);

      baseModel.Refresh();

      Assert.AreEqual(0, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(0, tester.NodesChangedPaths.Count, "NodesChanged count");
      Assert.AreEqual(0, tester.NodesRemovedPaths.Count, "NodesRemoved count");
      Assert.AreEqual(1, tester.StructureChangedPaths.Count, "StructureChanged count");
    }


    // для остальных событий могут как передаваться оригинальные события с измененными аргументами, так и событие OnStructureChanged()

    [Test]
    public void OnNodesInserted()
    {
      DataTableTreeModel baseModel = CreateBaseModel();
      SortedTreeModel sut = new SortedTreeModel(baseModel);
      sut.Comparer = new NameTestComparer(false);
      TreeModelEventTester tester = new TreeModelEventTester(sut);

      DataRow newRow = baseModel.Table.Rows.Add(7, 5, "GGG");

      Assert.AreEqual(0, tester.NodesChangedPaths.Count, "NodesChanged count");
      Assert.AreEqual(0, tester.NodesRemovedPaths.Count, "NodesRemoved count");
      if (tester.NodesInsertedPaths.Count == 1)
      {
        Assert.AreEqual(1, tester.NodesInsertedPaths.Count, "NodesInserted count");
        Assert.AreEqual(0, tester.StructureChangedPaths.Count, "StructureChanged count");

        TreePath wantedPath = baseModel.TreePathFromKey(5);
        Assert.AreEqual(wantedPath, tester.NodesInsertedPaths[0], "InsertPath");
        Assert.AreEqual(1, tester.NodesInsertedChildren.Count, "NodesInsertedChildren.Count");
        CollectionAssert.AreEqual(new DataRow[1] { newRow }, tester.NodesInsertedChildren[0], "InsertedRow");
      }
      else
      {
        Assert.AreEqual(0, tester.NodesInsertedPaths.Count, "NodesInserted count");
        Assert.AreEqual(1, tester.StructureChangedPaths.Count, "StructureChanged count");

        TreePath wantedPath = baseModel.TreePathFromKey(5);
        Assert.AreEqual(wantedPath, tester.StructureChangedPaths[0], "InsertPath");
      }
    }

    [Test]
    public void OnNodesRemoved()
    {
      DataTableTreeModel baseModel = CreateBaseModel();
      SortedTreeModel sut = new SortedTreeModel(baseModel);
      sut.Comparer = new NameTestComparer(false);
      TreeModelEventTester tester = new TreeModelEventTester(sut);

      DataRow delRow = baseModel.Table.Rows.Find(5);
      delRow.Delete();

      Assert.AreEqual(0, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(0, tester.NodesChangedPaths.Count, "NodesChanged count");
      if (tester.NodesRemovedPaths.Count == 1)
      {
        Assert.AreEqual(0, tester.StructureChangedPaths.Count, "StructureChanged count");

        TreePath wantedPath = baseModel.TreePathFromKey(2);
        Assert.AreEqual(wantedPath, tester.NodesRemovedPaths[0], "RemovedPath");
        Assert.AreEqual(1, tester.NodesRemovedChildren.Count, "NodesRemovedChildren.Count");
        CollectionAssert.AreEqual(new DataRow[1] { delRow }, tester.NodesRemovedChildren[0]);
      }
      else
      {
        Assert.AreEqual(0, tester.NodesRemovedPaths.Count, "NodesRemoved count");
        Assert.AreEqual(1, tester.StructureChangedPaths.Count, "StructureChanged count");

        TreePath wantedPath = baseModel.TreePathFromKey(2);
        Assert.AreEqual(wantedPath, tester.StructureChangedPaths[0], "ChangedPath");
      }
    }


    [Test]
    public void OnNodesChanged()
    {
      DataTableTreeModel baseModel = CreateBaseModel();
      SortedTreeModel sut = new SortedTreeModel(baseModel);
      sut.Comparer = new NameTestComparer(false);
      TreeModelEventTester tester = new TreeModelEventTester(sut);

      DataRow changedRow = baseModel.Table.Rows.Find(5);
      changedRow["Name"] =" GGG";

      Assert.AreEqual(0, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(0, tester.NodesRemovedPaths.Count, "NodesRemoved count");
      if (tester.NodesChangedPaths.Count == 1)
      {
        Assert.AreEqual(0, tester.StructureChangedPaths.Count, "StructureChanged count");

        TreePath wantedPath = baseModel.TreePathFromKey(2);
        Assert.AreEqual(wantedPath, tester.NodesChangedPaths[0], "ChangedPath");
        Assert.AreEqual(1, tester.NodesChangedChildren.Count, "NodesChangedChildren.Count");
        CollectionAssert.AreEqual(new DataRow[1] { changedRow }, tester.NodesChangedChildren[0]);
      }
      else
      {
        Assert.AreEqual(0, tester.NodesChangedPaths.Count, "NodesChanged count");
        Assert.AreEqual(1, tester.StructureChangedPaths.Count, "StructureChanged count");

        TreePath wantedPath = baseModel.TreePathFromKey(2);
        Assert.AreEqual(wantedPath, tester.StructureChangedPaths[0], "ChangedPath");
      }
    }

    #endregion
  }
}
