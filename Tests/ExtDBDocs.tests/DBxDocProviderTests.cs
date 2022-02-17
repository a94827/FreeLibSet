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

      DocTypeViewHistoryPermission hp = new DocTypeViewHistoryPermission(info.GlobalData.DocTypes, "D2", false);
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
      Int32 docId = CreateTestDoc(info);
      DataTable table = info.Provider.GetDocHistTable("D1", docId);
      DBxColumns cols = DBxColumns.FromColumns(table.Columns);
      Assert.IsTrue(cols.Contains("UserActionId,Version,Action,UserActionId.StartTime,UserActionId.ActionTime,UserActionId.ActionInfo,UserActionId.ApplyChangesTime,UserActionId.ApplyChangesCount"), "Table columns");
      Assert.AreEqual(1, table.Rows.Count, "#1 Rows.Count");
      Assert.AreEqual(UndoAction.Insert, DataTools.GetEnum<UndoAction>(table.Rows[0], "Action"));
    }

    #endregion
  }
}
