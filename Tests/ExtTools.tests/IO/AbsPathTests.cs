using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.IO;
using System.IO;
using FreeLibSet.Core;

namespace ExtTools_tests.IO
{
  [TestFixture]
  public class AbsPathTests
  {
    #region Конструкторы

    [Platform("Win")]
    [TestCase(@"X:\123\456", @"X:\123\456", Description = "Simple")]
    [TestCase(@"X:\123\456\", @"X:\123\456", Description = "Backslash removed")]
    [TestCase(@"W:\", @"W:\", Description = "Backslash is not removed")]
    [TestCase(@"V:", @"V:\", Description = "Backslash added")]
    [TestCase(@"""U:\123 456\789 012""", @"U:\123 456\789 012", Description = "Quotes removed")]
    [TestCase(@"\\Server\Share\123\456.txt", @"\\Server\Share\123\456.txt", Description = "Share path")]
    // В текущей реализации завершающий backslash не добавляется
    //[TestCase(@"\\Server\Share\", @"\\Server\Share\", Description = "Share naked")]
    public void Constructor_string_abs_windows(string s, string wantedPath)
    {
      AbsPath sut = new AbsPath(s);
      Assert.AreEqual(wantedPath, sut.Path, "Path");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [TestCase("123.txt")]
    [TestCase("123/456.txt")]
    [TestCase("../123.txt")]
    [TestCase("../123/456.txt")]
    public void Constructor_string_rel(string s)
    {
      s = s.Replace('/', Path.DirectorySeparatorChar);
      AbsPath sut = new AbsPath(s);

      string wantedRes = Path.GetFullPath(s);
      Assert.AreEqual(wantedRes, sut.Path, "Path");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Platform("Win")]
    [Test]
    public void Constructor_string_Uri()
    {
      string s = "file:///D:/123/456/789.txt";
      AbsPath sut = new AbsPath(s);
      Assert.AreEqual(@"D:\123\456\789.txt", sut.Path, "Path");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Test]
    public void Constructor_string_empty()
    {
      AbsPath sut = new AbsPath(String.Empty);
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
      Assert.AreEqual("", sut.Path, "Path");
    }

    [Platform("Win")]
    [Test]
    public void Constructor_string_exception_windows()
    {
      Assert.Catch(delegate () { new AbsPath(@"\:"); });
    }

    [TestCase("123.txt")]
    [TestCase("123/456.txt")]
    [TestCase("123/456/789.txt")]
    public void Constructor_basePath(string sSubNames)
    {
      sSubNames = sSubNames.Replace('/', Path.DirectorySeparatorChar);
      string[] subNames = sSubNames.Split(Path.DirectorySeparatorChar);

      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      Assert.IsFalse(basePath.IsEmpty, "BasePath.IsEmpty");

      AbsPath sut = new AbsPath(basePath, subNames);

      string wantedPath = Path.Combine(basePath.Path, sSubNames);
      Assert.AreEqual(wantedPath, sut.Path, "Path");
    }

    #endregion

    #region Create()

    [Platform("Win")]
    [TestCase(@"D:\123", @"D:\123")]
    [TestCase("", "")]
    [TestCase(@":\", "")]
    public void Create_string(string s, string wantedPath)
    {
      AbsPath res = AbsPath.Create(s);
      Assert.AreEqual(wantedPath, res.Path);
    }

    [Test]
    public void Create_basePath_normal()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath res = AbsPath.Create(basePath, "123", "456");
      Assert.IsFalse(res.IsEmpty, "IsEmpty");
      string wantedPath = Path.Combine(Path.Combine(basePath.Path, "123"), "456");
      Assert.AreEqual(wantedPath, res.Path, "Path");
    }


    [Test]
    [Platform("Win")]
    public void Create_basePath_error()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath res = AbsPath.Create(basePath, ":");
      Assert.IsTrue(res.IsEmpty);
    }

    #endregion

    #region SlashedPath, Uri, QuotedPath

    [TestCase(@"C:\Windows", @"C:\Windows\")]
    [TestCase(@"C:\", @"C:\")]
    [TestCase(@"", @"")]
    [Platform("Win")]
    public void SlashedPath_windows(string s, string wantedRes)
    {
      AbsPath sut = new AbsPath(s);
      Assert.AreEqual(wantedRes, sut.SlashedPath);
    }

    [TestCase(@"C:\Windows", @"file:///C:/Windows")]
    [TestCase(@"C:\", @"file:///C:/")]
    [Platform("Win")]
    public void Uri_windows(string s, string wantedRes)
    {
      AbsPath sut = new AbsPath(s);
      Assert.AreEqual(new Uri(wantedRes), sut.Uri, "Uri");
      Assert.AreEqual(wantedRes, sut.UriString, "UriString");
    }

    [Test]
    public void Uri_empty()
    {
      AbsPath sut = new AbsPath("");
      Assert.IsNull(sut.Uri, "Uri");
      Assert.AreEqual(String.Empty, sut.UriString, "UriString");
    }

    [TestCase(@"C:\Windows", @"""C:\Windows""")]
    [TestCase(@"C:\", @"""C:\""")]
    [TestCase(@"", @"")]
    [Platform("Win")]
    public void QuotedPath_windows(string s, string wantedRes)
    {
      AbsPath sut = new AbsPath(s);
      Assert.AreEqual(wantedRes, sut.QuotedPath);
    }

    #endregion

    #region Добавление подкаталога

    [Test]
    public void Operator_Add()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath res = basePath + "123";
      Assert.AreEqual(basePath.Path + Path.DirectorySeparatorChar + "123", res.Path);
    }

    [Test]
    public void Operator_empty()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath res = basePath + "";
      Assert.AreEqual(basePath.Path, res.Path);
    }

    #endregion

    #region ParentDir, RootDir

    [Test]
    public void ParentDir()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath sut = new AbsPath(basePath, "123");
      AbsPath res = sut.ParentDir;
      Assert.AreEqual(basePath.Path, res.Path);
    }

    [Test]
    public void ParentDir_empty()
    {
      AbsPath sut = new AbsPath("");
      AbsPath res = sut.ParentDir;
      Assert.AreEqual(sut, res);
    }

    [Test]
    [Platform("Win")]
    public void ParentDir_diskroot_windows()
    {
      AbsPath sut = new AbsPath(@"C:\");
      AbsPath res = sut.ParentDir;
      Assert.IsTrue(res.IsEmpty);
    }

    [Test]
    [Platform("Linux")]
    public void ParentDir_fileroot_linux()
    {
      AbsPath sut = new AbsPath("/");
      AbsPath res = sut.ParentDir;
      Assert.IsTrue(res.IsEmpty);
    }

    // Проверяем SlashedPath, чтобы устранить неоднозначность реализации backslash для путей \\Server\share\
    [Platform("Win")]
    [TestCase(@"C:\Windows\System32", @"C:\")]
    [TestCase(@"C:\", @"C:\")]
    [TestCase(@"\\Server\share\abc\def.txt", @"\\Server\share\")]
    [TestCase(@"\\Server\share\", @"\\Server\share\")]
    public void RootDir_windows(string s, string wantedSlashedPath)
    {
      AbsPath sut = new AbsPath(s);
      Assert.AreEqual(wantedSlashedPath, sut.RootDir.SlashedPath);
    }

    [Test]
    public void RootDir_empty()
    {
      AbsPath sut = AbsPath.Empty;
      Assert.IsTrue(sut.RootDir.IsEmpty);
    }

    #endregion

    #region Извлечение частей имени

    [Test]
    public void FileName_FileNameWithoutExtension_Extension_all()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath sut = new AbsPath(basePath, "readme.txt");
      Assert.AreEqual("readme.txt", sut.FileName);
      Assert.AreEqual("readme", sut.FileNameWithoutExtension);
      Assert.AreEqual(".txt", sut.Extension);
    }

    [Test]
    public void FileName_FileNameWithoutExtension_Extension_noExtension()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath sut = new AbsPath(basePath, "readme");
      Assert.AreEqual("readme", sut.FileName);
      Assert.AreEqual("readme", sut.FileNameWithoutExtension);
      Assert.AreEqual("", sut.Extension);
    }

    [Test]
    public void FileName_FileNameWithoutExtension_Extension_empty()
    {
      AbsPath sut = new AbsPath("");
      Assert.AreEqual("", sut.FileName);
      Assert.AreEqual("", sut.FileNameWithoutExtension);
      Assert.AreEqual("", sut.Extension);
    }

    // Пока нет проверок
    //[Test]
    //[Platform("Win")]
    //public void FileName_FileNameWithoutExtension_Extension_shareNaked_windows()
    //{
    //  AbsPath sut = new AbsPath(@"\\Server\Share");
    //  Assert.AreEqual("", sut.FileName);
    //  Assert.AreEqual("", sut.FileNameWithoutExtension);
    //  Assert.AreEqual("", sut.Extension);
    //}

    [Test]
    [Platform("Win")]
    public void FileName_FileNameWithoutExtension_Extension_rootDir_windows()
    {
      AbsPath sut = new AbsPath(@"C:\");
      Assert.AreEqual("", sut.FileName);
      Assert.AreEqual("", sut.FileNameWithoutExtension);
      Assert.AreEqual("", sut.Extension);
    }

    [Test]
    [Platform("Linux")]
    public void FileName_FileNameWithoutExtension_Extension_fileRoot_linux()
    {
      AbsPath sut = new AbsPath("/");
      Assert.AreEqual("", sut.FileName);
      Assert.AreEqual("", sut.FileNameWithoutExtension);
      Assert.AreEqual("", sut.Extension);
    }

    #endregion

    #region ChangeExtension

    [Test]
    public void ChangeExtension_normal()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath sut = new AbsPath(basePath, "readme.txt");

      AbsPath res = sut.ChangeExtension(".html");

      string wantedPath = sut.Path.Substring(0, sut.Path.Length - 3) + "html";
      Assert.AreEqual(wantedPath, res.Path);
    }

    [Test]
    public void ChangeExtension_noExtension()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath sut = new AbsPath(basePath, "readme");

      AbsPath res = sut.ChangeExtension(".html");

      string wantedPath = sut.Path + ".html";
      Assert.AreEqual(wantedPath, res.Path);
    }

    [Test]
    public void ChangeExtension_removeExtension()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath sut = new AbsPath(basePath, "readme.txt");

      AbsPath res = sut.ChangeExtension("");

      string wantedPath = sut.Path.Substring(0, sut.Path.Length - 4);
      Assert.AreEqual(wantedPath, res.Path);
    }


    [Test]
    public void ChangeExtension_exception()
    {
      AbsPath sut1 = new AbsPath("");
      Assert.Catch(delegate () { sut1.ChangeExtension(".txt"); }, "Empty");

      // Такой проверки пока нет
      //AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      //AbsPath sut2 = basePath.RootDir;
      //Assert.Catch(delegate () { sut2.ChangeExtension(".txt"); }, "RootDir");
    }

    #endregion

    // TODO: Остальные методы
  }
}
