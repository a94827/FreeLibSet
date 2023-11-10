using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using System.Reflection;
using System.Data.SQLite;
using FreeLibSet.Data.SQLite;
using System.Data.SqlClient;
using Npgsql;
using FreeLibSet.Data.Npgsql;
using FreeLibSet.Data.SqlClient;
using System.Data.Common;

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DBxManagerTests
  {
    #region Тесты в списке Managers

    /// <summary>
    /// Список строк-констант, заданных в DBxProviderNames
    /// </summary>
    public string[] ProviderNames
    {
      get
      {
        List<string> lst = new List<string>();
        Type t = typeof(DBxProviderNames);
        MemberInfo[] mis = t.GetMembers(BindingFlags.Static | BindingFlags.Public);
        foreach (MemberInfo mi in mis)
        {
          FieldInfo fi = mi as FieldInfo;
          if (fi == null)
            continue;
          if (fi.FieldType == typeof(String))
          {
            string s = fi.GetValue(null).ToString();
            lst.Add(s);
          }
        }
        return lst.ToArray();
      }
    }

    [Test]
    public void Managers()
    {
      Assert.AreEqual(ProviderNames.Length, DBxManager.Managers.Count, "Count");
    }

    [TestCaseSource("ProviderNames")]
    public void Test(string providerName)
    {
      DBxManager sut = DBxManager.Managers[providerName];
      Assert.IsNotNull(sut, providerName + "-DBxManager");
      Assert.AreEqual(providerName, sut.ProviderName, providerName + "-ProviderName");
      Assert.IsNotNull(sut.ProviderFactory, providerName + "-ProviderFactory");
      Assert.IsNotNull(sut.CreateConnectionStringBuilder(""), providerName + "-CreateConnectionStringBuilder()");
    }

    #endregion

    #region Защищенные методы ReplaceDBItem() и ReplaceFileItem()

    private class TestManager : DBxManager
    {
      #region Конструктор

      public TestManager() : base("Test") { }

      #endregion

      #region Вызов защищенных методов

      public string TestReplaceDBItem(string oldItem, string oldName, string newName)
      {
        return DBxManager.ReplaceDBItem(oldItem, oldName, newName);
      }

      public string TestReplaceFileItem(string oldItem, string oldName, string newName)
      {
        return DBxManager.ReplaceFileItem(oldItem, oldName, newName);
      }

      #endregion

      #region Заглушки

      public override DbProviderFactory ProviderFactory
      {
        get
        {
          throw new NotImplementedException();
        }
      }

      public override DbConnectionStringBuilder CreateConnectionStringBuilder(string connectionString)
      {
        throw new NotImplementedException();
      }

      public override DBx CreateDBObject(string connectionString)
      {
        throw new NotImplementedException();
      }

      public override string ReplaceDBName(string connectionString, string oldDBName, string newDBName)
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    [TestCase("db", "db", "xxx", "xxx")]
    [TestCase("adbb", "db", "x", "axb")]
    [TestCase("adbb", "db", "xxx", "axxxb")]
    [TestCase("adb", "db", "xxx", "axxx")]
    [TestCase("dbb", "db", "xxx", "xxxb")]
    [TestCase("adbb", "DB", "xxx", "axxxb", Description ="Ignore case")]
    [TestCase("adbdbb", "db", "xxx", "adbxxxb", Description ="Replace at last found position")]
    [TestCase("db", "", "xxx", "xxx", Description = "oldName missing")]
    [TestCase("adbb", "db", "db2", "adb2b", Description ="no recurse")]
    [TestCase("", "db", "xxx", "", Description ="empty oldItem means that there is no DB name; returns empty string")]
    public void ReplaceDBItem(string oldItem, string oldName, string newName, string wantedRes)
    {
      TestManager tester = new TestManager();
      string res = tester.TestReplaceDBItem(oldItem, oldName, newName);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("adbc", "db", "", Description = "newName is empty")]
    [TestCase("adbc", "xxx", "yyy", Description = "no fragment found")]
    public void ReplaceDBItem_exceptions(string oldItem, string oldName, string newName)
    {
      TestManager tester = new TestManager();
      Assert.Catch<ArgumentException>(delegate () { tester.TestReplaceDBItem(oldItem, oldName, newName); });
    }

    [Platform("Win")]
    [TestCase(@"C:\TEMP\db.mdf", "db", "xxx", @"C:\TEMP\xxx.mdf")]
    [TestCase(@"C:\TEMP\db.db", "db", "xxx", @"C:\TEMP\xxx.db", Description ="replace filename, not the extension")]
    [TestCase(@"C:\TEMP\ddb.db", "db", "xxx", @"C:\TEMP\dxxx.db")]
    [TestCase(@"C:\TEMP\dbb.db", "db", "xxx", @"C:\TEMP\xxxb.db")]
    [TestCase(@"C:\TEMP\ddbb.db", "db", "xxx", @"C:\TEMP\dxxxb.db")]
    [TestCase(@"C:\TEMP\ddbb.db", "db", "x", @"C:\TEMP\dxb.db")]
    [TestCase(@"C:\db\db.db", "db", "xxx", @"C:\db\xxx.db", Description = "replace filename, not the path")]
    [TestCase(@"C:\TEMP\ddbb.db", "DB", "xxx", @"C:\TEMP\dxxxb.db", Description ="Ignore case")]
    [TestCase("", "db", "xxx", "", Description ="oldItem is empty; returns empty string")]
    [TestCase(@"C:\TEMP\db.db", "", "xxx", @"C:\TEMP\xxx.db", Description = "file name missing")]
    public void ReplaceFileItem_Windows(string oldItem, string oldName, string newName, string wantedRes)
    {
      TestManager tester = new TestManager();
      string res = tester.TestReplaceFileItem(oldItem, oldName, newName);
      Assert.AreEqual(wantedRes, res);
    }

    [Platform("Linux")]
    [TestCase(@"~/test/db.mdf", "db", "xxx", @"~/test/xxx.mdf")]
    [TestCase(@"~/test/db.db", "db", "xxx", @"~/test/xxx.db", Description ="replace filename, not the extension")]
    [TestCase(@"~/test/ddb.db", "db", "xxx", @"~/test/dxxx.db")]
    [TestCase(@"~/test/dbb.db", "db", "xxx", @"~/test/xxxb.db")]
    [TestCase(@"~/test/ddbb.db", "db", "xxx", @"~/test/dxxxb.db")]
    [TestCase(@"~/test/ddbb.db", "db", "x", @"~/test/dxb.db")]
    [TestCase(@"~/db/db.db", "db", "xxx", @"~/db/xxx.db", Description = "replace filename, not the path")]
    [TestCase(@"~/test/ddbb.db", "DB", "xxx", @"~/test/dxxxb.db", Description ="Ignore case")]
    [TestCase("", "db", "xxx", "", Description ="oldItem is empty; returns empty string")]
    [TestCase(@"~/test/db.db", "", "xxx", @"~/test/xxx.db", Description = "file name missing")]
    public void ReplaceFileItem_Linux(string oldItem, string oldName, string newName, string wantedRes)
    {
      TestManager tester = new TestManager();
      string res = tester.TestReplaceFileItem(oldItem, oldName, newName);
      Assert.AreEqual(wantedRes, res);
    }

    [Platform("Win")]
    [TestCase(@"C:\TEMP\db.mdf", "db", "", Description ="newName is empty")]
    [TestCase(@"C:\TEMP\db.mdf", "TEMP", "xxx", Description = "no fragment found")]
    [TestCase(@"C:\TEMP\db.mdf", "mdf", "xxx", Description = "no fragment found")]
    [TestCase(@"C:\TEMP\db>.mdf", "db", "xxx", Description = "invalid char in oldItem")]
    public void ReplaceFileItem_exceptions_Windows(string oldItem, string oldName, string newName)
    {
      TestManager tester = new TestManager();
      Assert.Catch<ArgumentException>(delegate () { tester.TestReplaceFileItem(oldItem, oldName, newName); });
    }

    [Platform("Linux")]
    [TestCase(@"~/test/db.mdf", "db", "", Description ="newName is empty")]
    [TestCase(@"~/test/db.mdf", "TEMP", "xxx", Description = "no fragment found")]
    [TestCase(@"~/test/db.mdf", "mdf", "xxx", Description = "no fragment found")]
    //[TestCase(@"~/test/db>.mdf", "db", "xxx", Description = "invalid char in oldItem")]
    public void ReplaceFileItem_exceptions_Linux(string oldItem, string oldName, string newName)
    {
      TestManager tester = new TestManager();
      Assert.Catch<ArgumentException>(delegate () { tester.TestReplaceFileItem(oldItem, oldName, newName); });
    }

    #endregion
  }
}

