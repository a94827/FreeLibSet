using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.IO;
using NUnit.Framework;
using FreeLibSet.Core;
using System.Xml;

namespace ExtTools_tests.IO
{
  // TODO: Не все

  [TestFixture]
  public class FileToolsTests
  {
    #region Имена файлов

    [Test]
    public void CaseSensitive()
    {
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
          Assert.IsFalse(FileTools.CaseSensitive);
          break;
        case PlatformID.Unix:
          Assert.IsTrue(FileTools.CaseSensitive);
          break;
      }
    }

    [TestCase("f0001.txt", 4, "f0005.txt")]
    [TestCase("1.txt", 123, "124.txt")]
    [TestCase(@"c:\temp\01.txt", 7, @"c:\temp\08.txt")]
    [TestCase(@"c:\temp\f01.txt", 999999998, @"c:\temp\f999999999.txt")]
    [TestCase("f0001.txt", 0, "f0001.txt")]
    [TestCase("123.txt", 0, "123.txt")]
    [TestCase("01234", 1, "01235")]
    public void GetSerialFileName(string fileName, int fileIndex, string wantedRes)
    {
      string res = FileTools.GetSerialFileName(fileName, fileIndex);
      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void GetSerialFileNames()
    {
      string[] res = FileTools.GetSerialFileNames("f0129.txt", 3);
      CollectionAssert.AreEqual(new string[] { "f0129.txt", "f0130.txt", "f0131.txt" }, res);
    }

    #endregion

    #region Маски ? и * (Wildcards)

    [TestCase("a123.txt", "a*.txt", true)]
    [TestCase("a123.txt", "b*.txt", false)]
    [TestCase("a123.txt", "xxx/a*.txt", false)]
    [TestCase("a123.txt", "a*.txt/xxx", false)]
    [TestCase("bbb.xxx/a123.txt", "b?b.x?x/a*.txt", true)]
    [TestCase("bbb.xxx/a123.txt", "b?b.x?x/b*.txt", false)]
    [TestCase("aaa/bbb/ccc/ddd.txt", "a*/bbb/cc?/d*.*", true)]
    [TestCase("aaa/bbb/ddd.txt", "a*/bbb/cc?/d*.*", false)]
    public void TestRelFileNameWildcards(string fileName, string template, bool wantedRes)
    {
      fileName = fileName.Replace('/', System.IO.Path.DirectorySeparatorChar);
      template = template.Replace('/', System.IO.Path.DirectorySeparatorChar);

      bool res = FileTools.TestRelFileNameWildcards(fileName, template);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("123.txt", "*.txt", true)]
    [TestCase("123.txt", "*.xls", false)]
    [TestCase("123.txt", "123.*", true)]
    [TestCase("123.txt", "124.*", false)]
    [TestCase("123.txt", "*.*", true)]
    [TestCase("123.txt", "*3.txt", true)]
    [TestCase("123.txt", "*4.txt", false)]

    [TestCase("123.txt", "???.txt", true)]
    [TestCase("123.txt", "1??.txt", true)]
    [TestCase("123.txt", "1?3.txt", true)]
    [TestCase("123.txt", "1?4.txt", false)]
    [TestCase("123.txt", "1??3.txt", false)]
    [TestCase("123.txt", "??3.txt", true)]
    [TestCase("123.txt", "???3.txt", false)]
    [TestCase("123.txt", "?2?.txt", true)]
    [TestCase("123.txt", "?3?.txt", false)]
    [TestCase("123.txt", "?2*.txt", true)]
    [TestCase("123.txt", "?1*.txt", false)]
    [TestCase("123.txt", "*2?.txt", true)]
    [TestCase("123.txt", "*2.txt", false)]
    [TestCase("123.txt", "*3?.txt", false)]
    [TestCase("123.txt", "123.txt", true)]
    [TestCase("123.txt", "124.txt", false)]
    public void TestFileNameWildcards(string fileName, string template, bool wantedRes)
    {
      bool res = FileTools.TestFileNameWildcards(fileName, template);
      Assert.AreEqual(wantedRes, res);
    }

    [Platform("Win")]
    [TestCase("abc.txt", "aBc.txt", true)]
    [TestCase("abc.txt", "aBd.txt", false)]
    public void TestFileNameWildcards_ignoreCase(string fileName, string template, bool wantedRes)
    {
      bool res = FileTools.TestFileNameWildcards(fileName, template);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region Тестирование имен

    [Platform("Win")]
    [TestCase("c:/", true)]
    [TestCase("c:/123/", true)]
    [TestCase("c:/123", false)]
    [TestCase("//192.168.0.1/xxx/", true)]
    [TestCase("", false)]
    public void TestDirSlashedPath_2args(string dirName, bool wantedRes)
    {
      dirName = dirName.Replace('/', System.IO.Path.DirectorySeparatorChar);
      string errorText;
      bool res = FileTools.TestDirSlashedPath(dirName, out errorText);
      Assert.AreEqual(wantedRes, res, "Result");
      Assert.AreEqual(wantedRes, String.IsNullOrEmpty(errorText), "ErrorText");
    }

    [Test]
    public void TestDirSlashedPath_3args()
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath dirName1 = new AbsPath(dir.Dir, "1");
        System.IO.Directory.CreateDirectory(dirName1.Path);
        AbsPath dirName2 = new AbsPath(dir.Dir, "2");
        AbsPath dirName3 = new AbsPath(dir.Dir, "2");
        System.IO.File.WriteAllBytes(dirName3.Path, DataTools.EmptyBytes); // файл вместо каталога
        string errorText;
        Assert.IsTrue(FileTools.TestDirSlashedPath(dirName1.SlashedPath, TestPathMode.DirectoryExists, out errorText), "DirectoryExists 1");
        Assert.IsFalse(FileTools.TestDirSlashedPath(dirName2.SlashedPath, TestPathMode.DirectoryExists, out errorText), "DirectoryExists 2");
        Assert.IsFalse(FileTools.TestDirSlashedPath(dirName3.SlashedPath, TestPathMode.DirectoryExists, out errorText), "DirectoryExists 3");
        Assert.IsTrue(FileTools.TestDirSlashedPath(dirName1.SlashedPath, TestPathMode.RootExists, out errorText), "RootExists 1");
        Assert.IsTrue(FileTools.TestDirSlashedPath(dirName2.SlashedPath, TestPathMode.RootExists, out errorText), "RootExists 2");
        Assert.IsTrue(FileTools.TestDirSlashedPath(dirName3.SlashedPath, TestPathMode.RootExists, out errorText), "RootExists 3");
        Assert.IsTrue(FileTools.TestDirSlashedPath(dirName1.SlashedPath, TestPathMode.FormatOnly, out errorText), "FormatOnly 1");
        Assert.IsTrue(FileTools.TestDirSlashedPath(dirName2.SlashedPath, TestPathMode.FormatOnly, out errorText), "FormatOnly 2");
        Assert.IsTrue(FileTools.TestDirSlashedPath(dirName3.SlashedPath, TestPathMode.FormatOnly, out errorText), "FormatOnly 3");
        Assert.Catch<ArgumentException>(delegate () { FileTools.TestDirSlashedPath(dirName1.SlashedPath, TestPathMode.FileExists, out errorText); }, "#7");
      }
    }


