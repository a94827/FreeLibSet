// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.FIAS;
using FreeLibSet.DependedValues;
using FreeLibSet.Controls;
using FreeLibSet.Controls.FIAS;

namespace FreeLibSet.Forms.FIAS
{
  /// <summary>
  /// ��������� ��������� ��������� ���� ������ ���� (������� FiasDBSettings).
  /// ������������ ��� ��������� �������� �� ��������� ������.
  /// ����� ����� �������������� ��� �������������� �������� � ���������������� ����,
  /// ���� � ������� ����������� ����� ������������� ������������
  /// </summary>
  public sealed class EFPFiasDBSettingsPanel : EFPControl<FiasDBSettingsPanel>
  {
    #region �����������

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� �������</param>
    public EFPFiasDBSettingsPanel(EFPBaseProvider baseProvider, FiasDBSettingsPanel control)
      : base(baseProvider, control, false)
    {
      efpUseHouse = new EFPCheckBox(baseProvider, control.cbUseHouse);
      efpUseRoom = new EFPCheckBox(baseProvider, control.cbUseRoom);
      efpUseRoom.AllowDisabledChecked = true;
      efpUseRoom.DisabledChecked = false;
      efpUseRoom.EnabledEx = efpUseHouse.CheckedEx;

      efpRegionCodes = new EFPUserSelComboBox(baseProvider, control.cbRegionCodes);
      efpRegionCodes.Control.PopupClick += new EventHandler(efpRegionCodes_PopupClick);
      efpRegionCodes.Control.ClearButton = true;
      efpRegionCodes.Control.ClearClick += new EventHandler(Control_ClearClick);

      efpUseHistory = new EFPCheckBox(baseProvider, control.cbUseHistory);

      efpUseOKATO = new EFPCheckBox(baseProvider, control.cbUseOKATO);
      efpUseOKTMO = new EFPCheckBox(baseProvider, control.cbUseOKTMO);
      efpUseIFNS = new EFPCheckBox(baseProvider, control.cbUseIFNS);

      efpUseDates = new EFPCheckBox(baseProvider, control.cbUseDates);
      efpUseDates.AllowDisabledChecked = true;
      efpUseDates.DisabledChecked = true;
      efpUseDates.EnabledEx = new DepNot(efpUseHistory.CheckedEx);

      this.DBSettings = FiasDBSettings.DefaultSettings; // ������������� ��������� �������� �� ���������
    }

    /// <summary>
    /// ���������� "��������� �� ����".
    /// </summary>
    protected override string DefaultDisplayName { get { return "��������� �� ����"; } }

    #endregion

    #region ����

    private EFPCheckBox efpUseHouse, efpUseRoom;
    private EFPUserSelComboBox efpRegionCodes;
    private EFPCheckBox efpUseHistory;
    private EFPCheckBox efpUseOKATO, efpUseOKTMO, efpUseIFNS, efpUseDates;

    #endregion

    #region ����� ��������

    string[] RegionCodes;

    private void UpdateRegionCodesText()
    {
      if (RegionCodes == null)
        efpRegionCodes.Text = "���";
      else
        efpRegionCodes.Text = String.Join(", ", RegionCodes);
      efpRegionCodes.ClearButtonEnabled = (RegionCodes != null);
    }

    void efpRegionCodes_PopupClick(object sender, EventArgs args)
    {
      string[] Codes = new string[FiasTools.RegionCodes.Rows.Count];
      string[] Names = new string[FiasTools.RegionCodes.Rows.Count];
      for (int i = 0; i < FiasTools.RegionCodes.Rows.Count; i++)
      {
        Codes[i] = FiasTools.RegionCodes.Rows[i]["Code"].ToString();
        Names[i] = FiasTools.RegionCodes.Rows[i]["Name"].ToString();
      }

      ListSelectDialog dlg = new ListSelectDialog();
      dlg.Title = "����� ��������";
      dlg.MultiSelect = true;
      dlg.Items = Codes;
      dlg.SubItems = Names;
      if (RegionCodes != null)
        dlg.SetSelectedItems(RegionCodes);
      dlg.DialogPosition.PopupOwnerControl = efpRegionCodes.Control;
      dlg.CanBeEmpty = true;
      if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        return;

      if (dlg.AreAllSelected || dlg.AreAllUnselected)
        RegionCodes = null;
      else
        RegionCodes = dlg.GetSelectedItems();
      UpdateRegionCodesText();
    }

