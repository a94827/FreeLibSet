using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using FreeLibSet.Forms;
using FreeLibSet.Core;
using FreeLibSet.Formatting;

namespace WinFormsDemo.CultureDemo
{
  public partial class CultureSelForm : Form
  {
    #region Конструктор формы

    public CultureSelForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      #region Выбор культуры

      AllCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
      SortedDictionary<string, CultureInfo> dict = new SortedDictionary<string, CultureInfo>();
      for (int i = 0; i < AllCultures.Length; i++)
        dict[AllCultures[i].DisplayName] = AllCultures[i];
      dict[CultureInfo.CurrentCulture.DisplayName] = CultureInfo.CurrentCulture; // заменяем текущей культурой
      AllCultures = new CultureInfo[dict.Count];
      dict.Values.CopyTo(AllCultures, 0);

      efpCulture = new EFPListComboBox(efpForm, cbCulture);
      string[] names = new string[AllCultures.Length];
      int selIndex = 0;
      for (int i = 0; i < AllCultures.Length; i++)
      {
        names[i] = AllCultures[i].DisplayName + " (" + AllCultures[i].Name + ")";
        if (Object.ReferenceEquals(AllCultures[i], CultureInfo.CurrentCulture))
        {
          names[i] += " (current)";
          selIndex = i;
        }
      }
      cbCulture.Items.AddRange(names);
      efpCulture.SelectedIndex = selIndex;
      efpCulture.SelectedIndexEx.ValueChanged += new EventHandler(efpCulture_ValueChanged);

      btnShowTable.Image = EFPApp.MainImages.Images["Table"];
      btnShowTable.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpShowTable = new EFPButton(efpForm, btnShowTable);
      efpShowTable.DisplayName = "CultureInfo table";
      efpShowTable.Click += EfpShowTable_Click;

      #endregion

      InfoTime = DateTime.Now;
      InfoValue = 123456789.12m;

      #region Таблица параметров

      efpInfo = new EFPDataGridView(efpForm, grInfo);
      efpInfo.Columns.AddText("Param", false, "Param", 20, 10);
      efpInfo.Columns.AddTextFill("Value", false, "Value", 50, 10);
      efpInfo.Columns.AddTextFill("Sample", false, "Sample", 50, 10);
      efpInfo.DisableOrdering();
      efpInfo.ReadOnly = true;
      efpInfo.Control.ReadOnly = true;
      efpInfo.CanView = false;
      grInfo.RowCount = InfoTitles.Length;
      grInfo.VirtualMode = true;
      efpInfo.CellInfoNeeded += efpInfo_CellInfoNeeded;

      #endregion

      #region Стандартные форматы

      efpFormats = new EFPDataGridView(efpForm, grFormats);
      efpFormats.Columns.AddText("Format", false, "Format", 6, 1);
      efpFormats.Columns.AddTextFill("Value", false, "Value", 100, 10);
      efpFormats.DisableOrdering();
      efpFormats.ReadOnly = true;
      efpFormats.Control.ReadOnly = true;
      efpFormats.CanView = false;
      grFormats.RowCount = FormatTitles.Length;
      grFormats.VirtualMode = true;
      efpFormats.CellInfoNeeded += efpFormats_CellInfoNeeded;

      #endregion

      #region EditableDateTimeFormatter

      efpEditableDateTimeFormatter = new EFPDataGridView(efpForm, grEditableDateTimeFormatter);
      efpEditableDateTimeFormatter.Columns.AddText("Kind", false, "Kind", 10, 1);
      efpEditableDateTimeFormatter.Columns.AddText("Format", false, "Format", 10, 1);
      efpEditableDateTimeFormatter.Columns.AddText("EditMask", false, "EditMask", 12, 10);
      efpEditableDateTimeFormatter.Columns.AddInteger("TextWidth", false, "TextWidth", 4);
      efpEditableDateTimeFormatter.Columns.AddCheckBox("DefaultYearSupported", false, "DefaultYearSupported");
      efpEditableDateTimeFormatter.DisableOrdering();
      efpEditableDateTimeFormatter.ReadOnly = true;
      efpEditableDateTimeFormatter.Control.ReadOnly = true;
      efpEditableDateTimeFormatter.CanView = false;
      efpEditableDateTimeFormatter.Control.RowCount = DataTools.GetEnumRange(typeof(EditableDateTimeFormatterKind)).MaxValue + 1;
      efpEditableDateTimeFormatter.Control.VirtualMode = true;
      efpEditableDateTimeFormatter.CellInfoNeeded += efpEditableDateTimeFormatter_CellInfoNeeded;

