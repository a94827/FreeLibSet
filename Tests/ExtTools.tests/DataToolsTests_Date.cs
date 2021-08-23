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


    #endregion

    [Test]
    public void Constants()
    {
      Assert.AreEqual(DateTime.MinValue, Creators.CreateDate(MinDate));
      Assert.AreEqual(DateTime.MaxValue.Date, Creators.CreateDate(MaxDate));
    }

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
      return DataTools.DateInRange(Creators.CreateDate(testDate),
        Creators.CreateNDate(firstDate), Creators.CreateNDate(lastDate));
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
      return DataTools.DateRangeCrossed(Creators.CreateNDate(firstDate1), Creators.CreateNDate(lastDate1),
        Creators.CreateNDate(firstDate2), Creators.CreateNDate(lastDate2));
    }

    [TestCase("20210714", "20210101", "20211231", "20210714")]
    [TestCase("20210714", "20210714", "20211231", "20210714")]
    [TestCase("20210714", "20210101", "20210714", "20210714")]
    [TestCase("20210714", "20210715", "20211231", "20210715")]
    [TestCase("20210714", "20210101", "20210713", "20210713")]
    [TestCase("20210714", "20210714", "20211231", "20210714")]
    [TestCase("20210714", "", "20210714", "20210714")]
    [TestCase("20210714", "", "20210713", "20210713")]
    [TestCase("20210714", "20210714", "", "20210714")]
    [TestCase("20210714", "20210715", "", "20210715")]
    public void DateToRange(string testDate, string firstDate, string lastDate, string res)
    {
      DateTime dt = Creators.CreateDate(testDate);
      DataTools.DateToRange(ref dt,
        Creators.CreateNDate(firstDate), Creators.CreateNDate(lastDate));
      
      Assert.AreEqual(Creators.CreateDate(res), dt);
    }

    [TestCase("20210714", "20210715", "20210716", "20210717", false, "", "")]
    [TestCase("20210714", "20210716", "20210715", "20210717", true, "20210715", "20210716")]
    [TestCase("20210714", "", "20210715", "20210717", true, "20210715", "20210717")]
    [TestCase("", "20210716", "20210715", "20210717", true, "20210715", "20210716")]
    [TestCase("20210714", "20210715", "", "", true, "20210714", "20210715")]
    public void GetDateRangeCross(string sdt11, string sdt12, string sdt21, string sdt22, bool wanted, string sdt11res, string sdt12res)
    {
      DateTime? dt11 = Creators.CreateNDate(sdt11);
      DateTime? dt12 = Creators.CreateNDate(sdt12);
      DateTime? dt21 = Creators.CreateNDate(sdt21);
      DateTime? dt22 = Creators.CreateNDate(sdt22);

      bool res = DataTools.GetDateRangeCross(ref dt11, ref dt12, dt21, dt22);

      Assert.AreEqual(wanted, res, "Result");
      Assert.AreEqual(Creators.CreateNDate(sdt11res), dt11, "FirstDate1");
      Assert.AreEqual(Creators.CreateNDate(sdt12res), dt12, "LastDate1");
    }


    [TestCase("20210714", "20210715", "20210716", "20210717", "20210714", "20210717")]
    [TestCase("20210801", "20210831", "20210714", "20210714", "20210714", "20210831")]
    [TestCase("20210714", "20210715", "20210716", "", "20210714", "")]
    [TestCase("20210714", "20210715", "", "20210716", "", "20210716")]
    [TestCase("20210714", "", "", "20210716", "", "")]
    public void GetDateRangeUnion(string sdt11, string sdt12, string sdt21, string sdt22, string sdt11res, string sdt12res)
    {
      DateTime? dt11 = Creators.CreateNDate(sdt11);
      DateTime? dt12 = Creators.CreateNDate(sdt12);
      DateTime? dt21 = Creators.CreateNDate(sdt21);
      DateTime? dt22 = Creators.CreateNDate(sdt22);

      DataTools.GetDateRangeUnion(ref dt11, ref dt12, dt21, dt22);

      Assert.AreEqual(Creators.CreateNDate(sdt11res), dt11, "FirstDate1");
      Assert.AreEqual(Creators.CreateNDate(sdt12res), dt12, "LastDate1");
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
      return DataTools.YearInRange(year, Creators.CreateNDate(firstDate), Creators.CreateNDate(lastDate));
    }
  }
}
