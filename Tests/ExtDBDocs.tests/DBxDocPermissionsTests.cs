using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.IO;
using NUnit.Framework;
using FreeLibSet.Core;

namespace ExtDBDocs_tests.Data_Docs
{
  /// <summary>
  /// Проверка тестирования документов с помощью IDBxDocPermission.TestDocument()
  /// </summary>
  [TestFixture]
  public class DBxDocPermissionsTests
  {
    #region Тестирование вызовов TestDocAllowed()

    #region View()

    [TestCase(false)]
    [TestCase(true)]
    public void TestDocAllowed_View_ById(bool sideChange)
    {
      Int32 docId = Create1Doc(1);

      using (new TestWithTrace())
      {
        DBxDocProvider provider = new DBxRealDocProvider(_SourceWithTrace, 0, true);
        DBxDocSet ds = new DBxDocSet(provider);

        DBxSingleDoc doc = ds["TestDocs"].View(docId);
        TestWithTrace.AssertValues("View()",
          new OneCallInfo(DBxDocPermissionReason.View, 1));

        PerformSideChange(docId, sideChange);

        ds.ApplyChanges(false);
        TestWithTrace.AssertValues("ApplyChanges()");
      }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void TestDocAllowed_View_FromEdit(bool sideChange)
    {
      Int32 docId = Create1Doc(1);

      using (new TestWithTrace())
      {
        DBxDocProvider provider = new DBxRealDocProvider(_SourceWithTrace, 0, true);
        DBxDocSet ds = new DBxDocSet(provider);

        DBxSingleDoc doc = ds["TestDocs"].Edit(docId);
        TestWithTrace.LogValues.Clear(); // вызов Edit здесь не нужно тестировать

        doc.View();
        TestWithTrace.AssertValues("View()");

        PerformSideChange(docId, sideChange);

        ds.ApplyChanges(false);
        TestWithTrace.AssertValues("ApplyChanges()");
      }
    }

    #endregion

    #region Insert()

    [Test]
    public void TestDocAllowed_Insert()
    {
      using (new TestWithTrace())
      {
        DBxDocProvider provider = new DBxRealDocProvider(_SourceWithTrace, 0, true);
        DBxDocSet ds = new DBxDocSet(provider);

        DBxSingleDoc doc = ds["TestDocs"].Insert();
        doc.Values["F2"].SetInteger(1);
        TestWithTrace.AssertValues("Insert()");

        ds.ApplyChanges(false);
        TestWithTrace.AssertValues("ApplyChanges()",
          new OneCallInfo(DBxDocPermissionReason.ApplyNew, 1));
      }
    }

    #endregion

    #region Edit()

    [TestCase(false)]
    [TestCase(true)]
    public void TestDocAllowed_Edit_ById(bool sideChange)
    {
      Int32 docId = Create1Doc(1);

      using (new TestWithTrace())
      {
        DBxDocProvider provider = new DBxRealDocProvider(_SourceWithTrace, 0, true);
        DBxDocSet ds = new DBxDocSet(provider);

        DBxSingleDoc doc = ds["TestDocs"].Edit(docId);
        TestWithTrace.AssertValues("Edit()",
          new OneCallInfo(DBxDocPermissionReason.View, 1),
          new OneCallInfo(DBxDocPermissionReason.BeforeEdit, 1));

        PerformSideChange(docId, sideChange);

        doc.Values["F2"].SetInteger(2);
        ds.ApplyChanges(false);
        TestWithTrace.AssertValues("ApplyChanges()",
          new OneCallInfo(DBxDocPermissionReason.ApplyEditOrg, sideChange ? 99 : 1),
          new OneCallInfo(DBxDocPermissionReason.ApplyEditNew, 2));
      }
    }

    [Test]
    public void TestDocAllowed_Edit_ById_Restore()
    {
      Int32 docId = Create1Doc(1);
      Delete1Doc(docId);

      using (new TestWithTrace())
      {
        DBxDocProvider provider = new DBxRealDocProvider(_SourceWithTrace, 0, true);
        DBxDocSet ds = new DBxDocSet(provider);

        DBxSingleDoc doc = ds["TestDocs"].Edit(docId);
        TestWithTrace.AssertValues("Edit()",
          new OneCallInfo(DBxDocPermissionReason.View, 1),
          new OneCallInfo(DBxDocPermissionReason.BeforeRestore, 1),
          new OneCallInfo(DBxDocPermissionReason.BeforeEdit, 1));

        doc.Values["F2"].SetInteger(2);
        ds.ApplyChanges(false);
        TestWithTrace.AssertValues("ApplyChanges()",
          new OneCallInfo(DBxDocPermissionReason.ApplyRestore, 2),
          new OneCallInfo(DBxDocPermissionReason.ApplyEditOrg, 1),
          new OneCallInfo(DBxDocPermissionReason.ApplyEditNew, 2));
      }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void TestDocAllowed_Edit_ByView(bool sideChange)
    {
      Int32 docId = Create1Doc(1);

      using (new TestWithTrace())
      {
        DBxDocProvider provider = new DBxRealDocProvider(_SourceWithTrace, 0, true);
        DBxDocSet ds = new DBxDocSet(provider);

        DBxSingleDoc doc = ds["TestDocs"].View(docId);
        TestWithTrace.LogValues.Clear(); // Метод View() здесь не тестируем

        doc.Edit();
        TestWithTrace.AssertValues("Edit()",
          new OneCallInfo(DBxDocPermissionReason.BeforeEdit, 1));

        PerformSideChange(docId, sideChange);

        doc.Values["F2"].SetInteger(2);
        ds.ApplyChanges(false);
        TestWithTrace.AssertValues("ApplyChanges()",
          new OneCallInfo(DBxDocPermissionReason.ApplyEditOrg, sideChange ? 99 : 1),
          new OneCallInfo(DBxDocPermissionReason.ApplyEditNew, 2));
      }
    }

    [Test]
    public void TestDocAllowed_Edit_ByView_Restore()
    {
      Int32 docId = Create1Doc(1);
      Delete1Doc(docId);

      using (new TestWithTrace())
      {
        DBxDocProvider provider = new DBxRealDocProvider(_SourceWithTrace, 0, true);
        DBxDocSet ds = new DBxDocSet(provider);

        DBxSingleDoc doc = ds["TestDocs"].View(docId);
        TestWithTrace.LogValues.Clear(); // Метод View() здесь не тестируем

        doc.Edit();
        TestWithTrace.AssertValues("Edit()",
          new OneCallInfo(DBxDocPermissionReason.BeforeRestore, 1),
          new OneCallInfo(DBxDocPermissionReason.BeforeEdit, 1));

        doc.Values["F2"].SetInteger(2);
        ds.ApplyChanges(false);
        TestWithTrace.AssertValues("ApplyChanges()",
          new OneCallInfo(DBxDocPermissionReason.ApplyRestore, 2),
          new OneCallInfo(DBxDocPermissionReason.ApplyEditOrg, 1),
          new OneCallInfo(DBxDocPermissionReason.ApplyEditNew, 2));
      }
    }

    #endregion

    #region Delete()

    [TestCase(false)]
    [TestCase(true)]
    public void TestDocAllowed_Delete_ById(bool sideChange)
    {
      Int32 docId = Create1Doc(1);

      using (new TestWithTrace())
      {
        DBxDocProvider provider = new DBxRealDocProvider(_SourceWithTrace, 0, true);
        DBxDocSet ds = new DBxDocSet(provider);

        ds["TestDocs"].Delete(docId);
        TestWithTrace.AssertValues("Delete()",
          new OneCallInfo(DBxDocPermissionReason.View, 1),
          new OneCallInfo(DBxDocPermissionReason.BeforeDelete, 1));

        PerformSideChange(docId, sideChange);

        ds.ApplyChanges(false);
        TestWithTrace.AssertValues("ApplyChanges()",
          new OneCallInfo(DBxDocPermissionReason.ApplyDelete, sideChange ? 99 : 1));
      }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void TestDocAllowed_Delete_ByView(bool sideChange)
    {
      Int32 docId = Create1Doc(1);

      using (new TestWithTrace())
      {
        DBxDocProvider provider = new DBxRealDocProvider(_SourceWithTrace, 0, true);
        DBxDocSet ds = new DBxDocSet(provider);

        DBxSingleDoc doc = ds["TestDocs"].View(docId);
        TestWithTrace.LogValues.Clear(); // Метод View() здесь не тестируем

        doc.Delete();
        TestWithTrace.AssertValues("Delete()",
          new OneCallInfo(DBxDocPermissionReason.BeforeDelete, 1));

        PerformSideChange(docId, sideChange);

        ds.ApplyChanges(false);
        TestWithTrace.AssertValues("ApplyChanges()",
          new OneCallInfo(DBxDocPermissionReason.ApplyDelete, sideChange ? 99 : 1));
      }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void TestDocAllowed_Delete_ByAllView(bool sideChange)
    {
      Int32 docId = Create1Doc(1);

      using (new TestWithTrace())
      {
        DBxDocProvider provider = new DBxRealDocProvider(_SourceWithTrace, 0, true);
        DBxDocSet ds = new DBxDocSet(provider);

        ds["TestDocs"].View(docId);
        TestWithTrace.LogValues.Clear(); // Метод View() здесь не тестируем

        ds["TestDocs"].Delete();
        TestWithTrace.AssertValues("Delete()",
          new OneCallInfo(DBxDocPermissionReason.BeforeDelete, 1));

        PerformSideChange(docId, sideChange);

        ds.ApplyChanges(false);
        TestWithTrace.AssertValues("ApplyChanges()",
          new OneCallInfo(DBxDocPermissionReason.ApplyDelete, sideChange ? 99 : 1));
      }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void TestDocAllowed_Delete_ByEdit(bool sideChange)
    {
      Int32 docId = Create1Doc(1);

      using (new TestWithTrace())
      {
        DBxDocProvider provider = new DBxRealDocProvider(_SourceWithTrace, 0, true);
        DBxDocSet ds = new DBxDocSet(provider);

        DBxSingleDoc doc = ds["TestDocs"].Edit(docId);
        TestWithTrace.LogValues.Clear(); // Метод Edit() здесь не тестируем


        doc.Delete();
        TestWithTrace.AssertValues("Delete()",
          new OneCallInfo(DBxDocPermissionReason.BeforeDelete, 1));

        PerformSideChange(docId, sideChange);

        ds.ApplyChanges(false);
        TestWithTrace.AssertValues("ApplyChanges()",
          new OneCallInfo(DBxDocPermissionReason.ApplyDelete, sideChange ? 99 : 1));
      }
    }

    #endregion

    #region ViewVersion()

    [Test]
    public void TestDocAllowed_ViewVersion()
    {
      Int32 docId = Create1Doc(1);
      DBxDocProvider provider0 = new DBxRealDocProvider(_SourceAdm, 0, true);
      DBxDocSet ds0 = new DBxDocSet(provider0);
      ds0["TestDocs"].Edit(docId).Values["F2"].SetInteger(2);
      ds0.ApplyChanges(false);

      using (new TestWithTrace())
      {
        DBxDocProvider provider = new DBxRealDocProvider(_SourceWithTrace, 0, true);
        DBxDocSet ds = new DBxDocSet(provider);

        DBxSingleDoc doc = ds["TestDocs"].ViewVersion(docId, 1);
        TestWithTrace.AssertValues("ViewVersion()",
          new OneCallInfo(DBxDocPermissionReason.View, 1));

        ds.ApplyChanges(false);
        TestWithTrace.AssertValues("ApplyChanges()");
      }
    }

    #endregion

    #region GetDocHistTable()

    [Test]
    public void TestDocAllowed_GetDocHistTable()
    {
      Int32 docId = Create1Doc(1);

      using (new TestWithTrace())
      {
        DBxDocProvider provider = new DBxRealDocProvider(_SourceWithTrace, 0, true);
        provider.GetDocHistTable("TestDocs", docId);
        TestWithTrace.AssertValues("GetDocHistTable()",
          new OneCallInfo(DBxDocPermissionReason.View, 1),
          new OneCallInfo(DBxDocPermissionReason.ViewHistory, 1));
      }
    }

    #endregion

    #region Вспомогательные методы

    private Int32 Create1Doc(int valueF2)
    {
      DBxDocProvider provider = new DBxRealDocProvider(_SourceAdm, 0, true); // без ограничений
      DBxDocSet ds = new DBxDocSet(provider);

      DBxSingleDoc doc = ds["TestDocs"].Insert();
      doc.Values["F2"].SetInteger(valueF2);
      ds.ApplyChanges(true);
      return ds["TestDocs"][0].DocId;
    }

    private void Delete1Doc(Int32 docId)
    {
      DBxDocProvider provider = new DBxRealDocProvider(_SourceAdm, 0, true);
      DBxDocSet ds = new DBxDocSet(provider);
      ds["TestDocs"].Delete(docId);
      ds.ApplyChanges(false);
    }

    /// <summary>
    /// Эмуляция стороннего изменения документа.
    /// Если стороннее изменение тестируется, то в поле "F2" записывается значение 99
    /// </summary>
    /// <param name="docId">Идентификатор документа</param>
    /// <param name="sideChange">Нужно ли выполнить эмуляцию</param>
    private void PerformSideChange(Int32 docId, bool sideChange)
    {
      if (!sideChange)
        return;

      DBxDocProvider provider = new DBxRealDocProvider(_SourceAdm, 0, true);
      DBxDocSet ds = new DBxDocSet(provider);
      DBxSingleDoc doc = ds["TestDocs"].Edit(docId);
      doc.Values["F2"].SetInteger(99);
      ds.ApplyChanges(false);
    }

    #endregion

    #endregion

    #region Тестирование разрешений

    [Test]
    public void TestEdit()
    {
      Int32 docId1, docId2;
      Create2Docs(out docId1, out docId2);

      // Первый документ нельзя редактировать, а второй - можно

      DBxDocProvider provider = new DBxRealDocProvider(_SourceLim, 0, true); // с ограничениями
      DBxDocSet ds = new DBxDocSet(provider);

      Assert.Catch<DBxAccessException>(delegate () { ds["TestDocs"].Edit(docId1); });

      DBxSingleDoc doc2 = ds["TestDocs"].Edit(docId2);
      Assert.AreEqual(2, doc2.Values["F2"].AsInteger, "F2 read #2");
    }

    [Test]
    public void TestView()
    {
      Int32 docId1, docId2;
      Create2Docs(out docId1, out docId2);

      // Оба документа можно смотреть

      DBxDocProvider provider = new DBxRealDocProvider(_SourceLim, 0, true); // с ограничениями
      DBxDocSet ds = new DBxDocSet(provider);

      DBxSingleDoc doc1 = ds["TestDocs"].View(docId1);
      Assert.AreEqual(1, doc1.Values["F2"].AsInteger, "F2 read #1");

      DBxSingleDoc doc2 = ds["TestDocs"].Edit(docId2);
      Assert.AreEqual(2, doc2.Values["F2"].AsInteger, "F2 read #2");
    }

    [Test]
    public void TestDelete_ByView()
    {
      Int32 docId1, docId2;
      Create2Docs(out docId1, out docId2);

      // Первый документ нельзя удалить, а второй - можно

      DBxDocProvider provider = new DBxRealDocProvider(_SourceLim, 0, true); // с ограничениями
      DBxDocSet ds1 = new DBxDocSet(provider);
      Assert.Catch<DBxAccessException>(delegate ()
      {
        DBxSingleDoc doc1 = ds1["TestDocs"].View(docId1);
        doc1.Delete();
        ds1.ApplyChanges(false);
      });

      DBxDocSet ds2 = new DBxDocSet(provider);
      DBxSingleDoc doc2 = ds2["TestDocs"].Edit(docId2);
      doc2.Delete();
      ds2.ApplyChanges(false);
    }

    [Test]
    public void TestDelete_ByEdit()
    {
      Int32 docId1, docId2;
      Create2Docs(out docId1, out docId2);

      // Первый документ нельзя удалить, а второй - можно

      DBxDocProvider provider = new DBxRealDocProvider(_SourceLim, 0, true); // с ограничениями
      DBxDocSet ds1 = new DBxDocSet(provider);
      Assert.Catch<DBxAccessException>(delegate ()
      {
        DBxSingleDoc doc1 = ds1["TestDocs"].Edit(docId1);
        doc1.Delete();
        ds1.ApplyChanges(false);
      });

      DBxDocSet ds2 = new DBxDocSet(provider);
      DBxSingleDoc doc2 = ds2["TestDocs"].Edit(docId2);
      doc2.Delete();
      ds2.ApplyChanges(false);
    }

    [Test]
    public void TestDelete_Directly()
    {
      Int32 docId1, docId2;
      Create2Docs(out docId1, out docId2);

      // Первый документ нельзя удалить, а второй - можно

      DBxDocProvider provider = new DBxRealDocProvider(_SourceLim, 0, true); // с ограничениями
      DBxDocSet ds1 = new DBxDocSet(provider);
      Assert.Catch<DBxAccessException>(delegate ()
      {
        ds1["TestDocs"].Delete(docId1);
        ds1.ApplyChanges(false);
      });

      DBxDocSet ds2 = new DBxDocSet(provider);
      ds2["TestDocs"].Delete(docId2);
      ds2.ApplyChanges(false);
    }

    [Test]
    public void TestInsert()
    {
      DBxDocProvider provider = new DBxRealDocProvider(_SourceLim, 0, true); // с ограничениями
      DBxDocSet ds1 = new DBxDocSet(provider);
      DBxSingleDoc doc1 = ds1["TestDocs"].Insert();
      doc1.Values["F1"].SetBoolean(false);
      Assert.Catch<DBxAccessException>(delegate () { ds1.ApplyChanges(false); }, "#1");

      DBxDocSet ds2 = new DBxDocSet(provider);
      DBxSingleDoc doc2 = ds1["TestDocs"].Insert();
      doc2.Values["F1"].SetBoolean(true);
      ds2.ApplyChanges(false);
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

      DBxSingleDoc doc1 = ds["TestDocs"].Insert();
      doc1.Values["F1"].SetBoolean(false);
      doc1.Values["F2"].SetInteger(1);

      DBxSingleDoc doc2 = ds["TestDocs"].Insert();
      doc2.Values["F1"].SetBoolean(true);
      doc2.Values["F2"].SetInteger(2);

      ds.ActionInfo = "Create2Docs";
      ds.ApplyChanges(true);

      doc1 = ds["TestDocs"][0];
      doc2 = ds["TestDocs"][1];

      Assert.IsTrue(provider.IsRealDocId(doc1.DocId), "IsRealDocId() #1");
      Assert.AreEqual(1, doc1.Values["F2"].AsInteger, "F2 written #1");
      Assert.IsTrue(provider.IsRealDocId(doc2.DocId), "IsRealDocId() #2");
      Assert.AreEqual(2, doc2.Values["F2"].AsInteger, "F2 written #2");

      docId1 = doc1.DocId;
      docId2 = doc2.DocId;
    }

    #endregion

    #region Тестирование специальной ошибочной ситуации

    [Test]
    public void Remove_Lock_And_Change_While_Other_Editing()
    {
      DBxDocProvider providerAdm = new DBxRealDocProvider(_SourceAdm, 0, true); // без ограничений
      DBxDocProvider providerLim = new DBxRealDocProvider(_SourceLim, 0, true); // с ограничениями

      // 1. Создаем документ с F1=TRUE, который может редактировать "ограниченный пользоватеь"
      DBxDocSet ds1 = new DBxDocSet(providerLim);
      DBxSingleDoc doc1 = ds1["TestDocs"].Insert();
      doc1.Values["F1"].SetBoolean(true);
      doc1.Values["F2"].SetInteger(1);
      ds1.ApplyChanges(true);

      Int32 docId = ds1["TestDocs"].DocIds[0];
      ds1 = null;
      Assert.IsTrue(providerLim.IsRealDocId(doc1.DocId), "IsRealDocId()");
      Assert.AreEqual(1, providerLim.GetValue("TestDocs", docId, "F2"), "F2 #1");

      // 2. Открываем документ на редактирование, устанавливаем блокировку. Запись документа возможна.
      DBxDocSet ds2 = new DBxDocSet(providerLim);
      Guid lockGuid2 = ds2.AddLongLock();
      DBxSingleDoc doc2 = ds2["TestDocs"].Edit(docId);
      doc2.Values["F2"].SetInteger(2);
      ds2.ApplyChanges(true);
      Assert.AreEqual(2, providerLim.GetValue("TestDocs", docId, "F2"), "F2 #2");

      // 3. Снимаем блокировку от имени пользователя-"администратора"
      providerAdm.RemoveLongLock(lockGuid2); // в реальном коде еще нужно и найти идентификатор блокировки
      Assert.AreEqual(2, providerLim.GetValue("TestDocs", docId, "F2"), "F2 #3");

      // 4. Обычный пользователь ничего не подозревает и продолжает редактирование, еще раз сохраняет документ, хотя блокировка уже утеряна
      doc2.Values["F2"].SetInteger(4);
      ds2.ApplyChanges(true);
      Assert.AreEqual(4, providerLim.GetValue("TestDocs", docId, "F2"), "F2 #4");


      // 5. Администратор меняет документ, делая его недоступным для изменения обычным пользователем
      DBxDocSet ds5 = new DBxDocSet(providerAdm);
      DBxSingleDoc doc5 = ds5["TestDocs"].Edit(docId);
      doc5.Values["F1"].SetBoolean(false);
      doc5.Values["F2"].SetInteger(5);
      ds5.ApplyChanges(false);
      Assert.AreEqual(5, providerLim.GetValue("TestDocs", docId, "F2"), "F2 #5");

      // 6.Обычный пользователь опять пытается сохранить документ
      doc2.Values["F2"].SetInteger(6);
      Assert.Catch<DBxAccessException>(delegate () { ds2.ApplyChanges(true); }, "Save #6");

      // Проверяем, что ничего не испортилось
      Assert.AreEqual(5, providerLim.GetValue("TestDocs", docId, "F2"), "F2 #6");
    }

    #endregion

    #region Тестовая база данных

    //private TempDirectory _TempDir;
    private DBxRealDocProviderGlobal _GlobalData;

    /// <summary>
    /// Источник для пользователя с полными правами
    /// </summary>
    DBxRealDocProviderSource _SourceAdm;

    /// <summary>
    /// Источник для пользователя с правами записи документов только с F1=TRUE
    /// </summary>
    DBxRealDocProviderSource _SourceLim;

    /// <summary>
    /// Источник с поддержкой трассировки TestUserPermissionWithTrace 
    /// </summary>
    DBxRealDocProviderSource _SourceWithTrace;

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

      public string TableNames { get { return "TestDocs"; } }


      public void TestDocAllowed(DBxDocPermissionArgs args)
      {
        object F1 = args.Values["F1"].Value;
        object F2 = args.Values["F2"].Value;

        if (!args.IsReadOnly)
        {
          if (!args.Values["F1"].AsBoolean)
            args.ErrorMessages["Key2"] = ErrorMessage;
        }
      }

      #endregion
    }

    /// <summary>
    /// Псевдоразрешение, которое выполняет трассировку вызова метода TestDocAllowed().
    /// Для каждого вызова TestDocAllowed() записываются данные в TestWithTrace.LogValues
    /// </summary>
    private class TestUserPermissionWithTrace : UserPermission, IDBxDocPermission
    {
      #region Конструктор

      public TestUserPermissionWithTrace()
        : base("TestLog")
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

      public string TableNames { get { return "TestDocs"; } }


      public void TestDocAllowed(DBxDocPermissionArgs args)
      {
        if (TestWithTrace.LogValues == null)
          throw new NullReferenceException("Bug in a test. Log=null.");

        TestWithTrace.LogValues.Add(new OneCallInfo(args.Reason, args.Values["F2"].AsInteger));
      }

      #endregion
    }

    /// <summary>
    /// Информация по одному вызову метода TestDocAllowed().
    /// </summary>
    public struct OneCallInfo
    {
      #region Конструктор

      public OneCallInfo(DBxDocPermissionReason reason, int f2)
      {
        _Reason = reason;
        _F2 = f2;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Режим вызова
      /// </summary>
      public DBxDocPermissionReason Reason { get { return _Reason; } }
      private DBxDocPermissionReason _Reason;

      /// <summary>
      /// Значение поля "F2" в Values
      /// </summary>
      public int F2 { get { return _F2; } }
      private int _F2;

      #endregion
    }

    public class TestWithTrace : IDisposable
    {
      #region Конструктор и Dispose

      public TestWithTrace()
      {
        _LogValues = new List<OneCallInfo>();
      }

      public void Dispose()
      {
        _LogValues = null;
      }

      #endregion

      #region Трассировка

      /// <summary>
      /// Словарь для регистрации вызовов метода TestDocAllowed().
      /// Ключ в словаре - значение Reason, значение - поле "F2"
      /// </summary>
      public static IList<OneCallInfo> LogValues { get { return _LogValues; } }
      [ThreadStatic] // тесты могут выполняться в нескольких потоках
      private static List<OneCallInfo> _LogValues;

      /// <summary>
      /// Выполняет проверку значений.
      /// После этого очищает текущий список LogValues
      /// </summary>
      /// <param name="message">Дополнительное сообщение</param>
      /// <param name="wanted">Ожидавшиеся вызовы</param>
      public static void AssertValues(string message, params OneCallInfo[] wanted)
      {
        if (!String.IsNullOrEmpty(message))
          message = message + ". ";

        try
        {
          DBxDocPermissionReason[] a1 = new DBxDocPermissionReason[wanted.Length];
          for (int i = 0; i < wanted.Length; i++)
            a1[i] = wanted[i].Reason;

          DBxDocPermissionReason[] a2 = new DBxDocPermissionReason[LogValues.Count];
          for (int i = 0; i < LogValues.Count; i++)
            a2[i] = LogValues[i].Reason;

          Assert.AreEqual(a1, a2, message + "Reasons differ");

          int n = Math.Max(wanted.Length, _LogValues.Count);

          for (int i = 0; i < wanted.Length; i++)
            Assert.AreEqual(wanted[i].F2, LogValues[i].F2, message + "Value F2 for reason=" + wanted[i].Reason.ToString());
        }
        finally
        {
          LogValues.Clear();
        }
      }

      #endregion
    }

    #region Setup() / TearDown()

    [OneTimeSetUp]
    public void Setup()
    {
      //_TempDir = new TempDirectory();

      DBxDocTypes dts = new DBxDocTypes();
      dts.UsersTableName = String.Empty; // без пользователей
      DBxDocType dt;
      //DBxSubDocType sdt;

      dt = new DBxDocType("TestDocs");
      dt.Struct.Columns.AddBoolean("F1", false);
      dt.Struct.Columns.AddInt("F2", false);
      dts.Add(dt);

      DBxDocDBConnectionHelper conHelper = new DBxDocDBConnectionHelper();
      conHelper.ProviderName = "SQLite";
      //conHelper.ConnectionString = "Data Source=" + new AbsPath(_TempDir.Dir, "db.db").Path;
      conHelper.ConnectionString = "Data Source=:memory:";// +new AbsPath(_TempDir.Dir, "db.db").Path;
      conHelper.DocTypes = dts;

      _GlobalData = conHelper.CreateRealDocProviderGlobal();

      _SourceAdm = new DBxRealDocProviderSource(_GlobalData);

      UserPermissions ups2 = new UserPermissions(UserPermissionCreators.Empty);
      ups2.Add(new TestUserPermission2());

      _SourceLim = new DBxRealDocProviderSource(_GlobalData);
      _SourceLim.UserPermissions = ups2;


      UserPermissions upsWT = new UserPermissions(UserPermissionCreators.Empty);
      upsWT.Add(new TestUserPermissionWithTrace());

      _SourceWithTrace = new DBxRealDocProviderSource(_GlobalData);
      _SourceWithTrace.UserPermissions = upsWT;
    }

    [OneTimeTearDown]
    public void TearDown()
    {
      if (_GlobalData != null)
      {
        _GlobalData.DisposeDBs();
        _GlobalData = null;
      }

      /*
      if (_TempDir != null)
      {
        _TempDir.Dispose();
        _TempDir = null;
      } */
    }

    #endregion

    #endregion
  }
}
