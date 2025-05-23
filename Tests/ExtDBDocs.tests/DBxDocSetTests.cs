﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Data.Docs;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Tests;

namespace ExtDBDocs_tests.Data_Docs
{
  public class DBxDocSetTestsBase: FixtureWithSetUp
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
     *         "F107" - Числовое Nullable. Вычисляемое поле, равно "F102" * 2
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
     * Документ "D3"
     *   Поля: "F301TableId", "F301DocId" - переменная ссылка. Допускаются ссылки на документы "D1" и "D2"
     * Документ "Users" - только при UseUsers=true
     *   Поля:  "Name" CHAR(?) Not Null
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
    /// Получить доступ к базе данных с заданной комбинацией управляющих параметров
    /// </summary>
    /// <param name="useDeleted"></param>
    /// <param name="useVersions"></param>
    /// <param name="useTime"></param>
    /// <returns></returns>
    public TestDBInfo this[bool useDeleted, bool useVersions, bool useTime]
    {
      get
      {
        return this[useDeleted, useVersions, useTime, false];
      }
    }
    /// <summary>
    /// Получить доступ к базе данных с заданной комбинацией управляющих параметров
    /// </summary>
    /// <param name="useDeleted"></param>
    /// <param name="useVersions"></param>
    /// <param name="useTime"></param>
    /// <returns></returns>
    public TestDBInfo this[bool useDeleted, bool useVersions, bool useTime, bool useUsers]
    {
      get
      {
        if (_TestDBs == null)
          _TestDBs = new TestDBInfo[16];

        int index = (useDeleted ? 8 : 0) + (useVersions ? 4 : 0) + (useTime ? 2 : 0) + (useUsers ? 1 : 0);
        if (_TestDBs[index] == null)
        {
          TestDBInfo info = new TestDBInfo();
          DBxDocTypes dts = new DBxDocTypes();
          if (!useUsers)
            dts.UsersTableName = String.Empty; // без пользователей
          Assert.AreEqual(useUsers, dts.UseUsers, "DBxDocProvider.UseUsers set");
          dts.UseDeleted = useDeleted;
          dts.UseVersions = useVersions;
          dts.UseTime = useTime;
          DBxDocType dt;
          DBxSubDocType sdt;

          dt = new DBxDocType("D1");
          dt.Struct.Columns.AddBoolean("F101");
          dt.Struct.Columns.AddInt("F102", false);
          dt.Struct.Columns.AddInt("F103", true);
          dt.Struct.Columns.AddReference("F104", "D1", true);
          dt.Struct.Columns.AddReference("F105", "D2", true);
          dt.Struct.Columns.AddReference("F106", "SD21", true);
          dt.Struct.Columns.AddInt("F107", true);
          dt.CalculatedColumns.Add("F107");
          dt.BeforeWrite += dt1_BeforeWrite;
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

          dt = new DBxDocType("D3");
          DBxVTReference vtr = dt.VTRefs.Add("F301");
          vtr.MasterTableNames.Add("D1");
          vtr.MasterTableNames.Add("D2");
          dts.Add(dt);

          DBxDocDBConnectionHelper conHelper = new DBxDocDBConnectionHelper();
          conHelper.ProviderName = "SQLite";
          conHelper.ConnectionString = "Data Source=:memory:";
          conHelper.DocTypes = dts;

          info.GlobalData = conHelper.CreateRealDocProviderGlobal();

          info.Source = new DBxRealDocProviderSource(info.GlobalData);
          info.Provider = new DBxRealDocProvider(info.Source, useUsers ? 1 : 0, false);

          // Создаем пользователя
          if (useUsers)
          {
            DBxDocSet ds = new DBxDocSet(info.Provider);
            DBxSingleDoc doc = ds["Users"].Insert();
            doc.Values["Name"].SetString("Test user");
            ds.ApplyChanges(false);
          }

          _TestDBs[index] = info;
        }
        return _TestDBs[index];
      }
    }

    private static void dt1_BeforeWrite(object sender, ServerDocTypeBeforeWriteEventArgs args)
    {
      args.Doc.Values["F107"].SetInteger(args.Doc.Values["F102"].AsInteger * 2);
    }

