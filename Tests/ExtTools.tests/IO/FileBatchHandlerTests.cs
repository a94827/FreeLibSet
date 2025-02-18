using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FreeLibSet.Core;
using FreeLibSet.IO;
using NUnit.Framework;

namespace ExtTools_tests.IO
{
  // TODO: Как проверить Move между дисками (режим копировать+удалить)?

  [TestFixture]
  public class FileBatchHandlerTests
  {
    #region Конструктор

    [Test]
    public void Constructor()
    {
      FileBatchHandler sut = new FileBatchHandler();
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreEqual(FileBatchMode.Copy, sut.Mode, "Mode");
      Assert.IsFalse(sut.MoveAsCopyAndDelete, "MoveAsCopyAndDelete");
    }

    #endregion

    #region Copy

    [Test]
    public void Copy_WholeDir()
    {
      using (TempDirectory tempDir = new TempDirectory())
      {
        string dirs = "D1, D1/D11, D2, D0";
        string files = "1.txt, 2.bin, D1/3.txt, D1/D11/4.txt, D2/5.txt";
        CreateTestFiles(tempDir.Dir + "1", dirs, files);

        FileBatchHandler sut = new FileBatchHandler();
        sut.Mode = FileBatchMode.Copy;
        sut.Add(tempDir.Dir + "1", tempDir.Dir + "2");

        sut.Process();

        ValidateFiles(tempDir.Dir + "1", dirs, files, true, "Source");
        ValidateFiles(tempDir.Dir + "2", dirs, files, true, "Copy results");
      }
    }

    [Test]
    public void Copy_SubDir()
    {
      using (TempDirectory tempDir = new TempDirectory())
      {
        string dirs = "D1, D1/D11, D2, D0";
        string files = "1.txt, 2.bin, D1/3.txt, D1/D11/4.txt, D2/5.txt";
        CreateTestFiles(tempDir.Dir + "1", dirs, files);

        FileBatchHandler sut = new FileBatchHandler();
        sut.Mode = FileBatchMode.Copy;
        sut.Add(tempDir.Dir + "1" + "D1", tempDir.Dir + "2");

        sut.Process();

        ValidateFiles(tempDir.Dir + "1", dirs, files, true, "Source");
        ValidateFiles(tempDir.Dir + "2", "D11", "3.txt, D11/4.txt", true, "Copy results");
      }
    }

    [Test]
    public void Copy_SingleFile()
    {
      using (TempDirectory tempDir = new TempDirectory())
      {
        string dirs = "D1, D1/D11, D2, D0";
        string files = "1.txt, 2.bin, D1/3.txt, D1/D11/4.txt, D2/5.txt";
        CreateTestFiles(tempDir.Dir + "1", dirs, files);

        FileBatchHandler sut = new FileBatchHandler();
        sut.Mode = FileBatchMode.Copy;
        sut.Add(tempDir.Dir + "1" + "1.txt", tempDir.Dir + "2" + "22" + "2.txt");

        sut.Process();

        ValidateFiles(tempDir.Dir + "1", dirs, files, true, "Source");
        ValidateFiles(tempDir.Dir + "2", "22", "22/2.txt", false, "Copy results");
      }
    }

    [Test]
    public void Copy_Template_NoRecurse()
    {
      using (TempDirectory tempDir = new TempDirectory())
      {
        string dirs = "D1, D1/D11, D2, D0";
        string files = "1.txt, 2.bin, D1/3.txt, D1/D11/4.txt, D2/5.txt";
        CreateTestFiles(tempDir.Dir + "1", dirs, files);

        FileBatchHandler sut = new FileBatchHandler();
        sut.Mode = FileBatchMode.Copy;
        sut.Add(tempDir.Dir + "1", tempDir.Dir + "2", "*.txt", false);

        sut.Process();

        ValidateFiles(tempDir.Dir + "1", dirs, files, true, "Source");
        ValidateFiles(tempDir.Dir + "2", "", "1.txt", true, "Copy results");
      }
    }

    [Test]
    public void Copy_Template_Recurse()
    {
      using (TempDirectory tempDir = new TempDirectory())
      {
        string dirs = "D1, D1/D11, D2, D0";
        string files = "1.txt, 2.bin, D1/3.txt, D1/D11/4.txt, D2/5.txt";
        CreateTestFiles(tempDir.Dir + "1", dirs, files);

        FileBatchHandler sut = new FileBatchHandler();
        sut.Mode = FileBatchMode.Copy;
        sut.Add(tempDir.Dir + "1", tempDir.Dir + "2", "*.txt", true);

        sut.Process();

        ValidateFiles(tempDir.Dir + "1", dirs, files, true, "Source");
        ValidateFiles(tempDir.Dir + "2", "D1, D1/D11, D2", "1.txt, D1/3.txt, D1/D11/4.txt, D2/5.txt", true, "Copy results");
      }
    }

