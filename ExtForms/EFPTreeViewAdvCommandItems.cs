using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms.NodeControls;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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

namespace AgeyevAV.ExtForms
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
    /// <param name="owner">Провайдер управляющего элемента</param>
    public EFPTreeViewAdvCommandItems(EFPTreeViewAdv owner)
      : base(owner)
    {
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
        CheckNotAssigned();
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

    #endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPTreeViewAdv Owner { get { return (EFPTreeViewAdv)(base.Owner); } }

    /// <summary>
    /// Установка свойств EFPCommandItem.Usage
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

      if (!UseSelectAll)
        ciSelectAll.Usage = EFPCommandItemUsage.None;

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

      Owner.Control.NodeMouseDoubleClick += Control_NodeMouseDoubleClick;
      Owner.Control.MouseDown += new MouseEventHandler(Control_MouseDown);
      Owner.Control.MouseUp += new MouseEventHandler(Control_MouseUp);
      //Owner.Grid_VisibleChanged(null, null);


      PerformRefreshItems();

      // 14.08.2012 Добавляем обработчики
      //Owner.AfterControlAssigned();
    }

    /// <summary>
    /// Обновление доступности команд
    /// </summary>
    protected override void DoRefreshItems()
    {
      base.DoRefreshItems();

      EFPDataGridViewSelectedRowsState SelState = Owner.SelectedRowsState;

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
          ciEdit.Enabled = (SelState != EFPDataGridViewSelectedRowsState.NoSelection);
        else
          ciEdit.Enabled = (SelState == EFPDataGridViewSelectedRowsState.SingleRow);
        ciInsertCopy.Enabled = (SelState == EFPDataGridViewSelectedRowsState.SingleRow);
        ciDelete.Enabled = (SelState != EFPDataGridViewSelectedRowsState.NoSelection);

        ciView.Enabled = ciEdit.Enabled;
#endif

        if (SelState == EFPDataGridViewSelectedRowsState.MultiRows)
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

        // TODO: if (ciCut.Usage != EFPCommandItemUsage.None)
        // TODO:   ciCut.Enabled = !(Owner.ReadOnly && Owner.Control.ReadOnly);
        // TODO: if (!PasteHandler.AlwaysEnabled) // 27.11.2017
        // TODO:   PasteHandler.Enabled = !(Owner.ReadOnly && Owner.Control.ReadOnly);
      }

      if (ciInlineEditStatus != null)
      {
        BaseTextControl btc = Owner.Control.CurrentEditorOwner as BaseTextControl;

        if (Owner.ControlIsReadOnly)
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
        ciSelectAll.Enabled = (SelState != EFPDataGridViewSelectedRowsState.NoSelection) &&
          Owner.Control.SelectionMode == TreeViewAdvSelectionMode.Multi;
      }

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
          if (Owner.Control.SelectedNode != null)
          {
            // TODO: string ReadOnlyMessage;
            // TODO: if (!Owner.GetCellReadOnly(Owner.Control.CurrentCell, out ReadOnlyMessage))
            // TODO: return;
          }

          ClickOKButton(null, null);
        }
        else
        {
          if ((!Owner.ReadOnly) || Owner.CanView /*|| (!Handler.MainGrid.ReadOnly)*/)
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
      Owner.PerformRefresh();
    }

    #endregion

    #region Выбрать все

    EFPCommandItem ciSelectAll;

    private void SelectAll(object sender, EventArgs args)
    {
      Owner.Control.ExpandAll();
      Owner.Control.SelectAllNodes();
      PerformRefreshItems();
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
