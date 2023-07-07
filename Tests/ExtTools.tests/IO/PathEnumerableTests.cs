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
  /// <summary>
  /// Создает тестовую структуру каталогов и файлов для перебора
  /// </summary>
  public class PathEnumerableTestsBase
  {
    // Тестовая структура
    //
    // D1
    //   D11
    //     D111
    //       f1111.txt
    //       f1112.txt
    //       f1113.txt
    //     D112
    //       f1121.txt
    //     f113.txt  
    //   D12 
    //     f121.txt
    //   f13.txt
    // D1.1               // каталог с расширением
    // D2
    //   f21.txt
    //   f22.txt
    // f3.txt
    // f3.xml

    #region Создания тестовой структуры файлов

    [OneTimeSetUp]
    public void SetUp()
    {
      _TempDir = new TempDirectory();
      FileTools.ForceDirs(new AbsPath(_TempDir.Dir, "D1", "D11", "D111"));
      FileTools.ForceDirs(new AbsPath(_TempDir.Dir, "D1", "D11", "D112"));
      FileTools.ForceDirs(new AbsPath(_TempDir.Dir, "D1", "D12"));
      FileTools.ForceDirs(new AbsPath(_TempDir.Dir, "D1.1"));
      FileTools.ForceDirs(new AbsPath(_TempDir.Dir, "D2"));
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D1", "D11", "D111", "f1111.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D1", "D11", "D111", "f1112.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D1", "D11", "D111", "f1113.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D1", "D11", "D112", "f1121.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D1", "D11", "f113.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D1", "D12", "f121.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D1", "f13.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D2", "f21.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D2", "f22.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "f3.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "f3.xml").Path, DataTools.EmptyBytes);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
      if (_TempDir != null)
      {
        _TempDir.Dispose();
        _TempDir = null;
      }
    }

    private TempDirectory _TempDir;

    /// <summary>
    /// Каталог с вложенной структурой
    /// </summary>
    protected AbsPath RootDir
    {
      get
      {
        if (_TempDir == null)
          return AbsPath.Empty;
        else
          return _TempDir.Dir;
      }
    }

    #endregion
  }

  [TestFixture]
  public class AbsPathEnumerableTests: PathEnumerableTestsBase
  {
    #region Конструкторы

    [Test]
    public void Constructor_2args()
    {
      AbsPathEnumerable sut = new AbsPathEnumerable(RootDir, PathEnumerateKind.FilesAndDirectories);

      Assert.AreEqual(RootDir, sut.RootDirectory, "RootDirectory");
      Assert.AreEqual(PathEnumerateKind.FilesAndDirectories, sut.EnumerateKind, "EnumerateKind");
      Assert.AreEqual(PathEnumerateMode.FilesAndDirectories, sut.EnumerateMode, "EnumerateMode");
      Assert.AreEqual("*", sut.DirectorySearchPattern, "DirectorySearchPattern");
      Assert.AreEqual(PathEnumerateSort.None, sut.DirectorySort, "DirectorySort");
      Assert.IsFalse(sut.ReverseDirectories, "ReverseDirectories");
      Assert.AreEqual("*", sut.FileSearchPattern, "FileSearchPattern");
      Assert.AreEqual(PathEnumerateSort.None, sut.FileSort, "FileSort");
      Assert.IsFalse(sut.ReverseFiles, "ReverseFiles");
    }

    [Test]
    public void Constructor_1arg()
    {
      AbsPathEnumerable sut = new AbsPathEnumerable(RootDir);

      Assert.AreEqual(RootDir, sut.RootDirectory, "RootDirectory");
      Assert.AreEqual(PathEnumerateKind.Files, sut.EnumerateKind, "EnumerateKind");
      Assert.AreEqual(PathEnumerateMode.FilesAndDirectories, sut.EnumerateMode, "EnumerateMode");
      Assert.AreEqual("*", sut.DirectorySearchPattern, "DirectorySearchPattern");
      Assert.AreEqual(PathEnumerateSort.None, sut.DirectorySort, "DirectorySort");
      Assert.IsFalse(sut.ReverseDirectories, "ReverseDirectories");
      Assert.AreEqual("*", sut.FileSearchPattern, "FileSearchPattern");
      Assert.AreEqual(PathEnumerateSort.None, sut.FileSort, "FileSort");
      Assert.IsFalse(sut.ReverseFiles, "ReverseFiles");
    }

    [Test]
    public void Constructor_exception()
    {
      Assert.Catch<ArgumentException>(delegate () { new AbsPathEnumerable(AbsPath.Empty); });
    }

    #endregion

    #region GetEnumerator()

    [Test]
    public void GetEnumerator_simple()
    {
      AbsPathEnumerable sut = new AbsPathEnumerable(RootDir);
      DoTestEnumerator(sut, "f1111.txt,f1112.txt,f1113.txt,f1121.txt,f113.txt,f121.txt,f13.txt,f21.txt,f22.txt,f3.txt,f3.xml", false);
    }

    [TestCase(PathEnumerateKind.Files, "f1111.txt,f1112.txt,f1113.txt,f1121.txt,f113.txt,f121.txt,f13.txt,f21.txt,f22.txt,f3.txt,f3.xml")]
    [TestCase(PathEnumerateKind.Directories, "D1,D11,D111,D112,D12,D1.1,D2")]
    [TestCase(PathEnumerateKind.FilesAndDirectories, "D1,D11,D111,D112,D12,D1.1,D2,f1111.txt,f1112.txt,f1113.txt,f1121.txt,f113.txt,f121.txt,f13.txt,f21.txt,f22.txt,f3.txt,f3.xml")]
    public void GetEnumerator_EnumerateKind(PathEnumerateKind enumerateKind, string sWanted)
    {
      AbsPathEnumerable sut = new AbsPathEnumerable(RootDir, enumerateKind);
      DoTestEnumerator(sut, sWanted, false);
    }

    private void DoTestEnumerator(AbsPathEnumerable sut, string sNames, bool isSorted)
    {
      string[] a = DataTools.EmptyStrings;
      if (sNames.Length > 0)
        a = sNames.Split(',');

      List<string> lst = new List<string>();
      foreach (AbsPath path in sut)
        lst.Add(path.FileName);

      if (isSorted)
        CollectionAssert.AreEqual(a, lst);
      else
        CollectionAssert.AreEquivalent(a, lst);
    }

    #endregion
  }
}
