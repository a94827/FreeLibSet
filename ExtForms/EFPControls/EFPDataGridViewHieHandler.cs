using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

#if XXX
namespace FreeLibSet.Forms
{
  /// <summary>
  /// ���������� ����� EFPDataGridView � EFPHieView.
  /// ������ �� ��������� �������� �������� � ������������, � �� ������ EFPHieView ����� ���������� �����������
  /// ��� �������������
  /// </summary>
  public class EFPDataGridViewHieHandler
  {
#region �����������

    public EFPDataGridViewHieHandler(EFPDataGridView ControlProvider)
    {
      if (ControlProvider == null)
        throw new ArgumentNullException("ControlProvider");

      FSwapSubTotalColors = false;

      FControlProvider = ControlProvider;
      FControlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(GetGridRowAttributes);
      FControlProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(GetGridCellAttributes);
    }

#endregion

#region ��������

    /// <summary>
    /// ��������� ���������� ���������.
    /// �������� �������� � ������������
    /// </summary>
    public EFPDataGridView ControlProvider { get { return FControlProvider; } }
    private EFPDataGridView FControlProvider;

    /// <summary>
    /// ������ �������������� ���������. 
    /// ��������� �������� �������� �������� � ��������� ��������� ������ DataGridView.DataSource.
    /// ��� �������������, ��� ��������������� EFPHieView ���������� CreateDataView()
    /// </summary>
    public EFPHieView HieView
    {
      get { return FHieView; }
      set
      {
        if (value == null)
        {
          FHieView = null;
          FControlProvider.Control.DataSource = null;
        }
        else
        {
          if (value.DataView == null)
            value.CreateDataView();
          FHieView = value;
          FControlProvider.Control.DataSource = value.DataView;
        }
      }
    }
    private EFPHieView FHieView;

    /// <summary>
    /// ��� ��������� �������� � �������� true �������� ����� ����� ����� �����
    /// ������������� ������ (������� � ��������� ������).
    /// �� ��������� ������������ ������� ������ ��� ������� 1,3,5, ... � ��������� ��� 2,4,6
    /// </summary>
    public bool SwapSubTotalColors
    {
      get { return FSwapSubTotalColors; }
      set { FSwapSubTotalColors = value; }
    }
    private bool FSwapSubTotalColors;

#endregion

#region �������������� ���������

    /// <summary>
    /// ���� true, �� �������� ����� ��� ������ 1
    /// !!! ������� ��������� !!!
    /// </summary>
    private bool Level1Lines; // ���� �� ������������

    private void GetGridRowAttributes(object Sender, EFPDataGridViewRowAttributesEventArgs Args)
    {
      if (Args.RowIndex < 0)
        return;

      if (FHieView == null)
        return;

      // ����� � �������
      DataView dv = (DataView)(Args.ControlProvider.Control.DataSource);
      DataRow Row = dv[Args.RowIndex].Row;
      int lvl = (int)Row["Hie_Level"];

      Args.RowIdText = DataTools.GetString(Row, "Hie_Text");

      if (lvl > 0)
      {
        bool IsSum = (bool)Row["Hie_Sum"];
        if (IsSum)
        {
          if (lvl == FHieView.Levels.Length)
          {
            Args.ColorType = EFPDataGridViewColorType.TotalRow;
            Args.PrintWithPrevious = true;
          }
          else
          {
            bool Color2 = ((FHieView.Levels.Length - lvl) % 2) == 0;
            if (SwapSubTotalColors)
              Color2 = !Color2;
            if (Color2)
              Args.ColorType = EFPDataGridViewColorType.Total2;
            else
              Args.ColorType = EFPDataGridViewColorType.Total1;
            if (FHieView.Levels[lvl].SubTotalRowFirst)
            {
              Args.PrintWithNext = true;
              if (lvl == 1 && Level1Lines)
                Args.TopBorder = EFPDataGridViewBorderStyle.Thin;
            }
            else
            {
              Args.PrintWithPrevious = true;
              if (lvl == 1 && Level1Lines)
                Args.BottomBorder = EFPDataGridViewBorderStyle.Thin;
            }
          }
        }
        else
        {
          Args.ColorType = EFPDataGridViewColorType.Header;
          Args.PrintWithNext = true;
          if (lvl == 1 && Level1Lines)
            Args.TopBorder = EFPDataGridViewBorderStyle.Thin;
        }

        Args.ReadOnly = true; // 24.02.2013
        Args.ReadOnlyMessage = "������ ������������� ������ ������ � ���������";
      }
    }

    private void GetGridCellAttributes(object Sender, EFPDataGridViewCellAttributesEventArgs Args)
    {
      if (Args.RowIndex < 0)
        return;
      if (FHieView == null)
        return;


      // �������������� ������
      if (Args.ColumnName == "Hie_Text")
      {
        DataView dv = (DataView)(Args.Control.DataSource);
        DataRow Row = dv[Args.RowIndex].Row;
        int lvl = (int)Row["Hie_Level"];

        int off = FHieView.Levels.Length - lvl - 1;
        bool IsSum;
        if (lvl > 0)
          IsSum = (bool)Row["Hie_Sum"];
        else
          IsSum = false;
        if (IsSum)
          off++;

        //Args.ValueEx = new string(' ', off * 2) + (string)(Args.ValueEx);
        Args.IndentLevel = off;
        Args.FormattingApplied = true;
      }
    }

#endregion

#region �������������� ������
#if XXX

    /// <summary>
    /// ���� ����� ����� ���� ������ �� ����������� GridHandler.EditData � ���������
    /// ��������� ������. 
    /// ������������ ������� ��������, ��������������� ������� ������. � ��� ����������
    /// ������� EditRow
    /// ����� "��������������" - "��������" ������������ ��������� GridHandler.State
    /// </summary>
    /// <param name="ControlProvider">��������� �������� ��� �������, ��������� CreateResTable()</param>
    /// <returns>true, ���� ������ ���� ��������</returns>
    public bool EditReportRow()
    {
      if (HieView == null)
        return false;

      bool ReadOnly;
      switch (ControlProvider.State)
      {
        case EFPDataGridViewState.Edit: ReadOnly = false; break;
        case EFPDataGridViewState.View: ReadOnly = true; break;
        default:
          EFPApp.ShowTempMessage("���������������� ����� ��������������");
          return false;
      }
      return EditReportRow(ReadOnly);
    }


    /// <summary>
    /// ���� ����� ����� ���� ������ �� ����������� GridHandler.EditData � ���������
    /// ��������� ������. 
    /// ������������ ������� ��������, ��������������� ������� ������. � ��� ����������
    /// ������� EditRow
    /// </summary>
    /// <param name="ControlProvider">��������� �������� ��� �������, ��������� CreateResTable()</param>
    /// <param name="ReadOnly">true-����� ���������, false-��������������</param>
    /// <returns>true, ���� ������ ���� ��������</returns>
    private bool EditReportRow(bool ReadOnly)
    {
      if (!ControlProvider.CheckSingleRow())
        return false;

      int LevelIndex = GetLevelIndex(ControlProvider.CurrentDataRow);
      if (LevelIndex < 0)
      {
        EFPApp.ShowTempMessage("������ ������������� �������� ������");
        return false;
      }

      return Levels[LevelIndex].OnEditRow(ControlProvider.CurrentDataRow, ReadOnly);
    }
#endif

#endregion
  }
}
#endif
