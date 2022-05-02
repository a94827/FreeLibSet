// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using System.Collections.Generic;
using FreeLibSet.Forms;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{
  #region Интерфейс IEFPSubDocView

  /// <summary>
  /// Интерфейс для просмотра поддокументов.
  /// Реализуется EFPSubDocGridView и EFPSubDocTreeView
  /// </summary>
  public interface IEFPSubDocView : IEFPDBxView
  {
    /// <summary>
    /// Основной редактор документа, использующий данный просмотр
    /// Задается в конструкторе.
    /// Может быть null, если просмотр используется для выбора поддокумента вне редактора
    /// </summary>
    DocumentEditor MainEditor { get; }

    /// <summary>
    /// Интерфейс доступа к виду просматриваемых поддокументов
    /// </summary>
    SubDocTypeUI SubDocTypeUI { get; }

    /// <summary>
    /// Описание вида поддокументов (экивавалентно SubDocTypeUI.SubDocType)
    /// </summary>
    DBxSubDocType SubDocType { get; }

    /// <summary>
    /// Редактируемые поддокументы (объект доступа). 
    /// </summary>
    DBxMultiSubDocs SubDocs { get; }

    /// <summary>
    /// Провайдер для доступа к документам (SubDocTypeUI.UI.DocProvider)
    /// </summary>
    DBxDocProvider DocProvider { get; }

    /// <summary>
    /// Если установлено в true, то перед добавлением и редактированием записей
    /// вызывается MainEditor.ValidateDate(). В этом случае редактор поддокумента
    /// может использовать актуальные значения полей основного документа
    /// По умолчанию (false) проверка не выполняется. Допускается редактирование
    /// поддокументов, даже если на какой-либо вкладке редактора основного документа
    /// есть некорректно заполненные поля
    /// </summary>
    bool ValidateBeforeEdit { get; set; }

    /// <summary>
    /// Эти данные передаются обработчику инициализации табличного просмотра
    /// </summary>
    object UserInitData { get; }
  }

  #endregion

  /// <summary>
  /// Табличный просмотр для редактирования поддокументов (вложенных
  /// таблиц) внутри страницы редактора основного документа
  /// </summary>
  public class EFPSubDocGridView : EFPDBxGridView, IEFPSubDocView
  {
    #region Конструкторы

    #region Версии с участием DocumentEditor

    /// <summary>
    /// Создает провайдер для встраивания в редактор документа
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент - табличный просмотр</param>
    /// <param name="mainEditor">Редактор основного документа, на вкладку которого добавляется табличный просмотр</param>
    /// <param name="subDocs">Список просматриваемых поддокументов</param>
    public EFPSubDocGridView(EFPBaseProvider baseProvider, DataGridView control, DocumentEditor mainEditor, DBxMultiSubDocs subDocs)
      : base(baseProvider, control, mainEditor.UI)
    {
      Init(mainEditor, subDocs);
    }

    /// <summary>
    /// Создает провайдер для встраивания в редактор документа
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    /// <param name="mainEditor">Редактор основного документа, на вкладку которого добавляется табличный просмотр</param>
    /// <param name="subDocs">Список просматриваемых поддокументов</param>
    public EFPSubDocGridView(EFPControlWithToolBar<DataGridView> controlWithToolBar, DocumentEditor mainEditor, DBxMultiSubDocs subDocs)
      : base(controlWithToolBar, mainEditor.UI)
    {
      Init(mainEditor, subDocs);
    }

    #endregion

    #region Версии без DocumentEditor

    /// <summary>
    /// Создает провайдер для выбора (или автономного просмотра) списка поддокументов без встраивания в редактор документа.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент - табличный просмотр</param>
    /// <param name="subDocs">Список просматриваемых поддокументов</param>
    /// <param name="ui">Пользовательский интерфейс для документов</param>
    public EFPSubDocGridView(EFPBaseProvider baseProvider, DataGridView control, DBxMultiSubDocs subDocs, DBUI ui)
      : base(baseProvider, control, ui)
    {
      Init(null, subDocs);
    }

    /// <summary>
    /// Создает провайдер для выбора (или автономного просмотра) списка поддокументов без встраивания в редактор документа.
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    /// <param name="subDocs">Список просматриваемых поддокументов</param>
    /// <param name="ui">Пользовательский интерфейс для документов</param>
    public EFPSubDocGridView(EFPControlWithToolBar<DataGridView> controlWithToolBar, DBxMultiSubDocs subDocs, DBUI ui)
      : base(controlWithToolBar, ui)
    {
      Init(null, subDocs);
    }

    #endregion

    private void Init(DocumentEditor mainEditor, DBxMultiSubDocs subDocs)
    {
#if DEBUG
      //      if (MainEditor == null)
      //        throw new ArgumentNullException("MainEditor");
      if (subDocs == null)
        throw new ArgumentNullException("subDocs");
#endif

      _MainEditor = mainEditor;

      _SubDocs = subDocs;
      _SubDocTypeUI = base.UI.DocTypes[subDocs.Owner.DocType.Name].SubDocTypes[subDocs.SubDocType.Name]; // чтобы свойство работало быстро

      _ValidateBeforeEdit = false;
      _TableCache = _SubDocTypeUI.TableCache;

      Control.ReadOnly = true;
      Control.VirtualMode = true;
      Control.AutoGenerateColumns = false;

      base.GridProducer = _SubDocTypeUI.GridProducer;
      base.ConfigSectionName = subDocs.SubDocType.Name;
      CommandItems.EnterAsOk = false;
      Control.MultiSelect = true;

      Control.CellValidated += new DataGridViewCellEventHandler(Control_CellValidated);
      CommandItems.ManualOrderChanged += new EventHandler(CommandItems_ManualOrderChanged);

      if (_SubDocTypeUI.HasEditorHandlers && mainEditor != null)
      {
        this.ReadOnly = DocProvider.DBPermissions.TableModes[SubDocType.Name] != DBxAccessMode.Full;
        //if (mainEditor != null)
        this.ReadOnly |= mainEditor.IsReadOnly;
        //this.CanMultiEdit = false;
        // this.EditData += new EventHandler(ControlProvider_EditData);
      }
      else
      {
        // 23.08.2012
        this.ReadOnly = true;
        this.CanView = false;
      }
      this.CanMultiEdit = SubDocTypeUI.CanMultiEdit;
      this.CanInsertCopy = SubDocTypeUI.CanInsertCopy;

      //if (DocProvider.DBPermissions.TableModes[SubDocType.Name] != DBxAccessMode.None)
      //{
      //  Control.DataSource = subDocs.SubDocsView;
      //  // DebugTools.DebugDataTable(SourceAsDataTable, SubDocType.Name);
      //}

      bool showDocId;
      if (mainEditor == null)
        showDocId = subDocs.Owner.DocCount > 1; // ???
      else
        showDocId = mainEditor.MultiDocMode;
      //SubDocTypeUI.DoInitGrid(this, false, UsedColumnNames, null, ShowDocId, true);
      //base.CurrentConfigChanged += new CancelEventHandler(GridHandler_CurrentGridConfigChanged);

      base.DisableOrdering();

      CanInlineEdit = true;

      base.ShowRowCountInTopLeftCellToolTipText = true; // 15.12.2017

      _ConfirmDeletion = true;
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Основной редактор документа, использующий данный просмотр
    /// Задается в конструкторе.
    /// Может быть null, если просмотр используется для выбора поддокумента вне редактора
    /// </summary>
    public DocumentEditor MainEditor { get { return _MainEditor; } }
    private DocumentEditor _MainEditor;

    /// <summary>
    /// Тип редактируемых поддокументов. Задается в конструкторе
    /// </summary>
    public DBxSubDocType SubDocType { get { return _SubDocTypeUI.SubDocType; } }

    /// <summary>
    /// Редактируемые поддокументы (объект доступа). Ссылка извлекается из
    /// MainEditor.Documents в конструкторе
    /// </summary>
    public DBxMultiSubDocs SubDocs { get { return _SubDocs; } }
    private DBxMultiSubDocs _SubDocs;

    /// <summary>
    /// Провайдер для доступа к документам
    /// </summary>
    public DBxDocProvider DocProvider { get { return _SubDocs.DocSet.DocProvider; } }

    /// <summary>
    /// Имя столбца, предназначенного для ручной сортировки строк или null, если
    /// строки сортируются каким-то другим способом
    /// </summary>
    public string ManualOrderColumn
    {
      get { return CommandItems.ManualOrderColumn; }
      set { CommandItems.ManualOrderColumn = value; }
    }

    /// <summary>
    /// Интерфейс доступа к поддокументам
    /// </summary>
    public SubDocTypeUI SubDocTypeUI { get { return _SubDocTypeUI; } }
    private SubDocTypeUI _SubDocTypeUI;


    /// <summary>
    /// Доступ к кэшированным данным таблицы (SubDocTypeUI.TableCache)
    /// </summary>
    public DBxTableCache TableCache { get { return _TableCache; } }
    private DBxTableCache _TableCache;


    /// <summary>
    /// Если установлено в true, то перед добавлением и редактированием записей
    /// вызывается MainEditor.ValidateDate(). В этом случае редактор поддокумента
    /// может использовать актуальные значения полей основного документа
    /// По умолчанию (false) проверка не выполняется. Допускается редактирование
    /// поддокументов, даже если на какой-либо вкладке редактора основного документа
    /// есть некорректно заполненные поля.
    /// </summary>
    public bool ValidateBeforeEdit
    {
      get { return _ValidateBeforeEdit; }
      set
      {
        if (value && _MainEditor == null)
          throw new InvalidOperationException("Нельзя устанавливать свойство ValidateBeforeEdit в true, т.к. просмотр не относится к DocumentEditor"); // 21.01.2022
        _ValidateBeforeEdit = value;
      }
    }
    private bool _ValidateBeforeEdit;

    /// <summary>
    /// Если true (по умолчанию), то допускается редактирование "по месту".
    /// Для запрета редактирования нужно устанавливить в false.
    /// Простая установка свойства Control.ReadOnly=true не даст эффекта,
    /// т.к. свойство переустанавливается, если пользователь изменит настройку
    /// просмотра
    /// Для выборочного запрета редактирования ячеек используйте, вместо этого свойства,
    /// свойство GridProducerColumn.ReadOnly при объявлении поддокумента
    /// </summary>
    public bool CanInlineEdit { get { return _CanInlineEdit; } set { _CanInlineEdit = value; } }
    private bool _CanInlineEdit;


    /// <summary>
    /// Эти данные передаются обработчику инициализации просмотра.
    /// Свойство может устанавливаться только до вызова события Created, то есть сразу после вызова конструктора
    /// </summary>
    public object UserInitData
    {
      get { return _UserInitData; }
      set
      {
        CheckHasNotBeenCreated();
        _UserInitData = value;
      }
    }
    private object _UserInitData;

    /// <summary>
    /// Имена столбцов, необходимых для просмотра.
    /// Список заполняется после инициализации просмотра с помощью EFPGridProducer.
    /// Чтобы гарантировать заполнение свойства, можно использовать метод PerdormInitGrid().
    /// </summary>
    public DBxColumns UsedColumnNames { get { return _UsedColumnNames; } }
    private DBxColumns _UsedColumnNames; // для обновления строк

    #endregion

    #region OnCreated() и OnDisposed()


    /// <summary>
    /// Закончить инициализацию табличного просмотра. Присоединяет табличный просмотр
    /// и панель кнопок к форме.
    /// Добавление команд локального меню должно быть закончено до вызова метода
    /// </summary>
    protected override void OnCreated()
    {
#if XXX ///DEBUG
      if (base.ConfigSectionName != SubDocType.Name)
        throw new InvalidOperationException("В текущей реализации SubDocGrid нельзя изменять значение свойства EFPDataGridView.ConfigSectionName");
#endif

      AfterInitColumns();

      #region Буфер обмена

      #region Вырезать

      CommandItems.Cut += new EventHandler(CommandItems_Cut);

      #endregion

      #region Копировать

      base.CommandItems.AddCopyFormats += new DataObjectEventHandler(CommandItems_AddCopyFormats);

      #endregion

      #region Вставить

      // Текстовый формат является приоритетным
      // Обязательно должно идти после вызова AfterInitColumns()
      base.CommandItems.AddTextPasteFormats();

      //List<string> PastableDocTypeNames = new List<string>();
      for (int i = 0; i < SubDocTypeUI.PasteTableNames.Count; i++)
      {
        //this.CommandItems.ClipboardInToolBar = true;

        DBxDataSetPasteFormat fmtTable = new DBxDataSetPasteFormat(UI, SubDocTypeUI.PasteTableNames[i]);
        fmtTable.Paste += new EFPPasteDataObjectEventHandler(fmtTable_Paste);
        this.CommandItems.PasteHandler.Add(fmtTable);

        DBxDocSelectionPasteFormat fmtDocSel;
        if (fmtTable.UIBase.DocTypeBase.IsSubDoc)
        {
          // Для поддокумента может быть использован основной документ, скопированный в буфер обмена
          DBxSubDocType sdt = (DBxSubDocType)(fmtTable.UIBase.DocTypeBase);
          fmtDocSel = new DBxDocSelectionPasteFormat(UI, sdt.DocType.Name);
          fmtDocSel.Tag = fmtTable.UIBase; // 16.02.2018
          fmtDocSel.Paste += new EFPPasteDataObjectEventHandler(fmtDocSel_PasteSubDocs);
        }
        else
        {
          // Вместо таблицы документа, конечно, может быть использована ссылка на него
          fmtDocSel = new DBxDocSelectionPasteFormat(UI, fmtTable.TableName);
          fmtDocSel.Paste += new EFPPasteDataObjectEventHandler(fmtDocSel_PasteDocs);
        }
        this.CommandItems.PasteHandler.Add(fmtDocSel);
      }
      this.CommandItems.ClipboardInToolBar = true;

      #endregion

      #endregion

      try
      {
        // В режиме просмотра ручная сортировка поддокументов не допускается
        if (base.ReadOnly)
          ManualOrderColumn = null;

        if ((!String.IsNullOrEmpty(ManualOrderColumn)) && (!this.ReadOnly))
          InitOrderValues(_SubDocs.SubDocsView.Table, false /* 21.04.2020 */);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка установки порядка строк поддокументов \"" + SubDocType.PluralTitle + "\"");
      }

      base.OnCreated();

      if (SourceAsDataTable == null)
      {
        Int32 oldId = CurrentId;
        PerformRefresh(); // обязательно после вызова OnCreated(), иначе UsedColumnNames будет равен null
        CurrentId = oldId; // 23.11.2017
      }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Вызывает SubDocTypeUI.PerformInitGrid()
    /// </summary>
    protected override void OnInitDefaultGridConfig()
    {
      // не вызываем base.OnInitDefaultGridConfig();

      // Настройка просмотра для вида документов
      PerformInitGrid(true);
    }

    private bool _InisidePerformInitGrid; // 16.05.2018

    /// <summary>
    /// Инициализация колонок табличного просмотра.
    /// После вызова становится доступным свойство UsedColumnNames.
    /// Предотвращается реентрантный вызов.
    /// Вызов DocTypeUI.PerformInitGrid() выполняется виртуальным методом OnInitGrid().
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
    /// Инициализация табличного просмотра документов с помощью SubDocTypeUI.PerformInitGrid().
    /// Переопределенный метод может, например, добавить порядки сортировки в список Orders.
    /// </summary>
    /// <param name="reInit">true при повторном вызове метода (после изменения конфигурации просмотра)
    /// и false при первом вызове</param>
    /// <param name="columns">Сюда помещается список имен полей, которые требуются для текущей конфигурации просмотра</param>
    protected virtual void OnInitGrid(bool reInit, DBxColumnList columns)
    {
      if (MainEditor == null)
        SubDocTypeUI.PerformInitGrid(this, reInit, columns, UserInitData);
      else
        SubDocTypeUI.PerformInitGrid(this, reInit, columns, UserInitData, MainEditor.MultiDocMode, true);
      DBxColumns imgCols = UI.ImageHandlers.GetColumnNames(SubDocTypeUI.SubDocType.Name, true);
      columns.AddRange(imgCols); // Может быть, это нужно делать в SubDocTypeUI
    }


    /// <summary>
    /// Вызывает событие CurrentConfigChanged.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnCurrentConfigChanged(CancelEventArgs args)
    {
      base.OnCurrentConfigChanged(args);

      PerformInitGrid(true);
      if (HasBeenCreated &&
        CommandItems.UseRefresh /* 16.05.2018 */)
      {
        if (!EFPApp.InsideLoadComposition) // 30.05.2019
          PerformRefresh();
      }
      args.Cancel = true;
    }

    ///// <summary>
    ///// Пользователь изменил настройку табличного просмотра
    ///// </summary>
    ///// <param name="sender"></param>
    ///// <param name="args"></param>
    //void GridHandler_CurrentGridConfigChanged(object sender, CancelEventArgs args)
    //{
    //  _UsedColumnNameList = new DBxColumnList();
    //  SubDocTypeUI.PerformInitGrid(this, true, _UsedColumnNameList, null, MainEditor.MultiDocMode, true);
    //  AfterInitColumns();
    //  base.PerformRefresh();
    //  args.Cancel = true;
    //}

    /// <summary>
    /// После (повторной) инициализации столбцов табличного просмотра
    /// можно разрешить или запретить inline-редактирование простых столбцов
    /// </summary>
    private void AfterInitColumns()
    {
      if (this.ReadOnly || (!CanInlineEdit))
      {
        Control.ReadOnly = true;
        return;
      }

      DataTable emptyTable = SubDocs.DocSet.DocProvider.GetTemplate(SubDocType.DocType.Name, SubDocType.Name);

      bool hasEditable = false;
      bool[] readOnlyFlags = new bool[this.Columns.Count];
      for (int i = 0; i < this.Columns.Count; i++)
      {
        readOnlyFlags[i] = !IsColumnEditable(this.Columns[i], emptyTable);
        if (!readOnlyFlags[i])
          hasEditable = true;

        //if (!Editable)
        //  DebugTools.DebugObject(GridHandler.Columns[i].GridColumn, "i=" + i.ToString());
      }

      //DebugTools.DebugObject(GridHandler.Columns[3].GridColumn, "до");
      this.Control.ReadOnly = !hasEditable;
      if (hasEditable)
      {
        for (int i = 0; i < this.Columns.Count; i++)
          this.Columns[i].GridColumn.ReadOnly = readOnlyFlags[i];
      }
      //DebugTools.DebugObject(GridHandler.Columns[3].GridColumn, "после");
    }

    private bool IsColumnEditable(EFPDataGridViewColumn column, DataTable emptyTable)
    {
      if (!((column.GridColumn is DataGridViewTextBoxColumn ||
        column.GridColumn is DataGridViewCheckBoxColumn)))
        return false;

      if (String.IsNullOrEmpty(column.GridColumn.DataPropertyName))
        return false; // вычисляемый столбец

      EFPGridProducerColumn colProducer = column.ColumnProducer as EFPGridProducerColumn;
      if (colProducer != null) // 19.04.14
      {
        if (colProducer.ReadOnly)
          return false;
      }
      if (column.CanIncSearch)
        return false;

      //      if (Column.GridColumn.DataPropertyName.IndexOf('.') >= 0)
      //        return false; // ссылочный столбец
      // Могут быть вычисляемые поля без точек
      if (emptyTable.Columns.IndexOf(column.GridColumn.DataPropertyName) < 0)
        return false;

      switch (column.GridColumn.DataPropertyName)
      {
        case "Id":
        case "DocId":
        case "Deleted":
          return false;
        default:
          return true;
      }
    }


    /// <summary>
    /// Начальное присвоение порядковых номеров строкам поддокументов в режиме 
    /// ручной сортировки
    /// </summary>
    /// <param name="table">Таблица данных поддокументов</param>
    /// <param name="setChangeInfo">Если true, то будет установлен флаг изменений
    /// MainEditor.SubDocsChangeInfo.Changed = true, если сортировка внесла изменения</param>
    private void InitOrderValues(DataTable table, bool setChangeInfo)
    {
      using (DataView dv = new DataView(table))
      {
        dv.Sort = ManualOrderColumn;
        DataRow[] rows = DataTools.GetDataViewRows(dv);

        bool changed = false;

        for (int i = 0; i < rows.Length; i++)
        {
          int thisN = DataTools.GetInt(rows[i], ManualOrderColumn);
          if (thisN != (i + 1)) // присваиваем только если не совпадает
          {
            rows[i][ManualOrderColumn] = i + 1;
            changed = true;
          }
        }

        if (changed)
        {
          if (ManualOrderChanged != null)
            ManualOrderChanged(this, EventArgs.Empty);
          if (setChangeInfo /* 21.04.2020 */ &&
            MainEditor.SubDocsChangeInfo != null)
            MainEditor.SubDocsChangeInfo.Changed = true;
        }
      }
    }

    #endregion

    #region Событие

    /// <summary>
    /// Вызывается, когда выполнена ручная сортировка строк (по окончании изменения
    /// значений поля для всех строк)
    /// </summary>
    public event EventHandler ManualOrderChanged;

    void CommandItems_ManualOrderChanged(object sender, EventArgs args)
    {
      // Дублируем событие
      if (ManualOrderChanged != null)
        ManualOrderChanged(this, EventArgs.Empty);
      MainEditor.SubDocsChangeInfo.Changed = true;
    }

    #endregion

    #region Значения для ссылочных полей

    /// <summary>
    /// Создает объект DBxDataRowValueArrayWithCache()
    /// </summary>
    /// <returns>Новый объект для доступа к данным</returns>
    protected override IDataRowNamedValuesAccess CreateRowValueAccessObject()
    {
      return new DBxDataRowValueArrayWithCache(SubDocTypeUI.TableCache);
    }

    #endregion

    #region Загрузка данных

    /// <summary>
    /// Обновление или первичная инициализация таблицы данных
    /// </summary>
    /// <param name="args"></param>
    protected override void OnRefreshData(EventArgs args)
    {
      if (SourceAsDataView != null)
      {
        // Реальная перезагрузка, а не первоначальный доступ
        _SubDocTypeUI.DBCache.Clear();
      }

      PerformInitGrid(false);

      TableRepeater = null;

      if (SubDocs.DocSet.DocProvider.DBPermissions.TableModes[SubDocType.Name] != DBxAccessMode.None)
      {
        DataView dv = SubDocs.SubDocsView;
        DBxDataTableRepeaterWithCache rep = null;
        if (_UsedColumnNames.ContainsDots)
        {
          // Есть ссылочные поля
          rep = new DBxDataTableRepeaterWithCache(SubDocTypeUI.TableCache);
          rep.SlaveTable = dv.Table.Clone(); // все поля без точек нужны
          foreach (string colName in _UsedColumnNames)
          {
            if (!rep.SlaveTable.Columns.Contains(colName))
            {
              DataColumn col = SubDocTypeUI.TableCache.GetColumnStruct(colName).CreateDataColumn(colName);
              col.ColumnName = colName; // 22.06.2021. GetColumnStruct() может вернуть описание столбца с другим именем
              rep.SlaveTable.Columns.Add(col);
            }
          }
          foreach (DataColumn col in rep.SlaveTable.Columns)
            col.AllowDBNull = true; // иначе не будет работать

          rep.MasterTable = dv.Table;
          dv = rep.SlaveTable.DefaultView;
          // dv.Sort = SubDocs.SubDocsView.Sort; //
        }

        string dvSort = SubDocType.DefaultOrder.ToString(); // 02.05.2022
        if (!String.IsNullOrEmpty(ManualOrderColumn))
          dv.Sort = ManualOrderColumn;

        if (MainEditor != null)
        {
          if (MainEditor.MultiDocMode)
          {
            if (String.IsNullOrEmpty(dv.Sort))
              dvSort = "DocId";
            else
              dvSort += ",DocId";
          }
        }
        if (rep == null)
          Control.DataSource = dv;
        else
          base.TableRepeater = rep;
        SourceAsDataView.Sort = dvSort; // 02.05.2022
      }

      base.OnRefreshData(args);
    }

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool HasRefreshDataHandler { get { return true; } }

    #endregion

    #region Редактирование поддокумента

    /// <summary>
    /// Присоединяет обработчик в DocumentEditor
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();

      if (_MainEditor != null)
        _MainEditor.AfterWrite += new DocEditEventHandler(MainEditor_AfterWrite);
    }

    /// <summary>
    /// Отсоединяет обработчик от DocumentEditor
    /// </summary>
    protected override void OnDetached()
    {
      if (_MainEditor != null)
        _MainEditor.AfterWrite -= new DocEditEventHandler(MainEditor_AfterWrite);

      base.OnDetached();
    }

    void MainEditor_AfterWrite(object sender, DocEditEventArgs args)
    {
      // 21.01.2016
      // После нажатия кнопки "Запись" в редакторе документа необходимо выполнить обновление просмотра,
      // т.к. иначе он будет ссылаться на старый объект DataTable

      EFPDataGridViewSelectedRowsMode oldMode = base.SelectedRowsMode;
      base.SelectedRowsMode = EFPDataGridViewSelectedRowsMode.RowIndex;
      try
      {
        PerformRefresh();
      }
      finally
      {
        base.SelectedRowsMode = oldMode;
      }
    }


    /// <summary>
    /// Если true (по умолчанию), то перед удалением выбранных поддокументов запрашивается подтверждение.
    /// Если false, то удаление выполняется немедленно без запроса.
    /// </summary>
    public bool ConfirmDeletion { get { return _ConfirmDeletion; } set { _ConfirmDeletion = value; } }
    private bool _ConfirmDeletion;

    /// <summary>
    /// Удаляет текущие выбранные поддокументы без выдачи дополнительных запросов.
    /// Свойство ConfirmDeletion игнорируется.
    /// Этот метод может использоваться в обработчике команды Cut.
    /// </summary>
    public void PerformDeleteDataQuiet()
    {
      bool oldConfirmDeletion = ConfirmDeletion;
      try
      {
        ConfirmDeletion = false;
        PerformEditData(EFPDataGridViewState.Delete);
      }
      finally
      {
        ConfirmDeletion = oldConfirmDeletion;
      }
    }

    /// <summary>
    /// Обработка события редактирования данных
    /// </summary>
    /// <param name="args">Не используется</param>
    /// <returns>Возвращает true, если следует выполнить редактирование "по месту", если оно поддерживается табличным просмотром</returns>
    protected override bool OnEditData(EventArgs args)
    {
      if (ValidateBeforeEdit)
      {
        if (!MainEditor.ValidateData())
          return true;
      }

      DataRow[] rows = null;
      Int32 docId;
      if (this.State == EFPDataGridViewState.Insert)
      {
        if (!SubDocTypeUI.SelectOneDoc(this, out docId))
          return true;
      }
      else
      {
        rows = GetMasterRows(this.SelectedDataRows);
        if (rows.Length == 0)
        {
          EFPApp.MessageBox("Нет выбранных поддокументов \"" + SubDocType.PluralTitle + "\"");
          return true;
        }

        docId = DataTools.GetInt(rows[0], "DocId");
      }

      // TODO: Не должно ли быть это внутри SubDocumentEditor.Run()?
      DBxMultiSubDocs subDocs2;
      if (this.State == EFPDataGridViewState.Insert)
      {
        subDocs2 = new DBxMultiSubDocs(SubDocs, DataTools.EmptyIds);
        subDocs2.Insert();
      }
      else
      {
        subDocs2 = new DBxMultiSubDocs(SubDocs, rows);
        if (this.State == EFPDataGridViewState.Delete)
        {
          if (ConfirmDeletion)
          {
            string s;
            if (subDocs2.SubDocCount == 1)
              s = "Удалить запись \"" + SubDocType.SingularTitle + "\" (" + SubDocTypeUI.UI.TextHandlers.GetTextValue(subDocs2[0]) + ")?";
            else
              s = "Удалить выбранные записи \"" + SubDocs.SubDocType.PluralTitle + "\" (" + subDocs2.SubDocCount.ToString() + ")?";
            if (EFPApp.MessageBox(s, "Подтверждение удаления",
              MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
              return true;
          }

          //DebugTools.DebugDataSet(MainEditor.Documents.DataSet, "До удаления");


          //for (int i = 0; i < SubDocs2.SubDocCount;i++ )
          //  SubDocs[i].Delete();
          subDocs2.Delete();
          //EFPApp.MessageBox(SubDocs.SubDocCount.ToString(), "До очистки");
          SubDocs.MergeSubSet(subDocs2);
          //EFPApp.MessageBox(SubDocs.SubDocCount.ToString(), "После очистки");

          //DebugTools.DebugDataSet(MainEditor.Documents.DataSet, "После удаления");


          if (!String.IsNullOrEmpty(ManualOrderColumn))
            InitOrderValues(_SubDocs.SubDocsView.Table, true);
          MainEditor.SubDocsChangeInfo.Changed = true; // вдруг удалили последнюю строку?
          return true;
        }
        switch (this.State)
        {
          case EFPDataGridViewState.Edit:
            subDocs2.Edit();
            break;
          case EFPDataGridViewState.InsertCopy:
            subDocs2.InsertCopy();
            break;
        }
      }

      SubDocumentEditor sde = new SubDocumentEditor(MainEditor, subDocs2, this.State);
      if (sde.Run())
      {
        int lastOrder = 0;
        if (this.State == EFPDataGridViewState.Insert || this.State == EFPDataGridViewState.InsertCopy)
        {
          if (!String.IsNullOrEmpty(ManualOrderColumn))
            lastOrder = DataTools.MaxInt(SubDocs.SubDocsView, ManualOrderColumn, true) ?? 0;
        }

        SubDocs.MergeSubSet(subDocs2);

        if (this.State == EFPDataGridViewState.Insert || this.State == EFPDataGridViewState.InsertCopy)
        {
          DataRow lastRow = _SubDocs.SubDocsView.Table.Rows[_SubDocs.SubDocsView.Table.Rows.Count - 1];
          if (!String.IsNullOrEmpty(ManualOrderColumn))
          {
            // Присваиваем новой строке номер по порядку, чтобы она была в конце
            //LastRow[ManualOrderColumn] = FSubDocs.SubDocsView.Table.Rows.Count;
            // Исправлено 21.04.2020
            lastRow[ManualOrderColumn] = lastOrder + 1;
          }

          try { this.CurrentDataRow = GetSlaveRow(lastRow); }
          catch { } // 28.04.2022
        }
        MainEditor.SubDocsChangeInfo.Changed = true;
        if (this.State == EFPDataGridViewState.Edit)
        {
          try
          {
            UpdateSelectedRows();
          }
          catch (Exception ex)
          {
            EFPApp.ShowException(ex, "Ошибка обновления строк");
          }
        }
      }

      return true;
    }

    void Control_CellValidated(object sender, DataGridViewCellEventArgs args)
    {
      try
      {
        // после inline-редактирования поддокументы считаются модифицированными
        if (_MainEditor != null)
        {
          if (Control.IsCurrentCellInEditMode)
            _MainEditor.SubDocsChangeInfo.Changed = true;
        }
      }
      catch
      {
      }
    }

    #endregion

    #region Буфер обмена

    #region Вырезать

    void CommandItems_Cut(object sender, EventArgs args)
    {
      if (this.WholeRowsSelected)
      {
        // Вырезается не прямоугольный блок, а строки
        if (!this.CommandItems.PerformCopy())
          return; // не удалось скопировать
        DataRow[] rows = this.SelectedDataRows;
        for (int i = 0; i < rows.Length; i++)
          rows[i].Delete();

        if (!String.IsNullOrEmpty(ManualOrderColumn))
          InitOrderValues(_SubDocs.SubDocsView.Table, true); // 21.04.2020
        if (_MainEditor != null) // 21.01.2022
          _MainEditor.SubDocsChangeInfo.Changed = true;
        return;
      }

      // Обычное вырезание
      this.CommandItems.PerformCutText();
    }

    #endregion

    #region Копировать

    void CommandItems_AddCopyFormats(object sender, DataObjectEventArgs args)
    {
      // Добавляем в буфер обмена таблицу данных
      DataRow[] rows = this.SelectedDataRows;
      if (rows.Length == 0)
        return;

      DataSet ds = new DataSet();
      ds.RemotingFormat = SerializationFormat.Binary;
      //ds.ExtendedProperties["WorkAreaIdentity"] = DocProvider.DBIdentity;
      ds.ExtendedProperties["DBIdentity"] = DocProvider.DBIdentity; // 15.05.2020
      DataTable table = this.SourceAsDataTable.Clone();
      table.TableName = _SubDocTypeUI.SubDocType.Name;
      ds.Tables.Add(table);
      for (int i = 0; i < rows.Length; i++)
        table.Rows.Add(rows[i].ItemArray);

      args.DataObject.SetData(ds);
    }

    #endregion

    #region Вставить

    void fmtTable_Paste(object sender, EFPPasteDataObjectEventArgs args)
    {
      DBxDataSetPasteFormat fmtTable = (DBxDataSetPasteFormat)sender;
      DoPasteTable(fmtTable.Table, fmtTable.UIBase);
    }

    /// <summary>
    /// Выполняет вставку из выборки документов, возможно, другого вила
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void fmtDocSel_PasteSubDocs(object sender, EFPPasteDataObjectEventArgs args)
    {
      DBxDocSelectionPasteFormat fmtDocSel = (DBxDocSelectionPasteFormat)sender;
      Int32[] docIds = fmtDocSel.DocSel[fmtDocSel.DocTypeName];
      //if (DocIds.Length < 0)
      if (docIds.Length <= 0) // 28.12.2020
      {
        EFPApp.ErrorMessageBox("Нет выбранных документов \"" + fmtDocSel.DocType.DocType.PluralTitle + "\"");
        return;
      }

      if (docIds.Length > 100)
      {
        EFPApp.ErrorMessageBox("Выбрано слишком много документов \"" + fmtDocSel.DocType.DocType.PluralTitle + "\" (" +
          docIds.Length.ToString() + " шт.). Максимально за один раз могут быть вставлены поддокументы для 100 документов");
        return;
      }

      DBxFilter filter = new IdsFilter("DocId", docIds);
      if (UI.DocProvider.DocTypes.UseDeleted) // 23.05.2021
        filter = new AndFilter(filter, DBSSubDocType.DeletedFalseFilter);

      SubDocTypeUI sdt = (SubDocTypeUI)(fmtDocSel.Tag);
      DataTable Table = UI.DocProvider.FillSelect(sdt.SubDocType.Name, null,
        filter, null);

      if (Table.Rows.Count == 0)
      {
        EFPApp.ErrorMessageBox("Выбранные документы \"" + fmtDocSel.DocType.DocType.PluralTitle + "\" (" + docIds.Length.ToString() +
          " шт.) не содержат ни одного поддокумента \"" + sdt.SubDocType.SingularTitle + "\"");
        return;
      }

      DoPasteTable(Table, sdt);
    }

    void fmtDocSel_PasteDocs(object sender, EFPPasteDataObjectEventArgs args)
    {
      DBxDocSelectionPasteFormat fmtDocSel = (DBxDocSelectionPasteFormat)sender;
      Int32[] docIds = fmtDocSel.DocSel[fmtDocSel.DocTypeName];
      // if (DocIds.Length < 0)
      if (docIds.Length == 0) // 28.12.2020
      {
        EFPApp.ErrorMessageBox("Нет выбранных документов \"" + fmtDocSel.DocType.DocType.PluralTitle + "\"");
        return;
      }

      DataTable table = DocProvider.FillSelect(fmtDocSel.DocTypeName, null, new IdsFilter(docIds));

      // DoPasteTable(Table, fmtDocSel.DocType);
      // 22.08.2016
      // Вставляем строки в том порядке, как заданы идентификаторы
      DataTools.SetPrimaryKey(table, "Id");
      DataRow[] srcRows = new DataRow[docIds.Length];
      for (int i = 0; i < docIds.Length; i++)
      {
        DataRow row = table.Rows.Find(docIds[i]);
        if (row == null)
        {
          EFPApp.ErrorMessageBox("Не удалось загрузить из базы данных строку документа \"" + fmtDocSel.DocType.DocType.SingularTitle + "\" с идентификатором " + docIds[i]);
          return;
        }
        srcRows[i] = row;
      }

      DoPasteRows(srcRows, fmtDocSel.DocType);
    }

    void DoPasteTable(DataTable srcTable, DocTypeUIBase docTypeBase)
    {
      DataRow[] srcRows = new DataRow[srcTable.Rows.Count];
      srcTable.Rows.CopyTo(srcRows, 0);
      DoPasteRows(srcRows, docTypeBase);
    }

    void DoPasteRows(DataRow[] srcRows, DocTypeUIBase docTypeBase)
    {
      if (ValidateBeforeEdit)
      {
        if (!MainEditor.ValidateData())
          return;
      }

      Int32 docId;
      if (!SubDocTypeUI.SelectOneDoc(this, out docId))
        return;

      DBxSingleDoc mainDoc = MainEditor.Documents[SubDocType.DocType.Name].GetDocById(docId);

      // Нельзя использовать в качестве оригинала полученную строку, т.к. таблица в буфере обмена может быть неполной
      DBxMultiSubDocs subDocs2 = new DBxMultiSubDocs(SubDocs, DataTools.EmptyIds);

      int cntCancelled = 0;
      ErrorMessageList errors = new ErrorMessageList();
      for (int i = 0; i < srcRows.Length; i++)
      {
        DBxSubDoc subDoc2 = subDocs2.Insert();
        DBxDocValue.CopyValues(srcRows[i], subDoc2.Values);

        // Вызываем пользовательский обработчик
        AdjustPastedSubDocRowEventArgs args = new AdjustPastedSubDocRowEventArgs(subDoc2,
          srcRows[i], srcRows[0].Table.DataSet, srcRows[0].Table.TableName, mainDoc, i == 0);
        this.SubDocTypeUI.PerformAdjustPastedRow(args);
        if (args.Cancel)
        {
          cntCancelled++;
          if (String.IsNullOrEmpty(args.ErrorMessage))
            args.ErrorMessage = "Нельзя добавить строку";
          errors.AddWarning("Строка " + (i + 1).ToString() + " пропускается. " + args.ErrorMessage);
          subDoc2.Delete();
        }
      }

      // Убираем отмененные строки и возвращаем состояние Insert на место
      subDocs2.SubDocsView.Table.AcceptChanges();
      foreach (DataRow row in subDocs2.SubDocsView.Table.Rows)
        DataTools.SetRowState(row, DataRowState.Added);

      if (cntCancelled > 0)
      {
        if (subDocs2.SubDocCount == 0)
          errors.AddError("Ни одна из строк (" + srcRows.Length.ToString() + " шт.) не может быть добавлена");
        EFPApp.ShowErrorMessageListDialog(errors, "Вставка");
        if (subDocs2.SubDocCount == 0)
          return;
      }

      if (SubDocTypeUI.HasEditorHandlers && subDocs2.SubDocCount == 1)
      {
        // Открытие редактора поддокумента
        // Режим должен быть обязательно InsertCopy, иначе значения не прочитаются
        SubDocumentEditor sde = new SubDocumentEditor(MainEditor, subDocs2, EFPDataGridViewState.InsertCopy);
        sde.SuppressInsertColumnValues = true; // не нужна инициализация, иначе некоторые поля с режимом NewMode=AlwaysDefaultValue очистятся
        if (!sde.Run())
          return;
      }
      else
      {
        if (EFPApp.MessageBox("Вставить " + (docTypeBase.DocTypeBase.IsSubDoc ?
          "копии поддокументов" : "копии документов") + " (" + subDocs2.SubDocCount.ToString() + " шт.)?",
          "Подтверждение вставки поддокументов \"" + SubDocType.PluralTitle + "\"",
          MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)

          return;
      }

      int count = SubDocs.SubDocCount;
      int lastOrder = 0;
      if (!String.IsNullOrEmpty(ManualOrderColumn))
        lastOrder = DataTools.MaxInt(SubDocs.SubDocsView, ManualOrderColumn, true) ?? 0;

      SubDocs.MergeSubSet(subDocs2);
      List<DataRow> resRows = new List<DataRow>();
      for (int i = count; i < SubDocs.SubDocCount; i++)
      {
        if (!String.IsNullOrEmpty(ManualOrderColumn))
        {
          // Присваиваем новой строке номер по порядку, чтобы она была в конце
          //SubDocs[i].Values[ManualOrderColumn].SetInteger(i + 1); // исправлено 08.12.2015
          // Еще раз исправлено 21.04.2020
          lastOrder++;
          SubDocs[i].Values[ManualOrderColumn].SetInteger(lastOrder); // исправлено 08.12.2015
        }
        DataRow resRow = SubDocs.SubDocsView.Table.Rows[i];
        resRows.Add(GetSlaveRow(resRow));
      }
      this.SelectedDataRows = resRows.ToArray();
      MainEditor.SubDocsChangeInfo.Changed = true;
    }

    #endregion

    #endregion
  }
}
