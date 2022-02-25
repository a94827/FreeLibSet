using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Models.Tree;
using System.Data;
using FreeLibSet.Core;

namespace ExtTools_tests.Models.Tree
{
#if XXX
  [TestFixture(TypeArgs = new Type[] { typeof(Int32) })]
  [TestFixture(TypeArgs = new Type[] { typeof(Guid) })]
  [TestFixture(TypeArgs = new Type[] { typeof(String) })]
  public class TypedTests<T>
  {
    [Test]
    public void SimpeTest()
    {
      T x = default(T);
      T y = default(T);
      Assert.AreEqual(x, y);
    }
  }


#endif

  [TestFixture]
  public abstract class DataTableTreeModelWithIdsTests<T>
    where T : IEquatable<T>
  {
    #region DefaultId

    [Test]
    public void DefaultId()
    {
      DataTable table = CreateTestTable(false);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      Assert.AreEqual(DataTools.GetEmptyValue(typeof(T)), sut.DefaultId, "default");

      T newDefVal = CreateTestValue(123);
      sut.DefaultId = newDefVal;
      Assert.AreEqual(newDefVal, sut.DefaultId, "new DefaultId");
      Assert.AreEqual(newDefVal, sut.TreePathToId(TreePath.Empty), "TreePathToId()");
      Assert.IsTrue(sut.TreePathFromId(newDefVal).IsEmpty, "TreePathFromId()");
    }

    #endregion

    #region TreePathTo/FromId()

    // Нельзя одновременно использовать атрибуты [TestCase] и [Values].

    [TestCase(new int[] { 0 }, 1, true)]
    [TestCase(new int[] { 0 }, 1, false)]
    [TestCase(new int[] { 0, 2 }, 3, true)]
    [TestCase(new int[] { 0, 2 }, 3, false)]
    public void TreePathToId(int[] aRows, int nWanted, bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      TreePath path = CreateTreePath(table, aRows);
      T wanted = CreateTestValue(nWanted);

      Assert.AreEqual(wanted, sut.TreePathToId(path));
    }

    [Test]
    public void TreePathToId_EmptyPath([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      Assert.AreEqual(sut.DefaultId, sut.TreePathToId(TreePath.Empty));
    }

    [TestCase(1, true)]
    [TestCase(1, false)]
    [TestCase(3, true)]
    [TestCase(3, false)]
    public void TreePathFromId(int nId, bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      T Id = CreateTestValue(nId);
      TreePath path = sut.TreePathFromId(Id);
      Assert.IsFalse(path.IsEmpty);
      Assert.AreEqual(Id, ((DataRow)(path.LastNode))["F1"]);
    }

    [Test]
    public void TreePathFromId_zeroId([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      TreePath path = sut.TreePathFromId(sut.DefaultId);
      Assert.IsTrue(path.IsEmpty);
    }

    #endregion

    #region DataRowTo/FromId()

    [TestCase(0, 1, true)]
    [TestCase(0, 1, false)]
    [TestCase(2, 3, true)]
    [TestCase(2, 3, false)]
    public void DataRowToId(int rowIndex, int nWanted, bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      DataRow row = table.Rows[rowIndex];
      T wanted = CreateTestValue(nWanted);

      Assert.AreEqual(wanted, sut.DataRowToId(row));
    }

    [Test]
    public void DataRowToId_null([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");

      Assert.AreEqual(sut.DefaultId, sut.DataRowToId(null));
    }

    [TestCase(1, true)]
    [TestCase(1, false)]
    [TestCase(3, true)]
    [TestCase(3, false)]
    public void DataRowFromId(int nId, bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      T Id = CreateTestValue(nId);
      DataRow row = sut.DataRowFromId(Id);

      Assert.AreEqual(Id, row["F1"]);
    }

    [Test]
    public void DataRowFromId_zero([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      Assert.IsNull(sut.DataRowFromId(sut.DefaultId));
    }

    #endregion

    #region GetChildIds() / GetIdsWithChildren()

    [Test]
    public void GetChildIds_1([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      T[] res = sut.GetChildIds(CreateTestValue(1));
      Assert.AreEqual(2, res.Length);
      Assert.AreEqual(CreateTestValue(2), res[0]);
      Assert.AreEqual(CreateTestValue(3), res[1]);
    }

    [Test]
    public void GetChildIds_2([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      T[] res = sut.GetChildIds(CreateTestValue(2));
      Assert.AreEqual(0, res.Length);
    }

    [Test]
    public void GetChildIds_0([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      T[] res = sut.GetChildIds(sut.DefaultId);
      Assert.AreEqual(1, res.Length);
      Assert.AreEqual(CreateTestValue(1), res[0]);
    }

    [Test]
    public void GetIdWithChildren_1([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      T[] res = sut.GetIdWithChildren(CreateTestValue(1));
      Assert.AreEqual(3, res.Length);
      Assert.AreEqual(CreateTestValue(1), res[0]);
      Assert.AreEqual(CreateTestValue(2), res[1]);
      Assert.AreEqual(CreateTestValue(3), res[2]);
    }

    [Test]
    public void GetIdWithChildren_2([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      T[] res = sut.GetIdWithChildren(CreateTestValue(2));
      Assert.AreEqual(1, res.Length);
      Assert.AreEqual(CreateTestValue(2), res[0]);
    }

    [Test]
    public void GetIdWithChildren_0([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      T[] res = sut.GetIdWithChildren(sut.DefaultId);
      Assert.AreEqual(3, res.Length);
      Assert.AreEqual(CreateTestValue(1), res[0]);
      Assert.AreEqual(CreateTestValue(2), res[1]);
      Assert.AreEqual(CreateTestValue(3), res[2]);
    }

    #endregion

    #region TreePathToIdWithChildren()

    [Test]
    public void TreePathToIdWithChildren_1([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      TreePath path = new TreePath(table.Rows[0]);
      T[] res = sut.TreePathToIdWithChildren(path);
      Assert.AreEqual(3, res.Length);
      Assert.AreEqual(CreateTestValue(1), res[0]);
      Assert.AreEqual(CreateTestValue(2), res[1]);
      Assert.AreEqual(CreateTestValue(3), res[2]);
    }

    [Test]
    public void TreePathToIdWithChildren_2([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      TreePath path = new TreePath(new object[] { table.Rows[0], table.Rows[1] });
      T[] res = sut.TreePathToIdWithChildren(path);
      Assert.AreEqual(1, res.Length);
      Assert.AreEqual(CreateTestValue(2), res[0]);
    }

    [Test]
    public void TreePathToIdWithChildren_emptyPath([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      T[] res = sut.TreePathToIdWithChildren(TreePath.Empty);
      Assert.AreEqual(3, res.Length);
      Assert.AreEqual(CreateTestValue(1), res[0]);
      Assert.AreEqual(CreateTestValue(2), res[1]);
      Assert.AreEqual(CreateTestValue(3), res[2]);
    }

    #endregion

    #region DataRowToIdWithChildren()

    [Test]
    public void DataRowToIdWithChildren_1([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      T[] res = sut.DataRowToIdWithChildren(table.Rows[0]);
      Assert.AreEqual(3, res.Length);
      Assert.AreEqual(CreateTestValue(1), res[0]);
      Assert.AreEqual(CreateTestValue(2), res[1]);
      Assert.AreEqual(CreateTestValue(3), res[2]);
    }

    [Test]
    public void DataRowToIdWithChildren_2([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      T[] res = sut.DataRowToIdWithChildren(table.Rows[1]);
      Assert.AreEqual(1, res.Length);
      Assert.AreEqual(CreateTestValue(2), res[0]);
    }

    [Test]
    public void DataRowToIdWithChildren_null([Values(true, false)]bool usePK)
    {
      DataTable table = CreateTestTable(usePK);
      DataTableTreeModelWithIds<T> sut = new DataTableTreeModelWithIds<T>(table, "F1", "F2");
      T[] res = sut.DataRowToIdWithChildren(null);
      Assert.AreEqual(3, res.Length);
      Assert.AreEqual(CreateTestValue(1), res[0]);
      Assert.AreEqual(CreateTestValue(2), res[1]);
      Assert.AreEqual(CreateTestValue(3), res[2]);
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Создает тестовую таблицу-дерево с ключевым полем типа String. Корневым узлом является идеентификатор "1",
    /// из него выходят два дочерних узла "2" и "3".
    /// </summary>
    /// <returns></returns>
    private DataTable CreateTestTable(bool usePK)
    {
      DataTable table = new DataTable();
      DataColumn col = table.Columns.Add("F1", typeof(T));
      col.AllowDBNull = false;
      col = table.Columns.Add("F2", typeof(T));
      col.AllowDBNull = true;
      table.Columns.Add("F3", typeof(string)); // Дополнительное поле
      if (usePK)
        DataTools.SetPrimaryKey(table, "F1");

      table.Rows.Add(CreateTestValue(1), DBNull.Value, "AAA");
      table.Rows.Add(CreateTestValue(2), CreateTestValue(1), "BBB");
      table.Rows.Add(CreateTestValue(3), CreateTestValue(1), "CCC");
      return table;
    }

    protected abstract T CreateTestValue(int v);

    /*
    private TreePath CreateTreePath(string sPath)
    {
      if (String.IsNullOrEmpty(sPath))
        return TreePath.Empty;

      int[]a1=StdConvert.ToInt32Array(sPath);
      object[] a2 = new object[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = CreateTestValue(a1[i]);
      return new TreePath(a2);
    }
      */
    private TreePath CreateTreePath(DataTable table, int[] aRows)
    {
      if (aRows.Length == 0)
        return TreePath.Empty;

      object[] a2 = new object[aRows.Length];
      for (int i = 0; i < aRows.Length; i++)
        a2[i] = table.Rows[aRows[i]];
      return new TreePath(a2);
    }

    #endregion
  }

  public class DataTableTreeModelWithIdsTests_Int32 : DataTableTreeModelWithIdsTests<Int32>
  {
    protected override int CreateTestValue(int v)
    {
      return v;
    }
  }

  public class DataTableTreeModelWithIdsTests_Guid : DataTableTreeModelWithIdsTests<Guid>
  {
    protected override Guid CreateTestValue(int v)
    {
      return new Guid(v, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }
  }

  public class DataTableTreeModelWithIdsTests_String : DataTableTreeModelWithIdsTests<String>
  {
    protected override string CreateTestValue(int v)
    {
      return StdConvert.ToString(v);
    }
  }
}
