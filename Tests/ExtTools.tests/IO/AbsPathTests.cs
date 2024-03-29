﻿using System;
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
  public class AbsPathTests
  {
    #region Конструкторы

    [Platform("Win")]
    [TestCase(@"X:\123\456", @"X:\123\456", Description = "Simple")]
    [TestCase(@"X:\123\456\", @"X:\123\456", Description = "Backslash removed")]
    [TestCase(@"W:\", @"W:\", Description = "Backslash is not removed")]
    [TestCase(@"V:", @"V:\", Description = "Backslash added")]
    [TestCase(@"""U:\123 456\789 012""", @"U:\123 456\789 012", Description = "Quotes removed")]
    [TestCase(@"\\Server\Share\", @"\\Server\Share", Description = "Share naked")] // no backslash at the end. See Path.GetRootDir()
    [TestCase(@"\\Server\Share\123.txt", @"\\Server\Share\123.txt", Description = "Share path")]
    [TestCase(@"\\Server\Share\123\456.txt", @"\\Server\Share\123\456.txt", Description = "Share path with subdir")]
    public void Constructor_string_abs_windows(string s, string wantedPath)
    {
      AbsPath sut = new AbsPath(s);
      Assert.AreEqual(wantedPath, sut.Path, "Path");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Platform("Linux")]
    [TestCase(@"/", @"/", Description = "Root dir")]
    [TestCase(@"/tmp", @"/tmp", Description = "Abs dir")]
    public void Constructor_string_abs_linux(string s, string wantedPath)
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

    [Platform("Linux")]
    [TestCase("~")]
    [TestCase("~/123/456.txt")]
    public void Constructor_string_linuxhome(string s)
    {
      string wantedPath = s.Replace ("~", FileTools.UserProfileDir.Path);
      AbsPath sut = new AbsPath(s);
      Assert.AreEqual (wantedPath, sut.Path);
    }

    [Platform("Linux")]
    [Test]
    public void Constructor_string_tilda_inside_linux()
    {
      AbsPath sut = new AbsPath("./~");
      string wantedPath = new AbsPath (Environment.CurrentDirectory).SlashedPath + "~";
      Assert.AreEqual (wantedPath, sut.Path);
    }

    [Platform("Linux")]
    [Test]
    public void Constructor_string_tilda_tilda_linux()
    {
      AbsPath sut = new AbsPath("~/~");
      string wantedPath = FileTools.UserProfileDir.SlashedPath + "~";
      Assert.AreEqual (wantedPath, sut.Path);
    }

    [Platform("Linux")]
    [Test]
    public void Constructor_string_backslash_linux()
    {
      AbsPath sut = new AbsPath(@"/\");
      string wantedPath = @"/\";
      Assert.AreEqual (wantedPath, sut.Path);
    }

    [Platform("Win")]
    [Test]
    public void Constructor_string_Uri_windows()
    {
      string s = "file:///D:/123/456/789.txt";
      AbsPath sut = new AbsPath(s);
      Assert.AreEqual(@"D:\123\456\789.txt", sut.Path, "Path");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Platform("Linux")]
    [Test]
    public void Constructor_string_Uri_linux()
    {
      string s = "file:///123/456/789.txt";
      AbsPath sut = new AbsPath(s);
      Assert.AreEqual(@"/123/456/789.txt", sut.Path, "Path");
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

    [Platform("Linux")]
    [Test]
    public void Constructor_string_exception_linux()
    {
      Assert.Catch(delegate () { new AbsPath("\0xxx"); });
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
    public void Create_string_windows(string s, string wantedPath)
    {
      AbsPath res = AbsPath.Create(s);
      Assert.AreEqual(wantedPath, res.Path);
    }

    [Platform("Linux")]
    [TestCase("/", "/")]
    [TestCase("/123/456", "/123/456")]
    [TestCase("", "")]
    [TestCase("\0xxx", "")]
    public void Create_string_linux(string s, string wantedPath)
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


    [Platform("Win")]
    [Test]
    public void Create_basePath_error_windows()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath res = AbsPath.Create(basePath, ":");
      Assert.IsTrue(res.IsEmpty);
    }

    [Platform("Linux")]
    [Test]
    public void Create_basePath_error_linux()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath res = AbsPath.Create(basePath, "\0xxx");
      Assert.IsTrue(res.IsEmpty);
    }

    #endregion

    #region SlashedPath, Uri, QuotedPath

    [Platform("Win")]
    [TestCase(@"C:\Windows", @"C:\Windows\")]
    [TestCase(@"C:\", @"C:\")]
    [TestCase(@"", @"")]
    public void SlashedPath_windows(string s, string wantedRes)
    {
      AbsPath sut = new AbsPath(s);
      Assert.AreEqual(wantedRes, sut.SlashedPath);
    }

    [Platform("Linux")]
    [TestCase(@"/123", @"/123/")]
    [TestCase(@"/", @"/")]
    [TestCase(@"", @"")]
    public void SlashedPath_linux(string s, string wantedRes)
    {
      AbsPath sut = new AbsPath(s);
      Assert.AreEqual(wantedRes, sut.SlashedPath);
    }

    [Platform("Win")]
    [TestCase(@"C:\Windows", @"file:///C:/Windows")]
    [TestCase(@"C:\", @"file:///C:/")]
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

    [Platform("Win")]
    [TestCase(@"C:\Windows", @"""C:\Windows""")]
    [TestCase(@"C:\", @"""C:\""")]
    [TestCase(@"", @"")]
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
    public void Operator_Add_empty()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath res = basePath + "";
      Assert.AreEqual(basePath.Path, res.Path);
    }

    #endregion

    #region Получение относительного каталога

    [Platform("Win")]
    [TestCase(@"C:\AAA\BBB", @"C:\AAA", @"BBB")]
    [TestCase(@"C:\AAA\BBB", @"C:\AAA\CCC", @"..\BBB")]
    [TestCase(@"C:\AAA\BBB\CCC\DDD", @"C:\AAA\BBB\DDD\EEE", @"..\..\CCC\DDD")]
    [TestCase(@"C:\AAA\BBB", @"C:\CCC", @"..\AAA\BBB")]
    [TestCase(@"C:\AAA\BBB", @"", @"C:\AAA\BBB")]
    [TestCase(@"", @"C:\AAA\BBB", @"")]
    [TestCase(@"C:\AAA\BBB", @"C:\AAA\BBB", @".")]
    [TestCase(@"C:\AAA\BBB\CCC", @"D:\AAA\BBB", @"C:\AAA\BBB\CCC")]
    public void Operator_Substract_windows(string sChild, string sBase, string sWantedRes)
    {
      AbsPath childPath = new AbsPath(sChild);
      AbsPath basePath = new AbsPath(sBase);

      RelPath res = childPath - basePath;
      RelPath wantedRes = new RelPath(sWantedRes);
      Assert.AreEqual(wantedRes, res);
    }

    [Platform("Linux")]
    [TestCase("/AAA/BBB", "/AAA", "BBB")]
    [TestCase("/AAA/BBB", "/AAA/CCC", "../BBB")]
    [TestCase("/AAA/BBB/CCC/DDD", "/AAA/BBB/DDD/EEE", "../../CCC/DDD")]
    [TestCase("/AAA/BBB", "/CCC", "../AAA/BBB")]
    [TestCase("/AAA/BBB", "", "/AAA/BBB")]
    [TestCase("", "/AAA/BBB", "")]
    [TestCase("/AAA/BBB", "/AAA/BBB", ".")]
    public void Operator_Substract_linux(string sChild, string sBase, string sWantedRes)
    {
      AbsPath childPath = new AbsPath(sChild);
      AbsPath basePath = new AbsPath(sBase);

      RelPath res = childPath - basePath;
      RelPath wantedRes = new RelPath(sWantedRes);
      Assert.AreEqual(wantedRes, res);
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

    [Platform("Win")]
    [Test]
    public void ParentDir_diskroot_windows()
    {
      AbsPath sut = new AbsPath(@"C:\");
      AbsPath res = sut.ParentDir;
      Assert.IsTrue(res.IsEmpty);
    }

    [Platform("Linux")]
    [Test]
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
    //[Platform("Win")]
    //[Test]
    //public void FileName_FileNameWithoutExtension_Extension_shareNaked_windows()
    //{
    //  AbsPath sut = new AbsPath(@"\\Server\Share");
    //  Assert.AreEqual("", sut.FileName);
    //  Assert.AreEqual("", sut.FileNameWithoutExtension);
    //  Assert.AreEqual("", sut.Extension);
    //}

    [Platform("Win")]
    [Test]
    public void FileName_FileNameWithoutExtension_Extension_rootDir_windows()
    {
      AbsPath sut = new AbsPath(@"C:\");
      Assert.AreEqual("", sut.FileName);
      Assert.AreEqual("", sut.FileNameWithoutExtension);
      Assert.AreEqual("", sut.Extension);
    }

    [Platform("Linux")]
    [Test]
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

      AbsPath res = sut.ChangeExtension(null);

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

    #region ContainsExtension

    [TestCase("readme.txt", ".txt", true)]
    [TestCase("readme.txt", ".txt2", false)]
    [TestCase("readme.txt", ".tx", false)]
    [TestCase("readme.txt", "", false)]
    [TestCase("readme", ".txt", false)]
    [TestCase("readme", "", true)]
    [TestCase("book.fb2.zip", ".zip", true)]
    [TestCase("book.fb2.zip", ".fb2.zip", true)]
    [TestCase("book.fb2.zip", ".fb2", false)]
    // Такие странные имена файлов тоже могут быть, в том числе и в Windows
    [TestCase(".fb2.zip", ".zip", true)]
    [TestCase(".fb2.zip", ".fb2.zip", false)]
    [TestCase(".fb2.zip", ".fb2", false)]
    public void ContainsExtension(string fileName, string extension, bool wantedRes)
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath sut = new AbsPath(basePath, fileName);

      bool res = sut.ContainsExtension(extension);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase(".txt")]
    [TestCase("")]
    public void ContainsExtension_empty(string extension)
    {
      AbsPath sut = AbsPath.Empty;

      bool res = sut.ContainsExtension(extension);
      Assert.IsFalse(res);
    }

    [Platform("Win")]
    [Test]
    public void ContainsExtension_windows_ignoreCase()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath sut = new AbsPath(basePath, "readme.Txt");
      Assert.IsTrue(sut.ContainsExtension(".tXt"));
    }

    #endregion

    #region Сравнение

    [Platform("Win")]
    [TestCase(@"C:\WINDOWS\SYSTEM32", @"c:\windows\system32", true)]
    [TestCase(@"C:\WINDOWS\SYSTEM32", @"c:\windows", false)]
    [TestCase(@"", @"c:\windows", false)]
    [TestCase(@"", @"c:\", false)]
    [TestCase(@"", @"", true)]
    public void Equals_windows(string sa, string sb, bool wantedRes)
    {
      DoTestEquals(sa, sb, wantedRes, "#1");
      DoTestEquals(sb, sa, wantedRes, "#2");
    }

    [Platform("Linux")]
    [TestCase(@"/aaa/bbb", @"/aaa/bbb", true)]
    [TestCase(@"/aaa/bbb", @"/Aaa/bbb", false)]
    [TestCase(@"", @"/aaa/bbb", false)]
    [TestCase(@"", @"", true)]
    public void Equals_linux(string sa, string sb, bool wantedRes)
    {
      DoTestEquals(sa, sb, wantedRes, "#1");
      DoTestEquals(sb, sa, wantedRes, "#2");
    }

    private static void DoTestEquals(string sa, string sb, bool wantedRes, string messagePrefix)
    {
      AbsPath a = new AbsPath(sa);
      AbsPath b = new AbsPath(sb);

      Assert.AreEqual(wantedRes, a == b, messagePrefix + ", ==");
      Assert.AreEqual(!wantedRes, a != b, messagePrefix + ", !=");
      Assert.AreEqual(wantedRes, a.Equals(b), messagePrefix + ", Equals(AbsPath)");
      Assert.AreEqual(wantedRes, a.Equals((object)b), messagePrefix + ", Equals(object)");

      if (wantedRes)
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode(), "GetHashCode()");
    }

    #endregion

    #region Starts/EndsWith()

    #region StartsWith()

    [Platform("Win")]
    [TestCase(@"C:\WINDOWS\SYSTEM32", @"c:\windows\system32", true)]
    [TestCase(@"C:\WINDOWS\SYSTEM32", @"c:\windows", true)]
    [TestCase(@"C:\WINDOWS", @"C:\WINDOWS\SYSTEM32", false)]
    [TestCase(@"C:\WINDOWS123\SYSTEM32", @"C:\WINDOWS", false)]
    [TestCase(@"C:\WINDOWS123", @"C:\WINDOWS", false)]
    [TestCase(@"", @"c:\windows", false)]
    [TestCase(@"c:\windows", @"c:\", true)]
    [TestCase(@"c:\windows", @"", true)]
    [TestCase(@"", @"", false)]
    public void StartsWith_windows(string ssut, string spath, bool wantedRes)
    {
      DoTestStartsWith(ssut, spath, wantedRes);
    }

    [Platform("Linux")]
    [TestCase(@"/aaa/bbb", @"/aaa/bbb", true)]
    [TestCase(@"/aaa/bbb", @"/aaa", true)]
    [TestCase(@"/AAA/bbb", @"/aaa", false)]
    [TestCase(@"/aaa", @"/aaa/bbb", false)]
    [TestCase(@"/aaa123/bbb", @"/aaa", false)]
    [TestCase(@"/aaa123", @"/aaa", false)]
    [TestCase(@"/aaa", @"", true)]
    [TestCase(@"", @"/aaa", false)]
    [TestCase(@"", @"", false)]
    public void StartsWith_linux(string ssut, string spath, bool wantedRes)
    {
      DoTestStartsWith(ssut, spath, wantedRes);
    }

    private static void DoTestStartsWith(string ssut, string spath, bool wantedRes)
    {
      AbsPath sut = new AbsPath(ssut);
      AbsPath path = new AbsPath(spath);

      Assert.AreEqual(wantedRes, sut.StartsWith(path));
    }

    #endregion

    #region EndsWith()

    [Platform("Win")]
    [TestCase(@"C:\aaa\bbb\ccc", "ccc", true)]
    [TestCase(@"C:\aaa\bbb\ccc", "bbb|ccc", true)]
    [TestCase(@"C:\aaa\bbb\ccc", "aaa|bbb|ccc", true)]
    [TestCase(@"C:\aaa\bbb\ccc", "AAA|BBB|CCC", true)]
    [TestCase(@"C:\aaa\bbb\ccc", "bbb", false)]
    [TestCase(@"C:\aaa\bbb\ccc", "aaa", false)]
    [TestCase(@"C:\aaa\bbb\ccc", "cc", false)]
    [TestCase(@"C:\aaa\bbb\ccc", "cccc", false)]
    [TestCase(@"C:\aaa\bbb\ccc", "bb|ccc", false)]
    [TestCase(@"C:\aaa\bbb\ccc", "bbbb|ccc", false)]
    [TestCase(@"C:\aaa\bbb\ccc", "aa|bbb|ccc", false)]
    [TestCase(@"C:\aaa\bbb\ccc", "aaaa|bbb|ccc", false)]
    [TestCase(@"C:\aaa\bbb\ccc", "", true)]
    [TestCase(@"C:\", "", true)]
    [TestCase(@"C:\", "C", false)]
    [TestCase(@"", "", false)]
    [TestCase(@"", "aaa", false)]
    public void EndsWith_windows(string ssut, string sRelParts, bool wantedRes)
    {
      DoTestEndsWith(ssut, sRelParts, wantedRes);
    }

    [Platform("Linux")]
    [TestCase(@"/aaa/bbb/ccc", "ccc", true)]
    [TestCase(@"/aaa/bbb/ccc", "bbb|ccc", true)]
    [TestCase(@"/aaa/bbb/ccc", "aaa|bbb|ccc", true)]
    [TestCase(@"/aaa/bbb/ccc", "AAA|BBB|CCC", false)]
    [TestCase(@"/aaa/bbb/ccc", "bbb", false)]
    [TestCase(@"/aaa/bbb/ccc", "aaa", false)]
    [TestCase(@"/aaa/bbb/ccc", "cc", false)]
    [TestCase(@"/aaa/bbb/ccc", "cccc", false)]
    [TestCase(@"/aaa/bbb/ccc", "bb|ccc", false)]
    [TestCase(@"/aaa/bbb/ccc", "bbbb|ccc", false)]
    [TestCase(@"/aaa/bbb/ccc", "aa|bbb|ccc", false)]
    [TestCase(@"/aaa/bbb/ccc", "aaaa|bbb|ccc", false)]
    [TestCase(@"/aaa/bbb/ccc", "", true)]
    [TestCase(@"", "", false)]
    [TestCase(@"", "aaa", false)]
    public void EndsWith_linux(string ssut, string sRelParts, bool wantedRes)
    {
      DoTestEndsWith(ssut, sRelParts, wantedRes);
    }

    private static void DoTestEndsWith(string ssut, string sRelParts, bool wantedRes)
    {
      AbsPath sut = new AbsPath(ssut);
      string[] relParts = DataTools.EmptyStrings;
      if (sRelParts.Length > 0)
        relParts = sRelParts.Split('|');

      Assert.AreEqual(wantedRes, sut.EndsWith(relParts));
    }

    #endregion

    #region EndsWithIgnoreCase()

    [TestCase("BBB", true)]
    [TestCase("bbb", true)]
    [TestCase("AAA|BBB", true)]
    [TestCase("aaa|bbb", true)]
    [TestCase("AAA", false)]
    [TestCase("BB", false)]
    [TestCase("BBBB", false)]
    [TestCase("", true)]
    public void EndsWithIgnoreCase(string sRelParts, bool wantedRes)
    {

      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath sut = new AbsPath(basePath, "AAA", "BBB");

      string[] relParts = DataTools.EmptyStrings;
      if (sRelParts.Length > 0)
        relParts = sRelParts.Split('|');

      Assert.AreEqual(wantedRes, sut.EndsWithIgnoreCase(relParts));
    }


    #endregion

    #endregion

    // IsNetwork - устаревшее свойство

    #region Split() / Join()

    [Platform("Win")]
    [TestCase(@"C:\123\456.txt", @"C:\|123|456.txt")]
    [TestCase(@"C:\123.txt", @"C:\|123.txt")]
    [TestCase(@"C:\", @"C:\")]
    [TestCase(@"\\Server1\Share2\123.txt", @"\\Server1\Share2|123.txt")]
    [TestCase(@"\\Server1\Share2\Dir3\123.txt", @"\\Server1\Share2|Dir3|123.txt")]
    [TestCase(@"\\Server1\Share2\Dir3\Dir4\123.txt", @"\\Server1\Share2|Dir3|Dir4|123.txt")]
    public void Split_Join_windows(string path, string sWantedParts)
    {
      DoSplit_Join(path, sWantedParts);
    }

    [Platform("Linux")]
    [TestCase(@"/123/456.txt", @"/|123|456.txt")]
    [TestCase(@"/123.txt", @"/|123.txt")]
    [TestCase(@"/", @"/")]
    public void Split_Join_linux(string path, string sWantedParts)
    {
      DoSplit_Join(path, sWantedParts);
    }

    private static void DoSplit_Join(string path, string sWantedParts)
    {
      string[] wantedParts = sWantedParts.Split('|');

      AbsPath sut1 = new AbsPath(path);
      string[] parts1 = sut1.Split();
      CollectionAssert.AreEqual(wantedParts, parts1, "Split()");

      AbsPath res2 = AbsPath.Join(parts1);
      Assert.AreEqual(sut1, res2, "Join()");
    }

    [Test]
    public void Split_emptyPath()
    {
      string[] res = AbsPath.Empty.Split();
      CollectionAssert.AreEqual(DataTools.EmptyStrings, res);
    }

    [Test]
    public void Join_emptyStrings()
    {
      AbsPath res = AbsPath.Join(DataTools.EmptyStrings);
      Assert.IsTrue(res.IsEmpty);
    }

    [Test]
    public void Join_null()
    {
      string[] arg = null;
      AbsPath res = AbsPath.Join(arg);
      Assert.IsTrue(res.IsEmpty);
    }

    #endregion

    #region Operator &

    [Platform("Win")]
    [TestCase(@"C:\AAA\BBB\CCC", @"C:\AAA\bbb\DDD", @"C:\AAA\BBB")]
    [TestCase(@"C:\AAA\BBB\CCC", @"C:\AAA\BBB\CCC", @"C:\AAA\BBB\CCC")]
    [TestCase(@"C:\AAA\BBB\CCC", @"C:\AAA\BBB\CC", @"C:\AAA\BBB")]
    [TestCase(@"C:\AAA\BBB\CCC", @"C:\AAA\BBB\CCC.txt", @"C:\AAA\BBB")]
    [TestCase(@"C:\AAA\BBB\CCC", @"C:\AAA\BBB\CCC\DDD", @"C:\AAA\BBB\CCC")]
    [TestCase(@"C:\AAA\BBB\CCC", @"D:\AAA\BBB\CCC", @"")]
    [TestCase(@"C:\AAA", @"C:\BBB", @"C:\")]
    [TestCase(@"C:\AAA", @"", @"")]
    [TestCase(@"", @"", @"")]
    [TestCase(@"\\Server1\Share1\123.txt", @"\\Server1\Share1\456.txt", @"\\Server1\Share1")]
    [TestCase(@"\\Server1\Share1\123.txt", @"\\Server1\Share2\123.txt", @"")]
    public void Operator_And_windows(string sPath1, string sPath2, string sWantedRes)
    {
      AbsPath path1 = new AbsPath(sPath1);
      AbsPath path2 = new AbsPath(sPath2);
      AbsPath wantedRes = new AbsPath(sWantedRes);

      AbsPath res1 = path1 & path2;
      Assert.AreEqual(wantedRes, res1, "#1");
      AbsPath res2 = path2 & path1;
      Assert.AreEqual(wantedRes, res2, "#2");
    }

    [Platform("Linux")]
    [TestCase(@"/AAA/BBB/CCC", @"/AAA/BBB/DDD", @"/AAA/BBB")]
    [TestCase(@"/AAA/BBB/CCC", @"/AAA/bbb/DDD", @"/AAA")]
    [TestCase(@"/AAA/BBB/CCC", @"/AAA/BBB/CCC", @"/AAA/BBB/CCC")]
    [TestCase(@"/AAA/BBB/CCC", @"/AAA/BBB/CC", @"/AAA/BBB")]
    [TestCase(@"/AAA/BBB/CCC", @"/AAA/BBB/CCC.txt", @"/AAA/BBB")]
    [TestCase(@"/AAA/BBB/CCC", @"/AAA/BBB/CCC/DDD", @"/AAA/BBB/CCC")]
    [TestCase(@"/AAA", @"/BBB", @"/")]
    [TestCase(@"/AAA/BBB/CCC", @"DDD/BBB/CCC", @"/")]
    [TestCase(@"/AAA", @"", @"")]
    [TestCase(@"", @"", @"")]
    public void Operator_And_linux(string sPath1, string sPath2, string sWantedRes)
    {
      AbsPath path1 = new AbsPath(sPath1);
      AbsPath path2 = new AbsPath(sPath2);
      AbsPath wantedRes = new AbsPath(sWantedRes);

      AbsPath res1 = path1 & path2;
      Assert.AreEqual(wantedRes, res1, "#1");
      AbsPath res2 = path2 & path1;
      Assert.AreEqual(wantedRes, res2, "#2");
    }

    #endregion

    #region Empty

    [Test]
    public void Empty()
    {
      Assert.IsTrue(AbsPath.Empty.IsEmpty, "IsEmpty");
      Assert.AreEqual(String.Empty, AbsPath.Empty.Path, "Path");
      Assert.AreEqual("", AbsPath.Empty.SlashedPath, "SlashedPath");
      Assert.AreEqual("", AbsPath.Empty.QuotedPath, "QuotedPath");
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      AbsPath sut = new AbsPath(new AbsPath(Environment.CurrentDirectory));
      byte[] b = SerializationTools.SerializeBinary(sut);

      AbsPath res = (AbsPath)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.Path, res.Path);
    }

    #endregion
  }
}
