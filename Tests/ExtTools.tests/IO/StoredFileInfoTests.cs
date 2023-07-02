using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using FreeLibSet.IO;
using FreeLibSet.Core;
using FreeLibSet.Remoting;

namespace ExtTools_tests.IO
{
  [TestFixture]
  public class StoredFileInfoTests
  {
    #region Конструкторы и свойства

    [Test]
    public void Constructor_4args()
    {
      StoredFileInfo sut = new StoredFileInfo("test.txt", 123L, new DateTime(2023, 1, 1, 12, 34, 56), new DateTime(2023, 6, 30, 12, 34, 56));
      Assert.AreEqual("test.txt", sut.Name, "Name");
      Assert.AreEqual(123L, sut.Length, "Length");
      Assert.AreEqual(new DateTime(2023, 1, 1, 12, 34, 56), sut.CreationTime, "CreationTime");
      Assert.AreEqual(new DateTime(2023, 6, 30, 12, 34, 56), sut.LastWriteTime, "LastWriteTime");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Test]
    public void Constructor_2args()
    {
      StoredFileInfo sut = new StoredFileInfo("test.txt", 123L);
      Assert.AreEqual("test.txt", sut.Name, "Name");
      Assert.AreEqual(123L, sut.Length, "Length");
      // Не проверяем на равенство текущему времени
      //Assert.IsNull(sut.CreationTime, "CreationTime");
      //Assert.IsNull(sut.LastWriteTime, "LastWriteTime");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Test]
    public void Constructor_AbsPath_exists()
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath path = new AbsPath(dir.Dir, "test.txt");
        System.IO.File.WriteAllBytes(path.Path, new byte[10]);

        StoredFileInfo sut = new StoredFileInfo(path);

        Assert.AreEqual("test.txt", sut.Name, "Name");
        Assert.AreEqual(10L, sut.Length, "Length");
        Assert.AreEqual(System.IO.File.GetCreationTime(path.Path), sut.CreationTime, "CreationTime");
        Assert.AreEqual(System.IO.File.GetCreationTime(path.Path), sut.LastWriteTime, "LastWriteTime");
        Assert.IsFalse(sut.IsEmpty, "IsEmpty");
      }
    }

    [Test]
    public void Constructor_AbsPath_noFile()
    {
      using (TempDirectory dir = new TempDirectory()) // не используется
      {
        AbsPath path = new AbsPath(dir.Dir, "test.txt");

        StoredFileInfo sut = new StoredFileInfo(path);

        Assert.AreEqual("", sut.Name, "Name");
        Assert.AreEqual(0L, sut.Length, "Length");
        Assert.IsNull(sut.CreationTime, "CreationTime");
        Assert.IsNull(sut.LastWriteTime, "LastWriteTime");
        Assert.IsTrue(sut.IsEmpty, "IsEmpty");
      }
    }

    [Test]
    public void Constructor_default()
    {
      StoredFileInfo sut = new StoredFileInfo();

      Assert.AreEqual("", sut.Name, "Name");
      Assert.AreEqual(0L, sut.Length, "Length");
      Assert.IsNull(sut.CreationTime, "CreationTime");
      Assert.IsNull(sut.LastWriteTime, "LastWriteTime");
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
    }

    #endregion

    #region ApplyToFile()

    [Test]
    public void ApplyToFile()
    {
      // Не устанавливаем время с точностью до секунды, т.к. некоторые файловые системы округляют
      StoredFileInfo sut = new StoredFileInfo("src.txt", 123L, new DateTime(2023, 1, 1, 12, 34, 0), new DateTime(2023, 6, 30, 11, 22, 0));
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath path = new AbsPath(dir.Dir, "res.txt");
        System.IO.File.WriteAllBytes(path.Path, new byte[10]);

        sut.ApplyToFile(path);

        FileInfo fi = new FileInfo(path.Path);
        Assert.AreEqual(10, fi.Length, "Length");
        Assert.AreEqual(new DateTime(2023, 1, 1, 12, 34, 0), fi.CreationTime, "CreationTime");
        Assert.AreEqual(new DateTime(2023, 6, 30, 11, 22, 0), fi.LastWriteTime, "LastWriteTime");
      }
    }
    #endregion

    #region Empty

    [Test]
    public void Empty()
    {
      StoredFileInfo sut = StoredFileInfo.Empty;
      Assert.AreEqual("", sut.Name, "Name");
      Assert.AreEqual(0L, sut.Length, "Length");
      Assert.IsNull(sut.CreationTime, "CreationTime");
      Assert.IsNull(sut.LastWriteTime, "LastWriteTime");
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      StoredFileInfo sut = new StoredFileInfo("test.txt", 123L, new DateTime(2023, 1, 1, 12, 34, 56), new DateTime(2023, 6, 30, 12, 34, 56));
      byte[] b = SerializationTools.SerializeBinary(sut);

      StoredFileInfo res = (StoredFileInfo)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual("test.txt", res.Name, "Name");
      Assert.AreEqual(123L, res.Length, "Length");
      Assert.AreEqual(new DateTime(2023, 1, 1, 12, 34, 56), res.CreationTime, "CreationTime");
      Assert.AreEqual(new DateTime(2023, 6, 30, 12, 34, 56), res.LastWriteTime, "LastWriteTime");
    }

    #endregion
  }
}
