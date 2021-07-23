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
  /// <summary>
  /// Базовый класс для DBxBinDataHandlerTests и DBxBinDataValidatorTests
  /// </summary>
  public class DBxBinDataTestsBase
  {
    #region Структура базы данных

    private TempDirectory _TempDir;

    public DBxRealDocProviderGlobal GlobalData { get { return _GlobalData; } }
    private DBxRealDocProviderGlobal _GlobalData;

    protected DBxBinDataValidator Validator { get { return _Validator; } }
    private DBxBinDataValidator _Validator;

    private DBxRealDocProvider _DocProvider;

    /// <summary>
    /// Управляющее свойство.
    /// Надо ли использовать фрагментацию для двоичных данных.
    /// По умолчанию - false
    /// </summary>
    public bool UseFragmentation { get { return _UseFragmentation; } set { _UseFragmentation = value; } }
    private bool _UseFragmentation;

    /// <summary>
    /// Создание баз данныз.
    /// Создаются следующие виды документов (4) и поодокументов (2)
    /// "Doc1" и "Doc2" - документы со ссылками на двоичные данные. В "Doc2" есть поддокументы "SubDoc2", в котором задается ссылка
    /// "Doc3" и "Doc4" - документы со ссылками на файлы. В "Doc4" есть поддокументы "SubDoc4", в котором задается ссылка
    /// Имена полей: "B" - ссылка на двоичные данные, "F" - ссылка на файл
    /// </summary>
    protected void CreateDB()
    {
      _TempDir = new TempDirectory();

      #region Структура документов

      DBxDocTypes dts = new DBxDocTypes();
      dts.UsersTableName = String.Empty; // без пользователей
      DBxDocType dt;
      DBxSubDocType sdt;


      dt = new DBxDocType("Doc1");
      dt.BinDataRefs.Add("B");
      dts.Add(dt);

      dt = new DBxDocType("Doc2");
      dts.Add(dt);

      sdt = new DBxSubDocType("SubDoc2");
      sdt.BinDataRefs.Add("B");
      dt.SubDocs.Add(sdt);

      dt = new DBxDocType("Doc3");
      dt.FileRefs.Add("F");
      dts.Add(dt);

      dt = new DBxDocType("Doc4");
      dts.Add(dt);

      sdt = new DBxSubDocType("SubDoc4");
      sdt.FileRefs.Add("F");
      dt.SubDocs.Add(sdt);


      #endregion

      #region Инициализация баз данных

      DBxDocDBConnectionHelper conHelper = new DBxDocDBConnectionHelper();
      conHelper.ProviderName = "SQLite";
      conHelper.ConnectionString = "Data Source=" + new AbsPath(_TempDir.Dir, "db.db").Path;
      conHelper.DocTypes = dts;
      conHelper.UseBinDataFragmentation = UseFragmentation;

      _GlobalData = conHelper.CreateRealDocProviderGlobal();

      DBxRealDocProviderSource source = new DBxRealDocProviderSource(_GlobalData);

      _DocProvider = new DBxRealDocProvider(source, 0, true);

      #endregion

      _Validator = new DBxBinDataValidator(_GlobalData);
      _Validator.UseTrace = false; // незачем мусорить

      _DocInfoList = new List<CreatedDocInfo>();

      Assert.AreEqual(_UseFragmentation, _GlobalData.BinDataHandler.UseFragmentation, "UseFragmentation");
    }

    protected void TearDown()
    {
      _Validator = null;
      _DocProvider = null;
      if (_GlobalData != null)
      {
        _GlobalData.DisposeDBs();
        _GlobalData = null;
      }
      if (_TempDir != null)
        _TempDir.Dispose();

      _UseFragmentation = false; // восстанавливем значение по умолчанию
    }

    #endregion

    #region Создание документов

    /// <summary>
    /// Хранение информации для последующей проверки
    /// </summary>
    public class CreatedDocInfo
    {
      public Int32 DocId1, DocId2, DocId3, DocId4; // Идентификаторы документов

      public byte[] B1, B2; // Двоичные данные

      public FileContainer F3, F4; // имя файла и данные
    }

    List<CreatedDocInfo> _DocInfoList;

    /// <summary>
    /// Используется для генерации разных данных, но чтобы результаты были повторяемыми
    /// </summary>
    private int _DataGenCounter;

    /// <summary>
    /// Создает по одному документу каждого вида
    /// </summary>
    public CreatedDocInfo CreateDocs(bool fill)
    {
      CreatedDocInfo info = new CreatedDocInfo();
      DBxDocSet docSet = new DBxDocSet(_DocProvider);
      docSet.ActionInfo = fill ? "creating filled docs" : "creating empty docs";
      DBxSingleDoc doc;
      DBxSubDoc sd;

      doc = docSet["Doc1"].Insert();
      doc = docSet["Doc2"].Insert();
      sd = doc.SubDocs["SubDoc2"].Insert();
      doc = docSet["Doc3"].Insert();
      doc = docSet["Doc4"].Insert();
      sd = doc.SubDocs["SubDoc4"].Insert();

      // фиктивные идентификаторы
      info.DocId1 = docSet["Doc1"].DocIds[0];
      info.DocId2 = docSet["Doc2"].DocIds[0];
      info.DocId3 = docSet["Doc3"].DocIds[0];
      info.DocId4 = docSet["Doc4"].DocIds[0];
      // заполнение данных
      DoSetData(docSet, info, fill);

      // запись
      docSet.ApplyChanges(true);

      // реальные идентификаторы
      info.DocId1 = docSet["Doc1"].DocIds[0];
      info.DocId2 = docSet["Doc2"].DocIds[0];
      info.DocId3 = docSet["Doc3"].DocIds[0];
      info.DocId4 = docSet["Doc4"].DocIds[0];

      // Проверка
      _DocProvider.CheckIsRealDocId(info.DocId1);
      _DocProvider.CheckIsRealDocId(info.DocId2);
      _DocProvider.CheckIsRealDocId(info.DocId3);
      _DocProvider.CheckIsRealDocId(info.DocId4);

      _DocInfoList.Add(info);
      return info;
    }

    private void DoSetData(DBxDocSet docSet, CreatedDocInfo info, bool fill)
    {
      DBxSingleDoc doc;
      DBxSubDoc sd;

      doc = docSet["Doc1"].GetDocById(info.DocId1);
      if (fill)
      {
        info.B1 = CreateArray();
        doc.Values["B"].SetBinData(info.B1);
      }
      else
      {
        doc.Values["B"].SetNull();
        info.B1 = null;
      }

      doc = docSet["Doc2"].GetDocById(info.DocId2);
      sd = doc.SubDocs["SubDoc2"][0];
      if (fill)
      {
        info.B2 = CreateArray();
        sd.Values["B"].SetBinData(info.B2);
      }
      else
      {
        sd.Values["B"].SetNull();
        info.B2 = null;
      }

      doc = docSet["Doc3"].GetDocById(info.DocId3);
      if (fill)
      {
        info.F3 = CreateFile();
        doc.Values["F"].SetDBFile(info.F3);
      }
      else
      {
        doc.Values["F"].SetNull();
        info.F3 = null;
      }


      doc = docSet["Doc4"].GetDocById(info.DocId4);
      sd = doc.SubDocs["SubDoc4"][0];
      if (fill)
      {
        info.F4 = CreateFile();
        sd.Values["F"].SetDBFile(info.F4);
      }
      else
      {
        sd.Values["F"].SetNull();
        info.F4 = null;
      }
    }

    private byte[] CreateArray()
    {
      _DataGenCounter++;
      return DataTools.CreateArray<byte>(_DataGenCounter * 10, (byte)_DataGenCounter);
    }

    private FileContainer CreateFile()
    {
      byte[] b = CreateArray();
      AbsPath path = new AbsPath(_TempDir.Dir, "test" + _DataGenCounter.ToString() + ".bin");
      System.IO.File.WriteAllBytes(path.Path, b);
      return new FileContainer(path);
    }

    /// <summary>
    /// Модификация документов.
    /// Используется для тестирования истории
    /// </summary>
    /// <param name="info">Созданные документы</param>
    /// <param name="fill">Нужно ли установить двоичные данные или их очистить</param>
    public void ModifyDocs(CreatedDocInfo info, bool fill)
    {
      DBxDocSet docSet = new DBxDocSet(_DocProvider);
      docSet.ActionInfo = fill ? "Changing doc data" : "clearing doc data";
      docSet["Doc1"].Edit(info.DocId1);
      docSet["Doc2"].Edit(info.DocId2);
      docSet["Doc3"].Edit(info.DocId3);
      docSet["Doc4"].Edit(info.DocId4);
      DoSetData(docSet, info, fill);
      docSet.ApplyChanges(false);
    }

    /// <summary>
    /// Проверка всех документов
    /// </summary>
    public void ValidateDocs()
    {
      foreach (CreatedDocInfo info in _DocInfoList)
      {
        DBxDocSet docSet = new DBxDocSet(_DocProvider);
        DBxSingleDoc doc;
        DBxSubDoc sd;

        doc = docSet["Doc1"].View(info.DocId1);
        Assert.AreEqual(info.B1, doc.Values["B"].GetBinData(), "Doc1, DocId=" + info.DocId1.ToString());

        doc = docSet["Doc2"].View(info.DocId2);
        sd = doc.SubDocs["SubDoc2"][0];
        Assert.AreEqual(info.B2, sd.Values["B"].GetBinData(), "Doc2, DocId=" + info.DocId2.ToString());

        doc = docSet["Doc3"].View(info.DocId3);
        FileContainer fc = doc.Values["F"].GetDBFile();
        DoCompareFile("Doc3", info.F3, fc, info.DocId3);

        doc = docSet["Doc4"].View(info.DocId4);
        sd = doc.SubDocs["SubDoc4"][0];
        fc = sd.Values["F"].GetDBFile();
        DoCompareFile("Doc4", info.F4, fc, info.DocId4);
      }
    }

    private void DoCompareFile(string docTypeName, FileContainer wanted, FileContainer real, Int32 docId)
    {
      Assert.AreEqual(wanted != null, real != null, docTypeName + ", DocId=" + docId + ", file existance");
      if (wanted == null || real == null)
        return;

      Assert.AreEqual(wanted.FileInfo.Name, real.FileInfo.Name, ", DocId=" + docId + ", file name");
      Assert.AreEqual(wanted.FileInfo.Length, real.FileInfo.Length, ", DocId=" + docId + ", file length");
      Assert.AreEqual(CorrectTime(wanted.FileInfo.LastWriteTime), CorrectTime(real.FileInfo.LastWriteTime), ", DocId=" + docId + ", LastWriteTime");
      Assert.AreEqual(wanted.Contents, real.Contents, ", DocId=" + docId + ", file contents");
    }

    /// <summary>
    /// Исправляем время.
    /// В базе данных оно хранится не точно (как число с плавающей точкой?) и файл тоже может иметь неточное время (FAT)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private DateTime? CorrectTime(DateTime? value)
    {
      if (!value.HasValue)
        return null;

      DateTime dt = value.Value.Date;
      int sec2 = (int)Math.Round(value.Value.TimeOfDay.TotalSeconds / 2.0, 0, MidpointRounding.AwayFromZero);
      TimeSpan ts = TimeSpan.FromSeconds(sec2 * 2);
      return dt.Add(ts);
    }

    #endregion

    public void CreateDefault()
    {
      CreateDB();
      CreatedDocInfo info1 = CreateDocs(true);
      CreatedDocInfo info2 = CreateDocs(false);
      ModifyDocs(info1, true);
    }

    /// <summary>
    /// Проверяет, что все базы данных существуют, обновлены и содержат все необходимые таблицы
    /// </summary>
    protected void TestAllDBExist()
    {
      DBx[] dbs = GlobalData.GetDBs();
      for (int i = 0; i < dbs.Length; i++)
      {
        Assert.IsTrue(dbs[i].DatabaseExists, dbs[i].DatabaseName + " exists");
        Assert.IsTrue(dbs[i].StructHasBeenUpdated, dbs[i].DatabaseName + " StructHasBeenUpdated");
        ExtDB.tests.DBxTestHelpers.ValidateStruct(dbs[i].Struct, dbs[i].GetRealStruct(), dbs[i].DatabaseName + ". ");
      }
    }
  }

  [TestFixture]
  public class DBxBinDataValidatorTests : DBxBinDataTestsBase
  {
    [TearDown]
    public new void TearDown()
    {
      base.TearDown();
    }

    [Test]
    public void Test_empty()
    {
      CreateDB();

      Validator.Validate();

      Assert.AreEqual(ErrorMessageKind.Info, Validator.Errors.Severity);
      ValidateDocs();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Test_no_errors(bool useFragmentation)
    {
      this.UseFragmentation = useFragmentation;
      CreateDefault(); // с фрагментацией
      Validator.Validate();
      Assert.AreEqual(ErrorMessageKind.Info, Validator.Errors.Severity);
      ValidateDocs();
    }

    internal static readonly string EmptyMD5 = new string(' ', 32);

    [Test]
    public void Test_empty_md5()
    {
      CreateDefault();

      using (DBxCon con = new DBxCon(GlobalData.MainDBEntry))
      {
        con.AddRecord("BinData", new DBxColumns("MD5,Length"), new object[] { EmptyMD5, 0 });
      }

      Validator.Validate();
      Assert.AreEqual(ErrorMessageKind.Warning, Validator.Errors.Severity);
    }

    [Test]
    public void Test_bad_length()
    {
      CreateDefault();

      using (DBxCon con = new DBxCon(GlobalData.MainDBEntry))
      {
        con.SetValue("BinData", 1, "Length", 1000);
      }

      Validator.Validate();
      Assert.AreEqual(ErrorMessageKind.Error, Validator.Errors.Severity);
    }

    [Test]
    public void Test_bad_md5()
    {
      CreateDefault();

      using (DBxCon con = new DBxCon(GlobalData.MainDBEntry))
      {
        con.SetValue("BinData", 1, "MD5", new string('6', 20));
      }

      Validator.Validate();
      Assert.AreEqual(ErrorMessageKind.Error, Validator.Errors.Severity);
    }

    [Test]
    public void Test_repeated_md5()
    {
      CreateDefault();
      Assert.IsFalse(GlobalData.BinDataHandler.UseFragmentation, "UseFragmentation=false"); // предусловие



      using (DBxCon con = new DBxCon(GlobalData.MainDBEntry))
      {
        object[] a = con.GetValues("BinData", 1, new DBxColumns("MD5,Length"));
        byte[] b = con.ReadBlob("BinData", 1, "Contents");

        Int32 binDataId = con.AddRecordWithIdResult("BinData", new DBxColumns("MD5,Length"), a);
        con.WriteBlob("BinData", binDataId, "Contents", b);
      }

      Validator.Validate();
      Assert.AreEqual(ErrorMessageKind.Warning, Validator.Errors.Severity);
    }

    [Test]
    public void Test_bad_BinData_ref()
    {
      CreateDefault();

      // Не получится создать неправильную ссылку в основной базе данных.
      // Портим Undo
      using (DBxCon con = new DBxCon(GlobalData.UndoDBEntry))
      {
        con.SetValue("Doc1", 1, "B", 666);
      }

      Validator.Validate();
      Assert.AreEqual(ErrorMessageKind.Error, Validator.Errors.Severity);
    }

    [Test]
    public void Test_bad_FileName_ref()
    {
      CreateDefault();

      // Не получится создать неправильную ссылку в основной базе данных.
      // Портим Undo
      using (DBxCon con = new DBxCon(GlobalData.UndoDBEntry))
      {
        con.SetValue("SubDoc4", 1, "F", 666);
      }

      Validator.Validate();
      Assert.AreEqual(ErrorMessageKind.Error, Validator.Errors.Severity);
    }

    [Test]
    public void Test_bad_external_BinData_ref()
    {
      CreateDB();

      // Создаем еще одну таблицу документа
      DBxDocTypes dts2 = new DBxDocTypes();
      DBxDocType dt = new DBxDocType("ExtraDoc11");
      dt.BinDataRefs.Add("B");
      dts2.Add(dt);

      // Добавляем таблицу документов в основную БД и в Undo
      DBxStruct dbs = dts2.GetMainDBStruct();
      GlobalData.BinDataHandler.AddMainTableStructs(dbs);
      TempUpdateStruct(GlobalData.MainDBEntry, dbs);
      TempUpdateStruct(GlobalData.UndoDBEntry, dts2.GetUndoDBStruct());

      //DBxRealDocProviderGlobal globalData2 = new DBxRealDocProviderGlobal(dts2, GlobalData.MainDBEntry, GlobalData.UndoDBEntry, GlobalData.BinDataHandler);

      using (DBxConBase con = GlobalData.UndoDBEntry.CreateCon())
      {
        con.NameCheckingEnabled = false;
        con.AddRecord("ExtraDoc11", "B", 666);
      }

      Validator.Validate();
      Assert.AreEqual(ErrorMessageKind.Error, Validator.Errors.Severity);
    }

    [Test]
    public void Test_bad_external_FileName_ref()
    {
      CreateDB();

      // Создаем еще одну таблицу документа
      DBxDocTypes dts2 = new DBxDocTypes();
      DBxDocType dt = new DBxDocType("ExtraDoc12");
      dt.FileRefs.Add("F");
      dts2.Add(dt);

      // Добавляем таблицу документов в основную БД и в Undo
      DBxStruct dbs = dts2.GetMainDBStruct();
      GlobalData.BinDataHandler.AddMainTableStructs(dbs);
      TempUpdateStruct(GlobalData.MainDBEntry, dbs);
      TempUpdateStruct(GlobalData.UndoDBEntry, dts2.GetUndoDBStruct());

      //DBxRealDocProviderGlobal globalData2 = new DBxRealDocProviderGlobal(dts2, GlobalData.MainDBEntry, GlobalData.UndoDBEntry, GlobalData.BinDataHandler);

      using (DBxConBase con = GlobalData.UndoDBEntry.CreateCon())
      {
        con.NameCheckingEnabled = false;
        con.AddRecord("ExtraDoc12", "F", 666);
      }

      Validator.Validate();
      Assert.AreEqual(ErrorMessageKind.Error, Validator.Errors.Severity);
    }

    /// <summary>
    /// Обновляем структуру базы данных, но оставляем неизменным свойство DBx.Struct
    /// </summary>
    /// <param name="dbEntry"></param>
    /// <param name="dbs"></param>
    private void TempUpdateStruct(DBxEntry dbEntry, DBxStruct dbs)
    {
      if (dbEntry == null)
        return;

      DBxStruct oldStr = dbEntry.DB.Struct;
      bool strUpd = dbEntry.DB.StructHasBeenUpdated;

      dbEntry.DB.Struct = dbs;
      dbEntry.DB.UpdateStruct();

      dbEntry.DB.Struct = oldStr;
      if (strUpd)
        dbEntry.DB.UpdateStruct(); // чтобы соответствовало
    }
  }
}
