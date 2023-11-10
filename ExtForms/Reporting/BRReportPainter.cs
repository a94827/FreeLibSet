﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Reflection;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.IO;
using FreeLibSet.Reporting;

#pragma warning disable 1591


namespace FreeLibSet.Drawing.Reporting
{
  public sealed class BRMeasurer : IBRMeasurer
  {
    #region Защищенный конструктор

    private BRMeasurer() { }

    #endregion

    #region IBRMeasurer

    /// <summary>
    /// Объект для измерения шрифта.
    /// Создается при первом обращении.
    /// Все вызовы должны блокироваться
    /// </summary>
    private ExtTextRenderer _Renderer;

    public int GetWantedHeight(BRSelector sel, int columnWidth)
    {

      if (_Renderer == null)
        _Renderer = new ExtTextRenderer();

      lock (_Renderer)
      {
        InitRenderer(_Renderer, sel.CellStyle);
        return GetWantedHeight(_Renderer, sel, columnWidth);
      }

    }

    public void MeasureString(string s, BRCellStyle cellStyle, out int width, out int height)
    {
      if (_Renderer == null)
        _Renderer = new ExtTextRenderer();

      lock (_Renderer)
      {
        InitRenderer(_Renderer, cellStyle);
        Size sz = _Renderer.MeasureStringLM(s);
        width = sz.Width;
        height = sz.Height;
      }
    }


    internal static void InitRenderer(ExtTextRenderer renderer, BRCellStyle cellStyle)
    {
      renderer.FontName = cellStyle.FontName;
      renderer.FontHeight = cellStyle.FontHeightPt;
      if (cellStyle.LineHeightTwip > 0)
        renderer.LineHeight = cellStyle.LineHeightPt;
      else
        renderer.LineHeight = 0;
      if (cellStyle.FontWidthPt > 0)
        renderer.FontWidth = cellStyle.FontWidthPt;
      else if (cellStyle.FontWidthPercent > 0 && cellStyle.FontWidthPercent != 100)
        renderer.FontWidth = renderer.DefaultFontWidth * cellStyle.FontWidthPercent / 100f;
      else
        renderer.FontWidth = 0;
      renderer.Bold = cellStyle.Bold;
      renderer.Italic = cellStyle.Italic;
      renderer.Underline = cellStyle.Underline;
      renderer.Strikeout = cellStyle.Strikeout;

      renderer.WordWrap = cellStyle.WrapMode == BRWrapMode.WordWrap;

      BRColor clr = cellStyle.ForeColor;
      if (clr != BRColor.Auto)
        renderer.Color = Color.FromArgb(clr.R, clr.G, clr.B);
      else
        renderer.Color = Color.Black;
    }

    internal static int GetWantedHeight(ExtTextRenderer renderer, BRSelector sel, int columnWidth)
    {
      int h = 0;
      string[] a = GetLines(renderer, sel, columnWidth);
      int w = GetTextAreaWidth(renderer, sel, columnWidth);

      for (int i = 0; i < a.Length; i++)
      {
        Size sz = renderer.MeasureStringLM(a[i], w);
        h += sz.Height;
      }
      return h + sel.CellStyle.TopMargin + sel.CellStyle.BottomMargin;
    }

    private static int GetTextAreaWidth(ExtTextRenderer renderer, BRSelector sel, int columnWidth)
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
    internal static string[] GetLines(ExtTextRenderer renderer, BRSelector sel, int columnWidth)
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

    public static readonly BRMeasurer Default = new BRMeasurer();

    #endregion
  }


  public class BRReportPainter : DisposableObject, IBRMeasurer
  {
    #region Конструктор и Disposing

    public BRReportPainter()
    {
      Scale = 0.1f;
      //CellFrames = false;
      //ShowHiddenCells = false;
      //ShowSampleValues = true;

      _FontRenderers = new DisposableDictionary<string, ExtTextRenderer>();
      //_FontRenderers.DisposeOnDestuction = true;
      _BackBrushes = new DisposableDictionary<BRColor, Brush>();
      //_BackBrushes.DisposeOnDestuction = true;

      BorderPen = new Pen(Color.Black);
      LeaderPen = new Pen(Color.Black);

      _SB = new StringBuilder();
    }

