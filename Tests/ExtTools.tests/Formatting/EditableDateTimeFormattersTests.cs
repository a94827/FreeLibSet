using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.IO;
using FreeLibSet.Core;
using FreeLibSet.Formatting;
using System.Threading;
using System.Globalization;
using System.Reflection;

namespace ExtTools_tests.Formatting
{
  [TestFixture]
  public class EditableDateTimeFormattersTests
  {
    #region Объекты EditableDateTimeFormatter

    [Test]
    public void Date()
    {
      DoTestFormatter(EditableDateTimeFormatterKind.Date, true, false, new DateTime(2023, 7, 13));
    }

    [Test]
    public void Time()
    {
      DoTestFormatter(EditableDateTimeFormatterKind.Time, false, true, new DateTime(2023, 7, 13, 12, 34, 56));
    }
    [Test]
    public void DateTime()
    {
      DoTestFormatter(EditableDateTimeFormatterKind.DateTime, true, true, new DateTime(2023, 7, 13, 12, 34, 56));
    }
    [Test]
    public void ShortTime()
    {
      DoTestFormatter(EditableDateTimeFormatterKind.ShortTime, false, true, new DateTime(2023, 7, 13, 12, 34, 0));
    }
    [Test]
    public void ShortDateTime()
    {
      DoTestFormatter(EditableDateTimeFormatterKind.ShortDateTime, true, true, new DateTime(2023, 7, 13, 12, 34, 0));
    }

    /// <summary>
    /// Список культур, для которых выполняются тесты
    /// </summary>
    public static CultureInfo[] TestCultures
    {
      get { return CultureInfo.GetCultures(CultureTypes.SpecificCultures); }
    }

    private static string MessagePrefix { get { return "Culture=" + CultureInfo.CurrentCulture.ToString() + ". "; } }

    private static void DoTestFormatter(EditableDateTimeFormatterKind kind, bool containsDate, bool containsTime, DateTime testDt)
    {
      string propName = kind.ToString();
      PropertyInfo pi = typeof(EditableDateTimeFormatters).GetProperty(propName);
      Assert.IsNotNull(pi, propName + " - PropertyExists");

      CultureInfo oldCI = Thread.CurrentThread.CurrentCulture;
      try
      {
        foreach (CultureInfo ci in TestCultures)
        {
          Thread.CurrentThread.CurrentCulture = ci;

          EditableDateTimeFormatter sut = pi.GetValue(null, null) as EditableDateTimeFormatter;
          Assert.IsNotNull(sut, MessagePrefix + "IsNotNull");

          Assert.AreSame(sut, EditableDateTimeFormatters.Get(kind), MessagePrefix + "Get()");

          Assert.AreEqual(kind, sut.Kind, MessagePrefix + "Kind");

          Assert.AreEqual(containsDate, sut.ContainsDate, MessagePrefix + "ContainsDate");
          Assert.AreEqual(containsTime, sut.ContainsTime, MessagePrefix + "ContainsTime");

          string s = sut.ToString(testDt);
          DateTime resDt = sut.Parse(s);

          if (containsDate)
            Assert.AreEqual(testDt.Date, resDt.Date, MessagePrefix + "Date");
          if (containsTime)
            Assert.AreEqual(testDt.TimeOfDay, resDt.TimeOfDay, MessagePrefix + "Time");
        }
      }
      finally
      {
        Thread.CurrentThread.CurrentCulture = oldCI;
      }
    }

    #endregion

    #region MonthNames12, MonthGenitiveNames12

    [Test]
    public void MonthNames12()
    {
      CultureInfo oldCI = Thread.CurrentThread.CurrentCulture;
      try
      {
        foreach (CultureInfo ci in TestCultures)
        {
          Thread.CurrentThread.CurrentCulture = ci;

          Assert.AreEqual(12, EditableDateTimeFormatters.MonthNames12.Length, "Length");
          for (int i = 0; i < 12; i++)
            Assert.AreEqual(ci.DateTimeFormat.MonthNames[i], EditableDateTimeFormatters.MonthNames12[i], MessagePrefix + "[" + i.ToString() + "]");
        }
      }
      finally
      {
        Thread.CurrentThread.CurrentCulture = oldCI;
      }
    }

    [Test]
    public void MonthGenitiveNames12()
    {
      CultureInfo oldCI = Thread.CurrentThread.CurrentCulture;
      try
      {
        foreach (CultureInfo ci in TestCultures)
        {
          Thread.CurrentThread.CurrentCulture = ci;

          Assert.AreEqual(12, EditableDateTimeFormatters.MonthGenitiveNames12.Length, "Length");

          for (int i = 0; i < 12; i++)
            Assert.AreEqual(ci.DateTimeFormat.MonthGenitiveNames[i], EditableDateTimeFormatters.MonthGenitiveNames12[i], MessagePrefix + "[" + i.ToString() + "]");
        }
      }
      finally
      {
        Thread.CurrentThread.CurrentCulture = oldCI;
      }
    }

    #endregion
  }
}
