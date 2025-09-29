using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Text;
using FreeLibSet.Core;

namespace ExtTools_tests.Text
{
  [TestFixture]
  public class SingleByteTranscoderTests
  {
    #region Константы

    const int CodePageLatin = 1252;
    const int CodePageUTF8 = 65001;
    const int CodePageCyrWin = 1251;
    const int CodePageCyrDos = 866;

    #endregion

    #region Конструктор

    [Test]
    public void Constructor()
    {
      Encoding enc1 = Encoding.GetEncoding(CodePageCyrWin);
      Encoding enc2 = Encoding.GetEncoding(CodePageCyrDos);
      SingleByteTranscoder sut = new SingleByteTranscoder(enc1, enc2);
      Assert.AreSame(enc1, sut.SrcEncoding, "SrcEncoding");
      Assert.AreSame(enc2, sut.ResEncoding, "ResEncoding");
      Assert.IsFalse(sut.IsDirect, "IsDirect");
    }

    #endregion

    #region CanCreate()


    [TestCase(CodePageCyrWin, CodePageCyrDos, true)]
    [TestCase(CodePageCyrWin, CodePageCyrWin, true)]
    [TestCase(CodePageCyrWin, CodePageUTF8, false)]
    [TestCase(CodePageLatin, CodePageCyrWin, true)]
    [TestCase(CodePageLatin, CodePageLatin, true)]
    [TestCase(CodePageLatin, CodePageUTF8, false)]
    public void CanCreate(int srcCodePage, int resCodePage, bool wantedRes)
    {
      Encoding srcEncoding = Encoding.GetEncoding(srcCodePage);
      Encoding resEncoding = Encoding.GetEncoding(resCodePage);

      if (srcEncoding == null || resEncoding == null)
        throw new ArgumentException();

      bool res = SingleByteTranscoder.CanCreate(srcEncoding, resEncoding);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion


    [TestCase(CodePageCyrWin, CodePageCyrDos)]
    [TestCase(CodePageCyrWin, CodePageCyrWin)]
    public void Transcode(int srcCodePage, int resCodePage)
    {
      Encoding srcEncoding = Encoding.GetEncoding(srcCodePage);
      Encoding resEncoding = Encoding.GetEncoding(resCodePage);

      SingleByteTranscoder sut = new SingleByteTranscoder(srcEncoding, resEncoding);

      string testStr = "ABCD123АБВГ";
      byte[] src1 = srcEncoding.GetBytes(testStr);
      Assert.AreEqual(testStr.Length, src1.Length, "Src Length");
      byte[] wantedRes1 = resEncoding.GetBytes(testStr);
      Assert.AreEqual(testStr.Length, wantedRes1.Length, "Res Length");

      byte[] res1 = new byte[testStr.Length];
      sut.Transcode(src1, res1);
      CollectionAssert.AreEqual(wantedRes1, res1, "#1");

      byte[] src2 = ArrayTools.MergeArrays<byte>(new byte[] { 1, 2, 3 }, src1, new byte[] { 4, 5 });
      byte[] res2 = new byte[2 + testStr.Length + 1];
      ArrayTools.FillArray<byte>(res2, 6);
      byte[] wantedRes2 = ArrayTools.MergeArrays<byte>(new byte[] { 6, 6 }, wantedRes1, new byte[] { 6 });
      sut.Transcode(src2, 3, res2, 2, testStr.Length);
      CollectionAssert.AreEqual(wantedRes2, res2, "#2");
    }
  }
}
