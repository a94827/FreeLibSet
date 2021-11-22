// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.FIAS;
using FreeLibSet.Config;
using FreeLibSet.Controls;

namespace FreeLibSet.Forms.FIAS
{
  /// <summary>
  /// Форма поиска адресного объекта
  /// </summary>
  internal partial class FiasSearchForm : Form
  {
    #region Конструктор формы

    public FiasSearchForm()
    {
      Icon = EFPApp.MainImageIcon("Find");
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpText = new EFPHistComboBox(efpForm, cbText);
      efpText.CanBeEmpty = false;
      efpText.CommandItems.UseStatusBarRC = false;

      efpVillage = new EFPCheckBox(efpForm, cbVillage);
      efpPlanningStructure = new EFPCheckBox(efpForm, cbPlanningStructure);
      efpStreet = new EFPCheckBox(efpForm, cbStreet);
      EFPCheckBox.AddGroupAtLeastOneCheck(efpVillage, efpPlanningStructure, efpStreet);

      efpStartAddress = new EFPTextBox(efpForm, edStartAddress);
      efpStartAddress.CommandItems.UseStatusBarRC = false;

      efpActual = new EFPCheckBox(efpForm, cbActual);
    }

    #endregion

    #region Поля

    public EFPHistComboBox efpText;

    public EFPCheckBox efpVillage, efpPlanningStructure, efpStreet;

    public EFPTextBox efpStartAddress;

    public EFPCheckBox efpActual;

    #endregion
  }

  /// <summary>
  /// Форма просмотра списка адресов - результатов поиска
  /// </summary>
  internal class FiasSearchResultForm : OKCancelGridForm
  {
    #region Конструктор

    public FiasSearchResultForm(FiasUI ui, FiasAddress[] addresses)
    {
      _UI = ui;
      _Addresses = addresses;

      base.Text = "Результаты поиска (" + addresses.Length.ToString() + ")";
      base.Icon = EFPApp.MainImageIcon("Find");

      gh = new EFPDataGridView(ControlWithToolBar);
      gh.Columns.AddTextFill("AddressText", false, "Адрес", 100, 10);
      if (ui.ShowGuidsInTables)
      {
        gh.Columns.AddText("AOGUID", false, "AOGUID", 36, 36);
        gh.Columns.LastAdded.CanIncSearch = true;
      }
      if (ui.ShowGuidsInTables)
      {
        gh.Columns.AddText("RecId", false, "RecId (неустойчивый идентификатор записи)", 36, 36);
        gh.Columns.LastAdded.CanIncSearch = true;
      }
      if (ui.DBSettings.UseHistory)
      {
        gh.Columns.AddBool("Actuality", false, "Актуальность");
        gh.Columns.AddBool("Live", false, "Live");
      }
      if (ui.DBSettings.UseDates)
      {
        gh.Columns.AddDate("STARTDATE", false, "Дата начала действия");
        gh.Columns.AddDate("ENDDATE", false, "Дата окончание действия");
      }
      gh.DisableOrdering();
      gh.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(gh_GetRowAttributes);
      gh.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(gh_GetCellAttributes);
      gh.UseRowImages = true;
      gh.Control.RowCount = addresses.Length;

      gh.Control.ReadOnly = true;
      gh.ReadOnly = true;
      gh.EditData += new EventHandler(gh_EditData);
      gh.CommandItems.EnterAsOk = true;

      EFPCommandItem ciDetails = new EFPCommandItem("View", "Details");
      ciDetails.MenuText = "Подробности";
      ciDetails.ImageKey = "Fias.Details";
      ciDetails.ToolTipText = "Подробная информация по каждому уровню классификатора ФИАС";
      ciDetails.Click += new EventHandler(ciDetails_Click);
      ciDetails.GroupBegin = true;
      ciDetails.GroupEnd = true;
      gh.CommandItems.Add(ciDetails);

      if (addresses.Length == FiasTools.AddressSearchLimit)
      {
        InfoLabel lbl = base.AddInfoLabel(DockStyle.Bottom);
        lbl.Text = "Количество найденных объектов было ограничено";
        lbl.Icon = MessageBoxIcon.Warning;
      }
    }

    #endregion

    #region Поля

    public readonly EFPDataGridView gh;

    FiasUI _UI;
    FiasAddress[] _Addresses;

    #endregion

    #region Обработчики табличного просмотра

    void gh_GetRowAttributes(object sender, EFPDataGridViewRowAttributesEventArgs args)
    {
      if (args.RowIndex < 0)
        return;

      FiasAddress addr = _Addresses[args.RowIndex];
      if ((addr.Actuality == FiasActuality.Historical) || (!addr.Live??true))
        args.Grayed = true;
      if (addr.Messages.Count>0)
        args.AddRowErrorMessages(addr.Messages);
    }

    void gh_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.RowIndex < 0)
        return;

      FiasAddress addr = _Addresses[args.RowIndex];

      switch (args.ColumnName)
      {
        case "AddressText":
          args.Value = addr.ToString();
          break;
        case "AOGUID":
          args.Value = addr.AOGuid;
          break;
        case "RecId":
          args.Value = addr.AORecId;
          break;
        case "Actuality":
          switch (addr.Actuality)
          {
            case FiasActuality.Actual:
              args.Value = true;
              break;
            case FiasActuality.Historical:
              args.Value = false;
              break;
            default:
              args.ContentVisible = false;
              break;
          }
          break;
        case "Live":
          if (addr.Live.HasValue)
            args.Value = addr.Live.Value;
          else
            args.ContentVisible = false;
          break;
        case "STARTDATE":
          args.Value = addr.StartDate;
          break;
        case "ENDDATE":
          args.Value = addr.EndDate;
          break;
      }
    }

    void gh_EditData(object sender, EventArgs args)
    {
      if (!gh.CheckSingleRow())
        return;

      FiasAddress addr = _Addresses[gh.CurrentRowIndex];
      FiasAddressDialog dlg = new FiasAddressDialog(_UI);
      dlg.EditorLevel = FiasEditorLevel.Street;
      dlg.Title = "Просмотр адреса";
      dlg.Address = addr;
      dlg.ReadOnly = true;
      dlg.InsideSearch = true;
      dlg.ShowDialog();
    }

    #endregion

    #region Команды меню

    private void ciDetails_Click(object sender, EventArgs args)
    {
      if (!gh.CheckSingleRow())
        return;

      FiasAddress addr = _Addresses[gh.CurrentRowIndex];
      _UI.ShowDetails(addr);
    }

    #endregion
  }
}