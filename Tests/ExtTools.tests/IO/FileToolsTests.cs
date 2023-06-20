using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.IO;
using NUnit.Framework;

namespace ExtTools_tests.IO
{
  // TODO: Сделан только метод GetVersionFromStr()

  [TestFixture]
  public class FileToolsTests
  {
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
  }
}
