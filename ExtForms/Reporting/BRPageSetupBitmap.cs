using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Reporting;
using FreeLibSet.UICore;

#pragma warning disable 1591

namespace FreeLibSet.Forms.Reporting
{
  internal partial class BRPageSetupBitmap : Form
  {
    #region Конструктор

    public BRPageSetupBitmap(SettingsDialog dialog)
    {
      InitializeComponent();

      _BitmapSettings = dialog.Data.GetRequired<BRBitmapSettingsDataItem>();
      SettingsDialogPage page = dialog.Pages.Add(MainPanel);

      efpResolution = new EFPIntEditBox(page.BaseProvider, edResolution);
      efpResolution.Minimum = BRBitmapSettingsDataItem.MinResolution;
      efpResolution.Maximum = BRBitmapSettingsDataItem.MaxResolution;
      efpResolution.Control.UpDownHandler = new NumArrayUpDownHandler<int>(BRBitmapSettingsDataItem.RecommendedResolutions);

      cbColorFormat.Items.AddRange(BRBitmapSettingsDataItem.ColorFormatNames);
      efpColorFormat = new EFPListComboBox(page.BaseProvider, cbColorFormat);
      efpColorFormat.Codes = BRBitmapSettingsDataItem.ColorFormatCodes;
      efpColorFormat.CanBeEmpty = false;

      efpClipMargins = new EFPCheckBox(page.BaseProvider, cbClipMargins);

      page.Text = "Графика";
      page.ToolTipText = "Выбор разрешения и количества цветов";
      page.ImageKey = "Font";
      page.DataToControls += DataToControls;
      page.DataFromControls += DataFromControls;
    }

    #endregion

    #region Поля

    BRBitmapSettingsDataItem _BitmapSettings;

    EFPIntEditBox efpResolution;

    EFPListComboBox efpColorFormat;

    EFPCheckBox efpClipMargins;

    #endregion

    #region SettingsDialogPage

    private void DataToControls(object sender, EventArgs args)
    {
      efpResolution.Value = _BitmapSettings.Resolution;
      efpColorFormat.SelectedCode = _BitmapSettings.ColorFormat;
      efpClipMargins.Checked = _BitmapSettings.ClipMargins;
    }

    private void DataFromControls(object sender, EventArgs args)
    {
      _BitmapSettings.Resolution = efpResolution.Value;
      _BitmapSettings.ColorFormat = efpColorFormat.SelectedCode;
      _BitmapSettings.ClipMargins = efpClipMargins.Checked;
    }

    #endregion
  }
}

namespace FreeLibSet.Reporting
{
  public class BRBitmapSettingsDataItem : SettingsDataItem, ICloneable
  {
    #region Константы

    public const int DefaultResolution = 300;
    public const int MinResolution = 72;
    public const int MaxResolution = 1200;
    public static readonly int[] RecommendedResolutions = new int[] { 72, 100, 144, 150, 200, 300, 600, 1200 };

    public const string DefaultColorFormat = "C24";
    public static string[] ColorFormatCodes = new string[] { "BW", "C4", "C8", /*"C16",*/ "C24" };
    public static string[] ColorFormatNames = new string[] {
        "Черно-белый",
        "4-битный цвет",
        "8-битный цвет",
        //"16-битный цвет",
        "24-битный цвет"};

    #endregion

    #region Конструктор

    public BRBitmapSettingsDataItem()
    {
      _Resolution = DefaultResolution;
      _ColorFormat = DefaultColorFormat;
      _ClipMargins = false;
    }

    #endregion

    #region Свойства

    public int Resolution
    {
      get { return _Resolution; }
      set
      {
        if (value < MinResolution || value > MaxResolution)
          throw new ArgumentOutOfRangeException();
        _Resolution = value;
      }
    }
    private int _Resolution;

    public string ColorFormat
    {
      get { return _ColorFormat; }
      set
      {
        if (Array.IndexOf<string>(ColorFormatCodes, value) < 0)
          throw new ArgumentException();
        _ColorFormat = value;
      }
    }
    private string _ColorFormat;

    public PixelFormat PixelFormat
    {
      get
      {
        switch (_ColorFormat)
        {
          case "BW": return PixelFormat.Format1bppIndexed;
          case "C4": return PixelFormat.Format4bppIndexed;
          case "C8": return PixelFormat.Format8bppIndexed;
          //case "C16": return PixelFormat.Format16bppRgb565; 
          case "C24": return PixelFormat.Format24bppRgb;
          default:
            throw new BugException("Неизвестный пиксельный формат \"" + _ColorFormat + "\"");
        }
      }
    }

    public bool ClipMargins { get { return _ClipMargins; } set { _ClipMargins = value; } }

    private bool _ClipMargins;

    #endregion

    #region ISettingsDataItem

    public override SettingsPart UsedParts { get { return SettingsPart.User; } }

    public override void WriteConfig(CfgPart cfg, SettingsPart part)
    {
      if (part == SettingsPart.User)
      {
        cfg.SetInt("Resolution", Resolution);
        cfg.SetString("ColorFormat", ColorFormat);
        cfg.SetBool("ClipMargins", ClipMargins);
      }
    }

    public override void ReadConfig(CfgPart cfg, SettingsPart part)
    {
      if (part == SettingsPart.User)
      {
        Resolution = cfg.GetIntDef("Resolution", DefaultResolution);
        ColorFormat = cfg.GetStringDef("ColorFormat", DefaultColorFormat);
        ClipMargins = cfg.GetBoolDef("ClipMargins", false);
      }
    }

    #endregion

    #region Clone()

    /// <summary>
    /// Создает копию объекта
    /// </summary>
    /// <returns>Новый объект</returns>
    public BRBitmapSettingsDataItem Clone()
    {
      BRBitmapSettingsDataItem res = new BRBitmapSettingsDataItem();
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