    protected override void OnOneTimeTearDown()
    {
      if (_TestDBs == null)
        return;
      for (int i = 0; i < _TestDBs.Length; i++)
      {
        if (_TestDBs[i] != null)
          _TestDBs[i].GlobalData.DisposeDBs();
      }
      _TestDBs = null;
      base.OnOneTimeTearDown();
    }

    #endregion

    #region Вспомогательные методы

    protected static Int32 CreateTestDoc(TestDBInfo info)
    {
      return CreateTestDoc(info, true, 2, "ABC");
    }

    protected static Int32 CreateTestDoc(TestDBInfo info, bool F101, int F102, params string[] F111)
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

    protected static void DeleteTestDoc(TestDBInfo info, Int32 docId)
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
    protected static void AssertTestDoc(TestDBInfo info, Int32 docId, string message, bool F101, int F102, params string[] F111)
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
    protected static void AssertTestDoc(DBxSingleDoc doc, string message, bool F101, int F102, params string[] F111)
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

    protected static void AssertTestDocDeleted(TestDBInfo info, Int32 docId, string message)
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

  /// <summary>
  /// Проверка основных возможностей по работе с документами.
  /// Используется несколько вариантов конфигурации DBxDocTypes с использованием флагов
  /// UseDeleted, UseVersions, UseTime. Для каждой комбинации создается собственная тестовая база данных
  /// </summary>
  [TestFixture]
  public class DBxDocSetTests : DBxDocSetTestsBase
  {
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

    [Test]
    public void InsertCopy([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].View(docId1);

      ds.InsertCopy();
      Assert.AreEqual(DBxDocState.Insert, ds.DocState, "DocState");
      Assert.AreEqual(1, ds.DocCount, "DocCount");

      doc.Values["F102"].SetInteger(10);

      ds.ApplyChanges(true);
      doc = ds["D1"][0];
      Int32 docId2 = doc.DocId;
      Assert.AreNotEqual(docId1, docId2, "DocId");

      AssertTestDoc(info, docId1, "#1", true, 2, "ABC");
      AssertTestDoc(info, docId2, "#2", true, 10, "ABC");
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

      IDBxExtValues grpVals = ds["D1"].Values;
      Assert.AreEqual(2, grpVals.RowCount, "RowCount #1,2");
      Assert.IsTrue(grpVals.IsReadOnly, "IsReadOnly #1,2");
      Assert.IsFalse(grpVals["F101"].Grayed, "Grayed[F101] #1,2");
      Assert.IsTrue(grpVals["F101"].AsBoolean, "Value[F101] #1,2");
      Assert.IsTrue(grpVals["F102"].Grayed, "Grayed[F102] #1,2");

      // Открываем третий документ
      ds["D1"].View(docId3);
      grpVals = ds["D1"].Values;
      Assert.AreEqual(3, grpVals.RowCount, "RowCount #1,2,3");
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
      IDBxExtValues grpVals = ds["D1"].Values;
      Assert.AreEqual(2, grpVals.RowCount, "RowCount #1,2");
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
      IDBxExtValues grpVals = ds["D1"].SubDocs["SD11"].Values;
      Assert.AreEqual(2, grpVals.RowCount, "Values.RowCount #1");
      Assert.IsTrue(grpVals["F111"].Grayed, "Grayed #1");

      // Открываем еще один документ
      DBxSingleDoc doc2 = ds["D1"].View(docId2);
      Assert.AreEqual(1, doc2.SubDocs["SD11"].SubDocCount, "SubDocCount #2");
      grpVals = ds["D1"].SubDocs["SD11"].Values;
      Assert.AreEqual(3, grpVals.RowCount, "Values.RowCount #1,2");
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
      IDBxExtValues grpVals = ds["D1"].SubDocs["SD11"].Values;
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

    #region Тестирование прочих свойств и методов

    [Test]
    public void WriteIfNotChanged([Values(false, true)] bool useDeleted, [Values(false, true)] bool useTime,
      [Values(false, true)] bool propValue)
    {
      TestDBInfo info = this[useDeleted, true, useTime]; // версии нужно сохранять, иначе не проверить
      Int32 docId = CreateTestDoc(info, true, 1, "ABC", "DEF");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds.WriteIfNotChanged = propValue;
      DBxSingleDoc doc = ds["D1"].Edit(docId);
      Assert.IsFalse(doc.IsDataModified, "IsDataModified");

      ds.ApplyChanges(false);

      ds = new DBxDocSet(info.Provider);
      doc = ds["D1"].View(docId);
      Assert.AreEqual(propValue ? 2 : 1, doc.Version, "Version");
    }

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
      IDBxExtValues docVals1 = ds["D1"].Values;
      IDBxExtValues docVals2 = doc.Values;
      IDBxExtValues sdVals1 = ds["D1"].SubDocs["SD11"].Values;
      IDBxExtValues sdVals2 = sd.Values;

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

      IDBxExtValues sdVals1 = ds["D1"].SubDocs["SD11"].Values;
      IDBxExtValues sdVals2 = sd.Values;


      Assert.AreEqual(3, sdVals1["F111"].MaxLength, "F111 MaxLength #1");
      Assert.AreEqual(3, sdVals2["F111"].MaxLength, "F111 MaxLength #2");
      Assert.AreEqual(10, sdVals1["F112"].MaxLength, "F111 MaxLength #1");
      Assert.AreEqual(10, sdVals2["F112"].MaxLength, "F111 MaxLength #2");
    }

