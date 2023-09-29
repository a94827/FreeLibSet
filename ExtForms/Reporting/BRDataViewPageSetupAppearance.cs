using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreeLibSet.Forms.Reporting
{
  internal partial class BRDataViewPageSetupAppearance : Form
  {
    #region Конструктор

    public BRDataViewPageSetupAppearance(SettingsDialog dialog, IEFPDataView controlProvider)
    {
      InitializeComponent();
      _ControlProvider = controlProvider;
      _ViewData = dialog.Data.GetRequired<BRDataViewData>();

      SettingsDialogPage page = dialog.Pages.Add(MainPanel);

      page.Text = "Оформление";
      page.ToolTipText = "Выбор наличия границ, цветового оформления табличного просмотра";
      page.ImageKey = "Color";


      efpBorderStyle = new EFPListComboBox(page.BaseProvider, cbBorderStyle);

      efpColorStyle = new EFPListComboBox(page.BaseProvider, cbColorStyle);
      efpColorStyle.Enabled = _ViewData.UseColorStyle;

      efpBoolMode = new EFPListComboBox(page.BaseProvider, cbBoolMode);
      efpBoolMode.Enabled = _ViewData.UseBoolMode;

      btnCellParams.Image = EFPApp.MainImages.Images["Table"];
      btnCellParams.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpCellParams = new EFPButton(page.BaseProvider, btnCellParams);
      efpCellParams.Click += EfpCellParams_Click;

      page.DataToControls += Page_DataToControls;
      page.DataFromControls += Page_DataFromControls;
    }

    private IEFPDataView _ControlProvider;
    private BRDataViewData _ViewData;

    #endregion

    #region Поля

    public EFPListComboBox efpBorderStyle;
    public EFPListComboBox efpColorStyle;
    public EFPListComboBox efpBoolMode;

    #endregion

    #region Параметры ячейки

    private int _CellLeftMargin, _CellTopMargin, _CellRightMargin, _CellBottomMargin;

    private void UpdateCellParamsLabel()
    {
      StringBuilder sb = new StringBuilder();

      sb.Append("Поля ячейки: ");
      if (_CellLeftMargin == _CellTopMargin && _CellLeftMargin == _CellRightMargin && _CellLeftMargin == _CellBottomMargin)
      {
        sb.Append((_CellLeftMargin/100m).ToString("0.0#"));
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
        efpBorderStyle.SelectedIndex = (int)(_ViewData.BorderStyle);
        if (_ViewData.UseColorStyle)
          efpColorStyle.SelectedIndex = (int)(_ViewData.ColorStyle);
        if (_ViewData.UseBoolMode)
          efpBoolMode.SelectedIndex = (int)(_ViewData.BoolMode);
      }
      catch { }

      _CellLeftMargin= _ViewData.CellLeftMargin;
      _CellTopMargin = _ViewData.CellTopMargin;
      _CellRightMargin = _ViewData.CellRightMargin;
      _CellBottomMargin = _ViewData.CellBottomMargin;
      UpdateCellParamsLabel();
    }

    private void Page_DataFromControls(object sender, EventArgs args)
    {
      _ViewData.BorderStyle = (PrintGridBorderStyle)(efpBorderStyle.SelectedIndex);
      if (_ViewData.UseColorStyle)
        _ViewData.ColorStyle = (PrintGridColorStyle)(efpColorStyle.SelectedIndex);
      if (_ViewData.UseBoolMode)
        _ViewData.BoolMode = (EFPDataViewExpExcelBoolMode)(efpBoolMode.SelectedIndex);

      _ViewData.CellLeftMargin = _CellLeftMargin;
      _ViewData.CellTopMargin = _CellTopMargin;
      _ViewData.CellRightMargin = _CellRightMargin;
      _ViewData.CellBottomMargin = _CellBottomMargin;
    }

    #endregion
  }
}
