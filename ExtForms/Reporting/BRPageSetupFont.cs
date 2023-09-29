using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Drawing;
using FreeLibSet.Reporting;

#pragma warning disable 1591


namespace FreeLibSet.Forms.Reporting
{
  internal partial class BRPageSetupFont : Form
  {
    #region Конструктор

    public BRPageSetupFont(SettingsDialog dialog)
    {
      InitializeComponent();

      SettingsDialogPage page = dialog.Pages.Add(panFont);

      new FontComboBoxFiller(cbFontName);

      efpFontName = new EFPTextComboBox(page.BaseProvider, cbFontName);
      efpFontName.ToolTipText = "Имя гарнитуры шрифта";
      //efpFontName.Enabled = FForm.PageSetup.AllowFontName;
      efpFontName.TextEx.ValueChanged += new EventHandler(FontChanged);

      //grpFontHeight.Enabled = FForm.PageSetup.AllowFontSize;
      //grpFontWidth.Enabled = FForm.PageSetup.AllowFontSize;

      efpFontHeight = new EFPSingleEditBox(page.BaseProvider, edFontHeight);
      efpFontHeight.ToolTipText = "Высота шрифта в пунктах." + Environment.NewLine +
        "1 пт = 1/72 дюйма (примерно 0,35 мм)";
      efpFontHeight.Control.Minimum = BRReport.MinFontHeightTwip / 20f;
      efpFontHeight.Control.Maximum = BRReport.MaxFontHeightTwip / 20f;
      efpFontHeight.Control.Increment = 1f;
      efpFontHeight.ValueEx.ValueChanged += new EventHandler(FontChanged);

      efpFontWidthMode = new EFPRadioButtons(page.BaseProvider, rbWidthAuto);
      efpFontWidthMode.SelectedIndexEx.ValueChanged += new EventHandler(FontChanged);

      efpFontWidth = new EFPSingleEditBox(page.BaseProvider, edFontWidth);
      efpFontWidth.ToolTipText = "Ширина шрифта в пунктах." + Environment.NewLine +
        "1 пт = 1/72 дюйма (примерно 0,35 мм)";
      efpFontWidth.Control.Minimum = BRReport.MinFontWidthTwip / 20f;
      efpFontWidth.Control.Maximum = BRReport.MaxFontWidthTwip / 20f;
      efpFontWidth.Control.Increment = 1f;
      efpFontWidth.ValueEx.ValueChanged += new EventHandler(FontChanged);
      efpFontWidth.EnabledEx = efpFontWidthMode[1].CheckedEx;
      efpFontWidth.AllowDisabledValue = true;

      efpLineHeightMode = new EFPRadioButtons(page.BaseProvider, rbLineAuto);
      efpLineHeightMode.SelectedIndexEx.ValueChanged += new EventHandler(FontChanged);

      efpLineHeight = new EFPSingleEditBox(page.BaseProvider, edLineHeight);
      efpLineHeight.ToolTipText = "Высота строки в пунктах." + Environment.NewLine +
        "Складывается из высоты шрифта и межстрочного интервала";
      efpLineHeight.Control.Increment = 1f;
      efpLineHeight.Control.Minimum = BRReport.MinFontHeightTwip / 20f;
      efpLineHeight.Control.Maximum = BRReport.MaxFontHeightTwip * 2f / 20f;
      efpLineHeight.ValueEx.ValueChanged += new EventHandler(FontChanged);
      efpLineHeight.EnabledEx = efpLineHeightMode[1].CheckedEx;
      efpLineHeight.AllowDisabledValue = true;

      pbFontSample.Paint += new PaintEventHandler(pbFontSample_Paint);

      page.Text = "Шрифт";
      page.ToolTipText = "Выбор гарнитуры и размеров грифта";
      page.ImageKey = "Font";
      page.DataToControls += DataToControls;
      page.DataFromControls += DataFromControls;
    }

    #endregion

    #region Параметры шрифта

    EFPTextComboBox efpFontName;
    EFPSingleEditBox efpFontHeight, efpLineHeight, efpFontWidth;
    EFPRadioButtons efpFontWidthMode, efpLineHeightMode;


