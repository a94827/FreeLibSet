using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Globalization;
using NUnit.Framework;
using FreeLibSet.UICore;
using FreeLibSet.Core;
using FreeLibSet.DependedValues;
using FreeLibSet.Tests;
using FreeLibSet.Formatting;
using System.Text.RegularExpressions;
using System.ComponentModel;
using FreeLibSet.Calendar;

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
      DateTime? dt1 = Creators.NDateTime(sArg1);
      DateTime? dt2 = Creators.NDateTime(sArg2);
      DateTime? res1 = Creators.NDateTime(sRes1);
      DateTime? res2 = Creators.NDateTime(sRes2);

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
      DateTime? dt1 = Creators.NDateTime(sArg1);
      DateTime? dt2 = Creators.NDateTime(sArg2);
      DateTime? res1 = Creators.NDateTime(sRes1);
      DateTime? res2 = Creators.NDateTime(sRes2);

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
        double v2 = 0;
        Assert.DoesNotThrow(delegate () { v2 = double.Parse(s1, NumberStyles.Number, ci.NumberFormat); }, "parsing #1 \"" + s1 + "\", Culture: " + ci.ToString());
        Assert.AreEqual(v1, v2, "unchanged string \"" + s1 + "\", Culture: " + ci.ToString());

        string s2 = s1;
        UITools.CorrectNumberString(ref s2, ci.NumberFormat);
        double v3 = 0;
        Assert.DoesNotThrow(delegate () { v3 = double.Parse(s2, NumberStyles.Number, ci.NumberFormat); }, "parsing #2 \"" + s2 + "\", Culture: " + ci.ToString());
        Assert.AreEqual(v1, v3, "changed string \"" + s2 + "\", Culture: " + ci.ToString());
      }
    }

    #endregion

    #region GetMaskedText()

    [TestCase("00/00/0000", "07/19/2023", "07/19/2023")]
    [TestCase("00/00/0000", "07/19", "07/19")]
    [TestCase("00.##", "12.  ", "12")]
    [TestCase("00.##", "12.3 ", "12.3")]
    [TestCase("00.##", "12.34", "12.34")]
    [TestCase(@"[00\.00]", "[", "")]
    [TestCase(@"[00\.00]", "[0", "[0")]
    [TestCase(@"[00\.00]", "[00", "[00")]
    [TestCase(@"[00\.00]", "[00.0", "[00.0")]
    [TestCase(@"[00\.00]", "[00.00", "[00.00]")]
    public void GetMaskedText(string mask, string input, string wantedRes)
    {
      MaskedTextProvider provider = new MaskedTextProvider(mask, CultureInfo.GetCultureInfo("en-US"));
      provider.Set(input);

      string res = UITools.GetMaskedText(provider);

      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region IsNormalCharacterCasing()

    // не допускается конструктором [TestCase(@"", true)]
    [TestCase(@"0", false)]
    [TestCase(@"9", false)]
    [TestCase(@"#", false)]
    [TestCase(@"0.00#", false)]
    [TestCase(@"CC", true)]
    [TestCase(@">CC", false)]
    [TestCase(@"<CC", false)]
    [TestCase(@"\>CC", true)]
    [TestCase(@"000 >CC", false)]
    [TestCase(@"000 <CC", false)]
    [TestCase(@"000 \<CC", true)]
    public void IsNormalCharacterCasing(string mask, bool wantedRes)
    {
      MaskedTextProvider provider = new MaskedTextProvider(mask);
      bool res = UITools.IsNormalCharacterCasing(provider);
      Assert.AreEqual(wantedRes, res);
    }

    [Test]
    public void IsNormalCharacterCasing_null()
    {
      MaskedTextProvider provider = null;
      bool res = UITools.IsNormalCharacterCasing(provider);
      Assert.IsTrue(res);
    }

    #endregion

    #region GetGuidEditMask(), GetGuidRegEx()

    [Test]
    public void GetGuidEditMask([Values("", "N", "D", "B", "P")]string format, [Values(false, true)] bool upperCase)
    {
      string editMask = UITools.GetGuidEditMask(format, upperCase);
      StdMaskProvider mp = new StdMaskProvider(editMask);
      Assert.AreEqual(32, mp.Provider.EditPositionCount, "EditPositionCount");

      Guid g = new Guid("373D38E8-6845-496A-9B66-154FAD471BBA");
      string text = g.ToString(format);
      Assert.IsTrue(mp.Test(text), "Valid format: " + text);

      string badText = text.Replace('3', '!');
      Assert.IsFalse(mp.Test(badText), "Bad format: " + badText);
    }

    [Test]
    public void GetGuidRegEx([Values("", "N", "D", "B", "P")]string format, [Values(false, true)] bool upperCase)
    {
      string pattern = UITools.GetGuidRegEx(format);
      Guid g = new Guid("373D38E8-6845-496A-9B66-154FAD471BBA");

      string text = g.ToString(format);
      if (upperCase)
        text = text.ToUpperInvariant();

      Assert.IsTrue(Regex.IsMatch(text, pattern), "Valid format: " + text);

      string badText = text.Replace('D', 'G').Replace('d', 'g');
      Assert.IsFalse(Regex.IsMatch(badText, pattern), "Bad format: " + badText);
    }

    #endregion

    #region Validate

    [TestCase(UIValidateState.Ok)]
    [TestCase(UIValidateState.Warning)]
    [TestCase(UIValidateState.Error)]
    public void ValidateCanBeEmptyMode(UIValidateState canBeEmptyMode)
    {
      UISimpleValidableObject obj = new UISimpleValidableObject();
      Assert.AreEqual(UIValidateState.Ok, obj.ValidateState, "Before call");

      UITools.ValidateCanBeEmptyMode(canBeEmptyMode, obj, "XXX");
      Assert.AreEqual(canBeEmptyMode, obj.ValidateState, "ValidateState");

      if (canBeEmptyMode != UIValidateState.Ok)
        StringAssert.Contains("XXX", obj.Message);
    }

    [TestCase(0, 1, 2, false)]
    [TestCase(1, 1, 2, true)]
    [TestCase(2, 1, 2, true)]
    [TestCase(3, 1, 2, false)]
    [TestCase(0, 1, null, false)]
    [TestCase(1, 1, null, true)]
    [TestCase(2, 1, null, true)]
    [TestCase(0, null, 1, true)]
    [TestCase(1, null, 1, true)]
    [TestCase(2, null, 1, false)]
    [TestCase(1, null, null, true)]
    public void ValidateInRange_T(int value, int? minimum, int? maximum, bool wantedIsOk)
    {
      UISimpleValidableObject obj = new UISimpleValidableObject();
      UITools.ValidateInRange<int>(value, minimum, maximum, obj, "XXX", true, null, null);

      foreach (bool isError in new bool[] { true, false })
      {

        UIValidateState wantedRes = wantedIsOk ? UIValidateState.Ok : UIValidateState.Error;
        Assert.AreEqual(wantedRes, obj.ValidateState, "ValidateState");
      }
    }

    [Test]
    public void ValidateInRange_T_text()
    {
      UISimpleValidableObject obj = new UISimpleValidableObject();
      UITools.ValidateInRange<int>(123, 456, 789, obj, "XXX", true, null, null);
      StringAssert.Contains("XXX", obj.Message);
      //StringAssert.Contains("123", obj.Message);
      StringAssert.Contains("456", obj.Message);
      StringAssert.Contains("789", obj.Message);
    }

    [TestCase(true, UIValidateState.Error)]
    [TestCase(false, UIValidateState.Warning)]
    public void ValidateInRange_T_isError(bool isError, UIValidateState wantedState)
    {
      UISimpleValidableObject obj = new UISimpleValidableObject();
      UITools.ValidateInRange<int>(123, 456, 789, obj, "XXX", isError, null, null);
      Assert.AreEqual(wantedState, obj.ValidateState);
    }

    // Для остальных перегрузок не делаем отдельные тесты для isError и Text.

    [TestCase("20250129", "20250130", "20250131", false)]
    [TestCase("20250130", "20250130", "20250131", true)]
    [TestCase("20250129", "20250130", "", false)]
    [TestCase("20250129", "", "20250131", true)]
    [TestCase("20250129", "", "", true)]
    public void ValidateInRange_DateTime(string sValue, string sMinimum, string sMaximum, bool wantedIsOk)
    {
      DateTime value = Creators.DateTime(sValue);
      DateTime? minimum = Creators.NDateTime(sMinimum);
      DateTime? maximum = Creators.NDateTime(sMaximum);

      foreach (bool isError in new bool[] { true, false })
      {
        UISimpleValidableObject obj = new UISimpleValidableObject();
        UITools.ValidateInRange(value, minimum, maximum, obj, "XXX", isError);

        UIValidateState wantedRes = wantedIsOk ? UIValidateState.Ok : (isError ? UIValidateState.Error : UIValidateState.Warning);
        Assert.AreEqual(wantedRes, obj.ValidateState, "ValidateState");
        if (!wantedIsOk)
        {
          StringAssert.Contains("XXX", obj.Message, "DisplayName");
          StringAssert.Contains(DateRangeFormatter.Default.ToString(minimum, maximum, true), obj.Message, "Range");
        }
      }
    }


    [TestCase("20250129", "20250130-20250131", false)]
    [TestCase("20250130", "20250130-20250131", true)]
    [TestCase("20250129", "", false)] // в отличие от перегрузки с двумя датами
    public void ValidateInRange_DateTime_DateRange(string sValue, string sRange, bool wantedIsOk)
    {
      DateTime value = Creators.DateTime(sValue);
      DateRange range = Creators.DateRange(sRange);

      foreach (bool isError in new bool[] { true, false })
      {
        UISimpleValidableObject obj = new UISimpleValidableObject();
        UITools.ValidateInRange(value, range, obj, "XXX", isError);

        UIValidateState wantedRes = wantedIsOk ? UIValidateState.Ok : (isError ? UIValidateState.Error : UIValidateState.Warning);
        Assert.AreEqual(wantedRes, obj.ValidateState, "ValidateState");
        if (!wantedIsOk)
        {
          StringAssert.Contains("XXX", obj.Message, "DisplayName");
          StringAssert.Contains(DateRangeFormatter.Default.ToString(range, true), obj.Message, "DateRange");
        }
      }
    }

    [TestCase("202411", "202412", "202502", false)]
    [TestCase("202412", "202412", "202501", true)]
    [TestCase("202412", "202412", "", true)]
    [TestCase("202411", "202412", "", false)]
    [TestCase("202412", "", "202412", true)]
    [TestCase("202501", "", "202412", false)]
    [TestCase("202501", "", "", true)]
    [TestCase("", "", "", false)]
    public void ValidateInRange_YearMonth(string sValue, string sMinimum, string sMaximum, bool wantedIsOk)
    {
      YearMonth value = Creators.YearMonth(sValue);
      YearMonth minimum = Creators.YearMonth(sMinimum);
      YearMonth maximum = Creators.YearMonth(sMaximum);

      foreach (bool isError in new bool[] { true, false })
      {
        UISimpleValidableObject obj = new UISimpleValidableObject();
        UITools.ValidateInRange(value, minimum, maximum, obj, "XXX", isError);

        UIValidateState wantedRes = wantedIsOk ? UIValidateState.Ok : (isError ? UIValidateState.Error : UIValidateState.Warning);
        Assert.AreEqual(wantedRes, obj.ValidateState, "ValidateState");
        if (!wantedIsOk)
        {
          StringAssert.Contains("XXX", obj.Message, "DisplayName");
          StringAssert.Contains(DateRangeFormatter.Default.ToString(minimum, maximum), obj.Message, "Range");
        }
      }
    }

    [TestCase("202501", "202412-202501", true)] 
    [TestCase("202502", "202412-202501", false)]
    [TestCase("202501", "", false)] // в отличие от перегрузки с двумя YearMonth
    public void ValidateInRange_YearMonth_YearMonthRange(string sValue, string sRange, bool wantedIsOk)
    {
      YearMonth value = Creators.YearMonth(sValue);
      YearMonthRange range = Creators.YearMonthRange(sRange);

      foreach (bool isError in new bool[] { true, false })
      {
        UISimpleValidableObject obj = new UISimpleValidableObject();
        UITools.ValidateInRange(value, range, obj, "XXX", isError);

        UIValidateState wantedRes = wantedIsOk ? UIValidateState.Ok : (isError ? UIValidateState.Error : UIValidateState.Warning);
        Assert.AreEqual(wantedRes, obj.ValidateState, "ValidateState");
        if (!wantedIsOk)
        {
          StringAssert.Contains("XXX", obj.Message, "DisplayName");
          // способ форматирования диапазона не определен
          // StringAssert.Contains(DateRangeFormatter.Default.ToString(range, true), obj.Message, "DateRange");
        }
      }
    }

    [TestCase("0128", "0129-0201", false)]
    [TestCase("0130", "0129-0201", true)]
    [TestCase("", "0129-0201", false)]
    [TestCase("0130", "", false)]
    [TestCase("", "", false)]
    public void ValidateInRange_MonthDay_MonthDayRange(string sValue, string sRange, bool wantedIsOk)
    {
      MonthDay value = Creators.MonthDay(sValue);
      MonthDayRange range = Creators.MonthDayRange(sRange);

      foreach (bool isError in new bool[] { true, false })
      {
        UISimpleValidableObject obj = new UISimpleValidableObject();
        UITools.ValidateInRange(value, range, obj, "XXX", isError);

        UIValidateState wantedRes = wantedIsOk ? UIValidateState.Ok : (isError ? UIValidateState.Error : UIValidateState.Warning);
        Assert.AreEqual(wantedRes, obj.ValidateState, "ValidateState");
        if (!wantedIsOk)
        {
          StringAssert.Contains("XXX", obj.Message, "DisplayName");
          StringAssert.Contains(DateRangeFormatter.Default.ToString(range, true), obj.Message, "DateRange");
        }
      }
    }

    #endregion

    #region ToString()

    [Test]
    public void ToString_Type()
    {
      Assert.IsNotEmpty(UITools.ToString((Type)null), "null");
      Assert.IsNotEmpty(UITools.ToString(typeof(DBNull)), "DBNull");
      Assert.IsNotEmpty(UITools.ToString(typeof(Int32)), "Int32");
      Assert.IsNotEmpty(UITools.ToString(typeof(Int16)), "Int16");
      Assert.IsNotEmpty(UITools.ToString(typeof(Single)), "Single");
      Assert.IsNotEmpty(UITools.ToString(typeof(Double)), "Double");
      Assert.IsNotEmpty(UITools.ToString(typeof(Decimal)), "Decimal");
      Assert.IsNotEmpty(UITools.ToString(typeof(String)), "String");
      Assert.IsNotEmpty(UITools.ToString(typeof(Boolean)), "Boolean");
      Assert.IsNotEmpty(UITools.ToString(typeof(DateTime)), "DateTime");
      Assert.IsNotEmpty(UITools.ToString(typeof(TimeSpan)), "TimeSpan");
      Assert.IsNotEmpty(UITools.ToString(typeof(Guid)), "Guid");
      Assert.IsNotEmpty(UITools.ToString(typeof(IBindingList)), "interface");
    }

    [Test]
    public void ToString_EditableDateTimeFormatterKind()
    {
      foreach (EditableDateTimeFormatterKind value in Enum.GetValues(typeof(EditableDateTimeFormatterKind)))
        Assert.IsNotEmpty(UITools.ToString(value), value.ToString());
    }

    [Test]
    public void ToString_MaskedTextResultHint()
    {
      foreach (MaskedTextResultHint value in Enum.GetValues(typeof(MaskedTextResultHint)))
        Assert.IsNotEmpty(UITools.ToString(value), value.ToString());
    }

    [Test]
    public void ToString_ListSortDirection()
    {
      foreach (ListSortDirection value in Enum.GetValues(typeof(ListSortDirection)))
      {
        Assert.IsNotEmpty(UITools.ToString(value), value.ToString() + " - default");
        Assert.IsNotEmpty(UITools.ToString(value, true), value.ToString() + " - long");
        Assert.IsNotEmpty(UITools.ToString(value, false), value.ToString() + " - short");
        Assert.AreEqual(UITools.ToString(value, true), UITools.ToString(value), "long form is defaults");
      }
    }

    [Test]
    public void ToString_ErrorMessageKind()
    {
      foreach (ErrorMessageKind value in Enum.GetValues(typeof(ErrorMessageKind)))
      {
        Assert.IsNotEmpty(UITools.ToString(value), value.ToString() + " - default");
        Assert.IsNotEmpty(UITools.ToString(value, true), value.ToString() + " - long");
        Assert.IsNotEmpty(UITools.ToString(value, false), value.ToString() + " - short");
        Assert.AreEqual(UITools.ToString(value, true), UITools.ToString(value), "long form is defaults");
      }
    }

    [Test]
    public void ConvertErrorMessage_Type()
    {
      string s = "ABCD";
      Assert.IsNotEmpty(UITools.ConvertErrorMessage(s, typeof(Int32)), "Int32");
      Assert.IsNotEmpty(UITools.ConvertErrorMessage(s, typeof(Int16)), "Int16");
      Assert.IsNotEmpty(UITools.ConvertErrorMessage(s, typeof(Single)), "Single");
      Assert.IsNotEmpty(UITools.ConvertErrorMessage(s, typeof(Double)), "Double");
      Assert.IsNotEmpty(UITools.ConvertErrorMessage(s, typeof(Decimal)), "Decimal");
      Assert.IsNotEmpty(UITools.ConvertErrorMessage(s, typeof(DateTime)), "DateTime");
      Assert.IsNotEmpty(UITools.ConvertErrorMessage(s, typeof(Boolean)), "Boolean");
      Assert.IsNotEmpty(UITools.ConvertErrorMessage(s, typeof(Guid)), "Guid");
    }


    [Test]
    public void ConvertErrorMessage_EditableDateTimeFormatterKind()
    {
      foreach (EditableDateTimeFormatterKind kind in Enum.GetValues(typeof(EditableDateTimeFormatterKind)))
        Assert.IsNotEmpty(UITools.ConvertErrorMessage("ABCD", kind), kind.ToString());
    }

    [Test]
    public void ToYesNo()
    {
      Assert.IsNotEmpty(UITools.ToYesNo(true));
      Assert.IsNotEmpty(UITools.ToYesNo(false));
    }

    [Test]
    public void ToString_Format_null()
    {
      object value = null;
      string res = UITools.ToString(value, "XXX", CultureInfo.InvariantCulture);
      Assert.AreEqual("null", res);
    }

    private class NotFormattibleObj
    {
      public override string ToString()
      {
        return "ABC";
      }
    }

    [Test]
    public void ToString_Format_notFormattible()
    {
      object value = new NotFormattibleObj();
      string res = UITools.ToString(value, "XXX", CultureInfo.InvariantCulture);
      Assert.AreEqual("ABC", res);
    }


    [Test]
    public void ToString_Format_formattible()
    {
      object value = 123.45;

      string res1 = UITools.ToString(value, "0.0", CultureInfo.GetCultureInfo("en-US"));
      Assert.AreEqual("123.5", res1, "en-US");

      string res2 = UITools.ToString(value, "0.0", CultureInfo.GetCultureInfo("ru-RU"));
      Assert.AreEqual("123,5", res2, "ru-RU");
    }

    #endregion
  }
}
