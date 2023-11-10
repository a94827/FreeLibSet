using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using FreeLibSet.OLE.Excel;
using FreeLibSet.Shell;

namespace FreeLibSet.Reporting
{
  /// <summary>
  /// Отправка отчета в Excel через OLE
  /// На компьютере должен быть установлен Excel 2000 или новее
  /// </summary>
  public class BRFileExcelOLE
  {
    #region Свойства

    /// <summary>
    /// 
    /// </summary>
    public ISplash Splash
    {
      get
      {
        if (_Splash == null)
          _Splash = new DummySplash();
        return _Splash;
      }
      set { _Splash = value; }
    }
    private ISplash _Splash;

    #endregion

    #region Основной метод

    /// <summary>
    /// Создает экземпляр Excel.Application, создает в нем новую книгу с числом страниц, равным числу секций, и заполняет отчет.
    /// Приложение Excel становится видимым и содержит несохраненную безымянную книгу
    /// </summary>
    /// <param name="report"></param>
    public void Send(BRReport report)
    {
      Splash.PhaseText = "Запуск Excel";
      using (ExcelHelper helper = new ExcelHelper(true))
      {
        helper.ShowOnEnd = true;
        int excelVersion = helper.Application.Version.Major;
        bool oldDisplayAlerts = helper.Application.DisplayAlerts;
        helper.Application.SetDisplayAlerts(false);

        Splash.PhaseText = "Создание книги";

        // Передача через OLE
        Workbook wbk = helper.Application.Workbooks.Add(Math.Max(report.Sections.Count, 1)); // один лист является обязательным

        wbk.SetTitle(report.DocumentProperties.Title);
        wbk.SetSubject(report.DocumentProperties.Subject);
        wbk.SetAuthor(report.DocumentProperties.Author);
        wbk.SetCompany(report.DocumentProperties.Company);

        //wbk.Windows[1].SetView(XlWindowView.xlPageBreakPreview);

        Splash.PhaseText = "Передача данных в книгу Excel";
        Splash.PercentMax = report.BandCount;
        Splash.AllowCancel = true;

        for (int i = 0; i < report.Sections.Count; i++)
          SendSection(report.Sections[i], wbk.Sheets[i + 1], excelVersion);

        helper.Application.SetDisplayAlerts(oldDisplayAlerts);
      }
    }


    /// <summary>
    /// Хранилище для отложенной записи длинных строк
    /// </summary>
    private struct DelayedCellValue
    {
      #region Поля

      public int Row;

      public int Column;

      public string Value;

      #endregion
    }

