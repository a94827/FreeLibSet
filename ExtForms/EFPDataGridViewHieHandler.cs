using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

#if XXX
namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// Переходник между EFPDataGridView и EFPHieView.
  /// Ссылка на табличный просмотр задается в конструкторе, а на объект EFPHieView может задаваться динамически
  /// или отсутствовать
  /// </summary>
  public class EFPDataGridViewHieHandler
  {
#region Конструктор

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

#region Свойства

    /// <summary>
    /// Провайдер табличного просмотра.
    /// Свойство задается в конструкторе
    /// </summary>
    public EFPDataGridView ControlProvider { get { return FControlProvider; } }
    private EFPDataGridView FControlProvider;

    /// <summary>
    /// Данные иерархического просмотра. 
    /// Установка значения свойства приводит к установке источника данных DataGridView.DataSource.
    /// При необходимости, для присоединяемого EFPHieView вызывается CreateDataView()
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
    /// При установке свойства в значение true меняются между собой цвета строк
    /// промежуточных итогов (зеленые и сиреневые строки).
    /// По умолчанию используются зеленые строки для уровней 1,3,5, ... и сиреневые для 2,4,6
    /// </summary>
    public bool SwapSubTotalColors
    {
      get { return FSwapSubTotalColors; }
      set { FSwapSubTotalColors = value; }
    }
    private bool FSwapSubTotalColors;

#endregion

#region Форматирование просмотра

    /// <summary>
    /// Если true, то рисовать линии для уровня 1
    /// !!! Сделать настройку !!!
    /// </summary>
    private bool Level1Lines; // пока не используется

    private void GetGridRowAttributes(object Sender, EFPDataGridViewRowAttributesEventArgs Args)
    {
      if (Args.RowIndex < 0)
        return;

      if (FHieView == null)
        return;

      // Цвета в таблице
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
        Args.ReadOnlyMessage = "Нельзя редактировать строки итогов и подытогов";
      }
    }

    private void GetGridCellAttributes(object Sender, EFPDataGridViewCellAttributesEventArgs Args)
    {
      if (Args.RowIndex < 0)
        return;
      if (FHieView == null)
        return;


      // Форматирование текста
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

#region Редактирование данных
#if XXX

    /// <summary>
    /// Этот метод может быть вызван из обработчика GridHandler.EditData в табличном
    /// просмотре отчета. 
    /// Отыскивается уровень иерархии, соответствующий текущей строке. В нем вызывается
    /// событие EditRow
    /// Режим "Редактирование" - "Просмотр" определяется свойством GridHandler.State
    /// </summary>
    /// <param name="ControlProvider">Табличный просмотр для таблицы, созданный CreateResTable()</param>
    /// <returns>true, если данные были изменены</returns>
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
          EFPApp.ShowTempMessage("Неподдерживаемый режим редактирования");
          return false;
      }
      return EditReportRow(ReadOnly);
    }


    /// <summary>
    /// Этот метод может быть вызван из обработчика GridHandler.EditData в табличном
    /// просмотре отчета. 
    /// Отыскивается уровень иерархии, соответствующий текущей строке. В нем вызывается
    /// событие EditRow
    /// </summary>
    /// <param name="ControlProvider">Табличный просмотр для таблицы, созданный CreateResTable()</param>
    /// <param name="ReadOnly">true-режим просмотра, false-редактирование</param>
    /// <returns>true, если данные были изменены</returns>
    private bool EditReportRow(bool ReadOnly)
    {
      if (!ControlProvider.CheckSingleRow())
        return false;

      int LevelIndex = GetLevelIndex(ControlProvider.CurrentDataRow);
      if (LevelIndex < 0)
      {
        EFPApp.ShowTempMessage("Нельзя редактировать итоговую строку");
        return false;
      }

      return Levels[LevelIndex].OnEditRow(ControlProvider.CurrentDataRow, ReadOnly);
    }
#endif

#endregion
  }
}
#endif

