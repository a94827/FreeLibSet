using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data.Docs;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Data;

namespace ExtDBDocs_tests.Data_Docs
{
  /// <summary>
  /// Тестирование объекта DBxMultiSubDocs в режиме, используемом для редактирования поддокументов из таблицы DBxSubDocGridView в редакторе документа.
  /// Конструктор DBxMultiSubDocsTests создает отдельный набор на время редактирования поддокумента(ов), который затем сливается с основным набором.
  /// </summary>
  [TestFixture]
  public class DBxMultiSubDocsTests : DBxDocSetTestsBase
  {
    #region Конструкторы

    [Test]
    public void Constructor_DataRows([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info, false, 1, "AAA", "BBB", "CCC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);
      doc.SubDocs["SD11"][0].Values["F111"].SetString("DDD"); // меняем значение

      DataView dv = ds[0].SubDocs["SD11"].SubDocsView;
      Assert.AreEqual(3, dv.Count, "SubDocsView.Count");
      DataRow[] rows = new DataRow[2] { dv[0].Row, dv[2].Row }; // одну пропустили

      DBxMultiSubDocs sut = new DBxMultiSubDocs(ds[0].SubDocs["SD11"], rows);

      Assert.AreSame(sut.DocProvider, info.Provider, "DocProvider");
      Assert.AreNotSame(dv.Table, sut.SubDocsView.Table, "Table");
      Assert.AreEqual(2, sut.SubDocCount, "SubDocCount");
      Assert.AreEqual(sut[0].SubDocId, rows[0]["Id"], "SubDocId[0]");
      Assert.AreEqual("DDD", sut[0].Values["F111"].AsString, "Value[0]"); // Значение изменено
      Assert.AreEqual(DBxDocState.View, sut[0].SubDocState, "SubDocState[0]"); // а не Edit, как у оригинала поддокумента

      Assert.AreEqual(sut[1].SubDocId, rows[1]["Id"], "SubDocId[1]");
      Assert.AreEqual("CCC", sut[1].Values["F111"].AsString, "Value[1]");
      Assert.AreEqual(DBxDocState.View, sut[1].SubDocState, "SubDocState[1]");
    }

    [Test]
    public void Constructor_SubDocIds([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info, false, 1, "AAA", "BBB", "CCC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);
      doc.SubDocs["SD11"][0].Values["F111"].SetString("DDD"); // меняем значение

      DataView dv = ds[0].SubDocs["SD11"].SubDocsView;
      Assert.AreEqual(3, dv.Count, "SubDocsView.Count");

      Int32[] subDocIds = new Int32[] { doc.SubDocs["SD11"][0].SubDocId, doc.SubDocs["SD11"][2].SubDocId };
      DBxMultiSubDocs sut = new DBxMultiSubDocs(ds[0].SubDocs["SD11"], subDocIds);

      Assert.AreSame(sut.DocProvider, info.Provider, "DocProvider");
      Assert.AreNotSame(dv.Table, sut.SubDocsView.Table, "Table");
      Assert.AreEqual(2, sut.SubDocCount, "SubDocCount");
      Assert.AreEqual(sut[0].SubDocId, subDocIds[0], "SubDocId[0]");
      Assert.AreEqual("DDD", sut[0].Values["F111"].AsString, "Value[0]"); // Значение изменено
      Assert.AreEqual(DBxDocState.View, sut[0].SubDocState, "SubDocState[0]"); // а не Edit, как у оригинала поддокумента

      Assert.AreEqual(sut[1].SubDocId, subDocIds[1], "SubDocId[1]");
      Assert.AreEqual("CCC", sut[1].Values["F111"].AsString, "Value[1]");
      Assert.AreEqual(DBxDocState.View, sut[1].SubDocState, "SubDocState[1]");
    }

    [Test]
    public void Constructor_DataRows_EmptyArray([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Insert();

      DBxMultiSubDocs sut = new DBxMultiSubDocs(ds[0].SubDocs["SD11"], new DataRow[0]);
      Assert.AreEqual(0, sut.SubDocCount);
    }

    [Test]
    public void Constructor_SubDocIds_EmptyArray([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Insert();

      DBxMultiSubDocs sut = new DBxMultiSubDocs(ds[0].SubDocs["SD11"], EmptyArray<Int32>.Empty);
      Assert.AreEqual(0, sut.SubDocCount);
    }

    #endregion

    #region Сценарии использования

    [Test]
    public void Scenario_Insert_Insert([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Insert();
      doc.Values["F101"].SetBoolean(false); // обязательное поле

      DBxMultiSubDocs sut = new DBxMultiSubDocs(ds[0].SubDocs["SD11"], EmptyArray<Int32>.Empty);
      DBxSubDoc sdoc = sut.Insert();
      sut.Values["F111"].SetString("DDD");

      ds[0].SubDocs["SD11"].MergeSubSet(sut);
      Assert.AreEqual(1, doc.SubDocs["SD11"].SubDocCount, "SubDocCount");

      DBxSubDoc sd = doc.SubDocs["SD11"][0];
      Assert.AreEqual("DDD", sd.Values["F111"].AsString, "Value[0]");
      Assert.AreEqual(DBxDocState.Insert, sd.SubDocState, "SubDocState[0]");

      ds.ApplyChanges(false);
    }


    [Test]
    public void Scenario_Edit_Insert([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info, false, 1, "AAA", "BBB", "CCC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);
      IdArray<Int32> orgSubDocIds = IdTools.AsIdArray<Int32>(doc.SubDocs["SD11"].SubDocIds);


      DBxMultiSubDocs sut = new DBxMultiSubDocs(ds[0].SubDocs["SD11"], new DataRow[0]);
      DBxSubDoc sdoc = sut.Insert();
      Int32 newSubDocId = sdoc.SubDocId;
      sut.Values["F111"].SetString("DDD");

      ds[0].SubDocs["SD11"].MergeSubSet(sut);
      Assert.AreEqual(4, doc.SubDocs["SD11"].SubDocCount, "SubDocCount");
      // После добавления, поддокументы могли оказаться в другом порядке.
      // Если сортировка по умолчанию выполняется по идентификатору поддокумента, то он будет первым по
      // счету, но на это не стоит полагаться

      DBxSubDoc sd = doc.SubDocs["SD11"].GetSubDocById(orgSubDocIds[0]);
      Assert.AreEqual("AAA", sd.Values["F111"].AsString, "Value[0]");
      Assert.AreEqual(DBxDocState.View, sd.SubDocState, "SubDocState[0]");

      sd = doc.SubDocs["SD11"].GetSubDocById(orgSubDocIds[1]);
      Assert.AreEqual("BBB", sd.Values["F111"].AsString, "Value[1]");
      Assert.AreEqual(DBxDocState.View, sd.SubDocState, "SubDocState[1]");

      sd = doc.SubDocs["SD11"].GetSubDocById(orgSubDocIds[2]);
      Assert.AreEqual("CCC", sd.Values["F111"].AsString, "Value[2]");
      Assert.AreEqual(DBxDocState.View, sd.SubDocState, "SubDocState[2]");

      sd = doc.SubDocs["SD11"].GetSubDocById(newSubDocId);
      Assert.AreEqual("DDD", sd.Values["F111"].AsString, "Value[3]");
      Assert.AreEqual(DBxDocState.Insert, sd.SubDocState, "SubDocState[3]");

      ds.ApplyChanges(false);
      ds = new DBxDocSet(info.Provider);
      doc = ds["D1"].View(docId);
      // После повторного открытия документа, поддокументы будут идти в предсказуемом порядке
      Assert.AreEqual("AAA", doc.SubDocs["SD11"][0].Values["F111"].AsString, "Saved Value[0]");
      Assert.AreEqual("BBB", doc.SubDocs["SD11"][1].Values["F111"].AsString, "Saved Value[1]");
      Assert.AreEqual("CCC", doc.SubDocs["SD11"][2].Values["F111"].AsString, "Saved Value[2]");
      Assert.AreEqual("DDD", doc.SubDocs["SD11"][3].Values["F111"].AsString, "Saved Value[3]");
    }

    [Test]
    public void Scenario_Edit_Edit([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info, false, 1, "AAA", "BBB", "CCC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);

      Int32[] subDocIds = new Int32[] { doc.SubDocs["SD11"][0].SubDocId, doc.SubDocs["SD11"][2].SubDocId };
      DBxMultiSubDocs sut = new DBxMultiSubDocs(ds[0].SubDocs["SD11"], subDocIds);
      sut.Values["F111"].SetString("DDD");

      ds[0].SubDocs["SD11"].MergeSubSet(sut);

      Assert.AreEqual("DDD", doc.SubDocs["SD11"][0].Values["F111"].AsString, "Value[0]");
      Assert.AreEqual(DBxDocState.Edit, doc.SubDocs["SD11"][0].SubDocState, "SubDocState[0]");
      Assert.AreEqual("BBB", doc.SubDocs["SD11"][1].Values["F111"].AsString, "Value[1]");
      Assert.AreEqual(DBxDocState.View, doc.SubDocs["SD11"][1].SubDocState, "SubDocState[1]");
      Assert.AreEqual("DDD", doc.SubDocs["SD11"][2].Values["F111"].AsString, "Value[2]");
      Assert.AreEqual(DBxDocState.Edit, doc.SubDocs["SD11"][2].SubDocState, "SubDocState[2]");

      ds.ApplyChanges(false);
      ds = new DBxDocSet(info.Provider);
      doc = ds["D1"].View(docId);
      Assert.AreEqual("DDD", doc.SubDocs["SD11"][0].Values["F111"].AsString, "Saved Value[0]");
      Assert.AreEqual("BBB", doc.SubDocs["SD11"][1].Values["F111"].AsString, "Saved Value[1]");
      Assert.AreEqual("DDD", doc.SubDocs["SD11"][2].Values["F111"].AsString, "Saved Value[2]");
    }

    [Test]
    public void Scenario_Edit_Delete([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info, false, 1, "AAA", "BBB", "CCC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxSingleDoc doc = ds["D1"].Edit(docId);

      Int32[] subDocIds = new Int32[] { doc.SubDocs["SD11"][0].SubDocId, doc.SubDocs["SD11"][2].SubDocId };
      DBxMultiSubDocs sut = new DBxMultiSubDocs(ds[0].SubDocs["SD11"], subDocIds);
      sut[0].Delete();
      sut[1].Delete();

      ds[0].SubDocs["SD11"].MergeSubSet(sut);
      Assert.AreEqual(3, doc.SubDocs["SD11"].SubDocCount, "SubDocCount");

      Assert.AreEqual(DBxDocState.Delete, doc.SubDocs["SD11"][0].SubDocState, "SubDocState[0]");
      Assert.AreEqual("BBB", doc.SubDocs["SD11"][1].Values["F111"].AsString, "Value[1]");
      Assert.AreEqual(DBxDocState.View, doc.SubDocs["SD11"][1].SubDocState, "SubDocState[1]");
      Assert.AreEqual(DBxDocState.Delete, doc.SubDocs["SD11"][2].SubDocState, "SubDocState[2]");

      ds.ApplyChanges(false);
      ds = new DBxDocSet(info.Provider);
      doc = ds["D1"].View(docId);
      Assert.AreEqual("BBB", doc.SubDocs["SD11"][0].Values["F111"].AsString, "Saved Value[0]");
    }

    #endregion

    #region InsertForDocIds()


    [Test]
    public void InsertForDocIds([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId1 = CreateTestDoc(info);
      Int32 docId2 = CreateTestDoc(info);

      DBxDocSet ds = new DBxDocSet(info.Provider);
      DBxMultiSubDocs sut = ds["D1"].SubDocs["SD11"];
      DBxSingleDoc doc1 = ds["D1"].Edit(docId1);
      DBxSingleDoc doc2 = ds["D1"].View(docId2);
      DBxSingleDoc doc3 = ds["D1"].Insert();

      int nSubDocs = sut.SubDocCount;
      Assert.Catch<InvalidOperationException>(delegate () { sut.InsertForDocIds(new Int32[] { docId1, docId2 }); }, "Try for doc in view state");
      Assert.AreEqual(nSubDocs, sut.SubDocCount, "No subdocs created");

      DBxSubDoc[] subDocs2 = null;
      Assert.DoesNotThrow(delegate () { subDocs2 = sut.InsertForDocIds(new Int32[] { docId1, doc3.DocId }); }, "Insert and edit mode");
      Assert.AreEqual(nSubDocs + 2, sut.SubDocCount, "2 subdocs created");
      Assert.AreEqual(2, subDocs2.Length, "Length #2");
      Assert.AreEqual(docId1, subDocs2[0].DocId, "DocId1");
      Assert.AreEqual(doc3.DocId, subDocs2[1].DocId, "DocId2");
      Assert.AreEqual(DBxDocState.Insert, subDocs2[0].SubDocState, "State 1");
      Assert.AreEqual(DBxDocState.Insert, subDocs2[1].SubDocState, "State 2");
    }

    #endregion
  }
}