    protected override void Dispose(bool Disposing)
    {
      DataTools.Dispose(ref _FontRenderers);
      DataTools.Dispose<DisposableDictionary<BRColor, Brush>>(ref _BackBrushes);
      DataTools.Dispose<Pen>(ref BorderPen);
      DataTools.Dispose<Pen>(ref LeaderPen);
      base.Dispose(Disposing);
    }

    /// <summary>
    /// Сброс внутренних графических объектов в исходное состояние.
    /// Метод должен вызываться, если объект ParerDoc изменился в процессе
    /// использования данного объекта (в редакторе)
    /// </summary>
    public void Reset()
    {
      _FontRenderers.Clear();
      _BackBrushes.Clear();
    }

    #endregion

    #region Внутренние графические объекты для рисования

    #region Шрифты

    /// <summary>
    /// Шрифты
    /// Ключ - кодированное описание шрифта
    /// Значение - объект для рисования
    /// </summary>
    private DisposableDictionary<string, ExtTextRenderer> _FontRenderers;

    private ExtTextRenderer GetTextRenderer(BRCellStyle cellStyle)
    {
      _SB.Length = 0;
      _SB.Append(cellStyle.FontName);
      _SB.Append('|');
      _SB.Append(cellStyle.FontHeightTwip);
      _SB.Append('|');
      _SB.Append(cellStyle.LineHeightTwip);
      _SB.Append('|');
      _SB.Append(cellStyle.FontWidthTwip);
      _SB.Append('|');
      _SB.Append(cellStyle.FontWidthPercent);
      _SB.Append('|');
      _SB.Append(cellStyle.Bold ? '1' : '0');
      _SB.Append(cellStyle.Italic ? '1' : '0');
      _SB.Append(cellStyle.Underline ? '1' : '0');
      _SB.Append(cellStyle.Strikeout ? '1' : '0');
      _SB.Append('|');
      _SB.Append((int)(cellStyle.WrapMode));
      _SB.Append('|');
      _SB.Append(cellStyle.ForeColor.IntValue);

      string key = _SB.ToString();
      ExtTextRenderer renderer;
      if (!_FontRenderers.TryGetValue(key, out renderer))
      {
        renderer = new ExtTextRenderer();
        BRMeasurer.InitRenderer(renderer, cellStyle);

        _FontRenderers.Add(key, renderer);
      }
      return renderer;
    }

    #endregion

    /// <summary>
    /// Кисти фона
    /// Ключ - описатель цвета фона
    /// Значение - объект кисти или null
    /// </summary>
    private DisposableDictionary<BRColor, Brush> _BackBrushes;

    /// <summary>
    /// Используется при создании ключей по описанию шрифта
    /// </summary>
    private StringBuilder _SB;


    #endregion

    #region Опции рисования

    /// <summary>
    /// Масштаб
    /// </summary>
    private float Scale;

    ///// <summary>
    ///// True, если нужно рисовать скрытые строки и столбцы
    ///// </summary>
    //public bool ShowHiddenCells;

    ///// <summary>
    ///// True, если нужно выводить значения ячеек с ValueKind=Sample
    ///// </summary>
    //public bool ShowSampleValues;

    ///// <summary>
    ///// True, если требуется рисовать цветные рамки вокруг ячеек
    ///// </summary>
    //public bool CellFrames;

    ///// <summary>
    ///// Если true, то фон не рисуется, а текст рисуется белым шрифтом
    ///// </summary>
    //public bool SelectedCell;

    #endregion

    #region Рисование страницы

