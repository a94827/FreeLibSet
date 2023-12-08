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
  public class RelPathTests
  {
    #region Конструкторы

    [Platform("Win")]
    [TestCase(@"aaa", @"aaa", false)]
    [TestCase(@"aaa\bbb", @"aaa\bbb", false)]
    [TestCase(@"C:\WINDOWS\", @"C:\WINDOWS", true)]
    [TestCase(@"..\bbb", @"..\bbb", false)]
    public void Constructor_string_windows(string s, string wantedPath, bool wantedIsAbsPath)
    {
      RelPath sut = new RelPath(s);
      Assert.AreEqual(wantedPath, sut.Path, "Path");
      Assert.AreEqual(wantedIsAbsPath, sut.IsAbsPath, "IsAbsPath");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Platform("Linux")]
    [TestCase(@"/", @"/", true, Description = "Root dir")]
    [TestCase(@"/tmp", @"/tmp", true, Description = "Abs dir")]
    [TestCase(@"abc/def/ghi/", @"abc/def/ghi", false, Description = "Relative dir")]
    [TestCase(@"../abc.txt", @"../abc.txt", false, Description = "Relative file")]
    [TestCase(@"~", @"~", false, Description = "UserProfile")]
    [TestCase(@"~/hello.txt", @"~/hello.txt", false, Description = "File in UserProfile")]
    public void Constructor_string_linux(string s, string wantedPath, bool wantedIsAbsPath)
    {
      RelPath sut = new RelPath(s);
      Assert.AreEqual(wantedPath, sut.Path, "Path");
      Assert.AreEqual(wantedIsAbsPath, sut.IsAbsPath, "IsAbsPath");
      Assert.IsFalse(sut.IsEmpty, "IsEmpty");
    }

    [Test]
    public void Constructor_string_empty()
    {
      RelPath sut = new RelPath("");
      Assert.AreEqual("", sut.Path, "Path");
      Assert.AreEqual("", sut.QuotedPath, "QuotedPath");
      Assert.IsFalse(sut.IsAbsPath, "IsAbsPath");
      Assert.IsTrue(sut.IsEmpty, "IsEmpty");
    }

    [Platform("Win")]
    [TestCase(@"C:\WINDOWS\SYSTEM32", "AAA|BBB", @"C:\WINDOWS\SYSTEM32\AAA\BBB")]
    [TestCase(@"C:\WINDOWS\SYSTEM32", "", @"C:\WINDOWS\SYSTEM32")]
    [TestCase(@"..\AAA", "BBB|CCC", @"..\AAA\BBB\CCC")]
    [TestCase(@"", "AAA", @"AAA")]
    public void Constructor_basePath_windows(string sBasePath, string sSubNames, string wantedPath)
    {
      string[] subNames = DataTools.EmptyStrings;
      if (sSubNames.Length > 0)
        subNames = sSubNames.Split('|');
      RelPath basePath = new RelPath(sBasePath);

      RelPath sut = new RelPath(basePath, subNames);

      Assert.AreEqual(wantedPath, sut.Path);
    }

    [Platform("Linux")]
    [TestCase(@"/usr/bin", "AAA|BBB", @"/usr/bin/AAA/BBB")]
    [TestCase(@"/usr/bin", "", @"/usr/bin")]
    [TestCase(@"../AAA", "BBB|CCC", @"../AAA/BBB/CCC")]
    [TestCase(@"", "AAA", @"AAA")]
    public void Constructor_basePath_linux(string sBasePath, string sSubNames, string wantedPath)
    {
      string[] subNames = DataTools.EmptyStrings;
      if (sSubNames.Length > 0)
        subNames = sSubNames.Split('|');
      RelPath basePath = new RelPath(sBasePath);

      RelPath sut = new RelPath(basePath, subNames);

      Assert.AreEqual(wantedPath, sut.Path);
    }

    #endregion

    #region SlashedPath, QuotedPath

    [Platform("Win")]
    [TestCase(@"C:\Windows", @"C:\Windows\")]
    [TestCase(@"C:\", @"C:\")]
    [TestCase(@"", @"")]
    [TestCase(@"..", @"..\")]
    [TestCase(@"aaa\bbb", @"aaa\bbb\")]
    public void SlashedPath_windows(string s, string wantedRes)
    {
      RelPath sut = new RelPath(s);
      Assert.AreEqual(wantedRes, sut.SlashedPath);
    }

    [Platform("Linux")]
    [TestCase(@"/usr", @"/usr/")]
    [TestCase(@"/", @"/")]
    [TestCase(@"", @"")]
    [TestCase(@"..", @"../")]
    [TestCase(@"aaa/bbb", @"aaa/bbb/")]
    [TestCase(@"~", @"~/")]
    [TestCase(@"~/123", @"~/123/")]
    public void SlashedPath_linux(string s, string wantedRes)
    {
      RelPath sut = new RelPath(s);
      Assert.AreEqual(wantedRes, sut.SlashedPath);
    }

    [Platform("Win")]
    [TestCase(@"C:\Windows", @"""C:\Windows""")]
    [TestCase(@"", @"")]
    [TestCase(@"..", @"""..""")]
    [TestCase(@"aaa\bbb", @"""aaa\bbb""")]
    public void QuotedPath_windows(string s, string wantedRes)
    {
      RelPath sut = new RelPath(s);
      Assert.AreEqual(wantedRes, sut.QuotedPath);
    }

    #endregion

    #region Пребразования

    /// <summary>
    /// Используем CurDir в формате AbsPath, чтобы не думать о слэшах при добавлении подкаталога.
    /// Сам текущий каталог нужен, в основном, так как он точно существует.
    /// </summary>
    private static AbsPath CurDir { get { return new AbsPath(Environment.CurrentDirectory); } }

    [Test]
    public void Operator_RelPath_to_AbsPath_rel()
    {
      RelPath sut = new RelPath(".." + Path.DirectorySeparatorChar + "aaa");
      AbsPath res = (AbsPath)sut;

      AbsPath basePath = CurDir;
      AbsPath wantedRes = new AbsPath(basePath.ParentDir, "aaa");
      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void Operator_RelPath_to_AbsPath_abs()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      RelPath sut = new RelPath(basePath.Path);
      AbsPath res = (AbsPath)sut;

      Assert.AreEqual(basePath, res);
    }

    [Test]
    public void Operator_RelPath_to_AbsPath_empty()
    {
      RelPath sut = RelPath.Empty;
      AbsPath res = (AbsPath)sut;

      Assert.IsTrue(res.IsEmpty);
    }

    [Test]
    public void Operator_AbsPath_to_RelPath_ok()
    {
      string path = new AbsPath(CurDir,"aaa").Path;
      AbsPath sut = new AbsPath(path);
      RelPath res = (RelPath)sut;

      Assert.AreEqual(path, res.Path, "Path");
      Assert.IsTrue(res.IsAbsPath, "IsAbsPath");
    }

    [Test]
    public void Operator_AbsPath_to_RelPath_empty()
    {
      AbsPath sut = AbsPath.Empty;
      RelPath res = (RelPath)sut;

      Assert.IsTrue(res.IsEmpty, "IsEmpty");
    }

    #endregion

    #region Empty

    [Test]
    public void Empty()
    {
      Assert.IsTrue(RelPath.Empty.IsEmpty, "IsEmpty");
      Assert.IsFalse(RelPath.Empty.IsAbsPath, "IsAbsPath");
      Assert.AreEqual("", RelPath.Empty.Path, "Path");
      Assert.AreEqual("", RelPath.Empty.SlashedPath, "SlashedPath");
      Assert.AreEqual("", RelPath.Empty.QuotedPath, "QuotedPath");
    }

    #endregion

    #region ToAbsolute()

    [Test]
    public void ToAbsolute_rel_abs_1()
    {
      RelPath sut = new RelPath("bbb");
      AbsPath baseDir = new AbsPath(CurDir, "aaa");
      RelPath res = sut.ToAbsolute(baseDir);
      Assert.AreEqual(baseDir.SlashedPath + "bbb", res.Path);
    }

    public void ToAbsolute_rel_abs_2()
    {
      RelPath sut = new RelPath(".."+ Path.DirectorySeparatorChar + "bbb");
      AbsPath baseDir = new AbsPath(CurDir, "aaa");
      RelPath res = sut.ToAbsolute(baseDir);
      Assert.AreEqual(baseDir.ParentDir.SlashedPath + "bbb", res.Path);
    }

    [Test]
    public void ToAbsolute_rel_empty()
    {
      RelPath sut = new RelPath("bbb");
      RelPath res = sut.ToAbsolute(AbsPath.Empty);
      Assert.AreEqual(new AbsPath(CurDir, "bbb").Path, res.Path);
    }

    [Test]
    public void ToAbsolute_abs_abs()
    {
      RelPath sut = new RelPath(new AbsPath(CurDir, "bbb").Path);
      AbsPath baseDir = new AbsPath(CurDir, "aaa");
      RelPath res = sut.ToAbsolute(baseDir);
      Assert.AreEqual(sut.Path, res.Path);
    }

    [Test]
    [Platform("Linux")]
    public void ToAbsolute_linuxhome_abs()
    {
      RelPath sut = new RelPath("~/hello.txt");
      AbsPath baseDir = new AbsPath(CurDir, "aaa"); // no matter
      RelPath res = sut.ToAbsolute(baseDir);
      Assert.AreEqual(FileTools.UserProfileDir.SlashedPath+"hello.txt", res.Path);
    }

    [Test]
    public void ToAbsolute_abs_empty()
    {
      RelPath sut = new RelPath(new AbsPath(CurDir, "bbb").Path);
      RelPath res = sut.ToAbsolute(AbsPath.Empty);
      Assert.AreEqual(sut.Path, res.Path);
    }

    [Test]
    public void ToAbsolute_empty_abs()
    {
      AbsPath baseDir = new AbsPath(CurDir, "aaa");
      RelPath res = RelPath.Empty.ToAbsolute(baseDir);
      Assert.IsTrue(res.IsEmpty);
    }

    [Test]
    public void ToAbsolute_empty_empty()
    {
      RelPath res = RelPath.Empty.ToAbsolute(AbsPath.Empty);
      Assert.IsTrue(res.IsEmpty);
    }

    #endregion

    #region ToRelative()

    [Test]
    public void ToRelative_rel_abs()
    {
      RelPath sut = new RelPath("bbb");
      AbsPath baseDir = new AbsPath(CurDir, "aaa");
      RelPath res = sut.ToRelative(baseDir);
      Assert.AreEqual("bbb", res.Path);
    }

    [Test]
    public void ToRelative_rel_empty()
    {
      RelPath sut = new RelPath("bbb");
      RelPath res = sut.ToRelative(AbsPath.Empty);
      Assert.AreEqual("bbb", res.Path);
    }

    [Test]
    public void ToRelative_abs_abs()
    {
      RelPath sut = new RelPath(new AbsPath(CurDir, "bbb").Path);
      AbsPath baseDir = new AbsPath(CurDir, "aaa");
      RelPath res = sut.ToRelative(baseDir);
      Assert.AreEqual(".."+Path.DirectorySeparatorChar+"bbb", res.Path);
    }

    // Такое преобразование не выполняется
    //[Platform("Linux")]
    //[Test]
    //public void ToRelative_abslinuxhome_abs()
    //{
    //  RelPath sut = new RelPath(new AbsPath(FileTools.UserProfileDir, "bbb").Path);
    //  AbsPath baseDir = new AbsPath("/usr/bin"); // no matter
    //  RelPath res = sut.ToRelative(baseDir);
    //  Assert.AreEqual("~/bbb", res.Path);
    //}

    [Platform("Linux")]
    [Test]
    public void ToRelative_linuxhome_abs()
    {
      RelPath sut = new RelPath("~/bbb");
      AbsPath baseDir = new AbsPath("/usr/bin"); // no matter
      RelPath res = sut.ToRelative(baseDir);
      Assert.AreEqual("~/bbb", res.Path); // unchanged
    }

    [Test]
    public void ToRelative_abs_empty()
    {
      RelPath sut = new RelPath(new AbsPath(CurDir, "bbb").Path);
      RelPath res = sut.ToRelative(AbsPath.Empty);
      Assert.AreEqual("bbb", res.Path);
    }

    [Test]
    public void ToRelative_empty_abs()
    {
      AbsPath baseDir = new AbsPath(CurDir, "aaa");
      RelPath res = RelPath.Empty.ToRelative(baseDir);
      Assert.IsTrue(res.IsEmpty);
    }

    [Test]
    public void ToRelative_empty_empty()
    {
      RelPath res = RelPath.Empty.ToRelative(AbsPath.Empty);
      Assert.IsTrue(res.IsEmpty);
    }

    #endregion

    #region Добавление подкаталогов

    [Test]
    public void Operator_Add_RPrel_Srel()
    {
      RelPath arg1 = new RelPath(".." + Path.DirectorySeparatorChar + "aaa");
      string arg2 = ".." + Path.DirectorySeparatorChar + "bbb";
      RelPath res = arg1 + arg2;

      // Зависит от реализации Path.Combine().
      // По-хорошему, должен быть короткий вариант, но реально - длинный
      string wantedPath1 = ".." + Path.DirectorySeparatorChar + "bbb";
      string wantedPath2 = ".." + Path.DirectorySeparatorChar + "aaa" + Path.DirectorySeparatorChar +
        ".." + Path.DirectorySeparatorChar + "bbb";
      CollectionAssert.Contains(new string[] { wantedPath1, wantedPath2 }, res.Path);
    }

    [Test]
    public void Operator_Add_RPrel_Sabs()
    {
      RelPath arg1 = new RelPath(".." + Path.DirectorySeparatorChar + "aaa");
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      string arg2 = basePath.Path;
      RelPath res = arg1 + arg2;

      Assert.AreEqual(arg2, res.Path);
    }

    [Test]
    public void Operator_Add_RPrel_Sempty()
    {
      RelPath arg1 = new RelPath(".." + Path.DirectorySeparatorChar + "aaa");
      string arg2 = String.Empty;
      RelPath res = arg1 + arg2;

      string wantedPath = ".." + Path.DirectorySeparatorChar + "aaa";
      Assert.AreEqual(wantedPath, res.Path);
    }

    [Test]
    public void Operator_Add_RPrel_RPrel()
    {
      RelPath arg1 = new RelPath(".." + Path.DirectorySeparatorChar + "aaa");
      RelPath arg2 = new RelPath(".." + Path.DirectorySeparatorChar + "bbb");
      RelPath res = arg1 + arg2;

      // Зависит от реализации Path.Combine().
      // По-хорошему, должен быть короткий вариант, но реально - длинный
      string wantedPath1 = ".." + Path.DirectorySeparatorChar + "bbb";
      string wantedPath2 = ".." + Path.DirectorySeparatorChar + "aaa" + Path.DirectorySeparatorChar + 
        ".." + Path.DirectorySeparatorChar + "bbb";
      CollectionAssert.Contains(new string[] { wantedPath1, wantedPath2 }, res.Path);
    }

    [Test]
    public void Operator_Add_RPrel_RPabs()
    {
      RelPath arg1 = new RelPath(".." + Path.DirectorySeparatorChar + "aaa");
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      RelPath arg2 = new RelPath(basePath.Path);

      RelPath res = arg1 + arg2;

      Assert.AreEqual(arg2.Path, res.Path);
    }

    [Test]
    public void Operator_Add_RPrel_RPempty()
    {
      RelPath arg1 = new RelPath(".." + Path.DirectorySeparatorChar + "aaa");
      RelPath arg2 = RelPath.Empty;
      RelPath res = arg1 + arg2;

      Assert.AreEqual(arg1.Path, res.Path);
    }


    [Test]
    public void Operator_Add_RPempty_RPrel()
    {
      RelPath arg1 = RelPath.Empty;
      RelPath arg2 = new RelPath(".." + Path.DirectorySeparatorChar + "bbb");
      RelPath res = arg1 + arg2;

      Assert.AreEqual(arg2.Path, res.Path);
    }

    [Test]
    public void Operator_Add_RPempty_RPempty()
    {
      RelPath arg1 = RelPath.Empty; 
      RelPath arg2 = RelPath.Empty; 
      RelPath res = arg1 + arg2;

      Assert.IsTrue(res.IsEmpty);
    }
    #endregion

    #region Добавление к абсолютному пути

    [Test]
    public void Operator_Add_AbsPath_RelPath_abs_rel()
    {
      AbsPath arg1 = new AbsPath(".");
      RelPath arg2 = new RelPath(".." + Path.DirectorySeparatorChar + "aaa");
      AbsPath res = arg1 + arg2;

      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath wantedRes = new AbsPath(basePath.ParentDir, "aaa");
      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void Operator_Add_AbsPath_RelPath_abs_abs()
    {
      AbsPath arg1 = new AbsPath(Environment.SystemDirectory);
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      RelPath arg2 = new RelPath(basePath.Path);
      AbsPath res = arg1 + arg2; // arg1 полностью теряется

      Assert.AreEqual(basePath, res);
    }

    [Test]
    public void Operator_Add_AbsPath_RelPath_abs_empty()
    {
      AbsPath arg1 = new AbsPath(Environment.CurrentDirectory);
      RelPath arg2 = RelPath.Empty;
      AbsPath res = arg1 + arg2;

      Assert.AreEqual(arg1, res);
    }
    [Test]
    public void Operator_Add_AbsPath_RelPath_empty_rel()
    {
      AbsPath arg1 = AbsPath.Empty;
      RelPath arg2 = new RelPath(".." + Path.DirectorySeparatorChar + "aaa");
      AbsPath res = arg1 + arg2;

      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath wantedRes = new AbsPath(basePath.ParentDir, "aaa");
      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void Operator_Add_AbsPath_RelPath_empty_abs()
    {
      AbsPath arg1 = AbsPath.Empty;
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      RelPath arg2 = new RelPath(basePath.Path);
      AbsPath res = arg1 + arg2; // arg1 полностью теряется

      Assert.AreEqual(basePath, res);
    }

    [Test]
    public void Operator_Add_AbsPath_RelPath_empty_empty()
    {
      AbsPath arg1 = AbsPath.Empty;
      RelPath arg2 = RelPath.Empty;
      AbsPath res = arg1 + arg2;

      Assert.AreEqual(arg1, res);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      RelPath sut = new RelPath("aaa");
      byte[] b = SerializationTools.SerializeBinary(sut);

      RelPath res = (RelPath)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.Path, res.Path);
    }

    #endregion
  }
}
