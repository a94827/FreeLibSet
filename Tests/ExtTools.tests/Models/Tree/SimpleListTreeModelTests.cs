using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Models.Tree;
using FreeLibSet.Core;

namespace ExtTools_tests.Models.Tree
{
  [TestFixture]
  public class SimpleListTreeModelTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      List<string> lst = CreateTestList();
      SimpleListTreeModel sut = new SimpleListTreeModel(lst);
      CollectionAssert.AreEqual(lst, sut.GetChildren(TreePath.Empty), "GetChildren(Empty)");

      TreePath p1 = new TreePath("AAA");
      Assert.IsTrue(sut.IsLeaf(p1), "IsLeaf(1)");
    }

    private static List<string> CreateTestList()
    {
      List<string> lst = new List<string>();
      lst.Add("AAA");
      lst.Add("BBB");
      lst.Add("CCC");
      return lst;
    }

    #endregion

    #region Методы TreeModelBase

    [Test]
    public void Refresh()
    {
      List<string> lst = CreateTestList();
      ListTreeModel sut = new ListTreeModel(lst);
      TreeModelEventTester tester = new TreeModelEventTester(sut);

      sut.Refresh();

      Assert.AreEqual(3, sut.Count, "Count");
      CollectionAssert.AreEqual(new string[] { "AAA", "BBB", "CCC" }, lst, "List");

      Assert.AreEqual(0, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(0, tester.NodesChangedPaths.Count, "NodesChanged count");
      Assert.AreEqual(0, tester.NodesRemovedPaths.Count, "NodesRemoved count");
      Assert.AreEqual(1, tester.StructureChangedPaths.Count, "StructureChanged count");
    }

    [Test]
    public void RefreshNode()
    {
      List<string> lst = CreateTestList();
      ListTreeModel sut = new ListTreeModel(lst);
      TreeModelEventTester tester = new TreeModelEventTester(sut);

      TreePath p2 = new TreePath("BBB");
      sut.RefreshNode(p2);

      Assert.AreEqual(3, sut.Count, "Count");
      CollectionAssert.AreEqual(new string[] { "AAA", "BBB", "CCC" }, lst, "List");

      Assert.AreEqual(0, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(1, tester.NodesChangedPaths.Count, "NodesChanged count");
      Assert.AreEqual(TreePath.Empty, tester.NodesChangedPaths[0], "parent path");
      CollectionAssert.AreEqual(new string[] { "BBB" }, tester.NodesChangedChildren[0], "changed children");
      Assert.AreEqual(0, tester.NodesRemovedPaths.Count, "NodesRemoved count");
      Assert.AreEqual(0, tester.StructureChangedPaths.Count, "StructureChanged count");
    }

    #endregion


  }
}
