using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Controls;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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
  /// Расширение табличного просмотра для работы с фильтрами
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

    #region OnShown

    //protected override void OnShown()
    //{
    //  base.OnShown();
    //}


    /// <summary>
    /// Когда табличный просмотр первый раз выводится на экран, сохраняем сюда
    /// настройки фильтров "по умолчанию", то есть те, которые выполнены прикладным
    /// модулем. Например, для операций, могут быть по умолчанию установлен фильтр
    /// по дате операции (текущий месяц) и по нашей организации
    /// </summary>
    private TempCfg _DefaultFilterCfg;

    #endregion

    #region Чтение и запись настроек (общая часть)

    /// <summary>
    /// Заполняет список категорий, для которых испольщуются секции конфигурации..
    /// Добавляет категории "Filters" и "GridView", в зависимости от наличия фильтров и порядков сортировки
    /// </summary>
    /// <param name="categories">Список категорий для заполнения</param>
    /// <param name="rwMode">Режим чтения или записи</param>
    /// <param name="actionInfo">Описание действия</param>
    public override void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      base.GetConfigCategories(categories, rwMode, actionInfo);

      // !!!if (WantsGridConfig(Mode, Info))
      //  Categories.Add(EFPConfigCategories.GridConfig);

      if (WantsFiltersConfig(actionInfo))
        categories.Add(EFPConfigCategories.Filters);

      if (Orders.Count > 1)
        categories.Add(EFPConfigCategories.TreeView); // для записи Order
    }

    
    private bool WantsGridConfig(EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      /*  !!!!
      if (Info.Purpose == EFPConfigPurpose.Composition)
      {
        return HasConfigureViewHandler && (!AlwaysUseDefaultConfig);
      }

      if (UserConfigAutoSave)
      {
        if (Mode == EFPConfigMode.Write)
          return true;
        else if (!AlwaysUseDefaultConfig)
          return true;
      } */
      return false;
    } 

    private bool WantsFiltersConfig(EFPConfigActionInfo actionInfo)
    {
      if (!SaveFiltersAllowed)
        return false;
      if (!CommandItems.CanEditFilters)
        return false;
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
        //case EFPConfigCategories.GridConfig:
        //  if (WantsGridConfig(EFPConfigMode.Write, Info) && CurrentConfig != null)
        //  {
        //    EFPDataGridViewConfig GridConfig2 = CurrentConfig.Clone(this); // с учетом реальных размеров столбцов
        //    GridConfig2.WriteConfig(Config);
        //  }
        //  break;

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
        //case EFPConfigCategories.GridConfig:
        //  if (WantsGridConfig(EFPConfigMode.Read, Info))
        //    ReadConfigPartGridConfig(Config);
        //  break;

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
    /// Вызывается при первой загрузке конфигурации элемента перед вызовом LoadConfig().
    /// Метод вызывается независимо от установки свойства ConfigHandler.
    /// </summary>
    protected override void OnBeforeLoadConfig()
    {
      base.OnBeforeLoadConfig();
      //!!!!!FUserConfigAutoSave = GetUserConfigAutoSave();
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

      //if (CurrentConfig == null && GridProducer != null)
      //  //OnInitDefaultGridConfig();
      //  InitDefaultGridConfig(); // 30.09.2018
    }

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Команды локального меню
    /// </summary>
    public new EFPConfigurableDataTreeViewCommandItems CommandItems { get { return (EFPConfigurableDataTreeViewCommandItems)(base.CommandItems); } }

    /// <summary>
    /// Создает объект EFPConfigurableDataTreeViewCommandItems.
    /// </summary>
    /// <returns>Новый объект</returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      return new EFPConfigurableDataTreeViewCommandItems(this);
    }

    #endregion

    #region GridProducer

    /// <summary>
    /// Генератор табличного просмотра.
    /// Если реализует расширенный интерфейс IEFPConfigurableGridProducer и установлено свойство ConfigSectionName,
    /// то поддерживается команда "Настройка просмотра", а конфигурация автоматически сохраняется и восстанавливается
    /// при показе на экран
    /// </summary>
    public new IEFPGridProducer GridProducer
    {
      get { return base.GridProducer; }
      set { base.GridProducer = value; }
    }


    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то используется порядок сортировки строк, заданный в 
    /// GridProducer.
    /// Если используется особая реализация просмотра, то можно отключить инициализацию порядка сортировки
    /// строк, установив значение false. Установка допускается только до вывода просмотра на экран
    /// </summary>
    public bool UseGridProducerOrders
    {
      get { return _UseGridProducerOrders; }
      set
      {
        CheckHasNotBeenCreated();
        _UseGridProducerOrders = value;
      }
    }
    private bool _UseGridProducerOrders;

    /// <summary>
    /// Имя фиксированной настройки табличного просмотра
    /// Если свойство не установлено и свойство CurrentCfgName не установлено, 
    /// используется настройка по умолчанию: GridProducer.DefaultConfig
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
    /// Непереопределенный метод создает EFPDummyGridFilters.
    /// </summary>
    /// <returns>Созданный объект</returns>
    protected virtual IEFPGridFilters CreateGridFilters()
    {
      return new EFPDummyGridFilters();
    }


    /// <summary>
    /// Возвращает true, если Filter.Count больше 0. 
    /// Не создает дополнительный объект
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
    /// Если StartFilter не задан, активируется строка для первого непустого фильтра
    /// </summary>
    /// <param name="startFilter"></param>
    public bool ShowFilterDialog(string startFilter)
    {
      LoadConfig();

      if (!HasFilters)
      {
        EFPApp.ErrorMessageBox("Табличный просмотр не содержит фильтров");
        return false;
      }

      // Сохраняем существующие данные на случай нажатия кнопки "Отмена"
      TempCfg cfgOriginal = new TempCfg();
      Filters.WriteConfig(cfgOriginal);

      bool Res;
      Filters.BeginUpdate();
      try
      {
        GridFilterForm Form = new GridFilterForm(this, _DefaultFilterCfg, cfgOriginal);
        Form.SetStartFilter(startFilter);
        if (EFPApp.ShowDialog(Form, true) == DialogResult.OK)
        {
          // Вызываем виртуальный метод
          CallOnFilterChanged();
          Res = true;
        }
        else
        {
          // Выполняем откат редактирования
          Filters.ClearAllFilters();
          Filters.ReadConfig(cfgOriginal);
          Res = false;
        }
      }
      finally
      {
        Filters.EndUpdate();
      }

      return Res;
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
    /// Обновление просмотра при установке значений фильтров.
    /// Вызывает обработчик события AfterSetFilter, если он установлен.
    /// Затем вызывается PerformRefresh()
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

    #endregion
  }

  /// <summary>
  /// Команды локального меню для древовидного просмотра с возможностью
  /// фильтрации
  /// </summary>
  public class EFPConfigurableDataTreeViewCommandItems : EFPDataTreeViewCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает список команд
    /// </summary>
    /// <param name="owner">Провайдер управляющего элемента</param>
    public EFPConfigurableDataTreeViewCommandItems(EFPConfigurableDataTreeView owner)
      : base(owner)
    {
      #region Исходные значения свойств

      CanEditFilters = true;

      #endregion

      #region Создание команд

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
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPConfigurableDataTreeView Owner { get { return (EFPConfigurableDataTreeView)(base.Owner); } }

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
        if (Owner.HasFilters)
        {
          for (int i = 0; i < Owner.Filters.Count; i++)
          {
            if (Owner.Filters[i] is IEFPScrollableGridFilter)
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
        if (Owner.HasFilters)
        {
          for (int i = 0; i < Owner.Filters.Count; i++)
          {
            if (Owner.Filters[i] is IEFPScrollableGridFilter)
            {
              IEFPScrollableGridFilter f = (IEFPScrollableGridFilter)(Owner.Filters[i]);
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

    /// <summary>
    /// Инициализация свойство EFPCommandItem.Usage
    /// </summary>
    protected override void BeforeControlAssigned()
    {
      base.BeforeControlAssigned();

      if (!Owner.HasFilters)
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
    protected override void DoRefreshItems()
    {
      base.DoRefreshItems();

      IEFPScrollableGridFilter sf = ActiveScrollableFilter; // чтобы меньше поиска
      if (sf != null)
      {
        ciScrollFilterPrev.Enabled = sf.CanScrollUp;
        ciScrollFilterNext.Enabled = sf.CanScrollDown;
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
      Owner.ShowFilterDialog();
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

      Owner.PerformFilterChanged();
    }

    #endregion
  }
}
