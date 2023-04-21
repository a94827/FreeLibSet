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
  /// Используется провайдером EFPConfigParamSetComboBox.
  /// Интерфейс, как правило, реализуется непосредственно формой, в которой расположен ParamSetComboBox.
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
  /// Используется провайдером EFPConfigParamSetComboBox.
  /// Интерфейс, как правило, реализуется непосредственно формой, в которой расположен ParamSetComboBox.
  /// </summary>
  public interface IEFPConfigParamSetAuxTextHandler
  {
    /// <summary>
    /// Этот метод вызывается однократно перед последовательностью вызовов GetAuxText()
    /// </summary>
    void BeginGetAuxText();

    /// <summary>
    /// Этот метод должен вернуть строку AuxText для секции конфигурации с набором параметров
    /// </summary>
    /// <param name="cfg">Секция конфигурации, доступная для чтения, содержащая значения для одного набора</param>
    string GetAuxText(CfgPart cfg);

    /// <summary>
    /// Этот метод вызывается однократно после последовательности вызовов GetAuxText()
    /// </summary>
    void EndGetAuxText();
  }

  /// <summary>
  /// Провайдер управляющего элемента "Набор параметров", состоящего из
  /// комбоблока, кнопок "+" и "-".
  /// Предназначен для хранения "истории" введенных значений в форме, а
  /// также пользовательских "именных" наборов.
  /// Для хранения используется система, реализующая IEFPConfigManager.
  /// Вызывающий код должен предоставить интерфейс, выполняющий чтение и запись значений в поля формы.
  /// Должно быть обязательно установлено свойство ConfigSectionName (и, при необходимости, ParamsCategory и HistoryCategory). 
  /// Используйте список DefaultSets для задания набора(ов) по умолчанию.
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

      efpSaveButton = new EFPButton(baseProvider, control.SaveButton);
      efpSaveButton.DisplayName = "Сохранить набор";
      efpSaveButton.ToolTipText = "Сохранить установленные значения как новый пользовательский набор" + Environment.NewLine +
        "Перед нажатием кнопки в поле слева должно быть введено имя набора";

      efpDelButton = new EFPButton(baseProvider, control.DeleteButton);
      efpDelButton.DisplayName = "Удалить набор";
      efpDelButton.ToolTipText = "Удалить пользовательский набор значений, имя которого задано в списке слева";

      _ParamsCategory = EFPConfigCategories.UserParams;
      _HistoryCategory = EFPConfigCategories.UserHistory;

      _DefaultSets = new DefaultSetList(this);

      control.ShowImages = EFPApp.ShowListImages;

      InitConfigHandler(); // иначе потеряется свойство ConfigSectionName
    }

    #endregion

    #region Поля

    private EFPTextComboBox efpSelCB;

    private EFPButton efpSaveButton;

    private EFPButton efpDelButton;

    #endregion

    #region Используемые секции конфигурации

    /// <summary>
    /// Имя категории секций конфигурации, используемых для хранения текущих значений.
    /// По умолчанию равно EFPConfigCategories.UserParams
    /// Свойство должно быть установлено до вывода формы на экран.
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
    /// Имя категории секций конфигурации, используемых для хранения истории.
    /// По умолчанию равно EFPConfigCategories.UserHistory
    /// Свойство должно быть установлено до вывода формы на экран.
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

    #region Стандартные наборы

    /// <summary>
    /// Описание одной строчки в списке для значения "По умолчанию"
    /// </summary>
    public sealed class DefaultSet
    {
      #region Конструкторы

      /// <summary>
      /// Создает объект
      /// </summary>
      /// <param name="config">Набор параметров для значения "по умолчанию"</param>
      /// <param name="displayName">Отображаемое имя (без скобок)</param>
      /// <param name="imageKey">Значок в списке EFPApp.MainImages</param>
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
      /// Создает объект со значком по умолчанию "No"
      /// </summary>
      /// <param name="config">Набор параметров для значения "по умолчанию"</param>
      /// <param name="displayName">Отображаемое имя (без скобок)</param>
      public DefaultSet(TempCfg config, string displayName)
        : this(config, displayName, "No")
      {
      }

      /// <summary>
      /// Создает объект с текстом "По умолчанию" со значком по умолчанию "No"
      /// </summary>
      /// <param name="config">Набор параметров для значения "по умолчанию"</param>
      public DefaultSet(TempCfg config)
        : this(config, "По умолчанию", "No")
      {
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Набор параметров для значения "по умолчанию"
      /// </summary>
      public TempCfg Config { get { return _Config; } }
      private TempCfg _Config;

      /// <summary>
      /// Отображаемое имя (без скобок)
      /// </summary>
      public string DisplayName { get { return _DisplayName; } }
      private string _DisplayName;

      /// <summary>
      /// Значок в списке EFPApp.MainImages
      /// </summary>
      public string ImageKey { get { return _ImageKey; } }
      private string _ImageKey;

      /// <summary>
      /// Контрольная сумма для секции конфигурации
      /// </summary>
      public string MD5Sum { get { return _MD5Sum; } }
      private string _MD5Sum;

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
    /// Реализация свойства DefaultSets
    /// </summary>
    public sealed class DefaultSetList : ListWithReadOnly<DefaultSet>
    {
      #region Защищенный конструктор

      internal DefaultSetList(EFPConfigParamSetComboBox owner)
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
        base.Add(new DefaultSet(GetCurrentCfg(), displayName, imageKey));
      }

      /// <summary>
      /// Добавить установки, заданные в форме в настоящий момент, в качестве набора по умолчанию
      /// </summary>
      /// <param name="displayName"></param>
      public void Add(string displayName)
      {
        base.Add(new DefaultSet(GetCurrentCfg(), displayName));
      }

      /// <summary>
      /// Добавить установки, заданные в форме в настоящий момент, в качестве набора по умолчанию
      /// </summary>
      public void Add()
      {
        base.Add(new DefaultSet(GetCurrentCfg()));
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
    public DefaultSetList DefaultSets { get { return _DefaultSets; } }
    private DefaultSetList _DefaultSets;

    #endregion

    #region FormHandler

    /// <summary>
    /// Интерфейс, реализуемый в пользовательском коде, для чтения и записей полей формы.
    /// Задается в конструкторе. Не может быть null
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
        EFPApp.ShowException(e, "Ошибка записи значений в управляющие элементы");
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
        EFPApp.ShowException(e, "Ошибка получения значений из управляющих элементов");
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
        throw new NullReferenceException("Свойство \"ConfigSectionName\" должно быть установлено");

      base.OnCreated();

      _DefaultSets.SetReadOnly();

      _UseAuxText = AuxTextHandler != null && EFPApp.ShowParamSetAuxText;

      CreateSetsTables();

      FillSetItems();

      InitSubToolTips();

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
      Control.SelectedMD5Sum = cfg2.MD5Sum(); // выбираем подходящий набор, если есть

      // Присоединяем обработчики после выбора текущей позиции
      Control.ItemSelected += new ParamSetComboBoxItemEventHandler(SetComboBox_ItemSelected);
      Control.SaveClick += new ParamSetComboBoxSaveEventHandler(SetComboBox_SaveClick);
      Control.DeleteClick += new ParamSetComboBoxItemEventHandler(SetComboBox_DeleteClick);
      Control.CanDeleteItem += new ParamSetComboBoxItemCancelEventHandler(SetComboBox_CanDeleteItem);
    }

    private void InitSubToolTips()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("Выбор готового набора фильтров из выпадающего списка.");
      sb.Append(Environment.NewLine);
      sb.Append("В список входят:");
      sb.Append(Environment.NewLine);
      sb.Append("- пользовательские наборы, которые Вы сохранили;");
      sb.Append(Environment.NewLine);
      if (DefaultSets.Count > 0)
      {
        sb.Append("- настройки по умолчанию;");
        sb.Append(Environment.NewLine);
      }
      sb.Append("- а также до 9 последних наборов значений (история)");
      sb.Append(Environment.NewLine);
      sb.Append(Environment.NewLine);
      sb.Append("Поле для ввода названия для нового набора");

      efpSelCB.ToolTipText = sb.ToString();
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
      _TableHist.Columns.Add("Order", typeof(int));
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
                cfgOne.GetString("MD5"), _TableHist.Rows.Count + 1);
            }
            if (names[i].StartsWith("User", StringComparison.Ordinal))
            {
              CfgPart cfgOne = cfgHist.GetChild(names[i], false);
              _TableUser.Rows.Add(names[i], cfgOne.GetString("Name"), cfgOne.GetNullableDateTime("Time"),
                cfgOne.GetString("MD5"));
            }
          }
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка чтения списка истории");
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

      if (_UseAuxText)
        AuxTextHandler.BeginGetAuxText();
      try
      {
        string auxText = null;

        #region Сначала - именные данные пользователя

        _TableUser.DefaultView.Sort = "Name";
        foreach (DataRowView drv in _TableUser.DefaultView)
        {
          string code = DataTools.GetString(drv.Row, "Code");
          //DateTime? dt = DataTools.GetNullableDateTime(drv.Row, "Time");

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
          if (_UseAuxText)
            auxText = AuxTextHandler.GetAuxText(DefaultSets[i].Config);

          Control.Items.Add(new ParamSetComboBoxItem(i.ToString(), "(" + DefaultSets[i].DisplayName + ")", DefaultSets[i].ImageKey, null, GroupDefault, DefaultSets[i].MD5Sum, auxText));
        }

        #endregion

        #region Последние - данные истории

        _TableHist.DefaultView.Sort = "Order";
        int cnt = 0;
        for (int i = _TableHist.DefaultView.Count - 1; i >= 0; i--)
        {
          DataRow row = _TableHist.DefaultView[i].Row;
          string code = DataTools.GetString(row, "Code");
          DateTime? dt = DataTools.GetNullableDateTime(row, "Time");
          cnt++;
          string Name;
          switch (cnt)
          {
            case 1:
              Name = "(Последний)";
              break;
            case 2:
              Name = "(Предпоследний)";
              break;
            default:
              Name = "(Предыдущий №" + cnt.ToString() + ")";
              break;
          }


          if (_UseAuxText)
          {
            EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName, ParamsCategory, code);
            CfgPart cfgData;
            using (this.ConfigManager.GetConfig(configInfo, EFPConfigMode.Read, out cfgData))
            {
              auxText = AuxTextHandler.GetAuxText(cfgData);
            }
          }

          Control.Items.Add(new ParamSetComboBoxItem(code, Name, "Time", dt, GroupHist,
          DataTools.GetString(row, "MD5"), auxText));
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
        EFPApp.ErrorMessageBox("Не предусмотрено сохранение значений между сеансами работы");
        return;
      }

      if (!EFPConfigTools.IsPersist(this.ConfigManager.Persistence))
      {
        EFPApp.ErrorMessageBox("Cохранение значений между сеансами работы не предусмотрено в программе");
        return;
      }

      if (!BaseProvider.FormProvider.ValidateForm())
        return;

      ParamSetComboBoxItem oldItem = Control.Items.FindDisplayName(args.DisplayName);
      if (oldItem != null)
      {
        if (!oldItem.Code.StartsWith("User", StringComparison.Ordinal))
        {
          EFPApp.ShowTempMessage("Перезаписывать можно только пользовательские наборы");
          return;
        }
        if (EFPApp.MessageBox("Набор \"" + args.DisplayName + "\" уже существует. Вы хотите перезаписать его?",
          "Подтверждение перезаписи набора",
          MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
          return;
      }

      if (args.DisplayName.StartsWith("(", StringComparison.Ordinal))
      {
        EFPApp.ShowTempMessage("Имя набора не может начинаться со скобки");
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
          userSetName = "User" + cnt.ToString();
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

        ParamSetComboBoxItem newItem = new ParamSetComboBoxItem(userSetName, args.DisplayName, "User", null, GroupUser, cfgData.MD5Sum(), auxText);
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
        EFPApp.ErrorMessageBox("Этот набор нельзя удалить", "Удаление готового набора");
        return;
      }

      DataRow row = table.Rows.Find(args.Item.Code);
      if (row == null)
      {
        BugException ex = new BugException("Набор с кодом \"" + args.Item.Code + "\" не найден");
        ex.Data["Item"] = args.Item;
        throw ex;
      }

      if (EFPApp.MessageBox("Удалить набор \"" + args.Item.DisplayName + "\"?",
        "Подтверждение удаления набора", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
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

      bool found = false;
      foreach (DataRowView drv in _TableHist.DefaultView)
      {
        if (DataTools.GetString(drv.Row, "MD5") == md5Sum)
        {
          drv.Row["Time"] = DateTime.Now;
          drv.Row["Order"] = DataTools.GetInt(_TableHist.DefaultView[_TableHist.DefaultView.Count - 1].Row, "Order") + 1;
          found = true;
          break;
        }
      }

      if (!found)
      {
        // Новые данные записываем в другую секцию
        DataRow resRow = null;
        if (_TableHist.DefaultView.Count >= 9) // все позиции заняты
          resRow = _TableHist.DefaultView[0].Row;
        else
        {
          for (int i = 1; i <= 9; i++)
          {
            if (DataTools.FindOrAddPrimaryKeyRow(_TableHist, "Hist" + i.ToString(), out resRow))
              break;
          }
        }
        string userSetName = DataTools.GetString(resRow, "Code");
        resRow["Time"] = DateTime.Now;
        resRow["MD5"] = md5Sum;
        if (_TableHist.Rows.Count > 0)
          resRow["Order"] = DataTools.GetInt(_TableHist.DefaultView[_TableHist.DefaultView.Count - 1].Row, "Order") + 1;
        else
          resRow["Order"] = 1;


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
  }
}
