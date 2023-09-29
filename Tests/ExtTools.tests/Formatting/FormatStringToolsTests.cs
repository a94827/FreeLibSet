using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Formatting;
using System.Threading;
using System.Globalization;

namespace ExtTools_tests.Formatting
{
  [TestFixture]
  public class FormatStringToolsTests
  {
    #region DecimalPlacesFrom/ToNumberFormat()

    [TestCase(0, "0")]
    [TestCase(1, "0.0")]
    [TestCase(3, "0.000")]
    [TestCase(-1, "")]
    public void DecimalPlacesToNumberFormat(int decimalPlaces, string wantedRes)
    {
      Assert.AreEqual(wantedRes, FormatStringTools.DecimalPlacesToNumberFormat(decimalPlaces));
    }

    [TestCase("0", 0)]
    [TestCase("0.00", 2)]
    [TestCase("0.00##", 4)]
    [TestCase("", -1)]
    [TestCase("yyyy/MM/dd", -1)]
    public void DecimalPlacesFromNumberFormat(string format, int wantedRes)
    {
      Assert.AreEqual(wantedRes, FormatStringTools.DecimalPlacesFromNumberFormat(format));
    }

    #endregion

    #region ContainsDateTime()

    // Односимвольные стандартные форматы
    [TestCase("d", true, false)]
    [TestCase("D", true, false)]
    [TestCase("f", true, true)]
    [TestCase("F", true, true)]
    [TestCase("g", true, true)]
    [TestCase("G", true, true)]
    [TestCase("m", true, false)]
    [TestCase("M", true, false)]
    [TestCase("o", true, true)]
    [TestCase("O", true, true)]
    [TestCase("r", true, true)]
    [TestCase("R", true, true)]
    [TestCase("s", true, true)]
    [TestCase("t", false, true)]
    [TestCase("T", false, true)]
    [TestCase("u", true, true)]
    [TestCase("U", true, true)]
    [TestCase("y", true, false)]
    [TestCase("Y", true, false)]

    // Составные форматы
    [TestCase("yyyy/MM/dd", true, false)]
    [TestCase("hh:mm:ss", false, true)]
    [TestCase("yyyy/MM/dd hh:mm:ss", true, true)]
    [TestCase("yy", true, false)]
    [TestCase("yyyy", true, false)]
    [TestCase("MM", true, false)]
    [TestCase("dd", true, false)]

    [TestCase("HH", false, true)]
    [TestCase("hh", false, true)]
    [TestCase("mm", false, true)]
    [TestCase("ss", false, true)]

    // Посторонние форматы
    // пока не будем проверять [TestCase("", true, true)]
    [TestCase("0", false, false)]
    [TestCase("0.0", false, false)]
    [TestCase("0.0##", false, false)]
    public void ContainsDateTime_ContainsDate_ContainsTime(string format, bool wantedDate, bool wantedTime)
    {
      bool containsDate, containsTime;
      FormatStringTools.ContainsDateTime(format, out containsDate, out containsTime);
      Assert.AreEqual(wantedDate, containsDate, "ContainsDateTime() - Date");
      Assert.AreEqual(wantedTime, containsTime, "ContainsDateTime() - Time");

      Assert.AreEqual(wantedDate, FormatStringTools.ContainsDate(format), "ContainsDate()");

      Assert.AreEqual(wantedTime, FormatStringTools.ContainsTime(format), "ContainsTime()");
    }

    #endregion

    #region GetDateFormatOrder()

    [TestCase("yyyy/MM/dd hh:mm:ss", DateFormatYMDOrder.YMD)]
    [TestCase("dd/MM/yyyy hh:mm:ss", DateFormatYMDOrder.DMY)]
    [TestCase("M/d/yy hh:mm:ss", DateFormatYMDOrder.MDY)]
    [TestCase("M/d/yy hh:mm:ss", DateFormatYMDOrder.MDY)]
    public void GetDateFormatOrder_custom(string format, DateFormatYMDOrder wantedRes)
    {
      Assert.AreEqual(wantedRes, FormatStringTools.GetDateFormatOrder(format));
    }

