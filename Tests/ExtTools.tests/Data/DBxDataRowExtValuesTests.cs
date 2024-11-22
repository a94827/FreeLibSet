using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class DBxDataRowExtValuesTests
  {
    #region Тестирование конструктора

    [Test]
    public void Constructor()
    {
      DataTable table = CreateTestTable();
      DBxDataRowExtValues sut = new DBxDataRowExtValues(table.Rows[0]);

      Assert.AreSame(table.Rows[0], sut.Row, "Row");
      Assert.AreEqual(table.Columns.Count, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region Item

    [Test]
    public void Item()
    {
      DBxDataRowExtValues sut = new DBxDataRowExtValues(CreateTestTable().Rows[0]);
      DBxExtValue item1 = sut["F1"];
      Assert.AreEqual("F1", item1.Name, "#1");

      DBxExtValue item2 = sut[1];
      Assert.AreEqual("F2", item2.Name, "#2");

      DBxExtValue dummy;
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[-1]; }, "#3");
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[sut.Row.Table.Columns.Count]; }, "#4");
      Assert.Catch<ArgumentException>(delegate() { dummy = sut["XXX"]; }, "#5");
      Assert.Catch<ArgumentException>(delegate() { dummy = sut[""]; }, "#6");
    }

    [Test]
    public void GetName()
    {
      DBxDataRowExtValues sut = new DBxDataRowExtValues(CreateTestTable().Rows[0]);
      Assert.AreEqual("F2", sut.GetName(1));
    }


    [Test]
    public void GetDisplayName()
    {
      DataTable table = CreateTestTable();
      table.Columns[1].Caption = "XXX";
      DBxDataRowExtValues sut = new DBxDataRowExtValues(table.Rows[0]);
      Assert.AreEqual("XXX", sut.GetDisplayName(1));
    }


    [Test]
    public void IndexOf()
    {
      DBxDataRowExtValues sut = new DBxDataRowExtValues(CreateTestTable().Rows[0]);
      Assert.AreEqual(1, sut.IndexOf("F2"), "#1");
      Assert.AreEqual(-1, sut.IndexOf("XXX"), "#2");
      Assert.AreEqual(-1, sut.IndexOf(""), "#3");
    }

    [Test]
    public void IsReadOnly()
    {
      DBxDataRowExtValues sut = new DBxDataRowExtValues(CreateTestTable().Rows[0]);
      Assert.IsFalse(sut.IsReadOnly, "#1");
      sut.SetValue(1, 123);

      sut.IsReadOnly = true;
      Assert.IsTrue(sut.IsReadOnly, "#2");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.SetValue(1, 123); }, "SetValue()");

      sut.IsReadOnly = false;
      Assert.IsFalse(sut.IsReadOnly, "#3");
      Assert.DoesNotThrow(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly() #3");
    }

    #endregion

    #region Get/SetValue()

    [Test]
    public void GetValue()
    {
      DBxDataRowExtValues sut = new DBxDataRowExtValues(CreateTestTable().Rows[0]);
      Assert.AreEqual("ABC", sut.GetValue(0, DBxExtValuePreferredType.Unknown), "#1");
      Assert.AreEqual(1, sut.GetValue(1, DBxExtValuePreferredType.Unknown), "#2");

      sut.SetValue(0, "DEF");
      Assert.AreEqual("DEF", sut.GetValue(0, DBxExtValuePreferredType.Unknown), "#1");
    }

    [Test]
    public void SetValue()
    {
      DBxDataRowExtValues sut = new DBxDataRowExtValues(CreateTestTable().Rows[0]);

      sut.SetValue(0, "XXX");
      Assert.AreEqual("XXX", sut.GetValue(0, DBxExtValuePreferredType.Unknown), "#1");

      sut.SetValue(0, null);
      Assert.AreEqual(DBNull.Value, sut.GetValue(0, DBxExtValuePreferredType.Unknown), "#2");
    }

    #endregion

    #region IsNull()

    [Test]
    public void IsNull()
    {
      DataTable table = CreateTestTable();
      DBxDataRowExtValues sut = new DBxDataRowExtValues(table.Rows[0]);
      Assert.IsFalse(sut.IsNull(0), "#1");
      sut.SetValue(0, null);
      Assert.IsTrue(sut.IsNull(0), "#2");
    }

    #endregion

    #region GetGrayed()

    [Test]
    public void GetGrayed()
    {
      DataTable table = CreateTestTable();
      DBxDataRowExtValues sut = new DBxDataRowExtValues(table.Rows[0]);
      Assert.IsFalse(((IDBxExtValues)sut).GetGrayed(0));
    }

    #endregion

    #region AllowDBNull(), MaxLength(), GetValueReadOnly()

    [Test]
    public void AllowDBNull()
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string)).AllowDBNull = false;
      table.Columns.Add("F2", typeof(int)).AllowDBNull = true;
      table.Rows.Add("ABC", 123);

      DBxDataRowExtValues sut = new DBxDataRowExtValues(table.Rows[0]);
      Assert.IsFalse(sut.AllowDBNull(0), "F1");
      Assert.IsTrue(sut.AllowDBNull(1), "F2");
    }

    [Test]
    public void MaxLength()
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string));
      table.Columns.Add("F2", typeof(string)).MaxLength = 100;
      table.Rows.Add();

      DBxDataRowExtValues sut = new DBxDataRowExtValues(table.Rows[0]);
      Assert.AreEqual(-1, sut.MaxLength(0), "F1");
      Assert.AreEqual(100, sut.MaxLength(1), "F2");
    }


    [Test]
    public void GetValueReadOnly()
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(int));
      table.Columns.Add("F2", typeof(int)).ReadOnly = true;
      table.Columns.Add("F3", typeof(int)).Expression = "F1*2";
      table.Rows.Add();

      DBxDataRowExtValues sut = new DBxDataRowExtValues(table.Rows[0]);
      Assert.IsFalse(sut.GetValueReadOnly(0), "F1");
      Assert.IsTrue(sut.GetValueReadOnly(1), "F2");
      Assert.IsTrue(sut.GetValueReadOnly(2), "F3");
    }

    #endregion

    #region GetEnumerator()

    [Test]
    public void GetEnumerator()
    {
      DataTable table = CreateTestTable();
      DBxDataRowExtValues sut = new DBxDataRowExtValues(table.Rows[0]);

      List<string> lst = new List<string>();
      foreach (DBxExtValue item in sut)
        lst.Add(item.Name);

      Assert.AreEqual(new string[] { "F1", "F2" }, lst.ToArray());
    }

    #endregion

    #region Создание тестовой таблицы

    /// <summary>
    /// Создает тестовую таблицу. 
    /// </summary>
    /// <returns></returns>
    private static DataTable CreateTestTable()
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string));
      table.Columns.Add("F2", typeof(int));

      table.Rows.Add("ABC", 1);

      return table;
    }

    #endregion
  }
}