    /// <summary>
    /// Рисование одной страницы.
    /// Предполагается, что рисование уже было выполнено
    /// </summary>
    /// <param name="pageInfo">Информация о странице, полученная в процессе разбиения</param>
    /// <param name="graphics">Контекст для рисования</param>
    public void Paint(BRPaginatorPageInfo pageInfo, Graphics graphics)
    {
      DoPaint(pageInfo, graphics, 0f, 0f);
    }
    public void Paint(BRPaginatorPageInfo pageInfo, Graphics graphics, PageSettings pageSettings)
    {
      float dx = pageSettings.HardMarginX / 100f * 25.4f;
      float dy = pageSettings.HardMarginY / 100f * 25.4f;
      DoPaint(pageInfo, graphics, dx, dy);
    }
    private void DoPaint(BRPaginatorPageInfo pageInfo, Graphics graphics, float dx, float dy)
    {
      GraphicsState oldState = graphics.Save();
      try
      {
        graphics.PageUnit = GraphicsUnit.Millimeter;

        graphics.TranslateTransform(-dx, -dy);

        // Без этого неправильно будет рисоваться прочеркивание текста
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        graphics.TextContrast = 0;

        for (int i = 0; i < pageInfo.Blocks.Length; i++)
          Paint(pageInfo.Blocks[i], graphics);
      }
      finally
      {
        graphics.Restore(oldState);
        _FontRenderers.Clear(); // можно было бы установить свойство Graphics=null
      }
    }

    private void Paint(BRPaginatiorBlockInfo blockInfo, Graphics graphics)
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
          RectangleF rc = new RectangleF(x1, y1, x2 - x1, y2 - y1);
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