      #endregion


      #region Кнопка

      btnTest.Image = EFPApp.MainImages.Images["Ok"];
      btnTest.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpTest = new EFPButton(efpForm, btnTest);
      efpTest.Click += new EventHandler(efpTest_Click);

      #endregion
    }

    #endregion

    #region Список культур

    CultureInfo[] AllCultures;

    EFPListComboBox efpCulture;

    void efpCulture_ValueChanged(object sender, EventArgs args)
    {
      efpInfo.PerformRefresh();
      grInfo.Invalidate();
      efpFormats.PerformRefresh();
      grFormats.Invalidate();

      _Formatters = null;
      efpEditableDateTimeFormatter.PerformRefresh();
      efpEditableDateTimeFormatter.Control.Invalidate();
    }

    #endregion

    #region Таблица со списком культур

    private void EfpShowTable_Click(object sender, EventArgs args)
    {
      DataTable table = new DataTable();
      table.Columns.Add("Name", typeof(string));
      table.Columns.Add("DisplayName", typeof(string));
      table.Columns.Add("EnglishName", typeof(string));
      table.Columns.Add("NativeName", typeof(string));
      table.Columns.Add("IsNeutralCulture", typeof(bool));
      table.Columns.Add("LCID", typeof(int));
      table.Columns.Add("ThreeLetterWindowsLanguageName", typeof(string));
      table.Columns.Add("ThreeLetterISOLanguageName", typeof(string));
      table.Columns.Add("TwoLetterISOLanguageName", typeof(string));

      table.Columns.Add("FormatStringTools.DateFormatOrder", typeof(string));

      foreach (CultureInfo ci in AllCultures)
      {
        DataRow row = table.NewRow();
        DataTools.SetString(row, "Name", ci.Name);
        try
        {
          DataTools.SetString(row, "DisplayName", ci.DisplayName);
          DataTools.SetString(row, "EnglishName", ci.EnglishName);
          DataTools.SetString(row, "NativeName", ci.NativeName);
          row["IsNeutralCulture"] = ci.IsNeutralCulture;
          row["LCID"] = ci.LCID;
          DataTools.SetString(row, "ThreeLetterWindowsLanguageName", ci.ThreeLetterWindowsLanguageName);
          DataTools.SetString(row, "ThreeLetterISOLanguageName", ci.ThreeLetterISOLanguageName);
          DataTools.SetString(row, "TwoLetterISOLanguageName", ci.TwoLetterISOLanguageName);
        }
        catch { }

        try
        {
          row["FormatStringTools.DateFormatOrder"] = FormatStringTools.GetDateFormatOrder("d", ci.DateTimeFormat);
        }
        catch (Exception e)
        {
          row["FormatStringTools.DateFormatOrder"] = e.Message;
        }

        table.Rows.Add(row);
      }
      table.DefaultView.Sort = "DisplayName";

      SimpleGridForm form = new SimpleGridForm();
      form.Text = "All cultures";
      EFPDataGridView efpGr = new EFPDataGridView(form.ControlWithToolBar);
      efpGr.Control.AutoGenerateColumns = true;
      efpGr.ReadOnly = true;
      efpGr.Control.ReadOnly = true;
      efpGr.CanView = false;
      efpGr.AutoSort = true;
      efpGr.CustomOrderAllowed = true;
      efpGr.CustomOrderActive = true;
      efpGr.Control.DataSource = table.DefaultView;

      EFPApp.ShowDialog(form, true);
    }

    #endregion

    #region Список параметров

    EFPDataGridView efpInfo;

    DateTime InfoTime;
    decimal InfoValue;


    private static readonly string[] InfoTitles = new string[] {
          "ShortDatePattern",
          "DateSeparator",
          "LongDatePattern",
          "ShortTimePattern",
          "TimeSeparator",
          "LongTimePattern",
          "NumberDecimalSeparator",
          "NumberGroupSeparator",
          "NumberGroupSizes",
          "FormatStringTools.DateFormatOrder"};

