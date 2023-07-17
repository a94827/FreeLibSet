using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Formatting;
using System.Threading;
using System.Globalization;
using System.Reflection;
using FreeLibSet.Core;

namespace ExtTools_tests.Formatting
{
  [TestFixture]
  class EditableDateTimeFormatterTests
  {
    #region Конструктор

    [TestCase(EditableDateTimeFormatterKind.Date)]
    [TestCase(EditableDateTimeFormatterKind.ShortTime)]
    [TestCase(EditableDateTimeFormatterKind.Time)]
    [TestCase(EditableDateTimeFormatterKind.ShortDateTime)]
    [TestCase(EditableDateTimeFormatterKind.DateTime)]
    public void Constructor(EditableDateTimeFormatterKind kind)
    {
      CultureInfo ci = CultureInfo.GetCultureInfo("en-US");
      Assert.IsNotNull(ci, "CultureInfo exists");

      EditableDateTimeFormatter sut = new EditableDateTimeFormatter(ci, kind);

      Assert.AreEqual(kind, sut.Kind, "Kind");


      bool wantedContainsDate;
      bool wantedContainsTime;
      int wantedDigitCount;
      bool wantedDefaultYearSupported = false;
      switch (kind)
      {
        case EditableDateTimeFormatterKind.Date:
          wantedContainsDate = true;
          wantedContainsTime = false;
          wantedDigitCount = 8;
          wantedDefaultYearSupported = true;
          break;
        case EditableDateTimeFormatterKind.ShortTime:
          wantedContainsDate = false;
          wantedContainsTime = true;
          wantedDigitCount = 4;
          break;
        case EditableDateTimeFormatterKind.Time:
          wantedContainsDate = false;
          wantedContainsTime = true;
          wantedDigitCount = 6;
          break;
        case EditableDateTimeFormatterKind.ShortDateTime:
          wantedContainsDate = true;
          wantedContainsTime = true;
          wantedDigitCount = 8 + 4;
          break;
        case EditableDateTimeFormatterKind.DateTime:
          wantedContainsDate = true;
          wantedContainsTime = true;
          wantedDigitCount = 8 + 6;
          break;
        default:
          throw new ArgumentException("Unknown kind");
      }

      Assert.AreEqual(wantedContainsDate, sut.ContainsDate, "ContainsDate");
      Assert.AreEqual(wantedContainsTime, sut.ContainsTime, "ContainsTime");

      Assert.AreEqual(wantedContainsDate, sut.Format.IndexOf("yyyy", StringComparison.Ordinal) >= 0, "Format - year");
      Assert.AreEqual(wantedContainsDate, sut.Format.IndexOf("MM", StringComparison.Ordinal) >= 0, "Format - month");
      Assert.AreEqual(wantedContainsDate, sut.Format.IndexOf("dd", StringComparison.Ordinal) >= 0, "Format - day");
      Assert.AreEqual(wantedContainsTime, sut.Format.IndexOf("hh", StringComparison.Ordinal) >= 0, "Format - hour");
      Assert.AreEqual(wantedContainsTime, sut.Format.IndexOf("mm", StringComparison.Ordinal) >= 0, "Format - minute");
      Assert.AreEqual(kind == EditableDateTimeFormatterKind.Time || kind == EditableDateTimeFormatterKind.DateTime,
        sut.Format.IndexOf("ss", StringComparison.Ordinal) >= 0, "Format - second");

      Assert.AreEqual(wantedDigitCount, DataTools.GetCharCount(sut.EditMask, '0'), "EditMask - digital places");

      Assert.AreSame(ci.DateTimeFormat, sut.FormatProvider, "FormatProvider");
      Assert.Greater(sut.TextWidth, wantedDigitCount, "TextWidth");
      Assert.AreEqual(sut.EditMask, sut.MaskProvider.EditMask, "MaskProvider.EditMask");
      Assert.AreEqual(wantedDefaultYearSupported, sut.DefaultYearSupported, "DefaultYearSupported");
    }

    #endregion

    #region ToString()

    [TestCase(EditableDateTimeFormatterKind.Date, "07/17/2023")]
    [TestCase(EditableDateTimeFormatterKind.ShortTime, "12:34 PM")]
    [TestCase(EditableDateTimeFormatterKind.Time, "12:34:56 PM")]
    [TestCase(EditableDateTimeFormatterKind.ShortDateTime, "07/17/2023 12:34 PM")]
    [TestCase(EditableDateTimeFormatterKind.DateTime, "07/17/2023 12:34:56 PM")]
    public void ToString(EditableDateTimeFormatterKind kind, string wantedRes)
    {
      CultureInfo ci = CultureInfo.GetCultureInfo("en-US");
      EditableDateTimeFormatter sut = new EditableDateTimeFormatter(ci, kind);
      DateTime dt = new DateTime(2023, 7, 17, 12, 34, 56);
      Assert.AreEqual(wantedRes, sut.ToString(dt));
    }

