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
  /// ���������� ����������� ������ ������
  /// </summary>
  internal class EFPDataGridViewRowsResizer
  {
    #region �����������

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

    #region ��������

    private DataGridView Control;

    /// <summary>
    /// ������ ������ ������������ ������ ���������, ������� ��� ��� ��������� ������ CalcRowHeights().
    /// ���� �� �� ��������� � ������� ���������� ������, ������ �������� �� �����������
    /// </summary>
    private int _FirstDisplayedRowIndex;

    private static bool _CalcRowHeightsExceptionHandled = false;

    #endregion

    #region �����������

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

    #region ������ ������

    /// <summary>
    /// ������ ������ �����, ������� � ���������
    /// </summary>
    private void CalcRowHeights()
    {
      if (Control.FirstDisplayedScrollingRowIndex == _FirstDisplayedRowIndex)
        return; // ������ �� ����������

      EFPApp.BeginWait("���������� ������ �����");
      try
      {
        // ������ ������������ ���� for, ��� ��� � �������� ������� ����� �������� ������ ����� �,
        // ��������������, ����� ������������ �����.
        int rowIndex = Control.FirstDisplayedScrollingRowIndex;
        if (rowIndex < 0)
          rowIndex = 0;
        while (rowIndex < Control.RowCount)
        {
          DataGridViewRow row = Control.Rows[rowIndex];
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
          LogoutTools.LogoutException(e, "������ ������� ������ ����� ���������� ���������. ��������� ������ �� ��������������");
        }
      }
      EFPApp.EndWait();

      _FirstDisplayedRowIndex = Control.FirstDisplayedScrollingRowIndex;
    }

    #endregion
  }
}
