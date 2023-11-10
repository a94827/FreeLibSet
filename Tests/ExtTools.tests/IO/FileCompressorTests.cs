using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.IO;
using FreeLibSet.Core;

namespace ExtTools_tests.IO
{
  [TestFixture]
  public class FileCompressorTests
  {
    #region Конструктор FileCompressor

    [Test]
    public void Constructor()
    {
      FileCompressor sut = new FileCompressor();
      Assert.IsTrue(sut.ArchiveFileName.IsEmpty, "ArchiveFileName");
      Assert.IsTrue(sut.FileDirectory.IsEmpty, "FileDirectory");
      Assert.AreEqual(0, sut.FileTemplates.Count, "FileTemplates");
      Assert.IsTrue(String.IsNullOrEmpty(sut.Password), "Password");
      Assert.IsTrue(sut.UseSplashPercent, "UseSplashPercent");
      Assert.AreEqual(0, sut.VolumeSize, "VolumeSize");
    }

    #endregion

    #region Проверка действий 

    public static readonly bool[] ForceCmdLineValues = GetForceCmdLineValues();

    private static bool[] GetForceCmdLineValues()
    {
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
          return new bool[] { false, true };
        default:
          return new bool[] { false };
      }
    }

    [Test]
    public void SimpleTest([Values(".7z", ".zip")]string arcExt,
      [ValueSource("ForceCmdLineValues")]bool forceCmdLine)
    {
      using (TestPlace place = new TestPlace())
      {
        place.CreateTextFile("f1.txt");
        place.CreateBinFile("f2.bin");

        SutSet sut = new SutSet(place, "test" + arcExt, forceCmdLine);
        sut.Process();

        place.CompareFiles("f1.txt", "f2.bin");
      }
    }

    [Test]
    public void FileTemplates([Values(".7z", ".zip")]string arcExt,
      [ValueSource("ForceCmdLineValues")]bool forceCmdLine)
    {
      using (TestPlace place = new TestPlace())
      {
        place.CreateTextFile("f1.txt");
        place.CreateBinFile("f2.bin");
        place.CreateTextFile("aaa/bbb/f3.txt");
        place.CreateBinFile("aaa/f4.bin");

        SutSet sut = new SutSet(place, "test" + arcExt, forceCmdLine);
        sut.Sut1.FileTemplates.Add("*.txt", true);
        sut.Sut1.FileTemplates.Add("*.bin", false);
        sut.Process();

        place.CompareFiles("f1.txt", "f2.bin", "aaa/bbb/f3.txt");
      }
    }

    [Test]
    public void TestArchive([Values(".7z")]string arcExt,
      [ValueSource("ForceCmdLineValues")]bool forceCmdLine)
    {
      using (TestPlace place = new TestPlace())
      {
        place.CreateBinFile("f1.bin");

        SutSet sut = new SutSet(place, "test" + arcExt, forceCmdLine);
        sut.Sut1.Compress();
        Assert.IsTrue(sut.Sut2.TestArchive(), "Valid archive");

        // Меняем 1 байт
        byte[] b = System.IO.File.ReadAllBytes(sut.Sut2.ArchiveFileName.Path);
        b[1000] = (byte)(~b[1000]);
        System.IO.File.WriteAllBytes(sut.Sut2.ArchiveFileName.Path, b);
        Assert.IsFalse(sut.Sut2.TestArchive(), "Bad archive");
      }
    }

    [Test]
    public void TestArchive_volume([Values(".7z", ".zip")]string arcExt,
      //[ValueSource("ForceCmdLineValues")]bool forceCmdLine,
      [Values(true)]bool forceCmdLine,
      [Values(0, 1, 2, 3)]int fileIndex)
    {
      using (TestPlace place = new TestPlace())
      {
        place.CreateBinFile("f1.bin", 10000);

        SutSet sut = new SutSet(place, "test" + arcExt, forceCmdLine);
        sut.Sut1.VolumeSize = 3000; // будет 4 архива
        sut.Sut1.Compress();
        Assert.IsTrue(sut.Sut2.TestArchive(), "Valid archive");

        string[] aFiles = System.IO.Directory.GetFiles(place.ArcDir.Path);
        Assert.AreEqual(4, aFiles.Length, "File count");
        // Меняем 1 байт
        byte[] b = System.IO.File.ReadAllBytes(aFiles[fileIndex]);
        b[1000] = (byte)(~b[1000]);
        System.IO.File.WriteAllBytes(aFiles[fileIndex], b);
        Assert.IsFalse(sut.Sut2.TestArchive(), "Bad archive");
      }
    }

