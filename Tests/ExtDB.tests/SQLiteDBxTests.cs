using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data.SQLite;
using FreeLibSet.Data;
using FreeLibSet.IO;

namespace ExtDB.tests
{
  [TestFixture]
  public class SQLiteDBxTests
  {
    #region Рабочая конфигурация

    private TempDirectory _TempDir;

    private SQLiteDBx _DB;

    [SetUp]
    public void Setup()
    {
      _TempDir = new TempDirectory();
    }

    [TearDown]
    public void TearDown()
    {
      if (_DB != null)
      {
        _DB.Dispose();
        _DB = null;
      }

      if (_TempDir != null)
      {
        //_TempDir.Dispose();
        _TempDir = null;
      }
    }

    #endregion

    [Test]
    public void Constructor()
    {
      AbsPath path = new AbsPath(_TempDir.Dir, "test.db");
      _DB = new SQLiteDBx(path, false);
      Assert.IsFalse(_DB.InMemory, "InMemory");
      //может вернуть пустую структуру Assert.IsNull(_DB.Struct, "Struct"); 
      Assert.IsFalse(_DB.StructHasBeenSet, "StructHasBeenSet");
      Assert.IsFalse(_DB.StructHasBeenUpdated, "StructHasBeenUpdated");
      Assert.IsNotNull(_DB.MainEntry, "MainEntry");
      Assert.IsFalse(_DB.DatabaseExists, "DatabaseExists");
      Assert.AreEqual(path.Path, _DB.FileName.Path, "FileName");
    }

    [Test]
    public void CreateIfRequired()
    {
      AbsPath path = new AbsPath(_TempDir.Dir, "test.db");
      _DB = new SQLiteDBx(path, false);

      _DB.CreateIfRequired();
      Assert.IsTrue(_DB.DatabaseExists, "DatabaseExists");
    }

    [Test]
    public void UpdateStruct()
    {
      AbsPath path = new AbsPath(_TempDir.Dir, "test.db");
      _DB = new SQLiteDBx(path, false);

      _DB.CreateIfRequired();

      DBxStruct dbs = CreateDBStruct();
      _DB.Struct = dbs;
      Assert.AreSame(dbs, _DB.Struct, "Struct");
      Assert.IsTrue(_DB.StructHasBeenSet, "StructHasBeenSet before update");
      Assert.IsFalse(_DB.StructHasBeenUpdated, "StructHasBeenUpdated before update");

      bool res = _DB.UpdateStruct();
      Assert.IsTrue(_DB.StructHasBeenUpdated, "StructHasBeenUpdated after update");
      Assert.IsTrue(res, "UpdateStruct() returned true");

      _DB.ResetStruct();
      Assert.IsFalse(_DB.StructHasBeenSet, "StructHasBeenSet after ResetStruct()");

      DBxTestHelpers.ValidateStruct(dbs, _DB.Struct, "Created struct");
    }

    private static DBxStruct CreateDBStruct()
    {
      DBxStruct dbs = new DBxStruct();
      DBxTableStruct ts = dbs.Tables.Add("Table1");
      ts.Columns.AddId();
      ts.Columns.AddString("F1", 20, false);
      ts.Columns.AddInt("F2");
      return dbs;
    }
  }
}