    private void SendSection(BRSection section, Worksheet sheet, int excelVersion)
    {
      sheet.SetName(section.Name);

      #region Форматирование по умолчанию

      sheet.Cells.Font.SetName(section.Report.DefaultCellStyle.FontName);
      sheet.Cells.Font.SetSize(section.Report.DefaultCellStyle.FontHeightPt);
      // Атрибуты задаем на всякий случай, вряд ли они установлены в шаблоне по умолчанию
      sheet.Cells.Font.SetBold(false);
      sheet.Cells.Font.SetItalic(false);
      sheet.Cells.Font.SetUnderline(false);
      sheet.Cells.Font.SetStrikeout(false);

      sheet.Cells.SetVerticalAlignment(GetVAlign(section.Report.DefaultCellStyle.VAlign));

      #endregion

      #region Параметры страницы

      sheet.PageSetup.SetOrientation(section.PageSetup.Orientation == BROrientation.Landscape ? XlPageOrientation.xlLandscape : XlPageOrientation.xlPortrait);
      sheet.PageSetup.SetPaperSize(GetPageSize(section.PageSetup));
      sheet.PageSetup.SetMarginsLM(section.PageSetup.LeftMargin, section.PageSetup.TopMargin, section.PageSetup.RightMargin, section.PageSetup.BottomMargin);
      sheet.PageSetup.SetCenterVertically(section.PageSetup.CenterVertical);
      sheet.PageSetup.SetCenterHorizontally(section.PageSetup.CenterHorizontal);

      #endregion

      BRColumnBoundList colBounds = new BRColumnBoundList(section, 10, 1);

      #region Размеры столбцов

      CellWidthScale cws = new CellWidthScale(sheet);
      for (int k = 0; k < colBounds.WidthArray.Length; k++)
      {
        double wPt = colBounds.WidthArray[k] / 254.0 * 72.0;
        double colWidth = DataTools.Truncate(cws.PtToColumnWidth(wPt), 1);
        sheet.Columns[k + 1].SetColumnWidth(colWidth);
        //if (sheet.Columns[k + 1].Width > wPt)
        //{
        //  double k2 = wPt / sheet.Columns[k + 1].Width;
        //  double colWidth2 = DataTools.Truncate(colWidth * k2, 1);
        //  sheet.Columns[k + 1].SetColumnWidth(colWidth2);
        //}
      }
      //double sumWPt = 0;
      //for (int k = 0; k < colBounds.WidthArray.Length; k++)
      //  sumWPt+=sheet.Columns[k + 1].Width;
      //double w01 = sumWPt / 72.0 * 254.0;

      //Range wRow2 = sheet.GetRange(1, 1, 1, colBounds.WidthArray.Length);
      //double w02 = wRow2.Width / 72.0 * 254.0;

      // Не получается точно задать размеры. Тут ширина в пунктах получается нормальная, а после показа Excel на экране становится больше.
      // Исправляем: Если секция должна быть на одной странице по ширине, задаем это в явном виде
      if (DataTools.SumInt(colBounds.WidthArray) <= section.PageSetup.PrintAreaWidth)
        sheet.PageSetup.SetFitToPages(1, 0);

      #endregion

      int currRow = 1; // номер строки в Excel

      for (int i = 0; i < section.Bands.Count; i++)
      {
        #region Зазор между полосами

        int gap = 0;
        if (i > 0)
          gap = Math.Max(section.Bands[i - 1].BottomMargin, section.Bands[i].TopMargin);
        if (gap >= 10)
        {
          sheet.Rows[currRow].SetRowHeight(gap / 254.0 * 72.0);
          currRow++;
        }

        #endregion

        BRBand band = section.Bands[i];
        BRSelector sel1 = band.CreateSelector();
        BRSelector sel2 = band.CreateSelector();
        int firstBandRow = currRow;
        int[] leftCols = colBounds.LeftColumns[i];
        int[] rightCols = colBounds.RightColumns[i];

        bool areAllBordersSame = band.AreAllBordersSame;
        if (areAllBordersSame && sel1.CellStyle.LeftBorder != BRLine.None)
        {
          Range rBand = sheet.GetRange(firstBandRow, leftCols[0], firstBandRow + band.RowCount - 1, rightCols[band.ColumnCount - 1]);
          SetBorders(rBand.Borders, sel1.CellStyle.LeftBorder);
        }

        // Выполняем запись блоками. В блоке используем максимум 8000 ячеек
        int maxBlockRowCount = 8000 / band.ColumnCount;

        for (int j1 = 0; j1 < band.RowCount; j1 += maxBlockRowCount)
        {
          int blockRowCount = Math.Min(band.RowCount - j1, maxBlockRowCount);
          int firstBlockRow = currRow;

          #region Сбор значений для блока

          object[,] values = new object[blockRowCount, rightCols[rightCols.Length - 1]];
          List<DelayedCellValue> dcvs = null; // отложенная запись длинных строк

          for (int j2 = 0; j2 < blockRowCount; j2++)
          {
            sel1.RowIndex = j1 + j2;
            //if (!sel.RowInfo.Visible)
            //  continue;
            if (sel1.RowInfo.Height != BRReport.AutoRowHeight)
              sheet.Rows[currRow].SetRowHeight(sel1.RowInfo.Height / 254.0 * 72.0);
            for (int k = 0; k < band.ColumnCount; k++)
            {
              sel1.ColumnIndex = k;

              object value = sel1.Value;
              if (value is Boolean)
                value = ((bool)value) ? 1 : 0;

              if (value is String)
              {
                string s = (string)value;
                s = DataTools.ReplaceAny(s, BRFileTools.BadValueChars, ' ');

                // Длинные строки будем передавать поштучно
                // Excel 2003 [иногда] начинает глючить, если строка длинная.
                // Дефект вылазеет, только если используется свойство Range.FormulaArray и отсутствует,
                // если используется свойство Formula
                if ((excelVersion < MicrosoftOfficeTools.MicrosoftOffice_2007 && s.Length > 255) ||
                  s.IndexOf(Environment.NewLine) > 0)
                {
                    DelayedCellValue dcv = new DelayedCellValue();
                    dcv.Row = firstBlockRow+j2;
                    dcv.Column = leftCols[k];
                    dcv.Value = s;
                    if (dcvs == null)
                      dcvs = new List<DelayedCellValue>();
                    dcvs.Add(dcv);
                    continue;
                }
                value = s;
              }
              values[j2, leftCols[k] - 1] = value;
            } // цикл по столбцам

            currRow++;
          }

          #endregion

          #region Запись блока

          Range r = sheet.GetRange(firstBlockRow, leftCols[0], firstBlockRow + blockRowCount - 1, rightCols[band.ColumnCount - 1]);
          try
          {
            r.Formula = values;
          }
          catch
          {
            // Если попытка не удалась, записываем значения по одному

            for (int j = 0; j < blockRowCount; j++)
            {
              for (int k = 0; k < band.ColumnCount; k++)
              {
                int xlsRow = firstBlockRow + j;
                int xlsColumn = leftCols[k];

                Range rSingle = sheet.Cells[xlsRow, xlsColumn];
                rSingle.Value = values[j, k];
              }
            }
          }

          #endregion

          #region Запись длинных строк

          if (dcvs != null)
          {
            for (int j = 0; j < dcvs.Count; i++)
            {
              Range rSingle = sheet.Cells[dcvs[i].Row, dcvs[i].Column];
              //rSingle.Formula = DCVs[i].Value;
              rSingle.Value = dcvs[i].Value.Replace(Environment.NewLine, "\r");
            }
          }

          #endregion
        } // цикл по блокам

        #region Объединение ячеек

        for (int j = 0; j < band.RowCount; j++)
        {
          sel1.RowIndex = j;
          for (int k = 0; k < band.ColumnCount; k++)
          {
            sel1.ColumnIndex = k;
            BRRange merge = sel1.MergeInfo;
            if (merge.FirstRowIndex == j && merge.FirstColumnIndex == k)
            {
              int xlsRow1 = firstBandRow + j;
              int xlsRow2 = xlsRow1 + merge.RowCount - 1;
              int xlsColumn1 = leftCols[merge.FirstColumnIndex];
              int xlsColumn2 = rightCols[merge.LastColumnIndex];
              if (xlsRow2 > xlsRow1 || xlsColumn2 > xlsColumn1)
              {
                Range rMerge = sheet.GetRange(xlsRow1, xlsColumn1, xlsRow2, xlsColumn2);
                rMerge.Merge();
              }
            }
          }
        }

        #endregion

        #region Форматирование

        // Форматирование должно выполняться после объединения, иначе форматирование потеряется

        // Находим "образцовую" строку.
        // Берем середину блока, чтобы с меньшей вероятностью попасть в заголовки и итоги, которые обычно имеют другое форматирование

        sel1.RowIndex = band.RowCount / 2;
        for (int k = 0; k < band.ColumnCount; k++)
        {
          sel1.ColumnIndex = k;
          sel2.ColumnIndex = k;

          #region 1. Форматирование столбца в-целом

          Range rCol = sheet.GetRange(firstBandRow, leftCols[k], firstBandRow + band.RowCount - 1, rightCols[k]);
          if (sel1.CellStyle.FontName != section.Report.DefaultCellStyle.FontName)
            rCol.Font.SetName(sel1.CellStyle.FontName);
          if (sel1.CellStyle.FontHeightPt != section.Report.DefaultCellStyle.FontHeightPt)
            rCol.Font.SetSize(sel1.CellStyle.FontHeightPt);
          // Атрибуты шрифта установим поштучно
          rCol.SetHorizontalAlignment(GetHAlign(sel1.ActualHAlign));
          if (sel1.CellStyle.VAlign != section.Report.DefaultCellStyle.VAlign)
            rCol.SetVerticalAlignment(GetVAlign(sel1.CellStyle.VAlign));
          rCol.SetWrapText(sel1.CellStyle.WrapMode != BRWrapMode.NoWrap);

          #endregion

          #region 2. Форматирование отдельных ячеек

          for (int j = 0; j < band.RowCount; j++)
          {
            sel2.RowIndex = j;

            bool flagFontName = sel1.CellStyle.FontName != sel2.CellStyle.FontName;
            bool flagFontSize = sel1.CellStyle.FontHeightPt != sel2.CellStyle.FontHeightPt;
            bool flagFontAttrs = sel2.CellStyle.Bold | sel2.CellStyle.Italic | sel2.CellStyle.Underline | sel2.CellStyle.Strikeout;
            bool flagBorders = false;
            if (!areAllBordersSame)
              flagBorders = sel2.CellStyle.AllBorders != BRLine.None;
            bool flagDiagonalUp = sel2.CellStyle.DiagonalUp != BRLine.None;
            bool flagDiagonalDown = sel2.CellStyle.DiagonalDown != BRLine.None;
            bool flagHAlign = sel1.ActualHAlign != sel2.ActualHAlign;
            bool flagVAlign = sel1.CellStyle.VAlign != sel2.CellStyle.VAlign;
            bool flagWrapMode = sel1.CellStyle.WrapMode != sel2.CellStyle.WrapMode;
            bool flagIndent = sel2.CellStyle.IndentLevel > 0;
            bool flagBackColor = sel2.CellStyle.BackColor != BRColor.Auto;
            bool flagForeColor = sel2.CellStyle.ForeColor != BRColor.Auto;

            if (flagFontName || flagFontSize || flagFontAttrs || flagBorders || flagDiagonalUp || flagDiagonalDown ||
              flagHAlign || flagVAlign || flagWrapMode || flagIndent || flagBackColor || flagForeColor)
            {
              Range rSingle = sheet.GetRange(firstBandRow + j, leftCols[k], firstBandRow + j, rightCols[k]);
              if (flagFontName)
                rSingle.Font.SetName(sel2.CellStyle.FontName);
              if (flagFontSize)
                rSingle.Font.SetSize(sel2.CellStyle.FontHeightPt);
              if (flagFontAttrs)
              {
                if (sel2.CellStyle.Bold)
                  rSingle.Font.SetBold(true);
                if (sel2.CellStyle.Italic)
                  rSingle.Font.SetItalic(true);
                if (sel2.CellStyle.Underline)
                  rSingle.Font.SetUnderline(true);
                if (sel2.CellStyle.Strikeout)
                  rSingle.Font.SetStrikeout(true);
              }

              if (flagBorders)
              {
                if (sel2.CellStyle.AreaAllBordersSame)
                  SetBorders(rSingle.Borders, sel2.CellStyle.LeftBorder);
                else
                {
                  SetBorder(rSingle.Borders[XlBordersIndex.xlEdgeLeft], sel2.CellStyle.LeftBorder);
                  SetBorder(rSingle.Borders[XlBordersIndex.xlEdgeTop], sel2.CellStyle.TopBorder);
                  SetBorder(rSingle.Borders[XlBordersIndex.xlEdgeRight], sel2.CellStyle.RightBorder);
                  SetBorder(rSingle.Borders[XlBordersIndex.xlEdgeBottom], sel2.CellStyle.BottomBorder);
                }
              }
              if (flagDiagonalUp)
                SetBorder(rSingle.Borders[XlBordersIndex.xlDiagonalUp], sel2.CellStyle.DiagonalUp);
              if (flagDiagonalDown)
                SetBorder(rSingle.Borders[XlBordersIndex.xlDiagonalDown], sel2.CellStyle.DiagonalDown);

              if (flagHAlign)
                rSingle.SetHorizontalAlignment(GetHAlign(sel2.ActualHAlign));
              if (flagVAlign)
                rSingle.SetVerticalAlignment(GetVAlign(sel2.CellStyle.VAlign));
              if (flagWrapMode)
                rSingle.SetWrapText(sel2.CellStyle.WrapMode != BRWrapMode.NoWrap);
              if (flagIndent)
                rSingle.SetIndentLevel(sel2.CellStyle.IndentLevel);

              if (flagBackColor)
                rSingle.Interior.SetColor(GetColor(sel2.CellStyle.BackColor));
              if (flagForeColor)
                rSingle.Font.SetColor(GetColor(sel2.CellStyle.ForeColor));
            } //флаги
          } // цикл по строкам

          #endregion
        } // цикл по столбцам

        #endregion

        Splash.IncPercent();
      } // цикл по полосам

      //Range wRow3 = sheet.GetRange(1, 1, 1, colBounds.WidthArray.Length);
      //double w03 = wRow3.Width / 72.0 * 254.0;

    }

