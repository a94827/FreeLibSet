using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using AgeyevAV.ExtDB.Docs;
using AgeyevAV.IO;
using AgeyevAV;
using AgeyevAV.ExtDB;

namespace ExtDBDocs.tests
{
  public class DBxBinDataValidatorBase
  {
    #region Двоичные данные

    public static readonly byte[] ByteArray1 = DataTools.CreateArray<byte>(100, 1);

    public static readonly byte[] ByteArray2 = DataTools.CreateArray<byte>(200, 2);

    public static readonly byte[] ByteArray3 = DataTools.CreateArray<byte>(300, 3);

    public static readonly byte[] ByteArray4 = DataTools.CreateArray<byte>(400, 4);

    #endregion

    #region Инициализация и настройка тестов

    private TempDirectory _TempDir;

    private DBxRealDocProviderGlobal _GlobalData;

    protected DBxBinDataValidator Validator { get { return _Validator; } }
    private DBxBinDataValidator _Validator;

    private DBxRealDocProvider _DocProvider;

    private Int32 FirstDocId = 1;

    protected void Setup()
    {
      _TempDir = new TempDirectory();

      #region Структура документов

      DBxDocTypes dts = new DBxDocTypes();
      dts.UsersTableName = String.Empty; // без пользователей
      DBxDocType dt;
      DBxSubDocType sdt;

      dt = new DBxDocType("Doc1");
      dt.Struct.Columns.AddString("C11", 20, false);
      dt.BinDataRefs.Add("C12");
      dts.Add(dt);

      sdt = new DBxSubDocType("SubDoc2");
      sdt.Struct.Columns.AddString("C21", 20, false);
      // без файлов
      dt.SubDocs.Add(sdt);

      sdt = new DBxSubDocType("SubDoc3");
      sdt.Struct.Columns.AddString("C31", 20, false);
      dt.BinDataRefs.Add("C32");
      dt.SubDocs.Add(sdt);


      dt = new DBxDocType("Doc4");
      dt.Struct.Columns.AddString("C41", 20, false);
      dt.BinDataRefs.Add("C42");
      dts.Add(dt);

      #endregion

      #region Инициализация баз данных

      DBxDocDBConnectionHelper conHelper = new DBxDocDBConnectionHelper();
      conHelper.ProviderName = "SQLite";
      conHelper.ConnectionString = "Data Source=" + new AbsPath(_TempDir.Dir, "db.db").Path;
      conHelper.DocTypes = dts;

      _GlobalData = conHelper.CreateRealDocProviderGlobal();

      DBxRealDocProviderSource source = new DBxRealDocProviderSource(_GlobalData);

      _DocProvider = new DBxRealDocProvider(source, 0, true);

      #endregion

      #region Создание документов

      DBxDocSet docSet;
      DBxSingleDoc doc;
      DBxSubDoc sd;

      docSet = new DBxDocSet(_DocProvider);
      doc = docSet["Doc1"].Insert();
      doc.Values["C11"].SetString("First doc");
      doc.Values["C12"].SetBinData(ByteArray1);

      sd = doc.SubDocs["SubDoc2"].Insert();
      sd.Values["C21"].SetString("AAA");

      sd = doc.SubDocs["SubDoc3"].Insert();
      sd.Values["C31"].SetString("BBB");
      doc.Values["C32"].SetBinData(ByteArray3);

      doc = docSet["Doc4"].Insert();
      doc.Values["C41"].SetString("Second doc");
      doc.Values["C42"].SetBinData(ByteArray4);
      docSet.ApplyChanges(false);

      #endregion

      #region Изменение документа

      docSet = new DBxDocSet(_DocProvider);
      doc = docSet["Doc1"].View(FirstDocId);
      doc.Values["C12"].SetBinData(ByteArray2);
      docSet.ApplyChanges(false);

      #endregion

      _Validator = new DBxBinDataValidator(_GlobalData);
    }

    protected void ValidateDocs()
    {
      DBxDocSet docSet = new DBxDocSet(_DocProvider);
      DBxSingleDoc doc = docSet["Doc1"].View(FirstDocId);
      Assert.AreEqual(ByteArray2, doc.Values["C12"].GetBinData(), "C12");
      DBxSubDoc sd = doc.SubDocs["SubDoc2"][0];
      Assert.AreEqual(ByteArray3, doc.Values["C32"].GetBinData(), "C32");

      doc = docSet["Doc4"].View(FirstDocId);
      Assert.AreEqual(ByteArray4, doc.Values["C42"].GetBinData(), "C42");
    }

    protected void TearDown()
    {
      if (_GlobalData != null)
      {
        _GlobalData.DisposeDBs();
        _GlobalData = null;
      }
      if (_TempDir != null)
        _TempDir.Dispose();
    }

    #endregion
  }

  [TestFixture]
  public class DBxBinDataValidatorTests : DBxBinDataValidatorBase
  {
    [SetUp]
    public new void Setup()
    {
      base.Setup();
    }

    [TearDown]
    public new void TearDown()
    {
      base.TearDown();
    }

    [Test]
    public void Test_ok()
    {
      Validator.CheckData();
      Assert.AreEqual(ErrorMessageKind.Info, Validator.Errors.Severity);
      ValidateDocs();
    }

    internal static readonly string EmptyMD5 = new string(' ', 32);

    [Test]
    public void Test_empty_md5()
    {
      using (DBxCon con = new DBxCon(Validator.GlobalData.MainDBEntry))
      {
        con.AddRecord("BinData", new DBxColumns("MD5,Length"), new object[] { EmptyMD5, 0 });
      }

      Validator.CheckData();
      Assert.AreEqual(ErrorMessageKind.Warning, Validator.Errors.Severity);
    }

    [Test]
    public void Test_bad_length()
    {
      using (DBxCon con = new DBxCon(Validator.GlobalData.MainDBEntry))
      {
        con.SetValue("BinData", 1, "Length", 1000);
      }

      Validator.CheckData();
      Assert.AreEqual(ErrorMessageKind.Error, Validator.Errors.Severity);
    }

    [Test]
    public void Test_bad_md5()
    {
      using (DBxCon con = new DBxCon(Validator.GlobalData.MainDBEntry))
      {
        con.SetValue("BinData", 1, "MD5", new string('6', 20));
      }

      Validator.CheckData();
      Assert.AreEqual(ErrorMessageKind.Error, Validator.Errors.Severity);
    }

    [Test]
    public void Test_repeated_md5()
    {
      Assert.IsFalse(Validator.GlobalData.BinDataHandler.UseFragmentation, "UseFragmentation=false"); // предусловие

      using (DBxCon con = new DBxCon(Validator.GlobalData.MainDBEntry))
      {
        Int32 binDataId=con.AddRecordWithIdResult("BinData", new DBxColumns("MD5,Length"), new object[] { 
          DataTools.MD5Sum(ByteArray1),
          ByteArray1.Length});
        con.WriteBlob("BinData", binDataId, "Contents", ByteArray1);
      }

      Validator.CheckData();
      Assert.AreEqual(ErrorMessageKind.Warning, Validator.Errors.Severity);
    }
  }
}
