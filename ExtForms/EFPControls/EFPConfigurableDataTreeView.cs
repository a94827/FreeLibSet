// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Controls;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Расширение иерархического просмотра для работы с фильтрами и возможностью настройки столбцов с помощью <see cref="EFPGridProducer"/>.
  /// </summary>
  public class EFPConfigurableDataTreeView : EFPDataTreeView, IEFPControlWithFilters
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPConfigurableDataTreeView(EFPBaseProvider baseProvider, TreeViewAdv control)
      : base(baseProvider, control)
    {
      Init();
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    public EFPConfigurableDataTreeView(IEFPControlWithToolBar<TreeViewAdv> controlWithToolBar)
      : base(controlWithToolBar)
    {
      Init();
    }

    private void Init()
    {
      //Control.ColumnWidthChanged += new DataGridViewColumnEventHandler(Grid_ColumnWidthChanged);
      //Control.ColumnDisplayIndexChanged += new DataGridViewColumnEventHandler(Grid_ColumnDisplayIndexChanged);
      Control.ColumnWidthChanged += Control_ColumnWidthChanged;
      Control.ColumnReordered += Control_ColumnReordered;

      base.InitConfigHandler();
    }

    /*
protected override void OnCurrentCellChanged()
{
base.OnCurrentCellChanged();

// Обработка перемещений для установки видимости команд локального меню
CommandItems.PerformRefreshItems();
}

 */
    #endregion

    #region Чтение и запись настроек (общая часть)

    /// <summary>
    /// Заполняет список категорий, для которых испольщуются секции конфигурации..
    /// Добавляет категории <see cref="EFPConfigCategories.TreeView"/>, <see cref="EFPConfigCategories.TreeConfig"/> и <see cref="EFPConfigCategories.Filters"/>, в зависимости от наличия фильтров и возможности настройки столбцов
    /// </summary>
    /// <param name="categories">Список категорий для заполнения</param>
    /// <param name="rwMode">Режим чтения или записи</param>
    /// <param name="actionInfo">Описание действия</param>
    public override void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      base.GetConfigCategories(categories, rwMode, actionInfo);

      if (WantsTreeConfig(rwMode, actionInfo))
        categories.Add(EFPConfigCategories.TreeConfig);

      if (WantsFiltersConfig(actionInfo))
        categories.Add(EFPConfigCategories.Filters);

      if (Orders.Count > 1)
        categories.Add(EFPConfigCategories.TreeView); // для записи Order
    }
    
    private bool WantsTreeConfig(EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
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

    private bool WantsFiltersConfig(EFPConfigActionInfo actionInfo)
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
    /// Запись секции конфигурации
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="cfg">Заполняемая секция</param>
    /// <param name="actionInfo">Информация о действии</param>
    public override void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      switch (category)
      {
        case EFPConfigCategories.TreeConfig:
          if (WantsTreeConfig(EFPConfigMode.Write, actionInfo) && CurrentConfig != null)
          {
            EFPDataGridViewConfig config2 = CurrentConfig.Clone(this); // с учетом реальных размеров столбцов
            config2.WriteConfig(cfg);
          }
          break;

        case EFPConfigCategories.Filters:
          if (WantsFiltersConfig(actionInfo))
            Filters.WriteConfig(cfg);
          break;

        case EFPConfigCategories.TreeView:
          if (Orders.Count > 1)
            cfg.SetString("Order", CurrentOrderName);
          break;
      }
      base.WriteConfigPart(category, cfg, actionInfo);
    }

    /// <summary>
    /// Чтение секции конфигурации
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="cfg">Считываемая секция</param>
    /// <param name="actionInfo">Информация о действии</param>
    public override void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      switch (category)
      {
        case EFPConfigCategories.TreeConfig:
          if (WantsTreeConfig(EFPConfigMode.Read, actionInfo))
            ReadConfigPartGridConfig(cfg);
          break;

        case EFPConfigCategories.Filters:
          if (WantsFiltersConfig(actionInfo))
          {
            if (actionInfo.Purpose == EFPConfigPurpose.Composition)
              Filters.ClearAllFilters(); // 25.12.2018 - в композиции фильтры задаются
            Filters.ReadConfig(cfg);
            CallOnFilterChanged();
          }
          break;

        case EFPConfigCategories.TreeView:
          if (Orders.Count > 1)
            CurrentOrderName = cfg.GetString("Order");
          break;

      }
      base.ReadConfigPart(category, cfg, actionInfo);
    }

    // ReSharper disable once RedundantOverriddenMember
    /// <summary>
    /// Вызывается при первой загрузке конфигурации элемента перед вызовом <see cref="EFPControlBase.LoadConfig"/>.
    /// Метод вызывается независимо от установки свойства <see cref="EFPControlBase.ConfigHandler"/>.
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
    public new EFPConfigurableDataTreeViewCommandItems CommandItems { get { return (EFPConfigurableDataTreeViewCommandItems)(base.CommandItems); } }

    /// <summary>
    /// Создает объект <see cref="EFPConfigurableDataTreeViewCommandItems"/>.
    /// </summary>
    /// <returns>Новый объект</returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      return new EFPConfigurableDataTreeViewCommandItems(this);
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
      Reporting.BRDataTreeViewMenuOutItem outItem = (Reporting.BRDataTreeViewMenuOutItem)sender;
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


    ///// <summary>
    ///// Если свойство установлено в true (по умолчанию), то используется порядок сортировки строк, заданный в 
    ///// GridProducer.
    ///// Если используется особая реализация просмотра, то можно отключить инициализацию порядка сортировки
    ///// строк, установив значение false. Установка допускается только до вывода просмотра на экран
    ///// </summary>
    //public bool UseGridProducerOrders
    //{
    //  get { return _UseGridProducerOrders; }
    //  set
    //  {
    //    CheckHasNotBeenCreated();
    //    _UseGridProducerOrders = value;
    //  }
    //}
    //private bool _UseGridProducerOrders;

    /// <summary>
    /// Имя фиксированной настройки табличного просмотра
    /// Если свойство не установлено, используется настройка по умолчанию: <see cref="EFPGridProducer.DefaultConfig"/>.
    /// 
    /// Свойство должно устанавливаться до <see cref="EFPDataTreeView.CurrentConfig"/>.
    /// </summary>
    public string DefaultConfigName
    {
      get { return _DefaultConfigName; }
      set
      {
        if (CurrentConfigHasBeenSet)
          throw ExceptionFactory.ObjectPropertyAlreadySet(this, "CurrentConfig");
        _DefaultConfigName = value;
      }
    }
    private string _DefaultConfigName;

    #endregion

    #region Обработчики изменения столбцов в просмотре

    private void Control_ColumnWidthChanged(object sender, TreeColumnEventArgs args)
    {
      try
      {
        if (true /*args.Column.AutoSizeMode != DataGridViewAutoSizeColumnMode.Fill*/)
          ConfigHandler.Changed[EFPConfigCategories.TreeConfig] = true;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    private void Control_ColumnReordered(object sender, TreeColumnEventArgs args)
    {
      try
      {
        ConfigHandler.Changed[EFPConfigCategories.TreeConfig] = true;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    #endregion

    #region Настройка просмотра

    // TODO: Может быть стоит сделать более сложное событие

    /// <summary>
    /// Событие вызывается при выполнении команды меню "Настройка просмотра".
    /// Если обработчик не установлен, показывается диалог, использующий интерфейс <see cref="IEFPConfigurableGridProducer"/>.
    /// Свойство <see cref="CancelEventArgs.Cancel"/> влияет на результат, возвращаемый методом <see cref="ShowConfigDialog()"/>.
    /// </summary>
    public event CancelEventHandler ConfigureView;

    /// <summary>
    /// Свойство возвращает true, если есть обработчик события <see cref="ConfigureView"/> или можно использовать интерфейс <see cref="IEFPConfigurableGridProducer"/>.
    /// </summary>
    public virtual bool HasConfigureViewHandler
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
      dlg.ConfigCategory = EFPConfigCategories.TreeConfig; // 26.03.2024
      dlg.HistoryCategory = EFPConfigCategories.TreeConfigHistory; // 26.03.2024
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
      ConfigHandler.Changed[EFPConfigCategories.TreeConfig] = true;

      return true;
    }

    /// <summary>
    /// Возвращает true, если при открытии / закрытии просмотра сохраняется последняя пользовательская 
    /// конфигурация.
    /// До показа просмотра на экране свойство возвращает значение, зависящее от других свойств.
    /// При показе таблицы значение фиксируется.
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
        throw new ReenteranceException();
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
    /// Вызывается при показе просмотра, если предполагается загрузка последней пользовательской конфигурации просмотра,
    /// но просмотр открывается впервые и нет сохраненной конфигурации.
    /// Также метод вызывается в случае ошибки загрузки.
    /// Метод должен установить свойство <see cref="EFPDataGridView.CurrentConfig"/>
    /// </summary>
    protected virtual void OnInitDefaultGridConfig()
    {
      if (GridProducer != null)
      {
        GridProducer.InitTreeView(this, false);
        PerformGridProducerPostInit();
      }
    }

    /// <summary>
    /// Если свойство установлено в true, то при открытии просмотра будет всегда использоваться
    /// настройка просмотра по умолчанию, а не последняя сохраненная конфигурация.
    /// При этом сохранение настроек работает как обычно. Пользователь, в частности, может выбрать
    /// последнюю сохраненную конфигурацию или любую другую из истории.
    /// Свойство действует, если <see cref="UserConfigAutoSave"/> возвращает true.
    /// Установка свойства разрешается только до показа просмотра на экране.
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
          EFPApp.ShowException(e, Res.EFPConfigurableDataView_ErrTitle_ReadConfig);
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
        throw new BugException("Property CurrentConfig has not been set whyle InitDefaultGridConfig() method called");
    }

    #endregion

    #region Фильтры

    /// <summary>
    /// Коллекция фильтров для табличного просмотра. Если есть хотя бы один фильтр,
    /// то в локальном меню появляется команда "Фильтр". После установки пользователем
    /// фильтра вызывается обновление просмотра. Ответственность за обработку
    /// фильтров лежит на вызывающей программе.
    /// Чтобы проверить наличие возможных фильтров, следует использовать свойство
    /// <see cref="HasFilters"/>, которое позволяет избежать создания лишних объектов.
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
    /// Непереопределенный метод создает <see cref="EFPDummyGridFilters"/>.
    /// </summary>
    /// <returns>Созданный объект</returns>
    protected virtual IEFPGridFilters CreateGridFilters()
    {
      return new EFPDummyGridFilters();
    }


    /// <summary>
    /// Возвращает true, если <see cref="Filters"/>.Count больше 0. 
    /// Не создает дополнительный объект.
    /// Не имеет значения, есть ли при этом активные фильтры.
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
    /// Если <paramref name="startFilter"/> не задан, активируется строка для первого непустого фильтра.
    /// </summary>
    /// <param name="startFilter">Имя фильтра, который должен быть выбран в таблице</param>
    public bool ShowFilterDialog(string startFilter)
    {
      LoadConfig();

      if (!HasFilters)
      {
        EFPApp.ErrorMessageBox(Res.EFPConfigurableDataView_Err_NoFilters);
        return false;
      }

      // Сохраняем существующие данные на случай нажатия кнопки "Отмена"
      TempCfg cfgOriginal = new TempCfg();
      Filters.WriteConfig(cfgOriginal);

      bool res;
      Filters.BeginUpdate();
      try
      {
        GridFilterForm form = new GridFilterForm(this, _DefaultFilterCfg, cfgOriginal);
        form.SetStartFilter(startFilter);
        if (EFPApp.ShowDialog(form, true) == DialogResult.OK)
        {
          // Вызываем виртуальный метод
          CallOnFilterChanged();
          res = true;
        }
        else
        {
          // Выполняем откат редактирования
          Filters.ClearAllFilters();
          Filters.ReadConfig(cfgOriginal);
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
    /// Вызывается для дополнительной инициализации табличного просмотра в редакторе фильтров.
    /// Переопределеннный метод может, например, инициализировать дополнительные команды меню.
    /// </summary>
    /// <param name="filterGridProvider">Обработчик таблицы фильтров</param>
    public virtual void InitGridFilterEditorGridView(EFPGridFilterEditorGridView filterGridProvider)
    {
    }


    /// <summary>
    /// Свойство возвращает true, если табличный просмотр выполняет чтение и запись фильтров в
    /// секцию конфигурации.
    /// Если свойство не установлено в явном виде, то возвращается значение <see cref="EFPConfigurableDataTreeViewCommandItems.CanEditFilters"/>.
    /// Свойство может устанавливаться только до вывода просмотра на экран.
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
    /// <see cref="ShowFilterDialog()"/>, но диалог не выводится.
    /// </summary>
    public void PerformFilterChanged()
    {
      CallOnFilterChanged();
    }


    /// <summary>
    /// Свойство возвращает true, если в данный момент вызывается метод <see cref="OnFilterChanged()"/>.
    /// Свойство может быть проанализировано в обработчике события <see cref="EFPTreeViewAdv.RefreshData"/>, чтобы определить
    /// причину вызова.
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
    /// Обновление просмотра при установке значений фильтров.
    /// Вызывает обработчик события <see cref="AfterSetFilter"/>, если он установлен.
    /// Затем вызывается <see cref="EFPDataTreeView.PerformRefresh()"/>
    /// </summary>
    protected virtual void OnFilterChanged()
    {
      // Вызываем пользовательский блок
      if (AfterSetFilter != null)
        AfterSetFilter(this, EventArgs.Empty);

      // Запоминаем текущие настройки фильтров
      if (HasBeenCreated)
      {
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
  }

  /// <summary>
  /// Команды локального меню для древовидного просмотра <see cref="EFPConfigurableDataTreeView"/>
  /// </summary>
  public class EFPConfigurableDataTreeViewCommandItems : EFPDataTreeViewCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает список команд
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    public EFPConfigurableDataTreeViewCommandItems(EFPConfigurableDataTreeView controlProvider)
      : base(controlProvider)
    {
      #region Исходные значения свойств

      CanEditFilters = true;

      #endregion

      #region Создание команд

      #region Настройка просмотра

      ciEditConfig = new EFPCommandItem("View", "GridConfig");
      ciEditConfig.MenuText = Res.Cmd_Menu_View_GridConfig;
      ciEditConfig.ImageKey = "EditGridConfig";
      ciEditConfig.Click += new EventHandler(ciEditConfig_Click);
      // пусть будет везде ciEditConfig.Usage = EFPCommandItemUsage.Menu;
      Add(ciEditConfig);

      #endregion

      #region Фильтр

      _MenuFilter = new EFPCommandItem("View", "MenuFilter");
      _MenuFilter.MenuText = Res.Cmd_Menu_Filter;
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
      ciSetFilter.MenuText = Res.Cmd_Menu_Filter_SetFilter;
      ciSetFilter.Click += new EventHandler(ciSetFilter_Click);
      Add(ciSetFilter);

      ciScrollFilterPrev = new EFPCommandItem("View", "PrevDateFilter");
      ciScrollFilterPrev.Parent = _MenuFilter;
      ciScrollFilterPrev.MenuText = Res.Cmd_Menu_Filter_PrevDateFilter;
      ciScrollFilterPrev.ShortCut = Keys.Alt | Keys.Up;
      ciScrollFilterPrev.ImageKey = "ArrowUp";
      ciScrollFilterPrev.GroupBegin = true;
      ciScrollFilterPrev.Usage = EFPCommandItemUsage.None;
      ciScrollFilterPrev.Click += new EventHandler(ciScrollFilterPrev_Click);
      Add(ciScrollFilterPrev);

      ciScrollFilterNext = new EFPCommandItem("View", "NextDateFilter");
      ciScrollFilterNext.Parent = _MenuFilter;
      ciScrollFilterNext.MenuText = Res.Cmd_Menu_Filter_NextDateFilter;
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
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPConfigurableDataTreeView ControlProvider { get { return (EFPConfigurableDataTreeView)(base.ControlProvider); } }

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
    /// фильтр можно прокручивать).
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
    /// обрабатываются клавиши Ctrl-СтрелкаВверх и Ctrl-СтрелкаВниз для переключения
    /// между периодами дат.
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
    /// Инициализация видимости команд
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
        ciSetFilter.Enabled = CanEditFilters;

      if (HasScrollableFilter && ciSortMoveDown.Usage == EFPCommandItemUsage.None)
      {
        ciScrollFilterPrev.Usage = EFPCommandItemUsage.Everywhere;
        ciScrollFilterNext.Usage = EFPCommandItemUsage.Everywhere;
      }

      if (_MenuFilter.Usage != EFPCommandItemUsage.None)
        _MenuFilter.InitMenuVisible(); // 09.07.2019
    }

    /// <summary>
    /// Обновление доступности команд
    /// </summary>
    protected override void OnRefreshItems()
    {
      base.OnRefreshItems();

      IEFPScrollableGridFilter sf = ActiveScrollableFilter; // чтобы меньше поиска
      if (sf != null)
      {
        ciScrollFilterPrev.Enabled = sf.CanScrollUp;
        ciScrollFilterNext.Enabled = sf.CanScrollDown;
        ciScrollFilterPrev.ToolTipText = String.Format(Res.EFPConfigurableDataView_ToolTip_ForFilter, ciScrollFilterPrev.MenuText, ((IEFPGridFilter)sf).DisplayName);
        ciScrollFilterNext.ToolTipText = String.Format(Res.EFPConfigurableDataView_ToolTip_ForFilter, ciScrollFilterNext.MenuText, ((IEFPGridFilter)sf).DisplayName);
      }
      else
      {
        ciScrollFilterPrev.Enabled = false;
        ciScrollFilterNext.Enabled = false;
        ciScrollFilterPrev.ToolTipText = String.Format(Res.EFPConfigurableDataView_ToolTip_ForNoActiveFilter, ciScrollFilterPrev.MenuText);
        ciScrollFilterNext.ToolTipText = String.Format(Res.EFPConfigurableDataView_ToolTip_ForNoActiveFilter, ciScrollFilterNext.MenuText);
      }
    }

    #endregion

    #region Настройка просмотра

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
        EFPApp.MessageBox(String.Format(Res.EFPConfigurableDataView_Err_ScrollFilter, ((IEFPGridFilter)ActiveScrollableFilter).DisplayName));
        return;
      }

      if (forward)
        ActiveScrollableFilter.ScrollDown();
      else
        ActiveScrollableFilter.ScrollUp();

      ControlProvider.PerformFilterChanged();
    }

    #endregion
  }
}