    void Control_ClearClick(object sender, EventArgs args)
    {
      RegionCodes = null;
      UpdateRegionCodesText();
    }

    #endregion

    #region ������� ������

    /// <summary>
    /// ��������������� ��� ������������� ������.
    /// ��������� �������� �������������� ���� ������.
    /// ��� ���������� �������� ������ ��� ��������� ����� ������
    /// </summary>
    public FiasDBSettings DBSettings
    {
      get
      {
        FiasDBSettings s = new FiasDBSettings();
        s.UseHouse = efpUseHouse.Checked;
        s.UseRoom = efpUseRoom.Checked;
        if (RegionCodes != null)
        {
          for (int i = 0; i < RegionCodes.Length; i++)
            s.RegionCodes.Add(RegionCodes[i]);
        }
        s.UseHistory = efpUseHistory.Checked;
        s.UseOKATO = efpUseOKATO.Checked;
        s.UseOKTMO = efpUseOKTMO.Checked;
        s.UseIFNS = efpUseIFNS.Checked;
        s.UseDates = efpUseDates.Checked;

        return s;
      }
      set
      {
        efpUseHouse.Checked = value.UseHouse;
        efpUseRoom.Checked = value.UseRoom;

        if (value.RegionCodes.Count == 0)
          RegionCodes = null;
        else
        {
          RegionCodes = new string[value.RegionCodes.Count];
          value.RegionCodes.CopyTo(RegionCodes, 0);
        }
        UpdateRegionCodesText();
        efpUseHistory.Checked = value.UseHistory;
        efpUseOKATO.Checked = value.UseOKATO;
        efpUseOKTMO.Checked = value.UseOKTMO;
        efpUseIFNS.Checked = value.UseIFNS;
        efpUseDates.Checked = value.UseDates;
      }
    }

    #endregion

    /// <summary>
    /// �������� �������� ���� ������ ����
    /// </summary>
    /// <param name="dbSettings">��������������� ���������. �� ����� ���� null</param>
    public static void ShowSettings(FiasDBSettings dbSettings)
    {
      ShowSettings(dbSettings, null);
    }

    internal static void ShowSettings(FiasDBSettings dbSettings, IFiasSource source)
    {
      if (dbSettings == null)
        throw new ArgumentNullException("dbSettings");

      OKCancelForm frm = new OKCancelForm(true);
      frm.Text = "�������� �������� ���� ������ ����";
      frm.Icon = EFPApp.MainImageIcon("FIAS.Address");

      FiasDBSettingsPanel panel = new FiasDBSettingsPanel();
      panel.Enabled = false;
      frm.MainPanel.Controls.Add(panel);
      WinFormsTools.OkCancelFormToOkOnly(frm);

      int infoLabelH = 0;
      if (source != null)
      {
        // 30.07.2020
        InfoLabel lbl = frm.AddInfoLabel(System.Windows.Forms.DockStyle.Bottom);
        lbl.Text = "���� ������������ ��������������: " + source.ActualDate.ToString("d", System.Globalization.CultureInfo.CurrentUICulture);
        infoLabelH = lbl.Height;
      }

      frm.Size = frm.Size + panel.Size - frm.MainPanel.ClientSize+new System.Drawing.Size(0, infoLabelH);

      EFPFiasDBSettingsPanel efpPanel = new EFPFiasDBSettingsPanel(frm.FormProvider, panel);
      efpPanel.DBSettings = dbSettings;

      EFPApp.ShowDialog(frm, true);
    }
  }
}
