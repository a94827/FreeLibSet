using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Formatting;
using System.Threading;
using System.Globalization;
using System.ComponentModel;
using FreeLibSet.Tests;

namespace ExtTools_tests.Formatting
{
  [TestFixture]
  public class StdMaskProviderTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_MaskedTextProvider()
    {
      MaskedTextProvider mtp = new MaskedTextProvider("00");
      StdMaskProvider sut = new StdMaskProvider(mtp);

      Assert.AreSame(mtp, sut.Provider, "Provider");
      Assert.AreSame(mtp.Culture, sut.Culture, "Culture");
      Assert.AreEqual(mtp.Mask, sut.EditMask, "EditMask");
    }

    [Test]
    public void Constructor_Mask()
    {
      string editMask = "00";
      StdMaskProvider sut = new StdMaskProvider(editMask);

      Assert.IsNotNull(sut.Provider, "Provider");
      Assert.AreEqual(editMask, sut.EditMask, "EditMask");
      Assert.AreEqual(editMask, sut.Provider.Mask, "Provider.Mask");
      Assert.AreSame(CultureInfo.CurrentCulture, sut.Culture, "Culture");
      Assert.AreSame(CultureInfo.CurrentCulture, sut.Provider.Culture, "Provider.Culture");
    }


    [Test]
    public void Constructor_Mask_Culture()
    {
      string editMask = "0.0";
      CultureInfo culture = CultureInfo.GetCultureInfo("ru-RU");
      StdMaskProvider sut = new StdMaskProvider(editMask, culture);

      Assert.IsNotNull(sut.Provider, "Provider");
      Assert.AreEqual(editMask, sut.EditMask, "EditMask");
      Assert.AreEqual(editMask, sut.Provider.Mask, "Provider.Mask");
      Assert.AreSame(culture, sut.Culture, "Culture");
      Assert.AreSame(culture, sut.Provider.Culture, "Provider.Culture");
    }

    #endregion

    #region Test()

    [TestCase("0.0", null, "3.5", true)]
    [TestCase("0.0", null, "3,5", false)]
    [TestCase("0.0", "en-US", "3.5", true)]
    [TestCase("0.0", "en-US", "3,5", false)]
    [TestCase("0.0", "ru-RU", "3.5", false)]
    [TestCase("0.0", "ru-RU", "3,5", true)]
    [TestCase("0.0", null, "", false)]
    public void Test(string mask, string sCulture, string text, bool wantedRes)
    {
      CultureInfo culture = CultureInfo.InvariantCulture;
      if (!String.IsNullOrEmpty(sCulture))
        culture = CultureInfo.GetCultureInfo(sCulture);
      Assert.IsNotNull(culture, "Culture exists");

      StdMaskProvider sut = new StdMaskProvider(mask, culture);

      bool res1 = sut.Test(text);
      Assert.AreEqual(wantedRes, res1, "Result #1");

      string errorText;
      bool res2 = sut.Test(text, out errorText);
      Assert.AreEqual(wantedRes, res2, "Result #2");
      Assert.AreEqual(wantedRes, String.IsNullOrEmpty(errorText), "ErrorText #2");
    }

    [Test]
    public void Test_multithread()
    {
      StdMaskProvider sut = new StdMaskProvider("00000", CultureInfo.InvariantCulture);
      Thread[] thrds = new Thread[8];
      Exception[] es = new Exception[thrds.Length];
      for (int i = 0; i < thrds.Length; i++)
      {
        thrds[i] = new Thread(delegate () { es[i] = DoTestOneThread(sut); });
        thrds[i].Start();
      }

      for (int i = 0; i < thrds.Length; i++)
        thrds[0].Join();

      for (int i = 0; i < thrds.Length; i++)
      {
        if (es[i] != null)
          throw (es[i]);
      }
    }

    private static Exception DoTestOneThread(StdMaskProvider sut)
    {
      try
      {
        DateTime dtStart = DateTime.Now;
        DateTime dtEnd = dtStart + TimeSpan.FromMilliseconds(500);
        Random rnd = new Random();
        while (DateTime.Now <= dtEnd)
        {
          string text = rnd.Next(180000).ToString("00000", CultureInfo.InvariantCulture); // в половине случаев соответствует маске, в половине - нет
          sut.Test(text);
        }
        return null;
      }
      catch (Exception e)
      {
        return e;
      }
    }

    #endregion
  }
}
