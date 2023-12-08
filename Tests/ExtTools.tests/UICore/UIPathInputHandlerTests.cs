using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.UICore;
using FreeLibSet.DependedValues;
using FreeLibSet.Core;
using FreeLibSet.IO;
using System.IO;

namespace ExtTools_tests.UICore
{
  [TestFixture]
  public class UIPathInputHandlerTests
  {
    #region Конструктор

    private static readonly AbsPath CurDir = new AbsPath(Environment.CurrentDirectory);

    private static readonly AbsPath SomeAbsPath = CreateSomeAbsPath();
    private static AbsPath CreateSomeAbsPath()
    {
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT: return new AbsPath(@"C:\AAA\BBB");
        case PlatformID.Unix: return new AbsPath(@"/AAA/BBB");
        default:
          throw new NotImplementedException();
      }
    }

    private static readonly string InvalidPathStr = new string(Path.GetInvalidPathChars());
//    private static readonly string InvalidPathStr = CreateInvalidPathStr();
//    private static string CreateInvalidPathStr()
//    {
//      switch (Environment.OSVersion.Platform)
//      {
//        case PlatformID.Win32NT: return ":::";
//        case PlatformID.Unix: return "\n";
//        default: throw new NotImplementedException();
//      }
//    }

    [Test]
    public void Constructor_abs()
    {
      string testPath = new AbsPath(CurDir, "aaa").Path;
      DepInput<string> textEx = new DepInput<string>();
      textEx.Value = testPath;
      UIPathInputHandler sut = new UIPathInputHandler(textEx);

      Assert.AreEqual(testPath, textEx.Value, "TextEx");
      Assert.AreEqual(testPath, sut.Path.Path, "Path");
      Assert.AreEqual(testPath, sut.RelPath.Path, "RelPath");
      Assert.IsTrue(sut.IsNotEmptyEx.Value, "IsNotEmpty");
      Assert.AreEqual(CurDir, sut.BasePath, "BasePath");
      Assert.IsFalse(sut.UseSlashedPath, "UseSlashedPath");
    }

    [Test]
    public void Constructor_rel()
    {
      string testPath = "aaa" + Path.DirectorySeparatorChar + "bbb";
      DepInput<string> textEx = new DepInput<string>();
      textEx.Value = testPath;
      UIPathInputHandler sut = new UIPathInputHandler(textEx);

      Assert.AreEqual(testPath, textEx.Value, "TextEx");
      Assert.AreEqual(new AbsPath(CurDir, "aaa", "bbb").Path, sut.Path.Path, "Path");
      Assert.AreEqual(testPath, sut.RelPath.Path, "RelPath");
      Assert.IsTrue(sut.IsNotEmptyEx.Value, "IsNotEmpty");
      Assert.AreEqual(CurDir, sut.BasePath, "BasePath");
      Assert.IsFalse(sut.UseSlashedPath, "UseSlashedPath");
    }

    [Test]
    public void Constructor_empty()
    {
      DepInput<string> textEx = new DepInput<string>();
      textEx.Value = String.Empty;
      UIPathInputHandler sut = new UIPathInputHandler(textEx);

      Assert.AreEqual(String.Empty, textEx.Value, "TextEx");
      Assert.IsTrue(sut.Path.IsEmpty, "Path");
      Assert.IsTrue(sut.RelPath.IsEmpty, "RelPath");
      Assert.IsFalse(sut.IsNotEmptyEx.Value, "IsNotEmpty");
      Assert.AreEqual(CurDir, sut.BasePath, "BasePath");
      Assert.IsFalse(sut.UseSlashedPath, "UseSlashedPath");
    }

    [Test]
    public void Constructor_error()
    {
      DepInput<string> textEx = new DepInput<string>();
      textEx.Value = InvalidPathStr;
      UIPathInputHandler sut = new UIPathInputHandler(textEx);

      Assert.AreEqual(InvalidPathStr, textEx.Value, "TextEx");
      Assert.IsTrue(sut.Path.IsEmpty, "Path");
      Assert.IsTrue(sut.RelPath.IsEmpty, "RelPath");
      Assert.IsFalse(sut.IsNotEmptyEx.Value, "IsNotEmpty");
      Assert.AreEqual(CurDir, sut.BasePath, "BasePath");
      Assert.IsFalse(sut.UseSlashedPath, "UseSlashedPath");
    }

    #endregion

    #region BasePath

    [Test]
    public void BasePath_rel()
    {
      DepInput<string> textEx = new DepInput<string>();
      textEx.Value = "aaa";
      UIPathInputHandler sut = new UIPathInputHandler(textEx);
      sut.BasePath = SomeAbsPath;

      Assert.AreEqual(SomeAbsPath.Path, sut.BasePath.Path, "BasePath");
      Assert.AreEqual("aaa", textEx.Value, "TextEx");
      Assert.AreEqual(new AbsPath(SomeAbsPath, "aaa").Path, sut.Path.Path, "Path");
      Assert.AreEqual("aaa", sut.RelPath.Path, "RelPath");
    }

