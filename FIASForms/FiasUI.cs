using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.FIAS;
using System.Windows.Forms;
using System.Data;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Controls;
using FreeLibSet.UICore;
using FreeLibSet.Controls.FIAS;

/*
 * The BSD License
 * 
 * Copyright (c) 2020, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace FreeLibSet.Forms.FIAS
{
  /// <summary>
  /// ��������� ����������������� ���������� ��� ������ � ����.
  /// ������ ��������� � ������������ ���������� �� ������� �������.
  /// ������ �� ������ ���������� ���� ��������� ����������������� ����������
  /// ��������� � ������� ������ ����������� �� ��������� ������ ����������, � ������� ������ EFApp.InitApp().
  /// ��� ��������� ���������� ���������� �������� ������ �� ������ FiasUI � ������ EFPApp.RICreators,
  /// </summary>
  public sealed class FiasUI : FreeLibSet.Forms.RI.IEFPAppRICreator
  {
    #region �����������

    /// <summary>
    /// ������� ��������� �������.
    /// �� ������ ������ ������ ���� ������ ����� EFPApp.InitApp() (� ��� �� ������)
    /// </summary>
    /// <param name="source"></param>
    public FiasUI(IFiasSource source)
    {
      InitImages();

      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;

      EFPApp.CheckMainThread(); // ������ ���������, ��� ��� ����� EFPApp.WasInit()

      //_Level = FiasLevel.Flat;

      ShowGuidsInTables = true;
      //ShowDates = true;
      ShowDates = source.DBSettings.UseDates; // 28.04.2020
    }

    #endregion

    #region ���������� ������� � ����� ��������

    /// <summary>
    /// �������� ������ ��� ��������������
    /// </summary>
    public IFiasSource Source { get { return _Source; } }
    private IFiasSource _Source;

    /// <summary>
    /// ��������� ���� ������ �������������� ���� (������ ��������)
    /// </summary>
    public FiasDBSettings DBSettings { get { return _Source.DBSettings; } }

    /// <summary>
    /// ���������� ��������� ��������������.
    /// �� ������������ � ���������� ����
    /// </summary>
    public FiasInternalSettings InternalSettings { get { return _Source.InternalSettings; } }

    #endregion

    #region ���������� � ���� ������������

    /// <summary>
    /// ���� ������������ ��������������
    /// </summary>
    public DateTime ActualDate { get { return _Source.ActualDate; } }

    /// <summary>
    /// ���������� ���������� �� ���� ������ ��������������.
    /// </summary>
    public FiasDBStat DBStat { get { return _Source.DBStat; } }

    /// <summary>
    /// ��������� ��������� ���� ������������.
    /// ���� ����� ����� ���������� ��������, ���� ���� �������������, ��� ���� ������������ ����� ����������
    /// </summary>
    public void UpdateActualDate()
    {
      _Source.UpdateActualDate();
    }

    #endregion

    #region ��������� ��� ��������� ������


    internal bool ShowGuidsInTables;

    internal bool ShowDates;

    #endregion

    #region ������������� �����������

    /// <summary>
    /// ���������� ����������� ���������� ExtDBDocForms
    /// </summary>
    private static void InitImages()
    {
      if (_ImagesWasInit)
        return;

      DummyForm frm = new DummyForm();
      EFPApp.AddMainImages(frm.MainImageList);
    }

    private static bool _ImagesWasInit = false;

    #endregion

    #region ����� ������ �������

    /// <summary>
    /// ���������� ������ "�����������" ��� ���������� ������
    /// </summary>
    /// <param name="address">����� ��� ���������</param>
    public void ShowDetails(FiasAddress address)
    {
      if (address == null)
        throw new ArgumentNullException("address");

      SimpleGridForm form = new SimpleGridForm();
      form.Text = "�����������";
      FiasHandler handler = new FiasHandler(_Source);
      form.AddInfoLabel(DockStyle.Top).Text = handler.GetTextWithoutPostalCode(address);
      form.Icon = EFPApp.MainImageIcon("Fias.Details");
      EFPFiasAddressDetailGridView efp = new EFPFiasAddressDetailGridView(form.ControlWithToolBar, this);
      efp.Address = address;
      EFPApp.ShowDialog(form, true);
    }

    /// <summary>
    /// ���������� ������� �������������� � ������ ��������� ��� ��������� ��������� �������, ���� ��� ���������
    /// </summary>
    /// <param name="tableType">��� �������</param>
    /// <param name="guid">"����������" ������������� �������</param>
    public void ShowHistory(FiasTableType tableType, Guid guid)
    {
      if (guid == Guid.Empty)
      {
        EFPApp.ErrorMessageBox("������ ��� ��������� ������� �� ������");
        return;
      }

      DataTable table;
      EFPApp.BeginWait("��������� ������� �������", "FiasAddress", true);
      try
      {
        table = Source.GetTableForGuid(guid, tableType).Tables[0];
      }
      finally
      {
        EFPApp.EndWait();
      }

      SimpleGridForm form = new SimpleGridForm();
      form.Text = "������� ���������: " + FiasEnumNames.ToString(tableType, false) + " GUID=" + guid.ToString();
      form.Icon = EFPApp.MainImageIcon("View");

      EFPFiasListDataGridView ghHist = new EFPFiasListDataGridView(form.ControlWithToolBar, this, tableType, true);
      ghHist.Control.DataSource = table.DefaultView;

      EFPApp.ShowDialog(form, true);
    }

    /// <summary>
    /// ����� ������.
    /// ������� ������ ���������� ������.
    /// ����� ����������� ����� � ��������� ����� �� ������� ��������� �������, �� ������� ����� ������� ����.
    /// </summary>
    /// <param name="address">�� ����� - ������� ����� ��� ������. �� ������ - ��������� �����</param>
    /// <returns>True, ���� ������������ �������� ����� � ������ �����</returns>
    public bool SearchAddress(ref FiasAddress address)
    {
      #region ������ ����������

      FiasSearchForm frm1 = new FiasSearchForm();
      FiasAddress startAddress = address.Clone();
      startAddress.ClearStartingWith(FiasLevel.Street);

      FiasHandler handler = new FiasHandler(_Source);
      handler.FillAddress(startAddress);
      startAddress.ClearAuxInfo();
      if (startAddress.IsEmpty)
        frm1.efpStartAddress.Text = "���� �������������";
      else
      {
        string s = startAddress.ToString();
        if (ShowGuidsInTables)
          s += Environment.NewLine + "(AOGUID=" + startAddress.AOGuid.ToString() + ")";
        frm1.efpStartAddress.Text = s;
      }

      CfgPart cfg;
      EFPConfigSectionInfo CfgInfo = new EFPConfigSectionInfo("FiasSearchAddress", EFPConfigCategories.UserParams);
      using (EFPApp.ConfigManager.GetConfig(CfgInfo, EFPConfigMode.Read, out cfg))
      {
        frm1.efpText.HistList = cfg.GetHist("Text");
        frm1.efpVillage.Checked = cfg.GetBoolDef("Village", true);
        frm1.efpPlanningStructure.Checked = cfg.GetBoolDef("PlanningStructure", true);
        frm1.efpStreet.Checked = cfg.GetBoolDef("Street", true);
        if (DBSettings.UseHistory)
          frm1.efpActual.Checked = cfg.GetBoolDef("ActualOnly", true);
        else
        {
          frm1.efpActual.Checked = true;
          frm1.efpActual.Enabled = false;
        }
      }

      if (EFPApp.ShowDialog(frm1, true) != DialogResult.OK)
        return false;

      using (EFPApp.ConfigManager.GetConfig(CfgInfo, EFPConfigMode.Write, out cfg))
      {
        cfg.SetHist("Text", frm1.efpText.HistList);
        cfg.SetBool("Village", frm1.efpVillage.Checked);
        cfg.SetBool("PlanningStructure", frm1.efpPlanningStructure.Checked);
        cfg.SetBool("Street", frm1.efpStreet.Checked);
        if (DBSettings.UseHistory)
          cfg.SetBool("ActualOnly", frm1.efpActual.Checked);
      }

      #endregion

      #region ����� �������

      FiasAddressSearchParams searchParams = new FiasAddressSearchParams();
      searchParams.Text = frm1.efpText.Text;
      List<FiasLevel> lvls = new List<FiasLevel>();
      if (frm1.efpVillage.Checked)
        lvls.Add(FiasLevel.Village);
      if (frm1.efpPlanningStructure.Checked)
        lvls.Add(FiasLevel.PlanningStructure);
      if (frm1.efpStreet.Checked)
        lvls.Add(FiasLevel.Street);
      searchParams.Levels = lvls.ToArray();
      searchParams.ActualOnly = frm1.efpActual.Checked;
      searchParams.StartAddress = startAddress;

      FiasAddress[] a;

      EFPApp.BeginWait("����� �������� ��������", "Find", true);
      try
      {
        a = handler.FindAddresses(searchParams);
      }
      finally
      {
        EFPApp.EndWait();
      }

      #endregion

      #region ����� �����������

      if (a.Length == 0)
      {
        EFPApp.MessageBox("�� ������� �� ������ ������." + Environment.NewLine + Environment.NewLine +
          searchParams.ToString(), "���������� ������");
        return false;
      }

      bool res = false;
      using (FiasSearchResultForm frm2 = new FiasSearchResultForm(this, a))
      {
        frm2.AddInfoLabel(DockStyle.Top).Text = searchParams.ToString();

        if (EFPApp.ShowDialog(frm2, false) == DialogResult.OK)
        {
          FiasAddress addr2 = a[frm2.gh.CurrentRowIndex];
          // �������� �� ���������� �����
          if (addr2.AOGuid == Guid.Empty)
            throw new BugException("��������� ����� �� �������� AOGUID");
          address = new FiasAddress();
          address.AOGuid = addr2.AOGuid;
          handler.FillAddress(address);

          res = true;
        }
      }

      #endregion

      return res;
    }

    /// <summary>
    /// �������� �������� ���� ������ ��� ����������� ��������������
    /// </summary>
    public void ShowDBSettings()
    {
      EFPFiasDBSettingsPanel.ShowSettings(DBSettings, this._Source);
    }

    /// <summary>
    /// ������� ������ � �������� ����������
    /// </summary>
    public void ShowClassifUpdateTable()
    {
      DataTable table = _Source.GetClassifUpdateTable();

      SimpleGridForm frm = new SimpleGridForm();
      frm.Text = "���������� ��������������";
      frm.Icon = EFPApp.MainImageIcon("Information");

      EFPDataGridView gh = new EFPDataGridView(frm.ControlWithToolBar);
      gh.Control.AutoGenerateColumns = false;
      gh.Columns.AddImage("Image");
      gh.Columns.AddDate("ActualDate", true, "���� ������������");
      gh.Columns.AddDateTime("StartTime", true, "���������� ��������");
      gh.Columns.AddDateTime("FinishTime", true, "���������� ���������");
      gh.Columns.AddText("TimeSpan", false, "�����", 8, 8);
      gh.Columns.LastAdded.TextAlign = HorizontalAlignment.Right;
      gh.Columns.AddInt("AddrObCount", true, "���-�� �������� ��������", 12);
      gh.Columns.LastAdded.GridColumn.DefaultCellStyle.Format = "#,##0";
      gh.Columns.LastAdded.SizeGroup = "ObjCount";
      if (_Source.DBSettings.UseHouse)
      {
        gh.Columns.AddInt("HouseCount", true, "���-�� ������", 12);
        gh.Columns.LastAdded.GridColumn.DefaultCellStyle.Format = "#,##0";
        gh.Columns.LastAdded.SizeGroup = "ObjCount";
      }
      if (_Source.DBSettings.UseRoom)
      {
        gh.Columns.AddInt("RoomCount", true, "���-�� ���������", 12);
        gh.Columns.LastAdded.GridColumn.DefaultCellStyle.Format = "#,##0";
        gh.Columns.LastAdded.SizeGroup = "ObjCount";
      }
      gh.DisableOrdering();
      gh.GetCellAttributes += ghClassifUpdate_GetCellAttributes;
      gh.ReadOnly = true;
      gh.Control.ReadOnly = true;
      gh.CanView = false;
      gh.ShowRowCountInTopLeftCellToolTipText = true;
      gh.Control.MultiSelect = true; // 24.02.2021
      gh.Control.DataSource = table;

      EFPApp.ShowDialog(frm, true);
    }

    void ghClassifUpdate_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      EFPDataGridView gh =(EFPDataGridView )sender;
      switch (args.ColumnName)
      { 
        case "Image":
          int errorCount = DataTools.GetInt(args.DataRow, "ErrorCount");
          if (errorCount > 0)
            args.Value = EFPApp.MainImages.Images["Error"];
          else if (args.DataRow.IsNull("FinishTime"))
            args.Value = EFPApp.MainImages.Images["Cancel"]; // 24.02.2021
          else if (DataTools.GetBool(args.DataRow, "Cumulative"))
            args.Value = EFPApp.MainImages.Images["Time"];
          else
            args.Value = EFPApp.MainImages.Images["New"];
          args.ToolTipText = "������ ��� ��������: " + errorCount.ToString();
          break;

        case "ActualDate": // 24.02.2021
          if (args.RowIndex > 0)
          {
            DataRow PrevRow = gh.SourceAsDataTable.Rows[args.RowIndex - 1];
            DateTime ad1 = DataTools.GetDateTime(PrevRow, "ActualDate").Date;
            DateTime ad2 = DataTools.GetDateTime(args.DataRow, "ActualDate").Date;
            if (ad1 == ad2 && (!PrevRow.IsNull("FinishTime")))
            {
              args.ColorType = EFPDataGridViewColorType.Warning;
              args.ToolTipText = "��������� �������� ���������� �� " + ad2.ToString("d");
            }
          }
          break;

        case "TimeSpan":
          DateTime? dt1 = DataTools.GetNullableDateTime(args.DataRow, "StartTime");
          DateTime? dt2 = DataTools.GetNullableDateTime(args.DataRow, "FinishTime");
          if (dt1.HasValue && dt2.HasValue)
          { 
            TimeSpan ts=dt2.Value-dt1.Value;
            string s = ts.ToString();
            // ������� ������� �����
            int p = s.LastIndexOf('.');
            if (p >= 0)
              s = s.Substring(0, p);
            args.Value = s;
          }
          break;
      }
    }

    #endregion

    #region IEFPAppRICreator Members

    FreeLibSet.Forms.RI.IEFPAppRIItem FreeLibSet.Forms.RI.IEFPAppRICreator.Create(FreeLibSet.RI.RIItem item, EFPBaseProvider baseProvider)
    {
      if (item is FreeLibSet.FIAS.RI.FiasAddressPanel)
        return new FiasAddressPanelItem(this, (FreeLibSet.FIAS.RI.FiasAddressPanel)item, baseProvider);
      if (item is FreeLibSet.FIAS.RI.FiasAddressComboBox)
        return new FiasAddressComboBoxItem(this, (FreeLibSet.FIAS.RI.FiasAddressComboBox)item, baseProvider);
      if (item is FreeLibSet.FIAS.RI.FiasAddressDialog)
        return new FiasAddressDialogItem(this, (FreeLibSet.FIAS.RI.FiasAddressDialog)item);

      return null;
    }

    private class FiasAddressPanelItem : EFPFiasAddressPanel, FreeLibSet.Forms.RI.IEFPAppRIControlItem
    {
      #region �����������

      public FiasAddressPanelItem(FiasUI ui, FreeLibSet.FIAS.RI.FiasAddressPanel riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new FiasAddressPanel(), ui, riItem.EditorLevel)
      {
        base.PostalCodeEditable = riItem.PostalCodeEditable;
        base.MinRefBookLevel = riItem.MinRefBookLevel;
        base.CanBeEmptyMode = riItem.CanBeEmptyMode;
        base.CanBePartialMode = riItem.CanBePartialMode;

        riItem.InternalSetSource(ui.Source);
        _RIItem = riItem;
      }

      FreeLibSet.FIAS.RI.FiasAddressPanel _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.Address = _RIItem.Address;
        //base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Address = base.Address;
      }

      #endregion
    }

    private class FiasAddressComboBoxItem : EFPFiasAddressComboBox, FreeLibSet.Forms.RI.IEFPAppRIControlItem
    {
      #region �����������

      public FiasAddressComboBoxItem(FiasUI ui, FreeLibSet.FIAS.RI.FiasAddressComboBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new UserSelComboBox(), ui)
      {
        base.EditorLevel = riItem.EditorLevel;
        base.PostalCodeEditable = riItem.PostalCodeEditable;
        base.MinRefBookLevel = riItem.MinRefBookLevel;
        base.CanBeEmptyMode = riItem.CanBeEmptyMode;
        base.CanBePartialMode = riItem.CanBePartialMode;

        base.TextFormat = riItem.TextFormat;
        base.Control.Width = 300; // ??

        riItem.InternalSetSource(ui.Source);
        _RIItem = riItem;
      }

      FreeLibSet.FIAS.RI.FiasAddressComboBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.Address = _RIItem.Address;
        //base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Address = base.Address;
      }

      #endregion
    }

    private class FiasAddressDialogItem: FreeLibSet.Forms.RI.IEFPAppRIStandardDialogItem
    {
      #region �����������

      public FiasAddressDialogItem(FiasUI ui, FreeLibSet.FIAS.RI.FiasAddressDialog riDialog)
      {
        riDialog.InternalSetSource(ui.Source);
        _RIDialog = riDialog;

        _WinDlg = new FreeLibSet.Forms.FIAS.FiasAddressDialog(ui);
        _WinDlg.Title = riDialog.Title;
        //_WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.EditorLevel = riDialog.EditorLevel;
        _WinDlg.PostalCodeEditable = riDialog.PostalCodeEditable;
        _WinDlg.MinRefBookLevel = riDialog.MinRefBookLevel;
        _WinDlg.CanBeEmptyMode = riDialog.CanBeEmptyMode;
        _WinDlg.CanBePartialMode = riDialog.CanBePartialMode;
        _WinDlg.ReadOnly = riDialog.ReadOnly;
      }

      private FreeLibSet.FIAS.RI.FiasAddressDialog _RIDialog;
      private FreeLibSet.Forms.FIAS.FiasAddressDialog _WinDlg;

      #endregion

      #region IEFPAppRIStandardDialogItem Members

      public void WriteValues()
      {
        _WinDlg.Address = _RIDialog.Address;
      }

      public void ReadValues()
      {
        _RIDialog.Address = _WinDlg.Address;
      }

      public FreeLibSet.RI.DialogResult ShowDialog()
      {
        return (FreeLibSet.RI.DialogResult)(int)(_WinDlg.ShowDialog());
      }

      #endregion
    }

    #endregion
  }
}