    [Test]
    public void Clear()
    {
      TestDBInfo info = this[true, true, true];
      Int32 docId1 = CreateTestDoc(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId1);
      doc.Values["F102"].SetInteger(10);
      ds.Clear();
      ds.ApplyChanges(false);

      AssertTestDoc(info, docId1, "Changes must be rejected", true, 2, "ABC");
    }

    [Test]
    public void ClearView()
    {
      TestDBInfo info = this[true, true, true];
      Int32 docId1 = CreateTestDoc(info, false, 1);
      Int32 docId2 = CreateTestDoc(info, false, 2);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["D1"].Edit(docId1);
      doc1.Values["F102"].SetInteger(10);
      DBxSingleDoc doc2 = ds["D1"].View(docId2);
      ds.ClearView();
      ds.ApplyChanges(false);

      AssertTestDoc(info, docId1, "#1", false, 10);
      AssertTestDoc(info, docId2, "#2", false, 2);
    }


    [Test]
    public void ChangeDocState()
    {
      TestDBInfo info = this[true, true, true];
      Int32 docId1 = CreateTestDoc(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId1);
      doc.Values["F102"].SetInteger(10);
      ds.ChangeDocState(DBxDocState.Edit, DBxDocState.Delete);
      ds.ApplyChanges(false);

      AssertTestDocDeleted(info, docId1, "");
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
    public void DocTree_Insert_Fail_FictiveDocId([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc2 = ds["D1"].Insert();
      doc2.Values["F101"].SetBoolean(false);
      doc2.Values["F102"].SetInteger(0);
      doc2.Values["F104"].SetInteger(-2);
      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    [Test]
    public void DocTree_Insert_Fail_UnknownDocId([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc2 = ds["D1"].Insert();
      doc2.Values["F101"].SetBoolean(false);
      doc2.Values["F102"].SetInteger(0);
      doc2.Values["F104"].SetInteger(100000);
      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    [Test]
    public void DocTree_Insert_TwoDocs_And_Delete_Fail([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);

      // Создаем первый документ
      DBxSingleDoc doc1 = ds["D1"].Insert();
      doc1.Values["F101"].SetBoolean(false);
      doc1.Values["F102"].SetInteger(0);

      // Создаем второй документ со ссылкой на первый
      DBxSingleDoc doc2 = ds["D1"].Insert();
      doc2.Values["F101"].SetBoolean(false);
      doc2.Values["F102"].SetInteger(0);
      doc2.Values["F104"].SetInteger(doc1.DocId);

      // Удаляем первый документ
      doc1.Delete();

      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    [Test]
    public void DocTree_Edit_Success([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Edit(docIds[2]);
      ds["D1"].Values["F104"].SetInteger(docIds[1]);
      Assert.DoesNotThrow(delegate() { ds.ApplyChanges(false); });
    }

    [Test]
    public void DocTree_Edit_Fail_FictiveDocId([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Edit(docIds[2]);
      ds["D1"].Values["F104"].SetInteger(-1);
      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    [Test]
    public void DocTree_Edit_Fail_UnknownDocId([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Edit(docIds[2]);
      ds["D1"].Values["F104"].SetInteger(100000);
      Assert.Catch(delegate() { ds.ApplyChanges(false); });
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


    [Test]
    public void DocTree_Edit_And_Delete_Success([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Edit(new Int32[2] { docIds[1], docIds[2] });
      ds["D1"].Values["F104"].SetNull(); // очистили все ссылки
      ds["D1"].Delete(docIds[0]); // На корневой документ больше нет ссылок
      Assert.DoesNotThrow(delegate() { ds.ApplyChanges(false); });
    }


    [Test]
    public void DocTree_Edit_And_Delete_Fail([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Edit(docIds[2]);
      ds["D1"].Values["F104"].SetInteger(docIds[1]); // Переключили ссылку. Теперь дерево: 1 <- 2 <- 3
      ds["D1"].Delete(docIds[1]); // Теперь на этот документ есть ссылка
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
    public void SubDocTree_Insert_Simple([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
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
    public void SubDocTree_Insert_FictiveSubDocId([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Insert();
      doc.Values["F101"].SetBoolean(false);
      doc.Values["F102"].SetInteger(0);

      DBxSubDoc sdoc = doc.SubDocs["SD11"].Insert();
      sdoc.Values["F111"].SetString("A");
      sdoc.Values["F113"].SetInteger(-2);

      Assert.Catch(delegate() { ds.ApplyChanges(true); });
    }

    [Test]
    public void SubDocTree_Insert_UnknownSubDocId([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Insert();
      doc.Values["F101"].SetBoolean(false);
      doc.Values["F102"].SetInteger(0);

      DBxSubDoc sdoc = doc.SubDocs["SD11"].Insert();
      sdoc.Values["F111"].SetString("A");
      sdoc.Values["F113"].SetInteger(100000);

      Assert.Catch(delegate() { ds.ApplyChanges(true); });
    }

    [Test]
    public void SubDocTree_Insert_TwoSubDocs_And_Delete_Fail([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Insert();
      doc.Values["F101"].SetBoolean(false);
      doc.Values["F102"].SetInteger(0);

      // Создаем первый поддокумент
      DBxSubDoc sdoc1 = doc.SubDocs["SD11"].Insert();
      sdoc1.Values["F111"].SetString("A");

      // Создаем второй поддокумент
      DBxSubDoc sdoc2 = doc.SubDocs["SD11"].Insert();
      sdoc2.Values["F111"].SetString("B");
      sdoc2.Values["F113"].SetInteger(sdoc1.SubDocId);

      // Удаляем первый поддокумент
      sdoc1.Delete();

      Assert.Catch(delegate() { ds.ApplyChanges(true); });
    }


    [Test]
    public void SubDocTree_Edit_Success([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      Int32 docId = CreateTestSubDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);

      doc.SubDocs["SD11"][1].Values["F113"].SetNull();
      doc.SubDocs["SD11"][2].Values["F113"].SetInteger(doc.SubDocs["SD11"][1].SubDocId);

      ds.ApplyChanges(false);
    }


    [Test]
    public void SubDocTree_Edit_Fail_FictiveSubDocId([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      Int32 docId = CreateTestSubDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);

      doc.SubDocs["SD11"][1].Values["F113"].SetInteger(-1);

      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }


    [Test]
    public void SubDocTree_Edit_Fail_UnknownSubDocId([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      Int32 docId = CreateTestSubDocTree(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);

      doc.SubDocs["SD11"][1].Values["F113"].SetInteger(100000);

      Assert.Catch(delegate() { ds.ApplyChanges(false); });
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

    #region Переменные ссылки

    #region Insert

    [Test]
    public void VTRef_Insert_ExistRef([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info, false, 1);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Insert();
      doc3.Values["F301TableId"].SetInteger(info.Provider.DocTypes["D1"].TableId);
      doc3.Values["F301DocId"].SetInteger(docId1);
      ds.ApplyChanges(true);
      Int32 docId3 = ds["D3"][0].DocId;

      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        object[] a = con.GetValues("D3", docId3, new DBxColumns("F301TableId,F301DocId"));
        Assert.AreEqual(info.Provider.DocTypes["D1"].TableId, a[0], "TableId");
        Assert.AreEqual(docId1, a[1], "DocId");
      }
    }

    [Test]
    public void VTRef_Insert_NewRef([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["D1"].Insert();
      doc1.Values["F101"].SetBoolean(false);
      doc1.Values["F102"].SetInteger(1);
      DBxSingleDoc doc3 = ds["D3"].Insert();
      doc3.Values["F301TableId"].SetInteger(info.Provider.DocTypes["D1"].TableId);
      doc3.Values["F301DocId"].SetInteger(doc1.DocId);
      ds.ApplyChanges(true);
      Int32 docId1 = ds["D1"][0].DocId;
      Int32 docId3 = ds["D3"][0].DocId;
      info.Provider.CheckIsRealDocId(docId1);
      info.Provider.CheckIsRealDocId(docId3);

      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        object[] a = con.GetValues("D3", docId3, new DBxColumns("F301TableId,F301DocId"));
        Assert.AreEqual(info.Provider.DocTypes["D1"].TableId, a[0], "TableId");
        Assert.AreEqual(docId1, a[1], "DocId");
      }
    }

    [Test]
    public void VTRef_Insert_Null([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info, false, 1);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Insert();
      doc3.Values["F301TableId"].SetNull();
      doc3.Values["F301DocId"].SetNull();
      ds.ApplyChanges(true);
      Int32 docId3 = ds["D3"][0].DocId;

      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        object[] a = con.GetValues("D3", docId3, new DBxColumns("F301TableId,F301DocId"));
        Assert.AreEqual(0, DataTools.GetInt(a[0]), "TableId");
        Assert.AreEqual(0, DataTools.GetInt(a[1]), "DocId");
      }
    }

    [Test]
    public void VTRef_Insert_Fail_Incomplete1([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info, false, 1);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Insert();
      doc3.Values["F301TableId"].SetInteger(info.Provider.DocTypes["D1"].TableId);
      // Не присвоили "F301DocId"
      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    [Test]
    public void VTRef_Insert_Fail_Incomplete2([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info, false, 1);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Insert();
      doc3.Values["F301DocId"].SetInteger(docId1);
      // Не присвоили "F301TableId"
      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    [Test]
    public void VTRef_Insert_Fail_Invalid_TableId([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info, false, 1);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Insert();
      doc3.Values["F301TableId"].SetInteger(1000000); // неправильный идентификатор таблицы
      doc3.Values["F301DocId"].SetInteger(1);
      // Не присвоили "F301DocId"
      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    [Test]
    public void VTRef_Insert_Fail_Unappliable_TableId([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info, false, 1);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Insert();
      doc3.Values["F301TableId"].SetInteger(info.Provider.DocTypes["D3"].TableId); // нет в списке разрешенных таблиц
      doc3.Values["F301DocId"].SetInteger(doc3.DocId);
      // Не присвоили "F301DocId"
      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    #endregion

    /// <summary>
    /// Создает документы D1, D2 и D3, в котором переменная ссылка F301 ссылается на документ D1
    /// </summary>
    /// <returns></returns>
    private Int32[] CreateTestDocsWithVTRefs(TestDBInfo info)
    {
      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["D1"].Insert();
      doc1.Values["F101"].SetBoolean(false);
      doc1.Values["F102"].SetInteger(1);

      DBxSingleDoc doc11 = ds["D1"].Insert(); // этот документ нужен только для обычной ссылки на него из D3
      doc11.Values["F101"].SetBoolean(true);
      doc11.Values["F102"].SetInteger(2);

      DBxSingleDoc doc2 = ds["D2"].Insert();
      doc2.Values["F201"].SetString("ABC");
      doc2.Values["F202"].SetInteger(doc11.DocId); // обычная ссылка. Нужна, т.к. поле обязательное

      DBxSingleDoc doc3 = ds["D3"].Insert();
      doc3.Values["F301TableId"].SetInteger(doc1.DocType.TableId);
      doc3.Values["F301DocId"].SetInteger(doc1.DocId);

      ds.ApplyChanges(true);

      Int32[] docIds = new Int32[3];
      docIds[0] = ds["D1"][0].DocId;
      docIds[1] = ds["D2"][0].DocId;
      docIds[2] = ds["D3"][0].DocId;

      info.Provider.CheckIsRealDocId(docIds[0]);
      info.Provider.CheckIsRealDocId(docIds[1]);
      info.Provider.CheckIsRealDocId(docIds[2]);

      return docIds;
    }

    #region Edit

    [Test]
    public void VTRef_Edit_ChangeRef([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocsWithVTRefs(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Edit(docIds[2]);
      Assert.AreEqual(info.Provider.DocTypes["D1"].TableId, doc3.Values["F301TableId"].AsInteger, "TableId before");
      Assert.AreEqual(docIds[0], doc3.Values["F301DocId"].AsInteger, "DocId before");

      doc3.Values["F301TableId"].SetInteger(info.Provider.DocTypes["D2"].TableId);
      doc3.Values["F301DocId"].SetInteger(docIds[1]);

      ds.ApplyChanges(false);

      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        object[] a = con.GetValues("D3", docIds[2], new DBxColumns("F301TableId,F301DocId"));
        Assert.AreEqual(info.Provider.DocTypes["D2"].TableId, a[0], "TableId");
        Assert.AreEqual(docIds[1], a[1], "DocId");
      }
    }

    [Test]
    public void VTRef_Edit_SetNull([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocsWithVTRefs(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Edit(docIds[2]);
      doc3.Values["F301TableId"].SetNull();
      doc3.Values["F301DocId"].SetNull();

      ds.ApplyChanges(false);
    }

    [Test]
    public void VTRef_Edit_Fail_Incomplete1([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocsWithVTRefs(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Edit(docIds[2]);
      Assert.AreEqual(info.Provider.DocTypes["D1"].TableId, doc3.Values["F301TableId"].AsInteger, "old TableId");
      Assert.AreEqual(docIds[0], doc3.Values["F301DocId"].AsInteger, "old DocId");

      doc3.Values["F301TableId"].SetNull();

      Assert.Catch(delegate() { ds.ApplyChanges(false); }, "ApplyChanges() must throw an exception because VTRef is half-initialized");
    }

    [Test]
    public void VTRef_Edit_Fail_Incomplete2([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocsWithVTRefs(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Edit(docIds[2]);
      doc3.Values["F301DocId"].SetNull();

      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    [Test]
    public void VTRef_Edit_Fail_UnappliableTableId([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocsWithVTRefs(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Edit(docIds[2]);

      doc3.Values["F301TableId"].SetInteger(info.Provider.DocTypes["D3"].TableId);
      doc3.Values["F301DocId"].SetInteger(docIds[2]);

      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    [Test]
    public void VTRef_Edit_Fail_FictiveDocId([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocsWithVTRefs(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Edit(docIds[2]);
      doc3.Values["F301DocId"].SetInteger(-1);

      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    [Test]
    public void VTRef_Edit_Fail_UnknownDocId([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocsWithVTRefs(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Edit(docIds[2]);
      doc3.Values["F301DocId"].SetInteger(1000000);

      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    #endregion

    #region Delete

    [Test]
    public void VTRef_Delete_Ok([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocsWithVTRefs(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D2"].Delete(docIds[1]); // на этот документ нет ссылки
      Assert.DoesNotThrow(delegate() { ds.ApplyChanges(false); }, "D2");

      ds = new DBxDocSet(info.Provider);
      ds["D3"].Delete(docIds[2]); // документ, содержащий ссылку
      Assert.DoesNotThrow(delegate() { ds.ApplyChanges(false); }, "D3");

      ds = new DBxDocSet(info.Provider);
      ds["D1"].Delete(docIds[0]); // теперь можно и этот документ удалить
      Assert.DoesNotThrow(delegate() { ds.ApplyChanges(false); }, "D1");
    }

    [Test]
    public void VTRef_Delete_Fail([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocsWithVTRefs(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Delete(docIds[0]);
      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    [Test]
    public void VTRef_Delete_Pair([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocsWithVTRefs(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Delete(docIds[0]);
      ds["D3"].Delete(docIds[2]);
      ds.ApplyChanges(false);
    }

    [Test]
    public void VTRef_Edit_And_Delete_Ok([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocsWithVTRefs(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Delete(docIds[0]);
      DBxSingleDoc doc3 = ds["D3"].Edit(docIds[2]);
      doc3.Values["F301TableId"].SetNull();
      doc3.Values["F301DocId"].SetNull();
      ds.ApplyChanges(false);
    }

    [Test]
    public void VTRef_Edit_And_Delete_Fail([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32[] docIds = CreateTestDocsWithVTRefs(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D2"].Delete(docIds[1]);
      DBxSingleDoc doc3 = ds["D3"].Edit(docIds[2]);
      doc3.Values["F301TableId"].SetInteger(info.Provider.DocTypes["D2"].TableId);
      doc3.Values["F301DocId"].SetInteger(docIds[1]);
      Assert.Catch(delegate() { ds.ApplyChanges(false); });
    }

    #endregion

    #endregion

    #region Длительные блокировки

    [Test]
    public void AddLongLock_whole([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1, docId2;
      CreateTestCrossRefDoc(info, out docId1, out docId2);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].Edit(docId1);
      ds["D2"].View(docId2);
      Guid g = ds.AddLongLock();
      Assert.AreNotEqual(Guid.Empty, g, "Guid");

      DBxDocSelection docSel1 = new DBxDocSelection(info.Provider.DBIdentity, "D1", docId1);
      DBxDocSelection docSel2 = new DBxDocSelection(info.Provider.DBIdentity, "D2", docId2);

      Assert.Catch(delegate() { TestEditLockedDoc(info, docSel1); }, "locked doc #1");
      TestEditLockedDoc(info, docSel2);

      ds.ApplyChanges(true);

      // Блокировка не должна была исчезнуть
      Assert.Catch(delegate() { TestEditLockedDoc(info, docSel1); }, "locked doc #2");

      ds.RemoveLongLock(g);

      TestEditLockedDoc(info, docSel1);
    }

    [Test]
    public void AddLongLock_docsel([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1, docId2;
      CreateTestCrossRefDoc(info, out docId1, out docId2);

      DBxDocSet ds = new DBxDocSet(info.Provider);

      ds["D1"].View(docId1);
      ds["D2"].View(docId2);
      DBxDocSelection docSel = ds.GetDocSelection(DBxDocState.View); // 2 документа
      Guid g = ds.AddLongLock(docSel);
      Assert.AreNotEqual(Guid.Empty, g, "Guid");

      DBxDocSelection docSel1 = new DBxDocSelection(info.Provider.DBIdentity, "D1", docId1);
      DBxDocSelection docSel2 = new DBxDocSelection(info.Provider.DBIdentity, "D2", docId2);

      Assert.Catch(delegate() { TestEditLockedDoc(info, docSel1); }, "locked doc #1");
      Assert.Catch(delegate() { TestEditLockedDoc(info, docSel2); }, "locked doc #2");

      ds.RemoveLongLock(g);

      TestEditLockedDoc(info, docSel1);
      TestEditLockedDoc(info, docSel2);
    }

    [Test]
    public void AddLongLock_secondtry([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1, docId2;
      CreateTestCrossRefDoc(info, out docId1, out docId2);

      DBxDocSet ds1 = new DBxDocSet(info.Provider);
      ds1["D1"].Edit(docId1);
      ds1["D2"].View(docId2);
      Guid g = ds1.AddLongLock();
      Assert.AreNotEqual(Guid.Empty, g, "Guid");

      DBxDocSet ds2 = new DBxDocSet(info.Provider);
      ds2["D1"].Edit(docId1); // это должно работать
      Assert.Catch(delegate() { ds2.AddLongLock(); });
    }



    /// <summary>
    /// Открываем на редактирование и сохраняем документы без изменения
    /// </summary>
    /// <param name="info"></param>
    /// <param name="docSel"></param>
    private void TestEditLockedDoc(TestDBInfo info, DBxDocSelection docSel)
    {
      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds.Edit(docSel);
      ds.ApplyChanges(false);
    }

    #endregion
  }
}
