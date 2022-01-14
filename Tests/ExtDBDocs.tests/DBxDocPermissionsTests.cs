using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.IO;
using NUnit.Framework;

namespace ExtDBDocs_tests.Data_Docs
{
  // TODO: Нужно больше тестов

  [TestFixture]
  public class DBxDocPermissionsTests
  {
    [Test]
    public void TestEdit()
    {
      Int32 docId1, docId2;
      Create2Docs(out docId1, out docId2);

      DBxDocProvider provider = new DBxRealDocProvider(_SourceLim, 0, true); // с ограничениями
      DBxDocSet ds = new DBxDocSet(provider);

      Assert.Catch<DBxAccessException>(delegate () { ds["Doc1"].Edit(docId1); });

      DBxSingleDoc doc2 = ds["Doc1"].Edit(docId2);
      Assert.AreEqual(2, doc2.Values["F2"].AsInteger, "F2 read #2");
    }

    /// <summary>
    /// Создает 2 новых документа, в первом F1=FALSE, во втором F1=TRUE.
    /// </summary>
    /// <param name="docId1"></param>
    /// <param name="docId2"></param>
    private void Create2Docs(out Int32 docId1, out Int32 docId2)
    {
      DBxDocProvider provider = new DBxRealDocProvider(_SourceAdm, 0, true); // без ограничений
      DBxDocSet ds = new DBxDocSet(provider);

      DBxSingleDoc doc1 = ds["Doc1"].Insert();
      doc1.Values["F1"].SetBoolean(false);
      doc1.Values["F2"].SetInteger(1);

      DBxSingleDoc doc2 = ds["Doc1"].Insert();
      doc2.Values["F1"].SetBoolean(true);
      doc2.Values["F2"].SetInteger(2);

      ds.ActionInfo = "Create2Docs";
      ds.ApplyChanges(true);

      doc1 = ds["Doc1"][0];
      doc2 = ds["Doc1"][1];

      Assert.IsTrue(provider.IsRealDocId(doc1.DocId), "IsRealDocId() #1");
      Assert.AreEqual(1, doc1.Values["F2"].AsInteger, "F2 written #1");
      Assert.IsTrue(provider.IsRealDocId(doc2.DocId), "IsRealDocId() #2");
      Assert.AreEqual(2, doc2.Values["F2"].AsInteger, "F2 written #2");

      docId1 = doc1.DocId;
      docId2 = doc2.DocId;
    }

    [Test]
    public void Remove_Lock_And_Change_While_Other_Editing()
    {
      DBxDocProvider providerAdm = new DBxRealDocProvider(_SourceAdm, 0, true); // без ограничений
      DBxDocProvider providerLim = new DBxRealDocProvider(_SourceLim, 0, true); // с ограничениями

      // 1. Создаем документ с F1=TRUE, который может редактировать "ограниченный пользоватеь"
      DBxDocSet ds1 = new DBxDocSet(providerLim);
      DBxSingleDoc doc1 = ds1["Doc1"].Insert();
      doc1.Values["F1"].SetBoolean(true);
      doc1.Values["F2"].SetInteger(1);
      ds1.ApplyChanges(true);

      Int32 docId = ds1["Doc1"].DocIds[0];
      ds1 = null;
      Assert.IsTrue(providerLim.IsRealDocId(doc1.DocId), "IsRealDocId()");
      Assert.AreEqual(1, providerLim.GetValue("Doc1", docId, "F2"), "F2 #1");

      // 2. Открываем документ на редактирование, устанавливаем блокировку. Запись документа возможна.
      DBxDocSet ds2 = new DBxDocSet(providerLim);
      Guid lockGuid2 = ds2.AddLongLock();
      DBxSingleDoc doc2 = ds2["Doc1"].Edit(docId);
      doc2.Values["F2"].SetInteger(2);
      ds2.ApplyChanges(true);
      Assert.AreEqual(2, providerLim.GetValue("Doc1", docId, "F2"), "F2 #2");

      // 3. Снимаем блокировку от имени пользователя-"администратора"
      providerAdm.RemoveLongLock(lockGuid2); // в реальном коде еще нужно и найти идентификатор блокировки
      Assert.AreEqual(2, providerLim.GetValue("Doc1", docId, "F2"), "F2 #3");

      // 4. Обычный пользователь ничего не подозревает и продолжает редактирование, еще раз сохраняет документ, хотя блокировка уже утеряна
      doc2.Values["F2"].SetInteger(4);
      ds2.ApplyChanges(true);
      Assert.AreEqual(4, providerLim.GetValue("Doc1", docId, "F2"), "F2 #4");


      // 5. Администратор меняет документ, делая его недоступным для изменения обычным пользователем
      DBxDocSet ds5 = new DBxDocSet(providerAdm);
      DBxSingleDoc doc5 = ds5["Doc1"].Edit(docId);
      doc5.Values["F1"].SetBoolean(false);
      doc5.Values["F2"].SetInteger(5);
      ds5.ApplyChanges(false);
      Assert.AreEqual(5, providerLim.GetValue("Doc1", docId, "F2"), "F2 #5");

      // 6.Обычный пользователь опять пытается сохранить документ
      doc2.Values["F2"].SetInteger(6);
      Assert.Catch<DBxAccessException>(delegate () { ds2.ApplyChanges(true); }, "Save #6");

      // Проверяем, что ничего не испортилось
      Assert.AreEqual(5, providerLim.GetValue("Doc1", docId, "F2"), "F2 #6");
    }


