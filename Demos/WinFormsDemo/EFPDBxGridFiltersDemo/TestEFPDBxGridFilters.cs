using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FreeLibSet.Core;
using FreeLibSet.Data;
using FreeLibSet.Forms;
using FreeLibSet.Forms.Data;

namespace WinFormsDemo.EFPDBxGridFiltersDemo
{
  class TestEFPDBxGridFilters
  {
    public static void PerfotmTest()
    {
      DataTable table = CreateTestTable();
      SimpleGridForm form = CreateTestForm(table);
      EFPApp.ShowDialog(form, true);
    }

    #region Тестовая таблица

    private static DataTable CreateTestTable()
    {
      DataTable table = new DataTable();
      table.Columns.Add("S1", typeof(string));
      table.Columns.Add("I1", typeof(int));
      table.Columns.Add("I2", typeof(int));
      table.Columns.Add("D1", typeof(DateTime));
      table.Columns.Add("D2", typeof(DateTime));
      table.Columns.Add("L1", typeof(bool));

      string[] aS = new string[] { "One", "Two", "Three", "Four", "Five" };

      for (int i = 0; i < 1000; i++)
      {
        object[] a = new object[6];
        ArrayTools.FillArray<object>(a, DBNull.Value);

        a[0] = aS[i % aS.Length];
        int i1 = i % 37;
        if ((i % 4) > 0)
          a[1] = i1;
        if ((i % 5) > 0)
          a[2] = i1 + 5;
        DateTime dt1 = DateTime.Today.AddDays(i);
        DateTime dt2 = dt1.AddDays(4);
        if ((i % 6) > 0)
          a[3] = dt1;
        if ((i % 7) > 0)
          a[4] = dt2;

        bool l1 = (i % 2) > 0;
        if ((i % 3) > 0)
          a[5] = l1;

        table.Rows.Add(a);
      }
      table.AcceptChanges();
      return table;
    }

    #endregion

    #region Форма

    private static SimpleGridForm CreateTestForm(DataTable table)
    {
      SimpleGridForm form = new SimpleGridForm();
      EFPConfigurableDataGridView efpGrid = new EFPConfigurableDataGridView(form.ControlWithToolBar);

      #region Фильтры

      EFPDBxGridFilters filters = new EFPDBxGridFilters();
      filters.SqlFilterRequired = true;

      StringValueGridFilter filtS1 = new StringValueGridFilter("S1");
      filters.Add(filtS1);

      IntRangeInclusionGridFilter filtI12 = new IntRangeInclusionGridFilter("I1", "I2");
      filters.Add(filtI12);

      DateRangeCrossGridFilter filtD12 = new DateRangeCrossGridFilter("D1", "D2");
      filters.Add(filtD12);

      BooleanValueGridFilter filtL1 = new BooleanValueGridFilter("L1");
      filters.Add(filtL1);

      NullNotNullGridFilter filtL1nn = new NullNotNullGridFilter("L1", typeof(bool));
      filtL1nn.Code = "L1_NN";
      filtL1nn.DisplayName = "L1=null?";
      filtL1nn.FilterTextNull = "NULL";
      filtL1nn.FilterTextNotNull = "NOT NULL";
      filters.Add(filtL1nn);

      efpGrid.Filters = filters;
      efpGrid.AfterSetFilter += EfpGrid_AfterSetFilter;

      #endregion

      #region Сортировка

      efpGrid.Orders.Add("", "No sort");
      efpGrid.Orders.Add("I1", "I1 (simple sort)");
      efpGrid.Orders.Add("S1 DESC,I1", "S1 DESC,I1");
      DBxExpression filtExpr1 = new DBxFunction(DBxFunctionKind.IIf, new DBxColumn("L1"), new DBxColumn("I1"), new DBxColumn("I2"));
      efpGrid.Orders.Add(new DBxOrder(filtExpr1), "IIF(L1, I1, I2)");
      DBxExpression filtExpr2 = new DBxFunction(DBxFunctionKind.Coalesce, new DBxColumn("D1"), new DBxColumn("D2"));
      efpGrid.Orders.Add(new DBxOrder(filtExpr2), "COALESCE(D1, D2)");
      efpGrid.AutoSort = true;

      #endregion

      efpGrid.CellInfoNeeded += EfpGrid_CellInfoNeeded;
      efpGrid.Control.ReadOnly = true;
      efpGrid.CanView = false;

      EFPGridFilterGridView efpFilt = new EFPGridFilterGridView(efpGrid, form.FilterGrid);

      efpGrid.ReadOnly = true;
      efpGrid.Control.ReadOnly = true;
      efpGrid.CanView = false;

      efpGrid.Control.DataSource = table.DefaultView;
      return form;
    }

    private static void EfpGrid_CellInfoNeeded(object sender, EFPDataGridViewCellInfoEventArgs args)
    {
      if (args.ColumnName == "L1")
      {
        if (args.DataRow.IsNull("L1"))
          args.ContentVisible = false;
      }
    }

    private static void EfpGrid_AfterSetFilter(object sender, EventArgs args)
    {
      EFPConfigurableDataGridView efpGrid = (EFPConfigurableDataGridView)sender;
      EFPDBxGridFilters filters = (EFPDBxGridFilters)(efpGrid.Filters);
      efpGrid.SourceAsDataView.RowFilter = filters.DataViewRowFilter;
    }

    #endregion
  }
}
