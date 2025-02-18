// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.ComponentModel;
using FreeLibSet.Config;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  // Этот модуль содержит страницы отчета с табличными просмотрами

  /// <summary>
  /// Страничка отчета с табличным просмотром.
  /// Табличный просмотр присоединяется к набору данных, а колонки инициализируются
  /// один раз.
  /// Создает <see cref="EFPDataGridView"/>, если не переопределено в производном классе.
  /// Если требуется <see cref="EFPConfigurableDataGridView"/>, следует использовать <see cref="EFPReportConfigurableGridPage"/>.
  /// </summary>
  public class EFPReportGridPage : EFPReportPage
  {
    #region Конструкторы

    /// <summary>
    /// Создать страницу
    /// </summary>
    public EFPReportGridPage()
    {
      _AutoInitStateImageKey = false;
      _AutoSaveOrder = true;
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Заголовок вкладки.
    /// Дополнительно устанавливает свойство <see cref="EFPControlBase.DisplayName"/>
    /// </summary>
    public override string Title
    {
      get { return base.Title; }
      set
      {
        base.Title = value;
        if (ControlProvider != null)
          ControlProvider.DisplayName = Title;
      }
    }

    /// <summary>
    /// Дополнительно выполняет инициализацию свойства <see cref="EFPReportPage.ImageKey"/>
    /// </summary>
    /// <param name="parentControl">Пустая панель, на которой будет расположена страница</param>
    public override void AssignParentControl(Panel parentControl)
    {
      base.AssignParentControl(parentControl);
      if (AutoInitStateImageKey)
        DataReady = true; // сразу создаем страницу и присоединяем данные
    }

    /// <summary>
    /// Создает страницу отчета
    /// </summary>
    /// <param name="parent">Панель для размещения страницы</param>
    protected override void CreatePage(Panel parent)
    {
      // Табличный просмотр может быть не будет создан, поэтому запоминаем панель
      _ParentControl = parent;
      if (AutoRecreateGrid && DataSource == null)
        return;

      CreateControlProvider();

      SetControlName(_ControlProvider, "Grid");
    }

    /// <summary>
    /// Инициализация данных страницы отчета.
    /// </summary>
    protected override void InitData()
    {
      base.InitData();
      if (DataSource != null)
      {
        EFPDataGridViewSelection oldSel = _ControlProvider.Selection;
        bool hasData = _ControlProvider.Control.DataSource != null;

        OnInitControlDataSource();
        // так не работает, т.к. GridProducer еще не применен
        //if (FControlProvider.OrderCount > 0 && FControlProvider.AutoSort)
        //  FControlProvider.PerformAutoSort(); // 31.08.2017

        if (hasData && DataSource != null)
          _ControlProvider.Selection = oldSel;
      }
    }

    /// <summary>
    /// Устанавливает свойство <see cref="DataGridView.DataSource"/> равным <see cref="DataSource"/>.
    /// Переопределяется в <see cref="EFPReportStdConfigurableGridPage"/>.
    /// </summary>
    protected virtual void OnInitControlDataSource()
    {
      _ControlProvider.Control.DataSource = DataSource;
    }

    private void ControlProvider_Validating(object sender, UICore.UIValidatingEventArgs args)
    {
      if (AutoInitStateImageKey)
      {
        if (_ControlProvider.UseRowImages && _ControlProvider.ShowRowCountInTopLeftCell)
          base.InitStateImageKey(_ControlProvider.TopLeftCellImageKind);
        else
          base.InitStateImageKey(_ControlProvider.GetAllRowsImageKind());
      }
    }


    /// <summary>
    /// Запомнить информацию табличного просмотра
    /// </summary>
    /// <param name="cfg">Заполняемая секция конфигурации</param>
    /// <returns>Обычно следует возвращать true</returns>
    public override bool SaveViewConfig(CfgPart cfg)
    {
      if (!base.SaveViewConfig(cfg))
        return false;

      // TODO:

#if XXX
      if (FControlProvider != null)
      {
        if (AutoSaveOrder && FControlProvider.OrderCount > 0)
          FControlProvider.Orders.WriteConfig();
        if (!FControlProvider.SaveGridConfigIfRequired())
          return false;
      }
#endif
      return true;
    }

    /// <summary>
    /// Этот метод вызывается при обновлении отчета нажатием клавиши [F5] при сбросе
    /// свойства <see cref="EFPReportPage.DataReady"/> в false
    /// </summary>
    protected override void InvalidateData()
    {
      base.InvalidateData();
      if (AutoInitStateImageKey)
        ImageKey = "UnknownState";
    }

    #endregion

    #region Свойство DataSource

    /// <summary>
    /// Сюда должен быть помещен набор данных, который будет отображен.
    /// Если свойство <see cref="AutoRecreateGrid"/> установлено в true, то при присвоении
    /// значения свойству будет (заново) создан новый табличный просмотр.
    /// </summary>
    public DataView DataSource
    {
      get { return _DataSource; }
      set
      {
        if (AutoInitStateImageKey && _ParentControl != null)
          SetPageCreated();

        _DataSource = value;
        OnDataSourceChanged(EventArgs.Empty);

        if (_ControlProvider != null)
        {
          if (AutoRecreateGrid)
            RecreateGrid();
          else
          {
            EFPDataGridViewSelection oldSel = _ControlProvider.Selection;
            bool hasData = _ControlProvider.Control.DataSource != null;

            OnInitControlDataSource();

            if (hasData && value != null)
              _ControlProvider.Selection = oldSel;
          }
        }
        else
        {
          if (_ParentControl != null)
          {
            CreateControlProvider();
            OnInitControlDataSource();
          }
        }
      }
    }

    private DataView _DataSource;

    /// <summary>
    /// Событие вызывается при установке свойства <see cref="DataSource"/>
    /// </summary>
    public event EventHandler DataSourceChanged;

    /// <summary>
    /// Вызывает событие <see cref="DataSourceChanged"/>
    /// </summary>
    /// <param name="args">Фиктивный аргумент</param>
    protected virtual void OnDataSourceChanged(EventArgs args)
    {
      if (DataSourceChanged != null)
        DataSourceChanged(this, args);
    }

    #endregion

    #region Прочие свойства

    private EFPControlWithToolBar<DataGridView> _MainPanel;

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public EFPDataGridView ControlProvider { get { return _ControlProvider; } }
    private EFPDataGridView _ControlProvider;

    /// <summary>
    /// Если установить в true, то будет видна панель кнопок.
    /// По умолчанию - false - без кнопок.
    /// Свойство можно устанавливать только до присоединения страницы к отчету.
    /// </summary>
    public bool ShowToolBar
    {
      get { return _ShowToolBar; }
      set
      {
        CheckPageNotCreated();
        _ShowToolBar = value;
      }
    }
    private bool _ShowToolBar;

    /// <summary>
    /// Если свойство установлено, то выполняется автоматическое управление значком
    /// вкладки (свойство <see cref="EFPReportPage.ImageKey"/>), в зависимости от наличия в табличном просмотре
    /// строк с ошибками и предупреждениями.
    /// Установка свойства заставляет отчет обеспечить готовность данных на странице
    /// немедленно после построения отчета, а не при первом обращении пользователя 
    /// к странице, что может привести к замедлению построения.
    /// По умолчанию свойство не установлено.
    /// Если свойство установлено, то ручное задание свойства <see cref="EFPReportPage.ImageKey"/> не требуется.
    /// Свойство можно устанавливать только до присоединения страницы к отчету.
    /// </summary>
    public bool AutoInitStateImageKey
    {
      get { return _AutoInitStateImageKey; }
      set
      {
        CheckPageNotCreated();
        _AutoInitStateImageKey = value;
        if (value)
          ImageKey = "UnknownState";
      }
    }
    private bool _AutoInitStateImageKey;

    /// <summary>
    /// По умолчанию (false) табличный просмотр создается один раз при инициализации страницы отчета.
    /// Событие <see cref="InitGrid"/> также вызывается один раз. 
    /// Повторное построение отчета не приводит к замене табличного просмотра.
    /// Установка свойства в true делает построение табличного просмотра многократным.
    /// Табличный просмотр заново создается при каждом перестроении отчета.
    /// Свойство можно устанавливать только до присоединения страницы к отчету.
    /// </summary>
    public bool AutoRecreateGrid
    {
      get { return _AutoRecreateGrid; }
      set
      {
        CheckPageNotCreated();
        _AutoRecreateGrid = value;
      }
    }
    private bool _AutoRecreateGrid;

    /// <summary>
    /// Автоматическая загрузка и сохранение порядка сортировки строк в секции 
    /// конфигурации (по умолчанию - true), если есть установленные 
    /// порядки сортировки <see cref="EFPDataGridView.Orders"/> или <see cref="EFPDataGridView.CustomOrderAllowed"/>=true. 
    /// Если требуется, чтобы страница отчета при каждом
    /// построении имела определенный порядок строк, а не запомненный ранее,
    /// следует установить значение false до присоединения страницы к отчету.
    /// </summary>
    public bool AutoSaveOrder { get { return _AutoSaveOrder; } set { _AutoSaveOrder = value; } }
    private bool _AutoSaveOrder;

    #endregion

    #region Создание табличного просмотра

    /// <summary>
    /// Запоминаем родительскую панель
    /// </summary>
    private Control _ParentControl;

    /// <summary>
    /// Создать провайдер управляющего элемента <see cref="EFPDataGridView"/> или производного класса.
    /// Для реального создания вызывается <see cref="DoCreateControlProvider(DataGridView)"/>.
    /// Затем вызывается метод <see cref="OnInitGrid()"/>.
    /// </summary>
    protected virtual void CreateControlProvider()
    {
#if DEBUG
      if (_ParentControl == null)
        throw new BugException("CreatePage() has not been called");
#endif

      WinFormsTools.DisposeChildren(_ParentControl);
      _MainPanel = new EFPControlWithToolBar<DataGridView>(BaseProvider, _ParentControl);
      _MainPanel.Control.AutoGenerateColumns = false;
      _MainPanel.Control.ReadOnly = true;
      _MainPanel.Control.AllowUserToAddRows = false;
      _MainPanel.Control.AllowUserToDeleteRows = false;

      _ControlProvider = DoCreateControlProvider(_MainPanel.Control);
      // Убрано 02.06.2015
      //_ControlProvider.ReadOnly = true;
      //_ControlProvider.CanView = false;

      _MainPanel.Control.MultiSelect = true; // после создания DocGridHandler
      _ControlProvider.CommandItems.UseRefresh = false; // обрабатывается на уровне отчета в-целом
      //GridPageSetup PageSetup = FControlProvider.AddGridPrint();
      //PageSetup.DocumentName = Title;
      _ControlProvider.DisplayName = Title;
      _ControlProvider.ShowRowCountInTopLeftCell = true; 

      if (_ControlProvider.DefaultOutItem!=null)
        _ControlProvider.DefaultOutItem.TitleNeeded += DefaultOutItem_TitleNeeded;

      OnInitGrid();

      if (_MainPanel.ToolBarPanel != null)
      {
        _MainPanel.ToolBarPanel.Visible = ShowToolBar;
        if (ShowToolBar)
          ControlProvider.ToolBarPanel = _MainPanel.ToolBarPanel;
      }
      // Убрано 21.05.2022.
      // Пусть пока команды меню останутся доступными для изменения.
      // Событие DocTypeUI.InitView будет вызвано позднее, в ответ на событие EFPControlBase.Created.
      // Пользовательский обработчик InitView может добавить еще команды меню
      // ControlProvider.PrepareCommandItems();

      ControlProvider.Validating += ControlProvider_Validating;
    }

    /// <summary>
    /// Непосредственное создание объекта <see cref="EFPDataGridView"/> или производного класса.
    /// Если не переопределено, создает простой табличный просмотр <see cref="EFPDataGridView"/>.
    /// В переопределенном методе следует использовать свойство <see cref="EFPReportPage.BaseProvider"/> при вызове конструктора создаваемого объекта.
    /// </summary>
    /// <param name="control">Управляющий элемент <see cref="DataGridView"/></param>
    /// <returns>Провайдер <see cref="EFPDataGridView"/> или производного класса</returns>
    protected virtual EFPDataGridView DoCreateControlProvider(DataGridView control)
    {
      return new EFPDataGridView(BaseProvider, control);
    }

    /// <summary>
    /// Повторное создание табличного просмотра. Если на момент вызова просмотр
    /// еще не был создан, никаких действий не выполняется.
    /// Если просмотр существует, то он будет удален и создан заново (с повторным
    /// вызовом события <see cref="InitGrid"/>). Текущая позиция сохраняется, если источник
    /// данных был присоединен.
    /// </summary>
    public void RecreateGrid()
    {
      if (_ControlProvider == null)
        return;

      EFPDataGridViewSelection oldSel = _ControlProvider.Selection;
      bool hasData = _ControlProvider.Control.DataSource != null; // а не _DataSource!

      // Удаляем старый просмотр
      RemoveGrid();
      // Создаем новый просмотр
      CreateControlProvider();

      // Восстанавливаем источник данных
      OnInitControlDataSource();

      if (hasData && _DataSource != null)
        _ControlProvider.Selection = oldSel;
    }

    /// <summary>
    /// Вызывается из метода <see cref="RecreateGrid()"/> для удаления существующего просмотра.
    /// Непереопределенный метод вызывает <see cref="Control.ControlCollection.Remove(Control)"/>.
    /// </summary>
    protected virtual void RemoveGrid()
    {
      _ParentControl.Controls.Remove(_ControlProvider.Control); // нельзя использовать Clear(), т.к. не будет Dispose()
    }

    #endregion

    #region Событие InitGrid

    /// <summary>
    /// Событие вызывается после создания объекта <see cref="EFPDataGridView"/>.
    /// Обработчик должен добавить столбцы и установить другие свойства, используя свойство
    /// <see cref="ControlProvider"/> для доступа к объекту.
    /// </summary>
    public event EventHandler InitGrid;

    /// <summary>
    /// Вызывает событие <see cref="InitGrid"/>.
    /// </summary>
    protected virtual void OnInitGrid()
    {
#if DEBUG
      if (_ControlProvider == null)
        throw new BugException("Control propvider has not been created");
#endif

      if (InitGrid != null)
        InitGrid(this, EventArgs.Empty);
    }

    #endregion
  }


  /// <summary>
  /// Страница отчета с табличным просмотром <see cref="EFPConfigurableDataGridView"/>
  /// </summary>
  public class EFPReportConfigurableGridPage : EFPReportGridPage
  {
    #region Переопределенные методы и свойства

    /// <summary>
    /// Создает провайдер управляющего элемента.
    /// </summary>
    protected override void CreateControlProvider()
    {
      base.CreateControlProvider();

      if (AutoSaveOrder && ControlProvider.OrderCount > 0)
        ControlProvider.LoadConfig();
    }

    /// <summary>
    /// Создает <see cref="EFPConfigurableDataGridView"/>
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <returns>Провайдер</returns>
    protected override EFPDataGridView DoCreateControlProvider(DataGridView control)
    {
      return new EFPConfigurableDataGridView(BaseProvider, control);
    }

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public new EFPConfigurableDataGridView ControlProvider { get { return (EFPConfigurableDataGridView)(base.ControlProvider); } }

    /// <summary>
    /// Инициализация просмотра
    /// </summary>
    protected override void OnInitGrid()
    {
      // Секцию конфигурации задаем обязательно до пользовательского обработчика
      if (_TempConfigSectionName != null)
      {
        ControlProvider.ConfigSectionName = _TempConfigSectionName;
        _TempConfigSectionName = null;
      }
      if (_TempGridProducer != null)
      {
        ControlProvider.GridProducer = _TempGridProducer;
        _TempGridProducer = null;
      }

      base.OnInitGrid();
    }

    #endregion

    #region IEFPConfigurable Members

    /// <summary>
    /// Имя секции конфигурации для хранения параметров.
    /// Свойство синхронизировано с основным объектом <see cref="EFPConfigurableDataGridView"/>.
    /// Допускается установка свойства до инициализации табличного просмотра.
    /// </summary>
    public string ConfigSectionName
    {
      get
      {
        if (ControlProvider == null)
          return _TempConfigSectionName;
        else
          return ControlProvider.ConfigSectionName;
      }
      set
      {
        if (ControlProvider == null)
          _TempConfigSectionName = value;
        else
          ControlProvider.ConfigSectionName = value;
      }
    }
    private string _TempConfigSectionName;

    /// <summary>
    /// Менеджер хранения настроек
    /// </summary>
    public IEFPConfigManager ConfigManager
    {
      get
      {
        if (ControlProvider == null)
          return BaseProvider.ConfigManager;
        else
          return ControlProvider.ConfigManager;
      }
    }

    #endregion

    #region GridProducer

    /// <summary>
    /// Инициализатор столбцов табличного просмотра.
    /// Это свойство синхронизировано с одноименным свойством <see cref="EFPConfigurableDataGridView"/>.
    /// Если свойство установлено, то GridProducer будет использован для инициализации столбцов.
    /// Свойство может быть установлено только до создания страниц.
    /// </summary>
    public IEFPGridProducer GridProducer
    {
      get
      {
        if (ControlProvider == null)
          return _TempGridProducer;
        else
          return ControlProvider.GridProducer;
      }
      set
      {
        CheckPageNotCreated();
        _TempGridProducer = value;
      }
    }
    private IEFPGridProducer _TempGridProducer;

    #endregion
  }

  /// <summary>
  /// Страница отчета с табличным просмотром <see cref="EFPStdConfigurableDataGridView"/>
  /// </summary>
  public class EFPReportStdConfigurableGridPage : EFPReportConfigurableGridPage
  {
    #region Переопределенные методы и свойства

    /// <summary>
    /// Создает <see cref="EFPStdConfigurableDataGridView"/>
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <returns>Провайдер</returns>
    protected override EFPDataGridView DoCreateControlProvider(DataGridView control)
    {
      return new EFPStdConfigurableDataGridView(BaseProvider, control);
    }

    void ControlProvider_GridProducerPostInit(object sender, EventArgs args)
    {
      OnInitControlDataSource();
    }

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public new EFPStdConfigurableDataGridView ControlProvider { get { return (EFPStdConfigurableDataGridView)(base.ControlProvider); } }

    #endregion

    #region GridProducer

    /// <summary>
    /// Инициализатор столбцов табличного просмотра.
    /// Это свойство синхронизировано с одноименным свойством <see cref="EFPConfigurableDataGridView"/>.
    /// Если свойство установлено, то GridProducer будет использован для инициализации столбцов.
    /// Свойство может быть установлено только до создания страниц.
    /// </summary>
    public new EFPGridProducer GridProducer
    {
      get { return (EFPGridProducer)(base.GridProducer); }
      set { base.GridProducer = value; }
    }

    #endregion

    #region UseTableRepeater

    /// <summary>
    /// Разрешено ли использовать таблицу-повторитель?
    /// Если true, то при установке набора данных определяется наличие вычисляемых столбцов в <see cref="EFPGridProducer"/>
    /// и, при необходимости, создается таблица-повторитель. Это может потребоваться, если в отчете используется
    /// произвольная сортировка (обработчик события <see cref="EFPReportGridPage.InitGrid"/> устанавливает <see cref="EFPDataGridView.CustomOrderAllowed"/>=true).
    /// Используется свойство <see cref="EFPStdConfigurableDataGridView.MasterDataView"/>.
    /// По умолчанию - false - источник данных <see cref="EFPReportGridPage.DataSource"/> используется напрямую, устанавливается свойство <see cref="EFPDataGridView.SourceAsDataView"/>.
    /// </summary>
    public bool UseTableRepeater
    {
      get { return _UseTableRepeater; }
      set
      {
        this.CheckPageNotCreated();
        _UseTableRepeater = value;
      }
    }
    private bool _UseTableRepeater;

    /// <summary>
    /// Если установлено свойство <see cref="UseTableRepeater"/>, то устанавливается <see cref="EFPDataGridView.MasterDataView"/>.
    /// </summary>
    protected override void OnInitControlDataSource()
    {
      if (UseTableRepeater)
        ControlProvider.MasterDataView = DataSource;
      else
        ControlProvider.SourceAsDataView = DataSource;
    }

    #endregion
  }

  /// <summary>
  /// Страничка отчета с табличным просмотром.
  /// Табличный просмотр создается в <see cref="EFPReport.BuildReport"/> и присоединяется к странице отчета
  /// целиком. Это позволят менять колонки отчета при перестроении.
  /// </summary>
  public class EFPReportVarGridPage : EFPReportPage
  {
    #region Конструктор

    /// <summary>
    /// Создает страницу
    /// </summary>
    public EFPReportVarGridPage()
    {
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Дополнительно устанавливает свойство <see cref="EFPControlBase.DisplayName"/>.
    /// </summary>
    public override string Title
    {
      get { return base.Title; }
      set
      {
        base.Title = value;
        if (ControlProvider != null)
          ControlProvider.DisplayName = Title;
      }
    }

    //public event InitGridEventHandler InitGrid;

    /// <summary>
    /// Этот метод только запоминает родительский элемент для размещения просмотра.
    /// Отложенное создание страницы выполняется в <see cref="InitData()"/>.
    /// </summary>
    /// <param name="parent"></param>
    protected override void CreatePage(Panel parent)
    {
      _ParentPanel = parent;
    }

    /// <summary>
    /// Выполняет отложенное создание табличного просмотра.
    /// </summary>
    protected override void InitData()
    {
      base.InitData();
      if (_ParentPanel.Controls.Count == 0)
        InitControls();
    }

    private void InitControls()
    {
      if (_ParentPanel == null)
        return;

      WinFormsTools.DisposeChildren(_ParentPanel);

      if (ControlProvider == null)
      {
        Label lbl = new Label();
        lbl.Text = Res.EFPReportVarGridPage_Msg_NoData;
        lbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lbl.Dock = DockStyle.Fill;
        _ParentPanel.Controls.Add(lbl);
      }
      else
      {
        _MainPanel = new EFPControlWithToolBar<DataGridView>(BaseProvider, _ParentPanel, ControlProvider.Control);
        if (_MainPanel.ToolBarPanel != null)
        {
          _MainPanel.ToolBarPanel.Visible = ShowToolBar;
          if (ShowToolBar)
            ControlProvider.ToolBarPanel = _MainPanel.ToolBarPanel;
        }

        SetControlName(ControlProvider, "Grid");
        ControlProvider.BaseProvider.Parent = base.BaseProvider;
      }
    }


    /// <summary>
    /// Этот метод вызывается внутри системы построения отчета и не должен вызываться
    /// прикладными модулями.
    /// </summary>
    /// <param name="parentControl">Пустая панель, на которой будет расположена страница</param>
    public override void AssignParentControl(Panel parentControl)
    {
      base.AssignParentControl(parentControl);

      if (AutoInitStateImageKey)
        DataReady = true;
    }

    /// <summary>
    /// Этот метод вызывается при обновлении отчета нажатием клавиши [F5] при сбросе
    /// свойства <see cref="EFPReportPage.DataReady"/> в false
    /// </summary>
    protected override void InvalidateData()
    {
      base.InvalidateData();
      if (AutoInitStateImageKey)
        ImageKey = "UnknownState";
    }

    #endregion

    #region Свойства

    private EFPControlWithToolBar<DataGridView> _MainPanel;

    /// <summary>
    /// "Скользящий" базовый провайдер.
    /// Используется прикладным кодом для создания объекта <see cref="EFPDataGridView"/>.
    /// После установки основного свойства <see cref="ControlProvider"/>, этот базовый провайдер заменяется на новый.
    /// Старый экземпляр используется только с текущим <see cref="ControlProvider"/>. Когда отчет перестраивается,
    /// старый провайдер отсоединяется от отчета, а новый - присоединяется.
    /// </summary>
    public new EFPBaseProvider BaseProvider
    {
      get
      {
        if (_BaseProvider == null)
          _BaseProvider = new EFPBaseProvider();
        return _BaseProvider;
      }
    }
    private EFPBaseProvider _BaseProvider;

    /// <summary>
    /// Сюда должен быть помещен табличный просмотр при построении отчета.
    /// </summary>
    public EFPDataGridView ControlProvider
    {
      get { return _ControlProvider; }
      set
      {
        if (value != null)
        {
          if (value.BaseProvider != _BaseProvider)
            throw ExceptionFactory.ArgProperty("value", value, "BaseProvider", value.BaseProvider, new object[] { _BaseProvider });
          if (_BaseProvider.Parent != null)
            throw ExceptionFactory.ObjectPropertyAlreadySet(_BaseProvider, "Parent");
        }

        if (_ControlProvider != null)
        {
          _ControlProvider.Validating -= ControlProvider_Validating;
          _ControlProvider.BaseProvider.Parent = null;

          if (_ControlProvider.DefaultOutItem != null)
            _ControlProvider.DefaultOutItem.TitleNeeded += DefaultOutItem_TitleNeeded;
        }

        _ControlProvider = value;
        _BaseProvider = null;

        if (_ControlProvider != null)
        {
          _ControlProvider.Validating += ControlProvider_Validating;

          _ControlProvider.DisplayName = Title;
          _ControlProvider.CommandItems.UseRefresh = false; // обрабатывается на уровне отчета в целом
          _ControlProvider.BaseProvider.Parent = base.BaseProvider;
        }
        else
        {
          if (AutoInitStateImageKey)
            ImageKey = "UnknownState";
        }

        InitControls();
      }
    }

    private EFPDataGridView _ControlProvider;

    private Panel _ParentPanel;

    /// <summary>
    /// Если установить в true, то будет видна панель кнопок.
    /// По умолчанию - false - без кнопок.
    /// </summary>
    public bool ShowToolBar
    {
      get { return _ShowToolBar; }
      set
      {
        CheckPageNotCreated();
        _ShowToolBar = value;
      }
    }
    private bool _ShowToolBar;

    /// <summary>
    /// Если свойство установлено, то выполняется автоматическое управление значком
    /// вкладки (свойство <see cref="EFPReportPage.ImageKey"/>), в зависимости от наличия в табличном просмотре
    /// строк с ошибками и предупреждениями.
    /// Установка свойства заставляет отчет обеспечить готовность данных на странице
    /// немедленно после построения отчета, а не при первом обращении пользователя 
    /// к странице, что может привести к замедлению построения.
    /// По умолчанию свойство не установлено.
    /// Если свойство установлено, то ручное задание свойства <see cref="EFPReportPage.ImageKey"/> не требуется.
    /// </summary>
    public bool AutoInitStateImageKey
    {
      get { return _AutoInitStateImageKey; }
      set
      {
        CheckPageNotCreated();
        _AutoInitStateImageKey = value;
        if (value)
          ImageKey = "UnknownState";
      }
    }
    private bool _AutoInitStateImageKey;


    private void ControlProvider_Validating(object sender, UICore.UIValidatingEventArgs args)
    {
      if (AutoInitStateImageKey)
      {
        if (_ControlProvider.UseRowImages && _ControlProvider.ShowRowCountInTopLeftCell)
          base.InitStateImageKey(_ControlProvider.TopLeftCellImageKind);
        else
          base.InitStateImageKey(_ControlProvider.GetAllRowsImageKind());
      }
    }


    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Запоминает конфигурацию табличного просмотра
    /// </summary>
    /// <param name="cfg">Записываемая секция</param>
    /// <returns>true, если не возникли ошибки</returns>
    public override bool SaveViewConfig(CfgPart cfg)
    {
      if (!base.SaveViewConfig(cfg))
        return false;

      if (_ControlProvider != null)
      {
        // TODO:
#if XXX
        if (AutoSaveOrder && FControlProvider.OrderCount > 0)
          FControlProvider.Orders.WriteConfig();
        if (!FControlProvider.SaveGridConfigIfRequired())
          return false;
#endif
      }
      return true;
    }

    #endregion
  }

#if XXXX
  /// <summary>
  /// Страница с иерархическим отчетом
  /// </summary>
  public class GridReportHiePage : EFPReportGridPage
  {
  #region Свойства

    /// <summary>
    /// Сюда должно быть присвоено значение вместо DataSource. При этом, если значение
    /// не равно null, будет вызван метод HieViewHandler.CreateView()
    /// </summary>
    public HieViewHandler HieHandler
    {
      get { return FHieHandler; }
      set
      {
        if (value == FHieHandler)
          return;
        RemoveHandlers();
        FHieHandler = value;
        AddHandlers();

        if (DataReady)
        {
          if (HieHandler == null)
            base.DataSource = null;
          else
          {
            DataView gv = HieHandler.CreateView();
            base.DataSource = gv;
          }
        }
      }
    }
    private HieViewHandler FHieHandler;

    private void AddHandlers()
    {
      if (GetRowAttributesHandler != null)
        return;
      if (HieHandler == null || ControlProvider == null)
        return;
      GetRowAttributesHandler = new EFPDataGridViewRowAttributesEventHandler(HieHandler.GetGridRowAttributes);
      GetCellAttributesHandler = new EFPDataGridViewCellAttributesEventHandler(HieHandler.GetGridCellAttributes);
      ControlProvider.GetRowAttributes += GetRowAttributesHandler;
      ControlProvider.GetCellAttributes += GetCellAttributesHandler;

      // 06.05.2009
      ControlProvider.SelectedRowsMode = EFPDataGridViewSelectedRowsMode.RowIndex;
    }

    private void RemoveHandlers()
    {
      if (GetRowAttributesHandler == null)
        return;
      ControlProvider.GetRowAttributes -= GetRowAttributesHandler;
      ControlProvider.GetCellAttributes -= GetCellAttributesHandler;
      GetRowAttributesHandler = null;
      GetCellAttributesHandler = null;
    }

    private EFPDataGridViewRowAttributesEventHandler GetRowAttributesHandler;
    private EFPDataGridViewCellAttributesEventHandler GetCellAttributesHandler;

  #endregion

  #region Переопределяемые методы

    protected override void InitData()
    {
      if (HieHandler == null)
        base.DataSource = null;
      else
      {
        DataView gv = HieHandler.CreateView();
        base.DataSource = gv;
      }
      base.InitData();
    }

    protected override void CreatePage(Panel Parent)
    {
      base.CreatePage(Parent);
      AddHandlers();
    }

    protected override void OnInitGrid()
    {
      base.OnInitGrid();
      AddHandlers();
    }

    protected override void RemoveGrid()
    {
      RemoveHandlers();
      base.RemoveGrid();
    }

  #endregion

  #region Реализация редактирования

    public void GridHandlerStdEditData(object Sender, EventArgs Args)
    {
      if (HieHandler != null)
        HieHandler.EditReportRow(ControlProvider);
    }

    public void GridHandlerStdGetDocSel(object Sender, EFPAccDepGridDocSelEventArgs Args)
    {
      if (HieHandler != null)
        HieHandler.OnGetDocSel(Sender, Args);
    }

    /// <summary>
    /// Реализация редактирования в иерархическом отчете по умолчанию.
    /// Разрешает просмотр и редактирование одной строки без добавления / удаления строк.
    /// Присоединяет обработчики GridHandler.EditData и GridHandler.CommandItems.GetDocSel.
    /// При этом будут вызываться события HieViewLevel.EditRow и GetDocSel соответственно
    /// Метод должен вызываться из события InitGrid
    /// </summary>
    public void InitStdEdit(InitGridEventArgs Args)
    {
      Args.ControlProvider.ReadOnly = false;
      Args.ControlProvider.CanInsert = false;
      Args.ControlProvider.CanDelete = false;
      Args.ControlProvider.CanView = true;
      Args.ControlProvider.CanMultiEdit = true;
      Args.ControlProvider.EditData += new EventHandler(GridHandlerStdEditData);
      Args.ControlProvider.GetDocSel += new EFPAccDepGridDocSelEventHandler(GridHandlerStdGetDocSel);
    }

  #endregion
  }

  /// <summary>
  /// Закладка с двумя табличными просмотрами
  /// </summary>
  public class GridReportTwoGridsPage : GridReportPage
  {
  #region Конструктор

    public GridReportTwoGridsPage()
    {
      AutoInitTopLeftCellInfo = true;
      AutoSaveOrder = true;
    }

  #endregion

  #region Переопределяемые методы

    /// <summary>
    /// Инициализация обоих табличных просмотров
    /// Используйте свойство GridReportTwoGridsPage.WhatGrid, чтобы определить, какая
    /// таблица инициализируется
    /// </summary>
    public event InitGridEventHandler InitGrid;

    /// <summary>
    /// Какая таблица сейчас инициализируется: 1-верхняя, 2-нижняя
    /// </summary>
    public int WhatGrid { get { return FWhatGrid; } }
    private int FWhatGrid;

    protected override void CreatePage(Panel Parent)
    {
      // Добавляем две панели с сепаратором
      SplitContainer TwoPanels = new SplitContainer();
      TwoPanels.Dock = DockStyle.Fill;
      TwoPanels.Orientation = Orientation.Horizontal;
      Parent.Controls.Add(TwoPanels);
      TwoPanels.SplitterIncrement = 8;
      WinFormsTools.SetSplitContainerDistancePercent(TwoPanels, 50);

      // Верхняя таблица
      FWhatGrid = 1;
      FMainPanel1 = new EFPControlWithToolBar<DataGridView>(BaseProvider, TwoPanels.Panel1);
      FGridHandler1 = new EFPAccDepGrid(BaseProvider, FMainPanel1.Control);
      FGridHandler1.Control.TabIndex = 1;
      if (FMainPanel1.ToolBarPanel != null)
      {
        FMainPanel1.ToolBarPanel.Visible = ShowToolBar1;
        if (ShowToolBar1)
          GridHandler1.ToolBarPanel = FMainPanel1.ToolBarPanel;
      }
      MyInitGrid(GridHandler1);

      Label1 = new Label();
      Label1.Dock = DockStyle.Top;
      Label1.AutoSize = false;
      Label1.Text = LabelText1;
      Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      Label1.Size = new System.Drawing.Size(10, 16);
      Label1.Visible = !String.IsNullOrEmpty(LabelText1);
      TwoPanels.Panel1.Controls.Add(Label1);

      // Нижняя таблица
      FWhatGrid = 2;
      FMainPanel2 = new EFPControlWithToolBar<DataGridView>(BaseProvider, TwoPanels.Panel2);
      FGridHandler2 = new EFPAccDepGrid(BaseProvider, FMainPanel2.Control);
      FGridHandler2.Control.TabIndex = 1;
      if (FMainPanel2.ToolBarPanel != null)
      {
        FMainPanel2.ToolBarPanel.Visible = ShowToolBar2;
        if (ShowToolBar2)
          GridHandler2.ToolBarPanel = FMainPanel2.ToolBarPanel;
      }
      MyInitGrid(GridHandler2);

      Label2 = new Label();
      Label2.Dock = DockStyle.Top;
      Label2.AutoSize = false;
      Label2.Text = LabelText2;
      Label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      Label2.Size = new System.Drawing.Size(10, 16);
      TwoPanels.Panel2.Controls.Add(Label2);
      Label2.Visible = !String.IsNullOrEmpty(LabelText2);
    }

    private void MyInitGrid(EFPAccDepGrid ControlProvider)
    {
      ControlProvider.Control.AutoGenerateColumns = false;
      ControlProvider.Control.ReadOnly = true;
      ControlProvider.Control.AllowUserToAddRows = false;
      ControlProvider.Control.AllowUserToDeleteRows = false;

      ControlProvider.ReadOnly = true;
      ControlProvider.CanView = false;

      ControlProvider.Control.MultiSelect = true; // после создания DocGridHandler
      ControlProvider.CommandItems.UseRefresh = false; // обрабатывается на уровне отчета в целом
      GridPageSetup PageSetup = ControlProvider.AddGridPrint();
      PageSetup.DocumentName = Title;
      string s = (WhatGrid == 1 ? LabelText1 : LabelText2);
      if (String.IsNullOrEmpty(s))
        s = WhatGrid.ToString();

      if (InitGrid != null)
      {
        InitGridEventArgs Args = new InitGridEventArgs(ControlProvider);
        InitGrid(this, Args);
      }

      ControlProvider.GetReadyCommandItems();
      if (AutoSaveOrder && ControlProvider.OrderCount > 0)
        ControlProvider.Orders.ReadConfig();
    }

    protected override void InitData()
    {
      base.InitData();

      if (DataSource1 != null)
      {
        EFPDataGridViewSelection OldSel = FGridHandler1.Selection;
        bool HasData = FGridHandler1.Control.DataSource != null;

        GridHandler1.Control.DataSource = DataSource1;
        if (AutoInitTopLeftCellInfo)
          FGridHandler1.InitTopLeftCellTotalInfo();

        if (HasData && DataSource1 != null)
          FGridHandler1.Selection = OldSel;
      }

      if (DataSource2 != null)
      {
        EFPDataGridViewSelection OldSel = FGridHandler2.Selection;
        bool HasData = FGridHandler2.Control.DataSource != null;

        GridHandler2.Control.DataSource = DataSource2;
        if (AutoInitTopLeftCellInfo)
          FGridHandler2.InitTopLeftCellTotalInfo();

        if (HasData && DataSource2 != null)
          FGridHandler2.Selection = OldSel;
      }
    }

    public override bool SaveViewConfig()
    {
      if (!base.SaveViewConfig())
        return false;

      if (FGridHandler1 != null)
      {
        //FGridHandler1.OrderCount
        if (AutoSaveOrder && FGridHandler1.OrderCount > 0)
          FGridHandler1.Orders.WriteConfig();
        if (!FGridHandler1.SaveGridConfigIfRequired())
          return false;
      }
      if (FGridHandler2 != null)
      {
        if (AutoSaveOrder && FGridHandler2.OrderCount > 0)
          FGridHandler2.Orders.WriteConfig();
        if (!FGridHandler2.SaveGridConfigIfRequired())
          return false;
      }
      return true;
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Сюда должен быть помещен набор данных, который будет отображен в верхней таблице
    /// </summary>
    public DataView DataSource1
    {
      get { return FDataSource1; }
      set
      {
        FDataSource1 = value;
        if (FGridHandler1 != null)
        {
          EFPDataGridViewSelection OldSel = FGridHandler1.Selection;
          bool HasData = FGridHandler1.Control.DataSource != null;

          FGridHandler1.Control.DataSource = value;
          if (AutoInitTopLeftCellInfo)
            FGridHandler1.InitTopLeftCellTotalInfo();

          if (HasData && value != null)
            FGridHandler1.Selection = OldSel;
        }
      }
    }
    private DataView FDataSource1;

    /// <summary>
    /// Сюда должен быть помещен набор данных, который будет отображен в нижней таблице
    /// </summary>
    public DataView DataSource2
    {
      get { return FDataSource2; }
      set
      {
        FDataSource2 = value;
        if (FGridHandler2 != null)
        {
          EFPDataGridViewSelection OldSel = FGridHandler2.Selection;
          bool HasData = FGridHandler2.Control.DataSource != null;

          FGridHandler2.Control.DataSource = value;
          if (AutoInitTopLeftCellInfo)
            FGridHandler2.InitTopLeftCellTotalInfo();

          if (HasData && value != null)
            FGridHandler2.Selection = OldSel;
        }
      }
    }
    private DataView FDataSource2;

    public EFPAccDepGrid GridHandler1 { get { return FGridHandler1; } }
    private EFPAccDepGrid FGridHandler1;

    public EFPAccDepGrid GridHandler2 { get { return FGridHandler2; } }
    private EFPAccDepGrid FGridHandler2;

    private EFPControlWithToolBar<DataGridView> FMainPanel1;

    private EFPControlWithToolBar<DataGridView> FMainPanel2;


    /// <summary>
    /// Если установить в true, то будет видна панель кнопок для верхней таблицы
    /// По умолчанию - false - без кнопок.
    /// Не рекомендуется устанавливать свойства ShowToolBar1, ShowToolBar1, LabelText1
    /// и LabelText2 одновременно, т.к. на экране может оказаться недостаточно места
    /// для отображения страниц
    /// </summary>
    public bool ShowToolBar1;

    /// <summary>
    /// Если установить в true, то будет видна панель кнопок для нижней таблицы
    /// По умолчанию - false - без кнопок.
    /// Не рекомендуется устанавливать свойства ShowToolBar1, ShowToolBar1, LabelText1
    /// и LabelText2 одновременно, т.к. на экране может оказаться недостаточно места
    /// для отображения страниц
    /// </summary>
    public bool ShowToolBar2;


    /// <summary>
    /// Метка для верхней таблицы (например, "Дебет")
    /// Метка скрывается, если пустая строка.
    /// Возможно динамическое изменение текста метки в процессе просмотра отчета
    /// </summary>
    public string LabelText1
    {
      get { return FLabelText1; }
      set
      {
        FLabelText1 = value;
        if (Label1 != null)
        {
          Label1.Text = value;
          Label1.Visible = !String.IsNullOrEmpty(value);
        }
      }
    }
    private string FLabelText1;

    /// <summary>
    /// Метка для нижней таблицы (например, "Кредит")
    /// Метка скрывается, если пустая строка.
    /// Возможно динамическое изменение текста метки в процессе просмотра отчета
    /// </summary>
    public string LabelText2
    {
      get { return FLabelText2; }
      set
      {
        FLabelText2 = value;
        if (Label2 != null)
        {
          Label2.Text = value;
          Label2.Visible = !String.IsNullOrEmpty(value);
        }
      }
    }
    private string FLabelText2;

    private Label Label1, Label2;

    /// <summary>
    /// Если свойство установлено (по умолчанию), а при инициализации просмотра
    /// было установлено свойство GridHandler1/2.UseRowImages, то в верехней левой
    /// ячейке просмотра будет выводиться подсказка о количестве ошибок и 
    /// предупреждений. Информация обновляется при присоединении источника данных
    /// с помощью свойства DataSource1/2.
    /// Чтобы вручную устанавливать значение ячейки или вообще не выводить статистику,
    /// свойство должно быть сброшено в false
    /// </summary>
    public bool AutoInitTopLeftCellInfo;

    /// <summary>
    /// Автоматическая загрузка и сохранение порядка сортировки строк в секции 
    /// конфигурации (по умолчанию - true), если для есть установленные 
    /// GridHandler.Orders. Если требуется, чтобы страница отчета при каждом
    /// построении имела определенный порядок строк, а не запомненный ранее,
    /// следует установить значение false до присоединения страницы к отчету
    /// </summary>
    public bool AutoSaveOrder;

  #endregion
  }
#endif

  /// <summary>
  /// Закладка со списком ошибок <see cref="EFPErrorDataGridView"/>.
  /// </summary>
  public class EFPReportErrorMessageListPage : EFPReportPage
  {
    #region Конструктор

    /// <summary>
    /// Создает страницу отчета
    /// </summary>
    public EFPReportErrorMessageListPage()
    {
      _AutoImageKey = true;
      _AutoTitle = true;
      _AutoToolTipText = true;

      InitImageAndTitle(); // начальные значения
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основное свойство - присоединенный список сообщений.
    /// Установка свойства задает заголовок, значок и всплывающую подсказку вкладки, если установлены соответствующие свойства <see cref="AutoTitle"/>, <see cref="AutoImageKey"/> и <see cref="AutoToolTipText"/>.
    /// Допускается установка значения null в начале построения / перестроения отчета, чтобы показать пользователю сообщение, что список ошибок еще не присоединен.
    /// </summary>
    public ErrorMessageList ErrorMessages
    {
      get { return _ErrorMessages; }
      set
      {
        _ErrorMessages = value;
        if (_ControlProvider != null)
          _ControlProvider.ErrorMessages = value;
        InitImageAndTitle();
      }
    }

    private ErrorMessageList _ErrorMessages;

    /// <summary>
    /// Табличный просмотр со списком ошибок
    /// </summary>
    private EFPErrorDataGridView _ControlProvider;

    /// <summary>
    /// Ширина области для вывода кодов ошибок в текстовых единицах.
    /// По умолчанию - 0 - коды не выводятся.
    /// Свойство должно устанавливаться после вызова конструктора до добавления страницы к отчету.
    /// </summary>
    public int CodeWidth
    {
      get { return _CodeWidth; }
      set
      {
        CheckPageNotCreated();
        _CodeWidth = value;
      }
    }
    private int _CodeWidth;

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то значок закладки
    /// устанавливается автоматически при присоединении списка. Чтобы установить
    /// свойство <see cref="EFPReportPage.ImageKey"/> вручную, сначала следует задать <see cref="AutoImageKey"/>=false.
    /// Свойство должно устанавливаться после вызова конструктора до добавления страницы к отчету.
    /// </summary>
    public bool AutoImageKey
    {
      get { return _AutoImageKey; }
      set
      {
        CheckPageNotCreated();
        _AutoImageKey = value;
      }
    }
    private bool _AutoImageKey;

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то заголовок закладки
    /// устанавливается автоматически при присоединении списка. Чтобы установить
    /// свойство <see cref="EFPReportPage.Title"/> вручную, сначала следует задать <see cref="AutoTitle"/>=false.
    /// Свойство должно устанавливаться после вызова конструктора до добавления страницы к отчету.
    /// </summary>
    public bool AutoTitle
    {
      get { return _AutoTitle; }
      set
      {
        CheckPageNotCreated();
        _AutoTitle = value;
      }
    }
    private bool _AutoTitle;

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то всплывающие подсказки заголовка закладки
    /// устанавливается автоматически при присоединении списка. Чтобы установить
    /// свойство <see cref="EFPReportPage.ToolTipText"/> вручную, сначала следует задать <see cref="AutoToolTipText"/>=false
    /// Свойство должно устанавливаться после вызова конструктора до добавления страницы к отчету.
    /// </summary>
    public bool AutoToolTipText
    {
      get { return _AutoToolTipText; }
      set
      {
        CheckPageNotCreated();
        _AutoToolTipText = value;
      }
    }
    private bool _AutoToolTipText;

    #endregion

    #region События

    /// <summary>
    /// Обработчик "редактирования" сообщения об ошибке.
    /// Если обработчик не присоединен, то пользователь не может "войти" в сообщение.
    /// Обработчик должен быть установлен до присоединения страницы к отчету.
    /// </summary>
    public event ErrorMessageItemEventHandler EditMessage
    {
      add
      {
        CheckPageNotCreated();
        _EditMessage += value;
      }
      remove
      {
        CheckPageNotCreated();
        _EditMessage -= value;
      }
    }
    private ErrorMessageItemEventHandler _EditMessage;

    private void OnEditMessage(object sender, ErrorMessageItemEventArgs args)
    {
      if (_EditMessage!=null)
        _EditMessage(this, args);
    }

    ///// <summary>
    ///// Обработчик для связи строк ошибок с документами в базе данных (команда
    ///// "Отправить-Выборка" и копирование ссылок в буфер обмена)
    ///// Обработчик должен быть установлен до присоединения страницы к отчету
    ///// </summary>
    // TODO: public event ErrorMessagesDocSelEventHandler GetDocSel;

    /*
    private void OnGetDocSel(object Sender, ErrorMessagesDocSelEventArgs Args)
    {
      GetDocSel(this, Args);
    }
      */
    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Создает страницу отчета
    /// </summary>
    /// <param name="parent">Панель для размещения страницы</param>
    protected override void CreatePage(Panel parent)
    {
      DataGridView control = new DataGridView();
      control.Dock = DockStyle.Fill;
      parent.Controls.Add(control);

      _ControlProvider = CreateControlProvider(control);
      _ControlProvider.CodeWidth = CodeWidth;

      if (_EditMessage != null)
        _ControlProvider.EditMessage += new ErrorMessageItemEventHandler(OnEditMessage);

      //if (GetDocSel != null)
      //  FControlProvider.GetDocSel += OnGetDocSel;
      _ControlProvider.CommandItems.UseRefresh = false; // обрабатывается на уровне отчета в целом

      // 31.08.2016. Необходимо вернуть EnterAsOk в исходное состояние принудительно.
      // EFPErrorDataGridView почему-то не выставляет свойство при установке EditHandler (выше)
      _ControlProvider.CommandItems.EnterAsOk = false;

      if (_ControlProvider.DefaultOutItem != null)
        _ControlProvider.DefaultOutItem.TitleNeeded += DefaultOutItem_TitleNeeded;

      //FControlProvider.GridPageSetup.BeforePrinting += new CancelEventHandler(GridPageSetup_BeforePrinting);
      if (_ErrorMessages != null)
        _ControlProvider.ErrorMessages = _ErrorMessages;
      InitImageAndTitle();

      SetControlName(_ControlProvider, "Grid");
    }

    /// <summary>
    /// Создает объект <see cref="EFPErrorDataGridView"/>
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <returns>Провайдер</returns>
    protected virtual EFPErrorDataGridView CreateControlProvider(DataGridView control)
    {
      return new EFPErrorDataGridView(BaseProvider, control);
    }

    #endregion

    #region Управление заголовком и значком

    private void InitImageAndTitle()
    {
      if (AutoImageKey)
        InitImageKey();
      if (AutoTitle)
        InitTitle();
      if (AutoToolTipText)
        InitToolTipText();
    }

    /// <summary>
    /// Установить свойство <see cref="EFPReportPage.ImageKey"/> в соответствии с присоединенным списком <see cref="ErrorMessages"/>
    /// </summary>
    public void InitImageKey()
    {
      ImageKey = EFPApp.GetErrorImageKey(ErrorMessages);
    }

    /// <summary>
    /// Установить свойство <see cref="EFPReportPage.Title"/> в соответствии с присоединенным списком <see cref="ErrorMessages"/>
    /// </summary>
    public void InitTitle()
    {
      Title = EFPApp.GetErrorTitleText(ErrorMessages);
    }

    /// <summary>
    /// Установить свойство <see cref="EFPReportPage.ToolTipText"/> в соответствии с присоединенным списком <see cref="ErrorMessages"/>
    /// </summary>
    public void InitToolTipText()
    {
      ToolTipText = EFPApp.GetErrorToolTipText(ErrorMessages);
    }

    #endregion

    #region Печать таблицы

#if XXX
    void GridPageSetup_BeforePrinting(object Sender, CancelEventArgs Args)
    {
      if ((!AutoToolTipText) && (!String.IsNullOrEmpty(ToolTipText)))
        FControlProvider.GridPageSetup.Title = ToolTipText;
    }
#endif
    #endregion
  }
}
