// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using System.ComponentModel;
using System.Data;
using FreeLibSet.Models.Tree;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.Remoting;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Провайдер иерархического просмотра поддокументов
  /// </summary>
  public class EFPSubDocTreeView : EFPDBxTreeView, IEFPSubDocView
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
    public EFPSubDocTreeView(EFPBaseProvider baseProvider, TreeViewAdv control, DocumentEditor mainEditor, DBxMultiSubDocs subDocs)
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
    public EFPSubDocTreeView(IEFPControlWithToolBar<TreeViewAdv> controlWithToolBar, DocumentEditor mainEditor, DBxMultiSubDocs subDocs)
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
    public EFPSubDocTreeView(EFPBaseProvider baseProvider, TreeViewAdv control, DBxMultiSubDocs subDocs, DBUI ui)
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
    public EFPSubDocTreeView(IEFPControlWithToolBar<TreeViewAdv> controlWithToolBar, DBxMultiSubDocs subDocs, DBUI ui)
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
      if (String.IsNullOrEmpty(subDocs.SubDocType.TreeParentColumnName))
        throw new ArgumentException("Поддокументы \"" + subDocs.SubDocType.PluralTitle + "\" не поддерживают древовидное представление. Свойство DBxSubDocType.TreeParentColumnName не установлено", "subDocs");


      _MainEditor = mainEditor;

      _SubDocs = subDocs;
      _SubDocTypeUI = base.UI.DocTypes[subDocs.Owner.DocType.Name].SubDocTypes[subDocs.SubDocType.Name]; // чтобы свойство работало быстро

      _ValidateBeforeEdit = false;
      _TableCache = _SubDocTypeUI.TableCache;

      //Control.ReadOnly = true;

      base.GridProducer = _SubDocTypeUI.GridProducer; // 25.03.2021
      base.ConfigSectionName = subDocs.SubDocType.Name;
      CommandItems.EnterAsOk = false;
      base.CanMultiEdit = SubDocTypeUI.CanMultiEdit; // 14.03.2019

      if (_SubDocTypeUI.HasEditorHandlers && mainEditor != null)
      {
        this.ReadOnly = DocProvider.DBPermissions.TableModes[SubDocType.Name] != DBxAccessMode.Full;
        //if (MainEditor != null)
        this.ReadOnly |= mainEditor.IsReadOnly;
        //this.CanMultiEdit = false;
        base.CanInsertCopy = SubDocTypeUI.CanInsertCopy; // 14.03.2019
        Control.SelectionMode = TreeViewAdvSelectionMode.Multi; // 14.03.2019. Можно удалять несколько записей, но не всегда можно редактировать
      }
      else
      {
        // 23.08.2012
        this.ReadOnly = true;
        this.CanView = false;
      }

      // _CanInlineEdit = true;
      _ConfirmDeletion = true;
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Основной редактор документа, использующий данный просмотр.
    /// Задается в конструкторе.
    /// Может быть null, если просмотр используется для выбора поддокумента вне редактора.
    /// </summary>
    public DocumentEditor MainEditor { get { return _MainEditor; } }
    private DocumentEditor _MainEditor;

    /// <summary>
    /// Тип редактируемых поддокументов. Задается в конструкторе.
    /// </summary>
    public DBxSubDocType SubDocType { get { return _SubDocTypeUI.SubDocType; } }

    /// <summary>
    /// Редактируемые поддокументы (объект доступа). Ссылка извлекается из
    /// <see cref="MainEditor"/>.Documents в конструкторе.
    /// </summary>
    public DBxMultiSubDocs SubDocs { get { return _SubDocs; } }
    private DBxMultiSubDocs _SubDocs;

    /// <summary>
    /// Провайдер для доступа к документам
    /// </summary>
    public DBxDocProvider DocProvider { get { return _SubDocs.DocSet.DocProvider; } }

    /// <summary>
    /// Интерфейс для доступа к поддокументам
    /// </summary>
    public SubDocTypeUI SubDocTypeUI { get { return _SubDocTypeUI; } }
    private SubDocTypeUI _SubDocTypeUI;

    /// <summary>
    /// Кэшированные данные для таблицы поддокументов
    /// </summary>
    public DBxTableCache TableCache { get { return _TableCache; } }
    private DBxTableCache _TableCache;


    /// <summary>
    /// Эти данные передаются обработчику инициализации просмотра.
    /// Свойство может устанавливаться только до вызова события <see cref="EFPControlBase.Created"/>, то есть сразу после вызова конструктора.
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
    /// Если установлено в true, то перед добавлением и редактированием записей
    /// вызывается <see cref="DocumentEditor.WriteData()"/>. В этом случае редактор поддокумента
    /// может использовать актуальные значения полей основного документа.
    /// По умолчанию (false) проверка не выполняется. Допускается редактирование
    /// поддокументов, даже если на какой-либо вкладке редактора основного документа
    /// есть некорректно заполненные поля.
    /// Нельзя устанавливать в true, если <see cref="MainEditor"/>==null.
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
    /// Поля, которые требуются просмотру
    /// </summary>
    internal DBxColumns UsedColumnNames { get { return _UsedColumnNames; } }
    private DBxColumns _UsedColumnNames;

    ///// <summary>
    ///// Если true (по умолчанию), то допускается редактирование "по месту".
    ///// Для запрета редактирования нужно устанавливить в false.
    ///// Простая установка свойства GridHandler.Control.ReadOnly=true не даст эффекта,
    ///// т.к. свойство переустанавливается, если пользователь изменит настройку
    ///// просмотра
    ///// Для выборочного запрета редактирования ячеек используйте, вместо этого свойства,
    ///// свойство GridProducerColumn.ReadOnly при объявлении поддокумента
    ///// </summary>
    //public bool CanInlineEdit { get { return _CanInlineEdit; } set { _CanInlineEdit = value; } }
    //private bool _CanInlineEdit;

    #endregion

    #region Изменение ProviderState

    /// <summary>
    /// Закончить инициализацию просмотра. 
    /// Добавление команд локального меню должно быть закончено до вызова метода.
    /// </summary>
    protected override void OnCreated()
    {
      DBxColumnList columns = new DBxColumnList();
      SubDocTypeUI.PerformInitTree(this, false, columns, _UserInitData);
      _UsedColumnNames = new DBxColumns(columns);

      InitTreeSource();

      // В режиме просмотра ручная сортировка поддокументов не допускается
      if (!base.ReadOnly)
      {
        this.ManualOrderColumn = SubDocTypeUI.ManualOrderColumn;
      }

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
      //base.CommandItems.AddTextPasteFormats();

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
        // TODO: 05.02.2021
        //if ((!String.IsNullOrEmpty(ManualOrderColumn)) && (!this.ReadOnly))
        //  InitOrderValues(_SubDocs.SubDocsView.Table, false /* 21.04.2020 */);

        //DataView dv = FSubDocs.SubDocsData.DefaultView;
        // !!! надо бы централизовано как-то
        if (SourceAsDataView != null)
        {
          if (!String.IsNullOrEmpty(SubDocTypeUI.ManualOrderColumn))
            SourceAsDataView.Sort = SubDocTypeUI.ManualOrderColumn;
          else
            SourceAsDataView.Sort = SubDocType.DefaultOrder.ToString();
          if (MainEditor != null)
          {
            if (MainEditor.MultiDocMode)
            {
              if (String.IsNullOrEmpty(SourceAsDataView.Sort))
                SourceAsDataView.Sort = "DocId";
              else
                SourceAsDataView.Sort += ",DocId";
            }
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка установки порядка строк поддокументов \"" + SubDocType.PluralTitle + "\"");
      }
      //Control.DataSource = FDataSource;


      //MainGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
      //MainGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;



      // Присоединяем сетку к форме до присоединения локального меню
      //ParentControl.Controls.Add(FGridPanel);
      //ParentControl.Controls.Add(FSpeedPanel);


      // Локальное меню, панель и статусная строка
      //ControlProvider.ToolBarPanel = SpeedPanel;

      base.OnCreated();
    }


    /// <summary>
    /// Присоединяет обработчик в <see cref="DocumentEditor"/>
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();

      if (_MainEditor != null)
        _MainEditor.AfterWrite += new DocEditEventHandler(MainEditor_AfterWrite);
    }

    /// <summary>
    /// Отсоединяет обработчик от <see cref="DocumentEditor"/>
    /// </summary>
    protected override void OnDetached()
    {
      if (_MainEditor != null)
        _MainEditor.AfterWrite -= new DocEditEventHandler(MainEditor_AfterWrite);

      base.OnDetached();
    }