    /// <summary>
    /// В Excel цвет кодируется в обратном порядке относительно <see cref="BRColor.IntValue"/>
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    private static int GetColor(BRColor color)
    {
      return color.B << 16 | color.G << 8 | color.R;
    }

    /// <summary>
    /// Возвращаем какой-нибудь подходящий размер бумаги
    /// </summary>
    /// <param name="pageSetup"></param>
    /// <returns></returns>
    public static XlPaperSize GetPageSize(BRPageSetup pageSetup)
    {
      int w = pageSetup.PaperWidth;
      int h = pageSetup.PaperHeight;
      if (pageSetup.Orientation == BROrientation.Landscape)
        DataTools.Swap<int>(ref w, ref h);

      if (w <= 1480 && h <= 2100)
        return XlPaperSize.xlPaperA5;
      if (w <= 2100 && h <= 2970)
        return XlPaperSize.xlPaperA4;
      if (w <= 2970 && h <= 4200)
        return XlPaperSize.xlPaperA3;
      //if (w <= 4200 && h <= 4200)
      return XlPaperSize.xlPaperA3;
    }

    private static XlHAlign GetHAlign(BRHAlign value)
    {
      switch (value)
      {
        case BRHAlign.Left: return XlHAlign.xlHAlignLeft;
        case BRHAlign.Center: return XlHAlign.xlHAlignCenter;
        case BRHAlign.Right: return XlHAlign.xlHAlignRight;
        default:
          throw new ArgumentException();
      }
    }