namespace ExtDB_tests.Data_SQLite
{
  [TestFixture]
  public class DBxManagerTests_SQLite
  {
    private static string ConnectionString
    {
      get
      {
        SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder();
        csb.DataSource = @"c:\temp\test_db.db";
        csb.ForeignKeys = true;
        return csb.ToString();
      }
    }

    [Test]
    public void CreateDBObject()
    {
      DBxManager sut = DBxManager.Managers[DBxProviderNames.SQLite];
      DBx res = sut.CreateDBObject(ConnectionString);
      Assert.IsInstanceOf<SQLiteDBx>(res);
    }

    [Test]
    public void ReplaceDBName()
    {
      DBxManager sut = DBxManager.Managers[DBxProviderNames.SQLite];
      string res = sut.ReplaceDBName(ConnectionString, "db", "db2");
      SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder(res);
      Assert.AreEqual(@"c:\temp\test_db2.db", csb.DataSource, "DataSource");
      Assert.IsTrue(csb.ForeignKeys, "ForeignKeys");
    }
  }
}

namespace ExtDB_tests.Data_SqlClient
{
  [TestFixture]
  public class DBxManagerTests_MSSQL
  {
    private static string GetConnectionString(bool isFile)
    {
      System.Data.SqlClient.SqlConnectionStringBuilder csb = new System.Data.SqlClient.SqlConnectionStringBuilder();
      csb.DataSource = @".\SQLEXPRESS";
      csb.IntegratedSecurity = true;
      if (isFile)
        csb.AttachDBFilename = @"c:\temp\test_db.mdf";
      else
        csb.InitialCatalog = "test_db";
      return csb.ToString();
    }

