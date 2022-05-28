using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Models.Tree;
using System.Data;

namespace ExtTools_tests.Models.Tree
{
  [TestFixture]
  public class TreePathEnumerableTests
  {
    [Test]
    public void Constructor_EmptyPath()
    {
      SimpleListTreeModel model = new SimpleListTreeModel(new string[] { "AAA", "BBB", "CCC" });
      TreePathEnumerable sut = new TreePathEnumerable(model);
      List<string> lst = new List<string>();
      foreach (TreePath path in sut)
      {
        Assert.AreEqual(1, path.FullPath.Length, "FullPath.Length");
        lst.Add(path.FullPath[0].ToString());
      }
      Assert.AreEqual("AAA,BBB,CCC", String.Join(",", lst.ToArray()), "Result");
    }

    [TestCase(0, "AAA,BBB,CCC,DDD,EEE,FFF,GGG,HHH")]
    [TestCase(1, "")]
    [TestCase(2, "BBB,CCC")]
    [TestCase(3, "")]
    [TestCase(4, "FFF,GGG")]
    [TestCase(5, "")]
    [TestCase(6, "EEE,FFF,GGG,HHH")]
    [TestCase(7, "")]
    [TestCase(8, "")]
    public void GetEnumerator(int parentId, string wanted)
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Text", typeof(string));
      tbl.Columns.Add("Id", typeof(int));
      tbl.Columns.Add("ParentId", typeof(int));

      // Получаем дерево:
      // 2 "AAA"
      //   1 "BBB"
      //   3 "CCC"
      // 6 "DDD"
      //   4 "EEE"
      //     8 "FFF"
      //     7 "GGG"
      //   5 "HHH"

      tbl.Rows.Add("BBB", 1, 2);
      tbl.Rows.Add("AAA", 2);
      tbl.Rows.Add("CCC", 3, 2);
      tbl.Rows.Add("EEE", 4, 6);
      tbl.Rows.Add("HHH", 5, 6);
      tbl.Rows.Add("DDD", 6);
      tbl.Rows.Add("FFF", 7, 4);
      tbl.Rows.Add("GGG", 8, 4);
      DataTableTreeModel model = new DataTableTreeModel(tbl, "Id", "ParentId");

      TreePathEnumerable sut = new TreePathEnumerable(model, model.TreePathFromKey(parentId));
      List<string> lst = new List<string>();
      foreach (TreePath path in sut)
      {
        DataRow row = model.TreePathToDataRow(path);
        lst.Add(row["Text"].ToString());
      }

      Assert.AreEqual(wanted, String.Join(",", lst.ToArray()));
    }

  }
}
