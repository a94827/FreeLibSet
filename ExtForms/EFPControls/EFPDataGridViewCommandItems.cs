﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

//#define USE_PROPS // 10.06.2021. Управление сортировкой для ManualOrderColumn.
// Если определено, то используется новый вариант сортировки с помощью PropertyDescriptor
// Если не определено, то используется старый вариант, с объектами DataRow
// 13.06.2021.: Не работает. После того, как вызывается PropertyDescriptor.SetValue() для объекта DataRowView,
// с полем ManualOrderColumnName, меняется порядок строк в DataView. Объекты DataRowView не переставляются и начинают
// относиться к другим DataRow. 

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

namespace FreeLibSet.Forms
{
  #region DataObjectEventHandler

  /// <summary>
  /// Аргументы события AddCopyFormat
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
    private IDataObject _DataObject;

    #endregion
  }

  /// <summary>
  /// Делегат события AddCopyFormat
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void DataObjectEventHandler(object sender, DataObjectEventArgs args);

  #endregion

  #region Перечисление EFPDataGridViewCopyFormats

  /// <summary>
  /// Стандартные форматы копирования табличного просмотра в буфер обмена
  /// </summary>
  [Flags]
  public enum EFPDataGridViewCopyFormats
  {
    /// <summary>
    /// Текстовый формат.
    /// Столбцы отделяются друг от друга знаком табуляции
    /// </summary>
    Text = 0x01,

    /// <summary>
    /// CSV-формат.
    /// Стобцы отделяются друг от друга знаком ";". Значения заключаются в кавычки по необходимости
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

  /// <summary>
  /// Список команд локального меню табличного просмотра
  /// </summary>
  public class EFPDataGridViewCommandItems : EFPControlCommandItems, IEFPClipboardCommandItems
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Создает список команд
    /// </summary>
    /// <param name="owner">Провайдер табличного просмотра</param>
    public EFPDataGridViewCommandItems(EFPDataGridView owner)
    {
      if (owner == null)
        throw new ArgumentNullException("owner");
      _Owner = owner;

      #region Начальные значения свойств

      _EnterAsOk = false;
      _UseEditView = true;
      _UseRefresh = true;
      _UseSelectAll = true;
      _UseRowErrors = true;
      _UseRowErrorsListView = true;
      _CopyFormats = EFPDataGridViewCopyFormats.All;

      #endregion

      #region Создание команд

      #region Редактирование

      ciEdit = new EFPCommandItem("Edit", "Edit");
      ciEdit.MenuText = "Редактировать запись";
      ciEdit.ImageKey = "Edit";
      ciEdit.ToolTipText = "Редактировать";
      ciEdit.GroupBegin = true;
      ciEdit.Click += new EventHandler(ciEdit_Click);
      Add(ciEdit);

      // Фиктивная тема, для которой нет команды меню или кнопки
      ciEdit2 = new EFPCommandItem("Edit", "Edit2");
      ciEdit2.Usage = EFPCommandItemUsage.ShortCut;
      ciEdit2.Click += new EventHandler(ClickOKButton);
      Add(ciEdit2);

      ciInsert = new EFPCommandItem("Edit", "Insert");
      ciInsert.MenuText = "Новая запись";
      ciInsert.ImageKey = "Insert";
      ciInsert.ToolTipText = "Создать";
      ciInsert.ShortCut = Keys.Insert;
      ciInsert.Click += new EventHandler(ciInsert_Click);
      Add(ciInsert);

      ciInsertCopy = new EFPCommandItem("Edit", "InsertCopy");
      ciInsertCopy.MenuText = "Копия записи";
      ciInsertCopy.ImageKey = "InsertCopy";
      ciInsertCopy.ToolTipText = "Создать на основании существующего";
      ciInsertCopy.ShortCut = Keys.Insert | Keys.Alt;
      ciInsertCopy.Click += new EventHandler(ciInsertCopy_Click);
      Add(ciInsertCopy);

      ciDelete = new EFPCommandItem("Edit", "Delete");
      ciDelete.MenuText = "Удалить запись";
      ciDelete.ImageKey = "Delete";
      ciDelete.ToolTipText = "Удалить";
      ciDelete.ShortCut = Keys.Delete;
      ciDelete.Click += new EventHandler(ciDelete_Click);
      Add(ciDelete);

      ciView = new EFPCommandItem("Edit", "View");
      ciView.MenuText = "Просмотреть запись";
      ciView.ImageKey = "View";
      ciView.ToolTipText = "Просмотр";
      ciView.GroupEnd = true;
      ciView.ShortCut = Keys.Shift | Keys.Return;
      ciView.Click += new EventHandler(ciView_Click);
      Add(ciView);

      if (!EFPApp.EasyInterface)
      {
        ciInlineEditStatus = new EFPCommandItem("Edit", "InlineEditState");
        ciInlineEditStatus.MenuText = "Редактирование по месту";
        ciInlineEditStatus.ImageKey = "EmptyImage";
        ciInlineEditStatus.StatusBarText = EFPCommandItem.EmptyStatusBarText;
        ciInlineEditStatus.Click += new EventHandler(ciInlineEditStatus_Click);
        ciInlineEditStatus.Usage = EFPCommandItemUsage.StatusBar;
        Add(ciInlineEditStatus);
      }

      #endregion

      #region Буфер обмена

      ciCut = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Cut);
      ciCut.Click += new EventHandler(DoCut);
      ciCut.GroupBegin = true;
      Add(ciCut);

      ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
      ciCopy.MenuText = "Копировать ячейки";
      ciCopy.Enabled = true;
      ciCopy.Click += new EventHandler(DoCopy);
      Add(ciCopy);

      if (EFPApp.ShowToolTips)
      {
        ciCopyToolTip = new EFPCommandItem("Edit", "CopyToolTip");
        ciCopyToolTip.MenuText = "Копировать всплывающую подсказку";
        ciCopyToolTip.Click += new EventHandler(DoCopyToolTip);
        Add(ciCopyToolTip);
      }
      AddSeparator();

      _PasteHandler = new EFPPasteHandler(this);

      #endregion

      #region Выделить все

      ciSelectAll = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SelectAll);
      ciSelectAll.MenuText = "Выделить все строки таблицы";
      //ciDebugChanges.ImageKey="SelectAll";
      ciSelectAll.ToolTipText = "Выделить все";
      ciSelectAll.Click += new EventHandler(SelectAll);
      Add(ciSelectAll);

      #endregion

      #region "Обновить"

      ciRefresh = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Refresh);
      ciRefresh.MenuText = "Обновить содержимое";
      ciRefresh.Click += new EventHandler(Refresh);
      Add(ciRefresh);

      #endregion

      #region Установка отметок

      MenuCheck = new EFPCommandItem("Edit", "CheckMarks");
      MenuCheck.MenuText = "Установка отметок для строк";
      MenuCheck.ImageKey = "CheckListChecked";
      MenuCheck.Usage = EFPCommandItemUsage.Menu;
      Add(MenuCheck);

      ciCheckSel = new EFPCommandItem("Edit", "SetCheckMarks");
      ciCheckSel.Parent = MenuCheck;
      ciCheckSel.GroupBegin = true;
      ciCheckSel.MenuText = "Установить отметки для выбранных строк";
      ciCheckSel.ImageKey = "CheckListChecked";
      ciCheckSel.ShortCut = Keys.Add;
      ciCheckSel.Click += new EventHandler(ciCheckSel_Click);
      Add(ciCheckSel);

      ciUncheckSel = new EFPCommandItem("Edit", "DeleteCheckMarks");
      ciUncheckSel.Parent = MenuCheck;
      ciUncheckSel.MenuText = "Снять отметки для выбранных строк";
      ciUncheckSel.ImageKey = "CheckListUnchecked";
      ciUncheckSel.ShortCut = Keys.Subtract;
      ciUncheckSel.Click += new EventHandler(ciUncheckSel_Click);
      Add(ciUncheckSel);

      ciInvertSel = new EFPCommandItem("Edit", "InvertCheckMarks");
      ciInvertSel.Parent = MenuCheck;
      ciInvertSel.MenuText = "Инвертировать отметки для выбранных строк";
      ciInvertSel.ImageKey = "CheckListInvert";
      ciInvertSel.ShortCut = Keys.Multiply;
      ciInvertSel.Click += new EventHandler(ciInvertSel_Click);
      Add(ciInvertSel);

      ciCheckAll = new EFPCommandItem("Edit", "SetAllCheckMarks");
      ciCheckAll.Parent = MenuCheck;
      ciCheckAll.MenuText = "Установить отметки для всех строк";
      ciCheckAll.ImageKey = "CheckListAll";
      ciCheckAll.ShortCut = Keys.Control | Keys.A;
      ciCheckAll.Click += new EventHandler(ciCheckAll_Click);
      Add(ciCheckAll);

      ciUncheckAll = new EFPCommandItem("Edit", "DeleteAllCheckMarks");
      ciUncheckAll.Parent = MenuCheck;
      ciUncheckAll.GroupEnd = true;
      ciUncheckAll.MenuText = "Снять отметки для всех строк";
      ciUncheckAll.ImageKey = "CheckListNone";
      ciUncheckAll.ShortCut = Keys.Control | Keys.Shift | Keys.A;
      ciUncheckAll.Click += new EventHandler(ciUncheckAll_Click);
      Add(ciUncheckAll);

      #endregion

      #region Поиск

      ciFind = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Find);
      ciFind.Click += new EventHandler(Find);
      ciFind.GroupBegin = true;
      Add(ciFind);

      ciIncSearch = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.IncSearch);
      ciIncSearch.Click += new EventHandler(IncSearch);
      ciIncSearch.StatusBarText = "??????????????????????";
      Add(ciIncSearch);

      ciFindNext = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.FindNext);
      ciFindNext.Click += new EventHandler(FindNext);
      ciFindNext.GroupEnd = true;
      Add(ciFindNext);

      #endregion

      #region Порядок строк

      _MenuSort = new EFPCommandItem("Edit", "MenuOrder");
      _MenuSort.MenuText = "Порядок строк";
      _MenuSort.ImageKey = "OrderAZ";
      _MenuSort.Usage = EFPCommandItemUsage.Menu;
      Add(_MenuSort);

      AddSeparator();

      ciSortMoveUp = new EFPCommandItem("Edit", "MoveUp");
      ciSortMoveUp.Parent = _MenuSort;
      ciSortMoveUp.MenuText = "Переместить вверх";
      ciSortMoveUp.ImageKey = "ArrowUp";
      ciSortMoveUp.ShortCut = Keys.Alt | Keys.Up;
      ciSortMoveUp.Click += new EventHandler(ciSortMoveUp_Click);
      ciSortMoveUp.Usage = EFPCommandItemUsage.None; // включим, когда надо
      Add(ciSortMoveUp);

      ciSortMoveDown = new EFPCommandItem("Edit", "MoveDown");
      ciSortMoveDown.Parent = _MenuSort;
      ciSortMoveDown.MenuText = "Переместить вниз";
      ciSortMoveDown.ImageKey = "ArrowDown";
      ciSortMoveDown.ShortCut = Keys.Alt | Keys.Down;
      ciSortMoveDown.Click += new EventHandler(ciSortMoveDown_Click);
      ciSortMoveDown.Usage = EFPCommandItemUsage.None; // включим, когда надо
      Add(ciSortMoveDown);

      ciSortRestore = new EFPCommandItem("Edit", "RestoreOrder");
      ciSortRestore.Parent = _MenuSort;
      ciSortRestore.MenuText = "Восстановить порядок по умолчанию";
      ciSortRestore.ImageKey = "RestoreDefaultOrder";
      ciSortRestore.Click += new EventHandler(ciSortRestore_Click);
      ciSortRestore.Usage = EFPCommandItemUsage.None; // включим, когда надо
      Add(ciSortRestore);

      AddSeparator();

      _OrderItems = null; // Потом

      #endregion

      #region Строки с ошибками

      _MenuRowErrors = new EFPCommandItem("View", "MenuRowErrors");
      _MenuRowErrors.MenuText = "Строки с ошибками";
      _MenuRowErrors.ImageKey = "ErrorRowNavigation";
      _MenuRowErrors.Usage = EFPCommandItemUsage.Menu;
      Add(_MenuRowErrors);

      ciGotoPrevErrorWarning = new EFPCommandItem("View", "PrevRowWarning");
      ciGotoPrevErrorWarning.Parent = _MenuRowErrors;
      ciGotoPrevErrorWarning.MenuText = "Перейти к предыдущей строке с ош./пр.";
      ciGotoPrevErrorWarning.ImageKey = "PrevErrorRow";
      ciGotoPrevErrorWarning.ShortCut = Keys.Control | Keys.OemOpenBrackets;
      ciGotoPrevErrorWarning.ToolTipText = "Перейти к предыдущей строке с ошибкой или предупреждением";
      ciGotoPrevErrorWarning.Click += new EventHandler(ciGotoPrevErrorWarning_Click);
      Add(ciGotoPrevErrorWarning);

      ciGotoNextErrorWarning = new EFPCommandItem("View", "NextRowWarning");
      ciGotoNextErrorWarning.Parent = _MenuRowErrors;
      ciGotoNextErrorWarning.MenuText = "Перейти к следующей строке с ош./пр.";
      ciGotoNextErrorWarning.ImageKey = "NextErrorRow";
      ciGotoNextErrorWarning.ShortCut = Keys.Control | Keys.OemCloseBrackets;
      ciGotoNextErrorWarning.ToolTipText = "Перейти к следующей строке с ошибкой или предупреждением";
      ciGotoNextErrorWarning.Click += new EventHandler(ciGotoNextErrorWarning_Click);
      Add(ciGotoNextErrorWarning);

      ciGotoPrevErrorOnly = new EFPCommandItem("View", "PrevRowError");
      ciGotoPrevErrorOnly.Parent = _MenuRowErrors;
      ciGotoPrevErrorOnly.MenuText = "Перейти к предыдущей строке с ошибкой";
      //ciGotoPrevErrorOnly.ImageKey = "PrevErrorRow";
      ciGotoPrevErrorOnly.ShortCut = Keys.Control | Keys.Shift | Keys.OemOpenBrackets;
      ciGotoPrevErrorOnly.ToolTipText = "Перейти к предыдущей строке с ошибкой";
      ciGotoPrevErrorOnly.Click += new EventHandler(ciGotoPrevErrorOnly_Click);
      Add(ciGotoPrevErrorOnly);

      ciGotoNextErrorOnly = new EFPCommandItem("View", "NextRowError");
      ciGotoNextErrorOnly.Parent = _MenuRowErrors;
      ciGotoNextErrorOnly.MenuText = "Перейти к следующей строке с ошибкой";
      //ciGotoNextErrorOnly.ImageKey = "NextErrorRow";
      ciGotoNextErrorOnly.ShortCut = Keys.Control | Keys.Shift | Keys.OemCloseBrackets;
      ciGotoNextErrorOnly.ToolTipText = "Перейти к следующей строке с ошибкой";
      ciGotoNextErrorOnly.GroupEnd = true;
      ciGotoNextErrorOnly.Click += new EventHandler(ciGotoNextErrorOnly_Click);
      Add(ciGotoNextErrorOnly);

      if (!EFPApp.EasyInterface)
      {
        ciCopyRowErrorMessages = new EFPCommandItem("Edit", "CopyError");
        ciCopyRowErrorMessages.Parent = _MenuRowErrors;
        ciCopyRowErrorMessages.MenuText = "Копировать сообщение в буфер обмена";
        ciCopyRowErrorMessages.Click += new EventHandler(ciCopyRowErrorMessages_Click);
        Add(ciCopyRowErrorMessages);
      }

      if (!EFPApp.EasyInterface)
      {
        ciShowRowErrorMessages = new EFPCommandItem("View", "ErrorList");
        ciShowRowErrorMessages.Parent = _MenuRowErrors;
        ciShowRowErrorMessages.MenuText = "Список сообщений";
        ciShowRowErrorMessages.Click += new EventHandler(ciShowRowErrorMessages_Click);
        Add(ciShowRowErrorMessages);

      }

      #endregion

      #region Отправить

      _MenuSendTo = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.MenuSendTo);
      _MenuSendTo.Usage = EFPCommandItemUsage.Menu;
      Add(_MenuSendTo);

      ciSendToMicrosoftExcel = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SendToMicrosoftExcel);
      ciSendToMicrosoftExcel.Parent = MenuSendTo;
      ciSendToMicrosoftExcel.Click += ciSendToMicrosoftExcel_Click;
      Add(ciSendToMicrosoftExcel);

      ciSendToOpenOfficeCalc = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SendToOpenOfficeCalc);
      ciSendToOpenOfficeCalc.Parent = MenuSendTo;
      ciSendToOpenOfficeCalc.Click += ciSendToOpenOfficeCalc_Click;
      Add(ciSendToOpenOfficeCalc);

      #endregion

      #region Окно суммы в статусной строке

      if (EFPApp.ShowAutoCalcSums)
      {
        ciStatCount = new EFPCommandItem("View", "StatusCount");
        ciStatCount.StatusBarText = "????";
        ciStatCount.MenuText = "Количество чисел";
        ciStatCount.ToolTipText = "Количество чисел в выбранных ячейках табличного просмотра" + Environment.NewLine +
          "Двойной щелчок мыши - копирование числа в буфер обмена";
        ciStatCount.Usage = EFPCommandItemUsage.StatusBar;
        ciStatCount.Click += new EventHandler(ciStatCount_Click);
        Add(ciStatCount);

        ciStatSumma = new EFPCommandItem("View", "StatusSum");
        ciStatSumma.StatusBarText = "??????????";
        //ciStatSumma.ImageKey = "Sum";
        ciStatSumma.MenuText = "Сумма";
        ciStatSumma.ToolTipText = "Сумма выбранных ячеек" + Environment.NewLine +
          "Двойной щелчок мыши - копирование суммы в буфер обмена";
        ciStatSumma.Usage = EFPCommandItemUsage.StatusBar;
        ciStatSumma.Click += new EventHandler(ciStatSumma_Click);
        Add(ciStatSumma);
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

    #endregion

    #region Управление наличием команд

    /// <summary>
    /// true, если нажатие клавиши Enter не обрабатывается, а передается
    /// форме для нажатия кнопки по умолчанию.
    /// false, если нажатие Enter выполняет редактирование ячейки таблицы
    /// По умолчанию - false
    /// </summary>
    public bool EnterAsOk
    {
      get
      {
        return _EnterAsOk;
      }
      set
      {
        CheckNotAssigned();
        _EnterAsOk = value;
      }
    }
    private bool _EnterAsOk;



    /// <summary>
    /// True, если есть команды "Редактировать", "Создать", "Удалить", "Просмотр"
    /// По умолчанию - true
    /// </summary>
    public bool UseEditView
    {
      get { return _UseEditView; }
      set
      {
        CheckNotAssigned();
        _UseEditView = value;
      }
    }
    private bool _UseEditView;

    /// <summary>
    /// True, если есть команда "Обновить" (F5)
    /// По умолчанию - true
    /// </summary>
    public bool UseRefresh
    {
      get { return _UseRefresh; }
      set
      {
        CheckNotAssigned();
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
        CheckNotAssigned();
        _UseSelectAll = value;
      }
    }
    private bool _UseSelectAll;

    /// <summary>
    /// Если установлено в true, то доступны команды ручной сортировки строк.
    /// Сортировка основывается на порядке строк в коллекции, а не на значении
    /// столбца. При сортировке строки переставляются местами внутри объекта
    /// DataGridViewRowCollection
    /// </summary>
    public bool ManualOrderRows
    {
      get { return _ManualOrderRows; }
      set
      {
        CheckNotAssigned();
        _ManualOrderRows = value;
      }
    }
    private bool _ManualOrderRows;

    /// <summary>
    /// Массив строк табличного просмотра в порядке по умолчанию. Свойство действует
    /// при ManualOrderRows=true. Если массив присвоен, то действует команда
    /// "Восстановить порядок по умолчанию"
    /// </summary>
    public DataGridViewRow[] DefaultManualOrderRows { get { return _DefaultManualOrderRows; } set { _DefaultManualOrderRows = value; } }
    private DataGridViewRow[] _DefaultManualOrderRows;

    /// <summary>
    /// Имя числового столбца, который определяет порядок строк при ручной сортировке
    /// Если задано, то в меню есть команды ручной сортировки
    /// </summary>
    public string ManualOrderColumn
    {
      get { return _ManualOrderColumn; }
      set
      {
        CheckNotAssigned();
        _ManualOrderColumn = value;

        if (!String.IsNullOrEmpty(value))
          Owner.AutoSort = false; // 21.07.2021
      }
    }
    private string _ManualOrderColumn;

    /// <summary>
    /// Имя числового столбца, который определяет порядок строк по умолчанию при ручной
    /// сортировке. Свойство действует при непустом значении ManualOrderColumn.
    /// Если имя присвоено, то действует команда "Восстановить порядок по умолчанию"
    /// </summary>
    public string DefaultManualOrderColumn
    {
      get { return _DefaultManualOrderColumn; }
      set
      {
        CheckNotAssigned();
        _DefaultManualOrderColumn = value;
      }
    }
    private string _DefaultManualOrderColumn;

    /// <summary>
    /// True (по умолчанию), если при установленном свойстве UseRowImages
    /// нужно выводит команды "Перейти к следующей / предыдущей строке с ошибками".
    /// Установите свойство в false, если картинки в заголовках строк используются
    /// для других целей, а не для показа ошибок.
    /// Свойство игнорируется, если UseRowImages=false
    /// </summary>
    public bool UseRowErrors
    {
      get { return _UseRowErrors; }
      set
      {
        CheckNotAssigned();
        _UseRowErrors = value;
      }
    }
    private bool _UseRowErrors;

    /// <summary>
    /// Если установлено (по умолчанию), а также установлены свойства UseRowErrors
    /// и UseRowImages, то в подменю "Строки с ошибками" есть команда
    /// "Показать список".
    /// Это свойство сбрасывается в false в самом просмотре списка ошибок EFPErrorDataGridView, т.к.
    /// рекурсивный вызов не имеет смысла
    /// </summary>
    public bool UseRowErrorsListView
    {
      get { return _UseRowErrorsListView; }
      set
      {
        CheckNotAssigned();
        _UseRowErrorsListView = value;
      }
    }
    private bool _UseRowErrorsListView;

    #endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер табличного просмотра.
    /// Задается в конструкторе
    /// </summary>
    public EFPDataGridView Owner { get { return _Owner; } }
    private EFPDataGridView _Owner;

    /// <summary>
    /// Инициализация EFPCommandItem.Usage перед инициализацией меню.
    /// </summary>
    protected override void BeforeControlAssigned()
    {
      base.BeforeControlAssigned();

      // Отключаем ненужные команды
      if (!UseEditView)
      {
        ciEdit.Usage = EFPCommandItemUsage.None;
        ciEdit2.Usage = EFPCommandItemUsage.None;
        ciInsert.Usage = EFPCommandItemUsage.None;
        ciInsertCopy.Usage = EFPCommandItemUsage.None;
        ciDelete.Usage = EFPCommandItemUsage.None;
        ciView.Usage = EFPCommandItemUsage.None;
      }

      if (UseRefresh)
        ciRefresh.Visible = Owner.HasRefreshDataHandler;
      else
        ciRefresh.Usage = EFPCommandItemUsage.None;

      EFPCommandItemUsage ClipboardUsage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
      if (ClipboardInToolBar)
        ClipboardUsage |= EFPCommandItemUsage.ToolBar;

      if (Cut == null && Owner.Control.ReadOnly)
      {
        ciCut.Enabled = false;
        ciCut.Usage = EFPCommandItemUsage.None;
      }
      else
        ciCut.Usage = ClipboardUsage;

      ciCopy.Usage = ClipboardUsage;


      // Добавляем форматы вставки текста после пользовательских форматов
      // (если уже не были добавлены явно)
      AddTextPasteFormats();

      _PasteHandler.InitCommandUsage(ClipboardInToolBar);
      _PasteHandler.PasteApplied += new EventHandler(PasteHandler_PasteApplied);

      Owner.Control.ReadOnlyChanged += new EventHandler(Control_ReadOnlyChanged); // 24.04.2019 - может переключаться динамически

      if (!UseSelectAll)
        ciSelectAll.Usage = EFPCommandItemUsage.None;

      if (String.IsNullOrEmpty(ManualOrderColumn) && (!ManualOrderRows))
      {
        _MenuSort.Usage = EFPCommandItemUsage.None;
        ciSortMoveDown.Usage = EFPCommandItemUsage.None;
        ciSortMoveUp.Usage = EFPCommandItemUsage.None;
        ciSortRestore.Usage = EFPCommandItemUsage.None;
      }
      else
      {
        ciSortMoveDown.Usage = EFPCommandItemUsage.Everywhere;
        ciSortMoveUp.Usage = EFPCommandItemUsage.Everywhere;
        if (ManualOrderRows && DefaultManualOrderRows != null)
          ciSortRestore.Usage = EFPCommandItemUsage.Everywhere;
        else
        {
          if ((!String.IsNullOrEmpty(ManualOrderColumn)) && (!String.IsNullOrEmpty(DefaultManualOrderColumn)))
            ciSortRestore.Usage = EFPCommandItemUsage.Everywhere;
          else
            ciSortRestore.Usage = EFPCommandItemUsage.None;
        }
      }

      if (Owner.OrderCount > 0 ||
        Owner.CustomOrderAllowed ||
        (Owner.GridProducer != null && Owner.GridProducer.OrderCount > 0 && Owner.UseGridProducerOrders))
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
        ciOrderCustom.MenuText = "&Произвольный";
        ciOrderCustom.ImageKey = "OrderCustom";
        ciOrderCustom.Parent = _MenuSort;
        ciOrderCustom.ShortCut = Keys.Control | Keys.D9;
        ciOrderCustom.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
        ciOrderCustom.Click += new EventHandler(ciOrderCustom_Click);
        Add(ciOrderCustom);


        ciOrderMore = new EFPCommandItem("View", "OrderDialog");
        ciOrderMore.MenuText = "&Еще ...";
        ciOrderMore.Parent = _MenuSort;
        ciOrderMore.ShortCut = Keys.Control | Keys.D0;
        ciOrderMore.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
        ciOrderMore.Click += new EventHandler(ciOrderMore_Click);
        Add(ciOrderMore);
      }
      InitOrderItems();

      if ((!Owner.UseRowImages) || (!UseRowErrors))
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
      }


      if (Owner.MarkRowsGridColumn == null)
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
        if (!Owner.Control.ShowCellToolTips)
          ciCopyToolTip.Usage = EFPCommandItemUsage.None;
      }

      if (!Owner.UseRowImages)
      {
        if (ciCopyRowErrorMessages != null)
          ciCopyRowErrorMessages.Usage = EFPCommandItemUsage.None;
      }

      if (Owner.TextSearchContext == null)
        ciFind.Usage = EFPCommandItemUsage.None;
    }

    /// <summary>
    /// Дополнительная инициализация команд
    /// </summary>
    protected override void AfterControlAssigned()
    {
      base.AfterControlAssigned();

      // Доназначаем обработчики и горячие клавиши в зависимости от свойства EnterAsOk
      if (EnterAsOk)
      {
        ciEdit.ShortCut = Keys.Control | Keys.Return;

        ciEdit2.ShortCut = Keys.Return;
      }
      else
      {
        ciEdit.ShortCut = Keys.Return;

        ciEdit2.ShortCut = Keys.Control | Keys.Return;
      }

      if (ciStatCount != null)
      {
        if (!(Owner.Control.MultiSelect && (Owner.Control.SelectionMode == DataGridViewSelectionMode.CellSelect ||
          Owner.Control.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect ||
          Owner.Control.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect)))
        {
          ciStatCount.Visible = false;
          ciStatSumma.Visible = false;
        }
      }

      Owner.Control.CellDoubleClick += new DataGridViewCellEventHandler(Grid_CellDoubleClick);
      Owner.Control.MouseDown += new MouseEventHandler(Grid_MouseDown);
      Owner.Control.MouseUp += new MouseEventHandler(Grid_MouseUp);
      Owner.Control_VisibleChanged(null, null);

      ciSendToMicrosoftExcel.Visible = EFPDataGridView.CanSendToMicrosoftExcel;
      ciSendToOpenOfficeCalc.Visible = EFPDataGridView.CanSendToOpenOfficeCalc;

      PerformRefreshItems();

      // 14.08.2012 Добавляем обработчики
      Owner.AfterControlAssigned();
    }

    #endregion

    #region Обновление состояния команд

    /// <summary>
    /// Вызывается при изменении текущей позиции в управляющем элементе или
    /// при вызове PerformRefreshItems()
    /// </summary>
    public event EventHandler RefreshItems;

    /// <summary>
    /// Обновление доступности команд локального меню после внешнего изменения
    /// выбранных ячеек просмотра
    /// </summary>
    public void PerformRefreshItems()
    {
      if (Owner == null)
        return;

      // Вызываем виртуальный метод
      DoRefreshItems();
      // Посылаем извещения
      if (RefreshItems != null)
        RefreshItems(this, EventArgs.Empty);
    }


    /// <summary>
    /// Обновление доступности команд локального меню после внешнего изменения
    /// выбранных ячеек просмотра
    /// </summary>
    protected virtual void DoRefreshItems()
    {
      EFPDataGridViewSelectedRowsState selState = Owner.SelectedRowsState;

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
        ciEdit.Visible = !Owner.ReadOnly;
        ciInsert.Visible = (!Owner.ReadOnly) && Owner.CanInsert;
        ciInsertCopy.Visible = (!Owner.ReadOnly) && Owner.CanInsertCopy;
        ciDelete.Visible = (!Owner.ReadOnly) && Owner.CanDelete;
        ciView.Visible = Owner.CanView;


        if (Owner.CanMultiEdit)
          ciEdit.Enabled = (selState != EFPDataGridViewSelectedRowsState.NoSelection);
        else
          ciEdit.Enabled = (selState == EFPDataGridViewSelectedRowsState.SingleRow);
        ciInsertCopy.Enabled = (selState == EFPDataGridViewSelectedRowsState.SingleRow);
        ciDelete.Enabled = (selState != EFPDataGridViewSelectedRowsState.NoSelection);

        ciView.Enabled = ciEdit.Enabled;
#endif

        if (selState == EFPDataGridViewSelectedRowsState.MultiRows)
        {
          if (!Owner.ReadOnly)
          {
            ciEdit.MenuText = "Редактировать выбранные записи";
            ciDelete.MenuText = "Удалить выбранные записи";
          }
          if (Owner.CanView)
            ciView.MenuText = "Просмотреть выбранные записи";
        }
        else
        {
          if (!Owner.ReadOnly)
          {
            ciEdit.MenuText = "Редактировать запись";
            ciDelete.MenuText = "Удалить запись";
          }
          if (Owner.CanView)
            ciView.MenuText = "Просмотреть запись";
        }

        if (ciCut.Usage != EFPCommandItemUsage.None)
          ciCut.Enabled = !(Owner.ReadOnly && Owner.Control.ReadOnly);
        if (!PasteHandler.AlwaysEnabled) // 27.11.2017
        {
          if (HasTextPasteFormats)
            PasteHandler.Enabled = !Owner.Control.ReadOnly; // 24.04.2019
          else
            PasteHandler.Enabled = !(Owner.ReadOnly && Owner.Control.ReadOnly);
        }
      }

      if (ciInlineEditStatus != null)
      {
        if (Owner.Control.ReadOnly)
        {
          ciInlineEditStatus.ImageKey = "TableInlineEditReadOnlyTable";
          ciInlineEditStatus.ToolTipText = "Просмотр не поддерживает редактирование по месту";
        }
        else if (Owner.Control.CurrentCell == null)
        {
          ciInlineEditStatus.ImageKey = "TableInlineEditTableReadOnly";
          ciInlineEditStatus.ToolTipText = "Нет выбранной ячейки";
        }
        else if (Owner.Control.IsCurrentCellInEditMode)
        {
          ciInlineEditStatus.ImageKey = "TableInlineEditProcess";
          ciInlineEditStatus.ToolTipText = "Выполняется редактирование ячейки";
        }
        else
        {
          string readOnlyMessage;
          if (Owner.GetCellReadOnly(Owner.Control.CurrentCell, out readOnlyMessage))
          {
            ciInlineEditStatus.ImageKey = "TableInlineEditReadOnlyCell";
            ciInlineEditStatus.ToolTipText = "Нельзя редактировать ячейку. " + readOnlyMessage;
          }
          else
          {
            ciInlineEditStatus.ImageKey = "TableInlineEdit";
            ciInlineEditStatus.ToolTipText = "Можно начать редактирование ячейки";
          }
        }
      }

      if (ciSelectAll != null)
      {
        ciSelectAll.Enabled = (selState != EFPDataGridViewSelectedRowsState.NoSelection) && Owner.Control.MultiSelect;
      }

      RefreshIncSearchItems();


      RefreshStatItems();
    }

    internal void RefreshIncSearchItems()
    {
      if (ciIncSearch == null)
        return;

      bool isEnabled;
      string menuText;
      string statusBarText;
      bool isChecked = false;

      if (Owner.CurrentIncSearchColumn != null)
      {
        // Поиск по буквам выполняется
        isEnabled = true;
        menuText = "Закончить поиск по буквам";
        string s = Owner.CurrentIncSearchChars;
        s = s.Replace(' ', (char)(0x00B7));
        s = s.PadRight(20);
        statusBarText = s.ToUpper();
        isChecked = true;
      }
      else
      {
        // Поиск по буквам не выполняется
        if (Owner.CanIncSearch)
        {
          menuText = "Начать поиск по буквам";
          if (Owner.CurrentColumn == null)
          {
            isEnabled = false;
            statusBarText = "<Столбец не выбран>";
          }
          else
          {
            isEnabled = Owner.CurrentColumn.CanIncSearch;
            if (isEnabled)
              statusBarText = "<Поиск не начат>";
            else
              statusBarText = "<Неподходящий столбец>";
          }
        }
        else
        {
          menuText = "Поиск по буквам";
          isEnabled = false;
          statusBarText = "<Поиск невозможен>";
        }
      }

      ciIncSearch.MenuText = menuText;
      ciIncSearch.Enabled = isEnabled;
      ciIncSearch.StatusBarText = statusBarText;
      ciIncSearch.Checked = isChecked;
      ciIncSearch.ToolTipText = menuText;

      if (Owner.CurrentIncSearchColumn == null)
      {
        if (Owner.TextSearchContext == null)
          ciFindNext.Enabled = false;
        else
          ciFindNext.Enabled = Owner.TextSearchContext.ContinueEnabled;
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
        _Owner.CurrentIncSearchColumn = null; // 28.01.2021
    }

    #endregion

    #region Команды редактирования

    private EFPCommandItem ciEdit, ciEdit2, ciInsert, ciInsertCopy, ciDelete, ciView;

    /// <summary>
    /// Доступ к командам редактирования записей
    /// </summary>
    /// <param name="state">Состояние, инициируемое командой меню</param>
    /// <returns>Команда меню</returns>
    public EFPCommandItem this[EFPDataGridViewState state]
    {
      get
      {
        switch (state)
        {
          case EFPDataGridViewState.Edit: return ciEdit;
          case EFPDataGridViewState.Insert: return ciInsert;
          case EFPDataGridViewState.InsertCopy: return ciInsertCopy;
          case EFPDataGridViewState.Delete: return ciDelete;
          case EFPDataGridViewState.View: return ciView;
          default:
            throw new ArgumentException();
        }
      }
    }

    /// <summary>
    /// Панелька со значком для inline-редактирования
    /// </summary>
    private EFPCommandItem ciInlineEditStatus;

    private void ClickOKButton(object sender, EventArgs args)
    {
      // Нажимаем в блоке диалога кнопку по умолчанию
      Form frm = Owner.Control.FindForm();
      if (frm.AcceptButton == null)
        return;
      frm.AcceptButton.PerformClick();
    }

    /// <summary>
    /// Двойной щелчок мыши на ячейке
    /// Если обрабатывать только это событие, а редактор запусакется медленно, то
    /// сетка переходит в режим выделения строк. Пользователь успевает нечаянно
    /// выделить несколько строк, пока открывается редактор. Это некрасиво.
    /// Поэтому откладываем запуск редактора до события MouseUp
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs args)
    {
      if (args.RowIndex < 0)
        return;
      if (args.ColumnIndex < 0 && (!EnterAsOk) /* добавлено 31.10.2018 */)
        return;
      _CellDoubleClicked = true;
    }

    private bool _CellDoubleClicked = false;

    void Grid_MouseUp(object sender, MouseEventArgs args)
    {
      if (_CellDoubleClicked)
      {
        _CellDoubleClicked = false;

        if (EnterAsOk)
        {
          // 16.08.2012
          // Если текущая ячейка допускает inline-редактирование, то нажимать кнопку
          // по умолчанию - неправильно
          if (Owner.Control.CurrentCell != null)
          {
            string ReadOnlyMessage;
            if (!Owner.GetCellReadOnly(Owner.Control.CurrentCell, out ReadOnlyMessage))
              return;
          }

          ClickOKButton(null, null);
        }
        else
        {
          if ((!Owner.ReadOnly) || Owner.CanView /*|| (!Handler.MainGrid.ReadOnly)*/)
          {
            ciEdit_Click(null, null);
          }
        }
      }
    }

    void Grid_MouseDown(object sender, MouseEventArgs args)
    {
      _CellDoubleClicked = false;
    }



    private void ciEdit_Click(object sender, EventArgs args)
    {
      try
      {
        Owner.PerformEditData(Owner.ReadOnly ? EFPDataGridViewState.View : EFPDataGridViewState.Edit);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка при " + (Owner.ReadOnly ? "просмотре" : "редактировании") + " данных");
      }
    }

    private void ciInsert_Click(object sender, EventArgs args)
    {
      try
      {
        Owner.PerformEditData(EFPDataGridViewState.Insert);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка при добавлении строки данных");
      }

    }

    private void ciInsertCopy_Click(object sender, EventArgs args)
    {
      try
      {
        Owner.PerformEditData(EFPDataGridViewState.InsertCopy);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка при создании копии строки данных");
      }

    }

    private void ciDelete_Click(object sender, EventArgs args)
    {
      try
      {
        Owner.PerformEditData(EFPDataGridViewState.Delete);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка при удалении данных");
      }
    }

    private void ciView_Click(object sender, EventArgs args)
    {
      try
      {
        Owner.PerformEditData(EFPDataGridViewState.View);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка при просмотре данных");
      }
    }

    void ciInlineEditStatus_Click(object sender, EventArgs args)
    {
      if (Owner.Control.IsCurrentCellInEditMode)
        Owner.Control.EndEdit();
      else
        Owner.Control.BeginEdit(false);
    }

    #endregion

    #region Буфер обмена

    /// <summary>
    /// Нужно ли показывать кнопки "Вырезать", "Копировать" и "Вставить" в панели
    /// инструментов (если она есть).
    /// По умолчанию - false (только в меню и горячие клавиши)
    /// </summary>
    public bool ClipboardInToolBar
    {
      get { return _ClipboardInToolBar; }
      set
      {
        CheckNotAssigned();
        _ClipboardInToolBar = value;
      }
    }
    private bool _ClipboardInToolBar;

    #region Вырезать

    private EFPCommandItem ciCut;

    /// <summary>
    /// Если обработчик установлен, то в локальное меню добавляется команда "Вырезать"
    /// Если обработчик не установлен, то поддерживается вырезка текста ячеек.
    /// При необходимости обработчик Cut может вызывать метод PerformCutText() или
    /// TryPerformCutText()
    /// </summary>
    public event EventHandler Cut;

    private void DoCut(object sender, EventArgs args)
    {
      //*** Owner.CurrentIncSearchColumn = null;

      if (Cut != null)
        Cut(this, EventArgs.Empty);
      else
        PerformCutText();

      PerformRefreshItems();
    }

    /// <summary>
    /// Вырезать текст из выбранных ячеек таблицы в буфер обмена.
    /// Вызывает метод TryPerformCutText() и, в случае ошибки, выводит сообщение
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
      if (Owner.Control.ReadOnly)
      {
        errorText = "Просмотр не допускает редактирование ячеек";
        return false;
      }

      EFPDataGridViewRectArea selArea = new EFPDataGridViewRectArea(Owner.Control);
      if (selArea.IsEmpty)
      {
        errorText = "Нет выбранных ячеек";
        return false;
      }

      if (!selArea.AreAllCellsSelected)
      {
        errorText = "Должна быть выбрана прямоугольная область ячеек";
        return false;
      }

      // Проверяем, что мы можем записать пустой текст во все ячейки
      for (int i = 0; i < selArea.RowCount; i++)
      {
        for (int j = 0; j < selArea.ColumnCount; j++)
        {
          DataGridViewCell cell = selArea[j, i];
          if (Owner.GetCellReadOnly(cell, out errorText))
            return false;

          if (!Owner.TrySetTextValue(cell, String.Empty, out errorText, true, EFPDataGridViewCellFinishedReason.Clear))
            return false;
        }
      }

      // Копируем все
      if (!PerformCopy())
      {
        errorText = "Ошибка при выполнении копирования ячеек";
        return false;
      }

      // Очищаем ячейки
      for (int i = 0; i < selArea.Rows.Count; i++)
      {
        for (int j = 0; j < selArea.Columns.Count; j++)
        {
          DataGridViewCell cell = selArea[j, i];
          Owner.SetTextValue(cell, String.Empty, EFPDataGridViewCellFinishedReason.Clear);
        }
      }

      errorText = null;
      return true;
    }

    #endregion

    #region Копировать

    private EFPCommandItem ciCopy;

    private void DoCopy(object sender, EventArgs args)
    {
      //*** Owner.CurrentIncSearchColumn = null;
      PerformCopy();
    }

    /// <summary>
    /// Стандартные форматы копирования в буфер обмена
    /// По умолчанию: Text, CSV и HTML
    /// Можно отключить стандартные форматы копирования, если необходимо копировать данные в нестандартном формате.
    /// Тогда эти форматы можно добавить в обработчике AddCopyFormats
    /// </summary>
    public EFPDataGridViewCopyFormats CopyFormats
    {
      get { return _CopyFormats; }
      set
      {
        CheckNotAssigned();
        _CopyFormats = value;
      }
    }
    private EFPDataGridViewCopyFormats _CopyFormats;

    /// <summary>
    /// Обработчик может добавить при копировании в буфер обмена дополнительные форматы
    /// </summary>
    public event DataObjectEventHandler AddCopyFormats;

    /// <summary>
    /// Вызывается при выполнении команды "Копировать".
    /// Непереопределенный метод вызывает событие AddCopyFormats.
    /// Переопределенный метод может добавить дополнительные форматы.
    /// Стандартные форматы (TEXT, CSV, HTML) уже добавлены на момент вызова, в зависимости от свойства CopyFormats.
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnAddCopyFormats(DataObjectEventArgs args)
    {
      if (AddCopyFormats != null)
        AddCopyFormats(this, args);
    }

    /// <summary>
    /// Выполнить копирование выделенных ячеек табличного просмотра в буфер обмена.
    /// В случае ошибки выдает сообщение на экран и возвращает false
    /// Метод может использоваться внутри реализации обработчика Cut
    /// </summary>
    /// <returns>true, если копирование успешно выполнено. false - в случае ошибки</returns>
    public bool PerformCopy()
    {
      try
      {
        if (Owner.Control.GetCellCount(DataGridViewElementStates.Selected) <= 0)
        {
          EFPApp.ShowTempMessage("В просмотре нет выбранных ячеек");
          return false;
        }

        EFPApp.BeginWait("Копирование ячеек в буфер обмена", "Copy");
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

          string txt;
          DataObject dobj2 = new DataObject();
          //dobj2.SetText(txt, TextDataFormat.UnicodeText);

          if ((CopyFormats & (EFPDataGridViewCopyFormats.Text | EFPDataGridViewCopyFormats.CSV)) != 0)
          {
            EFPDataGridViewRectArea area = Owner.GetRectArea(EFPDataGridViewExpRange.Selected);
            string[,] a = Owner.GetCellTextValues(area);

            if ((CopyFormats & EFPDataGridViewCopyFormats.Text) == EFPDataGridViewCopyFormats.Text)
            {
              txt = new TabTextConvert().ToString(a);
              dobj2.SetData(DataFormats.Text, true, txt);
            }

            if ((CopyFormats & EFPDataGridViewCopyFormats.CSV) == EFPDataGridViewCopyFormats.CSV)
            {
              txt = new CsvTextConvert().ToString(a);
              if (!String.IsNullOrEmpty(txt))
                dobj2.SetText(txt, TextDataFormat.CommaSeparatedValue);
            }
          }

          if ((CopyFormats & EFPDataGridViewCopyFormats.HTML) == EFPDataGridViewCopyFormats.HTML)
          {
            // HTML-формат собираем сами
            EFPDataGridViewExpHtmlSettings settings = new EFPDataGridViewExpHtmlSettings();
            settings.RangeMode = EFPDataGridViewExpRange.Selected;
            //if (Owner.Control.SelectionMode == DataGridViewSelectionMode.FullColumnSelect ||
            //  Owner.Control.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect)
            //  Settings.ShowColumnHeaders = true;
            //else
            //  Settings.ShowColumnHeaders = Owner.Control.AreAllCellsSelected(false);
            settings.ShowColumnHeaders = false; // 01.10.2020
            settings.Encoding = Encoding.UTF8;
            byte[] buffer = EFPDataGridViewExpHtml.GetHtmlBytes(Owner, settings, true);
            //System.IO.File.WriteAllBytes(@"d:\temp\table1.html", Buffer);
            MemoryStream strm = new MemoryStream(buffer);
            dobj2.SetData(DataFormats.Html, false, strm);
          }

          DataObjectEventArgs args = new DataObjectEventArgs(dobj2);
          OnAddCopyFormats(args);

          EFPApp.Clipboard.SetDataObject(dobj2, true);
        }
        finally
        {
          EFPApp.EndWait();
        }
        return true;
      }
      catch (Exception e)
      {
        EFPApp.MessageBox(e.Message, "Ошибка при копировании в буфер обмена",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
    }

    #endregion

    #region Копировать подсказку

    internal EFPCommandItem ciCopyToolTip;

    private void DoCopyToolTip(object sender, EventArgs args)
    {
      //*** Owner.CurrentIncSearchColumn = null;

      string text;
      if (Owner.Control.SelectedRows.Count > 0)
      {
        if (Owner.Control.SelectedRows.Count != 1)
        {
          EFPApp.ShowTempMessage("Должно быть выбрано не более одной строки");
          return;
        }

        text = Owner.GetRowToolTipText(Owner.Control.SelectedRows[0].Index);
        if (String.IsNullOrEmpty(text))
        {
          EFPApp.ShowTempMessage("Нет всплывающей подсказки для заголовка строки");
          return;
        }
      }
      else
      {
        if (Owner.Control.SelectedCells.Count > 1)
        {
          EFPApp.ShowTempMessage("Должно быть выбрано не более одной ячейки");
          return;
        }
        if (Owner.Control.CurrentCell == null)
        {
          EFPApp.ShowTempMessage("Нет выбранной ячейки");
          return;
        }

        text = Owner.GetCellToolTipText(Owner.Control.CurrentCell.RowIndex,
          Owner.Control.CurrentCell.ColumnIndex);
        if (String.IsNullOrEmpty(text))
        {
          EFPApp.ShowTempMessage("Нет всплывающей подсказки для выбранной ячейки");
          return;
        }
      }

      EFPApp.Clipboard.SetText(text);
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
      PerformRefreshItems();
    }

    /// <summary>
    /// Добавить форматы вставки текста из буфера обмена.
    /// Если метод не вызван явно при инициализации просмотра, он вызывается
    /// автоматически перед показом. Допускается многократный вызов, повторные
    /// вызовы игонируются. Возвращается массив объектов EFPPasteTextMatrixFormat.
    /// Команды не добавляется и возвращается пустой массив, если просмотр не
    /// поддерживает inline-редактирование
    /// </summary>
    /// <returns>Массив описателей форматов</returns>
    public EFPPasteTextMatrixFormat[] AddTextPasteFormats()
    {
      if (_TextPasteFormats != null)
        return _TextPasteFormats; // повторный вызов

      if (!Owner.Control.ReadOnly)
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
      if (!args.Appliable)
        return;

      if (Owner.Control.ReadOnly)
      {
        args.Appliable = false;
        args.DataInfoText = "Просмотр не поддерживает редактирование \"по месту\"";
        return;
      }

      if (Owner.Control.CurrentCell == null)
      {
        args.Appliable = false;
        args.DataInfoText = "В просмотре нет выбранной ячейки";
        return;
      }

      string errorText;
      //if (Owner.GetCellReadOnly(Owner.Control.CurrentCell, out ErrorText))
      if (!Owner.TestCanPasteText(fmt.TextMatrix, out errorText)) // 24.04.2019
      {
        args.Appliable = false;
        args.DataInfoText = errorText;
        return;
      }
    }

    void fmtTextOrCSV_Paste(object sender, EFPPasteDataObjectEventArgs args)
    {
      EFPPasteTextMatrixFormat fmt = (EFPPasteTextMatrixFormat)sender;

      Owner.PerformPasteText(fmt.TextMatrix);
    }



    void Control_ReadOnlyChanged(object sender, EventArgs args)
    {
      PerformRefreshItems();
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
      if (Owner.CheckMarkRows(rows, action) == 0)
        EFPApp.ShowTempMessage("Нет изменившихся отметок строк");
    }


    #endregion

    #region Команды поиска

    EFPCommandItem ciIncSearch, ciFind, ciFindNext;

    private void IncSearch(object sender, EventArgs args)
    {
      // Начать / закончить поиск по первым буквам
      if (Owner.CurrentIncSearchColumn == null)
      {
        if (Owner.CurrentColumn == null)
        {
          EFPApp.ShowTempMessage("Столбец не выбран");
          return;
        }
        if (!Owner.CurrentColumn.CanIncSearch)
        {
          EFPApp.ShowTempMessage("Текущий столбец не поддерживает поиск по первым буквам");
          return;
        }
        Owner.CurrentIncSearchColumn = Owner.CurrentColumn;
      }
      else
      {
        Owner.CurrentIncSearchColumn = null;
      }
    }

    private void Find(object sender, EventArgs args)
    {
      if (Owner.TextSearchContext != null) // 27.12.2020
        Owner.TextSearchContext.StartSearch();
    }

    private void FindNext(object sender, EventArgs args)
    {
      if (Owner.CurrentIncSearchColumn == null)
      {
        if (Owner.TextSearchContext != null)
          Owner.TextSearchContext.ContinueSearch();
      }
      else
        if (!Owner.CurrentIncSearchColumn.PerformIncSearch(Owner.CurrentIncSearchChars.ToUpper(), true))
          EFPApp.ShowTempMessage("Нет больше строк, в которых значение поля начинается с \"" +
            Owner.CurrentIncSearchChars + "\"");
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
    /// Команда "Еще" для дополнительных порядков сортировки (больше 9)
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

      int n = Owner.OrderCount;
      for (int i = 0; i < _OrderItems.Length; i++)
      {
        _OrderItems[i].Visible = (i < n);
        if (i < n)
        {
          _OrderItems[i].MenuText = (i + 1).ToString() + ". " + Owner.Orders[i].DisplayName;
          _OrderItems[i].ImageKey = Owner.Orders[i].ImageKey;
        }
      }

      ciOrderCustom.Visible = (n > 0) && Owner.CustomOrderAllowed;
      ciOrderMore.Visible = (n > _OrderItems.Length) || Owner.CustomOrderAllowed;
      _MenuSort.Enabled = (n > 0) || Owner.CustomOrderAllowed;
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
          _OrderItems[i].Checked = (Owner.CurrentOrderIndex == i);
        ciOrderCustom.Checked = Owner.CustomOrderActive;
      }

      if (Owner.CustomOrderActive)
        _MenuSort.ImageKey = "OrderCustom";
      else if (Owner.CurrentOrder == null)
        _MenuSort.ImageKey = "OrderAZ"; // по идее, не должно быть
      else if (Owner.CurrentOrder.SortInfo.Direction == ListSortDirection.Ascending)
        _MenuSort.ImageKey = "OrderAZ";
      else
        _MenuSort.ImageKey = "OrderZA";

      if (_MenuSort.Enabled)
      {
        string s;
        if (Owner.CurrentOrder == null)
          s = "Не задан";
        else
          s = Owner.CurrentOrder.DisplayName;
        _MenuSort.MenuRightText = s;
        _MenuSort.ToolTipText = "Порядок строк (" + s + ")";
      }
      else
      {
        _MenuSort.MenuRightText = String.Empty;
        _MenuSort.ToolTipText = "Порядок строк";
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
      int Order = (int)(ci.Tag);
      if (Order < Owner.OrderCount)
        Owner.CurrentOrderIndex = Order;
    }

    /// <summary>
    /// Выбор произвольной сортировки
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void ciOrderCustom_Click(object sender, EventArgs args)
    {
      if (!Owner.CustomOrderAllowed)
        return;

      if (Owner.OrderCount == 0)
        return;

      Owner.CustomOrderActive = !Owner.CustomOrderActive;
    }

    /// <summary>
    /// Выбор порядка строк из полного списка (окно диалога)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void ciOrderMore_Click(object sender, EventArgs args)
    {
      Owner.ShowSelectOrderDialog();
    }

    #endregion

    #region Ручная сортировка с помощью стрелочек

    internal EFPCommandItem ciSortMoveUp, ciSortMoveDown, ciSortRestore;

    void ciSortMoveUp_Click(object sender, EventArgs args)
    {
      DoReorder(false);
    }

    void ciSortMoveDown_Click(object sender, EventArgs args)
    {
      DoReorder(true);
    }

    private void DoReorder(bool down)
    {
      if (!Owner.Control.EndEdit())
      {
        EFPApp.ErrorMessageBox("Редактирование не закончено");
        return;
      }

      int oldColIdx = Owner.CurrentColumnIndex;

      bool changed;
      if (ManualOrderRows)
        changed = DoReorderByGridRows(down);
      else
        changed = DoReorderByDataColumn(down);

      // 9. Обновляем табличный просмотр
      if (changed)
      {
        Owner.CurrentColumnIndex = oldColIdx;

        if (ManualOrderChanged != null)
          ManualOrderChanged(this, EventArgs.Empty);
      }
    }

#if USE_PROPS

    /// <summary>
    /// Изменение порядка строк на основании числового поля
    /// </summary>
    /// <param name="down">true - сдвиг вниз, false -  сдвиг вверх</param>
    /// <returns>true, если перемещение было выполнено</returns>
    private bool DoReorderByDataColumn(bool down)
    {
      if (String.IsNullOrEmpty(ManualOrderColumn))
        throw new NullReferenceException("Свойство ManualOrderColumn не установлено");

      System.Collections.IList lst = ListBindingHelper.GetList(Owner.Control.DataSource, Owner.Control.DataMember) as IBindingList;
      if (lst == null)
        throw new InvalidOperationException("Источник данных не реализует IList");

      PropertyDescriptorCollection pdc = ListBindingHelper.GetListItemProperties(Owner.Control.DataSource, Owner.Control.DataMember, null);
      if (pdc == null)
        throw new InvalidOperationException("Источник данных не возвращает PropertyDescriptorCollection");
      PropertyDescriptor pdOrder = pdc[ManualOrderColumn];
      if (pdOrder == null)
        throw new InvalidOperationException("В источнике данных не найден дескриптор свойства \"" + ManualOrderColumn + "\"");

      // 1. Загружаем полный список строк DataRow в массив
      object[] Rows1 = new object[lst.Count];
      lst.CopyTo(Rows1, 0);

      // 2. Загружаем позиции выбранных строк 
      int[] SelPoss = Owner.SelectedRowIndices;
      if (SelPoss.Length == 0)
      {
        EFPApp.ShowTempMessage("Нет выбранных строк");
        return false;
      }
      Array.Sort<int>(SelPoss);
#if DEBUG
      CheckSelectedRowIndices(SelPoss);
#endif

      // 3. Проверяем, что не уперлись в границы списка
      bool lBound = false;
      if (down)
      {
        if (SelPoss[SelPoss.Length - 1] == Rows1.Length - 1)
          lBound = true;
      }
      else
      {
        if (SelPoss[0] == 0)
          lBound = true;
      }
      if (lBound)
      {
        string msg = "Нельзя передвинуть ";
        if (SelPoss.Length > 1)
          msg += "выбранные строки ";
        else
          msg += "выбранную строку ";
        if (down)
          msg += "вниз";
        else
          msg += "вверх";
        EFPApp.ShowTempMessage(msg);
        return false;
      }

      // 4. Подготавливаем массив строк для их размещения в новом порядке
      // Значения null в этом массиве означают временно пустые позиции
      object[] Rows2 = new object[Rows1.Length];

      // 5. Копируем в Rows2 строки из Rows1 со сдвигом для позиций, существующих
      // в SelRows.
      // В процессе перемещения будем очищать массив Rows1
      int Delta = down ? 1 : -1; // значение смещения
      int[] SelPoss2 = new int[SelPoss.Length];

      for (int i = 0; i < SelPoss.Length; i++)
      {
        int ThisPos = SelPoss[i];
        Rows2[ThisPos + Delta] = Rows1[ThisPos];
        Rows1[ThisPos] = null;

        SelPoss2[i] = ThisPos + Delta; // ???
      }
#if DEBUG
      CheckSelectedRowIndices(SelPoss2);
#endif

      // 6. Перебираем исходный массив и оставшиеся непустые строки размещаем в
      // новом массиве, отыскивая пустые места. Для этого используем переменную FreePos
      // для указания на очередную пустую позицию второго массива
      int FreePos = 0;
      for (int i = 0; i < Rows1.Length; i++)
      {
        if (Rows1[i] == null) // перемещенная позиция
          continue;
        // Поиск места
        while (Rows2[FreePos] != null)
          FreePos++;
        // Нашли дырку
        Rows2[FreePos] = Rows1[i];
        FreePos++;
      }

      // 7. Записываем номера строк в поле согласно новому порядку в Rows2
      ISupportInitialize isi = null; // lst as ISupportInitialize;
      bool Changed = false;
      bool InitStarted = false;
      try
      {
        for (int i = 0; i < Rows2.Length; i++)
        {
          if (DataTools.GetInt(pdOrder.GetValue(Rows2[i])) != (i + 1))
          {
            if (!Changed)
            {
              Changed = true;
              if (isi != null)
              {
                isi.BeginInit();
                InitStarted = true;
              }
            }
            pdOrder.SetValue(Rows2[i], i + 1);
          }
        }
      }
      finally
      {
        if (InitStarted)
          isi.EndInit();
      }

      // 9. Обновляем просмотр
      if (Changed)
      {
        Owner.Control.Invalidate();
        Owner.SelectedRowIndices = SelPoss2; 
      }

      return Changed;
    }

#if DEBUG
    private void CheckSelectedRowIndices(int[] selPoss)
    {
      if (selPoss[0] < 0)
        throw new BugException("SelectedRowIndex<0");
      if (selPoss[selPoss.Length - 1] >= Owner.Control.RowCount)
        throw new BugException("SelectedRowIndex>=RowCount");
      for (int i = 1; i < selPoss.Length; i++)
      {
        if (selPoss[i] == selPoss[i - 1])
          throw new BugException("SelectedRowIndex duplicated");
      }
    }
#endif

#else

    /// <summary>
    /// Изменение порядка строк на основании числового поля
    /// </summary>
    /// <param name="down">true - сдвиг вниз, false -  сдвиг вверх</param>
    /// <returns>true, если перемещение было выполнено</returns>
    private bool DoReorderByDataColumn(bool down)
    {
      int i;

      // Алгоритм взят из программы BAVAMBUX (модуль SERVER\iGridReorder.pas)
      // Упрощен, т.к. массивы строк DataRow могут содержать значение null,
      // в отличие от оригинала

      // Получаем доступ к объекту DataView
      DataView dv = Owner.SourceAsDataView;
      if (dv == null)
        throw new InvalidDataSourceException("Нельзя получить DataView");

      // 1. Загружаем полный список строк DataRow в массив
      DataRow[] rows1 = DataTools.GetDataViewRows(dv);

      // 2. Загружаем выбранные строки
      DataRow[] selRows = Owner.SelectedDataRows;
      if (selRows.Length == 0)
      {
        EFPApp.ShowTempMessage("Нет ни одной выбранной строки, которую надо перемещать");
        return false;
      }

      // 3. Получаем позиции выбранных строк в массиве всех строк
      int[] selPoss = new int[selRows.Length];
      for (i = 0; i < selRows.Length; i++)
        selPoss[i] = Array.IndexOf<DataRow>(rows1, selRows[i]);

      // 4. Проверяем, что не уперлись в границы списка
      bool lBound = false;
      if (down)
      {
        if (selPoss[selPoss.Length - 1] == rows1.Length - 1)
          lBound = true;
      }
      else
      {
        if (selPoss[0] == 0)
          lBound = true;
      }
      if (lBound)
      {
        string msg = "Нельзя передвинуть ";
        if (selRows.Length > 1)
          msg += "выбранные строки ";
        else
          msg += "выбранную строку ";
        if (down)
          msg += "вниз";
        else
          msg += "вверх";
        EFPApp.ShowTempMessage(msg);
        return false;
      }

      // 5. Подготавливаем массив строк для их размещения в новом порядке
      // Значения null в этом массиве означают временно пустые позиции
      DataRow[] rows2 = new DataRow[rows1.Length];

      // 6. Копируем в Rows2 строки из Rows1 со сдвигом для позиций, существующих
      // в SelRows.
      // В процессе перемещения будем очищать массив Rows1
      int delta = down ? 1 : -1; // значение смещения
      for (i = 0; i < selPoss.Length; i++)
      {
        int thisPos = selPoss[i];
        rows2[thisPos + delta] = rows1[thisPos];
        rows1[thisPos] = null;
      }

      // 7. Перебираем исходный массив и оставшиеся непустые строки размещаем в
      // новом массиве, отыскивая пустые места. Для этого используем переменную FreePos
      // для указания на очередную пустую позицию второго массива
      int freePos = 0;
      for (i = 0; i < rows1.Length; i++)
      {
        if (rows1[i] == null) // перемещенная позиция
          continue;
        // Поиск места
        while (rows2[freePos] != null)
          freePos++;
        // Нашли дырку
        rows2[freePos] = rows1[i];
        freePos++;
      }

      // 8. Записываем номера строк в поле согласно новому порядку в Rows2
      bool changed = false;
      for (i = 0; i < rows2.Length; i++)
      {
        if (DataTools.GetInt(rows2[i], ManualOrderColumn) != (i + 1))
        {
          rows2[i][ManualOrderColumn] = i + 1;
          changed = true;
        }
      }

      // 9. Обновляем просмотр
      if (changed)
      {
        Owner.Control.Invalidate();
        Owner.SelectedDataRows = selRows; // 26.10.2017
      }
      //if (Owner.Control.CurrentCell != null)
      //{
      //  if (Owner.Control.CurrentCell.IsInEditMode)
      //  {
      //    DataGridViewCell Cell = Owner.Control.CurrentCell;
      //    Owner.Control.CurrentCell = null;
      //    Owner.Control.CurrentCell = Cell;
      //  }
      //}

      return changed;
    }

#endif

    /// <summary>
    /// Изменение порядка строк на основании их расположения в коллекции
    /// </summary>
    /// <param name="down">true - сдвиг вниз, false -  сдвиг вверх</param>
    /// <returns>true, если перемещение было выполнено</returns>
    private bool DoReorderByGridRows(bool down)
    {
      // 1. Загружаем полный список строк DataGridViewRow в массив
      DataGridViewRow[] rows1 = new DataGridViewRow[Owner.Control.Rows.Count];
      Owner.Control.Rows.CopyTo(rows1, 0);

      // 2. Запоминаем выбранные строки
      DataGridViewRow[] selRows = Owner.SelectedGridRows;
      // 3. Получаем позиции выбранных строк в массиве всех строк
      int[] selPoss = Owner.SelectedRowIndices;
      if (selPoss.Length == 0)
      {
        EFPApp.ShowTempMessage("Нет ни одной выбранной строки, которую надо перемещать");
        return false;
      }

      // 4. Проверяем, что не уперлись в границы списка
      bool lBound = false;
      if (down)
      {
        if (selPoss[selPoss.Length - 1] == rows1.Length - 1)
          lBound = true;
      }
      else
      {
        if (selPoss[0] == 0)
          lBound = true;
      }
      if (lBound)
      {
        string msg = "Нельзя передвинуть ";
        if (selPoss.Length > 1)
          msg += "выбранные строки ";
        else
          msg += "выбранную строку ";
        if (down)
          msg += "вниз";
        else
          msg += "вверх";
        EFPApp.ShowTempMessage(msg);
        return false;
      }

      // 5. Подготавливаем массив строк для их размещения в новом порядке
      // Значения null в этом массиве означают временно пустые позиции
      DataGridViewRow[] rows2 = new DataGridViewRow[rows1.Length];

      // 6. Копируем в Rows2 строки из Rows1 со сдвигом для позиций, существующих
      // в SelRows.
      // В процессе перемещения будем очищать массив Rows1
      int delta = down ? 1 : -1; // значение смещения
      int i;
      for (i = 0; i < selPoss.Length; i++)
      {
        int thisPos = selPoss[i];
        rows2[thisPos + delta] = rows1[thisPos];
        rows1[thisPos] = null;
      }

      // 7. Перебираем исходный массив и оставшиеся непустые строки размещаем в
      // новом массиве, отыскивая пустые места. Для этого используем переменную FreePos
      // для указания на очередную пустую позицию второго массива
      int freePos = 0;
      for (i = 0; i < rows1.Length; i++)
      {
        if (rows1[i] == null) // перемещенная позиция
          continue;
        // Поиск места
        while (rows2[freePos] != null)
          freePos++;
        // Нашли дырку
        rows2[freePos] = rows1[i];
        freePos++;
      }

      // 8. Замещаем коллекцию строк
      Owner.Control.Rows.Clear();
      Owner.Control.Rows.AddRange(rows2);

      // 9. Восстанавливаем выбор
      Owner.SelectedGridRows = selRows;
      return true;
    }

    void ciSortRestore_Click(object sender, EventArgs args)
    {
      int oldColIdx = Owner.CurrentColumnIndex;

      bool changed;
      if (ManualOrderRows)
        changed = DoSortRestoreRows();
      else
        changed = DoSortRestoreColumn();

      // Обновляем табличный просмотр
      if (changed)
      {
        Owner.CurrentColumnIndex = oldColIdx;

        if (ManualOrderChanged != null)
          ManualOrderChanged(this, EventArgs.Empty);
      }
    }

#if USE_PROPS
    /// <summary>
    /// Восстановление порядка по умолчанию на основании DefaultManualOrderColumn
    /// </summary>
    /// <returns></returns>
    private bool DoSortRestoreColumn()
    {
      if (String.IsNullOrEmpty(ManualOrderColumn))
        throw new NullReferenceException("Свойство ManualOrderColumn не установлено");
      if (String.IsNullOrEmpty(DefaultManualOrderColumn))
        throw new NullReferenceException("Свойство DefaultManualOrderColumn не установлено");

      System.Collections.IList lst = ListBindingHelper.GetList(Owner.Control.DataSource, Owner.Control.DataMember) as IBindingList;
      if (lst == null)
        throw new InvalidOperationException("Источник данных не реализует IList");

      PropertyDescriptorCollection pdc = ListBindingHelper.GetListItemProperties(Owner.Control.DataSource, Owner.Control.DataMember, null);
      if (pdc == null)
        throw new InvalidOperationException("Источник данных не возвращает PropertyDescriptorCollection");
      PropertyDescriptor pdOrder = pdc[ManualOrderColumn];
      if (pdOrder == null)
        throw new InvalidOperationException("В источнике данных не найден дескриптор свойства \"" + ManualOrderColumn + "\"");
      PropertyDescriptor pdDefaultOrder = pdc[DefaultManualOrderColumn];
      if (pdDefaultOrder == null)
        throw new InvalidOperationException("В источнике данных не найден дескриптор свойства \"" + DefaultManualOrderColumn + "\"");

      ISupportInitialize isi = lst as ISupportInitialize;
      bool InitStarted = false;
      bool Changed = false;
      try
      {
        // Копируем значения поля сортировки по умолчанию
        for (int i = 0; i < lst.Count; i++)
        {
          int OldOrder = DataTools.GetInt(pdOrder.GetValue(lst[i]));
          int NewOrder = DataTools.GetInt(pdDefaultOrder.GetValue(lst[i]));
          if (NewOrder != OldOrder)
          {
            if (!Changed)
            {
              Changed = true;
              if (isi != null)
              {
                isi.BeginInit();
                InitStarted = true;
              }
            }
            pdOrder.SetValue(lst[i], NewOrder);
          }
        }
      }
      finally
      {
        if (InitStarted)
          isi.EndInit();
      }

      return Changed;
    }
#else
    /// <summary>
    /// Восстановление порядка по умолчанию на основании DefaultManualOrderColumn
    /// </summary>
    /// <returns></returns>
    private bool DoSortRestoreColumn()
    {
      if (String.IsNullOrEmpty(DefaultManualOrderColumn))
        throw new NullReferenceException("Свойство DefaultManualOrderColumn не установлено");
      int i;


      // Получаем доступ к объекту DataView
      DataView dv = Owner.SourceAsDataView;
      if (dv == null)
        throw new InvalidDataSourceException("Нельзя получить DataView");

      // 1. Загружаем полный список строк DataRow в массив
      DataRow[] rows1 = DataTools.GetDataViewRows(dv);

      // 2. Копируем значения поля сортировки по умолчанию
      bool changed = false;
      for (i = 0; i < rows1.Length; i++)
      {
        int oldOrder = DataTools.GetInt(rows1[i], ManualOrderColumn);
        int newOrder = DataTools.GetInt(rows1[i], DefaultManualOrderColumn);
        if (newOrder != oldOrder)
        {
          rows1[i][ManualOrderColumn] = newOrder;
          changed = true;
        }
      }

      if (!changed)
        return false;

      // 3. Обновляем просмотр
      Owner.Control.Refresh();

      return true;
    }
#endif
    /// <summary>
    /// Восстановление порядка по умолчанию на основании DefaultManualOrderRows
    /// </summary>
    /// <returns></returns>
    private bool DoSortRestoreRows()
    {
      if (DefaultManualOrderRows == null)
        throw new NullReferenceException("Свойство DefaultManulOrderRows не установлено");

      // 1. Загружаем полный список строк DataGridViewRow в массив
      DataGridViewRow[] rows1 = new DataGridViewRow[Owner.Control.Rows.Count];
      Owner.Control.Rows.CopyTo(rows1, 0);

      // 2. Запоминаем выбранные строки
      DataGridViewRow[] selRows = Owner.SelectedGridRows;

      // 3. Подготавливаем массив строк для их размещения в новом порядке
      // Значения null в этом массиве означают временно пустые позиции
      DataGridViewRow[] rows2 = new DataGridViewRow[rows1.Length];

      // 4. Копируем строки из массива по умолчанию
      int i;
      int cnt = 0;
      bool changed = false;
      for (i = 0; i < DefaultManualOrderRows.Length; i++)
      {
        int thisPos = Array.IndexOf<DataGridViewRow>(rows1, DefaultManualOrderRows[i]);
        if (thisPos < 0)
          continue; // Ошибка

        rows2[cnt] = rows1[thisPos];
        rows1[thisPos] = null;
        if (cnt != thisPos)
          changed = true;
        cnt++;
      }

      // 5. Копируем "лишние" строки, которых нет в массиве по умолчанию
      for (i = 0; i < rows1.Length; i++)
      {
        if (rows1[i] != null)
        {
          rows2[cnt] = rows1[i];
          if (cnt != i)
            changed = true;
          cnt++;
        }
      }

      if (!changed)
        return false;

      // 6. Замещаем коллекцию строк
      Owner.Control.Rows.Clear();
      Owner.Control.Rows.AddRange(rows2);

      // 7. Восстанавливаем выбор
      Owner.SelectedGridRows = selRows;
      return true;
    }

    /// <summary>
    /// Вызывается, когда выполнена ручная сортировка строк (по окончании изменения
    /// значений поля для всех строк)
    /// </summary>
    public event EventHandler ManualOrderChanged;

    #endregion

    #endregion

    #region Команды перехода между ошибками

    private EFPCommandItem _MenuRowErrors;
    private EFPCommandItem ciGotoPrevErrorWarning, ciGotoNextErrorWarning, ciGotoPrevErrorOnly, ciGotoNextErrorOnly;
    private EFPCommandItem ciCopyRowErrorMessages;

    void ciGotoNextErrorWarning_Click(object sender, EventArgs args)
    {
      Owner.GotoNextErrorRow(false, true, EFPDataGridViewImageKind.Warning);
    }

    void ciGotoPrevErrorWarning_Click(object sender, EventArgs args)
    {
      Owner.GotoNextErrorRow(false, false, EFPDataGridViewImageKind.Warning);
    }

    void ciGotoNextErrorOnly_Click(object sender, EventArgs args)
    {
      Owner.GotoNextErrorRow(false, true, EFPDataGridViewImageKind.Error);
    }

    void ciGotoPrevErrorOnly_Click(object sender, EventArgs args)
    {
      Owner.GotoNextErrorRow(false, false, EFPDataGridViewImageKind.Error);
    }

    void ciCopyRowErrorMessages_Click(object sender, EventArgs args)
    {
      if (!Owner.CheckSingleRow())
        return;
      ErrorMessageList errors = new ErrorMessageList();
      Owner.GetSelectedRowsErrorMessages(errors);

      if (errors.Count == 0)
      {
        EFPApp.ShowTempMessage("Для текущей строки нет сообщений об ошибках");
        return;
      }
      string[] a = new string[errors.Count];
      for (int i = 0; i < errors.Count; i++)
        a[i] = errors[i].Text;

      string text = String.Join(Environment.NewLine, a);

      EFPApp.Clipboard.SetText(text);
    }

    /// <summary>
    /// Доступность команд перехода к следующей / предыдущей строке с ошибкой
    /// или предупреждением. Наличие команд определяется свойством DocGridHandler.UserRowImages.
    /// Это свойство позволяет временно блокировать команды (установкой в false),
    /// например, на время заполнения просмотра в фоновом режиме, пока значения не определены
    /// </summary>
    public virtual bool GotoErrorEnabled
    {
      get { return ciGotoNextErrorWarning.Enabled; }
      set
      {
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

    #endregion

    #region Построение списка сообщений об ошибках

    private EFPCommandItem ciShowRowErrorMessages;

    private static int _ShowRowErrorMessagesRowMode = 0;

    void ciShowRowErrorMessages_Click(object sender, EventArgs args)
    {
      RadioSelectDialog dlg = new RadioSelectDialog();
      dlg.Title = "Показать список сообщений";
      int n = Owner.SelectedRowCount;
      dlg.Items = new string[]{Owner.IsCurrentRowSingleSelected? "Для текущей строки":"Для выбранных строк ("+n.ToString()+")",
        "Для всех строк в просмотре"};
      dlg.SelectedIndex = _ShowRowErrorMessagesRowMode;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      _ShowRowErrorMessagesRowMode = dlg.SelectedIndex;
      ErrorMessageList errors = new ErrorMessageList();
      string title;
      EFPApp.BeginWait("Сбор сообщений об ошибках");
      try
      {
        if (_ShowRowErrorMessagesRowMode == 1)
        {
          Owner.GetAllRowsErrorMessages(errors);
          title = "Сообщения для всех строк таблицы";
        }
        else
        {
          Owner.GetSelectedRowsErrorMessages(errors);
          if (Owner.IsCurrentRowSingleSelected)
            title = "Сообщения для текущей строки таблицы";
          else
            title = "Сообщения для выбранных строк таблицы (" + n.ToString() + ")";
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
    /// <param name="errorMessages"></param>
    /// <param name="title"></param>
    protected virtual void ShowErrorMessageListDialog(ErrorMessageList errorMessages, string title)
    {
      EFPApp.ShowErrorMessageListDialog(errorMessages, title, 0,
        new ErrorMessageItemEventHandler(EditRowForErrorMessage));
    }

    /// <summary>
    /// Обработка команды просмотра сообщения об ошибках
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    protected void EditRowForErrorMessage(object sender, ErrorMessageItemEventArgs args)
    {
      int rowIndex = DataTools.GetInt(args.Item.Tag);
      if (rowIndex >= 0 && rowIndex < Owner.Control.RowCount)
        Owner.SelectRowIndex(rowIndex);
      else
        EFPApp.ShowTempMessage("Несуществующий номер строки");
    }

    #endregion

    #region Отправить

    /// <summary>
    /// Подменю "Отправить"
    /// </summary>
    public EFPCommandItem MenuSendTo { get { return _MenuSendTo; } }
    private EFPCommandItem _MenuSendTo;

    internal EFPCommandItem ciSendToMicrosoftExcel, ciSendToOpenOfficeCalc;

    /// <summary>
    /// Выбранные пользователем настройки экспорта в Excel / Calc
    /// </summary>
    internal static EFPDataGridViewExpExcelSettings SendToSettings = new EFPDataGridViewExpExcelSettings();

    void ciSendToMicrosoftExcel_Click(object sender, EventArgs args)
    {
      OnSendToMicrosoftExcel();
    }

    /// <summary>
    /// Выполнение команды "Отправить" - "Microsoft Excel"
    /// </summary>
    protected virtual void OnSendToMicrosoftExcel()
    {
      EFPDataGridViewExpExcelForm form = new EFPDataGridViewExpExcelForm();
      form.Text = "Отправить в " + ciSendToMicrosoftExcel.MenuTextWithoutMnemonic;
      form.Icon = EFPApp.MainImageIcon(ciSendToMicrosoftExcel.ImageKey);
      form.LoadValues(SendToSettings);
      form.InitVisibility(Owner);
      if (EFPApp.ShowDialog(form, true) != DialogResult.OK)
        return;

      form.SaveValues(SendToSettings);

      Owner.SendToMicrosoftExcel(SendToSettings);
    }

    void ciSendToOpenOfficeCalc_Click(object sender, EventArgs args)
    {
      OnSendToOpenOfficeCalc();
    }

    /// <summary>
    /// Выполнение команды "Отправить" - "OpenOffice / LibreOffice Calc"
    /// </summary>
    protected virtual void OnSendToOpenOfficeCalc()
    {
      EFPDataGridViewExpExcelForm form = new EFPDataGridViewExpExcelForm();
      form.Text = "Отправить в " + ciSendToOpenOfficeCalc.MenuTextWithoutMnemonic;
      form.Icon = EFPApp.MainImageIcon(ciSendToOpenOfficeCalc.ImageKey);
      form.LoadValues(SendToSettings);
      form.InitVisibility(Owner);
      if (EFPApp.ShowDialog(form, true) != DialogResult.OK)
        return;

      form.SaveValues(SendToSettings);

      Owner.SendToOpenOfficeCalc(SendToSettings);
    }

    #endregion

    #region Окно суммы в статусной строке

    private EFPCommandItem ciStatCount, ciStatSumma/*, ciStatSummaCopy*/;

    private Timer _StatSummaTimer;

    private const string EmptyStatCountText = "    ";
    private const string EmptyStatSummaText = "    ";
    //private const int StatCountLen = 4;
    //private const int StatSummaLen = 12;

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
      ciStatSumma.ImageKey = String.Empty;
      ciStatCount.StatusBarText = EmptyStatCountText;
      ciStatSumma.StatusBarText = EmptyStatSummaText;
      if (!WantCalcStatItems())
        return;

      _CurrStatCellIndex = 0;
      _CurrStatCellSumma = null;
      _CurrStatCellSummaDigits = 0;
      _CurrStatCellCount = null;
      ciStatSumma.ImageKey = "HourGlass";
      if (Owner.Control.SelectedCells.Count <= StatCellCountPerCheck)
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
      if (Owner.Control.SelectedRows != null && Owner.Control.SelectedRows.Count > 0)
        return false;
      if (Owner.Control.SelectedColumns != null && Owner.Control.SelectedColumns.Count > 0)
        return false;
      if (Owner.Control.SelectedCells == null)
        return false;
      if (Owner.Control.SelectedCells.Count < 2)
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

    private bool CalcStatSum(bool checkTime)
    {
      bool res = true;
      try
      {
        if (_CurrStatCellIndex == 0)
          ciStatSumma.StatusBarText = "Идет расчет";
        if (DoCalcStatSum(checkTime))
        {
          if (_CurrStatCellSumma.HasValue)
          {
            ciStatSumma.StatusBarText = /*Math.Round(*/_CurrStatCellSumma.Value/*, CurrStatCellSummaDigits, MidpointRounding.AwayFromZero)*/.ToString();
            ciStatSumma.ImageKey = "Sum";
          }
          else
          {
            ciStatSumma.StatusBarText = EmptyStatSummaText;
            ciStatSumma.ImageKey = String.Empty;
          }
          if (_CurrStatCellCount.HasValue)
            ciStatCount.StatusBarText = _CurrStatCellCount.Value.ToString();
          else
            ciStatCount.StatusBarText = EmptyStatCountText;
        }
        else
          res = false;
      }
      catch
      {
        ciStatSumma.StatusBarText = "Ошибка!";
        ciStatSumma.ImageKey = String.Empty;
        ciStatCount.StatusBarText = EmptyStatCountText;
      }

      return res;
    }

    private bool DoCalcStatSum(bool checkTime)
    {
      DateTime startTime = DateTime.Now;
      while (_CurrStatCellIndex < Owner.Control.SelectedCells.Count)
      {
        if (checkTime && _CurrStatCellIndex > 0 && (_CurrStatCellIndex % 20) == 0)
        {
          TimeSpan ts = DateTime.Now - startTime;
          if (ts.TotalMilliseconds > 200.0) // 1/5 секунды
            return false;
        }
        DataGridViewCell cell = Owner.Control.SelectedCells[_CurrStatCellIndex];
        _CurrStatCellIndex++;
        Owner.DoGetRowAttributes(cell.RowIndex, EFPDataGridViewAttributesReason.View);
        EFPDataGridViewCellAttributesEventArgs args = Owner.DoGetCellAttributes(cell.ColumnIndex);
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

      WinFormsTools.CorrectNumberString(ref s);
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
        EFPApp.ShowTempMessage("Нет значения для копирования в буфер обмена");
      else
        EFPApp.Clipboard.SetText(s);
    }

    void ciStatSumma_Click(object sender, EventArgs args)
    {
      string s = ciStatSumma.StatusBarText.Trim();
      if (String.IsNullOrEmpty(s))
        EFPApp.ShowTempMessage("Нет значения для копирования в буфер обмена");
      else
        EFPApp.Clipboard.SetText(s);
    }

    #endregion

    #region Прочие команды

    EFPCommandItem ciSelectAll, ciRefresh;

    private void SelectAll(object sender, EventArgs args)
    {
      Owner.Control.SelectAll();
      PerformRefreshItems();
    }
    private void Refresh(object sender, EventArgs args)
    {
      Owner.PerformRefresh();
    }

    #endregion
  }
}