#if XXX

    /// <summary>
    /// Пользователь изменил настройку табличного просмотра
    /// </summary>
    /// <param name="Sender"></param>
    /// <param name="Args"></param>
    void GridHandler_CurrentGridConfigChanged(object Sender, CancelEventArgs Args)
    {
      UsedColumnNames = new DBxColumnList();
      SubDocTypeUI.DoInitGrid(this, true, UsedColumnNames, null, MainEditor.MultiDocMode, true);
      AfterInitColumns();
      base.PerformRefresh();
      Args.Cancel = true;
    }

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

      DataTable EmptyTable = SubDocs.DocSet.DocProvider.GetTemplate(SubDocType.DocType.Name, SubDocType.Name);

      bool HasEditable = false;
      bool[] ROs = new bool[this.Columns.Count];
      for (int i = 0; i < this.Columns.Count; i++)
      {
        ROs[i] = !IsColumnEditable(this.Columns[i], EmptyTable);
        if (!ROs[i])
          HasEditable = true;

        //if (!Editable)
        //  DebugTools.DebugObject(GridHandler.Columns[i].GridColumn, "i=" + i.ToString());
      }

      //DebugTools.DebugObject(GridHandler.Columns[3].GridColumn, "до");
      this.Control.ReadOnly = !HasEditable;
      if (HasEditable)
      {
        for (int i = 0; i < this.Columns.Count; i++)
          this.Columns[i].GridColumn.ReadOnly = ROs[i];
      }
      //DebugTools.DebugObject(GridHandler.Columns[3].GridColumn, "после");
    }

    private bool IsColumnEditable(EFPDataGridViewColumn Column, DataTable EmptyTable)
    {
      if (!((Column.GridColumn is DataGridViewTextBoxColumn ||
        Column.GridColumn is DataGridViewCheckBoxColumn)))
        return false;

      if (String.IsNullOrEmpty(Column.GridColumn.DataPropertyName))
        return false; // вычисляемый столбец

      GridProducerColumn ColProducer = Column.ColumnProducer as GridProducerColumn;
      if (ColProducer != null) // 19.04.14
      {
        if (ColProducer.ReadOnly)
          return false;
      }
      if (Column.CanIncSearch)
        return false;

      //      if (Column.GridColumn.DataPropertyName.IndexOf('.') >= 0)
      //        return false; // ссылочный столбец
      // Могут быть вычисляемые поля без точек
      if (EmptyTable.Columns.IndexOf(Column.GridColumn.DataPropertyName) < 0)
        return false;

      switch (Column.GridColumn.DataPropertyName)
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
    /// <param name="Table">Таблица данных поддокументов</param>
    private void InitOrderValues(DataTable Table)
    {
      DataView dv = new DataView(Table);
      dv.Sort = ManualOrderColumn;
      DataRow[] Rows = DataTools.GetDataViewRows(dv);

      bool Changed = false;

      for (int i = 0; i < Rows.Length; i++)
      {
        int ThisN = DataTools.GetInt(Rows[i], ManualOrderColumn);
        if (ThisN != (i + 1)) // присваиваем только если не совпадает
        {
          Rows[i][ManualOrderColumn] = i + 1;
          Changed = true;
        }
      }

      if (Changed)
      {
        if (ManualOrderChanged != null)
          ManualOrderChanged(this, EventArgs.Empty);
        if (MainEditor.SubDocsChangeInfo != null)
          MainEditor.SubDocsChangeInfo.Changed = true;
      }
    }
#endif


    /// <summary>
    /// Создает объект <see cref="DBxDataRowValueArrayWithCache"/>
    /// </summary>
    /// <returns>Новый объект для доступа к данным</returns>
    protected override IDataRowNamedValuesAccess CreateRowValueAccessObject()
    {
      return new DBxDataRowValueArrayWithCache(SubDocTypeUI.TableCache);
    }

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Устанавливает MainEditor.SubDocsChangeInfo.Changed=true.
    /// </summary>
    /// <param name="args">Не используется</param>
    protected override void OnManualOrderChanged(EventArgs args)
    {
      base.OnManualOrderChanged(args);
      if (MainEditor != null)
      {
        if (MainEditor.SubDocsChangeInfo != null)
          MainEditor.SubDocsChangeInfo.Changed = true;
      }
    }

    #endregion

    #region Редактирование поддокумента

    void MainEditor_AfterWrite(object sender, DocEditEventArgs args)
    {
      PerformRefresh(); // 28.04.2022
    }

    /// <summary>
    /// Если true (по умолчанию), то перед удалением выбранных поддокументов запрашивается подтверждение.
    /// Если false, то удаление выполняется немедленно без запроса.
    /// </summary>
    public bool ConfirmDeletion { get { return _ConfirmDeletion; } set { _ConfirmDeletion = value; } }
    private bool _ConfirmDeletion;

    /// <summary>
    /// Удаляет текущие выбранные поддокументы без выдачи дополнительных запросов.
    /// Свойство <see cref="ConfirmDeletion"/> игнорируется.
    /// Этот метод может использоваться в обработчике команды Cut.
    /// </summary>
    public void PerformDeleteDataQuiet()
    {
      bool oldConfirmDeletion = ConfirmDeletion;
      try
      {
        ConfirmDeletion = false;
        PerformEditData(UIDataState.Delete);
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
        if (!MainEditor.WriteData())
          return true;
      }

      // TODO: Не должно ли быть это внутри SubDocumentEditor.Run()?
      DBxMultiSubDocs subDocs2;
      DataRow[] rows = null;
      if (this.State == UIDataState.Insert)
      {
        Int32[] docIds = SubDocTypeUI.SelectDocsForInsert(this);
        if (docIds == null)
          return true;
        subDocs2 = new DBxMultiSubDocs(SubDocs, DataTools.EmptyIds);
        foreach (Int32 docId in docIds)
        {
          DBxSingleDoc doc = SubDocs.Owner.GetDocById(docId);
          subDocs2.Insert(doc);
        }
      }
      else
      {
        rows = this.SelectedDataRows;
        if (rows.Length == 0)
        {
          EFPApp.MessageBox("Нет выбранных поддокументов \"" + SubDocType.PluralTitle + "\"");
          return true;
        }
        subDocs2 = new DBxMultiSubDocs(SubDocs, rows);
        if (this.State == UIDataState.Delete)
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


          //!!!if (!String.IsNullOrEmpty(ManualOrderColumn))
          //!!!  InitOrderValues(FSubDocs.SubDocsView.Table);
          MainEditor.SubDocsChangeInfo.Changed = true;
          return true;
        }
        switch (this.State)
        {
          case UIDataState.Edit:
            subDocs2.Edit();
            break;
          case UIDataState.InsertCopy:
            subDocs2.InsertCopy();
            break;
        }
      }

      SubDocumentEditor sde = new SubDocumentEditor(MainEditor, subDocs2, this.State);
      if (sde.Run())
      {
        SubDocs.MergeSubSet(subDocs2);

        SubDocTypeUI.InitManualOrderColumnValueAfterEdit(this.SubDocs, subDocs2);

        if (this.State == UIDataState.Insert || this.State == UIDataState.InsertCopy)
        {
          DataRow lastRow = _SubDocs.SubDocsView.Table.Rows[_SubDocs.SubDocsView.Table.Rows.Count - 1];
          try { this.CurrentDataRow = lastRow; }
          catch { } // 28.04.2022
        }
        MainEditor.SubDocsChangeInfo.Changed = true;
        if (this.State == UIDataState.Edit)
        {
          try
          {
            this.SelectedDataRows = rows; // 22.02.2022
            //!!!!!!!!!!UpdateSelectedRows();
          }
          catch (Exception ex)
          {
            EFPApp.ShowException(ex, "Ошибка обновления строк");
          }
        }
      }

      return true;
    }

