using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV;
using NUnit.Framework;
using System.Data;

namespace ExtTools.tests
{
  [TestFixture]
  class DataToolsTests_Other
  {
    [Test]
    public void CloneDataColumn()
    {
      DataColumn col1 = new DataColumn("AAA", typeof(string));

      DataColumn col2 = DataTools.CloneDataColumn(col1);

      PropertyAssert.ProperiesEqual(col1, col2);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public void TrimEnd_DataTable(int rowCount)
    {
      DataTable table = new DataTable();
      table.Columns.Add("Col1", typeof(string)); // с пробелами
      table.Columns.Add("Col2", typeof(string)); // без пробелов
      for (int i = 0; i < rowCount; i++)
        table.Rows.Add(i.ToString(), // Col1
          i.ToString() + new string(' ', i + 2)); // Col2

      DataTools.TrimEnd(table);

      Assert.AreEqual(rowCount, table.Rows.Count, "All rows present");
      for (int i = 0; i < rowCount; i++)
      {
        string v1 = table.Rows[i]["Col1"].ToString();
        string v2 = table.Rows[i]["Col2"].ToString();
        Assert.AreEqual(i.ToString(), v1, "Unchanged Col1");
        Assert.AreEqual(i.ToString(), v2, "Trimmed Col2");
      }
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(100)]
    public void SortDataRow(int rowCount)
    {
      int[] TestValues = new int[] { 5, 1, 10, 4}; // можно было бы использовать генератор случайных чисел
      DataTable table = new DataTable();
      table.Columns.Add("Col1", typeof(int));
      for (int i = 0; i < rowCount; i++)
      {
        int v = TestValues[i % TestValues.Length]; // перебираем по кругу
        table.Rows.Add(v);
      }
      DataRow[] a = DataTools.GetDataTableRows(table); // нужен массив

      DataTools.SortDataRows(a, "Col1");

      Assert.AreEqual(rowCount, a.Length, "Array length");
      int[] a2 = new int[rowCount];
      for (int i = 0; i < rowCount; i++)
        a2[i] = (int)(a[i]["Col1"]);

      CollectionAssert.IsOrdered(a2);
    }
  }
}
