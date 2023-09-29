using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using FreeLibSet.IO;
using FreeLibSet.Core;
using FreeLibSet.Remoting;
using FreeLibSet.Tests;

namespace ExtTools_tests.IO
{
  [TestFixture]
  public class FileContainerTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_AbsPath()
    {
      byte[] bytes = new byte[10];
      for (int i = 0; i < bytes.Length; i++)
        bytes[i] = (byte)i;

      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath path = new AbsPath(dir.Dir, "test.bin");
        System.IO.File.WriteAllBytes(path.Path, bytes);

        FileContainer sut = new FileContainer(path);

        Assert.AreEqual("test.bin", sut.FileInfo.Name, "Name");
        Assert.AreEqual(10L, sut.FileInfo.Length, "Length");
        Assert.AreEqual("", sut.SubDir, "SubDir");
        CollectionAssert.AreEqual(bytes, sut.Content, "Content");
      }
    }

    [Test]
    public void Constructor_AbsPath_SubDir()
    {
      byte[] bytes = new byte[10];
      for (int i = 0; i < bytes.Length; i++)
        bytes[i] = (byte)i;

      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath path = new AbsPath(dir.Dir, "test.bin");
        System.IO.File.WriteAllBytes(path.Path, bytes);

        string subDir = "aaa" + System.IO.Path.DirectorySeparatorChar + "bbb";
        FileContainer sut = new FileContainer(path, subDir);

        Assert.AreEqual("test.bin", sut.FileInfo.Name, "Name");
        Assert.AreEqual(10L, sut.FileInfo.Length, "Length");
        Assert.AreEqual(subDir + System.IO.Path.DirectorySeparatorChar, sut.SubDir, "SubDir");
        CollectionAssert.AreEqual(bytes, sut.Content, "Content");
      }
    }

    [Test]
    public void Constructor_SFI()
    {
      byte[] bytes = new byte[10];
      for (int i = 0; i < bytes.Length; i++)
        bytes[i] = (byte)i;

      StoredFileInfo sfi = new StoredFileInfo("test.bin", 10);

      FileContainer sut = new FileContainer(sfi, bytes);

      Assert.AreEqual("test.bin", sut.FileInfo.Name, "Name");
      Assert.AreEqual(10L, sut.FileInfo.Length, "Length");
      Assert.AreEqual("", sut.SubDir, "SubDir");
      CollectionAssert.AreEqual(bytes, sut.Content, "Content");
    }

    [Test]
    public void Constructor_SFI_SubDir()
    {
      byte[] bytes = new byte[10];
      for (int i = 0; i < bytes.Length; i++)
        bytes[i] = (byte)i;

      StoredFileInfo sfi = new StoredFileInfo("test.bin", 10);
      string subDir = "aaa";
      FileContainer sut = new FileContainer(sfi, bytes, subDir);

      Assert.AreEqual("test.bin", sut.FileInfo.Name, "Name");
      Assert.AreEqual(10L, sut.FileInfo.Length, "Length");
      Assert.AreEqual(subDir + System.IO.Path.DirectorySeparatorChar, sut.SubDir, "SubDir");
      CollectionAssert.AreEqual(bytes, sut.Content, "Content");
    }

    [Test]
    public void Constructor_SFI_contentLen_exception()
    {
      byte[] bytes = new byte[10];
      for (int i = 0; i < bytes.Length; i++)
        bytes[i] = (byte)i;

      StoredFileInfo sfi = new StoredFileInfo("test.bin", 11);

      Assert.Catch<ArgumentException>(delegate () { new FileContainer(sfi, bytes); });
    }

    [Test]
    public void Constructor_FileName()
    {
      byte[] bytes = new byte[10];
      for (int i = 0; i < bytes.Length; i++)
        bytes[i] = (byte)i;

      FileContainer sut = new FileContainer("test.bin", bytes);

      Assert.AreEqual("test.bin", sut.FileInfo.Name, "Name");
      Assert.AreEqual(10L, sut.FileInfo.Length, "Length");
      Assert.AreEqual("", sut.SubDir, "SubDir");
      CollectionAssert.AreEqual(bytes, sut.Content, "Content");
    }

    [Test]
    public void Constructor_FileName_SubDir()
    {
      byte[] bytes = new byte[10];
      for (int i = 0; i < bytes.Length; i++)
        bytes[i] = (byte)i;

      string subDir = "aaa";
      FileContainer sut = new FileContainer("test.bin", bytes, subDir);

      Assert.AreEqual("test.bin", sut.FileInfo.Name, "Name");
      Assert.AreEqual(10L, sut.FileInfo.Length, "Length");
      Assert.AreEqual(subDir + System.IO.Path.DirectorySeparatorChar, sut.SubDir, "SubDir");
      CollectionAssert.AreEqual(bytes, sut.Content, "Content");
    }

    [TestCase("/aaa")]
    public void Constructor_SubDir_exception(string subDir)
    {
      subDir = subDir.Replace('/', System.IO.Path.DirectorySeparatorChar);
      Assert.Catch<ArgumentException>(delegate () { new FileContainer("test.bin", DataTools.EmptyBytes, subDir); });
    }

    #endregion

    #region Save(), SaveAs

    [Test]
    public void Save()
    {
      FileContainer sut = CreateTestObject();
      using (TempDirectory dir = new TempDirectory())
      {
        sut.Save(dir.Dir);

        AbsPath resPath = new AbsPath(dir.Dir, "aaa", "bbb", sut.FileInfo.Name);
        Assert.IsTrue(System.IO.File.Exists(resPath.Path), "File exists");
        CollectionAssert.AreEqual(sut.Content, System.IO.File.ReadAllBytes(resPath.Path), "Content");
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
          DateTimeAssert.AreEqual(sut.FileInfo.CreationTime, System.IO.File.GetCreationTime(resPath.Path), FileTools.FileTimeMaxDelta, "CreationTime");
        DateTimeAssert.AreEqual(sut.FileInfo.LastWriteTime, System.IO.File.GetLastWriteTime(resPath.Path), FileTools.FileTimeMaxDelta, "LastWriteTime");
      }
    }

    [Test]
    public void SaveAs()
    {
      FileContainer sut = CreateTestObject();
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath resPath = new AbsPath(dir.Dir, "xxx.bin");
        sut.SaveAs(resPath);

        Assert.IsTrue(System.IO.File.Exists(resPath.Path), "File exists");
        CollectionAssert.AreEqual(sut.Content, System.IO.File.ReadAllBytes(resPath.Path), "Content");
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
          DateTimeAssert.AreEqual(sut.FileInfo.CreationTime, System.IO.File.GetCreationTime(resPath.Path), FileTools.FileTimeMaxDelta, "CreationTime");
        DateTimeAssert.AreEqual(sut.FileInfo.LastWriteTime, System.IO.File.GetLastWriteTime(resPath.Path), FileTools.FileTimeMaxDelta, "LastWriteTime");

        Assert.IsFalse(System.IO.Directory.Exists(dir.Dir.SlashedPath + "aaa"), "Dont create dir");
      }
    }

    private FileContainer CreateTestObject()
    {
      byte[] bytes = new byte[1234];
      for (int i = 0; i < bytes.Length; i++)
        bytes[i] = (byte)(i % 100);

      StoredFileInfo fi = new StoredFileInfo("test.bin",
        bytes.Length,
        new DateTime(2023, 1, 1, 8, 0, 0),
        new DateTime(2023, 6, 30, 12, 34, 0));

      FileContainer sut = new FileContainer(fi, bytes, "aaa" + System.IO.Path.DirectorySeparatorChar + "bbb");
      return sut;
    }

    #endregion

    #region Clone()

    [Test]
    public void Clone_simple()
    {
      FileContainer sut = CreateTestObject();

      FileContainer res = sut.Clone();

      Assert.AreNotSame(sut, res, "Not same");
      Assert.AreEqual(sut.FileInfo.Name, res.FileInfo.Name, "Name");
      Assert.AreEqual(sut.FileInfo.Length, res.FileInfo.Length, "Length");
      Assert.AreEqual(sut.FileInfo.CreationTime, res.FileInfo.CreationTime, "CreationTime"); // здесь точное сравнение, без дельты
      Assert.AreEqual(sut.FileInfo.LastWriteTime, res.FileInfo.LastWriteTime, "LastWriteTime");
      Assert.AreEqual(sut.SubDir, res.SubDir, "SubDir");
      CollectionAssert.AreEqual(sut.Content, res.Content, "Content");
    }

    [Test]
    public void Clone_subDir()
    {
      FileContainer sut = CreateTestObject();

      string newSubDir = "xxx"+Path.DirectorySeparatorChar;
      FileContainer res = sut.Clone(newSubDir);

      Assert.AreNotSame(sut, res, "Not same");
      Assert.AreEqual(sut.FileInfo.Name, res.FileInfo.Name, "Name");
      Assert.AreEqual(sut.FileInfo.Length, res.FileInfo.Length, "Length");
      Assert.AreEqual(sut.FileInfo.CreationTime, res.FileInfo.CreationTime, "CreationTime");
      Assert.AreEqual(sut.FileInfo.LastWriteTime, res.FileInfo.LastWriteTime, "LastWriteTime");
      Assert.AreEqual(newSubDir, res.SubDir, "SubDir");
      CollectionAssert.AreEqual(sut.Content, res.Content, "Content");
    }

    #endregion

    // Сравнение файлов пока не тестируем

    #region Сериализация

    [Test]
    public void Serialization()
    {
      FileContainer sut = CreateTestObject();
      byte[] b = SerializationTools.SerializeBinary(sut);

      FileContainer res = (FileContainer)(SerializationTools.DeserializeBinary(b));

      Assert.AreEqual(sut.FileInfo.Name, res.FileInfo.Name, "Name");
      Assert.AreEqual(sut.FileInfo.Length, res.FileInfo.Length, "Length");
      Assert.AreEqual(sut.FileInfo.CreationTime, res.FileInfo.CreationTime, "CreationTime");
      Assert.AreEqual(sut.FileInfo.LastWriteTime, res.FileInfo.LastWriteTime, "LastWriteTime");
      Assert.AreEqual(sut.SubDir, res.SubDir, "SubDir");
      CollectionAssert.AreEqual(sut.Content, res.Content, "Content");
    }

    #endregion

    #region Тестирование производного класса

    // Не тестируем свойства XXXInternal, т.к. реализация, вероятно, будет изменена.

    /// <summary>
    /// Эта версия контейнера генерирует все свойства динамически
    /// </summary>
    private class TestContainer : FileContainer
    {
      public TestContainer()
      {
        // Конструктор базового класса является защищенным
      }

      public override StoredFileInfo FileInfo
      {
        get
        {
          return new StoredFileInfo("test.bin", 10, new DateTime(2023, 1, 1), new DateTime(2023, 7, 1));
        }
      }

      public override string SubDir
      {
        get
        {
          return "aaa" + System.IO.Path.DirectorySeparatorChar + "bbb";
        }
      }

      public override byte[] Content
      {
        get
        {
          byte[] a = new byte[10];
          for (int i = 0; i < a.Length; i++)
            a[i] = (byte)i;
          return a;
        }
      }
    }

    [Test]
    public void Save_Subclass()
    {
      TestContainer sut = new TestContainer();
      using (TempDirectory dir = new TempDirectory())
      {
        sut.Save(dir.Dir);

        AbsPath resPath = new AbsPath(dir.Dir, "aaa", "bbb", "test.bin");
        Assert.IsTrue(System.IO.File.Exists(resPath.Path), "File exists");
        CollectionAssert.AreEqual(sut.Content, System.IO.File.ReadAllBytes(resPath.Path), "Content");
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
          DateTimeAssert.AreEqual(sut.FileInfo.CreationTime, System.IO.File.GetCreationTime(resPath.Path), FileTools.FileTimeMaxDelta, "CreationTime");
        DateTimeAssert.AreEqual(sut.FileInfo.LastWriteTime, System.IO.File.GetLastWriteTime(resPath.Path), FileTools.FileTimeMaxDelta, "LastWriteTime");
      }
    }

    [Test]
    public void Clone_Subclass()
    {
      TestContainer sut = new TestContainer();
      FileContainer res = sut.Clone("ddd");
      Assert.AreEqual(typeof(FileContainer), res.GetType(), "GetType()");
      Assert.AreEqual(sut.FileInfo.Name, res.FileInfo.Name, "Name");
      Assert.AreEqual(sut.FileInfo.Length, res.FileInfo.Length, "Length");
      Assert.AreEqual(sut.FileInfo.CreationTime, res.FileInfo.CreationTime, "CreationTime");
      Assert.AreEqual(sut.FileInfo.LastWriteTime, res.FileInfo.LastWriteTime, "LastWriteTime");
      Assert.AreEqual("ddd"+Path.DirectorySeparatorChar, res.SubDir, "SubDir");
      CollectionAssert.AreEqual(sut.Content, res.Content, "Content");
    }

    #endregion
  }
}
