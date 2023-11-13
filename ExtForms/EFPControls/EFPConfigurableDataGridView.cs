// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using System.ComponentModel;
using FreeLibSet.Logging;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Интерфейс управляющего элемента с фильтрами
  /// </summary>
  public interface IEFPControlWithFilters : IEFPControl
  {
    /// <summary>
    /// Вызывается для дополнительной инициализации табличного просмотра в редакторе фильтров
    /// Переопределеннный метод может, например, инициализировать дополнительные команды меню
    /// </summary>
    /// <param name="filterGridProvider">Обработчик таблицы фильтров</param>
    void InitGridFilterEditorGridView(EFPGridFilterEditorGridView filterGridProvider);

    /// <summary>
    /// Возвращает список фильтров
    /// </summary>
    IEFPGridFilters Filters { get; }

    /// <summary>
    /// Возвращает true, если список фильтров непустой
    /// </summary>
    bool HasFilters { get; }

    /// <summary>
    /// Если свойство установлено в true, то управляющий элемент записывает и восстанавливает фильтры
    /// </summary>
    bool SaveFiltersAllowed { get; set; }

    /// <summary>
    /// Событие вызывается после изменения фильтров
    /// </summary>
    event EventHandler AfterSetFilter;

    /// <summary>
    /// Обновление просмотра при внешней установке значений фильтров
    /// </summary>
    void PerformFilterChanged();
  }

  /// <summary>
  /// Расширение табличного просмотра, поддерживающего индивидуальную настройку столбцов и фильтры
  /// </summary>
  public class EFPConfigurableDataGridView : EFPDataGridView, IEFPControlWithFilters
  {
    #region Конструкторы

    /// <summary>
    /// Создание провайдера
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPConfigurableDataGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control)
    {
      Init();
    }

    /// <summary>
    /// Создание провайдера
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    public EFPConfigurableDataGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar)
      : base(controlWithToolBar)
    {
      Init();
    }

    private void Init()
    {
      Control.AllowUserToOrderColumns = true; // 31.08.2015
      if (!DesignMode)
      {
        Control.ColumnWidthChanged += new DataGridViewColumnEventHandler(Control_ColumnWidthChanged);
        Control.ColumnDisplayIndexChanged += new DataGridViewColumnEventHandler(Control_ColumnDisplayIndexChanged);
      }

      _SaveCurrentColumnAllowed = true;

      base.InitConfigHandler();
    }

    #endregion

    #region Чтение и запись настроек (общая часть)

    /// <summary>
    /// Добавляет в список категории, которые требуется читать и записывать в секции конфигурации
    /// </summary>
    /// <param name="сategories">Список категорий для заполнения</param>
    /// <param name="rwMode">Режим: Чтение или запись</param>
    /// <param name="actionInfo">Выполняемое действие</param>
    public override void GetConfigCategories(ICollection<string> сategories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      base.GetConfigCategories(сategories, rwMode, actionInfo);

      if (WantsGridConfig(rwMode, actionInfo))
        сategories.Add(EFPConfigCategories.GridConfig);

      if (WantsFiltersConfig(/*rwMode, */actionInfo))
        сategories.Add(EFPConfigCategories.Filters);

      if ((_SaveCurrentColumnAllowed && Columns.Count > 1) || // 19.05.2021 для записи "ColumnName"
        Orders.Count > 1) // для записи Order
        сategories.Add(EFPConfigCategories.GridView);
      switch (Control.SelectionMode)
      {
        case DataGridViewSelectionMode.RowHeaderSelect:
        case DataGridViewSelectionMode.ColumnHeaderSelect:
          сategories.Add(EFPConfigCategories.GridView); // для записи режима выделения
          break;
      }
    }

    private bool WantsGridConfig(EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      if (actionInfo.Purpose == EFPConfigPurpose.Composition)
      {
        return HasConfigureViewHandler && (!AlwaysUseDefaultConfig);
      }

      if (UserConfigAutoSave)
      {
        if (rwMode == EFPConfigMode.Write)
          return true;
        else if (!AlwaysUseDefaultConfig)
          return true;
      }
      return false;
    }

    private bool WantsFiltersConfig(/*EFPConfigMode rwMode, */EFPConfigActionInfo actionInfo)
    {
      if (!SaveFiltersAllowed)
        return false;

      // Убрано 02.11.2018
      // Свойство SaveFiltersAllowed по умолчанию учитывает CommandItems.CanEditFilters
      //if (!CommandItems.CanEditFilters)
      //  return false;

      return (HasFilters || actionInfo.Purpose == EFPConfigPurpose.Preload || actionInfo.Purpose == EFPConfigPurpose.Composition);
    }

    /// <summary>
    /// Устанавливает признак Changed для EFPConfigCategories.GridView
    /// </summary>
    protected override void OnSaveConfig()
    {
      if (_SaveCurrentColumnAllowed && Columns.Count > 1)
      {
        if (_SavedColumnName != CurrentColumnName)
          ConfigHandler.Changed[EFPConfigCategories.GridView] = true;
      }
      base.OnSaveConfig();
    }

    /// <summary>
    /// Разрешено ли просмотру сохранять текущий столбец в секции конфигурации "GridView"?
    /// По умолчанию - true - разрешено.
    /// Пользовательский код должен сбросить свойство в false, если сохранение нежелательно.
    /// EFPGridProducer также может сбрасывать это свойство, если в настройках просмотра задан код столбца для активации.
    /// </summary>
    public bool SaveCurrentColumnAllowed
    {
      get { return _SaveCurrentColumnAllowed; }
      set { _SaveCurrentColumnAllowed = value; }
    }
    private bool _SaveCurrentColumnAllowed;

    /// <summary>
    /// Запоминаем при открытии просмотра текущий столбец
    /// </summary>
    private string _SavedColumnName;


    /// <summary>
    /// Запись секции конфигурации
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="cfg">Записываемая секция</param>
    /// <param name="actionInfo">Выполняемое действие</param>
    public override void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      switch (category)
      {
        case EFPConfigCategories.GridConfig:
          if (WantsGridConfig(EFPConfigMode.Write, actionInfo) && CurrentConfig != null)
          {
            EFPDataGridViewConfig GridConfig2 = CurrentConfig.Clone(this); // с учетом реальных размеров столбцов
            GridConfig2.WriteConfig(cfg);
          }
          break;

        case EFPConfigCategories.Filters:
          if (WantsFiltersConfig(/*EFPConfigMode.Write, */actionInfo))
            Filters.WriteConfig(cfg);
          break;

        case EFPConfigCategories.GridView:
          if (_SaveCurrentColumnAllowed && Columns.Count > 1)
          {
            cfg.SetString("ColumnName", CurrentColumnName); // 19.05.2021
            _SavedColumnName = CurrentColumnName; // чтобы второй раз не вызывалось
          }
          WriteSelectionMode(cfg); // режим выбора целой строки

          if (CustomOrderAllowed)
            cfg.SetBool("CustomOrderActive", CustomOrderActive); // Независимо от Orders.Count

          if (CustomOrderActive)
            cfg.SetString("CustomOrder", CurrentOrderName);
          else if (Orders.Count > 1)
            cfg.SetString("Order", CurrentOrderName);
          break;
      }
      base.WriteConfigPart(category, cfg, actionInfo);
    }

    /// <summary>
    /// Чтение секции конфигурации
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="cfg">Секция конфигурации</param>
    /// <param name="actionInfo">Выполняемое действие</param>
    public override void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      switch (category)
      {
        case EFPConfigCategories.GridConfig:
          if (WantsGridConfig(EFPConfigMode.Read, actionInfo))
            ReadConfigPartGridConfig(cfg);
          break;

        case EFPConfigCategories.Filters:
          if (WantsFiltersConfig(/*EFPConfigMode.Read, */actionInfo))
          {
            if (actionInfo.Purpose == EFPConfigPurpose.Composition)
              Filters.ClearAllFilters(); // 25.12.2018 - в композиции фильтры задаются
            Filters.ReadConfig(cfg);
            CallOnFilterChanged();
          }
          break;

        case EFPConfigCategories.GridView:
          if (_SaveCurrentColumnAllowed && Columns.Count > 1)
          {
            string sColumnName = cfg.GetString("ColumnName");
            if (Columns.IndexOf(sColumnName) >= 0)
            {
              CurrentColumnName = sColumnName; // 19.05.2021
              _SavedColumnName = sColumnName;
            }
            else
              _SavedColumnName = null; // как будто и не читали
          }
          ReadSelectionMode(cfg); // режим выбора целой строки

          if (CustomOrderAllowed)
            CustomOrderActive = cfg.GetBoolDef("CustomOrderActive", CustomOrderActive); // 06.07.2021. Если параметр не сохранен, то сохраняем существующее значение.
          if (CustomOrderActive)
            CurrentOrderName = cfg.GetString("CustomOrder");
          else if (Orders.Count > 1)
            CurrentOrderName = cfg.GetString("Order");
          break;

      }
      base.ReadConfigPart(category, cfg, actionInfo);
    }

    /// <summary>
    /// Вызывается при первой загрузке конфигурации элемента перед вызовом LoadConfig().
    /// </summary>
    protected override void OnBeforeLoadConfig()
    {
      base.OnBeforeLoadConfig();
      _UserConfigAutoSave = GetUserConfigAutoSave();
    }

    /// <summary>
    /// Вызывается при первой загрузке конфигурации элемента.
    /// </summary>
    protected override void OnLoadConfig()
    {
      if (HasFilters)
      {
        _DefaultFilterCfg = new TempCfg();
        if (!Filters.IsEmpty)
          Filters.WriteConfig(_DefaultFilterCfg);

        Filters.SetReadOnly();
      }

      base.OnLoadConfig();

      if (CurrentConfig == null && GridProducer != null)
        //OnInitDefaultGridConfig();
        InitDefaultGridConfig(); // 30.09.2018
    }

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Команды локального меню
    /// </summary>
    public new EFPConfigurableDataGridViewCommandItems CommandItems { get { return (EFPConfigurableDataGridViewCommandItems)(base.CommandItems); } }

    /// <summary>
    /// Создает объект EFPConfigurableDataGridViewCommandItems
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      return new EFPConfigurableDataGridViewCommandItems(this);
    }

    /// <summary>
    /// Добавляет обработчик события для формирования таблички фильтров при печати
    /// </summary>
    protected override void OnCommandItemsCreated()
    {
      base.OnCommandItemsCreated();
      DefaultOutItem.TitleNeeded += DefaultOutItem_TitleNeeded;
    }

    private void DefaultOutItem_TitleNeeded(object sender, EventArgs args)
    {
      Reporting.BRDataGridViewMenuOutItem outItem = (Reporting.BRDataGridViewMenuOutItem)sender;
      if (HasFilters)
      {
        foreach (IEFPGridFilter filter in Filters)
        {
          if (!filter.IsEmpty)
            outItem.FilterInfo.Add(filter.DisplayName, filter.FilterText);
        }
      }
    }

    #endregion

    #region GridProducer

    /// <summary>
    /// Имя фиксированной настройки табличного просмотра. 
    /// Используется EFPGridProducer.InitGridView(), когда свойство EFPDataGridView.CurrentConfig не установлено.
    /// 
    /// Именные настройки хранятся в EFPGridProducer.Configs. Обычно, когда свойство не установлено,
    /// используется основная настройка EFPGridProducer.DefaultConfig.
    /// </summary>
    public string DefaultConfigName
    {
      get { return _DefaultConfigName; }
      set
      {
        if (CurrentConfigHasBeenSet)
          throw new InvalidOperationException("Свойство DefaultConfigName должно устанавливаться до CurrentGridConfig");
        _DefaultConfigName = value;
      }
    }
    private string _DefaultConfigName;

    #endregion

    #region Обработчики изменения столбцов в табличном просмотре

    void Control_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs args)
    {
      try
      {
        if (args.Column.AutoSizeMode != DataGridViewAutoSizeColumnMode.Fill)
          ConfigHandler.Changed[EFPConfigCategories.GridConfig] = true;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Control_ColumnWidthChanged");
      }
    }

    void Control_ColumnDisplayIndexChanged(object sender, DataGridViewColumnEventArgs args)
    {
      try
      {
        ConfigHandler.Changed[EFPConfigCategories.GridConfig] = true;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Control_ColumnDisplayIndexChanged");
      }
    }

    #endregion

    #region Настройка просмотра GridConfig


    // TODO: Может быть стоит сделать более сложное событие

    /// <summary>
    /// Событие вызывается при выполнении команды меню "Настройка просмотра".
    /// Если обработчик не установлен, показывается диалог, использующий интерфейс IEFPConfigurableGridProducer.
    /// Свойство Cancel влияет на результат, возвращаемый методом ShowConfigDialog()
    /// </summary>
    public event CancelEventHandler ConfigureView;

    /// <summary>
    /// Свойство возвращает true, если есть обработчик события ConfigureView или можно использовать интерфейс IEFPConfigurableGridProducer.
    /// Если переопределен метод OnConfigureView() должно быть переопределено и это свойство
    /// </summary>
    public bool HasConfigureViewHandler
    {
      get
      {
        if (ConfigureView != null)
          return true;
        return (!String.IsNullOrEmpty(ConfigSectionName)) &&
          (GridProducer is IEFPConfigurableGridProducer);
      }
    }

    /// <summary>
    /// Открывает диалог настройки конфигурации табличного просмотра
    /// </summary>
    /// <returns>true, если конфигурация была изменена</returns>
    public bool ShowConfigDialog()
    {
      if (ConfigureView != null)
      {
        CancelEventArgs args = new CancelEventArgs();
        ConfigureView(this, args);
        return !args.Cancel;
      }

      if (String.IsNullOrEmpty(ConfigSectionName))
        return false;

      IEFPConfigurableGridProducer gridProducer2 = GridProducer as IEFPConfigurableGridProducer;
      if (gridProducer2 == null)
        return false;

      if (CurrentConfig == null)
        return false; // ??

      EFPDataGridViewConfigDialog dlg = new EFPDataGridViewConfigDialog();
      dlg.CallerControlProvider = this;
      dlg.GridProducer = gridProducer2;
      dlg.ConfigCategory = EFPConfigCategories.GridConfig;
      dlg.HistoryCategory = EFPConfigCategories.GridConfigHistory;
      //dlg.Value = CurrentConfig;
      dlg.Value = CurrentConfig.Clone(this); // 16.06.2021
      if (dlg.ShowDialog() != DialogResult.OK)
        return false;

      try
      {
        CurrentConfig = dlg.Value;
      }
      catch (Exception e)
      {
        e.Data["EFPConfigurableGridView.DisplayName"] = DisplayName;
        try
        {
          TempCfg cfg = new TempCfg();
          dlg.Value.WriteConfig(cfg);
          e.Data["EFPDataGridViewConfig.AsXmlText"] = cfg.AsXmlText;
        }
        catch { }
        throw;
      }
      ConfigHandler.Changed[EFPConfigCategories.GridConfig] = true;

      return true;
    }

    /// <summary>
    /// Возвращает true, если при открытии / закрытии просмотра сохраняется последняя пользовательская 
    /// конфигурация.
    /// До показа просмотра на экране свойство возвращает значение, зависящее от других свойств.
    /// При показе таблицы значение фиксируется
    /// </summary>
    public bool UserConfigAutoSave
    {
      get
      {
        if (LoadConfigCalled)
          return _UserConfigAutoSave;
        else
          return GetUserConfigAutoSave();
      }
    }
    private bool _UserConfigAutoSave;

    private bool GetUserConfigAutoSave()
    {
      if (String.IsNullOrEmpty(ConfigSectionName))
        return false;
      if (CurrentConfig != null)
        return false; // если конфигурация инициализирована в конструкторе производного класса, путь он и занимается ее сохранением

      return GridProducer is IEFPConfigurableGridProducer;
    }

    private void InitDefaultGridConfig()
    {
      if (base.InsideSetCurrentConfig)
        throw new BugException();
      base.InsideSetCurrentConfig = true;
      try
      {
        OnInitDefaultGridConfig();
      }
      finally
      {
        base.InsideSetCurrentConfig = false;
      }
    }

    /// <summary>
    /// Вызывается из OnShown(), если предполагается загрузка последней пользовательской конфигурации просмотра,
    /// но просмотр открывается впервые и нет сохраненной конфигурации.
    /// Также метод вызывается в случае ошибки загрузки.
    /// Метод должен установить свойство CurrentConfig
    /// </summary>
    protected virtual void OnInitDefaultGridConfig()
    {
      if (GridProducer != null)
      {
        GridProducer.InitGridView(this, false);
        PerformGridProducerPostInit();
      }
    }

    /// <summary>
    /// Если свойство установлено в true, то при открытии просмотра будет всегда использоваться
    /// настройка просмотра по умолчанию, а не последняя сохраненная конфигурация.
    /// При этом сохранение настроек работает как обычно. Пользователь, в частности, может выбрать
    /// последнюю сохраненную конфигурацию или любую другую из истории.
    /// Свойство действует, если UserConfigAutoSave возвращает true.
    /// Установка свойства разрешается только до показа просмотра на экране
    /// </summary>
    public bool AlwaysUseDefaultConfig
    {
      get { return _AlwaysUseDefaultConfig; }
      set
      {
        CheckHasNotBeenCreated();
        _AlwaysUseDefaultConfig = value;
      }
    }
    private bool _AlwaysUseDefaultConfig;

    private void ReadConfigPartGridConfig(CfgPart cfg)
    {
      if (cfg.IsEmpty)
        InitDefaultGridConfig();
      else
      {
        try
        {
          EFPDataGridViewConfig gridConfig = new EFPDataGridViewConfig();
          gridConfig.ReadConfig(cfg);
          gridConfig.SetReadOnly();
          base.CurrentConfig = gridConfig;
        }
        catch (Exception e)
        {
          try
          {
            e.Data["CfgPart.ToString()"] = cfg.ToString();
            e.Data["CfgPart.GetXmlText()"] = cfg.GetXmlText();
          }
          catch { }
          EFPApp.ShowException(e, "Ошибка загрузки сохраненной конфигурации табличного просмотра");
          InsideSetCurrentConfig = true; // 07.07.2017
          try
          {
            OnInitDefaultGridConfig();
          }
          catch { }
          InsideSetCurrentConfig = false;
        }
      }
      if (base.CurrentConfig == null)
        throw new BugException("Метод InitDefaultGridConfig() не установил свойство CurrentConfig");
    }

    #endregion

    #region Фильтры

    /// <summary>
    /// Коллекция фильтров для табличного просмотра. Если есть хотя бы один фильтр,
    /// то в локальном меню появляется команда "Фильтр". После установки пользователем
    /// фильтра вызывается обновление просмотра. Ответственность за обработку
    /// фильтров лежит на вызывающей программе
    /// Чтобы проверить наличие возможных фильтров, следует использовать свойство
    /// HasFilters, которое позволяет избежать создания лишних объектов
    /// </summary>
    public IEFPGridFilters Filters
    {
      get
      {
        if (_Filters == null)
          _Filters = CreateGridFilters();
        return _Filters;
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Filters = value;
        PerformFilterChanged();
      }
    }

    private IEFPGridFilters _Filters;

    /// <summary>
    /// Создает объект фильтров табличного просмотра.
    /// Непереопределенный метод создает EFPDummyGridFilters.
    /// </summary>
    /// <returns></returns>
    protected virtual IEFPGridFilters CreateGridFilters()
    {
      return new EFPDummyGridFilters();
    }


    /// <summary>
    /// Возвращает true, если Filter.Count больше 0. 
    /// Не создает дополнительный объект.
    /// Имеется в виду наличие любых добавленных фильтров, а не только тех, которые реально установлены.
    /// </summary>
    public bool HasFilters
    {
      get
      {
        if (_Filters == null)
          return false;
        else
          return _Filters.Count > 0;
      }
    }

    /// <summary>
    /// Вызывается после изменения фильтра пользователем
    /// </summary>
    public event EventHandler AfterSetFilter;

    /// <summary>
    /// Вызов диалога установки фильтра и обновление просмотра по необходимости
    /// </summary>
    public bool ShowFilterDialog()
    {
      return ShowFilterDialog(String.Empty);
    }

    /// <summary>
    /// Вызов диалога установки фильтра и обновление просмотра по необходимости
    /// При показе таблицы фильтров активируется строка фильтра с заданным именем.
    /// Если <paramref name="startFilter "/> не задан, активируется строка для первого непустого фильтра
    /// </summary>
    /// <param name="startFilter">Активируемый фильтр</param>
    public bool ShowFilterDialog(string startFilter)
    {
      LoadConfig();

      if (!HasFilters)
      {
        EFPApp.ErrorMessageBox("Табличный просмотр не содержит фильтров");
        return false;
      }

      // Сохраняем существующие данные на случай нажатия кнопки "Отмена"
      TempCfg originalConfigSection = new TempCfg();
      Filters.WriteConfig(originalConfigSection);

      bool res;
      Filters.BeginUpdate();
      try
      {
        GridFilterForm form = new GridFilterForm(this, _DefaultFilterCfg, originalConfigSection);
        form.SetStartFilter(startFilter);
        if (EFPApp.ShowDialog(form, true) == DialogResult.OK)
        {
          // 04.10.2017
          // Вызываем виртуальный метод
          CallOnFilterChanged();

          res = true;
        }
        else
        {
          // Выполняем откат редактирования
          Filters.ClearAllFilters();
          Filters.ReadConfig(originalConfigSection);
          res = false;
        }
      }
      finally
      {
        Filters.EndUpdate();
      }

      return res;
    }

    /// <summary>
    /// Вызывается для дополнительной инициализации табличного просмотра в редакторе фильтров
    /// Переопределеннный метод может, например, инициализировать дополнительные команды меню
    /// </summary>
    /// <param name="filterGridProvider">Обработчик таблицы фильтров</param>
    public virtual void InitGridFilterEditorGridView(EFPGridFilterEditorGridView filterGridProvider)
    {
    }

    /// <summary>
    /// Свойство возвращает true, если табличный просмотр выполняет чтение и запись фильтров в
    /// секцию конфигурации.
    /// Если свойство не установлено в явном виде, то возвращается значение CommandItems.CanEditFilters
    /// Свойство может устанавливаться только до вывода просмотра на экран
    /// </summary>
    public bool SaveFiltersAllowed
    {
      get { return _SaveFiltersAllowed ?? CommandItems.CanEditFilters; }
      set
      {
        CheckHasNotBeenCreated();
        _SaveFiltersAllowed = value;
      }
    }
    private bool? _SaveFiltersAllowed;

    /// <summary>
    /// Обновление просмотра при внешней установке значений фильтров. Идентично
    /// PerformSetFilter(), но диалог не выводится
    /// </summary>
    public void PerformFilterChanged()
    {
      CallOnFilterChanged();
    }

    /// <summary>
    /// Свойство возвращает true, если в данный момент вызывается метод OnFilterChanged.
    /// Свойство может быть проанализировано в обработчике события RefreshData, чтобы определить
    /// причину вызова
    /// </summary>
    public bool InsideFilterChanged { get { return _InsideFilterChanged; } }
    private bool _InsideFilterChanged;

    private void CallOnFilterChanged()
    {
      if (_InsideFilterChanged)
        return;

      _InsideFilterChanged = true;
      try
      {
        OnFilterChanged();
      }
      finally
      {
        _InsideFilterChanged = false;
      }
    }

    /// <summary>
    /// Вызывается при изменении фильтров.
    /// Непереопределенный метод вызывает событие AfterSetFilter и инициирует записбь
    /// </summary>
    protected virtual void OnFilterChanged()
    {
      // Вызываем пользовательский блок
      if (AfterSetFilter != null)
        AfterSetFilter(this, EventArgs.Empty);

      if (HasBeenCreated)
      {
        // Запоминаем текущие настройки фильтров
        ConfigHandler.Changed[EFPConfigCategories.Filters] = true;
        // Обновляем табличный просмотр
        PerformRefresh();
      }
    }


    /// <summary>
    /// Когда табличный просмотр первый раз выводится на экран, сохраняем сюда
    /// настройки фильтров "по умолчанию", то есть те, которые выполнены прикладным
    /// модулем. 
    /// </summary>
    private TempCfg _DefaultFilterCfg;

    #endregion

    #region Порядок строк

    /// <summary>
    /// Вызывается после установки нового значения свойства 
    /// CurrentOrderIndex (или CurrentOrder / CurrentOrderName).
    /// Переопределенный метод инициирует запись секции конфигурации категории "GridView"
    /// </summary>
    /// <param name="args">Не используется</param>
    protected override void OnCurrentOrderChanged(EventArgs args)
    {
      base.OnCurrentOrderChanged(args); // вызов пользовательского события

      ConfigHandler.Changed[EFPConfigCategories.GridView] = true;
    }

    #endregion

    #region Режим выделения целых строк или столбцов


    private void WriteSelectionMode(CfgPart cfg)
    {
      switch (Control.SelectionMode)
      {
        case DataGridViewSelectionMode.RowHeaderSelect:
          cfg.SetBool("WholeRowSelection", Control.SelectedRows.Count > 0);
          break;
        case DataGridViewSelectionMode.ColumnHeaderSelect:
          cfg.SetBool("WholeColumnSelection", Control.SelectedColumns.Count > 0);
          break;
      }
    }

    private void ReadSelectionMode(CfgPart cfg)
    {
      _SavedWholeRowSelection = cfg.GetBool("WholeRowSelection");
      _SavedWholeColumnSelection = cfg.GetBool("WholeColumnSelection");
      _DelayedSavedSelection = true;
      if (Control.RowCount > 0)
        FlushDelayedSavedSelection();
    }

    //protected override void OnDataBindingComplete(DataGridViewBindingCompleteEventArgs Args)
    //{
    //  base.OnDataBindingComplete(Args);
    //  FlushDelayedSavedSelection();
    //}

    /// <summary>
    /// Вызывается при изменении текущей выбранной ячейки.
    /// </summary>
    protected override void OnCurrentCellChanged()
    {
      base.OnCurrentCellChanged();

      if (Control.CurrentCell != null)
        FlushDelayedSavedSelection();
    }

    private bool _DelayedSavedSelection;
    private bool _SavedWholeRowSelection;
    private bool _SavedWholeColumnSelection;

    private void FlushDelayedSavedSelection()
    {
      if (!_DelayedSavedSelection)
        return;
      _DelayedSavedSelection = false; // в том числе предотвращается и рекурсивный вызов
      try
      {
        switch (Control.SelectionMode)
        {
          case DataGridViewSelectionMode.RowHeaderSelect:
            if (_SavedWholeRowSelection && Control.RowCount > 0 && Control.SelectedRows.Count == 0 && CurrentRowIndex >= 0)
              Control.Rows[CurrentRowIndex].Selected = true;
            break;
          case DataGridViewSelectionMode.ColumnHeaderSelect:
            if (_SavedWholeColumnSelection && Control.ColumnCount > 0 && Control.SelectedColumns.Count == 0 && CurrentColumnIndex >= 0)
              Control.Columns[CurrentColumnIndex].Selected = true;
            break;
        }
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e, "Ошибка восстановления режима выделения целой строки / столбца");
      }
    }


    #endregion
  }

  /// <summary>
  /// Команды локального меню для табличного просмотра EFPConfigurableDataGridView.
  /// Добавляет команды настройки табличного просмотра и подменю "Фильтр"
  /// </summary>
  public class EFPConfigurableDataGridViewCommandItems : EFPDataGridViewCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает команды
    /// </summary>
    /// <param name="owner">Провайдер управляющего элемента</param>
    public EFPConfigurableDataGridViewCommandItems(EFPConfigurableDataGridView owner)
      : base(owner)
    {
      #region Исходные значения свойств

      CanEditFilters = true;

      #endregion

      #region Создание команд

      #region Настройка просмотра

      ciEditConfig = new EFPCommandItem("View", "GridConfig");
      ciEditConfig.MenuText = "Настройка просмотра";
      ciEditConfig.ImageKey = "EditGridConfig";
      ciEditConfig.Click += new EventHandler(ciEditConfig_Click);
      // пусть будет везде ciEditConfig.Usage = EFPCommandItemUsage.Menu;
      Add(ciEditConfig);

      #endregion

      #region Фильтр

      _MenuFilter = new EFPCommandItem("View", "MenuFilter");
      _MenuFilter.MenuText = "&Фильтр";
      _MenuFilter.ImageKey = "Filter";
      _MenuFilter.Usage = EFPCommandItemUsage.Menu;
      Add(_MenuFilter);

      //if (HasMainMenu)
      //  ciSetFilter = new EFPCommandItem(AccDepMainMenu.Filter);
      //else
      //{
      ciSetFilter = new EFPCommandItem("View", "Filter");
      ciSetFilter.ShortCut = Keys.F7;
      ciSetFilter.ImageKey = "Filter";
      //}
      ciSetFilter.Parent = _MenuFilter;
      ciSetFilter.MenuText = "Установить фильтр";
      ciSetFilter.Click += new EventHandler(ciSetFilter_Click);
      Add(ciSetFilter);

      ciScrollFilterPrev = new EFPCommandItem("View", "PrevDateFilter");
      ciScrollFilterPrev.Parent = _MenuFilter;
      ciScrollFilterPrev.MenuText = "Предыдущий период времени";
      ciScrollFilterPrev.ShortCut = Keys.Alt | Keys.Up;
      ciScrollFilterPrev.ImageKey = "ArrowUp";
      ciScrollFilterPrev.GroupBegin = true;
      ciScrollFilterPrev.Usage = EFPCommandItemUsage.None;
      ciScrollFilterPrev.Click += new EventHandler(ciScrollFilterPrev_Click);
      Add(ciScrollFilterPrev);

      ciScrollFilterNext = new EFPCommandItem("View", "NextDateFilter");
      ciScrollFilterNext.Parent = _MenuFilter;
      ciScrollFilterNext.MenuText = "Следующий период времени";
      ciScrollFilterNext.ShortCut = Keys.Alt | Keys.Down;
      ciScrollFilterNext.ImageKey = "ArrowDown";
      ciScrollFilterNext.GroupEnd = true;
      ciScrollFilterNext.Usage = EFPCommandItemUsage.None;
      ciScrollFilterNext.Click += new EventHandler(ciScrollFilterNext_Click);
      Add(ciScrollFilterNext);

      AddSeparator();

      #endregion

      #endregion
    }

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public new EFPConfigurableDataGridView ControlProvider { get { return (EFPConfigurableDataGridView)(base.ControlProvider); } }

    #endregion

    #region Управление наличием команд

    /// <summary>
    /// True (по умолчанию), если пользователь может редактировать фильтры
    /// False, если фильтры не могут быть изменены пользователем
    /// </summary>
    public bool CanEditFilters { get { return _CanEditFilters; } set { _CanEditFilters = value; } }
    private bool _CanEditFilters;

    /// <summary>
    /// Возвращает true, если есть хотя бы один прокручиваемый фильтр
    /// (даже если в данный момент фильтр не установлен и прокрутка недоступна)
    /// </summary>
    public bool HasScrollableFilter
    {
      get
      {
        if (ControlProvider.HasFilters)
        {
          for (int i = 0; i < ControlProvider.Filters.Count; i++)
          {
            if (ControlProvider.Filters[i] is IEFPScrollableGridFilter)
              return true;
          }
        }
        return false;
      }
    }

    /// <summary>
    /// Возвращает активный прокручиваемый фильтр (с установленным значением, когда
    /// фильтр можно прокручивать)
    /// Если нет прокручиваемых фильтров или ни один из них не установлен, возвращается null
    /// </summary>
    public IEFPScrollableGridFilter ActiveScrollableFilter
    {
      get
      {
        if (ControlProvider.HasFilters)
        {
          for (int i = 0; i < ControlProvider.Filters.Count; i++)
          {
            if (ControlProvider.Filters[i] is IEFPScrollableGridFilter)
            {
              IEFPScrollableGridFilter f = (IEFPScrollableGridFilter)(ControlProvider.Filters[i]);
              if (f.CanScrollUp || f.CanScrollDown)
                return f;
            }
          }
        }
        return null;
      }
    }

    /// <summary>
    /// Фильтр по дате. Если свойство установлено, то в подменю "Фильтр"
    /// обрабатываются клавиши Ctrl-СтрелкаВверх и Ctrl-СтрелкаВниз для переключаения
    /// между периодами дат
    /// </summary>
    public IEFPScrollableGridFilter ScrollableFilter
    {
      get { return _ScrollableFilter; }
      set { _ScrollableFilter = value; }
    }
    private IEFPScrollableGridFilter _ScrollableFilter;

    #endregion

    #region Обновление команд

    /// <summary>
    /// Установка свойств EFPCommandItem.Usage
    /// </summary>
    protected override void OnPrepare()
    {
      base.OnPrepare();

      if (!ControlProvider.HasConfigureViewHandler)
        ciEditConfig.Usage = EFPCommandItemUsage.None;

      if (!ControlProvider.HasFilters)
      {
        ciSetFilter.Usage = EFPCommandItemUsage.None;
        if (_MenuFilter.MenuChildrenCount == 0)
          _MenuFilter.Usage = EFPCommandItemUsage.None;
      }
      else
        ciSetFilter.Visible = CanEditFilters;

      if (HasScrollableFilter && ciSortMoveDown.Usage == EFPCommandItemUsage.None)
      {
        ciScrollFilterPrev.Usage = EFPCommandItemUsage.Everywhere;
        ciScrollFilterNext.Usage = EFPCommandItemUsage.Everywhere;

        ciScrollFilterPrev.Visible = CanEditFilters; //28.05.2015
        ciScrollFilterNext.Visible = CanEditFilters;
      }

      if (_MenuFilter.Usage != EFPCommandItemUsage.None)
        _MenuFilter.InitMenuVisible(); // 09.07.2019
    }

    /// <summary>
    /// Установка свойств EFPCommandItem.Enabled
    /// </summary>
    protected override void DoRefreshItems()
    {
      base.DoRefreshItems();

      IEFPScrollableGridFilter sf = ActiveScrollableFilter; // чтобы меньше поиска
      if (sf != null)
      {
        ciScrollFilterPrev.Enabled = sf.CanScrollUp && CanEditFilters;
        ciScrollFilterNext.Enabled = sf.CanScrollDown && CanEditFilters;
        ciScrollFilterPrev.ToolTipText = ciScrollFilterPrev.MenuText + " для фильтра \"" + ((IEFPGridFilter)sf).DisplayName + "\"";
        ciScrollFilterNext.ToolTipText = ciScrollFilterNext.MenuText + " для фильтра \"" + ((IEFPGridFilter)sf).DisplayName + "\"";
      }
      else
      {
        ciScrollFilterPrev.Enabled = false;
        ciScrollFilterNext.Enabled = false;
        ciScrollFilterPrev.ToolTipText = ciScrollFilterPrev.MenuText + " (нет активного фильтра)";
        ciScrollFilterNext.ToolTipText = ciScrollFilterNext.MenuText + " (нет активного фильтра)";
      }
    }

    #endregion

    #region "Настройка просмотра"

    private EFPCommandItem ciEditConfig;

    private void ciEditConfig_Click(object sender, EventArgs args)
    {
      ControlProvider.ShowConfigDialog();
    }

    #endregion

    #region Команды фильтра

    /// <summary>
    /// Подменю "Фильтр". Может использоваться для добавления собственных команд
    /// </summary>
    public EFPCommandItem MenuFilter { get { return _MenuFilter; } }
    private EFPCommandItem _MenuFilter;

    private EFPCommandItem ciSetFilter;

    private EFPCommandItem ciScrollFilterNext, ciScrollFilterPrev;

    void ciSetFilter_Click(object sender, EventArgs args)
    {
      ControlProvider.ShowFilterDialog();
    }

    void ciScrollFilterPrev_Click(object sender, EventArgs args)
    {
      DoScrollFilter(false);
    }

    void ciScrollFilterNext_Click(object sender, EventArgs args)
    {
      DoScrollFilter(true);
    }

    private void DoScrollFilter(bool forward)
    {
      if (ActiveScrollableFilter == null)
        return;
      if (!(forward ? ActiveScrollableFilter.CanScrollDown : ActiveScrollableFilter.CanScrollUp))
      {
        EFPApp.MessageBox("Прокрутка фильтра \"" + ((IEFPGridFilter)ActiveScrollableFilter).DisplayName + "\" невозможна");
        return;
      }

      if (forward)
        ActiveScrollableFilter.ScrollDown();
      else
        ActiveScrollableFilter.ScrollUp();

      ControlProvider.PerformFilterChanged();
    }

    #endregion

    #region "Отправить"

