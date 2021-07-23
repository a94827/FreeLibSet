﻿using AgeyevAV;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ExtTools.tests
{
  [TestFixture]
  class DataToolsTests_GroupRows
  {
    [Test]
    public void GroupRows_null_is_not_zero()
    {
      DataTable table = CreateGroupTestTable();
      table.DefaultView.Sort = "Text"; // A B M N Y Z

      DataRow[][] res = DataTools.GroupRows(table.DefaultView, "Group", false);

      Assert.AreEqual("{6},{5},{1,3},{4,2}", GetIdString(res));
    }

    [Test]
    public void GroupRows_null_is_zero_1()
    {
      DataTable table = CreateGroupTestTable();
      table.DefaultView.Sort = "Text"; // A B M N Y Z

      DataRow[][] res = DataTools.GroupRows(table.DefaultView, "Group", true);
      Assert.AreEqual("{5,6},{1,3},{4,2}", GetIdString(res));
    }

    [Test]
    public void GroupRows_null_is_zero_2()
    {
      DataTable table = CreateGroupTestTable();
      table.DefaultView.Sort = "Id"; // A Z B Y M N

      DataRow[][] res = DataTools.GroupRows(table.DefaultView, "Group", true);
      Assert.AreEqual("{5,6},{1,3},{2,4}", GetIdString(res));
    }

    [Test]
    public void GroupRows_null_is_zero_3()
    {
      DataTable table = CreateGroupTestTable();
      table.DefaultView.Sort = "Id DESC"; // N M Y B Z A

      DataRow[][] res = DataTools.GroupRows(table.DefaultView, "Group", true);
      Assert.AreEqual("{6,5},{3,1},{4,2}", GetIdString(res));
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
          sb.Append(DataTools.CommaStringFromIds(ids, false));
        }
        sb.Append("}");
      }
      return sb.ToString();
    }
  }
}
