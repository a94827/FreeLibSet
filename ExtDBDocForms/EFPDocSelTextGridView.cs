// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.Forms.Data;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Возможные значения свойства <see cref="EFPDocSelTextGridView.OrderMode"/>
  /// </summary>
  public enum EFPDocSelGridViewOrderMode
  {
    /// <summary>
    /// Значение по умолчанию.
    /// Порядок строк не может меняться пользователем, а определяется порядком идентификаторов
    /// </summary>
    Fixed,

    /// <summary>
    /// Порядок строк имеет значение.
    /// Пользователь может менять порядок строк (ручная сортировка).
    /// </summary>
    Manual,

    // Не реализовано. Пользователь может выбирать порядок сортировки (только для EFPDocSelGridView).

    /// <summary>
    /// Порядок строк определяется текущей выбранной сортировкой. 
    /// Для <see cref="EFPDocSelTextGridView"/> используется сортировка по текстовому представлению.
    /// </summary>
    Natural,
  }

  /// <summary>
  /// Интерфейс просмотра выборки документов.
  /// Дополняет просмотр командами добавления элементов.
  /// Пока что интерфейс реализуется только <see cref="EFPDocSelTextGridView"/>.
  /// </summary>
  public interface IEFPDocSelView : IEFPDBxView
  {
    /// <summary>
    /// Интерфейс вида документов, отображаемых в выборке
    /// </summary>
    DocTypeUI DocTypeUI { get; }

    /// <summary>
    /// Идентификаторы документов, существующих в таблице
    /// </summary>
    Int32[] Ids { get; set;}

    /// <summary>
    /// Добавляет одну строку в таблицу.
    /// Если уже есть строка с таким идентификатором, никаких действий не выполняется.
    /// </summary>
    /// <param name="id">Идентификатор добавляемого документа</param>
    /// <returns>true, если строка была добавлена</returns>
    bool AddId(Int32 id);

    /// <summary>
    /// Добавляет несколько строк в таблицу.
    /// Для идентификаторов, которые уже есть в выборке, никаких действий не выполняется.
    /// </summary>
    /// <param name="ids">Идентификаторы добавляемых документов</param>
    /// <returns>Количество документов, которые добавлены</returns>
    int AddIds(Int32[] ids);

    /// <summary>
    /// Может ли в в списке не быть ни одного документа?
    /// </summary>
    bool CanBeEmpty { get;set;}
  }

  /// <summary>
  /// Провайдер табличного просмотра выборки документа.
  /// Отображаются документы только одного вида.
  /// Для документа отображается только значок и текстовое представление.
  /// Свойство <see cref="EFPDocSelTextGridView.Ids"/> используется для получения и установки списка строк.
  /// Порядок строк может иметь или не иметь значение, в зависимости от свойства <see cref="EFPDocSelTextGridView.OrderMode"/>.
  /// </summary>
  public class EFPDocSelTextGridView : EFPDBxGridView, IEFPDocSelView
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент табличного просмотра</param>
    /// <param name="docTypeUI">Интерфейс вида документов</param>
    public EFPDocSelTextGridView(EFPBaseProvider baseProvider, DataGridView control, DocTypeUI docTypeUI)
      : base(baseProvider, control, docTypeUI.UI)
    {
      Init(docTypeUI);
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    /// <param name="docTypeUI">Интерфейс вида документов</param>
    public EFPDocSelTextGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar, DocTypeUI docTypeUI)
      : base(controlWithToolBar, docTypeUI.UI)
    {
      Init(docTypeUI);
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент табличного просмотра</param>
    /// <param name="ui">Интерфейс для доступа к документам</param>
    /// <param name="docTypeName">Имя вида документов в просмотре</param>
    public EFPDocSelTextGridView(EFPBaseProvider baseProvider, DataGridView control, DBUI ui, string docTypeName)
      : base(baseProvider, control, ui)
    {
      Init(ui.DocTypes[docTypeName]);
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    /// <param name="ui">Интерфейс для доступа к документам</param>
    /// <param name="DocTypeName">Имя вида документов в просмотре</param>
    public EFPDocSelTextGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar, DBUI ui, string DocTypeName)
      : base(controlWithToolBar, ui)
    {
      Init(ui.DocTypes[DocTypeName]);
    }

    private void Init(DocTypeUI docTypeUI)
    {
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");

      _DocTypeUI = docTypeUI;
      _OrderMode = EFPDocSelGridViewOrderMode.Fixed;

      _Table = new DataTable();
      _Table.Columns.Add("Id", typeof(Int32));
      _Table.Columns.Add("Text", typeof(string));
      _Table.Columns.Add("Order", typeof(int)); // используется в режиме сортировки Manual и Fixed
      DataTools.SetPrimaryKey(_Table, "Id");

      Control.AutoGenerateColumns = false;
      if (EFPApp.ShowListImages) // 14.09.2016
      {
        Columns.AddImage();
        GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(GridHandler_GetCellAttributes);
      }
      Columns.AddTextFill("Text", true, DocType.SingularTitle, 100, 15);
      Columns.LastAdded.CanIncSearch = true;
      if (UI.DebugShowIds)
      {
        Columns.AddInt("Id", true, "Id", 5);
        Columns.LastAdded.CanIncSearch = true;
      }
      CurrentColumnIndex = 1;
      DisableOrdering();
      ReadOnly = false;
      CanInsertCopy = false;
      CanView = true;
      CanMultiEdit = false; // DocTypeUI.CanMultiEdit;
      Control.MultiSelect = true;
      Control.ReadOnly = true;
      CommandItems.Cut += new EventHandler(GridHandler_Cut);

      DBxDocSelectionPasteFormat fmtDocSel = new DBxDocSelectionPasteFormat(UI, DocTypeName);
      fmtDocSel.Paste += new EFPPasteDataObjectEventHandler(fmtDocSel_Paste);
      CommandItems.PasteHandler.Add(fmtDocSel);
      CommandItems.ClipboardInToolBar = true;

      _CanBeEmpty = true;

      _DocTypeUI.CallInitDocSelView(this);
    }

    #endregion

    #region Свойство Ids

    /// <summary>
    /// Таблица, содержащая поля "Id", "Text" и "Order"
    /// </summary>
    private DataTable _Table;

    /// <summary>
    /// Идентификаторы документов, существующих в таблице
    /// </summary>
    public Int32[] Ids
    {
      get
      {
        return DataTools.GetIds(_Table.DefaultView);
      }
      set
      {
        if (value == null)
          value = DataTools.EmptyIds;

        _Table.BeginLoadData();
        try
        {
          _Table.Rows.Clear();
          for (int i = 0; i < value.Length; i++)
          {
            UI.DocProvider.CheckIsRealDocId(value[i]);
            _Table.Rows.Add(value[i], DocTypeUI.GetTextValue(value[i]), i + 1);
          }
        }
        finally
        {
          _Table.EndLoadData();
        }
        Validate(); // 08.07.2019
      }
    }

    /// <summary>
    /// Добавляет одну строку в таблицу.
    /// Если уже есть строка с таким идентификатором, никаких действий не выполняется.
    /// </summary>
    /// <param name="id">Идентификатор добавляемого документа</param>
    /// <returns>true, если строка была добавлена</returns>
    public bool AddId(Int32 id)
    {
      if (id == 0)
        return false;
      if (_Table.Rows.Find(id) != null)
        return false; // уже есть
      DataRow NewRow = _Table.NewRow();
      NewRow["Id"] = id;
      NewRow["Text"] = DocTypeUI.GetTextValue(id);
      NewRow["Order"] = (DataTools.MaxInt(_Table, "Order", false) ?? 0) + 1;
      _Table.Rows.Add(NewRow);
      Validate(); // 08.07.2019
      return true;
    }

    /// <summary>
    /// Добавляет несколько строк в таблицу.
    /// Для идентификаторов, которые уже есть в выборке, никаких действий не выполняется.
    /// </summary>
    /// <param name="ids">Идентификаторы добавляемых документов</param>
    /// <returns>Количество документов, которые добавлены</returns>
    public int AddIds(Int32[] ids)
    {
      int cnt = 0;
      for (int i = 0; i < ids.Length; i++)
      {
        if (AddId(ids[i]))
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region Свойство CanBeEmpty

    /// <summary>
    /// Может ли в в списке не быть ни одного документа?
    /// По умолчанию - true.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set
      {
        if (value == _CanBeEmpty)
          return;
        _CanBeEmpty = value;
        Validate(); // 08.07.2019
      }
    }
    private bool _CanBeEmpty;

    #endregion

    #region Прочие свойства

    /// <summary>
    /// Интерфейс для работы с документами заданного вида.
    /// Свойства <see cref="DocTypeUI"/>, <see cref="DocTypeName"/> и <see cref="DocType"/> возвращают связанные значения.
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private DocTypeUI _DocTypeUI;

    /// <summary>
    /// Имя вида документа.
    /// Свойства <see cref="DocTypeUI"/>, <see cref="DocTypeName"/> и <see cref="DocType"/> возвращают связанные значения.
    /// </summary>
    public string DocTypeName { get { return _DocTypeUI.DocType.Name; } }

    /// <summary>
    /// Описание документа в базе данных.
    /// Свойства <see cref="DocTypeUI"/>, <see cref="DocTypeName"/> и <see cref="DocType"/> возвращают связанные значения.
    /// </summary>
    public DBxDocType DocType { get { return _DocTypeUI.DocType; } }

    /// <summary>
    /// Порядок сортировки строки.
    /// По умолчанию используется режим <see cref="EFPDocSelGridViewOrderMode.Fixed"/>.
    /// Свойство может устанавливаться только до вывода просмотра на экран.
    /// </summary>
    public EFPDocSelGridViewOrderMode OrderMode
    {
      get { return _OrderMode; }
      set
      {
        switch (value)
        {
          case EFPDocSelGridViewOrderMode.Fixed:
          case EFPDocSelGridViewOrderMode.Manual:
          case EFPDocSelGridViewOrderMode.Natural:
            break;
          default:
            throw new ArgumentException();
        }
        CheckHasNotBeenCreated();
        _OrderMode = value;
      }
    }
    private EFPDocSelGridViewOrderMode _OrderMode;

    /// <summary>
    /// Фильтры не влияют непосредственно на строки, существующие в просмотре.
    /// Фильтры используются при добавлении новых строк.
    /// Также выполняется проверка прохождения строк просмотра в методе <see cref="OnValidate()"/> при закрытии просмотра.
    /// При установке свойства обычно следует запретить редактирование фильтров пользователем, установив свойство <see cref="EFPDBxGridView.CommandItems"/>.CanEditFilters=false.
    /// </summary>
    public new EFPDBxGridFilters Filters
    {
      get { return base.Filters; }
      set
      {
        base.Filters = value;
        Validate(); // 08.07.2019
      }
    }

    #endregion

    #region OnShown и OnHidden

    /// <summary>
    /// Вызывается при первом показе формы на экране.
    /// Обрабатывает свойство <see cref="OrderMode"/>.
    /// </summary>
    protected override void OnCreated()
    {
      switch (OrderMode)
      {
        case EFPDocSelGridViewOrderMode.Fixed:
          _Table.DefaultView.Sort = "Order";
          break;

        case EFPDocSelGridViewOrderMode.Natural:
          _Table.DefaultView.Sort = "Text";
          break;

        case EFPDocSelGridViewOrderMode.Manual:
          _Table.DefaultView.Sort = "Order";
          ManualOrderColumn = "Order";
          break;
      }

      Control.DataSource = _Table.DefaultView;

      base.OnCreated();

    }

    /// <summary>
    /// Вызывается при показе таблицы.
    /// Создает объект, производный от <see cref="DocumentViewHandler"/>.
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();
      _ViewHandler = new IntDocumentViewHandler();
      _ViewHandler.Owner = this;
      _ViewHandler.ExternalEditorCaller = ExternalEditorCaller;
      DocTypeUI.Browsers.Add(_ViewHandler);
    }

    /// <summary>
    /// Вызывается, когда форма с управляющим элементом закрывается.
    /// </summary>
    protected override void OnDetached()
    {
      if (_ViewHandler != null)
      {
        _ViewHandler.Owner = null; // разрыв ссылки, чтобы текущий просмотр мог быть удален
        DocTypeUI.Browsers.Remove(_ViewHandler);
        _ViewHandler = null;
      }
      base.OnDetached();
    }

    #endregion

    #region Оформление просмотра

    void GridHandler_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.ColumnIndex != 0)
        return;
      if (args.DataRow == null)
        return;
      Int32 Id = DataTools.GetInt(args.DataRow, "Id");
      args.Value = DocTypeUI.GetImageValue(Id);
    }

    #endregion

    #region Редактирование

    /// <summary>
    /// Выполняет редактирование.
    /// </summary>
    /// <param name="args">Игнорируется</param>
    /// <returns>Всегда true, т.к. редактирование по месту выполняться не должно</returns>
    protected override bool OnEditData(EventArgs args)
    {
      //base.OnEditData(Args);

      Int32 id;
      switch (State)
      {
        case UIDataState.Insert:
          Int32[] selIds = DocTypeUI.SelectDocs(String.Format(Res.EFPDataView_Title_AddDocs,
            DocType.PluralTitle), this.Filters);
          if (selIds.Length == 0)
            return true;
          for (int i = 0; i < selIds.Length; i++)
            AddId(selIds[i]);
          SelectedIds = selIds;
          break;
        case UIDataState.Delete:
          DataRow[] selRows = SelectedDataRows;
          for (int i = 0; i < selRows.Length; i++)
            selRows[i].Delete();
          Validate(); // 08.07.2019
          break;
        case UIDataState.View:
          DocTypeUI.PerformEditing(CurrentId, true);
          break;
        case UIDataState.Edit:
          id = CurrentId;
          if (DocTypeUI.SelectDoc(ref id, Res.EFPDataView_Title_ChangeDocRef, false, this.Filters))
          {
            CurrentDataRow.Delete();
            AddId(id);
            CurrentId = id;
            Validate(); // 08.07.2019
          }
          break;
        default:
          EFPApp.ShowTempMessage("Unknown edit mode");
          break;
      }
      return true;
    }

    /// <summary>
    /// Внешний инициализатор для новых документов.
    /// Если свойство установлено, то при создании нового документа в качестве
    /// инициализатора значений полей (аргумент caller при вызове <see cref="FreeLibSet.Forms.Docs.DocTypeUI.PerformEditing(int[], UIDataState, bool, DocumentViewHandler)"/>) 
    /// будет использован этот инициализатор вместо текущих фильтров (<see cref="EFPDBxGridView.Filters"/>.
    /// Свойство может устанавливаться только до вывода просмотра на экран.
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

    #endregion

    #region Выборка документов

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool HasGetDocSelHandler { get { return true; } }

    /// <summary>
    /// Вызывает <see cref="FreeLibSet.Forms.Docs.DocTypeUI.PerformGetDocSel(DBxDocSelection, int[], EFPDBxViewDocSelReason)"/> для идентификаторов документов в просмотре.
    /// Затем вызывает обработчик события <see cref="EFPDBxGridView.GetDocSel"/>, если он установлен.
    /// </summary>
    /// <param name="args">Аргументы события <see cref="EFPDBxGridView.GetDocSel"/></param>
    protected override void OnGetDocSel(EFPDBxGridViewDocSelEventArgs args)
    {
      Int32[] ids = DataTools.GetIds(args.DataRows);
      DocTypeUI.PerformGetDocSel(args.DocSel, ids, args.Reason);

      base.OnGetDocSel(args); // если есть пользовательский обработчик
    }

    #endregion

    #region Буфер обмена

    void GridHandler_Cut(object sender, EventArgs args)
    {
      if (CommandItems.PerformCopy())
      {
        DataRow[] selRows = SelectedDataRows;
        for (int i = 0; i < selRows.Length; i++)
          selRows[i].Delete();
        Validate(); // 08.07.2019
      }
    }

    void fmtDocSel_Paste(object sender, EFPPasteDataObjectEventArgs args)
    {
      DBxDocSelectionPasteFormat fmtDocSel = (DBxDocSelectionPasteFormat)sender;
      Int32[] ids = fmtDocSel.DocSel[DocTypeName];

      int cnt = 0;
      for (int i = 0; i < ids.Length; i++)
      {
        if (AddId(ids[i]))
          cnt++;
      }
      if (cnt == 0)
        EFPApp.ShowTempMessage(String.Format(Res.EFPDataView_Err_PasteNoNewDocs,
          DocType.PluralTitle, ids.Length));
    }

    #endregion

    #region Реализация DocumentViewHandler

    private class IntDocumentViewHandler : DocumentViewHandler
    {
      #region Свойства

      public EFPDocSelTextGridView Owner;

      public DocumentViewHandler ExternalEditorCaller;

      #endregion

      #region Переопределенные методы и свойства

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

#pragma warning disable 0219 // TODO: Убрать лишние переменные

      /// <summary>
      /// Обновление табличного просмотра
      /// </summary>
      /// <param name="dataSet"></param>
      /// <param name="isCaller"></param>
      public override void ApplyChanges(DataSet dataSet, bool isCaller)
      {
        if (Owner == null)
          return;

        if (!dataSet.Tables.Contains(Owner.DocType.Name))
          return; // Нет таблицы

        DataTable srcTable = dataSet.Tables[Owner.DocType.Name];
        DataRow resRow;
        Int32 docId;
        bool changed = false;

        foreach (DataRow srcRow in srcTable.Rows)
        {
          switch (srcRow.RowState)
          {
            case DataRowState.Modified:
            case DataRowState.Deleted:
              docId = (Int32)(srcRow["Id"]);
              resRow = Owner._Table.Rows.Find(docId);
              if (resRow == null)
                continue;
              Owner.InvalidateDataRow(resRow); // не Update
              changed = true;
              break;
          }
        }
      }

#pragma warning restore 0219

      public override void UpdateRowsForIds(Int32[] docIds)
      {
        if (Owner != null)
          Owner.UpdateRowsForIds(docIds);
      }

      public override void InitNewDocValues(DBxSingleDoc newDoc)
      {
        if (ExternalEditorCaller == null)
        {
          if (Owner != null)
          {
            if (Owner.Filters != null)
              Owner.Filters.InitNewValues(newDoc.Values);
          }
        }
        else
          ExternalEditorCaller.InitNewDocValues(newDoc);
      }

      public override void ValidateDocValues(DBxSingleDoc savingDoc, ErrorMessageList errorMessages)
      {
        if (ExternalEditorCaller == null)
        {
          if (Owner != null)
          {
            if (Owner.Filters != null)
              Owner.Filters.ValidateValues(savingDoc.Values, errorMessages);
          }
        }
        else
          ExternalEditorCaller.ValidateDocValues(savingDoc, errorMessages);
      }

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
          return "Disconnected";
        else
          return Owner.ToString();
      }

      #endregion
    }

    /// <summary>
    /// Обработчик просмотра документов, связанный с текущим просмотром.
    /// Свойство имеет значение не null, когда просмотр выведен на экран.
    /// </summary>
    public DocumentViewHandler ViewHandler { get { return _ViewHandler; } }
    private IntDocumentViewHandler _ViewHandler;

    /// <summary>
    /// Идентификатор просмотра.
    /// Используется <see cref="DocumentViewHandler"/>.
    /// </summary>
    public Guid BrowserGuid
    {
      get { return _BrowserGuid; }
      set { _BrowserGuid = value; }
    }
    private Guid _BrowserGuid;

    #endregion

    #region Проверка

    /// <summary>
    /// Проверяет корректность данных.
    /// Если выполняется закрытие формы, свойство <see cref="CanBeEmpty"/>=false и нет ни одного документа в просмотре, устанавливается признак ошибки.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      //if (BaseProvider.FormProvider.ValidateReason == EFPFormValidateReason.Closing)
      //{
      // 09.07.2019 - Проверяем всегда
      if ((!CanBeEmpty) && _Table.DefaultView.Count == 0)
        base.SetError(Res.Common_ToolTipText_NoDoc);
      //}
      if (BaseProvider.FormProvider.ValidateReason == EFPFormValidateReason.Closing)
        ValidateFilters();
    }

    private void ValidateFilters()
    {
      if (Filters == null)
        return;
      if (Filters.IsEmpty)
        return;

      IdList badIds;
      EFPApp.BeginWait(Res.Common_Phase_ValidateSelDocs, "Filter");
      try
      {
        // Список полей для фильтра
        DBxColumnList colList = new DBxColumnList();
        colList.Add("Id"); // нужен нам для поиска
        Filters.GetColumnNames(colList);
        // Загружаем значения
        DataTable table2 = DocTypeUI.TableCache.CreateTable(this.Ids, new DBxColumns(colList));
        DataTableValueArray va = new DataTableValueArray(table2);

        // Проверяем попадание
        badIds = new IdList();
        foreach (DataRow row in table2.Rows)
        {
          va.CurrentRow = row;
          DBxCommonFilter BadFilter;
          if (!Filters.TestValues(va, out BadFilter))
            badIds.Add(DataTools.GetInt(row, "Id"));
        }
      }
      finally
      {
        EFPApp.EndWait();
      }

      if (badIds.Count > 0)
      {
        this.SelectedIds = badIds.ToArray();
        SetError(String.Format(Res.EFPDataView_Err_DocsMismatchFilters,
          badIds.Count, Control.RowCount));
      }
    }

    #endregion
  }
}
