using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Russian;

namespace ExtRussian_tests
{
  [TestFixture]
  public class RussianToolsTests
  {
    #region ContainsXXXChar()

    [TestCase("", false)]
    [TestCase("Ы", true)]
    [TestCase("W-Ё", true)]
    [TestCase("я", false)]
    [TestCase("Яя", true)]
    public void ContainsUpperRussianChar(string s, bool wantedRes)
    {
      bool res = RussianTools.ContainsUpperRussianChar(s);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("", false)]
    [TestCase("ы", true)]
    [TestCase("w-ё", true)]
    [TestCase("Я", false)]
    [TestCase("Яя", true)]
    public void ContainsLowerRussianChar(string s, bool wantedRes)
    {
      bool res = RussianTools.ContainsLowerRussianChar(s);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("", false)]
    [TestCase("ы", true)]
    [TestCase("w-ё", true)]
    [TestCase("Я", true)]
    [TestCase("Яя", true)]
    [TestCase("AZaz", false)]
    public void ContainsRussianChar(string s, bool wantedRes)
    {
      bool res = RussianTools.ContainsRussianChar(s);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("", false)]
    [TestCase("aaa", false)]
    [TestCase("aaA", true)]
    [TestCase("АБЁабё", false)]
    public void ContainsUpperLatinChar(string s, bool wantedRes)
    {
      bool res = RussianTools.ContainsUpperLatinChar(s);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("", false)]
    [TestCase("AAA", false)]
    [TestCase("aaA", true)]
    [TestCase("АБЁабё", false)]
    public void ContainsLowerLatinChar(string s, bool wantedRes)
    {
      bool res = RussianTools.ContainsLowerLatinChar(s);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("", false)]
    [TestCase("aaa", true)]
    [TestCase("AAA", true)]
    [TestCase("АБЁабё", false)]
    public void ContainsLatinChar(string s, bool wantedRes)
    {
      bool res = RussianTools.ContainsLatinChar(s);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region IsXXXChar()

    [Test]
    public void IsXXXChar()
    {
      string sChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьяэюя1234567890 ./";
      string sFlags = "LLLLLLLLLLLLLLLLLLLLLLLLLLllllllllllllllllllllllllllRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr-------------";

      for (int i = 0; i < sChars.Length; i++)
      {
        char c = sChars[i];
        bool isULat = sFlags[i] == 'L';
        bool isLLat = sFlags[i] == 'l';
        bool isURus = sFlags[i] == 'R';
        bool isLRus = sFlags[i] == 'r';
        bool isLat = isULat || isLLat;
        bool isRus = isURus || isLRus;

        Assert.AreEqual(isULat, RussianTools.IsUpperLatinChar(c));
        Assert.AreEqual(isLLat, RussianTools.IsLowerLatinChar(c));
        Assert.AreEqual(isLat, RussianTools.IsLatinChar(c));
        Assert.AreEqual(isURus, RussianTools.IsUpperRussianChar(c));
        Assert.AreEqual(isLRus, RussianTools.IsLowerRussianChar(c));
        Assert.AreEqual(isRus, RussianTools.IsRussianChar(c));
      }
    }

    #endregion

    #region AndList(), OrList()

    [TestCase("", "")]
    [TestCase("Один", "Один")]
    [TestCase("Один|Два", "Один и Два")]
    [TestCase("Один|Два|Три", "Один, Два и Три")]
    [TestCase("Один|Два|Три|Четыре", "Один, Два, Три и Четыре")]
    public void AndList(string sArgs, string wantedRes)
    {
      List<string> args = new List<string>(sArgs.Split('|'));
      string res = RussianTools.AndList(args);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("", "")]
    [TestCase("Один", "Один")]
    [TestCase("Один|Два", "Один или Два")]
    [TestCase("Один|Два|Три", "Один, Два или Три")]
    [TestCase("Один|Два|Три|Четыре", "Один, Два, Три или Четыре")]
    public void OrList(string sArgs, string wantedRes)
    {
      List<string> args = new List<string>(sArgs.Split('|'));
      string res = RussianTools.OrList(args);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion
  }
}
