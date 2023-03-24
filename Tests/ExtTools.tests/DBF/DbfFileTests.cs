using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DBF;
using FreeLibSet.IO;
using System.IO;
using FreeLibSet.Core;
using System.Data;

namespace ExtTools_tests.DBF
{
  /*
   * Для тестирования DbfFile используются тестовые DBF-файлы в каталоге DBF\Samples.
   * Эти файлы имеют разные форматы и созданы в различных программах.
   * Файлы имеют походую структуру со следующими полями:
   * "F1", C100
   * "F21", N3.0  - проектируется на тип Int32
   * "F22", N10.0 - проектируется на тип Int64
   * "F23", N12.2 - проектируется на тип Decimal
   * "F3", L
   * "F4", D      - нет в dBase II
   * "F5", M      - при наличии dbt-файла, нет в dBase II
   * "F6", F12.2  - только в dBase IV
   * Все тестовые таблицы имеют по 3 строки данных с одинаковыми значениями полей.
   * Кодировка - Windows-1251
   * 
   * Также выполняется тестирование создания таблиц средствами DbfFile. Эти файлы имеют такую же структуру, но исходно не содержат данных
   */

  [TestFixture]
  public class DbfFileTests
  {
    #region Конструкторы для создания DBF-файлов

    [TestCase(DbfFileFormat.dBase2, false)]
    [TestCase(DbfFileFormat.dBase3, false)]
    [TestCase(DbfFileFormat.dBase3, true)]
    [TestCase(DbfFileFormat.dBase4, false)]
    // TODO: не поддерживается [TestCase(DbfFileFormat.dBase4, true)]
    public void Constructor_CreateFile(DbfFileFormat fileFormat, bool useMemo)
    {
      DbfStruct dbs = CreateTestDbfStruct(fileFormat, useMemo);
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath pathDbf = new AbsPath(dir.Dir, "test.dbf");
        AbsPath pathDbt = new AbsPath(dir.Dir, "test.dbt");
        using (DbfFile sut = new DbfFile(pathDbf, dbs, System.Text.Encoding.GetEncoding(1251), fileFormat))
        {
          Assert.AreSame(dbs, sut.DBStruct, "DBStruct");
          Assert.IsTrue(dbs.IsReadOnly, "DBStruct.IsReadOnly");
          Assert.AreEqual(1251, sut.Encoding.CodePage, "Encoding.CodePage");
          Assert.AreEqual(fileFormat, sut.Format, "Format");
          Assert.AreEqual(0, sut.RecordCount, "RecordCount");
          Assert.AreEqual(useMemo, sut.HasMemoFile, "HasMemoFile");
          Assert.IsFalse(sut.IsDisposed, "IsDisposed");
          Assert.IsTrue(sut.SkipDeleted, "SkipDeleted");
          //Assert.IsFalse(sut.TableModified, "TableModified");
          Assert.AreEqual(0, sut.RecNo, "RecNo");
          Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
          Assert.IsFalse(sut.MemoFileMissing, "MemoFileMissing");
        }

        ValidateFiles(fileFormat, dbs, pathDbf, pathDbt, 0, 0);
      }
    }

    private static DbfStruct CreateTestDbfStruct(DbfFileFormat fileFormat, bool useMemo)
    {
      DbfStruct dbs = new DbfStruct();
      dbs.AddString("F1", 100);
      dbs.AddNum("F21", 3); // Int32
      dbs.AddNum("F22", 10); // Int64
      dbs.AddNum("F23", 12, 2); // Decimal
      dbs.AddBool("F3");

      if (fileFormat == DbfFileFormat.dBase3 || fileFormat == DbfFileFormat.dBase4)
      {
        dbs.AddDate("F4");
        if (useMemo)
          dbs.AddMemo("F5");
        if (fileFormat == DbfFileFormat.dBase4)
          dbs.Add(new DbfFieldInfo("F6", 'F', 12, 2));
      }
      return dbs;
    }

    internal static void ValidateFiles(DbfFileFormat fileFormat, DbfStruct dbs, AbsPath pathDbf, AbsPath pathDbt, int recordCount, int memoCount)
    {
      Assert.IsTrue(File.Exists(pathDbf.Path), "File.Exists(dbf)");
      Assert.AreEqual(dbs.HasMemo, File.Exists(pathDbt.Path), "File.Exists(dbt)");
      switch (fileFormat)
      {
        case DbfFileFormat.dBase2:
          ValidateFiles2(dbs, pathDbf, recordCount);
          break;
        case DbfFileFormat.dBase3:
          ValidateFiles3(dbs, pathDbf, pathDbt, recordCount, memoCount);
          break;
        // TODO: ValidateFiles4()
      }
    }

    private static void ValidateFiles2(DbfStruct dbs, AbsPath pathDbf, int recordCount)
    {
      // https://www.fileformat.info/format/dbf/corion-dbase-ii.htm

      Assert.IsFalse(dbs.HasMemo, "HasMemo"); // нет в dBase II
      Assert.LessOrEqual(dbs.Count, 32, "FieldCount"); // ограничение на количество полей

      int recordSize = dbs.RecordSize;
      int minDbfLen = 521; // фиксированный размер заголовка
      long realDbfLen = new FileInfo(pathDbf.Path).Length;
      Assert.GreaterOrEqual(realDbfLen, minDbfLen, "File.Length(dbf) #1");

      using (FileStream fs = new FileStream(pathDbf.Path, FileMode.Open))
      {
        using (BinaryReader rdr = new BinaryReader(fs, System.Text.Encoding.ASCII))
        {
          Assert.AreEqual(0x02, rdr.ReadByte(), "DBF.FileType");

          UInt16 rc = rdr.ReadUInt16();
          Assert.AreEqual(recordCount, (int)rc, "DBF.RecordCount");

          int m = rdr.ReadByte();
          int d = rdr.ReadByte();
          int y = rdr.ReadByte();
          ValidateDate(y, m, d);

          Assert.AreEqual(recordSize, rdr.ReadInt16(), "DBF.RecordSize");

          StringBuilder sb = new StringBuilder();
          for (int i = 0; i < dbs.Count; i++)
          {
            fs.Seek(8 + 16 * i, SeekOrigin.Begin);
            sb.Length = 0;
            for (int j = 0; j < 10; j++)
            {
              char ch = rdr.ReadChar();
              if (ch == '\0')
                break;
              sb.Append(ch);
            }
            Assert.AreEqual(dbs[i].Name.ToUpperInvariant(), sb.ToString(), "DBF.Field[" + i.ToString() + "].Name");

            fs.Seek(8 + 16 * i + 11, SeekOrigin.Begin);
            Assert.AreEqual(dbs[i].Type, rdr.ReadChar(), "DBF.Field[" + i.ToString() + "].Type");

            int len = rdr.ReadByte();
            int dataOff = rdr.ReadInt16();
            int prec = rdr.ReadByte();
            Assert.AreEqual(dbs[i].Length, len, "DBF.Field[" + i.ToString() + "].Length");
            Assert.AreEqual(dbs[i].Precision, prec, "DBF.Field[" + i.ToString() + "].Precision");
          }
          fs.Seek(8 + 16 * dbs.Count, SeekOrigin.Begin);
          int endMarker1 = fs.ReadByte();
          Assert.AreEqual(0x0D, endMarker1, "Field list end marker");

          if (dbs.Count < 32)
          {
            fs.Seek(520, SeekOrigin.Begin);
            int endMarker2 = fs.ReadByte();
            Assert.AreEqual(0x00, endMarker2, "Header end marker");
          }

          int wantedDbfLen = 521 + dbs.RecordSize * recordCount + 1;
          Assert.AreEqual(wantedDbfLen, realDbfLen, "File.Length(dbf) #2");

          // Проверяем маркеры '*'/' ' удаления записей. Это позволяет обнаружить ошибки в файле
          for (int i = 0; i < recordCount; i++)
          {
            fs.Seek(521 + i * dbs.RecordSize, SeekOrigin.Begin);
            Assert.IsTrue(" *".IndexOf(rdr.ReadChar()) >= 0, "Record deletion marker RecNo=" + (i + 1).ToString());
          }
        }
      }
    }

