using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using System.Data;
using System.Windows.Forms;
using FreeLibSet.Controls;
using FreeLibSet.Collections;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2016, Ageyev A.V.
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

namespace FreeLibSet.Forms
{
  /// <summary>
  /// ���������, ����������� ������ � ������ �������� ����� ����� � ������ ������������.
  /// ������������ ����������� EFPConfigParamSetComboBox.
  /// ���������, ��� �������, ����������� ��������������� ������, � ������� ���������� ParamSetComboBox.
  /// </summary>
  public interface IEFPConfigParamSetHandler
  {
    /// <summary>
    /// ���� ����� ������ ���������� ��������� ����������� ��������� ����� � ������������ �� ���������� � ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    void ConfigToControls(CfgPart cfg);

    /// <summary>
    /// ���� ����� ������ �������� ��������� ����������� ��������� ����� � ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    void ConfigFromControls(CfgPart cfg);
  }

  /// <summary>
  /// ���������, ����������� ��������� ����������� ������ AuxText ��� ����������� ������ "������� ������".
  /// ������������ ����������� EFPConfigParamSetComboBox.
  /// ���������, ��� �������, ����������� ��������������� ������, � ������� ���������� ParamSetComboBox.
  /// </summary>
  public interface IEFPConfigParamSetAuxTextHandler
  {
    /// <summary>
    /// ���� ����� ���������� ���������� ����� ������������������� ������� GetAuxText()
    /// </summary>
    void BeginGetAuxText();

    /// <summary>
    /// ���� ����� ������ ������� ������ AuxText ��� ������ ������������ � ������� ����������
    /// </summary>
    /// <param name="cfg">������ ������������, ��������� ��� ������, ���������� �������� ��� ������ ������</param>
    string GetAuxText(CfgPart cfg);

    /// <summary>
    /// ���� ����� ���������� ���������� ����� ������������������ ������� GetAuxText()
    /// </summary>
    void EndGetAuxText();
  }

  /// <summary>
  /// ��������� ������������ �������� "����� ����������", ���������� ��
  /// ����������, ������ "+" � "-".
  /// ������������ ��� �������� "�������" ��������� �������� � �����, �
  /// ����� ���������������� "�������" �������.
  /// ��� �������� ������������ �������, ����������� IEFPConfigManager.
  /// ���������� ��� ������ ������������ ���������, ����������� ������ � ������ �������� � ���� �����.
  /// ������ ���� ����������� ����������� �������� ConfigSectionName (�, ��� �������������, ParamsCategory � HistoryCategory). 
  /// ����������� ������ DefaultSets ��� ������� ������(��) �� ���������.
  /// </summary>
  public class EFPConfigParamSetComboBox : EFPControl<ParamSetComboBox>
  {
    #region �����������

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� �������</param>
    /// <param name="formHandler">��������� ��� ������ � ������ ��������. ������ ������ ���� ������</param>
    public EFPConfigParamSetComboBox(EFPBaseProvider baseProvider, ParamSetComboBox control, IEFPConfigParamSetHandler formHandler)
      : base(baseProvider, control, true)
    {
      if (formHandler == null)
        throw new ArgumentNullException("formHandler");
      _FormHandler = formHandler;

      efpSelCB = new EFPTextComboBox(baseProvider, control.TheCB);
      efpSelCB.DisplayName = "������� ������";

      efpSaveButton = new EFPButton(baseProvider, control.SaveButton);
      efpSaveButton.DisplayName = "��������� �����";
      efpSaveButton.ToolTipText = "��������� ������������� �������� ��� ����� ���������������� �����" + Environment.NewLine +
        "����� �������� ������ � ���� ����� ������ ���� ������� ��� ������";

      efpDelButton = new EFPButton(baseProvider, control.DeleteButton);
      efpDelButton.DisplayName = "������� �����";
      efpDelButton.ToolTipText = "������� ���������������� ����� ��������, ��� �������� ������ � ������ �����";

      _ParamsCategory = EFPConfigCategories.UserParams;
      _HistoryCategory = EFPConfigCategories.UserHistory;

      _DefaultSets = new DefaultSetList(this);

      control.ShowImages = EFPApp.ShowListImages;

      InitConfigHandler(); // ����� ���������� �������� ConfigSectionName
    }

