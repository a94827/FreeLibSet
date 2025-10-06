// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.ComponentModel;
using FreeLibSet.Core;
using FreeLibSet.Text;
using FreeLibSet.UICore;
using FreeLibSet.Forms.Reporting;
using FreeLibSet.Reporting;

namespace FreeLibSet.Forms
{
  #region DataObjectEventHandler

  /// <summary>
  /// Аргументы события <see cref="EFPDataGridViewCommandItems.AddCopyFormats"/>
  /// </summary>
  public class DataObjectEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Инициализация объекта
    /// </summary>
    /// <param name="dataObject">Интерфейс объекта данных</param>
    public DataObjectEventArgs(IDataObject dataObject)
    {
      _DataObject = dataObject;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс для работы со списком форматов данных
    /// </summary>
    public IDataObject DataObject { get { return _DataObject; } }
    private readonly IDataObject _DataObject;

    #endregion
  }

  /// <summary>
  /// Аргументы события <see cref="EFPDataGridViewCommandItems.AddCopyFormats"/>
  /// </summary>
  /// <param name="sender">Ссылка на объект, содержащий событие</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DataObjectEventHandler(object sender, DataObjectEventArgs args);

  #endregion

  #region Перечисление EFPDataViewCopyFormats

  /// <summary>
  /// Стандартные форматы копирования табличного просмотра в буфер обмена (Text, CSV и HTML)
  /// </summary>
  [Flags]
  public enum EFPDataViewCopyFormats
  {
    /// <summary>
    /// Текстовый формат.
    /// Столбцы отделяются друг от друга знаком табуляции
    /// </summary>
    Text = 0x01,

    /// <summary>
    /// CSV-формат.
    /// Столбцы отделяются друг от друга знаком ";". Значения заключаются в кавычки по необходимости
    /// </summary>
    CSV = 0x02,

    /// <summary>
    /// Формат HTML
    /// </summary>
    HTML = 0x04,

    /// <summary>
    /// Нет данных для копирования
    /// </summary>
    None = 0,

    /// <summary>
    /// Объединение Text, CSV и HTML
    /// </summary>
    All = Text | CSV | HTML
  }

  #endregion

  #region DataReorderHelperNeededEventHandler

  /// <summary>
  /// Аргументы события <see cref="EFPDataGridView.DataReorderHelperNeeded"/> и <see cref="EFPDataTreeView.DataReorderHelperNeeded"/>
  /// </summary>
  public class DataReorderHelperNeededEventArgs : EventArgs
  {
    #region Свойства

    /// <summary>
    /// Сюда обработчик события может поместить новый экземпляр класса, реализующего интерфейс <see cref="IDataReorderHelper"/>,
    /// например, <see cref="DataTableTreeReorderHelper"/>.
    /// Если останется значение по умолчанию null, то будет создан стандартный объект, обычно <see cref="DataTableReorderHelper"/>.
    /// </summary>
    public IDataReorderHelper Helper { get { return _Helper; } set { _Helper = value; } }
    private IDataReorderHelper _Helper;

    #endregion
  }

  /// <summary>
  /// Делегат события события <see cref="EFPDataGridView.DataReorderHelperNeeded"/> и <see cref="EFPDataTreeView.DataReorderHelperNeeded"/>
  /// </summary>
  /// <param name="sender">Провайдер управляющего элемента просмотра</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DataReorderHelperNeededEventHandler(object sender, DataReorderHelperNeededEventArgs args);

  #endregion

  #region Перечисление EFPDataViewCopyHyperlinkCommandUsage

  /// <summary>
  /// Доступность команды "Копировать гиперссылку" в табличном просмотре
  /// </summary>
  public enum EFPDataViewCopyHyperlinkCommandUsage
  {
    /// <summary>
    /// Определяется автоматически по факту наличия столбцов со ссылками
    /// </summary>
    Auto,

    /// <summary>
    /// Команда видима всегда
    /// </summary>
    AlwaysVisible,

    /// <summary>
    /// Команда не используется
    /// </summary>
    Unused
  }

  #endregion

  #region Перечисление EFPDataViewEnterKeyMode

  /// <summary>
  /// Какое действие выполняет нажатие клавиши "Enter" и двойной щелчок мыши в табличном просмотре
  /// </summary>
  public enum EFPDataViewEnterKeyMode
  {
    /// <summary>
    /// Если <see cref="IEFPDataView.ReadOnly"/>=false, то Enter или Ctrl+Enter - редактирование записи, Shift+Enter - просмотр записи
    /// Если <see cref="IEFPDataView.ReadOnly"/>=true, то Enter или Shift+Enter - просмотр записи, Ctrl+Enter - редактирование
    /// Этот режим используется по умолчанию.
    /// </summary>
    EditOrView,

    /// <summary>
    /// Enter или Ctrl+Enter - Выполняется редактирование. Shift+Enter - просмотр.
    /// </summary>
    Edit,

    /// <summary>
    /// Enter или Shift+Enter - выполняется просмотр. Ctrl+Enter - редактирование
    /// </summary>
    View,

    /// <summary>
    /// Нажимается кнопка по умолчанию для блока диалога <see cref="Form.AcceptButton"/> или назначенная свойством <see cref="IEFPControl.DefaultButton"/>.
    /// Ctrl+Enter - редактирование. Просмотр запускается комбинацией клавиш Shift+Enter.
    /// </summary>
    DefaultButton,

    /// <summary>
    /// Комбинации клавиш для редактирования и просмотра не назначаются. Допускается возможность их установки из прикладного кода
    /// </summary>
    None,
  }

  #endregion

  /// <summary>
  /// Список команд локального меню табличного просмотра
  /// </summary>
  public class EFPDataGridViewCommandItems : EFPControlCommandItems, IEFPClipboardCommandItems, IEFPDataViewClipboardCommandItems
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Создает список команд
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра</param>
    public EFPDataGridViewCommandItems(EFPDataGridView controlProvider)
      : base(controlProvider)
    {
      #region Начальные значения свойств

      _EnterKeyMode = EFPDataViewEnterKeyMode.EditOrView;
      _UseEditView = true;
      _UseRefresh = true;
      _UseSelectAll = true;
      _UseRowErrors = true;
      _GotoErrorEnabled = true;
      _UseRowErrorsListView = true;
      _CopyFormats = EFPDataViewCopyFormats.All;
      _CopyHyperlinkUsage = EFPDataViewCopyHyperlinkCommandUsage.Auto;
      _UseGotoRowWithDifferentCellText = true;

      #endregion

      #region Создание команд

      #region Редактирование

      MenuEdit = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.MenuEdit);
      Add(MenuEdit);

      ciEdit = new EFPCommandItem("Edit", "Edit");
      ciEdit.Parent = MenuEdit;
      ciEdit.MenuText = Res.Cmd_Menu_Edit_Edit;
      ciEdit.ImageKey = "Edit";
      ciEdit.GroupBegin = true;
      ciEdit.Click += new EventHandler(ciEdit_Click);
      Add(ciEdit);

      ciInsert = new EFPCommandItem("Edit", "Insert");
      ciInsert.Parent = MenuEdit;
      ciInsert.MenuText = Res.Cmd_Menu_Edit_Insert;
      ciInsert.ImageKey = "Insert";
      ciInsert.ShortCut = Keys.Insert;
      ciInsert.Click += new EventHandler(ciInsert_Click);
      Add(ciInsert);

      ciInsertCopy = new EFPCommandItem("Edit", "InsertCopy");
      ciInsertCopy.Parent = MenuEdit;
      ciInsertCopy.MenuText = Res.Cmd_Menu_Edit_InsertCopy;
      ciInsertCopy.ImageKey = "InsertCopy";
      ciInsertCopy.ToolTipText = Res.Cmd_Menu_ToolTip_InsertCopy;
      ciInsertCopy.ShortCut = Keys.Insert | Keys.Alt;
      ciInsertCopy.Click += new EventHandler(ciInsertCopy_Click);
      Add(ciInsertCopy);

      ciDelete = new EFPCommandItem("Edit", "Delete");
      ciDelete.Parent = MenuEdit;
      ciDelete.MenuText = Res.Cmd_Menu_Edit_Delete;
      ciDelete.ImageKey = "Delete";
      ciDelete.ShortCut = Keys.Delete;
      ciDelete.Click += new EventHandler(ciDelete_Click);
      Add(ciDelete);

      ciView = new EFPCommandItem("Edit", "View");
      ciView.Parent = MenuEdit;
      ciView.MenuText = Res.Cmd_Menu_Edit_View;
      ciView.ImageKey = "View";
      ciView.GroupEnd = true;
      ciView.Click += new EventHandler(ciView_Click);
      Add(ciView);

      ciOkButton = new EFPCommandItem("Edit", "OKButton");
      //ciOkButton.Parent = MenuEdit;
      ciOkButton.Usage = EFPCommandItemUsage.ShortCut;
      ciOkButton.Click += ciOkButton_Click;
      Add(ciOkButton);

      if (!EFPApp.EasyInterface)
      {
        ciInlineEditStatus = new EFPCommandItem("Edit", "EditInline");
        ciInlineEditStatus.Parent = MenuEdit;
        ciInlineEditStatus.MenuText = Res.Cmd_Menu_Edit_EditInline;
        ciInlineEditStatus.ImageKey = "EmptyImage";
        ciInlineEditStatus.StatusBarText = EFPCommandItem.EmptyStatusBarText;
        ciInlineEditStatus.Click += new EventHandler(ciInlineEditStatus_Click);
        ciInlineEditStatus.Usage = EFPCommandItemUsage.StatusBar;
        Add(ciInlineEditStatus);
      }

      #endregion

      #region Буфер обмена