#if XXX
    void FGrid_CellValidated(object Sender, DataGridViewCellEventArgs Args)
    {
      try
      {
        // после inline-редактирования поддокументы считаются модифицированными
        if (FMainEditor != null)
        {
          if (Control.IsCurrentCellInEditMode)
            FMainEditor.SubDocsChangeInfo.Changed = true;
        }
      }
      catch
      {
      }
    }
#endif

    #endregion

    #region Обновление данных

    /// <summary>
    /// Обновление или первичная инициализация просмотра
    /// </summary>
    /// <param name="args">Не используется</param>
    protected override void OnRefreshData(EventArgs args)
    {
      if (Control.Model != null) // 02.05.2022 - реальная перезагрузка.
        _SubDocTypeUI.DBCache.Clear();

      if (DocProvider.DBPermissions.TableModes[SubDocType.Name] != DBxAccessMode.None)
      {
        InitTreeSource();
      }

      base.OnRefreshData(args);
    }

    private void InitTreeSource()
    {
      if (SubDocs.DocSet.DocProvider.DBPermissions.TableModes[SubDocType.Name] != DBxAccessMode.None)
      {
        DBxSubDocTreeModel model = new DBxSubDocTreeModel(SubDocs, null);
        Control.Model = model;
      }
      else
        Control.Model = null;
    }

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool HasRefreshDataHandler { get { return true; } }

    #endregion

    #region Буфер обмена

    #region Вырезать

    void CommandItems_Cut(object sender, EventArgs args)
    {
      // Простое действие: Копировать + удалить
      if (!CommandItems.PerformCopy())
        return;

      PerformDeleteDataQuiet();
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
      //ds.RemotingFormat = SerializationFormat.Binary;
      //ds.ExtendedProperties["WorkAreaIdentity"] = DocProvider.DBIdentity;
      ds.ExtendedProperties["DBIdentity"] = DocProvider.DBIdentity; // 15.05.2020
      DataTable table = this.SourceAsDataTable.Clone();
      table.TableName = _SubDocTypeUI.SubDocType.Name;
      ds.Tables.Add(table);
      for (int i = 0; i < rows.Length; i++)
        table.Rows.Add(rows[i].ItemArray);

      SerializationTools.PrepareDataSet(ds); // 07.07.2022
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
      DataTable table = UI.DocProvider.FillSelect(sdt.SubDocType.Name, null,
        filter, null);

      if (table.Rows.Count == 0)
      {
        EFPApp.ErrorMessageBox("Выбранные документы \"" + fmtDocSel.DocType.DocType.PluralTitle + "\" (" + docIds.Length.ToString() +
          " шт.) не содержат ни одного поддокумента \"" + sdt.SubDocType.SingularTitle + "\"");
        return;
      }

      DoPasteTable(table, sdt);
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
        if (!MainEditor.WriteData())
          return;
      }

      Int32[] docIds = SubDocTypeUI.SelectDocsForInsert(this);
      if (docIds == null)
        return;

      //DBxSingleDoc mainDoc = MainEditor.Documents[SubDocType.DocType.Name].GetDocById(docId);

      DBxSubDoc[] newSubDocs = SubDocTypeUI.PerformPasteRows(MainEditor.Documents[SubDocType.DocType.Name].SubDocs[SubDocType.Name], docIds, srcRows, docTypeBase, this);
      if (newSubDocs == null)
        return;

      DataRow[] resRows = new DataRow[newSubDocs.Length];
      for (int i = 0; i < resRows.Length; i++)
        resRows[i] = SubDocs.SubDocsView.Table.Rows.Find(newSubDocs[i].SubDocId);
      this.SelectedDataRows = resRows;
    }

    #endregion

    #endregion

    #region Ручная сортировка строк

    /// <summary>
    /// Если для поддокумента задан иерархический порядок (свойство <see cref="DBxDocTypeBase.TreeParentColumnName"/>),
    /// то возвращается сортировщий для иерархической модели, а не обычный DataTableReorderHelper.
    /// 
    /// Если в редакторе документа есть и иерархический просмотр и плоская таблица, то в них будут одинаково переставляться строки.
    /// </summary>
    /// <returns>Объект, реализующий <see cref="IDataReorderHelper"/></returns>
    protected override IDataReorderHelper CreateDefaultDataReorderHelper()
    {
      if (SubDocs.Owner.DocCount != 1)
        throw new InvalidOperationException("Использование ручной сортировки поддокументов не допускается, если одновременно редактируется несколько документов");

      return base.CreateDefaultDataReorderHelper();
    }

    /// <summary>
    /// Инициализация поддокументов в просмотре, для которых не установлено поле для ручной сортировки строк (свойство <see cref="FreeLibSet.Forms.Docs.SubDocTypeUI.ManualOrderColumn"/>)
    /// </summary>
    public void InitManualOrderColumnValue()
    {
      if (SubDocTypeUI.InitManualOrderColumnValue(this.SubDocs))
        OnManualOrderChanged(EventArgs.Empty);
    }

    #endregion

    #region Оформление просмотра

    /// <summary>
    /// Вызывает <see cref="DocTypeUIBase.GetImageKey(DataRow)"/>
    /// </summary>
    /// <param name="node">Узел дерева</param>
    /// <returns>Имя изображения для поддокумента</returns>
    public override string GetNodeImageKey(TreeNodeAdv node)
    {
      // 01.12.2015
      // Значок поддокумента отображаем и для корневых узлов
      // if (!Node.IsLeaf)
      //   return String.Empty;

      DataRow row = node.Tag as DataRow;
      if (row != null)
        return SubDocTypeUI.GetImageKey(row);
      else
        return "UnknownState";
    }


    #endregion
  }

