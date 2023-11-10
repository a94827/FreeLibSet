using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.IO;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace ExtTools_tests.IO
{
  [TestFixture]
  public class SharedTempDirectoryTests
  {
    #region Конструктор и Dispose()

    [Test]
    public void Constructor_1arg()
    {
      using (TempDirectory tempDir = new TempDirectory())
      {
        AbsPath path = new AbsPath(tempDir.Dir, "AAA", "BBB");
        using (SharedTempDirectory sut = new SharedTempDirectory(path))
        {
          Assert.AreEqual(path, sut.Dir, "Dir");
        }
      }
    }

    [Test]
    public void Constructor_0args()
    {
      AbsPath testFilePath;
      using (SharedTempDirectory sut = new SharedTempDirectory())
      {
        Assert.IsFalse(sut.Dir.IsEmpty, "Dir");

        testFilePath = new AbsPath(sut.Dir, "SD", "1.txt");
        FileTools.ForceDirs(testFilePath.ParentDir);
        System.IO.File.WriteAllBytes(testFilePath.Path, DataTools.EmptyBytes);
        Assert.IsTrue(System.IO.File.Exists(testFilePath.Path), "Before dispose");
      }
      Assert.IsFalse(System.IO.File.Exists(testFilePath.Path), "After dispose");
    }

    #endregion

    #region Clear()


    [Test]
    public void Clear()
    {
      using (SharedTempDirectory sut = new SharedTempDirectory())
      {
        AbsPath testPath1 = new AbsPath(sut.Dir, "SD", "1.txt");
        AbsPath testPath2 = new AbsPath(sut.Dir, "SD", "2.txt");
        FileTools.ForceDirs(testPath1.ParentDir);
        System.IO.File.WriteAllBytes(testPath1.Path, DataTools.EmptyBytes);
        System.IO.File.WriteAllBytes(testPath2.Path, DataTools.EmptyBytes);
        System.IO.FileStream fs2 = new System.IO.FileStream(testPath2.Path, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite);
        Assert.IsTrue(System.IO.File.Exists(testPath1.Path), "#1 Before");
        Assert.IsTrue(System.IO.File.Exists(testPath2.Path), "#2 Before");

        sut.Clear();
        Assert.IsFalse(System.IO.File.Exists(testPath1.Path), "#1 After");
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) // под Linux очистка будет полной
          Assert.IsTrue(System.IO.File.Exists(testPath2.Path), "#2 After");

        fs2.Close();

        sut.Clear();
        Assert.IsFalse(System.IO.File.Exists(testPath2.Path), "#2 After the second");
      }
    }

    #endregion

    #region Получение имен файлов

    [Test]
    public void GetTempFileName()
    {
      using (SharedTempDirectory sut = new SharedTempDirectory())
      {
        SingleScopeStringList filenames = new SingleScopeStringList(true);

        for (int i = 1; i <= 1000; i++)
        {
          AbsPath path = sut.GetTempFileName("txt");

          Assert.IsTrue(System.IO.Directory.Exists(sut.Dir.Path), "Directory exists");
          Assert.IsTrue(path.StartsWith(sut.Dir), "path in the dir");
          Assert.AreEqual(sut.Dir, path.ParentDir, "ParentDir");
          Assert.AreEqual(".txt", path.Extension, "Extension");
          Assert.IsFalse(filenames.Contains(path.FileName), "Unique");
          filenames.Add(path.FileName);
        }
      }
    }


    [Test]
    public void GetFixedTempFileName()
    {
      using (SharedTempDirectory sut = new SharedTempDirectory())
      {
        SingleScopeStringList filenames = new SingleScopeStringList(true);

        for (int i = 1; i <= 1000; i++)
        {
          AbsPath path = sut.GetFixedTempFileName("123.txt");

          Assert.IsTrue(System.IO.Directory.Exists(sut.Dir.Path), "Directory exists");
          Assert.IsTrue(path.StartsWith(sut.Dir), "path in the dir");
          Assert.IsTrue(System.IO.Directory.Exists(path.ParentDir.Path), "ParentDir exists");
          Assert.AreEqual("123.txt", path.FileName, "FileName");
          Assert.IsFalse(filenames.Contains(path.Path), "Unique");
          filenames.Add(path.Path);
        }
      }
    }

    #endregion
  }
}
