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
  public class TreePathTests
  {
    #region Конструктор

    [Test]
    public void Constructor_oneNode()
    {
      TreePath sut = new TreePath(123);

      Assert.AreEqual(1, sut.FullPath.Length, "FullPath.Length");
      Assert.AreEqual(123, sut.FullPath[0], "FullPath[0]");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual(123, sut.FirstNode, "FirstNode");
      Assert.AreEqual(123, sut.LastNode, "LastNode");
      Assert.IsTrue(sut.Parent.IsEmpty, "Parent.IsEmpty");
    }

    [Test]
    public void Constructor_array()
    {
      TreePath sut = new TreePath(new string[] { "AAA", "BBB", "CCC" });

      Assert.AreEqual(3, sut.FullPath.Length, "FullPath.Length");
      Assert.AreEqual("AAA", sut.FullPath[0], "FullPath[0]");
      Assert.AreEqual("BBB", sut.FullPath[1], "FullPath[1]");
      Assert.AreEqual("CCC", sut.FullPath[2], "FullPath[2]");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual("AAA", sut.FirstNode, "FirstNode");
      Assert.AreEqual("CCC", sut.LastNode, "LastNode");
      Assert.IsFalse(sut.Parent.IsEmpty, "Parent.IsEmpty");
      Assert.AreEqual(new string[] { "AAA", "BBB" }, sut.Parent.FullPath, "Parent.FullPath");
    }

    [Test]
    public void Constructor_child()
    {
      TreePath parent = new TreePath(123);
      TreePath sut = new TreePath(parent, 456);

      Assert.AreEqual(2, sut.FullPath.Length, "FullPath.Length");
      Assert.AreEqual(123, sut.FullPath[0], "FullPath[0]");
      Assert.AreEqual(456, sut.FullPath[1], "FullPath[1]");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual(123, sut.FirstNode, "FirstNode");
      Assert.AreEqual(456, sut.LastNode, "LastNode");
      Assert.IsFalse(sut.Parent.IsEmpty, "Parent.IsEmpty");
      Assert.AreEqual(new int[] { 123 }, sut.Parent.FullPath, "Parent.FullPath");
    }

    #endregion

    #region Empty

    [Test]
    public void Empty()
    {
      Assert.AreEqual(0, TreePath.Empty.FullPath.Length, "FullPath.Length");
      Assert.IsTrue(TreePath.Empty.IsEmpty, "IsEmpty");
      Assert.IsNull(TreePath.Empty.FirstNode, "FirstNode");
      Assert.IsNull(TreePath.Empty.LastNode, "LastNode");
    }

    #endregion

    #region Сравнение путей

    public enum ModelKind { Int32, DataRow }

    [Test]
    public void Equals([Values(ModelKind.Int32, ModelKind.DataRow)] ModelKind modelKind,
      [Range(0, 3)]int firstIndex, [Range(0, 3)] int secondIndex)
    {
      TreePath[] a = GetTestTreePathArray(modelKind);
      TreePath path1 = a[firstIndex];
      TreePath path2 = a[secondIndex];
      bool wanted = (firstIndex == secondIndex);

      Assert.AreEqual(wanted, path1.Equals(path2), "TreePath.Equals()");
      Assert.AreEqual(wanted, Object.Equals(path1, path2), "Object.Equals()");
      Assert.AreEqual(wanted, path1 == path2, "operator ==");
      Assert.AreEqual(!wanted, path1 != path2, "operator !=");
    }

    /// <summary>
    /// Для теста сравнения узлов требуется, чтобы таблица была единственной
    /// </summary>
    private static DataTable _TestTreePathDataTable;

    private TreePath[] GetTestTreePathArray(ModelKind modelKind)
    {
      TreePath path1, path2, path3;
      switch (modelKind)
      {
        case ModelKind.Int32:
          path1 = new TreePath(1);
          path2 = new TreePath(path1, 2);
          path3 = new TreePath(path1, 3);
          break;
        case ModelKind.DataRow:
          if (_TestTreePathDataTable == null)
          {
            // Учитываем возможность асинхронного тестирования
            DataTable tbl = new DataTable();
            tbl.Columns.Add("Id", typeof(Int32));
            tbl.Columns.Add("ParentId", typeof(Int32));
            tbl.Rows.Add(1, DBNull.Value);
            tbl.Rows.Add(2, 1);
            tbl.Rows.Add(3, 1);

            _TestTreePathDataTable = tbl;
          }
          path1 = new TreePath(_TestTreePathDataTable.Rows[0]);
          path2 = new TreePath(path1, _TestTreePathDataTable.Rows[1]);
          path3 = new TreePath(path1, _TestTreePathDataTable.Rows[2]);
          break;
        default:
          throw new ArgumentException();
      }

      return new TreePath[4] { TreePath.Empty, path1, path2, path3 };
    }

    #endregion

    #region GetCommonPath()

    [TestCase("1,2,3", "1,2,4", "1,2")]
    [TestCase("1,2,3,2", "1,2,3", "1,2,3")]
    [TestCase("1,2,3", "1,2,3", "1,2,3")]
    [TestCase("1,2,3", "2,3", "")]
    [TestCase("1", "1", "1")]
    [TestCase("1", "2", "")]
    [TestCase("1,2,3", "", "")]
    [TestCase("", "", "")]
    public void GetCommonPath(string sPath1, string sPath2, string sWanted)
    {
      TreePath path1 = CreateTestPath(sPath1);
      TreePath path2 = CreateTestPath(sPath2);
      TreePath wanted = CreateTestPath(sWanted);

      TreePath res1 = TreePath.GetCommonPath(path1, path2);
      Assert.AreEqual(wanted, res1, "#1");

      TreePath res2 = TreePath.GetCommonPath(path2, path1);
      Assert.AreEqual(wanted, res2, "#2");
    }

    private static TreePath CreateTestPath(string s)
    {
      int[] a1 = StdConvert.ToInt32Array(s);
      object[] a2 = new object[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = a1[i];
      return new TreePath(a2);
    }

    #endregion

    #region IsDescendantOf(), IsAncestorOf()

    [TestCase("1,2,3", "1,2", true)]
    [TestCase("1,2,3", "1,2,3", false)]
    [TestCase("1,2,3", "1,2,3,4", false)]
    [TestCase("1,2,3", "1,3", false)]
    [TestCase("1,2,3", "1", true)]
    [TestCase("1,2,3", "", true)]
    [TestCase("1", "", true)]
    [TestCase("", "", false)]
    [TestCase("", "1", false)]
    public void IsDescendantOf(string sPath1, string sPath2, bool wanted)
    {
      TreePath path1 = CreateTestPath(sPath1);
      TreePath path2 = CreateTestPath(sPath2);

      bool res = path1.IsDescendantOf(path2);
      Assert.AreEqual(wanted, res);
    }

    [TestCase("1,2", "1,2,3", true)]
    [TestCase("1,2", "1,2,3,4", true)]
    [TestCase("1,2", "1,2", false)]
    [TestCase("1,2", "1", false)]
    [TestCase("1,2", "1", false)]
    [TestCase("1,2", "1", false)]
    [TestCase("1", "1", false)]
    [TestCase("", "1", true)]
    [TestCase("", "", false)]
    public void IsAncestorOf(string sPath1, string sPath2, bool wanted)
    {
      TreePath path1 = CreateTestPath(sPath1);
      TreePath path2 = CreateTestPath(sPath2);

      bool res = path1.IsAncestorOf(path2);
      Assert.AreEqual(wanted, res);
    }

    #endregion
  }
}
