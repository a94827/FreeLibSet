﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.Data;
using FreeLibSet.Forms;
using FreeLibSet.Forms.Data;

namespace DBxGridFiltersDemo
{
  static class Program
  {
    #region Main()

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      EFPApp.InitApp();
      try
      {
        DataTable table = CreateTestTable();
        SimpleGridForm form = CreateTestForm(table);
        Application.Run(form);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка запуска программы");
      }
    }

    #endregion

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
        DataTools.FillArray<object>(a, DBNull.Value);

        a[0] = aS[i % aS.Length];
        int i1 = i % 37;
        if ((i%4)>0)
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

      DBxGridFilters filters = new DBxGridFilters();
      filters.SqlFilterRequired = true;

      StringValueGridFilter filtS1 = new StringValueGridFilter("S1");
      filters.Add(filtS1);

      IntRangeInclusionGridFilter filtI12 = new IntRangeInclusionGridFilter("I1", "I2");
      filters.Add(filtI12);

      DateRangeCrossGridFilter filtD12 = new DateRangeCrossGridFilter("D1", "D2");
      filters.Add(filtD12);

      BoolValueGridFilter filtL1 = new BoolValueGridFilter("L1");
      filters.Add(filtL1);

      NullNotNullGridFilter filtL1nn = new NullNotNullGridFilter("L1", typeof(bool));
      filtL1nn.Code = "L1_NN";
      filtL1nn.DisplayName = "L1=null?";
      filtL1nn.FilterTextNull = "NULL";
      filtL1nn.FilterTextNotNull = "NOT NULL";
      filters.Add(filtL1nn);

      efpGrid.Filters = filters;
      efpGrid.AfterSetFilter += EfpGrid_AfterSetFilter;

      EFPGridFilterGridView efpFilt = new EFPGridFilterGridView(efpGrid, form.FilterGrid);

      efpGrid.ReadOnly = true;
      efpGrid.Control.ReadOnly = true;
      efpGrid.CanView = false;

      efpGrid.Control.DataSource = table.DefaultView;
      return form;
    }

    private static void EfpGrid_AfterSetFilter(object sender, EventArgs args)
    {
      EFPConfigurableDataGridView efpGrid = (EFPConfigurableDataGridView)sender;
      DBxGridFilters filters = (DBxGridFilters)(efpGrid.Filters);
      efpGrid.SourceAsDataView.RowFilter = filters.DataViewRowFilter;
    }

    #endregion
  }
}