    #endregion

    #region ����

    private EFPTextComboBox efpSelCB;

    private EFPButton efpSaveButton;

    private EFPButton efpDelButton;

    #endregion

    #region ������������ ������ ������������

    /// <summary>
    /// ��� ��������� ������ ������������, ������������ ��� �������� ������� ��������.
    /// �� ��������� ����� EFPConfigCategories.UserParams
    /// �������� ������ ���� ����������� �� ������ ����� �� �����.
    /// </summary>
    public string ParamsCategory
    {
      get { return _ParamsCategory; }
      set
      {
        base.CheckHasNotBeenCreated();
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException();
        _ParamsCategory = value;
      }
    }
    private string _ParamsCategory;


    /// <summary>
    /// ��� ��������� ������ ������������, ������������ ��� �������� �������.
    /// �� ��������� ����� EFPConfigCategories.UserHistory
    /// �������� ������ ���� ����������� �� ������ ����� �� �����.
    /// </summary>
    public string HistoryCategory
    {
      get { return _HistoryCategory; }
      set
      {
        base.CheckHasNotBeenCreated();
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException();
        _HistoryCategory = value;
      }
    }
    private string _HistoryCategory;

    #endregion

    #region ����������� ������

    /// <summary>
    /// �������� ����� ������� � ������ ��� �������� "�� ���������"
    /// </summary>
    public sealed class DefaultSet
    {
      #region ������������

      /// <summary>
      /// ������� ������
      /// </summary>
      /// <param name="config">����� ���������� ��� �������� "�� ���������"</param>
      /// <param name="displayName">������������ ��� (��� ������)</param>
      /// <param name="imageKey">������ � ������ EFPApp.MainImages</param>
      public DefaultSet(TempCfg config, string displayName, string imageKey)
      {
        if (config == null)
          throw new ArgumentNullException("config");
        if (String.IsNullOrEmpty(displayName))
          throw new ArgumentNullException("displayName");
        if (String.IsNullOrEmpty(imageKey))
          throw new ArgumentNullException("imageKey");

        _Config = config;
        _DisplayName = displayName;
        _ImageKey = imageKey;

        _MD5Sum = config.MD5Sum();
      }

      /// <summary>
      /// ������� ������ �� ������� �� ��������� "No"
      /// </summary>
      /// <param name="config">����� ���������� ��� �������� "�� ���������"</param>
      /// <param name="displayName">������������ ��� (��� ������)</param>
      public DefaultSet(TempCfg config, string displayName)
        : this(config, displayName, "No")
      {
      }

      /// <summary>
      /// ������� ������ � ������� "�� ���������" �� ������� �� ��������� "No"
      /// </summary>
      /// <param name="config">����� ���������� ��� �������� "�� ���������"</param>
      public DefaultSet(TempCfg config)
        : this(config, "�� ���������", "No")
      {
      }

      #endregion

      #region ��������

      /// <summary>
      /// ����� ���������� ��� �������� "�� ���������"
      /// </summary>
      public TempCfg Config { get { return _Config; } }
      private TempCfg _Config;

      /// <summary>
      /// ������������ ��� (��� ������)
      /// </summary>
      public string DisplayName { get { return _DisplayName; } }
      private string _DisplayName;

      /// <summary>
      /// ������ � ������ EFPApp.MainImages
      /// </summary>
      public string ImageKey { get { return _ImageKey; } }
      private string _ImageKey;

      /// <summary>
      /// ����������� ����� ��� ������ ������������
      /// </summary>
      public string MD5Sum { get { return _MD5Sum; } }
      private string _MD5Sum;

      /// <summary>
      /// ���������� �������� DisplayName
      /// </summary>
      /// <returns>��������� �������������</returns>
      public override string ToString()
      {
        return DisplayName;
      }

      #endregion
    }

