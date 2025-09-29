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
  public class SQLiteDBxTests_StringFilters
  {
    [TestCase("abcdefghijklmnopqrstuvwxyz", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    [TestCase("абвгдеёжзийклмнопрстуфхцчшщъыьэюя", "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ")]
    [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    [TestCase("АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ", "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ")]
    [TestCase("1234567890", "1234567890")]
    public void Upper(string arg, string wanted)
    {
      using (SQLiteDBx db = new SQLiteDBx())
      {
        using (DBxConBase con = db.MainEntry.CreateCon())
        {
          Assert.AreEqual(wanted, con.SQLExecuteScalar("SELECT UPPER(\'"+arg+"\')"));
        }
      }
    }

    [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "abcdefghijklmnopqrstuvwxyz")]
    [TestCase("АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ", "абвгдеёжзийклмнопрстуфхцчшщъыьэюя")]
    [TestCase("abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqrstuvwxyz")]
    [TestCase("абвгдеёжзийклмнопрстуфхцчшщъыьэюя", "абвгдеёжзийклмнопрстуфхцчшщъыьэюя")]
    [TestCase("1234567890", "1234567890")]
    public void Lower(string arg, string wanted)
    {
      using (SQLiteDBx db = new SQLiteDBx())
      {
        using (DBxConBase con = db.MainEntry.CreateCon())
        {
          Assert.AreEqual(wanted, con.SQLExecuteScalar("SELECT LOWER(\'" + arg + "\')"));
        }
      }
    }

    [Test]
    public void Upper_null()
    {
      using (SQLiteDBx db = new SQLiteDBx())
      {
        using (DBxConBase con = db.MainEntry.CreateCon())
        {
          Assert.DoesNotThrow(delegate() { con.SQLExecuteNonQuery("SELECT UPPER(NULL)"); });
        }
      }
    }

    [Test]
    public void Lower_null()
    {
      using (SQLiteDBx db = new SQLiteDBx())
      {
        using (DBxConBase con = db.MainEntry.CreateCon())
        {
          Assert.DoesNotThrow(delegate() { con.SQLExecuteNonQuery("SELECT LOWER(NULL)"); });
        }
      }
    }

    [TestCase("ABC", false, "1")]
    [TestCase("aBc", false, "2")]
    [TestCase("aBc", true, "1,2,3")]
    [TestCase("АБВ", false, "4")]
    [TestCase("аБв", false, "5")]
    [TestCase("аБв", true, "4,5,6")]
    [TestCase("123", false, "10")]
    [TestCase("123", true, "10")]
    public void StringValueFilter(string value, bool ignoreCase, string sWantedIds)
    {

      using (SQLiteDBx db = CreateSampleDB())
      {
        using (DBxConBase con = db.MainEntry.CreateCon())
        {
          con.SQLExecuteNonQuery("PRAGMA case_sensitive_like = OFF");
          IIdSet<Int32> ids = IdTools.AsIdSet<Int32>(con.GetIds("Tab1", new StringValueFilter("Col2", value, ignoreCase)));
          CollectionAssert.AreEquivalent(GetResultIds(sWantedIds), ids, "\"" + value + "\", IgnoreCase=" + ignoreCase.ToString());
        }
      }
    }

    [TestCase("BC", false, "1")]
    [TestCase("Bc", false, "2")]
    [TestCase("Bc", true, "1,2,3")]
    [TestCase("БВ", false, "4")]
    [TestCase("Бв", false, "5")]
    [TestCase("Бв", true, "4,5,6")]
    [TestCase("23", false, "7,8,9,10")]
    [TestCase("23", true, "7,8,9,10")]
    public void SubstringFilter(string value, bool ignoreCase, string sWantedIds)
    {
      using (SQLiteDBx db = CreateSampleDB())
      {
        using (DBxConBase con = db.MainEntry.CreateCon())
        {
          con.SQLExecuteNonQuery("PRAGMA case_sensitive_like = OFF");
          IIdSet<Int32> ids = IdTools.AsIdSet<Int32> (con.GetIds("Tab1", new SubstringFilter("Col2", 1, value, ignoreCase)));
          CollectionAssert.AreEquivalent(GetResultIds(sWantedIds), ids, "\"" + value + "\", IgnoreCase=" + ignoreCase.ToString());
        }
      }
    }

    [TestCase("AB", false, "1")]
    [TestCase("aB", false, "2")]
    [TestCase("aB", true, "1,2,3")]
    [TestCase("АБ", false, "4")]
    [TestCase("аБ", false, "5")]
    [TestCase("аБ", true, "4,5,6")]
    [TestCase("12", false, "7,8,9,10,11")]
    [TestCase("12", true, "7,8,9,10,11")]
    public void StartsWithFilter(string value, bool ignoreCase, string sWantedIds)
    {
      using (SQLiteDBx db = CreateSampleDB())
      {
        using (DBxConBase con = db.MainEntry.CreateCon())
        {
          con.SQLExecuteNonQuery("PRAGMA case_sensitive_like = OFF");
          IIdSet<Int32> ids = IdTools.AsIdSet<Int32> (con.GetIds("Tab1", new StartsWithFilter("Col2", value, ignoreCase)));
          Assert.AreEqual(GetResultIds(sWantedIds), ids, "\"" + value + "\", IgnoreCase=" + ignoreCase.ToString());
        }
      }
    }


    private SQLiteDBx CreateSampleDB()
    {
      DBxStruct dbs = new DBxStruct();
      DBxTableStruct ts = dbs.Tables.Add("Tab1");
      ts.Columns.AddId("Id");
      ts.Columns.AddString("Col2", 6, false);

      SQLiteDBx db = new SQLiteDBx();
      db.Struct = dbs;
      db.UpdateStruct();

      DataTable tbl = new DataTable("Tab1");
      tbl.Columns.Add("Id", typeof(int));
      tbl.Columns.Add("Col2", typeof(string));

      // Проверять нужно отдельно и латинские буквы, и русские.
      // См. https://www.sqlite.org/lang_expr.html и https://www.sqlite.org/pragma.html#pragma_case_sensitive_like
      tbl.Rows.Add(1, "ABC");
      tbl.Rows.Add(2, "aBc");
      tbl.Rows.Add(3, "abc");
      tbl.Rows.Add(4, "АБВ");
      tbl.Rows.Add(5, "аБв");
      tbl.Rows.Add(6, "абв");
      tbl.Rows.Add(7, "123ABC");
      tbl.Rows.Add(8, "123aBc");
      tbl.Rows.Add(9, "123abc");
      tbl.Rows.Add(10, "123");
      tbl.Rows.Add(11, "12"); // для проверки SUBSTR() выходящей за длину строки
      tbl.Rows.Add(12, "1");

      using (DBxCon con = new DBxCon(db.MainEntry))
      {
        con.AddRecords(tbl);
        Assert.AreEqual(tbl.Rows.Count, con.GetRecordCount("Tab1"), "Sample RowCount");
      }

      return db;
    }

    private static IIdSet<Int32> GetResultIds(string sIds)
    {
      Int32[] aIds = StdConvert.ToInt32Array(sIds);
      return new IdList<Int32>(aIds);
    }
  }
}
