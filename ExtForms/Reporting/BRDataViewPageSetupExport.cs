using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreeLibSet.Forms.Reporting
{
  internal partial class BRDataViewPageSetupExport : Form
  {
    #region Конструктор

    public BRDataViewPageSetupExport(SettingsDialog dialog, IEFPDataView controlProvider)
    {
      InitializeComponent();
      _ControlProvider = controlProvider;
      _ViewData = dialog.Data.GetRequired<BRDataViewData>();

      SettingsDialogPage page = dialog.Pages.Add(MainPanel);

      efpExpArea = new EFPRadioButtons(page.BaseProvider, rbAll);
      efpExpHeaders = new EFPCheckBox(page.BaseProvider, cbExpHeaders);
      efpExpHeaders.Enabled = _ViewData.UseExpColumnHeaders;

      page.Text = "Область";
      page.ToolTipText = "Выбор области просмотра для экспорта";
      page.ImageKey = "Table";

      page.DataToControls += Page_DataToControls;
      page.DataFromControls += Page_DataFromControls;
    }

    private IEFPDataView _ControlProvider;
    private BRDataViewData _ViewData;

    #endregion

    #region Поля

    public EFPRadioButtons efpExpArea;
    public EFPCheckBox efpExpHeaders;

    #endregion

    #region Чтение и запись значений

    private void Page_DataToControls(object sender, EventArgs args)
    {
      efpExpArea.SelectedIndex = _ViewData.ExpRange == EFPDataViewExpRange.Selected ? 1 : 0;
      if (_ViewData.UseExpColumnHeaders)
        efpExpHeaders.Checked = _ViewData.ExpColumnHeaders;
    }

    private void Page_DataFromControls(object sender, EventArgs args)
    {
      _ViewData.ExpRange = efpExpArea.SelectedIndex == 1 ? EFPDataViewExpRange.Selected : EFPDataViewExpRange.All;
      if (_ViewData.UseExpColumnHeaders)
        _ViewData.ExpColumnHeaders = efpExpHeaders.Checked;
    }

    #endregion
  }
}
