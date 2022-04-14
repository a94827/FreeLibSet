using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data.Docs;
using System.Data;

namespace ExtDBDocs_tests.Data_Docs
{
  /// <summary>
  /// Тестирование объекта DBxMultiSubDocs в режиме, используемом для редактирования поддокументов из таблицы DBxSubDocGridView в редакторе документа.
  /// Конструктор DBxMultiSubDocsTests создает отдельный набор на время редактирования поддокумента(ов), который затем сливается с основным набором.
  /// </summary>
  [TestFixture]
  public class DBxMultiSubDocsTests: DBxDocSetTestsBase
  {
    #region Конструкторы

    [Test]
    public void Constructor_DataRows_Edit([Values(false, true)] bool useDeleted, [Values(false, true)] bool useVersions, [Values(false, true)] bool useTime)
    {
      TestDBInfo info = this[useDeleted, useVersions, useTime];
      Int32 docId = CreateTestDoc(info, false, 1, "AAA", "BBB", "CCC");

      DBxDocSet ds = new DBxDocSet(info.Provider);
      ds.Edit(docId);

      DataView dv = ds[0].SubDocs["SD11"].SubDocsView;
      Assert.AreEqual(3, dv.Count, "SubDocsView.Count");
      DataRow[] rows = new DataRow[2] { dv[0].Row, dv[2].Row }; // одну пропустили

      DBxMultiSubDocs sut = new DBxMultiSubDocs(ds[0].SubDocs["SD11"], rows);

      Assert.AreSame(sut.DocProvider, info.Provider, "DocProvider");
      Assert.AreNotSame(dv.Table, sut.SubDocsView.Table, "Table");
      Assert.AreEqual(2, sut.SubDocCount, "SubDocCount");
      Assert.AreEqual(sut[0].SubDocId, rows[0]["Id"], "SubDocId[0]");
      sut[0].Values.
      Assert.AreEqual(sut[1].SubDocId, rows[1]["Id"], "SubDocId[1]");
    }

    #endregion
  }
}
