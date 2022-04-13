using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data.SQLite;
using FreeLibSet.Data;
using System.Data;
using FreeLibSet.Core;

namespace ExtDB_tests.Data_SQLite
{
  [TestFixture]
  public class SQLiteDBxTests_InMemory
  {
    [Test]
    public void SimpleUsage()
    {
      using (SQLiteDBx db = CreateSampleDB())
      {
        AddTestRows(db, 5, 7);
        int sum = GetSumValue(db);
        Assert.AreEqual(5 + 6 + 7, sum);
      }
    }

    [Test]
    public void ConcurentUsage()
    {
      using (SQLiteDBx db1 = CreateSampleDB())
      {
        using (SQLiteDBx db2 = CreateSampleDB())
        {
          using (SQLiteDBx db3 = CreateSampleDB())
          {
            AddTestRows(db1, 5, 7);
            AddTestRows(db2, 7, 11);
            AddTestRows(db3, 12, 14);
            int sum1 = GetSumValue(db1);
            int sum2 = GetSumValue(db2);
            int sum3 = GetSumValue(db3);

            Assert.AreEqual(5 + 6 + 7, sum1, "DB #1");
            Assert.AreEqual(7 + 8 + 9 + 10 + 11, sum2, "DB #2");
            Assert.AreEqual(12 + 13 + 14, sum3, "DB #3");
          }
        }
      }
    }

    private static SQLiteDBx CreateSampleDB()
    {
      DBxStruct dbs = new DBxStruct();
      DBxTableStruct ts = dbs.Tables.Add("FirstTable");
      ts.Columns.AddId();
      ts.Columns.AddInt("Col1");
      ts.Columns.AddReference("Col2", "FirstTable", true);

      SQLiteDBx db = new SQLiteDBx();
      db.Struct = dbs;
      db.UpdateStruct();
      return db;
    }

    private static void AddTestRows(SQLiteDBx db, int v1, int v2)
    {
      using (DBxCon con = new DBxCon(db.MainEntry))
      {
        DataTable table = new DataTable();
        table.Columns.Add("Col1", typeof(int));
        for (int v = v1; v <= v2; v++)
          table.Rows.Add(v);

        con.AddRecords("FirstTable", table);
      }
    }

    private int GetSumValue(SQLiteDBx db)
    {
      int sum;
      using (DBxCon con = new DBxCon(db.MainEntry))
      {
        sum = DataTools.GetInt(con.GetSumValue("FirstTable", "Col1", null));
      }
      return sum;
    }

    [Test]
    public void RefsIntegrity()
    {
      using (SQLiteDBx db = CreateSampleDB())
      {
        using (DBxCon con = new DBxCon(db.MainEntry))
        {
          con.AddRecord("FirstTable", new DBxColumns("Id,Col1"), new object[] { 1, 1 });
          con.AddRecord("FirstTable", new DBxColumns("Id,Col1,Col2"), new object[] { 2, 2, 1 });

          Assert.Catch(delegate() { con.Delete("FirstTable", 1); });
        }
      }
    }
  }
}