    private static XlVAlign GetVAlign(BRVAlign value)
    {
      switch (value)
      {
        case BRVAlign.Top: return XlVAlign.xlVAlignTop;
        case BRVAlign.Center: return XlVAlign.xlVAlignCenter;
        case BRVAlign.Bottom: return XlVAlign.xlVAlignBottom;
        default:
          throw new ArgumentException();
      }
    }

    #endregion

    private static void SetBorders(Borders borders, BRLine line)
    {
      XlLineStyle? lineStyle;
      XlBorderWeight? weight;
      int? color;
      GetLineProps(line, out lineStyle, out weight, out color);

      if (lineStyle.HasValue)
        borders.SetLineStyle(lineStyle.Value);
      if (weight.HasValue)
        borders.SetWeight(weight.Value);
      if (color.HasValue)
        borders.SetColor(color.Value);
    }

    private static void SetBorder(Border border, BRLine line)
    {
      XlLineStyle? lineStyle;
      XlBorderWeight? weight;
      int? color;
      GetLineProps(line, out lineStyle, out weight, out color);

      if (lineStyle.HasValue)
        border.SetLineStyle(lineStyle.Value);
      if (weight.HasValue)
        border.SetWeight(weight.Value);
      if (color.HasValue)
        border.SetColor(color.Value);
    }

