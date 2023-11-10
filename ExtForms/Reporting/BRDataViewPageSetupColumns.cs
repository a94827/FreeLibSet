using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.Reporting;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Reporting
{
  internal partial class BRDataViewPageSetupColumns : Form
  {
    #region Конструктор

    public BRDataViewPageSetupColumns(SettingsDialog dialog, IEFPDataView controlProvider)
    {
      InitializeComponent();

      _ControlProvider = controlProvider;
      _PageSetup = dialog.Data.GetRequired<BRPageSettingsDataItem>().PageSetup;
      _ViewData = dialog.Data.GetRequired<BRDataViewSettingsDataItem>();
      _FontData = dialog.Data.GetRequired<BRFontSettingsDataItem>();

      SettingsDialogPage page = dialog.Pages.Add(MainPanel);

      ghColumns = new EFPDataGridView(page.BaseProvider, grColumns);
      ghColumns.Columns.AddBool("Flag", false, "П.");
      ghColumns.Columns.LastAdded.GridColumn.ToolTipText = "Печатать столбец";
      ghColumns.Columns.AddTextFill("Name", false, "Столбец", 100, 15);
      ghColumns.Columns.AddFixedPoint("Width", false, "Ширина, см", 10, 2, null);
      ghColumns.Columns.AddBool("AutoGrow", false, "Р.");

      grColumns.Columns[0].ToolTipText = "Установите флажки для тех столбцов," + Environment.NewLine +
        "которые должны быть напечатаны";
      grColumns.Columns[1].ToolTipText = "Название столбца";
      grColumns.Columns[2].ToolTipText = "Ширина области, отведенной для печати столбца (в сантиметрах)";
      grColumns.Columns[3].ToolTipText = "Расширять столбец для заполнения листа";
      grColumns.Columns[1].ReadOnly = true;
      ghColumns.Columns[1].CanIncSearch = true;
      ghColumns.DisableOrdering();
      ghColumns.ReadOnly = true;
      grColumns.ReadOnly = false;
      ghColumns.CanView = false;
      ghColumns.MarkRowsColumnIndex = 0;
      grColumns.VirtualMode = false;
      ghColumns.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(ghColumns_GetCellAttributes);
      grColumns.CellValueChanged += GrColumns_CellValueChanged;
      ghColumns.MenuOutItems.Clear(); // иначе будут параметры страницы в параметрах страницы :)
      ghColumns.ToolBarPanel = panSpbColumns;

      EFPLabel efpWholeWidthValueText = new EFPLabel(page.BaseProvider, lblWholeWidthValueText);
      efpWholeWidthValueText.DisplayName = "Ширина печатаемых столбцов";
      efpWholeWidthValueText.ToolTipText = "Полная ширина отмеченных столбцов в списке (в сантиметрах)";

      EFPLabel efpWorkWidthValueText = new EFPLabel(page.BaseProvider, lblWorkWidthValueText);
      efpWorkWidthValueText.DisplayName = "Ширина рабочей области";
      efpWorkWidthValueText.ToolTipText = "Ширина области печати, вычисляемая как" + Environment.NewLine +
        "Ширина страницы - Левое поле - Правое поле" + Environment.NewLine +
        "(в сантиметрах). Для изменения значения (если возможно) используйте" + Environment.NewLine +
        "закладки \"Страница\" и \"Поля\"";

      efpRepeatedColumns = new EFPIntEditBox(page.BaseProvider, edRepeatColumns);
      efpRepeatedColumns.ToolTipText = "Количество столбцов, которые будут повторятся для каждой полосы." + Environment.NewLine +
        "Повторяться могут только первые печататаемые столбцы, выбранные в списке." + Environment.NewLine +
        "Нулевое значение означает, что повторяемых столбцов нет";
      efpRepeatedColumns.Control.Minimum = 0;


      efpColumnSubHeaderNumbers = new EFPListComboBox(page.BaseProvider, cbColumnSubHeaderNumbers);
      efpColumnSubHeaderNumbers.ToolTipText = "Наличие дополнительной строки под заголовками столбов, содержащей номера колонок";
      //efpColumnSubHeaderNumbers.Enabled = _PageSetup.AllowColumnSubHeaderNumbers;

      page.Text = "Столбцы";
      page.ToolTipText = "Выбор печатаемых столбцов и их ширины";
      page.ImageKey = "TableColumns";

      page.DataToControls += Page_DataToControls;
      page.DataFromControls += Page_DataFromControls;
      page.PageShow += Page_PageShow;
    }

    private IEFPDataView _ControlProvider;
    private BRPageSetup _PageSetup;
    private BRDataViewSettingsDataItem _ViewData;
    private BRFontSettingsDataItem _FontData;

    #endregion

    #region Таблица столбцов

    public EFPDataGridView ghColumns;

    void ghColumns_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      IEFPDataViewColumn column = args.GetGridRow().Tag as IEFPDataViewColumn;

      switch (args.ColumnIndex)
      {
        case 0:
          if (!column.Printable)
          {
            args.ContentVisible = false;
            args.ReadOnly = true;
          }
          break;

        case 2:
          if (column.Printable)
          {
            bool flag = DataTools.GetBool(grColumns.Rows[args.RowIndex].Cells[0].Value);
            if (!flag)
            {
              args.Grayed = true;
              //Args.ContentVisible = false;
              args.ReadOnly = true;
              args.ReadOnlyMessage = "Нельзя задавать ширину столбца, который не печатается";
            }
          }
          else
          {
            args.ContentVisible = false;
            args.ReadOnly = true;
          }
          break;
      }
    }

    private void GrColumns_CellValueChanged(object sender, DataGridViewCellEventArgs args)
    {
      if (_InsideInitColumns)
        return;

      _InsideInitColumns = true;
      try
      {
        DataGridViewRow gridRow = grColumns.Rows[args.RowIndex];
        IEFPDataViewColumn column = (IEFPDataViewColumn)(gridRow.Tag);
        switch (args.ColumnIndex)
        {
          case 0:
            _ViewData.SetColumnPrinted(column, DataTools.GetBool(gridRow.Cells[0].Value));
            break;
          case 2:
            _ViewData.SetColumnWidth(column, (int)Math.Round(DataTools.GetDouble(gridRow.Cells[2].Value) * 100.0, 0, MidpointRounding.AwayFromZero));
            break;
          case 3:
            _ViewData.SetColumnAutoGrow(column, DataTools.GetBool(gridRow.Cells[3].Value));
            break;
        }
        InitColumnWidths();
        CalculateWholeWidth();
      }
      finally
      {
        _InsideInitColumns = false;
      }
    }

    #endregion

    #region Полная ширина

    /// <summary>
    /// Вычисление полной ширины всех печататаемых столбцов
    /// </summary>
    private void CalculateWholeWidth()
    {
      int n = 0;
      double w = 0.0;
      for (int i = 0; i < grColumns.Rows.Count; i++)
      {
        if (DataTools.GetBool(grColumns.Rows[i].Cells[0].Value))
        {
          n++;
          w += DataTools.GetDouble(grColumns.Rows[i].Cells[2].Value);
        }
      }
      lblWholeWidthValueText.Text = w.ToString("0.00");
      lblWorkWidthValueText.Text = (((double)(_PageSetup.PrintAreaWidth)) / 100.0).ToString("0.00");

      EFPControlBase.SetLabelForeColor(lblWholeWidth, n > 0 ? UIValidateState.Ok : UIValidateState.Error);
    }

    #endregion

    public EFPIntEditBox efpRepeatedColumns;

    public EFPListComboBox efpColumnSubHeaderNumbers;

    #region SettingsDialogPage

    private bool _InsideInitColumns;

    private void Page_DataToControls(object sender, EventArgs args)
    {
      _InsideInitColumns = true;
      try
      {
        IEFPDataViewColumn[] columns = _ControlProvider.VisibleColumns;
        grColumns.RowCount = columns.Length;
        for (int i = 0; i < columns.Length; i++)
        {
          IEFPDataViewColumn column = columns[i];
          DataGridViewRow gridRow = grColumns.Rows[i];
          gridRow.Tag = column;

          gridRow.Cells[0].Value = _ViewData.GetColumnPrinted(column);
          gridRow.Cells[1].Value = columns[i].DisplayName;
          gridRow.Cells[3].Value = _ViewData.GetColumnAutoGrow(column);
        }
        InitColumnWidths();
        CalculateWholeWidth();
      }
      finally
      {
        _InsideInitColumns = false;
      }
      efpRepeatedColumns.Value = _ViewData.RepeatedColumnCount;
      efpColumnSubHeaderNumbers.SelectedIndex = (int)_ViewData.ColumnSubHeaderNumbers;
    }

    private void InitColumnWidths()
    {
      foreach (DataGridViewRow gridRow in grColumns.Rows)
      {
        IEFPDataViewColumn column = (IEFPDataViewColumn)(gridRow.Tag);
        gridRow.Cells[2].Value = _ViewData.GetRealColumnWidth(column, _FontData) / 100.0;
      }
    }

    private void Page_DataFromControls(object sender, EventArgs args)
    {
      // Хотя параметры столбцы переносятся динамически в процессе редактирования таблички,
      // надо передавать значения еще раз.
      // Иначе, если был выбран готовый набор, то параметры попали в табличку, но не попадут в сохраняемый набор параметров
      IEFPDataViewColumn[] columns = _ControlProvider.VisibleColumns;
      for (int i = 0; i < columns.Length; i++)
      {
        IEFPDataViewColumn column = columns[i];
        DataGridViewRow gridRow = grColumns.Rows[i];

        _ViewData.SetColumnAutoGrow(column, DataTools.GetBool(gridRow.Cells[3].Value));
        _ViewData.SetColumnWidth(column, (int)Math.Round(DataTools.GetDouble(gridRow.Cells[2].Value) * 100.0, 0, MidpointRounding.AwayFromZero));
        _ViewData.SetColumnPrinted(column, DataTools.GetBool(gridRow.Cells[0].Value));
      }

      _ViewData.RepeatedColumnCount = efpRepeatedColumns.Value;
      _ViewData.ColumnSubHeaderNumbers = (BRDataViewColumnSubHeaderNumbersMode)(efpColumnSubHeaderNumbers.SelectedIndex);
    }

    private void Page_PageShow(object sender, EventArgs args)
    {
      if (grColumns.RowCount > 0)
      {
        InitColumnWidths();
        CalculateWholeWidth();
      }
    }

    #endregion
  }
}
