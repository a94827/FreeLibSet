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
  /// <summary>
  /// Создает тестовую структуру каталогов и файлов для перебора
  /// </summary>
  public abstract class PathEnumerableTestsBase: FixtureWithSetUp
  {
    // Тестовая структура
    //
    // D1
    //   D11
    //     D111
    //       f1111.txt
    //       f1112.xml
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
    // f3.xml
    // f4.txt

    #region Создания тестовой структуры файлов

    protected override void OnOneTimeSetUp()
    {
      base.OnOneTimeSetUp();

      _TempDir = new TempDirectory();

      if (_TempDir.Dir.IsEmpty)
        throw new BugException("TempDir.Dir.IsEmpty");

      FileTools.ForceDirs(new AbsPath(_TempDir.Dir, "D1", "D11", "D111"));
      FileTools.ForceDirs(new AbsPath(_TempDir.Dir, "D1", "D11", "D112"));
      FileTools.ForceDirs(new AbsPath(_TempDir.Dir, "D1", "D12"));
      FileTools.ForceDirs(new AbsPath(_TempDir.Dir, "D1.1"));
      FileTools.ForceDirs(new AbsPath(_TempDir.Dir, "D2"));
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D1", "D11", "D111", "f1111.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D1", "D11", "D111", "f1112.xml").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D1", "D11", "D111", "f1113.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D1", "D11", "D112", "f1121.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D1", "D11", "f113.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D1", "D12", "f121.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D1", "f13.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D2", "f21.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "D2", "f22.txt").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "f3.xml").Path, DataTools.EmptyBytes);
      File.WriteAllBytes(new AbsPath(_TempDir.Dir, "f4.txt").Path, DataTools.EmptyBytes);
    }

    protected override void OnOneTimeTearDown()
    {
      if (_TempDir != null)
      {
        _TempDir.Dispose();
        _TempDir = null;
      }
      base.OnOneTimeTearDown();
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

    #region GetEnumerator()

    protected abstract PathEnumerableBase CreateTestObject(PathEnumerateKind enumerateKind);
    protected abstract void DoTestEnumerator(PathEnumerableBase sut, string sWanted, bool isSorted);

    [TestCase(PathEnumerateKind.Files, "f1111.txt,f1112.xml,f1113.txt,f1121.txt,f113.txt,f121.txt,f13.txt,f21.txt,f22.txt,f3.xml,f4.txt")]
    [TestCase(PathEnumerateKind.Directories, "D1,D11,D111,D112,D12,D1.1,D2")]
    [TestCase(PathEnumerateKind.FilesAndDirectories, "D1,D11,D111,D112,D12,D1.1,D2,f1111.txt,f1112.xml,f1113.txt,f1121.txt,f113.txt,f121.txt,f13.txt,f21.txt,f22.txt,f3.xml,f4.txt")]
    public void GetEnumerator_EnumerateKind(PathEnumerateKind enumerateKind, string sWanted)
    {
      PathEnumerableBase sut = CreateTestObject(enumerateKind);
      DoTestEnumerator(sut, sWanted, false);
    }


    [TestCase(PathEnumerateMode.FilesAndDirectories, "f3.xml,f4.txt,D1,f13.txt,D11,f113.txt,D111,f1111.txt,f1112.xml,f1113.txt,D112,f1121.txt,D12,f121.txt,D1.1,D2,f21.txt,f22.txt")]
    [TestCase(PathEnumerateMode.DirectoriesAndFiles, "D1,D11,D111,f1111.txt,f1112.xml,f1113.txt,D112,f1121.txt,f113.txt,D12,f121.txt,f13.txt,D1.1,D2,f21.txt,f22.txt,f3.xml,f4.txt")]
    [TestCase(PathEnumerateMode.DirectoriesOnly, "D1,D11,D111,D112,D12,D1.1,D2")]
    [TestCase(PathEnumerateMode.FilesOnly, "f3.xml,f4.txt")]
    [TestCase(PathEnumerateMode.None, "")]
    public void GetEnumerator_EnumerateMode(PathEnumerateMode enumerateMode, string sWanted)
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.FilesAndDirectories);
      sut.DirectorySort = PathEnumerateSort.ByName;
      sut.FileSort = PathEnumerateSort.ByName;
      sut.EnumerateMode = enumerateMode;
      DoTestEnumerator(sut, sWanted, true);
    }

    [TestCase("D1*.*", "D1,D11,D111,D112,D12,D1.1,f1111.txt,f1112.xml,f1113.txt,f1121.txt,f113.txt,f121.txt,f13.txt,f3.xml,f4.txt")]
    [TestCase("D2*.*", "D2,f21.txt,f22.txt,f3.xml,f4.txt")]
    [TestCase("D1", "D1,f13.txt,f3.xml,f4.txt")]
    public void GetEnumerator_DirectorySearchPattern(string directorySearchPattern, string sWanted)
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.FilesAndDirectories);
      sut.DirectorySearchPattern = directorySearchPattern;
      DoTestEnumerator(sut, sWanted, false);
    }

    [TestCase("f11*.*", "D1,D11,D111,D112,D12,D1.1,D2,f1111.txt,f1112.xml,f1113.txt,f1121.txt,f113.txt")]
    [TestCase("*.xml", "D1,D11,D111,D112,D12,D1.1,D2,f3.xml,f1112.xml")]
    [TestCase("*21.txt", "D1,D11,D111,D112,D12,D1.1,D2,f1121.txt,f121.txt,f21.txt")]
    [TestCase("f13.txt", "D1,D11,D111,D112,D12,D1.1,D2,f13.txt")]
    public void GetEnumerator_FileSearchPattern(string fileSearchPattern, string sWanted)
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.FilesAndDirectories);
      sut.FileSearchPattern = fileSearchPattern;
      DoTestEnumerator(sut, sWanted, false);
    }


    [TestCase(PathEnumerateSort.ByName, "D1,D11,D111,D112,D12,D1.1,D2")]
    [TestCase(PathEnumerateSort.ByExtension, "D1,D11,D111,D112,D12,D2,D1.1")]
    public void GetEnumerator_DirectorySort(PathEnumerateSort directorySort, string sWanted)
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.Directories);
      sut.DirectorySort = directorySort;
      DoTestEnumerator(sut, sWanted, true);
    }


    [TestCase(PathEnumerateSort.ByName, "f1111.txt,f1112.xml,f1113.txt,f1121.txt,f113.txt,f121.txt,f13.txt,f21.txt,f22.txt,f3.xml,f4.txt")]
    [TestCase(PathEnumerateSort.ByExtension, "f1111.txt,f1113.txt,f1112.xml,f1121.txt,f113.txt,f121.txt,f13.txt,f21.txt,f22.txt,f4.txt,f3.xml")]
    public void GetEnumerator_FileSort(PathEnumerateSort fileSort, string sWanted)
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.Files);
      sut.EnumerateMode = PathEnumerateMode.DirectoriesAndFiles;
      sut.DirectorySort = PathEnumerateSort.ByName;
      sut.FileSort = fileSort;
      DoTestEnumerator(sut, sWanted, true);
    }

    [TestCase(false, "f3.xml,f4.txt,D1,f13.txt,D11,f113.txt,D111,f1111.txt,f1112.xml,f1113.txt,D112,f1121.txt,D12,f121.txt,D1.1,D2,f21.txt,f22.txt")]
    [TestCase(true, "f3.xml,f4.txt,D2,f21.txt,f22.txt,D1.1,D1,f13.txt,D12,f121.txt,D11,f113.txt,D112,f1121.txt,D111,f1111.txt,f1112.xml,f1113.txt")]
    public void GetEnumerator_ReverseDirectories(bool reverseDirectories, string sWanted)
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.FilesAndDirectories);
      sut.DirectorySort = PathEnumerateSort.ByName;
      sut.FileSort = PathEnumerateSort.ByName;
      sut.EnumerateMode = PathEnumerateMode.FilesAndDirectories;
      sut.ReverseDirectories = reverseDirectories;
      DoTestEnumerator(sut, sWanted, true);
    }

    [TestCase(false, "f3.xml,f4.txt,D1,f13.txt,D11,f113.txt,D111,f1111.txt,f1112.xml,f1113.txt,D112,f1121.txt,D12,f121.txt,D1.1,D2,f21.txt,f22.txt")]
    [TestCase(true, "f4.txt,f3.xml,D1,f13.txt,D11,f113.txt,D111,f1113.txt,f1112.xml,f1111.txt,D112,f1121.txt,D12,f121.txt,D1.1,D2,f22.txt,f21.txt")]
    public void GetEnumerator_ReverseFiles(bool reverseFiles, string sWanted)
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.FilesAndDirectories);
      sut.DirectorySort = PathEnumerateSort.ByName;
      sut.FileSort = PathEnumerateSort.ByName;
      sut.EnumerateMode = PathEnumerateMode.FilesAndDirectories;
      sut.ReverseFiles = reverseFiles;
      DoTestEnumerator(sut, sWanted, true);
    }

    #endregion

    #region BeforeDirectory/AfterDirectory

    /// <summary>
    /// Тестер для событий
    /// Дополнительные тестовые обработчики BeforeDirectory должны добавляться после TestEventHandler
    /// </summary>
    private class TestEventHandler
    {
      public TestEventHandler(PathEnumerableBase sut)
      {
        _BeforeList = new List<string>();
        _AfterList = new List<string>();
        sut.BeforeDirectory += Sut_BeforeDirectory;
        sut.AfterDirectory += Sut_AfterDirectory;
      }

      private void Sut_BeforeDirectory(object sender, EnumDirectoryEventArgs args)
      {
        if (args.Level == 0)
          _BeforeList.Add("ROOT");
        else
          _BeforeList.Add(args.Directory.FileName);

        TestArgs(sender, args, true);
      }

      private void Sut_AfterDirectory(object sender, EnumDirectoryEventArgs args)
      {
        if (args.Level == 0)
          _AfterList.Add("ROOT");
        else
          _AfterList.Add(args.Directory.FileName);
        TestArgs(sender, args, false);
      }

      private List<string> _BeforeList;
      private List<string> _AfterList;

      private void TestArgs(object sender, EnumDirectoryEventArgs args, bool isBefore)
      {
        PathEnumerableBase sut = (PathEnumerableBase)sender;
        AbsPath testPath = sut.RootDirectory + args.DirectoryRel;
        Assert.AreEqual(args.Directory, testPath, "EnumDirectoryEventArgs.DirectoryRel");

        // Проверяем, что исходные значения не поменялись.
        // Дополнительные тестовые обработчики BeforeDirectory добавляются после TestEventHandler
      }

      public void Validate(string sWanted)
      {
        string[] a = DataTools.EmptyStrings;
        if (!String.IsNullOrEmpty(sWanted))
          a = sWanted.Split(',');

        CollectionAssert.AreEqual(a, _BeforeList, "BeforeDirectory event calls");
        CollectionAssert.AreEquivalent(a, _AfterList, "AfterDirectory event calls"); // порядок обхода другой
        if (a.Length > 0)
        {
          Assert.AreEqual("ROOT", _BeforeList[0], "First call of BeforeDirectory is ROOT");
          Assert.AreEqual("ROOT", _AfterList[_AfterList.Count - 1], "Last call of AfterDirectory is ROOT");
        }
      }
    }

    [Test]
    public void Events_NoAction()
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.Files);
      sut.DirectorySort = PathEnumerateSort.ByName;
      sut.FileSort = PathEnumerateSort.ByName;
      TestEventHandler handtester = new TestEventHandler(sut);
      DoTestEnumerator(sut, "f3.xml,f4.txt,f13.txt,f113.txt,f1111.txt,f1112.xml,f1113.txt,f1121.txt,f121.txt,f21.txt,f22.txt", true);
      handtester.Validate("ROOT,D1,D11,D111,D112,D12,D1.1,D2");
    }

    [Test]
    public void Events_EnumerateMode_DirectoryOnly()
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.Files);
      sut.DirectorySort = PathEnumerateSort.ByName;
      sut.FileSort = PathEnumerateSort.ByName;
      TestEventHandler handtester = new TestEventHandler(sut);
      sut.BeforeDirectory += Set_DirectoriesOnly;
      DoTestEnumerator(sut, "", true); // файлы не перечисляются
      handtester.Validate("ROOT,D1,D11,D111,D112,D12,D1.1,D2"); // каталоги перечисляются
    }

    private static void Set_DirectoriesOnly(object sender, EnumDirectoryEventArgs args)
    {
      args.EnumerateMode = PathEnumerateMode.DirectoriesOnly;
    }


    [Test]
    public void Events_EnumerateMode_NoneAfterLevel0()
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.Files);
      sut.DirectorySort = PathEnumerateSort.ByName;
      sut.FileSort = PathEnumerateSort.ByName;
      TestEventHandler handtester = new TestEventHandler(sut);
      sut.BeforeDirectory += Set_NoneAfterLevel0;
      DoTestEnumerator(sut, "f3.xml,f4.txt", true);
      handtester.Validate("ROOT,D1,D1.1,D2");
    }

    private static void Set_NoneAfterLevel0(object sender, EnumDirectoryEventArgs args)
    {
      if (args.Level > 0)
        args.EnumerateMode = PathEnumerateMode.None;
    }

    [Test]
    public void Events_EnumerateMode_NoneAfterLevel1()
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.Files);
      sut.DirectorySort = PathEnumerateSort.ByName;
      sut.FileSort = PathEnumerateSort.ByName;
      TestEventHandler handtester = new TestEventHandler(sut);
      sut.BeforeDirectory += Set_NoneAfterLevel1;
      DoTestEnumerator(sut, "f3.xml,f4.txt,f13.txt,f21.txt,f22.txt", true);
      handtester.Validate("ROOT,D1,D11,D12,D1.1,D2");
    }

    private static void Set_NoneAfterLevel1(object sender, EnumDirectoryEventArgs args)
    {
      if (args.Level > 1)
        args.EnumerateMode = PathEnumerateMode.None;
    }

    [Test]
    public void Events_DirectorySearchPattern()
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.Files);
      sut.DirectorySort = PathEnumerateSort.ByName;
      sut.FileSort = PathEnumerateSort.ByName;
      sut.BeforeDirectory += Set_DirectorySearchPattern;
      DoTestEnumerator(sut, "f3.xml,f4.txt,f13.txt,f113.txt,f1121.txt,f121.txt,f21.txt,f22.txt", true); // файлы из каталога D111 не перечисляются
    }

    private static void Set_DirectorySearchPattern(object sender, EnumDirectoryEventArgs args)
    {
      if (args.Directory.FileName == "D11")
        args.DirectorySearchPattern = "*2";
    }

    [Test]
    public void Events_FileSearchPattern()
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.Files);
      sut.DirectorySort = PathEnumerateSort.ByName;
      sut.FileSort = PathEnumerateSort.ByName;
      sut.BeforeDirectory += Set_FileSearchPattern;
      DoTestEnumerator(sut, "f3.xml,f4.txt,f13.txt,f113.txt,f1112.xml,f1121.txt,f121.txt,f21.txt,f22.txt", true); // f1111.txt и f1113.txt пропускается
    }

    private static void Set_FileSearchPattern(object sender, EnumDirectoryEventArgs args)
    {
      if (args.Directory.FileName == "D111")
        args.FileSearchPattern = "*.xml";
    }

    [Test]
    public void Events_DirectorySort()
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.Files);
      sut.DirectorySort = PathEnumerateSort.ByName;
      sut.FileSort = PathEnumerateSort.ByName;
      TestEventHandler handtester = new TestEventHandler(sut);
      sut.BeforeDirectory += Set_DirectorySort;
      DoTestEnumerator(sut, "f3.xml,f4.txt,f13.txt,f113.txt,f1111.txt,f1112.xml,f1113.txt,f1121.txt,f121.txt,f21.txt,f22.txt", true);
      handtester.Validate("ROOT,D1,D11,D111,D112,D12,D2,D1.1");
    }

    private static void Set_DirectorySort(object sender, EnumDirectoryEventArgs args)
    {
      if (args.Level == 0)
        args.DirectorySort = PathEnumerateSort.ByExtension;
    }

    [Test]
    public void Events_FileSort()
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.Files);
      sut.DirectorySort = PathEnumerateSort.ByName;
      sut.FileSort = PathEnumerateSort.ByName;
      TestEventHandler handtester = new TestEventHandler(sut);
      sut.BeforeDirectory += Set_FileSort;
      DoTestEnumerator(sut, "f3.xml,f4.txt,f13.txt,f113.txt,f1111.txt,f1113.txt,f1112.xml,f1121.txt,f121.txt,f21.txt,f22.txt", true);
      handtester.Validate("ROOT,D1,D11,D111,D112,D12,D1.1,D2");
    }

    private static void Set_FileSort(object sender, EnumDirectoryEventArgs args)
    {
      if (args.Directory.FileName == "D111")
        args.FileSort = PathEnumerateSort.ByExtension;
    }

    [Test]
    public void Events_ReverseDirectories()
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.Files);
      sut.DirectorySort = PathEnumerateSort.ByName;
      sut.FileSort = PathEnumerateSort.ByName;
      TestEventHandler handtester = new TestEventHandler(sut);
      sut.BeforeDirectory += Set_ReverseDirectories;
      DoTestEnumerator(sut, "f3.xml,f4.txt,f13.txt,f113.txt,f1121.txt,f1111.txt,f1112.xml,f1113.txt,f121.txt,f21.txt,f22.txt", true);
      handtester.Validate("ROOT,D1,D11,D112,D111,D12,D1.1,D2");
    }

    private static void Set_ReverseDirectories(object sender, EnumDirectoryEventArgs args)
    {
      if (args.Directory.FileName == "D11")
        args.ReverseDirectories = true;
    }

    [Test]
    public void Events_ReverseFiles()
    {
      PathEnumerableBase sut = CreateTestObject(PathEnumerateKind.Files);
      sut.DirectorySort = PathEnumerateSort.ByName;
      sut.FileSort = PathEnumerateSort.ByName;
      TestEventHandler handtester = new TestEventHandler(sut);
      sut.BeforeDirectory += Set_ReverseFiles;
      DoTestEnumerator(sut, "f4.txt,f3.xml,f13.txt,f113.txt,f1111.txt,f1112.xml,f1113.txt,f1121.txt,f121.txt,f21.txt,f22.txt", true);
      handtester.Validate("ROOT,D1,D11,D111,D112,D12,D1.1,D2");
    }

    private static void Set_ReverseFiles(object sender, EnumDirectoryEventArgs args)
    {
      if (args.Level==0)
        args.ReverseFiles = true;
    }

    #endregion
  }

  [TestFixture]
  public class AbsPathEnumerableTests : PathEnumerableTestsBase
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
      //if (RootDir.IsEmpty)
      //  throw new BugException("PathEnumerableTestsBase.SetUp has not been called");

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

    protected override PathEnumerableBase CreateTestObject(PathEnumerateKind enumerateKind)
    {
      return new AbsPathEnumerable(RootDir, enumerateKind);
    }

    protected override void DoTestEnumerator(PathEnumerableBase sut, string sNames, bool isSorted)
    {
      string[] a = DataTools.EmptyStrings;
      if (sNames.Length > 0)
        a = sNames.Split(',');

      List<string> lst = new List<string>();
      foreach (AbsPath path in (AbsPathEnumerable)sut)
        lst.Add(path.FileName);

      if (isSorted)
        CollectionAssert.AreEqual(a, lst, "Ordered enumerated names");
      else
        CollectionAssert.AreEquivalent(a, lst, "Disordered enumerated names");
    }

    #endregion
  }

  [TestFixture]
  public class RelPathEnumerableTests : PathEnumerableTestsBase
  {
    #region Конструкторы

    [Test]
    public void Constructor_2args()
    {
      RelPathEnumerable sut = new RelPathEnumerable(RootDir, PathEnumerateKind.FilesAndDirectories);

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
      RelPathEnumerable sut = new RelPathEnumerable(RootDir);

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
      Assert.Catch<ArgumentException>(delegate () { new RelPathEnumerable(AbsPath.Empty); });
    }

    #endregion

    #region GetEnumerator()

    protected override PathEnumerableBase CreateTestObject(PathEnumerateKind enumerateKind)
    {
      return new RelPathEnumerable(RootDir, enumerateKind);
    }

    protected override void DoTestEnumerator(PathEnumerableBase sut, string sNames, bool isSorted)
    {
      string[] a = DataTools.EmptyStrings;
      if (sNames.Length > 0)
        a = sNames.Split(',');

      List<string> lst = new List<string>();
      foreach (RelPath path in (RelPathEnumerable)sut)
      {
        AbsPath path2 = RootDir + path;
        lst.Add(path2.FileName);
      }

      if (isSorted)
        CollectionAssert.AreEqual(a, lst, "Ordered enumerated names");
      else
        CollectionAssert.AreEquivalent(a, lst, "Disordered enumerated names");
    }

    #endregion
  }
}