    private static void GetLineProps(BRLine line, out XlLineStyle? lineStyle, out XlBorderWeight? weight, out int? color)
    {
      lineStyle = null;
      weight = null;
      color = null;

      if (line.Style == BRLineStyle.None)
        return;

      weight = XlBorderWeight.xlThin;
      switch (line.Style)
      {
        case BRLineStyle.Dot:
          lineStyle = XlLineStyle.xlDot;
          break;
        case BRLineStyle.Dash:
          lineStyle = XlLineStyle.xlDash;
          break;
        case BRLineStyle.DashDot:
          lineStyle = XlLineStyle.xlDashDot;
          break;
        case BRLineStyle.DashDotDot:
          lineStyle = XlLineStyle.xlDashDotDot;
          break;
        case BRLineStyle.Thin:
          lineStyle = XlLineStyle.xlContinuous;
          break;
        case BRLineStyle.Medium:
          lineStyle = XlLineStyle.xlContinuous;
          weight = XlBorderWeight.xlMedium;
          break;
        case BRLineStyle.Thick:
          lineStyle = XlLineStyle.xlContinuous;
          weight = XlBorderWeight.xlThick;
          break;
      }

      if (line.Color != BRColor.Auto)
        color = GetColor(line.Color);
    }

  }

  /// <summary>
  /// Вычисление границ столбцов для секции.
  /// Используется при передаче секции в Excel.
  /// Для определенности, "столбцом" здесь называется столбец в BRBand, а "колонкой" - в результирующей таблице
  /// </summary>
  internal struct BRColumnBoundList
  {
    #region Конструктор

