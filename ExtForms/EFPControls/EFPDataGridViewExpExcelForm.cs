// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Xml;
using System.IO;
using FreeLibSet.OLE.Excel;
using FreeLibSet.IO;
using FreeLibSet.Shell;
using FreeLibSet.Forms.Reporting;

namespace FreeLibSet.Forms
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

      bool hasBoolColumn = false;
      for (int i = 0; i < controlProvider.Control.Columns.Count; i++)
      {
        DataGridViewColumn gridCol = controlProvider.Control.Columns[i];
        if (!gridCol.Visible)
          continue;
        if (gridCol is DataGridViewCheckBoxColumn)
        {
          hasBoolColumn = true;
          break;
        }
        if (gridCol.ValueType == typeof(Boolean))
        {
          hasBoolColumn = true;
          break;
        }
      }
      efpBoolMode.Enabled = hasBoolColumn;
    }

    #endregion

    #region Чтение и запись значений

    public void LoadValues(EFPDataGridViewExpExcelSettings settings)
    {
      efpArea.SelectedIndex = settings.RangeMode == EFPDataViewExpRange.Selected ? 1 : 0;
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
      settings.RangeMode = efpArea.SelectedIndex == 1 ? EFPDataViewExpRange.Selected : EFPDataViewExpRange.All;
      settings.ShowColumnHeaders = efpHeaders.Checked;
      settings.UseInterior = efpUseFill.Checked;
      settings.UseBorders = efpUseBorders.Checked;
      settings.BoolMode = (BRDataViewBoolMode)(efpBoolMode.SelectedIndex);
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
      bool withFile = EFPApp.MicrosoftExcelVersion.Major >= MicrosoftOfficeTools.MicrosoftOffice_XP;
      //if (Settings.Mode != GridHandlerExpExcelMode.Table)
      //  WithFile = false;

      //#if DEBUG
      //          if (AccDepClientExec.DebugShowIds)
      //            EFPApp.MessageBox("Метод передачи в Excel: " +
      //              (WithFile ? " через временный XML-файл (быстрый)" : "через OLE (медленный)") +
      //              "\r\nВерсия Excel: " + Helper.Application.Version.ToString(),
      //              "Передача таблицы в MS Excel");
      //#endif

      if (withFile)
      {
        Splash spl = new Splash(new string[] { 
          "Создание файла", 
          "Запуск Microsoft Excel"});
        try
        {
          // Передача через временный файл
          string fileName = EFPApp.SharedTempDir.GetTempFileName("xml").Path;
          controlProvider.SaveExcel2003(fileName, settings);
          spl.Complete();
          MicrosoftOfficeTools.OpenWithExcel(new AbsPath(fileName), true);
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
          ExcelHelper helper = new ExcelHelper(true);
          try
          {
            // Передача через OLE
            Workbook wbk = helper.Application.Workbooks.Add(1);
            Worksheet sht = wbk.Sheets[1];
            spl.Complete();
            DoSendTable2(controlProvider, settings, sht, 1, 1, spl);
          }
          finally
          {
            helper.Dispose();
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
      EFPDataGridViewRectArea area = controlProvider.GetRectArea(settings.RangeMode);

      int i, j;
      Range r = new Range();

      // Устанавливаем ширину столбцов в единицах ширины шрифта
      double[] widths = new double[area.ColumnCount];
      for (i = 0; i < area.ColumnCount; i++)
      {
        EFPDataGridViewColumn Column = controlProvider.Columns[area.ColumnIndices[i]];
        widths[i] = Column.TextWidth;
      }
      sheet.SetColumnTextWidths(widths);

      int currRow = firstRow;

      object[] rowBuffer = new object[area.ColumnCount];
      // Заголовки 
      if (settings.ShowColumnHeaders)
      {
        for (j = 0; j < area.ColumnCount; j++)
        {
          rowBuffer[j] = MyGetCellFormula(area.Columns[j].HeaderText);
        }
        r = sheet.GetRange(currRow, firstCol, currRow, firstCol + area.ColumnCount - 1);
        r.FormulaArray = rowBuffer;
        r.HorizontalAlignment = XlHAlign.xlHAlignCenter; // независимо от флага
        r.Font.SetBold(true);
        currRow++;
      }

      if (area.RowCount > 0)
      {
        // Выравнивание и формат
        for (j = 0; j < area.ColumnCount; j++)
        {
          r = sheet.GetRange(currRow, firstCol + j, currRow + area.RowCount - 1, firstCol + j);
          // Выравнивание для столбца по умолчанию
          XlHAlign colHAlign = GetHAlign(area.Columns[j].DefaultCellStyle.Alignment);
          XlVAlign colVAlign = GetVAlign(area.Columns[j].DefaultCellStyle.Alignment);
          r.HorizontalAlignment = colHAlign;
          if (colVAlign != XlVAlign.xlVAlignCenter)
            r.VerticalAlignment = colVAlign;

          string format = area.Columns[j].DefaultCellStyle.Format;
          r.NumberFormat = format;
        }

        // Сюда запоминаем цвета ячеек строки
        EFPDataGridViewExcelCellAttributes[] cellAttrBuffer = null;
        if (settings.UseInterior)
          cellAttrBuffer = new EFPDataGridViewExcelCellAttributes[area.ColumnCount];

        // Строки данных
        spl.SetPercent(0, area.RowCount);
        spl.AllowCancel = true;
        for (i = 0; i < area.RowCount; i++)
        {
          controlProvider.DoGetRowAttributes(area.RowIndices[i], EFPDataGridViewAttributesReason.Print);

          for (j = 0; j < area.ColumnCount; j++)
          {
            int columnIndex = area.ColumnIndices[j];
            EFPDataGridViewCellAttributesEventArgs cellArgs = controlProvider.DoGetCellAttributes(columnIndex);
            object v = cellArgs.FormattedValue;
            if (v is Bitmap)
              v = null;
            rowBuffer[j] = MyGetCellFormula(v);

            bool cellDefined = false;

            // Проверяем выравнивание. Если оно не совпадает с заданным для столбца,
            // то определяем его для ячейки
            XlHAlign colHAlign = GetHAlign(area.Columns[j].DefaultCellStyle.Alignment);
            XlVAlign colVAlign = GetVAlign(area.Columns[j].DefaultCellStyle.Alignment);
            XlHAlign cellHAlign = GetHAlign(cellArgs.CellStyle.Alignment);
            XlVAlign cellVAlign = GetVAlign(cellArgs.CellStyle.Alignment);
            if (cellHAlign != colHAlign)
            {
              if (!cellDefined)
              {
                r = sheet.Cells[currRow, firstCol + j];
                cellDefined = true;
              }
              r.HorizontalAlignment = cellHAlign;
            }
            if (cellVAlign != colVAlign)
            {
              if (!cellDefined)
              {
                r = sheet.Cells[currRow, firstCol + j];
                cellDefined = true;
              }
              r.VerticalAlignment = cellVAlign;
            }
            if (cellArgs.IndentLevel > 0)
            {
              if (!cellDefined)
              {
                r = sheet.Cells[currRow, firstCol + j];
                cellDefined = true;
              }
              int indentLevel = cellArgs.IndentLevel;
              // if (IndentLevel < 0) IndentLevel = 0; // 27.12.2020 Лишняя проверка
              if (indentLevel > 15) indentLevel = 15;
              r.IndentLevel = indentLevel;
            }


            // Цвета
            if (settings.UseInterior)
              cellAttrBuffer[j] = EFPDataGridView.GetExcelCellAttr(cellArgs);
          } // цикл по ячейкам в строке

          r = sheet.GetRange(currRow, firstCol, currRow, firstCol + area.ColumnCount - 1);
          try
          {
            r.FormulaArray = rowBuffer;
          }
          catch
          {
            // Пытаемся заполнить по одной ячейке
            for (int k = 0; k < area.ColumnCount; k++)
            {
              Range r1 = sheet.GetRange(currRow, firstCol + k, currRow, firstCol + k);
              r1.Formula = rowBuffer[k];
            }
          }

          if (settings.UseInterior)
          {
            // Заполняем для строки
            // !!! Оптимизация
            for (j = 0; j < area.ColumnCount; j++)
            {
              if (!cellAttrBuffer[j].IsEmpty)
              {
                r = sheet.Cells[currRow, firstCol + j];
                if (!cellAttrBuffer[j].BackColor.IsEmpty)
                  r.Interior.SetColor(ExcelWinFormsTools.ColorToRgb(cellAttrBuffer[j].BackColor));
                if (!cellAttrBuffer[j].ForeColor.IsEmpty)
                  r.Font.SetColor(ExcelWinFormsTools.ColorToRgb(cellAttrBuffer[j].ForeColor));
                if (cellAttrBuffer[j].Bold)
                  r.Font.SetBold(true);
                if (cellAttrBuffer[j].Italic)
                  r.Font.SetItalic(true);
                if (cellAttrBuffer[j].Underline)
                  r.Font.SetUnderline(true);
              }
            }
          }

          spl.IncPercent();
          currRow++;
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
              FreeLibSet.OLE.Excel.Font Font = CurrCell.Font;
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

      string fileName = EFPApp.SharedTempDir.GetTempFileName("ods").Path;
      controlProvider.SaveOpenOfficeCalc(fileName, settings);
      EFPApp.UsedOpenOffice.Parts[OpenOfficePart.Calc].OpenFile(new AbsPath(fileName), true); // без указания имени файла
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