    [Test]
    public void BasePath_abs()
    {
      DepInput<string> textEx = new DepInput<string>();
      string testPath = new AbsPath(CurDir, "aaa").Path;
      textEx.Value = testPath;
      UIPathInputHandler sut = new UIPathInputHandler(textEx);
      sut.BasePath = SomeAbsPath;

      Assert.AreEqual(SomeAbsPath.Path, sut.BasePath.Path, "BasePath");
      Assert.AreEqual(testPath, textEx.Value, "TextEx");
      Assert.AreEqual(testPath, sut.Path.Path, "Path");
      Assert.AreEqual(testPath, sut.RelPath.Path, "RelPath");
    }

    #endregion

    #region UseSlashedPath

    [Test]
    public void UseSlashedPath_rel()
    {
      DepInput<string> textEx = new DepInput<string>();
      textEx.Value = "aaa";
      UIPathInputHandler sut = new UIPathInputHandler(textEx);
      sut.UseSlashedPath = true;

      Assert.IsTrue(sut.UseSlashedPath, "UseSlashedPath");
      Assert.AreEqual("aaa" + Path.DirectorySeparatorChar, textEx.Value, "TextEx");
      Assert.AreEqual(new AbsPath(CurDir, "aaa").Path, sut.Path.Path, "Path");
      Assert.AreEqual("aaa", sut.RelPath.Path, "RelPath");
    }

    [Test]
    public void UseSlashedPath_abs()
    {
      DepInput<string> textEx = new DepInput<string>();
      string testPath = new AbsPath(CurDir, "aaa").Path;
      textEx.Value = testPath;
      UIPathInputHandler sut = new UIPathInputHandler(textEx);
      sut.UseSlashedPath = true;

      Assert.IsTrue(sut.UseSlashedPath, "UseSlashedPath");
      Assert.AreEqual(testPath + Path.DirectorySeparatorChar, textEx.Value, "TextEx");
      Assert.AreEqual(testPath, sut.Path.Path, "Path");
      Assert.AreEqual(testPath, sut.RelPath.Path, "RelPath");
    }

    [Test]
    public void UseSlashedPath_empty()
    {
      DepInput<string> textEx = new DepInput<string>();
      textEx.Value = String.Empty;
      UIPathInputHandler sut = new UIPathInputHandler(textEx);
      sut.UseSlashedPath = true;

      Assert.IsTrue(sut.UseSlashedPath, "UseSlashedPath");
      Assert.AreEqual(String.Empty, textEx.Value, "TextEx");
      Assert.IsTrue(sut.Path.IsEmpty, "Path");
      Assert.IsTrue(sut.RelPath.IsEmpty, "RelPath");
      Assert.IsFalse(sut.IsNotEmptyEx.Value, "IsNotEmpty");
    }

    #endregion

    #region Text

    [Test]
    public void Text_rel([Values(false, true)] bool useStartText,
      [Values(false, true)]bool useSlashedPath)
    {
      DepInput<string> textEx = new DepInput<string>();
      if (useStartText)
        textEx.Value = SomeAbsPath.Path;
      else
        textEx.Value = String.Empty;
      UIPathInputHandler sut = new UIPathInputHandler(textEx);
      sut.UseSlashedPath = useSlashedPath;

      textEx.Value = "aaa";

      Assert.AreEqual("aaa", textEx.Value, "TextEx"); // slash must not be added
      Assert.AreEqual(new AbsPath(CurDir, "aaa").Path, sut.Path.Path, "Path");
      Assert.AreEqual("aaa", sut.RelPath.Path, "RelPath");
      Assert.IsTrue(sut.IsNotEmptyEx.Value, "IsNotEmpty");
    }

    [Platform("Linux")]
    [Test]
    public void Text_rel_linux()
    {
      DepInput<string> textEx = new DepInput<string>();
      UIPathInputHandler sut = new UIPathInputHandler(textEx);

      textEx.Value = "~/hello.txt";

      Assert.AreEqual("~/hello.txt", textEx.Value, "TextEx"); 
      Assert.AreEqual(new AbsPath(FileTools.UserProfileDir, "hello.txt").Path, sut.Path.Path, "Path");
      Assert.AreEqual("~/hello.txt", sut.RelPath.Path, "RelPath");
      Assert.IsTrue(sut.IsNotEmptyEx.Value, "IsNotEmpty");
    }

    [Test]
    public void Text_empty([Values(false, true)] bool useStartText,
      [Values(false, true)]bool useSlashedPath)
    {
      DepInput<string> textEx = new DepInput<string>();
      if (useStartText)
        textEx.Value = SomeAbsPath.Path;
      else
        textEx.Value = String.Empty;
      UIPathInputHandler sut = new UIPathInputHandler(textEx);
      sut.UseSlashedPath = useSlashedPath;

      textEx.Value = String.Empty;

      Assert.AreEqual(String.Empty, textEx.Value, "TextEx"); // slash must not be added
      Assert.IsTrue(sut.Path.IsEmpty, "Path");
      Assert.IsTrue(sut.RelPath.IsEmpty, "RelPath");
      Assert.IsFalse(sut.IsNotEmptyEx.Value, "IsNotEmpty");
    }