#if XXX
  /// <summary>
  /// Источник данных дерева для поддокументов
  /// </summary>
  public class SubDocsTreeSource : DataTableTreeSource, IEFPTreeSource_666
  {
  #region Конструктор

    public SubDocsTreeSource(DBxMultiSubDocs SubDocs, SubDocTypeUI SubDocTypeUI, string ParentColumnName, string TextColumnName)
      : base(SubDocs.SubDocsView.Table, "Id", ParentColumnName, GetBaseTextColumnName(TextColumnName))
    {
      if (SubDocs == null)
        throw new ArgumentNullException("SubDocs");
      FSubDocs = SubDocs;
      if (SubDocTypeUI == null)
        throw new ArgumentNullException("SubDocTypeUI");
      FSubDocTypeUI = SubDocTypeUI;
      FTextColumnName = TextColumnName;
    }

    private static string GetBaseTextColumnName(string TextColumnName)
    {
      if (String.IsNullOrEmpty(TextColumnName))
        throw new ArgumentNullException("TextColumnName");
      int p = TextColumnName.IndexOf('.');
      if (p >= 0)
        return TextColumnName.Substring(0, p);
      else
        return TextColumnName;
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Имя текстового столбца может содержать точку
    /// </summary>
    public new string TextColumnName { get { return FTextColumnName; } }
    private string FTextColumnName;

    public DBxMultiSubDocs SubDocs { get { return FSubDocs; } }
    private DBxMultiSubDocs FSubDocs;

    public SubDocTypeUI SubDocTypeUI { get { return FSubDocTypeUI; } }
    private SubDocTypeUI FSubDocTypeUI;

  #endregion

  #region Переопределенные методы

    public override string GetText(object TreeItem)
    {
      // TODO: Переделать на вызов GetTextValue()

      DataRow Row = TreeItem as DataRow;
      if (Row == null)
        throw new NullReferenceException("Неправильный объект Node");

      object BaseValue = Row[base.TextColumnName];
      if (this.TextColumnName.IndexOf('.') >= 0)
      {
        object v = SubDocTypeUI.TableCache.GetRefValue(this.TextColumnName, DataTools.GetInt(BaseValue));
        return DataTools.GetString(v);
      }
      else
        return DataTools.GetString(BaseValue);
    }

  #endregion

  #region IEFPTreeSource Members

    public void GetImageKey(object TreeItem, out string ImageKey, out string SelectedImageKey)
    {
      if (ContainsChildren(TreeItem))
      {
        ImageKey = "TreeViewClosedFolder";
        SelectedImageKey = "TreeViewOpenFolder";
      }
      else
      {
        DataRow Row = TreeItem as DataRow;
        ImageKey = SubDocTypeUI.GetImageKey(Row);
        SelectedImageKey = ImageKey;
      }
    }

  #endregion

  #region Вспомогательные методы

    public Int32[] GetIdsWithChildren(Int32 Id)
    {
      if (Id == 0)
        return DataTools.EmptyIds;
      else
      {
        DataRow Row = SubDocs.SubDocsView.Table.Rows.Find(Id);
        if (Row==null)
          return DataTools.EmptyIds;

        DataRow[] Rows = base.GetRowsWithChildren((object)Row);
        return DataTools.GetIds(Rows);
      }
    }

  #endregion
  }

#endif
}
