using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FreeLibSet.Core;

namespace ExtTools_tests.Core
{
  [TestFixture]
  class DataToolsTests_GroupRows
  {
    [Test]
    public void GroupRows_null_is_not_zero()
    {
      DataTable table = CreateGroupTestTable();
      table.DefaultView.Sort = "Text"; // A B M N Y Z

      DoTest(table.DefaultView, "Group", false, "{6},{5},{1,3},{4,2}");
    }

    [Test]
    public void GroupRows_null_is_zero_1()
    {
      DataTable table = CreateGroupTestTable();
      table.DefaultView.Sort = "Text"; // A B M N Y Z

      DoTest(table.DefaultView, "Group", true, "{5,6},{1,3},{4,2}");
    }

    [Test]
    public void GroupRows_null_is_zero_2()
    {
      DataTable table = CreateGroupTestTable();
      table.DefaultView.Sort = "Id"; // A Z B Y M N

      DoTest(table.DefaultView, "Group", true, "{5,6},{1,3},{2,4}");
    }

    [Test]
    public void GroupRows_null_is_zero_3()
    {
      DataTable table = CreateGroupTestTable();
      table.DefaultView.Sort = "Id DESC"; // N M Y B Z A

      DoTest(table.DefaultView, "Group", true, "{6,5},{3,1},{4,2}");
    }

    private DataTable CreateGroupTestTable()
    {
      DataTable table = new DataTable();
      table.Columns.Add("Id", typeof(int));
      table.Columns.Add("Group", typeof(int));
      table.Columns.Add("Text", typeof(string));
      table.Rows.Add(1, 1, "A");
      table.Rows.Add(2, 2, "Z");
      table.Rows.Add(3, 1, "B");
      table.Rows.Add(4, 2, "Y");
      table.Rows.Add(5, 0, "M");
      table.Rows.Add(6, DBNull.Value, "N");
      //DataTools.SetPrimaryKey(table, "Id");
      return table;
    }

    private void DoTest(DataView dv, string keyColumnNames, bool dbNullAsZero, string result)
    {
      DataRow[][] res1 = DataTools.GroupRows(dv, keyColumnNames, dbNullAsZero);
      Assert.AreEqual(result, GetIdString(res1), "for DataView");

      DataRow[][] res2 = DataTools.GroupRows(dv.ToTable(), keyColumnNames, dbNullAsZero);
      Assert.AreEqual(result, GetIdString(res2), "for DataTable");

      DataRow[][] res3 = DataTools.GroupRows(DataTools.GetDataViewRows(dv), keyColumnNames, dbNullAsZero);
      Assert.AreEqual(result, GetIdString(res3), "for DataRow[]");
    }

    private static string GetIdString(DataRow[][] rows)
    {
      if (rows == null)
        return "null";
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < rows.Length; i++)
      {
        if (i > 0)
          sb.Append(",");
        sb.Append("{");
        if (rows[i] == null)
          sb.Append("null");
        else
        {
          Int32[] ids = DataTools.GetIds(rows[i]);
          sb.Append(StdConvert.ToString(ids));
        }
        sb.Append("}");
      }
      return sb.ToString();
    }
  }
}
