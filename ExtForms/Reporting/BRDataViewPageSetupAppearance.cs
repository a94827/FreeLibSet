using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.DependedValues;

namespace FreeLibSet.Forms.Reporting
{
  internal partial class BRDataViewPageSetupAppearance : Form
  {
    // Есть в Windows-7, Windows-10, но не в Windows-XP
    // Этих символов нет в Linux
    // При экспорте в PDF, в части программ просмотра будет поддерживаться, а в части - нет

    //public const char CheckBoxUncheckedChar = '\u2610';
    //public const char CheckBoxCheckedChar = '\u2611';
    public const string CheckBoxUncheckedStr = "\u2610";
    public const string CheckBoxCheckedStr = "\u2611";


    #region Конструктор

    public BRDataViewPageSetupAppearance(SettingsDialog dialog, IEFPDataView controlProvider, bool textOnly)
    {
      InitializeComponent();
      _ControlProvider = controlProvider;
      _ViewData = dialog.Data.GetRequired<BRDataViewSettingsDataItem>();

      SettingsDialogPage page = dialog.Pages.Add(MainPanel);

      page.Text = "Оформление";
      page.ToolTipText = "Выбор наличия границ, цветового оформления табличного просмотра";
      page.ImageKey = "CircleGreenYellowRed";

      efpBorderStyle = new EFPListComboBox(page.BaseProvider, cbBorderStyle);

      efpColorStyle = new EFPListComboBox(page.BaseProvider, cbColorStyle);
      efpColorStyle.Enabled = _ViewData.UseColorStyle;

      efpBoolMode = new EFPListComboBox(page.BaseProvider, cbBoolMode);
      efpBoolMode.Enabled = _ViewData.UseBoolMode;

      efpTextTrue = new EFPTextBox(page.BaseProvider, edTextTrue);
      efpTextFalse = new EFPTextBox(page.BaseProvider, edTextFalse);

      EFPButtonWithMenu efpSelText = new EFPButtonWithMenu(page.BaseProvider, btnSelText);

      if (_ViewData.UseBoolMode)
      {
        efpTextTrue.CanBeEmpty = true;
        efpTextFalse.CanBeEmpty = true;
        efpTextTrue.Validating += EfpTextTrueFalse_Validating;
        efpTextFalse.Validating += EfpTextTrueFalse_Validating;
        efpTextTrue.TextEx.ValueChanged += efpTextFalse.Validate;
        efpTextFalse.TextEx.ValueChanged += efpTextTrue.Validate;
        efpTextTrue.EnabledEx = new DepEqual<int>(efpBoolMode.SelectedIndexEx, (int)(BRDataViewBoolMode.Text));
        efpTextFalse.EnabledEx = new DepEqual<int>(efpBoolMode.SelectedIndexEx, (int)(BRDataViewBoolMode.Text));

        _TrueTextValues = new List<string>();
        _FalseTextValues = new List<string>();

        _TrueTextValues.Add("[X]");
        _FalseTextValues.Add("[ ]");
        _TrueTextValues.Add("+");
        _FalseTextValues.Add("-");

        bool useCheckBoxStr = false;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT && (!EnvironmentTools.IsWine) && Environment.OSVersion.Version.Major >= 6)
          useCheckBoxStr = true;

        if (useCheckBoxStr)
        {
          _TrueTextValues.Add(CheckBoxCheckedStr);
          _FalseTextValues.Add(CheckBoxUncheckedStr);
        }

        for (int i = 0; i < _TrueTextValues.Count; i++)
        {
          EFPCommandItem ci = new EFPCommandItem("Edit", "SetText" + StdConvert.ToString(i));
          ci.MenuText = _TrueTextValues[i] + " / " + _FalseTextValues[i];
          if (useCheckBoxStr && (i == (_TrueTextValues.Count - 1)))
            ci.MenuText += " (может не поддерживаться)";
          ci.Tag = i;
          ci.Click += SelBoolText_Click;
          efpSelText.CommandItems.Add(ci);
        }
      }
      else
      {
        efpTextTrue.Enabled = false;
        efpTextFalse.Enabled = false;
        efpSelText.Enabled = false;
      }

      btnCellParams.Image = EFPApp.MainImages.Images["Table"];
      btnCellParams.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpCellParams = new EFPButton(page.BaseProvider, btnCellParams);
      efpCellParams.Click += EfpCellParams_Click;

      _TextOnly = textOnly;
      if (textOnly)
      {
        efpBorderStyle.Visible = false;
        efpColorStyle.Visible = false;
        efpCellParams.Visible = false;
        lblCellParams.Visible = false;
        grpPageBreak.Visible = false;
      }

      page.DataToControls += Page_DataToControls;
      page.DataFromControls += Page_DataFromControls;
    }

    private IEFPDataView _ControlProvider;
    private BRDataViewSettingsDataItem _ViewData;
    private bool _TextOnly;

    #endregion

    #region Поля

    public EFPListComboBox efpBorderStyle;
    public EFPListComboBox efpColorStyle;

