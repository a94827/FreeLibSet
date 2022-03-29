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
  /// ����� ������ ��������� �������
  /// </summary>
  internal partial class FiasSearchForm : Form
  {
    #region ����������� �����

    public FiasSearchForm()
    {
      Icon = EFPApp.MainImageIcon("Find");
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpText = new EFPHistComboBox(efpForm, cbText);
      efpText.CanBeEmpty = false;
      efpText.CommandItems.UseStatusBarRC = false;

      efpDistrict = new EFPCheckBox(efpForm, cbDistrict);
      efpCity = new EFPCheckBox(efpForm, cbCity);
      efpVillage = new EFPCheckBox(efpForm, cbVillage);
      efpPlanningStructure = new EFPCheckBox(efpForm, cbPlanningStructure);
      efpStreet = new EFPCheckBox(efpForm, cbStreet);
      EFPCheckBox.AddGroupAtLeastOneCheck(efpDistrict, efpCity, efpVillage, efpPlanningStructure, efpStreet);

      efpStartAddress = new EFPTextBox(efpForm, edStartAddress);
      efpStartAddress.CommandItems.UseStatusBarRC = false;

      efpActual = new EFPCheckBox(efpForm, cbActual);
    }

    #endregion

    #region ����

    public EFPHistComboBox efpText;

    public EFPCheckBox efpDistrict, efpCity, efpVillage, efpPlanningStructure, efpStreet;

    public EFPTextBox efpStartAddress;

    public EFPCheckBox efpActual;

    #endregion
  }

  /// <summary>
  /// ����� ��������� ������ ������� - ����������� ������
  /// </summary>
  internal class FiasSearchResultForm : OKCancelGridForm
  {
    #region �����������

    public FiasSearchResultForm(FiasUI ui, FiasAddress[] addresses)
    {
      _UI = ui;
      _Addresses = addresses;

      base.Text = "���������� ������ (" + addresses.Length.ToString() + ")";
      base.Icon = EFPApp.MainImageIcon("Find");

      gh = new EFPDataGridView(ControlWithToolBar);
      gh.Columns.AddTextFill("AddressText", false, "�����", 100, 10);
      if (ui.ShowGuidsInTables)
      {
        gh.Columns.AddText("AOGUID", false, "AOGUID", 36, 36);
        gh.Columns.LastAdded.CanIncSearch = true;
      }
      if (ui.ShowGuidsInTables)
      {
        gh.Columns.AddText("RecId", false, "RecId (������������ ������������� ������)", 36, 36);
        gh.Columns.LastAdded.CanIncSearch = true;
      }
      if (ui.DBSettings.UseHistory)
      {
        gh.Columns.AddBool("Actuality", false, "������������");
        gh.Columns.AddBool("Live", false, "Live");
      }
      if (ui.DBSettings.UseDates)
      {
        gh.Columns.AddDate("STARTDATE", false, "���� ������ ��������");
        gh.Columns.AddDate("ENDDATE", false, "���� ��������� ��������");
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
      ciDetails.MenuText = "�����������";
      ciDetails.ImageKey = "Fias.Details";
      ciDetails.ToolTipText = "��������� ���������� �� ������� ������ �������������� ����";
      ciDetails.Click += new EventHandler(ciDetails_Click);
      ciDetails.GroupBegin = true;
      ciDetails.GroupEnd = true;
      gh.CommandItems.Add(ciDetails);

      if (addresses.Length == FiasTools.AddressSearchLimit)
      {
        InfoLabel lbl = base.AddInfoLabel(DockStyle.Bottom);
        lbl.Text = "���������� ��������� �������� ���� ����������";
        lbl.Icon = MessageBoxIcon.Warning;
      }
    }

    #endregion

    #region ����

    public readonly EFPDataGridView gh;

    FiasUI _UI;
    FiasAddress[] _Addresses;

    #endregion

    #region ����������� ���������� ���������

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
      dlg.Title = "�������� ������";
      dlg.Address = addr;
      dlg.ReadOnly = true;
      dlg.InsideSearch = true;
      dlg.ShowDialog();
    }

    #endregion

    #region ������� ����

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