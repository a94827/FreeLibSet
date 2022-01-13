// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

#define DEBUG_FILTERS // Определено для отладки исчезновения фильтров при загрузке композиции

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using System.Data;
using System.ComponentModel;
using FreeLibSet.Logging;
using FreeLibSet.Config;
using FreeLibSet.Remoting;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{
  #region Интерфейс IEFPDocView

  /// <summary>
  /// Интерфейс для просмотра документов.
  /// Реализуется EFPDocGridView и EFPDocTreeView
  /// </summary>
  public interface IEFPDocView : IEFPDBxView
  {
    /// <summary>
    /// Возвращает интерфейс для вида документа
    /// </summary>
    DocTypeUI DocTypeUI { get; }

    /// <summary>
    /// Возвращает объект вида документа
    /// </summary>
    DBxDocType DocType { get; }

    /// <summary>
    /// Возвращает фильтры табличного просмотра
    /// </summary>
    new GridFilters Filters { get; set; }

    /// <summary>
    /// true, если в просмотре показываются документы, помеченные на удаление.
    /// Если DBxDocTypes.UseDeleted=false, свойство всегда равно false и не может быть установлено в true.
    /// </summary>
    bool ShowDeleted { get; set; }

    /// Это свойство управляет наличием команды "Фильтр" - "Показывать удаленные записи".
    /// Значение по умолчанию - true (команда присутствует).
    /// Если DBxDocTypes.UseDeleted=false, свойство всегда равно false и не может быть установлено в true.
    bool UseShowDeleted { get; set; }

    /// <summary>
    /// Событие вызывается, когда пользователь нажимает кнопку "Показывать удаленные записи".
    /// </summary>
    event EventHandler ShowDeletedChanged;

    /// <summary>
    /// Произвольные пользовательские данные, переданные конструктору EFPDocGridView / EFPDocTreeView. 
    /// </summary>
    object UserInitData { get; }

    /// <summary>
    /// Внешний инициализатор для новых документов
    /// Если свойство установлено, то при создании нового документа в качестве
    /// инициализатора значений полей (аргумент Caller при вызове ClientDocType.PerformEdit()) 
    /// будет использован этот инициализатор вместо текущих фильтров (DocGridHandler.DocFilters)
    /// Свойство может устанавливаться только до вывода просмотра на экран
    /// </summary>
    DocumentViewHandler ExternalEditorCaller { get; }

    /// <summary>
    /// Идентификатор просмотра.
    /// Используется ViewHandler
    /// </summary>
    Guid BrowserGuid { get; }

    /// <summary>
    /// Дополнительный фильтр по идентификатором групп документов.
    /// Значение null означает отсутствие фильтра ("Все документы").
    /// Массив нулевой длины означает фильтр "Документы без группы".
    /// Свойство можно устанавливать только для документов, у которых используются группы.
    /// Этот фильтр не входит в собычный список фильтров Filters. Событие FilterChanged не вызывается.
    /// Запрос на обновление данных просмотра не посылается, вместо этого накладывается дополнительный
    /// фильтр на уже существующий DataView.
    /// </summary>
    Int32[] AuxFilterGroupIds { get; set; }
  }

  #endregion

  /// <summary>
  /// Табличный просмотр для работы с документами
  /// </summary>
  public class EFPDocGridView : EFPDBxGridView, IEFPDocView
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент - дерево</param>
    /// <param name="ui">Интерфейс доступа к документам</param>
    /// <param name="docTypeName">Имя вида документов</param>
    public EFPDocGridView(EFPBaseProvider baseProvider, DataGridView control, DBUI ui, string docTypeName)
      : this(baseProvider, control, ui.DocTypes[docTypeName], String.Empty, null)
    {
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент - дерево</param>
    /// <param name="docTypeUI">Интерфейс доступа к виду документов</param>
    public EFPDocGridView(EFPBaseProvider baseProvider, DataGridView control, DocTypeUI docTypeUI)
      : this(baseProvider, control, docTypeUI, String.Empty, null)
    {
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент - дерево</param>
    /// <param name="docTypeUI">Интерфейс доступа к виду просматриваемых документов</param>
    /// <param name="defaultConfigName">Имя фиксированной настройки табличного просмотра</param>
    public EFPDocGridView(EFPBaseProvider baseProvider, DataGridView control, DocTypeUI docTypeUI, string defaultConfigName)
      : this(baseProvider, control, docTypeUI, defaultConfigName, null)
    {
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент - дерево</param>
    /// <param name="docTypeUI">Интерфейс доступа к виду просматриваемых документов</param>
    /// <param name="defaultConfigName">Имя фиксированной настройки табличного просмотра</param>
    /// <param name="userInitData">Пользовательские данные передаются обработчику инициализации табличного просмотра</param>
    public EFPDocGridView(EFPBaseProvider baseProvider, DataGridView control, DocTypeUI docTypeUI, string defaultConfigName, object userInitData)
      : base(baseProvider, control, docTypeUI.UI)
    {
      Init(docTypeUI, defaultConfigName, userInitData);
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    /// <param name="ui">Интерфейс доступа к документам</param>
    /// <param name="docTypeName">Имя вида документов</param>
    public EFPDocGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar, DBUI ui, string docTypeName)
      : this(controlWithToolBar, ui.DocTypes[docTypeName], String.Empty, null)
    {
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    /// <param name="docTypeUI">Интерфейс доступа к виду просматриваемых документов</param>
    public EFPDocGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar, DocTypeUI docTypeUI)
      : this(controlWithToolBar, docTypeUI, String.Empty, null)
    {
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    /// <param name="docTypeUI">Интерфейс доступа к виду просматриваемых документов</param>
    /// <param name="defaultConfigName">Имя фиксированной настройки табличного просмотра</param>
    public EFPDocGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar, DocTypeUI docTypeUI, string defaultConfigName)
      : this(controlWithToolBar, docTypeUI, defaultConfigName, null)
    {
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    /// <param name="docTypeUI">Интерфейс доступа к виду просматриваемых документов</param>
    /// <param name="defaultConfigName">Имя фиксированной настройки табличного просмотра</param>
    /// <param name="userInitData">Пользовательские данные передаются обработчику инициализации табличного просмотра</param>
    public EFPDocGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar, DocTypeUI docTypeUI, string defaultConfigName, object userInitData)
      : base(controlWithToolBar, docTypeUI.UI)
    {
      Init(docTypeUI, defaultConfigName, userInitData);
    }

    private void Init(DocTypeUI docTypeUI, string defaultConfigName, object userInitData)
    {
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");

      _DocTypeUI = docTypeUI;
      _UserInitData = userInitData;

      base.GridProducer = docTypeUI.GridProducer; // 25.03.2021
      base.ConfigSectionName = DocType.Name;
      base.DefaultConfigName = DefaultConfigName;
      base.ReadOnly = docTypeUI.UI.DocProvider.DBPermissions.TableModes[DocType.Name] != DBxAccessMode.Full;
      base.CanView = true;
      base.EditData += new EventHandler(MyEdit);
      base.CanMultiEdit = docTypeUI.CanMultiEdit;
      base.CanInsertCopy = docTypeUI.CanInsertCopy;
      Control.ReadOnly = true;
      //?? не видно текущий столбец для IncSearch ?? MainGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      Control.AutoGenerateColumns = false;
      Control.AllowUserToDeleteRows = false;
      Control.AllowUserToAddRows = false;
      Control.ShowEditingIcon = false;
      //MainGrid.RowHeadersWidth = 25;
      Control.MultiSelect = true; // Можно удалять несколько записей, но не всегда можно редактировать

      Control.VirtualMode = true;
      Control.CellValueNeeded += new DataGridViewCellValueEventHandler(Control_CellValueNeeded);
      Control.DataSourceChanged += new EventHandler(Control_DataSourceChanged);

      _BrowserGuid = Guid.NewGuid();

      // Дополнительные команды меню (до вызова InitView, иначе кнопки будут не по порядку)

      if (docTypeUI.UI.DocProvider.DocTypes.UseDeleted)
      {
        ciShowDeleted = new EFPCommandItem("View", "ShowHiddenDocs");
        ciShowDeleted.MenuText = "Показывать удаленные записи";
        ciShowDeleted.ToolTipText = "Показывать удаленные записи";
        ciShowDeleted.Parent = CommandItems.MenuFilter;
        ciShowDeleted.ImageKey = "ShowHiddenDocs";
        ciShowDeleted.Click += new EventHandler(ShowDeletedClick);
        CommandItems.Add(ciShowDeleted);
      }
      //EFPCommandItem ci = new EFPCommandItem("Edit", "MergeDocs");
      //ci.MenuText = "Объединить записи";
      //ci.ImageKey = "MergeDocs";
      //ci.Enabled = false; // TODO: AccDepClientExec.Permissions.Merger[DocTypeName];
      //ci.Click += new EventHandler(RowMerge_Click);
      //CommandItems.Add(ci);

      base.ShowRowCountInTopLeftCellToolTipText = true; // 15.12.2017

      CustomOrderAllowed = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс для работы с документами заданного вида
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private DocTypeUI _DocTypeUI;

    /// <summary>
    /// Тип просматриваемого документа
    /// </summary>
    public DBxDocType DocType { get { return _DocTypeUI.DocType; } }

    /// <summary>
    /// Эти данные передаются обработчику инициализации табличного просмотра
    /// </summary>
    public object UserInitData { get { return _UserInitData; } }
    private object _UserInitData;

    /// <summary>
    /// Имена столбцов, необходимых для просмотра.
    /// Список заполняется после инициализации просмотра с помощью EFPGridProducer.
    /// Чтобы гарантировать заполнение свойства, можно использовать метод PerformInitGrid().
    /// </summary>
    public DBxColumns UsedColumnNames { get { return _UsedColumnNames; } }
    private DBxColumns _UsedColumnNames;

    /// <summary>
    /// Ограничение на максимальное число строк в просмотре.
    /// Если установленные фильтры дают в результате количество строк, превышающее ограничение,
    /// при загрузке данных выдается предупреждение
    /// </summary>
    public int MaxRecordCount
    {
      get { return _MaxRecordCount; }
      set
      {
        CheckHasNotBeenCreated();
        if (value < 0)
          throw new ArgumentOutOfRangeException();
        if (value == int.MaxValue)
          value = 0;
        _MaxRecordCount = value;
      }
    }
    private int _MaxRecordCount;

    #endregion

    #region Показывать удаленные записи

    /// <summary>
    /// Это свойство позволяет динамически управлять показом удаленных записей
    /// В основном требуется для реализации интерфейса IDocTableBrowser
    /// Значение по умолчанию - false (удаленные строки не показываются)
    /// Для управления наличием команды локального меню "Показывать удаленные записи"
    /// используется свойство UseShowDeleted
    /// </summary>
    public bool ShowDeleted
    {
      get { return _ShowDeleted; }
      set
      {
        if (value == _ShowDeleted)
          return;
        _ShowDeleted = value;
        if (ciShowDeleted != null) // 23.04.2018 - добавлено условие
          ciShowDeleted.Checked = value;
        OnShowDeletedChanged(EventArgs.Empty);
        if (HasBeenCreated) // 16.02.2018 - добавлено условие
          PerformRefresh();
      }
    }
    private bool _ShowDeleted;

    /// <summary>
    /// Это событие вызывается при изменении свойства ShowDeleted
    /// </summary>
    public event EventHandler ShowDeletedChanged;

    /// <summary>
    /// Вызывает событие ShowDeletedChanged
    /// </summary>
    /// <param name="args">Фиктивный аргумент</param>
    protected virtual void OnShowDeletedChanged(EventArgs args)
    {
      if (ShowDeletedChanged != null)
        ShowDeletedChanged(this, args);
    }

    /// <summary>
    /// Это свойство управляет наличием команды "Фильтр" - "Показывать удаленные записи".
    /// Значение по умолчанию - true (команда присутствует)
    /// </summary>
    public bool UseShowDeleted
    {
      get
      {
        if (ciShowDeleted == null)
          return false;
        else
          return ciShowDeleted.Visible;
      }
      set
      {
        if (ciShowDeleted == null)
        {
          if (value)
            throw new InvalidOperationException("Просмотр удаленных документов не поддерживается");
        }
        else
          ciShowDeleted.Visible = value;
      }
    }
    private EFPCommandItem ciShowDeleted;

    void ShowDeletedClick(object sender, EventArgs args)
    {
      ShowDeleted = !ShowDeleted;
    }

    #endregion

    #region Присоединение источника данных

    /// <summary>
    /// Первоначальная загрузка или обновление данных по запросу
    /// </summary>
    /// <param name="args"></param>
    protected override void OnRefreshData(EventArgs args)
    {
      if (SourceAsDataTable == null)
        EFPApp.BeginWait("Загрузка данных", DocTypeUI.TableImageKey);
      else
        EFPApp.BeginWait("Обновление данных", "Refresh");
      try
      {
        if (SourceAsDataView != null)
        {
          // Реальная перезагрузка, а не первоначальный доступ
          UI.TextHandlers.DBCache.Clear();
          DocTypeUI.RefreshBufferedData();
        }

        PerformInitGrid(false);
        OnInitSourceDataView();
      }
      finally
      {
        EFPApp.EndWait();
      }

      base.OnRefreshData(args);
    }

    /// <summary>
    /// Вызывается из OnRefreshData(), чтобы присвоить значение Control.DataSource.
    /// Переопределяется редактором выборки документов
    /// </summary>
    protected virtual void OnInitSourceDataView()
    {
      DataTable masterTable;
      if (FixedDocIds == null)
      {
        // Получаем данные для просмотра с помощью фильтра
        DBxFilter SqlFilter = Filters.GetSqlFilter();
        using (DataView dv = DocTypeUI.CreateDataView(UsedColumnNames,
          SqlFilter,
          _ShowDeleted,
          DocType.DefaultOrder, MaxRecordCount))
        {
          if (DataTools.GetBool(dv.Table.ExtendedProperties["Limited"]))
          {
            LogoutRowCountLimited(SqlFilter);
          }
          //DebugTools.DebugDataView(dv, "DV");

          if (String.IsNullOrEmpty(dv.RowFilter))
            masterTable = dv.Table;
          else
          {
            masterTable = dv.ToTable(); // нужна копия таблицы
          }
        }
      }
      else
      {
        // Получаем данные для просмотра по фиксированному списку документов
        masterTable = DocTypeUI.TableCache.CreateTable(FixedDocIds.ToArray(), this.UsedColumnNames);
      }
      DataTools.CheckPrimaryKey(masterTable, "Id");

      base.InitTableRepeaterForGridProducer(masterTable);
    }

    #region Отладка ограничения количества строк в просмотре

    // TODO: Это только для отладки. Надо будет убрать, когда пойму, почем выходит "левое" сообщением об ошибке

    private void LogoutRowCountLimited(DBxFilter SqlFilter)
    {
      try
      {
        Exception e = new Exception("Количество строк в просмотре было ограничено");
        e.Data["UsedColumnNames"] = UsedColumnNames;
        e.Data["SqlFilter"] = SqlFilter;

        Dictionary<string, string> filts = new Dictionary<string, string>();
        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
        foreach (IEFPGridFilter f in Filters)
          filts.Add(f.DisplayName, f.FilterText);
        e.Data["GridFilters"] = filts;

        TempCfg Cfg = new TempCfg();
        Filters.WriteConfig(Cfg);
        e.Data["FiltersConfig"] = Cfg.AsXmlText;
        e.Data["SaveFiltersAllowed"] = base.SaveFiltersAllowed;
        e.Data["CommandItems.CanEditFilters"] = CommandItems.CanEditFilters;
#if DEBUG_FILTERS
        e.Data["DebugFiltersCfgXml"] = _DebugFiltersCfgXml;
        e.Data["DebugFiltersCfgCategories"] = _DebugFiltersCfgCategories;
        e.Data["DebugFiltersStackTrace"] = _DebugFiltersStackTrace;
#endif

        e.Data["ShowDeleted"] = ShowDeleted;
        e.Data["MaxRecordCount"] = MaxRecordCount;
        e.Data["StackTrace"] = Environment.StackTrace;
        throw e;
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e, "Количество строк в просмотре было ограничено");
      }
      EFPApp.WarningMessageBox("Количество строк в просмотре было ограничено до " + MaxRecordCount.ToString() + ". Используйте фильтрацию документов", DisplayName);
    }

#if DEBUG_FILTERS
    /// <summary>
    /// Для отладки
    /// </summary>
    /// <param name="categories"></param>
    /// <param name="rwMode"></param>
    /// <param name="actionInfo"></param>
    public override void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      base.GetConfigCategories(categories, rwMode, actionInfo);
      if (rwMode == EFPConfigMode.Read && _DebugFiltersCfgXml == null)
      {
        string[] a = new string[categories.Count];
        categories.CopyTo(a, 0);
        _DebugFiltersCfgCategories = String.Join(", ", a);
        _DebugFiltersStackTrace = Environment.StackTrace;
      }
    }

    private string _DebugFiltersCfgCategories;
    private string _DebugFiltersStackTrace;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <param name="category"></param>
    /// <param name="cfg"></param>
    /// <param name="actionInfo"></param>
    public override void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      base.ReadConfigPart(category, cfg, actionInfo);
      if (category == EFPConfigCategories.Filters)
      {
        TempCfg cfg2 = new TempCfg();
        cfg.CopyTo(cfg2);
        _DebugFiltersCfgXml = cfg2.AsXmlText;
      }
    }

    private string _DebugFiltersCfgXml;
#endif

    #endregion

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool HasRefreshDataHandler { get { return true; } }

    ///// <summary>
    ///// Возвращает true, если в таблице есть первичный ключ по полю Id
    ///// </summary>
    //internal bool HasPrimaryKey
    //{
    //  get
    //  {
    //    if (SourceAsDataTable == null)
    //      return false;
    //    return String.Compare(DataTools.GetPrimaryKey(SourceAsDataTable), "Id", StringComparison.OrdinalIgnoreCase) == 0;
    //  }
    //}

    #endregion

    #region Значения для полей

    // 02.06.2015
    // Табличный просмотр может использоваться не для нормального просмотра документов, когда все столбцы,
    // включая ссылочные, присутствуют в таблице данных.
    // Просмотр может быть использован, например в отчете, когда есть только столбец Id

    void Control_CellValueNeeded(object sender, DataGridViewCellValueEventArgs args)
    {
      try
      {
        if (SourceAsDataView == null)
          return;
        if (args.RowIndex < 0 || args.RowIndex >= SourceAsDataView.Count || args.ColumnIndex < 0)
          return;
        string PropName = Control.Columns[args.ColumnIndex].DataPropertyName;
        if (string.IsNullOrEmpty(PropName))
          return;

        if (!Char.IsLetterOrDigit(PropName, 0))
          return; // 30.03.2021

        DataRow Row = GetDataRow(args.RowIndex);
        if (Row.Table.Columns.Contains(PropName))
          return;

        Int32 Id = DataTools.GetInt(Row, "Id");
        args.Value = DocTypeUI.TableCache.GetValue(Id, PropName, Row.Table.DataSet);
      }
      catch (Exception e)
      {
        EFPApp.ShowTempMessage(e.Message);
      }
    }

    /// <summary>
    /// Создает объект DBxDataRowValueArrayWithCache()
    /// </summary>
    /// <returns>Новый объект для доступа к данным</returns>
    protected override IDataRowNamedValuesAccess CreateRowValueAccessObject()
    {
      return new DBxDataRowValueArrayWithCache(DocTypeUI.TableCache);
    }

    #endregion

    #region Расширение свойства Filters

    /// <summary>
    /// Дополнительный фильтр по идентификатором групп документов.
    /// Значение null означает отсутствие фильтра ("Все документы").
    /// Массив нулевой длины означает фильтр "Документы без группы".
    /// Свойство можно устанавливать только для документов, у которых используются группы.
    /// Этот фильтр не входит в собычный список фильтров Filters. Событие FilterChanged не вызывается.
    /// Запрос на обновление данных просмотра не посылается, вместо этого накладывается дополнительный
    /// фильтр на уже существующий DataView.
    /// </summary>
    public Int32[] AuxFilterGroupIds
    {
      get { return _AuxFilterGroupIds; }
      set
      {
        if (value != null && DocTypeUI.GroupDocType == null)
          throw new InvalidOperationException("Нельзя устанавливать свойство AuxFilterGroupIds для просмотра документов \"" + DocTypeUI.DocType.PluralTitle + "\"");

        _AuxFilterGroupIds = value;
        if (SourceAsDataView != null)
        {
          EFPDataGridViewSelection OldSel = Selection;
          if (AuxFilterGroupIds == null)
            SourceAsDataView.RowFilter = _OriginalRowFilter;
          else
          {
            if (_AuxFilterGroupIds.Length == 0)
            {
              ValueFilter Filter2 = new ValueFilter(DocTypeUI.DocType.GroupRefColumnName, 0);
              SourceAsDataView.RowFilter = Filter2.AddToDataViewRowFilter(_OriginalRowFilter);
            }
            else
            {
              IdsFilter Filter2 = new IdsFilter(DocTypeUI.DocType.GroupRefColumnName, AuxFilterGroupIds);
              SourceAsDataView.RowFilter = Filter2.AddToDataViewRowFilter(_OriginalRowFilter);
            }
          }
          Selection = OldSel; // по возможности

          // 24.11.2017
          // 15.12.2017
          // Вывод подсказки реализован в EFPDataGridView
          //TopLeftCellToolTipText = "Документов в просмотре: " + SourceAsDataView.Count.ToString();
        }
      }
    }
    private Int32[] _AuxFilterGroupIds;

    private string _OriginalRowFilter;

    void Control_DataSourceChanged(object sender, EventArgs args)
    {
      if (Control.DataSource != null)
      {
        _OriginalRowFilter = SourceAsDataView.RowFilter;
        AuxFilterGroupIds = _AuxFilterGroupIds; // доинициализация фильтра
      }
      else
        _OriginalRowFilter = String.Empty;
    }

    #endregion

    #region Фиксированный массив идентификаторов

    /// <summary>
    /// Фиксированный набор идентификаторов.
    /// Если свойство устанавливается первый раз, то блокируется список фильтров, команды добавления и удаления строк.
    /// Порядок строк в списке не имеет значения, т.к. сортировка выполняется в соответствии с настройками просмотра
    /// </summary>
    public IdList FixedDocIds
    {
      get { return _FixedDocIds; }
      set
      {
        if (_FixedDocIds == null && value != null)
        {
          // Первая установка
          base.CanInsert = false;
          base.CanDelete = false;
          base.CommandItems.CanEditFilters = false;
        }
        _FixedDocIds = value;

        if (value != null && SourceAsDataTable != null)
          OnInitSourceDataView(); // 06.07.2021
      }
    }
    private IdList _FixedDocIds;

    #endregion

    #region Редактирование документов

    /// <summary>
    /// Внешний инициализатор для новых документов
    /// Если свойство установлено, то при создании нового документа в качестве
    /// инициализатора значений полей (аргумент Caller при вызове ClientDocType.PerformEdit()) 
    /// будет использован этот инициализатор вместо текущих фильтров (DocGridHandler.DocFilters)
    /// Свойство может устанавливаться только до вывода просмотра на экран
    /// </summary>
    public DocumentViewHandler ExternalEditorCaller
    {
      get { return _ExternalEditorCaller; }
      set
      {
        CheckHasNotBeenCreated();
        _ExternalEditorCaller = value;
      }
    }
    private DocumentViewHandler _ExternalEditorCaller;


    private void MyEdit(object sender, EventArgs args)
    {
      Int32[] EditIds = SelectedIds;
      DocTypeUI.PerformEditing(EditIds, State, Control.FindForm().Modal, _ViewHandler);
    }

    #endregion

    #region Инициализация просмотра

    /// <summary>
    /// Вызывается из OnShown(), если предполагается загрузка последней пользовательской конфигурации просмотра,
    /// но просмотр открывается впервые и нет сохраненной конфигурации.
    /// Также метод вызывается в случае ошибки загрузки.
    /// Метод должен установить свойство CurrentConfig
    /// </summary>
    protected override void OnInitDefaultGridConfig()
    {
      // не вызываем! base.InitDefaultGridConfig();

      // Настройка просмотра для вида документов
      Columns.Clear();
      PerformInitGrid(true);
      // PerformRefresh();
    }


    private bool _InisidePerformInitGrid;

    /// <summary>
    /// Инициализация колонок табличного просмотра.
    /// После вызова становится доступным свойство UsedColumnNames.
    /// Предотвращается реентрантный вызов.
    /// Вызов DocTypeUI.PerformInitGrid() выполняется виртуальным методом OnInitGrid().
    /// При первом вызове, в частности, вызывается обработчик события DocTypeUI.InitView, отвечающий за добавление фильтров.
    /// <param name="forced">Если true, то инициализация будет выполнена обязательно. Используется при обработке события OnCurrentConfigChanged.
    /// Если false, то инициализация выполняется только, если она еще не была выполнена ранее.</param>
    /// </summary>
    public void PerformInitGrid(bool forced)
    {
      if (_InisidePerformInitGrid)
        return;
      _InisidePerformInitGrid = true;
      try
      {
        if (_UsedColumnNames == null || forced)
        {
          DBxColumnList usedColumnNameList = new DBxColumnList();
          OnInitGrid(_UsedColumnNames != null, usedColumnNameList);
          _UsedColumnNames = new DBxColumns(usedColumnNameList);
        }
      }
      finally
      {
        _InisidePerformInitGrid = false;
      }
    }

    /// <summary>
    /// Инициализация табличного просмотра документов с помощью DocTypeUI.PerformInitGrid().
    /// Переопределенный метод может, например, добавить порядки сортировки в список Orders.
    /// </summary>
    /// <param name="reInit">true при повторном вызове метода (после изменения конфигурации просмотра)
    /// и false при первом вызове</param>
    /// <param name="columns">Сюда помещается список имен полей, которые требуются для текущей конфигурации просмотра</param>
    protected virtual void OnInitGrid(bool reInit, DBxColumnList columns)
    {
      DocTypeUI.PerformInitGrid(this, reInit, columns, UserInitData);
      base.Filters.GetColumnNames(columns);
    }

    /// <summary>             
    /// Пользователь изменил настройку табличного просмотра
    /// </summary>
    protected override void OnCurrentConfigChanged(CancelEventArgs args)
    {
      base.OnCurrentConfigChanged(args);

      if (HasBeenCreated &&
        CommandItems.UseRefresh) // 16.09.2016
      {
        // 31.08.2018
        // Если просмотр уже выведен, то отключаем источник данных
        EFPDataGridViewSelection OldSel = this.Selection;
        Control.DataSource = null;

        PerformInitGrid(true);
        if (!EFPApp.InsideLoadComposition) // 30.05.2019
          PerformRefresh();
        this.Selection = OldSel;
      }
      else
        PerformInitGrid(true);

      args.Cancel = true;
    }

    /*
    private new void Grid_VisibleChanged(object Sender, EventArgs Args)
    {
      if (Control.DataSource == null && WinFormsTools.AreControlAndFormVisible(Control))
        MyRefresh(null, null);
    } */

    #endregion

    #region Команды локального меню

    void RowMerge_Click(object sender, EventArgs args)
    {
      // TODO:
      //RowMerge.PerformMerge(DocType.Name, SelectedIds);
      PerformRefresh();
    }

    #endregion

    #region Реализация DocumentViewHandler

    /// <summary>
    /// Реализация DocumentViewHandler для EFPDocGridView
    /// </summary>
    protected class IntDocumentViewHandler : DocumentViewHandler
    {
      #region Конструктор и Dispose

      /// <summary>
      /// Создает объект
      /// </summary>
      /// <param name="owner">Провайдер табличного просмотра - владелец</param>
      public IntDocumentViewHandler(EFPDocGridView owner)
      {
#if DEBUG
        if (owner == null)
          throw new ArgumentNullException("owner");
#endif

        _Owner = owner;
        _ExternalEditorCaller = owner.ExternalEditorCaller;
      }

      /// <summary>
      /// Уничтожает объект при закрытии формы
      /// </summary>
      /// <param name="disposing">True, если вызван метод Dispose()</param>
      protected override void Dispose(bool disposing)
      {
        base.Dispose(disposing);
        _Owner = null; // разрыв ссылки, чтобы текущий просмотр мог быть удален
        _ExternalEditorCaller = null;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Провайдер табличного просмотра - владелец
      /// </summary>
      public EFPDocGridView Owner { get { return _Owner; } }
      private EFPDocGridView _Owner;

      /// <summary>
      /// Используется при инициализации полей нового документа
      /// </summary>
      public DocumentViewHandler ExternalEditorCaller { get { return _ExternalEditorCaller; } }
      private DocumentViewHandler _ExternalEditorCaller;

      #endregion

      #region Переопределенные свойства

      /// <summary>
      /// Интерфейс вида документов
      /// </summary>
      public override DocTypeUI DocTypeUI
      {
        get
        {
          if (Owner == null)
            return null;
          else
            return Owner.DocTypeUI;
        }
      }

      /// <summary>
      /// Возвращает EFPDocGridView.BrowserGuid
      /// </summary>
      public override Guid BrowserGuid
      {
        get
        {
          if (Owner == null)
            return Guid.Empty;
          else
            return Owner.BrowserGuid;
        }
      }

      /// <summary>
      /// Возвращает EFPDocGridView.CurrentColumnName
      /// </summary>
      public override string CurrentColumnName
      {
        get
        {
          if (Owner == null)
            return String.Empty;
          else
            return Owner.CurrentColumnName;
        }
      }

      #endregion

      #region ApplyChanges()

      /// <summary>
      /// Обновление табличного просмотра
      /// </summary>
      /// <param name="dataSet"></param>
      /// <param name="isCaller"></param>
      public override void ApplyChanges(DataSet dataSet, bool isCaller)
      {
        if (Owner == null)
          return;
        if (Owner.SourceAsDataTable == null)
          return; // 04.06.2018

        if (!dataSet.Tables.Contains(Owner.DocType.Name))
          return; // Нет таблицы

        DataTable srcTable = dataSet.Tables[Owner.DocType.Name]; // Таблица в DataSet
        DataTable resTable = Owner.MasterDataTable; // Мастер-таблица в табличном просмотре
        List<DataRow> newSelRows = new List<DataRow>(); // строки в мастер-таблице, которые нужно будет выбрать

        DBxColumns filterColumns = Owner.Filters.GetColumnNames();

        // Есть в таблице первичный ключ по полю "Id"?
        bool hasPrimaryKey = String.Compare(DataTools.GetPrimaryKey(resTable), "Id", StringComparison.OrdinalIgnoreCase) == 0;

        // 08.07.2016
        // Таблица может не содержать первичного ключа
        DataView dvFind = null;
        if (!hasPrimaryKey)
        {
          dvFind = new DataView(resTable);
          dvFind.Sort = "Id";
        }
        try
        {
          foreach (DataRow srcRow in srcTable.Rows)
          {
            // 15.07.2020. Алгоритм изменен

            #region Определяем необходимость наличия строки в просмотре

            bool isVisible;
            switch (srcRow.RowState)
            {
              case DataRowState.Added: // нельзя базироваться только на RowState
                if (isCaller)
                  isVisible = true; // Не уверен, что надо всегда показывать
                else
                  isVisible = TestFilters(srcRow, filterColumns);
                break;
              case DataRowState.Modified:
                isVisible = TestFilters(srcRow, filterColumns);
                break;
              case DataRowState.Deleted:
                if (Owner.ShowDeleted)
                  isVisible = TestFilters(srcRow, filterColumns);
                else
                  isVisible = false;
                break;
              default: // Unchanged
                continue;

            }

            #endregion

            #region Определяем существование строки в просмотре

            Int32 docId;
            if (srcRow.RowState == DataRowState.Deleted)
              docId = (Int32)(srcRow["Id", DataRowVersion.Original]);
            else
              docId = (Int32)(srcRow["Id"]);
            DataRow resRow = FindDocRow(docId, resTable, dvFind); // строка в мастер-таблице

            #endregion

            #region Добавление / обновление / удаление строки в просмотре

            if (isVisible)
            {
              if (resRow == null)
              {
                resRow = resTable.NewRow();
                DataTools.CopyRowValues(srcRow, resRow, true);
                UpdateRefValues(srcRow, resRow);
                resTable.Rows.Add(resRow);

              }
              else
              {
                DataTools.CopyRowValues(srcRow, resRow, true);
                UpdateRefValues(srcRow, resRow);
                Owner.InvalidateDataRow(Owner.GetSlaveRow(resRow)); // не Update
              }

              // Перенесено сюда 26.05.2021
              // Может быть несколько просмотров, привязанных к одной кэшированной таблице данных.
              // В этом случае предыдущее условие "ResRow==null" выполняется для первого просмотра,
              // а второй просмотр не будет добавлять дублирующую строку в таблицу.
              // Но позиционировать на новую строку надо в любом случае, если она видна в просмотре.
              if (srcRow.RowState == DataRowState.Added)
                newSelRows.Add(resRow);
            }
            else
            {
              if (resRow != null)
                resRow.Delete();
            }

            #endregion
          }
        }
        finally
        {
          if (dvFind != null)
            dvFind.Dispose();
        }

        if (newSelRows.Count > 0)
        {
          try
          {
            Owner.SelectedDataRows = Owner.GetSlaveRows(newSelRows.ToArray());
          }
          catch
          {
            EFPApp.ErrorMessageBox("Не удалось активировать добавленные строки в табличном просмотре");
          }
        }
      }

      private DataRow FindDocRow(Int32 docId, DataTable resTable, DataView dvFind)
      {
        if (docId == 0)
          return null;

        if (dvFind == null)
          return resTable.Rows.Find(docId);
        else
        {
          int p = dvFind.Find(docId);
          if (p >= 0)
            return dvFind[p].Row;
          else
            return null;
        }
      }

      //private StringArrayIndexer ServiceColumnIndexer
      //{
      //  get
      //  {
      //    if (_ServiceColumnIndexer == null)
      //    {
      //      DBxColumns AllNames=Owner.DocTypeUI.UI.DocProvider.AllDocServiceColumns - new DBxColumns("Id");
      //      _ServiceColumnIndexer = new StringArrayIndexer(AllNames.AsArray);
      //    }
      //    return _ServiceColumnIndexer;
      //  }
      //}
      //private StringArrayIndexer _ServiceColumnIndexer;

      private void UpdateRefValues(DataRow srcRow, DataRow resRow)
      {
        for (int i = 0; i < resRow.Table.Columns.Count; i++)
        {
          string ColName = resRow.Table.Columns[i].ColumnName;

          // 12.02.2021, 18.10.2021
          switch (ColName)
          {
            case "CreateTime":
            case "CreateUserId":
            case "ChangeTime":
            case "ChangeUserId":
              object value = Owner.UI.TextHandlers.DBCache[Owner.DocType.Name].GetValue(DataTools.GetInt(resRow, "Id"), ColName);
              if (value == null)
                resRow[i] = DBNull.Value;
              else
                resRow[i] = value;
              continue;
          }

          // 13.11.2020
          // Строка srcRow может быть помечена на удаление
          if (srcRow.RowState != DataRowState.Deleted)
          {
            int p = ColName.IndexOf('.');
            if (p >= 0)
            {
              string MainColName = ColName.Substring(0, p);
              int pCol = srcRow.Table.Columns.IndexOf(MainColName);
              if (pCol >= 0)
              {
                Int32 RefId = DataTools.GetInt(srcRow, MainColName); // в ResRow может не быть базового поля
                object RefValue = Owner.UI.TextHandlers.DBCache[Owner.DocType.Name].GetRefValue(ColName, RefId);
                if (RefValue == null)
                  resRow[i] = DBNull.Value; // 26.10.2016
                else
                  resRow[i] = RefValue;
              }
            }
          }
        }
      }

      /// <summary>
      /// Проверяет, проходит ли строка документа из набора DBxDataSet условия фильтра, заданные для просмотра
      /// </summary>
      /// <param name="srcRow">Строка документа для DBxSingleDoc</param>
      /// <param name="filterColumns">Список столбов, которые требуются для проверки установленных фильтров табличного просмотра.
      /// В списке могут быть ссылочные поля</param>
      /// <returns>True, если документ проходит фильтрацию</returns>
      private bool TestFilters(DataRow srcRow, DBxColumns filterColumns)
      {
        if (filterColumns == null)
          return true; // Нечего тестировать

        // 13.11.2020
        // Проверяемая строка может быть помечена на удаление
        DataRowVersion rowVer = srcRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;

        NamedValues pairs = new NamedValues();
        for (int i = 0; i < filterColumns.Count; i++)
        {
          string ColName = filterColumns[i];
          object value;
          int pDot = ColName.IndexOf('.');
          if (pDot >= 0)
          {
            string MainColName = ColName.Substring(0, pDot);
            int pCol = srcRow.Table.Columns.IndexOf(MainColName);
            if (pCol >= 0)
            {
              Int32 RefId = DataTools.GetInt(srcRow[MainColName, rowVer]);
              value = Owner.UI.TextHandlers.DBCache[Owner.DocType.Name].GetRefValue(ColName, RefId);
            }
            else
              throw new BugException("Не найдено поле \"" + MainColName + "\"");
          }
          else
            value = srcRow[ColName, rowVer];

          if (value is DBNull)
            value = null;
          pairs.Add(ColName, value);
        }

        return _Owner.Filters.TestValues(pairs);
      }

      #endregion

      #region Прочие переопределенные методы

      /// <summary>
      /// Возвращает Owner.ToString(), если объект присоединен к просмотру
      /// </summary>
      /// <returns>Текстовое представление для табличного просмотра</returns>
      public override string ToString()
      {
        if (Owner == null)
          return "[ Отключен от просмотра ]";
        else
          return Owner.ToString();
      }

      /// <summary>
      /// Вызывает EFPDocGridView.UpdateRowsForIds()
      /// </summary>
      /// <param name="docIds">Идентификаторы обновляемых документов</param>
      public override void UpdateRowsForIds(Int32[] docIds)
      {
        if (Owner != null)
          Owner.UpdateRowsForIds(docIds);
      }

      /// <summary>
      /// Инициализация полей нового документа
      /// </summary>
      /// <param name="newDoc"></param>
      public override void InitNewDocValues(DBxSingleDoc newDoc)
      {
        if (ExternalEditorCaller == null)
        {
          if (Owner != null)
          {
            if (Owner.Filters != null)
              Owner.Filters.InitNewDocValues(newDoc);


            // 10.06.2019
            DocTypeUI.InitNewDocGroupIdValue(newDoc, Owner.AuxFilterGroupIds);
          }
        }
        else
          ExternalEditorCaller.InitNewDocValues(newDoc);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="savingDoc"></param>
      /// <param name="errorMessages"></param>
      public override void ValidateDocValues(DBxSingleDoc savingDoc, ErrorMessageList errorMessages)
      {
        if (ExternalEditorCaller == null)
        {
          if (Owner != null)
          {
            if (Owner.Filters != null)
              Owner.Filters.ValidateDocValues(savingDoc, errorMessages);
          }
        }
        else
          ExternalEditorCaller.ValidateDocValues(savingDoc, errorMessages);
      }

      /// <summary>
      /// Вызывает DataGridView.Invalidate()
      /// </summary>
      public override void InvalidateControl()
      {
        if (Owner != null)
          Owner.Control.Invalidate();
      }

      #endregion
    }

    /// <summary>
    /// Создает объект DocumentViewHandler для просмотра.
    /// Вызывается из OnShown(). Полученное значение присваивается свойству ViewHandler
    /// Методы может быть переопределен в производном классе (в том числе возвращать null),
    /// если требуется нестандартная обработка.
    /// </summary>
    /// <returns>Объект класса, производного от DocumentViewHandler</returns>
    protected virtual DocumentViewHandler CreateDocumentViewHandler()
    {
      return new IntDocumentViewHandler(this);
    }

    /// <summary>
    /// Обработчик просмотра документов, связанный с текущим просмотром.
    /// Свойство имеет значение не null, когда просмотр выведен на экран
    /// </summary>
    public DocumentViewHandler ViewHandler { get { return _ViewHandler; } }
    private DocumentViewHandler _ViewHandler;

    /// <summary>
    /// Идентификатор просмотра.
    /// Используется ViewHandler
    /// </summary>
    public Guid BrowserGuid
    {
      get { return _BrowserGuid; }
      set { _BrowserGuid = value; }
    }
    private Guid _BrowserGuid;

    /// <summary>
    /// Выполняет загрузку таблицы документов, если она не была выполнена в явном виде.
    /// </summary>
    protected override void OnCreated()
    {
      base.OnCreated();

      if (SourceAsDataTable == null)
      {
        Int32 OldId = CurrentId;
        PerformRefresh(); // обязательно после вызова OnShown(), иначе UsedColumnNames будет равен null
        CurrentId = OldId; // 23.11.2017
      }
    }

    /// <summary>
    /// Загружает таблицу документов, если это не было сделано в явном виде.
    /// Инициализирует свойство ViewHandler
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();

      _ViewHandler = CreateDocumentViewHandler();
      if (_ViewHandler != null)
        DocTypeUI.Browsers.Add(_ViewHandler);
    }

    /// <summary>
    /// Вызывается при первой загрузке конфигурации элемента.
    /// </summary>
    protected override void OnLoadConfig()
    {
      //Columns.Clear();
      PerformInitGrid(false); // фильтры должны быть инициализированы до загрузки конфигурации

      base.OnLoadConfig();

#if DEBUG
      if (UsedColumnNames == null)
        throw new NullReferenceException("Свойство UsedColumnNames не инициализировано");
#endif
    }

    /// <summary>
    /// Обнуляет свойство ViewHandler
    /// </summary>
    protected override void OnDetached()
    {
      if (_ViewHandler != null)
      {
        DocTypeUI.Browsers.Remove(_ViewHandler);
        _ViewHandler.Dispose();
        _ViewHandler = null;
      }
      base.OnDetached();
    }

    #endregion

    #region Выборочное обновление строк

    /// <summary>
    /// Очистка кэша для выбранных идентификаторов.
    /// Вызывается однократно перед вызовами LoadDataRowForUpdate()
    /// </summary>
    /// <param name="docIds">Массив идентификаторов. Длина массива больше 0</param>
    protected override void ClearCacheForUpdate(Int32[] docIds)
    {
      if (!Object.ReferenceEquals(this.GridProducer, DocTypeUI.GridProducer))
        return; // 25.03.2021. Если заменен GridProducer, то обновление невозможно

      _DocTypeUI.TableCache.Clear(docIds);
    }

    /// <summary>
    /// Обновление строки табличного просмотра из кэша.
    /// </summary>
    /// <param name="row">Строка данных. Не может быть null</param>
    protected override void LoadDataRowForUpdate(DataRow row)
    {
      Int32 DocId = DataTools.GetInt(row, "Id");
      if (DocId == 0)
        return;

      if (!Object.ReferenceEquals(this.GridProducer, DocTypeUI.GridProducer))
        return; // 25.03.2021. Если заменен GridProducer, то обновление невозможно

      object[] Values = DocTypeUI.TableCache.GetValues(DocId, _UsedColumnNames);
      for (int i = 0; i < _UsedColumnNames.Count; i++)
      {
        int p = row.Table.Columns.IndexOf(_UsedColumnNames[i]);
        if (p >= 0)
        {
          if (Values[i] == null)
            row[p] = DBNull.Value;
          else
            row[p] = Values[i];
        }
      }
    }

    #endregion
  }
}
