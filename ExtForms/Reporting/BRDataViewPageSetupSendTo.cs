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

    public BRDataViewPageSetupSendTo(SettingsDialog dialog, IEFPDataView controlProvider, BRDialogKind dialogKind)
    {
      InitializeComponent();
      _ControlProvider = controlProvider;
      _ViewData = dialog.Data.GetRequired<BRDataViewSettingsDataItem>();
      _DialogKind = dialogKind;

      SettingsDialogPage page = dialog.Pages.Add(MainPanel);

      efpExpArea = new EFPRadioButtons(page.BaseProvider, rbAll);
      if (dialogKind == BRDialogKind.ControlSendTo)
      {
        if (controlProvider is EFPDataTreeView)
        {
          TreeViewAdv control = controlProvider.Control as TreeViewAdv;
          rbAll.Text = "Весь просмотр";
          if (controlProvider.MultiSelect)
            rbSelected.Text = "Выбранные строки";
          else
            rbSelected.Text = "Выбранная строка";

          if (control.Root.Nodes.Count == 0)
            efpExpArea[1].Enabled = false; // 17.06.2024

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
          if (control.RowCount == 0)
            efpExpArea[1].Enabled = false; // 17.06.2024

          lblInfo.Text = "В режиме \"" + rbAll.Text + "\" печатаются столбцы, отмеченные в диалоге \"Параметры страницы\"." + Environment.NewLine +
            "В режиме \"" + rbSelected.Text + "\" выбранные столбцы не учитываются.";
        }
      }
      else
      {
        efpExpArea.SelectedIndex = 0;
        efpExpArea.Enabled = false;
        lblInfo.Text = "В файл записываются все строки таблицы и столбцы, отмеченные в диалоге \"Параметры страницы\"";
      }

      efpExpTableHeader = new EFPCheckBox(page.BaseProvider, cbExpTableHeader);
      efpExpTableHeader.Enabled = _ViewData.UseExpTableHeader;

      efpExpTableFilters = new EFPCheckBox(page.BaseProvider, cbExpTableFilters);
      efpExpTableFilters.Enabled = _ViewData.UseExpTableFilters;

      efpExpColumnHeaders = new EFPCheckBox(page.BaseProvider, cbExpColumnHeaders);
      efpExpColumnHeaders.Enabled = _ViewData.UseExpColumnHeaders;

      page.Text = "Область";
      page.ToolTipText = "Выбор области просмотра для экспорта";
      page.ImageKey = "Table";

      page.DataToControls += Page_DataToControls;
      page.DataFromControls += Page_DataFromControls;
    }

    private readonly IEFPDataView _ControlProvider;
    private readonly BRDataViewSettingsDataItem _ViewData;
    private readonly BRDialogKind _DialogKind;

    #endregion

    #region Поля

    public readonly EFPRadioButtons efpExpArea;
    public readonly EFPCheckBox efpExpTableHeader, efpExpTableFilters,  efpExpColumnHeaders;

    #endregion

    #region Чтение и запись значений

    private void Page_DataToControls(object sender, EventArgs args)
    {
      if (_DialogKind == BRDialogKind.ControlSendTo)
      {
        if (efpExpArea[1].Enabled)
          efpExpArea.SelectedIndex = _ViewData.ExpRange == EFPDataViewExpRange.Selected ? 1 : 0;
        else
          efpExpArea.SelectedIndex = 0; // вся таблица
      }
      if (_ViewData.UseExpTableHeader)
        efpExpTableHeader.Checked = _ViewData.ExpTableHeader;
      if (_ViewData.UseExpTableFilters)
        efpExpTableFilters.Checked = _ViewData.ExpTableFilters;
      if (_ViewData.UseExpColumnHeaders)
        efpExpColumnHeaders.Checked = _ViewData.ExpColumnHeaders;
    }

    private void Page_DataFromControls(object sender, EventArgs args)
    {
      if (_DialogKind == BRDialogKind.ControlSendTo)
      {
        _ViewData.ExpRange = efpExpArea.SelectedIndex == 1 ? EFPDataViewExpRange.Selected : EFPDataViewExpRange.All;
      }
      if (_ViewData.UseExpTableHeader)
        _ViewData.ExpTableHeader = efpExpTableHeader.Checked;
      if (_ViewData.UseExpTableFilters)
        _ViewData.ExpTableFilters = efpExpTableFilters.Checked;
      if (_ViewData.UseExpColumnHeaders)
        _ViewData.ExpColumnHeaders = efpExpColumnHeaders.Checked;
    }

    #endregion
  }
}
