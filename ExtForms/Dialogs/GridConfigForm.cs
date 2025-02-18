// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Форма настройки табличного просмотра с помощью интерфейса IEFPGridProducerEditor
  /// </summary>
  internal partial class GridConfigForm : Form, IEFPConfigParamSetHandler
  {
    #region Конструктор формы

    public GridConfigForm(IEFPControl callerControlProvider, string configCategory, string historyCategory)
    {
      InitializeComponent();
      Icon = EFPApp.MainImages.Icons["EditGridConfig"];

#if DEBUG
      if (callerControlProvider == null)
        throw new ArgumentNullException("callerControlProvider");
      if (String.IsNullOrEmpty(configCategory))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("configCategory");
      if (String.IsNullOrEmpty(historyCategory))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("historyCategory");
#endif

      _CallerControlProvider = callerControlProvider;
      _ConfigCategory = configCategory;
      _HistoryCategory = historyCategory;

      _FormProvider = new EFPFormProvider(this);
      _FormProvider.ConfigSectionName = "GridConfigForm";
      _FormProvider.FormChecks.Add(new UIValidatingEventHandler(FormCheck));

      #region Готовые наборы

      _UseHistory = EFPConfigTools.IsPersist(_CallerControlProvider.ConfigManager.Persistence);

      if (_UseHistory)
      {
        efpParamSet = new EFPConfigParamSetComboBox(_FormProvider, SetComboBox, this);
        efpParamSet.ConfigSectionName = callerControlProvider.ConfigHandler.ConfigSectionName;
        efpParamSet.ParamsCategory = _ConfigCategory;
        efpParamSet.HistoryCategory = _HistoryCategory;
      }
      else
      {
        grpSets.Visible = false;
        btnDefault.Image = EFPApp.MainImages.Images["No"];
        btnDefault.ImageAlign = ContentAlignment.MiddleLeft;
        panDefault.Visible = true;
        EFPButton efpDefault = new EFPButton(_FormProvider, btnDefault);
        efpDefault.ToolTipText = Res.GridConfig_ToolTip_Default;
        efpDefault.Click += new EventHandler(efpDefault_Click);
      }

      #endregion

      #region Буфер обмена

      btnCopy.Image = EFPApp.MainImages.Images["Copy"];
      btnCopy.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpCopy = new EFPButton(_FormProvider, btnCopy);
      efpCopy.DisplayName = Res.Btn_Name_Copy;
      efpCopy.ToolTipText = Res.GridConfig_ToolTip_Copy;
      efpCopy.Click += new EventHandler(efpCopy_Click);

      btnPaste.Image = EFPApp.MainImages.Images["Paste"];
      btnPaste.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpPaste = new EFPButton(_FormProvider, btnPaste);
      efpPaste.DisplayName = Res.Btn_Name_Paste;
      efpPaste.ToolTipText = Res.GridConfig_ToolTip_Paste;
      efpPaste.Click += new EventHandler(efpPaste_Click);

      #endregion
    }

    #endregion

    #region Свойства

    public EFPFormProvider FormProvider { get { return _FormProvider; } }
    private readonly EFPFormProvider _FormProvider;

    /// <summary>
    /// Выполняет чтение и запись значений в форму
    /// </summary>
    public IEFPGridProducerEditor Editor
    {
      get { return _Editor; }
      set
      {
        _Editor = value;
        efpParamSet.AuxTextHandler = value as IEFPConfigParamSetAuxTextHandler;
      }
    }
    private IEFPGridProducerEditor _Editor;


    /// <summary>
    /// Используются ли готовые наборы
    /// </summary>
    bool _UseHistory;

    /// <summary>
    /// Вызывающий табличный просмотр
    /// </summary>
    private IEFPControl _CallerControlProvider;

    /// <summary>
    /// Категория "GridConfig"
    /// </summary>
    private string _ConfigCategory;

    /// <summary>
    /// Категория "GridConfigHistory"
    /// </summary>
    private string _HistoryCategory;

    /// <summary>
    /// Готовые наборы
    /// </summary>
    private EFPConfigParamSetComboBox efpParamSet;

    #endregion

    #region Чтение и запись информации о готовых наборах

    #region Константы

    /// <summary>
    /// Именной пользовательский набор
    /// </summary>
    private const int GroupUser = 1;

    /// <summary>
    /// Набор по умолчанию (GridProducer.DefaultConfig) или 
    /// именная стандартная настройка (GridProducer.Configs[Code])
    /// </summary>
    private const int GroupDefault = 2;

    /// <summary>
    /// История
    /// </summary>
    private const int GroupHist = 3;

    #endregion

    /// <summary>
    /// Заполнение комбоблока "Наборы"
    /// (вызывается после вызова конструктора)
    /// </summary>
    public void FillSetItems()
    {
      if (!_UseHistory)
        return; // 25.11.2015

      string[] defaultConfigCodes;
      EFPDataGridViewConfig[] defaultConfigs;
      Editor.GetDefaultConfigs(out defaultConfigCodes, out defaultConfigs);
      for (int i = 0; i < defaultConfigCodes.Length; i++)
      {
        string displayName;
        string imageKey;
        if (String.IsNullOrEmpty(defaultConfigCodes[i]))
          displayName = Res.GridConfig_Msg_Default;
        else
          displayName = defaultConfigCodes[i];

        imageKey = defaultConfigs[i].ImageKey; // 23.11.2016
        if (String.IsNullOrEmpty(imageKey))
          imageKey = "No";

        TempCfg cfgDefault = new TempCfg();
        defaultConfigs[i].WriteConfig(cfgDefault);
        //cfgDefault = efpParamSet.UpdateDefaultConfig(cfgDefault);

        EFPConfigParamDefaultSet defSet = new EFPConfigParamDefaultSet(cfgDefault, displayName, imageKey);
        efpParamSet.DefaultSets.Add(defSet);
      }
    }

    #endregion

    #region Кнопка "По умолчанию"

    void efpDefault_Click(object sender, EventArgs args)
    {
      string[] defaultConfigCodes;
      EFPDataGridViewConfig[] defaultConfigs;
      Editor.GetDefaultConfigs(out defaultConfigCodes, out defaultConfigs);
      switch (defaultConfigCodes.Length)
      {
        case 0:
          EFPApp.ShowTempMessage(Res.GridConfig_Err_NoDefaults);
          break;
        case 1:
          Editor.WriteFormValues(defaultConfigs[0]);
          break;
        default:
          // Выбираем, какую настройку применить
          ListSelectDialog dlg = new ListSelectDialog();
          dlg.Title = Res.GridConfig_Title_SelDefaults;
          dlg.ImageKey = "No";
          dlg.Items = new string[defaultConfigCodes.Length];
          for (int i = 0; i < defaultConfigCodes.Length; i++)
          {
            //string ImageKey;
            if (String.IsNullOrEmpty(defaultConfigCodes[i]))
              dlg.Items[i] = "По умолчанию";
            else
              dlg.Items[i] = defaultConfigCodes[i];

            dlg.ImageKeys[i] = defaultConfigs[i].ImageKey;
          }
          dlg.CanBeEmpty = false;
          dlg.ConfigSectionName = "GridConfigSelectDefaultDialog";

          if (dlg.ShowDialog() != DialogResult.OK)
            return;

          Editor.WriteFormValues(defaultConfigs[dlg.SelectedIndex]);
          break;
      }
    }

    #endregion

    #region Буфер обмена

    private void efpCopy_Click(object sender, EventArgs args)
    {
      EFPDataGridViewConfig config = new EFPDataGridViewConfig();
      string errorText;
      if (!Editor.ReadFormValues(config, out errorText))
      {
        EFPApp.ErrorMessageBox(errorText);
        return;
      }

      DataObject dobj = new DataObject();
      dobj.SetData(config);
      Clipboard.SetDataObject(dobj);
    }

    private void efpPaste_Click(object sender, EventArgs args)
    {
      IDataObject dobj = Clipboard.GetDataObject();
      if (dobj == null)
      {
        EFPApp.ShowTempMessage(Res.Clipboard_Err_Empty);
        return;
      }

      EFPDataGridViewConfig config = dobj.GetData(typeof(EFPDataGridViewConfig)) as EFPDataGridViewConfig;
      if (config == null)
      {
        // string txtFormats = String.Join(", ", dobj.GetFormats());
        EFPApp.ShowTempMessage(Res.GridConfig_Err_ClipboardNoData);
        return;
      }
      Editor.WriteFormValues(config);
    }

    #endregion

    #region IEFPConfigParamSetHandler Members

    public void ConfigToControls(CfgPart cfgData)
    {
      EFPDataGridViewConfig config = new EFPDataGridViewConfig();
      config.ReadConfig(cfgData);
      Editor.WriteFormValues(config);
    }

    public void ConfigFromControls(CfgPart cfgData)
    {
      EFPDataGridViewConfig config = new EFPDataGridViewConfig();
      string errorText;
      Editor.ReadFormValues(config, out errorText);
      config.WriteConfig(cfgData);
    }

    #endregion

    #region Вызывается после закрытия диалога

    /// <summary>
    /// Конфигурация, созданная при нажатии кнопки "ОК".
    /// </summary>
    public EFPDataGridViewConfig ResultConfig { get { return _ResultConfig; } }
    private EFPDataGridViewConfig _ResultConfig;

    private void FormCheck(object sender, UIValidatingEventArgs args)
    {
      if (_FormProvider.ValidateReason == EFPFormValidateReason.Shown)
        return;

      _ResultConfig = new EFPDataGridViewConfig();
      string errorText;
      if (!Editor.ReadFormValues(_ResultConfig, out errorText))
      {
        args.SetError(errorText);
        return;
      }
    }

    #endregion
  }
}
