using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using NUnit.Framework;
using System.Data;
using FreeLibSet.Core;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class DataTableRepeaterTests
  {
    #region Constructor()

    [Test]
    public void Constructor()
    {
      DataTableRepeater sut = new DataTableRepeater();
      Assert.IsNull(sut.MasterTable, "MasterTable");
      Assert.IsNotNull(sut.SlaveTable, "SlaveTable");
      Assert.AreEqual(0, sut.SlaveTable.Columns.Count, "SlaveTable.Columns.Count");

      sut.Dispose();
    }

    #endregion

    #region MasterTable

    [Test]
    public void MasterTable_set_noPK()
    {
      DataTableRepeater sut = new DataTableRepeater();
      InitTestSlaveTable(sut.SlaveTable);
      sut.ValueNeeded += Test_ValueNeeded;

      DataTable masterTable = CreateTestMasterTable();
      sut.MasterTable = masterTable;

      Assert.AreSame(masterTable, sut.MasterTable, "MasterTable");
      Assert.AreEqual(new int[] { 2, 4, 6 }, DataTools.GetValuesFromColumn<int>(sut.SlaveTable, "F4"), "F4");
    }

    [Test]
    public void MasterTable_set_withPK()
    {
      DataTableRepeater sut = new DataTableRepeater();
      InitTestSlaveTable(sut.SlaveTable);
      DataTools.SetPrimaryKey(sut.SlaveTable, "F1");
      sut.ValueNeeded += Test_ValueNeeded;

      DataTable masterTable = CreateTestMasterTable();
      DataTools.SetPrimaryKey(masterTable, "F1");
      sut.MasterTable = masterTable;

      Assert.AreSame(masterTable, sut.MasterTable, "MasterTable");
      Assert.AreEqual(new int[] { 2, 4, 6 }, DataTools.GetValuesFromColumn<int>(sut.SlaveTable, "F4"), "F4");
    }

    [Test]
    public void MasterTable_set_exception_mastertablewithoutPK()
    {
      DataTableRepeater sut = new DataTableRepeater();
      InitTestSlaveTable(sut.SlaveTable);
      DataTools.SetPrimaryKey(sut.SlaveTable, "F1");
      sut.ValueNeeded += Test_ValueNeeded;

      DataTable masterTable = CreateTestMasterTable();
      masterTable.PrimaryKey = new DataColumn[0];
      Assert.Catch(delegate() { sut.MasterTable = masterTable; });
    }

    [Test]
    public void MasterTable_set_exception_emptyslavetable()
    {
      DataTableRepeater sut = new DataTableRepeater();
      DataTable tbl = CreateTestMasterTable();

      Assert.Catch(delegate() { sut.MasterTable = tbl; });
    }

    #endregion

    #region Вспомогательные методы

    private static DataTable CreateTestMasterTable()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("F1", typeof(int));
      tbl.Columns.Add("F2", typeof(string));
      tbl.Columns.Add("F3", typeof(int));
      tbl.Rows.Add(1, "AAA", 1);
      tbl.Rows.Add(2, "BBB", 2);
      tbl.Rows.Add(3, "CCC", 3);
      return tbl;
    }

    private static void InitTestSlaveTable(DataTable slaveTable)
    {
      slaveTable.Columns.Add("F1", typeof(int));
      slaveTable.Columns.Add("F2", typeof(string));
      // не добавляем slaveTable.Columns.Add("F3", typeof(int));
      slaveTable.Columns.Add("F4", typeof(int)); // новое поле
    }

    private static void Test_ValueNeeded(object sender, DataTableRepeaterValueNeededEventArgs args)
    {
      switch (args.ColumnName)
      {
        case "F4":
          args.Value = DataTools.GetInt(args.SourceRow, "F3") * 2;
          break;
      }
    }

    /// <summary>
    /// Режимы тестирования для первичного ключа
    /// </summary>
    public enum PKMode
    {
      /// <summary>
      /// Нет первичного ключа. Связь по номерам строк
      /// </summary>
      None,

      /// <summary>
      /// Простой первичный ключ по одному полю F1
      /// </summary>
      Simple,

      /// <summary>
      /// Составной первичный ключ по полям F1+F2
      /// </summary>
      Complex
    }

    private static DataTableRepeater CreateTestObject(PKMode pkMode)
    {
      DataTableRepeater sut = new DataTableRepeater();
      InitTestSlaveTable(sut.SlaveTable);
      switch (pkMode)
      {
        case PKMode.Simple:
          DataTools.SetPrimaryKey(sut.SlaveTable, "F1");
          break;
        case PKMode.Complex:
          DataTools.SetPrimaryKey(sut.SlaveTable, "F1,F2");
          break;
      }

      sut.ValueNeeded += Test_ValueNeeded;
      DataTable masterTable = CreateTestMasterTable();
      DataTools.SetPrimaryKey(masterTable, DataTools.GetPrimaryKey(sut.SlaveTable)); // одинаковые ключи
      sut.MasterTable = masterTable;
      return sut;
    }

    #endregion

    #region Master data changing

    [Test]
    public void MasterValueChanged_DirectCopy([Values(PKMode.None, PKMode.Simple
      /*, PKMode.Complex нельзя менять ключевое поле F2*/)]
      PKMode pkMode)
    {
      DataTableRepeater sut = CreateTestObject(pkMode);

      sut.MasterTable.Rows[1]["F2"] = "XXX";

      Assert.AreEqual(new string[] { "AAA", "XXX", "CCC" }, DataTools.GetValuesFromColumn<string>(sut.SlaveTable, "F2"), "F2");
    }

    [Test]
    public void MasterValueChanged_Calculated([Values(PKMode.None, PKMode.Simple, PKMode.Complex)]PKMode pkMode)
    {
      DataTableRepeater sut = CreateTestObject(pkMode);

      sut.MasterTable.Rows[1]["F3"] = 10;

      Assert.AreEqual(new int[] { 2, 20, 6 }, DataTools.GetValuesFromColumn<int>(sut.SlaveTable, "F4"), "F4");
    }

    [Test]
    public void MasterAddRow([Values(PKMode.None, PKMode.Simple, PKMode.Complex)]PKMode pkMode)
    {
      DataTableRepeater sut = CreateTestObject(pkMode);

      sut.MasterTable.Rows.Add(4, "DDD", 4);

      Assert.AreEqual(new int[] { 1, 2, 3, 4 }, DataTools.GetValuesFromColumn<int>(sut.SlaveTable, "F1"), "F1");
      Assert.AreEqual(new string[] { "AAA", "BBB", "CCC", "DDD" }, DataTools.GetValuesFromColumn<string>(sut.SlaveTable, "F2"), "F2");
      Assert.AreEqual(new int[] { 2, 4, 6, 8 }, DataTools.GetValuesFromColumn<int>(sut.SlaveTable, "F4"), "F4");
    }

    [Test]
    public void MasterDeleteRow([Values(PKMode.None, PKMode.Simple, PKMode.Complex)]PKMode pkMode)
    {
      DataTableRepeater sut = CreateTestObject(pkMode);

      sut.MasterTable.Rows[1].Delete();

      Assert.AreEqual(new int[] { 1, 3 }, DataTools.GetValuesFromColumn<int>(sut.SlaveTable, "F1"), "F1");
      Assert.AreEqual(new string[] { "AAA", "CCC" }, DataTools.GetValuesFromColumn<string>(sut.SlaveTable, "F2"), "F2");
      Assert.AreEqual(new int[] { 2, 6 }, DataTools.GetValuesFromColumn<int>(sut.SlaveTable, "F4"), "F4");
    }

    [Test]
    public void MasterClearRows([Values(PKMode.None, PKMode.Simple, PKMode.Complex)]PKMode pkMode)
    {
      DataTableRepeater sut = CreateTestObject(pkMode);

      sut.MasterTable.Rows.Clear();

      Assert.AreEqual(0, sut.SlaveTable.Rows.Count);
    }

    #endregion

    #region Master<->Slave rows

    [Test]
    public void GetSlaveRow_frommasterrow([Values(PKMode.None, PKMode.Simple, PKMode.Complex)]PKMode pkMode)
    {
      DataTableRepeater sut = CreateTestObject(pkMode);

      DataRow slaveRow = sut.GetSlaveRow(sut.MasterTable.Rows[1]);
      Assert.AreEqual(new object[] { 2, "BBB", 4 }, slaveRow.ItemArray);
    }

    [Test]
    public void GetSlaveRow_null([Values(PKMode.None, PKMode.Simple, PKMode.Complex)]PKMode pkMode)
    {
      DataTableRepeater sut = CreateTestObject(pkMode);
      Assert.IsNull(sut.GetSlaveRow(null));
    }

    [Test]
    public void GetSlaveRows([Values(PKMode.None, PKMode.Simple, PKMode.Complex)]PKMode pkMode)
    {
      DataTableRepeater sut = CreateTestObject(pkMode);

      DataRow[] masterRows = new DataRow[] { sut.MasterTable.Rows[0], sut.MasterTable.Rows[2] };
      DataRow[] slaveRows = sut.GetSlaveRows(masterRows);
      Assert.AreEqual(new object[] { 2, 6 }, DataTools.GetValuesFromColumn<int>(slaveRows, "F4"));
    }


    [Test]
    public void GetMasterRow_fromslaverow([Values(PKMode.None, PKMode.Simple, PKMode.Complex)]PKMode pkMode)
    {
      DataTableRepeater sut = CreateTestObject(pkMode);

      DataRow masterRow = sut.GetMasterRow(sut.SlaveTable.Rows[1]);
      Assert.AreEqual(new object[] { 2, "BBB", 2 }, masterRow.ItemArray);
    }

    [Test]
    public void GetMasterRow_null([Values(PKMode.None, PKMode.Simple, PKMode.Complex)]PKMode pkMode)
    {
      DataTableRepeater sut = CreateTestObject(pkMode);
      Assert.IsNull(sut.GetMasterRow(null));
    }

    [Test]
    public void GetMasterRows([Values(PKMode.None, PKMode.Simple, PKMode.Complex)]PKMode pkMode)
    {
      DataTableRepeater sut = CreateTestObject(pkMode);

      DataRow[] slaveRows = new DataRow[] { sut.SlaveTable.Rows[0], sut.SlaveTable.Rows[2] };
      DataRow[] masterRows = sut.GetMasterRows(slaveRows);
      Assert.AreEqual(new object[] { 1, 3 }, DataTools.GetValuesFromColumn<int>(masterRows, "F3"));
    }

    #endregion
  }
}
