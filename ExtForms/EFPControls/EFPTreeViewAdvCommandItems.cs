﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Controls;
using FreeLibSet.UICore;

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
      ciEdit.MenuText = Res.Cmd_Menu_Edit_Edit;
      ciEdit.ImageKey = "Edit";
      ciEdit.GroupBegin = true;
      ciEdit.Click += new EventHandler(ciEdit_Click);
      Add(ciEdit);

      // Фиктивная тема, для которой нет команды меню или кнопки
      ciEdit2 = new EFPCommandItem("Edit", "Edit2");
      ciEdit2.Usage = EFPCommandItemUsage.ShortCut;
      ciEdit2.Click += new EventHandler(ClickOKButton);
      Add(ciEdit2);

      ciInsert = new EFPCommandItem("Edit", "Insert");
      ciInsert.MenuText = Res.Cmd_Menu_Edit_Insert;
      ciInsert.ImageKey = "Insert";
      ciInsert.ShortCut = Keys.Insert;
      ciInsert.Click += new EventHandler(ciInsert_Click);
      Add(ciInsert);

      ciInsertCopy = new EFPCommandItem("Edit", "InsertCopy");
      ciInsertCopy.MenuText = Res.Cmd_Menu_Edit_InsertCopy;
      ciInsertCopy.ImageKey = "InsertCopy";
      ciInsertCopy.ShortCut = Keys.Insert | Keys.Alt;
      ciInsertCopy.Click += new EventHandler(ciInsertCopy_Click);
      Add(ciInsertCopy);

      ciDelete = new EFPCommandItem("Edit", "Delete");
      ciDelete.MenuText = Res.Cmd_Menu_Edit_Delete;
      ciDelete.ImageKey = "Delete";
      ciDelete.ShortCut = Keys.Delete;
      ciDelete.Click += new EventHandler(ciDelete_Click);
      Add(ciDelete);

      ciView = new EFPCommandItem("Edit", "View");
      ciView.MenuText = Res.Cmd_Menu_Edit_View;
      ciView.ImageKey = "View";
      ciView.GroupEnd = true;
      ciView.ShortCut = Keys.Shift | Keys.Return;
      ciView.Click += new EventHandler(ciView_Click);
      Add(ciView);

      if (!EFPApp.EasyInterface)
      {
        ciInlineEditStatus = new EFPCommandItem("View", "InlineEditsStatus");
        ciInlineEditStatus.MenuText = Res.Cmd_Menu_Edit_EditInline;
        ciInlineEditStatus.ImageKey = "EmptyImage";
        ciInlineEditStatus.StatusBarText = EFPCommandItem.EmptyStatusBarText;
        ciInlineEditStatus.Click += new EventHandler(ciInlineEditStatus_Click);
        ciInlineEditStatus.Usage = EFPCommandItemUsage.StatusBar;
        Add(ciInlineEditStatus);
      }

      #endregion

      #region Выделить все

      ciSelectAll = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SelectAll);
      //ciDebugChanges.ImageKey="SelectAll";
      ciSelectAll.Click += new EventHandler(SelectAll);
      Add(ciSelectAll);

      #endregion

      #region "Обновить"

      ciRefresh = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Refresh);
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

      if ((!ForceInlineEditStatusPanel) && ciInlineEditStatus != null)
      {
        if (ControlProvider.ControlIsReadOnly)
          ciInlineEditStatus.Usage = EFPCommandItemUsage.None;
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

      UISelectedRowsState selState = ControlProvider.SelectedRowsState;

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

      } // UseEditView
        // 16.01.2023
        //if (this[EFPAppStdCommandItems.Cut].Usage != EFPCommandItemUsage.None)
      this[EFPAppStdCommandItems.Cut].Enabled = selState != UISelectedRowsState.NoSelection &&
        (!ControlProvider.ReadOnly);
      this[EFPAppStdCommandItems.Copy].Enabled = selState != UISelectedRowsState.NoSelection; // 17.06.2024
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
          ciInlineEditStatus.ToolTipText = Res.Cmd_ToolTip_Edit_EditInline_ControlReadOnly;
        }
        else if (btc != null)
        {
          ciInlineEditStatus.ImageKey = "TableInlineEditProcess";
          ciInlineEditStatus.ToolTipText = Res.Cmd_ToolTip_Edit_EditInline_EditInProgress;
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
          ciInlineEditStatus.ToolTipText = Res.Cmd_ToolTip_Edit_EditInline_CanStart;
          //}
        }
      }

      if (ciSelectAll != null)
      {
        ciSelectAll.Enabled = (selState != UISelectedRowsState.NoSelection) &&
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
      EFPDataViewCopyFormats copyFormats2 = SelectedCopyFormats & (EFPDataViewCopyFormats.Text | EFPDataViewCopyFormats.CSV);
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
            throw new ArgumentException();
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
          if (((!ControlProvider.ReadOnly) && ControlProvider.CanEdit /* 19.07.2024 */) ||
            ControlProvider.CanView
            /*|| (!Handler.MainGrid.ReadOnly)*/)
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
        ControlProvider.PerformEditData(ControlProvider.ReadOnly ? UIDataState.View : UIDataState.Edit);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    private void ciInsert_Click(object sender, EventArgs args)
    {
      try
      {
        ControlProvider.PerformEditData(UIDataState.Insert);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }

    }

    private void ciInsertCopy_Click(object sender, EventArgs args)
    {
      try
      {
        ControlProvider.PerformEditData(UIDataState.InsertCopy);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }

    }

    private void ciDelete_Click(object sender, EventArgs args)
    {
      try
      {
        ControlProvider.PerformEditData(UIDataState.Delete);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    private void ciView_Click(object sender, EventArgs args)
    {
      try
      {
        ControlProvider.PerformEditData(UIDataState.View);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
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
