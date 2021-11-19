using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data.Docs;
using FreeLibSet.IO;
using FreeLibSet.Data;

namespace ExtDBDocs_tests.Data_Docs
{
  [TestFixture]
  public class DBxBinDataHandlerTests : DBxBinDataTestsBase
  {
    #region Проверка структуры таблиц

    [Test]
    public void Structure_without_fragmentation()
    {
      UseFragmentation = false;
      CreateDB();

      TestAllDBExist();
      TestStructureBase();
      Assert.AreEqual(0, GlobalData.BinDataHandler.SectionEntryCount, "SectionEntryCount");

      DBxTableStruct tsBinData = GlobalData.BinDataHandler.MainEntry.DB.Struct.Tables["BinData"];
      Assert.IsFalse(tsBinData.Columns.Contains("Section"), "Table BinData, column Section");
      Assert.IsTrue(tsBinData.Columns.Contains("Contents"), "Table BinData, column Contents");

    }

    [Test]
    public void Structure_with_fragmentation()
    {
      UseFragmentation = true;
      CreateDB();

      TestAllDBExist();
      TestStructureBase();
      Assert.AreEqual(1, GlobalData.BinDataHandler.SectionEntryCount, "SectionEntryCount");

      DBxTableStruct tsBinData = GlobalData.BinDataHandler.MainEntry.DB.Struct.Tables["BinData"];
      Assert.IsTrue(tsBinData.Columns.Contains("Section"), "Table BinData, column Section");
      Assert.IsFalse(tsBinData.Columns.Contains("Contents"), "Table BinData, column Contents");

      DBxEntry e1 = GlobalData.BinDataHandler.GetSectionEntry(1);
      Assert.IsNotNull(e1, "GetSectionEntry(1)");
      Assert.AreNotSame(GlobalData.BinDataHandler.MainEntry.DB, e1.DB, "Section DB cannot be a main DB");

      DBxTableStruct tsBDS = e1.DB.Struct.Tables["BinDataStorage"];
      Assert.IsNotNull(tsBDS, "BinDataStorage table struct");
      Assert.IsTrue(tsBDS.Columns.Contains("Id"), "Table BinDataStorage, column Id");
      Assert.IsTrue(tsBDS.Columns.Contains("Contents"), "Table BinDataStorage, column Contents");
    }

    private void TestStructureBase()
    {
      Assert.AreEqual(base.UseFragmentation, GlobalData.BinDataHandler.UseFragmentation, "UseFragmentation");
      Assert.IsTrue(GlobalData.BinDataHandler.UseBinData, "UseBinData");
      Assert.IsTrue(GlobalData.BinDataHandler.UseFiles, "UseFiles");
      Assert.IsNotNull(GlobalData.BinDataHandler.MainEntry, "MainEntry");

      DBx mainDB = GlobalData.BinDataHandler.MainEntry.DB;

      DBxTableStruct tsBinData = mainDB.Struct.Tables["BinData"];
      Assert.IsNotNull(tsBinData, "BinData table struct");
      Assert.IsTrue(tsBinData.Columns.Contains("Id"), "Table BinData, column Id");
      Assert.IsTrue(tsBinData.Columns.Contains("MD5"), "Table BinData, column MD5");
      Assert.IsTrue(tsBinData.Columns.Contains("Length"), "Table BinData, column Length");

      DBxTableStruct tsFileNames = mainDB.Struct.Tables["FileNames"];
      Assert.IsTrue(tsFileNames.Columns.Contains("Id"), "Table FileNames, column Id");
      Assert.IsTrue(tsFileNames.Columns.Contains("Name"), "Table FileNames, column Name");
      Assert.IsTrue(tsFileNames.Columns.Contains("Data"), "Table FileNames, column Data");
      Assert.IsTrue(tsFileNames.Columns.Contains("CreationTime"), "Table FileNames, column CreationTime");
      Assert.IsTrue(tsFileNames.Columns.Contains("LastWriteTime"), "Table FileNames, column LastWriteTime");
    }

    #endregion
  }
}
