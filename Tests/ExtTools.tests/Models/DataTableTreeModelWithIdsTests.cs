using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Models.Tree;
using System.Data;
using FreeLibSet.Core;

namespace ExtTools_tests.Models
{
  [TestFixture]
  public class DataTableTreeModelWithIdsTests
  {
    [TestCase("1", 1)]
    public void TreePathToId(string sPath, int wanted,
      [Values(true, false)] bool usePK, 
      [Values(TestDataType.Int32, TestDataType.Guid, TestDataType.String)]TestDataType dataType)
    {
      DataTable table = CreateTestTable(usePK, dataType);
    }

    private enum TestDataType { Int32, Guid, String }

    /// <summary>
    /// Создает тестовую таблицу-дерево с ключевым полем типа String. Корневым узлом является идеентификатор "1",
    /// из него выходят два дочерних узла "2" и "3".
    /// </summary>
    /// <returns></returns>
    private static DataTable CreateTestTable(bool usePK, TestDataType dataType)
    {
      Type dt;
      switch (dataType)
      {
        case TestDataType.Int32: dt = typeof(Int32); break;
        case TestDataType.Guid: dt = typeof(Guid); break;
        case TestDataType.String: dt = typeof(String); break;
        default: throw new ArgumentException("dataType");
      }

      DataTable table = new DataTable();
      DataColumn col = table.Columns.Add("F1", dt);
      col.AllowDBNull = false;
      col = table.Columns.Add("F2", dt);
      col.AllowDBNull = true;
      table.Columns.Add("F3", typeof(string)); // Дополнительное поле
      if (usePK)
        DataTools.SetPrimaryKey(table, "F1");

      table.Rows.Add(CreateTestValue(1, dataType), DBNull.Value, "AAA");
      table.Rows.Add(CreateTestValue(2, dataType), CreateTestValue(1, dataType), "BBB");
      table.Rows.Add(CreateTestValue(3, dataType), CreateTestValue(1, dataType), "CCC");
      return table;
    }

    private static object CreateTestValue(int v, TestDataType dataType)
    {
      switch (dataType)
      {
        case TestDataType.Int32: return v;
        case TestDataType.Guid:
          char ch = (char)('0' + (char)v);
          string s = new string(ch, 32);
          return new Guid(s);
        case TestDataType.String:
          return StdConvert.ToString(v);
        default:
          throw new ArgumentException("dataType");
      }
    }


  }
}
