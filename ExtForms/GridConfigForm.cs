using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.Config;

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// Форма настройки табличного просмотра с помощью интерфейса IEFPGridProducerEditor
  /// </summary>
  internal partial class GridConfigForm : Form
  {
    #region Конструктор формы

    public GridConfigForm(IEFPControl callerControlProvider, string configCategory, string historyCategory)
    {
      InitializeComponent();
      Icon = EFPApp.MainImageIcon("EditGridConfig");

#if DEBUG
      if (callerControlProvider == null)
        throw new ArgumentNullException("callerControlProvider");
      if (String.IsNullOrEmpty(configCategory))
        throw new ArgumentNullException("configCategory");
      if (String.IsNullOrEmpty(historyCategory))
        throw new ArgumentNullException("historyCategory");
#endif

      _CallerControlProvider = callerControlProvider;
      _ConfigCategory = configCategory;
      _HistoryCategory = historyCategory;

      FormProvider = new EFPFormProvider(this);
      FormProvider.ConfigSectionName = "GridConfigForm";
      FormProvider.AddFormCheck(new EFPValidatingEventHandler(FormCheck));

      #region Готовые наборы

      UseHistory = EFPConfigTools.IsPersist(_CallerControlProvider.ConfigManager.Persistence);

      if (UseHistory)
      {
        SetComboBox.ShowImages = EFPApp.ShowListImages;
        EFPTextComboBox efpSelCB = new EFPTextComboBox(FormProvider, SetComboBox.TheCB);
        efpSelCB.DisplayName = "Готовые наборы";
        efpSelCB.ToolTipText = "Выбор готового набора настроек из выпадающего списка." + Environment.NewLine +
          "В список входят:" + Environment.NewLine +
          "- пользовательские наборы, которые Вы сохранили;" + Environment.NewLine +
          "- настройки по умолчанию;" + Environment.NewLine +
          "- а также до 9 последних наборов настроек (история)" + Environment.NewLine + Environment.NewLine +
          "Поле для ввода названия для нового набора";

        EFPButton efpSaveButton = new EFPButton(FormProvider, SetComboBox.SaveButton);
        efpSaveButton.DisplayName = "Сохранить набор";
        efpSaveButton.ToolTipText = "Сохранить заданные настройки как новый пользовательский набор." + Environment.NewLine +
          "Перед нажатием кнопки в поле слева должно быть введено имя набора";

        EFPButton efpDelButton = new EFPButton(FormProvider, SetComboBox.DeleteButton);
        efpDelButton.DisplayName = "Удалить набор";
        efpDelButton.ToolTipText = "Удалить пользовательский набор настроек, имя которого задано в списке слева";

        CreateSetsTables();

        SetComboBox.ItemSelected += new ParamSetComboBoxItemEventHandler(SetComboBox_ItemSelected);
        SetComboBox.SaveClick += new ParamSetComboBoxSaveEventHandler(SetComboBox_SaveClick);
        SetComboBox.DeleteClick += new ParamSetComboBoxItemEventHandler(SetComboBox_DeleteClick);
        SetComboBox.CanDeleteItem += new ParamSetComboBoxItemCancelEventHandler(SetComboBox_CanDeleteItem);

      }
      else
      {
        grpSets.Visible = false;
        btnDefault.Image = EFPApp.MainImages.Images["No"];
        btnDefault.ImageAlign = ContentAlignment.MiddleLeft;
        panDefault.Visible = true;
        EFPButton efpDefault = new EFPButton(FormProvider, btnDefault);
        efpDefault.ToolTipText = "Сброс настроек столбцов и подсказок в исходное состояние";
        efpDefault.Click += new EventHandler(efpDefault_Click);
      }

      #endregion

      #region Буфер обмена

      btnCopy.Image = EFPApp.MainImages.Images["Copy"];
      btnCopy.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpCopy = new EFPButton(FormProvider, btnCopy);
      efpCopy.DisplayName = "Копировать";
      efpCopy.ToolTipText = "Копирует текущие настройки, заданные в этом окне, в буфер обмена";
      efpCopy.Click += new EventHandler(efpCopy_Click);

      btnPaste.Image = EFPApp.MainImages.Images["Paste"];
      btnPaste.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpPaste = new EFPButton(FormProvider, btnPaste);
      efpPaste.DisplayName = "Вставить";
      efpPaste.ToolTipText = "Заменяет текущие настройки просмотра, ранее скопированными в буфер обмена." + Environment.NewLine +
        "Если настройки были скопированы для другого просмотра, то будут вставлены только совместимые настройки";
      efpPaste.Click += new EventHandler(efpPaste_Click);

      #endregion
    }

    #endregion

    #region Свойства

    public EFPFormProvider FormProvider;

    /// <summary>
    /// Выполняет чтение и запись значений в форму
    /// </summary>
    public IEFPGridProducerEditor Editor;

    /// <summary>
    /// Используются ли готовые наборы
    /// </summary>
    bool UseHistory;

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
      _TableHist.Columns.Add("Код", typeof(string));
      _TableHist.Columns.Add("Время", typeof(DateTime));
      _TableHist.Columns.Add("MD5", typeof(string));
      _TableHist.Columns.Add("NPop", typeof(int));
      DataTools.SetPrimaryKey(_TableHist, "Код");

      _TableUser = new DataTable();
      _TableUser.Columns.Add("Код", typeof(string));
      _TableUser.Columns.Add("Название", typeof(string));
      _TableUser.Columns.Add("Время", typeof(DateTime));
      _TableUser.Columns.Add("MD5", typeof(string));
      DataTools.SetPrimaryKey(_TableUser, "Код");

      CfgPart cfgHist;
      EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(_CallerControlProvider.ConfigSectionName,
        this._ConfigCategory, String.Empty);
      using (_CallerControlProvider.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Read, out cfgHist))
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
          EFPApp.ShowException(e, "Ошибка чтения списка истории");
        }
      }
    }

    private string[] _DefaultConfigCodes;
    private EFPDataGridViewConfig[] _DefaultConfigs;

    /// <summary>
    /// Заполнение комбоблока "Наборы"
    /// (вызывается после вызова конструктора)
    /// </summary>
    public void FillSetItems()
    {
      if (!UseHistory)
        return; // 25.11.2015

      // Сначала - именные данные пользователя
      _TableUser.DefaultView.Sort = "Название";
      foreach (DataRowView drv in _TableUser.DefaultView)
      {
        //DateTime? dt = DataTools.GetNullableDateTime(drv.Row, "Время");
        SetComboBox.Items.Add(new ParamSetComboBoxItem(DataTools.GetString(drv.Row, "Код"),
        DataTools.GetString(drv.Row, "Название"), "User", null, GroupUser,
        DataTools.GetString(drv.Row, "MD5")));
      }

      // Затем - по умолчанию
      Editor.GetDefaultConfigs(out _DefaultConfigCodes, out _DefaultConfigs);
      for (int i = 0; i < _DefaultConfigCodes.Length; i++)
      {
        string DisplayName;
        string ImageKey;
        if (String.IsNullOrEmpty(_DefaultConfigCodes[i]))
          DisplayName = "(По умолчанию)";
        else
          DisplayName = _DefaultConfigCodes[i];

        ImageKey = _DefaultConfigs[i].ImageKey; // 23.11.2016
        if (String.IsNullOrEmpty(ImageKey))
          ImageKey = "No";

        TempCfg Tmp = new TempCfg();
        _DefaultConfigs[i].WriteConfig(Tmp);
        SetComboBox.Items.Add(new ParamSetComboBoxItem(_DefaultConfigCodes[i], DisplayName, ImageKey, null, GroupDefault, Tmp.MD5Sum()));
      }
#if XXX
      // Затем - именованые стандартные настройки
      foreach(EFPDataGridViewConfig Config in FCallerControlProvider.GridProducer.)
#endif

      // Последние - данные истории
      _TableHist.DefaultView.Sort = "NPop";
      int cnt = 0;
      for (int i = _TableHist.DefaultView.Count - 1; i >= 0; i--)
      {
        DataRow Row = _TableHist.DefaultView[i].Row;
        DateTime? dt = DataTools.GetNullableDateTime(Row, "Время");
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
        SetComboBox.Items.Add(new ParamSetComboBoxItem(DataTools.GetString(Row, "Код"),
        Name, "Time", dt, GroupHist,
        DataTools.GetString(Row, "MD5")));
      }

    }


    private void SaveSetsTables()
    {
      EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(_CallerControlProvider.ConfigHandler.ConfigSectionName,
        this._HistoryCategory, String.Empty);
      CfgPart cfgHist;
      using (_CallerControlProvider.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Write, out cfgHist))
      {
        cfgHist.Clear();
        foreach (DataRowView drv in _TableHist.DefaultView)
        {
          CfgPart cfgOne = cfgHist.GetChild(DataTools.GetString(drv.Row, "Код"), true);
          cfgOne.SetNullableDateTime("Time", DataTools.GetNullableDateTime(drv.Row, "Время"));
          cfgOne.SetString("MD5", DataTools.GetString(drv.Row, "MD5"));
        }
        foreach (DataRowView drv in _TableUser.DefaultView)
        {
          CfgPart cfgOne = cfgHist.GetChild(DataTools.GetString(drv.Row, "Код"), true);
          cfgOne.SetString("Name", DataTools.GetString(drv.Row, "Название"));
          cfgOne.SetNullableDateTime("Time", DataTools.GetNullableDateTime(drv.Row, "Время"));
          cfgOne.SetString("MD5", DataTools.GetString(drv.Row, "MD5"));
        }
      }
    }

    #endregion

    #region Обработчики для списка работы с готовыми наборами

    void SetComboBox_ItemSelected(object sender, ParamSetComboBoxItemEventArgs args)
    {
      if (!FormProvider.IsFormVisible) // вызов при начальной установке свойства
        return;

      if (args.Item.Group == GroupDefault)
      {
        // Выбран набор по умолчанию
        int p = Array.IndexOf<string>(_DefaultConfigCodes, args.Item.Code);
        if (p < 0)
          throw new BugException("Не нашли стандартную настройку с кодом \"" + args.Item.Code + "\"");
        EFPDataGridViewConfig Config = _DefaultConfigs[p];
        Editor.WriteFormValues(Config);
      }
      else
      {
        string UserSetName = args.Item.Code;
        EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(_CallerControlProvider.ConfigHandler.ConfigSectionName,
          this._ConfigCategory, UserSetName);
        CfgPart cfgData;
        using (_CallerControlProvider.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Read, out cfgData))
        {
          EFPDataGridViewConfig Config = new EFPDataGridViewConfig();
          Config.ReadConfig(cfgData);
          Editor.WriteFormValues(Config);
        }
      }
    }

    void SetComboBox_SaveClick(object sender, ParamSetComboBoxSaveEventArgs args)
    {
      if (String.IsNullOrEmpty(_CallerControlProvider.ConfigSectionName))
      {
        EFPApp.ErrorMessageBox("Табличный просмотр не предусматривает сохранение настроек между сеансами работы");
        return;
      }

      if (!EFPConfigTools.IsPersist(_CallerControlProvider.ConfigManager.Persistence))
      {
        EFPApp.ErrorMessageBox("Cохранение параметров между сеансами работы не предусмотрено в программе");
        return;
      }

      if (!FormProvider.ValidateForm())
        return;

      ParamSetComboBoxItem OldItem = SetComboBox.Items.FindDisplayName(args.DisplayName);
      if (OldItem != null)
      {
        if (!OldItem.Code.StartsWith("User"))
        {
          EFPApp.ShowTempMessage("Перезаписывать можно только пользовательские наборы");
          return;
        }
        if (EFPApp.MessageBox("Набор \"" + args.DisplayName + "\" уже существует. Вы хотите перезаписать его?",
          "Подтверждение перезаписи набора",
          MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
          return;
      }

      if (args.DisplayName.StartsWith("("))
      {
        EFPApp.ShowTempMessage("Имя набора не может начинаться со скобки");
        return;
      }

      string UserSetName;
      if (OldItem != null)
      {
        UserSetName = OldItem.Code;
        SetComboBox.Items.Remove(OldItem);
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

      EFPDataGridViewConfig Config = new EFPDataGridViewConfig();
      string ErrorText;
      if (!Editor.ReadFormValues(Config, out ErrorText))
      {
        EFPApp.ErrorMessageBox(ErrorText);
        return;
      }
      CfgPart cfgData;
      EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(_CallerControlProvider.ConfigHandler.ConfigSectionName,
        this._ConfigCategory, UserSetName);
      using (_CallerControlProvider.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Write, out cfgData))
      {
        cfgData.Clear();
        Config.WriteConfig(cfgData);

        ParamSetComboBoxItem NewItem = new ParamSetComboBoxItem(UserSetName, args.DisplayName, "User", null, GroupUser, cfgData.MD5Sum());
        SetComboBox.Items.Insert(0, NewItem);
        SetComboBox.SelectedItem = NewItem;
        DataRow Row = DataTools.FindOrAddPrimaryKeyRow(_TableUser, UserSetName);
        Row["Название"] = args.DisplayName;
        Row["Время"] = DateTime.Now;
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
        EFPApp.ErrorMessageBox("Этот набор нельзя удалить", "Удаление готового набора");
        return;
      }

      DataRow Row = Table.Rows.Find(args.Item.Code);
      if (Row == null)
      {
        BugException Ex = new BugException("Набор с кодом \"" + args.Item.Code + "\" не найден");
        Ex.Data["Item"] = args.Item;
        throw Ex;
      }

      if (EFPApp.MessageBox("Удалить набор \"" + args.Item.DisplayName + "\"?",
        "Подтверждение удаления набора", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
        return;

      Table.Rows.Remove(Row);
      SaveSetsTables();

      SetComboBox.Items.Remove(args.Item);
    }

    void SetComboBox_CanDeleteItem(object sender, ParamSetComboBoxItemCancelEventArgs args)
    {
      if (args.Item.Code.StartsWith("Hist") || args.Item.Code.StartsWith("User"))
        args.Cancel = false;
      else
        args.Cancel = true;
    }

    #endregion

    #region Кнопка "По умолчанию"

    void efpDefault_Click(object sender, EventArgs args)
    {
      string[] DefaultConfigCodes;
      EFPDataGridViewConfig[] DefaultConfigs;
      Editor.GetDefaultConfigs(out DefaultConfigCodes, out DefaultConfigs);
      switch (DefaultConfigCodes.Length)
      {
        case 0:
          EFPApp.ShowTempMessage("Настройка по умолчанию не определена");
          break;
        case 1:
          Editor.WriteFormValues(DefaultConfigs[0]);
          break;
        default:
          // Выбираем, какую настройку применить
          ListSelectDialog dlg = new ListSelectDialog();
          dlg.Title = "Выбор настройки по умолчанию";
          dlg.ImageKey = "No";
          dlg.Items = new string[DefaultConfigCodes.Length];
          for (int i = 0; i < DefaultConfigCodes.Length; i++)
          {
            //string ImageKey;
            if (String.IsNullOrEmpty(DefaultConfigCodes[i]))
              dlg.Items[i] = "(По умолчанию)";
            else
              dlg.Items[i] = DefaultConfigCodes[i];

            dlg.ImageKeys[i] = DefaultConfigs[i].ImageKey;
          }
          dlg.CanBeEmpty = false;
          dlg.ConfigSectionName = "GridConfigSelectDefaultDialog";

          if (dlg.ShowDialog() != DialogResult.OK)
            return;

          Editor.WriteFormValues(DefaultConfigs[dlg.SelectedIndex]);
          break;
      }
    }

    #endregion

    #region Буфер обмена

    private void efpCopy_Click(object sender, EventArgs args)
    {
      EFPDataGridViewConfig Config = new EFPDataGridViewConfig();
      string ErrorText;
      if (!Editor.ReadFormValues(Config, out ErrorText))
      {
        EFPApp.ErrorMessageBox(ErrorText);
        return;
      }

      DataObject dobj = new DataObject();
      dobj.SetData(Config);
      Clipboard.SetDataObject(dobj);
    }

    private void efpPaste_Click(object sender, EventArgs args)
    {
      IDataObject dobj = Clipboard.GetDataObject();
      if (dobj == null)
      {
        EFPApp.ShowTempMessage("Буфер обмена пуст");
        return;
      }

      EFPDataGridViewConfig Config = dobj.GetData(typeof(EFPDataGridViewConfig)) as EFPDataGridViewConfig;
      if (Config == null)
      {
        // string txtFormats = String.Join(", ", dobj.GetFormats());
        EFPApp.ShowTempMessage("Буфер обмена не содержит настроек табличного просмотра");
        return;
      }
      Editor.WriteFormValues(Config);
    }

    #endregion

    #region Вызывается после закрытия диалога

    /// <summary>
    /// Конфигурация, созданная при нажатии кнопки "ОК".
    /// Нельзя будет извлечь конгигурацию, когда форма закрыта
    /// </summary>
    public EFPDataGridViewConfig ResultConfig;

    private void FormCheck(object sender, EFPValidatingEventArgs args)
    {
      if (FormProvider.ValidateReason == EFPFormValidateReason.Shown)
        return;

      ResultConfig = new EFPDataGridViewConfig();
      string ErrorText;
      if (!Editor.ReadFormValues(ResultConfig, out ErrorText))
      {
        args.SetError(ErrorText);
        return;
      }

      TempCfg TempSectData = new TempCfg();
      ResultConfig.WriteConfig(TempSectData);
      string MD5Sum = TempSectData.MD5Sum();

      if (UseHistory)
      {
        bool Found = false;
        foreach (DataRowView drv in _TableHist.DefaultView)
        {
          if (DataTools.GetString(drv.Row, "MD5") == MD5Sum)
          {
            drv.Row["Время"] = DateTime.Now;
            drv.Row["NPop"] = DataTools.GetInt(_TableHist.DefaultView[_TableHist.DefaultView.Count - 1].Row, "NPop") + 1;
            Found = true;
            break;
          }
        }

        if (!Found)
        {
          // Новые данные записываем в другую секцию
          DataRow ResRow = null;
          if (_TableHist.DefaultView.Count >= 9) // все позиции заняты
            ResRow = _TableHist.DefaultView[0].Row;
          else
          {
            for (int i = 1; i <= 9; i++)
            {
              if (DataTools.FindOrAddPrimaryKeyRow(_TableHist, "Hist" + i.ToString(), out ResRow))
                break;
            }
          }
          string UserSetName = DataTools.GetString(ResRow, "Код");
          ResRow["Время"] = DateTime.Now;
          ResRow["MD5"] = MD5Sum;
          if (_TableHist.Rows.Count > 0)
            ResRow["NPop"] = DataTools.GetInt(_TableHist.DefaultView[_TableHist.DefaultView.Count - 1].Row, "NPop") + 1;
          else
            ResRow["NPop"] = 1;

          EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(_CallerControlProvider.ConfigHandler.ConfigSectionName,
            this._ConfigCategory, UserSetName);
          CfgPart cfgData;
          using (_CallerControlProvider.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Write, out cfgData))
          {
            cfgData.Clear();
            ResultConfig.WriteConfig(cfgData);
          }
        }

        SaveSetsTables();
      }
    }

    #endregion
  }
}