    #endregion

    #region Parse(), TryParse()

    [TestCase(EditableDateTimeFormatterKind.Date, "07/17/2023")]
    [TestCase(EditableDateTimeFormatterKind.ShortTime, "12:34 PM")]
    [TestCase(EditableDateTimeFormatterKind.Time, "12:34:56 PM")]
    [TestCase(EditableDateTimeFormatterKind.ShortDateTime, "07/17/2023 12:34 PM")]
    [TestCase(EditableDateTimeFormatterKind.DateTime, "07/17/2023 12:34:56 PM")]
    public void Parse_TryParse_success(EditableDateTimeFormatterKind kind, string s)
    {
      CultureInfo ci = CultureInfo.GetCultureInfo("en-US");
      EditableDateTimeFormatter sut = new EditableDateTimeFormatter(ci, kind);

      DateTime res1;
      Assert.IsTrue(sut.TryParse(s, out res1), "TryParse()-1 result");
      DoParse_TryParse_success(sut, res1, "TryParse()-1");

      DateTime res2;
      Assert.IsTrue(sut.TryParse(s, out res2, 0), "TryParse()-2 result");
      DoParse_TryParse_success(sut, res2, "TryParse()-2");

      DateTime res3 = sut.Parse(s);
      DoParse_TryParse_success(sut, res3, "Parse()-1");

      DateTime res4 = sut.Parse(s, 0);
      DoParse_TryParse_success(sut, res3, "Parse()-2");
    }

    private static void DoParse_TryParse_success(EditableDateTimeFormatter sut, DateTime res, string messagePrefix)
    {
      if (sut.ContainsDate)
        Assert.AreEqual(new DateTime(2023, 7, 17), res.Date, messagePrefix + " - Date");

      switch (sut.Kind)
      {
        case EditableDateTimeFormatterKind.ShortTime:
        case EditableDateTimeFormatterKind.ShortDateTime:
          Assert.AreEqual(new TimeSpan(12, 34, 0), res.TimeOfDay, messagePrefix + " - Time");
          break;

        case EditableDateTimeFormatterKind.Time:
        case EditableDateTimeFormatterKind.DateTime:
          Assert.AreEqual(new TimeSpan(12, 34, 56), res.TimeOfDay, messagePrefix + " - Time");
          break;
      }
    }

    public void Parse_TryParse_withDefaultYear()
    {
      CultureInfo ci = CultureInfo.GetCultureInfo("en-US");
      EditableDateTimeFormatter sut = new EditableDateTimeFormatter(ci, EditableDateTimeFormatterKind.Date);

      DateTime res1;
      Assert.IsTrue(sut.TryParse("07/17", out res1, 1984), "TryParse()-1 result");
      Assert.AreEqual(new DateTime(1984, 7, 17), res1, "TryParse() Date");

      DateTime res2 = sut.Parse("07/17", 1984);
      Assert.AreEqual(new DateTime(1984, 7, 17), res1, "Parse() Date");
    }


    [TestCase(EditableDateTimeFormatterKind.Date, "07/32/2023")]
    [TestCase(EditableDateTimeFormatterKind.ShortTime, "25:00 PM")]
    [TestCase(EditableDateTimeFormatterKind.Time, "25:00:00 PM")]
    [TestCase(EditableDateTimeFormatterKind.ShortDateTime, "07/32/2023 12:34 PM")]
    [TestCase(EditableDateTimeFormatterKind.DateTime, "07/32/2023 12:34:56 PM")]
    public void Parse_TryParse_error(EditableDateTimeFormatterKind kind, string s)
    {
      CultureInfo ci = CultureInfo.GetCultureInfo("en-US");
      EditableDateTimeFormatter sut = new EditableDateTimeFormatter(ci, kind);

      DateTime res1;
      Assert.IsFalse(sut.TryParse(s, out res1), "TryParse()-1 result");

      DateTime res2;
      Assert.IsFalse(sut.TryParse(s, out res2, 0), "TryParse()-2 result");

      Assert.Catch<FormatException>(delegate () { sut.Parse(s); }, "Parse()-1");
      Assert.Catch<FormatException>(delegate () { sut.Parse(s, 0); }, "Parse()-2");
    }

