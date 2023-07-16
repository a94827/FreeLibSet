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

namespace TestCulture
{
  public partial class MainForm : Form
  {
    #region Конструктор формы

    public MainForm()
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
          names[i] += " (Текущая)";
          selIndex = i;
        }
      }
      cbCulture.Items.AddRange(names);
      efpCulture.SelectedIndex = selIndex;
      efpCulture.SelectedIndexEx.ValueChanged += new EventHandler(efpCulture_ValueChanged);

      #endregion

      InfoTime = DateTime.Now;
      InfoValue = 123456789.12m;

      #region Таблица параметров

      efpInfo = new EFPDataGridView(efpForm, grInfo);
      efpInfo.Columns.AddText("Параметр", false, "Параметр", 20);
      efpInfo.Columns.AddTextFill("Значение", 50);
      efpInfo.Columns.AddTextFill("Пример", 50);
      efpInfo.DisableOrdering();
      efpInfo.ReadOnly = true;
      efpInfo.Control.ReadOnly = true;
      efpInfo.CanView = false;
      grInfo.RowCount = InfoTitles.Length;
      grInfo.VirtualMode = true;
      efpInfo.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(efpInfo_GetCellAttributes);

      #endregion

      #region Стандартные форматы

      efpFormats = new EFPDataGridView(efpForm, grFormats);
      efpFormats.Columns.AddText("Формат", false, "Формат", 6);
      efpFormats.Columns.AddTextFill("Значение");
      efpFormats.DisableOrdering();
      efpFormats.ReadOnly = true;
      efpFormats.Control.ReadOnly = true;
      efpFormats.CanView = false;
      grFormats.RowCount = FormatTitles.Length;
      grFormats.VirtualMode = true;
      efpFormats.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(efpFormats_GetCellAttributes);

      #endregion

      #region EditableDateTimeFormatter

      efpEditableDateTimeFormatter = new EFPDataGridView(efpForm, grEditableDateTimeFormatter);
      efpEditableDateTimeFormatter.Columns.AddText("Kind");
      efpEditableDateTimeFormatter.Columns.AddText("Format");
      efpEditableDateTimeFormatter.Columns.AddText("EditMask");
      efpEditableDateTimeFormatter.Columns.AddInt("TextWidth");
      efpEditableDateTimeFormatter.Columns.AddBool("DefaultYearSupported");
      efpEditableDateTimeFormatter.DisableOrdering();
      efpEditableDateTimeFormatter.ReadOnly = true;
      efpEditableDateTimeFormatter.Control.ReadOnly = true;
      efpEditableDateTimeFormatter.CanView = false;
      efpEditableDateTimeFormatter.Control.RowCount = DataTools.GetEnumRange(typeof(EditableDateTimeFormatterKind)).MaxValue + 1;
      efpEditableDateTimeFormatter.Control.VirtualMode = true;
      efpEditableDateTimeFormatter.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(efpEditableDateTimeFormatter_GetCellAttributes);

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

    void efpInfo_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
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

    void efpFormats_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
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

    private void efpEditableDateTimeFormatter_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
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

      TestForm.PerformTest();
    }

    #endregion
  }
}
