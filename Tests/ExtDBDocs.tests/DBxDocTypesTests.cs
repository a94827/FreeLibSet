using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Core;

namespace ExtDBDocs_tests.Data_Docs
{
  [TestFixture]
  public class DBxDocTypesTests
  {
    #region Конструктор

    [Test]
    public void Contstructor()
    {
      DBxDocTypes sut = new DBxDocTypes();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual("Users", sut.UsersTableName, "UsersTableName");
      Assert.IsTrue(sut.UseUsers, "UseUsers");
      Assert.GreaterOrEqual(sut.ActionInfoMaxLength, 20, "ActionInfoMaxLength");
      Assert.IsTrue(sut.UseDeleted, "UseDeleted");
      Assert.IsTrue(sut.UseVersions, "UseVersions");
      Assert.IsTrue(sut.UseTime, "UseTime");
      Assert.IsFalse(sut.UseSessionId, "UseSessionId");
      Assert.IsFalse(sut.HasBinDataRefs, "HasBinDataRefs");
      Assert.IsFalse(sut.HasFileRefs, "HasFileRefs");
    }

    #endregion

    #region Свойства UseXXX

    [Test]
    public void UseUsers()
    {
      DBxDocTypes sut1 = new DBxDocTypes();
      sut1.UsersTableName = String.Empty;
      Assert.IsFalse(sut1.UseUsers, "#1");

      DBxDocTypes sut2 = new DBxDocTypes();
      DBxDocType dt = new DBxDocType("D1");
      dt.Struct.Columns.AddString("Name", 10, false);
      sut2.Add(dt);
      sut1.UsersTableName = dt.Name;
      Assert.IsTrue(sut2.UseUsers, "#2");
    }

    #endregion

    #region Получение структуры таблиц

    [Test]
    public void GetMainDBStruct_main_columns()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();

      DBxStruct dbs = sut.GetMainDBStruct();
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");

      DBxTableStruct ts1 = dbs.Tables["D1"];
      Assert.IsNotNull(ts1, "D1");
      AssertColumn(ts1.Columns, "Id", DBxColumnType.Int32, false, "D1");
      AssertColumn(ts1.Columns, "F101", DBxColumnType.String, false, "D1");
      AssertColumn(ts1.Columns, "F102", DBxColumnType.Int32, true, "D1");
      Assert.AreEqual("Id", ts1.PrimaryKey.AsString, "D1 Primary Key");

      DBxTableStruct ts11 = dbs.Tables["SD11"];
      Assert.IsNotNull(ts11, "SD11");
      AssertColumn(ts11.Columns, "Id", DBxColumnType.Int32, false, "SD11 table");
      AssertColumn(ts11.Columns, "DocId", DBxColumnType.Int32, false, "SD11 table");
      Assert.AreEqual("D1", ts11.Columns["DocId"].MasterTableName, "SD11 MasterTableName");
      Assert.AreEqual(DBxRefType.Disallow, ts11.Columns["DocId"].RefType, "SD11 DocId RefType");
      AssertColumn(ts11.Columns, "F111", DBxColumnType.String, true, "SD11");
      Assert.AreEqual("Id", ts11.PrimaryKey.AsString, "SD1 Primary Key");
    }

    [Test]
    public void GetMainDBStruct_service_columns(
      [Values(false, true)] bool useDeleted,
      [Values(false, true)] bool useVersions,
      [Values(false, true)] bool useTime,
      [Values(false, true)] bool useUsers)
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();
      sut.UseDeleted = useDeleted;
      sut.UseVersions = useVersions;
      sut.UseTime = useTime;
      if (useUsers)
        sut.UsersTableName = "D1";
      Assert.AreEqual(useUsers, sut.UseUsers, "UseUsers");

      DBxStruct dbs = sut.GetMainDBStruct();

      #region Документ

