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
    /*
     * Структура тестовой базы данных
     * Документ "D1"
     *   Поля: "F101" - логическое Not Null
     *         "F102" - Числовое Not Null
     *         "F103" - Числовое Nullable
     *         "F104" - Ссылочное поле на "D1" (для организации деревьев). Nullable
     *         "F105" - Ссылочное поле на "D2" Nullable
     *         "F106" - Ссылочное поле на "SD21" Nullable
     *   Поддокумент "SD11"      
     *      Поля:
     *         "F111" - CHAR(30) Not Null
     *         "F112" - CHAR(10) Nullable
     *         "F113" - Ссылочное поле на "SD11" (дерево поддокументов)
     *         "F114" - Ссылочное поле на "D2"  Nullable
     *         "F115" - Ссылочное поле на "SD21" Nullable
     * Документ "D2"        
     *   Поля: "F201" CHAR(10) Not Null
     *         "F202" - Ссылочное поле на "D1" Not Null
     *   Поддокумент "SD21"
     *      Поля: "F211" Date Nullable
     */

    #region Доступ к базе данных

    /// <summary>
    /// Описание для одной тестовой базы данных
    /// </summary>
    public class TestDBInfo
    {
      #region Поля

      public DBxRealDocProviderGlobal GlobalData;

      public DBxRealDocProviderSource Source;

      public DBxRealDocProvider Provider;

      #endregion
    }

    /// <summary>
    /// Тестовые базы данных с разными комбинациями параметров UseXXX
    /// </summary>
    private TestDBInfo[] _TestDBs;

    /// <summary>
    /// Получить доступ к базе данных с заданной комбинацией управляющих
    /// </summary>
    /// <param name="useDeleted"></param>
    /// <param name="useVersions"></param>
    /// <param name="useTime"></param>
    /// <returns></returns>
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

          dt = new DBxDocType("D1");
          dt.Struct.Columns.AddBoolean("F101", false);
          dt.Struct.Columns.AddInt("F102", false);
          dt.Struct.Columns.AddInt("F103", true);
          dt.Struct.Columns.AddReference("F104", "D1", true);
          dt.Struct.Columns.AddReference("F105", "D2", true);
          dt.Struct.Columns.AddReference("F106", "SD21", true);
          dts.Add(dt);

          sdt = new DBxSubDocType("SD11");
          sdt.Struct.Columns.AddString("F111", 3, false);
          sdt.Struct.Columns.AddString("F112", 10, true);
          sdt.Struct.Columns.AddReference("F113", "SD11", true);
          sdt.Struct.Columns.AddReference("F114", "D2", true);
          sdt.Struct.Columns.AddReference("F115", "SD21", true);
          dt.SubDocs.Add(sdt);

          dt = new DBxDocType("D2");
          dt.Struct.Columns.AddString("F201", 10, false);
          dt.Struct.Columns.AddReference("F202", "D1", true);
          dts.Add(dt);

          sdt = new DBxSubDocType("SD21");
          sdt.Struct.Columns.AddDate("F211", true);
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
      //Assert.IsFalse(sut.EditIfNotChanged, "EditIfNotChanged");
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
      DBxSingleDoc doc = ds["D1"].Insert();
      doc.Values["F101"].SetBoolean(true);
      doc.Values["F102"].SetInteger(2);
      DBxSubDoc sd = doc.SubDocs["SD11"].Insert();
      sd.Values["F111"].SetString("ABC");

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

      doc.Values["F101"].SetBoolean(false);
      doc.Values["F102"].SetInteger(3);
      doc.SubDocs["SD11"][0].Values["F111"].SetString("DEF");

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
      DBxSingleDoc doc = ds["D1"].Edit(docId);

      Assert.AreEqual(docId, doc.DocId, "DocId #1");
      Assert.AreEqual(DBxDocState.Edit, doc.DocState, "DBxSingleDoc.DocState");
      Assert.AreEqual(DBxDocState.Edit, ds.DocState, "DBxSingleDoc.DocState");
      Assert.IsFalse(doc.Deleted, "Deleted");
      if (useVersions)
        Assert.AreEqual(1, doc.Version, "Version #1");
      Assert.IsFalse(doc.IsDataModified, "IsDataModified after Edit()");
      Assert.AreEqual(2, doc.Values["F102"].AsInteger, "F102 #1");

      doc.Values["F102"].SetInteger(3);
      Assert.IsTrue(doc.IsDataModified, "IsDataModified before ApplyChanges()");

      ds.ApplyChanges(true);
      doc = ds[0][0];
      Assert.AreEqual(docId, doc.DocId, "DocId #2");
      Assert.AreEqual(3, doc.Values["F102"].AsInteger, "F102 #2");
      if (useVersions)
        Assert.AreEqual(2, doc.Version, "Version #2");
      AssertTestDoc(info, docId, "Version #2", true, 3, "ABC");

      doc.Values["F102"].SetInteger(4);
      ds.ApplyChanges(true);
      doc = ds[0][0];
      Assert.AreEqual(docId, doc.DocId, "DocId #3");
      Assert.AreEqual(4, doc.Values["F102"].AsInteger, "F102 #3");
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
      DBxSingleDoc doc = ds["D1"].View(docId);

      doc.Edit();

      Assert.AreEqual(docId, doc.DocId, "DocId #1");
      Assert.AreEqual(DBxDocState.Edit, doc.DocState, "DBxSingleDoc.DocState");
      Assert.AreEqual(DBxDocState.Edit, ds.DocState, "DBxSingleDoc.DocState");
      Assert.IsFalse(doc.Deleted, "Deleted");
      if (useVersions)
        Assert.AreEqual(1, doc.Version, "Version #1");
      Assert.IsFalse(doc.IsDataModified, "IsDataModified after Edit()");
      Assert.AreEqual(2, doc.Values["F102"].AsInteger, "F102 #1");

      doc.Values["F102"].SetInteger(3);
      Assert.IsTrue(doc.IsDataModified, "IsDataModified before ApplyChanges()");

      ds.ApplyChanges(true);
      doc = ds[0][0];
      Assert.AreEqual(docId, doc.DocId, "DocId #2");
      Assert.AreEqual(3, doc.Values["F102"].AsInteger, "F102 #2");
      if (useVersions)
        Assert.AreEqual(2, doc.Version, "Version #2");
      AssertTestDoc(info, docId, "Version #2", true, 3, "ABC");

      doc.Values["F102"].SetInteger(4);
      ds.ApplyChanges(true);
      doc = ds[0][0];
      Assert.AreEqual(docId, doc.DocId, "DocId #3");
      Assert.AreEqual(4, doc.Values["F102"].AsInteger, "F102 #3");
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
      DBxSingleDoc doc = ds["D1"].View(docId);

      Assert.AreEqual(docId, doc.DocId, "DocId #1");
      Assert.AreEqual(DBxDocState.View, doc.DocState, "DBxSingleDoc.DocState #1");
      Assert.AreEqual(DBxDocState.View, ds.DocState, "DBxDocSet.DocState #1");
      Assert.IsFalse(doc.Deleted, "Deleted");
      if (useVersions)
        Assert.AreEqual(1, doc.Version, "Version #1");
      Assert.IsFalse(doc.IsDataModified, "IsDataModified");
      Assert.AreEqual(2, doc.Values["F102"].AsInteger, "F102 #1");

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
      ds["D1"].Delete(docId);

      Assert.AreEqual(DBxDocState.Delete, ds.DocState, "DBxDocSet.DocState");
      Assert.AreEqual(1, ds.DocCount, "DocCount #1");
      ds.ApplyChanges(true);

      AssertTestDocDeleted(info, docId, "");
    }

    [Test]
    public void Delete_ByView([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].View(docId);

      doc.Delete();
      Assert.AreEqual(DBxDocState.Delete, doc.DocState, "DBxSingleDoc.DocState");
      Assert.AreEqual(DBxDocState.Delete, ds.DocState, "DBxDocSet.DocState");
      Assert.AreEqual(1, ds.DocCount, "DocCount #1");
      ds.ApplyChanges(true);

      AssertTestDocDeleted(info, docId, "");
    }

    [Test]
    public void Delete_ByEdit([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);

      doc.Delete();
      Assert.AreEqual(DBxDocState.Delete, doc.DocState, "DBxSingleDoc.DocState");
      Assert.AreEqual(DBxDocState.Delete, ds.DocState, "DBxDocSet.DocState");
      Assert.AreEqual(1, ds.DocCount, "DocCount #1");
      ds.ApplyChanges(true);
      AssertTestDocDeleted(info, docId, "");
    }

    [Test]
    public void Delete_Forever([Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      // Проверка удаления для базы данных без поля Deleted

      TestDBInfo info = this[false, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);
      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Delete(docId);
      ds.ApplyChanges(false);

      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        Assert.AreEqual(0, con.FillSelect("D1", null, new ValueFilter("Id", docId)).Rows.Count, "doc table");
        Assert.AreEqual(0, con.FillSelect("SD11", null, new ValueFilter("DocId", docId)).Rows.Count, "subdoc table");
      }
    }

    [Test]
    public void Delete_UseDeleted([Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      // Проверка удаления для базы данных при наличии поля Deleted

      TestDBInfo info = this[true, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);
      DeleteTestDoc(info, docId);

      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        DataTable tblDoc = con.FillSelect("D1", null, new ValueFilter("Id", docId));
        Assert.AreEqual(1, tblDoc.Rows.Count, "doc table");
        Assert.IsTrue(DataTools.GetBool(tblDoc.Rows[0], "Deleted"), "Deleted");
        Assert.AreEqual(1, con.FillSelect("SD11", null, new ValueFilter("DocId", docId)).Rows.Count, "subdoc table");
        // У поддокументов поле Deleted не устанавливается в True.
      }
    }

    [Test]
    public void Restore([Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[true, useVersions, useTime];
      Int32 docId = CreateTestDoc(info);
      DeleteTestDoc(info, docId);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);
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
      ds["D1"].View(new Int32[] { docId1, docId2 });

      Assert.AreEqual(docId1, ds["D1"][0].DocId, "DocId #1");
      Assert.AreEqual(docId2, ds["D1"][1].DocId, "DocId #2");

      IDBxDocValues grpVals = ds["D1"].Values;
      Assert.AreEqual(2, grpVals.DocCount, "DocCount #1,2");
      Assert.IsTrue(grpVals.IsReadOnly, "IsReadOnly #1,2");
      Assert.IsFalse(grpVals["F101"].Grayed, "Grayed[F101] #1,2");
      Assert.IsTrue(grpVals["F101"].AsBoolean, "Value[F101] #1,2");
      Assert.IsTrue(grpVals["F102"].Grayed, "Grayed[F102] #1,2");

      // Открываем третий документ
      ds["D1"].View(docId3);
      grpVals = ds["D1"].Values;
      Assert.AreEqual(3, grpVals.DocCount, "DocCount #1,2,3");
      Assert.AreEqual(docId3, ds["D1"][2].DocId, "DocId #2");

      Assert.IsTrue(grpVals.IsReadOnly, "IsReadOnly #1,2,3");
      Assert.IsTrue(grpVals["F101"].Grayed, "Grayed[F101] #1,2,3");
      Assert.IsTrue(grpVals["F102"].Grayed, "Grayed[F102] #1,2,3");

      Assert.AreEqual(1, ds["D1"][0].Values["F102"].AsInteger, "Value[F102] #1");
      Assert.AreEqual(2, ds["D1"][1].Values["F102"].AsInteger, "Value[F102] #2");
      Assert.AreEqual(3, ds["D1"][2].Values["F102"].AsInteger, "Value[F102] #3");
    }

    [Test]
    public void Group_EditDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info, true, 1, "ABC");
      Int32 docId2 = CreateTestDoc(info, false, 2, "DEF");
      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Edit(new Int32[] { docId1, docId2 });
      IDBxDocValues grpVals = ds["D1"].Values;
      Assert.AreEqual(2, grpVals.DocCount, "DocCount #1,2");
      Assert.IsFalse(grpVals.IsReadOnly, "IsReadOnly #1,2");

      grpVals["F101"].SetBoolean(false);
      ds["D1"][0].Values["F102"].SetInteger(3);
      ds["D1"][1].Values["F102"].SetInteger(4);


      Assert.IsFalse(grpVals["F101"].Grayed, "Grayed[F101] #1,2");
      Assert.IsTrue(grpVals["F102"].Grayed, "Grayed[F102] #1,2");
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
      DBxSingleDoc doc = ds["D1"].Insert();
      DBxSubDoc sd = doc.SubDocs["SD11"].Insert();
      sd.Values["F111"].SetString("ABC");
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
      DBxSingleDoc doc = ds["D1"].Edit(docId);

      DBxSubDoc sd = doc.SubDocs["SD11"].Insert();
      sd.Values["F111"].SetString("DEF");
      sd.Delete();

      ds.ApplyChanges(false);
      AssertTestDoc(info, docId, "", true, 1, "ABC");
    }

    [TestCase(DBxDocState.View)]
    [TestCase(DBxDocState.Delete)]
    public void DeleteSubDoc_FailByDocState(DBxDocState docState)
    {
      TestDBInfo info = this[false, false, false];
      Int32 docId = CreateTestDoc(info, true, 1, "ABC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].View(docId);
      if (docState == DBxDocState.Delete)
        doc.Delete();

      Assert.Catch(delegate() { doc.SubDocs["SD11"].Delete(); });
    }

    #endregion

    #region Редактирование поддокументов

    [Test]
    public void Edit_AddSubDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info, true, 1, "ABC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);

      DBxSubDoc sd = doc.SubDocs["SD11"].Insert();
      sd.Values["F111"].SetString("DEF");

      ds.ApplyChanges(false);
      AssertTestDoc(info, docId, "", true, 1, "ABC", "DEF");
    }

    [Test]
    public void Edit_DeleteSubDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info, true, 1, "ABC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);

      doc.SubDocs["SD11"][0].Delete();

      ds.ApplyChanges(false);
      AssertTestDoc(info, docId, "", true, 1);
    }


    [Test]
    public void Edit_ChangeSubDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info, true, 1, "ABC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);

      doc.SubDocs["SD11"][0].Values["F111"].SetString("DEF");

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
      DBxSingleDoc doc1 = ds["D1"].View(docId1);
      Assert.AreEqual(2, doc1.SubDocs["SD11"].SubDocCount, "SubDocCount #1");
      IDBxDocValues grpVals = ds["D1"].SubDocs["SD11"].Values;
      Assert.AreEqual(2, grpVals.DocCount, "Values.DocCount #1");
      Assert.IsTrue(grpVals["F111"].Grayed, "Grayed #1");

      // Открываем еще один документ
      DBxSingleDoc doc2 = ds["D1"].View(docId2);
      Assert.AreEqual(1, doc2.SubDocs["SD11"].SubDocCount, "SubDocCount #2");
      grpVals = ds["D1"].SubDocs["SD11"].Values;
      Assert.AreEqual(3, grpVals.DocCount, "Values.DocCount #1,2");
      Assert.IsTrue(grpVals["F111"].Grayed, "Grayed #1,2");
    }

    [Test]
    public void Group_EditSubDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info, true, 1, "ABC", "DEF");
      Int32 docId2 = CreateTestDoc(info, true, 2, "GHI");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["D1"].Edit(docId1);
      DBxSingleDoc doc2 = ds["D1"].Edit(docId2);
      IDBxDocValues grpVals = ds["D1"].SubDocs["SD11"].Values;
      grpVals["F111"].SetString("XXX");
      Assert.IsFalse(grpVals["F111"].Grayed, "Grayed #1");

      doc1.SubDocs["SD11"][1].Values["F111"].SetString("YYY");
      Assert.IsTrue(grpVals["F111"].Grayed, "Grayed #2");
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
      ds["D1"].Edit(new Int32[] { docId1, docId2 });
      DBxMultiSubDocs sds = ds[0].SubDocs["SD11"];
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
      DBxSingleDoc doc = ds["D1"].Edit(docId);
      doc.Values["F102"].SetInteger(2);
      DBxSubDoc sd = doc.SubDocs["SD11"].Insert();
      sd.Values["F111"].SetString("DEF");
      ds.ApplyChanges(false);

      // Убрать в DBxDocProviderTests
      //DataTable tbl=ds.DocProvider.GetDocHistTable("D1", docId);

      ds = new DBxDocSet(info.Provider);
      doc = ds["D1"].ViewVersion(docId, 1);
      Assert.IsTrue(ds.VersionView, "VersionView #1");
      Assert.AreEqual(1, doc.Version, "Version #1");
      AssertTestDoc(doc, "#1", true, 1, "ABC");

      ds = new DBxDocSet(info.Provider);
      doc = ds["D1"].ViewVersion(docId, 2);
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
      ds["D1"].View(docId1);

      Assert.Catch(delegate() { ds["D1"].ViewVersion(docId2, 1); });
    }
