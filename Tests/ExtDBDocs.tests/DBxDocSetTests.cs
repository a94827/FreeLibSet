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
    public void Constructor([Values(false, true)] bool useHistory)
    {
      TestDBInfo info = this[useHistory, useHistory, useHistory];
      DBxDocSet sut = new DBxDocSet(info.Provider);
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.AreEqual(0, sut.DocCount, "DocCount");
      Assert.AreSame(info.Provider, sut.DocProvider, "DocProvider");
      Assert.IsTrue(sut.DocSelection.IsEmpty, "DocSelection");
      Assert.AreEqual(DBxDocState.None, sut.DocState, "DocState");
      Assert.AreEqual(DBxDocState.None, sut.DocStateNoView, "DocStateNoNew");
      Assert.IsFalse(sut.EditIfNotChanged, "EditIfNotChanged");
      Assert.IsFalse(sut.IgnoreAllLocks, "IgnoreAllLocks");
      Assert.AreEqual(0, sut.IgnoredLocks.Count, "IgnoredLocks");
      Assert.IsFalse(sut.IsDataModified, "IsDataModified");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(0, sut.UserActionId, "UserActionId");
      Assert.IsFalse(sut.VersionView, "VersionView");
    }

#if XXX
    [Test]
    public void Insert_SimpleTest([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
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
#endif

    [Test]
    public void Insert([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].Insert();
      doc.Values["F1"].SetBoolean(true);
      doc.Values["F2"].SetInteger(2);
      DBxSubDoc sd = doc.SubDocs["TestS1"].Insert();
      sd.Values["F3"].SetString("ABC");

      Assert.AreEqual(DBxDocState.Insert, doc.DocState, "DBxSingleDoc.DocState #1");
      Assert.AreEqual(DBxDocState.Insert, ds.DocState, "DBxDocSet.DocState #1");
      Assert.IsFalse(doc.Deleted, "Deleted");
      Assert.AreEqual(1, ds.DocCount, "DBxDocSet.DocCount #1");
      Assert.Less(doc.DocId, 0, "DocId #1");

      ds.ApplyChanges(true);

      Assert.AreEqual(1, ds.DocCount, "DBxDocSet.DocCount #3");

      doc = ds[0][0];
      Int32 docId = ds[0][0].DocId;
      Assert.Greater(doc.DocId, 0, "DocId #2");
      if (useVersions)
        Assert.AreEqual(1, doc.Version, "DBxSingleDoc.Version #2");
      Assert.AreEqual(DBxDocState.Insert, doc.DocState, "DBxSingleDoc.DocState #2");
      Assert.AreEqual(DBxDocState.Insert, ds.DocState, "DBxDocSet.DocState #2");

      doc.Values["F1"].SetBoolean(false);
      doc.Values["F2"].SetInteger(3);
      doc.SubDocs["TestS1"][0].Values["F3"].SetString("DEF");

      ds.ApplyChanges(true);
      doc = ds[0][0];
      Assert.AreEqual(DBxDocState.Insert, doc.DocState, "DBxSingleDoc.DocState #4");
      Assert.AreEqual(DBxDocState.Insert, ds.DocState, "DBxDocSet.DocState #4");
      if (useVersions)
        Assert.AreEqual(1, doc.Version, "DBxSingleDoc.Version #4");

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


    [Test]
    public void Edit_ById([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].Edit(docId);

      Assert.AreEqual(docId, doc.DocId, "DocId #1");
      Assert.AreEqual(DBxDocState.Edit, doc.DocState, "DBxSingleDoc.DocState");
      Assert.AreEqual(DBxDocState.Edit, ds.DocState, "DBxSingleDoc.DocState");
      Assert.IsFalse(doc.Deleted, "Deleted");
      if (useVersions)
        Assert.AreEqual(1, doc.Version, "Version #1");
      Assert.IsFalse(doc.IsDataModified, "IsDataModified after Edit()");
      Assert.AreEqual(2, doc.Values["F2"].AsInteger, "F2 #1");

      doc.Values["F2"].SetInteger(3);
      Assert.IsTrue(doc.IsDataModified, "IsDataModified before ApplyChanges()");

      ds.ApplyChanges(true);
      doc = ds[0][0];
      Assert.AreEqual(docId, doc.DocId, "DocId #2");
      Assert.AreEqual(3, doc.Values["F2"].AsInteger, "F2 #2");
      if (useVersions)
        Assert.AreEqual(2, doc.Version, "Version #2");

      doc.Values["F2"].SetInteger(4);
      ds.ApplyChanges(true);
      doc = ds[0][0];
      Assert.AreEqual(docId, doc.DocId, "DocId #3");
      Assert.AreEqual(4, doc.Values["F2"].AsInteger, "F2 #3");
      if (useVersions)
        Assert.AreEqual(2, doc.Version, "Version #3");
    }


    [Test]
    public void Edit_ByView([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].View(docId);

      doc.Edit();

      Assert.AreEqual(docId, doc.DocId, "DocId #1");
      Assert.AreEqual(DBxDocState.Edit, doc.DocState, "DBxSingleDoc.DocState");
      Assert.AreEqual(DBxDocState.Edit, ds.DocState, "DBxSingleDoc.DocState");
      Assert.IsFalse(doc.Deleted, "Deleted");
      if (useVersions)
        Assert.AreEqual(1, doc.Version, "Version #1");
      Assert.IsFalse(doc.IsDataModified, "IsDataModified after Edit()");
      Assert.AreEqual(2, doc.Values["F2"].AsInteger, "F2 #1");

      doc.Values["F2"].SetInteger(3);
      Assert.IsTrue(doc.IsDataModified, "IsDataModified before ApplyChanges()");

      ds.ApplyChanges(true);
      doc = ds[0][0];
      Assert.AreEqual(docId, doc.DocId, "DocId #2");
      Assert.AreEqual(3, doc.Values["F2"].AsInteger, "F2 #2");
      if (useVersions)
        Assert.AreEqual(2, doc.Version, "Version #2");

      doc.Values["F2"].SetInteger(4);
      ds.ApplyChanges(true);
      doc = ds[0][0];
      Assert.AreEqual(docId, doc.DocId, "DocId #3");
      Assert.AreEqual(4, doc.Values["F2"].AsInteger, "F2 #3");
      if (useVersions)
        Assert.AreEqual(2, doc.Version, "Version #3");
    }

    [Test]
    public void View([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].View(docId);

      Assert.AreEqual(docId, doc.DocId, "DocId #1");
      Assert.AreEqual(DBxDocState.View, doc.DocState, "DBxSingleDoc.DocState #1");
      Assert.AreEqual(DBxDocState.View, ds.DocState, "DBxDocSet.DocState #1");
      Assert.IsFalse(doc.Deleted, "Deleted");
      if (useVersions)
        Assert.AreEqual(1, doc.Version, "Version #1");
      Assert.IsFalse(doc.IsDataModified, "IsDataModified");
      Assert.AreEqual(2, doc.Values["F2"].AsInteger, "F2 #1");

      ds.ApplyChanges(true);
      doc = ds[0][0];
      Assert.AreEqual(docId, doc.DocId, "DocId #2");
      Assert.AreEqual(DBxDocState.View, doc.DocState, "DBxSingleDoc.DocState #2");
      Assert.AreEqual(DBxDocState.View, ds.DocState, "DBxDocSet.DocState #2");
    }

    [Test]
    public void Delete_ById([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["TestDocs"].Delete(docId);

      Assert.AreEqual(DBxDocState.Delete, ds.DocState, "DBxDocSet.DocState");
      Assert.AreEqual(1, ds.DocCount, "DocCount #1");
      ds.ApplyChanges(true);
    }

    [Test]
    public void Delete_ByView([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].View(docId);

      doc.Delete();
      Assert.AreEqual(DBxDocState.Delete, doc.DocState, "DBxSingleDoc.DocState");
      Assert.AreEqual(DBxDocState.Delete, ds.DocState, "DBxDocSet.DocState");
      Assert.AreEqual(1, ds.DocCount, "DocCount #1");
      ds.ApplyChanges(true);
    }

    [Test]
    public void Delete_ByEdit([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].Edit(docId);

      doc.Delete();
      Assert.AreEqual(DBxDocState.Delete, doc.DocState, "DBxSingleDoc.DocState");
      Assert.AreEqual(DBxDocState.Delete, ds.DocState, "DBxDocSet.DocState");
      Assert.AreEqual(1, ds.DocCount, "DocCount #1");
      ds.ApplyChanges(true);
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
