using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text;
using FreeLibSet.Core;
using FreeLibSet.IO;
using FreeLibSet.Reporting;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace FreeLibSet.Drawing.Reporting
{
  internal sealed class BRPdfMeasurer : IBRMeasurer
  {
    #region Защищенный конструктор

    private BRPdfMeasurer() { }

    #endregion

    #region IBRMeasurer

    /// <summary>
    /// Объект для измерения шрифта.
    /// Создается при первом обращении.
    /// Все вызовы должны блокироваться
    /// </summary>
    private PdfTextRenderer _Renderer;

    public int GetWantedHeight(BRSelector sel, int columnWidth)
    {

      if (_Renderer == null)
        _Renderer = new PdfTextRenderer();

      lock (_Renderer)
      {
        InitRenderer(_Renderer, sel.CellStyle);
        return GetWantedHeight(_Renderer, sel, columnWidth);
      }

    }

    public void MeasureString(string s, BRCellStyle cellStyle, out int width, out int height)
    {
      if (_Renderer == null)
        _Renderer = new PdfTextRenderer();

      lock (_Renderer)
      {
        InitRenderer(_Renderer, cellStyle);
        Size sz = _Renderer.MeasureStringLM(s);
        width = sz.Width;
        height = sz.Height;
      }
    }


    internal static void InitRenderer(PdfTextRenderer renderer, BRCellStyle cellStyle)
    {
      renderer.FontName = cellStyle.FontName;
      renderer.FontHeight = cellStyle.FontHeightPt;
      if (cellStyle.LineHeightTwip > 0)
        renderer.LineHeight = cellStyle.LineHeightPt;
      if (cellStyle.FontWidthPt > 0)
        renderer.FontWidth = cellStyle.FontWidthPt;
      else if (cellStyle.FontWidthPercent > 0 && cellStyle.FontWidthPercent != 100)
        renderer.FontWidth = renderer.DefaultFontWidth * cellStyle.FontWidthPercent / 100f;
      renderer.Bold = cellStyle.Bold;
      renderer.Italic = cellStyle.Italic;
      renderer.Underline = cellStyle.Underline;
      renderer.Strikeout = cellStyle.Strikeout;

      renderer.WordWrap = cellStyle.WrapMode == BRWrapMode.WordWrap;
      if (cellStyle.WrapMode == BRWrapMode.WordWrap)
      { 
      }

      BRColor clr = cellStyle.ForeColor;
      if (clr != BRColor.Auto)
        renderer.Color = XColor.FromArgb(clr.R, clr.G, clr.B);
    }

    internal static int GetWantedHeight(PdfTextRenderer renderer, BRSelector sel, int columnWidth)
    {
      int h = 0;
      string[] a = GetLines(renderer, sel, columnWidth);
      int w = GetTextAreaWidth(renderer, sel, columnWidth);

      for (int i = 0; i < a.Length; i++)
      {
        Size sz = renderer.MeasureStringLM(a[i]/*, w*/); // !!!!!!!!!!!!!!!!!!!
        h += sz.Height;
      }
      return h + sel.CellStyle.TopMargin + sel.CellStyle.BottomMargin;
    }

    private static int GetTextAreaWidth(PdfTextRenderer renderer, BRSelector sel, int columnWidth)
    {
      int w = columnWidth - sel.CellStyle.LeftMargin - sel.CellStyle.RightMargin;
      w -= (int)Math.Round(renderer.FontWidth * 72f / 254f * sel.CellStyle.IndentLevel);
      return w;
    }

    private static readonly List<string> _TempStrList = new List<string>();
    private static readonly BRWordWrapper _TempWordWrapper = new BRWordWrapper();
    private static object SyncRoot { get { return _TempStrList; } }

    /// <summary>
    /// Получение массива строк текста ячейки с учетом переноса по словам.
    /// Если ячейка содержит пустое значение, возвращается массив нулевой длины.
    /// Свойство используется при прорисовке ячеек и вычислении WantedHeight.
    /// Строки не содержат символов мягкого переноса.
    /// </summary>
    internal static string[] GetLines(PdfTextRenderer renderer, BRSelector sel, int columnWidth)
    {
      string s = sel.AsString;
      if (String.IsNullOrEmpty(s))
        return DataTools.EmptyStrings;

      if (sel.CellStyle.WrapMode == BRWrapMode.NoWrap)
        s = DataTools.RemoveSoftHyphens(s);
      string[] a = s.Split(DataTools.NewLineSeparators, StringSplitOptions.None); // Разбиение на строки, заданное в исходном значении

      if (sel.CellStyle.WrapMode == BRWrapMode.NoWrap)
        // Перенос запрещен
        return a;

      // Максимальная ширина текста
      int maxWidth = GetTextAreaWidth(renderer, sel, columnWidth);
      if (maxWidth <= 0) // Какой-то глюк с полями
        return a;

      string[] res;

      lock (SyncRoot)
      {
        _TempStrList.Clear();

        for (int i = 0; i < a.Length; i++)
        {
          _TempWordWrapper.Text = a[i];

          _TempWordWrapper.WrapStart();
          while (_TempWordWrapper.WrapHasLine())
          {
            while (_TempWordWrapper.WrapNeedsMeasure())
            {
              Size sz = renderer.MeasureStringLM(_TempWordWrapper.WrapString);
              _TempWordWrapper.WrapRes = sz.Width <= maxWidth;
            }
            _TempStrList.Add(_TempWordWrapper.WrapString);
          }
        }
        res = _TempStrList.ToArray();
      }
      return res;
    }


    #endregion

    #region Статический экземпляр

    public static readonly BRPdfMeasurer Default = new BRPdfMeasurer();

    #endregion
  }

  /// <summary>
  /// Создание pdf-файла с помощью библиотеки PdfSharp
  /// </summary>
  public class BRFilePdf: BRFileCreator
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой объект
    /// </summary>
    public BRFilePdf()
    {
      CompressContentStreams = true;

      //Scale = 0.1f;
      Scale = 72f/254f;
      //CellFrames = false;
      //ShowHiddenCells = false;
      //ShowSampleValues = true;

      _FontRenderers = new Dictionary<string, PdfTextRenderer>();
      _BackBrushes = new Dictionary<BRColor, XBrush>();

      BorderPen = new XPen(Color.Black);
      TextFillerPen = new XPen(Color.Black);

      _SB = new StringBuilder();
    }

    #endregion

    #region Управляющие свойства

    /// <summary>
    /// Если установлено в true (по умолчанию), то будут сжиматься потоки внутри pdf-файла
    /// Можно установить в false для отладочных целей
    /// </summary>
    public bool CompressContentStreams;

    #endregion

    #region Внутренние графические объекты для рисования

    #region Шрифты

    /// <summary>
    /// Шрифты
    /// Ключ - кодированное описание шрифта
    /// Значение - объект для рисования
    /// </summary>
    private Dictionary<string, PdfTextRenderer> _FontRenderers;

    private PdfTextRenderer GetTextRenderer(BRSelector sel, XGraphics graphics)
    {
      _SB.Length = 0;
      _SB.Append(sel.CellStyle.FontName);
      _SB.Append('|');
      _SB.Append(sel.CellStyle.FontHeightTwip);
      _SB.Append('|');
      _SB.Append(sel.CellStyle.LineHeightTwip);
      _SB.Append('|');
      _SB.Append(sel.CellStyle.FontWidthTwip);
      _SB.Append('|');
      _SB.Append(sel.CellStyle.FontWidthPercent);
      _SB.Append('|');
      _SB.Append(sel.CellStyle.Bold ? '1' : '0');
      _SB.Append(sel.CellStyle.Italic ? '1' : '0');
      _SB.Append(sel.CellStyle.Underline ? '1' : '0');
      _SB.Append(sel.CellStyle.Strikeout ? '1' : '0');
      _SB.Append('|');
      _SB.Append((int)(sel.CellStyle.WrapMode));
      _SB.Append('|');
      _SB.Append(sel.CellStyle.ForeColor.IntValue);

      string key = _SB.ToString();
      PdfTextRenderer renderer;
      if (!_FontRenderers.TryGetValue(key, out renderer))
      {
        renderer = new PdfTextRenderer();
        renderer.Graphics = graphics;
        BRPdfMeasurer.InitRenderer(renderer, sel.CellStyle);

        _FontRenderers.Add(key, renderer);
      }
      renderer.Graphics = graphics;
      return renderer;
    }

    #endregion

    /// <summary>
    /// Кисти фона
    /// Ключ - описатель цвета фона
    /// Значение - объект кисти или null
    /// </summary>
    private Dictionary<BRColor, XBrush> _BackBrushes;

    /// <summary>
    /// Используется при создании ключей по описанию шрифта
    /// </summary>
    private StringBuilder _SB;

    #endregion

    #region Основной метод

    /// <summary>
    /// Создает файл
    /// </summary>
    /// <param name="report">Отчет</param>
    /// <param name="filePath">Путь к файлу</param>
    protected override void DoCreateFile(BRReport report, AbsPath filePath)
    {
      // Создаем документ
      PdfDocument pdfDoc = new PdfDocument();
      pdfDoc.Options.CompressContentStreams = true;

      #region Сводка

      // Проблема с русской кодировкой
      // Сводка должна записываться не в кодировке Unicode, а в Encoding.BigEndianUnicode
      // Переставляем местами байты
      if (!String.IsNullOrEmpty(report.DocumentProperties.Title))
        pdfDoc.Info.Elements["/Title"] = new PdfString(ConvertSummaryString(report.DocumentProperties.Title), PdfStringEncoding.Unicode);
      if (!String.IsNullOrEmpty(report.DocumentProperties.Author))
        pdfDoc.Info.Elements["/Author"] = new PdfString(ConvertSummaryString(report.DocumentProperties.Author), PdfStringEncoding.Unicode);
      if (!String.IsNullOrEmpty(report.DocumentProperties.Subject))
        pdfDoc.Info.Elements["/Subject"] = new PdfString(ConvertSummaryString(report.DocumentProperties.Subject), PdfStringEncoding.Unicode);

      pdfDoc.Info.Creator = CreatorString;

      #endregion

      using (BRReportPainter painter = new BRReportPainter())
      {
        BRPaginator paginator = new BRPaginator(painter);
        BRPaginatorPageInfo[] pages = paginator.CreatePages(report);

        for (int i = 0; i < pages.Length; i++)
        {
          BRPageSetup ps = pages[i].Section.PageSetup;
          PdfPage PdfPage = pdfDoc.AddPage();
          if (ps.Orientation == BROrientation.Landscape)
          {
            PdfPage.Width = LMToXUnit(ps.PaperHeight);
            PdfPage.Height = LMToXUnit(ps.PaperWidth);
            PdfPage.Orientation = PdfSharp.PageOrientation.Landscape;
          }
          else
          {
            PdfPage.Width = LMToXUnit(ps.PaperWidth);
            PdfPage.Height = LMToXUnit(ps.PaperHeight);
            PdfPage.Orientation = PdfSharp.PageOrientation.Portrait;
          }

          using (XGraphics gfx = XGraphics.FromPdfPage(PdfPage))
          {
            // Увы, использовать XGraphics.Graphics нельзя, рисует на рабочем столе :)
            //painter.Paint(pages[i], gfx.Graphics);

            int x0, y0;
            object x = gfx.PageUnit;
            //if (Doc.Pages[i].PageSetup.CenterPage)
            //{
            //  int w1 = Doc.Pages[i].Columns.VisibleWidth;
            //  int h1 = Doc.Pages[i].Rows.VisibleHeight;
            //  int w2 = Doc.Pages[i].PageSetup.PrintAreaWidth;
            //  int h2 = Doc.Pages[i].PageSetup.PrintAreaHeight;
            //  x0 = Doc.Pages[i].PageSetup.LeftMargin + (w2 - w1) / 2;
            //  y0 = Doc.Pages[i].PageSetup.TopMargin + (h2 - h1) / 2;
            //}
            //else
            //{
            x0 = ps.LeftMargin;
            y0 = ps.TopMargin;
            //}

            Paint(pages[i], gfx);
          }
        }
      }

      // Сохраняем документ
      pdfDoc.Save(filePath.Path);
    }

    #endregion

    #region Рисование страницы

    /// <summary>
    /// Рисование одной страницы.
    /// Предполагается, что рисование уже было выполнено
    /// </summary>
    /// <param name="pageInfo">Информация о странице, полученная в процессе разбиения</param>
    /// <param name="graphics">Контекст для рисования</param>
    public void Paint(BRPaginatorPageInfo pageInfo, XGraphics graphics)
    {
      for (int i = 0; i < pageInfo.Blocks.Length; i++)
        Paint(pageInfo.Blocks[i], graphics);
    }

    float Scale;

    private void Paint(BRPaginatiorBlockInfo blockInfo, XGraphics graphics)
    {
      // Вычисляем координаты ячеек в контексте вывода
      float[] xx = new float[blockInfo.ColumnIndexes.Length + 1];
      xx[0] = (blockInfo.Band.Section.PageSetup.LeftMargin + blockInfo.LeftOffset) * Scale;
      for (int i = 1; i < xx.Length; i++)
      {
        //if (ShowHiddenCells || Frame.Columns[i - 1].Visible)
        xx[i] = xx[i - 1] + blockInfo.ColumnWidths[i - 1] * Scale;
        //else
        //  xx[i] = xx[i - 1];
      }

      float[] yy = new float[blockInfo.RowIndexes.Length + 1];
      yy[0] = (blockInfo.Band.Section.PageSetup.TopMargin + blockInfo.TopOffset) * Scale;
      for (int j = 1; j < yy.Length; j++)
      {
        //if (ShowHiddenCells || Frame.Rows[j - 1].Visible)
        yy[j] = yy[j - 1] + blockInfo.RowHeights[j - 1] * Scale;
        //else
        //  yy[j] = yy[j - 1];
      }

      BRLine[,] hLines = new BRLine[blockInfo.RowIndexes.Length + 1, blockInfo.ColumnIndexes.Length];
      BRLine[,] vLines = new BRLine[blockInfo.RowIndexes.Length, blockInfo.ColumnIndexes.Length + 1];

      BRSelector sel = blockInfo.Band.CreateSelector();
      for (int iRow = 0; iRow < blockInfo.RowIndexes.Length; iRow++)
      {
        sel.RowIndex = blockInfo.RowIndexes[iRow];
        for (int iCol = 0; iCol < blockInfo.ColumnIndexes.Length; iCol++)
        {
          sel.ColumnIndex = blockInfo.ColumnIndexes[iCol];

          // Для объединенных ячеек рисование выполняется только для верхней левой
          // ячейки, чтобы не возникало всяких неприятных дефектов
          bool isFirstCell;
          BRRange cellRange;
          blockInfo.GetCellMerge(sel, out isFirstCell, out cellRange);
          if (!isFirstCell)
            continue;
          float x1 = xx[cellRange.FirstColumnIndex];
          float x2 = xx[cellRange.LastColumnIndex + 1];
          float y1 = yy[cellRange.FirstRowIndex];
          float y2 = yy[cellRange.LastRowIndex + 1];

          //float x1 = xx[iCol];
          //float x2 = xx[iCol + 1];
          //float y1 = yy[iRow];
          //float y2 = yy[iRow + 1];
          XRect rc = new XRect(x1, y1, x2 - x1, y2 - y1);
          PaintCellContext(graphics, rc, sel);

          // Запоминаем границы для будущего рисования
          for (int iCol2 = cellRange.FirstColumnIndex; iCol2 <= cellRange.LastColumnIndex; iCol2++)
          {
            hLines[cellRange.FirstRowIndex, iCol2] |= sel.CellStyle.TopBorder;
            hLines[cellRange.LastRowIndex + 1, iCol2] = sel.CellStyle.BottomBorder;
          }
          for (int iRow2 = cellRange.FirstRowIndex; iRow2 <= cellRange.LastRowIndex; iRow2++)
          {
            vLines[iRow2, cellRange.FirstColumnIndex] |= sel.CellStyle.LeftBorder;
            vLines[iRow2, cellRange.LastColumnIndex + 1] = sel.CellStyle.RightBorder;
          }

          // Диагональные линии
          DrawBorder(sel.CellStyle.DiagonalDown, graphics, rc.Left, rc.Top, rc.Right, rc.Bottom);
          DrawBorder(sel.CellStyle.DiagonalUp, graphics, rc.Left, rc.Bottom, rc.Right, rc.Top);
        }
      }

      #region Рисование горизонтальных линий

      for (int i = 0; i <= blockInfo.RowIndexes.Length; i++)
        for (int j = 0; j < blockInfo.ColumnIndexes.Length; j++)
          DrawBorder(hLines[i, j], graphics, xx[j], yy[i], xx[j + 1], yy[i]);

      #endregion

      #region Рисование вертикальных линий

      for (int i = 0; i < blockInfo.RowIndexes.Length; i++)
        for (int j = 0; j <= blockInfo.ColumnIndexes.Length; j++)
          DrawBorder(vLines[i, j], graphics, xx[j], yy[i], xx[j], yy[i + 1]);

      #endregion
    }

    #endregion

    #region Рисование внутренней части ячейки

    private void PaintCellContext(XGraphics graphics, XRect rc, BRSelector sel)
    {
      if (sel.CellStyle.BackColor != BRColor.Auto)
      {
        XBrush br;
        if (!_BackBrushes.TryGetValue(sel.CellStyle.BackColor, out br))
        {
          Color clr = Color.FromArgb(sel.CellStyle.BackColor.R, sel.CellStyle.BackColor.G, sel.CellStyle.BackColor.B);
          br = new SolidBrush(clr);
          _BackBrushes.Add(sel.CellStyle.BackColor, br);
        }

        graphics.DrawRectangle(br, rc);
      }

      PdfTextRenderer renderer = GetTextRenderer(sel, graphics);
      PrepareRendererAlign(sel, renderer);

      //renderer.Graphics = graphics;
      string[] lines;
      try
      {
        lines = BRPdfMeasurer.GetLines(renderer, sel, (int)(rc.Width / Scale));
      }
      catch (Exception e)
      {
        graphics.DrawRectangle(XBrushes.Red, rc);
        lines = new string[] { "Ошибка получения текста", e.Message };
      }

      if (lines.Length == 0 && sel.CellStyle.TextFiller == BRTextFiller.None)
        return;

      // Область для текста с учетом отступов
      rc.X += sel.CellStyle.LeftMargin * Scale;
      rc.Y += sel.CellStyle.TopMargin * Scale;
      rc.Width -= (sel.CellStyle.LeftMargin + sel.CellStyle.RightMargin) * Scale;
      rc.Height -= (sel.CellStyle.TopMargin + sel.CellStyle.BottomMargin) * Scale;
      if (sel.CellStyle.IndentLevel > 0)
      {
        double off = (float)(renderer.FontWidth / 72f * 254f * sel.CellStyle.IndentLevel) * Scale;
        switch (sel.CellStyle.HAlign)
        {
          case BRHAlign.Left:
            rc.X += off;
            rc.Width -= off;
            break;
          case BRHAlign.Right:
            rc.Width -= off;
            break;
        }
      }

      double orgFontWidth = -1; // Если потребуется поменять ширину шрифта, то здесь ее запомним
      if (sel.CellStyle.MaxEnlargePercent > 100)
      {
        // Разрешено увеличение размеров шрифта
        // Реальная ширина текста (для самой длинной строки)
        double textW = 0f;
        for (int i = 0; i < lines.Length; i++)
          textW = Math.Max(textW, renderer.MeasureString(lines[i]).Width);
        // Здесь, в отличии от TextFiller'а, при измерении прямоугольник не задается,
        // т.к. нас интересует абстрактный размер без возможных дополнительных
        // уменьшений, которые будут при рисовании
        if (textW < rc.Width && textW > 0.0)
        {
          // Требуется увеличение
          int enPrc = (int)(rc.Width / textW * 100.0);
          if (enPrc > sel.CellStyle.MaxEnlargePercent)
          {
            if (sel.CellStyle.AlwaysEnlarge)
              enPrc = sel.CellStyle.MaxEnlargePercent;
            else
              enPrc = 100;
          }

          if (enPrc > 100)
          {
            orgFontWidth = renderer.FontWidth;
            renderer.FontWidth = orgFontWidth * enPrc / 100f;
            //float TextW2 = 0f;
            //for (int i = 0; i < lines.Length; i++)
            //  TextW2 = Math.Max(TextW, renderer.MeasureString(lines[i]).Width);
          }
        }
      }

      // Рисование текстовых строк
      renderer.DrawLines(lines, rc);
      if (orgFontWidth > 0)
        renderer.FontWidth = orgFontWidth;

      // Прорисовка заполнителя
      if (sel.CellStyle.TextFiller != BRTextFiller.None)
      {
        // Реальная ширина текста (для самой длинной строки)
        double textW = 0f;
        for (int i = 0; i < lines.Length; i++)
          textW = Math.Max(textW, renderer.MeasureString(lines[i], new XSize(rc.Width, rc.Height)).Width);

        XRect fillerRC1 = rc; // слева от текста
        XRect fillerRC2 = rc; // справа от текста

        switch (sel.ActualHAlign)
        {
          case BRHAlign.Left:
            // Заполнитель справа
            fillerRC2.X += textW;
            fillerRC2.Width -= textW;
            DrawTextFiller(renderer, sel, fillerRC2);
            break;
          case BRHAlign.Right:
            // Заполнитель слева
            fillerRC1.Width -= textW;
            DrawTextFiller(renderer, sel, fillerRC1);
            break;
          case BRHAlign.Center:
            // Заполнитель слева
            fillerRC1.Width -= textW;
            fillerRC1.Width /= 2;
            DrawTextFiller(renderer, sel, fillerRC1);
            // Заполнитель справа
            fillerRC2.Width -= textW;
            fillerRC2.Width /= 2;
            fillerRC2.X += (float)textW;
            fillerRC2.X += fillerRC2.Width;
            DrawTextFiller(renderer, sel, fillerRC2);
            break;
        }
      }

      // Теперь уменьшаем размер
      if (orgFontWidth >= 0)
        renderer.FontWidth = orgFontWidth;
    }


    private static void PrepareRendererAlign(BRSelector sel, PdfTextRenderer renderer)
    {
      switch (sel.ActualHAlign)
      {
        case BRHAlign.Left:
          renderer.StringFormat.Alignment = XStringAlignment.Near;
          break;
        case BRHAlign.Center:
          renderer.StringFormat.Alignment = XStringAlignment.Center;
          break;
        case BRHAlign.Right:
          renderer.StringFormat.Alignment = XStringAlignment.Far;
          break;
      }
      switch (sel.CellStyle.VAlign)
      {
        case BRVAlign.Top:
          renderer.StringFormat.LineAlignment = XLineAlignment.Near;
          break;
        case BRVAlign.Center:
          renderer.StringFormat.LineAlignment = XLineAlignment.Center;
          break;
        case BRVAlign.Bottom:
          renderer.StringFormat.LineAlignment = XLineAlignment.Far;
          break;
      }
    }

    #endregion

    #region Рисование TextFiiler

    /// <summary>
    /// Перо для рисования ячеек с прочеркиванием
    /// Толщина линии фиксированная
    /// Цвет задается перед рисованием
    /// </summary>
    private XPen TextFillerPen;

    /// <summary>
    /// Рисование заполнителя
    /// </summary>
    private void DrawTextFiller(PdfTextRenderer renderer, BRSelector sel, XRect rc)
    {
      if (sel.CellStyle.TextFiller == BRTextFiller.None)
        return;
      if (rc.Height <= 0f || rc.Width <= 0f)
        return;

      TextFillerPen.Color = renderer.Color; // прочерк всегда имеет цвет текста
      if (sel.CellStyle.TextFiller == BRTextFiller.TwoLines)
      {
        // Перо
        TextFillerPen.Width = ThinLineWidth01mm * Scale; // Тонкая линия
        // Расстояние между двумя линиями
        float dh = (sel.CellStyle.FontHeightPt / 72f * 254f) * 0.25f * Scale; // !!!
        renderer.Graphics.DrawLine(TextFillerPen,
          rc.X, rc.Y + (rc.Height - dh) / 2f,
          rc.Right, rc.Y + (rc.Height - dh) / 2f);
        renderer.Graphics.DrawLine(TextFillerPen,
          rc.X, rc.Y + (rc.Height + dh) / 2f,
          rc.Right, rc.Y + (rc.Height + dh) / 2f);
      }
      else
      {
        float w;

        switch (sel.CellStyle.TextFiller)
        {
          case BRTextFiller.Thin: w = ThinLineWidth01mm; break;
          case BRTextFiller.Medium: w = MediumLineWidth01mm; break;
          case BRTextFiller.Thick: w = ThickLineWidth01mm; break;
          default: return;
        }
        // Перо
        TextFillerPen.Width = w * Scale; // Тонкая линия
        renderer.Graphics.DrawLine(TextFillerPen,
          rc.X, rc.Y + rc.Height / 2f,
          rc.Right, rc.Y + rc.Height / 2f);
      }
    }

    #endregion

    #region Вспомогательные методы рисования рамок

    #region Константы

    /// <summary>
    /// Толщина пера для тонких линий в единицах 0.1 мм
    /// </summary>
    public const float ThinLineWidth01mm = 2f;

    /// <summary>
    /// Толщина пера для средних линий в единицах 0.1 мм
    /// </summary>
    public const float MediumLineWidth01mm = 5f;

    /// <summary>
    /// Толщина пера для толстых линий в единицах 0.1 мм
    /// </summary>
    public const float ThickLineWidth01mm = 10f;


    ///// <summary>
    ///// Толщина пера для тонких линий в пунктах
    ///// </summary>
    //public const double ThinLineWidthPt = 0.57;

    ///// <summary>
    ///// Толщина пера для средних линий в пунктах
    ///// </summary>
    //public const double MediumLineWidthPt = 1.42;

    ///// <summary>
    ///// Толщина пера для толстых линий в пунктах
    ///// </summary>
    //public const double ThickLineWidthPt = 2.83;


    ///// <summary>
    ///// Толщина пера для тонких линий в пунктах
    ///// </summary>
    //public const int ThinLineWidthTwips = 11;

    ///// <summary>
    ///// Толщина пера для средних линий в пунктах
    ///// </summary>
    //public const int MediumLineWidthTwips = 28;

    ///// <summary>
    ///// Толщина пера для толстых линий в пунктах
    ///// </summary>
    //public const int ThickLineWidthTwips = 57;

    //public static double GetLineWidthPt(PaperDocLineStyle LineStyle)
    //{
    //  switch (LineStyle)
    //  {
    //    case BRLineStyle.None: return 0;
    //    case BRLineStyle.Medium: return MediumLineWidthPt;
    //    case BRLineStyle.Thick: return ThickLineWidthPt;
    //    default: return ThinLineWidthPt;
    //  }
    //}

    #endregion

    /// <summary>
    /// Перо для рисования рамок
    /// Толщина линии задается перед рисованием
    /// </summary>
    private XPen BorderPen;

    //public void DrawBorders(BRSelector sel, XGraphics graphics, XRect rc)
    //{
    //  DrawBorder(sel.CellStyle.TopBorder, graphics, rc.Left, rc.Top, rc.Right, rc.Top);
    //  DrawBorder(sel.CellStyle.BottomBorder, graphics, rc.Left, rc.Bottom, rc.Right, rc.Bottom);
    //  DrawBorder(sel.CellStyle.LeftBorder, graphics, rc.Left, rc.Top, rc.Left, rc.Bottom);
    //  DrawBorder(sel.CellStyle.RightBorder, graphics, rc.Right, rc.Top, rc.Right, rc.Bottom);
    //}

    private void DrawBorder(BRLine line, XGraphics graphics, double x1,
      double y1, double x2, double y2)
    {

      if (line.Style == BRLineStyle.None)
        return;
      MyInitBorderPen(line);
      graphics.DrawLine(BorderPen, x1, y1, x2, y2);
    }

    //public void DrawBorder(BRLine line, XGraphics graphics, int x1,
    //  int y1, int x2, int y2)
    //{
    //  if (line.Style == BRLineStyle.None)
    //    return;
    //  MyInitBorderPen(line);
    //  graphics.DrawLine(BorderPen, x1, y1, x2, y2);
    //}

    private void MyInitBorderPen(BRLine line)
    {
      switch (line.Style)
      {
        case BRLineStyle.Thin:
        case BRLineStyle.Medium:
        case BRLineStyle.Thick:
          BorderPen.DashStyle = XDashStyle.Solid;
          break;
        case BRLineStyle.Dot:
          BorderPen.DashStyle = XDashStyle.Dot;
          break;
        case BRLineStyle.Dash:
          BorderPen.DashStyle = XDashStyle.Dash;
          break;
        case BRLineStyle.DashDot:
          BorderPen.DashStyle = XDashStyle.DashDot;
          break;
        case BRLineStyle.DashDotDot:
          BorderPen.DashStyle = XDashStyle.DashDotDot;
          break;
        default:
          throw new ArgumentException("Неправильная толщина линии");
      }
      BorderPen.Width = BRLine.GetLineWidthPt01mm(line.Style) * Scale;
      if (line.Color == BRColor.Auto)
        BorderPen.Color = XColor.FromArgb(0, 0, 0);
      else
        BorderPen.Color = XColor.FromArgb(line.Color.R, line.Color.G, line.Color.B);
    }

    #endregion

    #region Сводка

    /// <summary>
    /// Строка для заполнения свойства Creator.
    /// Возвращается текст, содержащий только символы с кодами 20-127, чтобы
    /// ее хорошо было видно в файле
    /// Строка соответствует определению useragent
    /// в протоколе HTTP, описанного в RFC2616 в разделе 14.43 
    /// </summary>
    internal static string CreatorString
    {
      get
      {
        Assembly asm = Assembly.GetExecutingAssembly();
        AssemblyName an = new AssemblyName(asm.FullName);
        return "FreeLibSet/" + an.Version.ToString();
      }
    }

    private static string ConvertSummaryString(string s)
    {
      char[] ch = new char[s.Length];
      for (int i = 0; i < s.Length; i++)
      {
        int b1 = s[i] & 0xFF;
        int b2 = (s[i] & 0xFF00) >> 8;
        ch[i] = (char)((b1 << 8) | b2);
      }
      return new string(ch);
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Преобразование единиц 0.1 мм в единицы измерения PDF
    /// </summary>
    /// <param name="Value01mm"></param>
    /// <returns></returns>
    private static XUnit LMToXUnit(int Value01mm)
    {
      return new XUnit(Value01mm / 10.0, XGraphicsUnit.Millimeter);
    }

    /// <summary>
    /// Преобразование единиц 0.1 мм в координаты устройства (пункты)
    /// </summary>
    /// <param name="Value01mm"></param>
    /// <returns></returns>
    private static double LMToCoord(int Value01mm)
    {
      return Value01mm / 254.0 * 72.0;
    }

    private static double LMToCoord(double Value01mm)
    {
      return Value01mm / 254.0 * 72.0;
    }

    #endregion
  } 

  /// <summary>
  /// Статические методы для работы с библиотекой PdfSharp
  /// </summary>
  public static class PdfFileTools
  {
    #region Наличие библиотеки SharpZipLib

    private static readonly object SyncRoot = new object();

    /// <summary>
    /// Возвращает true, если библиотека PdfSharp.dll загружена
    /// и можно создавать объекты <see cref="BRFilePdf"/>.
    /// </summary>
    public static bool PdfLibAvailable
    {
      [DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
      get
      {
        lock (SyncRoot)
        {
          if (!_PdfLibAvailable.HasValue)
          {
            switch (Environment.OSVersion.Platform)
            {
              case PlatformID.Win32NT:
              case PlatformID.Win32Windows:
                try
                {
                  TryTestPdfLibFile();
                  _PdfLibAvailable = true;
                }
                catch
                {
                  _PdfLibAvailable = false;
                }
                break;
              default:
                // Linux+Mono без Wine:
                // Библиотека-то доступна, но при попытке загрузить шрифт возникает ошибка из-за отсутствия User32.dll.
                // Надо подумать о сборке библиотеки PdfSharp с использованием GdiPlus, но там тоже все не очевидно.

                _PdfLibAvailable = false;
                break;
            }
          }

          return _PdfLibAvailable.Value;
        }
      }
    }
    private static bool? _PdfLibAvailable = null;

    /// <summary>
    /// Это должно быть в отдельном методе, т.к. оно может не запускаться
    /// </summary>
    [DebuggerStepThrough]
    private static void TryTestPdfLibFile()
    {
      Type dummy = typeof(PdfDocument);
    }

    /// <summary>
    /// Выбрасывает исключение, если <see cref="PdfLibAvailable"/>=false.
    /// </summary>
    public static void CheckPdfLibAvailable()
    {
      if (!PdfLibAvailable)
        throw new DllNotFoundException("Не удалось загрузить библиотеку PdfSharp.dll. Без нее невозможно создание pdf-файлов");
    }

    #endregion
  }
}