    void efpInfo_CellInfoNeeded(object sender, EFPDataGridViewCellInfoEventArgs args)
    {
      if (args.ColumnIndex < 0 || args.RowIndex < 0)
        return;

      if (args.ColumnIndex == 0)
      {
        args.Value = InfoTitles[args.RowIndex];
      }
      else
      {
        if (efpCulture.SelectedIndex < 0)
          return;
        CultureInfo ci = AllCultures[efpCulture.SelectedIndex];

        if (args.ColumnIndex == 1)
        {
          switch (args.RowIndex)
          {
            case 0: args.Value = ci.DateTimeFormat.ShortDatePattern; break;
            case 1: args.Value = ci.DateTimeFormat.DateSeparator; break;
            case 2: args.Value = ci.DateTimeFormat.LongDatePattern; break;
            case 3: args.Value = ci.DateTimeFormat.ShortTimePattern; break;
            case 4: args.Value = ci.DateTimeFormat.TimeSeparator; break;
            case 5: args.Value = ci.DateTimeFormat.LongTimePattern; break;

            case 6: args.Value = ci.NumberFormat.NumberDecimalSeparator; break;
            case 7: args.Value = ci.NumberFormat.NumberGroupSeparator; break;
            case 8: args.Value = StdConvert.ToString(ci.NumberFormat.NumberGroupSizes); break;
            case 9: args.Value = FormatStringTools.GetDateFormatOrder("d", ci.DateTimeFormat); break;
          }
        }
        else
        {
          switch (args.RowIndex)
          {
            case 0: args.Value = InfoTime.ToString("d", ci.DateTimeFormat); break;
            case 2: args.Value = InfoTime.ToString("D", ci.DateTimeFormat); break;
            case 3: args.Value = InfoTime.ToString("t", ci.DateTimeFormat); break;
            case 5: args.Value = InfoTime.ToString("T", ci.DateTimeFormat); break;
            case 6: args.Value = InfoValue.ToString("0.00", ci.NumberFormat); break;
            case 7: args.Value = InfoValue.ToString("#,##0.00", ci.NumberFormat); break;
          }
        }
      }
    }


    #endregion

    #region Стандартные форматы

    EFPDataGridView efpFormats;

    private static readonly string[] FormatTitles = new string[] { "d", "D", "t", "T", "f", "F", "g", "G", "M", "Y", "y" };

    void efpFormats_CellInfoNeeded(object sender, EFPDataGridViewCellInfoEventArgs args)
    {
      if (args.ColumnIndex < 0 || args.RowIndex < 0)
        return;

      if (args.ColumnIndex == 0)
      {
        args.Value = FormatTitles[args.RowIndex];
      }
      else
      {
        if (efpCulture.SelectedIndex < 0)
          return;
        CultureInfo ci = AllCultures[efpCulture.SelectedIndex];
        args.Value = InfoTime.ToString(FormatTitles[args.RowIndex], ci.DateTimeFormat);
      }
    }

    #endregion

    #region EditableDateTimeFormatter

    EFPDataGridView efpEditableDateTimeFormatter;

    private EditableDateTimeFormatter[] _Formatters;

    private void efpEditableDateTimeFormatter_CellInfoNeeded(object sender, EFPDataGridViewCellInfoEventArgs args)
    {
      if (_Formatters == null)
      {
        CultureInfo ci = AllCultures[efpCulture.SelectedIndex];
        _Formatters = new EditableDateTimeFormatter[efpEditableDateTimeFormatter.Control.RowCount];
        for (int i = 0; i < _Formatters.Length; i++)
          _Formatters[i] = new EditableDateTimeFormatter(ci, (EditableDateTimeFormatterKind)i);
      }
      if (args.RowIndex < 0 || args.ColumnIndex < 0)
        return;

      EditableDateTimeFormatter f = _Formatters[args.RowIndex];
      if (f != null)
      {
        switch (args.ColumnIndex)
        {
          case 0: args.Value = f.Kind.ToString(); break;
          case 1: args.Value = f.Format; break;
          case 2: args.Value = f.EditMask; break;
          case 3: args.Value = f.TextWidth; break;
          case 4: args.Value = f.DefaultYearSupported; break;
        }
      }
    }

    #endregion

    #region Тестирование

    void efpTest_Click(object sender, EventArgs args)
    {
      Thread.CurrentThread.CurrentCulture = AllCultures[efpCulture.SelectedIndex];
      Thread.CurrentThread.CurrentUICulture = AllCultures[efpCulture.SelectedIndex];

      CultureTestForm.PerformTest();
    }

    #endregion
  }
}
