// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using System.Data;
using System.Windows.Forms;
using FreeLibSet.Controls;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Интерфейс, реализующий чтение и запись значений полей формы в данные конфигурации.
  /// Используется провайдером <see cref="EFPConfigParamSetComboBox"/>.
  /// Интерфейс, как правило, реализуется непосредственно формой, в которой расположен элемент <see cref="ParamSetComboBox"/>.
  /// </summary>
  public interface IEFPConfigParamSetHandler
  {
    /// <summary>
    /// Этот метод должен установить состояние управляющих элементов формы в соответствии со значениями в секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    void ConfigToControls(CfgPart cfg);

    /// <summary>
    /// Этот метод должен записать состояние управляющих элементов формы в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    void ConfigFromControls(CfgPart cfg);
  }

  /// <summary>
  /// Интерфейс, реализующий получение поясняющего текста AuxText для выпадающего списка "Готовые наборы".
  /// Используется провайдером <see cref="EFPConfigParamSetComboBox"/>.
  /// Интерфейс, как правило, реализуется непосредственно формой, в которой расположен элемент <see cref="ParamSetComboBox"/>.
  /// </summary>
  public interface IEFPConfigParamSetAuxTextHandler
  {
    /// <summary>
    /// Этот метод вызывается однократно перед последовательностью вызовов <see cref="GetAuxText(CfgPart)"/>
    /// </summary>
    void BeginGetAuxText();

    /// <summary>
    /// Этот метод должен вернуть строку AuxText для секции конфигурации с набором параметров
    /// </summary>
    /// <param name="cfg">Секция конфигурации, доступная для чтения, содержащая значения для одного набора</param>
    string GetAuxText(CfgPart cfg);

    /// <summary>
    /// Этот метод вызывается однократно после последовательности вызовов <see cref="GetAuxText(CfgPart)"/>
    /// </summary>
    void EndGetAuxText();
  }


  /// <summary>
  /// Описание одной строчки в списке для значения "По умолчанию"
  /// </summary>
  public sealed class EFPConfigParamDefaultSet
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="config">Набор параметров для значения "по умолчанию"</param>
    /// <param name="displayName">Отображаемое имя (без скобок)</param>
    /// <param name="imageKey">Имя значка в списке <see cref="EFPApp.MainImages"/></param>
    public EFPConfigParamDefaultSet(TempCfg config, string displayName, string imageKey)
    {
      if (config == null)
        throw new ArgumentNullException("config");
      if (String.IsNullOrEmpty(displayName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("displayName");
      if (String.IsNullOrEmpty(imageKey))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("imageKey");

      _Config = config;
      _DisplayName = displayName;
      _ImageKey = imageKey;

      // _MD5Sum = config.MD5Sum();
    }

    /// <summary>
    /// Создает объект со значком по умолчанию "No"
    /// </summary>
    /// <param name="config">Набор параметров для значения "по умолчанию"</param>
    /// <param name="displayName">Отображаемое имя (без скобок)</param>
    public EFPConfigParamDefaultSet(TempCfg config, string displayName)
      : this(config, displayName, "No")
    {
    }

    /// <summary>
    /// Создает объект с текстом "По умолчанию" со значком по умолчанию "No"
    /// </summary>
    /// <param name="config">Набор параметров для значения "по умолчанию"</param>
    public EFPConfigParamDefaultSet(TempCfg config)
      : this(config, "По умолчанию", "No")
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Набор параметров для значения "по умолчанию"
    /// </summary>
    public TempCfg Config { get { return _Config; } }
    private readonly TempCfg _Config;

    /// <summary>
    /// Отображаемое имя (без скобок)
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private readonly string _DisplayName;

    /// <summary>
    /// Значок в списке EFPApp.MainImages
    /// </summary>
    public string ImageKey { get { return _ImageKey; } }
    private readonly string _ImageKey;

    ///// <summary>
    ///// Контрольная сумма для секции конфигурации
    ///// </summary>
    //public string MD5Sum { get { return _MD5Sum; } }
    //private readonly string _MD5Sum;

    /// <summary>
    /// Возвращает свойство DisplayName
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName;
    }

    #endregion
  }

  /// <summary>
  /// Провайдер управляющего элемента "Набор параметров" (<see cref="ParamSetComboBox"/>), состоящего из
  /// комбоблока, кнопок "+" и "-".
  /// Предназначен для хранения "истории" введенных значений в форме, а
  /// также пользовательских "именных" наборов.
  /// Для хранения используется система, реализующая <see cref="IEFPConfigManager"/>.
  /// Вызывающий код должен предоставить интерфейс, выполняющий чтение и запись значений в поля формы.
  /// Должно быть обязательно установлено свойство <see cref="EFPControlBase.ConfigSectionName"/> 
  /// (и, при необходимости, <see cref="ParamsCategory"/> и <see cref="HistoryCategory"/>). 
  /// Используйте список <see cref="DefaultSets"/> для задания набора(ов) по умолчанию.
  /// </summary>
  public class EFPConfigParamSetComboBox : EFPControl<ParamSetComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="formHandler">Интерфейс для чтения и записи значений. Ссылка должна быть задана</param>
    public EFPConfigParamSetComboBox(EFPBaseProvider baseProvider, ParamSetComboBox control, IEFPConfigParamSetHandler formHandler)
      : base(baseProvider, control, true)
    {
      if (formHandler == null)
        throw new ArgumentNullException("formHandler");
      _FormHandler = formHandler;

      efpSelCB = new EFPTextComboBox(baseProvider, control.TheCB);
      efpSelCB.DisplayName = "Готовые наборы";
      efpSelCB.CanBeEmpty = true;
      efpSelCB.ToolTipText = Res.ParamSetComboBox_ToolTip_ComboBox;

      if (DebugCommands)
      {
        EFPCommandItem ci;
        ci = new EFPCommandItem("Debug", "CurrentSettingsXML");
        ci.MenuText = Res.Cmd_Menu_Debug_CurrentSettingsXML;
        ci.Click += DebugCurrentSettings_Click;
        ci.GroupBegin = true;
        efpSelCB.CommandItems.Add(ci);

        ci = new EFPCommandItem("Debug", "SavedSettingsXML");
        ci.MenuText = Res.Cmd_Menu_Debug_SavedSettingsXML;
        ci.Click += DebugSavedSettings_Click;
        ci.GroupEnd = true;
        efpSelCB.CommandItems.Add(ci);

        control.ShowMD5 = true;
      }

      efpSaveButton = new EFPButton(baseProvider, control.SaveButton);
      efpSaveButton.DisplayName = Res.ParamSetComboBox_Name_SaveButton;
      efpSaveButton.ToolTipText = Res.ParamSetComboBox_ToolTip_SaveButton;

      efpDelButton = new EFPButton(baseProvider, control.DeleteButton);
      efpDelButton.DisplayName = Res.ParamSetComboBox_Name_DelButton;
      efpDelButton.ToolTipText = Res.ParamSetComboBox_ToolTip_DelButton;

      _ParamsCategory = EFPConfigCategories.UserParams;
      _HistoryCategory = EFPConfigCategories.UserHistory;

      _DefaultSets = new DefaultSetCollection(this);

      control.ShowImages = EFPApp.ShowListImages;

      InitConfigHandler(); // иначе потеряется свойство ConfigSectionName
    }

    #endregion

    #region Поля

    private readonly EFPTextComboBox efpSelCB;

    private readonly EFPButton efpSaveButton;

    private readonly EFPButton efpDelButton;

    #endregion

    #region Используемые секции конфигурации

    /// <summary>
    /// Имя категории секций конфигурации, используемых для хранения текущих значений.
    /// По умолчанию равно <see cref="EFPConfigCategories.UserParams"/>.
    /// Свойство должно быть установлено до вывода формы на экран.
    /// </summary>
    public string ParamsCategory
    {
      get { return _ParamsCategory; }
      set
      {
        base.CheckHasNotBeenCreated();
        if (String.IsNullOrEmpty(value))
          throw ExceptionFactory.ArgStringIsNullOrEmpty("value");
        _ParamsCategory = value;
      }
    }
    private string _ParamsCategory;


    /// <summary>
    /// Имя категории секций конфигурации, используемых для хранения истории.
    /// По умолчанию равно <see cref="EFPConfigCategories.UserHistory"/>
    /// Свойство должно быть установлено до вывода формы на экран.
    /// </summary>
    public string HistoryCategory
    {
      get { return _HistoryCategory; }
      set
      {
        base.CheckHasNotBeenCreated();
        if (String.IsNullOrEmpty(value))
          throw ExceptionFactory.ArgStringIsNullOrEmpty("value");
        _HistoryCategory = value;
      }
    }
    private string _HistoryCategory;

    #endregion

    #region Стандартные наборы

    /// <summary>
    /// Реализация свойства <see cref="DefaultSets"/>
    /// </summary>
    public sealed class DefaultSetCollection : ListWithReadOnly<EFPConfigParamDefaultSet>
    {
      #region Защищенный конструктор

      internal DefaultSetCollection(EFPConfigParamSetComboBox owner)
      {
        _Owner = owner;
      }

      private EFPConfigParamSetComboBox _Owner;

      #endregion

      #region Методы добавления

      /// <summary>
      /// Добавить установки, заданные в форме в настоящий момент, в качестве набора по умолчанию
      /// </summary>
      /// <param name="displayName"></param>
      /// <param name="imageKey"></param>
      public void Add(string displayName, string imageKey)
      {
        base.Add(new EFPConfigParamDefaultSet(GetCurrentCfg(), displayName, imageKey));
      }

      /// <summary>
      /// Добавить установки, заданные в форме в настоящий момент, в качестве набора по умолчанию
      /// </summary>
      /// <param name="displayName"></param>
      public void Add(string displayName)
      {
        base.Add(new EFPConfigParamDefaultSet(GetCurrentCfg(), displayName));
      }

      /// <summary>
      /// Добавить установки, заданные в форме в настоящий момент, в качестве набора по умолчанию
      /// </summary>
      public void Add()
      {
        base.Add(new EFPConfigParamDefaultSet(GetCurrentCfg()));
      }

      private TempCfg GetCurrentCfg()
      {
        TempCfg cfg = new TempCfg();
        _Owner.ConfigFromControls(cfg);
        return cfg;
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
    /// Список наборов "по умолчанию".
    /// Список может заполняться только до вывода формы на экран
    /// </summary>
    public DefaultSetCollection DefaultSets { get { return _DefaultSets; } }
    private readonly DefaultSetCollection _DefaultSets;

    #endregion

    #region FormHandler

    /// <summary>
    /// Интерфейс, реализуемый в пользовательском коде, для чтения и записей полей формы.
    /// Задается в конструкторе. Не может быть null.
    /// </summary>
    public IEFPConfigParamSetHandler FormHandler { get { return _FormHandler; } }
    private readonly IEFPConfigParamSetHandler _FormHandler;

    private void ConfigToControls(CfgPart cfg)
    {
      try
      {
        FormHandler.ConfigToControls(cfg);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, Res.ParamSetComboBox_ErrTitle_ConfigToControls);
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
        EFPApp.ShowException(e, Res.ParamSetComboBox_ErrTitle_ConfigFromControls);
      }
    }

    /// <summary>
    /// Обработчик для получения дополнительного текста для выпадающего списка "Готовые наборы".
    /// Если обработчик не установлен, дополнительный текст не выводится.
    /// Свойство может устанавливаться до вывода формы на экран
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

    private bool _UseAuxText; // определяем в OnShown()

    #endregion

    #region Прочие свойства

    /// <summary>
    /// Нужно ли при открытии формы загрузить последний сохраненный набор настроек.
    /// По умолчанию - false - управляющие элементы сохраняют значения, установленные в пользовательском коде
    /// (или пустые значения).
    /// Установите свойство в true, если для управляющих элементов не создан собственный режим сохранения значений
    /// между сеансами работы.
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

    #region Обработчики элемента в-целом

    /// <summary>
    /// Метод вызывается при первом появлении элемента на экране
    /// </summary>
    protected override void OnCreated()
    {
      if (String.IsNullOrEmpty(ConfigSectionName))
        throw ExceptionFactory.ObjectPropertyNotSet(this, "ConfigSectionName");

      base.OnCreated();

      _DefaultSets.SetReadOnly();

      _UseAuxText = AuxTextHandler != null && EFPApp.ShowParamSetAuxText;

      CreateSetsTables();

      FillSetItems();


      if (AutoLoadLastConfig)
      {
        EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName,
           ParamsCategory, String.Empty);
        CfgPart cfg1;
        using (this.ConfigManager.GetConfig(configInfo, EFPConfigMode.Read, out cfg1))
        {
          ConfigToControls(cfg1);
        }
      }

      // Выделяем в списке позицию, соответствующую текущему выбору
      TempCfg cfg2 = new TempCfg();
      ConfigFromControls(cfg2);
      string md5 = cfg2.MD5Sum();
      Control.SelectedMD5Sum = md5; // выбираем подходящий набор, если есть

      // Присоединяем обработчики после выбора текущей позиции
      Control.ItemSelected += new ParamSetComboBoxItemEventHandler(SetComboBox_ItemSelected);
      Control.SaveClick += new ParamSetComboBoxSaveEventHandler(SetComboBox_SaveClick);
      Control.DeleteClick += new ParamSetComboBoxItemEventHandler(SetComboBox_DeleteClick);
      Control.CanDeleteItem += new ParamSetComboBoxItemCancelEventHandler(SetComboBox_CanDeleteItem);
    }


    #endregion

    #region Чтение и запись информации о готовых наборах

    #region Константы

    private const int GroupUser = 1;
    private const int GroupDefault = 2;
    private const int GroupHist = 3;

    #endregion

    /// <summary>
    /// Данные из секции SectHist, относящиеся к автоматически создаваемым записям, в виде таблицы
    /// </summary>
    private DataTable _TableHist;

    /// <summary>
    /// Данные из секции SectHist, относящиеся к пользовательским записям, в виде таблицы
    /// </summary>
    private DataTable _TableUser;

    /// <summary>
    /// Создание и заполнение таблиц TableHist и TableUser
    /// </summary>
    private void CreateSetsTables()
    {
      _TableHist = new DataTable();
      _TableHist.Columns.Add("Code", typeof(string));
      _TableHist.Columns.Add("Time", typeof(DateTime));
      _TableHist.Columns.Add("MD5", typeof(string));
      DataTools.SetPrimaryKey(_TableHist, "Code");

      _TableUser = new DataTable();
      _TableUser.Columns.Add("Code", typeof(string));
      _TableUser.Columns.Add("Name", typeof(string));
      _TableUser.Columns.Add("Time", typeof(DateTime));
      _TableUser.Columns.Add("MD5", typeof(string));
      DataTools.SetPrimaryKey(_TableUser, "Code");

      EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName, HistoryCategory, String.Empty);
      CfgPart cfgHist;
      using (this.ConfigManager.GetConfig(configInfo, EFPConfigMode.Read, out cfgHist))
      {
        try
        {
          string[] names = cfgHist.GetChildNames();
          for (int i = 0; i < names.Length; i++)
          {
            if (names[i].StartsWith("Hist", StringComparison.Ordinal))
            {
              CfgPart cfgOne = cfgHist.GetChild(names[i], false);
              _TableHist.Rows.Add(names[i], cfgOne.GetNullableDateTime("Time"),
                cfgOne.GetString("MD5")/*, _TableHist.Rows.Count + 1*/);
            }
            else if (names[i].StartsWith("User", StringComparison.Ordinal))
            {
              CfgPart cfgOne = cfgHist.GetChild(names[i], false);
              _TableUser.Rows.Add(names[i], cfgOne.GetString("Name"), cfgOne.GetNullableDateTime("Time"),
                cfgOne.GetString("MD5"));
            }
          }
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, Res.ParamSetComboBox_ErrTitle_ReadHistory);
        }
      }
    }

    /// <summary>
    /// Заполнение комбоблока "Наборы"
    /// (вызывается в методе OnShown())
    /// </summary>
    private void FillSetItems()
    {
      if (_UseAuxText)
      {
        #region Выполняем упреждающее чтение секций конфигурации

        List<EFPConfigSectionInfo> preloadInfos = new List<EFPConfigSectionInfo>();
        foreach (DataRow row in _TableUser.Rows)
          preloadInfos.Add(new EFPConfigSectionInfo(ConfigSectionName,
          ParamsCategory, DataTools.GetString(row, "Code")));
        foreach (DataRow row in _TableHist.Rows)
          preloadInfos.Add(new EFPConfigSectionInfo(ConfigSectionName,
          ParamsCategory, DataTools.GetString(row, "Code")));

        this.ConfigManager.Preload(preloadInfos.ToArray(), EFPConfigMode.Read);

        #endregion
      }

      #region Нормирование настроек конфигурации

      // Должно быть вне блока Begin/EndGetAuxText()
      TempCfg[] defaultSetCfgs = new TempCfg[DefaultSets.Count];

      TempCfg currCfg = new TempCfg();
      FormHandler.ConfigFromControls(currCfg);
      try
      {
        for (int i = 0; i < DefaultSets.Count; i++)
        {
          FormHandler.ConfigToControls(DefaultSets[i].Config);
          defaultSetCfgs[i] = new TempCfg();
          FormHandler.ConfigFromControls(defaultSetCfgs[i]);
        }
      }
      finally
      {
        FormHandler.ConfigToControls(currCfg);
      }

      #endregion

      if (_UseAuxText)
        AuxTextHandler.BeginGetAuxText();
      try
      {
        #region Сначала - именные данные пользователя

        _TableUser.DefaultView.Sort = "Name";
        foreach (DataRowView drv in _TableUser.DefaultView)
        {
          string code = DataTools.GetString(drv.Row, "Code");
          //DateTime? dt = DataTools.GetNullableDateTime(drv.Row, "Time");

          string auxText = null;
          if (_UseAuxText)
          {
            EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName, ParamsCategory, code);
            CfgPart cfgData;
            using (this.ConfigManager.GetConfig(configInfo, EFPConfigMode.Read, out cfgData))
            {
              auxText = AuxTextHandler.GetAuxText(cfgData);
            }
          }

          Control.Items.Add(new ParamSetComboBoxItem(code,
          DataTools.GetString(drv.Row, "Name"),
          "User",
          null,
          GroupUser,
          DataTools.GetString(drv.Row, "MD5"), auxText));
        }

        #endregion

        #region Затем - по умолчанию

        for (int i = 0; i < DefaultSets.Count; i++)
        {
          string auxText = null;
          if (_UseAuxText)
            auxText = AuxTextHandler.GetAuxText(DefaultSets[i].Config);

          Control.Items.Add(new ParamSetComboBoxItem(StdConvert.ToString(i), "(" + DefaultSets[i].DisplayName + ")", DefaultSets[i].ImageKey, null, GroupDefault, defaultSetCfgs[i].MD5Sum(), auxText));
        }

        #endregion

        #region Последние - данные истории

        //_TableHist.DefaultView.Sort = "Order";
        _TableHist.DefaultView.Sort = "Time DESC"; // 06.12.2024
        int cnt = 0;
        foreach (DataRowView drv in _TableHist.DefaultView)
        {
          string code = DataTools.GetString(drv.Row, "Code");
          DateTime? dt = DataTools.GetNullableDateTime(drv.Row, "Time");
          cnt++;
          string name;
          switch (cnt)
          {
            case 1:
              name = Res.ParamSetComboBox_Msg_Hist0;
              break;
            case 2:
              name = Res.ParamSetComboBox_Msg_Hist1;
              break;
            default:
              name = String.Format(Res.ParamSetComboBox_Msg_HistN, cnt);
              break;
          }

          string auxText = null;
          if (_UseAuxText)
          {
            EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName, ParamsCategory, code);
            CfgPart cfgData;
            using (this.ConfigManager.GetConfig(configInfo, EFPConfigMode.Read, out cfgData))
            {
              auxText = AuxTextHandler.GetAuxText(cfgData);
            }
          }

          Control.Items.Add(new ParamSetComboBoxItem(code, name, "Time", dt, GroupHist,
          DataTools.GetString(drv.Row, "MD5"), auxText));
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
      EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName,
        HistoryCategory, String.Empty);
      CfgPart cfgHist;
      using (this.ConfigManager.GetConfig(configInfo, EFPConfigMode.Write, out cfgHist))
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

    #region Обработчики для списка работы с готовыми наборами

    void SetComboBox_ItemSelected(object sender, ParamSetComboBoxItemEventArgs args)
    {
      if (args.Item.Group == GroupDefault)
      {
        int defIndex = int.Parse(args.Item.Code);
        // Выбран набор по умолчанию
        ConfigToControls(DefaultSets[defIndex].Config);
      }
      else
      {
        string userSetName = args.Item.Code;
        EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName,
          ParamsCategory, userSetName);
        CfgPart cfgData;
        using (this.ConfigManager.GetConfig(configInfo, EFPConfigMode.Read, out cfgData))
        {
          ConfigToControls(cfgData);
        }
      }
    }

    void SetComboBox_SaveClick(object sender, ParamSetComboBoxSaveEventArgs args)
    {
      if (String.IsNullOrEmpty(ConfigSectionName))
      {
        EFPApp.ErrorMessageBox(Res.ParamSetComboBox_Err_NotPersist);
        return;
      }

      if (!EFPConfigTools.IsPersist(this.ConfigManager.Persistence))
      {
        EFPApp.ErrorMessageBox(Res.ParamSetComboBox_Err_NotPersist);
        return;
      }

      if (!BaseProvider.FormProvider.ValidateForm())
        return;

      ParamSetComboBoxItem oldItem = Control.Items.FindDisplayName(args.DisplayName);
      if (oldItem != null)
      {
        if (!oldItem.Code.StartsWith("User", StringComparison.Ordinal))
        {
          EFPApp.ShowTempMessage(Res.ParamSetComboBox_Err_NotUserSetOverwrite);
          return;
        }
        if (EFPApp.MessageBox(String.Format(Res.ParamSetComboBox_Msg_Overwrite, args.DisplayName),
          Res.ParamSetComboBox_Title_Overwrite,
          MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
          return;
      }

      if (args.DisplayName.StartsWith("(", StringComparison.Ordinal))
      {
        EFPApp.ShowTempMessage(Res.ParamSetComboBox_Err_NameStartsWithBracket);
        return;
      }

      string userSetName;
      if (oldItem != null)
      {
        userSetName = oldItem.Code;
        Control.Items.Remove(oldItem);
      }
      else
      {
        int cnt = 1;
        while (true)
        {
          userSetName = "User" + StdConvert.ToString(cnt);
          if (_TableUser.Rows.Find(userSetName) == null)
            break;
          cnt++;
        }
      }

      EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName,
        ParamsCategory, userSetName);
      CfgPart cfgData;
      using (this.ConfigManager.GetConfig(configInfo, EFPConfigMode.Write, out cfgData))
      {
        cfgData.Clear();
        ConfigFromControls(cfgData);

        string auxText = null;
        if (_UseAuxText)
        {
          AuxTextHandler.BeginGetAuxText();
          try
          {
            auxText = AuxTextHandler.GetAuxText(cfgData);
          }
          finally
          {
            AuxTextHandler.EndGetAuxText();
          }
        }

        // 06.12.2024
        // Тоже требуется TempCfg для правильной контрольной суммы
        TempCfg cfgData2 = new TempCfg();
        ConfigFromControls(cfgData2);
        string md5 = cfgData2.MD5Sum();

        ParamSetComboBoxItem newItem = new ParamSetComboBoxItem(userSetName, args.DisplayName, "User", null, GroupUser, md5, auxText);
        Control.Items.Insert(0, newItem);
        Control.SelectedItem = newItem;
        DataRow row = DataTools.FindOrAddPrimaryKeyRow(_TableUser, userSetName);
        row["Name"] = args.DisplayName;
        row["Time"] = DateTime.Now;
        row["MD5"] = newItem.MD5Sum;
        SaveSetsTables();
      }
    }

    void SetComboBox_DeleteClick(object sender, ParamSetComboBoxItemEventArgs args)
    {
      DataTable table;
      if (args.Item.Code.StartsWith("User", StringComparison.Ordinal))
        table = _TableUser;
      else if (args.Item.Code.StartsWith("Hist", StringComparison.Ordinal))
        table = _TableHist;
      else
      {
        EFPApp.ErrorMessageBox(Res.ParamSetComboBox_Err_CannotDelete, Res.ParamSetComboBox_Title_Delete);
        return;
      }

      DataRow row = table.Rows.Find(args.Item.Code);
      if (row == null)
      {
        BugException ex = new BugException("Preset with code \"" + args.Item.Code + "\" not found");
        ex.Data["Item"] = args.Item;
        throw ex;
      }

      if (EFPApp.MessageBox(String.Format(Res.ParamSetComboBox_Msg_Delete, args.Item.DisplayName),
        Res.ParamSetComboBox_Title_Delete, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
        return;

      table.Rows.Remove(row);
      SaveSetsTables();

      Control.Items.Remove(args.Item);
    }

    void SetComboBox_CanDeleteItem(object sender, ParamSetComboBoxItemCancelEventArgs args)
    {
      if (args.Item.Code.StartsWith("Hist", StringComparison.Ordinal) || args.Item.Code.StartsWith("User", StringComparison.Ordinal))
        args.Cancel = false;
      else
        args.Cancel = true;
    }

    #endregion

    #region Вызывается после закрытия диалога

    /// <summary>
    /// При закрытии формы нажатием "ОК" извлекает текущие значения из формы и помещает их в секцию конфигурации
    /// в качестве истории.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();

      if (BaseProvider.FormProvider.ValidateReason == EFPFormValidateReason.Closing)
      {
        EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName,
          ParamsCategory, String.Empty);
        CfgPart cfg;
        using (this.ConfigManager.GetConfig(configInfo, EFPConfigMode.Write, out cfg))
        {
          ConfigFromControls(cfg);
        }

        // 06.12.2024
        // Запись конфигурации в реестр и последующее вычисление суммы MD5 может быть не эквивалентно вычислению суммы для временной секции конфигурации.
        // Используем отдельный TempCfg.
        TempCfg cfg2 = new TempCfg();
        ConfigFromControls(cfg2);
        string md5 = cfg2.MD5Sum();
        AfterSave(md5);
      }
    }

    private void AfterSave(string md5Sum)
    {
      if (_TableHist == null)
        return; // 10.03.2016

      bool found = false;
      foreach (DataRowView drv in _TableHist.DefaultView)
      {
        if (DataTools.GetString(drv.Row, "MD5") == md5Sum)
        {
          drv.Row["Time"] = DateTime.Now;
          // drv.Row["Order"] = DataTools.GetInt32(_TableHist.DefaultView[_TableHist.DefaultView.Count - 1].Row, "Order") + 1;
          found = true;
          break;
        }
      }

      if (!found)
      {
        // Новые данные записываем в другую секцию
        DataRow resRow = null;
        if (_TableHist.DefaultView.Count >= 9) // все позиции заняты
          resRow = _TableHist.DefaultView[_TableHist.DefaultView.Count - 1].Row; // вместо самой старой позиции
        else
        {
          for (int i = 1; i <= 9; i++)
          {
            if (DataTools.FindOrAddPrimaryKeyRow(_TableHist, "Hist" + StdConvert.ToString(i), out resRow))
              break;
          }
        }
        string userSetName = DataTools.GetString(resRow, "Code");
        resRow["Time"] = DateTime.Now;
        resRow["MD5"] = md5Sum;
        //if (_TableHist.Rows.Count > 0)
        //  resRow["Order"] = DataTools.GetInt32(_TableHist.DefaultView[_TableHist.DefaultView.Count - 1].Row, "Order") + 1;
        //else
        //  resRow["Order"] = 1;


        EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName,
          ParamsCategory, userSetName);
        CfgPart cfgData;
        using (this.ConfigManager.GetConfig(configInfo, EFPConfigMode.Write, out cfgData))
        {
          cfgData.Clear();
          ConfigFromControls(cfgData);
        }

        SaveSetsTables();
      }
    }

    #endregion

    //#region Вспомогательные методы

    ///// <summary>
    ///// Обновить настройки по умолчанию в секции конфигурации.
    ///// Передает значения из <paramref name="cfg"/> в управляющие элементы с помощью <see cref="FormHandler"/>
    ///// а затем считывает их в другую секцию конфигурации, которая и возвращается.
    ///// На время вызова сохраняются и восстанавливаются текущие настройки формы
    ///// </summary>
    ///// <param name="cfg">Исходная секция, возможно, с неполным комплектом настроек</param>
    ///// <returns>Полный комплект настроек</returns>
    //public TempCfg UpdateDefaultConfig(TempCfg cfg)
    //{
    //  TempCfg currCfg = new TempCfg();
    //  FormHandler.ConfigFromControls(currCfg);

    //  FormHandler.ConfigToControls(cfg);
    //  TempCfg newCfg = new TempCfg();
    //  FormHandler.ConfigFromControls(newCfg);

    //  FormHandler.ConfigToControls(currCfg);

    //  return newCfg;
    //}

    //#endregion

    #region Отладочные средства

    /// <summary>
    /// Показывать отладочные команды в локальном меню?
    /// </summary>
    public static bool DebugCommands { get { return _DebugCommands; } set { _DebugCommands = value; } }
    private static bool _DebugCommands = false;

    private void DebugCurrentSettings_Click(object sender, EventArgs args)
    {
      TempCfg cfg = new TempCfg();
      ConfigFromControls(cfg);
      string md5 = cfg.MD5Sum();
      EFPApp.ShowXmlView(cfg.Document, String.Format(Res.ParamSetComboBox_Title_CurrentSettingsXML, md5));
    }

    private void DebugSavedSettings_Click(object sender, EventArgs args)
    {
      ParamSetComboBoxItem item = Control.Items.FindDisplayName(efpSelCB.Text);
      if (item == null)
      {
        EFPApp.ShowTempMessage(Res.ParamSetComboBox_Msg_NoSelection);
        return;
      }

      TempCfg cfg;
      if (item.Group == GroupDefault)
      {
        int defIndex = int.Parse(item.Code);
        // Выбран набор по умолчанию
        cfg = DefaultSets[defIndex].Config;
      }
      else
      {
        string userSetName = item.Code;
        EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName,
          ParamsCategory, userSetName);
        CfgPart cfgData;
        using (this.ConfigManager.GetConfig(configInfo, EFPConfigMode.Read, out cfgData))
        {
          cfg = new TempCfg();
          cfgData.CopyTo(cfg);
        }
      }
      EFPApp.ShowXmlView(cfg.Document, item.DisplayName + " (" + item.MD5Sum + ")");
    }

    #endregion
  }
}