    #region Тестовая база данных

    private TempDirectory _TempDir;
    private DBxRealDocProviderGlobal _GlobalData;

    /// <summary>
    /// Источник для пользователя с полными правами
    /// </summary>
    DBxRealDocProviderSource _SourceAdm;

    /// <summary>
    /// Источник для пользователя с правами записи документов только с F1=TRUE
    /// </summary>
    DBxRealDocProviderSource _SourceLim;

    private class TestUserPermission2 : UserPermission, IDBxDocPermission
    {
      #region Конструктор

      public TestUserPermission2()
        : base("Test2")
      {
      }

      #endregion

      #region Переопределенные методы - заглушки

      public override string ValueText { get { return String.Empty; } }

      public override void Read(CfgPart cfg)
      {
      }

      public override void Write(CfgPart cfg)
      {
      }

      #endregion

      #region Основной метод

      public const string ErrorMessage = "Cannot change doc with F1=false";

      public string TableNames { get { return "Doc1"; } }


      public void TestDocAllowed(DBxDocPermissionArgs args)
      {
        if (!args.IsReadOnly)
        {
          if (!args.Values["F1"].AsBoolean)
            args.ErrorMessages["Key2"] = ErrorMessage;
        }
      }

      #endregion
    }

    [OneTimeSetUp]
    public void Setup()
    {
      _TempDir = new TempDirectory();

      DBxDocTypes dts = new DBxDocTypes();
      dts.UsersTableName = String.Empty; // без пользователей
      DBxDocType dt;
      //DBxSubDocType sdt;

      dt = new DBxDocType("Doc1");
      dt.Struct.Columns.AddBoolean("F1", false);
      dt.Struct.Columns.AddInt("F2", false);
      dts.Add(dt);

      DBxDocDBConnectionHelper conHelper = new DBxDocDBConnectionHelper();
      conHelper.ProviderName = "SQLite";
      conHelper.ConnectionString = "Data Source=" + new AbsPath(_TempDir.Dir, "db.db").Path;
      conHelper.DocTypes = dts;

      _GlobalData = conHelper.CreateRealDocProviderGlobal();

      _SourceAdm = new DBxRealDocProviderSource(_GlobalData);

      UserPermissions ups = new UserPermissions(UserPermissionCreators.Empty);
      ups.Add(new TestUserPermission2());

      _SourceLim = new DBxRealDocProviderSource(_GlobalData);
      _SourceLim.UserPermissions = ups;

    }

    [OneTimeTearDown]
    public void TearDown()
    {
      if (_GlobalData != null)
      {
        _GlobalData.DisposeDBs();
        _GlobalData = null;
      }

      if (_TempDir != null)
      {
        _TempDir.Dispose();
        _TempDir = null;
      }
    }

    #endregion
  }
}