    [Test]
    public void Copy_Overwrite()
    {
      using (TempDirectory tempDir = new TempDirectory())
      {
        string dirs = "D1";
        string files = "1.txt, D1/2.txt";
        CreateTestFiles(tempDir.Dir + "1", dirs, files);
        CreateBadFile(tempDir.Dir + "2" + "D1" + "2.txt");

        FileBatchHandler sut = new FileBatchHandler();
        sut.Mode = FileBatchMode.Copy;
        sut.Add(tempDir.Dir + "1", tempDir.Dir + "2", "*.txt", true);

        sut.Process();

        ValidateFiles(tempDir.Dir + "2", dirs, files, true, "Copy results");
      }
    }

    #endregion

    #region Move

    [Test]
    public void Move_SubDir([Values(false, true)]bool moveAsCopyAndDelete)
    {
      using (TempDirectory tempDir = new TempDirectory())
      {
        string dirs = "D1, D1/D11, D2, D0";
        string files = "1.txt, 2.bin, D1/3.txt, D1/D11/4.txt, D2/5.txt";
        CreateTestFiles(tempDir.Dir + "1", dirs, files);

        FileBatchHandler sut = new FileBatchHandler();
        sut.Mode = FileBatchMode.Move;
        sut.MoveAsCopyAndDelete = moveAsCopyAndDelete;
        sut.Add(tempDir.Dir + "1" + "D1", tempDir.Dir + "2");

        sut.Process();

        ValidateFiles(tempDir.Dir + "1", "D2, D0", "1.txt, 2.bin, D2/5.txt", true, "Reminded");
        ValidateFiles(tempDir.Dir + "2", "D11", "3.txt, D11/4.txt", true, "Move results");
      }
    }

    [Test]
    public void Move_SingleFile([Values(false, true)]bool moveAsCopyAndDelete)
    {
      using (TempDirectory tempDir = new TempDirectory())
      {
        string dirs = "D1, D1/D11, D2, D0";
        string files = "1.txt, 2.bin, D1/3.txt, D1/D11/4.txt, D2/5.txt";
        CreateTestFiles(tempDir.Dir + "1", dirs, files);

        FileBatchHandler sut = new FileBatchHandler();
        sut.Mode = FileBatchMode.Move;
        sut.MoveAsCopyAndDelete = moveAsCopyAndDelete;
        sut.Add(tempDir.Dir + "1" + "1.txt", tempDir.Dir + "2" + "22" + "2.txt");

        sut.Process();

        ValidateFiles(tempDir.Dir + "1", dirs, "2.bin, D1/3.txt, D1/D11/4.txt, D2/5.txt", true, "Reminded");
        ValidateFiles(tempDir.Dir + "2", "22", "22/2.txt", false, "Move results");
      }
    }

    [Test]
    public void Move_Template_NoRecurse([Values(false, true)]bool moveAsCopyAndDelete)
    {
      using (TempDirectory tempDir = new TempDirectory())
      {
        string dirs = "D1, D1/D11, D2, D0";
        string files = "1.txt, 2.bin, D1/3.txt, D1/D11/4.txt, D2/5.txt";
        CreateTestFiles(tempDir.Dir + "1", dirs, files);

        FileBatchHandler sut = new FileBatchHandler();
        sut.Mode = FileBatchMode.Move;
        sut.MoveAsCopyAndDelete = moveAsCopyAndDelete;
        sut.Add(tempDir.Dir + "1", tempDir.Dir + "2", "*.txt", false);

        sut.Process();

        ValidateFiles(tempDir.Dir + "1", dirs, "2.bin, D1/3.txt, D1/D11/4.txt, D2/5.txt", true, "Source");
        ValidateFiles(tempDir.Dir + "2", "", "1.txt", true, "Move results");
      }
    }

    [Test]
    public void Move_Template_Recurse([Values(false, true)]bool moveAsCopyAndDelete)
    {
      using (TempDirectory tempDir = new TempDirectory())
      {
        string dirs = "D1, D1/D11, D2, D0";
        string files = "1.txt, 2.bin, D1/3.txt, D1/D11/4.txt, D2/5.txt";
        CreateTestFiles(tempDir.Dir + "1", dirs, files);

        FileBatchHandler sut = new FileBatchHandler();
        sut.Mode = FileBatchMode.Move;
        sut.MoveAsCopyAndDelete = moveAsCopyAndDelete;
        sut.Add(tempDir.Dir + "1", tempDir.Dir + "2", "*.txt", true);

        sut.Process();

        ValidateFiles(tempDir.Dir + "1", dirs, "2.bin", true, "Source");
        ValidateFiles(tempDir.Dir + "2", "D1, D1/D11, D2", "1.txt, D1/3.txt, D1/D11/4.txt, D2/5.txt", true, "Move results");
      }
    }

