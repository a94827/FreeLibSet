using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data.Docs;
using System.Data;
using FreeLibSet.Data;
using FreeLibSet.Core;

namespace ExtDBDocs_tests.Data_Docs
{
  [TestFixture]
  class DBxDocProviderTests : DBxDocSetTestsBase
  {
    // Значительная часть функционала тестируется в DBxDocSetTests

    #region Пересчет вычисляемых полей

    [Test]
    public void RecalcColumns_all([Values(false, true)] bool useDeleted)
    {
      TestDBInfo info = this[useDeleted, true, true, false];
      Int32 docId1 = CreateTestDoc(info, false, 5);
      Int32 docId2 = CreateTestDoc(info, false, 7);
      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D1"].View(new Int32[] { docId1, docId2 });
      Assert.AreEqual(5 * 2, ds["D1"][0].Values["F107"].AsInteger, "original calculated value #1");
      Assert.AreEqual(7 * 2, ds["D1"][1].Values["F107"].AsInteger, "original calculated value #2");

      // Портим вычисляемые поля
      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        con.SetValue("D1", docId1, "F107", 1);
        con.SetValue("D1", docId2, "F107", 2);
      }

      info.Provider.RecalcColumns("D1", null);
      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        Assert.AreEqual(5 * 2, con.GetValue("D1", docId1, "F107"), "recalculated #1");
        Assert.AreEqual(7 * 2, con.GetValue("D1", docId2, "F107"), "recalculated #2");
      }
    }

    [Test]
    public void RecalcColumns_selected([Values(false, true)] bool useDeleted)
    {
      TestDBInfo info = this[useDeleted, true, true, false];
      Int32 docId1 = CreateTestDoc(info, false, 5);
      Int32 docId2 = CreateTestDoc(info, false, 7);

      // Портим вычисляемые поля
      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        con.SetValue("D1", docId1, "F107", 1);
        con.SetValue("D1", docId2, "F107", 2);
      }

      info.Provider.RecalcColumns("D1", new Int32[1] { docId2 });
      using (DBxCon con = new DBxCon(info.GlobalData.MainDBEntry))
      {
        Assert.AreEqual(1, con.GetValue("D1", docId1, "F107"), "not recalculated #1");
        Assert.AreEqual(7 * 2, con.GetValue("D1", docId2, "F107"), "recalculated #2");
      }
    }

    #endregion

    #region Прочие методы свойства

    [Test]
    public void AllDocServiceColumns([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime, [Values(false, true)] bool useUsers)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime, useUsers];

      Assert.IsTrue(info.Provider.AllDocServiceColumns.Contains("Id"), "Id");
      Assert.AreEqual(useDeleted, info.Provider.AllDocServiceColumns.Contains("Deleted"), "Deleted");
      Assert.AreEqual(useVersions, info.Provider.AllDocServiceColumns.Contains("Version"), "Version");
      Assert.AreEqual(useTime, info.Provider.AllDocServiceColumns.Contains("CreateTime"), "CreateTime");
      Assert.AreEqual(useUsers, info.Provider.AllDocServiceColumns.Contains("CreateUserId"), "CreateUserId");
      Assert.AreEqual(useTime, info.Provider.AllDocServiceColumns.Contains("ChangeTime"), "CreateTime");
      Assert.AreEqual(useUsers, info.Provider.AllDocServiceColumns.Contains("ChangeUserId"), "CreateUserId");
    }

    [Test]
    public void AllSubDocServiceColumns([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime, [Values(false, true)] bool useUsers)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime, useUsers];

      Assert.IsTrue(info.Provider.AllSubDocServiceColumns.Contains("Id"), "Id");
      Assert.IsTrue(info.Provider.AllSubDocServiceColumns.Contains("DocId"), "DocId");
      Assert.AreEqual(useDeleted, info.Provider.AllSubDocServiceColumns.Contains("Deleted"), "Deleted");
    }


    [Test]
    public void Clone()
    {
      TestDBInfo info = this[false, false, false, false];
      Assert.IsFalse(info.Provider.CloningAllowed, "CloningAllowed original value");

      Assert.Catch(delegate() { info.Provider.Clone(); }, "CloningAllowed=false");

      DBxDocProvider provider2 = null; // чтобы компилятор не ругался
      info.Provider.CloningAllowed = true;
      try
      {
        Assert.DoesNotThrow(delegate() { provider2 = info.Provider.Clone(); }, "CloningAllowed=true");
      }
      finally
      {
        info.Provider.CloningAllowed = false; // иначе повторные тесты испортятся
      }

      Assert.AreSame(provider2.GetType(), info.Provider.GetType(), "is DBxRealDocProvider");
    }

    // Бесполезно тестировать, т.к. у автоматически созданного справочника пользователей нет TextHandler'а
    //[Test]
    //public void UserName()
    //{
    //  TestDBInfo info = this[false, false, false, true];

    //  Assert.AreEqual("Test user", info.Provider.UserName);
    //}



    #endregion

    #region Права пользователя

    [Test]
    public void UserPermissions()
    {
      TestDBInfo info = this[true, true, true, true];
      DBxRealDocProvider provider1 = info.Provider;
      Assert.IsNull(provider1.UserPermissions.GetLast<DocTypePermission>(), "unlimited");

      DBxRealDocProvider provider2 = CreateTestDocProviderWithPermissions(info);
      Assert.IsNotNull(provider2.UserPermissions.GetLast<DocTypePermission>(), "limited");
    }

    /// <summary>
    /// Создает тестовый DBxRealDocProvider.
    /// Для документов "D2" доступ только для чтения
    /// Для документов "D3" доступ запрещен.
    /// Для документов "D1" запрещен доступ к истории документов
    /// </summary>
    private DBxRealDocProvider CreateTestDocProviderWithPermissions(TestDBInfo info)
    {
      DBxRealDocProviderSource source = new DBxRealDocProviderSource(info.GlobalData);

      UserPermissions ups = new UserPermissions(UserPermissionCreators.Empty);
      DocTypePermission p2 = new DocTypePermission(info.GlobalData.DocTypes, "D2", DBxAccessMode.ReadOnly);
      ups.Add(p2);
      DocTypePermission p3 = new DocTypePermission(info.GlobalData.DocTypes, "D3", DBxAccessMode.None);
      ups.Add(p3);

      DocTypeViewHistoryPermission hp = new DocTypeViewHistoryPermission(info.GlobalData.DocTypes, "D1", false);
      ups.Add(hp);

      source.UserPermissions = ups;
      Assert.IsTrue(ups.IsReadOnly, "UserPermissions.IsReadOnly");
      return new DBxRealDocProvider(source, info.GlobalData.DocTypes.UseUsers ? 1 : 0, false);
    }


    [Test]
    public void DBPermissions()
    {
      TestDBInfo info = this[false, false, false, false];
      DBxDocProvider provider1 = info.Provider;
      Assert.AreEqual(DBxAccessMode.Full, provider1.DBPermissions.DBMode, "DBMode unlimited");
      Assert.AreEqual(DBxAccessMode.Full, provider1.DBPermissions.TableModes["D1"], "D1 unlimited");
      Assert.AreEqual(DBxAccessMode.Full, provider1.DBPermissions.TableModes["D2"], "D2 unlimited");
      Assert.AreEqual(DBxAccessMode.Full, provider1.DBPermissions.TableModes["D3"], "D3 unlimited");

      DBxDocProvider provider2 = CreateTestDocProviderWithPermissions(info);
      Assert.AreEqual(DBxAccessMode.Full, provider2.DBPermissions.DBMode, "DBMode limited");
      Assert.AreEqual(DBxAccessMode.Full, provider2.DBPermissions.TableModes["D1"], "D1 limited");
      Assert.AreEqual(DBxAccessMode.ReadOnly, provider2.DBPermissions.TableModes["D2"], "D2 limited");
      Assert.AreEqual(DBxAccessMode.None, provider2.DBPermissions.TableModes["D3"], "D3 limited");
    }

    #endregion

    #region Доступ к версиям документов

    [Test]
    public void GetDocHistTable()
    {
      TestDBInfo info = this[true, true, true, false];

      // 1. Создание документа
      Int32 docId = CreateTestDoc(info);
      DataTable table = info.Provider.GetDocHistTable("D1", docId);
      DBxColumns cols = DBxColumns.FromColumns(table.Columns);
      Assert.IsTrue(cols.Contains("UserActionId,Version,Action,UserActionId.StartTime,UserActionId.ActionTime,UserActionId.ActionInfo,UserActionId.ApplyChangesTime,UserActionId.ApplyChangesCount"), "Table columns");
      Assert.AreEqual(1, table.Rows.Count, "#1 Rows.Count");
      Assert.AreEqual(UndoAction.Insert, DataTools.GetEnum<UndoAction>(table.Rows[0], "Action"), "#1 Action");
      Assert.AreEqual(1, DataTools.GetInt(table.Rows[0], "Version"), "#1 Version");
      Assert.AreEqual(1, DataTools.GetInt(table.Rows[0], "UserActionId.ApplyChangesCount"), "# ApplyChangesCount");
      Int32 userActionId1 = DataTools.GetInt(table.Rows[0], "UserActionId");
      Assert.GreaterOrEqual(1, userActionId1, "#1 UserActionId");

      // 2. Изменение документа
      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);
      doc.Values["F102"].SetInteger(11);
      ds.ApplyChanges(true);

      table = info.Provider.GetDocHistTable("D1", docId);
      Assert.AreEqual(2, table.Rows.Count, "#2 Rows.Count");
      Assert.AreEqual(UndoAction.Edit, DataTools.GetEnum<UndoAction>(table.Rows[1], "Action"), "#2 Action");
      Assert.AreEqual(2, DataTools.GetInt(table.Rows[1], "Version"), "#2 Version");
      Assert.AreEqual(1, DataTools.GetInt(table.Rows[1], "UserActionId.ApplyChangesCount"), "#2 ApplyChangesCount");
      Int32 userActionId2 = DataTools.GetInt(table.Rows[1], "UserActionId");
      Assert.Greater(userActionId2, userActionId1, "#2 UserActionId");

      // 3. Повторная запись документа в том же сеансе - нет новой версии
      doc = ds["D1"][0];
      doc.Values["F102"].SetInteger(12);
      ds.ApplyChanges(true);

      table = info.Provider.GetDocHistTable("D1", docId);
      Assert.AreEqual(2, table.Rows.Count, "#3 Rows.Count");
      Assert.AreEqual(UndoAction.Edit, DataTools.GetEnum<UndoAction>(table.Rows[1], "Action"), "#3 Action");
      Assert.AreEqual(2, DataTools.GetInt(table.Rows[1], "Version"), "#3 Version");
      Assert.AreEqual(2, DataTools.GetInt(table.Rows[1], "UserActionId.ApplyChangesCount"), "#3 ApplyChangesCount");
      Int32 userActionId3 = DataTools.GetInt(table.Rows[1], "UserActionId");
      Assert.AreEqual(userActionId2, userActionId3, "#3 UserActionId");

      // 4. Удаление документа в том же сеансе - создается новая версия
      doc = ds["D1"][0];
      doc.Delete();
      ds.ApplyChanges(false);

      table = info.Provider.GetDocHistTable("D1", docId);
      Assert.AreEqual(3, table.Rows.Count, "#4 Rows.Count");
      Assert.AreEqual(UndoAction.Delete, DataTools.GetEnum<UndoAction>(table.Rows[2], "Action"), "#4 Action");
      Assert.AreEqual(3, DataTools.GetInt(table.Rows[2], "Version"), "#4 Version");
      Assert.AreEqual(3, DataTools.GetInt(table.Rows[2], "UserActionId.ApplyChangesCount"), "#4 ApplyChangesCount");
      Int32 userActionId4 = DataTools.GetInt(table.Rows[2], "UserActionId");
      Assert.AreEqual(userActionId2, userActionId4, "#4 UserActionId");

      // 5. Восстановление документа
      ds = new DBxDocSet(info.Provider);
      doc = ds["D1"].Edit(docId);
      ds.ApplyChanges(false);

      table = info.Provider.GetDocHistTable("D1", docId);
      Assert.AreEqual(4, table.Rows.Count, "#5 Rows.Count");
      Assert.AreEqual(UndoAction.Edit, DataTools.GetEnum<UndoAction>(table.Rows[3], "Action"), "#5 Action");
      Assert.AreEqual(4, DataTools.GetInt(table.Rows[3], "Version"), "#5 Version");
      Assert.AreEqual(1, DataTools.GetInt(table.Rows[3], "UserActionId.ApplyChangesCount"), "#5 ApplyChangesCount");
      Int32 userActionId5 = DataTools.GetInt(table.Rows[3], "UserActionId");
      Assert.Greater(userActionId5, userActionId4, "#5 UserActionId");

      // 6. Удаление документа
      ds = new DBxDocSet(info.Provider);
      ds["D1"].Delete(docId);
      ds.ApplyChanges(false);

      table = info.Provider.GetDocHistTable("D1", docId);
      Assert.AreEqual(5, table.Rows.Count, "#5 Rows.Count");
      Assert.AreEqual(UndoAction.Delete, DataTools.GetEnum<UndoAction>(table.Rows[4], "Action"), "#6 Action");
      Assert.AreEqual(5, DataTools.GetInt(table.Rows[4], "Version"), "#6 Version");
      Assert.AreEqual(1, DataTools.GetInt(table.Rows[4], "UserActionId.ApplyChangesCount"), "#6 ApplyChangesCount");
      Int32 userActionId6 = DataTools.GetInt(table.Rows[4], "UserActionId");
      Assert.Greater(userActionId6, userActionId5, "#6 UserActionId");
    }

    [Test]
    public void GetDocHistTable_Fail_FictiveDocId()
    {
      TestDBInfo info = this[true, true, true, false];
      Assert.Catch(delegate() { info.Provider.GetDocHistTable("D1", 100000); }, "Unknown DocId");
      Assert.Catch(delegate() { info.Provider.GetDocHistTable("D1", 0); }, "DocId=0");
      Assert.Catch(delegate() { info.Provider.GetDocHistTable("D1", -1); }, "Fictive DocId");
    }

    [Test]
    public void GetDocHistTable_Fail_Access()
    {
      TestDBInfo info = this[true, true, true, false];
      Int32[] docIds = CreateTest3Docs(info);

      DBxDocProvider provider2 = CreateTestDocProviderWithPermissions(info);
      Assert.Catch<DBxAccessException>(delegate() { provider2.GetDocHistTable("D1", docIds[0]); }, "D1 DocTypeViewHistoryPermission");
      Assert.DoesNotThrow(delegate() { provider2.GetDocHistTable("D2", docIds[1]); }, "D2");
      Assert.Catch<DBxAccessException>(delegate() { provider2.GetDocHistTable("D3", docIds[2]); }, "D3 DocTypePermission");
    }

    /// <summary>
    /// Создает 3 текстовых документа D1, D2 и D3
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public Int32[] CreateTest3Docs(TestDBInfo info)
    {
      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc1 = ds["D1"].Insert();
      doc1.Values["F101"].SetBoolean(false);
      doc1.Values["F102"].SetInteger(1);

      DBxSingleDoc doc2 = ds["D2"].Insert();
      doc2.Values["F201"].SetString("123");
      doc2.Values["F202"].SetInteger(doc1.DocId);

      DBxSingleDoc doc3 = ds["D3"].Insert();

      ds.ApplyChanges(true);

      return new Int32[3] { ds["D1"][0].DocId, ds["D2"][0].DocId, ds["D3"][0].DocId };
    }

    [Test]
    public void GetUserActionsTable()
    {
      TestDBInfo info = this[true, true, true, true];
      Int32 docId = CreateTestDoc(info);
      DataTable histTable = info.Provider.GetDocHistTable("D1", docId);
      Int32 userActionId = DataTools.GetInt(histTable.Rows[0], "UserActionId");

      DataTable uaTable = info.Provider.GetUserActionsTable(null, null, info.Provider.UserId, "D1");
      DBxColumns cols = DBxColumns.FromColumns(uaTable.Columns);
      Assert.IsTrue(cols.Contains("Id,StartTime,ActionTime,ActionInfo,ApplyChangesTime,ApplyChangesCount"), "Table columns");

      DataTools.CheckPrimaryKey(uaTable, "Id");
      DataRow uaRow = uaTable.Rows.Find(userActionId);
      Assert.IsNotNull(uaRow, "UserAction row");
    }

    [Test]
    public void GetUserActionDocTable()
    {
      TestDBInfo info = this[true, true, true, true];
      Int32 docId = CreateTestDoc(info);
      DataTable histTable = info.Provider.GetDocHistTable("D1", docId);
      Int32 userActionId = DataTools.GetInt(histTable.Rows[0], "UserActionId");

      DataTable docTable = info.Provider.GetUserActionDocTable(userActionId);
      DBxColumns cols = DBxColumns.FromColumns(docTable.Columns);
      Assert.IsTrue(cols.Contains("DocTableId,DocId,Version,Action"), "Table columns");

      Assert.AreEqual(1, docTable.Rows.Count, "Rows.Count");
      Assert.AreEqual(info.GlobalData.DocTypes["D1"].TableId, DataTools.GetInt(docTable.Rows[0], "DocTableId"), "DocTableId");
      Assert.AreEqual(docId, DataTools.GetInt(docTable.Rows[0], "DocId"), "DocId");
      Assert.AreEqual(1, DataTools.GetInt(docTable.Rows[0], "Version"), "Version");
    }

    [Test]
    public void GetDocRefTable()
    {
      TestDBInfo info = this[true, true, true, true];
      Int32[] docIds = CreateTest3Docs(info); // Есть ссылка в документе D2 на D1

      DataTable table = info.Provider.GetDocRefTable("D1", docIds[0], false, false);
      DBxColumns cols = DBxColumns.FromColumns(table.Columns);
      Assert.IsTrue(cols.Contains("FromDocTableId,FromDocId,FromDocDeleted,FromSubDocName,FromSubDocId,FromSubDocDeleted,FromDeleted,FromColumnName,ToSubDocName,ToSubDocId,ToSubDocDeleted,IsSameDoc"), "Table columns");

      Assert.AreEqual(1, table.Rows.Count, "Rows.Count");
      Assert.AreEqual(info.GlobalData.DocTypes["D2"].TableId, DataTools.GetInt(table.Rows[0], "FromDocTableId"), "FromDocTableId");
      Assert.AreEqual(docIds[1], DataTools.GetInt(table.Rows[0], "FromDocId"), "FromDocId");
      Assert.IsFalse(DataTools.GetBool(table.Rows[0], "FromDocDeleted"), "FromDocDeleted");
      Assert.AreEqual("", DataTools.GetString(table.Rows[0], "FromSubDocName"), "FromSubDocName");
      Assert.AreEqual(0, DataTools.GetInt(table.Rows[0], "FromSubDocId"), "FromSubDocId");
      Assert.IsFalse(DataTools.GetBool(table.Rows[0], "FromSubDocDeleted"), "FromSubDocDeleted");
      Assert.IsFalse(DataTools.GetBool(table.Rows[0], "FromDeleted"), "FromDeleted");
      Assert.AreEqual("F202", DataTools.GetString(table.Rows[0], "FromColumnName"), "FromColumnName");
      Assert.AreEqual("", DataTools.GetString(table.Rows[0], "ToSubDocName"), "ToSubDocName");
      Assert.AreEqual(0, DataTools.GetInt(table.Rows[0], "ToSubDocId"), "ToSubDocId");
      Assert.IsFalse(DataTools.GetBool(table.Rows[0], "ToSubDocDeleted"), "ToSubDocDeleted");
      Assert.IsFalse(DataTools.GetBool(table.Rows[0], "IsSameDoc"), "IsSameDoc");
    }

    [Test]
    public void GetDocRefTable_ShowDeleted()
    {
      TestDBInfo info = this[true, true, true, true];
      Int32[] docIds = CreateTest3Docs(info); // Есть ссылка в документе D2 на D1
      // Удаляем документ со ссылкой
      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds["D2"].Delete(docIds[1]);
      ds.ApplyChanges(false);


      DataTable table1 = info.Provider.GetDocRefTable("D1", docIds[0], true, false);
      Assert.AreEqual(1, table1.Rows.Count, "Rows.Count #1");
      Assert.AreEqual(info.GlobalData.DocTypes["D2"].TableId, DataTools.GetInt(table1.Rows[0], "FromDocTableId"), "FromDocTableId");
      Assert.AreEqual(docIds[1], DataTools.GetInt(table1.Rows[0], "FromDocId"), "FromDocId");
      Assert.IsTrue(DataTools.GetBool(table1.Rows[0], "FromDocDeleted"), "FromDocDeleted");
      Assert.IsFalse(DataTools.GetBool(table1.Rows[0], "FromSubDocDeleted"), "FromSubDocDeleted");
      Assert.IsTrue(DataTools.GetBool(table1.Rows[0], "FromDeleted"), "FromDeleted");

      DataTable table2 = info.Provider.GetDocRefTable("D1", docIds[0], false, false);
      Assert.AreEqual(0, table2.Rows.Count, "Rows.Count #2");
    }


    [Test]
    public void GetDocRefTable_FromSingleDocId()
    {
      TestDBInfo info = this[true, true, true, true];
      Int32[] docIds = CreateTest3Docs(info); // Есть ссылка в документе D2 на D1

      DataTable table1 = info.Provider.GetDocRefTable("D1", docIds[0], false, false, "D2", docIds[1]);
      Assert.AreEqual(1, table1.Rows.Count, "Rows.Count #1");

      DataTable table2 = info.Provider.GetDocRefTable("D1", docIds[0], false, false, "D3", docIds[2]);
      Assert.AreEqual(0, table2.Rows.Count, "Rows.Count #2");
    }


    [Test]
    public void GetDocRefTable_VTRef_1()
    {
      TestDBInfo info = this[true, true, true, true];
      Int32 docId1 = CreateTestDoc(info);

      // Документ D3 ссылается на D1
      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Insert();
      doc3.Values["F301TableId"].SetInteger(info.GlobalData.DocTypes["D1"].TableId);
      doc3.Values["F301DocId"].SetInteger(docId1);
      ds.ApplyChanges(true);
      Int32 docId3 = ds["D3"][0].DocId;

      DataTable table1 = info.Provider.GetDocRefTable("D1", docId1, false, false);
      Assert.AreEqual(1, table1.Rows.Count, "Rows.Count #1");
      Assert.AreEqual(info.GlobalData.DocTypes["D3"].TableId, DataTools.GetInt(table1.Rows[0], "FromDocTableId"), "FromDocTableId");
      Assert.AreEqual(docId3, DataTools.GetInt(table1.Rows[0], "FromDocId"), "FromDocId");
      Assert.AreEqual("F301DocId", DataTools.GetString(table1.Rows[0], "FromColumnName"), "FromColumnName");

      // Убираем ссылку
      ds = new DBxDocSet(info.Provider);
      doc3 = ds["D3"].Edit(docId3);
      doc3.Values["F301TableId"].SetNull();
      doc3.Values["F301DocId"].SetNull();
      ds.ApplyChanges(true);

      DataTable table2 = info.Provider.GetDocRefTable("D1", docId1, false, false);
      Assert.AreEqual(0, table2.Rows.Count, "Rows.Count #1");
    }

    [Test]
    public void GetDocRefTable_VTRef_2()
    {
      // 1. Документ D3 ссылается на D2
      TestDBInfo info = this[true, true, true, true];
      Int32[] docIds = CreateTest3Docs(info);
      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc3 = ds["D3"].Edit(docIds[2]);
      doc3.Values["F301TableId"].SetInteger(info.GlobalData.DocTypes["D2"].TableId);
      doc3.Values["F301DocId"].SetInteger(docIds[1]);
      ds.ApplyChanges(false);

      DataTable table1 = info.Provider.GetDocRefTable("D2", docIds[1], false, false);
      Assert.AreEqual(1, table1.Rows.Count, "Rows.Count #1");
      Assert.AreEqual(info.GlobalData.DocTypes["D3"].TableId, DataTools.GetInt(table1.Rows[0], "FromDocTableId"), "FromDocTableId");
      Assert.AreEqual(docIds[2], DataTools.GetInt(table1.Rows[0], "FromDocId"), "FromDocId");
      Assert.AreEqual("F301DocId", DataTools.GetString(table1.Rows[0], "FromColumnName"), "FromColumnName");

      // 1. Документ D3 ссылается на D1, а не D2
      ds = new DBxDocSet(info.Provider);
      doc3 = ds["D3"].Edit(docIds[2]);
      doc3.Values["F301TableId"].SetInteger(info.GlobalData.DocTypes["D1"].TableId);
      doc3.Values["F301DocId"].SetInteger(docIds[0]);
      ds.ApplyChanges(false);

      DataTable table2 = info.Provider.GetDocRefTable("D2", docIds[1], false, false);
      Assert.AreEqual(0, table2.Rows.Count, "Rows.Count #2");
    }

    #endregion
  }
}
