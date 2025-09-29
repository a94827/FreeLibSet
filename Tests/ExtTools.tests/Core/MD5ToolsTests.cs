using FreeLibSet.Core;
using FreeLibSet.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtTools_tests.Core
{
  [TestFixture]
  public class MD5ToolsTests
  {
    #region MD5

    [TestCase("", "d41d8cd98f00b204e9800998ecf8427e")] // https://ru.wikipedia.org/wiki/MD5
    [TestCase("414243", "902fbdd2b1df0c4f70b4a5d23525e932")] // строка "ABC"
    public void MD5Sum(string sBytes, string wanted)
    {
      byte[] b = StringTools.HexToBytes(sBytes);
      Assert.AreEqual(wanted, MD5Tools.MD5Sum(b));
    }

    // Тестовые суммы для строк получены на сайте https://dencode.com/ в режиме "UTF-16LE"

    [TestCase("ABC", "716774b53a92b98525bba179a159cc71")]
    [TestCase("123", "5fa285e1bebe0a6623e33afc04a1fbd5")]
    [TestCase("", "d41d8cd98f00b204e9800998ecf8427e")] // https://ru.wikipedia.org/wiki/MD5
    public void MD5SumFromString(string s, string wanted)
    {
      Assert.AreEqual(wanted, MD5Tools.MD5SumFromString(s));
    }

    #endregion

    #region MD5Sum()

    public void MD5Sum([Values(0, 100, 123456)]int len)
    {
      byte[] b = new byte[len];
      for (int i = 0; i < b.Length; i++)
        b[i] = (byte)(i % 100);

      string wantedRes = MD5Tools.MD5Sum(b);

      System.IO.MemoryStream ms1 = new System.IO.MemoryStream(b);
      ms1.Position = 0;
      string res1 = MD5Tools.MD5Sum(ms1);
      Assert.AreEqual(wantedRes, res1, "Stream");

      using (TempDirectory dir = new TempDirectory())
      {
        AbsPath file2 = new AbsPath(dir.Dir, "test.bin");
        System.IO.File.WriteAllBytes(file2.Path, b);

        string res2 = MD5Tools.MD5Sum(file2);
        Assert.AreEqual(wantedRes, res2, "File");
      }
    }

    #endregion
  }
}
