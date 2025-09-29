using FreeLibSet.Core;
using FreeLibSet.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class GroupDbDataReaderWrapperTests
  {
    #region Тестовая таблица

    public GroupDbDataReaderWrapperTests()
    {
      _TestTable = new DataTable();
      _TestTable.Columns.Add("Id", typeof(Int32));
      _TestTable.Columns.Add("FInt32", typeof(Int32));
      _TestTable.Columns.Add("FGuid", typeof(Guid));
      _TestTable.Columns.Add("FString", typeof(string));

      _TestTable.Rows.Add(1, DBNull.Value, _Guid1, "AAA");
      _TestTable.Rows.Add(2, 0, _Guid1, "AAA");
      _TestTable.Rows.Add(3, 1, _Guid1, "AAA");
      _TestTable.Rows.Add(4, 1, _Guid1, "BBB");
      _TestTable.Rows.Add(5, 2, _Guid2, "BBB");
      _TestTable.Rows.Add(6, 2, _Guid2, "CCC");
      _TestTable.Rows.Add(7, 2, DBNull.Value, "CCC");
      _TestTable.Rows.Add(8, 2, Guid.Empty, "CCC");
      _TestTable.Rows.Add(9, 2, _Guid2, "");
      _TestTable.Rows.Add(10, 2, _Guid2, DBNull.Value);
    }

    private DataTable _TestTable;
    private Guid _Guid1 = new Guid("{932AC6AB-35AC-4B14-93B4-5EB969F8A56D}");
    private Guid _Guid2 = new Guid("{851E2BB4-7892-429F-843D-E31D9D423653}");

    #endregion

    #region Конструктор

    [Test]
    public void Constructror()
    {
      using (DbDataReader reader = _TestTable.CreateDataReader())
      {
        GroupDbDataReaderWrapper sut = new GroupDbDataReaderWrapper(reader, "FGuid,FInt32");
        Assert.AreEqual(_TestTable.Columns.Count, sut.Table.Columns.Count, "Table.ColumnCount");
        for (int i = 0; i < _TestTable.Columns.Count; i++)
        {
          Assert.AreEqual(_TestTable.Columns[i].ColumnName, sut.Table.Columns[i].ColumnName, "ColumnName");
          Assert.AreEqual(_TestTable.Columns[i].DataType, sut.Table.Columns[i].DataType, "DataType");
        }
        Assert.IsFalse(sut.DBNullAsZero, "DBNullAsZero");
      }
    }

    #endregion

    #region Read()

    [TestCase("FInt32", false, "1,1,2,6")]
    [TestCase("FInt32", true, "2,2,6")]
    [TestCase("FGuid", false, "4,2,1,1,2")]
    [TestCase("FGuid", true, "4,2,2,2")]
    [TestCase("FString", false, "3,2,3,1,1")]
    [TestCase("FString", true, "3,2,3,2")]
    [TestCase("FInt32,FGuid", false, "1,1,2,2,1,1,2")]
    [TestCase("FInt32,FGuid", true, "2,2,2,2,2")]
    [TestCase("FGuid,FInt32", false, "1,1,2,2,1,1,2")]
    [TestCase("FGuid,FInt32", true, "2,2,2,2,2")]
    [TestCase("FInt32,FGuid,FString", false, "1,1,1,1,1,1,1,1,1,1")]
    [TestCase("FInt32,FGuid,FString", true, "2,1,1,1,1,2,2")]
    public void Read(string keyColumnNames, bool dbNullAsZero, string sWantedGroups)
    {
      int[] wantedGroups = StdConvert.ToInt32Array(sWantedGroups);
      Assert.AreEqual(_TestTable.Rows.Count, DataTools.SumInt32(wantedGroups), "Wanted Groups");

      using (DbDataReader reader = _TestTable.CreateDataReader())
      {
        GroupDbDataReaderWrapper sut = new GroupDbDataReaderWrapper(reader, keyColumnNames);
        sut.DBNullAsZero = dbNullAsZero;
        string groups = DoRead(sut, _TestTable.Rows.Count);
        Assert.AreEqual(sWantedGroups, groups);
      }
    }

    [Test]
    public void Read_NoRows()
    {
      DataTable table = _TestTable.Clone();
      Assert.AreEqual(0, table.Rows.Count);

      using (DbDataReader reader = table.CreateDataReader())
      {
        GroupDbDataReaderWrapper sut = new GroupDbDataReaderWrapper(reader, "FInt32,FGuid");
        string groups = DoRead(sut, 0);
        Assert.AreEqual("", groups);
      }
    }

    [Test]
    public void Read_OneRow()
    {
      DataTable table = _TestTable.Clone();
      table.Rows.Add(1, 123);
      Assert.AreEqual(1, table.Rows.Count);

      using (DbDataReader reader = table.CreateDataReader())
      {
        GroupDbDataReaderWrapper sut = new GroupDbDataReaderWrapper(reader, "FInt32,FGuid");
        string groups = DoRead(sut, 1);
        Assert.AreEqual("1", groups);
      }
    }

    private static string DoRead(GroupDbDataReaderWrapper sut, int wantedRowCount)
    {
      StringBuilder sb = new StringBuilder();
      int cnt = 0;
      while (sut.Read())
      {
        if (sb.Length > 0)
          sb.Append(',');

        foreach (DataRow row in sut.Table.Rows)
        {
          cnt++;
          Int32 thisId = (Int32)(row["Id"]);
          Assert.AreEqual(cnt, thisId, "Id secuence");
        }
        sb.Append(StdConvert.ToString(sut.Table.Rows.Count));
      }

      Assert.AreEqual(wantedRowCount, cnt, "Total Row Count");
      return sb.ToString();
    }

    /*
    private int[][] CreateWantedIds(string sWantedGroups)
    {
      int[] wantedGroups = StdConvert.ToInt32Array(sWantedGroups);
      Int32[][] wantedIds = new Int32[wantedGroups.Length][];
      int cnt = 0;
      for (int i = 0; i < wantedGroups.Length; i++)
      {
        Int32[] groupIds = new Int32[wantedGroups[i]];
        for(int j=0)
      }
    }
    */

    #endregion

    #region Прочее

    [Test]
    public void Table()
    {
      using (DbDataReader reader = _TestTable.CreateDataReader())
      {
        GroupDbDataReaderWrapper sut = new GroupDbDataReaderWrapper(reader, "FGuid,FInt32");
        DataTable firstTable = sut.Table;
        while (sut.Read())
        {
          Assert.AreSame(firstTable, sut.Table, "Table");
          Assert.GreaterOrEqual(sut.Table.Rows.Count, 1, "Row count");
        }
      }

      // Состояние Table после окончания чтения не определено
    }

    #endregion
  }
}