    /// <summary>
    /// ���������� �������� DefaultSets
    /// </summary>
    public sealed class DefaultSetList : ListWithReadOnly<DefaultSet>
    {
      #region ���������� �����������

      internal DefaultSetList(EFPConfigParamSetComboBox owner)
      {
        _Owner = owner;
      }

      private EFPConfigParamSetComboBox _Owner;

      #endregion

      #region ������ ����������

      /// <summary>
      /// �������� ���������, �������� � ����� � ��������� ������, � �������� ������ �� ���������
      /// </summary>
      /// <param name="displayName"></param>
      /// <param name="imageKey"></param>
      public void Add(string displayName, string imageKey)
      {
        base.Add(new DefaultSet(GetCurrentCfg(), displayName, imageKey));
      }

      /// <summary>
      /// �������� ���������, �������� � ����� � ��������� ������, � �������� ������ �� ���������
      /// </summary>
      /// <param name="displayName"></param>
      public void Add(string displayName)
      {
        base.Add(new DefaultSet(GetCurrentCfg(), displayName));
      }

      /// <summary>
      /// �������� ���������, �������� � ����� � ��������� ������, � �������� ������ �� ���������
      /// </summary>
      public void Add()
      {
        base.Add(new DefaultSet(GetCurrentCfg()));
      }

      private TempCfg GetCurrentCfg()
      {
        TempCfg Cfg = new TempCfg();
        _Owner.ConfigFromControls(Cfg);
        return Cfg;
      }

      #endregion

      #region SetReadOnly

      internal new void SetReadOnly()
      {
        base.SetReadOnly();
      }

      #endregion
    }

    /// <summary>
    /// ������ ������� "�� ���������".
    /// ������ ����� ����������� ������ �� ������ ����� �� �����
    /// </summary>
    public DefaultSetList DefaultSets { get { return _DefaultSets; } }
    private DefaultSetList _DefaultSets;

    #endregion

    #region FormHandler

    /// <summary>
    /// ���������, ����������� � ���������������� ����, ��� ������ � ������� ����� �����.
    /// �������� � ������������. �� ����� ���� null
    /// </summary>
    public IEFPConfigParamSetHandler FormHandler { get { return _FormHandler; } }
    private IEFPConfigParamSetHandler _FormHandler;

    private void ConfigToControls(CfgPart cfg)
    {
      try
      {
        FormHandler.ConfigToControls(cfg);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "������ ������ �������� � ����������� ��������");
      }
    }

