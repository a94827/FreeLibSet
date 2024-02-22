using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Russian;
using FreeLibSet.Tests;
using FreeLibSet.Core;

namespace ExtRussian_tests
{
  [TestFixture]
  public class RusNumberConvertTests
  {
    #region Денежная сумма

    // Структуру RoublesAndCopecks не тестируем, так как она не используется в прикладном коде в явном виде
    // Также не тестируем перегрузки с StringBuilder

    [TestCase("1234.56", "Одна тысяча двести тридцать четыре рубля 56 копеек")]
    [TestCase("1234", "Одна тысяча двести тридцать четыре рубля 00 копеек")]
    [TestCase("-1234", "Минус одна тысяча двести тридцать четыре рубля 00 копеек")]
    public void MoneyStr_simple(string sValue, string wantedRes)
    {
      decimal value = StdConvert.ToDecimal(sValue);
      string res = RusNumberConvert.MoneyStr(value);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("1234", RusNumberConvert.CopecksFormat.Digits, "Одна тысяча двести тридцать четыре рубля 00 копеек")]
    [TestCase("1234.56", RusNumberConvert.CopecksFormat.None, "Одна тысяча двести тридцать четыре рубля")]
    [TestCase("1234.56", RusNumberConvert.CopecksFormat.NoneIfZero, "Одна тысяча двести тридцать четыре рубля 56 копеек")]
    [TestCase("1234", RusNumberConvert.CopecksFormat.NoneIfZero, "Одна тысяча двести тридцать четыре рубля")]
    [TestCase("1234.56", RusNumberConvert.CopecksFormat.String, "Одна тысяча двести тридцать четыре рубля пятьдесят шесть копеек")]
    public void MoneyStr_copecksFormat(string sValue, RusNumberConvert.CopecksFormat copecksFormat, string wantedRes)
    {
      decimal value = StdConvert.ToDecimal(sValue);
      string res = RusNumberConvert.MoneyStr(value, copecksFormat);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase("1234.56", RusCase.Nominative, "Одна тысяча двести тридцать четыре рубля пятьдесят шесть копеек")]
    [TestCase("1234.56", RusCase.Genitive, "одной тысячи двухсот тридцати четырех рублей пятидесяти шести копеек")]
    [TestCase("1234.56", RusCase.Dative, "одной тысяче двумстам тридцати четырем рублям пятидесяти шести копейкам")]
    [TestCase("1234.56", RusCase.Accusative, "одну тысячу двести тридцать четыре рубля пятьдесят шесть копеек")]
    [TestCase("1234.56", RusCase.Instrumental, "одной тысячей двумястами тридцатью четырьмя рублями пятьюдесятью шестью копейками")]
    [TestCase("1234.56", RusCase.Locative, "одной тысяче двухстах тридцати четырех рублях пятидесяти шести копейках")]
    public void MoneyStr_case(string sValue, RusCase theCase, string wantedRes)
    {
      decimal value = StdConvert.ToDecimal(sValue);
      string res = RusNumberConvert.MoneyStr(value, RusNumberConvert.CopecksFormat.String, theCase);
      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void MoneyStr_specChars([Values(false, true)]bool specChars, 
      [Values(RusNumberConvert.CopecksFormat.Digits, RusNumberConvert.CopecksFormat.String)]RusNumberConvert.CopecksFormat copecksFormat)
    {
      string s = RusNumberConvert.MoneyStr(1234.56m, copecksFormat, RusCase.Nominative, specChars);
      Assert.AreEqual(specChars, s.IndexOf(DataTools.NonBreakSpaceChar) >= 0, "NonBreakSpace");
      Assert.AreEqual(specChars, s.IndexOf(DataTools.SoftHyphenChar) >= 0, "SoftHyphenChar");
      Assert.IsFalse(s.IndexOf('-') >= 0, "Minus char");
    }

    #endregion

    #region Целые числа

    [TestCase(-321L, RusGender.Masculine, RusCase.Nominative, "минус триста двадцать один")]
    [TestCase(-321L, RusGender.Masculine, RusCase.Genitive, "минус трехсот двадцати одного")]
    [TestCase(-321L, RusGender.Masculine, RusCase.Dative, "минус тремстам двадцати одному")]
    [TestCase(-321L, RusGender.Masculine, RusCase.Accusative, "минус триста двадцать один")]
    [TestCase(-321L, RusGender.Masculine, RusCase.Instrumental, "минус тремястами двадцатью одним")]
    [TestCase(-321L, RusGender.Masculine, RusCase.Locative, "минус трехстах двадцати одном")]

    [TestCase(0L, RusGender.Masculine, RusCase.Nominative, "ноль")]
    [TestCase(0L, RusGender.Feminine, RusCase.Genitive, "ноля")]
    [TestCase(0L, RusGender.Newter, RusCase.Dative, "нолю")]
    [TestCase(0L, RusGender.Newter, RusCase.Accusative, "ноль")]
    [TestCase(0L, RusGender.Newter, RusCase.Instrumental, "нолем")]
    [TestCase(0L, RusGender.Newter, RusCase.Locative, "ноле")]

    [TestCase(22L, RusGender.Masculine, RusCase.Nominative, "двадцать два")]
    [TestCase(22L, RusGender.Masculine, RusCase.Genitive, "двадцати двух")]
    [TestCase(22L, RusGender.Masculine, RusCase.Dative, "двадцати двум")]
    [TestCase(22L, RusGender.Masculine, RusCase.Accusative, "двадцать два")]
    [TestCase(22L, RusGender.Masculine, RusCase.Instrumental, "двадцатью двумя")]
    [TestCase(22L, RusGender.Masculine, RusCase.Locative, "двадцати двух")]
    [TestCase(22L, RusGender.Feminine, RusCase.Nominative, "двадцать две")]
    [TestCase(22L, RusGender.Feminine, RusCase.Genitive, "двадцати двух")]
    [TestCase(22L, RusGender.Feminine, RusCase.Dative, "двадцати двум")]
    [TestCase(22L, RusGender.Feminine, RusCase.Accusative, "двадцать две")]
    [TestCase(22L, RusGender.Feminine, RusCase.Instrumental, "двадцатью двумя")]
    [TestCase(22L, RusGender.Feminine, RusCase.Locative, "двадцати двух")]

    [TestCase(101L, RusGender.Masculine, RusCase.Nominative, "сто один")]
    [TestCase(101L, RusGender.Masculine, RusCase.Genitive, "ста одного")]
    [TestCase(101L, RusGender.Masculine, RusCase.Dative, "ста одному")]
    [TestCase(101L, RusGender.Masculine, RusCase.Accusative, "сто один")]
    [TestCase(101L, RusGender.Masculine, RusCase.Instrumental, "ста одним")]
    [TestCase(101L, RusGender.Masculine, RusCase.Locative, "ста одном")]
    [TestCase(101L, RusGender.Feminine, RusCase.Nominative, "сто одна")]
    [TestCase(101L, RusGender.Feminine, RusCase.Genitive, "ста одной")]
    [TestCase(101L, RusGender.Feminine, RusCase.Dative, "ста одной")]
    [TestCase(101L, RusGender.Feminine, RusCase.Accusative, "сто одну")]
    [TestCase(101L, RusGender.Feminine, RusCase.Instrumental, "ста одной")]
    [TestCase(101L, RusGender.Feminine, RusCase.Locative, "ста одной")]
    [TestCase(101L, RusGender.Newter, RusCase.Nominative, "сто одно")]
    [TestCase(101L, RusGender.Newter, RusCase.Genitive, "ста одного")]
    [TestCase(101L, RusGender.Newter, RusCase.Dative, "ста одному")]
    [TestCase(101L, RusGender.Newter, RusCase.Accusative, "сто одно")]
    [TestCase(101L, RusGender.Newter, RusCase.Instrumental, "ста одним")]
    [TestCase(101L, RusGender.Newter, RusCase.Locative, "ста одном")]

    [TestCase(456L, RusGender.Newter, RusCase.Nominative, "четыреста пятьдесят шесть")]
    [TestCase(456L, RusGender.Newter, RusCase.Genitive, "четырехсот пятидесяти шести")]
    [TestCase(456L, RusGender.Newter, RusCase.Dative, "четыремстам пятидесяти шести")]
    [TestCase(456L, RusGender.Newter, RusCase.Accusative, "четыреста пятьдесят шесть")]
    [TestCase(456L, RusGender.Newter, RusCase.Instrumental, "четыремястами пятьюдесятью шестью")]
    [TestCase(456L, RusGender.Newter, RusCase.Locative, "четырехстах пятидесяти шести")]

    [TestCase(1000L, RusGender.Masculine, RusCase.Nominative, "одна тысяча")]
    [TestCase(1000L, RusGender.Masculine, RusCase.Genitive, "одной тысячи")]
    [TestCase(1000L, RusGender.Masculine, RusCase.Dative, "одной тысяче")]
    [TestCase(1000L, RusGender.Masculine, RusCase.Accusative, "одну тысячу")]
    [TestCase(1000L, RusGender.Masculine, RusCase.Instrumental, "одной тысячей")]
    [TestCase(1000L, RusGender.Masculine, RusCase.Locative, "одной тысяче")]

    [TestCase(1000000L, RusGender.Masculine, RusCase.Nominative, "один миллион")]
    [TestCase(1000000L, RusGender.Masculine, RusCase.Genitive, "одного миллиона")]
    [TestCase(1000000L, RusGender.Masculine, RusCase.Dative, "одному миллиону")]
    [TestCase(1000000L, RusGender.Masculine, RusCase.Accusative, "один миллион")]
    [TestCase(1000000L, RusGender.Masculine, RusCase.Instrumental, "одним миллионом")]
    [TestCase(1000000L, RusGender.Masculine, RusCase.Locative, "одном миллионе")]

    [TestCase(1000000000L, RusGender.Feminine, RusCase.Nominative, "один миллиард")]
    [TestCase(1000000000L, RusGender.Feminine, RusCase.Genitive, "одного миллиарда")]
    [TestCase(1000000000L, RusGender.Feminine, RusCase.Dative, "одному миллиарду")]
    [TestCase(1000000000L, RusGender.Feminine, RusCase.Accusative, "один миллиард")]
    [TestCase(1000000000L, RusGender.Feminine, RusCase.Instrumental, "одним миллиардом")]
    [TestCase(1000000000L, RusGender.Feminine, RusCase.Locative, "одном миллиарде")]

    [TestCase(3000000000L, RusGender.Newter, RusCase.Nominative, "три миллиарда")]
    [TestCase(3000000000L, RusGender.Newter, RusCase.Genitive, "трех миллиардов")]
    [TestCase(3000000000L, RusGender.Newter, RusCase.Dative, "трем миллиардам")]
    [TestCase(3000000000L, RusGender.Newter, RusCase.Accusative, "три миллиарда")]
    [TestCase(3000000000L, RusGender.Newter, RusCase.Instrumental, "тремя миллиардами")]
    [TestCase(3000000000L, RusGender.Newter, RusCase.Locative, "трех миллиардах")]

    [TestCase(999999999999L, RusGender.Masculine, RusCase.Instrumental, "девятьюстами девяноста девятью миллиардами девятьюстами девяноста девятью миллионами девятьюстами девяноста девятью тысячами девятьюстами девяноста девятью")]
    public void ToString_Int64(Int64 value, RusGender gender, RusCase theCase, string wantedRes)
    {
      string res1 = RusNumberConvert.ToString(value, gender, theCase);
      Assert.AreEqual(wantedRes, res1, "3-args result");
      Assert.IsFalse(res1.Contains(DataTools.NonBreakSpaceStr), "3-args NonBreakSpace");
      Assert.IsFalse(res1.Contains(DataTools.SoftHyphenStr), "3-args SoftHyphen");

      string res2 = RusNumberConvert.ToString(value, gender, theCase, false);
      Assert.AreEqual(wantedRes, res2, "specChars=false result");
      Assert.IsFalse(res2.Contains(DataTools.NonBreakSpaceStr), "specChars=false NonBreakSpace");
      Assert.IsFalse(res2.Contains(DataTools.SoftHyphenStr), "specChars=false SoftHyphen");

      string res3 = RusNumberConvert.ToString(value, gender, theCase, true);
      res3 = res3.Replace(DataTools.NonBreakSpaceChar, ' ');
      res3 = res3.Replace(DataTools.SoftHyphenStr, "");
      Assert.AreEqual(wantedRes, res3, "specChars=true result");
    }

    [TestCase(0UL, RusGender.Masculine, RusCase.Nominative, "ноль")]
    [TestCase(22UL, RusGender.Masculine, RusCase.Instrumental, "двадцатью двумя")]
    public void ToString_UInt64(UInt64 value, RusGender gender, RusCase theCase, string wantedRes)
    {
      string res1 = RusNumberConvert.ToString(value, gender, theCase);
      Assert.AreEqual(wantedRes, res1, "3-args result");
      Assert.IsFalse(res1.Contains(DataTools.NonBreakSpaceStr), "3-args NonBreakSpace");
      Assert.IsFalse(res1.Contains(DataTools.SoftHyphenStr), "3-args SoftHyphen");

      string res2 = RusNumberConvert.ToString(value, gender, theCase, false);
      Assert.AreEqual(wantedRes, res2, "specChars=false result");
      Assert.IsFalse(res2.Contains(DataTools.NonBreakSpaceStr), "specChars=false NonBreakSpace");
      Assert.IsFalse(res2.Contains(DataTools.SoftHyphenStr), "specChars=false SoftHyphen");

      string res3 = RusNumberConvert.ToString(value, gender, theCase, true);
      res3 = res3.Replace(DataTools.NonBreakSpaceChar, ' ');
      res3 = res3.Replace(DataTools.SoftHyphenStr, "");
      Assert.AreEqual(wantedRes, res3, "specChars=true result");
    }

    #endregion

    #region Дробные числа

    // Целые числа
    [TestCase("-321", 11, RusGender.Masculine, RusCase.Instrumental, "минус тремястами двадцатью одним")]
    [TestCase("0", 10, RusGender.Feminine, RusCase.Genitive, "ноля")]
    [TestCase("22", 8, RusGender.Masculine, RusCase.Accusative, "двадцать два")]
    [TestCase("22", 6, RusGender.Feminine, RusCase.Accusative, "двадцать две")]
    [TestCase("101", 4, RusGender.Masculine, RusCase.Dative, "ста одному")]
    [TestCase("101", 2, RusGender.Feminine, RusCase.Dative, "ста одной")]
    [TestCase("101", 0, RusGender.Newter, RusCase.Dative, "ста одному")]
    [TestCase("999999999999", 3, RusGender.Masculine, RusCase.Instrumental, "девятьюстами девяноста девятью миллиардами девятьюстами девяноста девятью миллионами девятьюстами девяноста девятью тысячами девятьюстами девяноста девятью")]
    // Дробные числа
    [TestCase("3.14", 2, RusGender.Masculine, RusCase.Nominative, "три целых четырнадцать сотых")]
    [TestCase("3.14", 2, RusGender.Masculine, RusCase.Genitive, "трех целых четырнадцати сотых")]
    [TestCase("3.14", 2, RusGender.Masculine, RusCase.Dative, "трем целым четырнадцати сотым")]
    [TestCase("3.14", 2, RusGender.Masculine, RusCase.Accusative, "три целых четырнадцать сотых")]
    [TestCase("3.14", 2, RusGender.Masculine, RusCase.Instrumental, "тремя целыми четырнадцатью сотыми")]
    [TestCase("3.14", 2, RusGender.Masculine, RusCase.Locative, "трех целых четырнадцати сотых")]
    [TestCase("-876.11222333444", 11, RusGender.Masculine, RusCase.Instrumental, "минус восемьюстами семьюдесятью шестью целыми одиннадцатью миллиардами двумястами двадцатью двумя миллионами тремястами тридцатью тремя тысячами четыремястами сорока четырьмя стомиллиардными")]
    // Округление
    [TestCase("21.333333333", 4, RusGender.Newter, RusCase.Nominative, "двадцать одна целая три тысячи триста тридцать три десятитысячных")]
    [TestCase("21.333333333", 2, RusGender.Newter, RusCase.Nominative, "двадцать одна целая тридцать три сотых")]
    [TestCase("21.333333333", 0, RusGender.Newter, RusCase.Nominative, "двадцать одно")]
    [TestCase("2.48", 1, RusGender.Masculine, RusCase.Instrumental, "двумя целыми пятью десятыми")]

    public void ToString_Decimal(string sValue, int maxDecimalPlaces, RusGender gender, RusCase theCase, string wantedRes)
    {
      decimal value = StdConvert.ToDecimal(sValue);

      string res1 = RusNumberConvert.ToString(value, maxDecimalPlaces, gender, theCase);
      Assert.AreEqual(wantedRes, res1, "4-args result");
      Assert.IsFalse(res1.Contains(DataTools.NonBreakSpaceStr), "4-args NonBreakSpace");
      Assert.IsFalse(res1.Contains(DataTools.SoftHyphenStr), "4-args SoftHyphen");

      string res2 = RusNumberConvert.ToString(value, maxDecimalPlaces, gender, theCase, false);
      Assert.AreEqual(wantedRes, res2, "specChars=false result");
      Assert.IsFalse(res2.Contains(DataTools.NonBreakSpaceStr), "specChars=false NonBreakSpace");
      Assert.IsFalse(res2.Contains(DataTools.SoftHyphenStr), "specChars=false SoftHyphen");

      string res3 = RusNumberConvert.ToString(value, maxDecimalPlaces, gender, theCase, true);
      res3 = res3.Replace(DataTools.NonBreakSpaceChar, ' ');
      res3 = res3.Replace(DataTools.SoftHyphenStr, "");
      Assert.AreEqual(wantedRes, res3, "specChars=true result");
    }

    #endregion

    #region Сопряжение существительного и числительного

    [TestCase(1, "кошка", "кошки", "кошек", "1 кошка")]
    [TestCase(2, "кошка", "кошки", "кошек", "2 кошки")]
    [TestCase(5, "кошка", "кошки", "кошек", "5 кошек")]
    [TestCase(0, "кошка", "кошки", "кошек", "0 кошек")]
    [TestCase(-3, "кошка", "кошки", "кошек", "-3 кошки")]
    public void IntWithNoun(int value, string nominativeSingular,
      string genitiveSingular, string genitivePlural, string wantedRes)
    {
      string res = RusNumberConvert.IntWithNoun(value, nominativeSingular, genitiveSingular, genitivePlural);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase(0, 3)]
    [TestCase(1, 1)]
    [TestCase(2, 2)]
    [TestCase(3, 2)]
    [TestCase(4, 2)]
    [TestCase(5, 3)]
    [TestCase(6, 3)]
    [TestCase(7, 3)]
    [TestCase(8, 3)]
    [TestCase(9, 3)]
    [TestCase(10, 3)]
    [TestCase(11, 3)]
    [TestCase(20, 3)]
    [TestCase(21, 1)]
    [TestCase(22, 2)]
    [TestCase(25, 3)]
    [TestCase(100, 3)]
    [TestCase(101, 1)]
    [TestCase(1000, 3)]
    [TestCase(1001, 1)]
    [TestCase(1100, 3)]
    [TestCase(1101, 1)]
    [TestCase(-1, 1)]
    [TestCase(-2, 2)]
    [TestCase(-3, 2)]
    [TestCase(-4, 2)]
    [TestCase(-5, 3)]
    public void NounForInt(int value, int wantedForm)
    {
      string res = RusNumberConvert.NounForInt(value, "1", "2", "3");
      Assert.AreEqual(StdConvert.ToString(wantedForm), res);
    }

    #endregion
  }
}
