using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Logging;
using FreeLibSet.DependedValues;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// ����� ������� ���������� ����� ���������� ���������
  /// </summary>
  internal partial class EFPDataGridViewOrderForm : Form
  {
    #region �����������

    internal EFPDataGridViewOrderForm(EFPDataGridView controlProvider)
    {
      _ControlProvider = controlProvider;
      InitializeComponent();
      Icon = EFPApp.MainImageIcon("OrderAZ");
      EFPFormProvider efpForm = new EFPFormProvider(this);
      efpForm.ConfigSectionName = "EFPDataGridViewOrderForm"; // ��� ���������� �������� �������

      ThetabControl.ImageList = EFPApp.MainImages;
      EFPTabControl efpTabControl = new EFPTabControl(efpForm, ThetabControl);

      #region ���������������� �������

      if (controlProvider.Orders.Count > 0)
      {
        tpFixed.ImageKey = "OrderAZ";

        efpFixed = new EFPDataGridView(efpForm, grFixed);
        grFixed.RowHeadersVisible = false;
        grFixed.ColumnHeadersVisible = false;
        efpFixed.Columns.AddImage("Image");
        efpFixed.Columns.AddTextFill("DisplayName");
        efpFixed.DisableOrdering();
        efpFixed.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(efpFixed_GetCellAttributes);
        grFixed.RowCount = controlProvider.Orders.Count;
        efpFixed.ReadOnly = true;
        efpFixed.Control.ReadOnly = true;
        efpFixed.CanView = false;
        efpFixed.CommandItems.EnterAsOk = true;
        grFixed.MultiSelect = false;
        grFixed.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      }
      else
        ThetabControl.TabPages.Remove(tpFixed);

      #endregion

      #region ������������ ����������

      if (controlProvider.CustomOrderAllowed)
      {
        tpCustom.ImageKey = "OrderCustom";

        EFPDataGridView efpAvailable = new EFPDataGridView(efpForm, grAvailable);

        btnAdd.Image = EFPApp.MainImages.Images["RightRight"];
        btnAdd.ImageAlign = ContentAlignment.MiddleCenter;
        EFPButton efpAdd = new EFPButton(efpForm, btnAdd);
        efpAdd.DisplayName = "��������";
        efpAdd.ToolTipText = "��������� ������� � ������ ��������� ��� ����������";

        btnRemove.Image = EFPApp.MainImages.Images["LeftLeft"];
        btnRemove.ImageAlign = ContentAlignment.MiddleCenter;
        EFPButton efpRemove = new EFPButton(efpForm, btnRemove);
        efpRemove.DisplayName = "�������";
        efpRemove.ToolTipText = "������� ������� �� ����������";

        EFPDataGridView efpSelected = new EFPDataGridView(efpForm, grSelected);
        efpSelected.ToolBarPanel = panSpbSelected;

        efpCustom = new EFPTwoListSelector(efpAvailable, efpAdd, efpRemove, efpSelected);
        efpCustom.ItemInfoNeeded += new EFPTwoListSelectorItemInfoNeededEventHandler(efpCustom_ItemInfoNeeded);
        efpCustom.EditItem += new EFPTwoListSelectorEditItemEventHandler(efpCustom_EditItem);

        List<SortItem> lstAll = new List<SortItem>();
        // ������� ������������ ���� ����� ��� ���������� � EFPDataGridViewColumn
        Dictionary<string, EFPDataGridViewColumn> columnsForSort = new Dictionary<string, EFPDataGridViewColumn>();

        foreach (EFPDataGridViewColumn col in controlProvider.VisibleColumns)
        {
          if (col.CustomOrderAllowed)
          {
            SortItem item = new SortItem();
            item.Column = col;
            item.Direction = ListSortDirection.Ascending;
            lstAll.Add(item);

            columnsForSort[col.CustomOrderColumnName] = col;
          }
        }
        efpCustom.AllItems = lstAll.ToArray();

        try
        {
          if (controlProvider.CurrentOrder != null)
          {
            string[] columnNames;
            ListSortDirection[] directions;
            DataTools.GetDataViewSortColumnNames(controlProvider.CurrentOrder.Sort, out columnNames, out directions);
            SortItem[] a = new SortItem[columnNames.Length];

            for (int i = 0; i < columnNames.Length; i++)
            {
              EFPDataGridViewColumn col;
              if (!columnsForSort.TryGetValue(columnNames[i], out col))
              {
                // � ���������������� ������� ���������� ����������� �������, ������� ������ ������������ ��� ������������ ����������
                a = null;
                break;
              }

              a[i] = new SortItem();
              a[i].Column = col;
              a[i].Direction = directions[i];
            }
            if (a != null)
              efpCustom.SelectedItems = a;
          }
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "������ ��������� ��������� ����������");
        }

        efpCustom.CanBeEmptyEx = new DepNot(efpTabControl.TabPages[tpCustom].SelectedEx);
      }
      else
        ThetabControl.TabPages.Remove(tpCustom);

      #endregion
    }

    #endregion

    private EFPDataGridView _ControlProvider;

    #region ���������������� �������

    EFPDataGridView efpFixed;

    void efpFixed_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.RowIndex < 0)
        return;

      EFPDataViewOrder order = _ControlProvider.Orders[args.RowIndex];
      switch (args.ColumnIndex)
      {
        case 0:
          args.Value = EFPApp.MainImages.Images[order.ImageKey];
          break;
        case 1:
          args.Value = (args.RowIndex + 1).ToString() + ". " + order.DisplayName;
          break;
      }
    }

    #endregion

    #region ������������ ����������

    EFPTwoListSelector efpCustom;

    /// <summary>
    /// ���� "�������-������� ����������".
    /// ������ ������������ ����������� ����� ListSortDescription, ��� ��� ��� ������������ ������ �� PropertyDescriptor
    /// </summary>
    private class SortItem
    {
      #region ����

      public EFPDataGridViewColumn Column;

      public ListSortDirection Direction;

      #endregion

      #region ������

      public override string ToString()
      {
        return Column.Name;
      }

      public override bool Equals(object obj)
      {
        SortItem item2 = obj as SortItem;
        return this == item2;
      }

      public static bool operator ==(SortItem a, SortItem b)
      {
        //return Object.ReferenceEquals(a.Column, b.Column);

        // � ��������� ����� ���� ��� �������, ���������� ������� ����������� ���������
        // (��������, ��������� ������������� ������������ � ������)
        return String.Equals(a.Column.CustomOrderColumnName, b.Column.CustomOrderColumnName, StringComparison.Ordinal);
      }

      public static bool operator !=(SortItem a, SortItem b)
      {
        return !(a==b);
      }

      public override int GetHashCode()
      {
        return Column.GridColumn.Index;
      }

      #endregion
    }

    void efpCustom_ItemInfoNeeded(object sender, EFPTwoListSelectorItemInfoNeededEventArgs args)
    {
      SortItem item = (SortItem)(args.Item);
      if (args.IsSelected)
      {
        args.TextValue = item.Column.DisplayName + (item.Direction == ListSortDirection.Ascending ? " (�� �����������)" : " (�� ��������)");
        args.ImageKey = (item.Direction == ListSortDirection.Ascending) ? "Up" : "Down";
      }
      else
      {
        args.TextValue = item.Column.DisplayName;
        args.ImageKey = "Item";
      }

    }


    void efpCustom_EditItem(object sender, EFPTwoListSelectorEditItemEventArgs args)
    {
      SortItem item = (SortItem)(args.Item);
      if (item.Direction == ListSortDirection.Ascending)
        item.Direction = ListSortDirection.Descending;
      else
        item.Direction = ListSortDirection.Ascending;
    }

    #endregion

    #region ����������� �����

    internal static bool PerformSort(EFPDataGridView controlProvider)
    {
      using (EFPDataGridViewOrderForm frm = new EFPDataGridViewOrderForm(controlProvider))
      {
        if (controlProvider.CustomOrderActive)
        {
          frm.ThetabControl.SelectedTab = frm.tpCustom;
        }
        else
        {
          frm.ThetabControl.SelectedTab = frm.tpFixed;
          if (controlProvider.CurrentOrderIndex >= 0)
            frm.efpFixed.CurrentRowIndex = controlProvider.CurrentOrderIndex;
        }

        if (EFPApp.ShowDialog(frm, false) == DialogResult.OK)
        {
          if (frm.ThetabControl.SelectedTab == frm.tpFixed)
          {
            controlProvider.CurrentOrderIndex = frm.efpFixed.CurrentRowIndex;
          }
          else
          {
            // �������� ����������
            controlProvider.CustomOrderActive = true;
            string[] columnNames = new string[frm.efpCustom.SelectedItems.Length];
            ListSortDirection[] directions = new ListSortDirection[frm.efpCustom.SelectedItems.Length];
            for (int i = 0; i < frm.efpCustom.SelectedItems.Length; i++)
            {
              SortItem item = (SortItem)(frm.efpCustom.SelectedItems[i]);
              columnNames[i] = item.Column.CustomOrderColumnName;
              directions[i] = item.Direction;
            }
            string sort = DataTools.GetDataViewSort(columnNames, directions);
            EFPDataViewOrder order = new EFPDataViewOrder(sort);
            order.DisplayName = controlProvider.GetOrderDisplayName(order);
            controlProvider.CurrentOrder = order;
          }
          return true;
        }
      }

      return false;
    }

    #endregion
  }
}