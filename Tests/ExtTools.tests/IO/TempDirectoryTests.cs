using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.IO;
using FreeLibSet.Core;

namespace ExtTools_tests.IO
{
  [TestFixture]
  public class TempDirectoryTests
  {
    [Test]
    public void Test_ok()
    {
      AbsPath dir;
      using (TempDirectory sut = new TempDirectory())
      {
        dir = sut.Dir;
        Assert.IsTrue(System.IO.Directory.Exists(dir.Path), "Directory created");

        System.IO.File.WriteAllBytes(new AbsPath(dir, "test1.bin").Path, DataTools.EmptyBytes);

        AbsPath dir2 = new AbsPath(dir, "SubDir1", "SubDir2");
        FileTools.ForceDirs(dir2);
        System.IO.File.WriteAllText(new AbsPath(dir2, "test2.txt").Path, "Hello, world!");
      }
      Assert.IsFalse(System.IO.Directory.Exists(dir.Path), "Directory deleted");
    }

    [Test]
    public void DeletedOnDisposed_false()
    {
      AbsPath dir;
      using (TempDirectory sut = new TempDirectory())
      {
        dir = sut.Dir;

        sut.DeleteOnDispose = false;
      }

      Assert.IsTrue(System.IO.Directory.Exists(dir.Path), "Directory still exists");

      System.IO.Directory.Delete(dir.Path);
    }
  }
}