    private new void FontChanged(object sender, EventArgs args)
    {
      try
      {
        using (ExtTextRenderer rdr = new ExtTextRenderer())
        {
          InitRenderer(rdr);
          efpFontWidth.DisabledValue = rdr.FontWidth;
          efpLineHeight.DisabledValue = rdr.LineHeight;
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowTempMessage(e.Message);
      }

      pbFontSample.Invalidate();
    }

    #endregion

    #region Образец

    private void pbFontSample_Paint(object sender, PaintEventArgs args)
    {
      try
      {
        using (ExtTextRenderer rdr = new ExtTextRenderer())
          try
          {
            rdr.Graphics = args.Graphics;
            InitRenderer(rdr);

            float dy = rdr.Font.GetHeight(args.Graphics);

            float h1 = (float)(efpFontHeight.Value);
            float w1 = (float)(efpFontWidth.Value);
            float h2 = h1;
            float w2 = w1;
            //if (FForm.PageSetup.AllowFontSize)
            //{
            //  if (FForm.PageSetup.AllowAutoFontHeight && FForm.PageSetup.AutoFontHeight)
            //    h2 = FForm.PageSetup.MinFontHeight;
            //  if (FForm.PageSetup.AllowAutoFontWidth && FForm.PageSetup.AutoFontWidth)
            //    w2 = FForm.PageSetup.MinFontWidth;
            //}

            string s = rdr.Font.Name + ". Высота " + rdr.FontHeight.ToString() + ". Ширина " + rdr.FontWidth.ToString() + " пт.";
            string[] a = new string[5];
            DataTools.FillArray<string>(a, s);
            //RectangleF rc = pbFontSample.ClientRectangle;
            //rc.X += SystemInformation.Border3DSize.Width;
            //rc.Y += SystemInformation.Border3DSize.Height;
            //rc.Width -= 2*SystemInformation.Border3DSize.Width;
            //rc.Height -= 2*SystemInformation.Border3DSize.Height;

            RectangleF rc = new RectangleF(SystemInformation.Border3DSize.Width, SystemInformation.Border3DSize.Height, 1e6f, 1e6f);

            rdr.DrawLines(a, rc);
          }
          finally
          {
            rdr.Dispose();
          }
      }
      catch (Exception e)
      {
        args.Graphics.DrawString("Ошибка при построении изображения", SystemFonts.DefaultFont, Brushes.Red, 10, 10);
        args.Graphics.DrawString(e.Message, SystemFonts.DefaultFont, Brushes.Red, 10, 30);
      }
    }

    private void InitRenderer(ExtTextRenderer rdr)
    {
      if (!String.IsNullOrEmpty(efpFontName.Text))
        rdr.FontName = efpFontName.Text;
      if (efpFontHeight.Value >= BRReport.MinFontHeightTwip / 20f && efpFontHeight.Value <= BRReport.MaxFontHeightTwip / 20f)
        rdr.FontHeight = efpFontHeight.Value;
      if (efpFontWidthMode.SelectedIndex == 1 && efpFontWidth.Value >= BRReport.MinFontWidthTwip / 20f && efpFontWidth.Value <= BRReport.MaxFontWidthTwip / 20f)
        rdr.FontWidth = efpFontWidth.Value;
      else
        rdr.FontWidth = 0;
      if (efpLineHeightMode.SelectedIndex == 1 && efpLineHeight.Value >= BRReport.MinRowHeight)
        rdr.LineHeight = efpLineHeight.Value;
      else
        rdr.LineHeight = 0;
    }

    #endregion

    #region Обработчики DialogSettingsPage

    private void DataToControls(object sender, EventArgs args)
    {
      SettingsDialogPage page = (SettingsDialogPage)sender;
      BRFontSettings fs = page.Owner.Data.GetItem<BRFontSettings>();
      efpFontName.Text = fs.FontName;
      efpFontHeight.Value = fs.FontHeightTwip / 20f;
      if (fs.FontWidthTwip == 0)
        efpFontWidthMode.SelectedIndex = 0;
      else
      {
        efpFontWidthMode.SelectedIndex = 1;
        efpFontWidth.Value = fs.FontWidthTwip / 20f;
      }
      if (fs.LineHeightTwip == 0)
        efpLineHeightMode.SelectedIndex = 0;
      else
      {
        efpLineHeightMode.SelectedIndex = 1;
        efpLineHeight.Value = fs.LineHeightTwip / 20f;
      }
    }

    private void DataFromControls(object sender, EventArgs args)
    {
      SettingsDialogPage page = (SettingsDialogPage)sender;
      BRFontSettings fs = page.Owner.Data.GetItem<BRFontSettings>();
      fs.FontName = efpFontName.Text;
      fs.FontHeightTwip = (int)(efpFontHeight.Value * 20f);
      if (efpFontWidthMode.SelectedIndex == 0)
        fs.FontWidthTwip = 0;
      else
        fs.FontWidthTwip = (int)(efpFontWidth.Value * 20f);
      if (efpLineHeightMode.SelectedIndex == 0)
        fs.LineHeightTwip = 0;
      else
        fs.LineHeightTwip = (int)(efpLineHeight.Value * 20f);
    }

    #endregion
  }
}

namespace FreeLibSet.Reporting
{
  public class BRFontSettings : ISettingsDataItem
  {
    #region Конструктор

    public BRFontSettings()
    {
      _FontName = BRReport.DefaultFontName;
      _FontHeightTwip = BRReport.DefaultFontHeightTwip;
    }

    #endregion

    #region Свойства

    public string FontName { get { return _FontName; } set { _FontName = value; } }
    private string _FontName;

    public int FontHeightTwip { get { return _FontHeightTwip; } set { _FontHeightTwip = value; } }
    private int _FontHeightTwip;

    public int LineHeightTwip { get { return _LineHeightTwip; } set { _LineHeightTwip = value; } }
    private int _LineHeightTwip;

    public int FontWidthTwip { get { return _FontWidthTwip; } set { _FontWidthTwip = value; } }
    private int _FontWidthTwip;

    #endregion

    #region ISettingsDataItem

    public SettingsPart UsedParts { get { return SettingsPart.User; } }

    public void WriteConfig(CfgPart cfg, SettingsPart part)
    {
      cfg.SetString("FontName", FontName);
      cfg.SetSingle("FontHeight", FontHeightTwip / 20f);
      cfg.SetSingle("FontWidth", FontWidthTwip / 20f, true);
      cfg.SetSingle("LineHeight", LineHeightTwip / 20f);
    }

    public void ReadConfig(CfgPart cfg, SettingsPart part)
    {
      FontName = cfg.GetStringDef("FontName", FontName);
      FontHeightTwip = (int)(cfg.GetSingleDef("FontHeight", FontHeightTwip / 20f) * 20f);
      FontWidthTwip = (int)(cfg.GetSingleDef("FontWidth", FontWidthTwip / 20f) * 20f);
      LineHeightTwip = (int)(cfg.GetSingleDef("LineHeight", LineHeightTwip / 20f) * 20f);
    }

    #endregion

    public void InitCellStyle(BRCellStyle cellStyle)
    {
      cellStyle.FontName = FontName;
      cellStyle.FontHeightTwip = FontHeightTwip;
      cellStyle.FontWidthTwip = FontWidthTwip;
      cellStyle.LineHeightTwip = LineHeightTwip;
    }
  }
}