#if OLD_OUT
    /// <summary>
    /// Переопределение команды "Отправить"-"Microsoft Excel"
    /// с сохранением параметров между вызовами в секции конфигурации.
    /// </summary>
    protected override void OnSendToMicrosoftExcel()
    {
      ReadSendToSettings(SendToSettings);

      EFPDataGridViewExpExcelForm form = new EFPDataGridViewExpExcelForm();
      form.Text = "Отправить в " + ciSendToMicrosoftExcel.MenuTextWithoutMnemonic;
      form.Icon = EFPApp.MainImages.Icons[ciSendToMicrosoftExcel.ImageKey];
      form.LoadValues(SendToSettings);
      form.InitVisibility(ControlProvider);
      if (EFPApp.ShowDialog(form, true) != DialogResult.OK)
        return;

      form.SaveValues(SendToSettings);
      WriteSendToSettings(SendToSettings);

      ControlProvider.SendToMicrosoftExcel(SendToSettings);
    }

    /// <summary>
    /// Переопределение команды "Отправить"-"OpenOffice/LibreOffice Calc"
    /// с сохранением параметров между вызовами в секции конфигурации.
    /// </summary>
    protected override void OnSendToOpenOfficeCalc()
    {
      ReadSendToSettings(SendToSettings);

      EFPDataGridViewExpExcelForm form = new EFPDataGridViewExpExcelForm();
      form.Text = "Отправить в " + ciSendToOpenOfficeCalc.MenuTextWithoutMnemonic;
      form.Icon = EFPApp.MainImages.Icons[ciSendToOpenOfficeCalc.ImageKey];
      form.LoadValues(SendToSettings);
      form.InitVisibility(ControlProvider);
      if (EFPApp.ShowDialog(form, true) != DialogResult.OK)
        return;

      form.SaveValues(SendToSettings);
      WriteSendToSettings(SendToSettings);

      ControlProvider.SendToOpenOfficeCalc(SendToSettings);
    }

    /// <summary>
    /// Чтение настроек выполняется из одной из двух секций
    /// </summary>
    /// <param name="settings"></param>
    private void ReadSendToSettings(EFPDataGridViewExpExcelSettings settings)
    {
      CfgPart cfg;

      if ((!String.IsNullOrEmpty(ControlProvider.ConfigSectionName)) && EFPConfigTools.IsPersist(ControlProvider.ConfigManager.Persistence))
      {
        EFPConfigSectionInfo configInfo1 = new EFPConfigSectionInfo(ControlProvider.ConfigSectionName,
          EFPConfigCategories.SendToExcel, String.Empty);
        using (ControlProvider.ConfigManager.GetConfig(configInfo1, EFPConfigMode.Read, out cfg))
        {
          if (!cfg.IsEmpty)
          {
            settings.ReadConfig(cfg);
            return;
          }
        }
      }

      EFPConfigSectionInfo configInfo2 = new EFPConfigSectionInfo("Default",
        EFPConfigCategories.SendToExcel, String.Empty);
      using (ControlProvider.ConfigManager.GetConfig(configInfo2, EFPConfigMode.Read, out cfg))
      {
        settings.ReadConfig(cfg);
      }
    }

    /// <summary>
    /// Запись настроек выполняется в две секции
    /// </summary>
    /// <param name="settings"></param>
    private void WriteSendToSettings(EFPDataGridViewExpExcelSettings settings)
    {
      CfgPart cfg;

      if ((!String.IsNullOrEmpty(ControlProvider.ConfigSectionName)) && EFPConfigTools.IsPersist(ControlProvider.ConfigManager.Persistence))
      {
        EFPConfigSectionInfo configInfo1 = new EFPConfigSectionInfo(ControlProvider.ConfigSectionName,
          EFPConfigCategories.SendToExcel, String.Empty);

        using (ControlProvider.ConfigManager.GetConfig(configInfo1, EFPConfigMode.Write, out cfg))
        {
          settings.WriteConfig(cfg);
        }
      }

      // В секцию по умолчанию запись выполняется всегда

      EFPConfigSectionInfo configInfo2 = new EFPConfigSectionInfo("Default",
        EFPConfigCategories.SendToExcel, String.Empty);

      using (ControlProvider.ConfigManager.GetConfig(configInfo2, EFPConfigMode.Write, out cfg))
      {
        settings.WriteConfig(cfg);
      }
    }
