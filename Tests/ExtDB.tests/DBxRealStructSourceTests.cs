using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.Data.SQLite;
using System.Data;
using FreeLibSet.Data.SqlClient;
using FreeLibSet.Tests;

namespace ExtDB_tests.Data
{
  [TestFixture]
  public abstract class DBxRealStructSourceTestsBase: FixtureWithSetUp
  {
    #region Конструктор

    public DBxRealStructSourceTestsBase()
    {
      _TestStruct = new DBxStruct();
    }

    #endregion

    #region Абстрактные методы и свойства

    protected abstract DBxEntry Entry { get; }

    /// <summary>
    /// Структура заполняется производным классом и отличается для разных БД
    /// </summary>
    protected DBxStruct TestStruct { get { return _TestStruct; } }
    private DBxStruct _TestStruct;

    #endregion

    #region Test

    [Test]
    public void Test()
    {
      DBxRealStructSource sut = new DBxRealStructSource(Entry);
      Assert.AreSame(Entry, sut.Entry, "Entry");
      // Порядок таблиц может быть любым
      CollectionAssert.AreEquivalent(TestStruct.AllTableNames, sut.GetAllTableNames(), "TableNames");
      foreach (DBxTableStruct wantedTS in TestStruct.Tables)
      {
        string msg1 = "Table=" + wantedTS.TableName;
        DBxTableStruct realTS = sut.GetTableStruct(wantedTS.TableName);
        Assert.IsNotNull(realTS, msg1 + "-GetTableStruct()");
        Assert.AreEqual(wantedTS.TableName, realTS.TableName, msg1 + "-TableName");
        Assert.AreEqual(wantedTS.Comment, realTS.Comment, msg1 + "-Comment");
        CollectionAssert.AreEquivalent(wantedTS.AllColumnNames, realTS.AllColumnNames, msg1 + "-AllColumnNames");
        foreach (DBxColumnStruct wantedCS in wantedTS.Columns)
        {
          string msg2 = msg1 + ", Column=" + wantedCS.ColumnName;
          DBxColumnStruct realCS = realTS.Columns.GetRequired(wantedCS.ColumnName);
          Assert.AreEqual(wantedCS.ColumnName, realCS.ColumnName, msg2 + "-ColumnName");
          Assert.AreEqual(wantedCS.ColumnType, realCS.ColumnType, msg2 + "-ColumnType");
          Assert.AreEqual(wantedCS.Nullable, realCS.Nullable, msg2 + "-Nullable");
          Assert.AreEqual(wantedCS.DefaultValue, realCS.DefaultValue, msg2 + "-DefaultValue");
          Assert.AreEqual(wantedCS.MaxLength, realCS.MaxLength, msg2 + "-MaxLength");
          Assert.AreEqual(wantedCS.DataType, realCS.DataType, msg2 + "-DataType");
          Assert.AreEqual(wantedCS.Comment, realCS.Comment, msg2 + "-Comment");
        }

        foreach (DBxIndexStruct wantedIS in wantedTS.Indexes)
        {
          string msg2 = msg1 + ", Index=" + wantedIS.IndexName;
          DBxIndexStruct realIS = realTS.Indexes.FindByColumns(wantedIS.Columns);
          Assert.IsNotNull(realIS, msg2 + "-FindByColumns()");
          Assert.AreEqual(wantedIS.Comment, realIS.Comment, msg2 + "-Comment");
        }
      }
    }

    #endregion
  }
}

namespace ExtDB_tests.Data_SQLite
{
  [TestFixture]
  public class DBxRealStructSourceTestsSQLite : ExtDB_tests.Data.DBxRealStructSourceTestsBase
  {
    #region База данных в памяти

    protected override void OnOneTimeSetUp()
    {
      base.OnOneTimeSetUp();

      _DB = new SQLiteDBx();

      StringBuilder sb = new StringBuilder();
      sb.Append("CREATE TABLE T1 (");
      sb.Append("Id INTEGER PRIMARY KEY, ");
      sb.Append("F1 CHAR(20), ");
      sb.Append("F2 CHAR(30) NOT NULL, ");
      sb.Append("F3 INT, ");
      sb.Append("F4 SMALLINT, ");
      sb.Append("F5 TINYINT, ");
      sb.Append("F6 REAL, ");
      sb.Append("F7 NUMERIC(18,2), ");
      //sb.Append("F8 BOOLEAN NOT NULL DEFAULT FALSE, ");
      sb.Append("F9 BOOLEAN, ");
      sb.Append("F10 DATE, ");
      sb.Append("F11 TIMESTAMP, ");
      sb.Append("F12 CLOB");
      sb.Append(")");

      DBxTableStruct ts = TestStruct.Tables.Add("T1");
      DBxColumnStruct cs;
      ts.Columns.AddInt32("Id", false);
      ts.Columns.AddString("F1", 20, true);
      ts.Columns.AddString("F2", 30, false);
      ts.Columns.AddInt32("F3", true);
      ts.Columns.AddInt16("F4", true);
      ts.Columns.AddInteger("F5", 0, 255, true);
      ts.Columns.AddDouble("F6", true);
      ts.Columns.AddDecimal("F7", true);
      //ts.Columns.AddBoolean("F8");
      cs = ts.Columns.AddBoolean("F9");
      cs.Nullable = true;
      ts.Columns.AddDate("F10", true);
      ts.Columns.AddDateTime("F11", true);
      ts.Columns.AddMemo("F12");

      using (DBxConBase con = Entry.CreateCon())
      {
        con.SQLExecuteNonQuery(sb.ToString());
      }
    }

    protected override void OnOneTimeTearDown()
    {
      if (_DB != null)
        _DB.Dispose();
      base.OnOneTimeTearDown();
    }

    private SQLiteDBx _DB;

    #endregion

    #region Переопределенные методы

    protected override DBxEntry Entry { get { return _DB.MainEntry; } }

    #endregion
  }
}