      MenuClipboard = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.MenuClipboard);
      Add(MenuClipboard);

      ciCut = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Cut);
      ciCut.Parent = MenuClipboard;
      ciCut.Click += new EventHandler(DoCut);
      ciCut.GroupBegin = true;
      Add(ciCut);

      ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
      ciCopy.Parent = MenuClipboard;
      ciCopy.Enabled = true;
      ciCopy.Click += new EventHandler(DoCopy);
      Add(ciCopy);

      ciCopySettings = EFPDataViewCopyFormatsForm.AddCommandItem(this);
      ciCopySettings.Parent = MenuClipboard;

      ciCopyHyperlink = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.CopyHyperlink);
      ciCopyHyperlink.Parent = MenuClipboard;
      ciCopyHyperlink.Click += CopyHyperlink_Click;
      Add(ciCopyHyperlink);

      if (EFPApp.ShowToolTips)
      {
        ciCopyToolTip = new EFPCommandItem("Edit", "CopyToolTip");
        ciCopyToolTip.Parent = MenuClipboard;
        ciCopyToolTip.MenuText = Res.Cmd_Menu_Edit_CopyToolTip;
        ciCopyToolTip.ImageKey = "CopyToolTip";
        ciCopyToolTip.Click += new EventHandler(CopyToolTip_Click);
        Add(ciCopyToolTip);
      }
      MenuClipboard.Children.AddSeparator();
      _PasteHandler = new EFPPasteHandler(this, MenuClipboard);
      _PasteHandler.UseToolBar = false; // по умолчанию кнопок нет в буфере обмена

      #endregion

      #region Выделить все

      ciSelectAll = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SelectAll);
      ciSelectAll.Click += new EventHandler(SelectAll);
      Add(ciSelectAll);

      #endregion

      #region Обновить

      ciRefresh = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Refresh);
      ciRefresh.Click += new EventHandler(Refresh);
      Add(ciRefresh);

      #endregion

      #region Установка отметок

      MenuCheck = new EFPCommandItem("Edit", "MenuCheckMarks");
      MenuCheck.MenuText = Res.Cmd_Menu_CheckMarks;
      MenuCheck.ImageKey = "CheckListChecked";
      MenuCheck.Usage = EFPCommandItemUsage.Menu;
      Add(MenuCheck);

      ciCheckSel = new EFPCommandItem("Edit", "SetCheckMarks");
      ciCheckSel.Parent = MenuCheck;
      ciCheckSel.GroupBegin = true;
      ciCheckSel.MenuText = Res.Cmd_Menu_CheckMarks_SetSelected;
      ciCheckSel.ImageKey = "CheckListChecked";
      ciCheckSel.ShortCut = Keys.Add;
      ciCheckSel.Click += new EventHandler(ciCheckSel_Click);
      Add(ciCheckSel);

      ciUncheckSel = new EFPCommandItem("Edit", "DeleteCheckMarks");
      ciUncheckSel.Parent = MenuCheck;
      ciUncheckSel.MenuText = Res.Cmd_Menu_CheckMarks_DelSelected;
      ciUncheckSel.ImageKey = "CheckListUnchecked";
      ciUncheckSel.ShortCut = Keys.Subtract;
      ciUncheckSel.Click += new EventHandler(ciUncheckSel_Click);
      Add(ciUncheckSel);

      ciInvertSel = new EFPCommandItem("Edit", "InvertCheckMarks");
      ciInvertSel.Parent = MenuCheck;
      ciInvertSel.MenuText = Res.Cmd_Menu_CheckMarks_InvSelected;
      ciInvertSel.ImageKey = "CheckListInvert";
      ciInvertSel.ShortCut = Keys.Multiply;
      ciInvertSel.Click += new EventHandler(ciInvertSel_Click);
      Add(ciInvertSel);

      ciCheckAll = new EFPCommandItem("Edit", "SetAllCheckMarks");
      ciCheckAll.Parent = MenuCheck;
      ciCheckAll.MenuText = Res.Cmd_Menu_CheckMarks_SetAll;
      ciCheckAll.ImageKey = "CheckListAll";
      ciCheckAll.ShortCut = Keys.Control | Keys.A;
      ciCheckAll.Click += new EventHandler(ciCheckAll_Click);
      Add(ciCheckAll);

      ciUncheckAll = new EFPCommandItem("Edit", "DeleteAllCheckMarks");
      ciUncheckAll.Parent = MenuCheck;
      ciUncheckAll.GroupEnd = true;
      ciUncheckAll.MenuText = Res.Cmd_Menu_CheckMarks_DelAll;
      ciUncheckAll.ImageKey = "CheckListNone";
      ciUncheckAll.ShortCut = Keys.Control | Keys.Shift | Keys.A;
      ciUncheckAll.Click += new EventHandler(ciUncheckAll_Click);
      Add(ciUncheckAll);

      #endregion

      #region Поиск

      MenuSearch = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.MenuSearch);
      Add(MenuSearch);

      ciFind = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Find);
      ciFind.Parent = MenuSearch;
      ciFind.Click += new EventHandler(Find);
      ciFind.GroupBegin = true;
      Add(ciFind);

      ciIncSearch = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.IncSearch);
      ciIncSearch.Parent = MenuSearch;
      ciIncSearch.Click += new EventHandler(IncSearch);
      ciIncSearch.StatusBarText = "??????????????????????";
      Add(ciIncSearch);

      ciFindNext = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.FindNext);
      ciFindNext.Parent = MenuSearch;
      ciFindNext.Click += new EventHandler(FindNext);
      ciFindNext.GroupEnd = true;
      Add(ciFindNext);

      #endregion

      #region Дополнительные команды навигации

      ciGotoRowWithDiffTextUp = new EFPCommandItem("Edit", "GotoRowWithDiffTextUp");
      ciGotoRowWithDiffTextUp.Parent = MenuSearch;
      ciGotoRowWithDiffTextUp.MenuText = Res.Cmd_Menu_Edit_GotoRowWithDiffTextUp;
      ciGotoRowWithDiffTextUp.ShortCut = Keys.Alt | Keys.PageUp;
      ciGotoRowWithDiffTextUp.Click += CiGotoRowWithDiffTextUp_Click;
      ciGotoRowWithDiffTextUp.GroupBegin = true;
      Add(ciGotoRowWithDiffTextUp);

      ciGotoRowWithDiffTextDown = new EFPCommandItem("Edit", "GotoRowWithDiffTextDown");
      ciGotoRowWithDiffTextDown.Parent = MenuSearch;
      ciGotoRowWithDiffTextDown.MenuText = Res.Cmd_Menu_Edit_GotoRowWithDiffTextDown;
      ciGotoRowWithDiffTextDown.ShortCut = Keys.Alt | Keys.PageDown;
      ciGotoRowWithDiffTextDown.Click += CiGotoRowWithDiffTextDown_Click;
      ciGotoRowWithDiffTextDown.GroupEnd = true;
      Add(ciGotoRowWithDiffTextDown);

      #endregion

      #region Порядок строк

      _MenuSort = new EFPCommandItem("Edit", "MenuOrder");
      _MenuSort.MenuText = Res.Cmd_Menu_RowOrder;
      _MenuSort.ImageKey = "OrderAZ";
      _MenuSort.Usage = EFPCommandItemUsage.Menu;
      Add(_MenuSort);

      AddSeparator();

      ciSortMoveUp = new EFPCommandItem("Edit", "MoveUp");
      ciSortMoveUp.Parent = _MenuSort;
      ciSortMoveUp.MenuText = Res.Cmd_Menu_RowOrder_MoveUp;
      ciSortMoveUp.ImageKey = "ArrowUp";
      ciSortMoveUp.ShortCut = Keys.Alt | Keys.Up;
      ciSortMoveUp.Click += new EventHandler(ciSortMoveUp_Click);
      ciSortMoveUp.Usage = EFPCommandItemUsage.None; // включим, когда надо
      Add(ciSortMoveUp);

      ciSortMoveDown = new EFPCommandItem("Edit", "MoveDown");
      ciSortMoveDown.Parent = _MenuSort;
      ciSortMoveDown.MenuText = Res.Cmd_Menu_RowOrder_MoveDown;
      ciSortMoveDown.ImageKey = "ArrowDown";
      ciSortMoveDown.ShortCut = Keys.Alt | Keys.Down;
      ciSortMoveDown.Click += new EventHandler(ciSortMoveDown_Click);
      ciSortMoveDown.Usage = EFPCommandItemUsage.None; // включим, когда надо
      Add(ciSortMoveDown);

      ciSortRestore = new EFPCommandItem("Edit", "RestoreOrder");
      ciSortRestore.Parent = _MenuSort;
      ciSortRestore.MenuText = Res.Cmd_Menu_RowOrder_RestoreOrder;
      ciSortRestore.ImageKey = "RestoreDefaultOrder";
      ciSortRestore.Click += new EventHandler(ciSortRestore_Click);
      ciSortRestore.Usage = EFPCommandItemUsage.None; // включим, когда надо
      Add(ciSortRestore);

      AddSeparator();

      _OrderItems = null; // Потом

      #endregion

      #region Строки с ошибками

      _MenuRowErrors = new EFPCommandItem("View", "MenuRowErrors");
      _MenuRowErrors.MenuText = Res.Cmd_Menu_RowErrNav;
      _MenuRowErrors.ImageKey = "ErrorRowNavigation";
      _MenuRowErrors.Usage = EFPCommandItemUsage.Menu;
      Add(_MenuRowErrors);

      ciGotoPrevErrorWarning = new EFPCommandItem("View", "PrevRowWarning");
      ciGotoPrevErrorWarning.Parent = _MenuRowErrors;
      ciGotoPrevErrorWarning.MenuText = Res.Cmd_Menu_RowErrNav_PrevWarning;
      ciGotoPrevErrorWarning.ImageKey = "PrevErrorRow";
      ciGotoPrevErrorWarning.ShortCut = Keys.Control | Keys.OemOpenBrackets;
      ciGotoPrevErrorWarning.ToolTipText = Res.Cmd_ToolTip_RowErrNav_PrevWarning;
      ciGotoPrevErrorWarning.Click += new EventHandler(ciGotoPrevErrorWarning_Click);
      Add(ciGotoPrevErrorWarning);

      ciGotoNextErrorWarning = new EFPCommandItem("View", "NextRowWarning");
      ciGotoNextErrorWarning.Parent = _MenuRowErrors;
      ciGotoNextErrorWarning.MenuText = Res.Cmd_Menu_RowErrNav_NextWarning;
      ciGotoNextErrorWarning.ImageKey = "NextErrorRow";
      ciGotoNextErrorWarning.ShortCut = Keys.Control | Keys.OemCloseBrackets;
      ciGotoNextErrorWarning.ToolTipText = Res.Cmd_ToolTip_RowErrNav_NextWarning;
      ciGotoNextErrorWarning.Click += new EventHandler(ciGotoNextErrorWarning_Click);
      Add(ciGotoNextErrorWarning);

      ciGotoPrevErrorOnly = new EFPCommandItem("View", "PrevRowError");
      ciGotoPrevErrorOnly.Parent = _MenuRowErrors;
      ciGotoPrevErrorOnly.MenuText = Res.Cmd_Menu_RowErrNav_PrevError;
      //ciGotoPrevErrorOnly.ImageKey = "PrevErrorRow";
      ciGotoPrevErrorOnly.ShortCut = Keys.Control | Keys.Shift | Keys.OemOpenBrackets;
      ciGotoPrevErrorOnly.ToolTipText = Res.Cmd_ToolTip_RowErrNav_PrevError;
      ciGotoPrevErrorOnly.Click += new EventHandler(ciGotoPrevErrorOnly_Click);
      Add(ciGotoPrevErrorOnly);

      ciGotoNextErrorOnly = new EFPCommandItem("View", "NextRowError");
      ciGotoNextErrorOnly.Parent = _MenuRowErrors;
      ciGotoNextErrorOnly.MenuText = Res.Cmd_Menu_RowErrNav_NextError;
      //ciGotoNextErrorOnly.ImageKey = "NextErrorRow";
      ciGotoNextErrorOnly.ShortCut = Keys.Control | Keys.Shift | Keys.OemCloseBrackets;
      ciGotoNextErrorOnly.ToolTipText = Res.Cmd_ToolTip_RowErrNav_NextError;
      ciGotoNextErrorOnly.GroupEnd = true;
      ciGotoNextErrorOnly.Click += new EventHandler(ciGotoNextErrorOnly_Click);
      Add(ciGotoNextErrorOnly);

      if (!EFPApp.EasyInterface)
      {
        ciCopyRowErrorMessages = new EFPCommandItem("Edit", "CopyError");
        ciCopyRowErrorMessages.Parent = _MenuRowErrors;
        ciCopyRowErrorMessages.MenuText = Res.Cmd_Menu_RowErrNav_CopyMessage;
        ciCopyRowErrorMessages.ToolTipText = Res.Cmd_ToolTip_RowErrNav_CopyMessage;
        ciCopyRowErrorMessages.Click += new EventHandler(ciCopyRowErrorMessages_Click);
        Add(ciCopyRowErrorMessages);
      }

      if (!EFPApp.EasyInterface)
      {
        ciShowRowErrorMessages = new EFPCommandItem("View", "ErrorList");
        ciShowRowErrorMessages.Parent = _MenuRowErrors;
        ciShowRowErrorMessages.MenuText = Res.Cmd_Menu_RowErrNav_MessageList;
        ciShowRowErrorMessages.ToolTipText = Res.Cmd_ToolTip_RowErrNav_MessageList;
        ciShowRowErrorMessages.Click += new EventHandler(ciShowRowErrorMessages_Click);
        Add(ciShowRowErrorMessages);

      }

      #endregion

      #region Отправить

      _OutHandler = new EFPMenuOutHandler(this);
      _OutHandler.Items.Add(new BRDataGridViewMenuOutItem("Control", controlProvider));

      #endregion

      #region Окно суммы в статусной строке

      if (EFPApp.ShowAutoCalcSums)
      {
        ciStatCount = new EFPCommandItem("View", "StatusCount");
        ciStatCount.MenuText = Res.Cmd_ToolTip_View_StatCount; // для DisplayName
        ciStatCount.StatusBarText = "????";
        ciStatCount.Usage = EFPCommandItemUsage.StatusBar;
        ciStatCount.Click += new EventHandler(ciStatCount_Click);
        Add(ciStatCount);

        ciStatSum = new EFPCommandItem("View", "StatusSum");
        ciStatSum.MenuText = Res.Cmd_ToolTip_View_StatSum; // для DisplayName
        ciStatSum.StatusBarText = "??????????";
        //ciStatSumma.ImageKey = "Sum";
        ciStatSum.Usage = EFPCommandItemUsage.StatusBar;
        ciStatSum.Click += new EventHandler(ciStatSum_Click);
        Add(ciStatSum);
      }

      // так не работает
      //ciStatSummaCopy = new ClientItem("Правка", "КопироватьСумму");
      //ciStatSummaCopy.Parent = ciStatSumma;
      //ciStatSummaCopy.MenuText = "Копировать в буфер";
      //ciStatSummaCopy.ImageKey = "Copy";
      //ciStatSummaCopy.Usage = ClientItemUsage.Menu;
      //ciStatSummaCopy.Click += new EventHandler(ciStatSummaCopy_Click);
      //Add(ciStatSummaCopy);

      #endregion

      #endregion

      Idle += CommandItems_Idle;
    }

    /// <summary>
    /// Удаляет встроенный таймер
    /// </summary>
    /// <param name="disposing">true, если был вызван метод Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      if (_StatSummaTimer != null)
      {
        _StatSummaTimer.Dispose();
        _StatSummaTimer = null;
      }
      base.Dispose(disposing);
    }

    /// <summary>
    /// Провайдер табличного просмотра.
    /// Задается в конструкторе.
    /// </summary>
    public new EFPDataGridView ControlProvider { get { return (EFPDataGridView)(base.ControlProvider); } }

    #endregion

    #region OnPrepare()

    /// <summary>
    /// Инициализация <see cref="EFPCommandItem.Usage"/> перед инициализацией меню.
    /// </summary>
    protected override void OnPrepare()
    {
      // Добавляем форматы вставки текста после пользовательских форматов
      // (если уже не были добавлены явно)
      AddTextPasteFormats(); // Вызов метода базового класса заблокирует EFPPasteHandler

      base.OnPrepare();

      if (UseEditView)
      {
        // Назначаем горячие клавиши в зависимости от свойства EnterKeyMode
        if (EnterKeyMode != EFPDataViewEnterKeyMode.None)
        {
          switch (EnterKeyMode)
          {
            case EFPDataViewEnterKeyMode.EditOrView:
              if (ControlProvider.ReadOnly)
                ciView.ShortCuts.Add(Keys.Return);
              else
                ciEdit.ShortCuts.Add(Keys.Return);
              ciView.ShortCuts.Add(Keys.Shift | Keys.Return);
              ciOkButton.ShortCuts.Add(Keys.Control | Keys.Return);
              break;
            case EFPDataViewEnterKeyMode.Edit:
              ciEdit.ShortCuts.Add(Keys.Return);
              ciView.ShortCuts.Add(Keys.Shift | Keys.Return);
              ciOkButton.ShortCuts.Add(Keys.Control | Keys.Return);
              break;
            case EFPDataViewEnterKeyMode.View:
              ciView.ShortCuts.Add(Keys.Return);
              ciView.ShortCuts.Add(Keys.Shift | Keys.Return);
              ciOkButton.ShortCuts.Add(Keys.Control | Keys.Return);
              break;
            case EFPDataViewEnterKeyMode.DefaultButton:
              ciEdit.ShortCuts.Add(Keys.Control | Keys.Return);
              ciView.ShortCuts.Add(Keys.Shift | Keys.Return);
              ciOkButton.ShortCuts.Add(Keys.Return);
              break;
          }
        }
      }
      else
      {
        // Отключаем ненужные команды
        ciEdit.Usage = EFPCommandItemUsage.None;
        ciInsert.Usage = EFPCommandItemUsage.None;
        ciInsertCopy.Usage = EFPCommandItemUsage.None;
        ciDelete.Usage = EFPCommandItemUsage.None;
        ciView.Usage = EFPCommandItemUsage.None;
        ciOkButton.Usage = EFPCommandItemUsage.None;
      }
      MenuEdit.InitMenuVisible();

      if ((!ForceInlineEditStatusPanel) && ciInlineEditStatus != null)
      {
        if (ControlProvider.Control.ReadOnly)
          ciInlineEditStatus.Usage = EFPCommandItemUsage.None;
        else
        {
          // 29.05.2025
          bool hasEditableColumn = false;
          foreach (DataGridViewColumn column in ControlProvider.Control.Columns)
          {
            if (!column.ReadOnly)
            {
              if (column == ControlProvider.MarkRowsGridColumn)
                continue;
              hasEditableColumn = true;
              break;
            }
          }
          if (!hasEditableColumn)
            ciInlineEditStatus.Usage = EFPCommandItemUsage.None;
        }
      }

      if (UseRefresh)
        ciRefresh.Visible = ControlProvider.HasRefreshDataHandler;
      else
        ciRefresh.Usage = EFPCommandItemUsage.None;

      #region Буфер обмена


      EFPCommandItemUsage clipboardUsage1 = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
      if (ClipboardInToolBar)
        clipboardUsage1 |= EFPCommandItemUsage.ToolBar;
      EFPCommandItemUsage clipboardUsage2 = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
      if (ClipboardInToolBar)
        clipboardUsage2 |= EFPCommandItemUsage.ToolBarAux;

      if (Cut == null &&
        (ControlProvider.Control.ReadOnly || PasteHandler.Count == 0 /* 23.05.2025 */))
      {
        ciCut.Enabled = false;
        ciCut.Usage = EFPCommandItemUsage.None;
      }
      else
        ciCut.Usage = clipboardUsage1;

      ciCopy.Usage = clipboardUsage1;

      ciCopyHyperlink.Usage = clipboardUsage2;
      ciCopySettings.Usage = clipboardUsage2;
      ciCopyToolTip.Usage = clipboardUsage2;

      switch (CopyHyperlinkUsage)
      {
        case EFPDataViewCopyHyperlinkCommandUsage.Auto:
          ciCopyHyperlink.Visible = CopyHyperlinkDefaultVisible;
          break;
        case EFPDataViewCopyHyperlinkCommandUsage.Unused:
          ciCopyHyperlink.Usage = EFPCommandItemUsage.None;
          break;
      }

      _PasteHandler.PasteApplied += new EventHandler(PasteHandler_PasteApplied);

      #endregion

      if (!UseSelectAll)
        ciSelectAll.Usage = EFPCommandItemUsage.None;

      if (!UseGotoRowWithDifferentCellText)
      {
        ciGotoRowWithDiffTextUp.Usage = EFPCommandItemUsage.None;
        ciGotoRowWithDiffTextDown.Usage = EFPCommandItemUsage.None;
      }

      if (ControlProvider.ManualOrderSupported)
      {
        ciSortMoveDown.Usage = EFPCommandItemUsage.Everywhere;
        ciSortMoveUp.Usage = EFPCommandItemUsage.Everywhere;
        if (ControlProvider.RestoreManualOrderSupported)
          ciSortRestore.Usage = EFPCommandItemUsage.Everywhere;
        else
          ciSortRestore.Usage = EFPCommandItemUsage.None;
      }
      else
      {
        _MenuSort.Usage = EFPCommandItemUsage.None;
        ciSortMoveDown.Usage = EFPCommandItemUsage.None;
        ciSortMoveUp.Usage = EFPCommandItemUsage.None;
        ciSortRestore.Usage = EFPCommandItemUsage.None;
      }

      if (ControlProvider.OrderCount > 0 ||
        ControlProvider.CustomOrderAllowed ||
        (ControlProvider.GridProducer != null && ControlProvider.GridProducer.OrderCount > 0 && ControlProvider.UseGridProducerOrders))
      {
        // Команды сортировки строк существуют или могут появиться в будущем
        _MenuSort.Usage = EFPCommandItemUsage.Everywhere | EFPCommandItemUsage.DisableRightTextInToolTip;
        _OrderItems = new EFPCommandItem[8];
        for (int i = 0; i < _OrderItems.Length; i++)
        {
          EFPCommandItem ci1 = new EFPCommandItem("View", "Order" + (i + 1).ToString());
          ci1.MenuText = (i + 1).ToString();
          ci1.Parent = _MenuSort;
          ci1.GroupBegin = (i == 0);
          //ci1.ImageKey = "Item";
          if (i <= 8) // Ctrl+9 используется для произвольной сортировки
            ci1.ShortCut = Keys.Control | (Keys)(((int)Keys.D1) + i);
          ci1.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
          ci1.Tag = i;
          ci1.Click += new EventHandler(SelectOrder_Click);
          Add(ci1);
          _OrderItems[i] = ci1;
        }

        _MenuSort.Children.AddSeparator();

        ciOrderCustom = new EFPCommandItem("View", "CustomOrder");
        ciOrderCustom.MenuText = Res.Cmd_Menu_RowOrder_Custom;
        ciOrderCustom.ImageKey = "OrderCustom";
        ciOrderCustom.Parent = _MenuSort;
        ciOrderCustom.ShortCut = Keys.Control | Keys.D9;
        ciOrderCustom.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
        ciOrderCustom.Click += new EventHandler(ciOrderCustom_Click);
        Add(ciOrderCustom);


        ciOrderMore = new EFPCommandItem("View", "OrderDialog");
        ciOrderMore.MenuText = Res.Cmd_Menu_RowOrder_Dialog;
        ciOrderMore.Parent = _MenuSort;
        ciOrderMore.ShortCut = Keys.Control | Keys.D0;
        ciOrderMore.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
        ciOrderMore.Click += new EventHandler(ciOrderMore_Click);
        Add(ciOrderMore);
      }
      InitOrderItems();

      if ((!ControlProvider.UseRowImages) || (!UseRowErrors))
      {
        _MenuRowErrors.Usage = EFPCommandItemUsage.None;
        ciGotoNextErrorWarning.Usage = EFPCommandItemUsage.None;
        ciGotoPrevErrorWarning.Usage = EFPCommandItemUsage.None;
        ciGotoNextErrorOnly.Usage = EFPCommandItemUsage.None;
        ciGotoPrevErrorOnly.Usage = EFPCommandItemUsage.None;
        if (ciCopyRowErrorMessages != null)
          ciCopyRowErrorMessages.Usage = EFPCommandItemUsage.None;
        if (ciShowRowErrorMessages != null)
          ciShowRowErrorMessages.Usage = EFPCommandItemUsage.None;
      }
      if (!UseRowErrorsListView)
      {
        if (ciShowRowErrorMessages != null)
          ciShowRowErrorMessages.Usage = EFPCommandItemUsage.None;

        if (ciCopyRowErrorMessages != null)
          ciCopyRowErrorMessages.Usage = EFPCommandItemUsage.None; // 04.12.2024
      }

      if (ControlProvider.MarkRowsGridColumn == null)
      {
        MenuCheck.Usage = EFPCommandItemUsage.None;
        ciCheckSel.Usage = EFPCommandItemUsage.None;
        ciUncheckSel.Usage = EFPCommandItemUsage.None;
        ciInvertSel.Usage = EFPCommandItemUsage.None;
        ciCheckAll.Usage = EFPCommandItemUsage.None;
        ciUncheckAll.Usage = EFPCommandItemUsage.None;
      }

      if (ciCopyToolTip != null)
      {
        if (!ControlProvider.Control.ShowCellToolTips)
          ciCopyToolTip.Usage = EFPCommandItemUsage.None;
      }

      if (!ControlProvider.UseRowImages)
      {
        if (ciCopyRowErrorMessages != null)
          ciCopyRowErrorMessages.Usage = EFPCommandItemUsage.None;
      }

      if (ControlProvider.TextSearchContext == null)
        ciFind.Usage = EFPCommandItemUsage.None;

      if (!ForceIncSearchStatusPanel)
      {
        if (!ControlProvider.CanIncSearch)
          ciIncSearch.Usage &= (~EFPCommandItemUsage.StatusBar);
      }

      if (ciStatCount != null)
      {
        if (!(ControlProvider.Control.MultiSelect && (ControlProvider.Control.SelectionMode == DataGridViewSelectionMode.CellSelect ||
          ControlProvider.Control.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect ||
          ControlProvider.Control.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect)))
        {
          ciStatCount.Visible = false;
          ciStatSum.Visible = false;
        }
        else if (!ControlProvider.HasSummableColumns)
        {
          ciStatCount.Visible = false;
          ciStatSum.Visible = false;
        }
      }

      ControlProvider.Control.CellDoubleClick += new DataGridViewCellEventHandler(Control_CellDoubleClick);
      ControlProvider.Control.MouseDown += new MouseEventHandler(Control_MouseDown);
      ControlProvider.Control.MouseUp += new MouseEventHandler(Control_MouseUp);
      ControlProvider.Control_VisibleChanged(null, null);

      RefreshStatItems();
    }

    #endregion

    #region Обновление состояния команд

    private void CommandItems_Idle(object sender, EventArgs args)
    {
      OnRefreshItems();
    }

    /// <summary>
    /// Обновление доступности команд локального меню после внешнего изменения
    /// выбранных ячеек просмотра
    /// </summary>
    protected virtual void OnRefreshItems()
    {
      UISelectedRowsState selState = ControlProvider.SelectedRowsState;

      #region Правка

      if (UseEditView)
      {
#if XXX
        ciEdit.Visible = !Owner.ReadOnly;
        ciInsert.Visible = !Owner.ReadOnly;
        ciInsertCopy.Visible = !Owner.ReadOnly;
        ciDelete.Visible = !Owner.ReadOnly;
        ciView.Visible = Owner.CanView;


        if (Owner.CanMultiEdit)
          ciEdit.Enabled = (SelState != EFPDataGridViewSelectedRowsState.NoSelection);
        else
          ciEdit.Enabled = (SelState == EFPDataGridViewSelectedRowsState.SingleRow);
        ciInsert.Enabled = Owner.CanInsert;
        ciInsertCopy.Enabled = Owner.CanInsertCopy && (SelState == EFPDataGridViewSelectedRowsState.SingleRow);
        ciDelete.Enabled = Owner.CanDelete && (SelState != EFPDataGridViewSelectedRowsState.NoSelection);

        if (Owner.CanView)
          ciView.Enabled = ciEdit.Enabled;
        else
          ciView.Enabled = false;
#else
        // 21.08.2019

        ciEdit.Visible = (!ControlProvider.ReadOnly) && ControlProvider.CanEdit;
        ciInsert.Visible = (!ControlProvider.ReadOnly) && ControlProvider.CanInsert;
        ciInsertCopy.Visible = (!ControlProvider.ReadOnly) && ControlProvider.CanInsertCopy;
        ciDelete.Visible = (!ControlProvider.ReadOnly) && ControlProvider.CanDelete;
        ciView.Visible = ControlProvider.CanView;


        if (ControlProvider.CanMultiEdit)
          ciEdit.Enabled = (selState != UISelectedRowsState.NoSelection);
        else
          ciEdit.Enabled = (selState == UISelectedRowsState.SingleRow);
        ciInsertCopy.Enabled = (selState == UISelectedRowsState.SingleRow);
        ciDelete.Enabled = (selState != UISelectedRowsState.NoSelection);

        ciView.Enabled = ciEdit.Enabled;
#endif

        if (selState == UISelectedRowsState.MultiRows)
        {
          ciEdit.MenuText = Res.Cmd_Menu_Edit_Edit_MultiRows;
          ciDelete.MenuText = Res.Cmd_Menu_Edit_Delete_MultiRows;
          ciView.MenuText = Res.Cmd_Menu_Edit_View_MultiRows;
        }
        else
        {
          ciEdit.MenuText = Res.Cmd_Menu_Edit_Edit_SingleRow;
          ciDelete.MenuText = Res.Cmd_Menu_Edit_Delete_SingleRow;
          ciView.MenuText = Res.Cmd_Menu_Edit_View_SingleRow;
        }

        //if (ciCut.Usage != EFPCommandItemUsage.None)
        ciCut.Enabled = (selState != UISelectedRowsState.NoSelection) &&
          (!(ControlProvider.ReadOnly && ControlProvider.Control.ReadOnly));
        ciCopy.Enabled = selState != UISelectedRowsState.NoSelection; // 17.06.2024
        if (ciCopyToolTip != null)
          ciCopyToolTip.Enabled = selState != UISelectedRowsState.NoSelection;
        if (ciCopyHyperlink != null && ciCopyHyperlink.Visible)
        {
          if (selState == UISelectedRowsState.NoSelection)
            ciCopyHyperlink.Enabled = false;
          else
          {
            // TODO: Определение выбранных столбцов
            ciCopyHyperlink.Enabled = true;
          }
        }

        if (!PasteHandler.AlwaysEnabled) // 27.11.2017
        {
          if (HasTextPasteFormats)
            PasteHandler.Enabled = !ControlProvider.Control.ReadOnly; // 24.04.2019
          else
            PasteHandler.Enabled = !(ControlProvider.ReadOnly && ControlProvider.Control.ReadOnly);
        }
      }

      if (ciInlineEditStatus != null)
      {
        if (ControlProvider.Control.ReadOnly)
        {
          ciInlineEditStatus.ImageKey = "TableInlineEditReadOnlyTable";
          ciInlineEditStatus.ToolTipText = Res.Cmd_ToolTip_Edit_EditInline_ControlReadOnly;
        }
        else if (ControlProvider.Control.CurrentCell == null)
        {
          ciInlineEditStatus.ImageKey = "TableInlineEditTableReadOnly";
          ciInlineEditStatus.ToolTipText = Res.Cmd_ToolTip_Edit_EditInline_NoSelectedCell;
        }
        else if (ControlProvider.Control.IsCurrentCellInEditMode)
        {
          ciInlineEditStatus.ImageKey = "TableInlineEditProcess";
          ciInlineEditStatus.ToolTipText = Res.Cmd_ToolTip_Edit_EditInline_EditInProgress;
        }
        else
        {
          string readOnlyMessage;
          if (ControlProvider.GetCellReadOnly(ControlProvider.Control.CurrentCell, out readOnlyMessage))
          {
            ciInlineEditStatus.ImageKey = "TableInlineEditReadOnlyCell";
            ciInlineEditStatus.ToolTipText = String.Format(Res.Cmd_ToolTip_Edit_EditInline_CellReadOnly, readOnlyMessage);
          }
          else
          {
            ciInlineEditStatus.ImageKey = "TableInlineEdit";
            ciInlineEditStatus.ToolTipText = Res.Cmd_ToolTip_Edit_EditInline_CanStart;
          }
        }
      }

      #endregion

      if (ciSelectAll != null)
      {
        ciSelectAll.Enabled = (selState != UISelectedRowsState.NoSelection) && ControlProvider.Control.MultiSelect;
      }

      ciFind.Enabled = ControlProvider.Control.RowCount > 0 && ControlProvider.Control.ColumnCount > 0; // 17.06.2024
      RefreshIncSearchItems();

      if (UseGotoRowWithDifferentCellText)
      {
        if (selState == UISelectedRowsState.SingleRow && ControlProvider.CurrentColumnIndex >= 0)
        {
          ciGotoRowWithDiffTextUp.Enabled = ControlProvider.CurrentRowIndex > 0;
          ciGotoRowWithDiffTextDown.Enabled = ControlProvider.CurrentRowIndex < (ControlProvider.Control.RowCount - 1);
        }
        else
        {
          ciGotoRowWithDiffTextUp.Enabled = false;
          ciGotoRowWithDiffTextDown.Enabled = false;
        }
      }

      RefreshRowErrorsItems();
    }

    internal void RefreshIncSearchItems()
    {
      if (ciIncSearch == null)
        return;

      bool isEnabled;
      string menuText;
      string statusBarText;
      bool isChecked = false;

      if (ControlProvider.CurrentIncSearchColumn != null)
      {
        // Поиск по буквам выполняется
        isEnabled = true;
        menuText = Res.Cmd_Menu_Edit_IncSearch_Finish;
        string s = ControlProvider.CurrentIncSearchChars;
        s = s.Replace(' ', (char)(0x00B7));
        s = s.PadRight(20);
        statusBarText = s.ToUpper();
        isChecked = true;
      }
      else
      {
        // Поиск по буквам не выполняется
        if (ControlProvider.CanIncSearch)
        {
          menuText = Res.Cmd_Menu_Edit_IncSearch_Start;
          EFPDataGridViewColumn column = ControlProvider.DesiredIncSearchColumn;
          if (column == null)
          {
            isEnabled = false;
            if (ControlProvider.CurrentColumn == null)
              statusBarText = Res.Cmd_Status_Edit_IncSearch_NoColumn;
            else
              statusBarText = Res.Cmd_Status_Edit_IncSearch_WrongColumn;
          }
          else
          {
            isEnabled = true;
            statusBarText = Res.Cmd_Status_Edit_IncSearch_NotStarted;
          }
        }
        else
        {
          menuText = Res.Cmd_Menu_Edit_IncSearch;
          isEnabled = false;
          statusBarText = Res.Cmd_Status_Edit_IncSearch_Unsupported;
        }
      }

      ciIncSearch.MenuText = menuText;
      ciIncSearch.Enabled = isEnabled;
      ciIncSearch.StatusBarText = statusBarText;
      ciIncSearch.Checked = isChecked;
      ciIncSearch.ToolTipText = menuText;

      if (ControlProvider.CurrentIncSearchColumn == null)
      {
        if (ControlProvider.TextSearchContext == null)
          ciFindNext.Enabled = false;
        else
          ciFindNext.Enabled = ControlProvider.TextSearchContext.ContinueEnabled;
      }
      else
        ciFindNext.Enabled = true;
    }

    /// <summary>
    /// Отключает поиск по первым буквам
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnAfterClick(EFPCommandItemAfterClickEventArgs args)
    {
      base.OnAfterClick(args);

      if (args.Item != ciIncSearch && args.Item != ciFindNext)
        ControlProvider.CurrentIncSearchColumn = null; // 28.01.2021
    }

    #endregion

    #region Команды редактирования

    /// <summary>
    /// Режим работы клавиши "Enter" и двойного щелчка мыши.
    /// По умолчанию - <see cref="EFPDataViewEnterKeyMode.EditOrView"/>.
    /// </summary>
    public EFPDataViewEnterKeyMode EnterKeyMode
    {
      get { return _EnterKeyMode; }
      set
      {
        CheckNotReadOnly();
        _EnterKeyMode = value;
      }
    }
    private EFPDataViewEnterKeyMode _EnterKeyMode;

    /// <summary>
    /// true, если нажатие клавиши Enter не обрабатывается, а передается
    /// форме для нажатия кнопки по умолчанию.
    /// false, если нажатие Enter выполняет редактирование ячейки таблицы.
    /// По умолчанию - false.
    /// Дублирует свойство <see cref="EnterKeyMode"/>
    /// </summary>
    public bool EnterAsOk
    {
      get
      {
        return EnterKeyMode == EFPDataViewEnterKeyMode.DefaultButton;
      }
      set
      {
        if (value)
          EnterKeyMode = EFPDataViewEnterKeyMode.DefaultButton;
        else
          EnterKeyMode = EFPDataViewEnterKeyMode.EditOrView;
      }
    }

    /// <summary>
    /// True, если есть команды "Редактировать", "Создать", "Удалить", "Просмотр"
    /// По умолчанию - true.
    /// </summary>
    public bool UseEditView
    {
      get { return _UseEditView; }
      set
      {
        CheckNotReadOnly();
        _UseEditView = value;
      }
    }
    private bool _UseEditView;


    private EFPCommandItem MenuEdit, ciEdit, ciInsert, ciInsertCopy, ciDelete, ciView, ciOkButton;

    /// <summary>
    /// Доступ к командам редактирования записей
    /// </summary>
    /// <param name="state">Состояние, инициируемое командой меню</param>
    /// <returns>Команда меню</returns>
    public EFPCommandItem this[UIDataState state]
    {
      get
      {
        switch (state)
        {
          case UIDataState.Edit: return ciEdit;
          case UIDataState.Insert: return ciInsert;
          case UIDataState.InsertCopy: return ciInsertCopy;
          case UIDataState.Delete: return ciDelete;
          case UIDataState.View: return ciView;
          default:
            throw ExceptionFactory.ArgUnknownValue("state", state);
        }
      }
    }

    /// <summary>
    /// Панелька со значком для inline-редактирования
    /// </summary>
    private EFPCommandItem ciInlineEditStatus;

    /// <summary>
    /// Если установить в true, то значок состояния редактирования ячейки "по месту" будет присутствовать в статусной строке,
    /// даже если на момент показа просмотра свойство <see cref="DataGridView.ReadOnly"/>=true.
    /// Может потребоваться, если переключение просмотра в режим редактирования выполняется динамически.
    /// По умолчанию - false, значок присутствует, только если просмотр допускает редактирование "по месту".
    /// Свойство можно устанавливать только до вызова события <see cref="EFPControlBase.Created"/>.
    /// </summary>
    public bool ForceInlineEditStatusPanel
    {
      get { return _ForceInlineEditStatusPanel; }
      set
      {
        CheckNotReadOnly();
        _ForceInlineEditStatusPanel = value;
      }
    }
    private bool _ForceInlineEditStatusPanel;


    private void ciOkButton_Click(object sender, EventArgs args)
    {
      // Нажимаем в блоке диалога кнопку по умолчанию
      Form frm = ControlProvider.Control.FindForm();
      if (frm.AcceptButton == null)
        return;
      frm.AcceptButton.PerformClick();
    }

    /// <summary>
    /// Двойной щелчок мыши на ячейке.
    /// Если обрабатывать только это событие, а редактор запускается медленно, то
    /// сетка переходит в режим выделения строк. Пользователь успевает нечаянно
    /// выделить несколько строк, пока открывается редактор. Это некрасиво.
    /// Поэтому откладываем запуск редактора до события MouseUp.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void Control_CellDoubleClick(object sender, DataGridViewCellEventArgs args)
    {
      if (args.RowIndex < 0)
        return;
      if (args.ColumnIndex < 0 && (!EnterAsOk) /* добавлено 31.10.2018 */)
        return;
      _CellDoubleClicked = true;
    }

    private bool _CellDoubleClicked = false;

    void Control_MouseUp(object sender, MouseEventArgs args)
    {
      if (_CellDoubleClicked)
      {
        _CellDoubleClicked = false;

        switch (EnterKeyMode)
        {
          case EFPDataViewEnterKeyMode.DefaultButton:
            // 16.08.2012
            // Если текущая ячейка допускает inline-редактирование, то нажимать кнопку
            // по умолчанию - неправильно
            if (ControlProvider.Control.CurrentCell != null)
            {
              string ReadOnlyMessage;
              if (!ControlProvider.GetCellReadOnly(ControlProvider.Control.CurrentCell, out ReadOnlyMessage))
                return;
            }

            ciOkButton_Click(null, null);
            break;
          case EFPDataViewEnterKeyMode.EditOrView:
            //if (((!ControlProvider.ReadOnly) && ControlProvider.CanEdit /* 19.07.2024 */) ||
            //  ControlProvider.CanView
            //  /*|| (!Handler.MainGrid.ReadOnly)*/)
            //{
            //  ciEdit_Click(null, null);
            //}

            // 29.08.2025
            // Перехватываем исключения при выполнении команды
            // 04.09.2025
            // Проверяем, что предыдущий вызов команды уже завершился
            if (ciEdit.Visible && ciEdit.Enabled)
            {
              if (!ciEdit.InsideClick)
                ciEdit.PerformClick();
            }
            else if (ciView.Visible && ciView.Enabled)
            {
              if (!ciView.InsideClick)
                ciView.PerformClick();
            }
            break;
          case EFPDataViewEnterKeyMode.Edit:
            if (ciEdit.Visible && ciEdit.Enabled)
            {
              if (!ciEdit.InsideClick)
                ciEdit.PerformClick();
            }
            break;
          case EFPDataViewEnterKeyMode.View:
            if (ciView.Visible && ciView.Enabled)
            {
              if (!ciView.InsideClick)
                ciView.PerformClick();
            }
            break;
        }
      }
    }

    void Control_MouseDown(object sender, MouseEventArgs args)
    {
      _CellDoubleClicked = false;
    }

    private void ciEdit_Click(object sender, EventArgs args)
    {
      ControlProvider.PerformEditData(UIDataState.Edit);
    }

    private void ciInsert_Click(object sender, EventArgs args)
    {
      ControlProvider.PerformEditData(UIDataState.Insert);
    }

    private void ciInsertCopy_Click(object sender, EventArgs args)
    {
      ControlProvider.PerformEditData(UIDataState.InsertCopy);
    }

    private void ciDelete_Click(object sender, EventArgs args)
    {
      ControlProvider.PerformEditData(UIDataState.Delete);
    }

    private void ciView_Click(object sender, EventArgs args)
    {
      ControlProvider.PerformEditData(UIDataState.View);
    }

    void ciInlineEditStatus_Click(object sender, EventArgs args)
    {
      if (ControlProvider.Control.IsCurrentCellInEditMode)
        ControlProvider.Control.EndEdit();
      else
        ControlProvider.Control.BeginEdit(false);
    }

    #endregion

    #region Буфер обмена

    /// <summary>
    /// Нужно ли показывать кнопки "Вырезать", "Копировать" и "Вставить" в панели
    /// инструментов (если она есть).
    /// По умолчанию - false (только в меню и горячие клавиши).
    /// </summary>
    public bool ClipboardInToolBar
    {
      get { return _PasteHandler.UseToolBar; }
      set { _PasteHandler.UseToolBar = value; }
    }

    private EFPCommandItem MenuClipboard;

    #region Вырезать

    private EFPCommandItem ciCut;

    /// <summary>
    /// Если обработчик установлен, то в локальное меню добавляется команда "Вырезать".
    /// Если обработчик не установлен, то поддерживается вырезка текста ячеек.
    /// При необходимости обработчик Cut может вызывать метод <see cref="PerformCutText()"/> или
    /// <see cref="TryPerformCutText(out string)"/>.
    /// </summary>
    public event EventHandler Cut;

    private void DoCut(object sender, EventArgs args)
    {
      //*** Owner.CurrentIncSearchColumn = null;

      if (Cut != null)
        Cut(this, EventArgs.Empty);
      else
        PerformCutText();
    }

    /// <summary>
    /// Вырезать текст из выбранных ячеек таблицы в буфер обмена.
    /// Вызывает метод <see cref="TryPerformCutText(out string)"/> и, в случае ошибки, выводит сообщение.
    /// </summary>
    public void PerformCutText()
    {
      string ErrorText;
      if (!TryPerformCutText(out ErrorText))
        EFPApp.ShowTempMessage(ErrorText);
    }

    /// <summary>
    /// Вырезать текст из выбранных ячеек таблицы в буфер обмена.
    /// </summary>
    /// <param name="errorText">Сюда помещается текст сообщения об ошибке, если попытка не удалась</param>
    /// <returns>true, если вырезка выполнена и false в случае ошибки</returns>
    public bool TryPerformCutText(out string errorText)
    {
      if (ControlProvider.Control.ReadOnly)
      {
        errorText = Res.EFPDataView_Err_ControlReadOnly;
        return false;
      }

      EFPDataGridViewRectArea selArea = new EFPDataGridViewRectArea(ControlProvider.Control, EFPDataViewExpRange.Selected);
      if (selArea.IsEmpty)
      {
        errorText = Res.EFPDataView_Err_NoSelectedCell;
        return false;
      }

      if (!selArea.AreAllCellsSelected)
      {
        errorText = Res.EFPDataView_Err_NotRectangleSelection;
        return false;
      }

      // Проверяем, что мы можем записать пустой текст во все ячейки
      for (int i = 0; i < selArea.RowCount; i++)
      {
        for (int j = 0; j < selArea.ColumnCount; j++)
        {
          DataGridViewCell cell = selArea[j, i];
          if (ControlProvider.GetCellReadOnly(cell, out errorText))
            return false;

          if (!ControlProvider.TrySetTextValue(cell, String.Empty, out errorText, true, EFPDataGridViewCellFinishedReason.Clear))
            return false;
        }
      }

      // Копируем все
      if (!PerformCopy())
      {
        errorText = Res.EFPDataView_Err_Copy;
        return false;
      }

      // Очищаем ячейки
      for (int i = 0; i < selArea.Rows.Count; i++)
      {
        for (int j = 0; j < selArea.Columns.Count; j++)
        {
          DataGridViewCell cell = selArea[j, i];
          ControlProvider.SetTextValue(cell, String.Empty, EFPDataGridViewCellFinishedReason.Clear);
        }
      }

      errorText = null;
      return true;
    }

    #endregion

    #region Копировать

    private EFPCommandItem ciCopy, ciCopySettings;

    private void DoCopy(object sender, EventArgs args)
    {
      //*** Owner.CurrentIncSearchColumn = null;
      PerformCopy();
    }

    /// <summary>
    /// Стандартные форматы копирования в буфер обмена.
    /// По умолчанию: Text, CSV и HTML
    /// Можно отключить стандартные форматы копирования, если необходимо копировать данные в нестандартном формате.
    /// Тогда эти форматы можно добавить в обработчике <see cref="AddCopyFormats"/>.
    /// </summary>
    public EFPDataViewCopyFormats CopyFormats
    {
      get { return _CopyFormats; }
      set
      {
        CheckNotReadOnly();
        _CopyFormats = value;
      }
    }
    private EFPDataViewCopyFormats _CopyFormats;

    /// <summary>
    /// Стандартные форматы с учетом выбранных пользователем
    /// </summary>
    public EFPDataViewCopyFormats SelectedCopyFormats { get { return CopyFormats & EFPDataViewCopyFormatsForm.UserSelectedFormats; } }

    /// <summary>
    /// Выполнить копирование выделенных ячеек табличного просмотра в буфер обмена.
    /// В случае ошибки выдает сообщение на экран и возвращает false.
    /// Метод может использоваться внутри реализации обработчика Cut.
    /// </summary>
    /// <returns>true, если копирование успешно выполнено. false - в случае ошибки</returns>
    public bool PerformCopy()
    {
      try
      {
        if (ControlProvider.Control.GetCellCount(DataGridViewElementStates.Selected) <= 0)
        {
          EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoSelectedCell);
          return false;
        }

        EFPApp.BeginWait(Res.Clipboard_Phase_SetData, "Copy");
        try
        {
          // 05.06.2017
          // Больше совсем не используем метод DataGridViewContext.GetClipboardContent(), 
          // даже для получения текстового формата.
          // Этот метод возвращает неправильный текст, если используется обработчик GetCellAttributes
          // (все строки возвращают одинаковое значение)
          // DataObject dobj = Owner.Control.GetClipboardContent();
          //string[] a = dobj.GetFormats(false);
          // Возвращается 4 формата данных:
          // 1. Csv
          // 2. HTML Format
          // 3. Unicode Text
          // 4. Text

          DataObject dobj2 = new DataObject();
          DataObjectEventArgs args = new DataObjectEventArgs(dobj2);

          OnAddDefaultCopyFormats(args);
          OnAddCopyFormats(args);

          new EFPClipboard().SetDataObject(dobj2, true);
        }
        finally
        {
          EFPApp.EndWait();
        }
        return true;
      }
      catch (Exception e)
      {
        EFPApp.ErrorMessageBox(e.Message);
        return false;
      }
    }

    private void OnAddDefaultCopyFormats(DataObjectEventArgs args)
    {
      if ((SelectedCopyFormats & (EFPDataViewCopyFormats.Text | EFPDataViewCopyFormats.CSV)) != 0)
      {
        EFPDataGridViewRectArea area = ControlProvider.GetRectArea(EFPDataViewExpRange.Selected);
        string[,] a = ControlProvider.GetCellTextValues(area);

        if ((SelectedCopyFormats & EFPDataViewCopyFormats.Text) == EFPDataViewCopyFormats.Text)
          WinFormsTools.SetTextMatrixText(args.DataObject, a);

        if ((SelectedCopyFormats & EFPDataViewCopyFormats.CSV) == EFPDataViewCopyFormats.CSV)
          WinFormsTools.SetTextMatrixCsv(args.DataObject, a);
      }

      if ((SelectedCopyFormats & EFPDataViewCopyFormats.HTML) == EFPDataViewCopyFormats.HTML)
      {
        // HTML-формат собираем сами
        EFPDataGridViewExpHtmlSettings settings = new EFPDataGridViewExpHtmlSettings();
        settings.RangeMode = EFPDataViewExpRange.Selected;
        //if (Owner.Control.SelectionMode == DataGridViewSelectionMode.FullColumnSelect ||
        //  Owner.Control.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect)
        //  Settings.ShowColumnHeaders = true;
        //else
        //  Settings.ShowColumnHeaders = Owner.Control.AreAllCellsSelected(false);
        settings.ShowColumnHeaders = false; // 01.10.2020
        settings.Encoding = Encoding.UTF8;
        byte[] buffer = EFPDataGridViewExpHtml.GetHtmlBytes(ControlProvider, settings, true);
        //System.IO.File.WriteAllBytes(@"d:\temp\table1.html", Buffer);
        MemoryStream strm = new MemoryStream(buffer);
        args.DataObject.SetData(DataFormats.Html, false, strm);
      }
    }

    /// <summary>
    /// Обработчик может добавить при копировании в буфер обмена дополнительные форматы
    /// </summary>
    public event DataObjectEventHandler AddCopyFormats;

    /// <summary>
    /// Вызывается при выполнении команды "Копировать".
    /// Непереопределенный метод вызывает событие <see cref="AddCopyFormats"/>.
    /// Переопределенный метод может добавить дополнительные форматы.
    /// Стандартные форматы (TEXT, CSV, HTML) уже добавлены на момент вызова, в зависимости от свойства CopyFormats.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnAddCopyFormats(DataObjectEventArgs args)
    {
      if (AddCopyFormats != null)
        AddCopyFormats(this, args);
    }

    #endregion

    #region Копировать гиперссылку

    EFPCommandItem ciCopyHyperlink;

    ///// <summary>
    ///// Использование команды "Копировать гиперссылку".
    ///// Если свойство не установлено в явном виде, определяется автоматически по наличию столбцов <see cref="DataGridViewLinkColumn"/>
    ///// или по наличию ссылочных столбцов в <see cref="IEFPGridProducer"/>.
    ///// Свойство может устанавливаться только до вывода просмотра на экран
    ///// </summary>
    //public bool UseCopyHyperlink
    //{
    //  get
    //  {
    //    if (_UseCopyHyperlink.HasValue)
    //      return _UseCopyHyperlink.HasValue;
    //    else
    //    {
    //      foreach (DataGridViewColumn column in ControlProvider.Control.Columns)
    //      {
    //        if (column is DataGridViewLinkColumn)
    //          return true;
    //      }
    //      return false;
    //    }
    //  }
    //  set
    //  {
    //    ControlProvider.CheckHasNotBeenCreated();
    //    _UseCopyHyperlink = value;
    //  }
    //}
    //private bool? _UseCopyHyperlink;

    /// <summary>
    /// Использование команды "Копировать гиперссылку".
    /// По умолчанию - <see cref="EFPDataViewCopyHyperlinkCommandUsage.Auto"/>. Видимость команды определяется автоматически по наличию столбцов <see cref="DataGridViewLinkColumn"/>
    /// или по наличию ссылочных столбцов в <see cref="IEFPGridProducer"/>.
    /// Свойство может устанавливаться только до вывода просмотра на экран.
    /// </summary>
    public EFPDataViewCopyHyperlinkCommandUsage CopyHyperlinkUsage
    {
      get { return _CopyHyperlinkUsage; }
      set
      {
        ControlProvider.CheckHasNotBeenCreated();
        _CopyHyperlinkUsage = value;
      }
    }
    private EFPDataViewCopyHyperlinkCommandUsage _CopyHyperlinkUsage;

    private bool CopyHyperlinkDefaultVisible
    {
      get
      {
        foreach (DataGridViewColumn column in ControlProvider.Control.Columns)
        {
          if (column is DataGridViewLinkColumn)
            return true;
        }
        return false;
      }
    }

    private void CopyHyperlink_Click(object sender, EventArgs args)
    {
      List<EFPDataGridViewColumn> lstCols = new List<EFPDataGridViewColumn>();
      foreach (EFPDataGridViewColumn col in ControlProvider.SelectedColumns)
      {
        if (col.GridColumn is DataGridViewLinkColumn)
          lstCols.Add(col);
      }
      if (lstCols.Count == 0)
      {
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoSelectedLinkColumn);
        return;
      }
      int[] ridxs = ControlProvider.SelectedRowIndices;
      if (ridxs.Length == 0)
      {
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoSelectedRow);
        return;
      }

      string[,] a = new string[ridxs.Length, lstCols.Count];
      for (int i = 0; i < ridxs.Length; i++)
      {
        ControlProvider.GetRowInfo(ridxs[i], EFPDataViewInfoReason.View);
        for (int j = 0; j < lstCols.Count; j++)
        {
          EFPDataGridViewCellInfoEventArgs ci = ControlProvider.GetCellInfo(lstCols[j].Index);
          BRValueWithLink value = (ci.Value as BRValueWithLink);
          if (value != null)
            a[i, j] = value.LinkData;
          else
            a[i, j] = String.Empty;
        }
      }

      new EFPClipboard().SetTextMatrix(a);
    }

    #endregion

    #region Копировать подсказку

    internal EFPCommandItem ciCopyToolTip;

    private void CopyToolTip_Click(object sender, EventArgs args)
    {
      //*** Owner.CurrentIncSearchColumn = null;

      string text;
      if (ControlProvider.Control.SelectedRows.Count > 0)
      {
        if (ControlProvider.Control.SelectedRows.Count != 1)
        {
          EFPApp.ShowTempMessage(Res.EFPDataView_Err_MultiSelectedRows);
          return;
        }

        text = ControlProvider.GetRowToolTipText(ControlProvider.Control.SelectedRows[0].Index);
        if (String.IsNullOrEmpty(text))
        {
          EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoRowToolTip);
          return;
        }
      }
      else
      {
        if (ControlProvider.Control.SelectedCells.Count > 1)
        {
          EFPApp.ShowTempMessage(Res.EFPDataView_Err_MultiSelectedCells);
          return;
        }
        if (ControlProvider.Control.CurrentCell == null)
        {
          EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoSelectedCell);
          return;
        }

        text = ControlProvider.GetCellToolTipText(ControlProvider.Control.CurrentCell.RowIndex,
          ControlProvider.Control.CurrentCell.ColumnIndex);
        if (String.IsNullOrEmpty(text))
        {
          EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoCellToolTip);
          return;
        }
      }

      new EFPClipboard().SetText(text);
    }

    #endregion

    #region Вставить

    /// <summary>
    /// Обработчик для команд "Вставка" и "Специальная вставка"
    /// </summary>
    public EFPPasteHandler PasteHandler { get { return _PasteHandler; } }
    private EFPPasteHandler _PasteHandler;

    void PasteHandler_PasteApplied(object sender, EventArgs args)
    {
      //*** Owner.CurrentIncSearchColumn = null;
    }

    /// <summary>
    /// Добавить форматы вставки текста из буфера обмена.
    /// Если метод не вызван явно при инициализации просмотра, он вызывается
    /// автоматически перед показом. Допускается многократный вызов, повторные
    /// вызовы игнорируются. Возвращается массив из двух объектов <see cref="EFPPasteTextMatrixFormat"/> (Text и CSV).
    /// Команды не добавляется и возвращается пустой массив, если просмотр не
    /// поддерживает inline-редактирование.
    /// </summary>
    /// <returns>Массив описателей форматов</returns>
    public EFPPasteTextMatrixFormat[] AddTextPasteFormats()
    {
      if (_TextPasteFormats != null)
        return _TextPasteFormats; // повторный вызов

      if (!ControlProvider.Control.ReadOnly)
      {
        // 10.09.2012: сначала - текст, затем - csv
        EFPPasteTextMatrixFormat fmtText = new EFPPasteTextMatrixFormat(false);
        fmtText.Paste += new EFPPasteDataObjectEventHandler(fmtTextOrCSV_Paste);
        fmtText.TestFormat += new EFPTestDataObjectEventHandler(fmtTextOrCSV_TestFormat);
        _PasteHandler.Add(fmtText);

        EFPPasteTextMatrixFormat fmtCSV = new EFPPasteTextMatrixFormat(true);
        fmtCSV.Paste += new EFPPasteDataObjectEventHandler(fmtTextOrCSV_Paste);
        fmtCSV.TestFormat += new EFPTestDataObjectEventHandler(fmtTextOrCSV_TestFormat);
        _PasteHandler.Add(fmtCSV);

        _TextPasteFormats = new EFPPasteTextMatrixFormat[] { fmtText, fmtCSV };
      }
      else
        _TextPasteFormats = new EFPPasteTextMatrixFormat[0];

      return _TextPasteFormats;
    }

    private EFPPasteTextMatrixFormat[] _TextPasteFormats;

    private bool HasTextPasteFormats
    {
      get
      {
        if (_TextPasteFormats == null)
          return false;
        else
          return _TextPasteFormats.Length > 0;
      }
    }

    void fmtTextOrCSV_TestFormat(object sender, EFPTestDataObjectEventArgs args)
    {
      EFPPasteTextMatrixFormat fmt = (EFPPasteTextMatrixFormat)sender;

      // Дополнительная проверка применимости.
      // Проверяем, что текущая ячейка просмотра не ReadOnly
      if (args.Result != EFPTestDataObjectResult.Ok)
        return;

      if (ControlProvider.Control.ReadOnly)
      {
        args.Result = EFPTestDataObjectResult.ApplyError;
        args.DataInfoText = Res.EFPDataView_Err_InlineEditNotImplemented;
        return;
      }

      if (ControlProvider.Control.CurrentCell == null)
      {
        args.Result = EFPTestDataObjectResult.ApplyError;
        args.DataInfoText = Res.EFPDataView_Err_NoSelectedCell;
        return;
      }

      string errorText;
      //if (Owner.GetCellReadOnly(Owner.Control.CurrentCell, out ErrorText))
      if (!ControlProvider.TestCanPasteText(fmt.TextMatrix, out errorText)) // 24.04.2019
      {
        args.Result = EFPTestDataObjectResult.ApplyError;
        args.DataInfoText = errorText;
        return;
      }
    }

    void fmtTextOrCSV_Paste(object sender, EFPPasteDataObjectEventArgs args)
    {
      EFPPasteTextMatrixFormat fmt = (EFPPasteTextMatrixFormat)sender;

      string errorText;
      bool res = ControlProvider.PerformPasteText(fmt.TextMatrix, out errorText);
      if (!res)
      {
        if (args.Reason == EFPPasteReason.PasteSpecial)
          EFPApp.ErrorMessageBox(errorText, EFPCommandItem.RemoveMnemonic(Res.Cmd_Menu_Edit_PasteSpecial));
        else
          EFPApp.ShowTempMessage(errorText);
      }
    }

    #endregion

    #endregion

    #region Команды установки отмеченных строк

    EFPCommandItem MenuCheck, ciCheckSel, ciUncheckSel, ciInvertSel, ciCheckAll, ciUncheckAll;

    private void ciCheckSel_Click(object sender, EventArgs args)
    {
      DoCheckCommand(EFPDataGridViewCheckMarkRows.Selected, EFPDataGridViewCheckMarkAction.Check);
    }

    private void ciUncheckSel_Click(object sender, EventArgs args)
    {
      DoCheckCommand(EFPDataGridViewCheckMarkRows.Selected, EFPDataGridViewCheckMarkAction.Uncheck);
    }

    private void ciInvertSel_Click(object sender, EventArgs args)
    {
      DoCheckCommand(EFPDataGridViewCheckMarkRows.Selected, EFPDataGridViewCheckMarkAction.Invert);
    }

    private void ciCheckAll_Click(object sender, EventArgs args)
    {
      DoCheckCommand(EFPDataGridViewCheckMarkRows.All, EFPDataGridViewCheckMarkAction.Check);
    }

    private void ciUncheckAll_Click(object sender, EventArgs args)
    {
      DoCheckCommand(EFPDataGridViewCheckMarkRows.All, EFPDataGridViewCheckMarkAction.Uncheck);
    }

    private void DoCheckCommand(EFPDataGridViewCheckMarkRows rows, EFPDataGridViewCheckMarkAction action)
    {
      if (ControlProvider.CheckMarkRows(rows, action) == 0)
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_CheckMarksNotChanged);
    }

    #endregion

    #region Команды поиска

    EFPCommandItem MenuSearch, ciIncSearch, ciFind, ciFindNext;

    /// <summary>
    /// Если установить в true, то в статусной строке будет присутствовать панель поиска по первым буквам,
    /// даже если нет ни одного столбца с установленным свойством <see cref="EFPDataGridView.CanIncSearch"/>=true.
    /// Это может потребоваться, если столбцы добавляются динамически.
    /// По умолчанию - false, панель отображается, только если есть хотя бы один столбец, поддерживающий поиск.
    /// Свойство можно устанавливать только до вызова события <see cref="EFPControlBase.Created"/>.
    /// </summary>
    public bool ForceIncSearchStatusPanel
    {
      get { return _ForceIncSearchStatusPanel; }
      set
      {
        ControlProvider.CheckHasNotBeenCreated();
        _ForceIncSearchStatusPanel = value;
      }
    }
    private bool _ForceIncSearchStatusPanel;

    private void IncSearch(object sender, EventArgs args)
    {
      // Начать / закончить поиск по первым буквам
      if (ControlProvider.CurrentIncSearchColumn == null)
      {
        EFPDataGridViewColumn column = ControlProvider.DesiredIncSearchColumn;
        if (column == null)
        {
          if (ControlProvider.CurrentColumn == null)
            EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoSelectedColumn);
          else
            EFPApp.ShowTempMessage(Res.EFPDataView_Err_IncSearchNotSupportedByColumn);
          return;
        }
        ControlProvider.CurrentIncSearchColumn = column;
      }
      else
      {
        ControlProvider.CurrentIncSearchColumn = null;
      }
    }

    private void Find(object sender, EventArgs args)
    {
      if (ControlProvider.TextSearchContext != null) // 27.12.2020
        ControlProvider.TextSearchContext.StartSearch();
    }

    private void FindNext(object sender, EventArgs args)
    {
      if (ControlProvider.CurrentIncSearchColumn == null)
      {
        if (ControlProvider.TextSearchContext != null)
          ControlProvider.TextSearchContext.ContinueSearch();
      }
      else
      {
        if (!ControlProvider.CurrentIncSearchColumn.PerformIncSearch(ControlProvider.CurrentIncSearchChars.ToUpper(), true))
          EFPApp.ShowTempMessage(String.Format(Res.EFPDataView_Err_IncSearchNoMoreRows, ControlProvider.CurrentIncSearchChars));
      }
    }

    #endregion

    #region Дополнительные команды навигации

    /// <summary>
    /// Если установлено в true (по умолчанию), то доступны команды перехода вверх/вниз к строке с отличающимся текстом ячейки.
    /// Свойство следует сбрасывать в false для просмотров, в котором все строки содержат завдомо разный текст, например, как
    /// в диалоге <see cref="ListSelectDialog"/>.
    /// </summary>
    public bool UseGotoRowWithDifferentCellText
    {
      get { return _UseGotoRowWithDifferentCellText; }
      set
      {
        CheckNotReadOnly();
        _UseGotoRowWithDifferentCellText = value;
      }
    }
    private bool _UseGotoRowWithDifferentCellText;

    private EFPCommandItem ciGotoRowWithDiffTextUp, ciGotoRowWithDiffTextDown;

    private void CiGotoRowWithDiffTextUp_Click(object sender, EventArgs args)
    {
      ControlProvider.GotoRowWithDifferentText(false);
    }

    private void CiGotoRowWithDiffTextDown_Click(object sender, EventArgs args)
    {
      ControlProvider.GotoRowWithDifferentText(true);
    }

    #endregion

    #region Команды сортировки строк

    private EFPCommandItem _MenuSort;

    #region Сортировка путем выбора порядка сортировки

    /// <summary>
    /// Девять команд задания порядка сортировки строк (в том числе недействующие сейчас)
    /// </summary>
    private EFPCommandItem[] _OrderItems;

    private EFPCommandItem ciOrderCustom;

    /// <summary>
    /// Команда "Еще" для дополнительных порядков сортировки (больше 9) или настройки пользовательской сортировки при <see cref="EFPDataGridView.CustomOrderAllowed"/>=true.
    /// </summary>
    private EFPCommandItem ciOrderMore;

    /// <summary>
    /// Перестроение списка команд сортировки.
    /// Используется EFPGridProducer после инициализации списка сортировки
    /// </summary>
    internal void InitOrderItems()
    {
      if (_OrderItems == null)
        return;

      int n = ControlProvider.OrderCount;
      for (int i = 0; i < _OrderItems.Length; i++)
      {
        _OrderItems[i].Visible = (i < n);
        if (i < n)
        {
          _OrderItems[i].MenuText = (i + 1).ToString() + ". " + ControlProvider.Orders[i].DisplayName;
          _OrderItems[i].ImageKey = ControlProvider.Orders[i].ImageKey;
        }
      }

      ciOrderCustom.Visible = (n > 0) && ControlProvider.CustomOrderAllowed;
      ciOrderMore.Visible = (n > _OrderItems.Length) || ControlProvider.CustomOrderAllowed;
      _MenuSort.Enabled = (n > 0) || ControlProvider.CustomOrderAllowed;
      InitCurentOrder();
    }

    /// <summary>
    /// Выделение пометкой команды локального меню, соответствующей выбранному
    /// порядку строк
    /// </summary>
    internal void InitCurentOrder()
    {
      if (_OrderItems != null)
      {
        for (int i = 0; i < _OrderItems.Length; i++)
          _OrderItems[i].Checked = (ControlProvider.CurrentOrderIndex == i);
        ciOrderCustom.Checked = ControlProvider.CustomOrderActive;
      }

      if (ControlProvider.CustomOrderActive)
        _MenuSort.ImageKey = "OrderCustom";
      else if (ControlProvider.CurrentOrder == null)
        _MenuSort.ImageKey = "OrderAZ"; // по идее, не должно быть
      else if (ControlProvider.CurrentOrder.SortInfo.Direction == ListSortDirection.Ascending)
        _MenuSort.ImageKey = "OrderAZ";
      else
        _MenuSort.ImageKey = "OrderZA";

      if (_MenuSort.Enabled)
      {
        string s;
        if (ControlProvider.CurrentOrder == null)
          s = Res.EFPDataView_Order_None;
        else
          s = ControlProvider.CurrentOrder.DisplayName;
        _MenuSort.MenuRightText = s;
        _MenuSort.ToolTipText = String.Format(Res.Cmd_ToolTip_View_RowOrder_WithName, s);
      }
      else
      {
        _MenuSort.MenuRightText = String.Empty;
        _MenuSort.ToolTipText = String.Empty;
      }

    }

    /// <summary>
    /// Выбор порядка сортировки строк в диапазоне от 1 до 9
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void SelectOrder_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      int order = (int)(ci.Tag);
      if (order < ControlProvider.OrderCount)
        ControlProvider.CurrentOrderIndex = order;
    }

    /// <summary>
    /// Выбор произвольной сортировки
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void ciOrderCustom_Click(object sender, EventArgs args)
    {
      if (!ControlProvider.CustomOrderAllowed)
        return;

      if (ControlProvider.OrderCount == 0)
        return;

      ControlProvider.CustomOrderActive = !ControlProvider.CustomOrderActive;
    }

    /// <summary>
    /// Выбор порядка строк из полного списка (окно диалога)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void ciOrderMore_Click(object sender, EventArgs args)
    {
      ControlProvider.ShowSelectOrderDialog();
    }

    #endregion

    #region Ручная сортировка с помощью стрелочек

    internal EFPCommandItem ciSortMoveUp, ciSortMoveDown, ciSortRestore;

    void ciSortMoveUp_Click(object sender, EventArgs args)
    {
      ControlProvider.ChangeManualOrder(false);
    }

    void ciSortMoveDown_Click(object sender, EventArgs args)
    {
      ControlProvider.ChangeManualOrder(true);
    }

    void ciSortRestore_Click(object sender, EventArgs args)
    {
      ControlProvider.RestoreManualOrder();
    }

    #endregion

    #endregion

    #region Команды перехода между ошибками

    /// <summary>
    /// True (по умолчанию), если при установленном свойстве <see cref="EFPDataGridView.UseRowImages"/>
    /// нужно выводит команды "Перейти к следующей / предыдущей строке с ошибками".
    /// Установите свойство в false, если картинки в заголовках строк используются
    /// для других целей, а не для показа ошибок.
    /// Свойство игнорируется, если <see cref="EFPDataGridView.UseRowImages"/>=false.
    /// </summary>
    public bool UseRowErrors
    {
      get { return _UseRowErrors; }
      set
      {
        CheckNotReadOnly();
        _UseRowErrors = value;
      }
    }
    private bool _UseRowErrors;

    private EFPCommandItem _MenuRowErrors;
    private EFPCommandItem ciGotoPrevErrorWarning, ciGotoNextErrorWarning, ciGotoPrevErrorOnly, ciGotoNextErrorOnly;
    private EFPCommandItem ciCopyRowErrorMessages;

    void ciGotoNextErrorWarning_Click(object sender, EventArgs args)
    {
      ControlProvider.GotoNextErrorRow(false, true, UIDataViewImageKind.Warning);
    }

    void ciGotoPrevErrorWarning_Click(object sender, EventArgs args)
    {
      ControlProvider.GotoNextErrorRow(false, false, UIDataViewImageKind.Warning);
    }

    void ciGotoNextErrorOnly_Click(object sender, EventArgs args)
    {
      ControlProvider.GotoNextErrorRow(false, true, UIDataViewImageKind.Error);
    }

    void ciGotoPrevErrorOnly_Click(object sender, EventArgs args)
    {
      ControlProvider.GotoNextErrorRow(false, false, UIDataViewImageKind.Error);
    }

    void ciCopyRowErrorMessages_Click(object sender, EventArgs args)
    {
      if (!ControlProvider.CheckSingleRow())
        return;
      ErrorMessageList errors = new ErrorMessageList();
      ControlProvider.GetSelectedRowsErrorMessages(errors);

      if (errors.Count == 0)
      {
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoRowToolTip);
        return;
      }
      string[] a = new string[errors.Count];
      for (int i = 0; i < errors.Count; i++)
        a[i] = errors[i].Text;

      string text = String.Join(Environment.NewLine, a);

      new EFPClipboard().SetText(text);
    }

    /// <summary>
    /// Доступность команд перехода к следующей / предыдущей строке с ошибкой
    /// или предупреждением. Наличие команд определяется свойством <see cref="EFPDataGridView.UseRowImages"/>.
    /// Это свойство позволяет временно блокировать команды (установкой в false),
    /// например, на время заполнения просмотра в фоновом режиме, пока значения не определены.
    /// </summary>
    public virtual bool GotoErrorEnabled
    {
      get { return _GotoErrorEnabled; }
      set
      {
        if (value == _GotoErrorEnabled)
          return;

        _GotoErrorEnabled = value;
        ciGotoNextErrorWarning.Enabled = value;
        ciGotoPrevErrorWarning.Enabled = value;
        ciGotoNextErrorOnly.Enabled = value;
        ciGotoPrevErrorOnly.Enabled = value;
        if (ciCopyRowErrorMessages != null)
          ciCopyRowErrorMessages.Enabled = value;
        if (ciShowRowErrorMessages != null)
          ciShowRowErrorMessages.Enabled = value;
      }
    }
    private bool _GotoErrorEnabled;

    private void RefreshRowErrorsItems()
    {
      if (_MenuRowErrors.Usage == EFPCommandItemUsage.None)
        return;

      // Можно было бы точно определять наличие ошибок/предупреждений относительно текущей строки, но это затратно.
      // Ориентируемся только на значок в левом верхнем углу просмотра.

      UISelectedRowsState selState = ControlProvider.SelectedRowsState;
      bool possibleErrors = true;
      bool possibleWarnings = true;
      if (ControlProvider.ShowErrorCountInTopLeftCell)
      {
        // 04.12.2024
        switch (ControlProvider.TopLeftCellImageKind)
        {
          case UIDataViewImageKind.Error:
            possibleErrors = true;
            possibleWarnings = true;
            break;
          case UIDataViewImageKind.Warning:
            possibleErrors = false;
            possibleWarnings = true;
            break;
          default:
            possibleErrors = false;
            possibleWarnings = false;
            break;
        }
      }

      ciGotoNextErrorWarning.Enabled =
      ciGotoPrevErrorWarning.Enabled = _GotoErrorEnabled && selState != UISelectedRowsState.NoSelection && possibleWarnings;
      ciGotoNextErrorOnly.Enabled =
      ciGotoPrevErrorOnly.Enabled = _GotoErrorEnabled && selState != UISelectedRowsState.NoSelection && possibleErrors;
      if (ciCopyRowErrorMessages != null)
        //ciCopyRowErrorMessages.Enabled = _GotoErrorEnabled && selState != UISelectedRowsState.NoSelection;
        ciCopyRowErrorMessages.Enabled = _GotoErrorEnabled && selState == UISelectedRowsState.SingleRow;
      if (ciShowRowErrorMessages != null)
        ciShowRowErrorMessages.Enabled = _GotoErrorEnabled && selState != UISelectedRowsState.NoSelection;
    }

    #endregion

    #region Построение списка сообщений об ошибках

    /// <summary>
    /// Если установлено в true(по умолчанию), а также установлены свойства <see cref="UseRowErrors"/>
    /// и <see cref="EFPDataGridView.UseRowImages"/>, то в подменю "Строки с ошибками" есть команды
    /// "Показать список" и "Копировать сообщение в буфер обмена".
    /// Это свойство сбрасывается в false в самом просмотре списка ошибок <see cref="EFPErrorDataGridView"/>, т.к.
    /// рекурсивный вызов не имеет смысла.
    /// </summary>
    public bool UseRowErrorsListView
    {
      get { return _UseRowErrorsListView; }
      set
      {
        CheckNotReadOnly();
        _UseRowErrorsListView = value;
      }
    }
    private bool _UseRowErrorsListView;

    private EFPCommandItem ciShowRowErrorMessages;

    private static int _ShowRowErrorMessagesRowMode = 0;

    void ciShowRowErrorMessages_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;

      int nRows = ControlProvider.SelectedRowCount;
      string title1;
      if (nRows == 1)
        title1 = Res.EFPDataView_Title_RowMessagesForCurrentRow;
      else
        title1 = String.Format(Res.EFPDataView_Title_RowMessagesForSelectedRows, nRows);
      string title2 = Res.EFPDataView_Title_RowMessagesForAllRows;

      RadioSelectDialog dlg = new RadioSelectDialog();
      dlg.Title = ci.MenuTextWithoutMnemonic;
      dlg.Items = new string[] { title1, title2 };
      dlg.SelectedIndex = _ShowRowErrorMessagesRowMode;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      _ShowRowErrorMessagesRowMode = dlg.SelectedIndex;
      ErrorMessageList errors = new ErrorMessageList();
      string title;
      EFPApp.BeginWait(ci.MenuTextWithoutMnemonic);
      try
      {
        if (_ShowRowErrorMessagesRowMode == 1)
        {
          ControlProvider.GetAllRowsErrorMessages(errors);
          title = title2;
        }
        else
        {
          ControlProvider.GetSelectedRowsErrorMessages(errors);
          title = title1;
        }
      }
      finally
      {
        EFPApp.EndWait();
      }

      ShowErrorMessageListDialog(errors, title);
    }

    /// <summary>
    /// Вывод списка сообщений об ошибках
    /// </summary>
    /// <param name="errorMessages">Список сообщений</param>
    /// <param name="title">Заголовок окна</param>
    protected virtual void ShowErrorMessageListDialog(ErrorMessageList errorMessages, string title)
    {
      EFPApp.ShowErrorMessageListDialog(errorMessages, title, 0,
        new ErrorMessageItemEventHandler(EditRowForErrorMessage));
    }

    /// <summary>
    /// Обработка команды просмотра сообщения об ошибках
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Аргументы события</param>
    protected void EditRowForErrorMessage(object sender, ErrorMessageItemEventArgs args)
    {
      int rowIndex = DataTools.GetInt32(args.Item.Tag);
      if (rowIndex >= 0 && rowIndex < ControlProvider.Control.RowCount)
        ControlProvider.SelectRowIndex(rowIndex);
    }

    #endregion

    #region Отправить

    /// <summary>
    /// Объект для печати/просмотра/экспорта/отправки
    /// </summary>
    public EFPMenuOutHandler OutHandler { get { return _OutHandler; } }
    private readonly EFPMenuOutHandler _OutHandler;

    /// <summary>
    /// Локальное подменю "Отправить"
    /// </summary>
    public EFPCommandItem MenuSendTo { get { return _OutHandler.MenuSendTo; } }

    #endregion

    #region Окно суммы в статусной строке

    private EFPCommandItem ciStatCount, ciStatSum/*, ciStatSummaCopy*/;

    private Timer _StatSummaTimer;

    private const string EmptyStatCountText = "    ";
    private const string EmptyStatSummaText = "    ";


    /// <summary>
    /// Сколько ячеек можно просуммировать, прежде чем посмотреть на часы
    /// </summary>
    private const int StatCellCountPerCheck = 20;

    /// <summary>
    /// Инициализация ячеек статусной строки для вывода суммы выделенных ячеек
    /// Метод вызывается при любом изменении выделения
    /// </summary>
    public void RefreshStatItems()
    {
      if (ciStatCount == null)
        return;

      if (!ciStatCount.Visible)
        return;
      if (_StatSummaTimer != null)
        _StatSummaTimer.Enabled = false;
      ciStatCount.StatusBarText = EmptyStatCountText;
      ciStatCount.ImageKey = String.Empty;
      ciStatCount.ToolTipText = Res.Cmd_ToolTip_View_StatCount;
      ciStatSum.ImageKey = String.Empty;
      ciStatSum.StatusBarText = EmptyStatSummaText;
      ciStatSum.ToolTipText = Res.Cmd_ToolTip_View_StatSum;
      if (!WantCalcStatItems())
        return;

      _CurrStatCellIndex = 0;
      _CurrStatCellSumma = null;
      _CurrStatCellSummaDigits = 0;
      _CurrStatCellCount = null;
      _CurrStatState = StatCalcState.Process;
      ciStatSum.ImageKey = "HourGlass";
      if (ControlProvider.Control.SelectedCells.Count <= StatCellCountPerCheck)
      {
        CalcStatSum(false);
      }
      else
      {
        if (_StatSummaTimer == null)
        {
          _StatSummaTimer = new Timer();
          _StatSummaTimer.Interval = 10;
          _StatSummaTimer.Tick += new EventHandler(StatSummaTimer_Tick);
        }
        _StatSummaTimer.Enabled = true; // тикнет, когда система не будет занята
      }
    }

    private bool WantCalcStatItems()
    {
      //if (Handler.Grid.AreAllCellsSelected())
      //  return false;
      if (ControlProvider.Control.SelectedRows != null && ControlProvider.Control.SelectedRows.Count > 0)
        return false;
      if (ControlProvider.Control.SelectedColumns != null && ControlProvider.Control.SelectedColumns.Count > 0)
        return false;
      if (ControlProvider.Control.SelectedCells == null)
        return false;
      if (ControlProvider.Control.SelectedCells.Count < 2)
        return false;
      return true;
    }

    void StatSummaTimer_Tick(object sender, EventArgs args)
    {
      try
      {
        if (CalcStatSum(true))
          _StatSummaTimer.Enabled = false;
      }
      catch
      {
        _StatSummaTimer.Enabled = false;
      }
    }

    /// <summary>
    /// Счетчик ячеек при расчете суммы по таймеру
    /// </summary>
    private int _CurrStatCellIndex;
    private decimal? _CurrStatCellSumma;
    private int? _CurrStatCellCount;

    /// <summary>
    /// Число знаков после запятой для округления
    /// </summary>
    private int _CurrStatCellSummaDigits;

    private enum StatCalcState { Process, Ok, Error, NotSummable }
    private StatCalcState _CurrStatState;

    private bool CalcStatSum(bool checkTime)
    {
      if (_CurrStatCellIndex == 0)
        ciStatSum.StatusBarText = Res.Cmd_Status_View_StatCalculating;

      string errorMessage = null;
      try
      {
        if (!DoCalcStatSum(checkTime))
          return false;
        if (_CurrStatState == StatCalcState.Process)
          _CurrStatState = StatCalcState.Ok;
      }
      catch (Exception e)
      {
        _CurrStatState = StatCalcState.Error;
        errorMessage = e.Message;
      }

      // Вычисления закончены или завершились с ошибкой
      switch (_CurrStatState)
      {
        case StatCalcState.Ok:
          if (_CurrStatCellCount.HasValue)
          {
            ciStatCount.StatusBarText = _CurrStatCellCount.Value.ToString();
            ciStatCount.ImageKey = "Count";
            ciStatCount.ToolTipText = Res.Cmd_ToolTip_View_StatCount + Environment.NewLine + Res.Cmd_ToolTip_View_StatMouseInfo;
          }
          else
          {
            ciStatCount.StatusBarText = EmptyStatCountText;
            ciStatCount.ImageKey = String.Empty;
            ciStatCount.ToolTipText = Res.Cmd_ToolTip_View_StatCount;
          }

          if (_CurrStatCellSumma.HasValue)
          {
            ciStatSum.StatusBarText = /*Math.Round(*/_CurrStatCellSumma.Value/*, CurrStatCellSummaDigits, MidpointRounding.AwayFromZero)*/.ToString();
            ciStatSum.ImageKey = "Sum";
            ciStatSum.ToolTipText = Res.Cmd_ToolTip_View_StatSum + Environment.NewLine + Res.Cmd_ToolTip_View_StatMouseInfo;
          }
          else
          {
            ciStatSum.StatusBarText = EmptyStatSummaText;
            ciStatSum.ImageKey = String.Empty;
            ciStatSum.ToolTipText = Res.Cmd_ToolTip_View_StatSum;
          }
          break;

        case StatCalcState.Error:
          ciStatCount.StatusBarText = EmptyStatCountText;
          ciStatCount.ImageKey = String.Empty;
          ciStatCount.ToolTipText = Res.Cmd_ToolTip_View_StatCount + Environment.NewLine + Res.Cmd_ToolTip_View_StatError + Environment.NewLine + errorMessage;
          ciStatCount.ImageKey = "Error";
          ciStatCount.ToolTipText = Res.Cmd_ToolTip_View_StatSum + Environment.NewLine + Res.Cmd_ToolTip_View_StatError + Environment.NewLine + errorMessage;
          break;

        case StatCalcState.NotSummable:
          ciStatCount.StatusBarText = EmptyStatCountText;
          ciStatCount.ImageKey = String.Empty;
          ciStatCount.ToolTipText = Res.Cmd_ToolTip_View_StatCount + Environment.NewLine + Res.Cmd_ToolTip_View_StatNonSummable;
          ciStatSum.StatusBarText = EmptyStatSummaText;
          ciStatSum.ImageKey = String.Empty;
          ciStatSum.ToolTipText = Res.Cmd_ToolTip_View_StatSum + Environment.NewLine + Res.Cmd_ToolTip_View_StatNonSummable;
          break;
#if DEBUG
        default:
          throw new BugException(_CurrStatState.ToString());
#endif
      }

      return true;
    }

    private bool DoCalcStatSum(bool checkTime)
    {
      DateTime startTime = DateTime.Now;
      while (_CurrStatCellIndex < ControlProvider.Control.SelectedCells.Count)
      {
        if (checkTime && _CurrStatCellIndex > 0 && (_CurrStatCellIndex % 20) == 0)
        {
          TimeSpan ts = DateTime.Now - startTime;
          if (ts.TotalMilliseconds > 200.0) // 1/5 секунды
            return false;
        }
        DataGridViewCell cell = ControlProvider.Control.SelectedCells[_CurrStatCellIndex];
        EFPDataGridViewColumn col = ControlProvider.Columns[cell.ColumnIndex];
        if (!col.Summable)
        {
          _CurrStatState = StatCalcState.NotSummable;
          return true;
        }

        _CurrStatCellIndex++;
        ControlProvider.GetRowInfo(cell.RowIndex, EFPDataViewInfoReason.View);
        EFPDataGridViewCellInfoEventArgs args = ControlProvider.GetCellInfo(cell.ColumnIndex);
        decimal x;
        if (!TryGetCellValue(args.FormattedValue, out x))
        {
          if (args.Value == null)
            continue;
          if (!TryGetCellValue(args.Value, out x))
            continue;
        }

        if (_CurrStatCellSumma.HasValue)
          _CurrStatCellSumma = _CurrStatCellSumma.Value + x;
        else
          _CurrStatCellSumma = x;

        if (_CurrStatCellCount.HasValue)
          _CurrStatCellCount = _CurrStatCellCount.Value + 1;
        else
          _CurrStatCellCount = 1;
      }
      return true;
    }

    /// <summary>
    /// Получение числового значения для вычисления суммы в статусной строке
    /// </summary>
    /// <param name="text"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    protected bool TryGetCellValue(object text, out decimal x)
    {
      x = 0m;
      if (text == null)
        return false;
      if (text is DBNull)
        return false;

      string s = text.ToString().Trim();
      if (String.IsNullOrEmpty(s))
        return false;

      UITools.CorrectNumberString(ref s);
      bool res = decimal.TryParse(s, out x);
      if (res)
      {
        int p = s.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, StringComparison.Ordinal);
        if (p >= 0)
          _CurrStatCellSummaDigits = Math.Max(_CurrStatCellSummaDigits, s.Length - p - 1);
      }
      return res;
    }

    void ciStatCount_Click(object sender, EventArgs args)
    {
      string s = ciStatCount.StatusBarText.Trim();
      if (String.IsNullOrEmpty(s))
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoStatValueToCopy);
      else
        new EFPClipboard().SetText(s);
    }

    void ciStatSum_Click(object sender, EventArgs args)
    {
      string s = ciStatSum.StatusBarText.Trim();
      if (String.IsNullOrEmpty(s))
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoStatValueToCopy);
      else
        new EFPClipboard().SetText(s);
    }

    #endregion

    #region Прочие команды

    /// <summary>
    /// True, если есть команда "Обновить" (F5)
    /// По умолчанию - true.
    /// </summary>
    public bool UseRefresh
    {
      get { return _UseRefresh; }
      set
      {
        CheckNotReadOnly();
        _UseRefresh = value;
      }
    }
    private bool _UseRefresh;

    /// <summary>
    /// True, если есть команда "Выбрать все строки" (Ctrl-A)
    /// </summary>
    public bool UseSelectAll
    {
      get { return _UseSelectAll; }
      set
      {
        CheckNotReadOnly();
        _UseSelectAll = value;
      }
    }

    private bool _UseSelectAll;

    EFPCommandItem ciSelectAll, ciRefresh;

    private void SelectAll(object sender, EventArgs args)
    {
      ControlProvider.Control.SelectAll();
    }

    private void Refresh(object sender, EventArgs args)
    {
      ControlProvider.PerformRefresh();
    }

    #endregion
  }
}