    #endregion

    #region BoolMode

    public EFPListComboBox efpBoolMode;
    public EFPTextBox efpTextTrue, efpTextFalse;

    private List<string> _TrueTextValues, _FalseTextValues;

    private void SelBoolText_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      int textIndex = (int)(ci.Tag);
      efpTextTrue.Text = _TrueTextValues[textIndex];
      efpTextFalse.Text = _FalseTextValues[textIndex];
    }

    private void EfpTextTrueFalse_Validating(object sender, UICore.UIValidatingEventArgs args)
    {
      if (args.ValidateState == UICore.UIValidateState.Error)
        return;
      if (efpTextTrue.Text == efpTextFalse.Text)
        args.SetError("Текст для включенного и выключенного флажка должен различаться");
    }

    #endregion

    #region Параметры ячейки

    private int _CellLeftMargin, _CellTopMargin, _CellRightMargin, _CellBottomMargin;

    private void UpdateCellParamsLabel()
    {
      StringBuilder sb = new StringBuilder();

      sb.Append("Поля ячейки: ");
      if (_CellLeftMargin == _CellTopMargin && _CellLeftMargin == _CellRightMargin && _CellLeftMargin == _CellBottomMargin)
      {
        sb.Append((_CellLeftMargin / 100m).ToString("0.0#"));
        sb.Append(" см");
      }
      else
      {
        sb.Append(" левое ");
        sb.Append((_CellLeftMargin / 100m).ToString("0.0#"));
        sb.Append(" верхнее ");
        sb.Append((_CellTopMargin / 100m).ToString("0.0#"));
        sb.Append(" правое ");
        sb.Append((_CellRightMargin / 100m).ToString("0.0#"));
        sb.Append(" нижнее ");
        sb.Append((_CellBottomMargin / 100m).ToString("0.0#"));
        sb.Append(" см");
      }

      lblCellParams.Text = sb.ToString();
    }

    private void EfpCellParams_Click(object sender, EventArgs args)
    {
      BRDataViewPageSetupCellParamDialog dlg = new BRDataViewPageSetupCellParamDialog();
      dlg.efpLeftMargin.Value = _CellLeftMargin / 100m;
      dlg.efpTopMargin.Value = _CellTopMargin / 100m;
      dlg.efpRightMargin.Value = _CellRightMargin / 100m;
      dlg.efpBottomMargin.Value = _CellBottomMargin / 100m;
      if (EFPApp.ShowDialog(dlg, true) != DialogResult.OK)
        return;

      _CellLeftMargin = (int)(dlg.efpLeftMargin.Value * 100m);
      _CellTopMargin = (int)(dlg.efpTopMargin.Value * 100m);
      _CellRightMargin = (int)(dlg.efpRightMargin.Value * 100m);
      _CellBottomMargin = (int)(dlg.efpBottomMargin.Value * 100m);

      UpdateCellParamsLabel();
    }

    #endregion

    #region Чтение и запись значений

    private void Page_DataToControls(object sender, EventArgs args)
    {
      try
      {
        if (!_TextOnly)
        {
          efpBorderStyle.SelectedIndex = (int)(_ViewData.BorderStyle);
          if (_ViewData.UseColorStyle)
            efpColorStyle.SelectedIndex = (int)(_ViewData.ColorStyle);
        }
        if (_ViewData.UseBoolMode)
        {
          efpBoolMode.SelectedIndex = (int)(_ViewData.BoolMode);
          efpTextTrue.Text = _ViewData.BoolTextTrue;
          efpTextFalse.Text = _ViewData.BoolTextFalse;
        }
      }
      catch { }

      if (!_TextOnly)
      {
        _CellLeftMargin = _ViewData.CellLeftMargin;
        _CellTopMargin = _ViewData.CellTopMargin;
        _CellRightMargin = _ViewData.CellRightMargin;
        _CellBottomMargin = _ViewData.CellBottomMargin;

        UpdateCellParamsLabel();
      }
    }

    private void Page_DataFromControls(object sender, EventArgs args)
    {
      if (!_TextOnly)
      {
        _ViewData.BorderStyle = (BRDataViewBorderStyle)(efpBorderStyle.SelectedIndex);
        if (_ViewData.UseColorStyle)
          _ViewData.ColorStyle = (BRDataViewColorStyle)(efpColorStyle.SelectedIndex);
      }
      if (_ViewData.UseBoolMode)
      {
        _ViewData.BoolMode = (BRDataViewBoolMode)(efpBoolMode.SelectedIndex);
        _ViewData.BoolTextTrue = efpTextTrue.Text;
        _ViewData.BoolTextFalse = efpTextFalse.Text;
      }

      if (!_TextOnly)
      {
        _ViewData.CellLeftMargin = _CellLeftMargin;
        _ViewData.CellTopMargin = _CellTopMargin;
        _ViewData.CellRightMargin = _CellRightMargin;
        _ViewData.CellBottomMargin = _CellBottomMargin;
      }
    }

    #endregion
  }
}