    /// <summary>
    /// Выполняет распределение столбцов отчета по колонкам общей таблицы
    /// </summary>
    /// <param name="section">Секция, из которой извлекаются полосы</param>
    /// <param name="minWidth">Минимальная ширина колонки в единицах 0.1мм</param>
    /// <param name="firstCol">Номер первой колонки. Для Excel задается равным 1</param>
    public BRColumnBoundList(BRSection section, int minWidth, int firstCol)
    {
      if (minWidth < 1)
        throw new ArgumentOutOfRangeException("minWidth");

      #region Первый проход - собираем все размеры, игнорируя minWidth

      List<int> lstX = new List<int>(); // пока не проверяем даже повторы
      lstX.Add(0);
      foreach (BRBand band in section.Bands)
      {
        int x = 0;
        BRSelector sel = band.CreateSelector();
        int[] ws = GetExtColumnSizes(sel, section.PageSetup.PrintAreaWidth);
        for (int j = 0; j < band.ColumnCount; j++)
        {
          x += ws[j];
          lstX.Add(x);
        }
      }

      #endregion

      #region Удаляем повторы и маленькие колонки

      lstX.Sort();
      for (int j = lstX.Count - 1; j >= 1; j--)
      {
        int w = lstX[j] - lstX[j - 1];
        if (w < minWidth)
          lstX.RemoveAt(j);
      }

      _WidthArray = new int[lstX.Count - 1];
      for (int i = 0; i < _WidthArray.Length; i++)
        _WidthArray[i] = lstX[i + 1] - lstX[i];

      #endregion

      #region Второй проход - подбор подходящих границ

      int[] aX = lstX.ToArray();

      _LeftColumns = new int[section.Bands.Count][];
      _RightColumns = new int[section.Bands.Count][];
      for (int i = 0; i < section.Bands.Count; i++)
      {
        BRBand band = section.Bands[i];
        _LeftColumns[i] = new int[band.ColumnCount];
        _RightColumns[i] = new int[band.ColumnCount];

        int x = 0;
        BRSelector sel = band.CreateSelector();
        int[] ws = GetExtColumnSizes(sel, section.PageSetup.PrintAreaWidth);
        for (int j = 0; j < band.ColumnCount; j++)
        {
          _LeftColumns[i][j] = firstCol + IndexOfNearest(aX, x);
          x += ws[j];
          _RightColumns[i][j] = firstCol + IndexOfNearest(aX, x) - 1;
        }
      }

      #endregion

      #region Проверка

#if DEBUG
      for (int i = 0; i < section.Bands.Count; i++)
      {
        for (int j = 0; j < _LeftColumns[i].Length; j++)
        {
          int colLeft = _LeftColumns[i][j] - firstCol;
          int colRight = _RightColumns[i][j] - firstCol;
          if (colLeft < 0)
            throw new BugException("colLeft");
          if (colRight >= _WidthArray.Length)
            throw new BugException("colRight");
          if (colRight < colLeft)
            throw new BugException("colRight<colLeft");
        }
      }
#endif

      #endregion
    }

