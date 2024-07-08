// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Controls;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Команды локального меню древовидного просмотра
  /// </summary>
  public class EFPTreeViewAdvCommandItems : EFPTreeViewCommandItemsBase
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Создает список команд
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    public EFPTreeViewAdvCommandItems(EFPTreeViewAdv controlProvider)
      : base(controlProvider)
    {
      base.CopyFormats = EFPDataViewCopyFormats.Text | EFPDataViewCopyFormats.CSV;
      // TODO: base.CopyFormats = EFPDataViewCopyFormats.All;

      #region Начальные значения свойств

      _EnterAsOk = false;
      _UseEditView = false;
      _UseRefresh = true;
      _UseSelectAll = true;
      //CopyFormats = EFPDataGridViewCopyFormats.All;

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
        ciInlineEditStatus = new EFPCommandItem("View", "InlineEditsStatus");
        ciInlineEditStatus.MenuText = "Редактирование по месту";
        ciInlineEditStatus.ImageKey = "EmptyImage";
        ciInlineEditStatus.StatusBarText = EFPCommandItem.EmptyStatusBarText;
        ciInlineEditStatus.Click += new EventHandler(ciInlineEditStatus_Click);
        ciInlineEditStatus.Usage = EFPCommandItemUsage.StatusBar;
        Add(ciInlineEditStatus);
      }

      #endregion

      #region Выделить все

      ciSelectAll = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SelectAll);
      ciSelectAll.MenuText = "Выделить все строки в просмотре";
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

      base.AddCommands(); // команды поиска и установки отметок

      #endregion
    }

    //void ci_Click(object sender, EventArgs e)
    //{
    //  TreeNodeAdv Node = Owner.GetPreviousTreeNode(Owner.Control.SelectedNode);
    //  if (Node == null)
    //    EFPApp.ShowTempMessage("Нет больше");
    //  else
    //    Owner.Control.SelectedNode = Node;
    //}

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
        CheckNotReadOnly();
        _EnterAsOk = value;
      }
    }
    private bool _EnterAsOk;



    /// <summary>
    /// True, если есть команды "Редактировать", "Создать", "Удалить", "Просмотр"
    /// По умолчанию: для EFPTreeViewAdv - false, для EFPDatTreeView - true
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

    /// <summary>
    /// True, если есть команда "Обновить" (F5)
    /// По умолчанию - true
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

    #endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPTreeViewAdv ControlProvider { get { return (EFPTreeViewAdv)(base.ControlProvider); } }

    /// <summary>
    /// Установка свойств EFPCommandItem.Usage
    /// </summary>
    protected override void OnPrepare()
    {
      base.OnPrepare();

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
        ciRefresh.Visible = ControlProvider.HasRefreshDataHandler;
      else
        ciRefresh.Usage = EFPCommandItemUsage.None;

      if (!UseSelectAll)
        ciSelectAll.Usage = EFPCommandItemUsage.None;

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

      ControlProvider.Control.NodeMouseDoubleClick += Control_NodeMouseDoubleClick;
      ControlProvider.Control.MouseDown += new MouseEventHandler(Control_MouseDown);
      ControlProvider.Control.MouseUp += new MouseEventHandler(Control_MouseUp);
      //Owner.Grid_VisibleChanged(null, null);
    }

    /// <summary>
    /// Обновление доступности команд
    /// </summary>
    protected override void OnRefreshItems()
    {
      base.OnRefreshItems();

      EFPDataGridViewSelectedRowsState selState = ControlProvider.SelectedRowsState;

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
          ciEdit.Enabled = (selState != EFPDataGridViewSelectedRowsState.NoSelection);
        else
          ciEdit.Enabled = (selState == EFPDataGridViewSelectedRowsState.SingleRow);
        ciInsertCopy.Enabled = (selState == EFPDataGridViewSelectedRowsState.SingleRow);
        ciDelete.Enabled = (selState != EFPDataGridViewSelectedRowsState.NoSelection);

        ciView.Enabled = ciEdit.Enabled;
#endif

        if (selState == EFPDataGridViewSelectedRowsState.MultiRows)
        {
          if (!ControlProvider.ReadOnly)
          {
            ciEdit.MenuText = "Редактировать выбранные записи";
            ciDelete.MenuText = "Удалить выбранные записи";
          }
          if (ControlProvider.CanView)
            ciView.MenuText = "Просмотреть выбранные записи";
        }
        else
        {
          if (!ControlProvider.ReadOnly)
          {
            ciEdit.MenuText = "Редактировать запись";
            ciDelete.MenuText = "Удалить запись";
          }
          if (ControlProvider.CanView)
            ciView.MenuText = "Просмотреть запись";
        }

      } // UseEditView
        // 16.01.2023
        //if (this[EFPAppStdCommandItems.Cut].Usage != EFPCommandItemUsage.None)
      this[EFPAppStdCommandItems.Cut].Enabled = selState != EFPDataGridViewSelectedRowsState.NoSelection &&
        (!ControlProvider.ReadOnly);
      this[EFPAppStdCommandItems.Copy].Enabled = selState != EFPDataGridViewSelectedRowsState.NoSelection; // 17.06.2024
      if (!PasteHandler.AlwaysEnabled) // 27.11.2017
      {
        //if (HasTextPasteFormats)
        //  PasteHandler.Enabled = !ControlProvider.Control.ReadOnly; // 24.04.2019
        //else
        PasteHandler.Enabled = !(ControlProvider.ReadOnly /*&& ControlProvider.Control.ReadOnly*/);
      }

      if (ciInlineEditStatus != null)
      {
        BaseTextControl btc = ControlProvider.Control.CurrentEditorOwner as BaseTextControl;

        if (ControlProvider.ControlIsReadOnly)
        {
          ciInlineEditStatus.ImageKey = "TableInlineEditReadOnlyTable";
          ciInlineEditStatus.ToolTipText = "Просмотр не поддерживает редактирование по месту";
        }
        else if (btc != null)
        {
          ciInlineEditStatus.ImageKey = "TableInlineEditProcess";
          ciInlineEditStatus.ToolTipText = "Выполняется редактирование ячейки";
        }
        else
        {
          // ?????
          //string ReadOnlyMessage;
          //if (Owner.GetCellReadOnly(Owner.Control.CurrentCell, out ReadOnlyMessage))
          //{
          //  ciInlineEditStatus.ImageKey = "TableInlineEditReadOnlyCell";
          //  ciInlineEditStatus.ToolTipText = "Нельзя редактировать ячейку. " + ReadOnlyMessage;
          //}
          //else
          //{
          ciInlineEditStatus.ImageKey = "TableInlineEdit";
          ciInlineEditStatus.ToolTipText = "Можно начать редактирование ячейки";
          //}
        }
      }

      if (ciSelectAll != null)
      {
        ciSelectAll.Enabled = (selState != EFPDataGridViewSelectedRowsState.NoSelection) &&
          ControlProvider.Control.SelectionMode == TreeViewAdvSelectionMode.Multi;
      }

    }

    #endregion

    #region Буфер обмена

    /// <summary>
    /// Добавляет в буфер обмена текстовый формат для выбранных ячеек просмотра
    /// </summary>
    /// <param name="args">Ссылка на DataObject</param>
    protected override void OnAddDefaultCopyFormats(DataObjectEventArgs args)
    {
      EFPDataViewCopyFormats copyFormats2 = CopyFormats & (EFPDataViewCopyFormats.Text | EFPDataViewCopyFormats.CSV);
      if (copyFormats2 != EFPDataViewCopyFormats.None)
      {
        string[,] a = ControlProvider.GetSelectedNodesTextMatrix();
        AddDefaultCopyFormats(args.DataObject, a, copyFormats2);
      }

      // TODO: Поддержка для HTML
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
      Form frm = ControlProvider.Control.FindForm();
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
    void Control_NodeMouseDoubleClick(object sender, TreeNodeAdvMouseEventArgs args)
    {
      //if (Args.RowIndex < 0 || Args.ColumnIndex < 0)
      //  return;
      if (args.Node != null)
        _NodeDoubleClicked = true;
      else
        _NodeDoubleClicked = false; // 16.01.2019
      args.Handled = true; // 16.01.2019
    }

    private bool _NodeDoubleClicked = false;

    void Control_MouseUp(object sender, MouseEventArgs args)
    {
      if (_NodeDoubleClicked)
      {
        _NodeDoubleClicked = false;


        if (EnterAsOk)
        {
          // 16.08.2012
          // Если текущая ячейка допускает inline-редактирование, то нажимать кнопку
          // по умолчанию - неправильно
          if (ControlProvider.Control.SelectedNode != null)
          {
            // TODO: string ReadOnlyMessage;
            // TODO: if (!Owner.GetCellReadOnly(Owner.Control.CurrentCell, out ReadOnlyMessage))
            // TODO: return;
          }

          ClickOKButton(null, null);
        }
        else
        {
          if ((!ControlProvider.ReadOnly) || ControlProvider.CanView /*|| (!Handler.MainGrid.ReadOnly)*/)
            ciEdit_Click(null, null);
        }
      }
    }

    void Control_MouseDown(object sender, MouseEventArgs args)
    {
      _NodeDoubleClicked = false;
    }


    private void ciEdit_Click(object sender, EventArgs args)
    {
      try
      {
        ControlProvider.PerformEditData(ControlProvider.ReadOnly ? EFPDataGridViewState.View : EFPDataGridViewState.Edit);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка при " + (ControlProvider.ReadOnly ? "просмотре" : "редактировании") + " данных");
      }
    }

    private void ciInsert_Click(object sender, EventArgs args)
    {
      try
      {
        ControlProvider.PerformEditData(EFPDataGridViewState.Insert);
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
        ControlProvider.PerformEditData(EFPDataGridViewState.InsertCopy);
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
        ControlProvider.PerformEditData(EFPDataGridViewState.Delete);
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
        ControlProvider.PerformEditData(EFPDataGridViewState.View);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка при просмотре данных");
      }
    }

    void ciInlineEditStatus_Click(object sender, EventArgs args)
    {
      /*
      if (Owner.Control.IsCurrentCellInEditMode)
        Owner.Control.EndEdit();
      else
        Owner.Control.BeginEdit(false);
       * */
    }

    #endregion

    #region Обновление

    EFPCommandItem ciRefresh;

    private void Refresh(object sender, EventArgs args)
    {
      ControlProvider.PerformRefresh();
    }

    #endregion

    #region Выбрать все

    EFPCommandItem ciSelectAll;

    private void SelectAll(object sender, EventArgs args)
    {
      ControlProvider.Control.ExpandAll();
      ControlProvider.Control.SelectAllNodes();
    }

    #endregion


    ///// <summary>
    ///// Заглушка
    ///// </summary>
    //public void InitCurrentCfgName()
    //{
    //}

    ///// <summary>
    ///// Заглушка
    ///// </summary>
    //public void InitCurrentCfgModified()
    //{
    //}

  }
}
