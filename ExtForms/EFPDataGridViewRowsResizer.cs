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
      Control = controlProvider.Control;
      controlProvider.Idle += new EventHandler(ControlProvider_Idle);
      controlProvider.Control.SizeChanged += new EventHandler(Control_SizeChanged);
      controlProvider.Control.ColumnWidthChanged += new System.Windows.Forms.DataGridViewColumnEventHandler(Control_ColumnWidthChanged);
      controlProvider.Control.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(Control_DataBindingComplete);
      _FirstDisplayedRowIndex = -1;
    }

    #endregion

    #region Свойства

    private DataGridView Control;

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
      if (Control.FirstDisplayedScrollingRowIndex == _FirstDisplayedRowIndex)
        return; // ничего не изменилось

      EFPApp.BeginWait("Вычисление высоты строк");
      try
      {
        // Нельзя использовать цикл for, так как в процессе расчета может меняться высота строк и,
        // соответственно, число отображаемых строк.
        int RowIndex = Control.FirstDisplayedScrollingRowIndex;
        if (RowIndex < 0)
          RowIndex = 0;
        while (RowIndex < Control.RowCount)
        {
          DataGridViewRow Row = Control.Rows[RowIndex];
          if ((Row.State & DataGridViewElementStates.Displayed) == 0)
            break;

          int H = Row.GetPreferredHeight(RowIndex, DataGridViewAutoSizeRowMode.AllCells, true);
          Row.Height = H;

          RowIndex++;
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

      _FirstDisplayedRowIndex = Control.FirstDisplayedScrollingRowIndex;
    }

    #endregion
  }
}
