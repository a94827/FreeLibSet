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
using FreeLibSet.Models.Tree;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Команды древовидного просмотра с поддержкой столбцов
  /// </summary>
  public class EFPDataTreeViewCommandItems : EFPTreeViewAdvCommandItems, IEFPClipboardCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает список команд
    /// </summary>
    /// <param name="owner">Провайдер управляющего элемента</param>
    public EFPDataTreeViewCommandItems(EFPDataTreeView owner)
      : base(owner)
    {
      #region Начальные значения свойств

      UseEditView = true;
      UseRefresh = true;
      UseSelectAll = true;

      #endregion

      #region Создание команд


#if XXX
      #region Установка отметок

      MenuCheck = new EFPCommandItem("Правка", "МенюУстановкиОтметок");
      MenuCheck.MenuText = "Установка отметок для строк";
      MenuCheck.ImageKey = "CheckListChecked";
      MenuCheck.Usage = EFPCommandItemUsage.Menu;
      Add(MenuCheck);

      ciCheckSel = new EFPCommandItem("Правка", "УстановитьОтметкиСтрок");
      ciCheckSel.Parent = MenuCheck;
      ciCheckSel.GroupBegin = true;
      ciCheckSel.MenuText = "Установить отметки для выбранных строк";
      ciCheckSel.ImageKey = "CheckListChecked";
      ciCheckSel.ShortCut = Keys.Add;
      ciCheckSel.Click += new EventHandler(ciCheckSel_Click);
      Add(ciCheckSel);

      ciUncheckSel = new EFPCommandItem("Правка", "СнятьОтметкиСтрок");
      ciUncheckSel.Parent = MenuCheck;
      ciUncheckSel.MenuText = "Снять отметки для выбранных строк";
      ciUncheckSel.ImageKey = "CheckListUnchecked";
      ciUncheckSel.ShortCut = Keys.Subtract;
      ciUncheckSel.Click += new EventHandler(ciUncheckSel_Click);
      Add(ciUncheckSel);

      ciInvertSel = new EFPCommandItem("Правка", "ИнвертироватьОтметкиСтрок");
      ciInvertSel.Parent = MenuCheck;
      ciInvertSel.MenuText = "Инвертировать отметки для выбранных строк";
      ciInvertSel.ImageKey = "CheckListInvert";
      ciInvertSel.ShortCut = Keys.Multiply;
      ciInvertSel.Click += new EventHandler(ciInvertSel_Click);
      Add(ciInvertSel);

      ciCheckAll = new EFPCommandItem("Правка", "УстановитьОтметкиДляВсех");
      ciCheckAll.Parent = MenuCheck;
      ciCheckAll.MenuText = "Установить отметки для всех строк";
      ciCheckAll.ImageKey = "CheckListAll";
      ciCheckAll.ShortCut = Keys.Control | Keys.A;
      ciCheckAll.Click += new EventHandler(ciCheckAll_Click);
      Add(ciCheckAll);

      ciUncheckAll = new EFPCommandItem("Правка", "СнятьОтметкиДляВсех");
      ciUncheckAll.Parent = MenuCheck;
      ciUncheckAll.GroupEnd = true;
      ciUncheckAll.MenuText = "Снять отметки для всех строк";
      ciUncheckAll.ImageKey = "CheckListNone";
      ciUncheckAll.ShortCut = Keys.Control | Keys.Shift | Keys.A;
      ciUncheckAll.Click += new EventHandler(ciUncheckAll_Click);
      Add(ciUncheckAll);

      #endregion
#endif
      #region Порядок строк

      _MenuSort = new EFPCommandItem("Edit", "MenuOrder");
      _MenuSort.MenuText = "Порядок строк";
      _MenuSort.ImageKey = "OrderAZ";
      _MenuSort.Usage = EFPCommandItemUsage.Menu;
      Add(_MenuSort);

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

      #region Отправить

      _MenuSendTo = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.MenuSendTo);
      _MenuSendTo.Usage = EFPCommandItemUsage.Menu;
      Add(MenuSendTo);

      ciSendToMicrosoftExcel = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SendToMicrosoftExcel);
      ciSendToMicrosoftExcel.Parent = MenuSendTo;
      ciSendToMicrosoftExcel.Click += ciSendToMicrosoftExcel_Click;
      Add(ciSendToMicrosoftExcel);

      ciSendToOpenOfficeCalc = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SendToOpenOfficeCalc);
      ciSendToOpenOfficeCalc.Parent = MenuSendTo;
      ciSendToOpenOfficeCalc.Click += ciSendToOpenOfficeCalc_Click;
      Add(ciSendToOpenOfficeCalc);

      #endregion

      #endregion
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPDataTreeView ControlProvider { get { return (EFPDataTreeView)(base.ControlProvider); } }

    /// <summary>
    /// Установка свойств EFPCommandItem.Usage
    /// </summary>
    protected override void OnPrepare()
    {
      base.OnPrepare();

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

      if (ControlProvider.OrderCount > 0 || (ControlProvider.GridProducer != null && ControlProvider.GridProducer.OrderCount > 0))
      {
        // Команды сортировки строк существуют или могут появиться в будущем
        _MenuSort.Usage = EFPCommandItemUsage.Everywhere | EFPCommandItemUsage.DisableRightTextInToolTip;
        _OrderItems = new EFPCommandItem[9];
        for (int i = 0; i < _OrderItems.Length; i++)
        {
          EFPCommandItem ci1 = new EFPCommandItem("View", "Order" + (i + 1).ToString());
          ci1.MenuText = (i + 1).ToString();
          ci1.Parent = _MenuSort;
          ci1.GroupBegin = (i == 0);
          //ci1.ImageKey = "Item";
          ci1.ShortCut = Keys.Control | (Keys)(((int)Keys.D1) + i);
          ci1.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
          ci1.Tag = i;
          ci1.Click += new EventHandler(SelectOrder_Click);
          Add(ci1);
          _OrderItems[i] = ci1;
        }
        ciOrderMore = new EFPCommandItem("View", "OrderDialog");
        ciOrderMore.MenuText = "&Еще ...";
        ciOrderMore.Parent = _MenuSort;
        ciOrderMore.GroupBegin = true;
        ciOrderMore.ShortCut = Keys.Control | Keys.D0;
        ciOrderMore.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
        ciOrderMore.Click += new EventHandler(SelectOrderMore_Click);
        Add(ciOrderMore);

      }
      RefreshOrderItems();

      /*
      Owner.Control.CellDoubleClick += new DataGridViewCellEventHandler(Grid_CellDoubleClick);
      Owner.Control.MouseDown += new MouseEventHandler(Grid_MouseDown);
      Owner.Control.MouseUp += new MouseEventHandler(Grid_MouseUp);*/
      ControlProvider.Control_VisibleChanged(null, null);

      /*
      ciSendToMicrosoftExcel.Visible = EFPDataGridView.CanSendToMicrosoftExcel;
      ciSendToOpenOfficeCalc.Visible = EFPDataGridView.CanSendToOpenOfficeCalc;
        */
      ciSendToMicrosoftExcel.Visible = false;
      ciSendToOpenOfficeCalc.Visible = false;

      PerformRefreshItems();

      // 14.08.2012 Добавляем обработчики
      ControlProvider.AfterControlAssigned();
    }

    #endregion

    #region Обновление состояния команд

    ///// <summary>
    ///// Обновление состояния команд
    ///// </summary>
    //protected override void DoRefreshItems()
    //{
    //  base.DoRefreshItems();

    //  EFPDataGridViewSelectedRowsState SelState = Owner.SelectedRowsState;
    //}


    #endregion

    #region Команды сортировки строк

    private EFPCommandItem _MenuSort;

    #region Сортировка путем выбора порядка сортировки

    /// <summary>
    /// Девять команд задания порядка сортировки строк (в том числе недействующие сейчас)
    /// </summary>
    private EFPCommandItem[] _OrderItems;

    /// <summary>
    /// Команда "Еще" для дополнительных порядков сортировки (больше 9)
    /// </summary>
    private EFPCommandItem ciOrderMore;

    /// <summary>
    /// Перестроение списка команд сортировки
    /// </summary>
    public void RefreshOrderItems()
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
      ciOrderMore.Visible = (n > _OrderItems.Length);
      _MenuSort.Enabled = (n > 0);
      InitCurentOrder();
    }


    /// <summary>
    /// Выделение пометкой команды локального меню, соответствующей выбранному
    /// порядку строк
    /// </summary>
    internal void InitCurentOrder()
    {
      if (_OrderItems == null)
        return;

      for (int i = 0; i < _OrderItems.Length; i++)
        _OrderItems[i].Checked = (ControlProvider.CurrentOrderIndex == i);
      string s;
      if (ControlProvider.CurrentOrderIndex < 0 || ControlProvider.CurrentOrderIndex >= ControlProvider.OrderCount)
        s = "Не задан";
      else
        s = ControlProvider.Orders[ControlProvider.CurrentOrderIndex].DisplayName;
      _MenuSort.MenuRightText = s;
      _MenuSort.ToolTipText = "Порядок строк (" + s + ")";

      ControlProvider.InitColumnHeaderTriangles();
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
    /// Выбор порядка строк из полного списка (окно диалога)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void SelectOrderMore_Click(object sender, EventArgs args)
    {
      ControlProvider.ShowSelectOrderDialog();
    }

    #endregion

    #region Ручная сортировка с помощью стрелочек

    #region Общая часть

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
      /*
      if (!Owner.Control.EndEdit())
      {
        EFPApp.ErrorMessageBox("Редактирование не закончено");
        return;
      } */

      int oldColIdx = ControlProvider.CurrentColumnIndex;

      bool changed;
      if (ManualOrderRows)
        changed = DoReorderRows(down);
      else
        changed = DoReorderColumn(down);

      // 9. Обновляем табличный просмотр
      if (changed)
      {
        ControlProvider.CurrentColumnIndex = oldColIdx;

        if (ManualOrderChanged != null)
          ManualOrderChanged(this, EventArgs.Empty);
      }
    }

    void ciSortRestore_Click(object Sender, EventArgs Args)
    {
      EFPDataTreeViewSelection oldSel = ControlProvider.Selection;

      bool changed;
      if (ManualOrderRows)
        changed = DoSortRestoreRows();
      else
        changed = DoSortRestoreColumn();

      // Обновляем табличный просмотр
      if (changed)
      {
        ControlProvider.Selection = oldSel;

        if (ManualOrderChanged != null)
          ManualOrderChanged(this, EventArgs.Empty);
      }
    }

    /// <summary>
    /// Вызывается, когда выполнена ручная сортировка строк (по окончании изменения
    /// значений поля для всех строк)
    /// </summary>
    public event EventHandler ManualOrderChanged;

    #endregion

    #region Перестановка узлов дерева (не реализовано)

    /// <summary>
    /// НЕ РЕАЛИЗОВАНО.
    /// Если установлено в true, то доступны команды ручной сортировки строк.
    /// Сортировка основывается на порядке строк в коллекции, а не на значении
    /// столбца. При сортировке строки переставляются местами внутри объекта
    /// DataGridViewRowCollection
    /// </summary>
    public bool ManualOrderRows { get { return _ManualOrderRows; } set { _ManualOrderRows = value; } }
    private bool _ManualOrderRows;

    /// <summary>
    /// НЕ РЕАЛИЗОВАНО.
    /// Массив строк табличного просмотра в порядке по умолчанию. Свойство действует
    /// при ManualOrderRows=true. Если массив присвоен, то действует команда
    /// "Восстановить порядок по умолчанию"
    /// </summary>
    public DataGridViewRow[] DefaultManualOrderRows { get { return _DefaultManualOrderRows; } set { _DefaultManualOrderRows = value; } }
    private DataGridViewRow[] _DefaultManualOrderRows;

    /// <summary>
    /// Изменение порядка строк на основании их расположени в коллекции
    /// </summary>
    /// <param name="down"></param>
    /// <returns></returns>
    private bool DoReorderRows(bool down)
    {
      throw new NotImplementedException();
      /*
// 1. Загружаем полный список строк DataGridViewRow в массив
DataGridViewRow[] Rows1 = new DataGridViewRow[Owner.Control.Rows.Count];
Owner.Control.Rows.CopyTo(Rows1, 0);

// 2. Запоминаем выбранные строки
DataGridViewRow[] SelRows = Owner.SelectedGridRows;
// 3. Получаем позиции выбранных строк в массиве всех строк
int[] SelPoss = Owner.SelectedRowIndices;
if (SelPoss.Length == 0)
{
EFPApp.ShowTempMessage("Нет ни одной выбранной строки, которую надо перемещать");
return false;
}

// 4. Проверяем, что не уперлись в границы списка
bool lBound = false;
if (Down)
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
if (Down)
msg += "вниз";
else
msg += "вверх";
EFPApp.ShowTempMessage(msg);
return false;
}

// 5. Подготавливаем массив строк для их размещения в новом порядке
// Значения null в этом массиве означают временно пустые позиции
DataGridViewRow[] Rows2 = new DataGridViewRow[Rows1.Length];

// 6. Копируем в Rows2 строки из Rows1 со сдвигом для позиций, существующих
// в SelRows.
// В процессе перемещения будем очищать массив Rows1
int Delta = Down ? 1 : -1; // значение смещения
int i;
for (i = 0; i < SelPoss.Length; i++)
{
int ThisPos = SelPoss[i];
Rows2[ThisPos + Delta] = Rows1[ThisPos];
Rows1[ThisPos] = null;
}

// 7. Перебираем исходный массив и оставшиеся непустые строки размещаем в
// новом массиве, отыскивая пустые места. Для этого используем переменную FreePos
// для указания на очередную пустую позицию второго массива
int FreePos = 0;
for (i = 0; i < Rows1.Length; i++)
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

// 8. Замещаем коллекцию строк
Owner.Control.Rows.Clear();
Owner.Control.Rows.AddRange(Rows2);

// 9. Восстанавливаем выбор
Owner.SelectedGridRows = SelRows;     
return true;                          */
    }

    /// <summary>
    /// Восстановление порядка по умолчанию на основании DefaultManualOrderRows
    /// </summary>
    /// <returns></returns>
    private bool DoSortRestoreRows()
    {
      throw new NotImplementedException();

      /*
      if (DefaultManualOrderRows == null)
        throw new NullReferenceException("Свойство DefaultManulOrderRows не установлено");

      // 1. Загружаем полный список строк DataGridViewRow в массив
      DataGridViewRow[] Rows1 = new DataGridViewRow[Owner.Control.Rows.Count];
      Owner.Control.Rows.CopyTo(Rows1, 0);

      // 2. Запоминаем выбранные строки
      DataGridViewRow[] SelRows = Owner.SelectedGridRows;

      // 3. Подготавливаем массив строк для их размещения в новом порядке
      // Значения null в этом массиве означают временно пустые позиции
      DataGridViewRow[] Rows2 = new DataGridViewRow[Rows1.Length];

      // 4. Копируем строки из массива по умолчанию
      int i;
      int cnt = 0;
      bool Changed = false;
      for (i = 0; i < DefaultManualOrderRows.Length; i++)
      {
        int ThisPos = Array.IndexOf<DataGridViewRow>(Rows1, DefaultManualOrderRows[i]);
        if (ThisPos < 0)
          continue; // Ошибка

        Rows2[cnt] = Rows1[ThisPos];
        Rows1[ThisPos] = null;
        if (cnt != ThisPos)
          Changed = true;
        cnt++;
      }

      // 5. Копируем "лишние" строки, которых нет в массиве по умолчанию
      for (i = 0; i < Rows1.Length; i++)
      {
        if (Rows1[i] != null)
        {
          Rows2[cnt] = Rows1[i];
          if (cnt != i)
            Changed = true;
          cnt++;
        }
      }

      if (!Changed)
        return false;

      // 6. Замещаем коллекцию строк
      Owner.Control.Rows.Clear();
      Owner.Control.Rows.AddRange(Rows2);

      // 7. Восстанавливаем выбор
      Owner.SelectedGridRows = SelRows;
      return true; */
    }

    #endregion

    #region Перестановка с помощью числового поля

    #region Свойства

    /// <summary>
    /// Имя числового столбца, который определяет порядок строк при ручной сортировке
    /// Если задано, то в меню есть команды ручной сортировки
    /// </summary>
    public string ManualOrderColumn { get { return _ManualOrderColumn; } set { _ManualOrderColumn = value; } }
    private string _ManualOrderColumn;

    /// <summary>
    /// Имя числового столбца, который определяет порядок строк по умолчанию при ручной
    /// сортировке. Свойство действует при непустом значении ManualOrderColumn.
    /// Если имя присвоено, то действует команда "Восстановить порядок по умолчанию"
    /// </summary>
    public string DefaultManualOrderColumn { get { return _DefaultManualOrderColumn; } set { _DefaultManualOrderColumn = value; } }
    private string _DefaultManualOrderColumn;

    #endregion

    #region Событие DataReorderHelperNeeded

    /// <summary>
    /// Событие вызывается при выполнении команд ручной сортировки, если установлено свойство ManualOrderColumn.
    /// Обработчик события может создать собственный экземпляр и установить свойство Helper.
    /// Если обработчика нет, или он не создал объект, то создается объект по умолчанию
    /// </summary>
    public event DataReorderHelperNeededEventHandler DataReorderHelperNeeded;

    /// <summary>
    /// Вызывает обработчик события DataReorderHelperNeeded.
    /// Если IDataReorderHelper не получен, объект создается вызовом метода EFPDataGridView.CreateDataReorderHelper().
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnDataReorderHelperNeeded(DataReorderHelperNeededEventArgs args)
    {
      if (DataReorderHelperNeeded != null)
        DataReorderHelperNeeded(this, args);
      if (args.Helper == null)
        args.Helper = ControlProvider.CreateDataReorderHelper();
    }


    /// <summary>
    /// Создает объект, реализующий интерфейс IDataReorderHelper.
    /// Если свойство ManualOrderColumn не установлено, выбрасывается исключение.
    /// </summary>
    /// <returns>Объект IDataReorderHelper</returns>
    public IDataReorderHelper CreateDataReorderHelper()
    {
      DataReorderHelperNeededEventArgs args = new DataReorderHelperNeededEventArgs();
      OnDataReorderHelperNeeded(args);
      if (args.Helper == null)
        throw new NullReferenceException("Объект, реализующий IDataReorderHelper, не был создан");
      return args.Helper;
    }

    /// <summary>
    /// Создает объект, реализующий интерфейс IDataReorderHelper.
    /// Если свойство ManualOrderColumn не установлено, выбрасывается исключение.
    /// Эта перегрузка используется для реализации обработчика события у другого объекта EFPDataGrid/TreeViewCommandItems,
    /// если оба просмотра должны использовать одинаковые вспомогательные объекты для сортировки строк.
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Аргументы события</param>
    public void CreateDataReorderHelper(object sender, DataReorderHelperNeededEventArgs args)
    {
      args.Helper = CreateDataReorderHelper();
    }

    #endregion

    #region Перестановка строк

    /// <summary>
    /// Изменение порядка строк на основании числового поля
    /// </summary>
    /// <param name="down"></param>
    /// <returns></returns>
    private bool DoReorderColumn(bool down)
    {
      IDataTableTreeModel model = ControlProvider.Control.Model as IDataTableTreeModel;
      if (model == null)
        throw new NullReferenceException("Модель данных не является IDataTableTreeModel");

      IDataReorderHelper helper = CreateDataReorderHelper();

      //Загружаем выбранные строки
      DataRow[] selRows = ControlProvider.SelectedDataRows;
      if (selRows.Length == 0)
      {
        EFPApp.ShowTempMessage("Нет ни одной выбранной строки, которую надо перемещать");
        return false;
      }

      bool res;
      if (down)
        res = helper.MoveDown(selRows);
      else
        res = helper.MoveUp(selRows);

      if (!res)
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

      // Обновляем просмотр
      //ControlProvider.PerformRefresh();
      //Model.Refresh();
      /*
      if (Owner.Control.CurrentCell != null)
      {
        if (Owner.Control.CurrentCell.IsInEditMode)
        {
          DataGridViewCell Cell = Owner.Control.CurrentCell;
          Owner.Control.CurrentCell = null;
          Owner.Control.CurrentCell = Cell;
        }
      } */

      ControlProvider.SelectedDataRows = selRows; // восстанавливаем выбранные строки

      return true;
    }

    #endregion

    #region Восстановление порядка

    /// <summary>
    /// Восстановление порядка по умолчанию на основании DefaultManualOrderColumn
    /// </summary>
    /// <returns></returns>
    private bool DoSortRestoreColumn()
    {
      if (String.IsNullOrEmpty(DefaultManualOrderColumn))
        throw new NullReferenceException("Свойство DefaultManualOrderColumn не установлено");

      DataRow[] desiredOrder;
      using (DataView dv = new DataView(ControlProvider.SourceAsDataTable))
      {
        dv.Sort = DefaultManualOrderColumn;
        desiredOrder = DataTools.GetDataViewRows(dv);
      }

      IDataReorderHelper helper = CreateDataReorderHelper();
      bool changed = helper.Reorder(desiredOrder);

      // Обновляем просмотр
      //if (changed)
      //  ControlProvider.Control.Refresh();

      return changed;
    }

    #endregion

    #endregion

    #endregion

    #endregion

    #region Отправить

    /// <summary>
    /// Меню "Отправить"
    /// </summary>
    public EFPCommandItem MenuSendTo { get { return _MenuSendTo; } }
    private EFPCommandItem _MenuSendTo;

    internal EFPCommandItem ciSendToMicrosoftExcel, ciSendToOpenOfficeCalc;

    /// <summary>
    /// Выбранные пользователем настройки экспорта в Excel / Calc
    /// </summary>
    private static EFPDataGridViewExpExcelSettings _SendToSettings = new EFPDataGridViewExpExcelSettings();

    void ciSendToMicrosoftExcel_Click(object sender, EventArgs args)
    {
      OnSendToMicrosoftExcel();
    }

    /// <summary>
    /// Не реализовано
    /// </summary>
    protected virtual void OnSendToMicrosoftExcel()
    {
      throw new NotImplementedException();

      /*
      EFPDataGridViewExpExcelForm Form = new EFPDataGridViewExpExcelForm();
      Form.Text = "Отправить в " + ciSendToMicrosoftExcel.MenuTextWithoutMnemonic;
      Form.Icon = EFPApp.MainImageIcon(ciSendToMicrosoftExcel.ImageKey);
      Form.LoadValues(SendToSettings);
      if (EFPApp.ShowDialog(Form, true) != DialogResult.OK)
        return;

      Form.SaveValues(SendToSettings);

      Owner.SendToMicrosoftExcel(SendToSettings);
      */
    }

    void ciSendToOpenOfficeCalc_Click(object sender, EventArgs args)
    {
      OnSendToOpenOfficeCalc();
    }

    /// <summary>
    /// Не реализовано
    /// </summary>
    protected virtual void OnSendToOpenOfficeCalc()
    {
      throw new NotImplementedException();
      /*
      EFPDataGridViewExpExcelForm Form = new EFPDataGridViewExpExcelForm();
      Form.Text = "Отправить в " + ciSendToOpenOfficeCalc.MenuTextWithoutMnemonic;
      Form.Icon = EFPApp.MainImageIcon(ciSendToOpenOfficeCalc.ImageKey);
      Form.LoadValues(SendToSettings);
      if (EFPApp.ShowDialog(Form, true) != DialogResult.OK)
        return;

      Form.SaveValues(SendToSettings);

      Owner.SendToOpenOfficeCalc(SendToSettings);
       */
    }


    #endregion
  }
}
