using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Models.Tree;
using FreeLibSet.Core;

namespace ExtTools_tests.Models.Tree
{
  [TestFixture]
  public class ListTreeModelTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_default()
    {
      ListTreeModel sut = new ListTreeModel();
      Assert.AreEqual(0, sut.Count, "Count");
    }

    [Test]
    public void Constructor_list()
    {
      List<string> lst = CreateTestList();
      ListTreeModel sut = new ListTreeModel(lst);
      Assert.AreEqual(3, sut.Count, "Count");
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

    #region Методы изменения списка и события

    [Test]
    public void Add()
    {
      List<string> lst = CreateTestList();
      ListTreeModel sut = new ListTreeModel(lst);
      TreeModelEventTester tester = new TreeModelEventTester(sut);

      sut.Add("DDD");

      Assert.AreEqual(4, sut.Count, "Count");
      CollectionAssert.AreEqual(new string[] { "AAA", "BBB", "CCC", "DDD" }, lst, "List");

      Assert.AreEqual(1, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(1, tester.NodesInsertedChildren[0].Length, "NodesInserted Children.Length");
      Assert.AreEqual(null, tester.NodesInsertedPaths[0].LastNode, "path");
      Assert.AreEqual("DDD", tester.NodesInsertedChildren[0][0], "node");

      Assert.AreEqual(0, tester.NodesChangedPaths.Count, "NodesChanged count");
      Assert.AreEqual(0, tester.NodesRemovedPaths.Count, "NodesRemoved count");
      Assert.AreEqual(0, tester.StructureChangedPaths.Count, "StructureChanged count");
    }


    [Test]
    public void AddRange_3()
    {
      List<string> lst = CreateTestList();
      ListTreeModel sut = new ListTreeModel(lst);
      TreeModelEventTester tester = new TreeModelEventTester(sut);

      sut.AddRange(new string[] { "DDD","EEE","FFF" });

      Assert.AreEqual(6, sut.Count, "Count");
      CollectionAssert.AreEqual(new string[] { "AAA", "BBB", "CCC", "DDD", "EEE", "FFF" }, lst, "List");

      Assert.AreEqual(0, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(0, tester.NodesChangedPaths.Count, "NodesChanged count");
      Assert.AreEqual(0, tester.NodesRemovedPaths.Count, "NodesRemoved count");
      Assert.AreEqual(1, tester.StructureChangedPaths.Count, "StructureChanged count");
    }

    [Test]
    public void AddRange_1()
    {
      List<string> lst = CreateTestList();
      ListTreeModel sut = new ListTreeModel(lst);
      TreeModelEventTester tester = new TreeModelEventTester(sut);

      sut.AddRange(new string[] { "DDD" });

      Assert.AreEqual(4, sut.Count, "Count");
      CollectionAssert.AreEqual(new string[] { "AAA", "BBB", "CCC", "DDD" }, lst, "List");

      Assert.AreEqual(1, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(1, tester.NodesInsertedChildren[0].Length, "NodesInserted Children.Length");
      Assert.AreEqual(null, tester.NodesInsertedPaths[0].LastNode, "path");
      Assert.AreEqual("DDD", tester.NodesInsertedChildren[0][0], "node");

      Assert.AreEqual(0, tester.NodesChangedPaths.Count, "NodesChanged count");
      Assert.AreEqual(0, tester.NodesRemovedPaths.Count, "NodesRemoved count");
      Assert.AreEqual(0, tester.StructureChangedPaths.Count, "StructureChanged count");
    }

    [Test]
    public void AddRange_0()
    {
      List<string> lst = CreateTestList();
      ListTreeModel sut = new ListTreeModel(lst);
      TreeModelEventTester tester = new TreeModelEventTester(sut);

      sut.AddRange(new string[] { });

      Assert.AreEqual(3, sut.Count, "Count");
      CollectionAssert.AreEqual(new string[] { "AAA", "BBB", "CCC" }, lst, "List");

      Assert.AreEqual(0, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(0, tester.NodesChangedPaths.Count, "NodesChanged count");
      Assert.AreEqual(0, tester.NodesRemovedPaths.Count, "NodesRemoved count");
      Assert.AreEqual(0, tester.StructureChangedPaths.Count, "StructureChanged count");
    }

    [Test]
    public void Clear_3()
    {
      List<string> lst = CreateTestList();
      ListTreeModel sut = new ListTreeModel(lst);
      TreeModelEventTester tester = new TreeModelEventTester(sut);

      sut.Clear();

      Assert.AreEqual(0, sut.Count, "Count");
      CollectionAssert.AreEqual(DataTools.EmptyStrings, lst, "List");

      Assert.AreEqual(0, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(0, tester.NodesChangedPaths.Count, "NodesChanged count");
      Assert.AreEqual(0, tester.NodesRemovedPaths.Count, "NodesRemoved count");
      Assert.AreEqual(1, tester.StructureChangedPaths.Count, "StructureChanged count");
    }


    [Test]
    public void Clear_0()
    {
      List<string> lst = new List<string>();
      ListTreeModel sut = new ListTreeModel(lst);
      TreeModelEventTester tester = new TreeModelEventTester(sut);

      sut.Clear();

      Assert.AreEqual(0, sut.Count, "Count");
      CollectionAssert.AreEqual(DataTools.EmptyStrings, lst, "List");

      Assert.AreEqual(0, tester.NodesInsertedPaths.Count, "NodesInserted count");
      Assert.AreEqual(0, tester.NodesChangedPaths.Count, "NodesChanged count");
      Assert.AreEqual(0, tester.NodesRemovedPaths.Count, "NodesRemoved count");
      Assert.AreEqual(0, tester.StructureChangedPaths.Count, "StructureChanged count");
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
