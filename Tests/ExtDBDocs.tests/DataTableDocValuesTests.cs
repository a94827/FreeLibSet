using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data.Docs;
using System.Data;
using FreeLibSet.Core;

namespace ExtDBDocs_tests.Data_Docs
{
  [TestFixture]
  public class DataTableDocValuesTests
  {
    #region Тестирование конструктора

    [Test]
    public void Constructor()
    {
      DataTable table = CreateTestTable();
      DataTableDocValues sut = new DataTableDocValues(table);

      Assert.AreSame(table, sut.Table, "Table");
      Assert.AreEqual(table.Columns.Count, sut.Count, "Count");
      Assert.AreEqual(table.Rows.Count, sut.RowCount, "RowCount");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
    }

    #endregion

    #region ResetBuffer()

    [Test]
    public void ResetBuffer_all()
    {
      DataTableDocValues sut = CreateTestValuesForResetBuffer();
      sut.ResetBuffer();
      Assert.IsNull(sut.GetValue(2, DBxDocValuePreferredType.Unknown));
    }

    [Test]
    public void ResetBuffer_index()
    {
      DataTableDocValues sut = CreateTestValuesForResetBuffer();
      sut.ResetBuffer(2);
      Assert.IsNull(sut.GetValue(2, DBxDocValuePreferredType.Unknown));
    }

    [Test]
    public void ResetBuffer_name()
    {
      DataTableDocValues sut = CreateTestValuesForResetBuffer();
      sut.ResetBuffer("F3");
      Assert.IsNull(sut.GetValue(2, DBxDocValuePreferredType.Unknown));
    }

    // Не тестируем, что будет без вызова ResetBuffer() или вызове для другого поля.
    // Возможно, при изменении реализации метод ResetBuffer() станет фиктивным, и изменения в таблице
    // будут отслеживаться автоматически.

    private static DataTableDocValues CreateTestValuesForResetBuffer()
    {
      DataTable table = CreateTestTable();
      DataTableDocValues sut = new DataTableDocValues(table);
      object v1 = sut.GetValue(2, DBxDocValuePreferredType.Unknown);
      Assert.AreEqual(1, v1, "original");
      table.Rows.Add("", 0, 2);
      return sut;
    }

    #endregion

    #region Item