    [Test]
    public void Move_Overwrite([Values(false, true)]bool moveAsCopyAndDelete)
    {
      using (TempDirectory tempDir = new TempDirectory())
      {
        string dirs = "D1";
        string files = "1.txt, D1/2.txt";
        CreateTestFiles(tempDir.Dir + "1", dirs, files);
        CreateBadFile(tempDir.Dir + "2" + "D1" + "2.txt");

        FileBatchHandler sut = new FileBatchHandler();
        sut.Mode = FileBatchMode.Move;
        sut.MoveAsCopyAndDelete = moveAsCopyAndDelete;
        sut.Add(tempDir.Dir + "1", tempDir.Dir + "2", "*.txt", true);

        sut.Process();

        ValidateFiles(tempDir.Dir + "1", dirs, "", true, "Original");
        ValidateFiles(tempDir.Dir + "2", dirs, files, true, "Move results");
      }
    }

    #endregion

    #region Тестовые файлы

    private void CreateTestFiles(AbsPath path, string dirs, string files)
    {
      FileTools.ForceDirs(path);
      foreach (string[] names in SplitNames(dirs))
        FileTools.ForceDirs(new AbsPath(path, names));
      foreach (string[] names in SplitNames(files))
      {
        AbsPath filePath = new AbsPath(path, names);
        File.WriteAllBytes(filePath.Path, GetBytes(filePath.FileName));
      }
    }

    private string[][] SplitNames(string s)
    {
      if (String.IsNullOrEmpty(s))
        return new string[0][];

      string[] a = s.Split(',');
      string[][] res = new string[a.Length][];
      for (int i = 0; i < a.Length; i++)
      {
        string[] a2 = a[i].Split('/');
        for (int j = 0; j < a2.Length; j++)
          a2[j] = a2[j].Trim();
        res[i] = a2;
      }
      return res;
    }

    private static void ValidateFiles(AbsPath path, string wantedDirs, string wantedFiles, bool checkContents, string message)
    {
      Assert.True(Directory.Exists(path.Path), message + " - validated directory existance");

      string[] aRealDirs = Directory.GetDirectories(path.Path, "*", SearchOption.AllDirectories);
      TrimRootDir(aRealDirs, path);
      string[] aRealFiles = Directory.GetFiles(path.Path, "*", SearchOption.AllDirectories);
      TrimRootDir(aRealFiles, path);

      string[] aWantedDirs = GetWantedNames(wantedDirs);
      string[] aWantedFiles = GetWantedNames(wantedFiles);

      CollectionAssert.AreEquivalent(aWantedDirs, aRealDirs, message + " - directories");
      CollectionAssert.AreEquivalent(aWantedFiles, aRealFiles, message + " - files");

      if (checkContents)
      {
        foreach (string fileName in aWantedFiles)
        {
          AbsPath filePath = new AbsPath(path, fileName.Split('/'));
          byte[] realBytes = File.ReadAllBytes(filePath.Path);
          byte[] wantedBytes = GetBytes(filePath.FileName);
          CollectionAssert.AreEqual(wantedBytes, realBytes, message + " - contents of file " + fileName);
        }
      }
    }

    private static void TrimRootDir(string[] aRealNames, AbsPath path)
    {
      int n = path.SlashedPath.Length;
      for (int i = 0; i < aRealNames.Length; i++)
      {
        string s = aRealNames[i].Substring(n);
        s = s.Replace(Path.DirectorySeparatorChar, '/');
        aRealNames[i] = s;
      }
    }

    private static string[] GetWantedNames(string wantedNames)
    {
      if (String.IsNullOrEmpty(wantedNames))
        return DataTools.EmptyStrings;
      string[] a = wantedNames.Split(',');
      for (int i = 0; i < a.Length; i++)
        a[i] = a[i].Trim();
      return a;
    }

    private static byte[] GetBytes(string fileName)
    {
      string s = DataTools.MD5SumFromString(fileName.ToUpperInvariant());
      return Encoding.ASCII.GetBytes(s);
    }

    private static void CreateBadFile(AbsPath filePath)
    {
      FileTools.ForceDirs(filePath.ParentDir);
      File.WriteAllBytes(filePath.Path, DataTools.CreateArray<byte>(1000, 66));
    }

    #endregion

    #region Прочее

    [Test]
    public void Process_None([Values(FileBatchMode.Copy, FileBatchMode.Copy)]FileBatchMode mode)
    {
      FileBatchHandler sut = new FileBatchHandler();
      sut.Mode = mode;
      sut.Process();
    }

    #endregion
  }
}
