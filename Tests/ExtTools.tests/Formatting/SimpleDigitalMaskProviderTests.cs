using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Formatting;
using System.Globalization;
using System.ComponentModel;

namespace ExtTools_tests.Formatting
{
  [TestFixture]
  public class SimpleDigitalMaskProviderTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_mask()
    {
      SimpleDigitalMaskProvider sut = new SimpleDigitalMaskProvider("00.00");

      Assert.AreEqual("00.00", sut.Mask, "Mask");
      Assert.AreEqual(@"00\.00", sut.EditMask, "EditMask");
      Assert.AreSame(CultureInfo.InvariantCulture, sut.Culture, "Culture");
    }

    [Test]
    public void Constructor_digits()
    {
      SimpleDigitalMaskProvider sut = new SimpleDigitalMaskProvider(5);

      Assert.AreEqual("00000", sut.Mask, "Mask");
      Assert.AreEqual(@"00000", sut.EditMask, "EditMask");
      Assert.AreSame(CultureInfo.InvariantCulture, sut.Culture, "Culture");
    }

    #endregion

    #region Test()

    [TestCase("0.0", "3.5", true)]
    [TestCase("0.0", "3.A", false)]
    [TestCase("0.0", "000", false)]
    [TestCase("0.0", "3.51", false)]
    [TestCase("0.0", "3-5", false)]

    [TestCase("0-0", "3-5", true)]
    [TestCase("0-0", "3.5", false)]

    [TestCase("0:0", "3:5", true)]
    [TestCase("0:0", "3.5", false)]

    [TestCase("0 0", "3 5", true)]
    [TestCase("0 0", "3.5", false)]

    [TestCase("0/0", "3/5", true)]
    [TestCase("0/0", "3.5", false)]

    [TestCase("000-000-000 00", "123-456-789 64", true)]
    [TestCase("000-000-000 00", "123-456-789-64", false)]

    [TestCase("0 (0)", "3 (5)", true)]
    [TestCase("0 (0)", "3 [5]", false)]

    [TestCase("0 [0]", "3 [5]", true)]
    [TestCase("0 [0]", "3 (5)", false)]

    [TestCase("0 {0}", "3 {5}", true)]
    [TestCase("0 {0}", "3 [5]", false)]

    [TestCase("0.0", "", false)]
    public void Test(string mask, string text, bool wantedRes)
    {
      SimpleDigitalMaskProvider sut = new SimpleDigitalMaskProvider(mask);

      bool res1 = sut.Test(text);
      Assert.AreEqual(wantedRes, res1, "Result #1");

      string errorText;
      bool res2 = sut.Test(text, out errorText);
      Assert.AreEqual(wantedRes, res2, "Result #2");
      Assert.AreEqual(wantedRes, String.IsNullOrEmpty(errorText), "ErrorText #2");

      // Используем дополнительную проверку через MaskedTextProvider
      // Цель - проверка корректности EditMask
      MaskedTextProvider mtp = new MaskedTextProvider(sut.EditMask, CultureInfo.InvariantCulture);
      StdMaskProvider test3 = new StdMaskProvider(mtp); // в методе StdMaskProvider.Test() есть необходимая для теста логика
      bool res3 = test3.Test(text);
      Assert.AreEqual(wantedRes, res3, "Result #3");
    }

    #endregion
  }
}
