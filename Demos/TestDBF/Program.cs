using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.DBF;
using FreeLibSet.IO;
using FreeLibSet.Core;

namespace DBFTest
{
  class Program
  {
    static void Main(string[] args)
    {
      DataTable table = CreateTestTable();

      AbsPath testDir = new AbsPath(FileTools.ApplicationBaseDir, "Test");
      FileTools.ForceDirsAndClear(testDir);

      CreateFile(testDir, DbfFileFormat.dBase2, false, table);
      CreateFile(testDir, DbfFileFormat.dBase3, false, table);
      CreateFile(testDir, DbfFileFormat.dBase3, true, table);
      CreateFile(testDir, DbfFileFormat.dBase4, false, table);
      CreateFile(testDir, DbfFileFormat.dBase4, true, table);

      LoadFile(testDir, DbfFileFormat.dBase2, false, table);
      LoadFile(testDir, DbfFileFormat.dBase3, false, table);
      LoadFile(testDir, DbfFileFormat.dBase3, true, table);
      LoadFile(testDir, DbfFileFormat.dBase4, false, table);
      LoadFile(testDir, DbfFileFormat.dBase4, true, table);

      Console.WriteLine("Finished, press Enter");
      Console.ReadLine();
    }

    private static DataTable CreateTestTable()
    {
      DataTable table = new DataTable();
      table.Columns.Add("F1", typeof(string));
      table.Columns.Add("F21", typeof(Int32));
      table.Columns.Add("F22", typeof(Int64));
      table.Columns.Add("F23", typeof(Decimal));
      table.Columns.Add("F3", typeof(bool));
      table.Columns.Add("F4", typeof(DateTime));
      table.Columns.Add("F5", typeof(string));
      table.Columns.Add("F6", typeof(Decimal));

      for (int i = 1; i <= 3000; i++)
      {
        DataRow row = table.NewRow();
        row["F1"] = "Значение " + i.ToString();
        row["F21"] = i % 1000;
        row["F22"] = i * 1000;
        row["F23"] = (decimal)i + (i % 100) / 100m;
        row["F3"] = (i % 2) == 0;
        row["F4"] = new DateTime(2023, 3, 22).AddDays(i);
        char ch = (char)((int)'A' + (i - 1) % 26);
        row["F5"] = new string(ch, ((i % 10) + 1) * 100);
        row["F6"] = row["F23"];
        table.Rows.Add(row);

        if ((i % 100) == 0)
          table.Rows.Add(); // пустая строка
      }
      return table;
    }

    private static void CreateFile(AbsPath testDir, DbfFileFormat format, bool useMemo, DataTable table)
    {
      AbsPath dbfPath = new AbsPath(testDir, GetDbfFileName(format, useMemo));
      Console.WriteLine("Creating " + dbfPath.Path + " ...");

      DbfStruct dbs = CreateDbfStruct(format, useMemo);

      using (DbfFile file = new DbfFile(dbfPath, dbs, Encoding.GetEncoding(1251), format))
      {
        file.Append(table);
      }
    }

    private static void LoadFile(AbsPath testDir, DbfFileFormat format, bool useMemo, DataTable table)
    {
      AbsPath dbfPath = new AbsPath(testDir, GetDbfFileName(format, useMemo));
      Console.WriteLine("Loading " + dbfPath.Path + " ...");

      DbfStruct dbs = CreateDbfStruct(format, useMemo);

      using (DbfFile file = new DbfFile(dbfPath, Encoding.GetEncoding(1251), true))
      {
        if (!CompareDbfStruct(dbs, file.DBStruct))
          return;
        if (file.RecordCount != table.Rows.Count)
        {
          Console.WriteLine("Wrong RecordCount: " + file.RecordCount);
          return;
        }
        for (int i = 0; i < table.Rows.Count; i++)
        {
          file.RecNo = i + 1;
          for (int j = 0; j < dbs.Count; j++)
          {
            object v1 = file.GetValue(j);
            object v2 = table.Rows[i][dbs[j].Name];
            if (!DataTools.AreValuesEqual(v1, v2))
            {
              Console.WriteLine("RecNo=" + file.RecNo.ToString() + ", Field=" + dbs[j].Name + ", vanted value=" + v2.ToString() + ", real value=" + v1.ToString());
              return;
            }
          }
        }
      }
    }

    private static DbfStruct CreateDbfStruct(DbfFileFormat format, bool useMemo)
    {
      DbfStruct dbs = new DbfStruct();
      dbs.AddString("F1", 100);
      dbs.AddNum("F21", 3);
      dbs.AddNum("F22", 10);
      dbs.AddNum("F23", 12, 2);
      dbs.AddBool("F3");
      if (format != DbfFileFormat.dBase2)
        dbs.AddDate("F4");
      if (useMemo)
        dbs.AddMemo("F5");
      if (format == DbfFileFormat.dBase4)
        dbs.Add(new DbfFieldInfo("F6", 'F', 12, 2));
      return dbs;
    }

    private static string GetDbfFileName(DbfFileFormat format, bool useMemo)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(format.ToString());
      if (useMemo)
        sb.Append("m");
      sb.Append(".dbf");
      return sb.ToString();
    }


    private static bool CompareDbfStruct(DbfStruct dbs1, DbfStruct dbs2)
    {
      if (!DataTools.AreArraysEqual<string>(dbs1.GetNames(), dbs2.GetNames()))
      {
        Console.WriteLine("Field names are different");
        return false;
      }

      for (int i = 0; i < dbs1.Count; i++)
      {
        if (dbs1[i].Type != dbs2[i].Type)
        {
          Console.WriteLine("Field " + dbs1[i].Name + " type different: wanted=" + dbs1[i].Type+ ", real=" + dbs2[i].Type);
          return false;
        }
        if (dbs1[i].Length != dbs2[i].Length)
        {
          Console.WriteLine("Field " + dbs1[i].Name + " length different: wanted=" + dbs1[i].Length.ToString() + ", real=" + dbs2[i].Length.ToString());
          return false;
        }
        if (dbs1[i].Precision != dbs2[i].Precision)
        {
          Console.WriteLine("Field " + dbs1[i].Name + " precision different: wanted=" + dbs1[i].Precision.ToString() + ", real=" + dbs2[i].Precision.ToString());
          return false;
        }
      }
      return true;
    }
  }
}
