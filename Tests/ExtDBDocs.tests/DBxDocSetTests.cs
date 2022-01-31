using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Data.Docs;
using System.Data;
using FreeLibSet.Core;

namespace ExtDBDocs_tests.Data_Docs
{
  /// <summary>
  /// Проверка основных возможностей по работе с документами.
  /// Используется несколько вариантов конфигурации DBxDocTypes с использованием флагов
  /// UseDeleted, UseVersions, UseTime. Для каждой комбинации создается собственная тестовая база данных
  /// </summary>
  [TestFixture]
  public class DBxDocSetTests
  {
    #region Доступ к базе данных

    public class TestDBInfo
    {
      #region Поля

      public DBxRealDocProviderGlobal GlobalData;

      public DBxRealDocProviderSource Source;

      public DBxRealDocProvider Provider;

      #endregion
    }

    private TestDBInfo[] _TestDBs;

    public TestDBInfo this[bool useDeleted, bool useVersions, bool useTime]
    {
      get
      {
        if (_TestDBs == null)
          _TestDBs = new TestDBInfo[8];

        int index = (useDeleted ? 4 : 0) + (useVersions ? 2 : 0) + (useTime ? 1 : 0);
        if (_TestDBs[index] == null)
        {
          TestDBInfo info = new TestDBInfo();
          DBxDocTypes dts = new DBxDocTypes();
          dts.UsersTableName = String.Empty; // без пользователей
          dts.UseDeleted = useDeleted;
          dts.UseVersions = useVersions;
          dts.UseTime = useTime;
          DBxDocType dt;
          DBxSubDocType sdt;

          dt = new DBxDocType("TestDocs");
          dt.Struct.Columns.AddBoolean("F1", false);
          dt.Struct.Columns.AddInt("F2", false);
          dts.Add(dt);

          sdt = new DBxSubDocType("TestS1");
          sdt.Struct.Columns.AddString("F3", 10, true);
          dt.SubDocs.Add(sdt);

          DBxDocDBConnectionHelper conHelper = new DBxDocDBConnectionHelper();
          conHelper.ProviderName = "SQLite";
          conHelper.ConnectionString = "Data Source=:memory:";
          conHelper.DocTypes = dts;

          info.GlobalData = conHelper.CreateRealDocProviderGlobal();

          info.Source = new DBxRealDocProviderSource(info.GlobalData);
          info.Provider = new DBxRealDocProvider(info.Source, 0, false);

          _TestDBs[index] = info;
        }
        return _TestDBs[index];
      }
    }

    [OneTimeTearDown]
    public void TearDown()
    {
      if (_TestDBs == null)
        return;
      for (int i = 0; i < _TestDBs.Length; i++)
      {
        if (_TestDBs[i] != null)
          _TestDBs[i].GlobalData.DisposeDBs();
      }
      _TestDBs = null;
    }

    #endregion

    [Test]
    public void Insert([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);

      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        object[] values = con.GetValues("TestDocs", docId, "F1,F2");
        Assert.AreEqual(true, values[0], "F1");
        Assert.AreEqual(2, values[1], "F2");

        DataTable tbl = con.FillSelect("TestS1", new DBxColumns("F3"), new ValueFilter("DocId", docId));
        Assert.AreEqual(1, tbl.Rows.Count, "RowCount");
        Assert.AreEqual("ABC", tbl.Rows[0]["F3"], "F3");
      }
    }

    [Test]
    public void Insert_And_Edit([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].Insert();
      doc.Values["F1"].SetBoolean(true);
      doc.Values["F2"].SetInteger(2);
      DBxSubDoc sd = doc.SubDocs["TestS1"].Insert();
      sd.Values["F3"].SetString("ABC");
      ds.ApplyChanges(true);

      doc = ds[0][0];
      Int32 docId = ds[0][0].DocId;

      doc.Values["F1"].SetBoolean(false);
      doc.Values["F2"].SetInteger(3);
      doc.SubDocs["TestS1"][0].Values["F3"].SetString("DEF");
      ds.ApplyChanges(false);

      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        object[] values = con.GetValues("TestDocs", docId, "F1,F2");
        Assert.AreEqual(false, DataTools.GetBool(values[0]), "F1");
        Assert.AreEqual(3, values[1], "F2");

        DataTable tbl = con.FillSelect("TestS1", new DBxColumns("F3"), new ValueFilter("DocId", docId));
        Assert.AreEqual(1, tbl.Rows.Count, "RowCount");
        Assert.AreEqual("DEF", tbl.Rows[0]["F3"], "F3");
      }
    }


    private Int32 CreateTestDoc(TestDBInfo info)
    {
      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].Insert();
      doc.Values["F1"].SetBoolean(true);
      doc.Values["F2"].SetInteger(2);
      DBxSubDoc sd = doc.SubDocs["TestS1"].Insert();
      sd.Values["F3"].SetString("ABC");
      ds.ApplyChanges(true);

      Int32 docId = ds[0][0].DocId;
      Assert.Greater(docId, 0, "DocId");
      return docId;
    }
  }
}
