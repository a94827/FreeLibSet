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

    #region Конструктор

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

    #endregion

    #region Основные операции с документом

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
      AssertTestDoc(info, docId, "Version #2", true, 2, "ABC");

      doc.Values["F1"].SetBoolean(false);
      doc.Values["F2"].SetInteger(3);
      doc.SubDocs["TestS1"][0].Values["F3"].SetString("DEF");

      ds.ApplyChanges(true);
      doc = ds[0][0];
      Assert.AreEqual(DBxDocState.Insert, doc.DocState, "DBxSingleDoc.DocState #4");
      Assert.AreEqual(DBxDocState.Insert, ds.DocState, "DBxDocSet.DocState #4");
      if (useVersions)
        Assert.AreEqual(1, doc.Version, "DBxSingleDoc.Version #4");
      AssertTestDoc(info, docId, "Version #4", false, 3, "DEF");
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
      AssertTestDoc(info, docId, "Version #2", true, 3, "ABC");

      doc.Values["F2"].SetInteger(4);
      ds.ApplyChanges(true);
      doc = ds[0][0];
      Assert.AreEqual(docId, doc.DocId, "DocId #3");
      Assert.AreEqual(4, doc.Values["F2"].AsInteger, "F2 #3");
      if (useVersions)
        Assert.AreEqual(2, doc.Version, "Version #3");
      AssertTestDoc(info, docId, "Version #3", true, 4, "ABC");
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
      AssertTestDoc(info, docId, "Version #2", true, 3, "ABC");

      doc.Values["F2"].SetInteger(4);
      ds.ApplyChanges(true);
      doc = ds[0][0];
      Assert.AreEqual(docId, doc.DocId, "DocId #3");
      Assert.AreEqual(4, doc.Values["F2"].AsInteger, "F2 #3");
      if (useVersions)
        Assert.AreEqual(2, doc.Version, "Version #3");
      AssertTestDoc(info, docId, "Version #3", true, 4, "ABC");
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

      AssertTestDoc(info, docId, "#2", true, 2, "ABC");
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

    [Test]
    public void Delete_Forever([Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      // Проверка удаления для базы данных без поля Deleted

      TestDBInfo info = this[false, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);
      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["TestDocs"].Delete(docId);
      ds.ApplyChanges(false);

      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        Assert.AreEqual(0, con.FillSelect("TestDocs", null, new ValueFilter("Id", docId)).Rows.Count, "doc table");
        Assert.AreEqual(0, con.FillSelect("TestS1", null, new ValueFilter("DocId", docId)).Rows.Count, "subdoc table");
      }
    }

    [Test]
    public void Delete_UseDeleted([Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      // Проверка удаления для базы данных при наличии поля Deleted

      TestDBInfo info = this[true, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);
      DeleteDoc(info, docId);

      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        DataTable tblDoc = con.FillSelect("TestDocs", null, new ValueFilter("Id", docId));
        Assert.AreEqual(1, tblDoc.Rows.Count, "doc table");
        Assert.IsTrue(DataTools.GetBool(tblDoc.Rows[0], "Deleted"), "Deleted");
        Assert.AreEqual(1, con.FillSelect("TestS1", null, new ValueFilter("DocId", docId)).Rows.Count, "subdoc table");
        // У поддокументов поле Deleted не устанавливается в True.
      }
    }

    [Test]
    public void Restore([Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[true, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);
      DeleteDoc(info, docId);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].Edit(docId);
      Assert.IsTrue(doc.Deleted, "Deleted");

      ds.ApplyChanges(false);
      AssertTestDoc(info, docId, "", true, 2, "ABC");
    }

    #endregion

    #region Групповое редактирование документов

    [Test]
    public void Group_ViewDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info, true, 1);
      Int32 docId2 = CreateTestDoc(info, true, 2);
      Int32 docId3 = CreateTestDoc(info, false, 3);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["TestDocs"].View(new Int32[] { docId1, docId2 });

      Assert.AreEqual(docId1, ds["TestDocs"][0].DocId, "DocId #1");
      Assert.AreEqual(docId2, ds["TestDocs"][1].DocId, "DocId #2");

      IDBxDocValues grpVals = ds["TestDocs"].Values;
      Assert.AreEqual(2, grpVals.DocCount, "DocCount #1,2");
      Assert.IsTrue(grpVals.IsReadOnly, "IsReadOnly #1,2");
      Assert.IsFalse(grpVals["F1"].Grayed, "Grayed[F1] #1,2");
      Assert.IsTrue(grpVals["F1"].AsBoolean, "Value[F1] #1,2");
      Assert.IsTrue(grpVals["F2"].Grayed, "Grayed[F2] #1,2");

      // Открываем третий документ
      ds["TestDocs"].View(docId3);
      grpVals = ds["TestDocs"].Values;
      Assert.AreEqual(3, grpVals.DocCount, "DocCount #1,2,3");
      Assert.AreEqual(docId3, ds["TestDocs"][2].DocId, "DocId #2");

      Assert.IsTrue(grpVals.IsReadOnly, "IsReadOnly #1,2,3");
      Assert.IsTrue(grpVals["F1"].Grayed, "Grayed[F1] #1,2,3");
      Assert.IsTrue(grpVals["F2"].Grayed, "Grayed[F2] #1,2,3");

      Assert.AreEqual(1, ds["TestDocs"][0].Values["F2"].AsInteger, "Value[F2] #1");
      Assert.AreEqual(2, ds["TestDocs"][1].Values["F2"].AsInteger, "Value[F2] #2");
      Assert.AreEqual(3, ds["TestDocs"][2].Values["F2"].AsInteger, "Value[F2] #3");
    }

    [Test]
    public void Group_EditDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info, true, 1, "ABC");
      Int32 docId2 = CreateTestDoc(info, false, 2, "DEF");
      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["TestDocs"].Edit(new Int32[] { docId1, docId2 });
      IDBxDocValues grpVals = ds["TestDocs"].Values;
      Assert.AreEqual(2, grpVals.DocCount, "DocCount #1,2");
      Assert.IsFalse(grpVals.IsReadOnly, "IsReadOnly #1,2");

      grpVals["F1"].SetBoolean(false);
      ds["TestDocs"][0].Values["F2"].SetInteger(3);
      ds["TestDocs"][1].Values["F2"].SetInteger(4);


      Assert.IsFalse(grpVals["F1"].Grayed, "Grayed[F1] #1,2");
      Assert.IsTrue(grpVals["F2"].Grayed, "Grayed[F2] #1,2");
      ds.ApplyChanges(false);

      AssertTestDoc(info, docId1, "Doc #1", false, 3, "ABC");
      AssertTestDoc(info, docId2, "Doc #2", false, 4, "DEF");
    }

    #endregion

    #region Операции с поддокументами

    #region Создание и удаление поддокумента в одном сеансе редактирования

    [Test]
    public void Insert_AddAndDeleteSubDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].Insert();
      DBxSubDoc sd = doc.SubDocs["TestS1"].Insert();
      sd.Values["F3"].SetString("ABC");
      sd.Delete();
      ds.ApplyChanges(true);
      doc = ds[0][0];
      Int32 docId = doc.DocId;

      AssertTestDoc(info, docId, "", false, 0);
    }

    [Test]
    public void Edit_AddAndDeleteSubDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info, true, 1, "ABC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].Edit(docId);

      DBxSubDoc sd = doc.SubDocs["TestS1"].Insert();
      sd.Values["F3"].SetString("DEF");
      sd.Delete();

      ds.ApplyChanges(false);
      AssertTestDoc(info, docId, "", true, 1, "ABC");
    }

    #endregion

    #region Редактирование поддокументов

    [Test]
    public void Edit_AddSubDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info, true, 1, "ABC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].Edit(docId);

      DBxSubDoc sd = doc.SubDocs["TestS1"].Insert();
      sd.Values["F3"].SetString("DEF");

      ds.ApplyChanges(false);
      AssertTestDoc(info, docId, "", true, 1, "ABC", "DEF");
    }

    [Test]
    public void Edit_DeleteSubDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info, true, 1, "ABC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].Edit(docId);

      doc.SubDocs["TestS1"][0].Delete();

      ds.ApplyChanges(false);
      AssertTestDoc(info, docId, "", true, 1);
    }


    [Test]
    public void Edit_ChangeSubDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info, true, 1, "ABC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].Edit(docId);

      doc.SubDocs["TestS1"][0].Values["F3"].SetString("DEF");

      ds.ApplyChanges(false);
      AssertTestDoc(info, docId, "", true, 1, "DEF");
    }

    #endregion

    #region Групповое редактирование поддокументов

    [Test]
    public void Group_ViewSubDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info, true, 1, "ABC", "DEF");
      Int32 docId2 = CreateTestDoc(info, true, 2, "GHI");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1= ds["TestDocs"].View(docId1);
      Assert.AreEqual(2, doc1.SubDocs["TestS1"].SubDocCount, "SubDocCount #1");
      IDBxDocValues grpVals = ds["TestDocs"].SubDocs["TestS1"].Values;
      Assert.AreEqual(2, grpVals.DocCount, "Values.DocCount #1");
      Assert.IsTrue(grpVals["F3"].Grayed, "Grayed #1");

      // Открываем еще один документ
      DBxSingleDoc doc2 = ds["TestDocs"].View(docId2);
      Assert.AreEqual(1, doc2.SubDocs["TestS1"].SubDocCount, "SubDocCount #2");
      grpVals = ds["TestDocs"].SubDocs["TestS1"].Values;
      Assert.AreEqual(3, grpVals.DocCount, "Values.DocCount #1,2");
      Assert.IsTrue(grpVals["F3"].Grayed, "Grayed #1,2");
    }

    [Test]
    public void Group_EditSubDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info, true, 1, "ABC", "DEF");
      Int32 docId2 = CreateTestDoc(info, true, 2, "GHI");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["TestDocs"].Edit(docId1);
      DBxSingleDoc doc2 = ds["TestDocs"].Edit(docId2);
      IDBxDocValues grpVals = ds["TestDocs"].SubDocs["TestS1"].Values;
      grpVals["F3"].SetString("XXX");
      Assert.IsFalse(grpVals["F3"].Grayed, "Grayed #1");

      doc1.SubDocs["TestS1"][1].Values["F3"].SetString("YYY");
      Assert.IsTrue(grpVals["F3"].Grayed, "Grayed #2");
      ds.ApplyChanges(false);

      AssertTestDoc(info, docId1, "Doc #1", true, 1, "XXX", "YYY");
      AssertTestDoc(info, docId2, "Doc #2", true, 2, "XXX");
    }

    [Test]
    public void Group_DeleteSubDocs([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info, true, 1, "ABC", "DEF");
      Int32 docId2 = CreateTestDoc(info, true, 2, "GHI");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["TestDocs"].Edit(new Int32[] { docId1, docId2 });
      DBxMultiSubDocs sds=ds[0].SubDocs["TestS1"];
      Assert.AreEqual(3, sds.SubDocCount, "SubDocCount #1");

      sds.Delete();
      Assert.AreEqual(3, sds.SubDocCount, "SubDocCount #2");
      ds.ApplyChanges(false);
      AssertTestDoc(info, docId1, "Doc #1", true, 1);
      AssertTestDoc(info, docId2, "Doc #2", true, 2);
    }

    #endregion

    #endregion

    #region Просмотр истории

    [Test]
    public void ViewVersion()
    {
      TestDBInfo info = this[true, true, true];
      Int32 docId = CreateTestDoc(info, true, 1, "ABC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].Edit(docId);
      doc.Values["F2"].SetInteger(2);
      DBxSubDoc sd = doc.SubDocs["TestS1"].Insert();
      sd.Values["F3"].SetString("DEF");
      ds.ApplyChanges(false);

      // Убрать в DBxDocProviderTests
      //DataTable tbl=ds.DocProvider.GetDocHistTable("TestDocs", docId);

      ds = new DBxDocSet(info.Provider);
      doc = ds["TestDocs"].ViewVersion(docId, 1);
      Assert.IsTrue(ds.VersionView, "VersionView #1");
      Assert.AreEqual(1, doc.Version, "Version #1");
      AssertTestDoc(doc, "#1", true, 1, "ABC");

      ds = new DBxDocSet(info.Provider);
      doc = ds["TestDocs"].ViewVersion(docId, 2);
      Assert.IsTrue(ds.VersionView, "VersionView #2");
      Assert.AreEqual(2, doc.Version, "Version #2");
      AssertTestDoc(doc, "#2", true, 2, "ABC", "DEF");
    }

#if XXX // В текущей реализации не выбрасывает исключение. Не знаю, должно ли.
    [Test]
    public void ViewVersion_Failure()
    {
      TestDBInfo info = this[true, true, true];
      Int32 docId1 = CreateTestDoc(info, true, 1);
      Int32 docId2 = CreateTestDoc(info, true, 2);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["TestDocs"].View(docId1);

      Assert.Catch(delegate() { ds["TestDocs"].ViewVersion(docId2, 1); });
    }
#endif

    #endregion

    #region Тестирование прочих свойств

    [Test]
    public void EditIfNotChanged([Values(false, true)] bool useDeleted, [Values(false, true)] bool useTime,
      [Values(false, true)] bool propValue)
    {
      TestDBInfo info = this[useDeleted, true, useTime]; // версии нужно сохранять, иначе не проверить
      Int32 docId = CreateTestDoc(info, true, 1, "ABC", "DEF");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds.EditIfNotChanged = propValue;
      DBxSingleDoc doc = ds["TestDocs"].Edit(docId);
      Assert.IsFalse(doc.IsDataModified, "IsDataModified");

      ds.ApplyChanges(false);

      ds = new DBxDocSet(info.Provider);
      doc = ds["TestDocs"].View(docId);
      Assert.AreEqual(propValue ? 2 : 1, doc.Version, "Version");
    }

    [Test]
    public void DocCount_and_DocState()
    {
      TestDBInfo info = this[false, false, false];
      Int32 docId1 = CreateTestDoc(info, true, 1);
      Int32 docId2 = CreateTestDoc(info, true, 2);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["TestDocs"].View(docId1);
      ds["TestDocs"].Edit(docId2);

      Assert.AreEqual(2, ds.DocCount, "DocCount #1");
      Assert.AreEqual(DBxDocState.Mixed, ds.DocState, "DocState #1");
      Assert.AreEqual(DBxDocState.Edit, ds.DocStateNoView, "DocStateNoView #1");

      Assert.AreEqual(1, ds.GetDocCount(DBxDocState.View), "GetDocCount(View) #1");
      Assert.AreEqual(1, ds.GetDocCount(DBxDocState.Edit), "GetDocCount(Edit) #1");
      Assert.AreEqual(0, ds.GetDocCount(DBxDocState.Insert), "GetDocCount(Insert) #1");
      Assert.AreEqual(0, ds.GetDocCount(DBxDocState.Delete), "GetDocCount(Delete) #1");

      ds["TestDocs"].Insert();
      Assert.AreEqual(3, ds.DocCount, "DocCount #2");
      Assert.AreEqual(DBxDocState.Mixed, ds.DocStateNoView, "DocStateNoView #2");
      Assert.AreEqual(1, ds.GetDocCount(DBxDocState.Insert), "GetDocCount(Insert) #2");

      ds["TestDocs"][1].Delete();
      Assert.AreEqual(3, ds.DocCount, "DocCount #3");
      Assert.AreEqual(0, ds.GetDocCount(DBxDocState.Edit), "GetDocCount(Edit) #3");
      Assert.AreEqual(1, ds.GetDocCount(DBxDocState.Delete), "GetDocCount(Delete) #3");
    }

    [Test]
    public void DocSelection()
    {
      TestDBInfo info = this[false, false, false];
      Int32 docId1 = CreateTestDoc(info, true, 1);
      Int32 docId2 = CreateTestDoc(info, true, 2);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["TestDocs"].View(docId1);
      ds["TestDocs"].Edit(docId2);
      ds["TestDocs"].Insert();

      DBxDocSelection docSel = ds.DocSelection;
      Assert.AreEqual(2, docSel.TotalCount, "TotalCount #1");
      Assert.IsTrue(docSel.ContainsAll("TestDocs", new Int32[]{docId1, docId2}), "DocIds #1");

      docSel = ds.GetDocSelection(DBxDocState.View);
      Assert.AreEqual(1, docSel.TotalCount, "TotalCount #2");
      Assert.AreEqual(docId1, docSel["TestDocs"][0], "DocIds #2");

      docSel = ds.GetDocSelection(DBxDocState.Edit);
      Assert.AreEqual(1, docSel.TotalCount, "TotalCount #2");
      Assert.AreEqual(docId2, docSel["TestDocs"][0], "DocIds #2");

      docSel = ds.GetDocSelection(DBxDocState.Insert);
      Assert.AreEqual(0, docSel.TotalCount, "TotalCount #3");

      docSel = ds.GetDocSelection(DBxDocState.Delete);
      Assert.AreEqual(0, docSel.TotalCount, "TotalCount #4");

      ds["TestDocs"][1].Delete();

      docSel = ds.GetDocSelection(DBxDocState.Edit);
      Assert.AreEqual(0, docSel.TotalCount, "TotalCount #5");

      docSel = ds.GetDocSelection(DBxDocState.Delete);
      Assert.AreEqual(1, docSel.TotalCount, "TotalCount #6");
      Assert.AreEqual(docId2, docSel["TestDocs"][0], "DocIds #6");
    }

    #endregion

    #region Вспомогательные методы

    private Int32 CreateTestDoc(TestDBInfo info)
    {
      return CreateTestDoc(info, true, 2, "ABC");
    }

    private Int32 CreateTestDoc(TestDBInfo info, bool F1, int F2, params string[] F3)
    {
      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["TestDocs"].Insert();
      doc.Values["F1"].SetBoolean(F1);
      doc.Values["F2"].SetInteger(F2);
      for (int i = 0; i < F3.Length; i++)
      {
        DBxSubDoc sd = doc.SubDocs["TestS1"].Insert();
        sd.Values["F3"].SetString(F3[i]);
      }
      ds.ApplyChanges(true);
      Int32 docId = ds[0][0].DocId;
      Assert.Greater(docId, 0, "DocId");
      return docId;
    }

    private static void DeleteDoc(TestDBInfo info, Int32 docId)
    {
      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["TestDocs"].Delete(docId);
      ds.ApplyChanges(true);
    }

    /// <summary>
    /// Тестирование документа в базе данных
    /// </summary>
    /// <param name="info"></param>
    /// <param name="docId"></param>
    /// <param name="message"></param>
    /// <param name="F1"></param>
    /// <param name="F2"></param>
    /// <param name="F3"></param>
    private void AssertTestDoc(TestDBInfo info, Int32 docId, string message, bool F1, int F2, params string[] F3)
    {
      if (!String.IsNullOrEmpty(message))
        message += ". ";

      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        DataTable tblDoc = con.FillSelect("TestDocs", null, new ValueFilter("Id", docId));
        Assert.AreEqual(1, tblDoc.Rows.Count, message + "doc table count");
        if (info.GlobalData.DocTypes.UseDeleted)
          Assert.IsFalse(DataTools.GetBool(tblDoc.Rows[0], "Deleted"), message + "Deleted");
        Assert.AreEqual(F1, DataTools.GetBool(tblDoc.Rows[0], "F1"), message + "F1");
        Assert.AreEqual(F2, DataTools.GetInt(tblDoc.Rows[0], "F2"), message + "F2");

        Array.Sort<string>(F3);
        DBxFilter filter = new ValueFilter("DocId", docId);
        if (info.GlobalData.DocTypes.UseDeleted)
          filter = new AndFilter(filter, new ValueFilter("Deleted", false));
        DataTable tblSubDoc = con.FillSelect("TestS1", null, filter);
        string[] a = new string[tblSubDoc.Rows.Count];
        for (int i = 0; i < a.Length; i++)
          a[i] = DataTools.GetString(tblSubDoc.Rows[i], "F3");
        Array.Sort<string>(a);
        Assert.AreEqual(F3, a, message + "F3");
        // У поддокументов поле Deleted не устанавливается в True.
      }
    }

    /// <summary>
    /// Тестирование загруженного документа
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="message"></param>
    /// <param name="F1"></param>
    /// <param name="F2"></param>
    /// <param name="F3"></param>
    private void AssertTestDoc(DBxSingleDoc doc, string message, bool F1, int F2, params string[] F3)
    {
      if (!String.IsNullOrEmpty(message))
        message += ". ";

      Assert.AreEqual(F1, doc.Values["F1"].AsBoolean, message + "F1");
      Assert.AreEqual(F2, doc.Values["F2"].AsInteger, message + "F2");

      Array.Sort<string>(F3);
      string[] a = new string[doc.SubDocs["TestS1"].SubDocCount];
      for (int i = 0; i < a.Length; i++)
      {
        DBxSubDoc sd = doc.SubDocs["TestS1"][i];
        a[i] = sd.Values["F3"].AsString;
      }
      Array.Sort<string>(a);
      Assert.AreEqual(F3, a, message + "F3");
    }

    #endregion
  }
}
