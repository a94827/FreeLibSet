using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;
using System.Data;
using FreeLibSet.Tests;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class DBxStructTests : FixtureWithSetUp
  {
    #region Тестовые объекты

    private class TestStructSource : IDBxStructSource
    {
      public string[] GetAllTableNames()
      {
        return new string[] { "Test1", "Test2" };
      }

      public DBxTableStruct GetTableStruct(string tableName)
      {
        DBxTableStruct ts = new DBxTableStruct(tableName);
        ts.Columns.AddId();
        switch (tableName)
        {
          case "Test1":
            ts.Columns.AddString("F1", 10, false);
            return ts;
          case "Test2":
            ts.Columns.AddInt64("F2", true);
            ts.Columns.AddBoolean("F3");
            return ts;
          default:
            throw new ArgumentException("Invalid table name");
        }
      }

      public bool IsRealStruct { get { return false; } }
    }

    public static DBxStruct CreateTestObject(bool useSource)
    {
      IDBxStructSource src = new TestStructSource();
      DBxStruct sut = new DBxStruct(src);
      if (!useSource)
        sut = sut.Clone();

      Assert.AreEqual(useSource, !Object.ReferenceEquals(sut.Source, null), "Source");
      //Assert.DoesNotThrow(delegate () { DBxStructChecker.CheckStruct(sut); }, "CheckStruct()");

      return sut;
    }

    #endregion

    #region Конструктор

    [Test]
    public void Constructor_noargs()
    {
      DBxStruct sut = new DBxStruct();
      Assert.AreEqual(0, sut.Tables.Count, "Tables.Count");
      Assert.AreEqual(0, sut.AllTableNames.Length, "AllTableNames");
      Assert.IsNull(sut.Source, "Source");
      Assert.IsFalse(sut.HasComments, "HasComments");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.DoesNotThrow(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      //Assert.DoesNotThrow(delegate () { DBxStructChecker.CheckStruct(sut); }, "CheckStruct()");
    }

    [Test]
    public void Constructor_StructSource()
    {
      IDBxStructSource src = new TestStructSource();
      DBxStruct sut = new DBxStruct(src);
      Assert.AreSame(src, sut.Source, "Source");
      Assert.AreEqual(2, sut.Tables.Count, "Tables.Count");
      CollectionAssert.AreEqual(new string[] { "Test1", "Test2" }, sut.AllTableNames, "AllTableNames");
      Assert.IsFalse(sut.HasComments, "HasComments");
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      //Assert.DoesNotThrow(delegate () { DBxStructChecker.CheckStruct(sut); }, "CheckStruct()");
    }

    //[Test]
    //public void Constructor_DBxCon()
    //{
    //  using (SQLiteDBx db = new SQLiteDBx())
    //  {
    //    using (DBxCon con = new DBxCon(db.MainEntry))
    //    {
    //      DBxStruct sut = new DBxStruct(con.StructSource);

    //      Assert.AreEqual(0, sut.Tables.Count, "Tables.Count");
    //      Assert.AreEqual(0, sut.AllTableNames.Length, "AllTableNames");
    //      //Assert.AreSame(con, sut.Source, "Source");
    //      Assert.IsNotNull(sut.Source, "Source");
    //      Assert.IsFalse(sut.HasComments, "HasComments");
    //      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
    //      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
    //      Assert.DoesNotThrow(delegate () { DBxStructChecker.CheckStruct(sut); }, "CheckStruct()");
    //    }
    //  }
    //}

    #endregion

    #region Tables

    #region Доступ по имени и индексу

    [Test]
    public void AllTableNames([Values(false, true)]bool useSource)
    {
      DBxStruct sut = CreateTestObject(useSource);
      CollectionAssert.AreEqual(new string[2] { "Test1", "Test2" }, sut.AllTableNames);
    }

    [Test]
    public void Tables_Item_GetByName([Values(false, true)]bool useSource)
    {
      DBxStruct sut = CreateTestObject(useSource);

      DBxTableStruct res1 = sut.Tables["Test1"];
      Assert.AreEqual("Test1", res1.TableName, "Test1.TableName");
      CollectionAssert.AreEqual(new string[] { "Id", "F1" }, res1.AllColumnNames, "Test1.AllColumnNames");

      DBxTableStruct res2 = sut.Tables["Test2"];
      Assert.AreEqual("Test2", res2.TableName, "Test2.TableName");
      CollectionAssert.AreEqual(new string[] { "Id", "F2", "F3" }, res2.AllColumnNames, "Test2.AllColumnNames");

      DBxTableStruct res3 = sut.Tables["XXX"];
      Assert.IsNull(res3, "Test3");

      DBxTableStruct res4 = sut.Tables[""];
      Assert.IsNull(res4, "Test4");
    }

    [Test]
    public void Tables_Item_GetByIndex([Values(false, true)]bool useSource)
    {
      DBxStruct sut = CreateTestObject(useSource);

      DBxTableStruct res1 = sut.Tables[0];
      Assert.AreEqual("Test1", res1.TableName, "Test1.TableName");
      CollectionAssert.AreEqual(new string[] { "Id", "F1" }, res1.AllColumnNames, "Test1.AllColumnNames");

      DBxTableStruct res2 = sut.Tables[1];
      Assert.AreEqual("Test2", res2.TableName, "Test2.TableName");
      CollectionAssert.AreEqual(new string[] { "Id", "F2", "F3" }, res2.AllColumnNames, "Test2.AllColumnNames");

      DBxTableStruct res3;
      Assert.Catch(delegate () { res3 = sut.Tables[-1]; }, "[-1]");

      DBxTableStruct res4;
      Assert.Catch(delegate () { res4 = sut.Tables[2]; }, "[2]");
    }

    [Test]
    public void Tables_Item_SetByIndex()
    {
      DBxStruct sut = CreateTestObject(false);
      DBxTableStruct ts1 = sut.Tables["Test1"];

      DBxTableStruct ts2 = sut.Tables["Test2"].Clone("TestXXX");
      ts2.Columns.AddString("XXX", 10, true);

      sut.Tables[1] = ts2;

      CollectionAssert.AreEqual(new string[] { "Test1", "TestXXX" }, sut.AllTableNames, "AllTableNames");
      CollectionAssert.AreEqual(new DBxTableStruct[2] { ts1, ts2 }, sut.Tables, "Tables");
    }

    [Test]
    public void Tables_Item_GetRequired([Values(false, true)]bool useSource)
    {
      DBxStruct sut = CreateTestObject(useSource);

      DBxTableStruct res1 = sut.Tables.GetRequired("Test1");
      Assert.AreEqual("Test1", res1.TableName, "Test1.TableName");
      CollectionAssert.AreEqual(new string[] { "Id", "F1" }, res1.AllColumnNames, "Test1.AllColumnNames");

      DBxTableStruct res2 = sut.Tables.GetRequired("Test2");
      Assert.AreEqual("Test2", res2.TableName, "Test2.TableName");
      CollectionAssert.AreEqual(new string[] { "Id", "F2", "F3" }, res2.AllColumnNames, "Test2.AllColumnNames");

      DBxTableStruct res3;
      Assert.Catch(delegate () { res3 = sut.Tables.GetRequired("XXX"); }, "Test3");

      DBxTableStruct res4;
      Assert.Catch(delegate () { res4 = sut.Tables.GetRequired(""); }, "Test4");
    }

    #endregion

    #region IndexOf(), Contains()

    [Test]
    public void Tables_IndexOf_string([Values(false, true)]bool useSource)
    {
      DBxStruct sut = CreateTestObject(useSource);

      Assert.AreEqual(0, sut.Tables.IndexOf("Test1"), "#1");
      Assert.AreEqual(1, sut.Tables.IndexOf("Test2"), "#2");
      Assert.AreEqual(-1, sut.Tables.IndexOf("XXX"), "#3");
      Assert.AreEqual(-1, sut.Tables.IndexOf(""), "#4");
    }

    [Test]
    public void Tables_IndexOf_TableStruct([Values(false, true)]bool useSource)
    {
      DBxStruct sut = CreateTestObject(useSource);

      DBxTableStruct ts1 = sut.Tables[1];
      Assert.AreEqual(1, sut.Tables.IndexOf(ts1), "#1");

      DBxTableStruct ts2 = ts1.Clone();
      Assert.AreEqual(-1, sut.Tables.IndexOf(ts2), "#2");

      DBxTableStruct ts3 = null;
      Assert.AreEqual(-1, sut.Tables.IndexOf(ts3), "#3");
    }

    [Test]
    public void Tables_Contains_string([Values(false, true)]bool useSource)
    {
      DBxStruct sut = CreateTestObject(useSource);

      Assert.IsTrue(sut.Tables.Contains("Test1"), "#1");
      Assert.IsTrue(sut.Tables.Contains("Test2"), "#2");
      Assert.IsFalse(sut.Tables.Contains("XXX"), "#3");
      Assert.IsFalse(sut.Tables.Contains(""), "#4");
    }

    [Test]
    public void Tables_Contains_TableStruct([Values(false, true)]bool useSource)
    {
      DBxStruct sut = CreateTestObject(useSource);

      DBxTableStruct ts1 = sut.Tables[0];
      Assert.IsTrue(sut.Tables.Contains(ts1), "#1");

      DBxTableStruct ts2 = ts1.Clone();
      Assert.IsFalse(sut.Tables.Contains(ts2), "#2");

      DBxTableStruct ts3 = null;
      Assert.IsFalse(sut.Tables.Contains(ts3), "#3");
    }

    #endregion

    #region Add(), Insert()

    [Test]
    public void Tables_Add_TableStruct()
    {
      DBxStruct sut = CreateTestObject(false);
      CollectionAssert.AreEqual(new string[] { "Test1", "Test2" }, sut.AllTableNames, "AllTableNames #0");

      DBxTableStruct ts1 = new DBxTableStruct("TestXXX");
      ts1.Columns.AddId();
      ts1.Columns.AddReference("F101", "Test1");

      sut.Tables.Add(ts1);
      CollectionAssert.AreEqual(new string[] { "Test1", "Test2", "TestXXX" }, sut.AllTableNames, "AllTableNames #1");
      DBxTableStruct res1 = sut.Tables["TestXXX"];
      Assert.AreSame(ts1, res1, "Tables[]");
      Assert.AreSame(ts1, sut.Tables.LastAdded, "LastAdded");

      DBxTableStruct ts2 = new DBxTableStruct("TestXXX");
      ts2.Columns.AddId();
      Assert.Catch(delegate () { sut.Tables.Add(ts2); }, "Name already exists");
      CollectionAssert.AreEqual(new string[] { "Test1", "Test2", "TestXXX" }, sut.AllTableNames, "AllTableNames #2");
    }

    [Test]
    public void Tables_Add_TableName()
    {
      DBxStruct sut = CreateTestObject(false);
      CollectionAssert.AreEqual(new string[] { "Test1", "Test2" }, sut.AllTableNames, "AllTableNames #0");

      DBxTableStruct ts1 = sut.Tables.Add("TestXXX");
      ts1.Columns.AddId();
      ts1.Columns.AddReference("F101", "Test1");
      CollectionAssert.AreEqual(new string[] { "Test1", "Test2", "TestXXX" }, sut.AllTableNames, "AllTableNames #1");

      Assert.Catch(delegate () { sut.Tables.Add("TestXXX"); }, "Name already exists");
      CollectionAssert.AreEqual(new string[] { "Test1", "Test2", "TestXXX" }, sut.AllTableNames, "AllTableNames #2");
    }

    [Test]
    public void Tables_Insert()
    {
      DBxStruct sut = CreateTestObject(false);
      CollectionAssert.AreEqual(new string[] { "Test1", "Test2" }, sut.AllTableNames, "AllTableNames #0");

      DBxTableStruct ts1 = new DBxTableStruct("TestXXX");
      ts1.Columns.AddId();
      ts1.Columns.AddReference("F101", "Test1");

      sut.Tables.Insert(1, ts1);
      CollectionAssert.AreEqual(new string[] { "Test1", "TestXXX", "Test2" }, sut.AllTableNames, "AllTableNames #1");
      DBxTableStruct res1 = sut.Tables["TestXXX"];
      Assert.AreSame(ts1, res1, "Tables[]");

      DBxTableStruct ts2 = new DBxTableStruct("TestXXX");
      ts2.Columns.AddId();
      Assert.Catch(delegate () { sut.Tables.Insert(0, ts2); }, "Name already exists");
      CollectionAssert.AreEqual(new string[] { "Test1", "TestXXX", "Test2" }, sut.AllTableNames, "AllTableNames #2");
    }

    #endregion

    #region Remove(), RemoveAt(), Clear()

    [Test]
    public void Tables_Remove_TableStruct()
    {
      DBxStruct sut = CreateTestObject(false);
      CollectionAssert.AreEqual(new string[] { "Test1", "Test2" }, sut.AllTableNames, "AllTableNames #0");

      DBxTableStruct ts1 = sut.Tables.GetRequired("Test1");
      Assert.IsTrue(sut.Tables.Remove(ts1), "#1");
      CollectionAssert.AreEqual(new string[] { "Test2" }, sut.AllTableNames, "AllTableNames #1");

      DBxTableStruct ts2 = sut.Tables.GetRequired("Test2").Clone();
      Assert.IsFalse(sut.Tables.Remove(ts2), "#2");
      CollectionAssert.AreEqual(new string[] { "Test2" }, sut.AllTableNames, "AllTableNames #2");

      DBxTableStruct ts3 = null;
      Assert.IsFalse(sut.Tables.Remove(ts3), "#3");
      CollectionAssert.AreEqual(new string[] { "Test2" }, sut.AllTableNames, "AllTableNames #3");
    }


    [Test]
    public void Tables_Remove_string()
    {
      DBxStruct sut = CreateTestObject(false);
      CollectionAssert.AreEqual(new string[] { "Test1", "Test2" }, sut.AllTableNames, "AllTableNames #0");

      Assert.IsTrue(sut.Tables.Remove("Test1"), "#1");
      CollectionAssert.AreEqual(new string[] { "Test2" }, sut.AllTableNames, "AllTableNames #1");

      Assert.IsFalse(sut.Tables.Remove("TestXXX"), "#2");
      CollectionAssert.AreEqual(new string[] { "Test2" }, sut.AllTableNames, "AllTableNames #2");

      Assert.IsFalse(sut.Tables.Remove(""), "#3");
      CollectionAssert.AreEqual(new string[] { "Test2" }, sut.AllTableNames, "AllTableNames #3");
    }


    [Test]
    public void Tables_RemoveAt()
    {
      DBxStruct sut = CreateTestObject(false);
      CollectionAssert.AreEqual(new string[] { "Test1", "Test2" }, sut.AllTableNames, "AllTableNames #0");

      sut.Tables.RemoveAt(0);
      CollectionAssert.AreEqual(new string[] { "Test2" }, sut.AllTableNames, "AllTableNames #1");

      Assert.Catch<ArgumentOutOfRangeException>(delegate () { sut.Tables.RemoveAt(1); }, "#2");
      CollectionAssert.AreEqual(new string[] { "Test2" }, sut.AllTableNames, "AllTableNames #2");

      Assert.Catch<ArgumentOutOfRangeException>(delegate () { sut.Tables.RemoveAt(-1); }, "#3");
      CollectionAssert.AreEqual(new string[] { "Test2" }, sut.AllTableNames, "AllTableNames #3");
    }

    [Test]
    public void Tables_Clear()
    {
      DBxStruct sut = CreateTestObject(false);
      CollectionAssert.AreEqual(new string[] { "Test1", "Test2" }, sut.AllTableNames, "AllTableNames #0");

      sut.Tables.Clear();
      CollectionAssert.AreEqual(new string[] { }, sut.AllTableNames, "AllTableNames #1");
    }

    #endregion

    #region Перечислитель

    [Test]
    public void Tables_GetEnumerator([Values(false, true)]bool useSource)
    {
      DBxStruct sut = CreateTestObject(useSource);

      DoTestTablesEnumerator(sut, new string[] { "Test1", "Test2" }, "");
    }

    private static void DoTestTablesEnumerator(DBxStruct sut, string[] wanted, string message)
    {
      List<string> lst = new List<string>();
      foreach (DBxTableStruct ts in sut.Tables)
        lst.Add(ts.TableName);
      CollectionAssert.AreEqual(wanted, lst, message);
    }

    [Test]
    public void Tables_GetEnumerator_when_modified()
    {
      DBxStruct sut = CreateTestObject(false);
      sut.Tables.Add("TestXXX");
      DoTestTablesEnumerator(sut, new string[] { "Test1", "Test2", "TestXXX" }, "#1");

      sut.Tables.Remove("Test1");
      DoTestTablesEnumerator(sut, new string[] { "Test2", "TestXXX" }, "#2");

      sut.Tables.Clear();
      DoTestTablesEnumerator(sut, new string[] { }, "#3");
    }

    #endregion

    #endregion

    #region SetReadOnly()

    [Test]
    public void SetReadOnly()
    {
      DBxStruct sut = new DBxStruct();
      DBxTableStruct ts1 = new DBxTableStruct("Test1");
      ts1.Columns.AddId();
      ts1.Columns.AddString("F1", 10, true);
      sut.Tables.Add(ts1);
      sut.SetReadOnly();
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.DoesNotThrow(delegate () { sut.SetReadOnly(); }, "SetReadOnly() #2");

      CollectionAssert.AreEqual(new DBxTableStruct[1] { ts1 }, sut.Tables, "Tables");
      Assert.IsTrue(sut.Tables.IsReadOnly, "Tables.IsReadOnly");

      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Tables.Add("Test2"); }, "Tables.Add(tableName)");
      DBxTableStruct ts3 = new DBxTableStruct("Test3");
      ts3.Columns.AddId();
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Tables.Add(ts3); }, "Tables.Add(DBxTableStruct)");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Tables.Insert(0, ts3); }, "Tables.Insert()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Tables.Remove(ts1); }, "Tables.Remove(DBxTableStruct)");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Tables.Remove("Test1"); }, "Tables.Remove(tableName)");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Tables.RemoveAt(0); }, "Tables.RemoveAt()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Tables.Clear(); }, "Tables.Clear()");

      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Tables["Test1"].Columns.AddString("XXX", 5, true); }, "Columns.Add()");
      Assert.Catch<ObjectReadOnlyException>(delegate () { sut.Tables["Test1"].Indexes.Add("F1"); }, "Indexes.Add()");

      Assert.IsTrue(ts1.Columns[1].IsReadOnly, "DBxColumnStruct.IsReadOnly"); // Остальные тесты для столбца в DBxColumnStructTests
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone_simple()
    {
      DBxStruct sut = new DBxStruct();
      DBxTableStruct ts1 = new DBxTableStruct("Test1");
      ts1.Columns.AddId();
      sut.Tables.Add(ts1);
      sut.SetReadOnly();

      DBxStruct res = sut.Clone();
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      CollectionAssert.AreEqual(new string[1] { "Test1" }, res.AllTableNames, "AllTableNames");
      Assert.AreEqual("Id", sut.Tables["Test1"].Columns.Columns.AsString, "Columns");
    }

    [Test]
    public void Clone_objectWithSource()
    {
      IDBxStructSource src = new TestStructSource();
      DBxStruct sut = new DBxStruct(src);
      sut.SetReadOnly();

      DBxStruct res = sut.Clone();
      Assert.IsNull(res.Source, "Source");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
      CollectionAssert.AreEqual(new string[2] { "Test1", "Test2" }, res.AllTableNames, "AllTableNames");
      Assert.AreEqual("Id,F1", sut.Tables["Test1"].Columns.Columns.AsString, "Tables[Test1].Columns");
      Assert.AreEqual("Id,F2,F3", sut.Tables["Test2"].Columns.Columns.AsString, "Tables[Test2].Columns");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void CloneIfRequired(bool setReadOnly)
    {
      DBxStruct sut = new DBxStruct();
      DBxTableStruct ts1 = new DBxTableStruct("Test1");
      ts1.Columns.AddId();
      if (setReadOnly)
        sut.SetReadOnly();

      DBxStruct res = sut.CloneIfReadOnly();

      Assert.AreEqual(!setReadOnly, Object.ReferenceEquals(sut, res), "Instance");
      Assert.IsFalse(res.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region CreateDataSet(), CreateDataTable()

    [Test]
    public void CreateDataSet()
    {
      IDBxStructSource src = new TestStructSource();
      DBxStruct sut = new DBxStruct(src);

      DataSet res = sut.CreateDataSet();
      CollectionAssert.AreEqual(new string[2] { "Test1", "Test2" }, DataTools.GetTableNames(res), "TableNames");
      CollectionAssert.AreEqual(new string[2] { "Id", "F1" }, DataTools.GetColumnNames(res.Tables[0]), "ColumnNames(Test1)");
      CollectionAssert.AreEqual(new string[3] { "Id", "F2", "F3" }, DataTools.GetColumnNames(res.Tables[1]), "ColumnNames(Test2)");
      Assert.AreEqual(0, res.Tables[0].Rows.Count + res.Tables[1].Rows.Count, "RowCount");

      Assert.AreEqual(typeof(Int32), res.Tables[0].Columns[0].DataType, "DataType[Test1, Id]");
      Assert.IsFalse(res.Tables[0].Columns[0].AllowDBNull, "AllowDBNull[Test1, Id]");

      Assert.AreEqual(typeof(String), res.Tables[0].Columns[1].DataType, "DataType[Test1, F1]");
      Assert.IsFalse(res.Tables[0].Columns[0].AllowDBNull, "AllowDBNull[Test1, F1]");

      Assert.AreEqual(typeof(Int32), res.Tables[1].Columns[0].DataType, "DataType[Test2, Id]");
      Assert.IsFalse(res.Tables[1].Columns[0].AllowDBNull, "AllowDBNull[Test2, Id]");

      Assert.AreEqual(typeof(Int64), res.Tables[1].Columns[1].DataType, "DataType[Test2, F2]");
      Assert.IsTrue(res.Tables[1].Columns[1].AllowDBNull, "AllowDBNull[Test2, F2]");

      Assert.AreEqual(typeof(Boolean), res.Tables[1].Columns[2].DataType, "DataType[Test2, F3]");
      Assert.IsFalse(res.Tables[1].Columns[2].AllowDBNull, "AllowDBNull[Test2, F3]");
    }

    [Test]
    public void CreateDataTable_allColumns()
    {
      IDBxStructSource src = new TestStructSource();
      DBxStruct sut = new DBxStruct(src);

      DataTable res = sut.CreateDataTable("Test2", null);

      Assert.AreEqual("Test2", res.TableName, "TableName");
      Assert.AreEqual(0, res.Rows.Count, "RowCount");
      CollectionAssert.AreEqual(new string[3] { "Id", "F2", "F3" }, DataTools.GetColumnNames(res), "ColumnNames");

      Assert.AreEqual(typeof(Int32), res.Columns[0].DataType, "DataType[Id]");
      Assert.IsFalse(res.Columns[0].AllowDBNull, "AllowDBNull[Id]");

      Assert.AreEqual(typeof(Int64), res.Columns[1].DataType, "DataType[F2]");
      Assert.IsTrue(res.Columns[1].AllowDBNull, "AllowDBNull[F2]");

      Assert.AreEqual(typeof(Boolean), res.Columns[2].DataType, "DataType[F3]");
      Assert.IsFalse(res.Columns[2].AllowDBNull, "AllowDBNull[F3]");
    }

    #endregion

    #region FindColumn()

    [TestCase("Test1", "Id", true)]
    [TestCase("Test2", "F3", true)]
    [TestCase("Test0", "Id", false)]
    [TestCase("Test1", "XXX", false)]
    [TestCase("", "Id", false)]
    [TestCase("Test1", "", false)]
    [TestCase(null, "Id", false)]
    [TestCase("Test1", null, false)]
    public void FindColumn(string tableName, string columnName, bool wantedFound)
    {
      IDBxStructSource src = new TestStructSource();
      DBxStruct sut = new DBxStruct(src);

      DBxColumnStruct res = sut.FindColumn(tableName, columnName);

      if (wantedFound)
      {
        Assert.IsNotNull(res, "Found");
        Assert.AreEqual(columnName, res.ColumnName, "ColumnName");
        Assert.AreSame(sut.Tables[tableName].Columns[columnName], res, "Reference");
      }
      else
        Assert.IsNull(res, "Found");
    }

    #endregion

    #region HasComments

    [Test]
    public void HasComments_none()
    {
      DBxStruct sut = CreateTestObject(false);
      Assert.IsFalse(sut.HasComments);
    }

    [Test]
    public void HasComments_table()
    {
      DBxStruct sut = CreateTestObject(false);
      sut.Tables["Test1"].Comment = "XXX";
      Assert.IsTrue(sut.HasComments);
    }

    [Test]
    public void HasComments_column()
    {
      DBxStruct sut = CreateTestObject(false);
      sut.FindColumn("Test2", "F2").Comment = "XXX";
      Assert.IsTrue(sut.HasComments);
    }


    [Test]
    public void HasComments_index()
    {
      DBxStruct sut = CreateTestObject(false);
      DBxIndexStruct idx = sut.Tables["Test2"].Indexes.Add("F2");
      idx.Comment = "XXX";
      Assert.IsTrue(sut.HasComments);
    }

    #endregion

    #region Сериализация

    // Класс DBxStruct, в отличие от DBxTabkeStruct, не является сериализуемым!
    //[Test]
    //public void Serialization()
    //{
    //  DBxStruct sut = CreateTestObject(false);
    //  byte[] b = SerializationTools.SerializeBinary(sut);
    //  DBxStruct res = (DBxStruct)(SerializationTools.DeserializeBinary(b));
    //  Assert.AreEqual(sut.AllTableNames, res.AllTableNames);
    //}

    #endregion
  }
}