    [TestCase("O", DateFormatYMDOrder.YMD)]
    [TestCase("o", DateFormatYMDOrder.YMD)]
    [TestCase("R", DateFormatYMDOrder.DMY)]
    [TestCase("r", DateFormatYMDOrder.DMY)]
    [TestCase("s", DateFormatYMDOrder.YMD)]
    [TestCase("u", DateFormatYMDOrder.YMD)]
    public void GetDateFormatOrder_international(string format, DateFormatYMDOrder wantedRes)
    {
      CultureInfo oldCI = Thread.CurrentThread.CurrentCulture;
      try
      {
        // Можно было бы проверить все культуры, только смысла нет
        foreach (string culture in new string[] { "en-US", "ru-RU", "ja-JP" })
        {
          Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
          Assert.AreEqual(wantedRes, FormatStringTools.GetDateFormatOrder(format), "Culture=\"" + culture + "\"");
        }
      }
      finally
      {
        Thread.CurrentThread.CurrentCulture = oldCI;
      }
    }

    [TestCase("ru-RU", DateFormatYMDOrder.DMY)]
    [TestCase("en-US", DateFormatYMDOrder.MDY)]
    [TestCase("ja-JP", DateFormatYMDOrder.YMD)]
    public void GetDateFormatOrder_standard(string culture, DateFormatYMDOrder wantedRes)
    {
      CultureInfo ci = CultureInfo.GetCultureInfo(culture);
      Assert.IsNotNull(ci, "Culture Supported");

      CultureInfo oldCI = Thread.CurrentThread.CurrentCulture;
      try
      {
        foreach (string format in new string[] { "d", "D", "f", "F", "g", "G", "", "t", "T" })
        {
          Thread.CurrentThread.CurrentCulture = ci;
          Assert.AreEqual(wantedRes, FormatStringTools.GetDateFormatOrder(format), "1 arg, Format=\"" + format + "\"");

          Thread.CurrentThread.CurrentCulture = oldCI;
          Assert.AreEqual(wantedRes, FormatStringTools.GetDateFormatOrder(format, ci.DateTimeFormat), "2 args, Format=\"" + format + "\"");
        }
      }
      finally
      {
        Thread.CurrentThread.CurrentCulture = oldCI;
      }
    }

    [Test]
    public void DateFormatOrder()
    {
      DateFormatYMDOrder wanted = FormatStringTools.GetDateFormatOrder("d");
      Assert.AreEqual(wanted, FormatStringTools.DateFormatOrder);
    }

    #endregion

    #region ExpandDateTimeFormat()

    public void ExpandDateTimeFormat([Values("en-US", "ru-RU", "ja-JP")] string culture,
      [Values("d", "D", "f", "F", "g", "G", "M", "m", "O", "o", "R", "r", "s", "t", "T", "u", "U", "Y", "y",
      "", "yy/MM/dd", "hh:mm:ss")] string format)
    {
      DateTime dt = new DateTime(2023, 7, 13, 12, 34, 56);

      CultureInfo ci = CultureInfo.GetCultureInfo(culture);
      Assert.IsNotNull(ci, "Culture Supported");

      CultureInfo oldCI = Thread.CurrentThread.CurrentCulture;
      try
      {
        Thread.CurrentThread.CurrentCulture = ci;
        string wantedStr = dt.ToString(format);

        string format1 = FormatStringTools.ExpandDateTimeFormat(format);
        string str1 = dt.ToString(format1);
        Assert.AreEqual(wantedStr, str1, "1-arg, Expanded format=\"" + format1 + "\"");

        Thread.CurrentThread.CurrentCulture = oldCI;
        string format2 = FormatStringTools.ExpandDateTimeFormat(format, ci.DateTimeFormat);
        string str2 = dt.ToString(format2, ci);
        Assert.AreEqual(wantedStr, str2, "2-args, Expanded format=\"" + format2 + "\"");
      }
      finally
      {
        Thread.CurrentThread.CurrentCulture = oldCI;
      }
    }

    #endregion
  }
}