      DBxTableStruct ts1 = dbs.Tables["D1"];
      if (useVersions)
      {
        AssertColumn(ts1.Columns, "Version", DBxColumnType.Int16, true, "D1");
        // Поле Version2 не проверяем, т.к. оно для внутренних целей
      }
      else
        Assert.IsNull(ts1.Columns["Versions"], "D1.Version");
      if (useDeleted)
        AssertColumn(ts1.Columns, "Deleted", DBxColumnType.Boolean, null, "D1");
      else
        Assert.IsNull(ts1.Columns["Deleted"], "D1.Deleted");
      if (useTime)
      {
        AssertColumn(ts1.Columns, "CreateTime", DBxColumnType.DateTime, null, "D1");
        AssertColumn(ts1.Columns, "ChangeTime", DBxColumnType.DateTime, true, "D1");
      }
      else
      {
        Assert.IsNull(ts1.Columns["CreateTime"], "D1.CreateTime");
        Assert.IsNull(ts1.Columns["ChangeTime"], "D1.ChangeTime");
      }
      if (useUsers)
      {
        AssertColumn(ts1.Columns, "CreateUserId", DBxColumnType.Int32, null, "D1");
        Assert.AreEqual("D1", ts1.Columns["CreateUserId"].MasterTableName, "D1.CreateUserId.MasterTableName");
        Assert.AreEqual(DBxRefType.Disallow, ts1.Columns["CreateUserId"].RefType, "D1.CreateUserId.RefType");
        AssertColumn(ts1.Columns, "ChangeUserId", DBxColumnType.Int32, true, "D1");
        Assert.AreEqual("D1", ts1.Columns["ChangeUserId"].MasterTableName, "D1.ChangeUserId.MasterTableName");
        Assert.AreEqual(DBxRefType.Disallow, ts1.Columns["ChangeUserId"].RefType, "D1.ChangeUserId.RefType");
      }
      else
      {
        Assert.IsNull(ts1.Columns["CreateUserId"], "D1.CreateUserId");
        Assert.IsNull(ts1.Columns["ChangeUserId"], "D1.ChangeUserId");
      }

      #endregion

      #region Поддокумент

      DBxTableStruct ts11 = dbs.Tables["SD11"];
      if (useDeleted)
        AssertColumn(ts11.Columns, "Deleted", DBxColumnType.Boolean, null, "SD11");
      else
        Assert.IsNull(ts11.Columns["Deleted"], "SD11.Deleted");

      // Остальные поля поддокументов не проверяем, т.к. они являются служебными