    private static void ValidateFiles3(DbfStruct dbs, AbsPath pathDbf, AbsPath pathDbt, int recordCount, int memoCount)
    {
      int recordSize = dbs.RecordSize;
      int minDbfLen = 32 + dbs.Count * 32 + recordCount * recordSize + 2;
      long realDbfLen = new FileInfo(pathDbf.Path).Length;
      Assert.GreaterOrEqual(realDbfLen, minDbfLen, "File.Length(dbf) #1");

      using (FileStream fs = new FileStream(pathDbf.Path, FileMode.Open))
      {
        using (BinaryReader rdr = new BinaryReader(fs, System.Text.Encoding.ASCII))
        {
          Assert.AreEqual(dbs.HasMemo ? 0x83 : 0x03, rdr.ReadByte(), "DBF.FileType");

          int y = rdr.ReadByte();
          int m = rdr.ReadByte();
          int d = rdr.ReadByte();
          ValidateDate(y, m, d);

          uint rc = rdr.ReadUInt32();
          Assert.AreEqual(recordCount, (int)rc, "DBF.RecordCount");

          int dataOff = rdr.ReadInt16();
          Assert.GreaterOrEqual(dataOff, 32 + 32 * dbs.Count + 1, "DBF.DataOffset");

          Assert.AreEqual(recordSize, rdr.ReadInt16(), "DBF.RecordSize");

          StringBuilder sb = new StringBuilder();
          for (int i = 0; i < dbs.Count; i++)
          {
            fs.Seek(32 + 32 * i, SeekOrigin.Begin);
            sb.Length = 0;
            for (int j = 0; j < 10; j++)
            {
              char ch = rdr.ReadChar();
              if (ch == '\0')
                break;
              sb.Append(ch);
            }
            Assert.AreEqual(dbs[i].Name.ToUpperInvariant(), sb.ToString(), "DBF.Field[" + i.ToString() + "].Name");

            fs.Seek(32 + 32 * i + 11, SeekOrigin.Begin);
            Assert.AreEqual(dbs[i].Type, rdr.ReadChar(), "DBF.Field[" + i.ToString() + "].Type");

            fs.Seek(32 + 32 * i + 16, SeekOrigin.Begin);
            int len, prec;
            if (dbs[i].Type == 'N')
            {
              len = rdr.ReadByte();
              prec = rdr.ReadByte();
            }
            else
            {
              len = rdr.ReadInt16();
              prec = 0;
            }
            Assert.AreEqual(dbs[i].Length, len, "DBF.Field[" + i.ToString() + "].Length");
            Assert.AreEqual(dbs[i].Precision, prec, "DBF.Field[" + i.ToString() + "].Precision");
          }
          fs.Seek(32 + 32 * dbs.Count, SeekOrigin.Begin);
          Assert.AreEqual(0x0D, rdr.ReadByte(), "DBF.FieldList terminator");


          int wantedDbfLen = dataOff + dbs.RecordSize * recordCount + 1;
          Assert.AreEqual(wantedDbfLen, realDbfLen, "File.Length(dbf) #2");

          for (int i = 0; i < recordCount; i++)
          {
            fs.Seek(dataOff + i * recordSize, SeekOrigin.Begin);
            Assert.IsTrue(" *".IndexOf(rdr.ReadChar()) >= 0, "Record deletion marker RecNo=" + (i + 1).ToString());
          }
        }
      }

      if (dbs.HasMemo)
      {
        int minDbtLen = 512;
        if (memoCount > 0)
          minDbfLen += (memoCount - 1) * 512 + 1; // последняя запись не обязяна занимать весь 512-байтный блок
        Assert.GreaterOrEqual(new FileInfo(pathDbt.Path).Length, minDbtLen, "File.Length(dbt)");
      }
    }

    private static void ValidateDate(int y, int m, int d)
    {
      // Год не проверяем. Excel 2003 при создании файла записывает год как Year-1900. Соответственно, для 2023 года записывается значение 123.
      // Assert.LessOrEqual(y, 99, "Year");

      Assert.GreaterOrEqual(m, 1, "Month");
      Assert.LessOrEqual(m, 12, "Month");
      Assert.GreaterOrEqual(d, 1, "Day");
      Assert.LessOrEqual(d, DateTime.DaysInMonth(y + 2000, m), "Day");
    }

    #endregion

    #region Конструкторы для открытия DBF-файлов

    [TestCase("dbase3.dbf", "", DbfFileFormat.dBase3)]
    [TestCase("dbase3m.dbf", "dbase3m.dbt", DbfFileFormat.dBase3)]
    public void Constructor_OpenFile(string fileNameDbf, string fileNameDbt, DbfFileFormat wantedFileFormat)
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath pathDbf = new AbsPath(dir.Dir, fileNameDbf);
        byte[] bDbf = (byte[])(DbfResource.ResourceManager.GetObject(fileNameDbf.Replace('.', '_')));
        File.WriteAllBytes(pathDbf.Path, bDbf);

        AbsPath pathDbt = AbsPath.Empty;
        if (fileNameDbt.Length > 0)
        {
          pathDbt = new AbsPath(dir.Dir, fileNameDbt);
          byte[] bDbt = (byte[])(DbfResource.ResourceManager.GetObject(fileNameDbt.Replace('.', '_')));
          File.WriteAllBytes(pathDbt.Path, bDbt);
        }

