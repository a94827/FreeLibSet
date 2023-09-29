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
    public void Constructor_string_abs_linux(string s, string wantedPath, bool wantedIsAbsPath)
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

    [Test]
    public void Operator_AbsPath_rel()
    {
      RelPath sut = new RelPath(".." + Path.DirectorySeparatorChar + "aaa");
      AbsPath res = sut;

      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath wantedRes = new AbsPath(basePath.ParentDir, "aaa");
      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void Operator_AbsPath_abs()
    {
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      RelPath sut = new RelPath(basePath.Path);
      AbsPath res = sut;

      Assert.AreEqual(basePath, res);
    }

    [Test]
    public void Operator_AbsPath_empty()
    {
      RelPath sut = new RelPath("");
      AbsPath res = sut;

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
      RelPath arg2 = new RelPath("");
      RelPath res = arg1 + arg2;

      Assert.AreEqual(arg1.Path, res.Path);
    }


    [Test]
    public void Operator_Add_RPempty_RPrel()
    {
      RelPath arg1 = new RelPath("");
      RelPath arg2 = new RelPath(".." + Path.DirectorySeparatorChar + "bbb");
      RelPath res = arg1 + arg2;

      Assert.AreEqual(arg2.Path, res.Path);
    }

    [Test]
    public void Operator_Add_RPempty_RPempty()
    {
      RelPath arg1 = new RelPath("");
      RelPath arg2 = new RelPath("");
      RelPath res = arg1 + arg2;

      Assert.IsTrue(res.IsEmpty);
    }
    #endregion

    #region Добавление к абсолютному пути

    [Test]
    public void Operator_Add_AbsPath_RelPath_rel()
    {
      AbsPath arg1 = new AbsPath(".");
      RelPath arg2 = new RelPath(".." + Path.DirectorySeparatorChar + "aaa");
      AbsPath res = arg1 + arg2;

      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      AbsPath wantedRes = new AbsPath(basePath.ParentDir, "aaa");
      Assert.AreEqual(wantedRes, res);
    }


    [Test]
    public void Operator_Add_AbsPath_RelPath_abs()
    {
      AbsPath arg1 = new AbsPath(Environment.SystemDirectory);
      AbsPath basePath = new AbsPath(Environment.CurrentDirectory);
      RelPath arg2 = new RelPath(basePath.Path);
      AbsPath res = arg1 + arg2; // arg1 полностью теряется

      Assert.AreEqual(basePath, res);
    }


    [Test]
    public void Operator_Add_AbsPath_RelPath_empty()
    {
      AbsPath arg1 = new AbsPath(Environment.CurrentDirectory);
      RelPath arg2 = new RelPath("");
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