    [TestCase(EditableDateTimeFormatterKind.Date)]
    [TestCase(EditableDateTimeFormatterKind.ShortTime)]
    [TestCase(EditableDateTimeFormatterKind.Time)]
    [TestCase(EditableDateTimeFormatterKind.ShortDateTime)]
    [TestCase(EditableDateTimeFormatterKind.DateTime)]
    public void Parse_TryParse_emptyString(EditableDateTimeFormatterKind kind)
    {
      CultureInfo ci = CultureInfo.GetCultureInfo("en-US");
      EditableDateTimeFormatter sut = new EditableDateTimeFormatter(ci, kind);

      DateTime res1;
      Assert.IsFalse(sut.TryParse(String.Empty, out res1), "TryParse()-1 result");

      DateTime res2;
      Assert.IsFalse(sut.TryParse(String.Empty, out res2, 0), "TryParse()-2 result");

      Assert.Catch<FormatException>(delegate () { sut.Parse(String.Empty); }, "Parse()-1");
      Assert.Catch<FormatException>(delegate () { sut.Parse(String.Empty, 0); }, "Parse()-2");
    }

    #endregion

    #region ToNValue()


    [TestCase(EditableDateTimeFormatterKind.Date, "07/17/2023")]
    [TestCase(EditableDateTimeFormatterKind.ShortTime, "12:34 PM")]
    [TestCase(EditableDateTimeFormatterKind.Time, "12:34:56 PM")]
    [TestCase(EditableDateTimeFormatterKind.ShortDateTime, "07/17/2023 12:34 PM")]
    [TestCase(EditableDateTimeFormatterKind.DateTime, "07/17/2023 12:34:56 PM")]
    public void ToNValue_success(EditableDateTimeFormatterKind kind, string s)
    {
      CultureInfo ci = CultureInfo.GetCultureInfo("en-US");
      EditableDateTimeFormatter sut = new EditableDateTimeFormatter(ci, kind);

      DateTime? res1 = sut.ToNValue(s);
      Assert.IsTrue(res1.HasValue, "ToNValue()-1 HasValue");
      DoParse_TryParse_success(sut, res1.Value, "ToNValue()-1");

      DateTime? res2 = sut.ToNValue(s, 0);
      Assert.IsTrue(res2.HasValue, "ToNValue()-1 HasValue");
      DoParse_TryParse_success(sut, res2.Value, "ToNValue()-1");
    }

    public void ToNValue_withDefaultYear()
    {
      CultureInfo ci = CultureInfo.GetCultureInfo("en-US");
      EditableDateTimeFormatter sut = new EditableDateTimeFormatter(ci, EditableDateTimeFormatterKind.Date);

      DateTime? res1 = sut.ToNValue("07/17", 1984);
      Assert.AreEqual(new DateTime(1984, 7, 17), res1);
    }

    [TestCase(EditableDateTimeFormatterKind.Date, "07/32/2023")]
    [TestCase(EditableDateTimeFormatterKind.ShortTime, "25:00 PM")]
    [TestCase(EditableDateTimeFormatterKind.Time, "25:00:00 PM")]
    [TestCase(EditableDateTimeFormatterKind.ShortDateTime, "07/32/2023 12:34 PM")]
    [TestCase(EditableDateTimeFormatterKind.DateTime, "07/32/2023 12:34:56 PM")]
    public void ToNValue_error(EditableDateTimeFormatterKind kind, string s)
    {
      CultureInfo ci = CultureInfo.GetCultureInfo("en-US");
      EditableDateTimeFormatter sut = new EditableDateTimeFormatter(ci, kind);

      DateTime? res1 = sut.ToNValue(s);
      Assert.IsFalse(res1.HasValue, "ToNValue()-1 HasValue");

      DateTime? res2 = sut.ToNValue(s, 0);
      Assert.IsFalse(res2.HasValue, "ToNValue()-1 HasValue");
    }

    [TestCase(EditableDateTimeFormatterKind.Date)]
    [TestCase(EditableDateTimeFormatterKind.ShortTime)]
    [TestCase(EditableDateTimeFormatterKind.Time)]
    [TestCase(EditableDateTimeFormatterKind.ShortDateTime)]
    [TestCase(EditableDateTimeFormatterKind.DateTime)]
    public void ToNValue_emptyString(EditableDateTimeFormatterKind kind)
    {
      CultureInfo ci = CultureInfo.GetCultureInfo("en-US");
      EditableDateTimeFormatter sut = new EditableDateTimeFormatter(ci, kind);

      DateTime? res1 = sut.ToNValue(String.Empty);
      Assert.IsFalse(res1.HasValue, "ToNValue()-1 HasValue");

      DateTime? res2 = sut.ToNValue(String.Empty, 0);
      Assert.IsFalse(res2.HasValue, "ToNValue()-1 HasValue");
    }

    #endregion
  }
}
