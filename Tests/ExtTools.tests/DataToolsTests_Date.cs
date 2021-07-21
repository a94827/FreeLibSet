using AgeyevAV;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtTools.tests
{
  [TestFixture]
  class DataToolsTests_Date
  {
    #region Преобразования из константных строк

    /// <summary>
    /// Соответствует DateTime.MinValue
    /// </summary>
    const string MinDate = "00010101";

    /// <summary>
    /// Соответствует DateTime.MaxValue.Date
    /// </summary>
    const string MaxDate = "99991231";

    private static DateTime GetDate(string s)
    {
      return DateTime.ParseExact(s, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static DateTime? GetNDate(string s)
    {
      if (s.Length == 0)
        return null;
      else
        return GetDate(s);
    }

    private static string DateToString(DateTime value)
    {
      return value.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string NDateToString(DateTime? value)
    {
      if (value.HasValue)
        return DateToString(value.Value);
      else
        return "";
    }

    [Test]
    public void Constants()
    {
      Assert.AreEqual(DateTime.MinValue, GetDate(MinDate));
      Assert.AreEqual(DateTime.MaxValue.Date, GetDate(MaxDate));
    }

    #endregion

    [TestCase(MinDate, "", "", Result = true)]
    [TestCase(MaxDate, "", "", Result = true)]
    [TestCase("20210714", "20210714", "20210714", Result = true)]
    [TestCase("20210714", "20210713", "20210713", Result = false)]
    [TestCase("20210714", "20210715", "20210715", Result = false)]
    [TestCase("20210714", "20210714", "", Result = true)]
    [TestCase("20210714", "20210715", "", Result = false)]
    [TestCase("20210714", "", "20210714", Result = true)]
    [TestCase("20210714", "", "20210713", Result = false)]
    public bool DateInRange(string testDate, string firstDate, string lastDate)
    {
      return DataTools.DateInRange(GetDate(testDate), GetNDate(firstDate), GetNDate(lastDate));
    }

    [TestCase("20210701", "20210710", "20210710", "20210711", Result = true)]
    [TestCase("20210701", "20210710", "20210630", "20210701", Result = true)]
    [TestCase("20210701", "20210710", "20210711", "20210711", Result = false)]
    [TestCase("20210701", "20210710", "20210630", "20210630", Result = false)]
    [TestCase("", "", "20210714", "20210714", Result = true)]
    [TestCase("20210714", "20210714", "", "", Result = true)]
    [TestCase("", "20210714", "20210714", "", Result = true)]
    [TestCase("", "20210714", "20210715", "", Result = false)]
    [TestCase("20210714", "", "", "20210714", Result = true)]
    [TestCase("20210715", "", "", "20210714", Result = false)]
    [TestCase("", "", "", "", Result = true)]
    public bool DateRangeCrossed(string firstDate1, string lastDate1, string firstDate2, string lastDate2)
    {
      return DataTools.DateRangeCrossed(GetNDate(firstDate1), GetNDate(lastDate1), GetNDate(firstDate2), GetNDate(lastDate2));
    }

    [TestCase("20210714", "20210101", "20211231", Result = "20210714")]
    [TestCase("20210714", "20210714", "20211231", Result = "20210714")]
    [TestCase("20210714", "20210101", "20210714", Result = "20210714")]
    [TestCase("20210714", "20210715", "20211231", Result = "20210715")]
    [TestCase("20210714", "20210101", "20210713", Result = "20210713")]
    [TestCase("20210714", "20210714", "20211231", Result = "20210714")]
    [TestCase("20210714", "", "20210714", Result = "20210714")]
    [TestCase("20210714", "", "20210713", Result = "20210713")]
    [TestCase("20210714", "20210714", "", Result = "20210714")]
    [TestCase("20210714", "20210715", "", Result = "20210715")]
    public string DateToRange(string testDate, string firstDate, string lastDate)
    {
      DateTime dt = GetDate(testDate);
      DataTools.DateToRange(ref dt, GetNDate(firstDate), GetNDate(lastDate));
      return DateToString(dt);
    }

    [TestCase("20210714", "20210715", "20210716", "20210717", Result = "F-")]
    [TestCase("20210714", "20210716", "20210715", "20210717", Result = "T20210715-20210716")]
    [TestCase("20210714", "", "20210715", "20210717", Result = "T20210715-20210717")]
    [TestCase("", "20210716", "20210715", "20210717", Result = "T20210715-20210716")]
    [TestCase("20210714", "20210715", "", "", Result = "T20210714-20210715")]
    public string GetDateRangeCross(string firstDate1, string lastDate1, string firstDate2, string lastDate2)
    {
      DateTime? dt1 = GetNDate(firstDate1);
      DateTime? dt2 = GetNDate(lastDate1);

      bool res = DataTools.GetDateRangeCross(ref dt1, ref dt2, GetNDate(firstDate2), GetNDate(lastDate2));

      return (res ? "T" : "F") + NDateToString(dt1) + "-" + NDateToString(dt2);
    }


    [TestCase("20210714", "20210715", "20210716", "20210717", Result = "20210714-20210717")]
    [TestCase("20210801", "20210831", "20210714", "20210714", Result = "20210714-20210831")]
    [TestCase("20210714", "20210715", "20210716", "", Result = "20210714-")]
    [TestCase("20210714", "20210715", "", "20210716", Result = "-20210716")]
    [TestCase("20210714", "", "", "20210716", Result = "-")]
    public string GetDateRangeUnion(string firstDate1, string lastDate1, string firstDate2, string lastDate2)
    {
      DateTime? dt1 = GetNDate(firstDate1);
      DateTime? dt2 = GetNDate(lastDate1);

      DataTools.GetDateRangeUnion(ref dt1, ref dt2, GetNDate(firstDate2), GetNDate(lastDate2));

      return NDateToString(dt1) + "-" + NDateToString(dt2);
    }


    [TestCase(2021, "20211231", "20211231", Result = true)]
    [TestCase(2022, "20211231", "20211231", Result = false)]
    [TestCase(2021, "", "20210101", Result = true)]
    [TestCase(2021, "20211231", "", Result = true)]
    [TestCase(2021, "", "20201231", Result = false)]
    [TestCase(2021, "20220101", "", Result = false)]
    [TestCase(9999, "", "", Result = true)]
    public bool YearInRange(int year, string firstDate, string lastDate)
    {
      return DataTools.YearInRange(year, GetNDate(firstDate), GetNDate(lastDate));
    }
  }
}