      #endregion
    }


    [Test]
    public void GetUndoDBStruct_main_columns()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();

      DBxStruct dbs = sut.GetUndoDBStruct();
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");

      DBxTableStruct ts1 = dbs.Tables["D1"];
      Assert.IsNotNull(ts1, "D1");
      //AssertColumn(ts1.Columns, "Id", DBxColumnType.Int, false, "D1");
      AssertColumn(ts1.Columns, "F101", DBxColumnType.String, false, "D1");
      AssertColumn(ts1.Columns, "F102", DBxColumnType.Int32, true, "D1");
      //Assert.AreEqual("Id", ts1.PrimaryKey.AsString, "D1 Primary Key");

      DBxTableStruct ts11 = dbs.Tables["SD11"];
      Assert.IsNotNull(ts11, "SD11");
      //AssertColumn(ts11.Columns, "Id", DBxColumnType.Int, false, "SD11 table");
      //AssertColumn(ts11.Columns, "DocId", DBxColumnType.Int, false, "SD11 table");
      //Assert.AreEqual("", ts11.Columns["DocId"].MasterTableName, "SD11 MasterTableName");
      //Assert.AreEqual(DBxRefType.Disallow, ts11.Columns["DocId"].RefType, "SD11 DocId RefType");
      AssertColumn(ts11.Columns, "F111", DBxColumnType.String, true, "SD11");
      //Assert.AreEqual("Id", ts11.PrimaryKey.AsString, "SD1 Primary Key");
    }


    [Test]
    public void GetUndoDBStruct_null()
    {
      DBxDocTypes sut1 = CreateTestSimpleDocTypes();
      sut1.UseDeleted = false;
      Assert.IsNull(sut1.GetUndoDBStruct(), "UseDeleted=false");

      DBxDocTypes sut2 = CreateTestSimpleDocTypes();
      sut2.UseTime = false;
      Assert.IsNull(sut2.GetUndoDBStruct(), "UseTime=false");

      DBxDocTypes sut3 = CreateTestSimpleDocTypes();
      sut3.UseVersions = false;
      Assert.IsNull(sut3.GetUndoDBStruct(), "UseVersion=false");
    }

    #endregion

    #region Группы документов

    [Test]
    public void GetGroupDocTypes()
    {
      DBxDocTypes sut = CreateTestDocTypesWithGroups();
      DBxDocType[] res = sut.GetGroupDocTypes();
      Assert.AreEqual(1, res.Length);
      Assert.AreEqual("G21", res[0].Name);
    }

    [Test]
    public void GetGroupDocTypeNames()
    {
      DBxDocTypes sut = CreateTestDocTypesWithGroups();
      string[] res = sut.GetGroupDocTypeNames();
      Assert.AreEqual(1, res.Length);
      Assert.AreEqual("G21", res[0]);
    }

    private DBxDocTypes CreateTestDocTypesWithGroups()
    {
      DBxDocTypes dbs = CreateTestSimpleDocTypes();
      DBxDocType dt = new DBxDocType("G21");
      dt.Struct.Columns.AddReference("ParentId", "G21", true)
        .ColumnType = DBxColumnType.Int32;
      dt.Struct.Columns.AddString("Name", 20, false);
      dbs.Add(dt);
      dbs["D1"].Struct.Columns.AddReference("GroupId", "G21", true)
        .ColumnType = DBxColumnType.Int32;
      dbs["D1"].GroupRefColumnName = "GroupId";
      return dbs;
    }


    #endregion

    #region События на стороне сервера

    private class EventTester
    {
      #region Обработчики событий

      public int BeforeInsertCount;

      public void BeforeInsertHandler(object sender, ServerDocTypeBeforeInsertEventArgs args)
      {
        BeforeInsertCount++;
        DocState = args.Doc.DocState;

        // Меняем значение
        args.Doc.Values["F101"].SetString("NEW");
      }

      public int BeforeWriteCount;

      public void BeforeWriteHandler(object sender, ServerDocTypeBeforeWriteEventArgs args)
      {
        BeforeWriteCount++;
        DocState = args.Doc.DocState;

        // Меняем значение
        args.Doc.Values["F102"].SetInt32(123);
      }

      public int BeforeDeleteCount;

      public void BeforeDeleteHandler(object sender, ServerDocTypeBeforeDeleteEventArgs args)
      {
        BeforeDeleteCount++;
        DocState = args.Doc.DocState;
      }

      public int AfterChangeCount;

      public void AfterChangeHandler(object sender, ServerDocTypeAfterChangeEventArgs args)
      {
        AfterChangeCount++;
        DocState = args.Doc.DocState;
      }

      #endregion

      #region Для тестирования

      public DBxDocState DocState;

      public void ResetCounts()
      {
        BeforeInsertCount = 0;
        BeforeWriteCount = 0;
        BeforeDeleteCount = 0;
        AfterChangeCount = 0;
        DocState = DBxDocState.None;
      }

      public void AssertCounts(int beforeInsert, int beforeWrite, int beforeDelete, int afterChange, string message)
      {
        if (!String.IsNullOrEmpty(message))
          message += ". ";
        Assert.AreEqual(beforeInsert, BeforeInsertCount, message + "Event count: BeforeInsert");
        Assert.AreEqual(beforeWrite, BeforeWriteCount, message + "Event count: BeforeWrite");
        Assert.AreEqual(beforeDelete, BeforeDeleteCount, message + "Event count: BeforeDelete");
        Assert.AreEqual(afterChange, AfterChangeCount, message + "Event count: AfterChange");
      }

      #endregion
    }

    [Test]
    public void DBxDocType_sevrer_events()
    {
      DBxDocTypes dts = CreateTestSimpleDocTypes();
      EventTester tester = new EventTester();
      dts["D1"].BeforeInsert += tester.BeforeInsertHandler;
      dts["D1"].BeforeWrite += tester.BeforeWriteHandler;
      dts["D1"].BeforeDelete += tester.BeforeDeleteHandler;
      dts["D1"].AfterChange += tester.AfterChangeHandler;

      DBxDocDBConnectionHelper conHelper = new DBxDocDBConnectionHelper();
      conHelper.ProviderName = "SQLite";
      conHelper.ConnectionString = "Data Source=:memory:";
      conHelper.DocTypes = dts;

      DBxRealDocProviderGlobal globalData = conHelper.CreateRealDocProviderGlobal();
      try
      {
        DBxRealDocProviderSource source = new DBxRealDocProviderSource(globalData);
        DBxRealDocProvider provider = new DBxRealDocProvider(source, 0, false);

        #region Insert

        DBxDocSet ds = new DBxDocSet(provider);
        DBxSingleDoc doc = ds["D1"].Insert();
        doc.Values["F101"].SetString("ABC");
        ds.ApplyChanges(true);
        Int32 docId = doc.DocId;
        tester.AssertCounts(1, 1, 0, 1, "Insert");
        Assert.AreEqual(DBxDocState.Insert, tester.DocState, "Insert - DocState");
        Assert.AreEqual("NEW", doc.Values["F101"].AsString, "Insert - F101"); // заменено BeforeInsert
        Assert.AreEqual(123, doc.Values["F102"].AsInt32, "Insert - F102"); // заменено BeforeWrite

        #endregion

        #region Edit

        tester.ResetCounts();
        ds = new DBxDocSet(provider);
        doc = ds["D1"].Edit(docId);
        doc.Values["F101"].SetString("DEF");
        doc.Values["F102"].SetInt32(456);
        ds.ApplyChanges(true);
        tester.AssertCounts(0, 1, 0, 1, "Edit");
        Assert.AreEqual(DBxDocState.Edit, tester.DocState, "Edit - DocState");
        Assert.AreEqual("DEF", doc.Values["F101"].AsString, "Edit - F101"); // не поменялся
        Assert.AreEqual(123, doc.Values["F102"].AsInt32, "Edit - F102");  // заменено BeforeWrite

        #endregion

        #region Delete

        tester.ResetCounts();
        ds = new DBxDocSet(provider);
        ds["D1"].Delete(docId);
        ds.ApplyChanges(true);
        tester.AssertCounts(0, 0, 1, 1, "Delete");
        Assert.AreEqual(DBxDocState.Delete, tester.DocState, "Delete - DocState");

        #endregion

        #region Restore

        tester.ResetCounts();
        ds = new DBxDocSet(provider);
        doc = ds["D1"].Edit(docId);
        Assert.IsTrue(doc.Deleted, "Is deleted");
        Assert.AreEqual("DEF", doc.Values["F101"].AsString, "Restore - old F101");
        Assert.AreEqual(123, doc.Values["F102"].AsInt32, "Restore - old F102");
        doc.Values["F101"].SetString("GHI");
        doc.Values["F102"].SetInt32(789);
        ds.ApplyChanges(true);
        tester.AssertCounts(1, 1, 0, 1, "Restore");
        Assert.AreEqual(DBxDocState.Edit, tester.DocState, "Restore - DocState");
        Assert.AreEqual("NEW", doc.Values["F101"].AsString, "Resore - F101"); // заменено BeforeInsert
        Assert.AreEqual(123, doc.Values["F102"].AsInt32, "Edit - F102");  // заменено BeforeWrite

        #endregion
      }
      finally
      {
        globalData.DisposeDBs();
      }
    }

    #endregion

    #region Поиск таблиц

    [Test]
    public void FindByTableId()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();
      sut.UseDeleted = false;
      sut.UseVersions = false;
      sut.UseTime = false;

      Assert.Catch(delegate() { sut.FindByTableId(1); }, "Before DB created");

      DBxDocDBConnectionHelper conHelper = new DBxDocDBConnectionHelper();
      conHelper.ProviderName = "SQLite";
      conHelper.ConnectionString = "Data Source=:memory:";
      conHelper.DocTypes = sut;

      DBxRealDocProviderGlobal globalData = conHelper.CreateRealDocProviderGlobal();
      try
      {
        DBxDocType dt = sut.FindByTableId(1);
        Assert.AreSame(sut["D1"], dt, "After DB created");

        Assert.IsNull(sut.FindByTableId(0), "TableId=0");
      }
      finally
      {
        globalData.DisposeDBs();
      }
    }

    [Test]
    public void GetTableNameById()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();
      sut.UseDeleted = false;
      sut.UseVersions = false;
      sut.UseTime = false;

      Assert.Catch(delegate() { sut.GetTableNameById(1); }, "Before DB created");

      DBxDocDBConnectionHelper conHelper = new DBxDocDBConnectionHelper();
      conHelper.ProviderName = "SQLite";
      conHelper.ConnectionString = "Data Source=:memory:";
      conHelper.DocTypes = sut;

      DBxRealDocProviderGlobal globalData = conHelper.CreateRealDocProviderGlobal();
      try
      {
        string s1 = sut.GetTableNameById(1);
        Assert.AreEqual("D1", s1, "After DB created");

        string s2 = sut.GetTableNameById(0);
        Assert.AreEqual("", s2, "TableId=0");
      }
      finally
      {
        globalData.DisposeDBs();
      }
    }

    [Test]
    public void FindByTableName_1()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();

      Assert.AreSame(sut["D1"], sut.FindByTableName("D1"), "D1");
      Assert.AreSame(sut["D1"].SubDocs["SD11"], sut.FindByTableName("SD11"), "SD11");
      Assert.IsNull(sut.FindByTableName("XXX"), "No such table");
      Assert.IsNull(sut.FindByTableName(""), "Empty table name");
    }

    [Test]
    public void FindByTableName_2()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();
      DBxDocType dt;
      DBxSubDocType sdt;

      sut.FindByTableName("D1", out dt, out sdt);
      Assert.AreSame(sut["D1"], dt, "D1-DBxDocType");
      Assert.IsNull(sdt, "D1-DBxSubDocType");

      sut.FindByTableName("SD11", out dt, out sdt);
      Assert.AreSame(sut["D1"], dt, "SD11-DBxDocType");
      Assert.AreSame(sut["D1"].SubDocs["SD11"], sdt, "SD11-DBxSubDocType");

      sut.FindByTableName("XXX", out dt, out sdt);
      Assert.IsNull(dt, "No such table-DBxDocType");
      Assert.IsNull(sdt, "No such table-DBxSubDocType");

      sut.FindByTableName("", out dt, out sdt);
      Assert.IsNull(dt, "Empty table name-DBxDocType");
      Assert.IsNull(sdt, "Empty table name-DBxSubDocType");
    }
    [Test]
    public void GetDocTypeNames()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();
      string[] res = sut.GetDocTypeNames();
      Assert.AreEqual(new string[] { "D1" }, res);
    }

    #endregion

    #region SetReadOnly

    [Test]
    public void SetReadOnly()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly before");

      sut.SetReadOnly();
      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly after");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.UseDeleted = false; }, "UseDeleted");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.UseVersions = false; }, "UseVersions");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.UseTime = false; }, "UseTime");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.UsersTableName = "D1"; }, "UsersTableName");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.UseSessionId = true; }, "UseSessionId");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.ActionInfoMaxLength = 50; }, "ActionInfoMaxLength");

      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Add(new DBxDocType("DDummy")); }, "Add(DBxDocType)");
      DBxDocType dt = sut["D1"];
      Assert.IsTrue(dt.Struct.IsReadOnly, "DBxDocType.Struct.IsReadOnly");
      Assert.IsTrue(dt.BinDataRefs.IsReadOnly, "DBxDocType.BinDataRefs.IsReadOnly");
      Assert.IsTrue(dt.FileRefs.IsReadOnly, "DBxDocType.FileRefs.IsReadOnly");
      Assert.IsTrue(dt.VTRefs.IsReadOnly, "DBxDocType.VTRefs.IsReadOnly");
      Assert.IsTrue(dt.CalculatedColumns.IsReadOnly, "DBxDocType.CalculatedColumns.IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate() { dt.DefaultOrder = DBxOrder.FromDataViewSort("F102"); }, "DBxDocType.DefaultOrder");
      Assert.Catch<ObjectReadOnlyException>(delegate() { dt.GroupRefColumnName = "F102"; }, "DBxDocType.GroupRefColumnName");

      Assert.Catch<ObjectReadOnlyException>(delegate() { dt.SubDocs.Add(new DBxSubDocType("SDDummy")); }, "Add(DBxSubDocType)");
      DBxSubDocType sdt = sut["D1"].SubDocs["SD11"];
      Assert.IsTrue(sdt.Struct.IsReadOnly, "DBxSubDocType.Struct.IsReadOnly");
      Assert.IsTrue(sdt.BinDataRefs.IsReadOnly, "DBxSubDocType.BinDataRefs.IsReadOnly");
      Assert.IsTrue(sdt.FileRefs.IsReadOnly, "DBSubxDocType.FileRefs.IsReadOnly");
      Assert.IsTrue(sdt.VTRefs.IsReadOnly, "DBxSubDocType.VTRefs.IsReadOnly");
      Assert.IsTrue(sdt.CalculatedColumns.IsReadOnly, "DBxSubDocType.CalculatedColumns.IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sdt.DefaultOrder = DBxOrder.FromDataViewSort("F111"); }, "DBxDocType.DefaultOrder");
    }

    #endregion

    #region HasBinDataRefs, HasFileRefs и HasVTRefs

    [Test]
    public void HasBinDataRefs()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();
      Assert.IsFalse(sut.HasBinDataRefs, "before");
      sut["D1"].BinDataRefs.Add("B1");
      Assert.IsTrue(sut.HasBinDataRefs, "in DBxDocType");

      sut = CreateTestSimpleDocTypes();
      sut["D1"].SubDocs["SD11"].BinDataRefs.Add("B2");
      Assert.IsTrue(sut.HasBinDataRefs, "in DBxSubDocType");
    }

    [Test]
    public void HasFileRefs()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();
      Assert.IsFalse(sut.HasFileRefs, "before");
      sut["D1"].FileRefs.Add("F1");
      Assert.IsTrue(sut.HasFileRefs, "in DBxDocType");

      sut = CreateTestSimpleDocTypes();
      sut["D1"].SubDocs["SD11"].FileRefs.Add("F2");
      Assert.IsTrue(sut.HasFileRefs, "in DBxSubDocType");
    }


    [Test]
    public void HasVTRefs()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();
      Assert.IsFalse(sut.HasVTRefs, "before");

      DBxDocType dt = sut["D1"];
      dt.VTRefs.Add("V1");
      Assert.IsTrue(dt.Struct.Columns.Contains("V1TableId"));
      Assert.IsTrue(dt.Struct.Columns.Contains("V1DocId"));
      Assert.AreEqual(DBxColumnType.Int32, dt.Struct.Columns["V1TableId"].ColumnType, "V1TableId ColumnType");
      Assert.AreEqual(DBxColumnType.Int32, dt.Struct.Columns["V1DocId"].ColumnType, "V1DocId ColumnType");
      Assert.IsTrue(sut.HasVTRefs, "in DBxDocType");

      sut = CreateTestSimpleDocTypes();
      sut["D1"].SubDocs["SD11"].VTRefs.Add("V2");
      Assert.IsTrue(sut.HasVTRefs, "in DBxSubDocType");
    }

    #endregion

    #region InitDocTableIds()

    [Test]
    public void InitDocTableIds()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();
      sut.UseDeleted = false;
      sut.UseVersions = false;
      sut.UseTime = false;

      Assert.Catch(delegate() { sut.GetTableNameById(1); }, "Before DB created");

      using (DBx mainDB = DBxManager.Managers[DBxProviderNames.SQLite].CreateDBObject("Data Source=:memory:"))
      {
        mainDB.CreateIfRequired(); // ничего не делает
        mainDB.Struct = sut.GetMainDBStruct();
        mainDB.UpdateStruct();

        sut.InitDocTableIds(mainDB.MainEntry);

        string s1 = sut.GetTableNameById(1);
        Assert.AreEqual("D1", s1, "After DB created");
      }
    }

    #endregion

    #region GetToDocTypeRefs()

    [Test]
    public void GetToDocTypeRefs()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();
      sut["D1"].Struct.Columns.AddReference("R1", "D1", true); // ссылка на самого себя (дерево документов)
      DBxDocType dt2 = new DBxDocType("D2");
      dt2.Struct.Columns.AddReference("R2", "D1", false); // обычная ссылка
      dt2.Struct.Columns.AddReference("R3", "SD11", false); // ссылка на поддокумент
      DBxVTReference vtr4 = dt2.VTRefs.Add("R4");
      vtr4.MasterTableNames.Add("D1");
      sut.Add(dt2);

      DBxSubDocType sd21 = new DBxSubDocType("SD21");
      sd21.Struct.Columns.AddReference("R5", "D1", false); // ссылка на документ
      sd21.Struct.Columns.AddReference("R6", "SD11", false); // ссылка на поддокумент
      DBxVTReference vtr7 = sd21.VTRefs.Add("R7");
      vtr7.MasterTableNames.Add("D1");
      DBxVTReference vtr8 = sd21.VTRefs.Add("R8");
      vtr8.MasterTableNames.Add("D3"); // не ссылаемся на D1
      dt2.SubDocs.Add(sd21);

      DBxDocType dt3 = new DBxDocType("D3"); // на этот документ есть переменная ссылка
      sut.Add(dt3);

      Assert.AreEqual(0, sut.GetToDocTypeRefs("D2").Length, "No refs to D2");

      DBxDocTypeRefInfo[] a1 = sut.GetToDocTypeRefs("D1");
      // Порядок элементов в массиве не определен
      string[] a1str = new string[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a1str[i] = DBxDocTypeRefInfo_ToString(a1[i]);
      Array.Sort<string>(a1str);

      string[] aWanted = new string[7];
      aWanted[0] = "R1,Col,D1,D1";
      aWanted[1] = "R2,Col,D2,D1";
      aWanted[2] = "R3,Col,D2,SD11";
      aWanted[3] = "R4,VT,D2,D1";
      aWanted[4] = "R5,Col,SD21,D1";
      aWanted[5] = "R6,Col,SD21,SD11";
      aWanted[6] = "R7,VT,SD21,D1";
      Array.Sort<string>(aWanted);

      Assert.AreEqual(aWanted, a1str, "Refs to D1");

      sut.SetReadOnly();
      a1 = sut.GetToDocTypeRefs("D1");
      // Порядок элементов в массиве не определен
      a1str = new string[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a1str[i] = DBxDocTypeRefInfo_ToString(a1[i]);
      Array.Sort<string>(a1str);
      Assert.AreEqual(aWanted, a1str, "Refs to D1 after SetReadOnly()");
    }

    private static string DBxDocTypeRefInfo_ToString(DBxDocTypeRefInfo obj)
    {
      StringBuilder sb = new StringBuilder();
      if (obj.RefType == DBxDocTypeRefType.Column)
        sb.Append(obj.FromColumn.ColumnName);
      else
        sb.Append(obj.FromVTReference.Name);
      sb.Append(',');
      if (obj.RefType == DBxDocTypeRefType.Column)
        sb.Append("Col");
      else
        sb.Append("VT");
      sb.Append(',');
      sb.Append(obj.FromDocTypeBase.Name);
      sb.Append(',');
      sb.Append(obj.ToDocTypeBase.Name);
      return sb.ToString();
    }

    #endregion

    #region CheckStruct()

    [Test]
    public void CheckStruct_Ok()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();
      sut.CheckStruct(null);
    }

    // Если установить DBxDocTypes.UsersTableName на несуществующую таблицу, то будет автоматически создан новый вид документа.
    //[Test]
    //public void CheckStruct_UsersTableName()
    //{
    //  DBxDocTypes sut = CreateTestSimpleDocTypes();
    //  sut.UsersTableName = "DUnknown";
    //  Assert.IsTrue(sut.UseUsers, "UseUsers");
    //  Assert.Catch(delegate() { sut.CheckStruct(null); });
    //}

    [Test]
    public void CheckStruct_CalculatedColumns()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();
      sut["D1"].CalculatedColumns.Add("FUnknown");
      Assert.Catch(delegate() { sut.CheckStruct(null); });
    }

    [Test]
    public void CheckStruct_VTRefs_SameTableIdColumn()
    {
      // Для этого теста нельзя использовать перегрузки DBxVTReferenceList.Add(), принимающие имена столбцов.
      // Возникнет исключение при попытке повторно добавить столбец в структуру таблицы.
      // Нужно создать столбцы вручную.

      DBxDocTypes sut = CreateTestSimpleDocTypes();
      DBxColumnStruct colTableId = sut["D1"].Struct.Columns.AddInt32("TableId", true);
      DBxColumnStruct colDocId1 = sut["D1"].Struct.Columns.AddInt32("DocId1", true);
      DBxColumnStruct colDocId2 = sut["D1"].Struct.Columns.AddInt32("DocId2", true);

      sut["D1"].VTRefs.Add(new DBxVTReference("VTR1", sut["D1"].Struct, colTableId, colDocId1));
      sut["D1"].VTRefs.Add(new DBxVTReference("VTR2", sut["D1"].Struct, colTableId, colDocId2));
      Assert.Catch(delegate() { sut.CheckStruct(null); });
    }

    [Test]
    public void CheckStruct_VTRefs_SameDocIdColumn()
    {
      DBxDocTypes sut = CreateTestSimpleDocTypes();
      DBxColumnStruct colTableId1 = sut["D1"].Struct.Columns.AddInt32("TableId1", true);
      DBxColumnStruct colTableId2 = sut["D1"].Struct.Columns.AddInt32("TableId2", true);
      DBxColumnStruct colDocId = sut["D1"].Struct.Columns.AddInt32("DocId", true);
      sut["D1"].VTRefs.Add(new DBxVTReference("VTR1", sut["D1"].Struct, colTableId1, colDocId));
      sut["D1"].VTRefs.Add(new DBxVTReference("VTR2", sut["D1"].Struct, colTableId2, colDocId));
      Assert.Catch(delegate() { sut.CheckStruct(null); });
    }

    #endregion

    #region Вспомогательные методы

    private DBxDocTypes CreateTestSimpleDocTypes()
    {
      DBxDocTypes dts = new DBxDocTypes();
      dts.UsersTableName = String.Empty;

      DBxDocType dt = new DBxDocType("D1");
      dt.SingularTitle = "Test Doc";
      dt.PluralTitle = "Test Docs";
      dt.Struct.Columns.AddString("F101", 10, false);
      dt.Struct.Columns.AddInt32("F102", true);
      dts.Add(dt);

      DBxSubDocType sdt = new DBxSubDocType("SD11");
      sdt.Struct.Columns.AddString("F111", 10, true);
      dt.SubDocs.Add(sdt);

      return dts;
    }

    private void AssertColumn(DBxTableStruct.ColumnCollection columns, string colName, DBxColumnType columnType, bool? nullable, string message)
    {
      if (message == null)
        message += ", ";
      message += "ColumnName=\"" + colName + "\". ";
      DBxColumnStruct colStr = columns[colName];

      Assert.IsNotNull(colStr, message + "Column existance");
      Assert.AreEqual(columnType, colStr.ColumnType, message + "ColumnType");
      if (nullable.HasValue)
        Assert.AreEqual(nullable, colStr.Nullable, message + "Nullable");
    }

    #endregion
  }
}