    [Test]
    public void TestDirSlashedPath_None()
    {
      string errorText;
      Assert.IsTrue(FileTools.TestDirSlashedPath(@":/\666", TestPathMode.None, out errorText));
    }

    [Platform("Win")]
    [TestCase("c:/", false)]
    [TestCase("c:/123.txt", true)]
    [TestCase("c:/123", true)]
    [TestCase("c:/123/", false)]
    [TestCase("//192.168.0.1/xxx/1.txt", true)]
    [TestCase("//192.168.0.1/xxx/", false)]
    [TestCase("", false)]
    public void TestFilePath_2args(string fileName, bool wantedRes)
    {
      fileName = fileName.Replace('/', System.IO.Path.DirectorySeparatorChar);
      string errorText;
      bool res = FileTools.TestFilePath(fileName, out errorText);
      Assert.AreEqual(wantedRes, res, "Result");
      Assert.AreEqual(wantedRes, String.IsNullOrEmpty(errorText), "ErrorText");
    }

    [Test]
    public void TestFilePath_3args()
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath fileName1 = new AbsPath(dir.Dir, "1", "1.txt"); // файл существует
        AbsPath fileName2 = new AbsPath(dir.Dir, "1", "2.txt"); // файл не существует
        AbsPath fileName3 = new AbsPath(dir.Dir, "2", "1.txt"); // Каталог не существует
        System.IO.Directory.CreateDirectory(fileName1.ParentDir.Path);
        System.IO.File.WriteAllBytes(fileName1.Path, DataTools.EmptyBytes);
        AbsPath fileName4 = new AbsPath(dir.Dir, "1", "4.txt"); // Каталог вместо файла
        System.IO.Directory.CreateDirectory(fileName4.Path);