    #endregion

    #region Path

    [Test]
    public void Path_abs([Values(false, true)] bool useStartText,
      [Values(false, true)]bool useSlashedPath)
    {
      DepInput<string> textEx = new DepInput<string>();
      if (useStartText)
        textEx.Value = "aaa";
      else
        textEx.Value = String.Empty;
      UIPathInputHandler sut = new UIPathInputHandler(textEx);
      sut.UseSlashedPath = useSlashedPath;

      AbsPath testPath = new AbsPath(SomeAbsPath, "xxx");
      sut.Path = testPath;

      Assert.AreEqual(useSlashedPath?testPath.SlashedPath:testPath.Path, textEx.Value, "TextEx"); 
      Assert.AreEqual(testPath.Path, sut.Path.Path, "Path");
      Assert.AreEqual(testPath.Path, sut.RelPath.Path, "RelPath");
      Assert.IsTrue(sut.IsNotEmptyEx.Value, "IsNotEmpty");
    }

    [Test]
    public void Path_empty([Values(false, true)] bool useStartText,
      [Values(false, true)]bool useSlashedPath)
    {
      DepInput<string> textEx = new DepInput<string>();
      if (useStartText)
        textEx.Value = "aaa";
      else
        textEx.Value = String.Empty;
      UIPathInputHandler sut = new UIPathInputHandler(textEx);
      sut.UseSlashedPath = useSlashedPath;

      sut.Path = AbsPath.Empty;

      Assert.AreEqual(String.Empty, textEx.Value, "TextEx"); 
      Assert.IsTrue(sut.Path.IsEmpty, "Path");
      Assert.IsTrue(sut.RelPath.IsEmpty, "RelPath");
      Assert.IsFalse(sut.IsNotEmptyEx.Value, "IsNotEmpty");
    }

    #endregion

    #region RelPath

    [Test]
    public void RelPath_rel([Values(false, true)] bool useStartText,
      [Values(false, true)]bool useSlashedPath)
    {
      DepInput<string> textEx = new DepInput<string>();
      if (useStartText)
        textEx.Value = SomeAbsPath.Path;
      else
        textEx.Value = String.Empty;
      UIPathInputHandler sut = new UIPathInputHandler(textEx);
      sut.UseSlashedPath = useSlashedPath;

      RelPath testPath= new RelPath("xxx");
      sut.RelPath = testPath;

      Assert.AreEqual(useSlashedPath ? testPath.SlashedPath : testPath.Path, textEx.Value, "TextEx");
      Assert.AreEqual(new AbsPath(CurDir, "xxx").Path, sut.Path.Path, "Path");
      Assert.AreEqual("xxx", sut.RelPath.Path, "RelPath");
      Assert.IsTrue(sut.IsNotEmptyEx.Value, "IsNotEmpty");
    }

    [Test]
    public void RelPath_abs([Values(false, true)] bool useStartText,
      [Values(false, true)]bool useSlashedPath)
    {
      DepInput<string> textEx = new DepInput<string>();
      if (useStartText)
        textEx.Value = "aaa";
      else
        textEx.Value = String.Empty;
      UIPathInputHandler sut = new UIPathInputHandler(textEx);
      sut.UseSlashedPath = useSlashedPath;

      RelPath testPath = new RelPath(SomeAbsPath.Path);
      sut.RelPath = testPath;

      Assert.AreEqual(useSlashedPath ? testPath.SlashedPath : testPath.Path, textEx.Value, "TextEx");
      Assert.AreEqual(testPath.Path, sut.Path.Path, "Path");
      Assert.AreEqual(testPath.Path, sut.RelPath.Path, "RelPath");
      Assert.IsTrue(sut.IsNotEmptyEx.Value, "IsNotEmpty");
    }

    [Test]
    public void RelPath_empty([Values(false, true)] bool useStartText,
      [Values(false, true)]bool useSlashedPath)
    {
      DepInput<string> textEx = new DepInput<string>();
      if (useStartText)
        textEx.Value = "aaa";
      else
        textEx.Value = String.Empty;
      UIPathInputHandler sut = new UIPathInputHandler(textEx);
      sut.UseSlashedPath = useSlashedPath;

      sut.RelPath = RelPath.Empty;

      Assert.AreEqual(String.Empty, textEx.Value, "TextEx");
      Assert.IsTrue(sut.Path.IsEmpty, "Path");
      Assert.IsTrue(sut.RelPath.IsEmpty, "RelPath");
      Assert.IsFalse(sut.IsNotEmptyEx.Value, "IsNotEmpty");
    }

    #endregion
  }
}