    [Test]
    public void CreateDBObject([Values(false, true)] bool isFile)
    {
      DBxManager sut = DBxManager.Managers[DBxProviderNames.Sql];
      DBx res = sut.CreateDBObject(GetConnectionString(isFile));
      Assert.IsInstanceOf<SqlDBx>(res);
    }

    [Test]
    public void ReplaceDBName([Values(false, true)] bool isFile)
    {
      DBxManager sut = DBxManager.Managers[DBxProviderNames.Sql];
      string res = sut.ReplaceDBName(GetConnectionString(isFile), "db", "db2");
      SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(res);
      if (isFile)
        Assert.AreEqual(@"c:\temp\test_db2.mdf", csb.AttachDBFilename, "AttachDBFilename");
      else
        Assert.AreEqual(@"test_db2", csb.InitialCatalog, "InitialCatalog");
    }
  }
}

namespace ExtDB_tests.Data_Npgsql
{
  [TestFixture]
  public class DBxManagerTests_Npgsql
  {
    private static string ConnectionString
    {
      get
      {
        NpgsqlConnectionStringBuilder csb = new NpgsqlConnectionStringBuilder();
        csb.Host = "127.0.0.1";
        csb.Database = "test_db";
        csb.UserName = "postgres";
        csb.Password = "123";
        return csb.ToString();
      }
    }

    [Test]
    public void CreateDBObject()
    {
      DBxManager sut = DBxManager.Managers[DBxProviderNames.Npgsql];
      DBx res = sut.CreateDBObject(ConnectionString);
      Assert.IsInstanceOf<NpgsqlDBx>(res);
    }

    [Test]
    public void ReplaceDBName()
    {
      DBxManager sut = DBxManager.Managers[DBxProviderNames.Npgsql];
      string res = sut.ReplaceDBName(ConnectionString, "db", "db2");
      NpgsqlConnectionStringBuilder csb = new NpgsqlConnectionStringBuilder(res);
      Assert.AreEqual(@"test_db2", csb.Database, "Database");
    }
  }
}

