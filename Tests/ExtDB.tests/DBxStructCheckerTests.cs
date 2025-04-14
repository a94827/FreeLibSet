using System;
using System.Collections.Generic;
using System.Text;
using ExtTools_tests.Data;
using FreeLibSet.Data;
using FreeLibSet.Data.SQLite;
using FreeLibSet.Tests;
using NUnit.Framework;

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DBxStructCheckerTests : FixtureWithSetUp
  {
    #region CheckStruct()

    protected override void OnOneTimeSetUp()
    {
      base.OnOneTimeSetUp();
      _CheckStructDB = new SQLiteDBx();
    }

    protected override void OnOneTimeTearDown()
    {
      if (_CheckStructDB != null)
        _CheckStructDB.Dispose();
      base.OnOneTimeTearDown();
    }

    private SQLiteDBx _CheckStructDB;

    [Test]
    public void CheckStruct_ok()
    {
      DBxStruct dbs = DBxStructTests.CreateTestObject(false);
      Assert.DoesNotThrow(delegate () { DBxStructChecker.CheckStruct(dbs, _CheckStructDB); });
    }

    [Test]
    public void CheckStruct_invalidTableName()
    {
      DBxStruct dbs = DBxStructTests.CreateTestObject(false);
      DBxTableStruct ts = dbs.Tables.Add("===");
      ts.Columns.AddId();
      Assert.Catch<DBxStructException>(delegate () { DBxStructChecker.CheckStruct(dbs, _CheckStructDB); });
    }

    [Test]
    public void CheckStruct_tableWithoutColumns()
    {
      DBxStruct dbs = DBxStructTests.CreateTestObject(false);
      DBxTableStruct ts = dbs.Tables.Add("XXX");
      Assert.Catch<DBxStructException>(delegate () { DBxStructChecker.CheckStruct(dbs,_CheckStructDB); });
    }

    [Test]
    public void CheckStruct_invalidColumnName()
    {
      DBxStruct dbs = DBxStructTests.CreateTestObject(false);
      DBxTableStruct ts = dbs.Tables.GetRequired("Test1");
      ts.Columns.AddString("===", 10, true);
      Assert.Catch<DBxStructException>(delegate () { DBxStructChecker.CheckStruct(dbs, _CheckStructDB); });
    }

    [Test]
    public void CheckStruct_unknownColumnType()
    {
      DBxStruct dbs = DBxStructTests.CreateTestObject(false);
      DBxTableStruct ts = dbs.Tables.GetRequired("Test1");
      DBxColumnStruct cs = new DBxColumnStruct("XXX");
      ts.Columns.Add(cs);
      Assert.Catch<DBxStructException>(delegate () { DBxStructChecker.CheckStruct(dbs, _CheckStructDB); });
    }

    [Test]
    public void CheckStruct_zeroStringLength()
    {
      DBxStruct dbs = DBxStructTests.CreateTestObject(false);
      DBxTableStruct ts = dbs.Tables.GetRequired("Test1");
      DBxColumnStruct cs = new DBxColumnStruct("XXX");
      cs.ColumnType = DBxColumnType.String;
      cs.MaxLength = 0;
      ts.Columns.Add(cs);
      Assert.Catch<DBxStructException>(delegate () { DBxStructChecker.CheckStruct(dbs, null); });
    }

    [Test]
    public void CheckStruct_invalidMinMax()
    {
      DBxStruct dbs = DBxStructTests.CreateTestObject(false);
      DBxTableStruct ts = dbs.Tables.GetRequired("Test1");
      DBxColumnStruct cs = new DBxColumnStruct("XXX");
      cs.ColumnType = DBxColumnType.Int;
      cs.MinValue = 10;
      cs.MaxValue = 5;
      ts.Columns.Add(cs);
      Assert.Catch<DBxStructException>(delegate () { DBxStructChecker.CheckStruct(dbs, null); });
    }

    [Test]
    public void CheckStruct_refCol_invalidMasterTableName()
    {
      DBxStruct dbs = DBxStructTests.CreateTestObject(false);
      dbs.Tables["Test2"].Columns.AddReference("RR", "T666");
      Assert.Catch<DBxStructException>(delegate () { DBxStructChecker.CheckStruct(dbs, null); });
    }

    [Test]
    public void CheckStruct_refCol_invalidColumnType()
    {
      DBxStruct dbs = DBxStructTests.CreateTestObject(false);
      DBxColumnStruct cs = new DBxColumnStruct("RR");
      cs.MasterTableName = "Test1";
      cs.ColumnType = DBxColumnType.Guid;
      dbs.Tables["Test2"].Columns.Add(cs);
      Assert.Catch<DBxStructException>(delegate () { DBxStructChecker.CheckStruct(dbs, null); });
    }

    [Test]
    public void CheckStruct_refCol_noPK()
    {
      DBxStruct dbs = DBxStructTests.CreateTestObject(false);
      dbs.Tables["Test1"].PrimaryKey = DBxColumns.Empty;
      dbs.Tables["Test2"].Columns.AddReference("RR", "Test1");
      Assert.Catch<DBxStructException>(delegate () { DBxStructChecker.CheckStruct(dbs, null); });
    }

    #endregion
  }
}