    [Test]
    public void Password_ok([Values(".7z", ".zip")]string arcExt,
      [ValueSource("ForceCmdLineValues")]bool forceCmdLine)
    {
      using (TestPlace place = new TestPlace())
      {
        place.CreateBinFile("f1.bin");

        SutSet sut = new SutSet(place, "test" + arcExt, forceCmdLine);
        sut.Sut1.Password = "123";
        sut.Sut2.Password = "123";
        sut.Sut3.Password = "123";
        sut.Process();

        place.CompareFiles("f1.bin");
      }
    }

    public void Password_none([Values(".7z", ".zip")]string arcExt,
      [ValueSource("ForceCmdLineValues")]bool forceCmdLine)
    {
      using (TestPlace place = new TestPlace())
      {
        place.CreateBinFile("f1.bin");

        SutSet sut = new SutSet(place, "test" + arcExt, forceCmdLine);
        sut.Sut1.Password = "123";
        // Для Sut2 и Sut3 пароль не задаем
        Assert.DoesNotThrow(delegate () { sut.Sut1.Compress(); }, "Compress");
        Assert.IsFalse(sut.Sut2.TestArchive(), "TestArchive");
        Assert.Catch(delegate () { sut.Sut3.Decompress(); }, "Decompress");
      }
    }

    public void Password_wrong([Values(".7z", ".zip")]string arcExt,
      [ValueSource("ForceCmdLineValues")]bool forceCmdLine)
    {
      using (TestPlace place = new TestPlace())
      {
        place.CreateBinFile("f1.bin");

        SutSet sut = new SutSet(place, "test" + arcExt, forceCmdLine);
        sut.Sut1.Password = "123";
        sut.Sut2.Password = "456";
        sut.Sut3.Password = "456";
        Assert.DoesNotThrow(delegate () { sut.Sut1.Compress(); }, "Compress");
        Assert.IsFalse(sut.Sut2.TestArchive(), "TestArchive");
        Assert.Catch(delegate () { sut.Sut3.Decompress(); }, "Decompress");
      }
    }

    [Test]
    public void ArchiveType([Values(FileComressorArchiveType.SevenZip, FileComressorArchiveType.Zip)]
      FileComressorArchiveType archiveType,
      [ValueSource("ForceCmdLineValues")]bool forceCmdLine)
    {
      using (TestPlace place = new TestPlace())
      {
        place.CreateBinFile("f1.bin");

        SutSet sut = new SutSet(place, "test.abc", forceCmdLine);
        sut.Sut1.ArchiveType = archiveType;
        sut.Process();

        place.CompareFiles("f1.bin");

        byte[] bArc = System.IO.File.ReadAllBytes(sut.Sut1.ArchiveFileName.Path);
        TestFileSignature(archiveType, bArc);
      }
    }

    private static void TestFileSignature(FileComressorArchiveType wantedType, byte[] bArc)
    {
      FileComressorArchiveType realType;
      if (DataTools.ArrayStartsWith(bArc, new byte[] { 0x37, 0x7a })) // "7z"
        realType = FileComressorArchiveType.SevenZip;
      else if (DataTools.ArrayStartsWith(bArc, new byte[] { 0x50, 0x4b })) // "PK"
        realType = FileComressorArchiveType.Zip;
      else
        realType = FileComressorArchiveType.Unknown;

      Assert.AreEqual(wantedType, realType, "Real ArchiveType");
    }

