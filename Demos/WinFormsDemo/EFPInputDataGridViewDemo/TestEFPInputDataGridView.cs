using FreeLibSet.Formatting;
using FreeLibSet.Forms;
using FreeLibSet.UICore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace WinFormsDemo.EFPInputDataGridViewDemo
{
  public class TestEFPInputDataGridView
  {
    private static DataTable _Table;

    internal static void PerformTest()
    {
      if (_Table == null)
      {
        _Table = new DataTable();
        _Table.Columns.Add("F1", typeof(string));
        _Table.Columns.Add("F2", typeof(decimal));
        _Table.Columns.Add("F3", typeof(DateTime));
        _Table.Columns["F1"].Caption = "F1 (string)";
        _Table.Columns["F2"].Caption = "F2 (decimal)";
        _Table.Columns["F3"].Caption = "F3 (date)";
      }

      SimpleGridForm form = new SimpleGridForm();
      EFPInputDataGridView efpGrid = new EFPInputDataGridView(form.ControlWithToolBar);

      UIInputGridData data = new UIInputGridData(_Table);
      data.Columns["F1"].FillWeight = 100;
      data.Columns["F2"].Format = "0.00";
      data.Columns["F2"].TextWidth = 12;
      data.Columns["F3"].Format = EditableDateTimeFormatters.Date.Format;
      data.Columns["F3"].TextWidth = EditableDateTimeFormatters.Date.TextWidth;

      efpGrid.Data = data;

      EFPApp.ShowDialog(form, true);
    }
  }
}
