using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using System.Globalization;
using AgeyevAV;

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

      efpDB = new EFPDateBox(efpForm, edDB);

      efpDRB = new EFPDateRangeBox(efpForm, edDRB);

      efpDoRB = new EFPDateOrRangeBox(efpForm, cbDoRB);

      efpYMB = new EFPYearMonthBox(efpForm, edYMB);

      efpYMRB = new EFPYearMonthRangeBox(efpForm, edYMRB);

      efpMDB = new EFPMonthDayBox(efpForm, edMDB);

      efpNEB = new EFPNumEditBox(efpForm, edNEB);

      efpNUD1 = new EFPNumericUpDown(efpForm, edNUD1);

      efpNUD2 = new EFPExtNumericUpDown(efpForm, edNUD2);

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
    EFPDateBox efpDB;
    EFPDateRangeBox efpDRB;
    EFPDateOrRangeBox efpDoRB;
    EFPYearMonthBox efpYMB;
    EFPYearMonthRangeBox efpYMRB;
    EFPMonthDayBox efpMDB;
    EFPNumEditBox efpNEB;
    EFPNumericUpDown efpNUD1;
    EFPExtNumericUpDown efpNUD2;

    static DateTime? vDTP;
    static DateTime? vDB;
    static DateTime? vDRB1, vDRB2;
    static DateRange vDoRB;
    static YearMonth vYMB;
    static YearMonthRange vYMRB;
    static MonthDay vMDB;
    static decimal vNEB;
    static int vNUD1 = 1;
    static int vNUD2 = 1;

    #endregion

    #region Табличный просмотр

    EFPDataGridView efpGr1;

    #endregion

    #region Статический метод запуска

    internal static void PerformTest()
    {
      TestForm frm = new TestForm();

      frm.efpDTP.Value = vDTP;
      frm.efpDB.Value = vDB;
      frm.efpDRB.FirstDate.Value = vDRB1;
      frm.efpDRB.LastDate.Value = vDRB2;
      frm.efpDoRB.Value = vDoRB;
      if (!vYMB.IsEmpty)
        frm.efpYMB.YM = vYMB;
      if (!vYMRB.IsEmpty)
        frm.efpYMRB.YMRange = vYMRB;
      if (!vMDB.IsEmpty)
        frm.efpMDB.Value = vMDB;

      frm.efpNEB.DecimalValue = vNEB;
      frm.efpNUD1.IntValue = vNUD1;
      frm.efpNUD2.IntValue = vNUD2;

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

      vDTP = frm.efpDTP.Value;
      vDB = frm.efpDB.Value;
      vDRB1 = frm.efpDRB.FirstDate.Value;
      vDRB2 = frm.efpDRB.LastDate.Value;
      vDoRB = frm.efpDoRB.Value;
      vYMB = frm.efpYMB.YM;
      vYMRB = frm.efpYMRB.YMRange;
      vMDB = frm.efpMDB.Value;

      vNEB = frm.efpNEB.DecimalValue;
      vNUD1 = frm.efpNUD1.IntValue;
      vNUD2 = frm.efpNUD2.IntValue;
    }

    #endregion
  }
}