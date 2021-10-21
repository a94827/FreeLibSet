using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Возможные значения свойства OrderMode в EFPDocSelGridView и EFPDocSelTextGridView
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
    /// Пользователь может менять порядок строк (ручная сортировка)
    /// </summary>
    Manual,

    /// <summary>
    /// Порядок строк определяется текущей выбранной сортировкой. 
    /// Пользователь может выбирать порядок сортировки (только для EFPDocSelGridView).
    /// Для EFPDocSelTextGridView используется сортировка по текстовому представлению
    /// </summary>
    Natural,
  }

  /// <summary>
  /// Интерфейс просмотра выборки документов.
  /// Дополняет просмотр командами добавления элементов.
  /// Пока что интерфейс реализуется только EFPDocSelTextGridView.
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
    /// <returns>Количество учреждений, которые добавлены</returns>
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
  /// Свойство Ids используется для получения и установки списка строк.
  /// Порядок строк может иметь или не иметь значение, в зависимости от свойства OrderMode.
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
    public EFPDocSelTextGridView(EFPControlWithToolBar<DataGridView> controlWithToolBar, DocTypeUI docTypeUI)
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
    public EFPDocSelTextGridView(EFPControlWithToolBar<DataGridView> controlWithToolBar, DBUI ui, string DocTypeName)
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
    /// <returns>Количество учреждений, которые добавлены</returns>
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
    /// Интерфейс для работы с документами заданного вида
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private DocTypeUI _DocTypeUI;

    /// <summary>
    /// Имя вида документа
    /// </summary>
    public string DocTypeName { get { return _DocTypeUI.DocType.Name; } }

    /// <summary>
    /// Описание документа в базе данных
    /// </summary>
    public DBxDocType DocType { get { return _DocTypeUI.DocType; } }

    /// <summary>
    /// Порядок сортировки строки.
    /// По умолчанию используется режим Fixed.
    /// Свойство может устанавливаться только до вывода просмотра на экран
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
    /// Также выполняется проверка прохождения строк просмотра в методе OnValidate() при закрытии просмотра.
    /// При установке свойства обычно следует запретить редактирование фильтров пользователем, установив свойство CommandItems.CanEditFilters=false
    /// </summary>
    public new GridFilters Filters
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
    /// Вызывается при первом показе формы на экране
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
          CommandItems.ManualOrderColumn = "Order";
          break;
      }

      Control.DataSource = _Table.DefaultView;

      base.OnCreated();

    }

    /// <summary>
    /// Вызывается при показе таблицы.
    /// Создает объект DocumentViewhandler
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

      Int32 Id;
      switch (State)
      {
        case EFPDataGridViewState.Insert:
          Int32[] SelIds = DocTypeUI.SelectDocs("Добавление документов \"" + DocType.PluralTitle + "\" в просмотр", this.Filters);
          if (SelIds.Length == 0)
            return true;
          for (int i = 0; i < SelIds.Length; i++)
            AddId(SelIds[i]);
          SelectedIds = SelIds;
          break;
        case EFPDataGridViewState.Delete:
          DataRow[] SelRows = SelectedDataRows;
          for (int i = 0; i < SelRows.Length; i++)
            SelRows[i].Delete();
          Validate(); // 08.07.2019
          break;
        case EFPDataGridViewState.View:
          DocTypeUI.PerformEditing(CurrentId, true);
          break;
        case EFPDataGridViewState.Edit:
          Id = CurrentId;
          if (DocTypeUI.SelectDoc(ref Id, "Изменение ссылки на документ", false, this.Filters))
          {
            CurrentDataRow.Delete();
            AddId(Id);
            CurrentId = Id;
            Validate(); // 08.07.2019
          }
          break;
        default:
          EFPApp.ShowTempMessage("Неизвестный режим редактирования");
          break;
      }
      return true;
    }

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

    #endregion

    #region Выборка документов

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool HasGetDocSelHandler { get { return true; } }

    /// <summary>
    /// Вызывает DocTypeUI.PerformgetDocSel() для идентификаторов документов в просмотре.
    /// Затем вызывает обработчик события GetDocSel, если он установлен.
    /// </summary>
    /// <param name="args">Аргументы события GetDocSel</param>
    protected override void OnGetDocSel(EFPDBxGridViewDocSelEventArgs args)
    {
      Int32[] Ids = DataTools.GetIds(args.DataRows);
      DocTypeUI.PerformGetDocSel(args.DocSel, Ids, args.Reason);

      base.OnGetDocSel(args); // если есть пользовательский обработчик
    }

    #endregion

    #region Буфер обмена

    void GridHandler_Cut(object sender, EventArgs args)
    {
      if (CommandItems.PerformCopy())
      {
        DataRow[] SelRows = SelectedDataRows;
        for (int i = 0; i < SelRows.Length; i++)
          SelRows[i].Delete();
        Validate(); // 08.07.2019
      }
    }

    void fmtDocSel_Paste(object sender, EFPPasteDataObjectEventArgs args)
    {
      DBxDocSelectionPasteFormat fmtDocSel = (DBxDocSelectionPasteFormat)sender;
      Int32[] Ids = fmtDocSel.DocSel[DocTypeName];

      int cnt = 0;
      for (int i = 0; i < Ids.Length; i++)
      {
        if (AddId(Ids[i]))
          cnt++;
      }
      if (cnt == 0)
        EFPApp.ShowTempMessage("Все документы \"" + DocType.PluralTitle + "\" в буфере обмена (" + Ids.Length.ToString() + " шт.) уже есть в списке");
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

        DataTable SrcTable = dataSet.Tables[Owner.DocType.Name];
        DataRow ResRow;
        Int32 DocId;
        bool Changed = false;

        foreach (DataRow SrcRow in SrcTable.Rows)
        {
          switch (SrcRow.RowState)
          {
            case DataRowState.Modified:
            case DataRowState.Deleted:
              DocId = (Int32)(SrcRow["Id"]);
              ResRow = Owner._Table.Rows.Find(DocId);
              if (ResRow == null)
                continue;
              Owner.InvalidateDataRow(ResRow); // не Update
              Changed = true;
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
              Owner.Filters.InitNewDocValues(newDoc);
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
              Owner.Filters.ValidateDocValues(savingDoc, errorMessages);
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
    private IntDocumentViewHandler _ViewHandler;

    /// <summary>
    /// Идентификатор просмотра.
    /// Используется DocumentViewHandler
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
    /// Если выполняется закрытие формы, свойство CanBeEmpty=false и нет ни одного документа в просмотре, устанавливается признак ошибки
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      //if (BaseProvider.FormProvider.ValidateReason == EFPFormValidateReason.Closing)
      //{
      // 09.07.2019 - Проверяем всегда
      if ((!CanBeEmpty) && _Table.DefaultView.Count == 0)
        base.SetError("Не выбрано ни одного документа");
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

      IdList BadIds;
      EFPApp.BeginWait("Проверка выбранных документов", "Filter");
      try
      {
        // Список полей для фильтра
        DBxColumnList ColList = new DBxColumnList();
        ColList.Add("Id"); // нужен нам для поиска
        Filters.GetColumnNames(ColList);
        // Загружаем значения
        DataTable Table2 = DocTypeUI.TableCache.CreateTable(this.Ids, new DBxColumns(ColList));
        DataTableValueArray va = new DataTableValueArray(Table2);

        // Проверяем попадание
        BadIds = new IdList();
        foreach (DataRow Row in Table2.Rows)
        {
          va.CurrentRow = Row;
          DBxCommonFilter BadFilter;
          if (!Filters.TestValues(va, out BadFilter))
            BadIds.Add(DataTools.GetInt(Row, "Id"));
        }
      }
      finally
      {
        EFPApp.EndWait();
      }

      if (BadIds.Count > 0)
      {
        this.SelectedIds = BadIds.ToArray();
        SetError("Выбранные документы (" + BadIds.Count.ToString() + " из " + Control.RowCount.ToString() + ") не проходят условие фильтра");
      }
    }

    #endregion
  }
}
