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

      efpResolution = new EFPInt32EditBox(page.BaseProvider, edResolution);
      efpResolution.Minimum = BRBitmapSettingsDataItem.MinResolution;
      efpResolution.Maximum = BRBitmapSettingsDataItem.MaxResolution;
      efpResolution.Control.UpDownHandler = new NumericArrayUpDownHandler<int>(BRBitmapSettingsDataItem.RecommendedResolutions);

      cbColorFormat.Items.AddRange(BRBitmapSettingsDataItem.ColorFormatNames);
      efpColorFormat = new EFPListComboBox(page.BaseProvider, cbColorFormat);
      efpColorFormat.Codes = BRBitmapSettingsDataItem.ColorFormatCodes;
      efpColorFormat.CanBeEmpty = false;

      efpClipMargins = new EFPCheckBox(page.BaseProvider, cbClipMargins);

      page.Text = Res.BRPageSetupBitmap_Title_Tab;
      page.ToolTipText = Res.BRPageSetupBitmap_ToolTip_Tab;
      page.ImageKey = "Font";
      page.DataToControls += DataToControls;
      page.DataFromControls += DataFromControls;
    }

    #endregion

    #region Поля

    BRBitmapSettingsDataItem _BitmapSettings;

    EFPInt32EditBox efpResolution;

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
  /// <summary>
  /// Хранилище параметров преобразования отчета в файл TIFF.
  /// Содержит разрешение и цветность.
  /// </summary>
  public class BRBitmapSettingsDataItem : SettingsDataItem, ICloneable
  {
    #region Константы

    /// <summary>
    /// Разрешение по умолчанию, точек на дюйм
    /// </summary>
    public const int DefaultResolution = 300;

    /// <summary>
    /// Минимально допусимое разрешение, точек на дюйм
    /// </summary>
    public const int MinResolution = 72;
    /// <summary>
    /// Максимально допусимое разрешение, точек на дюйм
    /// </summary>
    public const int MaxResolution = 1200;

    /// <summary>
    /// Массив рекомендуемых разрешений, точек на дюйм
    /// </summary>
    public static readonly int[] RecommendedResolutions = new int[] { 72, 100, 144, 150, 200, 300, 600, 1200 };

    /// <summary>
    /// Цветовой формат, используемый по умолчанию
    /// </summary>
    public const string DefaultColorFormat = "C24";

    /// <summary>
    /// Допустимые коды цветовых форматов
    /// </summary>
    public static string[] ColorFormatCodes = new string[] { "BW", "C4", "C8", /*"C16",*/ "C24" };

    /// <summary>
    /// ИменаЮ соответствующие <see cref="ColorFormatCodes"/>
    /// </summary>
    public static string[] ColorFormatNames = new string[] {
        Res.BRPageSetupBitmap_Msg_BW,
        String.Format(Res.BRPageSetupBitmap_Msg_BitDepth, "4"),
        String.Format(Res.BRPageSetupBitmap_Msg_BitDepth, "8"),
        //String.Format(Res.BRPageSetupBitmap_Msg_BitDepth, "16"),
        String.Format(Res.BRPageSetupBitmap_Msg_BitDepth, "24")
    };

    #endregion

    #region Конструктор

    /// <summary>
    /// Создает настройки по умолчанию
    /// </summary>
    public BRBitmapSettingsDataItem()
    {
      _Resolution = DefaultResolution;
      _ColorFormat = DefaultColorFormat;
      _ClipMargins = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Разрешение, точек на дюйм.
    /// По умолчанию - <see cref="DefaultResolution"/>.
    /// Значение должно быть в диапазоне от <see cref="MinResolution"/> до <see cref="MaxResolution"/>.
    /// </summary>
    public int Resolution
    {
      get { return _Resolution; }
      set
      {
        if (value < MinResolution || value > MaxResolution)
          throw ExceptionFactory.ArgOutOfRange("value", value, MinResolution, MaxResolution);
        _Resolution = value;
      }
    }
    private int _Resolution;

    /// <summary>
    /// Цветовой формат. По умолчанию - <see cref="DefaultColorFormat"/>.
    /// Допускаются значения из списка <see cref="ColorFormatCodes"/>.
    /// </summary>
    public string ColorFormat
    {
      get { return _ColorFormat; }
      set
      {
        if (Array.IndexOf<string>(ColorFormatCodes, value) < 0)
          throw ExceptionFactory.ArgUnknownValue("value", value, ColorFormatCodes);
        _ColorFormat = value;
      }
    }
    private string _ColorFormat;

    /// <summary>
    /// Значение <see cref="ColorFormat"/> как перечисление <see cref="System.Drawing.Imaging.PixelFormat"/>.
    /// </summary>
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
            throw new BugException("Unknown ColorFormat=\"" + _ColorFormat + "\"");
        }
      }
    }

    /// <summary>
    /// True, если поля страницы не будут сохраняться как часть изображения.
    /// По умолчанию - false - поля входят в изображение.
    /// </summary>
    public bool ClipMargins { get { return _ClipMargins; } set { _ClipMargins = value; } }
    private bool _ClipMargins;

    #endregion

    #region ISettingsDataItem

    /// <summary>
    /// Возвращает <see cref="SettingsPart.User"/>.
    /// </summary>
    public override SettingsPart UsedParts { get { return SettingsPart.User; } }

    /// <summary>
    /// Запись значений в секцию конфигурации.
    /// </summary>
    /// <param name="cfg">Секция</param>
    /// <param name="part">Записываемая часть</param>
    public override void WriteConfig(CfgPart cfg, SettingsPart part)
    {
      if (part == SettingsPart.User)
      {
        cfg.SetInt32("Resolution", Resolution);
        cfg.SetString("ColorFormat", ColorFormat);
        cfg.SetBoolean("ClipMargins", ClipMargins);
      }
    }

    /// <summary>
    /// Чтение значений из секции конфигурации.
    /// </summary>
    /// <param name="cfg">Секция</param>
    /// <param name="part">Считываемая часть</param>
    public override void ReadConfig(CfgPart cfg, SettingsPart part)
    {
      if (part == SettingsPart.User)
      {
        Resolution = cfg.GetInt32Def("Resolution", DefaultResolution);
        ColorFormat = cfg.GetStringDef("ColorFormat", DefaultColorFormat);
        ClipMargins = cfg.GetBooleanDef("ClipMargins", false);
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