#endif

    #endregion

    #region Тестирование прочих свойств

#if XXX // Свойство убрано
    [Test]
    [Ignore("EditIfNotChanged does not work")]
    public void EditIfNotChanged([Values(false, true)] bool useDeleted, [Values(false, true)] bool useTime,
      [Values(false, true)] bool propValue)
    {
      TestDBInfo info = this[useDeleted, true, useTime]; // версии нужно сохранять, иначе не проверить
      Int32 docId = CreateTestDoc(info, true, 1, "ABC", "DEF");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds.EditIfNotChanged = propValue;
      DBxSingleDoc doc = ds["D1"].Edit(docId);
      Assert.IsFalse(doc.IsDataModified, "IsDataModified");

      ds.ApplyChanges(false);

      ds = new DBxDocSet(info.Provider);
      doc = ds["D1"].View(docId);
      Assert.AreEqual(propValue ? 2 : 1, doc.Version, "Version");
    }
#endif
    
    [Test]
    public void DocCount_and_DocState()
    {
      TestDBInfo info = this[false, false, false];
      Int32 docId1 = CreateTestDoc(info, true, 1);
      Int32 docId2 = CreateTestDoc(info, true, 2);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].View(docId1);
      ds["D1"].Edit(docId2);

      Assert.AreEqual(2, ds.DocCount, "DocCount #1");
      Assert.AreEqual(DBxDocState.Mixed, ds.DocState, "DocState #1");
      Assert.AreEqual(DBxDocState.Edit, ds.DocStateNoView, "DocStateNoView #1");

      Assert.AreEqual(1, ds.GetDocCount(DBxDocState.View), "GetDocCount(View) #1");
      Assert.AreEqual(1, ds.GetDocCount(DBxDocState.Edit), "GetDocCount(Edit) #1");
      Assert.AreEqual(0, ds.GetDocCount(DBxDocState.Insert), "GetDocCount(Insert) #1");
      Assert.AreEqual(0, ds.GetDocCount(DBxDocState.Delete), "GetDocCount(Delete) #1");

      ds["D1"].Insert();
      Assert.AreEqual(3, ds.DocCount, "DocCount #2");
      Assert.AreEqual(DBxDocState.Mixed, ds.DocStateNoView, "DocStateNoView #2");
      Assert.AreEqual(1, ds.GetDocCount(DBxDocState.Insert), "GetDocCount(Insert) #2");

      ds["D1"][1].Delete();
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
      ds["D1"].View(docId1);
      ds["D1"].Edit(docId2);
      ds["D1"].Insert();

      DBxDocSelection docSel = ds.DocSelection;
      Assert.AreEqual(2, docSel.TotalCount, "TotalCount #1");
      Assert.IsTrue(docSel.ContainsAll("D1", new Int32[] { docId1, docId2 }), "DocIds #1");

      docSel = ds.GetDocSelection(DBxDocState.View);
      Assert.AreEqual(1, docSel.TotalCount, "TotalCount #2");
      Assert.AreEqual(docId1, docSel["D1"][0], "DocIds #2");

      docSel = ds.GetDocSelection(DBxDocState.Edit);
      Assert.AreEqual(1, docSel.TotalCount, "TotalCount #2");
      Assert.AreEqual(docId2, docSel["D1"][0], "DocIds #2");

      docSel = ds.GetDocSelection(DBxDocState.Insert);
      Assert.AreEqual(0, docSel.TotalCount, "TotalCount #3");

      docSel = ds.GetDocSelection(DBxDocState.Delete);
      Assert.AreEqual(0, docSel.TotalCount, "TotalCount #4");

      ds["D1"][1].Delete();

      docSel = ds.GetDocSelection(DBxDocState.Edit);
      Assert.AreEqual(0, docSel.TotalCount, "TotalCount #5");

      docSel = ds.GetDocSelection(DBxDocState.Delete);
      Assert.AreEqual(1, docSel.TotalCount, "TotalCount #6");
      Assert.AreEqual(docId2, docSel["D1"][0], "DocIds #6");
    }

    [Test]
    public void AllowDBNull()
    {
      TestDBInfo info = this[false, false, false];
      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Insert();
      DBxSubDoc sd = doc.SubDocs["SD11"].Insert();

      // В DBxDocSet имеется 4 класса реализующих доступ к значениям
      IDBxDocValues docVals1 = ds["D1"].Values;
      IDBxDocValues docVals2 = doc.Values;
      IDBxDocValues sdVals1 = ds["D1"].SubDocs["SD11"].Values;
      IDBxDocValues sdVals2 = sd.Values;

      Assert.IsFalse(docVals1["F102"].AllowDBNull, "F102 AllowDBNull #1");
      Assert.IsFalse(docVals2["F102"].AllowDBNull, "F102 AllowDBNull #2");
      Assert.IsTrue(docVals1["F103"].AllowDBNull, "F103 AllowDBNull #1");
      Assert.IsTrue(docVals2["F103"].AllowDBNull, "F103 AllowDBNull #2");

      Assert.IsFalse(sdVals1["F111"].AllowDBNull, "F111 AllowDBNull #1");
      Assert.IsFalse(sdVals2["F111"].AllowDBNull, "F111 AllowDBNull #2");
      Assert.IsTrue(sdVals1["F112"].AllowDBNull, "F112 AllowDBNull #1");
      Assert.IsTrue(sdVals2["F112"].AllowDBNull, "F112 AllowDBNull #2");
    }

    [Test]
    public void MaxLength()
    {
      TestDBInfo info = this[false, false, false];
      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Insert();
      DBxSubDoc sd = doc.SubDocs["SD11"].Insert();

      IDBxDocValues sdVals1 = ds["D1"].SubDocs["SD11"].Values;
      IDBxDocValues sdVals2 = sd.Values;


      Assert.AreEqual(3, sdVals1["F111"].MaxLength, "F111 MaxLength #1");
      Assert.AreEqual(3, sdVals2["F111"].MaxLength, "F111 MaxLength #2");
      Assert.AreEqual(10, sdVals1["F112"].MaxLength, "F111 MaxLength #1");
      Assert.AreEqual(10, sdVals2["F112"].MaxLength, "F111 MaxLength #2");
    }

    #endregion

    #region Ссылки между документами

    #region Дерево документов

    [Test]
    public void DocTree_Insert_Simple([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info, true, 1);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc2 = ds["D1"].Insert();
      doc2.Values["F101"].SetBoolean(false);
      doc2.Values["F102"].SetInteger(0);
      doc2.Values["F104"].SetInteger(docId1);
      ds.ApplyChanges(true);
      doc2 = ds["D1"][0];

      Assert.AreEqual(docId1, doc2.Values["F104"].AsInteger, "Doc2");
      AssertTestDoc(info, doc2.DocId, "Doc #2", false, 0);
    }

    [Test]
    public void DocTree_Insert_TwoDocs([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["D1"].Insert();
      doc1.Values["F101"].SetBoolean(false);
      doc1.Values["F102"].SetInteger(1);

      DBxSingleDoc doc2 = ds["D1"].Insert();
      doc2.Values["F101"].SetBoolean(false);
      doc2.Values["F102"].SetInteger(2);
      doc2.Values["F104"].SetInteger(doc1.DocId); // фиктивный идентификатор

      ds.ApplyChanges(true);
      doc1 = ds["D1"][0];
      doc2 = ds["D1"][1];

      Assert.AreEqual(doc1.DocId, doc2.Values["F104"].AsInteger);
      AssertTestDoc(info, doc1.DocId, "Doc #1", false, 1);
      AssertTestDoc(info, doc2.DocId, "Doc #2", false, 2);
    }

    [Test]
    public void DocTree_Delete_Success_leaves([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Delete(docIds[1]);
      ds["D1"].Delete(docIds[2]);
      ds.ApplyChanges(false);
      AssertTestDocDeleted(info, docIds[1], "#1");
      AssertTestDocDeleted(info, docIds[2], "#2");
    }

    [Test]
    public void DocTree_Delete_Success_wholetree([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime,
      [Values(false, true)] bool reverseOrder)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);

      // Удаляем документы в произвольном порядке
      if (reverseOrder)
        Array.Reverse(docIds);
      for (int i = 0; i < docIds.Length; i++)
        ds["D1"].Delete(docIds[i]);

      ds.ApplyChanges(false);
      AssertTestDocDeleted(info, docIds[0], "#1");
      AssertTestDocDeleted(info, docIds[1], "#2");
      AssertTestDocDeleted(info, docIds[2], "#3");
    }

    [Test]
    public void DocTree_Delete_Fail_1([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Delete(docIds[0]);
      // Нельзя удалить узел 1, но оставить узлы 2 и 3
      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    [Test]
    public void DocTree_Delete_Fail_2([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime,
      [Values(false, true)] bool reverseOrder)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);

      Int32[] docIds2 = new Int32[2];
      docIds2[0] = docIds[0];
      docIds2[1] = docIds[1];
      if (reverseOrder)
        Array.Reverse(docIds2);

      for (int i = 0; i < docIds2.Length; i++)
        ds["D1"].Delete(docIds[i]);

      // Нельзя удалить узел 1, но оставить узел 3
      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    /// <summary>
    /// Создает тестовое дерево документов "D1". Первый документ является корнем, а остальные два - дочерними для него
    /// </summary>
    /// <param name="info">Тестовая база данных</param>
    /// <returns>Массив из трех идентификаторов документов</returns>
    private Int32[] CreateTestDocTree(TestDBInfo info)
    {
      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["D1"].Insert();
      doc1.Values["F101"].SetBoolean(false);
      doc1.Values["F102"].SetInteger(1);

      DBxSingleDoc doc2 = ds["D1"].Insert();
      doc2.Values["F101"].SetBoolean(false);
      doc2.Values["F102"].SetInteger(2);
      doc2.Values["F104"].SetInteger(doc1.DocId);

      DBxSingleDoc doc3 = ds["D1"].Insert();
      doc3.Values["F101"].SetBoolean(false);
      doc3.Values["F102"].SetInteger(3);
      doc3.Values["F104"].SetInteger(doc1.DocId);

      ds.ApplyChanges(true);

      Int32[] docIds = new Int32[3];
      docIds[0] = ds["D1"][0].DocId;
      docIds[1] = ds["D1"][1].DocId;
      docIds[2] = ds["D1"][2].DocId;
      return docIds;
    }

    #endregion

    #region Дерево поддокументов

    [Test]
    public void SubDocTree_Insert([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Insert();
      doc.Values["F101"].SetBoolean(false);
      doc.Values["F102"].SetInteger(0);

      DBxSubDoc sdoc1 = doc.SubDocs["SD11"].Insert();
      sdoc1.Values["F111"].SetString("A");

      DBxSubDoc sdoc2 = doc.SubDocs["SD11"].Insert();
      sdoc2.Values["F111"].SetString("B");
      sdoc2.Values["F113"].SetInteger(sdoc1.SubDocId);

      DBxSubDoc sdoc3 = doc.SubDocs["SD11"].Insert();
      sdoc3.Values["F111"].SetString("C");
      sdoc3.Values["F113"].SetInteger(sdoc1.SubDocId);

      ds.ApplyChanges(true);
      doc = ds["D1"][0];
      Assert.AreEqual(3, doc.SubDocs["SD11"].SubDocCount, "SubDocCount");
      sdoc1 = doc.SubDocs["SD11"][0];
      sdoc2 = doc.SubDocs["SD11"][1];
      sdoc3 = doc.SubDocs["SD11"][2];

      Assert.AreEqual(0, sdoc1.Values["F113"].AsInteger, "SubDoc1");
      Assert.AreEqual(sdoc1.SubDocId, sdoc2.Values["F113"].AsInteger, "SubDoc2");
      Assert.AreEqual(sdoc1.SubDocId, sdoc3.Values["F113"].AsInteger, "SubDoc3");
    }

    [Test]
    public void SubDocTree_Delete_Success_leaves([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      Int32 docId = CreateTestSubDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);

      doc.SubDocs["SD11"][1].Delete();
      doc.SubDocs["SD11"][2].Delete();

      ds.ApplyChanges(false);

      ds = new DBxDocSet(info.Provider);
      doc = ds["D1"].View(docId);
      Assert.AreEqual(1, doc.SubDocs["SD11"].SubDocCount);
    }


    [Test]
    public void SubDocTree_Delete_Success_wholetree([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      Int32 docId = CreateTestSubDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);

      // Тут бесполезно тестировать reverseOrder, т.к. порядок документов в таблице от этого не поменяется

      doc.SubDocs["SD11"][0].Delete();
      doc.SubDocs["SD11"][1].Delete();
      doc.SubDocs["SD11"][2].Delete();

      ds.ApplyChanges(false);

      ds = new DBxDocSet(info.Provider);
      doc = ds["D1"].View(docId);
      Assert.AreEqual(0, doc.SubDocs["SD11"].SubDocCount);
    }


    [Test]
    public void SubDocTree_Delete_Fail([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      Int32 docId = CreateTestSubDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);

      // Нельзя удалить корень дерева, оставив листья
      doc.SubDocs["SD11"][0].Delete();

      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    /// <summary>
    /// Создает тестовый документ "D1" с тремя документами "SD11", образующих простое дерево.
    /// Первый поддокумент является корнем, второй и третий - дочерними от первого
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private Int32 CreateTestSubDocTree(TestDBInfo info)
    {
      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Insert();
      doc.Values["F101"].SetBoolean(false);
      doc.Values["F102"].SetInteger(0);

      DBxSubDoc sdoc1 = doc.SubDocs["SD11"].Insert();
      sdoc1.Values["F111"].SetString("A");

      DBxSubDoc sdoc2 = doc.SubDocs["SD11"].Insert();
      sdoc2.Values["F111"].SetString("B");
      sdoc2.Values["F113"].SetInteger(sdoc1.SubDocId);

      DBxSubDoc sdoc3 = doc.SubDocs["SD11"].Insert();
      sdoc3.Values["F111"].SetString("C");
      sdoc3.Values["F113"].SetInteger(sdoc1.SubDocId);

      ds.ApplyChanges(true);
      doc = ds["D1"][0];

      return doc.DocId;
    }

    #endregion

    #region Взаимные ссылки между документами

    [Test]
    public void DocCrossRef_Insert([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      Int32 docId1, docId2;
      CreateTestCrossRefDoc(info, out docId1, out docId2);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["D1"].View(docId1);
      DBxSingleDoc doc2 = ds["D2"].View(docId2);

      Assert.AreEqual(doc2.DocId, doc1.Values["F105"].AsInteger, "#1");
      Assert.AreEqual(doc1.DocId, doc2.Values["F202"].AsInteger, "#2");
    }


    [Test]
    public void DocCrossRef_Delete_Success([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      Int32 docId1, docId2;
      CreateTestCrossRefDoc(info, out docId1, out docId2);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Delete(docId1);
      ds["D2"].Delete(docId2);

      ds.ApplyChanges(false);
    }

    private void CreateTestCrossRefDoc(TestDBInfo info, out int docId1, out int docId2)
    {
      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["D1"].Insert();
      doc1.Values["F101"].SetBoolean(false);
      doc1.Values["F102"].SetInteger(0);

      DBxSingleDoc doc2 = ds["D2"].Insert();
      doc2.Values["F201"].SetString("B");

      doc1.Values["F105"].SetInteger(doc2.DocId); // фиктивный идентификатор
      doc2.Values["F202"].SetInteger(doc1.DocId); // фиктивный идентификатор

      ds.ApplyChanges(true);
      doc1 = ds["D1"][0];
      doc2 = ds["D2"][0];

      Assert.AreEqual(doc2.DocId, doc1.Values["F105"].AsInteger, "CrossRef #1");
      Assert.AreEqual(doc1.DocId, doc2.Values["F202"].AsInteger, "CrossRef #2");

      docId1 = doc1.DocId;
      docId2 = doc2.DocId;
    }



    #endregion

    #region Ссылки на поддокументы

    [Test]
    public void SubDocRef_Insert([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      Int32 docId1, docId2;
      CreateSubDocRefTest(info, out docId1, out docId2);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["D1"].View(docId1);
      DBxSingleDoc doc2 = ds["D2"].View(docId2);
      DBxSubDoc sdoc21 = doc2.SubDocs["SD21"][0];

      Assert.AreEqual(docId2, doc1.Values["F105"].AsInteger, "F105");
      Assert.AreEqual(sdoc21.SubDocId, doc1.Values["F106"].AsInteger, "F106");
    }

    [Test]
    public void SubDocRef_DeleteSubDoc_Success([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      Int32 docId1, docId2;
      CreateSubDocRefTest(info, out docId1, out docId2);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["D1"].Edit(docId1);
      DBxSingleDoc doc2 = ds["D2"].Edit(docId2);
      DBxSubDoc sdoc21 = doc2.SubDocs["SD21"][0];

      doc1.Values["F106"].SetNull();
      sdoc21.Delete();

      ds.ApplyChanges(false);
    }

    [Test]
    public void SubDocRef_DeleteSubDoc_Fail([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      Int32 docId1, docId2;
      CreateSubDocRefTest(info, out docId1, out docId2);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc2 = ds["D2"].Edit(docId2);
      DBxSubDoc sdoc21 = doc2.SubDocs["SD21"][0];

      sdoc21.Delete();

      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    [Test]
    public void SubDocRef_DeleteWholeDoc_Success([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      Int32 docId1, docId2;
      CreateSubDocRefTest(info, out docId1, out docId2);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["D1"].Edit(docId1);

      doc1.Values["F105"].SetNull();
      doc1.Values["F106"].SetNull();
      ds["D2"].Delete(docId2);

      ds.ApplyChanges(false);
    }


    [Test]
    public void SubDocRef_DeleteWholeDoc_Fail([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      Int32 docId1, docId2;
      CreateSubDocRefTest(info, out docId1, out docId2);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["D1"].Edit(docId1);

      doc1.Values["F105"].SetNull();
      // Ссылка F106 на поддокумент осталась
      ds["D2"].Delete(docId2);

      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    private void CreateSubDocRefTest(TestDBInfo info, out int docId1, out int docId2)
    {
      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["D1"].Insert();
      doc1.Values["F101"].SetBoolean(false);
      doc1.Values["F102"].SetInteger(0);

      DBxSingleDoc doc2 = ds["D2"].Insert();
      doc2.Values["F201"].SetString("AAA");
      DBxSubDoc sdoc21 = doc2.SubDocs["SD21"].Insert();

      doc1.Values["F105"].SetInteger(doc2.DocId);
      doc1.Values["F106"].SetInteger(sdoc21.SubDocId);

      ds.ApplyChanges(true);
      docId1 = ds["D1"][0].DocId;
      docId2 = ds["D2"][0].DocId;
    }

    #endregion

    #endregion

    #region Вспомогательные методы

    private Int32 CreateTestDoc(TestDBInfo info)
    {
      return CreateTestDoc(info, true, 2, "ABC");
    }

    private Int32 CreateTestDoc(TestDBInfo info, bool F101, int F102, params string[] F111)
    {
      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Insert();
      doc.Values["F101"].SetBoolean(F101);
      doc.Values["F102"].SetInteger(F102);
      for (int i = 0; i < F111.Length; i++)
      {
        DBxSubDoc sd = doc.SubDocs["SD11"].Insert();
        sd.Values["F111"].SetString(F111[i]);
      }
      ds.ApplyChanges(true);
      Int32 docId = ds[0][0].DocId;
      Assert.Greater(docId, 0, "DocId");
      return docId;
    }

    private static void DeleteTestDoc(TestDBInfo info, Int32 docId)
    {
      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Delete(docId);
      ds.ApplyChanges(true);
    }

    /// <summary>
    /// Тестирование документа в базе данных
    /// </summary>
    /// <param name="info"></param>
    /// <param name="docId"></param>
    /// <param name="message"></param>
    /// <param name="F101"></param>
    /// <param name="F102"></param>
    /// <param name="F111"></param>
    private void AssertTestDoc(TestDBInfo info, Int32 docId, string message, bool F101, int F102, params string[] F111)
    {
      info.Provider.CheckIsRealDocId(docId);

      if (!String.IsNullOrEmpty(message))
        message += ". ";

      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        DataTable tblDoc = con.FillSelect("D1", null, new ValueFilter("Id", docId));
        Assert.AreEqual(1, tblDoc.Rows.Count, message + "doc table count");
        if (info.GlobalData.DocTypes.UseDeleted)
          Assert.IsFalse(DataTools.GetBool(tblDoc.Rows[0], "Deleted"), message + "Deleted");
        Assert.AreEqual(F101, DataTools.GetBool(tblDoc.Rows[0], "F101"), message + "F101");
        Assert.AreEqual(F102, DataTools.GetInt(tblDoc.Rows[0], "F102"), message + "F102");

        Array.Sort<string>(F111);
        DBxFilter filter = new ValueFilter("DocId", docId);
        if (info.GlobalData.DocTypes.UseDeleted)
          filter = new AndFilter(filter, new ValueFilter("Deleted", false));
        DataTable tblSubDoc = con.FillSelect("SD11", null, filter);
        string[] a = new string[tblSubDoc.Rows.Count];
        for (int i = 0; i < a.Length; i++)
          a[i] = DataTools.GetString(tblSubDoc.Rows[i], "F111");
        Array.Sort<string>(a);
        Assert.AreEqual(F111, a, message + "F111");
        // У поддокументов поле Deleted не устанавливается в True.
      }
    }

    /// <summary>
    /// Тестирование загруженного документа
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="message"></param>
    /// <param name="F101"></param>
    /// <param name="F102"></param>
    /// <param name="F111"></param>
    private void AssertTestDoc(DBxSingleDoc doc, string message, bool F101, int F102, params string[] F111)
    {
      if (!String.IsNullOrEmpty(message))
        message += ". ";

      Assert.AreEqual(F101, doc.Values["F101"].AsBoolean, message + "F101");
      Assert.AreEqual(F102, doc.Values["F102"].AsInteger, message + "F102");

      Array.Sort<string>(F111);
      string[] a = new string[doc.SubDocs["SD11"].SubDocCount];
      for (int i = 0; i < a.Length; i++)
      {
        DBxSubDoc sd = doc.SubDocs["SD11"][i];
        a[i] = sd.Values["F111"].AsString;
      }
      Array.Sort<string>(a);
      Assert.AreEqual(F111, a, message + "F111");
    }

    private void AssertTestDocDeleted(TestDBInfo info, Int32 docId, string message)
    {
      info.Provider.CheckIsRealDocId(docId);

      if (!String.IsNullOrEmpty(message))
        message += ". ";

      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        DataTable tblDoc = con.FillSelect("D1", null, new ValueFilter("Id", docId));
        if (info.GlobalData.DocTypes.UseDeleted)
        {
          Assert.AreEqual(1, tblDoc.Rows.Count, message + "doc table count must be 1");
          if (DataTools.GetInt(tblDoc.Rows[0], "Id") != docId)
            throw new BugException("Ошибка SELECT");

          Assert.IsTrue(DataTools.GetBool(tblDoc.Rows[0], "Deleted"), message + "Field \"Deleted\" has not been set to true");
        }
        else
        {
          Assert.AreEqual(0, tblDoc.Rows.Count, message + "doc table count must be 0");
          DataTable tblSubDoc = con.FillSelect("SD11", null, new ValueFilter("DocId", docId));
          Assert.AreEqual(0, tblSubDoc.Rows.Count, message + "subdoc table count must be 0");
        }
      }
    }

    #endregion
  }
}
