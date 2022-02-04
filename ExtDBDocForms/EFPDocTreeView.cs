// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data.Docs;
using System.Data;
using FreeLibSet.Forms;
using System.Collections;
using FreeLibSet.Data;
using FreeLibSet.Models.Tree;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Remoting;
using FreeLibSet.Core;
using FreeLibSet.Controls;

#pragma warning disable 0219 // TODO: Убрать лишние переменные

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Провайдер иерархического просмотра документов
  /// </summary>
  public class EFPDocTreeView : EFPDBxTreeView, IEFPDocView
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент - дерево</param>
    /// <param name="ui">Интерфейс доступа к документам</param>
    /// <param name="docTypeName">Имя вида документов</param>
    public EFPDocTreeView(EFPBaseProvider baseProvider, TreeViewAdv control, DBUI ui, string docTypeName)
      : this(baseProvider, control, ui.DocTypes[docTypeName])
    {
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент - дерево</param>
    /// <param name="docTypeUI">Интерфейс доступа к виду документов</param>
    public EFPDocTreeView(EFPBaseProvider baseProvider, TreeViewAdv control, DocTypeUI docTypeUI)
      : base(baseProvider, control, docTypeUI.UI)
    {
      Init(docTypeUI);
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    /// <param name="ui">Интерфейс доступа к документам</param>
    /// <param name="docTypeName">Имя вида документов</param>
    public EFPDocTreeView(IEFPControlWithToolBar<TreeViewAdv> controlWithToolBar, DBUI ui, string docTypeName)
      : this(controlWithToolBar, ui.DocTypes[docTypeName])
    {
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    /// <param name="docTypeUI">Интерфейс доступа к виду просматриваемых документов</param>
    public EFPDocTreeView(IEFPControlWithToolBar<TreeViewAdv> controlWithToolBar, DocTypeUI docTypeUI)
      : base(controlWithToolBar, docTypeUI.UI)
    {
      Init(docTypeUI);
    }

    private void Init(DocTypeUI docTypeUI)
    {
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");

      _DocTypeUI = docTypeUI;

      base.GridProducer = docTypeUI.GridProducer;
      base.ConfigSectionName = DocType.Name;
      base.ReadOnly = docTypeUI.UI.DocProvider.DBPermissions.TableModes[DocType.Name] != DBxAccessMode.Full;
      base.CanView = true;
      base.CanMultiEdit = docTypeUI.CanMultiEdit;
      //Control.LabelEdit = false;
      Control.SelectionMode = TreeViewAdvSelectionMode.Multi; // Можно удалять несколько записей, но не всегда можно редактировать

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
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс доступа к виду просматриваемых документов
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private DocTypeUI _DocTypeUI;

    /// <summary>
    /// Тип просматриваемого документа
    /// </summary>
    public DBxDocType DocType { get { return _DocTypeUI.DocType; } }


    //public string TextColumnName { get { return FTextColumnName; } }
    //private string FTextColumnName;


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
    /// Список используемых столбцов в базовом наборе данных
    /// </summary>
    internal DBxColumns UsedColumnNames { get { return _UsedColumnNames; } }
    private DBxColumns _UsedColumnNames; 

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
      get { return ciShowDeleted.Visible; }
      set { ciShowDeleted.Visible = value; }
    }
    private EFPCommandItem ciShowDeleted;

    void ShowDeletedClick(object sender, EventArgs args)
    {
      ShowDeleted = !ShowDeleted;
    }

    #endregion

    #region Загрузка данных

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool HasRefreshDataHandler { get { return true; } }

    /// <summary>
    /// Исходная модель с учетом обычных фильтров Filters, но без учета дополнительного фильтра по группам
    /// </summary>
    private DBxDocTreeModel _OriginalModel;

    /// <summary>
    /// Обновление данных или первоначальная загрузка
    /// </summary>
    /// <param name="args">Не используется</param>
    protected override void OnRefreshData(EventArgs args)
    {
      if (args == null)
        EFPApp.BeginWait("Загрузка данных", DocTypeUI.TableImageKey);
      else
        EFPApp.BeginWait("Обновление данных", "Refresh");
      try
      {
        DBxFilter Filter = Filters.GetSqlFilter();
        if ((!ShowDeleted) && UI.DocProvider.DocTypes.UseDeleted /* 23.05.2021 */)
        {
          if (Filter == null)
            Filter = DBSDocType.DeletedFalseFilter;
          else
            Filter = AndFilter.FromArray(new DBxFilter[] { Filter, DBSDocType.DeletedFalseFilter });
        }

        _OriginalModel = new DBxDocTreeModel(DocTypeUI.UI.DocProvider, DocTypeUI.DocType, UsedColumnNames, Filter);
        InitAuxFiltersModel();

        base.CallRefreshDataEventHandler(args);
      }
      finally
      {
        EFPApp.EndWait();
      }
#if DEBUG
      if (Control.Model == null)
        throw new NullReferenceException("Control.Model==null");
#endif
    }


    /// <summary>
    /// Возвращает true, если в таблице есть первичный ключ по полю Id
    /// </summary>
    internal bool HasPrimaryKey
    {
      get
      {
        if (SourceAsDataTable == null)
          return false;
        return String.Compare(DataTools.GetPrimaryKey(SourceAsDataTable), "Id", StringComparison.OrdinalIgnoreCase) == 0;
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
        if (_OriginalModel != null)
        {
          EFPDataTreeViewSelection OldSel = Selection;
          InitAuxFiltersModel();
          Selection = OldSel; // по возможности
        }
      }
    }
    private Int32[] _AuxFilterGroupIds;

    private void InitAuxFiltersModel()
    {
#if DEBUG
      if (_OriginalModel == null)
        throw new NullReferenceException("OriginalModel==null");
#endif

      Int32[] OldIds = SelectedIds;

      if (AuxFilterGroupIds == null)
        Control.Model = _OriginalModel;
      else
      {
        DBxFilter Filter2;
        if (AuxFilterGroupIds.Length == 0)
          Filter2 = new ValueFilter(DocTypeUI.DocType.GroupRefColumnName, 0);
        else
          Filter2 = new IdsFilter(DocTypeUI.DocType.GroupRefColumnName, AuxFilterGroupIds);
        Control.Model = new FilteredDataTableTreeModel(_OriginalModel, Filter2, _OriginalModel.IntegrityFlagColumnName);
      }

      try
      {
        SelectedIds = OldIds;
      }
      catch { }
    }


    #endregion

    #region Оформление просмотра

    /// <summary>
    /// Возвращает имя изображения для документа, к которому относится узел.
    /// Изображение находится в списке EFPApp.MainImages
    /// </summary>
    /// <param name="node">Узел</param>
    /// <returns>Имя изображения</returns>
    public override string GetNodeImageKey(TreeNodeAdv node)
    {
      if (!node.IsLeaf)
        return String.Empty;

      DataRow Row = node.Tag as DataRow;
      if (Row != null)
        return DocTypeUI.GetImageKey(Row);
      else
        return "UnknownState";
    }

    #endregion

    #region Редактирование

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

    /// <summary>
    /// Вызывает DocTypeUI.PerformEditing() для выбранных узлов
    /// </summary>
    /// <param name="args">Не используется</param>
    /// <returns>Игнорируется</returns>
    protected override bool OnEditData(EventArgs args)
    {
      DocTypeUI.PerformEditing(SelectedIds, State, Control.FindForm().Modal, ViewHandler);
      return true;
    }

    #endregion

    #region Реализация DocumentViewHandler

    /// <summary>
    /// Реализация DocumentViewHandler для EFPDocTreeView.
    /// Частично переопределяется в EFPGroupDocTreeView
    /// </summary>
    protected class IntDocumentViewHandler : FreeLibSet.Forms.Docs.DocumentViewHandler
    {
      #region Конструктор и Dispose

      /// <summary>
      /// Конструктор объекта
      /// </summary>
      /// <param name="owner">Провайдер иерархического просмотра - владелец</param>
      public IntDocumentViewHandler(EFPDocTreeView owner)
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
      /// Провайдер иерархического просмотра - владелец
      /// </summary>
      public EFPDocTreeView Owner { get { return _Owner; } }
      private EFPDocTreeView _Owner;

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
      /// Возвращает EFPDocTreeView.BrowserGuid
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
      /// Возвращает EFPDocTreeView.CurrentColumnName
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

        DataTable SrcTable = dataSet.Tables[Owner.DocType.Name];
        List<DataRow> NewSelRows = new List<DataRow>();

        DBxColumns FilterColumns = Owner.Filters.GetColumnNames();

        // 08.07.2016
        // Таблица может не содержать первичного ключа
        DataView dvFind = null;
        if (!Owner.HasPrimaryKey)
        {
          dvFind = new DataView(Owner.SourceAsDataTable);
          dvFind.Sort = "Id";
        }
        try
        {
          foreach (DataRow SrcRow in SrcTable.Rows)
          {
            // 15.07.2020. Алгоритм изменен

            #region Определяем необходимость наличия строки в просмотре

            bool Visible;
            switch (SrcRow.RowState)
            {
              case DataRowState.Added: // нельзя базироваться только на RowState
                if (isCaller)
                  Visible = true; // Не уверен, что надо всегда показывать
                else
                  Visible = TestFilters(SrcRow, FilterColumns);
                break;
              case DataRowState.Modified:
                Visible = TestFilters(SrcRow, FilterColumns);
                break;
              case DataRowState.Deleted:
                if (Owner.ShowDeleted)
                  Visible = TestFilters(SrcRow, FilterColumns);
                else
                  Visible = false;
                break;
              default: // Unchanged
                continue;

            }

            #endregion

            #region Определяем существование строки в просмотре

            Int32 DocId;
            if (SrcRow.RowState == DataRowState.Deleted)
              DocId = (Int32)(SrcRow["Id", DataRowVersion.Original]);
            else
              DocId = (Int32)(SrcRow["Id"]);
            DataRow ResRow = FindDocRow(DocId, dvFind);

            #endregion

            #region Добавление / обновление / удаление строки в просмотре

            if (Visible)
            {
              if (ResRow == null)
              {
                ResRow = Owner.SourceAsDataTable.NewRow();
                DataTools.CopyRowValues(SrcRow, ResRow, true);
                UpdateRefValues(SrcRow, ResRow);
                Owner.SourceAsDataTable.Rows.Add(ResRow);
              }
              else
              {
                DataTools.CopyRowValues(SrcRow, ResRow, true);
                UpdateRefValues(SrcRow, ResRow);
                Owner.InvalidateDataRow(ResRow); // не Update
              }

              // Перенесено сюда 26.05.2021
              // Может быть несколько просмотров, привязанных к одной кэшированной таблице данных.
              // В этом случае предыдущее условие "ResRow==null" выполняется для первого просмотра,
              // а второй просмотр не будет добавлять дублирующую строку в таблицу.
              // Но позиционировать на новую строку надо в любом случае, если она видна в просмотре.
              if (SrcRow.RowState == DataRowState.Added)
                NewSelRows.Add(ResRow);
            }
            else
            {
              if (ResRow != null)
                ResRow.Delete();
            }

            #endregion
#if XXX
            switch (SrcRow.RowState)
            {
              case DataRowState.Added: // нельзя базироваться только на RowState
                DocId = (Int32)(SrcRow["Id"]);
                ResRow = FindDocRow(DocId, dvFind);
                if (ResRow == null)
                {
                  ResRow = Owner.SourceAsDataTable.NewRow();
                  CopyRowValues(SrcRow, ResRow);
                  Owner.SourceAsDataTable.Rows.Add(ResRow);
                  NewSelRows.Add(ResRow);
                }
                else
                {
                  CopyRowValues(SrcRow, ResRow);
                  Owner.InvalidateDataRow(ResRow); // не Update
                }
                Changed = true;
                break;

              case DataRowState.Modified:
                DocId = (Int32)(SrcRow["Id"]);
                ResRow = FindDocRow(DocId, dvFind);
                if (ResRow == null)
                  continue;
                CopyRowValues(SrcRow, ResRow);
                Owner.InvalidateDataRow(ResRow); // не Update
                Changed = true;
                break;
              case DataRowState.Deleted:
                DocId = (Int32)(SrcRow["Id", DataRowVersion.Original]);
                ResRow = FindDocRow(DocId, dvFind);
                if (ResRow == null)
                  continue;
                if (Owner.ShowDeleted)
                {
                  ResRow["Deleted"] = true;
                  Owner.InvalidateDataRow(ResRow);
                }
                else
                  ResRow.Delete();
                break;
            }
#endif
          }

        }
        finally
        {
          if (dvFind != null)
            dvFind.Dispose();
        }
        if (NewSelRows.Count > 0)
          Owner.SelectedDataRows = NewSelRows.ToArray();

      }

      /// <summary>
      /// Заполнение строки DataRow <paramref name="resRow"/>, используемой моделью ирархического просмотра.
      /// Метод переопределяется в EFPGroupTreeView
      /// </summary>
      /// <param name="srcRow">Строка в обновленном наборе данных</param>
      /// <param name="resRow">Заполняемая строка в модели иерархического просмотра</param>
      protected virtual void CopyRowValues(DataRow srcRow, DataRow resRow)
      {
        DataTools.CopyRowValues(srcRow, resRow, true);
        UpdateRefValues(srcRow, resRow);
      }

      private DataRow FindDocRow(Int32 docId, DataView dvFind)
      {
        if (docId == 0)
          return null;

        if (dvFind == null)
          return Owner.SourceAsDataTable.Rows.Find(docId);
        else
        {
          int p = dvFind.Find(docId);
          if (p >= 0)
            return dvFind[p].Row;
          else
            return null;
        }
      }

      private void UpdateRefValues(DataRow srcRow, DataRow resRow)
      {
        // 13.11.2020
        // Строка srcRow может быть помечена на удаление
        if (srcRow.RowState == DataRowState.Deleted)
          return;

        for (int i = 0; i < resRow.Table.Columns.Count; i++)
        {
          string ColName = resRow.Table.Columns[i].ColumnName;
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

      #region Прочие определенные методы

      /// <summary>
      /// Вызывает EFPDocTreeView.UpdateRowsForIds()
      /// </summary>
      /// <param name="docIds"></param>
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

            if (Owner.CurrentId != 0)
            {
              if (newDoc.Values.IndexOf(DocTypeUI.DocType.TreeParentColumnName) >= 0)
                newDoc.Values[DocTypeUI.DocType.TreeParentColumnName].SetInteger(Owner.CurrentId);
            }

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
            //if (Owner.Filters != null)
            //  Owner.Filters.ValidateDocValues(SavingDoc, ErrorMessages);
          }
        }
        else
          ExternalEditorCaller.ValidateDocValues(savingDoc, errorMessages);
      }

      /// <summary>
      /// Вызывает TreeViewAdv.Invalidate()
      /// </summary>
      public override void InvalidateControl()
      {
        if (Owner != null)
          Owner.Control.Invalidate();
      }

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

      #endregion
    }

    /// <summary>
    /// Обработчик просмотра документов, связанный с текущим просмотром.
    /// Свойство имеет значение не null, когда просмотр выведен на экран
    /// </summary>
    public DocumentViewHandler ViewHandler { get { return _ViewHandler; } }
    private DocumentViewHandler _ViewHandler;

    /// <summary>
    /// Создает объект DocumentViewHandler для просмотра.
    /// Вызывается из OnShown(). Полученное значение присваивается свойству ViewHandler
    /// Методы может быть переопределен в производном классе (в том числе возвращать null),
    /// если требуется нестандартная обработка.
    /// </summary>
    /// <returns>Объект класса, производного от DocumentViewHandler</returns>
    protected virtual IntDocumentViewHandler CreateDocumentViewHandler()
    {
      return new IntDocumentViewHandler(this);
    }

    /// <summary>
    /// Идентификатор просмотра. Используется ViewHandler
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
      DBxColumnList columns = new DBxColumnList();
      DocTypeUI.PerformInitTree(this, false, columns, _UserInitData);
      _UsedColumnNames = new DBxColumns(columns);

      base.OnCreated();

      if (SourceAsDataTable == null)
      {
        try
        {
          Int32 OldId = CurrentId;
          PerformRefresh(); // обязательно после вызова OnCreated(), иначе UsedColumnNames будет равен null
          CurrentId = OldId; // 23.11.2017
        }
        catch (Exception e) // 04.02.2022
        {
          EFPApp.ShowException(e, DisplayName + " - ошибка загрузки данных");
        }
      }
    }

    /// <summary>
    /// Инициализирует свойство ViewHandler
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached(); // в том числе инициализирует столбцы

      _ViewHandler = CreateDocumentViewHandler();
      if (_ViewHandler != null)
        DocTypeUI.Browsers.Add(_ViewHandler);
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
  }
}
                         