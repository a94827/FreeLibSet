// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Logging;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Реализация автоподбора высоты строки
  /// </summary>
  internal class EFPDataGridViewRowsResizer
  {
    #region Конструктор

    public EFPDataGridViewRowsResizer(EFPDataGridView controlProvider)
    {
      _Control = controlProvider.Control;
      controlProvider.Idle += new EventHandler(ControlProvider_Idle);
      controlProvider.Control.SizeChanged += new EventHandler(Control_SizeChanged);
      controlProvider.Control.ColumnWidthChanged += new System.Windows.Forms.DataGridViewColumnEventHandler(Control_ColumnWidthChanged);
      controlProvider.Control.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(Control_DataBindingComplete);
      _FirstDisplayedRowIndex = -1;
    }

    #endregion

    #region Свойства

    private DataGridView _Control;

    /// <summary>
    /// Индекс первой отображаемой строки просмотра, который был при последнем вызове CalcRowHeights().
    /// Если он не изменился с момента последнего вызова, никаих действий не выполняется
    /// </summary>
    private int _FirstDisplayedRowIndex;

    private static bool _CalcRowHeightsExceptionHandled = false;

    #endregion

    #region Обработчики

    void Control_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs args)
    {
      _FirstDisplayedRowIndex = -1;
    }

    void Control_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs args)
    {
      _FirstDisplayedRowIndex = -1;
    }

    void Control_SizeChanged(object sender, EventArgs args)
    {
      _FirstDisplayedRowIndex = -1;
    }

    void ControlProvider_Idle(object sender, EventArgs args)
    {
      CalcRowHeights();
    }

    #endregion

    #region Подбор высоты

    /// <summary>
    /// Расчет высоты строк, видимых в просмотре
    /// </summary>
    private void CalcRowHeights()
    {
      if (_Control.FirstDisplayedScrollingRowIndex == _FirstDisplayedRowIndex)
        return; // ничего не изменилось

      EFPApp.BeginWait("Вычисление высоты строк");
      try
      {
        // Нельзя использовать цикл for, так как в процессе расчета может меняться высота строк и,
        // соответственно, число отображаемых строк.
        int rowIndex = _Control.FirstDisplayedScrollingRowIndex;
        if (rowIndex < 0)
          rowIndex = 0;
        while (rowIndex < _Control.RowCount)
        {
          DataGridViewRow row = _Control.Rows[rowIndex];
          if ((row.State & DataGridViewElementStates.Displayed) == 0)
            break;

          int h = row.GetPreferredHeight(rowIndex, DataGridViewAutoSizeRowMode.AllCells, true);
          row.Height = h;

          rowIndex++;
        }
      }
      catch (Exception e)
      {
        if (!_CalcRowHeightsExceptionHandled)
        {
          _CalcRowHeightsExceptionHandled = true;
          LogoutTools.LogoutException(e, "Ошибка расчета высоты строк табличного просмотра. Повторные ошибки не регистрируются");
        }
      }
      EFPApp.EndWait();

      _FirstDisplayedRowIndex = _Control.FirstDisplayedScrollingRowIndex;
    }

    #endregion
  }
}
