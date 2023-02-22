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

    internal EFPCommandItem ciSortMoveUp, ciSortMoveDown, ciSortRestore;

    void ciSortMoveUp_Click(object sender, EventArgs args)
    {
      ControlProvider.ChangeManualOrder(false);
    }

    void ciSortMoveDown_Click(object sender, EventArgs args)
    {
      ControlProvider.ChangeManualOrder(true);
    }

    void ciSortRestore_Click(object Sender, EventArgs Args)
    {
      ControlProvider.RestoreManualOrder();
    }

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