    //[Ignore("Decompression error because file is busy (when testing archive?)")]
    [Test]
    public void VolumeSize([Values(".7z")]string arcExt,// для Zip не поддеживается
      [Values(true)]bool forceCmdLine) // только для командной строки
    {
      using (TestPlace place = new TestPlace())
      {
        place.CreateBinFile("f1.bin", 100000);

        SutSet sut = new SutSet(place, "test" + arcExt, forceCmdLine);
        sut.Sut1.VolumeSize = 15000;
        sut.Process();

        place.CompareFiles("f1.bin");

        string[] aArcFiles = System.IO.Directory.GetFiles(place.ArcDir.Path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
        Assert.AreEqual(100000 / 15000 + 1, aArcFiles.Length, "Volume count");
      }
    }

    #endregion

    #region Вспомогательные классы

    /// <summary>
    /// Создает 3 каталога: для размещения исходных файлов, архива и распаковки файлов
    /// </summary>
    private class TestPlace : IDisposable
    {
      #region Конструктор и Dispose

      public TestPlace()
      {
        _TempDir = new TempDirectory();
        FileTools.ForceDirs(SrcDir);
        FileTools.ForceDirs(ArcDir);
        FileTools.ForceDirs(OutDir);
      }

      public void Dispose()
      {
        _TempDir.Dispose();
      }

      #endregion

      #region Каталоги

      private TempDirectory _TempDir;

      public AbsPath SrcDir { get { return new AbsPath(_TempDir.Dir, "SrcDir"); } }
      public AbsPath ArcDir { get { return new AbsPath(_TempDir.Dir, "ArcDir"); } }
      public AbsPath OutDir { get { return new AbsPath(_TempDir.Dir, "OutDir"); } }

      #endregion

      #region Методы создания файлов

      public void CreateTextFile(string fileName)
      {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < 1000; i++)
          sb.Append("Hello! ");
        System.IO.File.WriteAllText(GetFileName(fileName, true).Path, sb.ToString());
      }
      public void CreateBinFile(string fileName)
      {
        Random rnd = new Random();
        byte[] b = new byte[rnd.Next(10000, 20000)];
        rnd.NextBytes(b);
        System.IO.File.WriteAllBytes(GetFileName(fileName, true).Path, b);
      }
      public void CreateBinFile(string fileName, int len)
      {
        Random rnd = new Random();
        byte[] b = new byte[len];
        rnd.NextBytes(b);
        System.IO.File.WriteAllBytes(GetFileName(fileName, true).Path, b);
      }

      private AbsPath GetFileName(string fileName, bool isSrc)
      {
        if (String.IsNullOrEmpty(fileName))
          throw new ArgumentNullException();

        string[] a = fileName.Split('/');
        AbsPath res = new AbsPath(isSrc ? SrcDir : OutDir, a);
        if (isSrc)
          FileTools.ForceDirs(res.ParentDir);
        return res;
      }

      #endregion

      #region Сравнение файлов

      public void CompareFiles(params string[] fileNames)
      {
        string[] aFiles = System.IO.Directory.GetFiles(OutDir.Path, "*.*", System.IO.SearchOption.AllDirectories);
        int n = OutDir.SlashedPath.Length;
        for (int i = 0; i < aFiles.Length; i++)
          aFiles[i] = aFiles[i].Substring(n).Replace(System.IO.Path.DirectorySeparatorChar, '/');

        CollectionAssert.AreEquivalent(fileNames, aFiles, "OutDir files");

        for (int i = 0; i < aFiles.Length; i++)
        {
          AbsPath srcPath = GetFileName(fileNames[i], true);
          AbsPath outPath = GetFileName(fileNames[i], false);
          byte[] srcBytes = System.IO.File.ReadAllBytes(srcPath.Path);
          byte[] outBytes = System.IO.File.ReadAllBytes(outPath.Path);
          CollectionAssert.AreEqual(srcBytes, outBytes, "File content: " + fileNames[i]);
        }
      }

      #endregion
    }

    /// <summary>
    /// Набор из трех объектов FileCompressor для тестирования: Compress(), TestArchive() и Decompress()
    /// </summary>
    private class SutSet
    {
      #region Конструктор

      public SutSet(TestPlace place, string archiveName, bool forceCmdLine)
      {
        Sut1 = new FileCompressor();
        Sut1.ArchiveFileName = new AbsPath(place.ArcDir, archiveName);
        Sut1.FileDirectory = place.SrcDir;

        Sut2 = new FileCompressor();
        Sut2.ArchiveFileName = new AbsPath(place.ArcDir, archiveName);

        Sut3 = new FileCompressor();
        Sut3.ArchiveFileName = new AbsPath(place.ArcDir, archiveName);
        Sut3.FileDirectory = place.OutDir;

        Sut1.DebugForceCommandLine = forceCmdLine;
        Sut2.DebugForceCommandLine = forceCmdLine;
        Sut3.DebugForceCommandLine = forceCmdLine;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Для тестирования Compress()
      /// </summary>
      public readonly FileCompressor Sut1;
      /// <summary>
      /// Для тестирования TestArchive()
      /// </summary>
      public readonly FileCompressor Sut2;
      /// <summary>
      /// Для тестирования DeCompress()
      /// </summary>
      public readonly FileCompressor Sut3;

      #endregion

      #region Выполнение операций

      public void Process()
      {
        Assert.DoesNotThrow(delegate () { Sut1.Compress(); }, "Compress");
        Assert.IsTrue(Sut2.TestArchive(), "TestArchive");
        Assert.DoesNotThrow(delegate () { Sut3.Decompress(); }, "Decompress");
      }

      #endregion
    }

    #endregion
  }
}
