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
      efpFontName.ToolTipText = Res.BRPageSetupFont_ToolTip_FontName;
      //efpFontName.Enabled = FForm.PageSetup.AllowFontName;
      efpFontName.TextEx.ValueChanged += new EventHandler(FontChanged);

      //grpFontHeight.Enabled = FForm.PageSetup.AllowFontSize;
      //grpFontWidth.Enabled = FForm.PageSetup.AllowFontSize;

      efpFontHeight = new EFPSingleEditBox(page.BaseProvider, edFontHeight);
      efpFontHeight.ToolTipText = Res.BRPageSetupFont_ToolTip_FontHeight;
      efpFontHeight.Control.Minimum = BRReport.MinFontHeightTwip / 20f;
      efpFontHeight.Control.Maximum = BRReport.MaxFontHeightTwip / 20f;
      efpFontHeight.Control.Increment = 1f;
      efpFontHeight.ValueEx.ValueChanged += new EventHandler(FontChanged);

      efpFontWidthMode = new EFPRadioButtons(page.BaseProvider, rbWidthAuto);
      efpFontWidthMode.SelectedIndexEx.ValueChanged += new EventHandler(FontChanged);

      efpFontWidth = new EFPSingleEditBox(page.BaseProvider, edFontWidth);
      efpFontWidth.ToolTipText = Res.BRPageSetupFont_ToolTip_FontWidth;
      efpFontWidth.Control.Minimum = BRReport.MinFontWidthTwip / 20f;
      efpFontWidth.Control.Maximum = BRReport.MaxFontWidthTwip / 20f;
      efpFontWidth.Control.Increment = 1f;
      efpFontWidth.ValueEx.ValueChanged += new EventHandler(FontChanged);
      efpFontWidth.EnabledEx = efpFontWidthMode[1].CheckedEx;
      efpFontWidth.AllowDisabledValue = true;

      efpLineHeightMode = new EFPRadioButtons(page.BaseProvider, rbLineAuto);
      efpLineHeightMode.SelectedIndexEx.ValueChanged += new EventHandler(FontChanged);

      efpLineHeight = new EFPSingleEditBox(page.BaseProvider, edLineHeight);
      efpLineHeight.ToolTipText = Res.BRPageSetupFont_ToolTip_LineHeight;
      efpLineHeight.Control.Increment = 1f;
      efpLineHeight.Control.Minimum = BRReport.MinFontHeightTwip / 20f;
      efpLineHeight.Control.Maximum = BRReport.MaxFontHeightTwip * 2f / 20f;
      efpLineHeight.ValueEx.ValueChanged += new EventHandler(FontChanged);
      efpLineHeight.EnabledEx = efpLineHeightMode[1].CheckedEx;
      efpLineHeight.AllowDisabledValue = true;

      pbFontSample.Paint += new PaintEventHandler(pbFontSample_Paint);

      page.Text = Res.BRPageSetupFont_Title_Tab;
      page.ToolTipText = Res.BRPageSetupFont_ToolTip_Tab;
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
        using (ExtTextRenderer rdr = new ExtTextRenderer(args.Graphics, false))
          try
          {
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

            string s = String.Format(Res.BRPageSetupFont_Msg_Sample,
              rdr.Font.Name, rdr.FontHeight, rdr.FontWidth);
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
        args.Graphics.DrawString(Res.BRPageSetupFont_Err_Sample, SystemFonts.DefaultFont, Brushes.Red, 10, 10);
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
      BRFontSettingsDataItem fs = page.Owner.Data.GetItem<BRFontSettingsDataItem>();
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
      BRFontSettingsDataItem fs = page.Owner.Data.GetItem<BRFontSettingsDataItem>();
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
  /// <summary>
  /// Параметры шрифта, используемого в отчете.
  /// </summary>
  public class BRFontSettingsDataItem : SettingsDataItem, ICloneable
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует параметры значениями по умолчанию
    /// </summary>
    public BRFontSettingsDataItem()
    {
      _FontName = BRReport.DefaultFontName;
      _FontHeightTwip = BRReport.DefaultFontHeightTwip;
      _FontWidthTwip = 0;
      _LineHeightTwip = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя шрифта. По умолчанию используется значение из <see cref="BRReport.DefaultFontName"/>.
    /// </summary>
    public string FontName { get { return _FontName; } set { _FontName = value; } }
    private string _FontName;

    /// <summary>
    /// Высота шрифта в единицах 1/20 дюйма. По умолчанию используется значение из <see cref="BRReport.DefaultFontHeightTwip"/>.
    /// </summary>
    public int FontHeightTwip { get { return _FontHeightTwip; } set { _FontHeightTwip = value; } }
    private int _FontHeightTwip;

    /// <summary>
    /// Высота строки в единицах 1/20 дюйма с учетом межстрочного интервала.
    /// По умолчанию - 0 - использовать межстрочный интервал, определенный в шрифте.
    /// </summary>
    public int LineHeightTwip { get { return _LineHeightTwip; } set { _LineHeightTwip = value; } }
    private int _LineHeightTwip;

    /// <summary>
    /// Ширина символов в единицах 1/20 дюйма.
    /// По умолчанию - 0 - использовать ширину символов, определяемую гарнитурой шрифта.
    /// Для шрифтов с переменной шириной символов задает среднюю ширину символов, а не делает шрифт моноширинным.
    /// </summary>
    public int FontWidthTwip { get { return _FontWidthTwip; } set { _FontWidthTwip = value; } }
    private int _FontWidthTwip;

    #endregion

    #region ISettingsDataItem

    /// <summary>
    /// Запись значений
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="part"></param>
    public override void WriteConfig(CfgPart cfg, SettingsPart part)
    {
      cfg.SetString("FontName", FontName);
      cfg.SetSingle("FontHeight", FontHeightTwip / 20f);
      cfg.SetSingle("FontWidth", FontWidthTwip / 20f, true);
      cfg.SetSingle("LineHeight", LineHeightTwip / 20f);
    }

    /// <summary>
    /// Чтение значений
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="part"></param>
    public override void ReadConfig(CfgPart cfg, SettingsPart part)
    {
      FontName = cfg.GetStringDef("FontName", BRReport.DefaultFontName);
      FontHeightTwip = (int)(cfg.GetSingleDef("FontHeight", BRReport.DefaultFontHeightTwip / 20f) * 20f);
      FontWidthTwip = (int)(cfg.GetSingleDef("FontWidth", 0f) * 20f);
      LineHeightTwip = (int)(cfg.GetSingleDef("LineHeight", 0f) * 20f);
    }

    #endregion

    #region Инициализация BRCellStyle

    /// <summary>
    /// Инициализирует некоторые свойства объекта <see cref="BRCellStyle"/> значениями из текущего объекта.
    /// </summary>
    /// <param name="cellStyle">Формат ячейки отчета</param>
    public void InitCellStyle(BRCellStyle cellStyle)
    {
      cellStyle.FontName = FontName;
      cellStyle.FontHeightTwip = FontHeightTwip;
      cellStyle.FontWidthTwip = FontWidthTwip;
      cellStyle.LineHeightTwip = LineHeightTwip;
    }

    #endregion

    #region Clone()

    /// <summary>
    /// Создает копию объекта
    /// </summary>
    /// <returns>Новый объект</returns>
    public BRFontSettingsDataItem Clone()
    {
      BRFontSettingsDataItem res = new BRFontSettingsDataItem();
      TempCfg cfg = new TempCfg();
      this.WriteConfig(cfg, Config.SettingsPart.User);
      res.ReadConfig(cfg, Config.SettingsPart.User);
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }
}