    private void ConfigFromControls(CfgPart cfg)
    {
      try
      {
        FormHandler.ConfigFromControls(cfg);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "������ ��������� �������� �� ����������� ���������");
      }
    }

    /// <summary>
    /// ���������� ��� ��������� ��������������� ������ ��� ����������� ������ "������� ������".
    /// ���� ���������� �� ����������, �������������� ����� �� ���������.
    /// �������� ����� ��������������� �� ������ ����� �� �����
    /// </summary>
    public IEFPConfigParamSetAuxTextHandler AuxTextHandler
    {
      get { return _AuxTextHandler; }
      set
      {
        CheckHasNotBeenCreated();
        _AuxTextHandler = value;
      }
    }
    private IEFPConfigParamSetAuxTextHandler _AuxTextHandler;

    private bool _UseAuxText; // ���������� � OnShown()

    #endregion

    #region ������ ��������

    /// <summary>
    /// ����� �� ��� �������� ����� ��������� ��������� ����������� ����� ��������.
    /// �� ��������� - false - ����������� �������� ��������� ��������, ������������� � ���������������� ����
    /// (��� ������ ��������).
    /// ���������� �������� � true, ���� ��� ����������� ��������� �� ������ ����������� ����� ���������� ��������
    /// ����� �������� ������.
    /// </summary>
    public bool AutoLoadLastConfig
    {
      get { return _AutoLoadLastConfig; }
      set
      {
        CheckHasNotBeenCreated();
        _AutoLoadLastConfig = value;
      }
    }
    private bool _AutoLoadLastConfig;

    #endregion

    #region ����������� �������� �-�����

    /// <summary>
    /// ����� ���������� ��� ������ ��������� �������� �� ������
    /// </summary>
    protected override void OnCreated()
    {
      if (String.IsNullOrEmpty(ConfigSectionName))
        throw new NullReferenceException("�������� \"ConfigSectionName\" ������ ���� �����������");

      base.OnCreated();

      _DefaultSets.SetReadOnly();

      _UseAuxText = AuxTextHandler != null && EFPApp.ShowParamSetAuxText;

      CreateSetsTables();

      FillSetItems();

      InitSubToolTips();

      if (AutoLoadLastConfig)
      {
        EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName,
           ParamsCategory, String.Empty);
        CfgPart cfg1;
        using (this.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Read, out cfg1))
        {
          ConfigToControls(cfg1);
        }
      }

      // �������� � ������ �������, ��������������� �������� ������
      TempCfg cfg2 = new TempCfg();
      ConfigFromControls(cfg2);
      Control.SelectedMD5Sum = cfg2.MD5Sum(); // �������� ���������� �����, ���� ����

      // ������������ ����������� ����� ������ ������� �������
      Control.ItemSelected += new ParamSetComboBoxItemEventHandler(SetComboBox_ItemSelected);
      Control.SaveClick += new ParamSetComboBoxSaveEventHandler(SetComboBox_SaveClick);
      Control.DeleteClick += new ParamSetComboBoxItemEventHandler(SetComboBox_DeleteClick);
      Control.CanDeleteItem += new ParamSetComboBoxItemCancelEventHandler(SetComboBox_CanDeleteItem);
    }

    private void InitSubToolTips()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("����� �������� ������ �������� �� ����������� ������.");
      sb.Append(Environment.NewLine);
      sb.Append("� ������ ������:");
      sb.Append(Environment.NewLine);
      sb.Append("- ���������������� ������, ������� �� ���������;");
      sb.Append(Environment.NewLine);
      if (DefaultSets.Count > 0)
      {
        sb.Append("- ��������� �� ���������;");
        sb.Append(Environment.NewLine);
      }
      sb.Append("- � ����� �� 9 ��������� ������� �������� (�������)");
      sb.Append(Environment.NewLine);
      sb.Append(Environment.NewLine);
      sb.Append("���� ��� ����� �������� ��� ������ ������");

      efpSelCB.ToolTipText = sb.ToString();
    }

    #endregion

    #region ������ � ������ ���������� � ������� �������

    #region ���������

    private const int GroupUser = 1;
    private const int GroupDefault = 2;
    private const int GroupHist = 3;

    #endregion

    /// <summary>
    /// ������ �� ������ SectHist, ����������� � ������������� ����������� �������, � ���� �������
    /// </summary>
    private DataTable _TableHist;

    /// <summary>
    /// ������ �� ������ SectHist, ����������� � ���������������� �������, � ���� �������
    /// </summary>
    private DataTable _TableUser;

    /// <summary>
    /// �������� � ���������� ������ TableHist � TableUser
    /// </summary>
    private void CreateSetsTables()
    {
      _TableHist = new DataTable();
      _TableHist.Columns.Add("Code", typeof(string));
      _TableHist.Columns.Add("Time", typeof(DateTime));
      _TableHist.Columns.Add("MD5", typeof(string));
      _TableHist.Columns.Add("Order", typeof(int));
      DataTools.SetPrimaryKey(_TableHist, "Code");

      _TableUser = new DataTable();
      _TableUser.Columns.Add("Code", typeof(string));
      _TableUser.Columns.Add("Name", typeof(string));
      _TableUser.Columns.Add("Time", typeof(DateTime));
      _TableUser.Columns.Add("MD5", typeof(string));
      DataTools.SetPrimaryKey(_TableUser, "Code");

      EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName, HistoryCategory, String.Empty);
      CfgPart cfgHist;
      using (this.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Read, out cfgHist))
      {
        try
        {
          string[] Names = cfgHist.GetChildNames();
          for (int i = 0; i < Names.Length; i++)
          {
            if (Names[i].StartsWith("Hist"))
            {
              CfgPart cfgOne = cfgHist.GetChild(Names[i], false);
              _TableHist.Rows.Add(Names[i], cfgOne.GetNullableDateTime("Time"),
                cfgOne.GetString("MD5"), _TableHist.Rows.Count + 1);
            }
            if (Names[i].StartsWith("User"))
            {
              CfgPart cfgOne = cfgHist.GetChild(Names[i], false);
              _TableUser.Rows.Add(Names[i], cfgOne.GetString("Name"), cfgOne.GetNullableDateTime("Time"),
                cfgOne.GetString("MD5"));
            }
          }
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "������ ������ ������ �������");
        }
      }
    }

    /// <summary>
    /// ���������� ���������� "������"
    /// (���������� � ������ OnShown())
    /// </summary>
    private void FillSetItems()
    {
      if (_UseAuxText)
      {
        #region ��������� ����������� ������ ������ ������������

        List<EFPConfigSectionInfo> PreloadInfos = new List<EFPConfigSectionInfo>();
        foreach (DataRow Row in _TableUser.Rows)
          PreloadInfos.Add(new EFPConfigSectionInfo(ConfigSectionName,
          ParamsCategory, DataTools.GetString(Row, "Code")));
        foreach (DataRow Row in _TableHist.Rows)
          PreloadInfos.Add(new EFPConfigSectionInfo(ConfigSectionName,
          ParamsCategory, DataTools.GetString(Row, "Code")));

        this.ConfigManager.Preload(PreloadInfos.ToArray(), EFPConfigMode.Read);

        #endregion
      }

      if (_UseAuxText)
        AuxTextHandler.BeginGetAuxText();
      try
      {
        string AuxText = null;

        #region ������� - ������� ������ ������������

        _TableUser.DefaultView.Sort = "Name";
        foreach (DataRowView drv in _TableUser.DefaultView)
        {
          string Code = DataTools.GetString(drv.Row, "Code");
          //DateTime? dt = DataTools.GetNullableDateTime(drv.Row, "Time");

          if (_UseAuxText)
          {
            EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName, ParamsCategory, Code);
            CfgPart cfgData;
            using (this.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Read, out cfgData))
            {
              AuxText = AuxTextHandler.GetAuxText(cfgData);
            }
          }

          Control.Items.Add(new ParamSetComboBoxItem(Code,
          DataTools.GetString(drv.Row, "Name"),
          "User",
          null,
          GroupUser,
          DataTools.GetString(drv.Row, "MD5"), AuxText));
        }

        #endregion

        #region ����� - �� ���������

        for (int i = 0; i < DefaultSets.Count; i++)
        {
          if (_UseAuxText)
            AuxText = AuxTextHandler.GetAuxText(DefaultSets[i].Config);

          Control.Items.Add(new ParamSetComboBoxItem(i.ToString(), "(" + DefaultSets[i].DisplayName + ")", DefaultSets[i].ImageKey, null, GroupDefault, DefaultSets[i].MD5Sum, AuxText));
        }

        #endregion

        #region ��������� - ������ �������

        _TableHist.DefaultView.Sort = "Order";
        int cnt = 0;
        for (int i = _TableHist.DefaultView.Count - 1; i >= 0; i--)
        {
          DataRow Row = _TableHist.DefaultView[i].Row;
          string Code = DataTools.GetString(Row, "Code");
          DateTime? dt = DataTools.GetNullableDateTime(Row, "Time");
          cnt++;
          string Name;
          switch (cnt)
          {
            case 1:
              Name = "(���������)";
              break;
            case 2:
              Name = "(�������������)";
              break;
            default:
              Name = "(���������� �" + cnt.ToString() + ")";
              break;
          }


          if (_UseAuxText)
          {
            EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName, ParamsCategory, Code);
            CfgPart cfgData;
            using (this.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Read, out cfgData))
            {
              AuxText = AuxTextHandler.GetAuxText(cfgData);
            }
          }

          Control.Items.Add(new ParamSetComboBoxItem(Code, Name, "Time", dt, GroupHist,
          DataTools.GetString(Row, "MD5"), AuxText));
        }

        #endregion
      }
      finally
      {
        if (_UseAuxText)
          AuxTextHandler.EndGetAuxText();
      }
    }


    private void SaveSetsTables()
    {
      EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName,
        HistoryCategory, String.Empty);
      CfgPart cfgHist;
      using (this.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Write, out cfgHist))
      {
        cfgHist.Clear();
        foreach (DataRowView drv in _TableHist.DefaultView)
        {
          CfgPart cfgOne = cfgHist.GetChild(DataTools.GetString(drv.Row, "Code"), true);
          cfgOne.SetNullableDateTime("Time", DataTools.GetNullableDateTime(drv.Row, "Time"));
          cfgOne.SetString("MD5", DataTools.GetString(drv.Row, "MD5"));
        }
        foreach (DataRowView drv in _TableUser.DefaultView)
        {
          CfgPart cfgOne = cfgHist.GetChild(DataTools.GetString(drv.Row, "Code"), true);
          cfgOne.SetString("Name", DataTools.GetString(drv.Row, "Name"));
          cfgOne.SetNullableDateTime("Time", DataTools.GetNullableDateTime(drv.Row, "Time"));
          cfgOne.SetString("MD5", DataTools.GetString(drv.Row, "MD5"));
        }
      }
    }

    #endregion

    #region ����������� ��� ������ ������ � �������� ��������

    void SetComboBox_ItemSelected(object sender, ParamSetComboBoxItemEventArgs args)
    {
      if (args.Item.Group == GroupDefault)
      {
        int DefIndex = int.Parse(args.Item.Code);
        // ������ ����� �� ���������
        ConfigToControls(DefaultSets[DefIndex].Config);
      }
      else
      {
        string UserSetName = args.Item.Code;
        EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName,
          ParamsCategory, UserSetName);
        CfgPart cfgData;
        using (this.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Read, out cfgData))
        {
          ConfigToControls(cfgData);
        }
      }
    }

    void SetComboBox_SaveClick(object sender, ParamSetComboBoxSaveEventArgs args)
    {
      if (String.IsNullOrEmpty(ConfigSectionName))
      {
        EFPApp.ErrorMessageBox("�� ������������� ���������� �������� ����� �������� ������");
        return;
      }

      if (!EFPConfigTools.IsPersist(this.ConfigManager.Persistence))
      {
        EFPApp.ErrorMessageBox("C��������� �������� ����� �������� ������ �� ������������� � ���������");
        return;
      }

      if (!BaseProvider.FormProvider.ValidateForm())
        return;

      ParamSetComboBoxItem OldItem = Control.Items.FindDisplayName(args.DisplayName);
      if (OldItem != null)
      {
        if (!OldItem.Code.StartsWith("User"))
        {
          EFPApp.ShowTempMessage("�������������� ����� ������ ���������������� ������");
          return;
        }
        if (EFPApp.MessageBox("����� \"" + args.DisplayName + "\" ��� ����������. �� ������ ������������ ���?",
          "������������� ���������� ������",
          MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
          return;
      }

      if (args.DisplayName.StartsWith("("))
      {
        EFPApp.ShowTempMessage("��� ������ �� ����� ���������� �� ������");
        return;
      }

      string UserSetName;
      if (OldItem != null)
      {
        UserSetName = OldItem.Code;
        Control.Items.Remove(OldItem);
      }
      else
      {
        int cnt = 1;
        while (true)
        {
          UserSetName = "User" + cnt.ToString();
          if (_TableUser.Rows.Find(UserSetName) == null)
            break;
          cnt++;
        }
      }

      EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName,
        ParamsCategory, UserSetName);
      CfgPart cfgData;
      using (this.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Write, out cfgData))
      {
        cfgData.Clear();
        ConfigFromControls(cfgData);

        string AuxText = null;
        if (_UseAuxText)
        {
          AuxTextHandler.BeginGetAuxText();
          try
          {
            AuxText = AuxTextHandler.GetAuxText(cfgData);
          }
          finally
          {
            AuxTextHandler.EndGetAuxText();
          }
        }

        ParamSetComboBoxItem NewItem = new ParamSetComboBoxItem(UserSetName, args.DisplayName, "User", null, GroupUser, cfgData.MD5Sum(), AuxText);
        Control.Items.Insert(0, NewItem);
        Control.SelectedItem = NewItem;
        DataRow Row = DataTools.FindOrAddPrimaryKeyRow(_TableUser, UserSetName);
        Row["Name"] = args.DisplayName;
        Row["Time"] = DateTime.Now;
        Row["MD5"] = NewItem.MD5Sum;
        SaveSetsTables();
      }
    }

    void SetComboBox_DeleteClick(object sender, ParamSetComboBoxItemEventArgs args)
    {
      DataTable Table;
      if (args.Item.Code.StartsWith("User"))
        Table = _TableUser;
      else if (args.Item.Code.StartsWith("Hist"))
        Table = _TableHist;
      else
      {
        EFPApp.ErrorMessageBox("���� ����� ������ �������", "�������� �������� ������");
        return;
      }

      DataRow Row = Table.Rows.Find(args.Item.Code);
      if (Row == null)
      {
        BugException Ex = new BugException("����� � ����� \"" + args.Item.Code + "\" �� ������");
        Ex.Data["Item"] = args.Item;
        throw Ex;
      }

      if (EFPApp.MessageBox("������� ����� \"" + args.Item.DisplayName + "\"?",
        "������������� �������� ������", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
        return;

      Table.Rows.Remove(Row);
      SaveSetsTables();

      Control.Items.Remove(args.Item);
    }

    void SetComboBox_CanDeleteItem(object sender, ParamSetComboBoxItemCancelEventArgs args)
    {
      if (args.Item.Code.StartsWith("Hist") || args.Item.Code.StartsWith("User"))
        args.Cancel = false;
      else
        args.Cancel = true;
    }

    #endregion

    #region ���������� ����� �������� �������

    /// <summary>
    /// ��� �������� ����� �������� "��" ��������� ������� �������� �� ����� � �������� �� � ������ ������������
    /// � �������� �������.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();

      if (BaseProvider.FormProvider.ValidateReason == EFPFormValidateReason.Closing)
      {
        EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName,
          ParamsCategory, String.Empty);
        CfgPart cfg;
        using (this.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Write, out cfg))
        {
          ConfigFromControls(cfg);
          AfterSave(cfg.MD5Sum());
        }
      }
    }

    private void AfterSave(string md5Sum)
    {
      if (_TableHist == null)
        return; // 10.03.2016

      bool Found = false;
      foreach (DataRowView drv in _TableHist.DefaultView)
      {
        if (DataTools.GetString(drv.Row, "MD5") == md5Sum)
        {
          drv.Row["Time"] = DateTime.Now;
          drv.Row["Order"] = DataTools.GetInt(_TableHist.DefaultView[_TableHist.DefaultView.Count - 1].Row, "Order") + 1;
          Found = true;
          break;
        }
      }

      if (!Found)
      {
        // ����� ������ ���������� � ������ ������
        DataRow ResRow = null;
        if (_TableHist.DefaultView.Count >= 9) // ��� ������� ������
          ResRow = _TableHist.DefaultView[0].Row;
        else
        {
          for (int i = 1; i <= 9; i++)
          {
            if (DataTools.FindOrAddPrimaryKeyRow(_TableHist, "Hist" + i.ToString(), out ResRow))
              break;
          }
        }
        string UserSetName = DataTools.GetString(ResRow, "Code");
        ResRow["Time"] = DateTime.Now;
        ResRow["MD5"] = md5Sum;
        if (_TableHist.Rows.Count > 0)
          ResRow["Order"] = DataTools.GetInt(_TableHist.DefaultView[_TableHist.DefaultView.Count - 1].Row, "Order") + 1;
        else
          ResRow["Order"] = 1;


        EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName,
          ParamsCategory, UserSetName);
        CfgPart cfgData;
        using (this.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Write, out cfgData))
        {
          cfgData.Clear();
          ConfigFromControls(cfgData);
        }

        SaveSetsTables();
      }
    }

    #endregion
  }
}