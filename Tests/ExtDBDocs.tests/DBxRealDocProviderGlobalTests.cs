using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;

namespace ExtDBDocs_tests.Data_Docs
{
  [TestFixture]
  public class DBxRealDocProviderGlobalTests
  {
    #region Конструктор
                    
    [Test]
    public void Constructor_simple()
    {
      DBxDocTypes dts = CreateTestDocTypes();
      using (DBx db = CreateTestDB())
      {
        db.CreateIfRequired();
        db.Struct = dts.GetMainDBStruct();
        db.UpdateStruct();

        DBxRealDocProviderGlobal sut = new DBxRealDocProviderGlobal(dts, db.MainEntry, null);

        Assert.AreSame(dts, sut.DocTypes, "DocTypes");
        Assert.IsTrue(sut.DocTypes.IsReadOnly, "DocTypes.IsReadOnly");
        Assert.AreSame(db.MainEntry, sut.MainDBEntry, "MainDBEntry");
        Assert.IsNull(sut.UndoDBEntry, "UndoDBEntry");
        Assert.IsNull(sut.BinDataHandler, "BinDataHandler");
        Assert.IsFalse(String.IsNullOrEmpty(sut.DBIdentity), "DBIdentity");
        Assert.AreEqual(0, sut.LongLocks.Count, "LongLocks.Count");
        Assert.AreEqual(db.DBIdentity, sut.DBCache.DBIdentity, "DBCache.DBIdentity");
        Assert.AreEqual(0, sut.ClearCacheBuffer.LastVersion, "ClearCacheBuffer.LastVersion");
        Assert.IsFalse(sut.TextHandlers.IsReadOnly, "TextHandlers.IsReadOnly");
        Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");

        DBx[] aDBs = sut.GetDBs();
        Assert.AreEqual(new DBx[1] { db }, aDBs, "GetDBs()");
      }
    }

    [Test]
    public void Constructor_with_undo()
    {
      DBxDocTypes dts = CreateTestDocTypes();
      using (DBx mainDB = CreateTestDB())
      {
        mainDB.CreateIfRequired();
        mainDB.Struct = dts.GetMainDBStruct();
        mainDB.UpdateStruct();

        using (DBx undoDB = CreateTestDB())
        {
          undoDB.CreateIfRequired();
          undoDB.Struct = dts.GetUndoDBStruct();
          undoDB.UpdateStruct();

          DBxRealDocProviderGlobal sut = new DBxRealDocProviderGlobal(dts, mainDB.MainEntry, undoDB.MainEntry);

          Assert.AreSame(mainDB.MainEntry, sut.MainDBEntry, "MainDBEntry");
          Assert.AreSame(undoDB.MainEntry, sut.UndoDBEntry, "UndoDBEntry");

          DBx[] aDBs = sut.GetDBs();
          Assert.AreEqual(new DBx[2] { mainDB,undoDB }, aDBs, "GetDBs()");
        }
      }
    }

    #endregion

    #region Вспомогательные методы

    private DBxDocTypes CreateTestDocTypes()
    {
      DBxDocTypes dts = new DBxDocTypes();
      dts.UsersTableName = String.Empty; // без пользователей
      DBxDocType dt;
      dt = new DBxDocType("D1");
      dt.Struct.Columns.AddString("F101", 20, false);
      dts.Add(dt);
      return dts;
    }

    private DBx CreateTestDB()
    {
      return DBxManager.Managers[DBxProviderNames.SQLite].CreateDBObject("Data Source=:memory:");
    }

    #endregion
  }
}
