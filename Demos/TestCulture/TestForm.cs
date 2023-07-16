using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using FreeLibSet.Forms;
using FreeLibSet.Calendar;

namespace TestCulture
{
  public partial class TestForm : Form
  {
    #region Конструктор

    public TestForm()
    {
      InitializeComponent();

      this.Text = CultureInfo.CurrentCulture.DisplayName + " (" + CultureInfo.CurrentCulture.ToString() + ")";

      EFPFormProvider efpForm = new EFPFormProvider(this);

      #region Управляющие элементы

      efpDTP = new EFPDateTimePicker(efpForm, edDTP);

      efpDBs = new EFPDateTimeBox[5];
      efpDBs[0] = new EFPDateTimeBox(efpForm, edDB1);
      efpDBs[1] = new EFPDateTimeBox(efpForm, edDB2);
      efpDBs[2] = new EFPDateTimeBox(efpForm, edDB3);
      efpDBs[3] = new EFPDateTimeBox(efpForm, edDB4);
      efpDBs[4] = new EFPDateTimeBox(efpForm, edDB5);
      for (int i = 0; i < efpDBs.Length; i++)
        efpDBs[i].CanBeEmpty = true;

      efpDRB = new EFPDateRangeBox(efpForm, edDRB);

      efpDoRB = new EFPDateOrRangeBox(efpForm, cbDoRB);

      efpYMB = new EFPYearMonthBox(efpForm, edYMB);

      efpYMRB = new EFPYearMonthRangeBox(efpForm, edYMRB);

      efpMDB = new EFPMonthDayBox(efpForm, edMDB);

      efpDEB = new EFPDecimalEditBox(efpForm, edDEB);

      #endregion

      #region Табличный просмотр

      efpGr1 = new EFPDataGridView(efpForm, gr1);
      efpGr1.Control.AutoGenerateColumns = false;
      efpGr1.Columns.AddDate("Date", true, "Date");
      efpGr1.Columns.AddDateTime("DateTime", true, "DateTime");
      efpGr1.Columns.AddFixedPoint("FixedPoint", true, "FixedPoint", 15, 2, "FixedPoiunt");
      efpGr1.DisableOrdering();
      efpGr1.ReadOnly = true;
      efpGr1.Control.ReadOnly = true;
      efpGr1.CanView = false;
      efpGr1.Control.AllowUserToAddRows = true;
      efpGr1.Control.AllowUserToDeleteRows = true;

      #endregion
    }

    #endregion

    #region Управляющие элементы

    EFPDateTimePicker efpDTP;
    EFPDateTimeBox[] efpDBs;
    EFPDateRangeBox efpDRB;
    EFPDateOrRangeBox efpDoRB;
    EFPYearMonthBox efpYMB;
    EFPYearMonthRangeBox efpYMRB;
    EFPMonthDayBox efpMDB;
    EFPDecimalEditBox efpDEB;

    static DateTime? vDTP;
    static DateTime?[] vDBs = new DateTime?[5];
    static DateTime? vDRB1, vDRB2;
    static DateRange vDoRB;
    static YearMonth vYMB;
    static YearMonthRange vYMRB;
    static MonthDay vMDB;
    static decimal? vDEB;

    #endregion

    #region Табличный просмотр

    EFPDataGridView efpGr1;

    #endregion

    #region Статический метод запуска

    internal static void PerformTest()
    {
      TestForm frm = new TestForm();

      for (int i = 0; i < vDBs.Length; i++)
        frm.efpDBs[i].NValue = vDBs[i];

      frm.efpDTP.NValue = vDTP;
      frm.efpDRB.First.NValue = vDRB1;
      frm.efpDRB.Last.NValue = vDRB2;
      frm.efpDoRB.DateRange = vDoRB;
      if (!vYMB.IsEmpty)
        frm.efpYMB.YM = vYMB;
      if (!vYMRB.IsEmpty)
        frm.efpYMRB.YMRange = vYMRB;
      if (!vMDB.IsEmpty)
        frm.efpMDB.Value = vMDB;

      frm.efpDEB.NValue = vDEB;

      DataTable table = new DataTable();
      table.Columns.Add("Date", typeof(DateTime));
      table.Columns.Add("DateTime", typeof(DateTime));
      table.Columns.Add("FixedPoint", typeof(decimal));
      DateTime tm = DateTime.Now;
      for (int i = 0; i <= 100; i++)
        table.Rows.Add(tm.Date.AddDays(i), tm.AddMinutes(i), i * 100);
      frm.efpGr1.Control.DataSource = table.DefaultView;

      if (EFPApp.ShowDialog(frm, true) != DialogResult.OK)
        return;

      for (int i = 0; i < vDBs.Length; i++)
        vDBs[i] = frm.efpDBs[i].NValue;

      vDTP = frm.efpDTP.NValue;
      vDRB1 = frm.efpDRB.First.NValue;
      vDRB2 = frm.efpDRB.Last.NValue;
      vDoRB = frm.efpDoRB.DateRange;
      vYMB = frm.efpYMB.YM;
      vYMRB = frm.efpYMRB.YMRange;
      vMDB = frm.efpMDB.Value;

      vDEB = frm.efpDEB.NValue;
    }

    #endregion
  }
}
