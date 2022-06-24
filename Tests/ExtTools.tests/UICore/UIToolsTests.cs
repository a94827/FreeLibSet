using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.UICore;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.DependedValues;
using System.Globalization;

namespace ExtTools_tests.UICore
{
  [TestFixture]
  public class UIToolsTests
  {
    #region Text <-> Lines

    [Test]
    public void TextToLines()
    {
      string s = "AAA" + Environment.NewLine + "BBB";
      string[] res = UITools.TextToLines(s);
      Assert.AreEqual(new string[] { "AAA", "BBB" }, res);
    }

    [Test]
    public void TextToLines_empty()
    {
      string[] res = UITools.TextToLines(String.Empty);
      Assert.AreEqual(DataTools.EmptyStrings, res);
    }

    [Test]
    public void TextToLines_null()
    {
      string[] res = UITools.TextToLines(null);
      Assert.AreEqual(DataTools.EmptyStrings, res);
    }


    [Test]
    public void LinesToText()
    {
      string[] a = new string[] { "AAA", "BBB" };
      string res = UITools.LinesToText(a);
      Assert.AreEqual(a[0] + Environment.NewLine + a[1], res);
    }

    [Test]
    public void LinesToText_empty()
    {
      string res = UITools.LinesToText(DataTools.EmptyStrings);
      Assert.AreEqual(String.Empty, res);
    }

    [Test]
    public void LinesToText_null()
    {
      string res = UITools.LinesToText(null);
      Assert.AreEqual(String.Empty, res);
    }

    #endregion

    #region ShiftDateRange(), ShiftDateRangeYear()

    [TestCase("20220623", "20220623", true, true, "20220624", "20220624")] // один день
    [TestCase("20220623", "20220623", false, true, "20220622", "20220622")]
    [TestCase("20220601", "20220629", true, true, "20220630", "20220728")] // неполный месяц
    [TestCase("20220601", "20220629", false, true, "20220503", "20220531")]
    [TestCase("20220601", "20220630", true, true, "20220701", "20220731")] // полный месяц
    [TestCase("20220601", "20220630", false, true, "20220501", "20220531")]
    [TestCase("20220623", "20220622", true, false, "20220623", "20220622")] // неправильный исходный интервал
    [TestCase("", "20220623", true, true, "20220624", "")] // полуоткрытый интервал
    [TestCase("", "20220623", false, false, "", "20220623")]
    [TestCase("20220623", "", false, true, "", "20220622")]
    [TestCase("20220623", "", true, false, "20220623", "")]
    [TestCase("", "", true, false, "", "")] // пустой интервал
    [TestCase("", "", false, false, "", "")]
    public void ShiftDateRange(string sArg1, string sArg2, bool forward, bool wantedRes, string sRes1, string sRes2)
    {
      DateTime? dt1 = Creators.CreateNDate(sArg1);
      DateTime? dt2 = Creators.CreateNDate(sArg2);
      DateTime? res1 = Creators.CreateNDate(sRes1);
      DateTime? res2 = Creators.CreateNDate(sRes2);

      bool res = UITools.ShiftDateRange(ref dt1, ref dt2, forward);
      Assert.AreEqual(wantedRes, res, "Result");
      Assert.AreEqual(res1, dt1, "FirstDate");
      Assert.AreEqual(res2, dt2, "LastDate");
    }

    [TestCase("20220623", "20220623", true, true, "20230623", "20230623")] // один день
    [TestCase("20220623", "20220623", false, true, "20210623", "20210623")]
    [TestCase("20200201", "20200228", true, true, "20210201", "20210228")] // неполный месяц
    [TestCase("20200201", "20200228", false, true, "20190201", "20190228")]
    [TestCase("20200201", "20200229", true, true, "20210201", "20210228")] // полный месяц
    [TestCase("20200201", "20200229", false, true, "20190201", "20190228")]
    [TestCase("20190201", "20190228", true, true, "20200201", "20200229")]
    [TestCase("20200201", "20200229", false, true, "20190201", "20190228")]
    [TestCase("", "", false, false, "", "")]
    public void ShiftDateRangeYear(string sArg1, string sArg2, bool forward, bool wantedRes, string sRes1, string sRes2)
    {
      DateTime? dt1 = Creators.CreateNDate(sArg1);
      DateTime? dt2 = Creators.CreateNDate(sArg2);
      DateTime? res1 = Creators.CreateNDate(sRes1);
      DateTime? res2 = Creators.CreateNDate(sRes2);

      bool res = UITools.ShiftDateRangeYear(ref dt1, ref dt2, forward);
      Assert.AreEqual(wantedRes, res, "Result");
      Assert.AreEqual(res1, dt1, "FirstDate");
      Assert.AreEqual(res2, dt2, "LastDate");
    }

    #endregion

    #region CreateValidateResultEx()

    private class ChangeCounter
    {
      public int Count;

      public void Changed(object sender, EventArgs args)
      {
        Count++;
      }
    }

    [Test]
    public void CreateValidateResultEx()
    {
      DepInput<bool> arg = new DepInput<bool>();
      arg.Value = false;

      DepValue<UIValidateResult> res = UITools.CreateValidateResultEx(arg, "ABC");
      ChangeCounter counter = new ChangeCounter();
      res.ValueChanged += counter.Changed;

      Assert.IsFalse(res.Value.IsValid, "IsValid #1");
      Assert.AreEqual("ABC", res.Value.Message, "Message #1");
      Assert.AreEqual(0, counter.Count, "Count #1");

      arg.Value = true;
      Assert.IsTrue(res.Value.IsValid, "IsValid #2");
      Assert.IsTrue(String.IsNullOrEmpty(res.Value.Message), "Message #2");
      Assert.AreEqual(1, counter.Count, "Count #1");

      arg.Value = false;
      Assert.IsFalse(res.Value.IsValid, "IsValid #3");
      Assert.AreEqual("ABC", res.Value.Message, "Message #3");
      Assert.AreEqual(2, counter.Count, "Count #3");
    }

    #endregion

    #region CorrectNumberString()

    [TestCase("1 234,56", "1234,56")]
    [TestCase("1 234.56", "1234,56")]
    [TestCase("1 234 567", "1234567")]
    [TestCase("", "")]
    public void CorrectNumberString_ruRu(string arg, string wantedRes)
    {
      UITools.CorrectNumberString(ref arg, CultureInfo.GetCultureInfo("ru-RU").NumberFormat);
      Assert.AreEqual(wantedRes, arg);
    }

    [Test]
    public void CorrectNumberString_allCultures()
    {
      foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
      {
        Console.WriteLine("Testing " + ci.ToString() + "...");
        double v1 = 1234567.89;
        string s1 = v1.ToString("#.##", ci.NumberFormat);
        double v2=0;
        Assert.DoesNotThrow(delegate() { v2 = double.Parse(s1, NumberStyles.Number, ci.NumberFormat); }, "parsing #1 \"" + s1 + "\", Culture: " + ci.ToString());
        Assert.AreEqual(v1, v2, "unchanged string \"" + s1 + "\", Culture: " + ci.ToString());

        string s2 = s1;
        UITools.CorrectNumberString(ref s2, ci.NumberFormat);
        double v3 = 0;
        Assert.DoesNotThrow(delegate() { v3 = double.Parse(s2, NumberStyles.Number, ci.NumberFormat); }, "parsing #2 \"" + s2 + "\", Culture: " + ci.ToString());
        Assert.AreEqual(v1, v3, "changed string \"" + s2 + "\", Culture: " + ci.ToString());
      }
    }

    #endregion
  }
}