    [Test]
    public void Item()
    {
      DataTableDocValues sut = new DataTableDocValues(CreateTestTable());
      DBxDocValue item1 = sut["F2"];
      Assert.AreEqual("F2", item1.Name, "#1");

      DBxDocValue item2 = sut[2];
      Assert.AreEqual("F3", item2.Name, "#2");

      DBxDocValue dummy;
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[-1]; }, "#3");
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[sut.Table.Columns.Count]; }, "#4");
      Assert.Catch<ArgumentException>(delegate() { dummy = sut["XXX"]; }, "#5");
      Assert.Catch<ArgumentException>(delegate() { dummy = sut[""]; }, "#6");
    }

    [Test]
    public void GetName()
    {
      DataTableDocValues sut = new DataTableDocValues(CreateTestTable());
      Assert.AreEqual("F2", sut.GetName(1));
    }


    [Test]
    public void GetDisplayName()
    {
      DataTable table = CreateTestTable();
      table.Columns[1].Caption = "XXX";
      DataTableDocValues sut = new DataTableDocValues(table);
      Assert.AreEqual("XXX", sut.GetDisplayName(1));
    }


    [Test]
    public void IndexOf()
    {
      DataTableDocValues sut = new DataTableDocValues(CreateTestTable());
      Assert.AreEqual(1, sut.IndexOf("F2"), "#1");
      Assert.AreEqual(-1, sut.IndexOf("XXX"), "#2");
      Assert.AreEqual(-1, sut.IndexOf(""), "#3");
    }

    [Test]
    public void IsReadOnly()
    {
      DataTableDocValues sut = new DataTableDocValues(CreateTestTable());
      Assert.IsFalse(sut.IsReadOnly, "#1");
      sut.SetValue(1, 123);
      object[] a = sut.GetValueArray(1);
      sut.SetValueArray(1, a);
      sut.SetRowValue(0, 0, "XXX");

      sut.IsReadOnly = true;
      Assert.IsTrue(sut.IsReadOnly, "#2");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.SetValue(1, 123); }, "SetValue()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.SetValueArray(1, a); }, "SetValueArray()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.SetRowValue(0, 0, "XXX"); }, "SetRowValue()");

      sut.IsReadOnly = false;
      Assert.IsFalse(sut.IsReadOnly, "#3");
      Assert.DoesNotThrow(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly() #3");
    }

    #endregion

    #region Get/SetValue()

    [Test]
    public void GetValue()
    {
      DataTableDocValues sut = new DataTableDocValues(CreateTestTable());
      Assert.IsNull(sut.GetValue(0, DBxDocValuePreferredType.Unknown), "#1");
      Assert.IsNull(sut.GetValue(1, DBxDocValuePreferredType.Unknown), "#2");
      Assert.AreEqual(1, sut.GetValue(2, DBxDocValuePreferredType.Unknown), "#3");

      sut.SetRowValue(1, 0, 2);
      sut.SetRowValue(1, 2, 2);

      // проверка
      for (int i = 0; i < sut.RowCount; i++)
        Assert.AreEqual(2, sut.GetRowValue(1, i), "GetRowValue(" + i.ToString() + ")");
      Assert.AreEqual(2, sut.GetValue(1, DBxDocValuePreferredType.Unknown), "#4");

      sut.Table.Rows.Clear();
      sut.Table.AcceptChanges();
      sut.ResetBuffer();
      Assert.AreEqual(0, sut.RowCount, "RowCount #5");
      Assert.AreEqual(DBNull.Value, sut.GetValue(1, DBxDocValuePreferredType.Unknown), "#5");
    }

    [Test]
    public void SetValue()
    {
      DataTableDocValues sut = new DataTableDocValues(CreateTestTable());
      Assert.IsNull(sut.GetValue(0, DBxDocValuePreferredType.Unknown), "#1");

      sut.SetValue(0, "XXX");
      Assert.AreEqual("XXX", sut.GetValue(0, DBxDocValuePreferredType.Unknown), "#2");

      sut.SetValue(0, null);
      Assert.AreEqual(DBNull.Value, sut.GetValue(0, DBxDocValuePreferredType.Unknown), "#3");
    }

    #endregion

    #region IsNull()

    [Test]
    public void IsNull()
    {
      DataTable table = CreateTestTable();
      DataTableDocValues sut = new DataTableDocValues(table);

      Assert.IsTrue(sut.IsNull(0), "#1 (grayed)");
      Assert.IsFalse(sut.IsNull(2), "#2 (not grayed)");
      foreach (DataRow row in table.Rows)
        row["F1"] = DBNull.Value;
      sut.ResetBuffer();

      Assert.IsTrue(sut.IsNull(0), "#3 (DBNull everywhere)");
    }

    #endregion

    #region GetGrayed()

    [Test]
    public void GetGrayed()
    {
      DataTable table = CreateTestTable();
      DataTableDocValues sut = new DataTableDocValues(table);
      Assert.IsTrue(sut.GetGrayed(0), "#1 F1");
      Assert.IsTrue(sut.GetGrayed(1), "#1 F2");
      Assert.IsFalse(sut.GetGrayed(2), "#1 F3");

      sut.SetRowValue(0, 0, "GHI");
      sut.SetRowValue(0, 1, "GHI");
      Assert.IsFalse(sut.GetGrayed(0), "#2 F1");

      sut.SetValue(1, 123);
      Assert.IsFalse(sut.GetGrayed(1), "#3 F2");

      sut.SetValue(1, null);
      Assert.IsFalse(sut.GetGrayed(1), "#4 F2");

      sut.SetRowValue(1, 0, 123);
      Assert.IsTrue(sut.GetGrayed(1), "#5 F2");

      table.Rows.Clear();
      table.AcceptChanges();
      sut.ResetBuffer();
      Assert.IsFalse(sut.GetGrayed(1), "#6 F2");
    }

    #endregion

    #region Get/SetValueArray()

    [Test]
    public void GetValueArray()
    {
      DataTable table = CreateTestTable();
      DataTableDocValues sut = new DataTableDocValues(table);
      Assert.AreEqual(new object[] { "ABC", "DEF", "GHI" }, sut.GetValueArray(0), "F1");
      Assert.AreEqual(new object[] { 1, 2, 3 }, sut.GetValueArray(1), "F2");
      Assert.AreEqual(new object[] { 1, 1, 1 }, sut.GetValueArray(2), "F3");

      sut.SetRowValue(2, 0, DBNull.Value);
      Assert.AreEqual(new object[] { DBNull.Value, 1, 1 }, sut.GetValueArray(2), "#2 F3");

      table.Rows.Clear();
      table.AcceptChanges();
      sut.ResetBuffer();
      Assert.AreEqual(new object[0], sut.GetValueArray(2), "#3 F3");
    }


    [Test]
    public void SetValueArray()
    {
      DataTable table = CreateTestTable();
      DataTableDocValues sut = new DataTableDocValues(table);

      sut.SetValueArray(1, new object[] { 111, 222, DBNull.Value });
      Assert.AreEqual(111, sut.GetRowValue(1, 0), "#1 F1");
      Assert.AreEqual(222, sut.GetRowValue(1, 1), "#1 F2");
      Assert.AreEqual(DBNull.Value, sut.GetRowValue(1, 2), "#1 F3");

      Assert.Catch<ArgumentException>(delegate() { sut.SetValueArray(1, new object[] { 111, 222 }); }, "small array");
      Assert.Catch<ArgumentException>(delegate() { sut.SetValueArray(1, new object[] { 111, 222, 333, 444 }); }, "large array");
    }

    #endregion

    #region Get/SetRowValue()


    [Test]
    public void GetRowValue()
    {
      DataTable table = CreateTestTable();
      DataTableDocValues sut = new DataTableDocValues(table);

      Assert.AreEqual(1, sut.GetRowValue(1, 0), "#1 F1");
      Assert.AreEqual(2, sut.GetRowValue(1, 1), "#1 F2");
      Assert.AreEqual(3, sut.GetRowValue(1, 2), "#1 F3");

      sut.SetValue(1, 111);
      Assert.AreEqual(111, sut.GetRowValue(1, 0), "#2 F1");
      Assert.AreEqual(111, sut.GetRowValue(1, 1), "#2 F2");
      Assert.AreEqual(111, sut.GetRowValue(1, 2), "#2 F3");

      Assert.Catch<ArgumentException>(delegate() { sut.GetRowValue(1, -1); }, "negative row index");
      Assert.Catch<ArgumentException>(delegate() { sut.GetRowValue(1, 3); }, "row index row out range");
    }


    [Test]
    public void SetRowValue()
    {
      DataTable table = CreateTestTable();
      DataTableDocValues sut = new DataTableDocValues(table);
      sut.SetRowValue(1, 0, 111);

      Assert.AreEqual(111, sut.GetRowValue(1,0), "#1");
      Assert.Catch<ArgumentException>(delegate() { sut.SetRowValue(1, -1, 222); }, "negative row index");
      Assert.Catch<ArgumentException>(delegate() { sut.SetRowValue(1, 3, 222); }, "row index row out range");
    }

    #endregion

    #region AllowDBNull(), MaxLength(), GetValueReadOnly()

    [Test]
    public void AllowDBNull()
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string)).AllowDBNull = false;
      table.Columns.Add("F2", typeof(int)).AllowDBNull = true;

      DataTableDocValues sut = new DataTableDocValues(table);
      Assert.IsFalse(sut.AllowDBNull(0), "F1");
      Assert.IsTrue(sut.AllowDBNull(1), "F2");
    }

    [Test]
    public void MaxLength()
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string));
      table.Columns.Add("F2", typeof(string)).MaxLength = 100;

      DataTableDocValues sut = new DataTableDocValues(table);
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

      DataTableDocValues sut = new DataTableDocValues(table);
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
      DataTableDocValues sut = new DataTableDocValues(table);

      List<string> lst = new List<string>();
      foreach (DBxDocValue item in sut)
        lst.Add(item.Name);

      Assert.AreEqual(new string[] { "F1", "F2", "F3" }, lst.ToArray());
    }

    #endregion

    #region Создание тестовой таблицы

    /// <summary>
    /// Создает тестовую таблицу. 
    /// Столбцы F1 и F2 имеют разные значения, F3-одинаковые
    /// </summary>
    /// <returns></returns>
    private static DataTable CreateTestTable()
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string));
      table.Columns.Add("F2", typeof(int));
      table.Columns.Add("F3", typeof(int));

      table.Rows.Add("ABC", 1, 1);
      table.Rows.Add("DEF", 2, 1);
      table.Rows.Add("GHI", 3, 1);

      return table;
    }

    #endregion
  }
}
