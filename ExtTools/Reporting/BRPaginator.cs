// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

#define NEW_ROW_STRIPES
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Reporting
{
  #region Класс для вычисления размеров

  internal class BRBlockSize
  {
    #region Конструктор

    public BRBlockSize()
    {
      _Indexes = new List<int>();
      _Sizes = new List<int>();
    }

    #endregion

    #region Свойства

    public List<int> Indexes { get { return _Indexes; } }
    private readonly List<int> _Indexes;

    public List<int> Sizes { get { return _Sizes; } }
    private readonly List<int> _Sizes;

    public int TotalSize { get { return _TotalSize; } }
    private int _TotalSize;

    public override string ToString()
    {
      return StringTools.ToStringJoin<int>(",", Indexes);
    }

    #endregion

    #region Методы

    public void Add(int index, int size)
    {
#if DEBUG
      if (index < 0)
        throw ExceptionFactory.ArgOutOfRange("index", index, 0, null);
      if (size < 10) // 1мм
        throw ExceptionFactory.ArgOutOfRange("size", size, 10, null);
#endif

      _Indexes.Add(index);
      _Sizes.Add(size);
      _TotalSize += size;
    }

    #endregion
  }

  #endregion

  #region Делегат

  /// <summary>
  /// Описание фрагмента полосы, который должен быть выведен 
  /// </summary>
  public sealed class BRPaginatiorBlockInfo
  {
    #region Конструктор

    internal BRPaginatiorBlockInfo(BRBand band, BRBlockSize rows, BRBlockSize columns)
    {
#if DEBUG
      if (band == null)
        throw new ArgumentNullException("band");

      if (rows.Indexes.Count == 0)
        throw new ArgumentException("Count=0", "rows");
      if (columns.Indexes.Count == 0)
        throw new ArgumentException("Count=0", "columns");

#endif
      _Band = band;
      _RowIndexes = rows.Indexes.ToArray();
      _RowHeights = rows.Sizes.ToArray();
      _ColumnIndexes = columns.Indexes.ToArray();
      _ColumnWidths = columns.Sizes.ToArray();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Полоса
    /// </summary>
    public BRBand Band { get { return _Band; } }
    private readonly BRBand _Band;

    /// <summary>
    /// Смещение начала блока относительно верхней границы <see cref="BRPageSetup.TopMargin"/> в единицах 0.1мм
    /// </summary>
    public int TopOffset
    {
      get { return _TopOffset; }
      internal set { _TopOffset = value; }
    }
    private int _TopOffset;

    /// <summary>
    /// Смещение начала блока относительно левой границы <see cref="BRPageSetup.LeftMargin"/> в единицах 0.1мм
    /// </summary>
    public int LeftOffset
    {
      get { return _LeftOffset; }
      internal set { _LeftOffset = value; }
    }
    private int _LeftOffset;

    /// <summary>
    /// Индексы печатаемых строк
    /// </summary>
    public int[] RowIndexes { get { return _RowIndexes; } }
    private readonly int[] _RowIndexes;

    /// <summary>
    /// Индексы печатаемых столбцов
    /// </summary>
    public int[] ColumnIndexes { get { return _ColumnIndexes; } }
    private readonly int[] _ColumnIndexes;


    internal ArrayIndexer<int> RowArrayIndexer
    {
      get
      {
        if (_RowArrayIndexer == null)
          _RowArrayIndexer = new ArrayIndexer<int>(_RowIndexes);
        return _RowArrayIndexer;
      }
    }
    private ArrayIndexer<int> _RowArrayIndexer;

    internal ArrayIndexer<int> ColumnArrayIndexer
    {
      get
      {
        if (_ColumnArrayIndexer == null)
          _ColumnArrayIndexer = new ArrayIndexer<int>(_ColumnIndexes);
        return _ColumnArrayIndexer;
      }
    }
    private ArrayIndexer<int> _ColumnArrayIndexer;

    /// <summary>
    /// Высота каждой строки в единицах 0.1мм
    /// </summary>
    public int[] RowHeights { get { return _RowHeights; } }
    private readonly int[] _RowHeights;

    /// <summary>
    /// Ширина каждого столбца в единицах 0.1мм
    /// </summary>
    public int[] ColumnWidths { get { return _ColumnWidths; } }
    private readonly int[] _ColumnWidths;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "BandIndex=" + Band.BandIndex.ToString() +
        ", RowIndexes=" + StringTools.ToStringJoin<int>(",", RowIndexes) +
        ", ColumnIndexes=" + StringTools.ToStringJoin<int>(",", ColumnIndexes);
    }

    #endregion

    #region Методы

    /// <summary>
    /// Получение объединения данных для ячейки, входящей в блок
    /// </summary>
    /// <param name="selector">Селектор с выбранной ячейкой. Должен относиться к текущей полосе <see cref="Band"/>.</param>
    /// <param name="isFirstCell">Сюда записывается true, если текущая ячейка является верхней левой ячейкой объединения</param>
    /// <param name="mergeRange">Сюда помещается диапазон объединения. Индексы строк и столбцов относятся к массивам <see cref="RowIndexes"/> и <see cref="ColumnIndexes"/>, а не к номерам в <see cref="Band"/>.</param>
    public void GetCellMerge(BRSelector selector, out bool isFirstCell, out BRRange mergeRange)
    {
      if (!Object.ReferenceEquals(selector.Band, this.Band))
        throw new ArgumentException(Res.BRPaginator_Arg_SelectorForAnotherBand, "selector");

      int pRow = RowArrayIndexer.IndexOf(selector.RowIndex);
      int pColumn = ColumnArrayIndexer.IndexOf(selector.ColumnIndex);
      if (pRow < 0 || pColumn < 0)
        throw new ArgumentException(Res.BRPaginator_Arg_SelectorForAnotherCell, "selector");

      BRRange orgInfo = selector.MergeInfo;
      if (orgInfo.RowCount == 1 && orgInfo.ColumnCount == 1)
      {
        // Обычная ячейка
        isFirstCell = true;
        mergeRange = new BRRange(pRow, pColumn, 1, 1);
      }
      else
      {
        // Объединенная ячейка

        int r1 = pRow;
        int r2 = pRow;
        int c1 = pColumn;
        int c2 = pColumn;
        for (int r = pRow - 1; r >= 0; r--)
        {
          if (RowIndexes[r] >= orgInfo.FirstRowIndex)
            r1 = r;
          else
            break;
        }
        for (int r = pRow + 1; r < RowIndexes.Length; r++)
        {
          if (RowIndexes[r] <= orgInfo.LastRowIndex)
            r2 = r;
          else
            break;
        }
        for (int c = pColumn - 1; c >= 0; c--)
        {
          if (ColumnIndexes[c] >= orgInfo.FirstColumnIndex)
            c1 = c;
          else
            break;
        }
        for (int c = pColumn + 1; c < ColumnIndexes.Length; c++)
        {
          if (ColumnIndexes[c] <= orgInfo.LastColumnIndex)
            c2 = c;
          else
            break;
        }

        mergeRange = new BRRange(r1, c1, r2 - r1 + 1, c2 - c1 + 1);
        isFirstCell = selector.RowIndex == orgInfo.FirstRowIndex &&
          selector.ColumnIndex == orgInfo.FirstColumnIndex;
      }

#if DEBUG
      if (mergeRange.LastRowIndex >= RowIndexes.Length ||
        mergeRange.LastColumnIndex >= ColumnIndexes.Length)
        throw new BugException("Invalid range");
#endif
    }

    #endregion
  }

  /// <summary>
  /// Коллекция объектов блоков <see cref="BRPaginatiorBlockInfo"/>, которые выводятся для секции
  /// </summary>
  public sealed class BRPaginatorPageInfo
  {
    #region Конструктор

    internal BRPaginatorPageInfo(BRSection section, BRPaginatiorBlockInfo[] blocks)
    {
      _Section = section;
      _Blocks = blocks;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Секция, к которой относится текущая страница
    /// </summary>
    public BRSection Section { get { return _Section; } }
    private readonly BRSection _Section;

    /// <summary>
    /// Печатаемые блоки.
    /// Массив может быть пустым, если для широкой таблицы требуется пропуск пустых листов
    /// </summary>
    public BRPaginatiorBlockInfo[] Blocks { get { return _Blocks; } }
    private readonly BRPaginatiorBlockInfo[] _Blocks;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Тектовое представление</returns>
    public override string ToString()
    {
      return "Section=" + _Section.Name + ", Blocks=" + _Blocks.Length.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Аргументы события <see cref="BRPaginator.PageHandling"/>
  /// </summary>
  public sealed class BRPaginatorEventArgs : EventArgs
  {
    #region Свойства

    /// <summary>
    /// Описание страниц
    /// </summary>
    public BRPaginatorPageInfo Page
    {
      get { return _Page; }
      internal set { _Page = value; }
    }
    private BRPaginatorPageInfo _Page;

    /// <summary>
    /// True, если страница - первая в процессе разбиения
    /// </summary>
    public bool IsFirstPage
    {
      get { return _IsFirstPage; }
      internal set { _IsFirstPage = value; }
    }
    private bool _IsFirstPage;

    /// <summary>
    /// True, если страница - последняя в процессе разбиения
    /// </summary>
    public bool IsLastPage
    {
      get { return _IsLastPage; }
      internal set { _IsLastPage = value; }
    }
    private bool _IsLastPage;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="BRPaginator.PageHandling"/>
  /// </summary>
  /// <param name="sender">Объект <see cref="BRPaginator"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void BRPaginatorEventHandler(object sender, BRPaginatorEventArgs args);

  #endregion

  /// <summary>
  /// Интерфейс измерителя
  /// </summary>
  public interface IBRMeasurer
  {
    #region Методы

    /// <summary>
    /// Вычисляет желаемую высоту строки, исходя из содержимого текущей ячейки.
    /// Метод должен учесть размеры полей, задаваемых <see cref="BRCellStyle.LeftMargin"/>,<see cref="BRCellStyle.RightMargin"/> и <see cref="BRCellStyle.IndentLevel"/>,
    /// но использовать ширину <paramref name="columnWidth"/>, а не <see cref="BRColumnInfo.Width"/>.
    /// </summary>
    /// <param name="sel">Селектор с выбранной ячейкой</param>
    /// <param name="columnWidth">Ширина столбца в единицах 0.1мм</param>
    /// <returns>Желаемая высота строки в единицах 0.1мм</returns>
    int GetWantedHeight(BRSelector sel, int columnWidth);

    /// <summary>
    /// Измеряет строку <paramref name="s"/>. Строка не содержит символов переноса строки.
    /// Из объекта <paramref name="cellStyle"/> должны быть извлечены параметры шрифта, но не отступы.
    /// Должны возвращаться размеры в единицах 0.1мм.
    /// </summary>
    /// <param name="s">Измеряемая строка</param>
    /// <param name="cellStyle">Отсюда извлекаются параметры шрифта</param>
    /// <param name="width">Сюда записывается высота</param>
    /// <param name="height">Сюда записывается ширина</param>
    void MeasureString(string s, BRCellStyle cellStyle, out int width, out int height);

    #endregion
  }


  /// <summary>
  /// Простейший объект для "измерения" размеров шрифта.
  /// Исходит из того, что шрифты является моноширинными.
  /// </summary>
  public sealed class BRSimpleMeasurer : IBRMeasurer
  {
    #region Экземпляр

    private BRSimpleMeasurer()
    {
    }

    /// <summary>
    /// Единственный экземпляр объекта.
    /// </summary>
    public static readonly BRSimpleMeasurer Default = new BRSimpleMeasurer();

    #endregion

    // Константы подобрал для шрифта Arial

    /// <summary>
    /// Отношение "Высота строки" / "Размер шрифта" по умолчанию
    /// </summary>
    const float mLineHeight = 1.12f;

    /// <summary>
    /// Отношение "Ширина символа" / "Размер шрифта" по умолчанию
    /// </summary>
    const float mFontWidth = 0.56f;

    /// <summary>
    /// Возвращает высоту и ширину одного символа в пунктах
    /// </summary>
    /// <param name="cellStyle"></param>
    /// <param name="lineHpt"></param>
    /// <param name="charWpt"></param>
    private void GetFontSize(BRCellStyle cellStyle, out float lineHpt, out float charWpt)
    {
      if (cellStyle.LineHeightPt == 0f)
        lineHpt = cellStyle.FontHeightPt * mLineHeight;
      else
        lineHpt = cellStyle.LineHeightPt;

      if (cellStyle.FontWidthPt == 0f)
      {
        charWpt = cellStyle.FontHeightPt * mFontWidth;
        if (cellStyle.FontWidthPercent > 0)
          charWpt *= cellStyle.FontWidthPercent / 100f;
      }
      else
        charWpt = cellStyle.FontWidthPt;
    }

    /// <summary>
    /// Вычисляет желаемую высоту строки, исходя из содержимого текущей ячейки.
    /// Метод должен учесть размеры полей, задаваемых <see cref="BRCellStyle.LeftMargin"/>,<see cref="BRCellStyle.RightMargin"/> и <see cref="BRCellStyle.IndentLevel"/>,
    /// но использовать ширину <paramref name="columnWidth"/>, а не <see cref="BRColumnInfo.Width"/>.
    /// </summary>
    /// <param name="sel">Селектор с выбранной ячейкой</param>
    /// <param name="columnWidth">Ширина столбца в единицах 0.1мм</param>
    /// <returns>Желаемая высота строки в единицах 0.1мм</returns>
    public int GetWantedHeight(BRSelector sel, int columnWidth)
    {
      float lineHpt, charWpt;
      GetFontSize(sel.CellStyle, out lineHpt, out charWpt);

      float wPt = (columnWidth - sel.CellStyle.LeftMargin - sel.CellStyle.RightMargin) / 254f * 72f;
      wPt -= charWpt * sel.CellStyle.IndentLevel;

      string s = sel.AsString;
      if (String.IsNullOrEmpty(s))// По идее, не должно быть
        s = " ";
      // Символы переноса заменяем на одинарный"\n"
      s = s.Replace("\r\n", "\n");
      s = s.Replace("\n\r", "\n");
      s = s.Replace("\r", "\n");
      s = s.TrimEnd('\n');

      int lineCount;
      if (wPt < charWpt) // Не помещается даже один символ
        lineCount = s.Length;
      else
      {
        // Сколько символов входит по ширине
        int nCharsPerLine = (int)(wPt / charWpt);

        lineCount = 1;
        int nCh = 0;
        foreach (char c in s)
        {
          if (c == '\n')
          {
            lineCount++;
            nCh = 0;
          }
          else if (c == ' ' && nCh >= nCharsPerLine && sel.CellStyle.WrapMode == BRWrapMode.WordWrap)
          {
            lineCount++;
            nCh = 0;
          }
          else
            nCh++;
        }
      }

      return (int)Math.Ceiling((lineCount * lineHpt) * 72f / 254f);
    }

    /// <summary>
    /// Измеряет строку <paramref name="s"/>. Строка не содержит символов переноса строки.
    /// Из объекта <paramref name="cellStyle"/> должны быть извлечены параметры шрифта, но не отступы.
    /// Должны возвращаться размеры в единицах 0.1мм.
    /// </summary>
    /// <param name="s">Измеряемая строка</param>
    /// <param name="cellStyle">Отсюда извлекаются параметры шрифта</param>
    /// <param name="width">Сюда записывается высота</param>
    /// <param name="height">Сюда записывается ширина</param>
    public void MeasureString(string s, BRCellStyle cellStyle, out int width, out int height)
    {
      float lineHPt, charWpt;
      GetFontSize(cellStyle, out lineHPt, out charWpt);

      width = (int)Math.Ceiling(s.Length * (charWpt / 72f * 254f));
      height = (int)Math.Ceiling((lineHPt / 72f * 254f));
    }
  }

  /// <summary>
  /// Описание строки/столбца.
  /// Содержит индекс и размеры
  /// </summary>
  public struct BRStripeItem
  {
    #region Конструктор

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="size"></param>
    public BRStripeItem(int index, int size)
    {
#if DEBUG
      if (index < 0)
        throw ExceptionFactory.ArgOutOfRange("index", index, 0, null);
      if (size <= 0)
        throw ExceptionFactory.ArgOutOfRange("size", size, 0, null);
#endif
      _Index = index;
      _Size = size;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Индекс строки/столбца
    /// </summary>
    public int Index { get { return _Index; } }
    private readonly int _Index;

    /// <summary>
    /// Высота строки / ширина столбца в единицах 0.1мм
    /// </summary>
    public int Size { get { return _Size; } }
    private readonly int _Size;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Index=" + _Index.ToString() + ", Size=" + _Size.ToString();
    }

    /// <summary>
    /// Выполняет суммирование по свойству <see cref="Size"/>.
    /// </summary>
    /// <param name="a">Массив</param>
    /// <returns>Общий размер</returns>
    public static int GetTotalSize(BRStripeItem[] a)
    {
      int sz = 0;
      for (int i = 0; i < a.Length; i++)
        sz += a[i].Size;
      return sz;
    }

    #endregion
  }

  /// <summary>
  /// Разбиение отчета на страницы.
  /// Для разбиения на страницы должен быть вызван метод Paginate(). Для каждой страницы будет сгенерировано событие PageHandling
  /// </summary>
  public sealed class BRPaginator
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="measurer">Измеритель</param>
    public BRPaginator(IBRMeasurer measurer)
    {
      if (measurer == null)
        throw new ArgumentNullException("measurer");
      _Measurer = measurer;
    }

    private IBRMeasurer _Measurer;

    #endregion

    #region Основной метод

    /// <summary>
    /// Выполнить разбиение отчета на страницы
    /// </summary>
    /// <param name="report">Отчет</param>
    public void Paginate(BRReport report)
    {
      for (int i = 0; i < report.Sections.Count; i++)
      {
        bool isFirstSection = (i == 0);
        bool isLastSection = (i == report.Sections.Count - 1);
        DoPaginate(report.Sections[i], isFirstSection, isLastSection);
      }
    }

    /// <summary>
    /// Выполнить разбиение одной секции отчета на страницы
    /// </summary>
    /// <param name="section">Секция</param>
    public void Paginate(BRSection section)
    {
      DoPaginate(section, true, true);
    }

    /// <summary>
    /// Событие вызывается в процессе разбиения для каждой страницы
    /// </summary>
    public event BRPaginatorEventHandler PageHandling;

    private void OnPageHandling(BRPaginatorEventArgs args)
    {
      if (PageHandling != null)
        PageHandling(this, args);
    }

    #endregion

    #region Обработка секции

    // Дополнительные определения:
    // ColStripe - объединение из нескольких столбцов, печатаемых на одной странице. 
    // В секции отыскивается самая широкая таблица, которая и определяет количество полос при разбиении на страницы.
    // RowStripe - подмножество строк из одной или нескольких полос Band, которые помещаются по высоте страницы.

    private void DoPaginate(BRSection section, bool isFirstSection, bool isLastSection)
    {
      #region Распределение столбцов

      // Первый индекс - таблица Band
      // Второй индекс - списки столбцов с размерами
      BRBlockSize[][] colStripes = new BRBlockSize[section.Bands.Count][];

      // Реальная ширина столбцов
      // Первый индекс - таблица Band
      // Второй идекс - столбец в полосе (без учета разбиения на страницы)
      int[][] realColWidthArrays = new int[section.Bands.Count][];
      int nColStripes = 0;
      for (int iBand = 0; iBand < section.Bands.Count; iBand++)
      {
        colStripes[iBand] = GetColumnStripes(section.Bands[iBand]).ToArray();
        nColStripes = Math.Max(nColStripes, colStripes[iBand].Length);

        realColWidthArrays[iBand] = GetColumnWidthArray(section.Bands[iBand], colStripes[iBand]);
      }

      #endregion

      #region Распределение строк

      // Первый индекс горизонтальная полоса RowStripe (страница)
      // Второй индекс - блок строк, относящихся к одному BRBand
#if NEW_ROW_STRIPES
      RowProcessor rowProc = new RowProcessor(this, section);
      rowProc.CalcRowHeights(realColWidthArrays);
      RowStripePart[][] rowStripes = rowProc.Calculate();
#else
      RowStripePart[][] rowStripes = GetRowStripes(section, realColWidthArrays);
#endif
      #endregion

      if (nColStripes == 0 || rowStripes.Length == 0)
        return;


      for (int iRowStripe = 0; iRowStripe < rowStripes.Length; iRowStripe++)
      {
        RowStripePart[] rowParts = rowStripes[iRowStripe];
        int lastPageColStripe = -1;
        if (isLastSection && iRowStripe == rowStripes.Length - 1)
        {
          for (int iRowPart = 0; iRowPart < rowParts.Length; iRowPart++)
          {
            int bandIndex = rowParts[iRowPart].Band.BandIndex;
            lastPageColStripe = Math.Max(lastPageColStripe, colStripes[bandIndex].Length - 1);
          }
        }

        for (int iColStripe = 0; iColStripe < nColStripes; iColStripe++)
        {
          List<BRPaginatiorBlockInfo> blks = new List<BRPaginatiorBlockInfo>();
          int currH = 0;
          for (int iRowPart = 0; iRowPart < rowParts.Length; iRowPart++)
          {
            int bandIndex = rowParts[iRowPart].Band.BandIndex;
            if (iColStripe < colStripes[bandIndex].Length)
            {
              BRPaginatiorBlockInfo blk = new BRPaginatiorBlockInfo(rowParts[iRowPart].Band, rowParts[iRowPart].Rows, colStripes[bandIndex][iColStripe]);
              blk.LeftOffset = 0;
              blk.TopOffset = currH + rowParts[iRowPart].TopMargin;
              blks.Add(blk);
            }
            currH += rowParts[iRowPart].Rows.TotalSize + rowParts[iRowPart].TopMargin;
          }
          InitCenterPage(section, blks);

          BRPaginatorPageInfo page = new BRPaginatorPageInfo(section, blks.ToArray());
          BRPaginatorEventArgs args = new BRPaginatorEventArgs();
          args.Page = page;
          args.IsFirstPage = isFirstSection && iRowStripe == 0 && iColStripe == 0;
          args.IsLastPage = iColStripe == lastPageColStripe;
          OnPageHandling(args);
        }
      }
    }

    private static void InitCenterPage(BRSection section, List<BRPaginatiorBlockInfo> blks)
    {
      if (section.PageSetup.CenterHorizontal)
      {
        int maxW = 0;
        foreach (BRPaginatiorBlockInfo blk in blks)
          maxW = Math.Max(maxW, DataTools.SumInt32(blk.ColumnWidths));

        int leftOffset = (section.PageSetup.PrintAreaWidth - maxW) / 2;
        foreach (BRPaginatiorBlockInfo blk in blks)
          blk.LeftOffset = leftOffset;
      }
      if (section.PageSetup.CenterVertical)
      {
        BRPaginatiorBlockInfo lastBlk = blks[blks.Count - 1];
        int totalH = lastBlk.TopOffset + DataTools.SumInt32(lastBlk.RowHeights);
        int topOffset = (section.PageSetup.PrintAreaHeight - totalH) / 2;
        foreach (BRPaginatiorBlockInfo blk in blks)
          blk.TopOffset += topOffset;
      }
    }

    #endregion

    #region Распределение столбцов

    private static List<BRBlockSize> GetColumnStripes(BRBand band)
    {
      int maxW = band.Section.PageSetup.PrintAreaWidth;
      if (maxW < BRReport.MinColumnWidth)
        throw new BugException("Wrong page setup");

      List<BRBlockSize> lst = new List<BRBlockSize>();
      BRBlockSize bs = null;

      BRBlockSize reps = null; // создадим при необходимости

      BRSelector sel = band.CreateSelector();
      for (int i = 0; i < band.ColumnCount; i++)
      {
        sel.ColumnIndex = i;
        //if (!sel.ColumnInfo.Visible)
        //  continue;
        int w = sel.ColumnInfo.Width;
        if (w > maxW)
          w = maxW;

        if (bs == null)
          bs = new BRBlockSize();

        if ((bs.TotalSize + w) > maxW)
        {
          PrepareColumnAutoGrow(band, bs, sel);
          lst.Add(bs);
          bs = new BRBlockSize();
        }

        if (bs.Indexes.Count == 0 && reps != null)
        {
          // Добавляем повторяющиеся столбцы, но только, если они помещаются
          if ((reps.TotalSize + w) <= maxW)
          {
            for (int j = 0; j < reps.Indexes.Count; j++)
              bs.Add(reps.Indexes[j], reps.Sizes[j]);
          }
        }
        bs.Add(i, w);

        if (sel.ColumnInfo.Repeatable)
        {
          if (reps == null)
            reps = new BRBlockSize();
          reps.Add(i, w);
        }
      }
      if (bs != null)
      {
        PrepareColumnAutoGrow(band, bs, sel);
        lst.Add(bs);
      }
      return lst;
    }

    private static int[] GetColumnWidthArray(BRBand band, BRBlockSize[] colStripes)
    {
      int[] a = new int[band.ColumnCount];
      for (int i = 0; i < colStripes.Length; i++)
      {
        BRBlockSize bs = colStripes[i];
        for (int j = 0; j < bs.Indexes.Count; j++)
        {
#if DEBUG
          if (bs.Sizes[j] < BRReport.MinColumnWidth)
            throw new BugException("Wrong column width");
#endif

          if (a[bs.Indexes[j]] == 0)
            a[bs.Indexes[j]] = bs.Sizes[j];
          else
            a[bs.Indexes[j]] = Math.Min(a[bs.Indexes[j]], bs.Sizes[j]);
        }
      }
      return a;
    }

    #endregion

    #region Распределение строк

    private struct RowStripePart
    {
      #region Поля

      public BRBand Band;

      public BRBlockSize Rows;

      /// <summary>
      /// Зазор перед первой строкой
      /// </summary>
      public int TopMargin;

      public override string ToString()
      {
        return "BandIndex=" + Band.BandIndex + ", Rows=" + StringTools.ToStringJoin<int>(",", Rows.Indexes);
      }

      #endregion
    }

#if !NEW_ROW_STRIPES
    private RowStripePart[][] GetRowStripes(BRSection section, int[][] realColWidthArrays)
    {
      int maxH = section.PageSetup.PrintAreaHeight;
      List<RowStripePart[]> lst1 = new List<RowStripePart[]>();
      List<RowStripePart> lst2 = new List<RowStripePart>();
      int currH = 0;
      for (int i = 0; i < section.Bands.Count; i++)
      {
        BRBand band = section.Bands[i];
        RowStripePart part = new RowStripePart();
        BRSelector sel = band.CreateSelector();
        for (int j = 0; j < band.RowCount; j++)
        {
          sel.RowIndex = j;
          if (!sel.RowInfo.Visible)
            continue;

          int h = sel.RowInfo.Height;
          if (h == 0)
            h = GetDefaultRowHeight(sel, realColWidthArrays[i]);
          if (h > maxH)
            h = maxH;

          int gap = 0;
          if (currH > 0 && j == 0)
            gap = GetBandGap(section, i);

          if (part.Rows == null)
          {
            part.Rows = new BRBlockSize();
            part.Band = band;
            part.TopMargin = gap;
          }

          if (currH + part.Rows.TotalSize + h > maxH)
          {
            lst2.Add(part);
            part = new RowStripePart();
            part.Rows = new BRBlockSize();
            part.Band = band;
            part.TopMargin = 0;

            lst1.Add(lst2.ToArray());
            lst2.Clear();
            currH = 0;
          }
          part.Rows.Add(j, h);
        } // цикл по строкам

        // Полоса закончена
        if (part.Rows != null)
        {
          currH += part.Rows.TotalSize;
          lst2.Add(part);
          part = new RowStripePart();
        }
      } // цикл по Band
      if (lst2.Count > 0)
      {
        lst1.Add(lst2.ToArray());
      }
      return lst1.ToArray();
    }
#endif

    private static int GetBandGap(BRSection section, int bandIndex)
    {
#if DEBUG
      if (bandIndex < 0 || bandIndex >= section.Bands.Count)
        throw ExceptionFactory.ArgOutOfRange("bandIndex", bandIndex, 0, section.Bands.Count - 1);
#endif
      if (bandIndex == 0)
        return 0;
      int h1 = section.Bands[bandIndex - 1].BottomMargin;
      int h2 = section.Bands[bandIndex].TopMargin;
      return Math.Max(h1, h2);
    }

    #endregion

    #region Вариант 2
#if NEW_ROW_STRIPES
    private class RowProcessor
    {
      #region Конструктор

      public RowProcessor(BRPaginator paginator, BRSection section)
      {
        _Paginator = paginator;
        _Section = section;
        _MaxHeight = section.PageSetup.PrintAreaHeight;
        if (_MaxHeight < BRReport.MinRowHeight)
          throw new BugException("Wrong page setup");
      }

      private readonly BRPaginator _Paginator;

      //public BRSection Section { get { return _Section; } }
      private readonly BRSection _Section;

      private readonly int _MaxHeight;

      #endregion

      #region Вычисление размеров строк

      /// <summary>
      /// Вычисленные размеры строк.
      /// Первая размерность массива соответствует количеству полос <see cref="BRSection.Bands"/>.
      /// Вторая размерность соответствует номеру строки в полосе <see cref="BRBand.RowCount"/>.
      /// </summary>
      private int[][] _RowHeightArrays;

      /// <summary>
      /// Индексы повторяющихся строк.
      /// Первая размерность массива соответствует количеству полос <see cref="BRSection.Bands"/>.
      /// Вторая размерность массива соответствует количеству повторяемых строк.
      /// </summary>
      private int[][] _RowRepeatableIndexes;

      /// <summary>
      /// Вычислить размеры строк
      /// </summary>
      /// <param name="realColWidthArrays">Ширины столбцов. 
      /// Первая размерность массива соответствует количеству полос <see cref="BRSection.Bands"/>.
      /// Вторая размерность соответствует номеру столбца <see cref="BRBand.ColumnCount"/></param>
      public void CalcRowHeights(int[][] realColWidthArrays)
      {
#if DEBUG
        if (realColWidthArrays.Length != _Section.Bands.Count)
          throw ExceptionFactory.ArgWrongCollectionCount("realColWidthArrays", realColWidthArrays, _Section.Bands.Count);
#endif
        _RowHeightArrays = new int[_Section.Bands.Count][];
        _RowRepeatableIndexes = new int[_Section.Bands.Count][];

        List<int> lstRRI = new List<int>();
        for (int i = 0; i < _Section.Bands.Count; i++)
        {
          BRBand band = _Section.Bands[i];
          _RowHeightArrays[i] = new int[band.RowCount];
          lstRRI.Clear();
          BRSelector sel = band.CreateSelector();
          for (int j = 0; j < band.RowCount; j++)
          {
            sel.RowIndex = j;
            //if (!sel.RowInfo.Visible)
            //  continue;

            if (sel.RowInfo.Repeatable)
              lstRRI.Add(sel.RowIndex);

            int h = sel.RowInfo.Height;
            if (h == 0)
              h = _Paginator.GetDefaultRowHeight(sel, realColWidthArrays[i]);
            if (h > _MaxHeight)
              h = _MaxHeight;
            _RowHeightArrays[i][j] = h;
          } // цикл по строкам
          if (lstRRI.Count == 0)
            _RowRepeatableIndexes[i] = EmptyArray<Int32>.Empty;
          else
            _RowRepeatableIndexes[i] = lstRRI.ToArray();
        } // цикл по Band
      }

      #endregion

      #region Основной метод

      /// <summary>
      /// Разбиение на страницы
      /// </summary>
      /// <returns>Первый индекс массива соответствует будущему количеству страниц по вертикали. Второй индекс соответствует полосам на странице</returns>
      public RowStripePart[][] Calculate()
      {
        List<RowStripePart[]> lst1 = new List<RowStripePart[]>();
        List<RowStripePart> lst2 = new List<RowStripePart>();
        int currH = 0;
        bool newPage;
        RowStripePart[] block;
        while (TryGetBlock(currH, out block, out newPage))
        {
          if (newPage)
          {
            lst1.Add(lst2.ToArray());
            lst2.Clear();
            currH = 0;
          }
          lst2.AddRange(block);
          for (int i = 0; i < block.Length; i++)
            currH += block[i].TopMargin + block[i].Rows.TotalSize;
        }

        if (lst2.Count > 0)
          lst1.Add(lst2.ToArray());

        return lst1.ToArray();
      }

      //public RowStripePart[][] Calculate()
      //{
      //  List<RowStripePart[]> lst1 = new List<RowStripePart[]>();
      //  List<RowStripePart> lst2 = new List<RowStripePart>();
      //  int currH = 0;
      //  bool newPage;
      //  RowStripePart[] block;
      //  while (TryGetBlock(currH, out block, out newPage))
      //  {
      //    if (newPage)
      //    {
      //      lst1.Add(lst2.ToArray());
      //      lst2.Clear();
      //      currH = 0;
      //    }
      //    lst2.AddRange(block);
      //  }

      //  if (lst2.Count > 0)
      //    lst1.Add(lst2.ToArray());

      //  return lst1.ToArray();
      //}


      private bool TryGetBlock(int currH, out RowStripePart[] block, out bool newPage)
      {
        if (!HasMoreRows())
        {
          block = null;
          newPage = false;
          return false;
        }

        // Выполняем попытки с последовательным ослаблением требований

        if (TryGetBlock2(currH, true, true, true, true, out block, out newPage))
          return true;
        if (TryGetBlock2(currH, false, true, true, true, out block, out newPage))
          return true;
        if (TryGetBlock2(currH, false, true, false, true, out block, out newPage))
          return true;
        if (TryGetBlock2(currH, true, false, true, true, out block, out newPage))
          return true;
        if (TryGetBlock2(currH, false, false, false, true, out block, out newPage))
          return true;
        //if (TryGetBlock2(currH, false, false, false, false, out block, out newPage))
        //  return true;
        if (TryGetBlock3(currH, false, false, false, false, out block))
        {
          newPage = currH > 0;
          return true;
        }
        throw new BugException("Data get failed");
      }

      /// <summary>
      /// Выполняем сначала попытку для оставшейся части листа бумаги, а затем - для целой страницы
      /// </summary>
      /// <param name="currH"></param>
      /// <param name="useBandKeepWhole"></param>
      /// <param name="useBandKeepWith"></param>
      /// <param name="useRowKeepWith"></param>
      /// <param name="useRowRepeatable"></param>
      /// <param name="block"></param>
      /// <param name="newPage"></param>
      /// <returns></returns>
      private bool TryGetBlock2(int currH, bool useBandKeepWhole, bool useBandKeepWith, bool useRowKeepWith, bool useRowRepeatable, out RowStripePart[] block, out bool newPage)
      {
        newPage = false;
        if (TryGetBlock3(currH, useBandKeepWhole, useBandKeepWith, useRowKeepWith, useRowRepeatable, out block))
          return true;
        if (currH > 0)
        {
          newPage = true;
          return TryGetBlock3(0, useBandKeepWhole, useBandKeepWith, useRowKeepWith, useRowRepeatable, out block);
        }
        else
          return false;
      }

      #endregion

      #region Текущая строка

      /// <summary>
      /// Индекс текущей полосы.
      /// Меняется после удачной попытки извлечения.
      /// </summary>
      private int _CurrBandIndex;

      /// <summary>
      /// Индекс текущей строки в полосе.
      /// Меняется после удачной попытки извлечения.
      /// </summary>
      private int _CurrRowIndex;

      /// <summary>
      /// Возвращает true, если есть еще данные
      /// </summary>
      /// <returns></returns>
      bool HasMoreRows()
      {
        while (_CurrBandIndex < _Section.Bands.Count)
        {
          while (_CurrRowIndex < _Section.Bands[_CurrBandIndex].RowCount)
          {
            if (_RowHeightArrays[_CurrBandIndex][_CurrRowIndex] > 0)
              return true;
            else
              _CurrRowIndex++;
          }
          _CurrBandIndex++;
          _CurrRowIndex = 0;
        }
        return false;
      }


      #endregion

      #region Перечислитель

      private struct BandAndRow
      {
        public BandAndRow(int bandIndex, int rowIndex)
        {
          _BandIndex = bandIndex;
          _RowIndex = rowIndex;
        }

        public int BandIndex { get { return _BandIndex; } }
        private readonly int _BandIndex;

        public int RowIndex { get { return _RowIndex; } }
        private readonly int _RowIndex;

        public override string ToString()
        {
          return "BandIndex=" + BandIndex.ToString() + ", RowIndex=" + RowIndex.ToString();
        }
      }

      private struct SectionRowEnumerable : IEnumerable<BandAndRow>
      {
        public SectionRowEnumerable(BRSection section, BandAndRow start)
        {
          _Section = section;
          _Start = start;
        }

        private BRSection _Section;
        private BandAndRow _Start;

        public SectionRowEnumerator GetEnumerator()
        {
          return new SectionRowEnumerator(_Section, _Start);
        }

        IEnumerator<BandAndRow> IEnumerable<BandAndRow>.GetEnumerator()
        {
          return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
          return GetEnumerator();
        }
      }

      private struct SectionRowEnumerator : IEnumerator<BandAndRow>
      {
        public SectionRowEnumerator(BRSection section, BandAndRow start)
        {
          _Section = section;

          _Current = new BandAndRow(start.BandIndex, start.RowIndex - 1); // "минус первая" позиция
        }

        private BRSection _Section;
        public BandAndRow Current { get { return _Current; } }
        private BandAndRow _Current;

        object IEnumerator.Current
        {
          get { return _Current; }
        }

        public void Dispose() { }

        public bool MoveNext()
        {
          if (_Current.BandIndex >= _Section.Bands.Count)
            return false;
          _Current = new BandAndRow(_Current.BandIndex, _Current.RowIndex + 1);
          if (_Current.RowIndex >= _Section.Bands[_Current.BandIndex].RowCount)
            _Current = new BandAndRow(_Current.BandIndex + 1, 0);
          return _Current.BandIndex < _Section.Bands.Count;
        }

        public void Reset()
        {
          throw new NotImplementedException();
        }
      }

      #endregion

      #region Выполнение попытки извлечения

      private bool TryGetBlock3(int currH, bool useBandKeepWhole, bool useBandKeepWith, bool useRowKeepWith, bool useRowRepeatable, out RowStripePart[] block)
      {
        // На момент вызова гарантируется, что _CurrBandIndex и _CurrRowIndex указывают на видимую строку
        List<PageRowInfo> lst = GetPageRows(currH, useRowRepeatable);
        if (lst == null)
        {
          block = null;
          return false;
        }
#if DEBUG
        if (lst.Count < 1)
          throw new BugException("List is empty");
#endif

        // Массив флагов
        // Флаг содержит true, если нельзя выполнять разрыв ПОСЛЕ соответствующей строки
        bool[] flags = new bool[lst.Count];
        if (useBandKeepWhole)
          MarkRowsBandKeepWhole(lst, flags);
        if (useBandKeepWith)
          MarkRowsBandKeepWith(lst, flags);
        if (useRowKeepWith)
          MarkRowsRowKeepWith(lst, flags);

        // Находим последнюю не запрещенную строку
        for (int i = lst.Count - 1; i >= 0; i--)
        {
          if (!flags[i])
          {
            block = CreateBlock(lst, i + 1);
            _CurrBandIndex = lst[i].BR.BandIndex;
            _CurrRowIndex = lst[i].BR.RowIndex + 1; // на следующую строку/полосу
            return true;
          }
        }

        block = null;
        return false;
      }

      private void MarkRowsBandKeepWhole(List<PageRowInfo> lst, bool[] flags)
      {
        int prevBandIndex = -1;
        for (int i = 0; i < lst.Count; i++)
        {
          if (lst[i].BR.BandIndex == prevBandIndex)
          {
            BRBand band = _Section.Bands[lst[i].BR.BandIndex];
            if (band.KeepWhole)
              flags[i - 1] = true;
          }
          prevBandIndex = lst[i].BR.BandIndex;
        }

        // Специальный случай для последней строки
        PageRowInfo lastInfo = lst[lst.Count - 1];
        BRBand lastBand = _Section.Bands[lastInfo.BR.BandIndex];
        if (lastBand.KeepWhole)
        {
          int lastRowIndex = lastInfo.BR.RowIndex;
          if (lastRowIndex < lastBand.RowCount - 1)
          {
            //BRSelector sel = lastBand.CreateSelector();
            //for (int iRow = lastRowIndex + 1; iRow < lastBand.RowCount; iRow++)
            //{
            //  sel.RowIndex = iRow;
            //  if (sel.RowInfo.Visible)
            //  {
            //    flags[lst.Count - 1] = true;
            //    break;
            //  }
            //}
            flags[lst.Count - 1] = true;
          }
        }
      }

      private void MarkRowsBandKeepWith(List<PageRowInfo> lst, bool[] flags)
      {
        int prevBandIndex = lst[0].BR.BandIndex;
        for (int i = 1; i < lst.Count; i++)
        {
          if (lst[i].BR.BandIndex != prevBandIndex)
          {
            BRBand prevBand = _Section.Bands[lst[i - 1].BR.BandIndex];
            BRBand nextBand = _Section.Bands[lst[i].BR.BandIndex];
            if (prevBand.KeepWithNext || nextBand.KeepWithPrev)
              flags[i - 1] = true;
          }
          prevBandIndex = lst[i].BR.BandIndex;
        }
      }

      private void MarkRowsRowKeepWith(List<PageRowInfo> lst, bool[] flags)
      {
        BRSelector sel = null;
        for (int i = 0; i < lst.Count; i++)
        {
          if (IsFirstRowInBand(lst, i))
          {
            BRBand band = _Section.Bands[lst[i].BR.BandIndex];
            sel = band.CreateSelector();
          }
          sel.RowIndex = lst[i].BR.RowIndex;
          if ((!IsFirstRowInBand(lst, i)) && sel.RowInfo.KeepWithPrev)
            flags[i - 1] = true;
          if ((!IsLastRowInBand(lst, i)) && sel.RowInfo.KeepWithNext)
            flags[i] = true;
        }
      }

      private static bool IsFirstRowInBand(List<PageRowInfo> lst, int index)
      {
        if (index == 0)
          return true;
        return lst[index].BR.BandIndex != lst[index - 1].BR.BandIndex;
      }

      private static bool IsLastRowInBand(List<PageRowInfo> lst, int index)
      {
        if (index == lst.Count - 1)
          return true;
        return lst[index].BR.BandIndex != lst[index + 1].BR.BandIndex;
      }

      private RowStripePart[] CreateBlock(List<PageRowInfo> lst, int n)
      {
#if DEBUG
        if (n == 0 || n > lst.Count)
          throw ExceptionFactory.ArgOutOfRange("n", n, 1, lst.Count - 1);
#endif

        List<RowStripePart> lst2 = new List<RowStripePart>();
        RowStripePart part = new RowStripePart();
        int prevBandIndex = -1;
        for (int i = 0; i < n; i++)
        {
          if (prevBandIndex != lst[i].BR.BandIndex)
          {
            if (part.Band != null)
              lst2.Add(part);
            part = new RowStripePart();
            part.Band = _Section.Bands[lst[i].BR.BandIndex];
            part.Rows = new BRBlockSize();
            part.TopMargin = lst[i].Gap;
            prevBandIndex = lst[i].BR.BandIndex;
          }
          int h = _RowHeightArrays[lst[i].BR.BandIndex][lst[i].BR.RowIndex];
          part.Rows.Add(lst[i].BR.RowIndex, h);
        }
        lst2.Add(part);
        return lst2.ToArray();
      }

      #endregion

      #region Получение списка строк

      private struct PageRowInfo
      {
        public BandAndRow BR;
        public int Gap;
        public override string ToString()
        {
          string s = BR.ToString();
          if (Gap != 0)
            s += ", Gap=" + Gap;
          return s;
        }
      }

      private List<PageRowInfo> GetPageRows(int currH, bool useRowRepeatable)
      {
        List<PageRowInfo> lst = new List<PageRowInfo>();

        #region 1. Повторяющиеся строки

        if (useRowRepeatable)
        {
          int[] rri = _RowRepeatableIndexes[_CurrBandIndex];
          for (int i = 0; i < rri.Length; i++)
          {
            if (rri[i] >= _CurrRowIndex)
              break; // повторяющаяся строка еще не была выведена

            int h = _RowHeightArrays[_CurrBandIndex][rri[i]];
            if ((currH + h) > _MaxHeight)
              return null;

            PageRowInfo pri = new PageRowInfo();
            pri.BR = new BandAndRow(_CurrBandIndex, rri[i]);
            lst.Add(pri);
            currH += h;
          }
        }

        #endregion

        #region 2. Обычные строки

        int delayedGap = 0;
        int count = 0;
        foreach (BandAndRow br in new SectionRowEnumerable(_Section, new BandAndRow(_CurrBandIndex, _CurrRowIndex)))
        {
          if (br.RowIndex == 0)
          {
            if (currH > 0)
              delayedGap = GetBandGap(_Section, br.BandIndex);
          }

          int h = _RowHeightArrays[br.BandIndex][br.RowIndex];
          if (h == 0)
            continue; // скрытая строка не добавляется

          if ((currH + h + delayedGap) > _MaxHeight)
            break;

          PageRowInfo pri = new PageRowInfo();
          pri.BR = br;
          pri.Gap = delayedGap;
          lst.Add(pri);
          count++;
          currH += delayedGap + h;
          delayedGap = 0;
        }

        if (count == 0)
          return null;

        #endregion

        return lst;
      }

      #endregion
    }
#endif
    #endregion

    #region Создание списка страниц

    private class CreatePagesHelper : List<BRPaginatorPageInfo>
    {
      internal void PageHandling(object sender, BRPaginatorEventArgs args)
      {
        Add(args.Page);
      }
    }

    /// <summary>
    /// Создает массив описаний для страниц.
    /// </summary>
    public BRPaginatorPageInfo[] CreatePages(BRReport report)
    {
      CreatePagesHelper helper = new CreatePagesHelper();
      this.PageHandling += helper.PageHandling;
      try
      {
        Paginate(report);
      }
      finally
      {
        this.PageHandling -= helper.PageHandling;
      }
      return helper.ToArray();
    }

    #endregion

    #region Подбор высоты строки

    private int GetDefaultRowHeight(BRSelector sel, int[] realColWidthArray)
    {
#if DEBUG
      if (realColWidthArray.Length != sel.Band.ColumnCount)
        throw ExceptionFactory.ArgWrongCollectionCount("realColWidthArray", realColWidthArray, sel.Band.ColumnCount);
#endif
      int currRowIndex = sel.RowIndex;
      int h = 0;
      for (int iCol = 0; iCol < sel.Band.ColumnCount; iCol++)
      {
        sel.ColumnIndex = iCol;
        //if (!sel.ColumnInfo.Visible)
        //  continue;

#if DEBUG
        if (realColWidthArray[iCol] < BRReport.MinColumnWidth)
          throw new BugException("Invalid column width");
#endif

        if (sel.AsString.Length == 0)
          continue;

        // Распределяем на объединение строк
        BRRange r = sel.MergeInfo;
        if (iCol != r.FirstColumnIndex)
          continue;

        int w = realColWidthArray[iCol];
        for (int iCol2 = iCol + 1; iCol2 <= r.LastColumnIndex; iCol2++)
          w += realColWidthArray[iCol];

        int h1 = _Measurer.GetWantedHeight(sel, w);
        if (h1 <= 0)
          throw new BugException("H=" + h1.ToString());

        // Здесь пропускаем скрытые строки
        int cnt = 1;
        for (int iRow = r.FirstRowIndex; iRow <= r.LastRowIndex; iRow++)
        {
          if (iRow == currRowIndex)
            continue;
          sel.RowIndex = iRow;
          //if (sel.RowInfo.Visible)
          //{
          if (sel.RowInfo.Height == BRReport.AutoRowHeight)
            cnt++;
          //  else
          //   h1 -= sel.RowInfo.Height;
          //}
        }
        sel.RowIndex = currRowIndex;

        if (cnt == 1)
          h = Math.Max(h, h1);
        else
        {
          int h2 = h1 / cnt;
          if ((h1 % cnt) > 0) // ошибка округления
            h2++;
          h = Math.Max(h, h2);
        }
      }


      if (h == 0)
        return sel.Band.Report.DefaultEmptyRowHeight;
      else
        return Math.Max(h, BRReport.MinRowHeight);
    }

    #endregion

    #region Увеличение размеров строк и граф с признаком AutoGrow

    private static void PrepareColumnAutoGrow(BRBand band, BRBlockSize items, BRSelector sel)
    {
      #region Ширина

      int maxW = band.Section.PageSetup.PrintAreaWidth;
      int wholeW = DataTools.SumInt32(items.Sizes);
      if (wholeW < maxW)
      {
        int cnt = 0;
        int sumW = 0;
        for (int j = 0; j < items.Indexes.Count; j++)
        {
          sel.ColumnIndex = items.Indexes[j];
          if (sel.ColumnInfo.AutoGrow)
          {
            cnt++;
            sumW += items.Sizes[j];
          }
        }

        if (cnt > 0)
        {
          double k = 1.0 + (double)(maxW - wholeW) / (double)sumW;
          int lastIndex = -1;
          for (int j = 0; j < items.Indexes.Count; j++)
          {
            sel.ColumnIndex = items.Indexes[j];
            if (sel.ColumnInfo.AutoGrow)
            {
              items.Sizes[j] = (int)(items.Sizes[j] * k);
              lastIndex = j;
            }
          }
          // Устраняем ошибку округления
          items.Sizes[lastIndex] += (maxW - DataTools.SumInt32(items.Sizes));
        }
      }

      #endregion
    }

    #endregion

    #region Для Word/Writer

    /// <summary>
    /// Разбиение на вертикальные полосы для экспорта в Word/Writer.
    /// Разбиение выполняется для одной таблицы.
    /// В возвращаемом массиве первая размерность соответствует полосе (странице), а вторая задает столбцы для этой полосы.
    /// Учитывается автоподбор размеров столбцов и повторяющиеся столбцы.
    /// </summary>
    /// <param name="band">Таблица</param>
    /// <returns>Массив столбцов</returns>
    public static BRStripeItem[][] PaginateBandColumns(BRBand band)
    {
      List<BRBlockSize> lst = GetColumnStripes(band);
      BRStripeItem[][] a = new BRStripeItem[lst.Count][];
      for (int i = 0; i < lst.Count; i++)
      {
        a[i] = new BRStripeItem[lst[i].Indexes.Count];
        for (int j = 0; j < lst[i].Indexes.Count; j++)
          a[i][j] = new BRStripeItem(lst[i].Indexes[j], lst[i].Sizes[j]);
      }
      return a;
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Подготовка строки к вызову метода <see cref="IBRMeasurer.MeasureString(string, BRCellStyle, out int, out int)"/>.
    /// Убирает из строки символы мягкого переноса и заменяет символы неразрывного пробела на обычные пробелы
    /// </summary>
    /// <param name="s">Строка, которая будет измерена</param>
    /// <returns>Строка для передачи в <see cref="IBRMeasurer.MeasureString(string, BRCellStyle, out int, out int)"/></returns>
    public static string PrepareStringForMeasure(string s)
    {
      s = s.Replace(StringTools.NonBreakSpaceChar, ' ');
      s = s.Replace(StringTools.SoftHyphenStr, "");
      return s;
    }

    #endregion
  }
}