        string errorText;
        Assert.IsTrue(FileTools.TestFilePath(fileName1.Path, TestPathMode.FileExists, out errorText), "FileExists 1");
        Assert.IsFalse(FileTools.TestFilePath(fileName2.Path, TestPathMode.FileExists, out errorText), "FileExists 2");
        Assert.IsFalse(FileTools.TestFilePath(fileName3.Path, TestPathMode.FileExists, out errorText), "FileExists 3");
        Assert.IsFalse(FileTools.TestFilePath(fileName4.Path, TestPathMode.FileExists, out errorText), "FileExists 4");
        Assert.IsTrue(FileTools.TestFilePath(fileName1.Path, TestPathMode.DirectoryExists, out errorText), "DirectoryExists 1");
        Assert.IsTrue(FileTools.TestFilePath(fileName2.Path, TestPathMode.DirectoryExists, out errorText), "DirectoryExists 2");
        Assert.IsFalse(FileTools.TestFilePath(fileName3.Path, TestPathMode.DirectoryExists, out errorText), "DirectoryExists 3");
        Assert.IsTrue(FileTools.TestFilePath(fileName4.Path, TestPathMode.DirectoryExists, out errorText), "DirectoryExists 4");
        Assert.IsTrue(FileTools.TestFilePath(fileName1.Path, TestPathMode.RootExists, out errorText), "RootExists 1");
        Assert.IsTrue(FileTools.TestFilePath(fileName2.Path, TestPathMode.RootExists, out errorText), "RootExists 2");
        Assert.IsTrue(FileTools.TestFilePath(fileName3.Path, TestPathMode.RootExists, out errorText), "RootExists 3");
        Assert.IsTrue(FileTools.TestFilePath(fileName4.Path, TestPathMode.RootExists, out errorText), "RootExists 4");
        Assert.IsTrue(FileTools.TestFilePath(fileName1.Path, TestPathMode.FormatOnly, out errorText), "FormatOnly 1");
        Assert.IsTrue(FileTools.TestFilePath(fileName2.Path, TestPathMode.FormatOnly, out errorText), "FormatOnly 2");
        Assert.IsTrue(FileTools.TestFilePath(fileName3.Path, TestPathMode.FormatOnly, out errorText), "FormatOnly 3");
        Assert.IsTrue(FileTools.TestFilePath(fileName4.Path, TestPathMode.FormatOnly, out errorText), "FormatOnly 4");
      }
    }

    [Test]
    public void TestFilePath_None()
    {
      string errorText;
      Assert.IsTrue(FileTools.TestFilePath(@":/\666", TestPathMode.None, out errorText));
    }

    #endregion

    #region CheckDirectoryExitsts(), CheckFileExists()

    [Test]
    public void CheckDirectoryExitsts()
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath dirName1 = new AbsPath(dir.Dir, "1");
        AbsPath dirName2 = new AbsPath(dir.Dir, "2");
        AbsPath dirName3 = new AbsPath(dir.Dir, "3");
        System.IO.Directory.CreateDirectory(dirName1.Path);
        System.IO.File.WriteAllBytes(dirName3.Path, DataTools.EmptyBytes); // файл вместо каталога

        Assert.DoesNotThrow(delegate () { FileTools.CheckDirectoryExists(dirName1); }, "#1");
        Assert.Catch<System.IO.DirectoryNotFoundException>(delegate () { FileTools.CheckDirectoryExists(dirName2); }, "#2");
        Assert.Catch<System.IO.DirectoryNotFoundException>(delegate () { FileTools.CheckDirectoryExists(dirName3); }, "#3");
      }
    }

    [Test]
    public void CheckFileExitsts()
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath fileName1 = new AbsPath(dir.Dir, "1", "1.txt");
        AbsPath fileName2 = new AbsPath(dir.Dir, "1", "2.txt");
        AbsPath fileName3 = new AbsPath(dir.Dir, "2", "1.txt");
        AbsPath fileName4 = new AbsPath(dir.Dir, "1", "4.txt"); // каталог вместо файла
        System.IO.Directory.CreateDirectory(fileName1.ParentDir.Path);
        System.IO.File.WriteAllBytes(fileName1.Path, DataTools.EmptyBytes);
        System.IO.Directory.CreateDirectory(fileName4.Path);

        Assert.DoesNotThrow(delegate () { FileTools.CheckFileExists(fileName1); }, "#1");
        Assert.Catch<System.IO.FileNotFoundException>(delegate () { FileTools.CheckFileExists(fileName2); }, "#2");
        Assert.Catch<System.IO.DirectoryNotFoundException>(delegate () { FileTools.CheckFileExists(fileName3); }, "#3");
        Assert.Catch<System.IO.FileNotFoundException>(delegate () { FileTools.CheckFileExists(fileName4); }, "#4");
      }
    }

    #endregion

    #region IsAnyFileExist()

    [TestCase("aaa*", true, Description ="File in the directory")]
    [TestCase("bbb*", false, Description = "Directory")]
    [TestCase("ccc*", false, Description ="File in sub dir")]
    public void IsAnyFileExist(string template, bool wantedRes)
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath path1 = new AbsPath(dir.Dir, "aaa");
        System.IO.File.WriteAllBytes(path1.Path, DataTools.EmptyBytes);
        AbsPath path2 = new AbsPath(dir.Dir, "bbb");
        FileTools.ForceDirs(path2);
        AbsPath path3 = new AbsPath(dir.Dir, "SD", "ccc");
        FileTools.ForceDirs(path3.ParentDir);
        System.IO.File.WriteAllBytes(path3.Path, DataTools.EmptyBytes);

        bool res = FileTools.IsAnyFileExist(dir.Dir, template);
        Assert.AreEqual(wantedRes, res);
      }
    }

    [Test]
    public void IsAnyFileExist_noParentDir()
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath testDir = new AbsPath(dir.Dir, "XXX");
        Assert.IsFalse(FileTools.IsAnyFileExist(testDir, "*.*"));
      }
    }

    [Test]
    public void IsAnyFileExist_Empty()
    {
      Assert.IsFalse(FileTools.IsAnyFileExist(AbsPath.Empty, "*.*"));
    }

    #endregion

    #region DeleteFile()

    [Test]
    public void DeleteFile()
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath fileName1 = new AbsPath(dir.Dir, "1.txt");
        AbsPath fileName2 = new AbsPath(dir.Dir, "2.txt");
        System.IO.File.WriteAllBytes(fileName1.Path, DataTools.EmptyBytes);
        System.IO.File.SetAttributes(fileName1.Path, System.IO.FileAttributes.ReadOnly);
        Assert.IsTrue(System.IO.File.Exists(fileName1.Path), "#1 before");

        FileTools.DeleteFile(fileName1);
        Assert.IsFalse(System.IO.File.Exists(fileName1.Path), "#1 after");

        Assert.DoesNotThrow(delegate () { FileTools.DeleteFile(fileName2); }, "#2");
      }
    }

    [Test]
    public void DeleteFiles_3args([Values(true, false)]bool recurse)
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath fileName1 = new AbsPath(dir.Dir, "1.txt");
        AbsPath fileName2 = new AbsPath(dir.Dir, "1", "2.txt");
        AbsPath fileName3 = new AbsPath(dir.Dir, "1.doc");
        System.IO.File.WriteAllBytes(fileName1.Path, DataTools.EmptyBytes);
        System.IO.File.SetAttributes(fileName1.Path, System.IO.FileAttributes.ReadOnly);
        System.IO.Directory.CreateDirectory(fileName2.ParentDir.Path);
        System.IO.File.WriteAllBytes(fileName2.Path, DataTools.EmptyBytes);
        System.IO.File.SetAttributes(fileName2.Path, System.IO.FileAttributes.ReadOnly);
        System.IO.File.WriteAllBytes(fileName3.Path, DataTools.EmptyBytes);
        System.IO.File.SetAttributes(fileName3.Path, System.IO.FileAttributes.ReadOnly);
        Assert.IsTrue(System.IO.Directory.Exists(fileName2.ParentDir.Path), "Dir before");
        Assert.IsTrue(System.IO.File.Exists(fileName1.Path), "#1 before");
        Assert.IsTrue(System.IO.File.Exists(fileName2.Path), "#2 before");
        Assert.IsTrue(System.IO.File.Exists(fileName3.Path), "#3 before");

        FileTools.DeleteFiles(dir.Dir, "*.txt", recurse ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);

        Assert.IsTrue(System.IO.Directory.Exists(fileName2.ParentDir.Path), "Dir after");
        Assert.IsFalse(System.IO.File.Exists(fileName1.Path), "#1 after");
        Assert.AreEqual(!recurse, System.IO.File.Exists(fileName2.Path), "#2 after");
        Assert.IsTrue(System.IO.File.Exists(fileName3.Path), "#3 after");
      }
    }

    [Test]
    public void DeleteFiles_2args()
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath fileName1 = new AbsPath(dir.Dir, "1.txt");
        AbsPath fileName2 = new AbsPath(dir.Dir, "1", "2.txt");
        AbsPath fileName3 = new AbsPath(dir.Dir, "1.doc");
        System.IO.File.WriteAllBytes(fileName1.Path, DataTools.EmptyBytes);
        System.IO.File.SetAttributes(fileName1.Path, System.IO.FileAttributes.ReadOnly);
        System.IO.Directory.CreateDirectory(fileName2.ParentDir.Path);
        System.IO.File.WriteAllBytes(fileName2.Path, DataTools.EmptyBytes);
        System.IO.File.SetAttributes(fileName2.Path, System.IO.FileAttributes.ReadOnly);
        System.IO.File.WriteAllBytes(fileName3.Path, DataTools.EmptyBytes);
        System.IO.File.SetAttributes(fileName3.Path, System.IO.FileAttributes.ReadOnly);
        Assert.IsTrue(System.IO.Directory.Exists(fileName2.ParentDir.Path), "Dir before");
        Assert.IsTrue(System.IO.File.Exists(fileName1.Path), "#1 before");
        Assert.IsTrue(System.IO.File.Exists(fileName2.Path), "#2 before");
        Assert.IsTrue(System.IO.File.Exists(fileName3.Path), "#3 before");

        FileTools.DeleteFiles(dir.Dir, "*.txt");

        Assert.IsTrue(System.IO.Directory.Exists(fileName2.ParentDir.Path), "Dir after");
        Assert.IsFalse(System.IO.File.Exists(fileName1.Path), "#1 after");
        Assert.IsTrue(System.IO.File.Exists(fileName2.Path), "#2 after");
        Assert.IsTrue(System.IO.File.Exists(fileName3.Path), "#3 after");
      }
    }

    #endregion

    #region ForceDirs(), ForceDirsAndClear()

    [Test]
    public void ForceDirs()
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath dir1 = new AbsPath(dir.Dir, "1");
        AbsPath dir2 = new AbsPath(dir.Dir, "1", "2");
        AbsPath dir3 = new AbsPath(dir.Dir, "1", "2", "3");

        FileTools.ForceDirs(dir3);

        Assert.IsTrue(System.IO.Directory.Exists(dir1.Path), "#1");
        Assert.IsTrue(System.IO.Directory.Exists(dir2.Path), "#2");
        Assert.IsTrue(System.IO.Directory.Exists(dir3.Path), "#3");
      }
    }

    [Test]
    public void ForceDirsAndClear_1arg()
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath dir1 = new AbsPath(dir.Dir, "1");
        AbsPath dir2 = new AbsPath(dir.Dir, "1", "2");
        AbsPath dir3 = new AbsPath(dir.Dir, "1", "2", "3");
        FileTools.ForceDirs(dir3);
        AbsPath file1 = new AbsPath(dir1, "1.txt");
        AbsPath file2 = new AbsPath(dir2, "2.txt");
        AbsPath file3 = new AbsPath(dir3, "3.txt");
        System.IO.File.WriteAllBytes(file1.Path, DataTools.EmptyBytes);
        System.IO.File.WriteAllBytes(file2.Path, DataTools.EmptyBytes);
        System.IO.File.WriteAllBytes(file3.Path, DataTools.EmptyBytes);

        FileTools.ForceDirsAndClear(dir2);

        Assert.IsTrue(System.IO.Directory.Exists(dir1.Path), "dir 1");
        Assert.IsTrue(System.IO.Directory.Exists(dir2.Path), "dir 2");
        Assert.IsFalse(System.IO.Directory.Exists(dir3.Path), "dir 3");

        Assert.IsTrue(System.IO.File.Exists(file1.Path), "file 1.txt");
        Assert.IsFalse(System.IO.File.Exists(file2.Path), "file 2.txt");
        Assert.IsFalse(System.IO.File.Exists(file3.Path), "file 3.txt");
      }
    }

    #endregion

    #region ClearDirAsPossible(), DeleteDirAsPossible()

    [Test]
    public void ClearDirAsPossible([Values(false, true)] bool lockFile1, [Values(false, true)] bool lockFile2)
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath dir1 = new AbsPath(dir.Dir, "1");
        AbsPath dir2 = new AbsPath(dir.Dir, "1", "2");
        AbsPath file1 = new AbsPath(dir1, "1.txt");
        AbsPath file2 = new AbsPath(dir1, "2.txt");
        FileTools.ForceDirs(dir2);
        System.IO.File.WriteAllBytes(file1.Path, DataTools.EmptyBytes);
        System.IO.File.WriteAllBytes(file2.Path, DataTools.EmptyBytes);
        System.IO.FileStream fs1 = null;
        System.IO.FileStream fs2 = null;
        bool res;
        try
        {
          if (lockFile1)
            fs1 = new System.IO.FileStream(file1.Path, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite);
          if (lockFile2)
            fs2 = new System.IO.FileStream(file2.Path, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite);

          res = FileTools.ClearDirAsPossible(dir1);
        }
        finally
        {
          if (fs1 != null)
            fs1.Dispose();
          if (fs2 != null)
            fs2.Dispose();
        }

        Assert.AreEqual(!(lockFile1 || lockFile2), res, "Result");

        Assert.IsTrue(System.IO.Directory.Exists(dir1.Path), "dir1");
        Assert.IsFalse(System.IO.Directory.Exists(dir2.Path), "dir2");
        Assert.AreEqual(lockFile1, System.IO.File.Exists(file1.Path), "file1");
        Assert.AreEqual(lockFile2, System.IO.File.Exists(file2.Path), "file2");
      }
    }

    [Test]
    public void DeleteDirAsPossible([Values(false, true)] bool lockFile1, [Values(false, true)] bool lockFile2)
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath dir1 = new AbsPath(dir.Dir, "1");
        AbsPath dir2 = new AbsPath(dir.Dir, "1", "2");
        AbsPath file1 = new AbsPath(dir1, "1.txt");
        AbsPath file2 = new AbsPath(dir1, "2.txt");
        FileTools.ForceDirs(dir2);
        System.IO.File.WriteAllBytes(file1.Path, DataTools.EmptyBytes);
        System.IO.File.WriteAllBytes(file2.Path, DataTools.EmptyBytes);
        System.IO.FileStream fs1 = null;
        System.IO.FileStream fs2 = null;
        bool res;
        try
        {
          if (lockFile1)
            fs1 = new System.IO.FileStream(file1.Path, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite);
          if (lockFile2)
            fs2 = new System.IO.FileStream(file2.Path, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite);

          res = FileTools.DeleteDirAsPossible(dir1);
        }
        finally
        {
          if (fs1 != null)
            fs1.Dispose();
          if (fs2 != null)
            fs2.Dispose();
        }

        Assert.AreEqual(!(lockFile1 || lockFile2), res, "Result");

        Assert.AreEqual(lockFile1 || lockFile2, System.IO.Directory.Exists(dir1.Path), "dir1");
        Assert.IsFalse(System.IO.Directory.Exists(dir2.Path), "dir2");
        Assert.AreEqual(lockFile1, System.IO.File.Exists(file1.Path), "file1");
        Assert.AreEqual(lockFile2, System.IO.File.Exists(file2.Path), "file2");
      }
    }

    #endregion

    #region DeleteEmptyDirs()

    [Test]
    public void DeleteEmptyDirs([Values(false, true)] bool deleteRootDir,
      [Values(false, true)] bool createFile1,
      [Values(false, true)] bool createFile2,
      [Values(false, true)] bool createFile3,
      [Values(false, true)] bool createFile4)
    {
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath dir1 = new AbsPath(dir.Dir, "1");
        AbsPath dir2 = new AbsPath(dir.Dir, "1", "2");
        AbsPath dir3 = new AbsPath(dir.Dir, "1", "2", "3");
        AbsPath dir4 = new AbsPath(dir.Dir, "1", "2", "4");
        AbsPath file1 = new AbsPath(dir1, "1.txt");
        AbsPath file2 = new AbsPath(dir2, "2.txt");
        AbsPath file3 = new AbsPath(dir3, "3.txt");
        AbsPath file4 = new AbsPath(dir4, "4.txt");
        FileTools.ForceDirs(dir3);
        FileTools.ForceDirs(dir4);
        if (createFile1)
          System.IO.File.WriteAllBytes(file1.Path, DataTools.EmptyBytes);
        if (createFile2)
          System.IO.File.WriteAllBytes(file2.Path, DataTools.EmptyBytes);
        if (createFile3)
          System.IO.File.WriteAllBytes(file3.Path, DataTools.EmptyBytes);
        if (createFile4)
          System.IO.File.WriteAllBytes(file4.Path, DataTools.EmptyBytes);

        FileTools.DeleteEmptyDirs(dir1, deleteRootDir);

        Assert.AreEqual(!(deleteRootDir) | createFile1 | createFile2 | createFile3 | createFile4, System.IO.Directory.Exists(dir1.Path), "dir1");
        Assert.AreEqual(createFile2 | createFile3 | createFile4, System.IO.Directory.Exists(dir2.Path), "dir2");
        Assert.AreEqual(createFile3, System.IO.Directory.Exists(dir3.Path), "dir3");
        Assert.AreEqual(createFile4, System.IO.Directory.Exists(dir4.Path), "dir4");

        Assert.AreEqual(createFile1, System.IO.File.Exists(file1.Path), "file1");
        Assert.AreEqual(createFile2, System.IO.File.Exists(file2.Path), "file2");
        Assert.AreEqual(createFile3, System.IO.File.Exists(file3.Path), "file3");
        Assert.AreEqual(createFile4, System.IO.File.Exists(file4.Path), "file4");
      }
    }

    #endregion

    #region GetLongPath() / GetShortPath()

    [Platform("Win")]
    [Test]
    public void GetShortPath_GetLongPath()
    {
      AbsPath path = new AbsPath(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
      Assert.IsFalse(path.IsEmpty, "IsEmpty #1");
      Assert.IsTrue(System.IO.Directory.Exists(path.Path), "Exists #1");

      AbsPath path83 = FileTools.GetShortPath(path);
      Assert.IsFalse(path.IsEmpty, "IsEmpty #2");
      Assert.IsTrue(System.IO.Directory.Exists(path83.Path), "Exists #2");

      AbsPath pathLong = FileTools.GetLongPath(path83);
      Assert.AreEqual(path, pathLong, "Path #3");
    }

    #endregion

    #region WriteStream()

    [Test]
    public void WriteStream()
    {
      byte[] b = new byte[10000];
      for (int i = 0; i < b.Length; i++)
        b[i] = (byte)(i % 256);
      System.IO.MemoryStream src = new System.IO.MemoryStream(b);
      src.Position = 100;
      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath path = new AbsPath(dir.Dir, "1.bin");
        FileTools.WriteStream(path, src);

        byte[] b2 = System.IO.File.ReadAllBytes(path.Path);
        Assert.AreEqual(b.Length - 100, b2.Length, "Length");
        byte[] b3 = new byte[b.Length - 100];
        Array.Copy(b, 100, b3, 0, b3.Length);
        CollectionAssert.AreEqual(b3, b2, "Bytes");
      }

    }

    #endregion

    #region CopyStream()

    [Test]
    public void CopyStream([Values(0, 100, 1000, 10000)]int len, [Values(false, true)]bool seekStart)
    {
      byte[] b = new byte[len];
      for (int i = 0; i < len; i++)
        b[i] = (byte)(i % 256);
      System.IO.MemoryStream src = new System.IO.MemoryStream(b);
      int startPos = len / 3;
      src.Position = startPos;
      System.IO.MemoryStream dest = new System.IO.MemoryStream();

      FileTools.CopyStream(src, dest, seekStart);
      dest.Flush();
      byte[] b2 = dest.ToArray();

      byte[] wanted = new byte[len - (seekStart ? 0 : startPos)];
      Array.Copy(b, seekStart ? 0 : startPos, wanted, 0, wanted.Length);
      CollectionAssert.AreEqual(wanted, b2);
    }

    #endregion

    #region Read/WriteAllBytes()

    [Test]
    public void ReadAllBytes([Values(0, 100, 1000, 10000)]int len)
    {
      byte[] b = new byte[len];
      for (int i = 0; i < len; i++)
        b[i] = (byte)(i % 256);
      System.IO.MemoryStream src = new System.IO.MemoryStream(b);
      int startPos = len / 3;
      src.Position = startPos;

      byte[] res = FileTools.ReadAllBytes(src);
      byte[] wanted = new byte[len - startPos];
      Array.Copy(b, startPos, wanted, 0, wanted.Length);
      CollectionAssert.AreEqual(wanted, res);
    }

    [Test]
    public void WriteAllBytes([Values(0, 100, 1000, 10000)]int len)
    {
      byte[] b = new byte[len];
      for (int i = 0; i < len; i++)
        b[i] = (byte)(i % 256);
      System.IO.MemoryStream dst = new System.IO.MemoryStream();
      dst.WriteByte(1);
      dst.WriteByte(1);
      dst.WriteByte(1);

      FileTools.WriteAllBytes(dst, b);

      dst.Flush();
      byte[] b2 = dst.ToArray();

      byte[] wanted = new byte[len + 3];
      wanted[0] = 1;
      wanted[1] = 1;
      wanted[2] = 1;
      Array.Copy(b, 0, wanted, 3, len);
      CollectionAssert.AreEqual(wanted, b2);
    }

    #endregion

    #region WriteZeros()

    [Test]
    public void WriteZeros([Values(0, 10, 100, 1000, 10000, 123456)]int count)
    {
      System.IO.MemoryStream res = new System.IO.MemoryStream();
      res.WriteByte(1);
      FileTools.WriteZeros(res, (long)count);
      res.WriteByte(2);
      res.Flush();
      byte[] b2 = res.ToArray();

      byte[] wanted = new byte[count + 2];
      wanted[0] = 1;
      wanted[count + 1] = 2;

      CollectionAssert.AreEqual(wanted, b2);
    }

    #endregion

    #region TestSignature()

    [TestCase("ABCD", "ABC", true)]
    [TestCase("ABCD", "abc", false)]
    [TestCase("ABC", "ABC", true)]
    [TestCase("AB", "ABC", false)]
    [TestCase("", "ABC", false)]
    // аргумент signature должен быть задан
    //[TestCase("ABC", "", true)]
    //[TestCase("", "", true)]
    public void TestSignature(string str, string signature, bool wantedRes)
    {
      System.IO.StringReader rdr = new System.IO.StringReader(str);
      Assert.AreEqual(wantedRes, FileTools.TestSignature(rdr, signature));
    }

    #endregion

    #region StartsWith()

    [TestCase("1a2b3c4d", "1a2b3c", true)]
    [TestCase("1a2b3c4d", "1a2b3d", false)]
    [TestCase("1a2b3c4d", "1d2b3c", false)]
    [TestCase("1a2b3c", "1a2b3c", true)]
    [TestCase("1a2b3c", "1a2b3c4d", false)]
    [TestCase("1a2b3c", "", true)]
    [TestCase("", "1a2b3c", false)]
    [TestCase("", "", true)]
    public void StartsWith(string sStream, string sBytes, bool wantedRes)
    {
      byte[] b1 = DataTools.HexToBytes(sStream);
      System.IO.MemoryStream strm = new System.IO.MemoryStream(b1);
      byte[] bytes = DataTools.HexToBytes(sBytes);

      bool res = FileTools.StartsWith(strm, bytes);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region TextFileEncoding

    [Test]
    public void TextFileEncoding()
    {
      Assert.IsNotNull(FileTools.TextFileEncoding); // А что еще можно проверить
    }

    #endregion

    #region Read/WriteXmlDocument()

    [TestCase("windows-1251", 1251)]
    [TestCase("utf-8", 65001)]
    public void WriteXmlDocument(string encodingStr, int codePage)
    {
      XmlDocument xmlDoc = new XmlDocument();
      XmlElement elMain = xmlDoc.CreateElement("Эл1");
      xmlDoc.AppendChild(elMain);
      XmlElement el2 = xmlDoc.CreateElement("Эл2");
      elMain.AppendChild(el2);
      XmlAttribute attr = xmlDoc.CreateAttribute("Атр3");
      attr.Value = "ABC";
      el2.Attributes.Append(attr);
      XmlDeclaration decl = xmlDoc.CreateXmlDeclaration("1.0", encodingStr, "yes");
      xmlDoc.InsertBefore(decl, elMain);

      // <?xml version="1.0" encoding="xxx" standalone="yes"?>
      // <Эл1>
      //   <Эл2 Атр3="ABC"/>
      // </Эл1>

      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath file1 = new AbsPath(dir.Dir, "test1.xml");
        FileTools.WriteXmlDocument(file1, xmlDoc);
        DoTestWriteXmlDocument(file1, codePage, "#1");

        System.IO.MemoryStream ms2 = new System.IO.MemoryStream();
        FileTools.WriteXmlDocument(ms2, xmlDoc);
        ms2.Flush();
        byte[] b2 = ms2.ToArray();
        AbsPath file2 = new AbsPath(dir.Dir, "test2.xml");
        System.IO.File.WriteAllBytes(file2.Path, b2);
        DoTestWriteXmlDocument(file2, codePage, "#2");
      }
    }

    private static void DoTestWriteXmlDocument(AbsPath file, int codePage, string messagePrefix)
    {
      Assert.IsTrue(System.IO.File.Exists(file.Path), messagePrefix + ". File exists");
      Encoding enc = Encoding.GetEncoding(codePage);
      string[] lines = System.IO.File.ReadAllLines(file.Path, enc);
      Assert.AreEqual(4, lines.Length, messagePrefix + ". Line count");
      Assert.IsTrue(lines[0].StartsWith("<?xml version=\"1.0\"", StringComparison.Ordinal), messagePrefix + ". Line 1");
      // Мало просто использовать Trim(), т.к. пробелы могут быть вставлены в середину строки без определенных правил
      Assert.AreEqual("<Эл1>", DataTools.RemoveChars(lines[1], " "), messagePrefix + ". Line 2");
      Assert.AreEqual("<Эл2Атр3=\"ABC\"/>", DataTools.RemoveChars(lines[2], " "), messagePrefix + ". Line 3");
      Assert.AreEqual("</Эл1>", DataTools.RemoveChars(lines[3], " "), messagePrefix + ". Line 4");
    }

    [Test]
    public void ReadXmlDocument()
    {
      string[] lines = new string[] {
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
        "<Эл1>",
        "<Эл2 Атр3=\"ABC\"/>",
        "</Эл1>" };

      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath file1 = new AbsPath(dir.Dir, "test1.xml");
        System.IO.File.WriteAllLines(file1.Path, lines, Encoding.UTF8);

        XmlDocument xmlDoc1 = FileTools.ReadXmlDocument(file1);
        DoTestReadXmlDocument(xmlDoc1, "#1");

        byte[] b2 = System.IO.File.ReadAllBytes(file1.Path);
        System.IO.MemoryStream ms2 = new System.IO.MemoryStream(b2);
        XmlDocument xmlDoc2 = FileTools.ReadXmlDocument(ms2);
        DoTestReadXmlDocument(xmlDoc2, "#2");
      }
    }

    private static void DoTestReadXmlDocument(XmlDocument xmlDoc, string messagePrefix)
    {
      Assert.AreEqual(2, xmlDoc.ChildNodes.Count, messagePrefix + ". ChildNodeCount");
      Assert.IsInstanceOf<XmlDeclaration>(xmlDoc.ChildNodes[0], messagePrefix + ". Declaration type");
      Assert.IsInstanceOf<XmlElement>(xmlDoc.ChildNodes[1], messagePrefix + ". Root Element type");
      Assert.AreEqual("Эл1", xmlDoc.DocumentElement.Name, messagePrefix + ". Root Element name");
    }

    #endregion

    #region IsValidXmlStart()

    [Test]
    public void IsValidXmlStart_FileName()
    {
      // Условный xml-файл из одной строки
      string sGood = "<?xml version =\"1.0\"?>";
      string sBad = "<XXX>";

      // проверяем только одну кодировку, чтобы не записывать много файлов

      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath file1 = new AbsPath(dir.Dir, "test1.xml");
        System.IO.File.WriteAllText(file1.Path, sGood, Encoding.UTF8);
        Assert.IsTrue(FileTools.IsValidXmlStart(file1), "#1");

        AbsPath file2 = new AbsPath(dir.Dir, "test2.xml");
        System.IO.File.WriteAllText(file2.Path, sBad, Encoding.UTF8);
        Assert.IsFalse(FileTools.IsValidXmlStart(file2), "#2");
      }
    }

    /// <summary>
    /// Расширение класса EncodingInfo методом ToString() для отображения в списке тестов
    /// </summary>
    public struct EncodingInfo2
    {
      public EncodingInfo2(EncodingInfo info)
      {
        _Info = info;
      }

      public EncodingInfo Info { get { return _Info; } }
      private EncodingInfo _Info;

      public override string ToString()
      {
        if (_Info == null)
          return "Empty";
        else
          return _Info.DisplayName + ", CodePage=" + _Info.CodePage;
      }
    }

    public EncodingInfo2[] IsValidXmlStart_Encodings
    {
      get
      {
        List<EncodingInfo2> lst = new List<EncodingInfo2>();
        foreach (EncodingInfo ei in Encoding.GetEncodings())
        {
          if (ei.DisplayName.StartsWith("IBM EBCDIC"))
            continue; // может быть, и нужно делать для этих кодировок

          switch (ei.CodePage)
          {
            case 20924: // IBM Латиница-1 на Windows-XP
            case 1047: // IBM Латиница-1 на Windows-7
            case 65000: // UTF-7
              continue;
          }

          lst.Add(new EncodingInfo2(ei));
        }
        return lst.ToArray();
      }
    }

    [TestCaseSource("IsValidXmlStart_Encodings")]
    public void IsValidXmlStart_Stream(EncodingInfo2 ei2)
    {
      // Условный xml-файл из одной строки
      string sGood = "<?xml version =\"1.0\"?>";
      string sBad = "<XXX>";
      DoTestIsValidXmlStart(ei2.Info, sGood, true, "#1");
      DoTestIsValidXmlStart(ei2.Info, sBad, false, "#2");
    }

    private static void DoTestIsValidXmlStart(EncodingInfo ei, string s, bool wantedRes, string message)
    {
      Encoding enc = ei.GetEncoding();

      System.IO.MemoryStream ms = new System.IO.MemoryStream();
      System.IO.TextWriter wrt = new System.IO.StreamWriter(ms, enc);
      wrt.Write(s);
      wrt.Flush();
      string sHexBytes = DataTools.BytesToHex(ms.ToArray(), false);
      ms.Position = 0;

      bool res = FileTools.IsValidXmlStart(ms);
      Assert.AreEqual(wantedRes, res, message /*+ ", Stream="+sHexBytes*/);
    }

    #endregion

    #region WriteDataSetBinary(), ReadDataSetBinary()

    [Test]
    public void WriteDataSetBinary_ReadDataSetBinary()
    {
      System.Data.DataSet ds1 = new System.Data.DataSet();
      System.Data.DataTable tbl11 = ds1.Tables.Add("T1");
      tbl11.Columns.Add("F1", typeof(string));
      tbl11.Columns.Add("F2", typeof(int));
      tbl11.Columns.Add("F3", typeof(bool));
      tbl11.Rows.Add("AAA", 123, false);
      tbl11.Rows.Add("BBB", 456, true);

      System.Data.DataTable tbl12 = ds1.Tables.Add("T2");

      System.Data.DataSet ds2;

      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath file = new AbsPath(dir.Dir, "test.bin");
        FileTools.WriteDataSetBinary(file, ds1);

        ds2 = FileTools.ReadDataSetBinary(file);
      }

      CollectionAssert.AreEqual(new string[] { "T1", "T2" }, DataTools.GetTableNames(ds2), "TableNames");
      System.Data.DataTable tbl21 = ds2.Tables[0];
      System.Data.DataTable tbl22 = ds2.Tables[1];

      CollectionAssert.AreEqual(new string[] { "F1", "F2", "F3" }, DataTools.GetColumnNames(tbl21), "ColumnNames 1");
      Assert.AreEqual(2, tbl21.Rows.Count, "RowCount 1");
      CollectionAssert.AreEqual(tbl11.Rows[0].ItemArray, tbl21.Rows[0].ItemArray, "Values 1.1");
      CollectionAssert.AreEqual(tbl11.Rows[1].ItemArray, tbl21.Rows[1].ItemArray, "Values 1.1");

      Assert.AreEqual(0, tbl22.Rows.Count, "RowCount 2");
    }

    #endregion

    #region Версия файла

    #region GetFileVersion()

    [Test]
    [Platform("Win")]
    public void GetFileVersion_file_ok()
    {
      System.Reflection.Assembly asm = typeof(FileTools).Assembly;
      AbsPath path = new AbsPath(asm.Location);
      Assert.IsFalse(path.IsEmpty, "AbsPath.IsEmpty");
      Assert.IsTrue(System.IO.File.Exists(path.Path), "FileExists");

      Version res = FileTools.GetFileVersion(path);

      Assert.IsNotNull(res, "Version is not null");
      System.Reflection.AssemblyName an = new System.Reflection.AssemblyName(asm.FullName);
      Assert.AreEqual(an.Version, res, "Version");
    }

    [Test]
    [Platform("Win")]
    public void GetFileVersion_file_doesnotexist()
    {
      AbsPath path = new AbsPath("xxx.dll");
      Assert.IsFalse(System.IO.File.Exists(path.Path), "FileExists");
      Version res = FileTools.GetFileVersion(path);
      Assert.IsNull(res, "Version");
    }

    [Test]
    [Platform("Win")]
    public void GetFileVersion_file_empty()
    {
      AbsPath path = AbsPath.Empty;
      Version res = FileTools.GetFileVersion(path);
      Assert.IsNull(res, "Version");
    }

    [Test]
    [Platform("Win")]
    public void GetFileVersion_FileVersionInfo_ok()
    {
      System.Reflection.Assembly asm = typeof(FileTools).Assembly;
      AbsPath path = new AbsPath(asm.Location);
      Assert.IsFalse(path.IsEmpty, "AbsPath.IsEmpty");
      Assert.IsTrue(System.IO.File.Exists(path.Path), "FileExists");
      System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(path.Path);

      Version res = FileTools.GetFileVersion(fvi);

      Assert.IsNotNull(res, "Version is not null");
      System.Reflection.AssemblyName an = new System.Reflection.AssemblyName(asm.FullName);
      Assert.AreEqual(an.Version, res, "Version");
    }

    [Test]
    [Platform("Win")]
    public void GetFileVersion_FileVersionInfo_null()
    {
      System.Diagnostics.FileVersionInfo fvi = null;
      Version res = FileTools.GetFileVersion(fvi);
      Assert.IsNull(res, "Version");
    }

    #endregion

    #region GetVersionFromStr()

    [TestCase("", "")] // пустая строка
    [TestCase("1", "1.0")] // не хватает одного разряда
    [TestCase("1.2", "1.2")]
    [TestCase("1.2.3", "1.2.3")]
    [TestCase("1.2.3.4", "1.2.3.4")]
    [TestCase("1.2.3.4.5", "1.2.3.4")] // лишний разряд
    [TestCase("1a.2b.3c.4d", "1.2.3.4")] // лишние символы
    [TestCase("1.abc.3.4", "1.0.3.4")] // буквенная часть
    [TestCase("111111111111", "")] // слишком длинное число
    [TestCase("1.222222222222", "1.0")]
    [TestCase("1.2.333333333333", "1.2.0")]
    [TestCase("1.2.3.444444444444", "1.2.3.0")]
    [TestCase("1.-2", "1.0")] // отрицательное число
    public void GetVersionFromStr(string s, string sWantedRes)
    {
      Version res = FileTools.GetVersionFromStr(s);
      string sRes;
      if (res == null)
        sRes = "";
      else
        sRes = res.ToString();

      Assert.AreEqual(sWantedRes, sRes);
    }

    #endregion

    #endregion

    #region Is64bitPE()

    // Нет проверок для 64-битного приложения

    [Test]
    public void Is64bitPE_self()
    {
      System.Reflection.Assembly asm = typeof(FileTools).Assembly;
      AbsPath path = new AbsPath(asm.Location);
      Assert.IsFalse(path.IsEmpty, "AbsPath.IsEmpty");
      Assert.IsTrue(System.IO.File.Exists(path.Path), "FileExists");

      bool? res = FileTools.Is64bitPE(path);
      Assert.AreEqual(false, res, "Result");
    }

    #endregion

    #region IsFloppyDriveDir()

    // Есть только отрицательный тест, где же сейчас флоппи-дисковод найти?

    [Test]
    public void IsFloppyDriveDir()
    {
      AbsPath path = new AbsPath(System.IO.Path.GetTempPath());
      Assert.IsFalse(path.IsEmpty, "AbsPath.IsEmpty");

      Assert.IsFalse(FileTools.IsFloppyDriveDir(path), "Result");
    }

    #endregion

    #region Список компьютеров в сети

    // Не проверяем GetNetworkMachineNames()

    #endregion

    #region Copy()

    [Test]
    public void Copy([Values(0, 100, 123456)]int len)
    {
      byte[] b1 = new byte[len];
      byte[] b2 = new byte[len * 2];
      for (int i = 0; i < b1.Length; i++)
        b1[i] = (byte)(i % 100);
      for (int i = 0; i < b2.Length; i++)
        b2[i] = (byte)(i % 256);

      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath srcDir = new AbsPath(dir.Dir, "D1");
        AbsPath srcDir2 = new AbsPath(srcDir, "SD");
        System.IO.Directory.CreateDirectory(srcDir2.Path);
        System.IO.File.WriteAllBytes(srcDir.SlashedPath + "f1.bin", b1);
        System.IO.File.WriteAllBytes(srcDir.SlashedPath + "f2.bin", b1);
        System.IO.File.WriteAllBytes(srcDir2.SlashedPath + "f1.bin", b2);

        AbsPath dstDir = new AbsPath(dir.Dir, "D2");

        FileTemplateList templates = new FileTemplateList();
        templates.Add("f1.bin", true);

        FileTools.Copy(srcDir, dstDir, templates, null);

        Assert.IsTrue(System.IO.Directory.Exists(dstDir.SlashedPath + "SD"), "SD");
        Assert.IsTrue(System.IO.File.Exists(dstDir.SlashedPath + "f1.bin"), "f1");
        Assert.IsFalse(System.IO.File.Exists(dstDir.SlashedPath + "f2.bin"), "f2");
        AbsPath pathSdF1 = new AbsPath(dstDir, "SD", "f1.bin");
        Assert.IsTrue(System.IO.File.Exists(pathSdF1.Path), "SD/f1");

        CollectionAssert.AreEqual(b1, System.IO.File.ReadAllBytes(dstDir.SlashedPath + "f1.bin"), "Content f1");
        CollectionAssert.AreEqual(b2, System.IO.File.ReadAllBytes(pathSdF1.Path), "Content SD/f1");
      }
    }

    #endregion

    #region ApplicationPath, ApplicationBaseDir

    [Test]
    public void ApplicationPath_ApplicationBaseDir()
    {
      Assert.IsFalse(FileTools.ApplicationPath.IsEmpty, "ApplicationPath");
      Assert.IsFalse(FileTools.ApplicationBaseDir.IsEmpty, "ApplicationBaseDir");
      Assert.IsTrue(FileTools.ApplicationPath.StartsWith(FileTools.ApplicationBaseDir), "relation");
    }

    #endregion

    // TODO: FindExecutableFilePath()

    #region MD5Sum()

    public void MD5Sum([Values(0, 100, 123456)]int len)
    {
      byte[] b = new byte[len];
      for (int i = 0; i < b.Length; i++)
        b[i] = (byte)(i % 100);

      string wantedRes = DataTools.MD5Sum(b);

      System.IO.MemoryStream ms1 = new System.IO.MemoryStream(b);
      ms1.Position = 0;
      string res1 = FileTools.MD5Sum(ms1);
      Assert.AreEqual(wantedRes, res1, "Stream");

      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath file2 = new AbsPath(dir.Dir, "test.bin");
        System.IO.File.WriteAllBytes(file2.Path, b);

        string res2 = FileTools.MD5Sum(file2);
        Assert.AreEqual(wantedRes, res2, "File");
      }
    }

    #endregion
  }
}
