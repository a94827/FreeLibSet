using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Controls;

namespace FreeLibSet.Forms.Reporting
{
  internal partial class BRDataViewPageSetupSendTo : Form
  {
    #region Конструктор

    public BRDataViewPageSetupSendTo(SettingsDialog dialog, IEFPDataView controlProvider)
    {
      InitializeComponent();
      _ControlProvider = controlProvider;
      _ViewData = dialog.Data.GetRequired<BRDataViewSettingsDataItem>();

      SettingsDialogPage page = dialog.Pages.Add(MainPanel);

      if (controlProvider is EFPDataTreeView)
      {
        TreeViewAdv control = controlProvider.Control as TreeViewAdv;
        rbAll.Text = "Весь просмотр";
        if (controlProvider.MultiSelect)
          rbSelected.Text = "Выбранные строки";
        else
          rbSelected.Text = "Выбранная строка";

        if (control.UseColumns)
          lblInfo.Text = "Печатаются столбцы, отмеченные в диалоге \"Параметры страницы\"";
        else
          lblInfo.Visible = false;
      }
      else
      {
        DataGridView control = controlProvider.Control as DataGridView;
        if (control.SelectedRows.Count > 0)
        {
          if (control.SelectedRows.Count == 1)
            rbSelected.Text = "Выбранная строка";
          else
            rbSelected.Text = "Выбранные строки";
        }
        else
        {
          if (controlProvider.MultiSelect)
            rbSelected.Text = "Выбранные ячейки";
          else
            rbSelected.Text = "Выбранная ячейка";
        }
        lblInfo.Text = "В режиме \"" + rbAll.Text + "\" печатаются столбцы, отмеченные в диалоге \"Параметры страницы\"." + Environment.NewLine +
          "В режиме \"" + rbSelected.Text + "\" выбранные столбцы не учитываются.";
      }

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
    private BRDataViewSettingsDataItem _ViewData;

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