#endif

    #endregion
  }

  /// <summary>
  /// Хранение информации об установленных фильтрах в буфере обмена
  /// Объект содержит список фильтров (коды) и XML-строку с установленными значениями
  /// </summary>
  [Serializable]
  public class FilterClipboardInfo
  {
    #region Конструктор

    /// <summary>
    /// Создает заполненный объект.
    /// Установленные свойства нельзя изменить в дальнейшем.
    /// </summary>
    /// <param name="dbIdentity">Идентификатор базы данных для предотвращения вставки 
    /// в постороннюю программу. См. описание свойства IEFPGridFilters.DBIdentity.
    /// Может быть пустая строка, если фильтр не привязан к базе данных.
    /// </param>
    /// <param name="names">Список имен фильтров, которые входят в копируемый набор.
    /// См. описание свойства IEFPGridFilter.Name</param>
    /// <param name="xmlText">Данные фильтров в XML-формате</param>
    public FilterClipboardInfo(string dbIdentity, string[] names, string xmlText)
    {
      // Может быть пусто
      if (dbIdentity == null)
        dbIdentity = String.Empty;
      //if (String.IsNullOrEmpty(DBIdentity))
      //  throw new ArgumentNullException("DBIdentity");

      _DBIdentity = dbIdentity;
      _Names = names;
      _XmlText = xmlText;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Идентификатор набора данных
    /// Может быть пустая строка, если фильтр не привязан к базе данных.
    /// См. описание свойства IEFPGridFilters.DBIdentity.
    /// </summary>
    public string DBIdentity { get { return _DBIdentity; } }
    private string _DBIdentity;

    /// <summary>
    /// Список имен фильтров, которые входят в копируемый набор.
    /// См. описание свойства IEFPGridFilter.Name    
    /// </summary>
    public string[] Names { get { return _Names; } }
    private string[] _Names;

    /// <summary>
    /// Данные фильтров в XML-формате.
    /// Копируемые фильтры записываются во временную секцию конфигурации.
    /// Затем секция записывается в XML-документ. XML-документ преобразуется в строку.
    /// </summary>
    public string XmlText { get { return _XmlText; } }
    private string _XmlText;

    #endregion
  }
}