    private void PaintCellContext(Graphics graphics, RectangleF rc, BRSelector sel)
    {
      if (sel.CellStyle.BackColor != BRColor.Auto)
      {
        Brush br;
        if (!_BackBrushes.TryGetValue(sel.CellStyle.BackColor, out br))
        {
          Color clr = Color.FromArgb(sel.CellStyle.BackColor.R, sel.CellStyle.BackColor.G, sel.CellStyle.BackColor.B);
          br = new SolidBrush(clr);
          _BackBrushes.Add(sel.CellStyle.BackColor, br);
        }

        graphics.FillRectangle(br, rc);
      }

      ExtTextRenderer renderer = GetTextRenderer(sel.CellStyle);
      PrepareRendererAlign(sel, renderer);

      string[] lines;
      try
      {
        lines = BRMeasurer.GetLines(renderer, sel, (int)(rc.Width / Scale));
      }
      catch (Exception e)
      {
        graphics.FillRectangle(Brushes.Red, rc);
        lines = new string[] { "Ошибка получения текста", e.Message };
      }

      if (lines.Length == 0 && sel.CellStyle.TextLeader == BRTextLeader.None)
        return;

      // Область для текста с учетом отступов
      renderer.Graphics = graphics;
      rc.X += sel.CellStyle.LeftMargin * Scale;
      rc.Y += sel.CellStyle.TopMargin * Scale;
      rc.Width -= (sel.CellStyle.LeftMargin + sel.CellStyle.RightMargin) * Scale;
      rc.Height -= (sel.CellStyle.TopMargin + sel.CellStyle.BottomMargin) * Scale;
      if (sel.CellStyle.IndentLevel > 0)
      {
        float off = (float)(renderer.FontWidth / 72f * 254f * sel.CellStyle.IndentLevel) * Scale;
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

      float orgFontHeight = renderer.FontHeight;
      float orgFontWidth = renderer.FontWidth; // Если потребуется поменять ширину шрифта, то здесь ее запомним
      if (sel.CellStyle.MaxEnlargePercent > 100)
      {
        // Разрешено увеличение размеров шрифта
        // Реальная ширина текста (для самой длинной строки)
        float textW = 0f;
        for (int i = 0; i < lines.Length; i++)
          textW = Math.Max(textW, renderer.MeasureString(lines[i]).Width);
        // Здесь, в отличии от Leader'а, при измерении прямоугольник не задается,
        // т.к. нас интересует абстрактный размер без возможных дополнительных
        // уменьшений, которые будут при рисовании
        if (textW < rc.Width && textW > 0f)
        {
          // Требуется увеличение
          //int enPrc = (int)((((rc.Width - textW) / textW) + 1f) * 100f);
          int enPrc = (int)(rc.Width / textW * 100f);
          if (enPrc > sel.CellStyle.MaxEnlargePercent)
          {
            if (sel.CellStyle.AlwaysEnlarge)
              enPrc = sel.CellStyle.MaxEnlargePercent;
            else
              enPrc = 100;
          }

          if (enPrc > 100)
          {
            renderer.FontWidth = orgFontWidth * enPrc / 100f;
            //float TextW2 = 0f;
            //for (int i = 0; i < lines.Length; i++)
            //  TextW2 = Math.Max(TextW, renderer.MeasureString(lines[i]).Width);
          }
        }
      }

      // Рисование текстовых строк
      //System.Windows.Forms.MessageBox.Show(rc.ToString());
      renderer.DrawLines(lines, rc);
      renderer.FontHeight = orgFontHeight;
      renderer.FontWidth = orgFontWidth;

      // Прорисовка заполнителя
      if (sel.CellStyle.TextLeader != BRTextLeader.None)
      {
        // Реальная ширина текста (для самой длинной строки)
        float textW = 0f;
        for (int i = 0; i < lines.Length; i++)
          textW = Math.Max(textW, renderer.MeasureString(lines[i], new SizeF(rc.Width, rc.Height)).Width);

        RectangleF LeaderRC1 = rc; // слева от текста
        RectangleF LeaderRC2 = rc; // справа от текста

        switch (sel.ActualHAlign)
        {
          case BRHAlign.Left:
            // Заполнитель справа
            LeaderRC2.X += textW;
            LeaderRC2.Width -= textW;
            DrawLeader(renderer, sel, LeaderRC2);
            break;
          case BRHAlign.Right:
            // Заполнитель слева
            LeaderRC1.Width -= textW;
            DrawLeader(renderer, sel, LeaderRC1);
            break;
          case BRHAlign.Center:
            // Заполнитель слева
            LeaderRC1.Width -= textW;
            LeaderRC1.Width /= 2;
            DrawLeader(renderer, sel, LeaderRC1);
            // Заполнитель справа
            LeaderRC2.Width -= textW;
            LeaderRC2.Width /= 2;
            LeaderRC2.X += textW;
            LeaderRC2.X += LeaderRC2.Width;
            DrawLeader(renderer, sel, LeaderRC2);
            break;
        }
      }

      // Теперь уменьшаем размер
      if (orgFontWidth >= 0)
        renderer.FontWidth = orgFontWidth;
    }

    private static void PrepareRendererAlign(BRSelector sel, ExtTextRenderer renderer)
    {
      switch (sel.ActualHAlign)
      {
        case BRHAlign.Left:
          renderer.StringFormat.Alignment = StringAlignment.Near;
          break;
        case BRHAlign.Center:
          renderer.StringFormat.Alignment = StringAlignment.Center;
          break;
        case BRHAlign.Right:
          renderer.StringFormat.Alignment = StringAlignment.Far;
          break;
      }
      switch (sel.CellStyle.VAlign)
      {
        case BRVAlign.Top:
          renderer.StringFormat.LineAlignment = StringAlignment.Near;
          break;
        case BRVAlign.Center:
          renderer.StringFormat.LineAlignment = StringAlignment.Center;
          break;
        case BRVAlign.Bottom:
          renderer.StringFormat.LineAlignment = StringAlignment.Far;
          break;
      }
    }

    #endregion

    #region Рисование TextLeader

    /// <summary>
    /// Перо для рисования ячеек с прочеркиванием
    /// Толщина линии фиксированная
    /// Цвет задается перед рисованием
    /// </summary>
    private Pen LeaderPen;

    /// <summary>
    /// Рисование заполнителя
    /// </summary>
    private void DrawLeader(ExtTextRenderer renderer, BRSelector sel, RectangleF rc)
    {
      if (sel.CellStyle.TextLeader == BRTextLeader.None)
        return;
      if (rc.Height <= 0f || rc.Width <= 0f)
        return;

      LeaderPen.Color = renderer.Color; // прочерк всегда имеет цвет текста
      if (sel.CellStyle.TextLeader == BRTextLeader.TwoLines)
      {
        // Перо
        LeaderPen.Width = (float)BRLine.ThinLineWidth01mm * Scale; // Тонкая линия
        // Расстояние между двумя линиями
        float dh = (sel.CellStyle.FontHeightPt / 72f * 254f) * 0.25f * Scale; // !!!
        renderer.Graphics.DrawLine(LeaderPen,
          rc.X, rc.Y + (rc.Height - dh) / 2f,
          rc.Right, rc.Y + (rc.Height - dh) / 2f);
        renderer.Graphics.DrawLine(LeaderPen,
          rc.X, rc.Y + (rc.Height + dh) / 2f,
          rc.Right, rc.Y + (rc.Height + dh) / 2f);
      }
      else
      {
        double w;

        switch (sel.CellStyle.TextLeader)
        {
          case BRTextLeader.Thin: w = BRLine.ThinLineWidth01mm; break;
          case BRTextLeader.Medium: w = BRLine.MediumLineWidth01mm; break;
          case BRTextLeader.Thick: w = BRLine.ThickLineWidth01mm; break;
          default: return;
        }
        // Перо
        LeaderPen.Width = (float)w * Scale; // Тонкая линия
        renderer.Graphics.DrawLine(LeaderPen,
          rc.X, rc.Y + rc.Height / 2f,
          rc.Right, rc.Y + rc.Height / 2f);
      }
    }

    #endregion

    #region Вспомогательные методы рисования рамок

    /// <summary>
    /// Перо для рисования рамок
    /// Толщина линии задается перед рисованием
    /// </summary>
    private Pen BorderPen;

    public void DrawBorders(BRSelector sel, Graphics graphics, RectangleF rc)
    {
      DrawBorder(sel.CellStyle.TopBorder, graphics, rc.Left, rc.Top, rc.Right, rc.Top);
      DrawBorder(sel.CellStyle.BottomBorder, graphics, rc.Left, rc.Bottom, rc.Right, rc.Bottom);
      DrawBorder(sel.CellStyle.LeftBorder, graphics, rc.Left, rc.Top, rc.Left, rc.Bottom);
      DrawBorder(sel.CellStyle.RightBorder, graphics, rc.Right, rc.Top, rc.Right, rc.Bottom);
    }

    public void DrawBorder(BRLine line, Graphics graphics, float x1,
      float y1, float x2, float y2)
    {

      if (line.Style == BRLineStyle.None)
        return;
      MyInitBorderPen(line);
      graphics.DrawLine(BorderPen, x1, y1, x2, y2);
    }

    public void DrawBorder(BRLine line, Graphics graphics, int x1,
      int y1, int x2, int y2)
    {
      if (line.Style == BRLineStyle.None)
        return;
      MyInitBorderPen(line);
      graphics.DrawLine(BorderPen, x1, y1, x2, y2);
    }

    private void MyInitBorderPen(BRLine line)
    {
      switch (line.Style)
      {
        case BRLineStyle.Thin:
          BorderPen.DashStyle = DashStyle.Solid;
          break;
        case BRLineStyle.Medium:
          BorderPen.DashStyle = DashStyle.Solid;
          break;
        case BRLineStyle.Thick:
          BorderPen.DashStyle = DashStyle.Solid;
          break;
        case BRLineStyle.Dot:
          BorderPen.DashStyle = DashStyle.Dot;
          break;
        case BRLineStyle.Dash:
          BorderPen.DashStyle = DashStyle.Dash;
          break;
        case BRLineStyle.DashDot:
          BorderPen.DashStyle = DashStyle.DashDot;
          break;
        case BRLineStyle.DashDotDot:
          BorderPen.DashStyle = DashStyle.DashDotDot;
          break;
        default:
          throw new Exception("Неправильная толщина линии");
      }
      BorderPen.Width = (float)BRLine.GetLineWidthPt01mm(line.Style) * Scale;
      if (line.Color == BRColor.Auto)
        BorderPen.Color = Color.Black;
      else
        BorderPen.Color = Color.FromArgb(line.Color.R, line.Color.G, line.Color.B);
    }

    #endregion

    #region Создание документа

    private class PrintDocumentHelper : IBRMeasurer
    {
      public PrintDocumentHelper()
      {
      }

      public BRReport Report;
      public BRPaginatorPageInfo[] Pages;
      private BRReportPainter _Painter;

      int pageCount;

      internal void BeginPrint(object sender, PrintEventArgs args)
      {
        PreparePainter();

        pageCount = 0;
      }

      private void PreparePainter()
      {
        if (_Painter == null)
        {
          _Painter = new BRReportPainter();
        }
      }

      internal void PrintPage(object sender, PrintPageEventArgs args)
      {
        int lastPageNum = Pages.Length;
        if (args.PageSettings.PrinterSettings.PrintRange == PrintRange.SomePages)
        {
          if (pageCount == 0)
            pageCount = args.PageSettings.PrinterSettings.FromPage - 1;
          lastPageNum = args.PageSettings.PrinterSettings.ToPage;
        }

        if (pageCount < lastPageNum)
          _Painter.Paint(Pages[pageCount], args.Graphics, args.PageSettings);
        pageCount++;
        args.HasMorePages = pageCount < lastPageNum;
      }

      internal void QueryPageSettings(object sender, QueryPageSettingsEventArgs args)
      {
        if (pageCount >= Pages.Length)
          return;
        BRPageSetup ps = Pages[pageCount].Section.PageSetup;
        BRReportPainter.CopyPageSettings(ps, args.PageSettings);
      }

      internal void EndPrint(object sender, PrintEventArgs args)
      {
        if (_Painter != null)
        {
          _Painter.Dispose();
          _Painter = null;
        }
      }

      int IBRMeasurer.GetWantedHeight(BRSelector sel, int columnWidth)
      {
        PreparePainter();
        return ((IBRMeasurer)_Painter).GetWantedHeight(sel, columnWidth);
      }
      void IBRMeasurer.MeasureString(string s, BRCellStyle cellStyle, out int width, out int height)
      {
        PreparePainter();
        ((IBRMeasurer)_Painter).MeasureString(s, cellStyle, out width, out height);
      }
    }

    public static PrintDocument CreatePrintDocument(BRReport report)
    {
      BRPaginatorPageInfo[] pages;
      return CreatePrintDocument(report, out pages);
    }

    public static PrintDocument CreatePrintDocument(BRReport report, out BRPaginatorPageInfo[] pages)
    {
      PrintDocumentHelper helper = new PrintDocumentHelper();
      helper.Report = report;
      BRPaginator paginator = new BRPaginator(helper);
      pages = paginator.CreatePages(report);
      helper.Pages = pages;

      PrintDocument pd = new PrintDocument();
      pd.QueryPageSettings += new QueryPageSettingsEventHandler(helper.QueryPageSettings);
      pd.BeginPrint += new PrintEventHandler(helper.BeginPrint);
      pd.PrintPage += new PrintPageEventHandler(helper.PrintPage);
      pd.EndPrint += new PrintEventHandler(helper.EndPrint);
      pd.OriginAtMargins = false;
      return pd;
    }

    public static void CopyPageSettings(BRPageSetup src, PageSettings res)
    {
      res.Landscape = src.Orientation == BROrientation.Landscape;
      int w = Inch100(src.PaperWidth);
      int h = Inch100(src.PaperHeight);
      if (src.Orientation == BROrientation.Landscape)
        DataTools.Swap<int>(ref w, ref h);

      bool found = false;
      foreach (PaperSize psz in res.PrinterSettings.PaperSizes)
      {
        if (Math.Abs(psz.Width - w) <= 2 && Math.Abs(psz.Height - h) <= 2) // с точностью плюс-минус 0.5мм
        {
          res.PaperSize = psz;
          found = true;
          break;
        }
      }
      if (!found)
      {
        // Создаем выборочный размер листа
        res.PaperSize = new PaperSize();
        res.PaperSize.Width = w;
        res.PaperSize.Height = h;
      }
      res.Margins = new Margins(Inch100(src.LeftMargin), Inch100(src.TopMargin),
        Inch100(src.RightMargin), Inch100(src.BottomMargin));
    }

    /// <summary>
    /// Переводит размеры из единиц 0.1мм в единицы 0.01 дюйма
    /// </summary>
    /// <param name="sz01mm"></param>
    /// <returns></returns>
    private static int Inch100(int sz01mm)
    {
      return (int)(Math.Round((double)sz01mm / 2.54));
    }


    #endregion

    #region IBRMeasurer

    int IBRMeasurer.GetWantedHeight(BRSelector sel, int columnWidth)
    {
      ExtTextRenderer renderer = GetTextRenderer(sel.CellStyle);
      return BRMeasurer.GetWantedHeight(renderer, sel, columnWidth);
    }
    public void MeasureString(string s, BRCellStyle cellStyle, out int width, out int height)
    {
      ExtTextRenderer renderer = GetTextRenderer(cellStyle);
      Size sz = renderer.MeasureStringLM(s);
      width = sz.Width;
      height = sz.Height;
    }

    #endregion

    #region Создание графических файлов

    public static void CreateTIFFFile(BRReport report, AbsPath filePath, BRBitmapSettingsDataItem bitmapSettings, ISplash spl)
    {
      if (report == null)
        throw new ArgumentNullException("report");
      if (filePath.IsEmpty)
        throw new ArgumentNullException("filepath");

      if (spl == null)
        spl = new DummySplash();

      using (BRReportPainter painter = new BRReportPainter())
      {
        BRPaginator paginator = new BRPaginator(painter);
        BRPaginatorPageInfo[] pages = paginator.CreatePages(report);

        Bitmap firstBmp = null;
        System.Drawing.Imaging.Encoder encSaveFlag = System.Drawing.Imaging.Encoder.SaveFlag;
        EncoderParameters pars = new EncoderParameters(1);
        ImageCodecInfo ici = ImagingTools.GetImageCodecInfo(ImageFormat.Tiff);
        if (ici == null)
          throw new InvalidOperationException("В GDI+ не установлен кодировщик для записи файлов TIFF");

        for (int i = 0; i < pages.Length; i++)
        {
          spl.PhaseText = "Прорисовка страницы " + (i + 1).ToString();

          Bitmap bmp = painter.CreateBitmap(pages[i], bitmapSettings);

          try
          {
            SaveEXIFMetadata(bmp, report);
          }
          catch { }

          spl.PhaseText = "Запись страницы " + (i + 1).ToString();

          if (i == 0)
          {
            // Первая страница
            firstBmp = bmp;
            EncoderParameter encPar = new EncoderParameter(encSaveFlag, (long)EncoderValue.MultiFrame);
            pars.Param[0] = encPar;
            firstBmp.Save(filePath.Path, ici, pars);
          }
          else
          {
            EncoderParameter encPar = new EncoderParameter(encSaveFlag, (long)EncoderValue.FrameDimensionPage);
            pars.Param[0] = encPar;
            firstBmp.SaveAdd(bmp, pars);
            bmp.Dispose();
          }

          spl.IncPercent();
        }
        spl.Complete();

        if (firstBmp != null)
        {
          // Закрываем многостраничный TIFF
          EncoderParameter encPar = new EncoderParameter(encSaveFlag, (long)EncoderValue.Flush);
          pars.Param[0] = encPar;
          firstBmp.SaveAdd(pars);

          firstBmp.Dispose();
          spl.Complete();
        }
      }
    }

    private Bitmap CreateBitmap(BRPaginatorPageInfo page, BRBitmapSettingsDataItem bitmapSettings)
    {
      PixelFormat format = bitmapSettings.PixelFormat;

      if (format == PixelFormat.Format24bppRgb || format == PixelFormat.Format32bppRgb)
        return DoCreateBitmap(page, bitmapSettings.Resolution, format, bitmapSettings.ClipMargins);
      else
      {
        Bitmap bmp;
        Bitmap bmpOrg = DoCreateBitmap(page, bitmapSettings.Resolution, PixelFormat.Format24bppRgb, bitmapSettings.ClipMargins);
        try
        {
          switch (format)
          {
            case PixelFormat.Format1bppIndexed:
            case PixelFormat.Format4bppIndexed:
            case PixelFormat.Format8bppIndexed:
              bmp = ImagingTools.ConvertToIndexed(bmpOrg, format);
              break;
            default:
              bmp = bmpOrg.Clone(new Rectangle(0, 0, bmpOrg.Width, bmpOrg.Height), format); // вылетает на большинстве машин
              break;
          }
          bmp.SetResolution(bmpOrg.HorizontalResolution, bmpOrg.VerticalResolution);
        }
        finally
        {
          bmpOrg.Dispose();
        }
        return bmp;
      }
    }

    private Bitmap DoCreateBitmap(BRPaginatorPageInfo page, float dpi, PixelFormat format, bool clipMargins)
    {
      int w1, h1;
      if (clipMargins)
      {
        //w1 = Math.Min(page.Section.PageSetup.PrintAreaWidth, page.Columns.VisibleWidth);
        //h1 = Math.Min(page.Section.PageSetup.PrintAreaHeight, page.Rows.VisibleHeight);
        w1 = page.Section.PageSetup.PrintAreaWidth;
        h1 = page.Section.PageSetup.PrintAreaHeight;
      }
      else
      {
        w1 = page.Section.PageSetup.PaperWidth;
        h1 = page.Section.PageSetup.PaperHeight;
      }
      int w2 = (int)((float)w1 / 254f * dpi);
      int h2 = (int)((float)h1 / 254f * dpi);

      Bitmap bmp = new Bitmap(w2, h2, format);
      bmp.SetResolution(dpi, dpi);
      Graphics gr;
      try
      {
        gr = Graphics.FromImage(bmp);
      }
      catch (Exception e)
      {
        throw new ArgumentException("Не удалось создать контекст вывода для изображения в формате " + format.ToString() + ". " + e.Message, "format", e);
      }
      try
      {
        gr.FillRectangle(Brushes.White, 0, 0, w2, h2);
        //this.Scale = dpi / 2540f;
        DoPaint(page, gr,
          clipMargins ? (int)((float)page.Section.PageSetup.LeftMargin / 254f * dpi) : 0,
          clipMargins ? (int)((float)page.Section.PageSetup.TopMargin / 254f * dpi) : 0);
      }
      finally
      {
        gr.Dispose();
      }

      return bmp;
    }

    public static void SaveEXIFMetadata(Image img, BRReport report)
    {
      #region Эти константы приведены в справке для свойства PropertyItem.Id

      //const int PropertyTagDocumentName = 0x010D;
      const int PropertyTagImageDescription = 0x010E;
      //const int PropertyTagImageTitle = 0x0320;
      const int PropertyTagArtist = 0x013B;
      //const int PropertyTagExifUserComment = 0x9286;

      if (!String.IsNullOrEmpty(report.DocumentProperties.Title))
        SaveEXIFMetadataProperty(img, PropertyTagImageDescription, report.DocumentProperties.Title, Encoding.UTF8); // дублируется и в название, и в тему
      //if (!String.IsNullOrEmpty(TheDoc.Subject))
      //  SaveEXIFMetadataProperty(img, PropertyTagExifUserComment, TheDoc.Subject); не работает
      if (!String.IsNullOrEmpty(report.DocumentProperties.Author))
        SaveEXIFMetadataProperty(img, PropertyTagArtist, report.DocumentProperties.Author, Encoding.UTF8);

      #endregion

      #region Эти константы не документированы

      // Выяснено опытным путем
      // Свойства записываются в формате Unicode, а не UTF-8

      const int PropertyUnknownTitle = 40091;
      const int PropertyUnknownAuthor = 40093;
      const int PropertyUnknownSubject = 40095;

      if (!String.IsNullOrEmpty(report.DocumentProperties.Title))
        SaveEXIFMetadataProperty(img, PropertyUnknownTitle, report.DocumentProperties.Title, Encoding.Unicode);
      if (!String.IsNullOrEmpty(report.DocumentProperties.Author))
        SaveEXIFMetadataProperty(img, PropertyUnknownAuthor, report.DocumentProperties.Author, Encoding.Unicode);
      if (!String.IsNullOrEmpty(report.DocumentProperties.Subject))
        SaveEXIFMetadataProperty(img, PropertyUnknownSubject, report.DocumentProperties.Subject, Encoding.Unicode);

      #endregion
    }

    private static void SaveEXIFMetadataProperty(Image img, int propertyId, string value, Encoding encoding)
    {
      PropertyItem Item = CreatePropertyItem();

      Item.Id = (int)propertyId;
      // Type=1 means Array of Bytes. 
      Item.Type = 2; // строка ASCII
      byte[] b = encoding.GetBytes(value + "\0");
      Item.Len = b.Length;
      Item.Value = b;
      img.SetPropertyItem(Item);
      // img.Save(filepath);
    }


    private static PropertyItem CreatePropertyItem()
    {
      System.Reflection.ConstructorInfo ci = typeof(PropertyItem).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public,
        null, new Type[] { }, null);

      return (PropertyItem)ci.Invoke(null);
    }

    #endregion
  }
}
