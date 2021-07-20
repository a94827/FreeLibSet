using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using System.Drawing.Printing;
using System.Xml;
using System.IO;
using AgeyevAV;
using AgeyevAV.OLE.Excel;
using AgeyevAV.IO;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// Форма настройки параметров при передаче в Excel
  /// </summary>
  internal partial class EFPDataGridViewExpExcelForm : Form
  {
    #region Конструктор

    public EFPDataGridViewExpExcelForm()
    {
      InitializeComponent();
      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpArea = new EFPRadioButtons(efpForm, rbAll);
      efpHeaders = new EFPCheckBox(efpForm, cbHeaders);

      efpUseFill = new EFPCheckBox(efpForm, cbUseFill);
      efpUseBorders = new EFPCheckBox(efpForm, cbUseBorders);

      efpBoolMode = new EFPListComboBox(efpForm, cbBoolMode);
    }

    #endregion

    #region Поля

    public EFPRadioButtons efpArea;
    public EFPCheckBox efpHeaders;

    public EFPCheckBox efpUseFill;
    public EFPCheckBox efpUseBorders;

    public EFPListComboBox efpBoolMode;

    #endregion

    #region Инициализация видимости элементов

    /// <summary>
    /// Заблокировать лишние управляющие элементы формы, которые не оказывают влияния на экспорт из заданного
    /// табличного просмотра
    /// </summary>
    /// <param name="controlProvider">Табличный просмотр, настройка которого выполняется</param>
    public void InitVisibility(EFPDataGridView controlProvider)
    {
      efpHeaders.Enabled = controlProvider.Control.ColumnHeadersVisible;
      // не надежно. Могут быть переопределены методы On
      // efpUseFill.Enabled = ControlProvider.HasAttributeHandlers;

      bool HasBoolColumn = false;
      for (int i = 0; i < controlProvider.Control.Columns.Count; i++)
      {
        DataGridViewColumn Column = controlProvider.Control.Columns[i];
        if (!Column.Visible)
          continue;
        if (Column is DataGridViewCheckBoxColumn)
        {
          HasBoolColumn = true;
          break;
        }
        if (Column.ValueType == typeof(Boolean))
        {
          HasBoolColumn = true;
          break;
        }
      }
      efpBoolMode.Enabled = HasBoolColumn;
    }

    #endregion

    #region Чтение и запись значений

    public void LoadValues(EFPDataGridViewExpExcelSettings settings)
    {
      efpArea.SelectedIndex = settings.RangeMode == EFPDataGridViewExpRange.Selected ? 1 : 0;
      efpHeaders.Checked = settings.ShowColumnHeaders;

      efpUseFill.Checked = settings.UseInterior;
      efpUseBorders.Checked = settings.UseBorders;
      try
      {
        efpBoolMode.SelectedIndex = (int)(settings.BoolMode);
      }
      catch { }
    }

    public void SaveValues(EFPDataGridViewExpExcelSettings settings)
    {
      settings.RangeMode = efpArea.SelectedIndex == 1 ? EFPDataGridViewExpRange.Selected : EFPDataGridViewExpRange.All;
      settings.ShowColumnHeaders = efpHeaders.Checked;
      settings.UseInterior = efpUseFill.Checked;
      settings.UseBorders = efpUseBorders.Checked;
      settings.BoolMode = (EFPDataGridViewExpExcelBoolMode)(efpBoolMode.SelectedIndex);
    }

    #endregion
  }


  /// <summary>
  /// Передача табличного просмотра в Microsoft Excel
  /// Статические методы
  /// </summary>
  internal class EFPDataGridViewSendToExcel
  {
    #region Режим создания таблицы

    public static void SendTable(EFPDataGridView controlProvider, EFPDataGridViewExpExcelSettings settings)
    {
      bool WithFile = EFPApp.MicrosoftExcelVersion.Major >= MicrosoftOfficeTools.MicrosoftOffice_XP;
      //if (Settings.Mode != GridHandlerExpExcelMode.Table)
      //  WithFile = false;

      //#if DEBUG
      //          if (AccDepClientExec.DebugShowIds)
      //            EFPApp.MessageBox("Метод передачи в Excel: " +
      //              (WithFile ? " через временный XML-файл (быстрый)" : "через OLE (медленный)") +
      //              "\r\nВерсия Excel: " + Helper.Application.Version.ToString(),
      //              "Передача таблицы в MS Excel");
      //#endif

      if (WithFile)
      {
        Splash spl = new Splash(new string[] { 
          "Создание файла", 
          "Запуск Microsoft Excel"});
        try
        {
          // Передача через временный файл
          string FileName = EFPApp.SharedTempDir.GetTempFileName("xml").Path;
          controlProvider.SaveExcel2003(FileName, settings);
          spl.Complete();
          MicrosoftOfficeTools.OpenWithExcel(new AbsPath(FileName), true);
          spl.Complete();

          // Надо создать новую книгу и скопировать туда лист из исходной книги
          //Workbook wbk1=Helper.Application.ActiveWorkbook;
          //Workbook wbk2=Helper.Application.Workbooks.Add(1);
          //wbk2.Sheets.Add(
        }
        finally
        {
          spl.Close();
        }
      }
      else
      {
        Splash spl = new Splash(new string[] { 
        "Запуск Microsoft Excel",
        "Передача таблицы"});
        try
        {
          ExcelHelper Helper = new ExcelHelper(true);
          try
          {
            // Передача через OLE
            Workbook wbk = Helper.Application.Workbooks.Add(1);
            Worksheet Sheet = wbk.Sheets[1];
            spl.Complete();
            DoSendTable2(controlProvider, settings, Sheet, 1, 1, spl);
          }
          finally
          {
            Helper.Dispose();
          }
        }
        finally
        {
          spl.Close();
        }
      }
    }

    private static void DoSendTable2(EFPDataGridView controlProvider, EFPDataGridViewExpExcelSettings settings,
      Worksheet sheet, int firstRow, int firstCol, Splash spl)
    {
      // Корректируем параметры
      //if (Settings.Mode == GridHandlerExpExcelMode.Cells)
      //  Settings.ShowColumnHeaders = false;


      // Общие установки листа
      sheet.Cells.SetVerticalAlignment(XlVAlign.xlVAlignCenter);
      sheet.Cells.SetWrapText(true);
      // 1. Собираем массив печатаемых столбцов и строк
      EFPDataGridViewRectArea Area = controlProvider.GetRectArea(settings.RangeMode);

      int i, j;
      Range r = new Range();

      // Устанавливаем ширину столбцов в единицах ширины шрифта
      double[] Widthes = new double[Area.ColumnCount];
      for (i = 0; i < Area.ColumnCount; i++)
      {
        EFPDataGridViewColumn Column = controlProvider.Columns[Area.ColumnIndices[i]];
        Widthes[i] = Column.TextWidth;
      }
      sheet.SetColumnTextWidthes(Widthes);

      int CurrRow = firstRow;

      object[] RowBuffer = new object[Area.ColumnCount];
      // Заголовки 
      if (settings.ShowColumnHeaders)
      {
        for (j = 0; j < Area.ColumnCount; j++)
        {
          RowBuffer[j] = MyGetCellFormula(Area.Columns[j].HeaderText);
        }
        r = sheet.GetRange(CurrRow, firstCol, CurrRow, firstCol + Area.ColumnCount - 1);
        r.FormulaArray = RowBuffer;
        r.HorizontalAlignment = XlHAlign.xlHAlignCenter; // независимо от флага
        r.Font.SetBold(true);
        CurrRow++;
      }

      if (Area.RowCount > 0)
      {
        // Выравнивание и формат
        for (j = 0; j < Area.ColumnCount; j++)
        {
          r = sheet.GetRange(CurrRow, firstCol + j, CurrRow + Area.RowCount - 1, firstCol + j);
          // Выравнивание для столбца по умолчанию
          XlHAlign ColHAlign = GetHAlign(Area.Columns[j].DefaultCellStyle.Alignment);
          XlVAlign ColVAlign = GetVAlign(Area.Columns[j].DefaultCellStyle.Alignment);
          r.HorizontalAlignment = ColHAlign;
          if (ColVAlign != XlVAlign.xlVAlignCenter)
            r.VerticalAlignment = ColVAlign;

          string Format = Area.Columns[j].DefaultCellStyle.Format;
          r.NumberFormat = Format;
        }

        // Сюда запоминаем цвета ячеек строки
        EFPDataGridViewExcelCellAttributes[] CellAttrBuffer = null;
        if (settings.UseInterior)
          CellAttrBuffer = new EFPDataGridViewExcelCellAttributes[Area.ColumnCount];

        // Строки данных
        spl.SetPercent(0, Area.RowCount);
        spl.AllowCancel = true;
        for (i = 0; i < Area.RowCount; i++)
        {
          controlProvider.DoGetRowAttributes(Area.RowIndices[i], EFPDataGridViewAttributesReason.Print);

          for (j = 0; j < Area.ColumnCount; j++)
          {
            int ColumnIndex = Area.ColumnIndices[j];
            EFPDataGridViewCellAttributesEventArgs CellArgs = controlProvider.DoGetCellAttributes(ColumnIndex);
            object v = CellArgs.FormattedValue;
            if (v is Bitmap)
              v = null;
            RowBuffer[j] = MyGetCellFormula(v);

            bool CellDefined = false;

            // Проверяем выравнивание. Если оно не совпадает с заданным для столбца,
            // то определяем его для ячейки
            XlHAlign ColHAlign = GetHAlign(Area.Columns[j].DefaultCellStyle.Alignment);
            XlVAlign ColVAlign = GetVAlign(Area.Columns[j].DefaultCellStyle.Alignment);
            XlHAlign CellHAlign = GetHAlign(CellArgs.CellStyle.Alignment);
            XlVAlign CellVAlign = GetVAlign(CellArgs.CellStyle.Alignment);
            if (CellHAlign != ColHAlign)
            {
              if (!CellDefined)
              {
                r = sheet.Cells[CurrRow, firstCol + j];
                CellDefined = true;
              }
              r.HorizontalAlignment = CellHAlign;
            }
            if (CellVAlign != ColVAlign)
            {
              if (!CellDefined)
              {
                r = sheet.Cells[CurrRow, firstCol + j];
                CellDefined = true;
              }
              r.VerticalAlignment = CellVAlign;
            }
            if (CellArgs.IndentLevel > 0)
            {
              if (!CellDefined)
              {
                r = sheet.Cells[CurrRow, firstCol + j];
                CellDefined = true;
              }
              int IndentLevel = CellArgs.IndentLevel;
              // if (IndentLevel < 0) IndentLevel = 0; // 27.12.2020 Лишняя проверка
              if (IndentLevel > 15) IndentLevel = 15;
              r.IndentLevel = IndentLevel;
            }


            // Цвета
            if (settings.UseInterior)
              CellAttrBuffer[j] = EFPDataGridView.GetExcelCellAttr(CellArgs);
          } // цикл по ячейкам в строке

          r = sheet.GetRange(CurrRow, firstCol, CurrRow, firstCol + Area.ColumnCount - 1);
          try
          {
            r.FormulaArray = RowBuffer;
          }
          catch
          {
            // Пытаемся заполнить по одной ячейке
            for (int k = 0; k < Area.ColumnCount; k++)
            {
              Range r1 = sheet.GetRange(CurrRow, firstCol + k, CurrRow, firstCol + k);
              r1.Formula = RowBuffer[k];
            }
          }

          if (settings.UseInterior)
          {
            // Заполняем для строки
            // !!! Оптимизация
            for (j = 0; j < Area.ColumnCount; j++)
            {
              if (!CellAttrBuffer[j].IsEmpty)
              {
                r = sheet.Cells[CurrRow, firstCol + j];
                if (!CellAttrBuffer[j].BackColor.IsEmpty)
                  r.Interior.SetColor(CellAttrBuffer[j].BackColor);
                if (!CellAttrBuffer[j].ForeColor.IsEmpty)
                  r.Font.SetColor(CellAttrBuffer[j].ForeColor);
                if (CellAttrBuffer[j].Bold)
                  r.Font.SetBold(true);
                if (CellAttrBuffer[j].Italic)
                  r.Font.SetItalic(true);
                if (CellAttrBuffer[j].Underline)
                  r.Font.SetUnderline(true);
              }
            }
          }

          spl.IncPercent();
          CurrRow++;
        }
      }
    }

    #endregion

#if XXX
    #region Режим заполнения ячеек

    private static void DoExportCells(EFPAccDepGrid GridHandler, GridHandlerExpExcelSettings Settings)
    {

      Splash spl = new Splash(new string[] { 
        "Активация Microsoft Excel",
        "Передача таблицы"});
      try
      {
        ExcelHelper Helper = new ExcelHelper(false);
        try
        {
          Range StartCell = Helper.Application.ActiveCell;
          spl.Complete();
          DoExportCells2(GridHandler, Settings, StartCell, spl);
        }
        finally
        {
          Helper.Dispose();
        }
      }
      finally
      {
        spl.Close();
      }
    }

    private static void DoExportCells2(EFPAccDepGrid GridHandler, GridHandlerExpExcelSettings Settings,
      Range StartCell, Splash spl)
    {
      // 1. Собираем массив печатаемых столбцов и строк
      DataGridViewRow[] Rows;
      EFPDataGridViewColumn[] Columns;
      if (Settings.RangeMode==EFPDataGridViewExpRange.Selected)
      {
        Columns = GridHandler.SelectedColumns;
        Rows = GridHandler.SelectedGridRows;
      }
      else
      {
        Columns = GridHandler.VisibleColumns;
        Rows = GridHandler.VisibleGridRows;
      }

      int i, j;

      Range RowLeftCell = StartCell;
      spl.SetPercent(0, Rows.Length * Columns.Length);
      spl.AllowCancel = true;
      for (i = 0; i < Rows.Length; i++)
      {
        GridHandler.DoGetRowAttributes(Rows[i].Index, EFPDataGridViewAttributesReason.Print);
        // Цикл по столбцам
        Range CurrCell = RowLeftCell;
        for (j = 0; j < Columns.Length; j++)
        {
          int ColumnIndex = Columns[j].GridColumn.Index;
          EFPDataGridViewCellAttributesEventArgs CellArgs = GridHandler.DoGetCellAttributes(ColumnIndex);
          object v = CellArgs.FormattedValue;
          if (v is Bitmap)
            v = null;
          CurrCell.Value = MyGetCellFormula(v);

          // Цвета
          if (Settings.UseFill)
          {
            Color clr = CellArgs.CellStyle.BackColor;
            if ((!clr.IsEmpty) && (!clr.IsSystemColor))
            {
              Interior Interior = CurrCell.Interior;
              Interior.Color = clr;
            }
            clr = CellArgs.CellStyle.ForeColor;
            if ((!clr.IsEmpty) && (!clr.IsSystemColor))
            {
              AgeyevAV.OLE.Excel.Font Font = CurrCell.Font;
              Font.Color = clr;
            }
          }

          // Переходим к следующему столбцу
          if (j < (Columns.Length - 1))
            CurrCell = CurrCell.Offset(0, 1);

          spl.IncPercent();
        }
        if (i < (Rows.Length - 1))
          RowLeftCell = RowLeftCell.Offset(1, 0);
      }
    }

    #endregion
#endif
    #region Вспомогательные методы

    /// <summary>
    /// Преобразование произвольного значения в формулу Excel
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static object MyGetCellFormula(object value)
    {
      if (value is DBNull)
        return null;
      if (value is Boolean)
        return ((bool)value) ? "[X]" : "[ ]";
      if (value is String)
      {
        string s = (string)value;
        if (s.Length == 0)
          return null;
        s = s.Replace("\r\n", "\n");
        if ("0123456789+-.,/'".IndexOf(s[0]) >= 0)
          return "'" + s;
        else
          return s;
      }
      return value;
    }

    public static XlHAlign GetHAlign(DataGridViewContentAlignment align)
    {
      switch (align)
      {
        case DataGridViewContentAlignment.TopLeft:
        case DataGridViewContentAlignment.MiddleLeft:
        case DataGridViewContentAlignment.BottomLeft:
          return XlHAlign.xlHAlignLeft;

        case DataGridViewContentAlignment.TopCenter:
        case DataGridViewContentAlignment.MiddleCenter:
        case DataGridViewContentAlignment.BottomCenter:
          return XlHAlign.xlHAlignCenter;
        default:
          return XlHAlign.xlHAlignRight;
      }
    }

    public static XlVAlign GetVAlign(DataGridViewContentAlignment align)
    {
      switch (align)
      {
        case DataGridViewContentAlignment.TopLeft:
        case DataGridViewContentAlignment.TopCenter:
        case DataGridViewContentAlignment.TopRight:
          return XlVAlign.xlVAlignTop;
        case DataGridViewContentAlignment.MiddleLeft:
        case DataGridViewContentAlignment.MiddleCenter:
        case DataGridViewContentAlignment.MiddleRight:
          return XlVAlign.xlVAlignCenter;
        default:
          return XlVAlign.xlVAlignBottom;
      }
    }

    #endregion
  }

  internal static class EFPDataGridViewSendToOpenOfficeCalc
  {
    #region Метод запуска "Отправить"

    public static void SendTable(EFPDataGridView controlProvider, EFPDataGridViewExpExcelSettings settings)
    {

      string FileName = EFPApp.SharedTempDir.GetTempFileName("ods").Path;
      controlProvider.SaveOpenOfficeCalc(FileName, settings);
      EFPApp.UsedOpenOffice.OpenWithCalc(new AbsPath(FileName), true); // без указания имени файла
    }

    #endregion

#if XXXX
    #region Прямая запись ячеек

    private static void DoExportCells(EFPAccDepGrid GridHandler, GridHandlerExpExcelSettings Settings)
    {
      // Не реализовано.
      // Не знаю, как правильно работать с курсором в таблице
      OpenOfficeHelper Helper = new OpenOfficeHelper();
      //XSpreadsheet Sheet = Helper.CurrentSpreadsheet;
      //if (Sheet == null)
      //{
      //  EFPApp.ShowTempMessage("Нет активного листа");
      //  return;
      //}

      XSheetCellRange Range = Helper.CurrentCellRange;
      if (Range == null)
      {
        EFPApp.ShowTempMessage("Нет активного листа");
        return;
      }
      {
        CellRangeAddress Addr = ((XCellRangeAddressable)Range).getRangeAddress();

        Range = Range.getSpreadsheet() as XSheetCellRange;
        XSheetCellCursor Cursor=Range.getSpreadsheet().createCursor();
        XCellCursor Cursor2=(XCellCursor )Cursor;

        //Cursor.
        Cursor2.getCellByPosition(Addr.StartColumn, Addr.StartRow);
        for (int i = 0; i < 100; i++)
          for (int j = 0; j < 100; j++)
            Range.getCellByPosition(Addr.StartColumn + i, Addr.StartRow + j).setFormula((i * j).ToString());
      }
    }

    #endregion
#endif
  }

}