    /// <summary>
    /// Возвращает размеры столбцов в единицах 0.1мм.
    /// Если столбцы занимают меньшую ширину, чем <paramref name="printAreaWidth"/>, то выполняется попытка расширить
    /// столбцы, у которых <see cref="BRColumnInfo.AutoGrow"/>=true
    /// </summary>
    /// <param name="sel"></param>
    /// <param name="printAreaWidth"></param>
    /// <returns></returns>
    private static int[] GetExtColumnSizes(BRSelector sel, int printAreaWidth)
    {
      int[] ws = new int[sel.Band.ColumnCount];
      bool[] grows = new bool[sel.Band.ColumnCount];
      for (int i = 0; i < sel.Band.ColumnCount; i++)
      {
        sel.ColumnIndex = i;
        ws[i] = sel.ColumnInfo.Width;
        grows[i] = sel.ColumnInfo.Visible && sel.ColumnInfo.AutoGrow;
      }
      int wholeW = DataTools.SumInt(ws);
      if (wholeW < printAreaWidth)
      {
        int gw = 0;
        for (int i = 0; i < grows.Length; i++)
        {
          if (grows[i])
            gw += ws[i];
        }
        if (gw > 0)
        {
          double k = ((double)(printAreaWidth - wholeW) / (double)gw) + 1.0; // коэффициент увеличения
#if DEBUG
          if (k <= 1.0)
            throw new BugException();
#endif
          for (int i = 0; i < grows.Length; i++)
          {
            if (grows[i])
              ws[i] = (int)(ws[i] * k); // пусть обрезается, а не округляется
          }
        }
      }
      return ws;
    }

    /// <summary>
    /// Поиск ближайшего значения
    /// </summary>
    /// <param name="a">Массив границ</param>
    /// <param name="v">Искомое значение (положение границы)</param>
    /// <returns>Индекс ближайшего значения</returns>
    private static int IndexOfNearest(int[] a, int v)
    {
      // Используем быстрый поиск

      int minIndex = Array.BinarySearch<int>(a, v);
      if (minIndex >= 0) // нашли точное совпадение
        return minIndex;

      minIndex = ~minIndex;
      if (minIndex >= a.Length) // больше последней границы (так теоретически может быть за счет округления)
        return a.Length - 1;
      if (minIndex < 1) // меньше первой границы (так теоретически может быть за счет округления)
        return 0;

      // Нужно вернуть либо MinIndex-1, либо MinIndex
      int delta1 = v - a[minIndex - 1];
      int delta2 = a[minIndex] - v;
      // оба числа положительны

      if (delta2 < delta1)
        return minIndex;
      else
        return minIndex - 1;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Размеры колонок в Excel в единицах 0.1мм
    /// </summary>
    public int[] WidthArray { get { return _WidthArray; } }
    private readonly int[] _WidthArray;

    /// <summary>
    /// Номера колонок Excel, соответствующее левой части столбца.
    /// Первая размерность массива соответствует полосе <see cref="BRBand.BandIndex"/>.
    /// Вторая размерность массива соответствует столбцам <see cref="BRBand.ColumnCount"/>.
    /// Нумерация колонок начинается со значения параметра firstCol, переданного конструктору.
    /// </summary>
    public int[][] LeftColumns { get { return _LeftColumns; } }
    private readonly int[][] _LeftColumns;

    /// <summary>
    /// Номера колонок Excel, соответствующее правой части столбца.
    /// Первая размерность массива соответствует полосе <see cref="BRBand.BandIndex"/>.
    /// Вторая размерность массива соответствует столбцам <see cref="BRBand.ColumnCount"/>.
    /// Нумерация колонок начинается со значения параметра firstCol, переданного конструктору.
    /// Значения в <see cref="LeftColumns"/> и <see cref="RightColumns"/> совпадают, когда нет объединения ячеек для полосы
    /// </summary>
    public int[][] RightColumns { get { return _RightColumns; } }
    private readonly int[][] _RightColumns;

    #endregion
  }
}
