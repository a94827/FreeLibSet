using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using NUnit.Framework;
using FreeLibSet.Core;

namespace ExtTools.tests
{
  class DataToolsTests_DataRow
  {

    private DataRow CreateRowWithRowState(DataTable table, DataRowState state)
    {
      DataRow row = table.Rows.Add("AAA");

      #region Подготовка состояния строки

      switch (state)
      {
        case DataRowState.Added:
          break;
        case DataRowState.Modified:
          table.AcceptChanges();
          row["Text"] = "BBB";
          break;
        case DataRowState.Deleted:
          table.AcceptChanges();
          row.Delete();
          break;
        case DataRowState.Unchanged:
          table.AcceptChanges();
          break;
        case DataRowState.Detached:
          table.AcceptChanges();
          table.Rows.Remove(row);
          break;
      }

      #endregion

      Assert.AreEqual(state, row.RowState, "Original state");

      return row;
    }

    [Test]
    public void SetRowState(
      [Values(
        DataRowState.Unchanged,
        DataRowState.Added,
        DataRowState.Modified,
        DataRowState.Deleted)] DataRowState oldState,
      [Values(
        DataRowState.Unchanged,
        DataRowState.Added,
        DataRowState.Modified,
        DataRowState.Deleted)] DataRowState newState)
    {
      DataTable table = new DataTable();
      table.Columns.Add("Text", typeof(string)); // непосредственно в тесте не используется
      DataRow row = CreateRowWithRowState(table, oldState);

      DataTools.SetRowState(row, newState);

      Assert.AreEqual(newState, row.RowState, "New state");
    }


    [TestCase(DataRowState.Unchanged, DataRowVersion.Current, "AAA")]
    [TestCase(DataRowState.Added, DataRowVersion.Current, "AAA")]
    [TestCase(DataRowState.Modified, DataRowVersion.Original, "AAA")]
    [TestCase(DataRowState.Modified, DataRowVersion.Current, "BBB")]
    [TestCase(DataRowState.Deleted, DataRowVersion.Original, "AAA")]
    public void GetRowValues(DataRowState state, DataRowVersion rowVersion, string result)
    {
      DataTable table = new DataTable();
      table.Columns.Add("Text", typeof(string)); // непосредственно в тесте не используется
      DataRow row = CreateRowWithRowState(table, state);

      object[] res1 = DataTools.GetRowValues(row, rowVersion);
      Assert.AreEqual(1, res1.Length);
      Assert.AreEqual(result, DataTools.GetString(res1[0]));

      IDictionary<string, object> res2 = DataTools.GetRowValueDictionary(row, rowVersion);
      Assert.AreEqual(1, res2.Count);
      Assert.AreEqual(result, DataTools.GetString(res2["Text"]));
    }
  }
}