        DbfFileFormat format;
        DbfStruct dbs;
        using (DbfFile sut = new DbfFile(pathDbf, System.Text.Encoding.GetEncoding(1251)))
        {
          Assert.IsTrue(sut.DBStruct.IsReadOnly, "DBStruct.IsReadOnly");
          Assert.AreEqual(1251, sut.Encoding.CodePage, "Encoding.CodePage");
          Assert.AreEqual(wantedFileFormat, sut.Format, "Format");
          Assert.AreEqual(3, sut.RecordCount, "RecordCount");
          Assert.AreEqual(fileNameDbt.Length > 0, sut.HasMemoFile, "HasMemoFile");
          Assert.IsFalse(sut.IsDisposed, "IsDisposed");
          Assert.IsTrue(sut.SkipDeleted, "SkipDeleted");
          //Assert.IsFalse(sut.TableModified, "TableModified");
          Assert.AreEqual(0, sut.RecNo, "RecNo");
          Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
          Assert.IsFalse(sut.MemoFileMissing, "MemoFileMissing");

          format = sut.Format;
          dbs = sut.DBStruct;
        }
        ValidateFiles(format, dbs, pathDbf, pathDbt, 3, fileNameDbt.Length > 0 ? 2 : 0);
      }
    }

    #endregion

    #region Тестовый объект для разных форматов и режимов пустой/заполненной таблицы

    public static readonly string[] TestFormatCodes = new string[] { "dbase2", "dbase3", "dbase3m", "dbase4" /*, "dbase4m" пока не поддерживается */};

    /// <summary>
    /// Обертка, содержащая временный каталог и объект DbfFile
    /// </summary>
    private struct DbfFileTestWrapper : IDisposable
    {
      #region Конструктор и Dispose()

      /// <summary>
      /// Создает временный каталог и объект DbfFile
      /// </summary>
      /// <param name="formatCode">Кодированная строка из TestFormatCodes</param>
      /// <param name="emptyTable">true - создается пустой файл, false - содержащий тестовые данные</param>
      public DbfFileTestWrapper(string format, bool emptyTable)
      {
        _Dir = new TempDirectory();
        _PathDbf = new AbsPath(_Dir.Dir, format + ".dbf");
        bool hasMemo = format.EndsWith("m");
        if (hasMemo)
          _PathDbt = new AbsPath(_Dir.Dir, format + ".dbt");
        else
          _PathDbt = AbsPath.Empty;

        string sEnum = format;
        if (hasMemo)
          sEnum = sEnum.Substring(0, sEnum.Length - 1);
        _FileFormat = StdConvert.ToEnum<DbfFileFormat>(sEnum);


        if (emptyTable)
        {
          // Создаем файл программно
          _SUT = new DbfFile(_PathDbf, CreateTestDbfStruct(_FileFormat, hasMemo), System.Text.Encoding.GetEncoding(1251), _FileFormat);
        }
        else
        {
          // Загружаем файл из ресурсов
          File.WriteAllBytes(_PathDbf.Path, (byte[])(DbfResource.ResourceManager.GetObject(format + "_dbf")));
          if (hasMemo)
            File.WriteAllBytes(_PathDbt.Path, (byte[])(DbfResource.ResourceManager.GetObject(format + "_dbt")));
          _SUT = new DbfFile(_PathDbf, System.Text.Encoding.GetEncoding(1251), false);

          CompareDbfStruct(CreateTestDbfStruct(_FileFormat, hasMemo), _SUT.DBStruct);
        }
        Assert.AreEqual(_FileFormat, _SUT.Format, "FileFormat");
        Assert.AreEqual(hasMemo, _SUT.HasMemoFile, "HasMemoFile");
        Assert.IsFalse(_SUT.MemoFileMissing, "MemoFileMissing");

      }

      private static void CompareDbfStruct(DbfStruct dbs1, DbfStruct dbs2)
      {
        Assert.AreEqual(dbs1.GetNames(), dbs2.GetNames(), "Field Names");
        for (int i = 0; i < dbs1.Count; i++)
        {
          Assert.AreEqual(dbs1[i].Type, dbs2[i].Type, dbs1[i].Name + " - Type");
          Assert.AreEqual(dbs1[i].Length, dbs2[i].Length, dbs1[i].Name + " - Length");
          Assert.AreEqual(dbs1[i].Precision, dbs2[i].Precision, dbs1[i].Name + " - Precision");
        }
      }

      public void Dispose()
      {
        int recordCount = 0;
        int memoCount = 0;
        DbfStruct dbs = null;
        DbfFileFormat fileFormat = DbfFileFormat.Undefined;
        if (_SUT != null)
        {
          dbs = _SUT.DBStruct;
          fileFormat = _SUT.Format;
          recordCount = _SUT.RecordCount;
          if (dbs.HasMemo)
          {
            for (int recNo = 1; recNo < _SUT.RecordCount; recNo++)
            {
              _SUT.RecNo = recNo;
              if (_SUT.GetString("F5").Length > 0)
                memoCount++;
            }
          }

          _SUT.Dispose();
          _SUT = null;
        }

        try
        {
          if (dbs != null)
            ValidateFiles(fileFormat, dbs, _PathDbf, _PathDbt, recordCount, memoCount);
        }
        finally
        {
          if (_Dir != null)
          {
            _Dir.Dispose();
            _Dir = null;
          }
        }
      }

      #endregion

      #region Свойства

      private TempDirectory _Dir;

      public DbfFile SUT { get { return _SUT; } }
      private DbfFile _SUT;

      public AbsPath PathDbf { get { return _PathDbf; } }
      private AbsPath _PathDbf;

      public AbsPath PathDbt { get { return _PathDbt; } }
      private AbsPath _PathDbt;

      public DbfFileFormat FileFormat { get { return _FileFormat; } }
      private DbfFileFormat _FileFormat;

      #endregion

      #region Reload()

      /// <summary>
      /// Закрывает и открывает файл заново. Создает новый объект <see cref="DbfFile"/>
      /// </summary>
      public void Reload()
      {
        _SUT.Flush();
        _SUT.Dispose();
        _SUT = null;

        _SUT = new DbfFile(_PathDbf, System.Text.Encoding.GetEncoding(1251), false);
        Assert.AreEqual(_FileFormat, _SUT.Format, "FileFormat");
      }

      #endregion
    }

    #endregion

    #region RecNo

    [Test]
    public void RecNo()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", false))
      {
        wrapper.SUT.RecNo = 2;
        wrapper.SUT.RecordDeleted = true; // проверить, что не будет пропущено

        for (int i = 1; i <= 3; i++)
        {
          wrapper.SUT.RecNo = i;
          Assert.AreEqual(111 * i, wrapper.SUT.GetInt("F21"), "Value #" + i);
          Assert.AreEqual(i, wrapper.SUT.RecNo, "RecNo get()");
        }

        wrapper.SUT.RecNo = 0;
        Assert.AreEqual(0, wrapper.SUT.RecNo, "#0");

        Assert.Catch(delegate() { wrapper.SUT.RecNo = 4; }, "#4");
      }
    }

    #endregion

    #region Read()

    [Test]
    public void Read()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3", false))
      {
        int res = 0;
        while (wrapper.SUT.Read())
          res += wrapper.SUT.GetInt("F21");
        Assert.AreEqual(111 + 222 + 333, res);
      }
    }

    #endregion

    #region Чтение / установка значений

    #region Get/SetValue()

    [Test]
    public void GetValue([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, false))
      {
        wrapper.SUT.RecNo = 1;
        Assert.AreEqual("Значение 1", wrapper.SUT.GetValue("F1"), "F1");
        Assert.AreEqual(111, wrapper.SUT.GetValue("F21"), "F21");
        Assert.AreEqual(111L, wrapper.SUT.GetValue("F22"), "F22");
        Assert.AreEqual(111.11m, wrapper.SUT.GetValue("F23"), "F23");
        Assert.AreEqual(true, wrapper.SUT.GetValue("F3"), "F3");
        if (wrapper.SUT.DBStruct.IndexOf("F4") >= 0)
          Assert.AreEqual(new DateTime(2023, 3, 18), wrapper.SUT.GetValue("F4"), "F4");
        if (wrapper.SUT.DBStruct.IndexOf("F5") >= 0)
          Assert.AreEqual("Длинный текст 1", wrapper.SUT.GetValue("F5"), "F5");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(111.11, wrapper.SUT.GetValue("F6"), "F6");
      }
    }

    [Test]
    public void GetValue_emptyValues()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", true))
      {
        wrapper.SUT.AppendRecord();
        for (int i = 0; i < wrapper.SUT.DBStruct.Count; i++)
          Assert.AreEqual(DBNull.Value, wrapper.SUT.GetValue(i), wrapper.SUT.DBStruct[i].Name);
      }
    }

    [Test]
    public void SetValue_string([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, true))
      {
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetValue("F1", "ABC");
        wrapper.SUT.SetValue("F21", "66");
        wrapper.SUT.SetValue("F22", "77");
        wrapper.SUT.SetValue("F23", "12.34");
        wrapper.SUT.SetValue("F3", "T");
        if (wrapper.SUT.DBStruct.IndexOf("F4") >= 0)
          wrapper.SUT.SetValue("F4", "20230321");
        if (wrapper.SUT.DBStruct.IndexOf("F5") >= 0)
          wrapper.SUT.SetValue("F5", "DEF");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          wrapper.SUT.SetValue("F6", "0.12");

        wrapper.Reload();
        wrapper.SUT.RecNo = 1;

        Assert.AreEqual("ABC", wrapper.SUT.GetString("F1"), "F1");
        Assert.AreEqual(66, wrapper.SUT.GetInt("F21"), "F21");
        Assert.AreEqual(77, wrapper.SUT.GetInt("F22"), "F22");
        Assert.AreEqual(12.34m, wrapper.SUT.GetDecimal("F23"), "F23");
        Assert.IsTrue(wrapper.SUT.GetBool("F3"), "F3");
        if (wrapper.SUT.DBStruct.IndexOf("F4") >= 0)
          Assert.AreEqual(new DateTime(2023, 3, 21), wrapper.SUT.GetNullableDate("F4"), "F4");
        if (wrapper.SUT.DBStruct.IndexOf("F5") >= 0)
          Assert.AreEqual("DEF", wrapper.SUT.GetString("F5"), "F5");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(0.12m, wrapper.SUT.GetDecimal("F6"), "F6");
      }
    }

    [Test]
    public void SetValue_int([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, true))
      {
        wrapper.SUT.AppendRecord();
        //wrapper.SUT.SetValue("F1", 12);
        wrapper.SUT.SetValue("F21", 34);
        wrapper.SUT.SetValue("F22", 56);
        wrapper.SUT.SetValue("F23", 78);
        //wrapper.SUT.SetValue("F4", 1);
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          wrapper.SUT.SetValue("F6", 78);

        wrapper.Reload();
        wrapper.SUT.RecNo = 1;

        //Assert.AreEqual("12", wrapper.SUT.GetString("F1"), "F1");
        Assert.AreEqual(34, wrapper.SUT.GetInt("F21"), "F21");
        Assert.AreEqual(56, wrapper.SUT.GetInt("F22"), "F22");
        Assert.AreEqual(78.00m, wrapper.SUT.GetDecimal("F23"), "F23");
        //Assert.IsTrue(wrapper.SUT.GetBool("F4"), "F4");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(78.0, wrapper.SUT.GetDouble("F6"), "F6");
      }
    }

    [Test]
    public void SetValue_decimal([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, true))
      {
        wrapper.SUT.AppendRecord();
        //wrapper.SUT.SetValue("F1", 12m);
        wrapper.SUT.SetValue("F21", 12m);
        wrapper.SUT.SetValue("F22", 34m);
        wrapper.SUT.SetValue("F23", 56m);
        //wrapper.SUT.SetValue("F4", 1m);
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          wrapper.SUT.SetValue("F6", 78m);

        wrapper.Reload();
        wrapper.SUT.RecNo = 1;

        //Assert.AreEqual("12", wrapper.SUT.GetString("F1"), "F1");
        Assert.AreEqual(12, wrapper.SUT.GetInt("F21"), "F21");
        Assert.AreEqual(34, wrapper.SUT.GetInt64("F22"), "F22");
        Assert.AreEqual(56.00m, wrapper.SUT.GetDecimal("F23"), "F23");
        //Assert.IsTrue(wrapper.SUT.GetBool("F4"), "F4");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(78.0, wrapper.SUT.GetDouble("F6"), "F6");
      }
    }

    [Test]
    public void SetValue_bool()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", true))
      {
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetValue("F3", true);

        wrapper.Reload();
        wrapper.SUT.RecNo = 1;

        Assert.IsTrue(wrapper.SUT.GetBool("F3"), "F3");
      }
    }

    [Test]
    public void SetValue_dateTime()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", true))
      {
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetValue("F4", new DateTime(2023, 3, 21));

        wrapper.Reload();
        wrapper.SUT.RecNo = 1;

        Assert.AreEqual(new DateTime(2023, 3, 21), wrapper.SUT.GetNullableDate("F4"), "F4");
      }
    }

    [Test]
    public void SetValue_emptyValues()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", false))
      {
        byte[] wantedValue = new byte[wrapper.SUT.DBStruct.RecordSize];
        DataTools.FillArray<byte>(wantedValue, 32); // пробелы

        wrapper.SUT.RecNo = 1;
        for (int i = 0; i < wrapper.SUT.DBStruct.Count; i++)
          wrapper.SUT.SetValue(i, null);
        Assert.AreEqual(wantedValue, wrapper.SUT.RecordBuffer, "null");

        wrapper.SUT.RecNo = 2;
        for (int i = 0; i < wrapper.SUT.DBStruct.Count; i++)
          wrapper.SUT.SetValue(i, DBNull.Value);
        Assert.AreEqual(wantedValue, wrapper.SUT.RecordBuffer, "DBNull");

        wrapper.SUT.RecNo = 3;
        for (int i = 0; i < wrapper.SUT.DBStruct.Count; i++)
          wrapper.SUT.SetValue(i, "");
        Assert.AreEqual(wantedValue, wrapper.SUT.RecordBuffer, "String.Empty");
      }
    }

    #endregion

    #region Get/SetString()

    [Test]
    public void GetString([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, false))
      {
        wrapper.SUT.RecNo = 1;
        Assert.AreEqual("Значение 1", wrapper.SUT.GetString("F1"), "F1");
        Assert.AreEqual("111", wrapper.SUT.GetString("F21").Trim(), "F21");
        Assert.AreEqual("111", wrapper.SUT.GetString("F22").Trim(), "F22");
        Assert.AreEqual("111.11", wrapper.SUT.GetString("F23").Trim(), "F23");
        Assert.AreEqual("T", wrapper.SUT.GetString("F3"), "F3");
        if (wrapper.SUT.DBStruct.IndexOf("F4") >= 0)
          Assert.AreEqual("20230318", wrapper.SUT.GetString("F4"), "F4");
        if (wrapper.SUT.DBStruct.IndexOf("F5") >= 0)
          Assert.AreEqual("Длинный текст 1", wrapper.SUT.GetString("F5"), "F5");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual("111.11", wrapper.SUT.GetString("F6").Trim(), "F6");
      }
    }

    [Test]
    public void SetString([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, true))
      {
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetString("F1", "ABC");
        wrapper.SUT.SetString("F21", "66");
        wrapper.SUT.SetString("F22", "77");
        wrapper.SUT.SetString("F23", "12.34");
        wrapper.SUT.SetString("F3", "T");
        if (wrapper.SUT.DBStruct.IndexOf("F4") >= 0)
          wrapper.SUT.SetString("F4", "20230321");
        if (wrapper.SUT.DBStruct.IndexOf("F5") >= 0)
          wrapper.SUT.SetString("F5", "DEF");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          wrapper.SUT.SetString("F6", "1.25");

        wrapper.Reload();
        wrapper.SUT.RecNo = 1;

        Assert.AreEqual("ABC", wrapper.SUT.GetString("F1"), "F1");
        Assert.AreEqual(66, wrapper.SUT.GetInt("F21"), "F21");
        Assert.AreEqual(77, wrapper.SUT.GetInt("F22"), "F22");
        Assert.AreEqual(12.34m, wrapper.SUT.GetDecimal("F23"), "F23");
        Assert.IsTrue(wrapper.SUT.GetBool("F3"), "F3");
        if (wrapper.SUT.DBStruct.IndexOf("F4") >= 0)
          Assert.AreEqual(new DateTime(2023, 3, 21), wrapper.SUT.GetNullableDate("F4"), "F4");
        if (wrapper.SUT.DBStruct.IndexOf("F5") >= 0)
          Assert.AreEqual("DEF", wrapper.SUT.GetString("F5"), "F5");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(1.25m, wrapper.SUT.GetDecimal("F6"), "F6");
      }
    }

    [Test]
    public void SetString_emptyValues()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", false))
      {
        byte[] wantedValue = new byte[wrapper.SUT.DBStruct.RecordSize];
        DataTools.FillArray<byte>(wantedValue, 32); // пробелы

        wrapper.SUT.RecNo = 1;
        for (int i = 0; i < wrapper.SUT.DBStruct.Count; i++)
          wrapper.SUT.SetString(i, "");
        Assert.AreEqual(wantedValue, wrapper.SUT.RecordBuffer);
      }
    }

    [Test]
    public void SetString_trim([Values(true, false)]bool trim)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3", true))
      {
        wrapper.SUT.AppendRecord();
        string value = new string('X', wrapper.SUT.DBStruct["F1"].Length + 1);
        TestDelegate d = delegate() { wrapper.SUT.SetString("F1", value, trim); };

        if (trim)
          Assert.DoesNotThrow(d);
        else
          Assert.Catch(d);
      }
    }

    #endregion

    #region GetSetInt()

    [Test]
    public void GetInt([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, false))
      {
        wrapper.SUT.RecNo = 1;
        Assert.AreEqual(111, wrapper.SUT.GetInt("F21"), "F21");
        Assert.AreEqual(111, wrapper.SUT.GetInt("F22"), "F22");
        Assert.AreEqual(111, wrapper.SUT.GetInt("F23"), "F23");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(111, wrapper.SUT.GetInt("F21"), "F6");

        wrapper.SUT.AppendRecord();
        Assert.AreEqual(0, wrapper.SUT.GetInt("F21"), "F21");
        Assert.AreEqual(0, wrapper.SUT.GetInt("F22"), "F22");
        Assert.AreEqual(0, wrapper.SUT.GetInt("F23"), "F3");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(0, wrapper.SUT.GetInt("F23"), "F6");
      }
    }

    [Test]
    public void GetInt_outOfRange()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3", true))
      {
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetInt64("F22", 2000000000L);
        Assert.DoesNotThrow(delegate() { wrapper.SUT.GetInt("F22"); }, "2000000000");

        wrapper.SUT.SetInt64("F22", 3000000000L);
        Assert.Catch(delegate() { wrapper.SUT.GetInt("F22"); }, "3000000000");
      }
    }

    [Test]
    public void SetInt([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, true))
      {
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetInt("F21", 123);
        wrapper.SUT.SetInt("F22", 456);
        wrapper.SUT.SetInt("F23", 789);
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          wrapper.SUT.SetInt("F6", 333);

        wrapper.Reload();
        wrapper.SUT.RecNo = 1;

        Assert.AreEqual(123, wrapper.SUT.GetInt("F21"), "F21");
        Assert.AreEqual(456, wrapper.SUT.GetInt("F22"), "F22");
        Assert.AreEqual(789m, wrapper.SUT.GetDecimal("F23"), "F23");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(333.0, wrapper.SUT.GetDouble("F6"), "F6");
      }
    }

    [Test]
    public void SetInt_outOfRange()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3", true))
      {
        wrapper.SUT.AppendRecord();
        Assert.DoesNotThrow(delegate() { wrapper.SUT.SetInt("F21", 999); }, "999");
        Assert.Catch<ArgumentOutOfRangeException>(delegate() { wrapper.SUT.SetInt("F21", 1000); }, "1000");
        Assert.DoesNotThrow(delegate() { wrapper.SUT.SetInt("F21", -99); }, "-99");
        Assert.Catch<ArgumentOutOfRangeException>(delegate() { wrapper.SUT.SetInt("F21", -100); }, "-100");
      }
    }

    #endregion

    #region GetSetInt64()

    [Test]
    public void GetInt64([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, false))
      {
        wrapper.SUT.RecNo = 1;
        Assert.AreEqual(111L, wrapper.SUT.GetInt64("F21"), "F21");
        Assert.AreEqual(111L, wrapper.SUT.GetInt64("F22"), "F22");
        Assert.AreEqual(111L, wrapper.SUT.GetInt64("F23"), "F23");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(111L, wrapper.SUT.GetInt64("F6"), "F6");

        wrapper.SUT.AppendRecord();
        Assert.AreEqual(0L, wrapper.SUT.GetInt64("F21"), "F21");
        Assert.AreEqual(0L, wrapper.SUT.GetInt64("F22"), "F22");
        Assert.AreEqual(0L, wrapper.SUT.GetInt64("F23"), "F23");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(0L, wrapper.SUT.GetInt64("F6"), "F6");
      }
    }

    [Test]
    public void SetInt64([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, true))
      {
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetInt64("F21", 123L);
        wrapper.SUT.SetInt64("F22", 456L);
        wrapper.SUT.SetInt64("F23", 789L);
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          wrapper.SUT.SetInt64("F6", 444L);

        wrapper.Reload();
        wrapper.SUT.RecNo = 1;

        Assert.AreEqual(123, wrapper.SUT.GetInt("F21"), "F21");
        Assert.AreEqual(456, wrapper.SUT.GetInt("F22"), "F22");
        Assert.AreEqual(789m, wrapper.SUT.GetDecimal("F23"), "F23");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(444.0, wrapper.SUT.GetDouble("F6"), "F6");
      }
    }

    [Test]
    public void SetInt64_outOfRange()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3", true))
      {
        wrapper.SUT.AppendRecord();
        Assert.DoesNotThrow(delegate() { wrapper.SUT.SetInt64("F22", 9999999999L); }, "9999999999");
        Assert.Catch<ArgumentOutOfRangeException>(delegate() { wrapper.SUT.SetInt64("F22", 10000000000L); }, "10000000000");
        Assert.DoesNotThrow(delegate() { wrapper.SUT.SetInt64("F22", -999999999L); }, "-999999999");
        Assert.Catch<ArgumentOutOfRangeException>(delegate() { wrapper.SUT.SetInt64("F22", -1000000000L); }, "-1000000000");
      }
    }

    #endregion

    #region GetSetSingle()

    [Test]
    public void GetSingle([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, false))
      {
        wrapper.SUT.RecNo = 1;
        Assert.AreEqual(111f, wrapper.SUT.GetSingle("F21"), "F21");
        Assert.AreEqual(111f, wrapper.SUT.GetSingle("F22"), "F22");
        Assert.AreEqual(111.11f, wrapper.SUT.GetSingle("F23"), "F23");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(111.11f, wrapper.SUT.GetSingle("F6"), "F6");

        wrapper.SUT.AppendRecord();
        Assert.AreEqual(0f, wrapper.SUT.GetSingle("F21"), "F21");
        Assert.AreEqual(0f, wrapper.SUT.GetSingle("F22"), "F22");
        Assert.AreEqual(0f, wrapper.SUT.GetSingle("F23"), "F23");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(0f, wrapper.SUT.GetSingle("F6"), "F6");
      }
    }

    [Test]
    public void SetSingle([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, true))
      {
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetSingle("F21", 123f);
        wrapper.SUT.SetSingle("F22", 456);
        wrapper.SUT.SetSingle("F23", 789.5f);
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          wrapper.SUT.SetSingle("F6", 222.5f);

        wrapper.Reload();
        wrapper.SUT.RecNo = 1;

        Assert.AreEqual(123, wrapper.SUT.GetInt("F21"), "F21");
        Assert.AreEqual(456, wrapper.SUT.GetInt("F22"), "F22");
        Assert.AreEqual(789.5m, wrapper.SUT.GetDecimal("F23"), "F23");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(222.5, wrapper.SUT.GetDouble("F6"), "F6");
      }
    }

    [Test]
    public void SetSingle_outOfRange()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3", true))
      {
        wrapper.SUT.AppendRecord();
        // В отличие от Decimal, нельзя тестировать значение 999999999, т.к. может возникнуть внутренняя ошибка округления
        Assert.DoesNotThrow(delegate() { wrapper.SUT.SetSingle("F23", 900000000f); }, "900000000");
        Assert.Catch<ArgumentOutOfRangeException>(delegate() { wrapper.SUT.SetSingle("F23", 1000000000f); }, "1000000000");
        Assert.DoesNotThrow(delegate() { wrapper.SUT.SetSingle("F23", -90000000f); }, "-90000000");
        Assert.Catch<ArgumentOutOfRangeException>(delegate() { wrapper.SUT.SetSingle("F23", -100000000f); }, "-100000000");
      }
    }

    #endregion

    #region GetSetDouble()

    [Test]
    public void GetDouble([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, false))
      {
        wrapper.SUT.RecNo = 1;
        Assert.AreEqual(111.0, wrapper.SUT.GetDouble("F21"), "F21");
        Assert.AreEqual(111.0, wrapper.SUT.GetDouble("F22"), "F22");
        Assert.AreEqual(111.11, wrapper.SUT.GetDouble("F23"), "F23");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(111.11, wrapper.SUT.GetDouble("F6"), "F6");

        wrapper.SUT.AppendRecord();
        Assert.AreEqual(0.0, wrapper.SUT.GetDouble("F21"), "F21");
        Assert.AreEqual(0.0, wrapper.SUT.GetDouble("F22"), "F22");
        Assert.AreEqual(0.0, wrapper.SUT.GetDouble("F23"), "F23");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(0.0, wrapper.SUT.GetDouble("F6"), "F6");
      }
    }

    [Test]
    public void SetDouble([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, true))
      {
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetDouble("F21", 123.0);
        wrapper.SUT.SetDouble("F22", 456.0);
        wrapper.SUT.SetDouble("F23", 789.5);
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          wrapper.SUT.SetDouble("F6", 222.2);

        wrapper.Reload();
        wrapper.SUT.RecNo = 1;

        Assert.AreEqual(123, wrapper.SUT.GetInt("F21"), "F21");
        Assert.AreEqual(456, wrapper.SUT.GetInt("F22"), "F22");
        Assert.AreEqual(789.5m, wrapper.SUT.GetDecimal("F23"), "F23");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(222.2, wrapper.SUT.GetDouble("F6"), "F6");
      }
    }

    [Test]
    public void SetDouble_outOfRange()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase4", true))
      {
        wrapper.SUT.AppendRecord();
        // В отличие от Decimal, нельзя тестировать значение 999999999, т.к. может возникнуть внутренняя ошибка округления
        Assert.DoesNotThrow(delegate() { wrapper.SUT.SetDouble("F6", 900000000.0); }, "900000000");
        Assert.Catch<ArgumentOutOfRangeException>(delegate() { wrapper.SUT.SetDouble("F6", 1000000000.0); }, "1000000000");
        Assert.DoesNotThrow(delegate() { wrapper.SUT.SetDouble("F6", -90000000.0); }, "-90000000");
        Assert.Catch<ArgumentOutOfRangeException>(delegate() { wrapper.SUT.SetDouble("F6", -100000000.0); }, "-100000000");
      }
    }

    #endregion

    #region GetSetDecimal()

    [Test]
    public void GetDecimal([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, false))
      {
        wrapper.SUT.RecNo = 1;
        Assert.AreEqual(111m, wrapper.SUT.GetDecimal("F21"), "F21");
        Assert.AreEqual(111m, wrapper.SUT.GetDecimal("F22"), "F22");
        Assert.AreEqual(111.11m, wrapper.SUT.GetDecimal("F23"), "F23");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(111.11m, wrapper.SUT.GetDecimal("F6"), "F6");

        wrapper.SUT.AppendRecord();
        Assert.AreEqual(0m, wrapper.SUT.GetDecimal("F21"), "F21");
        Assert.AreEqual(0m, wrapper.SUT.GetDecimal("F22"), "F22");
        Assert.AreEqual(0m, wrapper.SUT.GetDecimal("F23"), "F23");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(0m, wrapper.SUT.GetDecimal("F6"), "F6");
      }
    }

    [Test]
    public void SetDecimal([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, true))
      {
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetDecimal("F21", 123m);
        wrapper.SUT.SetDecimal("F22", 456m);
        wrapper.SUT.SetDecimal("F23", 789.5m);
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          wrapper.SUT.SetDecimal("F6", 555.5m);

        wrapper.Reload();
        wrapper.SUT.RecNo = 1;

        Assert.AreEqual(123, wrapper.SUT.GetInt("F21"), "F21");
        Assert.AreEqual(456, wrapper.SUT.GetInt("F22"), "F22");
        Assert.AreEqual(789.5m, wrapper.SUT.GetDecimal("F23"), "F23");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(555.5, wrapper.SUT.GetDouble("F6"), "F6");
      }
    }

    [Test]
    public void SetDecimal_outOfRange()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3", true))
      {
        wrapper.SUT.AppendRecord();
        Assert.DoesNotThrow(delegate() { wrapper.SUT.SetDecimal("F23", 999999999m); }, "999999999");
        Assert.Catch<ArgumentOutOfRangeException>(delegate() { wrapper.SUT.SetDecimal("F23", 1000000000m); }, "1000000000");
        Assert.DoesNotThrow(delegate() { wrapper.SUT.SetDecimal("F23", -99999999); }, "-99999999");
        Assert.Catch<ArgumentOutOfRangeException>(delegate() { wrapper.SUT.SetDecimal("F23", -100000000m); }, "-100000000");
      }
    }

    #endregion

    #region GetSetBool()

    [Test]
    public void GetBool()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", false))
      {
        wrapper.SUT.RecNo = 1;
        Assert.IsTrue(wrapper.SUT.GetBool("F3"), "#1");

        wrapper.SUT.RecNo = 2;
        Assert.IsFalse(wrapper.SUT.GetBool("F3"), "#2");

        wrapper.SUT.AppendRecord();
        Assert.IsFalse(wrapper.SUT.GetBool("F3"), "new");
      }
    }

    [Test]
    public void SetBool()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", true))
      {
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetBool("F3", false);
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetBool("F3", true);

        wrapper.Reload();
        wrapper.SUT.RecNo = 1;
        Assert.IsFalse(wrapper.SUT.GetBool("F3"), "#1");
        wrapper.SUT.RecNo = 2;
        Assert.IsTrue(wrapper.SUT.GetBool("F3"), "#2");
      }
    }

    #endregion

    #region GetSetNullableDate()

    [Test]
    public void GetNullableDate()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", false))
      {
        wrapper.SUT.RecNo = 1;
        Assert.AreEqual(new DateTime(2023, 3, 18), wrapper.SUT.GetNullableDate("F4"), "#1");

        wrapper.SUT.RecNo = 2;
        Assert.AreEqual(new DateTime(2023, 3, 19), wrapper.SUT.GetNullableDate("F4"), "#2");

        wrapper.SUT.AppendRecord();
        Assert.IsNull(wrapper.SUT.GetNullableDate("F4"), "new");
      }
    }

    [Test]
    public void SetNullableDate()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", true))
      {
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetNullableDate("F4", null);
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetNullableDate("F4", new DateTime(2023, 3, 21));

        wrapper.Reload();
        wrapper.SUT.RecNo = 1;
        Assert.AreEqual("", wrapper.SUT.GetString("F4"), "#1");
        wrapper.SUT.RecNo = 2;
        Assert.AreEqual("20230321", wrapper.SUT.GetString("F4"), "#2");
      }
    }

    #endregion

    #region GetSetDate()

    [Test]
    public void GetDate()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", false))
      {
        wrapper.SUT.RecNo = 1;
        Assert.AreEqual(new DateTime(2023, 3, 18), wrapper.SUT.GetDate("F4"), "#1");

        wrapper.SUT.RecNo = 2;
        Assert.AreEqual(new DateTime(2023, 3, 19), wrapper.SUT.GetDate("F4"), "#2");
      }
    }

    [Test]
    public void SetDate()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", true))
      {
        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetDate("F4", new DateTime(2023, 3, 21));

        wrapper.Reload();
        wrapper.SUT.RecNo = 1;
        Assert.AreEqual("20230321", wrapper.SUT.GetString("F4"), "#1");
      }
    }

    #endregion

    #region SetNull()/IsDBNull()

    [Test]
    public void SetNull()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", false))
      {
        byte[] wantedValue = new byte[wrapper.SUT.DBStruct.RecordSize];
        DataTools.FillArray<byte>(wantedValue, 32); // пробелы

        wrapper.SUT.RecNo = 1;
        for (int i = 0; i < wrapper.SUT.DBStruct.Count; i++)
          wrapper.SUT.SetNull(i);
        Assert.AreEqual(wantedValue, wrapper.SUT.RecordBuffer);
      }
    }

    [Test]
    public void IsDBNull()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", false))
      {
        wrapper.SUT.RecNo = 1;
        for (int i = 0; i < wrapper.SUT.DBStruct.Count; i++)
          Assert.IsFalse(wrapper.SUT.IsDBNull(i), "#1");

        wrapper.SUT.AppendRecord();
        for (int i = 0; i < wrapper.SUT.DBStruct.Count; i++)
          Assert.IsTrue(wrapper.SUT.IsDBNull(i), "#2");
      }
    }

    #endregion

    #endregion

    #region Добавление записей

    [Test]
    public void AppendRecord([ValueSource("TestFormatCodes")]string formatCode, [Values(true, false)]bool emptyTable)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, emptyTable))
      {
        for (int i = 0; i < 5; i++)
        {
          wrapper.SUT.AppendRecord();
          wrapper.SUT.SetValue("F1", "Hello " + StdConvert.ToString(i));
        }
        Assert.AreEqual((emptyTable ? 0 : 3) + 5, wrapper.SUT.RecordCount, "RecordCount");
        for (int i = 0; i < 5; i++)
        {
          wrapper.SUT.RecNo = wrapper.SUT.RecordCount - 4 + i;
          Assert.AreEqual("Hello " + StdConvert.ToString(i), wrapper.SUT.GetString("F1"), "Value " + i.ToString());
        }
      }
    }

    [Test]
    public void IsNewRecord()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3", true))
      {
        Assert.IsFalse(wrapper.SUT.IsNewRecord, "#1");
        wrapper.SUT.AppendRecord();
        Assert.IsTrue(wrapper.SUT.IsNewRecord, "#2");
        wrapper.SUT.AppendRecord();
        Assert.IsTrue(wrapper.SUT.IsNewRecord, "#3");
        wrapper.SUT.RecNo = 1;
        Assert.IsFalse(wrapper.SUT.IsNewRecord, "#4");
      }
    }

    #endregion

    #region Удаление записей

    [Test]
    public void RecordDeleted([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, false))
      {
        wrapper.SUT.RecNo = 2;
        wrapper.SUT.RecordDeleted = true;

        wrapper.Reload();

        Assert.AreEqual(3, wrapper.SUT.RecordCount, "RecordCount");

        wrapper.SUT.RecNo = 1;
        Assert.IsFalse(wrapper.SUT.RecordDeleted, "RecNo=1");

        wrapper.SUT.RecNo = 2;
        Assert.IsTrue(wrapper.SUT.RecordDeleted, "RecNo=2");

        wrapper.SUT.RecNo = 3;
        Assert.IsFalse(wrapper.SUT.RecordDeleted, "RecNo=3");
      }
    }


    [Test]
    public void SkipDeleted([ValueSource("TestFormatCodes")]string formatCode, [Values(true, false)] bool skipDeleted)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, false))
      {
        wrapper.SUT.RecNo = 2;
        wrapper.SUT.RecordDeleted = true;

        wrapper.Reload();
        wrapper.SUT.SkipDeleted = skipDeleted;

        int cnt = 0;
        wrapper.SUT.RecNo = 0;
        while (wrapper.SUT.Read())
          cnt++;

        Assert.AreEqual(skipDeleted ? 2 : 3, cnt);
      }
    }

    #endregion

    #region Flush()

    [Test]
    public void Flush()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", true))
      {
        for (int i = 0; i < 5; i++)
        {
          long szDbf1 = new FileInfo(wrapper.PathDbf.Path).Length;
          long szDbt1 = new FileInfo(wrapper.PathDbt.Path).Length;

          wrapper.SUT.AppendRecord();
          wrapper.SUT.SetString("F5", "Memo value");
          wrapper.SUT.Flush();

          long szDbf2 = new FileInfo(wrapper.PathDbf.Path).Length;
          long szDbt2 = new FileInfo(wrapper.PathDbt.Path).Length;
          Assert.Greater(szDbf2, szDbf1, "DBF #" + (i + 1).ToString());
          Assert.Greater(szDbt2, szDbt1, "DBT #" + (i + 1).ToString());
        }
      }
    }

    #endregion

    #region Append()

    [Test]
    public void Append([ValueSource("TestFormatCodes")]string formatCode)
    {
      DataTable table1 = new DataTable();
      table1.Columns.Add("XXX", typeof(string));
      table1.Columns.Add("F21", typeof(int));
      table1.Rows.Add("AAA", 1);
      table1.Rows.Add("BBB", 2);
      table1.Rows.Add("CCC", 3);
      table1.Rows.Add("DDD", 4);


      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, false))
      {

        Assert.AreEqual(3, wrapper.SUT.RecordCount, "RecordCount #1");
        wrapper.SUT.Append(table1);

        Assert.AreEqual(3 + 4, wrapper.SUT.RecordCount, "RecordCount #2");

        DataTable table2 = wrapper.SUT.CreateTable();
        Assert.AreEqual(new String[] { "Значение 1", "Значение 2", "Значение 3", null, null, null, null }, DataTools.GetValuesFromColumn<String>(table2, "F1"), "Values F1");
        Assert.AreEqual(new Int32[] { 111, 222, 333, 1, 2, 3, 4 }, DataTools.GetValuesFromColumn<Int32>(table2, "F21"), "Values F21");

        Assert.AreEqual(4, table1.Rows.Count, "source table RowCount"); // ничего не испортили?
      }
    }

    #endregion

    #region CreateTable()

    [Test]
    public void CreateTable([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, false))
      {
        wrapper.SUT.AppendRecord(); // чтобы была пустая строка

        DataTable table = wrapper.SUT.CreateTable();
        Assert.AreEqual(wrapper.SUT.DBStruct.GetNames(), DataTools.GetColumnNames(table), "ColumnNames");
        Assert.AreEqual(typeof(String), table.Columns["F1"].DataType, "DataType F1");
        Assert.AreEqual(typeof(Int32), table.Columns["F21"].DataType, "DataType F21");
        Assert.AreEqual(typeof(Int64), table.Columns["F22"].DataType, "DataType F22");
        Assert.AreEqual(typeof(Decimal), table.Columns["F23"].DataType, "DataType F23");
        Assert.AreEqual(typeof(Boolean), table.Columns["F3"].DataType, "DataType F3");
        if (wrapper.SUT.DBStruct.IndexOf("F4") >= 0)
          Assert.AreEqual(typeof(DateTime), table.Columns["F4"].DataType, "DataType F4");
        if (wrapper.SUT.DBStruct.IndexOf("F5") >= 0)
          Assert.AreEqual(typeof(String), table.Columns["F5"].DataType, "DataType F5");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(typeof(Double), table.Columns["F6"].DataType, "DataType F6");

        Assert.AreEqual(4, table.Rows.Count, "RowCount");
        Assert.AreEqual(new String[] { "Значение 1", "Значение 2", "Значение 3", null }, DataTools.GetValuesFromColumn<String>(table, "F1"), "Values F1");
        Assert.AreEqual(new Int32[] { 111, 222, 333, 0 }, DataTools.GetValuesFromColumn<Int32>(table, "F21"), "Values F21");
        Assert.AreEqual(new Int64[] { 111L, 222L, 333L, 0L }, DataTools.GetValuesFromColumn<Int64>(table, "F22"), "Values F22");
        Assert.AreEqual(new Decimal[] { 111.11m, 222.22m, 333.33m, 0m }, DataTools.GetValuesFromColumn<Decimal>(table, "F23"), "Values F23");
        Assert.AreEqual(new Boolean[] { true, false, false, false }, DataTools.GetValuesFromColumn<Boolean>(table, "F3"), "Values F3");
        if (wrapper.SUT.DBStruct.IndexOf("F4") >= 0)
          Assert.AreEqual(new DateTime[] { new DateTime(2023, 3, 18), new DateTime(2023, 3, 19), new DateTime(2023, 3, 20), new DateTime() }, DataTools.GetValuesFromColumn<DateTime>(table, "F4"), "Values F4");
        if (wrapper.SUT.DBStruct.IndexOf("F5") >= 0)
          Assert.AreEqual(new String[] { "Длинный текст 1", "Длинный текст 2", null, null }, DataTools.GetValuesFromColumn<String>(table, "F5"), "Values F5");
        if (wrapper.SUT.DBStruct.IndexOf("F6") >= 0)
          Assert.AreEqual(new Double[] { 111.11, 222.22, 333.33, 0.0 }, DataTools.GetValuesFromColumn<Double>(table, "F6"), "Values F6");
      }
    }


    [Test]
    public void CreateTable_columns([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, false))
      {
        wrapper.SUT.AppendRecord(); // чтобы была пустая строка

        DataTable table = wrapper.SUT.CreateTable("F21,F3");
        Assert.AreEqual(new string[] { "F21", "F3" }, DataTools.GetColumnNames(table), "ColumnNames");
        Assert.AreEqual(typeof(Int32), table.Columns["F21"].DataType, "DataType F21");
        Assert.AreEqual(typeof(Boolean), table.Columns["F3"].DataType, "DataType F3");

        Assert.AreEqual(4, table.Rows.Count, "RowCount");
        Assert.AreEqual(new Int32[] { 111, 222, 333, 0 }, DataTools.GetValuesFromColumn<Int32>(table, "F21"), "Values F21");
        Assert.AreEqual(new Boolean[] { true, false, false, false }, DataTools.GetValuesFromColumn<Boolean>(table, "F3"), "Values F3");
      }
    }

    [Test]
    public void CreateTable_errorMessages([Values(false, true)]bool makeError)
    {
      byte[] bytes = (byte[])(DbfResource.ResourceManager.GetObject("dbase3_dbf"));
      MemoryStream fsDBF = new MemoryStream();
      fsDBF.Write(bytes, 0, bytes.Length); // копия
      if (makeError)
      {
        fsDBF.Position = 0x146; // Место, где записано значение "111" для поля F21 первой строки
        fsDBF.WriteByte(65); // Записали букву 'A'. Получилось "A11" - испортили число
      }
      fsDBF.Position = 0;

      using (DbfFile sut = new DbfFile(fsDBF, null, System.Text.Encoding.GetEncoding(1251), true))
      {
        ErrorMessageList errors = new ErrorMessageList();
        DataTable table = sut.CreateTable("F21", null, errors);
        Assert.AreEqual(makeError ? ErrorMessageKind.Error : ErrorMessageKind.Info, errors.Severity, "Severity");
        Assert.AreEqual(3, table.Rows.Count, "RowCount");
        Assert.AreEqual(new Int32[] { (makeError ? 0 : 111), 222, 333 },
          DataTools.GetValuesFromColumn<Int32>(table, "F21"), "Values F21");
      }
    }

    #endregion

    #region CreateBlockedTable()

    [Test]
    public void CreateBlockedTable([ValueSource("TestFormatCodes")]string formatCode)
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper(formatCode, true))
      {
        for (int i = 1; i <= 500; i++)
        {
          wrapper.SUT.AppendRecord(); // чтобы была пустая строка
          wrapper.SUT.SetInt("F21", i);
        }
        wrapper.Reload();
        Assert.AreEqual(500, wrapper.SUT.RecordCount);

        DataTable table;
        int blockCount = 0;
        ErrorMessageList errors = new ErrorMessageList();
        while (wrapper.SUT.CreateBlockedTable(90, errors, out table))
        {
          Assert.AreEqual(wrapper.SUT.DBStruct.GetNames(), DataTools.GetColumnNames(table), "ColumnNames, #" + (blockCount + 1).ToString());
          int firstRN = blockCount * 90 + 1;
          int lastRN = Math.Min(firstRN + 90 - 1, 500);
          Assert.AreEqual(lastRN - firstRN + 1, table.Rows.Count, "RowCount, #" + (blockCount + 1).ToString());

          int[] wantedValues21 = new int[lastRN - firstRN + 1];
          for (int j = 0; j < wantedValues21.Length; j++)
            wantedValues21[j] = firstRN + j;
          Assert.AreEqual(wantedValues21, DataTools.GetValuesFromColumn<Int32>(table, "F21"), "Values F21, #" + (blockCount + 1).ToString());

          blockCount++;
        }
        Assert.AreEqual(500 / 90 + 1, blockCount, "BlockCount");
        Assert.AreEqual(0, errors.Count, "ErrorCount");
      }
    }

    [Test]
    public void CreateBlockedTable_emptyTable()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3", true))
      {
        DataTable table;
        int blockCount = 0;
        ErrorMessageList errors = new ErrorMessageList();
        while (wrapper.SUT.CreateBlockedTable(90, errors, out table))
        {
          blockCount++;
        }
        Assert.AreEqual(0, blockCount, "BlockCount");
        Assert.AreEqual(0, errors.Count, "ErrorCount");
      }
    }

    [Test]
    public void CreateBlockedTable_singleBlock()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3", true))
      {
        for (int i = 1; i <= 100; i++)
        {
          wrapper.SUT.AppendRecord(); // чтобы была пустая строка
          wrapper.SUT.SetInt("F21", i);
        }
        wrapper.Reload();

        DataTable table;
        int blockCount = 0;
        ErrorMessageList errors = new ErrorMessageList();
        while (wrapper.SUT.CreateBlockedTable(100, errors, out table))
        {
          Assert.AreEqual(100, table.Rows.Count, "RowCount");
          blockCount++;
        }
        Assert.AreEqual(1, blockCount, "BlockCount");
        Assert.AreEqual(0, errors.Count, "ErrorCount");
      }
    }


    [Test]
    public void CreateBlockedTable_columnName()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3", true))
      {
        for (int i = 1; i <= 200; i++)
        {
          wrapper.SUT.AppendRecord(); // чтобы была пустая строка
          wrapper.SUT.SetInt("F21", i);
        }
        wrapper.Reload();

        DataTable table;
        int blockCount = 0;
        ErrorMessageList errors = new ErrorMessageList();
        while (wrapper.SUT.CreateBlockedTable(90, "F21,F22", errors, out table))
        {
          Assert.AreEqual(new string[] { "F21", "F22" }, DataTools.GetColumnNames(table), "ColumnNames, #" + (blockCount + 1).ToString());
          int firstRN = blockCount * 90 + 1;
          int lastRN = Math.Min(firstRN + 90 - 1, 200);
          Assert.AreEqual(lastRN - firstRN + 1, table.Rows.Count, "RowCount, #" + (blockCount + 1).ToString());

          int[] wantedValues21 = new int[lastRN - firstRN + 1];
          for (int j = 0; j < wantedValues21.Length; j++)
            wantedValues21[j] = firstRN + j;
          Assert.AreEqual(wantedValues21, DataTools.GetValuesFromColumn<Int32>(table, "F21"), "Values F21, #" + (blockCount + 1).ToString());
          blockCount++;
        }
        Assert.AreEqual(200 / 90 + 1, blockCount, "BlockCount");
        Assert.AreEqual(0, errors.Count, "ErrorCount");
      }
    }

    #endregion

    #region Encoding

    public struct EncodingTestData
    {
      public EncodingTestData(Encoding encoding, string text)
      {
        _Encoding = encoding;
        _Text = text;
      }

      public Encoding Encoding { get { return _Encoding; } }
      private Encoding _Encoding;

      public string Text { get { return _Text; } }
      private string _Text;

      public override string ToString()
      {
        return "CodePage=" + _Encoding.CodePage.ToString();
      }
    }

    public static EncodingTestData[] EncodingTestDataArray
    {
      get
      {
        List<EncodingTestData> lst = new List<EncodingTestData>();
        AddEncodingTestData(lst, 1252, "Hello");
        AddEncodingTestData(lst, 866, "Привет");
        AddEncodingTestData(lst, 1251, "Привет");
        return lst.ToArray();
      }
    }

    private static void AddEncodingTestData(List<EncodingTestData> lst, int codePage, string text)
    {
      System.Text.Encoding enc;
      try { enc = System.Text.Encoding.GetEncoding(codePage); }
      catch { return; }

      lst.Add(new EncodingTestData(enc, text));
    }

    [Test]
    public void Encoding([ValueSource("EncodingTestDataArray")]EncodingTestData testData)
    {
      DbfStruct dbs = new DbfStruct();
      dbs.AddString("F1", 100);
      dbs.AddMemo("F5");

      MemoryStream fsDBF = new MemoryStream();
      MemoryStream fsDBT = new MemoryStream();

      using (DbfFile sut1 = new DbfFile(fsDBF, fsDBT, dbs, testData.Encoding, DbfFileFormat.dBase3))
      {
        Assert.AreSame(dbs, sut1.DBStruct, "DBStruct #1");
        Assert.AreSame(testData.Encoding, sut1.Encoding, "Encoding #1");
        sut1.AppendRecord();
        sut1.SetString("F1", testData.Text);
        sut1.SetString("F5", testData.Text);
      }

      byte[] wantedBytes = testData.Encoding.GetBytes(testData.Text);
      byte[] buffer = new byte[wantedBytes.Length];

      fsDBF.Position = 32 + // заголовок
        32 * 2 + // описания полей F1 и F5
        1 + // 0x0D
        1;  // Маркер удаленной записи


      fsDBF.Read(buffer, 0, buffer.Length);
      Assert.AreEqual(wantedBytes, buffer, "F1 #2");

      fsDBT.Position = 512;
      fsDBT.Read(buffer, 0, buffer.Length);
      Assert.AreEqual(wantedBytes, buffer, "F5 #2");

      fsDBF.Position = 0;
      fsDBT.Position = 0;

      using (DbfFile sut3 = new DbfFile(fsDBF, fsDBT, testData.Encoding, true))
      {
        Assert.AreSame(testData.Encoding, sut3.Encoding, "Encoding #3");
        sut3.RecNo = 1;
        Assert.AreEqual(testData.Text, sut3.GetString("F1"), "F1 #3");
        Assert.AreEqual(testData.Text, sut3.GetString("F5"), "F5 #3");
      }
    }

    [Test]
    public void DefaultEncoding()
    {
      Assert.IsNotNull(DbfFile.DefaultEncoding); // больше нечего проверять
    }

    #endregion

    #region Прочие методы

    [Test]
    public void GetFieldOffset()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", true))
      {
        int wantedValue = 1;

        for (int i = 0; i < wrapper.SUT.DBStruct.Count; i++)
        {
          int realValue = wrapper.SUT.GetFieldOffset(i);
          Assert.AreEqual(realValue, wantedValue, wrapper.SUT.DBStruct[i].Name);

          wantedValue += wrapper.SUT.DBStruct[i].Length;
        }

        Assert.AreEqual(wrapper.SUT.DBStruct.RecordSize, wantedValue, "RecordSize");

        Assert.Catch(delegate() { wrapper.SUT.GetFieldOffset(-1); }, "(-1)");
        Assert.Catch(delegate() { wrapper.SUT.GetFieldOffset(wrapper.SUT.DBStruct.Count); }, "(Count)");
      }
    }

    [Test]
    public void GetLength()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", false))
      {
        wrapper.SUT.RecNo = 1;
        Assert.AreEqual("Значение 1".Length, wrapper.SUT.GetLength("F1"), "F1 #1");
        Assert.AreEqual(wrapper.SUT.DBStruct["F21"].Length, wrapper.SUT.GetLength("F21"), "F21 #1");
        Assert.AreEqual(wrapper.SUT.DBStruct["F22"].Length, wrapper.SUT.GetLength("F22"), "F22 #1");
        Assert.AreEqual(wrapper.SUT.DBStruct["F23"].Length, wrapper.SUT.GetLength("F23"), "F23 #1");
        Assert.AreEqual(1, wrapper.SUT.GetLength("F3"), "F3 #1");
        Assert.AreEqual(8, wrapper.SUT.GetLength("F4"), "F4 #1");
        Assert.AreEqual("Длинный текст 1".Length, wrapper.SUT.GetLength("F5"), "F5 #1");

        wrapper.SUT.AppendRecord();
        for (int i = 0; i < wrapper.SUT.DBStruct.Count; i++)
        {
          string fieldName = wrapper.SUT.DBStruct[i].Name;
          Assert.AreEqual(0, wrapper.SUT.GetLength(fieldName), fieldName + " #2");
        }
      }
    }

    [Test]
    public void GetMaxLengths()
    {
      using (DbfFileTestWrapper wrapper = new DbfFileTestWrapper("dbase3m", true))
      {
        DbfStruct dbs = wrapper.SUT.DBStruct;
        int[] wantedValue = new int[]{0, // "F1"
          dbs["F21"].Length, 
          dbs["F22"].Length, 
          dbs["F23"].Length, 
          /*dbs["F3"].Length*/ 1, 
          /*dbs["F4"].Length*/ 8, 
          0}; // "F5"
        Assert.AreEqual(wantedValue, wrapper.SUT.GetMaxLengths(), "#0");
        wrapper.SUT.AppendRecord();
        Assert.AreEqual(wantedValue, wrapper.SUT.GetMaxLengths(), "#1 empty");

        wrapper.SUT.SetString("F1", "AAA");
        wrapper.SUT.SetString("F5", "BBB");
        wantedValue[dbs.IndexOf("F1")] = 3;
        wantedValue[dbs.IndexOf("F5")] = 3;
        Assert.AreEqual(wantedValue, wrapper.SUT.GetMaxLengths(), "#1 filled");

        wrapper.SUT.AppendRecord(); // пустая строка
        Assert.AreEqual(wantedValue, wrapper.SUT.GetMaxLengths(), "#2");

        wrapper.SUT.AppendRecord();
        wrapper.SUT.SetString("F1", "CCCC");
        wrapper.SUT.SetString("F5", new string('X', 10000));
        wantedValue[dbs.IndexOf("F1")] = 4;
        wantedValue[dbs.IndexOf("F5")] = 10000;
        Assert.AreEqual(wantedValue, wrapper.SUT.GetMaxLengths(), "#3");
      }
    }

    #endregion

    #region IsReadOnly

    [Test]
    public void Constructor_OpenFile()
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath pathDbf = new AbsPath(dir.Dir, "dbase3.dbf");
        byte[] bDbf = (byte[])(DbfResource.ResourceManager.GetObject("dbase3_dbf"));
        File.WriteAllBytes(pathDbf.Path, bDbf);

        using (DbfFile sut = new DbfFile(pathDbf, System.Text.Encoding.GetEncoding(1251)))
        {
          Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");

          sut.RecNo = 1;
          Assert.Catch(delegate() { sut.SetValue("F1", "X"); }, "SetValue()");
          Assert.Catch(delegate() { sut.SetString("F1", "X", true); }, "SetString()");
          Assert.Catch(delegate() { sut.SetInt("F21", 1); }, "SetInt()");
          Assert.Catch(delegate() { sut.SetInt64("F22", 1); }, "SetInt64()");
          Assert.Catch(delegate() { sut.SetSingle("F23", 1f); }, "SetSingle()");
          Assert.Catch(delegate() { sut.SetDouble("F23", 1.0); }, "SetDouble()");
          Assert.Catch(delegate() { sut.SetDecimal("F23", 1m); }, "SetDecimal()");
          Assert.Catch(delegate() { sut.SetBool("F3", true); }, "SetBool()");
          Assert.Catch(delegate() { sut.SetNullableDate("F4", null); }, "SetNullableDate()");
          Assert.Catch(delegate() { sut.SetDate("F4", new DateTime(2023, 3, 23)); }, "SetDate()");
          Assert.Catch(delegate() { sut.SetNull("F11"); }, "SetNull()");
          Assert.Catch(delegate() { sut.RecordDeleted = false; }, "RecordDeleted");

          Assert.Catch(delegate() { sut.AppendRecord(); }, "AppendRecord()");

          DataTable srcTable = new DataTable();
          srcTable.Columns.Add("F21", typeof(Int32));
          Assert.Catch(delegate() { sut.Append(srcTable); }, "Append()");

          Assert.AreEqual(3, sut.RecordCount, "RecordCount");
        }
      }
    }


    #endregion

    #region Чтение/запись при отсутствии memo-файла

    [Test]
    public void DbtFileMissing_reading()
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath pathDbf = new AbsPath(dir.Dir, "dbase3m.dbf");
        byte[] bDbf = (byte[])(DbfResource.ResourceManager.GetObject("dbase3m_dbf"));
        File.WriteAllBytes(pathDbf.Path, bDbf);

        // Файл dbase3m.dbt отсутствует, хотя должен быть
        using (DbfFile sut = new DbfFile(pathDbf, System.Text.Encoding.GetEncoding(1251)))
        {
          Assert.IsTrue(sut.DBStruct.IndexOf("F5") >= 0, "F5 field presents");
          Assert.AreEqual('M', sut.DBStruct["F5"].Type, "F5 is a memo field");
          Assert.IsTrue(sut.MemoFileMissing, "MemoFileMissing");

          List<int> lst21 = new List<int>();
          while (sut.Read())
            lst21.Add(sut.GetInt("F21"));
          Assert.AreEqual(new int[] { 111, 222, 333 }, lst21.ToArray(), "Read()");

          sut.RecNo = 1;
          Assert.AreEqual("Значение 1", sut.GetValue("F1"), "F1 value");
          Assert.AreEqual(111, sut.GetValue("F21"), "F21 value");
          Assert.AreEqual(111L, sut.GetValue("F22"), "F22 value");
          Assert.AreEqual(111.11m, sut.GetValue("F23"), "F23 value");
          Assert.AreEqual(true, sut.GetValue("F3"), "F3 value");
          Assert.AreEqual(new DateTime(2023, 3, 18), sut.GetValue("F4"), "F4 value");
          Assert.Catch<DbfMemoFileMissingException>(delegate() { sut.GetValue("F5"); }, "F5 value");
        }
      }
    }

    [Test]
    public void DbtFileMissing_writing()
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath pathDbf = new AbsPath(dir.Dir, "dbase3m.dbf");
        byte[] bDbf = (byte[])(DbfResource.ResourceManager.GetObject("dbase3m_dbf"));
        File.WriteAllBytes(pathDbf.Path, bDbf);

        // Файл dbase3m.dbt отсутствует, хотя должен быть
        using (DbfFile sut = new DbfFile(pathDbf, System.Text.Encoding.GetEncoding(1251), false))
        {
          Assert.IsTrue(sut.MemoFileMissing, "MemoFileMissing #1");
          Assert.IsFalse(sut.IsReadOnly, "IsReadOnly #1");

          sut.RecNo = 1;
          Assert.DoesNotThrow(delegate() { sut.SetString("F1", "XXX"); }, "Set F1 value");
          Assert.Catch<DbfMemoFileMissingException>(delegate() { sut.SetString("F5", "XXX"); }, "Set F5 value");

          Assert.DoesNotThrow(delegate() { sut.Flush(); }, "Flush()");

          sut.RecNo = 2;
          Assert.DoesNotThrow(delegate() { sut.RecordDeleted = true; }, "Set RecordDeleted");

          Assert.DoesNotThrow(delegate() { sut.AppendRecord(); }, "AppendRecord()");
          Assert.DoesNotThrow(delegate() { sut.SetString("F1", "YYY"); }, "Set F1 value");
        }

        using (DbfFile sut = new DbfFile(pathDbf, System.Text.Encoding.GetEncoding(1251), true))
        {
          Assert.IsTrue(sut.MemoFileMissing, "MemoFileMissing #2");
          Assert.IsTrue(sut.IsReadOnly, "IsReadOnly #2");
          List<string> lst = new List<string>();
          while (sut.Read())
            lst.Add(sut.GetString("F1"));
          Assert.AreEqual(new string[] { "XXX", "Значение 3", "YYY" }, lst.ToArray(), "F1 values #2");
        }
      }
    }

    #endregion
  }
}
